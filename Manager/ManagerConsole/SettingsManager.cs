﻿using ManagerConsole.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SettingSet = Manager.Common.SettingSet;
using Logger = Manager.Common.Logger;
using UpdateAction = Manager.Common.UpdateAction;
using CommonCustomer = Manager.Common.Settings.Customer;
using CommonAccount = Manager.Common.Settings.Account;
using CommonInstrument = Manager.Common.Settings.Instrument;
using CommonTradePolicy = Manager.Common.Settings.TradePolicy;
using CommonTradePolicyDetail = Manager.Common.Settings.TradePolicyDetail;
using CommonAccountGroup = Manager.Common.Settings.AccountGroup;
using Manager.Common.QuotationEntities;
using SettingsParameter = Manager.Common.Settings.SettingsParameter;
using ParameterSetting = Manager.Common.Settings.ParameterSetting;
using SoundSetting = Manager.Common.Settings.SoundSetting;
using SetValueSetting = Manager.Common.Settings.SetValueSetting;
using System.Xml.Linq;

namespace ManagerConsole
{
    public class SettingsManager
    {
        public delegate void SettingsChangedEventHandler(object sender, SettingsChangedEventArgs eventArgs);
        public event SettingsChangedEventHandler SettingsChanged;

        private Dictionary<Guid, Customer> _Customers = new Dictionary<Guid, Customer>();
        private Dictionary<Guid, Account> _Accounts = new Dictionary<Guid, Account>();
        private Dictionary<Guid, AccountGroup> _AccountGroups = new Dictionary<Guid, AccountGroup>();
        private Dictionary<Guid, InstrumentClient> _Instruments = new Dictionary<Guid, InstrumentClient>();
        private Dictionary<Guid, Dictionary<Guid, QuotePolicyDetail>> _QuotePolicyDetails = new Dictionary<Guid, Dictionary<Guid, QuotePolicyDetail>>();
        
        private Dictionary<Guid, TradePolicy> _TradePolicies = new Dictionary<Guid, TradePolicy>();
        private Dictionary<Guid, Dictionary<Guid, TradePolicyDetail>> _TradePolicyDetails = new Dictionary<Guid, Dictionary<Guid, TradePolicyDetail>>();
        private Dictionary<Guid, Dictionary<Guid, DealingPolicyDetail>> _DealingPolicyDetails = new Dictionary<Guid, Dictionary<Guid, DealingPolicyDetail>>();

        private Dictionary<string, SettingsParameter> _SettingsParameters = new Dictionary<string, SettingsParameter>();
        
        public SettingsManager()
        {
            Toolkit.SettingsManager = this;

            //just test
            this._Instruments = TestData.GetInitializeTestDataForInstrument();
            this._Accounts = TestData.GetInitializeTestDataForAccount();
        }

        #region Public Property
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

        
        #endregion

        public void InitializeSettingParameter(XElement parametersXml)
        {
            string exchangeCode = parametersXml.Attribute("ExchangeCode").Value;
            SettingsParameter settings = new SettingsParameter();
            XElement parameterNode = parametersXml.Element("Parameter");
            XElement soundNode = parametersXml.Element("Sound");
            XElement setValueNode = parametersXml.Element("SetValue");
            if (parameterNode != null)
            {
                ParameterSetting parameterSetting = new ParameterSetting();
                parameterSetting.InitializeSystemParameter(parameterNode);
                settings.ParameterSetting = parameterSetting;
            }
            if (soundNode != null)
            {
            }
            if (setValueNode != null)
            {
                SetValueSetting setValueSetting = new SetValueSetting();
                setValueSetting.InitializeSystemParameter(setValueNode);
                settings.SetValueSetting = setValueSetting;
            }

            Toolkit.AddDictionary<SettingsParameter>(exchangeCode, settings, this._SettingsParameters);
        }

        public void Initialize(SettingSet settingSet)
        {
            try
            {
                this.Update(settingSet, UpdateAction.Initialize);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "SettingsManager.Initialize.\r\n{0}", ex.ToString());
            }
        }

        public void UpdateNotify(SettingSet addSet, SettingSet deletedSet, SettingSet modifySet)
        {
            SettingsChangedEventArgs settingsChangedEventArgs = this.CreateSettingsChangedEventArgs(addSet, deletedSet, modifySet);

            if (deletedSet != null)
            {
                this.Update(deletedSet, UpdateAction.Delete);
            }
            if (modifySet != null)
            {
                this.Update(modifySet, UpdateAction.Modify);
            }
            if (addSet != null)
            {
                this.Update(addSet, UpdateAction.Add);
            }

            if (settingsChangedEventArgs != null)
            {
                SettingsChangedEventHandler settingsChanged = this.SettingsChanged;
                if (settingsChanged != null) settingsChanged(this, settingsChangedEventArgs);
            }
        }

