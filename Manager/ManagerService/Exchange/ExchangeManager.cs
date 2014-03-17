using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using Manager.Common;
using Manager.Common.QuotationEntities;
using System.Xml;

namespace ManagerService.Exchange
{
    public class ExchangeManager
    {
        private ServiceHost _ServiceHost;
        private ConnectionManager _ConnectionManager;
        private Dictionary<string, ExchangeSystem> _ExchangeSystems = new Dictionary<string,ExchangeSystem>();
        private Dictionary<string, ConnectionState> _ExchangeConnectionStates = new Dictionary<string, ConnectionState>();

        public ExchangeManager(ExchangeSystemSetting[] exchangeSystemSettings)
        {
            this._ConnectionManager = new ConnectionManager(this._ExchangeSystems);
            for (int i = 0; i < exchangeSystemSettings.Length; i++)
            {
                this._ExchangeSystems.Add(exchangeSystemSettings[i].Code, new ExchangeSystem(exchangeSystemSettings[i], this._ConnectionManager));
                this._ExchangeConnectionStates.Add(exchangeSystemSettings[i].Code, ConnectionState.Unknown);
            }
        }

        public Dictionary<string, ConnectionState> ExchangeConnectionStates { get { return this._ExchangeConnectionStates; } }

        public void Start(string serviceAddress)
        {
            this._ServiceHost = new ServiceHost(typeof(ExchangeService));
            NetTcpBinding binding = new NetTcpBinding(SecurityMode.None) { ReceiveTimeout = TimeSpan.FromHours(10) };
            this._ServiceHost.AddServiceEndpoint("ManagerService.Exchange.IExchangeService", binding, serviceAddress);
            ServiceHelper.AddWcfErrorLog(this._ServiceHost);
            this._ServiceHost.Open();
            this.NotifyExchangeManagerStarted();
        }

        public void Stop()
        {
            foreach (ExchangeSystem exchangeSystem in this._ExchangeSystems.Values)
            {
                exchangeSystem.Stop();
            }
            this._ConnectionManager.Stop();
            this._ServiceHost.Close();
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

        public void NotifyExchangeConnectionStatus(string exchangeCode, ConnectionState state)
        {
            this._ExchangeConnectionStates[exchangeCode] = state;
            MainService.ClientManager.Dispatch(new ExchangeConnectionStatusMessage() { ExchangeCode = exchangeCode, ConnectionState = state });
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
            string value = string.Empty;
            if (set.type == InstrumentQuotationEditType.IsPriceEnabled || set.type == InstrumentQuotationEditType.IsOriginHiLo || set.type == InstrumentQuotationEditType.IsAutoFill || set.type == InstrumentQuotationEditType.IsAutoEnablePrice)
            {
                value = XmlConvert.ToString((bool)set.Value);
            }
            else
            {
                value = set.Value.ToString();
            }
            task.ExchangeSettings.Add(new iExchange.Common.Manager.ExchangeSetting { Id = set.InstrumentId, ParameterKey = Enum.GetName(typeof(InstrumentQuotationEditType), set.type), ParameterValue = value });
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
                    set.ParameterValue = XmlConvert.ToString(resume);
                    task.ExchangeSettings.Add(set);
                }
                foreach (Guid id in instruments[system])
                {
                    iExchange.Common.Manager.ExchangeSetting set = new iExchange.Common.Manager.ExchangeSetting();
                    set.Id = id;
                    set.ParameterKey = "IsAutoEnablePrice";
                    set.ParameterValue = XmlConvert.ToString(resume);
                    task.ExchangeSettings.Add(set);
                }
                if (!this._ExchangeSystems[system].UpdateInstrument(task))
                {
                    isSuccess = false;
                }
            }
            return isSuccess;
        }

        public bool UpdateQuotationServer(string exchangeCode,string xmlUpdateStr)
        {
            bool isSuccess = false;
            isSuccess = this._ExchangeSystems[exchangeCode].UpdateQuotationServer(xmlUpdateStr);
            return isSuccess;
        }

        public UpdateHighLowBatchProcessInfo UpdateHighLow(string exchangeCode, Guid instrumentId, bool isOriginHiLo, string newInput, bool isUpdateHigh)
        {
            return this._ExchangeSystems[exchangeCode].UpdateHighLow(instrumentId, isOriginHiLo, newInput, isUpdateHigh);
        }

        public bool FixOverridedQuotationHistory(string exchangeCode, string quotation, bool needApplyAutoAdjustPoints)
        {
            iExchange.Common.OriginQuotation[] originQs;
            iExchange.Common.OverridedQuotation[] overridedQs;
            bool needBroadcastQuotation;
            XmlNode fixChartDatas;
            return this._ExchangeSystems[exchangeCode].FixOverridedQuotationHistory(new iExchange.Common.Token(), quotation, needApplyAutoAdjustPoints, out originQs, out overridedQs, out needBroadcastQuotation, out fixChartDatas);
        }

        public bool RestoreHighLow(string exchangeCode, int batchProcessId,out string errorMessage)
        {
            return this._ExchangeSystems[exchangeCode].RestoreHighLow(batchProcessId, out errorMessage);
        }
    }
}
