﻿using ManagerConsole.Model;
using ManagerConsole.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using CommonTransaction = Manager.Common.Transaction;
using CommonOrder = Manager.Common.Order;
using CommonOrderRelation = Manager.Common.OrderRelation;
using PlaceMessage = Manager.Common.PlaceMessage;
using QuoteMessage = Manager.Common.QuoteMessage;
using HitMessage = Manager.Common.HitMessage;
using DeleteMessage = Manager.Common.DeleteMessage;
using ExecuteMessage = Manager.Common.ExecuteMessage;
using CommonParameter = Manager.Common.Settings.SystemParameter;
using TransactionError = Manager.Common.TransactionError;
using ManagerConsole.Helper;
using SettingSet = Manager.Common.SettingSet;
using InitializeData = Manager.Common.InitializeData;
using ConfigParameters = Manager.Common.Settings.ConfigParameters;
using Phase = Manager.Common.Phase;
using OrderRelationType = Manager.Common.OrderRelationType;
using Manager.Common.QuotationEntities;

namespace ManagerConsole
{
    public class InitDataManager
    {
        public delegate void HitPriceReceivedRefreshUIEventHandler(int hitOrderCount);
        public event HitPriceReceivedRefreshUIEventHandler OnHitPriceReceivedRefreshUIEvent;

        public delegate void OrderHitPriceNotifyHandler(OrderTask orderTask);
        public event OrderHitPriceNotifyHandler OnOrderHitPriceNotifyEvent;

        public delegate void DeleteOrderNotifyHandler(Order deletedOrder);
        public event DeleteOrderNotifyHandler OnDeleteOrderNotifyEvent;

        public delegate void ExecutedOrderNotifyHandler(Order order);
        public event ExecutedOrderNotifyHandler OnExecutedOrderNotifyEvent;

        private Dictionary<Guid, Account> _Accounts = new Dictionary<Guid, Account>();
        private Dictionary<Guid, InstrumentClient> _Instruments = new Dictionary<Guid, InstrumentClient>();
        private Dictionary<Guid, Transaction> _Transactions = new Dictionary<Guid, Transaction>();
        private Dictionary<Guid, Order> _Orders = new Dictionary<Guid, Order>();
        private Dictionary<string, ConfigParameters> _ConfigParameters = new Dictionary<string, ConfigParameters>();
        private Dictionary<string, List<ExchangeQuotation>> _ExchangeQuotations = new Dictionary<string,List<ExchangeQuotation>>();
        private ObservableCollection<OrderTask> _OrderTaskEntities = new ObservableCollection<OrderTask>();
        private DQOrderTaskForInstrumentModel _DQOrderTaskForInstrumentModel = new DQOrderTaskForInstrumentModel();
        private MooMocOrderForInstrumentModel _MooMocOrderForInstrumentModel = new MooMocOrderForInstrumentModel();
        private OrderTaskModel _OrderTaskModel = new OrderTaskModel();
        private ProcessInstantOrder _ProcessInstantOrder = new ProcessInstantOrder();
        private LMTProcessModel _LMTProcessModel = new LMTProcessModel();
        private ObservableCollection<Order> _ExecutedOrders = new ObservableCollection<Order>();
        private ExecuteOrderSummaryItemModel _ExecuteOrderSummaryItemModel = new ExecuteOrderSummaryItemModel();
        private bool _IsInitializeCompleted = false;

        //询价
        private QuotePriceClientModel _QuotePriceClientModel = new QuotePriceClientModel();

        private TranPhaseManager _TranPhaseManager;

        #region Public Property
        public ExecuteOrderSummaryItemModel ExecuteOrderSummaryItemModel
        {
            get { return this._ExecuteOrderSummaryItemModel; }
            set { this._ExecuteOrderSummaryItemModel = value; }
        }
        public TranPhaseManager TranPhaseManager
        {
            get { return this._TranPhaseManager; }
            set { this._TranPhaseManager = value; }
        }

        public QuotePriceClientModel QuotePriceClientModel
        {
            get { return this._QuotePriceClientModel; }
            set { this._QuotePriceClientModel = value; }
        }

        public SettingsManager SettingsManager
        {
            get;
            set;
        }

        public bool IsInitializeCompleted
        {
            get { return this._IsInitializeCompleted; }
        }

