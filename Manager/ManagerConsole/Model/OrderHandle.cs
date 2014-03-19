using ManagerConsole.Helper;
using ManagerConsole.Model;
using ManagerConsole.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using CommonParameter = Manager.Common.Settings.SystemParameter;
using AccountInfor = iExchange.Common.Manager.AccountInformation;
using OperationType = Manager.Common.OperationType;
using ConfigParameter = Manager.Common.Settings.ConfigParameter;
using TransactionError = iExchange.Common.TransactionError;
using System.Xml;
using System.Collections.ObjectModel;
using Manager.Common.LogEntities;
using iExchange.Common.Manager;
using OrderType = iExchange.Common.OrderType;
using TransactionType = iExchange.Common.TransactionType;
using CancelReason = iExchange.Common.CancelReason;
using System.IO;

namespace ManagerConsole.Model
{
    public class OrderHandle
    {
        private const string returnLine = "\r\n";
        private static string _DealingOrderPath = @"D:\DealingOrder.txt";

        public delegate void WaitOrderNotifyHandler(OrderTask orderTask);
        public event WaitOrderNotifyHandler OnOrderWaitNofityEvent;

        public delegate void ExecuteOrderNotifyHandler(OrderTask orderTask);
        public event ExecuteOrderNotifyHandler OnExecuteOrderNotifyEvent;

        private CommonDialogWin _CommonDialogWin;
        private ConfirmDialogWin _ConfirmDialogWin;
        private ConfirmOrderDialogWin ConfirmOrderDialogWin;
        private ManagerConsole.MainWindow _App;
        private Transaction _RejectTran;
        private TranPhaseManager _TranPhaseManager;

        public OrderHandle()
        {
            this._App = ((ManagerConsole.MainWindow)Application.Current.MainWindow);
            this._CommonDialogWin = this._App._CommonDialogWin;
            this._ConfirmDialogWin = this._App._ConfirmDialogWin;
            this.ConfirmOrderDialogWin = this._App._ConfirmOrderDialogWin;
            this.ConfirmOrderDialogWin.OnConfirmDialogResult += new ConfirmOrderDialogWin.ConfirmDialogResultHandle(ExecuteOrderHandle);
            this.ConfirmOrderDialogWin.OnModifyPriceDialogResult += new ConfirmOrderDialogWin.ConfirmModifyPriceResultHandle(ModifyPriceHandle);
            this.ConfirmOrderDialogWin.OnRejectOrderDialogResult += new ConfirmOrderDialogWin.RejectOrderResultHandle(RejectOrderHandle);
            this._TranPhaseManager = this._App.ExchangeDataManager.TranPhaseManager;
        }

        public TranPhaseManager TranPhaseManager
        {
            get { return this._TranPhaseManager; }
            set { this._TranPhaseManager = value; }
        }

        #region Order Action

