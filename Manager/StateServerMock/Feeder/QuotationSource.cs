using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Manager.Common;

namespace TestConsole.Feeder
{
    public class QuotationSource
    {
        private TcpClient _TcpClient = null;
        private Timer _RecoveTimer;

        private string _Server;
        private int _Port;
        private SendTask _SendTask;

        private Timer _ReceiveTimer;
        private byte[] _ReceiveBuffer; // For receiving of Connection Detection Packet

        private bool _IsRunning = true;
        private ManualResetEventSlim _ConnectedEvent = new ManualResetEventSlim(false);

        public QuotationSource(string server, int port, SendTask sendTask)
        {
            this._Server = server;
            this._Port = port;
            this._SendTask = sendTask;

            this._RecoveTimer = new Timer(delegate(object state)
            {
                try
                {
                    this.ConnectAndAuthenticate();
                }
                catch (Exception exception)
                {
                    Logger.TraceEvent(TraceEventType.Error, "QuotationManager ctor\r\n{0}", exception);
                    this.Recove(3000);
                }
            }, null, Timeout.Infinite, Timeout.Infinite);

            this._ReceiveBuffer = new byte[32];
            this._ReceiveTimer = new Timer(delegate(object state)
            {
                try
                {
                    while (this._TcpClient.Available > 0)
                    {
                        this._TcpClient.GetStream().Read(this._ReceiveBuffer, 0, this._ReceiveBuffer.Length);
                    }
                    this._ReceiveTimer.Change(30000, Timeout.Infinite);
                }
                catch { }
            });

            ThreadPool.QueueUserWorkItem(this.InternalSendQuotation);

            // start here
            this.Recove(0);
        }

        private void ConnectAndAuthenticate()
        {
            this._TcpClient = new TcpClient();
            this._TcpClient.Connect(this._Server, this._Port);

            string packet = string.Format("1{0}\t{1}\t{2}", this._SendTask.SourceName, this._SendTask.LoginName, this._SendTask.Password);
            this.Send(packet);

            int result = this._TcpClient.GetStream().ReadByte();

            if (result == 1)
            {
                this._SendTask.Status = "Login Succeeded!";
                this._ConnectedEvent.Set();
                this._ReceiveTimer.Change(9000, Timeout.Infinite);
            }
            else
            {
                this._SendTask.Status = "Login failed!";
            }
        }

        protected void InternalStop()
        {
            try
            {
                this._TcpClient.Close();
                this._SendTask.Status = "Stopped.";
            }
            catch (Exception exception)
            {
                Logger.TraceEvent(TraceEventType.Error, "QuotationSource.InternalStop exception\r\n{0}", exception);
            }
        }

        private void InternalSendQuotation(object @object)
        {
            try
            {
                List<string> quotations = new List<string>();
                using (StreamReader fileReader = new StreamReader(this._SendTask.DataFileName))
                {
                    string line;
                    while (!string.IsNullOrEmpty(line = fileReader.ReadLine()))
                    {
                        quotations.Add(line);
                    }
                }
                while(this._IsRunning)
                {
                    string line, last, high, low;
                    for (int i = 0; i < quotations.Count && this._IsRunning; i++)
                    {
                        line = quotations[i];
                        this._ConnectedEvent.Wait();
                        if (line.TrimStart().StartsWith("--")) continue;
                        string[] items = line.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                        int waitTime = int.Parse(items[1]);
                        if (waitTime > 0)
                        {
                            Thread.Sleep(waitTime);
                        }
                        last = items.Length > 4 ? items[4] : string.Empty;
                        high = items.Length > 5 ? items[5] : string.Empty;
                        low = items.Length > 6 ? items[6] : string.Empty;
                        string quotation2 = string.Format("2{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}",
                            this._SendTask.SourceName, items[0], DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), items[2], items[3], last, high, low);
                        try
                        {
                            this.Send(quotation2);
                        }
                        catch (Exception exception)
                        {
                            this._ConnectedEvent.Reset();
                            try
                            {
                                this._TcpClient.Close();
                            }
                            catch
                            {
                            }

                            this._SendTask.Status = "Stopped while sending.";
                            Logger.TraceEvent(TraceEventType.Error, "QuotationSource.InternalSendQuotation exception\r\n{0}", exception);
                            this.Recove(5000);
                        }
                    }
                    if (!this._SendTask.IsRepeat) break;
                }
                this._SendTask.Status = "Finished";
            }
            catch (Exception exception)
            {
                this._SendTask.Status = "Finished with error";
                Logger.TraceEvent(TraceEventType.Error, "InternalSendQuotation source name:{0}\r\n{1}", this._SendTask.SourceName, exception);
            }
        }

        private void Recove(int delayTime)
        {
            this._ReceiveTimer.Change(Timeout.Infinite, Timeout.Infinite);
            this._RecoveTimer.Change(delayTime, Timeout.Infinite);
        }

        private void Send(string info)
        {
            byte[] packet = ASCIIEncoding.ASCII.GetBytes(info);
            int length = packet.Length;
            this._TcpClient.GetStream().WriteByte((byte)(length / 256));
            this._TcpClient.GetStream().WriteByte((byte)length);

            this._TcpClient.GetStream().Write(packet, 0, packet.Length);
        }

        internal void Stop()
        {
            this._IsRunning = false;
        }
    }
}
