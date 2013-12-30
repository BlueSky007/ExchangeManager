using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ManagerService.Console;
using ManagerService.Quotation;
using ManagerService.Exchange;
using ManagerService.RiskManage;
using ManagerService.SettingScheduler;

namespace ManagerService
{
    public class MainService
    {
        public MainService()
        {
            MainService.ManagerSettings = ManagerSettings.Load();
            MainService.ExchangeManager = new ExchangeManager(MainService.ManagerSettings.ExchangeSystems);
            MainService.ClientManager = new ClientManager();
            MainService.QuotationManager = new QuotationManager();
            MainService.SettingSchedulerManager = new SettingSchedulerManager();
        }

        public static ManagerSettings ManagerSettings { get; private set; }
        public static ExchangeManager ExchangeManager { get; private set; }
        public static ClientManager ClientManager { get; private set; }
        public static QuotationManager QuotationManager { get; private set; }
        public static SettingSchedulerManager SettingSchedulerManager { get; private set; }

        public void Start()
        {
            MainService.QuotationManager.Start(MainService.ManagerSettings.QuotationListenPort);
            MainService.ExchangeManager.Start(MainService.ManagerSettings.ServiceAddressForExchange);
            MainService.ClientManager.Start(MainService.ManagerSettings.ServiceAddressForConsole);
            MainService.SettingSchedulerManager.Start();
        }

        public void Stop()
        {
        }
    }
}