        public void Update(SettingSet settingSet, UpdateAction action)
        {
            if (action == UpdateAction.Initialize)
            {
                this.SystemParameter = new SystemParameter(settingSet.SystemParameter);
            }
            if (settingSet.SystemParameter != null)
            {
                if (action == UpdateAction.Modify)
                {
                    this.SystemParameter.Update(settingSet.SystemParameter);
                }
            }
            if (settingSet.AccountGroups != null)
            {
                foreach (CommonAccountGroup accountGroup in settingSet.AccountGroups)
                {
                    SettingsManagerHelper.UpdateEntity<CommonAccountGroup, AccountGroup>
                        (accountGroup,this._AccountGroups, action, accountGroup.Id, SettingsManagerHelper.UpdateManagerEntity);
                }
            }
            if (settingSet.Accounts != null)
            {
                foreach (CommonAccount account in settingSet.Accounts)
                {
                    SettingsManagerHelper.UpdateEntity<CommonAccount, Account>
                        (account, this._Accounts, action, account.Id, SettingsManagerHelper.UpdateManagerEntity);
                }
            }

            if (settingSet.Customers != null)
            {
                foreach (CommonCustomer customer in settingSet.Customers)
                {
                    SettingsManagerHelper.UpdateEntity<CommonCustomer, Customer>
                        (customer, this._Customers, action, customer.Id, SettingsManagerHelper.UpdateManagerEntity);
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
                    else if (action == UpdateAction.Modify)
                    {
                        this._TradePolicies[tradePolicy.Id].Update(tradePolicy);
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
                            //this.IsCalculateNecessary = true;
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
                return null;
            }
        }
        internal SettingsParameter GetSettingsParameter(string exchangeCode)
        {
            return this._SettingsParameters[exchangeCode];
        }
        #endregion
    }

    internal static class SettingsManagerHelper
    {
        internal static void InitializeSystemParameter(this ParameterSetting parameterSetting, XElement xmlNode)
        {
            foreach (XAttribute atrribute in xmlNode.Attributes())
            {
                string nodeName = atrribute.Name.ToString();
                String nodeValue = atrribute.Value;
                if (nodeName == "InactiveWaitTime")
                {
                    parameterSetting.InactiveWaitTime = int.Parse(nodeValue);
                    continue;
                }
                else if (nodeName == "EnquiryWaitTime")
                {
                    parameterSetting.EnquiryWaitTime = int.Parse(nodeValue); ;
                    continue;
                }
                else if (nodeName == "ApplyAutoAdjustPoints")
                {
                    parameterSetting.ApplyAutoAdjustPoints = bool.Parse(nodeValue);
                    continue;
                }
                else if (nodeName == "PriceOrderSetting")
                {
                    parameterSetting.PriceOrderSetting = int.Parse(nodeValue);
                    continue;
                }
                else if (nodeName == "DisablePopup")
                {
                    parameterSetting.DisablePopup = bool.Parse(nodeValue);
                    continue;
                }
                else if (nodeName == "AutoConfirm")
                {
                    parameterSetting.AutoConfirm = bool.Parse(nodeValue);
                    continue;
                }
                else if (nodeName == "ApplySetValueToDealingPolicy")
                {
                    parameterSetting.ApplySetValueToDealingPolicy = bool.Parse(nodeValue);
                    continue;
                }
                else if (nodeName == "LimitStopSummaryIsTimeRange")
                {
                    parameterSetting.LimitStopSummaryIsTimeRange = bool.Parse(nodeValue);
                    continue;
                }
                else if (nodeName == "LimitStopSummaryTimeRangeValue")
                {
                    parameterSetting.LimitStopSummaryTimeRangeValue = int.Parse(nodeValue);
                    continue;
                }
                else if (nodeName == "LimitStopSummaryPriceRangeValue")
                {
                    parameterSetting.LimitStopSummaryPriceRangeValue = int.Parse(nodeValue);
                    continue;
                }
            }

       
        }

        internal static void InitializeSystemParameter(this SoundSetting soundSetting, XElement xmlNode)
        {
            foreach (XAttribute atrribute in xmlNode.Attributes())
            {
                string nodeName = atrribute.Name.ToString();
                String nodeValue = atrribute.Value;
                if (nodeName == "ID")
                {
                    soundSetting.Id = int.Parse(nodeValue);
                    continue;
                }
                if (nodeName == "FileName")
                {
                    soundSetting.FilePath = nodeValue;
                    continue;
                }
            }
        }

        internal static void InitializeSystemParameter(this SetValueSetting setValueSetting, XElement xmlNode)
        {
            foreach (XAttribute atrribute in xmlNode.Attributes())
            {
                string nodeName = atrribute.Name.ToString();
                String nodeValue = atrribute.Value;
                if (nodeName == "OriginInactiveTime")
                {
                    setValueSetting.OriginInactiveTime = int.Parse(nodeValue);
                    continue;
                }
                else if (nodeName == "AlertVariation")
                {
                    setValueSetting.AlertVariation = int.Parse(nodeValue); ;
                    continue;
                }
                else if (nodeName == "NormalWaitTime")
                {
                    setValueSetting.NormalWaitTime = int.Parse(nodeValue); ;
                    continue;
                }
                else if (nodeName == "AlertWaitTime")
                {
                    setValueSetting.AlertWaitTime = int.Parse(nodeValue); ;
                    continue;
                }
                else if (nodeName == "MaxDQLot")
                {
                    setValueSetting.MaxDQLot = decimal.Parse(nodeValue);
                    continue;
                }
                else if (nodeName == "MaxOtherLot")
                {
                    setValueSetting.MaxOtherLot = decimal.Parse(nodeValue);
                    continue;
                }
                else if (nodeName == "DQQuoteMinLot")
                {
                    setValueSetting.DQQuoteMinLot = decimal.Parse(nodeValue);
                    continue;
                }
                else if (nodeName == "AutoDQMaxLot")
                {
                    setValueSetting.AutoDQMaxLot = decimal.Parse(nodeValue);
                    continue;
                }
                else if (nodeName == "AutoLmtMktMaxLot")
                {
                    setValueSetting.AutoLmtMktMaxLot = decimal.Parse(nodeValue);
                    continue;
                }
                else if (nodeName == "AcceptDQVariation")
                {
                    setValueSetting.AcceptDQVariation = int.Parse(nodeValue);
                    continue;
                }
                else if (nodeName == "AcceptLmtVariation")
                {
                    setValueSetting.AcceptLmtVariation = int.Parse(nodeValue);
                    continue;
                }
                else if (nodeName == "AcceptCloseLmtVariation")
                {
                    setValueSetting.AcceptCloseLmtVariation = int.Parse(nodeValue);
                    continue;
                }
                else if (nodeName == "CancelLmtVariation")
                {
                    setValueSetting.CancelLmtVariation = int.Parse(nodeValue);
                    continue;
                }
                else if (nodeName == "MaxMinAdjust")
                {
                    setValueSetting.MaxMinAdjust = int.Parse(nodeValue);
                    continue;
                }
                else if (nodeName == "IsBetterPrice")
                {
                    setValueSetting.IsBetterPrice = bool.Parse(nodeValue);
                    continue;
                }
                else if (nodeName == "AutoAcceptMaxLot")
                {
                    setValueSetting.AutoAcceptMaxLot = decimal.Parse(nodeValue);
                    continue;
                }
                else if (nodeName == "AutoCancelMaxLot")
                {
                    setValueSetting.AutoCancelMaxLot = decimal.Parse(nodeValue);
                    continue;
                }
                else if (nodeName == "AllowedNewTradeSides")
                {
                    setValueSetting.AllowedNewTradeSides = int.Parse(nodeValue);
                    continue;
                }
                else if (nodeName == "HitTimes")
                {
                    setValueSetting.HitTimes = int.Parse(nodeValue);
                    continue;
                }
                else if (nodeName == "PenetrationPoint")
                {
                    setValueSetting.PenetrationPoint = int.Parse(nodeValue);
                    continue;
                }
                else if (nodeName == "PriceValidTime")
                {
                    setValueSetting.PriceValidTime = int.Parse(nodeValue);
                    continue;
                }
                else if (nodeName == "LastAcceptTimeSpan")
                {
                    setValueSetting.LastAcceptTimeSpan = TimeSpan.Parse(nodeValue);
                    continue;
                }
                else if (nodeName == "AutoDQDelay")
                {
                    setValueSetting.AutoDQDelay = int.Parse(nodeValue);
                    continue;
                }
                else if (nodeName == "HitPriceVariationForSTP")
                {
                    setValueSetting.HitPriceVariationForSTP = int.Parse(nodeValue);
                    continue;
                }
            }


        }

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
        internal static void UpdateManagerEntity(this AccountGroup accountGroup, CommonAccountGroup commonAccountGroup, UpdateAction action)
        {
            accountGroup.Update(commonAccountGroup);
        }
        internal static void UpdateManagerEntity(this Account account, CommonAccount commonAccount, UpdateAction action)
        {
            account.Update(commonAccount);
        }
        internal static void UpdateManagerEntity(this Customer customer, CommonCustomer commonCustomer, UpdateAction action)
        {
            customer.Update(commonCustomer);
        }
        #endregion


    }
}
