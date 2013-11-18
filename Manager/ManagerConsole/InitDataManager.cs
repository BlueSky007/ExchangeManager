﻿using ManagerConsole.Model;
using ManagerConsole.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using CommonTransaction = Manager.Common.Transaction;
using CommonOrder = Manager.Common.Order;
using PlaceMessage = Manager.Common.PlaceMessage;
using HitMessage = Manager.Common.HitMessage;
using CommonParameter = Manager.Common.SystemParameter;
using ManagerConsole.Helper;
using SettingSet = Manager.Common.SettingSet;
using InitializeData = Manager.Common.InitializeData;
using SettingParameters = Manager.Common.SettingParameters;

namespace ManagerConsole
{
    public class InitDataManager
    {
        public delegate void HitPriceReceivedRefreshUIEventHandler(int hitOrderCount);
        public event HitPriceReceivedRefreshUIEventHandler OnHitPriceReceivedRefreshUIEvent;

        private CommonParameter _SystemParameter = new CommonParameter();
        private Dictionary<Guid, Account> _Accounts = new Dictionary<Guid, Account>();
        private Dictionary<Guid, InstrumentClient> _Instruments = new Dictionary<Guid, InstrumentClient>();
        private Dictionary<Guid, Transaction> _Transactions = new Dictionary<Guid, Transaction>();
        private Dictionary<Guid, Order> _Orders = new Dictionary<Guid, Order>();
        private Dictionary<string, SettingParameters> _SettingParameters = new Dictionary<string, SettingParameters>();

        private ObservableCollection<OrderTask> _OrderTaskEntities = new ObservableCollection<OrderTask>();
        private LmtOrderTaskForInstrumentModel _LmtOrderTaskForInstrumentModel = new LmtOrderTaskForInstrumentModel();
        private DQOrderTaskForInstrumentModel _DQOrderTaskForInstrumentModel = new DQOrderTaskForInstrumentModel();
        private MooMocOrderForInstrumentModel _MooMocOrderForInstrumentModel = new MooMocOrderForInstrumentModel();
        private OrderTaskModel _OrderTaskModel = new OrderTaskModel();
        private LMTProcessModel _LMTProcessModel = new LMTProcessModel();

        #region Public Property
        public SettingsManager SettingsManager
        {
            get;
            set;
        }

