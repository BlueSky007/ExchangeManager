using Manager.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ManagerService.Quotation
{
    public class QuotationReceiver
    {
        private int _ListenPort;
        private TcpListener _Listener;
        private Thread _ListeningThread;
        private QuotationManager _QuotationManager;

        public QuotationReceiver(int listenPort)
        {
            this._ListenPort = listenPort;
        }

        public void Start(QuotationManager quotationManager)
        {
            this._QuotationManager = quotationManager;

            this._ListeningThread = new Thread(delegate()
            {
                try
                {
                    this._Listener = new TcpListener(new IPEndPoint(IPAddress.Any, this._ListenPort));
                    this._Listener.Start();
                }
                catch (Exception exception)
                {
                    Logger.TraceEvent(TraceEventType.Error,"QuotationReceiver.Start Can't listen on {0}\r\n{1}", this._ListenPort, exception.ToString());
                    this.Stop();
                    return;
                }

                while (true)
                {
                    try
                    {
                        TcpClient client = this._Listener.AcceptTcpClient();
                        QuotationClient quotationClient = new QuotationClient(client, this._QuotationManager);
                        quotationClient.Start();
                    }
                    catch (Exception exception)
                    {
                        Logger.TraceEvent(TraceEventType.Error, "QuotationReceiver.Start Accept QuotationClient failed\r\n{0}", exception.ToString());
                        this.Stop();
                        break;
                    }
                }

            }
            );
            this._ListeningThread.IsBackground = true;
            this._ListeningThread.Start();
        }

        public void Stop()
        {
            try
            {
                if (this._Listener != null)
                {
                    this._Listener.Stop();
                }
            }
            catch
            {
            }
            finally
            {
                Logger.AddEvent(TraceEventType.Information, "Quotation receiver stopped");
                this._Listener = null;
            }
        }
    }
}
