using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using Manager.Common;
using Manager.Common.QuotationEntities;

namespace ManagerService.Exchange
{
    public class ExchangeManager
    {
        private ServiceHost _ServiceHost;
        private ConnectionManager _ConnectionManager;
        private Dictionary<string, ExchangeSystem> _ExchangeSystems = new Dictionary<string,ExchangeSystem>();

        public ExchangeManager(ExchangeSystemSetting[] exchangeSystemSettings)
        {
            this._ConnectionManager = new ConnectionManager(this._ExchangeSystems);
            for (int i = 0; i < exchangeSystemSettings.Length; i++)
            {
                this._ExchangeSystems.Add(exchangeSystemSettings[i].Code, new ExchangeSystem(exchangeSystemSettings[i], this._ConnectionManager));
            }
        }

        public ICollection<ExchangeSystem> GetExchangeSystems()
        {
            return new List<ExchangeSystem>(this._ExchangeSystems.Values);
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

        public bool AddQuotations(List<GeneralQuotation> quotations)
        {
            foreach (ExchangeSystem exchangeSystem in this._ExchangeSystems.Values)
            {
                exchangeSystem.AddQuotation(quotations);
            }
            return true;
        }

        public void SwitchPriceEnableState(int instrumentId, bool enable)
        {
            HashSet<string> originCodeSet = new HashSet<string>();
            originCodeSet.Add(MainService.QuotationManager.ConfigMetadata.Instruments[instrumentId].Code);
            MainService.QuotationManager.ConfigMetadata.GetDerivativeInstrumentCodes(instrumentId, originCodeSet);
            foreach (ExchangeSystem exchangeSystem in this._ExchangeSystems.Values)
            {
                exchangeSystem.SwitchPriceState(originCodeSet.ToArray(), enable);
            }
        }

        public void SuspendResume(int[] instrumentIds, bool resume)
        {
            HashSet<string> originCodeSet = new HashSet<string>();
            for (int i = 0; i < instrumentIds.Length; i++)
            {
                originCodeSet.Add(MainService.QuotationManager.ConfigMetadata.Instruments[instrumentIds[i]].Code);
                MainService.QuotationManager.ConfigMetadata.GetDerivativeInstrumentCodes(instrumentIds[i], originCodeSet);
            }
            
            foreach (ExchangeSystem exchangeSystem in this._ExchangeSystems.Values)
            {
                exchangeSystem.SuspendResume(originCodeSet.ToArray(), resume);
            }
        }

        public bool UpdateInstrument(InstrumentQuotationSet set)
        {
            Guid[] instruments = new Guid[1];
            instruments[0] = set.InstrumentId;
            iExchange.Common.Manager.ParameterUpdateTask task = new iExchange.Common.Manager.ParameterUpdateTask();
            task.Instruments = instruments;
            task.ExchangeSettings.Add(new iExchange.Common.Manager.ExchangeSetting { Id = set.InstrumentId, ParameterKey = Enum.GetName(typeof(InstrumentQuotationEditType), set.type), ParameterValue = set.Value.ToString() });
            return this._ExchangeSystems[set.ExchangeCode].UpdateInstrument(task);
        }

        public bool ExchangeSuspendResume(Dictionary<string, List<Guid>> instruments, bool resume)
        {
            bool isSuccess = true;
            foreach (string system in instruments.Keys)
            {
                iExchange.Common.Manager.ParameterUpdateTask task = new iExchange.Common.Manager.ParameterUpdateTask();
                task.Instruments = instruments[system].ToArray();
                foreach (Guid id in instruments[system])
                {
                    iExchange.Common.Manager.ExchangeSetting set = new iExchange.Common.Manager.ExchangeSetting();
                    set.Id = id;
                    set.ParameterKey = "IsPriceEnabled";
                    set.ParameterValue = resume.ToString();
                    task.ExchangeSettings.Add(set);
                }
                foreach (Guid id in instruments[system])
                {
                    iExchange.Common.Manager.ExchangeSetting set = new iExchange.Common.Manager.ExchangeSetting();
                    set.Id = id;
                    set.ParameterKey = "IsAutoEnablePrice";
                    set.ParameterValue = resume.ToString();
                    task.ExchangeSettings.Add(set);
                }
                if (!this._ExchangeSystems[system].UpdateInstrument(task))
                {
                    isSuccess = false;
                }
            }
            return isSuccess;
        }
    }
}
