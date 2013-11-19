using ManagerConsole.Model;
using ManagerConsole.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using CommonTransaction = Manager.Common.Transaction;
using CommonOrder = Manager.Common.Order;
using PlaceMessage = Manager.Common.PlaceMessage;
using QuoteMessage = Manager.Common.QuoteMessage;
using HitMessage = Manager.Common.HitMessage;
using CommonParameter = Manager.Common.Settings.SystemParameter;
using ManagerConsole.Helper;
using SettingSet = Manager.Common.SettingSet;
using InitializeData = Manager.Common.InitializeData;
using SettingParameters = Manager.Common.Settings.SettingParameters;

namespace ManagerConsole
{
    public class InitDataManager
    {
        public delegate void HitPriceReceivedRefreshUIEventHandler(int hitOrderCount);
        public event HitPriceReceivedRefreshUIEventHandler OnHitPriceReceivedRefreshUIEvent;

        public delegate void OrderHitPriceNotifyHandler(OrderTask orderTask);
        public event OrderHitPriceNotifyHandler OnOrderHitPriceNotifyEvent;

        private Dictionary<Guid, Account> _Accounts = new Dictionary<Guid, Account>();
        private Dictionary<Guid, InstrumentClient> _Instruments = new Dictionary<Guid, InstrumentClient>();
        private Dictionary<Guid, Transaction> _Transactions = new Dictionary<Guid, Transaction>();
        private Dictionary<Guid, Order> _Orders = new Dictionary<Guid, Order>();
        private Dictionary<string, SettingParameters> _SettingParameters = new Dictionary<string, SettingParameters>();

        private ObservableCollection<OrderTask> _OrderTaskEntities = new ObservableCollection<OrderTask>();
        private DQOrderTaskForInstrumentModel _DQOrderTaskForInstrumentModel = new DQOrderTaskForInstrumentModel();
        private MooMocOrderForInstrumentModel _MooMocOrderForInstrumentModel = new MooMocOrderForInstrumentModel();
        private OrderTaskModel _OrderTaskModel = new OrderTaskModel();
        private LMTProcessModel _LMTProcessModel = new LMTProcessModel();

        //询价
        private ObservableCollection<QuotePriceForInstrument> _ClientQuotePriceForInstrument = new ObservableCollection<QuotePriceForInstrument>();

        #region Public Property
        public ObservableCollection<QuotePriceForInstrument> ClientQuotePriceForInstrument
        {
            get { return this._ClientQuotePriceForInstrument; }
            set { this._ClientQuotePriceForInstrument = value; }
        }

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

        public MooMocOrderForInstrumentModel MooMocOrderForInstrumentModel
        {
            get { return this._MooMocOrderForInstrumentModel; }
            set { this._MooMocOrderForInstrumentModel = value; }
        }
        #endregion

        public InitDataManager()
        {
            this.SettingsManager = new SettingsManager();
            this._Accounts = TestData.GetInitializeTestDataForAccount();
            this._Instruments = TestData.GetInitializeTestDataForInstrument();
        }

        #region Initialize Data
        public void Initialize(ObservableCollection<InitializeData> initializeDatas)
        {
            foreach (InitializeData initializeData in initializeDatas)
            {
                this.SettingsManager.Initialize(initializeData.SettingSet);
                this._SettingParameters = initializeData.SettingParameters;
                this.UpdateTradingSetting();
            }
        }

        public void UpdateTradingSetting()
        {
            try
            {
                foreach (InstrumentClient instrument in this.SettingsManager.GetInstruments())
                {
                    if (!this._Instruments.ContainsKey(instrument.Id))
                    {
                        this._Instruments.Add(instrument.Id, instrument);
                    }
                }
                foreach (Account account in this.SettingsManager.GetAccounts())
                {
                    if (!this._Instruments.ContainsKey(account.Id))
                    {
                        this._Accounts.Add(account.Id, account);
                    }
                }
            }
            catch (Exception ex)
            {
                
            }
        }
        #endregion

        #region Received Notify Convert
        public void ProcessQuoteMessage(QuoteMessage quoteMessage)
        {
            int waiteTime = 50;     //取初始化数据系统参数
            Guid customerId = quoteMessage.CustomerID;
            //通过CustomerId获取Customer对象
            //var customer = this._Customers.SingleOrDefault(P => P.id == customerId);
            var customer = new Customer();
            customer.Id = quoteMessage.CustomerID;
            customer.Code = "WF007";
            QuotePriceClient quotePriceClient = new QuotePriceClient(quoteMessage, waiteTime, customer);
            QuotePriceForInstrument clientQuotePriceForInstrument = null;
            clientQuotePriceForInstrument = this.ClientQuotePriceForInstrument.SingleOrDefault(P => P.InstrumentClient.Id == quotePriceClient.InstrumentId);
            if (clientQuotePriceForInstrument == null)
            {
                //从内存中获取Instrument
                //var instrumentEntity = this._Instruments.SingleOrDefault(P => P.InstrumentId == clientQuotePriceForAccount.InstrumentId);
                clientQuotePriceForInstrument = new QuotePriceForInstrument();
                var instrument = TestData.GetInstrument(quotePriceClient);
                clientQuotePriceForInstrument.InstrumentClient = instrument;
                clientQuotePriceForInstrument.Origin = instrument.Origin;
                clientQuotePriceForInstrument.Adjust = decimal.Parse(instrument.Origin);
                clientQuotePriceForInstrument.AdjustLot = quotePriceClient.QuoteLot;
                this.ClientQuotePriceForInstrument.Add(clientQuotePriceForInstrument);
            }
            clientQuotePriceForInstrument.OnEmptyQuotePriceClient += new QuotePriceForInstrument.EmptyQuotePriceHandle(ClientQuotePriceForInstrument_OnEmptyClientQuotePriceClient);
            clientQuotePriceForInstrument.AddNewQuotePrice(quotePriceClient);
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
                Guid accountId = tran.Account.Id;
                bool existAccount = this._Accounts.ContainsKey(accountId);
                TranPhaseManager.SetOrderStatus(tran, existAccount);

                foreach (Order order in tran.Orders)
                {
                    this.AddOrderTaskEntity(order);
                }
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
        void ClientQuotePriceForInstrument_OnEmptyClientQuotePriceClient(QuotePriceForInstrument clientQuotePriceForInstrument)
        {
            this.ClientQuotePriceForInstrument.Remove(clientQuotePriceForInstrument);
        }
        #endregion
    }
}
