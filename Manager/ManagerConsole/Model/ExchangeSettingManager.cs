using ManagerConsole.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SettingSet = Manager.Common.SettingSet;
using Logger = Manager.Common.Logger;
using UpdateAction = Manager.Common.UpdateAction;
using CommonCustomer = Manager.Common.Settings.Customer;
using CommonQuotePolicy = Manager.Common.Settings.QuotePolicy;
using CommonQuotePolicyDetail = Manager.Common.Settings.QuotePolicyDetail;
using CommonInstrument = Manager.Common.Settings.Instrument;
using CommonOrder = iExchange.Common.Manager.Order;
using CommonAccount = Manager.Common.Settings.Account;
using CommonTradePolicy = Manager.Common.Settings.TradePolicy;
using CommonTradePolicyDetail = Manager.Common.Settings.TradePolicyDetail;
using CommonAccountGroup = Manager.Common.Settings.AccountGroup;
using Manager.Common.QuotationEntities;
using SettingsParameter = Manager.Common.Settings.SettingsParameter;
using ParameterSetting = Manager.Common.Settings.DealingOrderParameter;
using SoundSetting = Manager.Common.Settings.SoundSetting;
using SetValueSetting = Manager.Common.Settings.SetValueSetting;
using System.Xml.Linq;
using Manager.Common.ExchangeEntities;

namespace ManagerConsole.Model
{
    public class ExchangeSettingManager
    {
        public delegate void SettingsChangedEventHandler(object sender, SettingsChangedEventArgs eventArgs);
        public event SettingsChangedEventHandler SettingsChanged;
        private string _ExchangeCode;
        private Dictionary<Guid, Customer> _Customers = new Dictionary<Guid, Customer>();
        private Dictionary<Guid, Account> _Accounts = new Dictionary<Guid, Account>();
        private Dictionary<Guid, AccountGroup> _AccountGroups = new Dictionary<Guid, AccountGroup>();
        private Dictionary<Guid, InstrumentClient> _Instruments = new Dictionary<Guid, InstrumentClient>();
        private Dictionary<Guid, QuotePolicy> _QuotePolicies = new Dictionary<Guid, QuotePolicy>();
        private Dictionary<Guid, Dictionary<Guid, ExchangeQuotation>> _ExchangeQuotations = new Dictionary<Guid, Dictionary<Guid, ExchangeQuotation>>();

        // Map for QuotePolicyId - InstrumentId - QuotePolicyDetail
        private Dictionary<Guid, Dictionary<Guid, QuotePolicyDetail>> _QuotePolicyDetails = new Dictionary<Guid, Dictionary<Guid, QuotePolicyDetail>>();
        private Dictionary<Guid, TradePolicy> _TradePolicies = new Dictionary<Guid, TradePolicy>();
        // Map for TradePolicyId - InstrumentId - TradePolicyDetail
        private Dictionary<Guid, Dictionary<Guid, TradePolicyDetail>> _TradePolicyDetails = new Dictionary<Guid, Dictionary<Guid, TradePolicyDetail>>();
        private Dictionary<Guid, Dictionary<Guid, DealingPolicyDetail>> _DealingPolicyDetails = new Dictionary<Guid, Dictionary<Guid, DealingPolicyDetail>>();

        public ExchangeSettingManager(string exchangeCode)
        {
            Toolkit.SettingsManager = this;
            this._ExchangeCode = exchangeCode;
        }

        #region Public Property
        public string ExchangeCode
        {
            get { return this._ExchangeCode; }
        }

        public string Language
        {
            get;
            set;
        }

        public SystemParameter SystemParameter
        {
            get;
            private set;
        }

        public Dictionary<Guid, InstrumentClient> Instruments
        {
            get { return this._Instruments; }
            set { this._Instruments = value; }
        }

        public Dictionary<Guid, Account> Accounts
        {
            get { return this._Accounts; }
            set { this._Accounts = value; }
        }

        public Dictionary<Guid, Dictionary<Guid, ExchangeQuotation>> ExchangeQuotations
        {
            get { return this._ExchangeQuotations; }
            set { this._ExchangeQuotations = value; }
        }
        #endregion

