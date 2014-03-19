using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using ManagerService.Console;
using ManagerService.Quotation;
using ManagerService.Exchange;
using ManagerService.RiskManage;
using Manager.Common;
using ManagerService.SettingsTaskManager;

namespace ManagerService
{
    public class MainService
    {
        public MainService()
        {
            try
            {
                MainService.IsShuttingDown = false;
                System.IO.Directory.SetCurrentDirectory(System.AppDomain.CurrentDomain.BaseDirectory);
                MainService.ManagerSettings = ManagerSettings.Load();
                MainService.ExchangeManager = new ExchangeManager(MainService.ManagerSettings.ExchangeSystems);
                MainService.ClientManager = new ClientManager();
                MainService.QuotationManager = new QuotationManager();
                MainService.SettingsTaskSchedulerManager = new SettingsTaskSchedulerManager();
                Logger.AddEvent(TraceEventType.Information, "Manager Service constructed.");
            }
            catch (Exception exception)
            {
                Logger.TraceEvent(TraceEventType.Error, "Manager Service construct failed.\r\n{0}", exception);
            }
        }

        public static ManagerSettings ManagerSettings { get; private set; }
        public static ExchangeManager ExchangeManager { get; private set; }
        public static ClientManager ClientManager { get; private set; }
        public static QuotationManager QuotationManager { get; private set; }
        public static SettingsTaskSchedulerManager SettingsTaskSchedulerManager { get; private set; }
        public static bool IsShuttingDown { get; private set; }

        public void Start()
        {
            try
            {
                MainService.QuotationManager.Start(MainService.ManagerSettings.QuotationListenPort);
                MainService.ExchangeManager.Start(MainService.ManagerSettings.ServiceAddressForExchange);
                MainService.ClientManager.Start(MainService.ManagerSettings.ServiceAddressForConsole);
                MainService.SettingsTaskSchedulerManager.Start();
                Logger.AddEvent(TraceEventType.Information, "Manager started, pid:{0}", Process.GetCurrentProcess().Id);
            }
            catch (Exception exception)
            {
                Logger.TraceEvent(TraceEventType.Error, "Manager Service Start failed.\r\n{0}", exception);
            }
        }

        public void Stop()
        {
            try
            {
                MainService.IsShuttingDown = true;
                MainService.QuotationManager.Stop();
                MainService.ExchangeManager.Stop();
                MainService.ClientManager.Stop();
                MainService.SettingsTaskSchedulerManager.Stop();
                Logger.TraceEvent(TraceEventType.Information, "Manager Service stopped.");
            }
            catch (Exception exception)
            {
                Logger.TraceEvent(TraceEventType.Error, "MainService.Stop exception\r\n{0}", exception);
            }
  
        }
    }
}
