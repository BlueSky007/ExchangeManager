using ManagerConsole.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TransactionError = iExchange.Common.TransactionError;
using CommonTransaction = iExchange.Common.Manager.Transaction;
using CommonOrder = iExchange.Common.Manager.Order;
using CommonOrderRelation = iExchange.Common.Manager.OrderRelation;
using OverridedQuotationMessage = Manager.Common.OverridedQuotationMessage;
using PlaceMessage = Manager.Common.PlaceMessage;
using QuoteMessage = Manager.Common.QuoteMessage;
using HitMessage = Manager.Common.HitMessage;
using DeleteMessage = Manager.Common.DeleteMessage;
using ExecuteMessage = Manager.Common.ExecuteMessage;
using UpdateMessage = Manager.Common.UpdateMessage;
using ManagerConsole.Helper;
using InitializeData = Manager.Common.InitializeData;
using ConfigParameter = Manager.Common.Settings.ConfigParameter;
using Phase = iExchange.Common.OrderPhase;
using OrderRelationType = iExchange.Common.OrderRelationType;
using OverridedQuotation = iExchange.Common.OverridedQuotation;
using SettingsParameter = Manager.Common.Settings.SettingsParameter;
using ExchangeInitializeData = Manager.Common.ExchangeInitializeData;
using Scheduler = iExchange.Common.Scheduler;
using OrderType = iExchange.Common.OrderType;
using System.Windows.Controls;
using SoundOption = Manager.Common.SoundOption;

namespace ManagerConsole.ViewModel
{
    public class ExchangeDataManager
    {
        public delegate void HitPriceReceivedRefreshUIEventHandler(int hitOrderCount);
        public event HitPriceReceivedRefreshUIEventHandler OnHitPriceReceivedRefreshUIEvent;

        public delegate void OrderHitPriceNotifyHandler(OrderTask orderTask);

        public delegate void DeleteOrderNotifyHandler(Order deletedOrder);
        public event DeleteOrderNotifyHandler OnDeleteOrderNotifyEvent;

        public delegate void ExecutedOrderNotifyHandler(Order order);
        public event ExecutedOrderNotifyHandler OnExecutedOrderNotifyEvent;

        private ObservableCollection<string> _ExchangeCodes = new ObservableCollection<string>();
        private Dictionary<Guid, Transaction> _Transactions = new Dictionary<Guid, Transaction>();
        private Dictionary<Guid, Order> _Orders = new Dictionary<Guid, Order>();
        private ConfigParameter _ConfigParameter = new ConfigParameter();
        private ObservableCollection<OrderTask> _OrderTaskEntities = new ObservableCollection<OrderTask>();
        private DQOrderTaskForInstrumentModel _DQOrderTaskForInstrumentModel = new DQOrderTaskForInstrumentModel();
        private MooMocOrderForInstrumentModel _MooMocOrderForInstrumentModel = new MooMocOrderForInstrumentModel();
        private OrderTaskModel _OrderTaskModel = new OrderTaskModel();

        private ProcessInstantOrder _ProcessInstantOrder = new ProcessInstantOrder();
        private ProcessLmtOrder _ProcessLmtOrder = new ProcessLmtOrder();
        private LMTProcessModel _LMTProcessModel = new LMTProcessModel();
        private ObservableCollection<Order> _ExecutedOrders = new ObservableCollection<Order>();
        private ExecuteOrderSummaryItemModel _ExecuteOrderSummaryItemModel = new ExecuteOrderSummaryItemModel();
        private SettingsParameterManager _SettingsParameterManager;
        private bool _IsInitializeCompleted = false;

        //private static IComparer<OrderTask> _HitOrderCompare = new HitOrderCompare();
        private Scheduler Scheduler = new Scheduler();
        private Scheduler.Action _RemoveTransactionWhenTimeOver;

        private MediaElement _SoundMedia;