        public void Initialize(SettingSet settingSet)
        {
            try
            {
                this.Update(settingSet, UpdateAction.Initialize);
                this._ExchangeQuotations = this.InitExchangeQuotation(settingSet);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "SettingsManager.Initialize.\r\n{0}", ex.ToString());
            }
        }

        private Dictionary<Guid, Dictionary<Guid, ExchangeQuotation>> InitExchangeQuotation(SettingSet set)
        {
            try
            {
                Dictionary<Guid, Dictionary<Guid, ExchangeQuotation>> quotations = new Dictionary<Guid, Dictionary<Guid, ExchangeQuotation>>();
                foreach (Manager.Common.Settings.QuotePolicyDetail item in set.QuotePolicyDetails)
                {
                    Manager.Common.Settings.Instrument instrument = set.Instruments.SingleOrDefault(i => i.Id == item.InstrumentId);
                    ExchangeQuotation quotation = new ExchangeQuotation(item, instrument);
                    quotation.QuotationPolicyCode = set.QuotePolicies.SingleOrDefault(q => q.Id == item.QuotePolicyId).Code;
                    Manager.Common.Settings.OverridedQuotation overridedQuotation = set.OverridedQuotations.SingleOrDefault(o => o.QuotePolicyId == item.QuotePolicyId && o.InstrumentId == item.InstrumentId);
                    if (overridedQuotation != null)
                    {
                        quotation.Ask = overridedQuotation.Ask;
                        quotation.Bid = overridedQuotation.Bid;
                        quotation.High = overridedQuotation.High;
                        quotation.Low = overridedQuotation.Low;
                        quotation.Origin = overridedQuotation.Origin;
                        quotation.Timestamp = overridedQuotation.Timestamp;
                    }
                    if (quotations.ContainsKey(item.QuotePolicyId))
                    {
                        quotations[item.QuotePolicyId].Add(item.InstrumentId, quotation);
                    }
                    else
                    {
                        quotations.Add(item.QuotePolicyId, new Dictionary<Guid, ExchangeQuotation>());
                        quotations[item.QuotePolicyId].Add(item.InstrumentId, quotation);
                    }
                }

                return quotations;
            }
            catch (Exception ex)
            {
                Manager.Common.Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "InitExchangeQuotation.\r\n{0}", ex.ToString());
                return null;
            }
        }

        public void UpdateNotify(SettingSet addSet, SettingSet deletedSet, ExchangeUpdateData[] modifySet)
        {
            //SettingsChangedEventArgs settingsChangedEventArgs = this.CreateSettingsChangedEventArgs(addSet, deletedSet, modifySet);
            if (deletedSet != null)
            {
                this.Update(deletedSet, UpdateAction.Delete);
            }
            if (modifySet != null)
            {
                this.Update(modifySet);
            }
            if (addSet != null)
            {
                this.Update(addSet, UpdateAction.Add);
            }

            //if (settingsChangedEventArgs != null)
            //{
            //    SettingsChangedEventHandler settingsChanged = this.SettingsChanged;
            //    if (settingsChanged != null) settingsChanged(this, settingsChangedEventArgs);
            //}
        }

        private void Update(ExchangeUpdateData[] modifySet)
        {
            for (int i = 0; i < modifySet.Length; i++)
            {
                switch (modifySet[i].ExchangeMetadataType)
                {
                    case ExchangeMetadataType.Instrument:
                        Guid instrumentId = Guid.Parse(modifySet[i].FieldsAndValues[ExchangeFieldSR.ID]);
                        this._Instruments[instrumentId].Update(modifySet[i].FieldsAndValues);
                        foreach (Dictionary<Guid, ExchangeQuotation> instrument in this._ExchangeQuotations.Values)
                        {
                            if (instrument.ContainsKey(instrumentId))
                            {
                                instrument[instrumentId].Update(modifySet[i].FieldsAndValues, this._ExchangeCode);
                            }
                        }
                        break;
                    case ExchangeMetadataType.Account:
                        Guid accountId = Guid.Parse(modifySet[i].FieldsAndValues[ExchangeFieldSR.ID]);
                        this._Accounts[accountId].Update(modifySet[i].FieldsAndValues);
                        break;
                    case ExchangeMetadataType.Customer:
                        Guid customerId = Guid.Parse(modifySet[i].FieldsAndValues[ExchangeFieldSR.ID]);
                        this._Customers[customerId].Update(modifySet[i].FieldsAndValues);
                        break;
                    case ExchangeMetadataType.SystemParameter:
                        this.SystemParameter.Update(modifySet[i].FieldsAndValues);
                        break;
                    case ExchangeMetadataType.QuotePolicy:
                        Guid quotePolicyId = Guid.Parse(modifySet[i].FieldsAndValues[ExchangeFieldSR.ID]);
                        this._QuotePolicies[quotePolicyId].Update(modifySet[i].FieldsAndValues);
                        break;
                    case ExchangeMetadataType.QuotePolicyDetail:
                        instrumentId = Guid.Parse(modifySet[i].FieldsAndValues[ExchangeFieldSR.InstrumentId]);
                        quotePolicyId = Guid.Parse(modifySet[i].FieldsAndValues[ExchangeFieldSR.QuotePolicyId]);

                        QuotePolicyDetail quotePolicyDetail = this.GetQuotePolicyDetail(quotePolicyId, instrumentId);
                        quotePolicyDetail.Update(modifySet[i].FieldsAndValues);
                        break;
                    case ExchangeMetadataType.TradePolicy:
                        Guid tradePolicyId = Guid.Parse(modifySet[i].FieldsAndValues[ExchangeFieldSR.ID]);
                        this._TradePolicies[tradePolicyId].Update(modifySet[i].FieldsAndValues);
                        break;
                    case ExchangeMetadataType.TradePolicyDetail:
                        instrumentId = Guid.Parse(modifySet[i].FieldsAndValues[ExchangeFieldSR.InstrumentId]);
                        tradePolicyId = Guid.Parse(modifySet[i].FieldsAndValues[ExchangeFieldSR.TradePolicyId]);

                        TradePolicyDetail tradePolicyDetail = this.GetTradePolicyDetail(tradePolicyId, instrumentId);
                        tradePolicyDetail.Update(modifySet[i].FieldsAndValues);
                        break;
                    default:
                        break;
                }
            }
        }

        public void Update(SettingSet settingSet, UpdateAction action)
        {
            if (action == UpdateAction.Initialize)
            {
                this.SystemParameter = new SystemParameter(settingSet.SystemParameter);
            }
            if (settingSet.Instruments != null)
            {
                foreach (CommonInstrument instrument in settingSet.Instruments)
                {
                    ExchangeDataManagerHelper.UpdateEntity<CommonInstrument, InstrumentClient>
                            (instrument, this._Instruments, action, instrument.Id, ExchangeDataManagerHelper.UpdateManagerEntity);
                }
            }
            if (settingSet.AccountGroups != null)
            {
                foreach (CommonAccountGroup accountGroup in settingSet.AccountGroups)
                {
                    ExchangeDataManagerHelper.UpdateEntity<CommonAccountGroup, AccountGroup>
                        (accountGroup,this._AccountGroups, action, accountGroup.Id, ExchangeDataManagerHelper.UpdateManagerEntity);
                }
            }
            if (settingSet.Accounts != null)
            {
                foreach (CommonAccount account in settingSet.Accounts)
                {
                    ExchangeDataManagerHelper.UpdateEntity<CommonAccount, Account>
                        (account, this._Accounts, action, account.Id, ExchangeDataManagerHelper.UpdateManagerEntity);
                }
            }

            if (settingSet.Customers != null)
            {
                foreach (CommonCustomer customer in settingSet.Customers)
                {
                    ExchangeDataManagerHelper.UpdateEntity<CommonCustomer, Customer>
                        (customer, this._Customers, action, customer.Id, ExchangeDataManagerHelper.UpdateManagerEntity);
                }
            }

            if (settingSet.QuotePolicies != null)
            {
                foreach (CommonQuotePolicy quotePolicy in settingSet.QuotePolicies)
                {
                    ExchangeDataManagerHelper.UpdateEntity<CommonQuotePolicy, QuotePolicy>
                        (quotePolicy, this._QuotePolicies, action, quotePolicy.Id, ExchangeDataManagerHelper.UpdateManagerEntity);
                }
            }
      
            if (settingSet.QuotePolicyDetails != null)
            {
                foreach (CommonQuotePolicyDetail quotePolicyDetail in settingSet.QuotePolicyDetails)
                {
                    if (action == UpdateAction.Initialize || action == UpdateAction.Add)
                    {
                        Dictionary<Guid, QuotePolicyDetail> quotePolicyDetails = null;
                        if (!this._QuotePolicyDetails.TryGetValue(quotePolicyDetail.QuotePolicyId, out quotePolicyDetails))
                        {
                            quotePolicyDetails = new Dictionary<Guid, QuotePolicyDetail>();
                            this._QuotePolicyDetails.Add(quotePolicyDetail.QuotePolicyId, quotePolicyDetails);
                        }
                        quotePolicyDetails[quotePolicyDetail.InstrumentId] = new QuotePolicyDetail(quotePolicyDetail);
                    }
                    else if (action == UpdateAction.Modify)
                    {
                        Dictionary<Guid, QuotePolicyDetail> quotePolicyDetails = null;
                        if (this._QuotePolicyDetails.TryGetValue(quotePolicyDetail.QuotePolicyId, out quotePolicyDetails)
                            && quotePolicyDetails.ContainsKey(quotePolicyDetail.InstrumentId))
                        {
                            quotePolicyDetails[quotePolicyDetail.InstrumentId].Update(quotePolicyDetail);
                        }
                    }
                    else if (action == UpdateAction.Delete)
                    {
                        Dictionary<Guid, QuotePolicyDetail> quotePolicyDetails = null;
                        if (this._QuotePolicyDetails.TryGetValue(quotePolicyDetail.QuotePolicyId, out quotePolicyDetails))
                        {
                            quotePolicyDetails.Remove(quotePolicyDetail.QuotePolicyId);
                        }
                    }
                }
            }

            if (settingSet.TradePolicies != null)
            {
                foreach (CommonTradePolicy tradePolicy in settingSet.TradePolicies)
                {
                    if (action == UpdateAction.Initialize || action == UpdateAction.Add)
                    {
                        this._TradePolicies[tradePolicy.Id] = new TradePolicy(tradePolicy);
                    }
                    else if (action == UpdateAction.Delete)
                    {
                        this._TradePolicies.Remove(tradePolicy.Id);
                    }
                }
            }

            if (settingSet.TradePolicyDetails != null)
            {
                foreach (CommonTradePolicyDetail tradePolicyDetail in settingSet.TradePolicyDetails)
                {
                    if (action == UpdateAction.Initialize || action == UpdateAction.Add)
                    {
                        Dictionary<Guid, TradePolicyDetail> tradePolicyDetails = null;
                        if (!this._TradePolicyDetails.TryGetValue(tradePolicyDetail.TradePolicyId, out tradePolicyDetails))
                        {
                            tradePolicyDetails = new Dictionary<Guid, TradePolicyDetail>();
                            this._TradePolicyDetails.Add(tradePolicyDetail.TradePolicyId, tradePolicyDetails);
                        }
                        tradePolicyDetails[tradePolicyDetail.InstrumentId] = new TradePolicyDetail(tradePolicyDetail);
                    }
                    else if (action == UpdateAction.Modify)
                    {
                        Dictionary<Guid, TradePolicyDetail> tradePolicyDetails = null;
                        if (this._TradePolicyDetails.TryGetValue(tradePolicyDetail.TradePolicyId, out tradePolicyDetails)
                            && tradePolicyDetails.ContainsKey(tradePolicyDetail.InstrumentId))
                        {
                            tradePolicyDetails[tradePolicyDetail.InstrumentId].Update(tradePolicyDetail);
                        }
                    }
                    else if (action == UpdateAction.Delete)
                    {
                        Dictionary<Guid, TradePolicyDetail> tradePolicyDetails = null;
                        if (this._TradePolicyDetails.TryGetValue(tradePolicyDetail.TradePolicyId, out tradePolicyDetails))
                        {
                            tradePolicyDetails.Remove(tradePolicyDetail.TradePolicyId);
                        }
                    }
                }
            }
        }

        private SettingsChangedEventArgs CreateSettingsChangedEventArgs(SettingSet addedSet, SettingSet deletedSet, SettingSet modifiedSet)
        {

            if (this.SettingsChanged != null)
            {
                List<Account> changedAccounts = new List<Account>();
                List<Account> removedAccounts = new List<Account>();
                List<InstrumentClient> addedInstruments = new List<InstrumentClient>();
                List<InstrumentClient> removedInstruments = new List<InstrumentClient>();
                List<InstrumentClient> changedInstruments = new List<InstrumentClient>();

                if (addedSet != null && addedSet.Instruments != null && addedSet.Instruments.Count() > 0)
                {
                    for (int index = 0; index < addedSet.Instruments.Length; index++)
                    {
                        if (this._Instruments.ContainsKey(addedSet.Instruments[index].Id))
                        {
                            addedInstruments.Add(this._Instruments[addedSet.Instruments[index].Id]);
                        }
                    }
                }
                if (deletedSet != null)
                {
                    if (deletedSet.Instruments != null && deletedSet.Instruments.Length > 0)
                    {
                        for (int index = 0; index < deletedSet.Instruments.Length; index++)
                        {
                            if (this._Instruments.ContainsKey(deletedSet.Instruments[index].Id))
                            {
                                removedInstruments.Add(this._Instruments[deletedSet.Instruments[index].Id]);
                            }
                        }
                    }
                    if (deletedSet.Accounts != null && deletedSet.Accounts.Length > 0)
                    {
                        for (int index = 0; index < deletedSet.Accounts.Length; index++)
                        {
                            if (this._Accounts.ContainsKey(deletedSet.Accounts[index].Id))
                            {
                                removedAccounts.Add(this._Accounts[deletedSet.Accounts[index].Id]);
                            }
                        }
                    }
                }

                if (modifiedSet != null)
                {
                    if (modifiedSet.Accounts != null && modifiedSet.Accounts.Length > 0)
                    {
                        for (int index = 0; index < modifiedSet.Accounts.Length; index++)
                        {
                            if (this._Accounts.ContainsKey(modifiedSet.Accounts[index].Id))
                            {
                                changedAccounts.Add(this._Accounts[modifiedSet.Accounts[index].Id]);
                            }
                        }
                    }

                    if (modifiedSet.Instruments != null && modifiedSet.Instruments.Length > 0)
                    {
                        for (int index = 0; index < modifiedSet.Instruments.Length; index++)
                        {
                            if (this._Instruments.ContainsKey(modifiedSet.Instruments[index].Id))
                            {
                                changedInstruments.Add(this._Instruments[modifiedSet.Instruments[index].Id]);
                            }
                        }
                    }
                }
                SettingsChangedEventArgs settingsChangedEventArgs
                    = new SettingsChangedEventArgs(changedAccounts.ToArray(), removedAccounts.ToArray(), addedInstruments.ToArray(), removedInstruments.ToArray(), changedInstruments.ToArray());

                return settingsChangedEventArgs;
            }
            else
            {
                return null;
            }
        }


        #region 辅助方法
        internal bool TryGetAccount(Guid id, out Account account)
        {
            return this._Accounts.TryGetValue(id, out account);
        }

        internal Account GetAccount(Guid id)
        {
            return this._Accounts[id];
        }

        internal Customer GetCustomer(Guid id)
        {
            return this._Customers[id];
        }

        internal bool ContainsInstrument(Guid id)
        {
            return this._Instruments.ContainsKey(id);
        }

        internal InstrumentClient GetInstrument(Guid id)
        {
            return this._Instruments[id];
        }

        internal ICollection<InstrumentClient> GetInstruments()
        {
            return new List<InstrumentClient>(this._Instruments.Values);
        }

        internal IList<Account> GetAccounts()
        {
            return new List<Account>(this._Accounts.Values);
        }
        internal IList<Customer> GetCustomers()
        {
            return new List<Customer>(this._Customers.Values);
        }
        internal IList<AccountGroup> GetAccountGroups()
        {
            return new List<AccountGroup>(this._AccountGroups.Values);
        }
        internal TradePolicyDetail GetTradePolicyDetail(Guid tradePolicyId, Guid instrumentId)
        {
            return this._TradePolicyDetails.ContainsKey(tradePolicyId) && this._TradePolicyDetails[tradePolicyId].ContainsKey(instrumentId) ? this._TradePolicyDetails[tradePolicyId][instrumentId] : null;
        }

        internal QuotePolicyDetail GetQuotePolicyDetail(Guid quotePolicyId, Guid instrumentId)
        {
            QuotePolicyDetail quotePolicyDetail = null;
            Dictionary<Guid, QuotePolicyDetail> quotePolicyDetails;

            if (this._QuotePolicyDetails.TryGetValue(quotePolicyId, out quotePolicyDetails)
                && quotePolicyDetails.TryGetValue(instrumentId, out quotePolicyDetail))
            {
                return quotePolicyDetail;
            }
            else
            {
                return null;
            }
        }

        internal QuotePolicyDetail GetQuotePolicyDetail(Guid instrumentId,Customer customer)
        {
            QuotePolicyDetail quotePolicyDetail = null;
            Dictionary<Guid, QuotePolicyDetail> quotePolicyDetails;

            if (this._QuotePolicyDetails.TryGetValue(customer.PrivateQuotePolicyId, out quotePolicyDetails)
                && quotePolicyDetails.TryGetValue(instrumentId, out quotePolicyDetail))
            {
                return quotePolicyDetail;
            }
            else if (this._QuotePolicyDetails.TryGetValue(customer.PublicQuotePolicyId, out quotePolicyDetails)
                && quotePolicyDetails.TryGetValue(instrumentId, out quotePolicyDetail))
            {
                return quotePolicyDetail;
            }
            else
            {
                //return null;
                //just test
                quotePolicyDetails = this._QuotePolicyDetails[new Guid("DE03F6DA-C501-4B87-8E5E-09733ACB4FDD")];
                return quotePolicyDetails[new Guid("547EE4C0-7292-4611-AB1D-14CE326792EE")];
            }
        }

        #endregion
    }

    internal static class ExchangeDataManagerHelper
                {
        //T1 --Manager.Commn Object,T2 ManagerConsole object
        internal delegate void UpdateManagerCommon<T2, T1>(T2 value, T1 settingValue, UpdateAction action);
        internal static void UpdateEntity<T1, T2>(T1 value, Dictionary<Guid, T2> newValues, UpdateAction action, Guid id, UpdateManagerCommon<T2, T1> update)
            where T1:new()
            where T2:new()
        {
            if (action == UpdateAction.Initialize)
            {
                newValues[id] = new T2();
                update(newValues[id], value, action);
            }
            else if (action == UpdateAction.Modify)
            {
                if (newValues.ContainsKey(id))
                {
                    update(newValues[id], value, action);
                }
            }
            else if (action == UpdateAction.Delete)
            {
                newValues.Remove(id);
            }
        }

        #region 更新静态方法
        internal static void UpdateManagerEntity(this InstrumentClient instrument, CommonInstrument commonInstrument, UpdateAction action)
        {
            instrument.Initialize(commonInstrument);
        }
        internal static void UpdateManagerEntity(this AccountGroup accountGroup, CommonAccountGroup commonAccountGroup, UpdateAction action)
        {
            accountGroup.Initialize(commonAccountGroup);
        }
        internal static void UpdateManagerEntity(this Account account, CommonAccount commonAccount, UpdateAction action)
        {
            account.Initialize(commonAccount);
        }
        internal static void UpdateManagerEntity(this Customer customer, CommonCustomer commonCustomer, UpdateAction action)
        {
            customer.Initialize(commonCustomer);
        }
        internal static void UpdateManagerEntity(this QuotePolicy quotePolicy, CommonQuotePolicy commonQuotePolicy, UpdateAction action)
        {
            quotePolicy.Initialize(commonQuotePolicy);
        }


        #endregion


    }
}