        public Dictionary<string, ConfigParameters> ConfigParameters
        {
            get { return this._ConfigParameters; }
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

        public ProcessInstantOrder ProcessInstantOrder
        {
            get { return this._ProcessInstantOrder; }
            set { this._ProcessInstantOrder = value; }
        }

        public DQOrderTaskForInstrumentModel DQOrderTaskForInstrumentModel
        {
            get { return this._DQOrderTaskForInstrumentModel; }
            set { this._DQOrderTaskForInstrumentModel = value; }
        }

        public MooMocOrderForInstrumentModel MooMocOrderForInstrumentModel
        {
            get { return this._MooMocOrderForInstrumentModel; }
            set { this._MooMocOrderForInstrumentModel = value; }
        }

        public ObservableCollection<Order> ExecutedOrders
        {
            get { return this._ExecutedOrders; }
            set { this._ExecutedOrders = value; }
        }

        public Dictionary<string, List<ExchangeQuotation>> ExchangeQuotations
        {
            get { return this._ExchangeQuotations; }
            set { this._ExchangeQuotations = value; }
        }
        #endregion

        public InitDataManager()
        {
            this.SettingsManager = new SettingsManager();
            this._Accounts = TestData.GetInitializeTestDataForAccount();
            this._Instruments = TestData.GetInitializeTestDataForInstrument();
            this._TranPhaseManager = new TranPhaseManager(this);
        }

        #region Initialize Data
        public void Initialize(List<InitializeData> initializeDatas)
        {
            foreach (InitializeData initializeData in initializeDatas)
            {
                this.SettingsManager.Initialize(initializeData.SettingSet);
                this._ConfigParameters = initializeData.SettingParameters;
                this.UpdateTradingSetting();
                this._ExchangeQuotations.Add(initializeData.ExchangeCode, this.InitExchangeQuotation(initializeData.SettingSet));
            }
            this._IsInitializeCompleted = true;
        }

        public void UpdateTradingSetting()
        {
            foreach (InstrumentClient instrument in this.SettingsManager.GetInstruments())
            {
                Toolkit.AddDictionary<InstrumentClient>(instrument.Id, instrument, this._Instruments);
            }
            foreach (Account account in this.SettingsManager.GetAccounts())
            {
                Toolkit.AddDictionary<Account>(account.Id, account, this._Accounts);
            }
        }

        private List<ExchangeQuotation> InitExchangeQuotation(SettingSet set)
        {
            try
            {
                List<ExchangeQuotation> quotations = new List<ExchangeQuotation>();
                foreach (Manager.Common.Settings.QuotePolicyDetail item in set.QuotePolicyDetails)
                {
                    ExchangeQuotation quotation = new ExchangeQuotation(item);
                    quotation.QuotationPolicyCode = set.QuotePolicies.SingleOrDefault(q => q.Id == item.QuotePolicyId).Code;
                    quotation.InstrumentCode = set.Instruments.SingleOrDefault(i => i.Id == item.InstrumentId).Code;
                    quotation.OriginInstrumentCode = set.Instruments.SingleOrDefault(i => i.Id == item.InstrumentId).OriginCode;
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
                    quotations.Add(quotation);
                }

                return quotations;
            }
            catch (Exception ex)
            {
               Manager.Common.Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "InitExchangeQuotation.\r\n{0}", ex.ToString());
               return null;
            }
        }

        #endregion

        #region Received Notify Convert
        
        public void ProcessQuoteMessage(QuoteMessage quoteMessage)
        {
            int waiteTime = this.SettingsManager.GetSettingsParameter(quoteMessage.ExchangeCode).ParameterSetting.EnquiryWaitTime;
            Guid customerId = quoteMessage.CustomerID;
            //通过CustomerId获取Customer对象
            //Customer customer = this.SettingsManager.GetCustomers().SingleOrDefault(P => P.Id == customerId);
            var customer = new Customer();
            customer.Id = quoteMessage.CustomerID;
            customer.Code = "WF007";

            if (!this._Instruments.ContainsKey(quoteMessage.InstrumentID)) return;
            InstrumentClient instrument = this._Instruments[quoteMessage.InstrumentID];

            QuotePriceClient quotePriceClient = new QuotePriceClient(quoteMessage,waiteTime,instrument,customer);
            this._QuotePriceClientModel.AddSendQuotePrice(quotePriceClient);
        }

        public void ProcessPlaceMessage(PlaceMessage placeMessage)
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
                this.TranPhaseManager.UpdateTransaction(tran);

