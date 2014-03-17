using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.IO;
using System.Configuration;
using iExchange.Common;
using System.Diagnostics;
using System.Threading;
using System.Net.Sockets;
using System.Text;

namespace ManagerService.QuotationExchange
{
    internal class FileNameComparer : IComparer<string>
    {
        int IComparer<string>.Compare(string left, string right)
        {
            left = Path.GetFileName(left);
            right = Path.GetFileName(right);
            left = left.Substring(0, left.Length - 4);
            right = right.Substring(0, right.Length - 4);
            return long.Parse(left).CompareTo(long.Parse(right));
        }
    }

    internal class QuotaionExporter
    {
        private static FileNameComparer FileNameComparer = new FileNameComparer();

        private bool _IsRunning = false;
        private AutoResetEvent[] _Events = new AutoResetEvent[] { new AutoResetEvent(false), new AutoResetEvent(false) };
        private Queue<string> _Quotations = new Queue<string>();
        private object _Lock = new object();

        private List<Instrument> _Instruments = new List<Instrument>();
        private bool _IsConnectionBroken = true;
        private long _NextFileSerialNumber = 1;

        internal QuotaionExporter(ExporterConfig config)
        {
            this.Config = config;
        }

        private ExporterConfig Config { get; set; }

        internal void Start()
        {
            this._NextFileSerialNumber = this.GetNextFileSerialNumber();

            this._IsRunning = true;
            Thread thread = new Thread(() =>
            {
                this.DoWork();
            });
            thread.IsBackground = true;
            thread.Priority = ThreadPriority.Lowest;
            thread.Start();
        }

        private long GetNextFileSerialNumber()
        {
            if (string.IsNullOrEmpty(this.Config.CacheFilePath) || !Directory.Exists(this.Config.CacheFilePath)) return 1;

            List<string> fileNames = new List<string>(Directory.EnumerateFiles(this.Config.CacheFilePath, "*.dat"));
            if (fileNames.Count > 0)
            {
                fileNames.Sort(FileNameComparer);
                string lastFileName = Path.GetFileName(fileNames[fileNames.Count - 1]);
                return long.Parse(lastFileName.Substring(0, lastFileName.Length - 4)) + 1;
            }
            else
            {
                return 1;
            }
        }

        private void DoWork()
        {
            while (this._IsRunning)
            {
                Socket client = null;
                try
                {
                    client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    client.Connect(this.Config.Host, int.Parse(this.Config.Port));
                }
                catch (Exception exception)
                {
                    AppDebug.LogEvent("QuotaionExporter.DoWork", exception.ToString(), EventLogEntryType.Warning);
                    Thread.Sleep(3000);
                    continue;
                }

                lock (this._Lock)
                {
                    this._IsConnectionBroken = false;
                }

                while (this._IsRunning)
                {
                    try
                    {
                        if (!this.SendBackupFiles(client))
                        {
                            lock (this._Lock)
                            {
                                this._IsConnectionBroken = true;
                                try
                                {
                                    this.SaveQuotations(this._Quotations.DequeueAllIntoString());
                                }
                                catch (Exception exception)
                                {
                                    AppDebug.LogEvent("QuotaionExporter.DoWork", "SaveQuotations error, DoWork exist: " + exception.ToString(), EventLogEntryType.Error);
                                    return;
                                }
                            }
                            break;
                        }
                    }
                    catch (Exception exception)
                    {
                        AppDebug.LogEvent("QuotaionExporter.DoWork", "SendBackup error, DoWork exist: " + exception.ToString(), EventLogEntryType.Warning);
                        return;
                    }

                    this._NextFileSerialNumber = 1;
                    this._IsConnectionBroken = false;
                    int eventIndex = AutoResetEvent.WaitAny(this._Events);
                    if (eventIndex == 0) break;//stopped

                    if (eventIndex == 1)//got quotations
                    {
                        lock (this._Lock)
                        {
                            string quotations = this._Quotations.DequeueAllIntoString();

                            if (!this.Send(client, quotations))
                            {
                                this._IsConnectionBroken = true;
                                try
                                {
                                    this.SaveQuotations(quotations);
                                }
                                catch (Exception exception)
                                {
                                    AppDebug.LogEvent("QuotaionExporter.DoWork", "SaveQuotations error, DoWork exist: " + exception.ToString(), EventLogEntryType.Error);
                                    return;
                                }
                            }
                        }
                    }
                    if (this._IsConnectionBroken) break;
                }
            }
        }

