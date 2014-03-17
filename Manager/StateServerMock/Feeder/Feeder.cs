using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Diagnostics;
using Manager.Common;
using System.ComponentModel;

namespace TestConsole.Feeder
{
    public class Feeder
    {
        private static Feeder _Feeder = new Feeder();
        public static Feeder Instance { get { return Feeder._Feeder; } }

        private string _Server;
        private int _Port;
        private List<QuotationSource> _QuotationSources = new List<QuotationSource>();

        private List<SendTask> _SendTasks = new List<SendTask>();
        public Feeder()
        {
            try
            {
                string serverConfig = ConfigurationManager.AppSettings["QuotationServer"];
                this._Server = serverConfig.Substring(0, serverConfig.IndexOf(':'));
                this._Port = int.Parse(serverConfig.Substring(serverConfig.IndexOf(':') + 1));
            }
            catch (Exception exception)
            {
                Logger.TraceEvent(TraceEventType.Error, "Feeder ctor exception\r\n{0}", exception);
            }
        }

        public Action FinishedCallback { get; set; }

        public void Start(ObservableCollection<SendTask> sendTasks)
        {
            foreach (SendTask sendTask in sendTasks)
            {
                if (sendTask.IsSelected)
                {
                    this._SendTasks.Add(sendTask);
                    this._QuotationSources.Add(new QuotationSource(this._Server, this._Port, sendTask));
                    sendTask.PropertyChanged += sendTask_PropertyChanged;
                }
            }
        }

        public void Stop()
        {
            foreach (QuotationSource source in this._QuotationSources)
            {
                source.Stop();
            }
        }

        private void sendTask_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Status")
            {
                SendTask sendTask = (SendTask)sender;
                if (sendTask.Status.StartsWith("Finished"))
                {
                    this._SendTasks.Remove((SendTask)sender);
                    if (this._SendTasks.Count == 0)
                    {
                        this.FinishedCallback();
                    }
                }
            }
        }
    }
}
