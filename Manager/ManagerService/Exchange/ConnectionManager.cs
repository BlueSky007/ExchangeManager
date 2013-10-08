using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Diagnostics;
using System.Threading;
using Manager.Common;

namespace ManagerService.Exchange
{
    [XmlSerializerFormat]
    [ServiceContract(Namespace = "http://www.omnicare.com/StateServer/")]
    public interface IStateServerWebService
    {
        [OperationContract]
        void NotifyManagerStarted(string managerAddress, string exchangeCode);
    }

    public class ConnectionManager
    {
        private Dictionary<string, ExchangeSystem> _ExchangeSystems;
        private Thread _MonitorThread;

        public ConnectionManager(Dictionary<string, ExchangeSystem> exchangeSystems)
        {
            this._ExchangeSystems = exchangeSystems;
        }

        public void NotifyExchangeManagerStarted()
        {
            foreach (var exchangeSystem in this._ExchangeSystems.Values)
            {
                this.NotifyExchangeToConnect(exchangeSystem.StateServerUrl, exchangeSystem.ExchangeCode);
            }
            this._MonitorThread = new Thread(this.Monitor) { IsBackground = true };
            this._MonitorThread.Start();
        }

        private void NotifyExchangeToConnect(string stateServerUrl, string exchangeSystemCode)
        {
            try
            {
                EndpointAddress address = new EndpointAddress(stateServerUrl);
                CustomBinding binding = new CustomBinding();
                binding.Elements.Add(new TextMessageEncodingBindingElement(MessageVersion.Soap12, Encoding.UTF8));
                binding.Elements.Add(new HttpTransportBindingElement());
                IStateServerWebService stateServer = ChannelFactory<IStateServerWebService>.CreateChannel(binding, address);
                stateServer.NotifyManagerStarted(Manager.ManagerSettings.ServiceAddressForExchange, exchangeSystemCode);
            }
            catch (EndpointNotFoundException)
            {
                // It's maybe StateServerUrl is wrong or the IIS that host stateserver is not started.
                Logger.AddEvent(TraceEventType.Error,
                    "ConnectionManager.NotifyExchangeToConnect EndpointNotFoundException\r\nstateServerUrl:{0}\r\nexchangeSystemCode:{1}",
                    stateServerUrl, exchangeSystemCode);
            }
            catch (Exception ex)
            {
                Logger.AddEvent(TraceEventType.Error, "ConnectionManager.NotifyExchangeToConnect error:\r\n{0}", ex.ToString());
            }
        }

        private void Monitor()
        {
            try
            {
                TimeSpan interval = TimeSpan.FromMinutes(5);
                while (true)
                {
                    foreach (var exchangeSystem in this._ExchangeSystems.Values)
                    {
                        if (exchangeSystem.ConnectionState == ConnectionState.Disconnected)
                        {
                            this.NotifyExchangeToConnect(exchangeSystem.StateServerUrl, exchangeSystem.ExchangeCode);
                        }
                    }
                    Thread.Sleep(interval);
                }
            }
            catch (Exception ex)
            {
                Logger.AddEvent(TraceEventType.Error, "ConnectionManager.Monitor thread exit.\r\n" + ex.ToString());
            }
        }
    }
}
