using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ManagerService.Console;
using ManagerService.Quotation;
using ManagerService.Exchange;
using ManagerService.RiskManage;

namespace ManagerService
{
    public class Manager
    {
        public Manager()
        {
            Manager.ManagerSettings = ManagerSettings.Load();
            Manager.ExchangeManager = new ExchangeManager(Manager.ManagerSettings.ExchangeSystems);
            Manager.ClientManager = new ClientManager();
            Manager.QuotationManager = new QuotationManager();
        }

        public static ManagerSettings ManagerSettings { get; private set; }
        public static ExchangeManager ExchangeManager { get; private set; }
        public static ClientManager ClientManager { get; private set; }
        public static QuotationManager QuotationManager { get; private set; }

        public void Start()
        {
            Manager.QuotationManager.Start(Manager.ManagerSettings.QuotationListenPort);
            Manager.ExchangeManager.Start(Manager.ManagerSettings.ServiceAddressForExchange);
            Manager.ClientManager.Start(Manager.ManagerSettings.ServiceAddressForConsole);
        }

        public void Stop()
        {
        }
    }
}