                foreach (Order order in tran.Orders)
                {
                    this.AddOrderTaskEntity(order);
                }
            }
        }

        public void ProcessExecuteMessage(ExecuteMessage executeMessage)
        {
            foreach (CommonTransaction commonTransaction in executeMessage.Transactions)
            {
                if (commonTransaction.Error != null && commonTransaction.Error.Value != TransactionError.OK)
                { 
                    //...
                }
                if(this._Transactions.ContainsKey(commonTransaction.Id) && this._Transactions[commonTransaction.Id].Phase == Phase.Executed)
                {
                    continue;
                }
                else
                {
                    this.Process(commonTransaction);
                }
            }
            //Sound.PlayExecute()

            foreach (CommonOrder commonOrder in executeMessage.Orders)
            {
                commonOrder.ExchangeCode = executeMessage.ExchangeCode;
                this.Process(commonOrder, false);
            }

            foreach (CommonTransaction commonTransaction in executeMessage.Transactions)
            {
                Transaction tran = this._Transactions[commonTransaction.Id];
                this.TranPhaseManager.UpdateTransaction(tran);
            }
        }

        public void ProcessHitMessage(HitMessage hitMessage)
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

        public void ProcessDeleteMessage(DeleteMessage deleteMessage)
        {
            if (deleteMessage.DeletedOrderId != Guid.Empty)
            {
                if (!this._Orders.ContainsKey(deleteMessage.DeletedOrderId)) return;
                Order deletedOrder = this._Orders[deleteMessage.DeletedOrderId];
                //Refresh Lot
                if (this._ExecutedOrders.Contains(deletedOrder))
                {
                    this.TranPhaseManager.DeleteOrderNotifyUpdateLot(deleteMessage.InstrumentID, deletedOrder);
                }

                this.TranPhaseManager.DeletedExecutedOrderSummaryItem(deletedOrder);

                if (this.OnDeleteOrderNotifyEvent != null)
                {
                    this.OnDeleteOrderNotifyEvent(deletedOrder);
                }
            }
            //Sound.PlayDelete();

            foreach (CommonTransaction commonTransaction in deleteMessage.Transactions)
            {
                this.Process(commonTransaction);
            }
            foreach (CommonOrder commonOrder in deleteMessage.Orders)
            {
                this.Process(commonOrder,false);
            }
            foreach (CommonOrderRelation commonOrderRelation in deleteMessage.OrderRelations)
            {
                this.Process(commonOrderRelation);
            }
            foreach (CommonTransaction commonTran in deleteMessage.Transactions)
            {
                Transaction newTransaction = this._Transactions[commonTran.Id];
                foreach (Order order in newTransaction.Orders)
                {
                    this.TranPhaseManager.AddExecutedOrder(order);
                    this.TranPhaseManager.AddOrderProcessBuySellLot(deleteMessage.InstrumentID, order);
                    this.TranPhaseManager.AddExecutedOrder(order);
                }
                //Refresh OpenInterestGrid
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

                if (this.OnOrderHitPriceNotifyEvent != null)
                {
                    this.OnOrderHitPriceNotifyEvent(orderTask);
                }
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
            foreach (LMTProcessForInstrument entity in this._LMTProcessModel.LMTProcessForInstruments)
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
                if (transaction.ContractSize == 0)
                {
                    TradePolicyDetail tradePolicyDetail = this.SettingsManager.GetTradePolicyDetail(account.TradePolicyId, instrument.Id);
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

        private void Process(CommonOrderRelation commonOrderRelation)
        {
            if(commonOrderRelation.RelationType == OrderRelationType.Close)
            {
 
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
            else if(order.Transaction.OrderType == Manager.Common.OrderType.SpotTrade)
            {
                OrderTask orderTask = new OrderTask(order);
                orderTask.BaseOrder = order;

                this._ProcessInstantOrder.AddInstanceOrder(orderTask);
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

        public void AddExecutedOrderSummaryItem(Order order)
        {
            if (this.OnExecutedOrderNotifyEvent != null)
            {
                this.OnExecutedOrderNotifyEvent(order);
            }
        }

        #region Empty OrderTask Event
        void DQOrderTaskForInstrument_OnEmptyDQOrderTask(DQOrderTaskForInstrument orderTaskForInstrument)
        {
            this._DQOrderTaskForInstrumentModel.DQOrderTaskForInstruments.Remove(orderTaskForInstrument);
        }
        void ClientQuotePriceForInstrument_OnEmptyClientQuotePriceClient(QuotePriceForInstrument clientQuotePriceForInstrument)
        {
            //this.ClientQuotePriceForInstrument.Remove(clientQuotePriceForInstrument);
        }
        #endregion
    }
}
