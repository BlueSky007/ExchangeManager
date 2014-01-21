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

namespace ManagerConsole.Model
{
    public class OrderHandle
    {
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
            this._TranPhaseManager = this._App.InitDataManager.TranPhaseManager;
        }

        public TranPhaseManager TranPhaseManager
        {
            get { return this._TranPhaseManager; }
            set { this._TranPhaseManager = value; }
        }

        #region Order Action

        public void OnOrderAccept(OrderTask orderTask)
        {
            SystemParameter systemParameter = this._App.InitDataManager.GetExchangeSetting(orderTask.ExchangeCode).SystemParameter;
            ConfigParameter configParameter = this._App.InitDataManager.ConfigParameter;
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

        public void OnOrderReject(OrderTask order)
        {
            ConfigParameter parameter = this._App.InitDataManager.ConfigParameter;
            if (order.OrderStatus == OrderStatus.WaitOutPriceDQ || order.OrderStatus == OrderStatus.WaitOutLotDQ)
            {
                if (parameter.ConfirmRejectDQOrder)
                {
                    if (MessageBox.Show("Are you sure reject the order?", "Reject", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        if (order.OrderStatus == OrderStatus.WaitAcceptRejectPlace
                            || order.OrderStatus == OrderStatus.WaitAcceptRejectCancel
                            || order.OrderStatus == OrderStatus.WaitOutPriceDQ
                            || order.OrderStatus == OrderStatus.WaitOutLotDQ
                            || (order.OrderStatus == OrderStatus.WaitNextPrice && order.Transaction.OrderType == OrderType.Limit))
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
                }
            }
        }


        //接受限价单下单
        private void AcceptLmtPlace(OrderTask orderTask)
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
            ExchangeSettingManager settingManager = this._App.InitDataManager.GetExchangeSetting(orderTask.ExchangeCode);
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
            if (orderTask.OrderStatus == OrderStatus.WaitOutPriceLMT 
                || orderTask.OrderStatus == OrderStatus.WaitOutLotLMT
                || orderTask.OrderStatus == OrderStatus.WaitOutLotLMTOrigin)
            {
                orderTask.ChangeStatus(OrderStatus.WaitNextPrice);
                orderTask.ResetHit();
                Guid[] orderIds = new Guid[] { orderTask.OrderId };
                ConsoleClient.Instance.ResetHit(orderTask.ExchangeCode,orderIds);

                if (this.OnOrderWaitNofityEvent != null)
                {
                    this.OnOrderWaitNofityEvent(orderTask);
                }
            }
        }

        public void OnOrderModify(OrderTask orderTask)
        {
            if (orderTask.OrderStatus == OrderStatus.WaitOutLotLMT)
            {
                this.ConfirmOrderDialogWin.ShowDialogWin("Modify Price", orderTask, HandleAction.OnOrderModify);
            }
        }

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

       
        //Accept or Reject Lmt Order
        public void OnOrderAcceptPlace(OrderTask orderTask)
        {
            if (orderTask.OrderStatus == OrderStatus.WaitAcceptRejectPlace)
            {
                foreach (Order order in orderTask.Transaction.Orders)
                {
                    order.ChangeStatus(OrderStatus.WaitServerResponse);
                }
            }
            this.AcceptLmtPlace(orderTask);
        }

        public void OnOrderRejectPlace(OrderTask orderTask)
        {
            if (orderTask.OrderStatus == OrderStatus.WaitAcceptRejectPlace)
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

        public void OnLMTExecute(LMTProcessForInstrument lMTProcessForInstrument)
        {
            foreach (OrderTask orderTask in lMTProcessForInstrument.OrderTasks)
            {
                if (orderTask.IsSelected)
                {
                    bool? isValidPrice = OrderTaskManager.IsValidPrice(orderTask.Instrument, decimal.Parse(orderTask.SetPrice));
                    if (isValidPrice.HasValue)
                    {
                        if (isValidPrice.Value)
                        {
                            //合法
                            string executePrice = orderTask.IsBuy == BuySell.Buy ? lMTProcessForInstrument.Instrument.Ask : lMTProcessForInstrument.Instrument.Bid;
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
        }
        #endregion

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
                AccountInformation accountInfor = ConsoleClient.Instance.GetAcountInfo(orderTask.ExchangeCode,Guid.Empty);
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
        void ModifyPriceHandle(bool yesOrNo, string newPrice, OrderTask orderTask, HandleAction action)
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
                    isValidPrice = OrderTaskManager.IsValidPrice(instrument,decimal.Parse(newPrice));

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
                        string sMsg = "";// this._Message.GetMessageForOrder("DealerCanceled");
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

            foreach (Order order in tran.Orders)
            {
                order.ChangeStatus(OrderStatus.Canceled);

                foreach (OrderTask orderTask in this._App.InitDataManager.ProcessInstantOrder.OrderTasks)
                {
                    if (orderTask.OrderId == order.Id)
                    {
                        instanceOrders.Add(orderTask);
                        continue;
                    }
                }
            }

            if (instanceOrders.Count <= 0) return;
            this._App.InitDataManager.ProcessInstantOrder.RemoveInstanceOrder(instanceOrders);
        }
    }
}