        private bool SendBackupFiles(Socket client)
        {
            if (string.IsNullOrEmpty(this.Config.CacheFilePath) || !Directory.Exists(this.Config.CacheFilePath)) return true;

            List<string> fileNames = new List<string>(Directory.EnumerateFiles(this.Config.CacheFilePath, "*.dat"));
            if (fileNames.Count > 0)
            {
                fileNames.Sort(FileNameComparer);
                List<string> sentFiles = new List<string>();
                foreach (string fileName in fileNames)
                {
                    using (StreamReader reader = new StreamReader(fileName))
                    {
                        string quotation = reader.ReadToEnd();
                        if (this.Send(client, quotation))
                        {
                            sentFiles.Add(fileName);
                        }
                        else
                        {
                            return false;
                        }
                    }
                }

                foreach (string fileName in sentFiles)
                {
                    File.Delete(fileName);
                }
            }
            return true;
        }

        private byte[] header = new byte[4];
        private bool Send(Socket client, string quotations)
        {
            byte[] buffer = Encoding.Unicode.GetBytes(quotations);

            int length = buffer.Length;
            header[0] = (byte)(length >> 24);
            header[1] = (byte)(length >> 16);
            header[2] = (byte)(length >> 8);
            header[3] = (byte)(length);
            return QuotaionExporter.SendAll(client, header) && QuotaionExporter.SendAll(client, buffer);
        }

        private static bool SendAll(Socket client, byte[] buffer)
        {
            int sentLen = 0;
            while (sentLen < buffer.Length)
            {
                try
                {
                    int len = client.Send(buffer, sentLen, buffer.Length - sentLen, SocketFlags.None);
                    sentLen += len;
                }
                catch (Exception exception)
                {
                    AppDebug.LogEvent("QuotaionExporter.SendAll", exception.ToString(), EventLogEntryType.Warning);
                    return false;
                }
            }
            return true;
        }

        internal void Stop()
        {
            this._IsRunning = false;
            this._Events[0].Set();
        }

        internal void AddQuotation(ICollection<OverridedQuotation> quotations)
        {
            lock (this._Lock)
            {
                bool hasQuotaionAdded = false;
                foreach (OverridedQuotation quotation in quotations)
                {
                    if (quotation.ModifyState == ModifyState.Unchanged) continue;

                    if (quotation.QuotePolicy.ID == this.Config.QuotePolicyId)
                    {
                        if (this._Instruments.Contains(quotation.Instrument))
                        {
                            this._Quotations.Enqueue(quotation.ToFormatString());
                            hasQuotaionAdded = true;
                        }
                        else if (Array.Exists(this.Config.InstrumentList, (string item) => { return string.Compare(item, quotation.Instrument.Code, true) == 0; }))
                        {
                            this._Instruments.Add(quotation.Instrument);

                            this._Quotations.Enqueue(quotation.ToFormatString());
                            hasQuotaionAdded = true;
                        }
                    }
                }
                if (hasQuotaionAdded)
                {
                    if (this._IsConnectionBroken)
                    {
                        this.SaveQuotations(this._Quotations.DequeueAllIntoString());
                    }
                    else
                    {
                        this._Events[1].Set();
                    }
                }
            }
        }

        private void SaveQuotations(string quotation)
        {
            if (string.IsNullOrEmpty(this.Config.CacheFilePath)) return;

            if (!Directory.Exists(this.Config.CacheFilePath))
            {
                Directory.CreateDirectory(this.Config.CacheFilePath);
            }

            string fileFullName = Path.Combine(this.Config.CacheFilePath, string.Format("{0}.dat", this._NextFileSerialNumber++));
            if (File.Exists(fileFullName)) File.Delete(fileFullName);

            using (StreamWriter writer = new StreamWriter(fileFullName))
            {
                writer.Write(quotation);
            }
        }
    }

    internal static class QuotationHelper
    {
        internal static string ToFormatString(this OverridedQuotation quotation)
        {
            return string.Format("${0}|{1}|{2}|{3}|{4}|{5}", quotation.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                quotation.Instrument.Code, quotation.Ask, quotation.Bid, quotation.High, quotation.Low);
        }

