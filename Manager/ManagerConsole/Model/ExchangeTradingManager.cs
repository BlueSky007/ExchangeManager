using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SettingSet = Manager.Common.SettingSet;
using Logger = Manager.Common.Logger;
using UpdateAction = Manager.Common.UpdateAction;
using CommonAccount = Manager.Common.Settings.Account;
using CommonTransaction = iExchange.Common.Manager.Transaction;
using CommonOrder = iExchange.Common.Manager.Order;
using CommonOrderRelation = iExchange.Common.Manager.OrderRelation;


namespace ManagerConsole.Model
{
    public class ExchangeTradingManager
    {
        private string _ExchangeCode;
        private Dictionary<Guid, Account> _Accounts = new Dictionary<Guid, Account>();
        private Dictionary<Guid, InstrumentClient> _Instruments = new Dictionary<Guid, InstrumentClient>();
        private Dictionary<Guid, Transaction> _Transactions = new Dictionary<Guid, Transaction>();
        private Dictionary<Guid, Order> _Orders = new Dictionary<Guid, Order>();

        public ExchangeTradingManager(ExchangeSettingManager exchangeSettingManager)
        {
            this.ExchangeSettingManager = exchangeSettingManager;
            this._ExchangeCode = exchangeSettingManager.ExchangeCode;
        }

        private ExchangeSettingManager ExchangeSettingManager
        {
            get;
            set;
        }

        public IEnumerable<Account> Accounts
        {
            get { return this._Accounts.Values; }
        }

        public IEnumerable<InstrumentClient> Instruments
        {
            get { return this._Instruments.Values; }
        }

        public IEnumerable<Order> Orders
        {
            get { return this._Orders.Values; }
        }

        public IEnumerable<Transaction> Transactions
        {
            get { return this._Transactions.Values; }
        }

        public void Initialize(SettingSet settingSet)
        {
            try
            {
                if (settingSet.Orders != null)
                {
                    foreach (CommonOrder commonOrder in settingSet.Orders)
                    {
                        commonOrder.ExchangeCode = this._ExchangeCode;
                        this.InitializeOrder(commonOrder);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "ExchangeTradingManager.Initialize.\r\n{0}", ex.ToString());
            }
            
        }

        private void InitializeOrder(CommonOrder commonOrder)
        {
            Transaction transaction = null;
            Guid accountId = commonOrder.AccountId;
            Guid instrumentId = commonOrder.InstrumentId;

            if (!this.ExchangeSettingManager.Instruments.ContainsKey(instrumentId) || !this.ExchangeSettingManager.Accounts.ContainsKey(accountId))
                    return;

            Account account = this.ExchangeSettingManager.Accounts[accountId];
            InstrumentClient instrument = this.ExchangeSettingManager.Instruments[instrumentId];

            if (!this._Orders.ContainsKey(commonOrder.Id))
            {
                if (!this._Transactions.ContainsKey(commonOrder.TransactionId))
                {
                    transaction = new Transaction(commonOrder, account, instrument);

                    this._Transactions.Add(transaction.Id, transaction);

                    Order order = new Order(transaction, commonOrder);
                    this._Orders.Add(order.Id, order);
                    transaction.Add(order);
                }
            }
            else
            {
                transaction = this._Orders[commonOrder.Id].Transaction;

                if (this._Orders[commonOrder.Id].Phase == iExchange.Common.OrderPhase.Executed)
                    return;
                this._Orders[commonOrder.Id].Update(commonOrder);
            }
        }


    }
}
