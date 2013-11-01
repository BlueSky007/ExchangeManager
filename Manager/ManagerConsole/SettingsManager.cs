using ManagerConsole.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SettingSet = Manager.Common.SettingSet;
using UpdateAction = Manager.Common.UpdateAction;
using CommonAccount = Manager.Common.Account;
using CommonInstrument = Manager.Common.Instrument;

namespace ManagerConsole
{
    public class SettingsManager
    {
        public delegate void SettingsChangedEventHandler(object sender, SettingsChangedEventArgs eventArgs);
        public event SettingsChangedEventHandler SettingsChanged;

        private Dictionary<Guid, Account> _Accounts = new Dictionary<Guid, Account>();
        private Dictionary<Guid, InstrumentClient> _Instruments = new Dictionary<Guid, InstrumentClient>();
        private Dictionary<Guid, Dictionary<Guid, QuotePolicyDetail>> _QuotePolicyDetails = new Dictionary<Guid, Dictionary<Guid, QuotePolicyDetail>>();
        private Dictionary<Guid, TradePolicy> _TradePolicies = new Dictionary<Guid, TradePolicy>();
        private Dictionary<Guid, Dictionary<Guid, TradePolicyDetail>> _TradePolicyDetails = new Dictionary<Guid, Dictionary<Guid, TradePolicyDetail>>();
        private Dictionary<Guid, Dictionary<Guid, DealingPolicyDetail>> _DealingPolicyDetails = new Dictionary<Guid, Dictionary<Guid, DealingPolicyDetail>>();

        public SettingsManager()
        {
            Toolkit.SettingsManager = this;
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

        public void Initialize(SettingSet settingSet)
        {
            this.Update(settingSet, UpdateAction.Initialize);
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
            if (settingSet.Accounts != null)
            {
                foreach (CommonAccount account in settingSet.Accounts)
                {
                    if (action == UpdateAction.Initialize)
                    {
                        this._Accounts[account.Id] = new Account(account);
                    }
                    else if (action == UpdateAction.Modify)
                    {
                        if (this._Accounts.ContainsKey(account.Id))
                        {
                            this._Accounts[account.Id].Update(account);
                        }
                    }
                    else if (action == UpdateAction.Delete)
                    {
                        this._Accounts.Remove(account.Id);
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
        internal TradePolicyDetail GetTradePolicyDetail(Guid tradePolicyId, Guid instrumentId)
        {
            return this._TradePolicyDetails.ContainsKey(tradePolicyId) && this._TradePolicyDetails[tradePolicyId].ContainsKey(instrumentId) ? this._TradePolicyDetails[tradePolicyId][instrumentId] : null;
        }
        #endregion
    }
}
