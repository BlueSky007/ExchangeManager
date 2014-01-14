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
        private Timer _MonitorTimer;

        public ConnectionManager(Dictionary<string, ExchangeSystem> exchangeSystems)
        {
            this._ExchangeSystems = exchangeSystems;
            this._MonitorTimer = new Timer(this.Monitor);
        }

        public void NotifyExchangeManagerStarted()
        {
            foreach (var exchangeSystem in this._ExchangeSystems.Values)
            {
                this.NotifyExchangeToConnect(exchangeSystem.StateServerUrl, exchangeSystem.ExchangeCode);
            }
        }

        public void NotifyExchangeToConnect(string stateServerUrl, string exchangeSystemCode)
        {
            bool callFailed = true;
            try
            {
                EndpointAddress address = new EndpointAddress(stateServerUrl);
                CustomBinding binding = new CustomBinding();
                binding.Elements.Add(new TextMessageEncodingBindingElement(MessageVersion.Soap12, Encoding.UTF8));
                binding.Elements.Add(new HttpTransportBindingElement());
                IStateServerWebService stateServer = ChannelFactory<IStateServerWebService>.CreateChannel(binding, address);
                stateServer.NotifyManagerStarted(MainService.ManagerSettings.ServiceAddressForExchange, exchangeSystemCode);
                callFailed = false;
            }
            catch (EndpointNotFoundException)
            {
                // It's maybe StateServerUrl is wrong or the IIS that host stateserver is not started.
                Logger.AddEvent(TraceEventType.Error,
                    "ConnectionManager.NotifyExchangeToConnect EndpointNotFoundException\r\nstateServerUrl:{0}\r\nexchangeSystemCode:{1}",
                    stateServerUrl, exchangeSystemCode);
            }
            catch (Exception exception)
            {
                Logger.AddEvent(TraceEventType.Error,
                    "ConnectionManager.NotifyExchangeToConnect\r\nstateServerUrl:{0}\r\nexchangeSystemCode:{1}\r\n{2}",
                    stateServerUrl, exchangeSystemCode, exception);
            }
            if(callFailed)
            {
                this._MonitorTimer.Change(5000, 30000);
            }
        }

        private void Monitor(object state)
        {
            try
            {
                bool allConnected = true;
                foreach (var exchangeSystem in this._ExchangeSystems.Values)
                {
                    if (exchangeSystem.ConnectionState == ConnectionState.Disconnected)
                    {
                        this.NotifyExchangeToConnect(exchangeSystem.StateServerUrl, exchangeSystem.ExchangeCode);
                        allConnected = false;
                    }
                }
                if (allConnected)
                {
                    this._MonitorTimer.Change(Timeout.Infinite, Timeout.Infinite);
                }
            }
            catch (Exception exception)
            {
                Logger.AddEvent(TraceEventType.Error, "ConnectionManager.Monitor\r\n{0}", exception);
            }
        }
    }
}