        internal static string DequeueAllIntoString(this Queue<string> quotations)
        {
            StringBuilder builder = new StringBuilder();
            foreach (string quotation in quotations)
            {
                builder.Append(quotation);
            }
            quotations.Clear();
            return builder.ToString();
        }
    }

    public class ExporterConfig : ConfigurationSection
    {
        [ConfigurationProperty("Host", IsKey = true, IsRequired = true)]
        public string Host
        {
            get { return (string)base["Host"]; }
        }

        [ConfigurationProperty("Port", IsKey = true, IsRequired = true)]
        public string Port
        {
            get { return (string)base["Port"]; }
        }

        [ConfigurationProperty("QuotePolicyCode", IsKey = true, IsRequired = true)]
        public string QuotePolicyCode
        {
            get { return ((string)base["QuotePolicyCode"]).ToLower().Trim(); }
        }

        [ConfigurationProperty("Instruments", IsKey = true, IsRequired = true)]
        public string Instruments
        {
            get { return (string)base["Instruments"]; }
        }

        [ConfigurationProperty("CacheFilePath", IsKey = true, IsRequired = false)]
        public string CacheFilePath
        {
            get { return (string)base["CacheFilePath"]; }
        }

        public string[] InstrumentList
        {
            get;
            set;
        }

        public Guid QuotePolicyId
        {
            get;
            set;
        }
    }

    internal static class QuotaionExporterManager
    {
        private static List<QuotaionExporter> Exporters = new List<QuotaionExporter>();

        internal static bool HasExporter
        {
            get { return Exporters.Count > 0; }
        }
        
        internal static void Start(Dictionary<string, Guid> quotePolicies)
        {
            string configFilePath = Path.Combine(Environment.CurrentDirectory, "QuotationExporter.config");
            //string configFilePath = @"D:\Teams\iExchangeCollection\iExchange3 Team\Manager\ManagerService\QuotationExchange\QuotationExporter.config";
            if (File.Exists(configFilePath))
            {
                try
                {
                    ExeConfigurationFileMap configMap = new ExeConfigurationFileMap();
                    configMap.ExeConfigFilename = configFilePath;
                    Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);

                    ConfigurationSectionGroup quotationExporterGroup = config.GetSectionGroup("QuotationExporterGroup");
                    if (quotationExporterGroup == null) return;
                    foreach (ConfigurationSection item in quotationExporterGroup.Sections)
                    {
                        ExporterConfig exporter = (ExporterConfig)item;
                    }
                    foreach (ExporterConfig exporterConfig in quotationExporterGroup.Sections)
                    {
                        try
                        {
                            Guid quotePolicyId;
                            if (quotePolicies.TryGetValue(exporterConfig.QuotePolicyCode, out quotePolicyId))
                            {
                                exporterConfig.QuotePolicyId = quotePolicyId;
                                exporterConfig.InstrumentList = exporterConfig.Instruments.Split('|');
                                QuotaionExporter exporter = new QuotaionExporter(exporterConfig);
                                exporter.Start();
                                Exporters.Add(exporter);
                            }
                            else
                            {
                                AppDebug.LogEvent("QuotaionExporterManager.Start", "Can't find quotepolicy which code is: " + exporterConfig.QuotePolicyCode, EventLogEntryType.Warning);
                            }
                        }
                        catch (Exception exception)
                        {
                            AppDebug.LogEvent("QuotaionExporterManager.Start", exception.ToString(), EventLogEntryType.Error);
                        }
                    }
                }
                catch (Exception exception)
                {
                    AppDebug.LogEvent("QuotaionExporterManager.Start", exception.ToString(), EventLogEntryType.Error);
                }
            }
        }

        internal static void Stop()
        {
            foreach (QuotaionExporter exporter in Exporters)
            {
                try
                {
                    exporter.Stop();
                }
                catch (Exception exception)
                {
                    AppDebug.LogEvent("QuotaionExporterManager.Stop", exception.ToString(), EventLogEntryType.Error);
                }
            }

        }

        internal static void AddQuotation(ICollection<OverridedQuotation> quotations)
        {
            foreach (QuotaionExporter exporter in Exporters)
            {
                try
                {
                    exporter.AddQuotation(quotations);
                }
                catch (Exception exception)
                {
                    AppDebug.LogEvent("QuotaionExporterManager.AddQuotation", exception.ToString(), EventLogEntryType.Error);
                }
            }
        }
    }
}