        public void OnOrderAccept(OrderTask orderTask)
        {
            try
            {
                SystemParameter systemParameter = this._App.ExchangeDataManager.GetExchangeSetting(orderTask.ExchangeCode).SystemParameter;
                ConfigParameter configParameter = this._App.ExchangeDataManager.ConfigParameter;
                bool allowModifyOrderLot = configParameter.AllowModifyOrderLot;
                systemParameter.CanDealerViewAccountInfo = true;
                bool isOK = OrderTaskManager.CheckDQOrder(orderTask, systemParameter, configParameter);
                isOK = true;

                if (isOK)
                {
                    if (OrderTaskManager.IsNeedDQMaxMove(orderTask))
                    {
                        this.Commit(orderTask, orderTask.SetPrice, (decimal)orderTask.Lot);
                    }
                    else
                    {
                        this.Commit(orderTask, string.Empty, (decimal)orderTask.Lot);
                    }
                }
                else
                {
                    if (systemParameter.CanDealerViewAccountInfo)
                    {
                        this.ShowConfirmOrderFrm(systemParameter.CanDealerViewAccountInfo, orderTask, HandleAction.OnOrderAccept);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                string logData = "OrderHandle.OnOrderAccept:Error:" + returnLine;
                logData += ex.ToString();
                this.WriteLog(logData);
            }
        }

        public void OnOrderReject(OrderTask order)
        {
            DealingOrderParameter parameter = this._App.ExchangeDataManager.SettingsParameterManager.DealingOrderParameter;
            if (order.OrderStatus == OrderStatus.WaitOutPriceDQ || order.OrderStatus == OrderStatus.WaitOutLotDQ)
            {
                if (parameter.ConfirmRejectDQOrder)
                {
                    if (MessageBox.Show("Are you sure reject the order?", "Reject", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        this.DoReject(order);
                    }
                }
                else
                {
                    this.DoReject(order);
                }
            }
        }

        public void DoReject(OrderTask order)
        {
            if (OrderTaskManager.CheckWhenRejectOrder(order))
            {
                foreach (Order orderEntity in order.Transaction.Orders)
                {
                    orderEntity.Status = OrderStatus.Deleting;
                }
                this.CancelTransaction(order);
            }
            else
            {
                string sMsg = "The order is canceled or executed already";
                this._CommonDialogWin.ShowDialogWin(sMsg, "Alert");
            }
        }


        //接受限价单下单
        private void AcceptOrderPlace(OrderTask orderTask)
        {
            LogOrder logEntity = LogEntityConvert.GetLogOrderEntity(orderTask, OperationType.OnOrderAcceptPlace, "ExecuteOrder");
            ConsoleClient.Instance.AcceptPlace(orderTask.Transaction, logEntity, AcceptPlaceCallback);
        }

        private void CancelTransaction(OrderTask orderTask)
        {
            LogOrder logEntity = LogEntityConvert.GetLogOrderEntity(orderTask, OperationType.OnOrderCancel, "RejectOrder");
            ConsoleClient.Instance.Cancel(orderTask.Transaction, CancelReason.DealerCanceled, logEntity, CancelTransactionCallback);
        }

        public void OnOrderCancel(OrderTask orderTask)
        {
            if (orderTask.OrderStatus == OrderStatus.WaitAcceptRejectPlace
                || orderTask.OrderStatus == OrderStatus.WaitAcceptRejectCancel
                || orderTask.OrderStatus == OrderStatus.WaitOutPriceLMT
                || orderTask.OrderStatus == OrderStatus.WaitOutLotLMT
                || orderTask.OrderStatus == OrderStatus.WaitOutLotLMTOrigin)
            {
                //at this case, do nothing.
            }
            else
            {
                string rejectOrderMsg = this.GetRejectOrderMessage(orderTask);
                this.ConfirmOrderDialogWin.ShowRejectOrderWin(rejectOrderMsg, orderTask, HandleAction.OnOrderCancel);
            }
        }

        private void RejectPlace(Transaction transaction)
        {
            ConsoleClient.Instance.CancelPlace(transaction, CancelReason.CustomerCanceled, RejectPlaceCallback);
        }

        #region 撞线状态操作
        public void OnOrderExecute(OrderTask orderTask)
        {
            ExchangeSettingManager settingManager = this._App.ExchangeDataManager.GetExchangeSetting(orderTask.ExchangeCode);
            SystemParameter systemParameter = settingManager.SystemParameter;
            systemParameter.CanDealerViewAccountInfo = true;
            bool isOK = OrderTaskManager.CheckExecuteOrder(orderTask);

            if (isOK)
            {
                this.ShowConfirmOrderFrm(systemParameter.CanDealerViewAccountInfo, orderTask,HandleAction.OnOrderExecute);
            }
        }

        public void OnOrderWait(OrderTask orderTask)
        {
            try
            {
                if (orderTask.OrderStatus == OrderStatus.WaitOutPriceLMT
                    || orderTask.OrderStatus == OrderStatus.WaitOutLotLMT
                    || orderTask.OrderStatus == OrderStatus.WaitOutLotLMTOrigin)
                {
                    orderTask.ChangeStatus(OrderStatus.WaitNextPrice);
                    orderTask.ResetHit();
                    Guid[] orderIds = new Guid[] { orderTask.OrderId };
                    ConsoleClient.Instance.ResetHit(orderTask.ExchangeCode, orderIds);

                    if (this.OnOrderWaitNofityEvent != null)
                    {
                        this.OnOrderWaitNofityEvent(orderTask);
                    }
                }
            }
            catch (Exception ex)
            {
                string logData = "OrderHandle.OnOrderWait:Error:" + returnLine;
                logData += ex.ToString();
                this.WriteLog(logData);
            }
        }

        public void OnOrderModify(OrderTask orderTask,string origin)
        {
            if (orderTask.OrderStatus == OrderStatus.WaitOutLotLMT)
            {
                this.ConfirmOrderDialogWin.ShowDialogWin("Modify Price", orderTask, origin, HandleAction.OnOrderModify);
            }
        }
        public void OnOrderUpdate(OrderTask orderTask,LmtOrderForInstrument currentInstrument)
        { 
            if(orderTask.Transaction.OrderType == OrderType.Limit || orderTask.Transaction.OrderType == OrderType.Market)
            {
                if (orderTask.OrderStatus == OrderStatus.WaitOutLotLMT || orderTask.OrderStatus == OrderStatus.WaitOutLotLMTOrigin)
                {
                    InstrumentClient instrument = orderTask.Instrument;
                    if (instrument.IsNormal ^ (orderTask.IsBuy == BuySell.Buy))
                        orderTask.BestPrice = currentInstrument.Bid;
                    else
                        orderTask.BestPrice = currentInstrument.Ask;

                    if (orderTask.Transaction.OrderType == OrderType.Limit)
                        orderTask.SetPrice = orderTask.BestPrice;
                }
            }
        }
        //Old Function
        public void OnOrderUpdate(OrderTask orderTask)
        { 
            if(orderTask.Transaction.OrderType == OrderType.Limit || orderTask.Transaction.OrderType == OrderType.Market)
            {
                if (orderTask.OrderStatus == OrderStatus.WaitOutLotLMT || orderTask.OrderStatus == OrderStatus.WaitOutLotLMTOrigin)
                {
                    InstrumentClient instrument = orderTask.Instrument;
                    if (instrument.IsNormal ^ (orderTask.IsBuy == BuySell.Buy))
                        orderTask.BestPrice = instrument.Bid;
                    else
                        orderTask.BestPrice = instrument.Ask;

                    if (orderTask.Transaction.OrderType == OrderType.Limit)
                        orderTask.SetPrice = orderTask.BestPrice;
                }
            }
        }
        #endregion

       
        //Accept or Reject Lmt Order/Moo.Moc
        public void OnOrderAcceptPlace(OrderTask orderTask)
        {
            if (orderTask.OrderStatus == OrderStatus.WaitAcceptRejectPlace)
            {
                foreach (Order order in orderTask.Transaction.Orders)
                {
                    order.ChangeStatus(OrderStatus.WaitServerResponse);
                }
            }
            this.AcceptOrderPlace(orderTask);
        }

        public void OnOrderRejectPlace(OrderTask orderTask)
        {
            if (orderTask.OrderStatus == OrderStatus.WaitAcceptRejectPlace
            || orderTask.OrderStatus == OrderStatus.WaitAcceptRejectCancel
            || orderTask.OrderStatus == OrderStatus.WaitOutPriceDQ
            || orderTask.OrderStatus == OrderStatus.WaitOutLotDQ
            || (orderTask.OrderStatus == OrderStatus.WaitNextPrice && orderTask.Transaction.OrderType == OrderType.Limit))
            {
                string rejectOrderMsg = this.GetRejectOrderMessage(orderTask);
                this._RejectTran = orderTask.Transaction;
                this.ConfirmOrderDialogWin.ShowRejectOrderWin(rejectOrderMsg, orderTask,HandleAction.OnOrderRejectPlace);
            }
        }

        #region 批量成交单
        public void OnLMTApply(LMTProcessForInstrument lMTProcessForInstrument)
        {
            string newAskPrice = lMTProcessForInstrument.AskPrice;
            string newBidPrice = lMTProcessForInstrument.BidPrice;
            foreach (OrderTask orderTask in lMTProcessForInstrument.OrderTasks)
            {
                if (orderTask.IsBuy == BuySell.Buy)
                {
                    orderTask.SetPrice = newAskPrice;
                }
                else
                {
                    orderTask.SetPrice = newBidPrice;
                }
            }
        }

        //All Lmt Order
        public void OnLMTExecute(ProcessLmtOrder processLmtOrder,bool isBuy)
        {
            foreach (OrderTask orderTask in processLmtOrder.OrderTasks)
            {
                if (isBuy != (orderTask.IsBuy == BuySell.Buy)) continue;
                if (!OrderTaskManager.CheckExecuteOrder(orderTask)) continue;

                string executedPrice = isBuy ? processLmtOrder.LmtOrderForInstrument.CustomerBidPrice.ToString() : processLmtOrder.LmtOrderForInstrument.CustomerAskPrice.ToString();
                string marketPrice = isBuy ? processLmtOrder.LmtOrderForInstrument.Ask : processLmtOrder.LmtOrderForInstrument.Bid;

                if (!string.IsNullOrEmpty(marketPrice))
                {
                    bool isValidPrice = OrderTaskManager.IsProblePrice(orderTask.Instrument, marketPrice, executedPrice);
                    if (!isValidPrice)
                    {
                        string executePrice = orderTask.IsBuy == BuySell.Buy ? processLmtOrder.LmtOrderForInstrument.Ask : processLmtOrder.LmtOrderForInstrument.Bid;
                        this.Commit(orderTask, executePrice, (decimal)orderTask.Lot);
                    }
                }
                else
                {
                    string msg = "Out of Range,Accept or Reject?";
                    this.ConfirmOrderDialogWin.ShowRejectOrderWin(msg, orderTask, HandleAction.OnLMTExecute);
                }
            }
        }

        public void OnLMTOrderWait(ProcessLmtOrder processLmtOrder, bool isBuy)
        {
            foreach (OrderTask orderTask in processLmtOrder.OrderTasks)
            {
                if (isBuy != (orderTask.IsBuy == BuySell.Buy)) continue;

                if (orderTask.OrderStatus == OrderStatus.WaitOutPriceLMT
                || orderTask.OrderStatus == OrderStatus.WaitOutLotLMT
                || orderTask.OrderStatus == OrderStatus.WaitOutLotLMTOrigin)
                {
                    orderTask.ChangeStatus(OrderStatus.WaitNextPrice);
                    orderTask.ResetHit();
                    Guid[] orderIds = new Guid[] { orderTask.OrderId };
                    ConsoleClient.Instance.ResetHit(orderTask.ExchangeCode, orderIds);

                    if (this.OnOrderWaitNofityEvent != null)
                    {
                        this.OnOrderWaitNofityEvent(orderTask);
                    }
                }
            } 
        }
        #endregion

        #endregion

        #region MOO/MOC Order Action

        public void OnMooMocAccept(MooMocOrderForInstrument mooMocOrderForInstrument)
        {
            foreach (OrderTask orderTask in mooMocOrderForInstrument.OrderTasks)
            {
                if (orderTask.IsSelected)
                {
                    this.OnOrderAcceptPlace(orderTask);
                }
            }
        }

        public void OnMooMocReject(MooMocOrderForInstrument mooMocOrderForInstrument)
        {
            foreach (OrderTask orderTask in mooMocOrderForInstrument.OrderTasks)
            {
                if (orderTask.IsSelected)
                {
                    orderTask.SetPrice = orderTask.IsBuy == BuySell.Buy ? mooMocOrderForInstrument.Bid : mooMocOrderForInstrument.Ask;
                    this.OnOrderRejectPlace(orderTask);
                }
            }
        }

        public void OnMooMocCancel(MooMocOrderForInstrument mooMocOrderForInstrument)
        {
            if (MessageBox.Show("Are you sure cancel the order?", "Alert", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                foreach (OrderTask orderTask in mooMocOrderForInstrument.OrderTasks)
                {
                    if (orderTask.IsSelected)
                    {
                        this.CancelTransaction(orderTask);
                    }
                }
            }
        }

        public void OnMooMocExecute(MooMocOrderForInstrument mooMocOrderForInstrument)
        { 
            foreach(OrderTask orderTask in mooMocOrderForInstrument.OrderTasks)
            {
                if(orderTask.IsSelected)
                {
                    if (!this.IsCanExecutedMooMoc(orderTask)) continue;

                    this.Commit(orderTask, orderTask.SetPrice, (decimal)orderTask.Lot);
                }
            }
        }

        public void OnMooMocApply(MooMocOrderForInstrument mooMocOrderForInstrument)
        {
            foreach (OrderTask orderTask in mooMocOrderForInstrument.OrderTasks)
            {
                if (orderTask.IsSelected)
                {
                    InstrumentClient instrument = orderTask.Transaction.Instrument;
                    if (instrument.IsNormal == (orderTask.IsBuy== BuySell.Buy))
                    {
                        orderTask.SetPrice = mooMocOrderForInstrument.Ask;
                    }
                    else
                    {
                        orderTask.SetPrice = mooMocOrderForInstrument.Bid;
                    }
                }
            }
        }

        private bool IsCanExecutedMooMoc(OrderTask orderTask)
        {
            bool canExecute = false;
            InstrumentClient instrument = orderTask.Transaction.Instrument;
            if (orderTask.OrderType == OrderType.MarketOnOpen)
            {
                canExecute = instrument.DayOpenTime != null 
                    && orderTask.Transaction.BeginTime.Ticks == instrument.DayOpenTime.Ticks;
            }
            else if (orderTask.OrderType == OrderType.MarketOnClose)
            {
                canExecute = true;
            }
            return canExecute;
        }
        #endregion

        #region 辅助方法
        private void GetAccountInforCaption(HandleAction action,out string message, out string title)
        {
            message = string.Empty;
            title = string.Empty;
            switch (action)
            {
                case HandleAction.OnOrderAccept:
                    message = "Accept the order?";
                    title = "Confirm";
                    break;
                case HandleAction.OnOrderExecute:
                    message = "Execute the order?";
                    title = "Execute";
                    break;
            }
        }

        private void ShowConfirmOrderFrm(bool canDealerViewAccountInfo, OrderTask orderTask,HandleAction action)
        {
            string message = string.Empty;
            string title = string.Empty;
            this.GetAccountInforCaption(action,out message,out title);
            if (canDealerViewAccountInfo)
            {
                //just test data
                AccountInformation accountInfor = ConsoleClient.Instance.GetAcountInfo(orderTask.ExchangeCode, orderTask.Transaction.Id);
                if (accountInfor == null)
                {
                    this._App._CommonDialogWin.ShowDialogWin("Get account information failed!","Error");
                    return;
                }
                this._App._ConfirmOrderDialogWin.ShowDialogWin(accountInfor, title, orderTask, action);
            }
            else
            {
                if (MessageBox.Show(message, title, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    this.Commit(orderTask, string.Empty, (decimal)orderTask.Lot);
                }
            }
        }

        private void ProcessOrder(OrderTask orderTask)
        {
            if (orderTask.OrderStatus == OrderStatus.WaitOutPriceLMT || orderTask.OrderStatus == OrderStatus.WaitOutLotLMT
            || orderTask.OrderStatus == OrderStatus.WaitOutLotLMTOrigin)
            {
                if (this.IsPriceExceedMaxMin(orderTask.BestPrice) == true && orderTask.OrderStatus == OrderStatus.WaitOutLotLMT)
                {
                    //Waiting for Dealer Accept/Reject
                    orderTask.ChangeStatus(OrderStatus.WaitOutPriceLMT);
                    //this.mainWindow.oDealingConsole.PlaySound(SoundOption.LimitDealerIntervene);
                }
                else
                {
                    if (orderTask.Transaction.OrderType == OrderType.Market)
                    {
                        orderTask.SetPrice = orderTask.BestPrice;
                    }
                    //Commit Transaction
                    //if (this.lotChanged) lot = this.lot.toString();
                    this.Commit(orderTask,string.Empty,(decimal)orderTask.Lot);
                }
            }
        }

        private bool IsPriceExceedMaxMin(string bestPrice)
        {
            return true;
        }

        private string GetRejectOrderMessage(OrderTask orderTask)
        {
            bool isBuy = orderTask.IsBuy == BuySell.Buy;
            Account account = orderTask.Transaction.Account;
            InstrumentClient instrument = orderTask.Instrument;
            return account.Code + (isBuy ? " buy " : " sell ") + orderTask.Lot + " " + instrument.Code + " at " + orderTask.SetPrice;
        }

        private string GetMooMocDescription(OrderTask orderTask,string marketPrice)
        {
            bool isBuy = orderTask.IsBuy == BuySell.Buy;
            Account account = orderTask.Transaction.Account;
            InstrumentClient instrument = orderTask.Instrument;
            return account.Code + (isBuy ? " buy " : " sell ") + orderTask.Lot + " " + instrument.Code + " at " + marketPrice;
        }
 
        private void Commit(OrderTask orderTask, string executePrice, decimal lot)
        {
            orderTask.ChangeStatus(OrderStatus.WaitServerResponse);
            orderTask.BaseOrder.ChangeStatus(OrderStatus.WaitServerResponse);

            string buyPrice = string.Empty;
            string sellPrice = string.Empty;
            executePrice = string.IsNullOrEmpty(executePrice) ? orderTask.SetPrice:executePrice;

            if(orderTask.IsBuy == BuySell.Buy)
                buyPrice = executePrice;
            else
                sellPrice = executePrice;

            switch(orderTask.Transaction.Type)
            {
                case TransactionType.Single:
                    foreach (Order order in orderTask.Transaction.Orders)
                    {
                        order.ChangeStatus(OrderStatus.WaitServerResponse);
                    }
                    break;
                case TransactionType.Pair:
                    foreach (Order orderTemp in orderTask.Transaction.Orders)
                    {
                        if (orderTemp.Id != orderTask.OrderId)
                        {
                            orderTemp.ChangeStatus(OrderStatus.WaitServerResponse);
                            executePrice = orderTemp.SetPrice;

                            if (orderTemp.BuySell == BuySell.Buy)
                                buyPrice = (string.IsNullOrEmpty(executePrice)) ? "" : executePrice.ToString();
                            else
                                sellPrice = (string.IsNullOrEmpty(executePrice)) ? "" : executePrice.ToString();
                            break;
                        }
                    }
                    break;
                case TransactionType.OneCancelOther:
                    foreach (Order orderTemp in orderTask.Transaction.Orders)
                    {
                        if (orderTemp.Id != orderTask.OrderId)
                        {
                            orderTemp.ChangeStatus(OrderStatus.Deleting);
                        }
                    }
                    break;
            }
            //if (!order.id) alert("The order is not valid!");
            LogOrder logEntity = LogEntityConvert.GetLogOrderEntity(orderTask, OperationType.OnOrderExecute, "ExecuteOrder");
            ConsoleClient.Instance.Execute(orderTask.Transaction, buyPrice, sellPrice, (decimal)orderTask.Lot, orderTask.OrderId, logEntity, ExecuteCallback);
        }

        private void ChangeOrderStatus(OrderTask order,OrderStatus newStatus)
        {
            foreach (Order baseOrder in order.Transaction.Orders)
            {
                if (baseOrder.Id != order.OrderId)
                {
                    baseOrder.ChangeStatus(newStatus);
                }
            }
        }

        //成交单
        void ExecuteOrderHandle(bool yesOrNo, OrderTask orderTask, HandleAction action)
        {
            if (yesOrNo)
            {
                switch(action)
                {
                    case HandleAction.None:
                        break;
                    case HandleAction.OnOrderAccept:
                        this.Commit(orderTask, string.Empty, (decimal)orderTask.Lot);
                        break;
                    case HandleAction.OnOrderExecute:
                        this.Commit(orderTask, string.Empty, (decimal)orderTask.Lot);
                        break;
                }
            }
        }

        //修改价格
        void ModifyPriceHandle(bool yesOrNo, string newPrice, OrderTask orderTask,string origin, HandleAction action)
        {
            if (yesOrNo)
            {
                if (action == HandleAction.OnOrderModify)
                {
                    //Check Modify Price DailyMaxMove
                    InstrumentClient instrument = orderTask.Transaction.Instrument;
                    if (OrderTaskManager.IsLimitedPriceByDailyMaxMove(newPrice, instrument))
                    {
                        string msg = "Out of daily max move,previous close price is " + instrument.PreviousClosePrice;
                        this._CommonDialogWin.ShowDialogWin(msg, "Alert");
                        return;
                    }
                    bool? isValidPrice = false;
                    isValidPrice = OrderTaskManager.IsValidPrice(instrument,origin,decimal.Parse(newPrice));

                    if(isValidPrice.HasValue)
                    {
                        if (isValidPrice.Value)
                        {
                            orderTask.BestPrice = newPrice;
                            orderTask.SetPrice = newPrice;
                        }
                    }
                }
            }
        }

        //取消单
        void RejectOrderHandle(bool yesOrNo, OrderTask orderTask, HandleAction action)
        {
            if (yesOrNo)
            {
                switch (action)
                {
                    case HandleAction.OnOrderRejectPlace:
                        this.CancelTransaction(orderTask);
                        break;
                    case HandleAction.OnOrderCancel:
                        this.CancelTransaction(orderTask);
                        break;
                    case HandleAction.OnLMTExecute:
                        this.Commit(orderTask, string.Empty, (decimal)orderTask.Lot);
                        break;
                }
            }
        }
        #endregion

        #region 批单回调事件
        //Call Back Event
        private void AcceptPlaceCallback(Transaction tran, TransactionError transactionError)
        {
            App.MainFrameWindow.Dispatcher.BeginInvoke((Action)delegate()
            {
                if (transactionError == TransactionError.OK)
                {
                    tran.Phase = iExchange.Common.OrderPhase.Placed;
                    TranPhaseManager.UpdateTransaction(tran);
                }
            });
        }

        private void RejectPlaceCallback(Transaction tran,TransactionError transactionError)
        {
            App.MainFrameWindow.Dispatcher.BeginInvoke((Action)delegate()
            {
                if (transactionError == TransactionError.OK)
                {
                    this._CommonDialogWin.ShowDialogWin("Reject Place Succeed", "Infromation");
                }
            });
        }

        private void CancelTransactionCallback(Transaction tran,TransactionError transactionError)
        {
            App.MainFrameWindow.Dispatcher.BeginInvoke((Action)delegate()
            {
                if (transactionError == TransactionError.OK)
                {
                    foreach (Order order in tran.Orders)
                    {
                        if (order.Status == OrderStatus.Deleting)
                        {
                            order.ChangeStatus(OrderStatus.Canceled);
                        }
                    }
                    this.RemoveTransaction(tran);
                }
                else
                {
                    bool oDisablePopup = true;  //配置参数
                    if (oDisablePopup)
                    {
                        string sMsg = MessageHelper.Instance.GetMessageForOrder(transactionError.ToString()); // this._Message.GetMessageForOrder("DealerCanceled");
                        this._CommonDialogWin.ShowDialogWin(sMsg, "Alert", 300, 200);
                    }
                }
            });
        }

        //Execute DQ Order/Lmt Order
        private void ExecuteCallback(Transaction tran, TransactionResult transactionResult)
        {
            App.MainFrameWindow.Dispatcher.BeginInvoke((Action)delegate()
            {
                if (transactionResult == null) return;
                if (transactionResult.TransactionError == TransactionError.OK)
                {
                    foreach (Order order in tran.Orders)
                    {
                        if (order.Status == OrderStatus.Deleting || order.Status == OrderStatus.Deleting)
                        {
                            order.ChangeStatus(OrderStatus.Deleted);
                        }

                        if (this.OnExecuteOrderNotifyEvent != null)
                        {
                        }
                    }
                    this.RemoveTransaction(tran);

                    this.TranPhaseManager.UpdateTransaction(tran);
                }
            });
        }
        #endregion

        private void RemoveTransaction(Transaction tran)
        {
            List<OrderTask> instanceOrders = new List<OrderTask>();
            List<OrderTask> lmtOrders = new List<OrderTask>();
            List<OrderTask> mooMocOrders = new List<OrderTask>();
            MooMocOrderForInstrument mooMocOrderForInstrument = null;

            foreach (Order order in tran.Orders)
            {
                if (order.Transaction.OrderType == OrderType.SpotTrade)
                {
                    foreach (OrderTask orderTask in this._App.ExchangeDataManager.ProcessInstantOrder.OrderTasks)
                    {
                        if (orderTask.OrderId == order.Id)
                        {
                            instanceOrders.Add(orderTask);
                        }
                    }
                }
                else if(order.Transaction.OrderType == OrderType.Limit || order.Transaction.OrderType == OrderType.Stop)
                {
                    foreach (OrderTask orderTask in this._App.ExchangeDataManager.ProcessLmtOrder.OrderTasks)
                    {
                        if (orderTask.OrderId == order.Id)
                        {
                            lmtOrders.Add(orderTask);
                        }
                    }
                }
                else if (order.Transaction.OrderType == OrderType.MarketOnOpen || order.Transaction.OrderType == OrderType.MarketOnClose)
                {
                    if (mooMocOrderForInstrument == null)
                    {
                        mooMocOrderForInstrument = this._App.ExchangeDataManager.MooMocOrderForInstrumentModel.MooMocOrderForInstruments.SingleOrDefault(P => P.Instrument.Id == order.Transaction.Instrument.Id);
                    }
                    if (mooMocOrderForInstrument != null)
                    {
                        OrderTask orderTask = mooMocOrderForInstrument.OrderTasks.SingleOrDefault(P => P.OrderId == order.Id);

                        if (orderTask == null) continue;
                        mooMocOrders.Add(orderTask);
                    }
                }
            }
            this._App.ExchangeDataManager.ProcessInstantOrder.RemoveInstanceOrder(instanceOrders);
            this._App.ExchangeDataManager.ProcessLmtOrder.RemoveLmtOrder(lmtOrders);
            if (mooMocOrderForInstrument == null) return;

            mooMocOrderForInstrument.RemoveMooMocOrder(mooMocOrders);

            if (mooMocOrderForInstrument.OrderTasks.Count == 0)
            {
                this._App.ExchangeDataManager.MooMocOrderForInstrumentModel.RemoveMooMocOrderForInstrument(mooMocOrderForInstrument);
            }
        }


        #region 辅助方法
        private void WriteLog(string data)
        {
            try
            {
                using (StreamWriter sw = File.AppendText(_DealingOrderPath))
                {
                    if (data != returnLine)
                    {
                        sw.Write(DateTime.Now);
                        sw.Write(":  ");
                        sw.Write(data);
                    }
                    else
                    {
                        sw.Write(data);
                    }
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception ex)
            {
                Manager.Common.Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "WriteLog.\r\n{0}", ex.ToString());
            }
        }
        #endregion

    }
}