        //询价
        private QuotePriceClientModel _QuotePriceClientModel = new QuotePriceClientModel();

        private TranPhaseManager _TranPhaseManager;


        #region Public Property
        public SettingsParameterManager SettingsParameterManager
        {
            get { return this._SettingsParameterManager; }
            set { this._SettingsParameterManager = value; }
        }

        public ReportDataManager ReportDataManager
        {
            get;
            set;
        }

        public ObservableCollection<string> ExchangeCodes
        {
            get { return this._ExchangeCodes; }
            set { this._ExchangeCodes = value; }
        }

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
        public Dictionary<string, ExchangeSettingManager> ExchangeSettingManagers
        {
            get;
            set;
        }

        public Dictionary<string, ExchangeTradingManager> ExchangeTradingManagers
        {
            get;
            set;
        }

        public bool IsInitializeCompleted
        {
            get { return this._IsInitializeCompleted; }
        }

        public ConfigParameter ConfigParameter
        {
            get { return this._ConfigParameter; }
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

        public ProcessLmtOrder ProcessLmtOrder
        {
            get { return this._ProcessLmtOrder; }
            set { this._ProcessLmtOrder = value; }
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

        #endregion

        public ExchangeDataManager(MediaElement mediaElement)
        {
            this._SoundMedia = mediaElement;
            this.SettingsManager = new SettingsManager();
            this.ExchangeSettingManagers = new Dictionary<string, ExchangeSettingManager>();
            this.ExchangeTradingManagers = new Dictionary<string, ExchangeTradingManager>();
            
            this._RemoveTransactionWhenTimeOver = new Scheduler.Action(this.RemoveTransactionWhenTimeOver);
            this._TranPhaseManager = new TranPhaseManager(this);
            this.ReportDataManager = new ReportDataManager(this);
        }

        #region Initialize Data
        public void Initialize(InitializeData initializeData)
        {
            this._ConfigParameter = initializeData.ConfigParameter;
            foreach (ExchangeInitializeData exchangeInitializeData in initializeData.ExchangeInitializeDatas)
            {
                string exchangeCode = exchangeInitializeData.ExchangeCode;
                ExchangeSettingManager settingManager = new ExchangeSettingManager(exchangeCode);
                settingManager.Initialize(exchangeInitializeData.SettingSet);
                
                this._ExchangeCodes.Add(exchangeCode);
                this.ExchangeSettingManagers.Add(exchangeCode, settingManager);

                ExchangeTradingManager exchangeTradingManager = new ExchangeTradingManager(settingManager);
                exchangeTradingManager.Initialize(exchangeInitializeData.SettingSet);
                this.ExchangeTradingManagers.Add(exchangeCode,exchangeTradingManager);
            }
            this.UpdateTradingSetting();
            this._IsInitializeCompleted = true;
        }

        public void Clear()
        {
            this._ExchangeCodes.Clear();
            this.ExchangeSettingManagers.Clear();
            this.ExchangeTradingManagers.Clear();
            this._IsInitializeCompleted = false;
            this._Transactions.Clear();
            this._Orders.Clear();
        }

        public void UpdateTradingSetting()
        {
            foreach (ExchangeTradingManager tradingManager in this.ExchangeTradingManagers.Values)
            {
                foreach (Transaction transaction in tradingManager.Transactions)
                {
                    this._Transactions.Add(transaction.Id, transaction);
                }
                foreach (Order order in tradingManager.Orders)
                {
                    this._Orders.Add(order.Id, order);
                }
            }

            this.AddOrderFromTransaction();
        }

        public void AddOrderFromTransaction()
        {
            App.MainFrameWindow.Dispatcher.BeginInvoke((Action)delegate()
            {
                foreach (Transaction transaction in this._Transactions.Values)
                {
                    Transaction tran = this._Transactions[transaction.Id];
                    this.TranPhaseManager.UpdateTransaction(tran);

                    foreach (Order order in tran.Orders)
                    {
                        order.Hit2();
                        this.AddOrderTaskEntity(order);
                    }
                }
            });
        }

        public void InitializeSettingParameter(SettingsParameter settingsParameter)
        {
            this._SettingsParameterManager = new SettingsParameterManager(settingsParameter);
        }
        #endregion

        #region Received Notify Convert

        public void ProcessOverridedQuotation(OverridedQuotationMessage overridedQuotationMessage)
        {
            string exchangeCode = overridedQuotationMessage.ExchangeCode;
            if (!this.ExchangeSettingManagers.ContainsKey(exchangeCode)) return;

            Dictionary<Guid, Dictionary<Guid, ExchangeQuotation>> exchangeQuotations = this.ExchangeSettingManagers[exchangeCode].ExchangeQuotations;

            foreach (OverridedQuotation quotation in overridedQuotationMessage.OverridedQs)
            {
                ExchangeQuotation exchangeQuotation = exchangeQuotations[quotation.QuotePolicyID][quotation.InstrumentID];

                if (exchangeQuotation == null) continue;
                exchangeQuotation.Ask = quotation.Ask;
                exchangeQuotation.Bid = quotation.Bid;
                exchangeQuotation.High = quotation.High;
                exchangeQuotation.Low = quotation.Low;
                exchangeQuotation.Origin = quotation.Origin;

                //Update DQ/Lmt UI Price
                if (this._ProcessInstantOrder.InstantOrderForInstrument != null)
                {
                    this._ProcessInstantOrder.InstantOrderForInstrument.UpdateOverridedQuotation(exchangeQuotation);
                }
                if (this._ProcessLmtOrder.LmtOrderForInstrument != null)
                {
                    this._ProcessLmtOrder.LmtOrderForInstrument.UpdateOverridedQuotation(exchangeQuotation);
                }
                if (this._MooMocOrderForInstrumentModel.MooMocOrderForInstruments.Count > 0)
                {
                    foreach (MooMocOrderForInstrument mooMocOrderForInstrument in this._MooMocOrderForInstrumentModel.MooMocOrderForInstruments)
                    {
                        mooMocOrderForInstrument.UpdateOverridedQuotation(exchangeQuotation);
                    }
                }
            }
        }
        
        public void ProcessQuoteMessage(QuoteMessage quoteMessage)
        {
            this.PlaySound(SoundOption.Enquiry);
            int waiteTime = this._SettingsParameterManager.DealingOrderParameter.EnquiryWaitTime;

            string exhcangeCode = quoteMessage.ExchangeCode;
            Guid customerId = quoteMessage.CustomerID;

            ExchangeSettingManager settingManager = this.GetExchangeSetting(exhcangeCode);

            Dictionary<Guid, InstrumentClient> instruments = this.GetInstruments(exhcangeCode);
            Customer customer = settingManager.GetCustomers().SingleOrDefault(P => P.Id == customerId);
            if (customer == null) return;

            if (!instruments.ContainsKey(quoteMessage.InstrumentID)) return;
            InstrumentClient instrument = instruments[quoteMessage.InstrumentID];
            QuotePolicyDetail quotePolicy = settingManager.GetQuotePolicyDetail(instrument.Id, customer);
            ExchangeQuotation quotation = this.GetExchangeQuotation(exhcangeCode, quotePolicy);

            QuotePriceClient quotePriceClient = new QuotePriceClient(quoteMessage, waiteTime, instrument, customer, quotation);
            this._QuotePriceClientModel.AddSendQuotePrice(quotePriceClient);

            App.MainFrameWindow.QuotePriceWindow.ShowQuotePriceWindow();
        }

        public void ProcessPlaceMessage(PlaceMessage placeMessage)
        {
            string exchangeCode = placeMessage.ExchangeCode;
            foreach (CommonTransaction commonTransaction in placeMessage.Transactions)
            {
                this.ProcessTransaction(exchangeCode,commonTransaction, false);

                if (commonTransaction.OrderType == OrderType.SpotTrade 
                    || commonTransaction.OrderType == OrderType.Limit 
                    || commonTransaction.OrderType == OrderType.Stop)
                {
                    this.Scheduler.Add(this.RemoveTransactionWhenTimeOver, commonTransaction.Id, DateTime.Now.AddSeconds(commonTransaction.OrderValidDuration));
                }
            }
            foreach (CommonOrder commonOrder in placeMessage.Orders)
            {
                commonOrder.ExchangeCode = placeMessage.ExchangeCode;
                this.Process(commonOrder,false);
            }
            foreach (CommonOrderRelation orderRelation in placeMessage.OrderRelations)
            {
                this.Process(orderRelation);
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
            try
            {
                string exchangeCode = executeMessage.ExchangeCode;
                bool oDisablePopup = this._SettingsParameterManager.DealingOrderParameter.DisablePopup;
                foreach (CommonTransaction commonTransaction in executeMessage.Transactions)
                {
                    if (commonTransaction.Error != null && commonTransaction.Error.Value != TransactionError.OK)
                    {
                        this.PlaySound(SoundOption.DQTradeFailed);
                    }
                    else if (this._Transactions.ContainsKey(commonTransaction.Id)
                        && this._Transactions[commonTransaction.Id].Phase == Phase.Executed)
                    {
                        continue;
                    }
                    else if (this._Transactions.ContainsKey(commonTransaction.Id))
                    {
                        Transaction tran = this._Transactions[commonTransaction.Id];
                        tran.ExecuteTime = commonTransaction.ExecuteTime;
                        tran.Phase = iExchange.Common.OrderPhase.Executed;
                    }
                    else
                    {
                        this.ProcessTransaction(executeMessage.ExchangeCode, commonTransaction);
                    }
                }

                this.PlaySound(SoundOption.DQTradeSucceed);
                foreach (CommonOrder commonOrder in executeMessage.Orders)
                {
                    commonOrder.ExchangeCode = executeMessage.ExchangeCode;
                    this.Process(commonOrder, false);
                }

                //Change Order status
                foreach (CommonTransaction commonTransaction in executeMessage.Transactions)
                {
                    Transaction tran = this._Transactions[commonTransaction.Id];
                    this.TranPhaseManager.UpdateTransaction(tran);

                    this.TranPhaseManager.AddExecutedOrder(tran);
                    this.TranPhaseManager.AddOrderToGroupNetPosition(tran);
                }

               
            }
            catch (Exception ex)
            {
                throw ex;
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

                    Order hitOrder = this._Orders[commonOrder.Id];
                    this._ProcessLmtOrder.UpdateHitOrder(hitOrder);
                    this._ProcessLmtOrder.TopLmtOrder(hitOrder);
                }
            }

            if (this.OnHitPriceReceivedRefreshUIEvent != null)
            {
                int hitOrdersCount = hitMessage.Orders.Count();
                this.OnHitPriceReceivedRefreshUIEvent(hitOrdersCount);
            }
        }

        public void ProcessUpdateMessage(UpdateMessage updateMessage)
        {
            string exchangeCode = updateMessage.ExchangeCode;
            ExchangeSettingManager settingManager = this.GetExchangeSetting(exchangeCode);

            settingManager.UpdateNotify(updateMessage.AddSettingSets, updateMessage.DeletedSettings, updateMessage.ExchangeUpdateDatas);
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

            this.PlaySound(SoundOption.DQDealerIntervene);

            foreach (CommonTransaction commonTransaction in deleteMessage.Transactions)
            {
                this.ProcessTransaction(deleteMessage.ExchangeCode,commonTransaction);
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
                this.TranPhaseManager.AddExecutedOrder(newTransaction);
                foreach (Order order in newTransaction.Orders)
                {
                    this.TranPhaseManager.AddOrderProcessBuySellLot(deleteMessage.InstrumentID, order); 
                }

                TranPhaseManager.UpdateTransaction(newTransaction);
            }
        }

        private void ProcessTransaction(string exchangeCode,CommonTransaction commonTransaction)
        {
            this.ProcessTransaction(exchangeCode, commonTransaction, false);
        }

        private void ProcessTransaction(string exchangeCode, CommonTransaction commonTransaction, bool isExecuted)
        {
            ExchangeSettingManager settingManager = this.GetExchangeSetting(exchangeCode);
            Dictionary<Guid, InstrumentClient> instruments = this.GetInstruments(exchangeCode);
            Dictionary<Guid, Account> accounts = this.GetAccount(exchangeCode);
            if (!this._Transactions.ContainsKey(commonTransaction.Id))
            {
                if (!instruments.ContainsKey(commonTransaction.InstrumentId) || !accounts.ContainsKey(commonTransaction.AccountId))
                    return;
                Account account = accounts[commonTransaction.AccountId];
                InstrumentClient instrument = instruments[commonTransaction.InstrumentId];
                Transaction transaction = new Transaction(commonTransaction, account, instrument);

                //Set Transaction.ContractSize
                if (transaction.ContractSize == 0)
                {
                    TradePolicyDetail tradePolicyDetail = settingManager.GetTradePolicyDetail(account.TradePolicyId, instrument.Id);
                    if (tradePolicyDetail != null)
                    {
                        transaction.ContractSize = tradePolicyDetail.ContractSize;
                    }
                }
                this._Transactions.Add(transaction.Id, transaction);
            }
            else
            {
                if (isExecuted && commonTransaction.SubType == iExchange.Common.TransactionSubType.Match)
                {
                    Transaction transaction = this._Transactions[commonTransaction.Id];
                    if (transaction.Phase == iExchange.Common.OrderPhase.Canceled || transaction.Phase == iExchange.Common.OrderPhase.Executed)
                    {
                        return;
                    }
                }
                if (this._Transactions[commonTransaction.Id].Phase == iExchange.Common.OrderPhase.Executed && isExecuted)
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

                if (isExecuted && transaction.SubType == iExchange.Common.TransactionSubType.Match
                    && (transaction.Phase == iExchange.Common.OrderPhase.Canceled || transaction.Phase == iExchange.Common.OrderPhase.Executed))
                {
                    return;
                }
                if (this._Orders[commonOrder.Id].Phase == iExchange.Common.OrderPhase.Executed && isExecuted)
                    return;
                this._Orders[commonOrder.Id].Update(commonOrder);
            }
        }

        private void Process(CommonOrderRelation commonOrderRelation)
        {
            Order order = null;
            if (this._Orders.ContainsKey(commonOrderRelation.OrderId))
            {
                order = this._Orders[commonOrderRelation.OrderId];

                if (commonOrderRelation.RelationType == OrderRelationType.Close)
                {
                    OrderRelation relation = new OrderRelation(commonOrderRelation);

                    Order openOrder = this._Orders.ContainsKey(relation.OpenOrderId) ? this._Orders[relation.OpenOrderId] : null;

                    if (openOrder != null)
                    {
                        string openOrderInfo = string.Format("{0}x{1}x{2}", openOrder.Transaction.SubmitTime.ToString("yyyy-MM-dd"), openOrder.Lot, openOrder.SetPrice);
                        relation.OpenOrderInfo = openOrderInfo;

                        CloseOrder closerOrder = new CloseOrder(order, relation.ClosedLot);
                        order.CloseOrders.Add(closerOrder);
                    }

                    order.Transaction.AddOrderRelation(relation);
                }
            }       
        }

        private void AddOrderTaskEntity(Order order)
        {
            if (order.Transaction.OrderType == iExchange.Common.OrderType.MarketOnOpen || order.Transaction.OrderType == iExchange.Common.OrderType.MarketOnClose)
            {
                OrderTask orderTask = new OrderTask(order);
                orderTask.BaseOrder = order;
                orderTask.OrderStatus = OrderStatus.TimeArrived;

                this._MooMocOrderForInstrumentModel.AddMooMocOrderForInstrument(orderTask);
            }
            else if(order.Transaction.OrderType == iExchange.Common.OrderType.SpotTrade)
            {
                OrderTask orderTask = new OrderTask(order);
                orderTask.BaseOrder = order;

                this._ProcessInstantOrder.AddInstanceOrder(orderTask);
            }
            else
            {
                OrderTask orderTask = new OrderTask(order);
                orderTask.BaseOrder = order;
                this._ProcessLmtOrder.AddLmtOrder(orderTask);
            }
        }
        #endregion

        public void AddExecutedOrderSummaryItem(Order order)
        {
            if (this.OnExecutedOrderNotifyEvent != null)
            {
                this.OnExecutedOrderNotifyEvent(order);
            }
        }

        #region 辅助方法
        private Dictionary<Guid, InstrumentClient> GetInstruments(string exchangeCode)
        {
            return this.ExchangeSettingManagers[exchangeCode].Instruments;
        }

        private Dictionary<Guid, Account> GetAccount(string exchangeCode)
        {
            return this.ExchangeSettingManagers[exchangeCode].Accounts;
        }

        internal ICollection<Order> GetOrders()
        {
            return new List<Order>(this._Orders.Values);
        }

        internal ExchangeSettingManager GetExchangeSetting(string exchangeCode)
        {
            return this.ExchangeSettingManagers[exchangeCode];
        }

        internal ExchangeQuotation GetExchangeQuotation(string exchangeCode, QuotePolicyDetail quotePolicyDetail)
        {
            if (this.ExchangeSettingManagers.ContainsKey(exchangeCode))
            {
                ExchangeQuotation exchangeQuotation = this.ExchangeSettingManagers[exchangeCode].ExchangeQuotations[quotePolicyDetail.QuotePolicyId][quotePolicyDetail.InstrumentId];
                return exchangeQuotation;
            }
            else
            {
                return null;
            }
        }

        internal void PlaySound(SoundOption soundkey)
        {
            string soundPath = this._SettingsParameterManager.GetSoundPath(soundkey);

            MediaManager.PlayMedia(this._SoundMedia, soundPath);
        }
        #endregion

        private void RemoveTransactionWhenTimeOver(object sender, object Args)
        {
            App.MainFrameWindow.Dispatcher.BeginInvoke((Action)delegate()
            {
                Guid transactionId = (Guid)Args;

                if (!this._Transactions.ContainsKey(transactionId)) return;
                Transaction tran = this._Transactions[transactionId];

                List<OrderTask> instanceOrders = new List<OrderTask>();
                List<OrderTask> lmtOrders = new List<OrderTask>();

                foreach (Order order in tran.Orders)
                {
                    if (order.Transaction.OrderType == OrderType.SpotTrade)
                    {
                        foreach (OrderTask orderTask in this._ProcessInstantOrder.OrderTasks)
                        {
                            if (orderTask.OrderId == order.Id)
                            {
                                instanceOrders.Add(orderTask);
                            }
                        }
                    }
                    else if (order.Transaction.OrderType == OrderType.Limit || order.Transaction.OrderType == OrderType.Stop)
                    {
                        foreach (OrderTask orderTask in this._ProcessLmtOrder.OrderTasks)
                        {
                            if (orderTask.OrderId == order.Id)
                            {
                                lmtOrders.Add(orderTask);
                            }
                        }
                    }
                }
                this._ProcessInstantOrder.RemoveInstanceOrder(instanceOrders);
                this._ProcessLmtOrder.RemoveLmtOrder(lmtOrders);
            });
        }
    }
}
