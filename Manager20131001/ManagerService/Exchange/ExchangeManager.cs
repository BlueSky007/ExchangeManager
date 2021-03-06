﻿using Manager.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;

namespace ManagerService.Exchange
{
    public class ExchangeManager
    {
        private ServiceHost _ServiceHost;
        private ConnectionManager _ConnectionManager;
        private Dictionary<string, ExchangeSystem> _ExchangeSystems = new Dictionary<string,ExchangeSystem>();

        public ExchangeManager(ExchangeSystemSetting[] exchangeSystemSettings)
        {
            for (int i = 0; i < exchangeSystemSettings.Length; i++)
            {
                this._ExchangeSystems.Add(exchangeSystemSettings[i].Code, new ExchangeSystem(exchangeSystemSettings[i]));
            }
            this._ConnectionManager = new ConnectionManager(this._ExchangeSystems);
        }

        public void Start(string serviceAddress)
        {
            this._ServiceHost = new ServiceHost(typeof(ExchangeService));
            NetTcpBinding binding = new NetTcpBinding(SecurityMode.None) { ReceiveTimeout = TimeSpan.FromHours(10) };
            this._ServiceHost.AddServiceEndpoint("ManagerService.Exchange.IExchangeService", binding, serviceAddress);
            this._ServiceHost.Open();
            this.NotifyExchangeManagerStarted();
        }

        public void NotifyExchangeManagerStarted()
        {
            this._ConnectionManager.NotifyExchangeManagerStarted();
        }

        public bool TryRegister(string exchangeCode, string sessionId, IStateServer stateServer, out ExchangeSystem exchangeSystem)
        {
            exchangeSystem = null;
            if (this._ExchangeSystems.TryGetValue(exchangeCode, out exchangeSystem))
            {
                exchangeSystem.Replace(sessionId, stateServer);
            }
            else
            {
                Logger.AddEvent(TraceEventType.Error, "ExchangeManager.Register Invalid exchangeCode:" + exchangeCode);
            }
            return exchangeSystem != null;
        }

        public void ProcessQuotation(List<PrimitiveQuotation> quotations)
        {
            foreach (ExchangeSystem exchangeSystem in this._ExchangeSystems.Values)
            {
                exchangeSystem.ProcessQuotation(quotations);
            }
        }
        public ExchangeSystem GetExchangeSystem(string exchangeCode)
        {
            ExchangeSystem exchangeSystem = null;
            if (this._ExchangeSystems.TryGetValue(exchangeCode, out exchangeSystem))
            {
                return exchangeSystem;
            }
            else
            {
                return null;
            }
        }
    }
}