        public Dictionary<string, SettingParameters> SettingParameters
        {
            get { return this._SettingParameters; }
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

        public LMTProcessModel LMTProcessModel
        {
            get { return this._LMTProcessModel; }
            set { this._LMTProcessModel = value; }
        }

        public OrderTaskModel OrderTaskModel
        {
            get { return this._OrderTaskModel; }
            set { this._OrderTaskModel = value; }
        }

        public DQOrderTaskForInstrumentModel DQOrderTaskForInstrumentModel
        {
            get { return this._DQOrderTaskForInstrumentModel; }
            set { this._DQOrderTaskForInstrumentModel = value; }
        }

        public LmtOrderTaskForInstrumentModel LmtOrderTaskForInstrumentModel
        {
            get { return this._LmtOrderTaskForInstrumentModel; }
            set { this._LmtOrderTaskForInstrumentModel = value; }
        }

        public MooMocOrderForInstrumentModel MooMocOrderForInstrumentModel
        {
            get { return this._MooMocOrderForInstrumentModel; }
            set { this._MooMocOrderForInstrumentModel = value; }
        }
        #endregion

        public InitDataManager()
        {
            this.SettingsManager = new SettingsManager();
            this.GetInitializeTestData();
        }

        private void GetInitializeTestData()
        {
            for (int i = 0; i < 10; i++)
            {
                InstrumentClient instrument = new InstrumentClient();
                string guidStr = "66adc06c-c5fe-4428-867f-be97650eb3b" + i;
                instrument.Id = new Guid(guidStr);
                instrument.Code = "GBPUSA" + i;
                instrument.Ask = "1.58" + i;
                instrument.Bid = "1.56" + i;
                instrument.Denominator = 10;
                instrument.NumeratorUnit = 2;
                instrument.MaxSpread = 100;
                instrument.MaxAutoPoint = 100;
                instrument.AcceptDQVariation = 10;
                instrument.Spread = 5;
                instrument.AutoPoint = 10;
                instrument.Origin = "1.555";
                instrument.IsNormal = true;
                this._Instruments.Add(instrument.Id, instrument);
            }

            Account account = new Account();
            account.Id = new Guid("9538eb6e-57b1-45fa-8595-58df7aabcfc9");
            account.Code = "DEAccount009";
            Account account2 = new Account();
            account2.Id = new Guid("9538eb6e-57b1-45fa-8595-58df7aabcfc8");
            account2.Code = "DEAccount008";
            Account account3 = new Account();
            account3.Id = new Guid("9538eb6e-57b1-45fa-8595-58df7aabcfc7");
            account3.Code = "DEAccount007";
            this._Accounts.Add(account.Id, account);
            this._Accounts.Add(account2.Id, account2);
            this._Accounts.Add(account3.Id, account3);
        }

        #region Initialize Data
        public void Initialize(ObservableCollection<InitializeData> initializeDatas)
        {
            foreach (InitializeData initializeData in initializeDatas)
            {
                this.SettingsManager.Initialize(initializeData.SettingSet);
                this._SettingParameters = initializeData.SettingParameters;
            }
        }
        #endregion

        public InitDataManager(List<CommonOrder> orders)
        {
            foreach (CommonOrder commonOrder in orders)
            {
                //this._Orders.Add(new Order(order));
            }
        }

        #region Received Notify Convert
        public void AddPlaceMessage(PlaceMessage placeMessage)
        {
            foreach (CommonTransaction commonTransaction in placeMessage.Transactions)
            {
                this.Process(commonTransaction, false);
            }
            foreach (CommonOrder commonOrder in placeMessage.Orders)
            {
                commonOrder.ExchangeCode = placeMessage.ExchangeCode;
                this.Process(commonOrder,false);
            }


            //Change Order status
            foreach (CommonTransaction commonTransaction in placeMessage.Transactions)
            {
                Transaction tran = this._Transactions[commonTransaction.Id];
                Guid accountId = tran.Account.Id;
                bool existAccount = this._Accounts.ContainsKey(accountId);
                TranPhaseManager.SetOrderStatus(tran, existAccount);

                foreach (Order order in tran.Orders)
                {
                    this.AddOrderTaskEntity(order);
                }
            }
        }

        public void AddHitMessage(HitMessage hitMessage)
        {
            foreach (CommonOrder commonOrder in hitMessage.Orders)
            {
                commonOrder.ExchangeCode = hitMessage.ExchangeCode;
                if (this._Orders.ContainsKey(commonOrder.Id))
                {
                    this._Orders[commonOrder.Id].HitUpdate(commonOrder);

                    this.UpdateOrderTask(this._Orders[commonOrder.Id]);
                }
            } 
        }

        //Move Hit Order To the First Row
        private void MoveHitOrder(Order newOrder)
        {
            List<OrderTask> hitOrders = new List<OrderTask>();
            IEnumerable<OrderTask> orders = this._OrderTaskModel.OrderTasks.Where(P => P.OrderId == newOrder.Id);

            foreach (OrderTask entity in orders)
            {
                hitOrders.Add(entity);
            }
            foreach (OrderTask entity in hitOrders)
            {
                this._OrderTaskModel.RemoveOrderTask(entity);
                OrderTask orderTask = new OrderTask(newOrder);
                orderTask.BaseOrder = newOrder;
                this._OrderTaskModel.OrderTasks.Insert(0, orderTask);
            }

            if (this.OnHitPriceReceivedRefreshUIEvent != null)
            {
                int hitOrdersCount = hitOrders.Count();
                this.OnHitPriceReceivedRefreshUIEvent(hitOrdersCount);
            }
        }

        private void UpdateOrderTask(Order newOrder)
        {
            this.MoveHitOrder(newOrder);
            //Update DQ Order
            foreach (DQOrderTaskForInstrument entity in this._DQOrderTaskForInstrumentModel.DQOrderTaskForInstruments)
            {
                foreach (OrderTask orderTask in entity.OrderTasks)
                {
                    if (orderTask.OrderId == newOrder.Id)
                    {
                        orderTask.Update(newOrder);
                    }
                }
            }
            //Update Lmt/Stop Order
            foreach (LmtOrderTaskForInstrument entity in this._LmtOrderTaskForInstrumentModel.LmtOrderTaskForInstruments)
            {
                foreach (OrderTask orderTask in entity.OrderTasks)
                {
                    if (orderTask.OrderId == newOrder.Id)
                    {
                        orderTask.Update(newOrder);
                    }
                }
            }
        }

        private void Process(CommonTransaction commonTransaction)
        {
            this.Process(commonTransaction, false);
        }

        private void Process(CommonTransaction commonTransaction, bool isExecuted)
        {
            if (!this._Transactions.ContainsKey(commonTransaction.Id))
            {
                if (!this._Instruments.ContainsKey(commonTransaction.InstrumentId) || !this._Accounts.ContainsKey(commonTransaction.AccountId))
                    return;
                Account account = this._Accounts[commonTransaction.AccountId];
                InstrumentClient instrument = this._Instruments[commonTransaction.InstrumentId];
                Transaction transaction = new Transaction(commonTransaction, account, instrument);

                //Set Transaction.ContractSize
                if (transaction.ContractSize == null)
                {
                    TradePolicyDetail tradePolicyDetail = null;// this.SettingsManager.GetTradePolicyDetail(account.TradePolicyId, instrument.Id);
                    if (tradePolicyDetail != null)
                    {
                        transaction.ContractSize = tradePolicyDetail.ContractSize;
                    }
                }
                this._Transactions.Add(transaction.Id, transaction);
            }
            else
            {
                if (isExecuted && commonTransaction.SubType == Manager.Common.TransactionSubType.Match)
                {
                    Transaction transaction = this._Transactions[commonTransaction.Id];
                    if (transaction.Phase == Manager.Common.Phase.Canceled || transaction.Phase == Manager.Common.Phase.Executed)
                    {
                        return;
                    }
                }
                if (this._Transactions[commonTransaction.Id].Phase == Manager.Common.Phase.Executed && isExecuted)
                    return;
                this._Transactions[commonTransaction.Id].Update(commonTransaction);
            }
        }

        private void Process(CommonOrder commonOrder, bool isExecuted)
        {
            Transaction transaction = null;
            if (!this._Orders.ContainsKey(commonOrder.Id))
            {
                if (!this._Transactions.ContainsKey(commonOrder.TransactionId))
                    return;
                transaction = this._Transactions[commonOrder.TransactionId];
                Order order = new Order(transaction, commonOrder);
                this._Orders.Add(order.Id, order);
                transaction.Add(order);

                //Add Binding Entity
               // this.AddOrderTaskEntity(order);
            }
            else
            {
                transaction = this._Orders[commonOrder.Id].Transaction;

                if (isExecuted && transaction.SubType == Manager.Common.TransactionSubType.Match
                    && (transaction.Phase == Manager.Common.Phase.Canceled || transaction.Phase == Manager.Common.Phase.Executed))
                {
                    return;
                }
                if (this._Orders[commonOrder.Id].Phase == Manager.Common.Phase.Executed && isExecuted)
                    return;
                this._Orders[commonOrder.Id].Update(commonOrder);
            }
        }

        private void AddOrderTaskEntity(Order order)
        {
            if (order.Transaction.OrderType == Manager.Common.OrderType.MarketOnOpen || order.Transaction.OrderType == Manager.Common.OrderType.MarketOnClose)
            {
                OrderTask orderTask = new OrderTask(order);
                orderTask.BaseOrder = order;
                orderTask.OrderStatus = OrderStatus.TimeArrived;
                MooMocOrderForInstrument mooMocOrderForInstrument = null;
                mooMocOrderForInstrument = this._MooMocOrderForInstrumentModel.MooMocOrderForInstruments.SingleOrDefault(P => P.Instrument.Id == order.Transaction.Instrument.Id);
                if (mooMocOrderForInstrument == null)
                {
                    mooMocOrderForInstrument = new MooMocOrderForInstrument();
                    InstrumentClient instrument = order.Transaction.Instrument;
                    mooMocOrderForInstrument.Instrument = instrument;
                    mooMocOrderForInstrument.Origin = instrument.Origin;
                    mooMocOrderForInstrument.Variation = 0;

                    this._MooMocOrderForInstrumentModel.MooMocOrderForInstruments.Add(mooMocOrderForInstrument);
                }
                if (orderTask.IsBuy == BuySell.Buy)
                {
                    mooMocOrderForInstrument.SumBuyLot += orderTask.Lot.Value;
                }
                else
                {
                    mooMocOrderForInstrument.SumSellLot += orderTask.Lot.Value;
                }
                mooMocOrderForInstrument.OrderTasks.Add(orderTask);
            }
            else
            {
                OrderTask orderTask = new OrderTask(order);
                orderTask.BaseOrder = order;

                this._OrderTaskModel.OrderTasks.Add(orderTask);
                orderTask.SetCellDataDefine(orderTask.OrderStatus);
            }
        }
        #endregion

        internal ICollection<Order> GetOrders()
        {
            return new List<Order>(this._Orders.Values);
        }

        internal ICollection<InstrumentClient> GetInstruments()
        {
            return new List<InstrumentClient>(this._Instruments.Values);
        }
        #region Empty OrderTask Event
        void DQOrderTaskForInstrument_OnEmptyDQOrderTask(DQOrderTaskForInstrument orderTaskForInstrument)
        {
            this._DQOrderTaskForInstrumentModel.DQOrderTaskForInstruments.Remove(orderTaskForInstrument);
        }

        #endregion
    }
}
