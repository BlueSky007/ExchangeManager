using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Diagnostics;
using Manager.Common;
using Manager.Common.QuotationEntities;

namespace ManagerService.Quotation
{
    public class QuotationClient
    {
        private static Dictionary<string, QuotationClient> _Sources = new Dictionary<string, QuotationClient>();

        // Map for SourceName - [InstrumentCode -  PrimitiveQuotation]
        private Dictionary<string, Dictionary<string, PrimitiveQuotation>> _LastPriceData = new Dictionary<string, Dictionary<string, PrimitiveQuotation>>();
        private object _PacketLock = new object();
        private AutoResetEvent _PacketArrivedEvent = new AutoResetEvent(false);

        private TcpClient _TcpClient;
        private NetworkStream _NetworkStream;
        private string _SourceName;
        private QuotationManager _QuotationManager;
        private RelayEngine<string> _QuotationRelayEngine;

        public QuotationClient(TcpClient tcpClient, QuotationManager quotationManager)
        {
            this._QuotationManager = quotationManager;
            this._TcpClient = tcpClient;
            this._SourceName = "";
        }

        public void Start()
        {
            this._NetworkStream = this._TcpClient.GetStream();
            Thread receiveDataThread = new Thread(delegate()
            {
                while (true)
                {
                    byte[] buffer = new byte[2];
                    if (!this.ReadAll(buffer))
                    {
                        this.Stop();
                        break;
                    }
                    int packetLength = ((int)buffer[0]) * 256 + buffer[1];
                    if (packetLength <= 0) continue;
                    buffer = new byte[packetLength];

                    if (this.ReadAll(buffer))
                    {
                        try
                        {
                            this.Process(buffer);
                        }
                        catch (Exception e)
                        {
                            this.Stop();
                            Logger.TraceEvent(TraceEventType.Error, "QuotationClient.start ", e.ToString());
                        }
                    }
                    else
                    {
                        this.Stop();
                        break;
                    }
                }
            });
            receiveDataThread.IsBackground = true;
            receiveDataThread.Start();

            this._QuotationRelayEngine = new RelayEngine<string>(this.ProcessQuotation, delegate(Exception ex) { });
        }

        private void Process(byte[] buffer)
        {
            string packet = ASCIIEncoding.ASCII.GetString(buffer);
            char commandChar = packet[0];
            if (commandChar == '2')//price data
            {
                packet = packet.Substring(1);
                lock (this._PacketLock)
                {
                    this._QuotationRelayEngine.AddItem(packet);
                }
                this._PacketArrivedEvent.Set();
            }
            else if (commandChar == '1')//authenticate
            {
                this.ProcessAuthentication(packet.Substring(1));
            }
            else//illegal data
            {
                Logger.AddEvent(TraceEventType.Warning, "Receive illegal data " + packet + Environment.NewLine + "from " + this._SourceName);
            }
        }

        private bool ProcessQuotation(string packet)
        {
            string[] priceData = packet.Split('\t');
            string sourceName = priceData[0];
            string instrumentCode = priceData[1];
            DateTime timestamp = DateTime.Parse(priceData[2]);
            string bid = priceData[3];
            string ask = priceData[4];
            string last = priceData[5];
            string high = priceData[6];
            string low = priceData[7];

            PrimitiveQuotation quotation;

            // TODO: It should not use the same object to carry the data!
            if (this._LastPriceData[sourceName].TryGetValue(instrumentCode, out quotation))
            {
                if (quotation.Timestamp != timestamp || quotation.Bid != bid || quotation.Last != last || quotation.High != high || quotation.Low != low || quotation.Ask != ask)
                {
                    quotation.Ask = ask;
                    quotation.Bid = bid;
                    quotation.Last = last;
                    quotation.High = high;
                    quotation.Low = low;
                    quotation.Timestamp = timestamp;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                quotation = new PrimitiveQuotation { SourceName = sourceName, InstrumentCode = instrumentCode, Ask = ask, Bid = bid, Last = last, High = high, Low = low, Timestamp = timestamp };
                this._LastPriceData[sourceName].Add(instrumentCode, quotation);
            }

            this._QuotationManager.ProcessQuotation(quotation);
            return true;
        }

        private void ProcessAuthentication(string packet)
        {
            string[] authenticationInfo = packet.Split('\t');
            string sourceName = authenticationInfo[0];
            string loginName = authenticationInfo[1];
            string password = authenticationInfo[2];
            this._SourceName = sourceName;

            this._QuotationManager.QuotationSourceStatusChanged(sourceName, ConnectionState.Connecting);

            QuotationClient oldQuotationClient;
            if (QuotationClient._Sources.TryGetValue(sourceName, out oldQuotationClient))
            {
                Logger.AddEvent(TraceEventType.Warning, "QuotationReceiver.ProcessAuthentication sourceName:{0} already connected", sourceName);
                oldQuotationClient.Stop();
            }

            bool isValid = this._QuotationManager.AuthenticateSource(sourceName, loginName, password);
            if (isValid)
            {
                Logger.AddEvent(TraceEventType.Information, "QuotationReceiver.ProcessAuthentication Connection from {0} is established", this._SourceName);
                this._TcpClient.GetStream().WriteByte(1);
                QuotationClient._Sources.Add(sourceName, this);
                this._QuotationManager.QuotationSourceStatusChanged(sourceName, ConnectionState.Connected);

                if (!this._LastPriceData.ContainsKey(sourceName))
                {
                    this._LastPriceData[sourceName] = new Dictionary<string, PrimitiveQuotation>();
                }
            }
            else
            {
                this._TcpClient.GetStream().WriteByte(0);
                Logger.AddEvent(TraceEventType.Information, "QuotationReceiver.ProcessAuthentication Connection from {0} is closed for authentication failed", this._SourceName);
                this.Stop();
            }
        }

        private void Stop()
        {
            try
            {
                this._QuotationRelayEngine.Stop();
                this._TcpClient.Close();
            }
            finally
            {
                if (!string.IsNullOrEmpty(this._SourceName))
                {
                    QuotationClient._Sources.Remove(this._SourceName);
                    this._QuotationManager.QuotationSourceStatusChanged(this._SourceName, ConnectionState.Disconnected);
                }
            }
        }

        private bool ReadAll(byte[] buffer)
        {
            try
            {
                int offset = 0;
                int len = buffer.Length;

                while (len > 0)
                {
                    int readLength = this._NetworkStream.Read(buffer, offset, len);
                    offset += readLength;
                    len = buffer.Length - offset;
                    Thread.Sleep(10);
                }
                return true;
            }
            catch(Exception ex)
            {
                Logger.TraceEvent(TraceEventType.Error, "QuotationClient.ReadAll error:\r\n{0}", ex.ToString());
                return false;
            }
        }
    }
}
