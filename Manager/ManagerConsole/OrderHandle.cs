﻿using ManagerConsole.Helper;
using ManagerConsole.Model;
using ManagerConsole.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using TransactionError = Manager.Common.TransactionError;
using CommonParameter = Manager.Common.SystemParameter;
using OrderType = Manager.Common.OrderType;
using TransactionType = Manager.Common.TransactionType;
using AccountInfor = Manager.Common.AccountInformation;
using System.Xml;
using System.Collections.ObjectModel;

namespace ManagerConsole
{
    public class OrderHandle : UserControl
    {
        private CommonDialogWin _CommonDialogWin;
        private ConfirmDialogWin _ConfirmDialogWin;
        private ConfirmOrderDialogWin ConfirmOrderDialogWin;
        private ManagerConsole.MainWindow _App;
        private Transaction _RejectTran;

        public OrderHandle()
        {
            this._App = ((ManagerConsole.MainWindow)Application.Current.MainWindow);
            this._CommonDialogWin = this._App.CommonDialogWin;
            this._ConfirmDialogWin = this._App.ConfirmDialogWin;
            this.ConfirmOrderDialogWin = this._App.ConfirmOrderDialogWin;
            this.ConfirmOrderDialogWin.OnConfirmDialogResult += new ConfirmOrderDialogWin.ConfirmDialogResultHandle(ProcessHandle);
            this.ConfirmOrderDialogWin.OnModifyPriceDialogResult += new ConfirmOrderDialogWin.ConfirmModifyPriceResultHandle(ModifyPriceHandle);
            this.ConfirmOrderDialogWin.OnRejectOrderDialogResult += new ConfirmOrderDialogWin.RejectOrderResultHandle(RejectOrderHandle);
        }

        #region Order Action

        public void OnOrderAccept(OrderTask orderTask)
        {
            SystemParameter systemParameter = this._App.InitDataManager.SettingsManager.SystemParameter;
            systemParameter.CanDealerViewAccountInfo = true;
            bool isOK = OrderTaskManager.CheckDQOrder(orderTask, systemParameter);
            isOK = true;

            if (isOK)
            {
                if (systemParameter.CanDealerViewAccountInfo)
                {
                    this.ShowConfirmOrderFrm(systemParameter.CanDealerViewAccountInfo, orderTask, HandleAction.OnOrderAccept);
                    return;
                }
                else
                {
                    if (MessageBox.Show("Accept the order?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        this.AcceptDQPlace(orderTask);
                    }
                }
            }
        }

        public void OnOrderReject(OrderTask order)
        {
            SystemParameter systemParameter = this._App.InitDataManager.SettingsManager.SystemParameter;
            systemParameter.ConfirmRejectDQOrder = true;//test Data from WebConfig

            if (systemParameter.ConfirmRejectDQOrder)
            {
                if (MessageBox.Show("Are you sure reject the order?", "Reject", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    if (order.OrderStatus == OrderStatus.WaitAcceptRejectPlace
                        || order.OrderStatus == OrderStatus.WaitAcceptRejectCancel
                        || order.OrderStatus == OrderStatus.WaitOutPriceDQ
                        || order.OrderStatus == OrderStatus.WaitOutLotDQ
                        || (order.OrderStatus == OrderStatus.WaitNextPrice && order.Transaction.OrderType == Manager.Common.OrderType.Limit))
                    {
                        foreach (Order orderEntity in order.Transaction.Orders)
                        {
                            orderEntity.Status = OrderStatus.Deleting;
                        }
                        this.RejectPlace(order.Transaction.Id);
                    }
                    else
                    {
                        string sMsg = "The order is canceled or executed already";
                        this._CommonDialogWin.ShowDialogWin(sMsg, "Alert");
                    }
                    this._App.InitDataManager.OrderTaskModel.RemoveOrderTask(order);
                }
            }
        }

        private void AcceptDQPlace(OrderTask order)
        {
            ConsoleClient.Instance.AcceptPlace(order.Transaction, AcceptPlaceCallback);

            this._App.InitDataManager.OrderTaskModel.RemoveOrderTask(order);
        }

        //接受限价单下单
        private void AcceptLmtPlace(OrderTask order)
        {
            ConsoleClient.Instance.AcceptPlace(order.Transaction, AcceptPlaceCallback);
        }

        private void RejectPlace(Guid transactionId)
        {
            ConsoleClient.Instance.CancelPlace(transactionId, Manager.Common.CancelReason.CustomerCanceled, RejectPlaceCallback);
        }
        
        public void OnOrderExecute(OrderTask orderTask)
        {
            SystemParameter systemParameter = this._App.InitDataManager.SettingsManager.SystemParameter;
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
                ConsoleClient.Instance.ResetHit(orderIds);
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

        public void CancelTransaction(OrderTask orderTask)
        {
            if (orderTask.OrderStatus == OrderStatus.WaitAcceptRejectPlace
                || orderTask.OrderStatus == OrderStatus.WaitAcceptRejectCancel

                || orderTask.OrderStatus == OrderStatus.WaitOutPriceDQ
                || orderTask.OrderStatus == OrderStatus.WaitOutLotDQ
                || (orderTask.OrderStatus == OrderStatus.WaitNextPrice && orderTask.Transaction.OrderType == OrderType.Limit))
            {
                foreach(Order order in orderTask.Transaction.Orders)
                {
                    order.ChangeStatus(OrderStatus.Deleting);
                }
                orderTask.ChangeStatus(OrderStatus.Deleting);

                this._RejectTran = orderTask.Transaction;

                ConsoleClient.Instance.CancelPlace(orderTask.Transaction.Id, Manager.Common.CancelReason.DealerCanceled, CancelTransactionCallback);
            }
            else
            {
                var sMsg = "The order is canceled or executed already";
                this._CommonDialogWin.ShowDialogWin(sMsg, "Alert", 300, 180);
            }
        }

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
        //public void OnLMTApply(LmtOrderTaskForInstrument lmtOrderForInstrument)
        //{
        //    string newAskPrice = lmtOrderForInstrument.AskPrice;
        //    string newBidPrice = lmtOrderForInstrument.BidPrice;
        //    foreach (OrderTask orderTask in lmtOrderForInstrument.OrderTasks)
        //    {
        //        if (orderTask.IsBuy == BuySell.Buy)
        //        {
        //            orderTask.SetPrice = newAskPrice;
        //        }
        //        else
        //        {
        //            orderTask.SetPrice = newBidPrice;
        //        }
        //    }
        //}

        public void OnLMTExecute(LmtOrderTaskForInstrument lmtOrderForInstrument)
        {
            foreach (OrderTask orderTask in lmtOrderForInstrument.OrderTasks)
            {
                if (orderTask.IsSelected)
                {
                    bool? isValidPrice = OrderTaskManager.IsValidPrice(orderTask.Instrument, decimal.Parse(orderTask.SetPrice));
                    if (isValidPrice.HasValue)
                    {
                        if (isValidPrice.Value)
                        {
                            //合法
                            string executePrice = orderTask.IsBuy == BuySell.Buy ? lmtOrderForInstrument.Instrument.Ask : lmtOrderForInstrument.Instrument.Bid;
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
                Manager.Common.AccountInformation accountInfor = ConsoleClient.Instance.GetAcountInfo(Guid.Empty);
                this._App.ConfirmOrderDialogWin.ShowDialogWin(accountInfor, title, orderTask, action);
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

        private void Commit(OrderTask order, string executePrice, decimal lot)
        {
            order.ChangeStatus(OrderStatus.WaitServerResponse);

            string buyPrice = string.Empty;
            string sellPrice = string.Empty;
            executePrice = string.IsNullOrEmpty(executePrice) ? order.SetPrice:executePrice;

            if(order.IsBuy == BuySell.Buy)
            {
                buyPrice = executePrice;
            }
            else
            {
                sellPrice = executePrice;
            }

            switch(order.Transaction.Type)
            {
                case TransactionType.Single:
                    this.ChangeOrderStatus(order, OrderStatus.WaitServerResponse);
                    break;
                case TransactionType.Pair:
                    this.ChangeOrderStatus(order, OrderStatus.WaitServerResponse);
                    break;
                case TransactionType.OneCancelOther:
                    this.ChangeOrderStatus(order, OrderStatus.Deleting);
                    break;
            }
            //if (!order.id) alert("The order is not valid!");
            XmlNode xmlNode = null;
            ConsoleClient.Instance.Execute(order.Transaction.Id, buyPrice, sellPrice, (decimal)order.Lot, order.OrderId, out xmlNode, ExecuteCallback);
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

        void ProcessHandle(bool yesOrNo,OrderTask orderTask,HandleAction action)
        {
            if (yesOrNo)
            {
                switch(action)
                {
                    case HandleAction.None:
                        break;
                    case HandleAction.OnOrderAccept:
                        this.AcceptDQPlace(orderTask);
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
                        var s = "";
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
            this.Dispatcher.BeginInvoke((Action<TransactionError>)delegate(TransactionError result)
            {
                if (result == TransactionError.OK)
                {
                    tran.Phase = Manager.Common.Phase.Placed;
                    TranPhaseManager.SetOrderStatus(tran,true);
                }
            }, transactionError);
        }

        private void RejectPlaceCallback(TransactionError transactionError)
        {
            this.Dispatcher.BeginInvoke((Action<TransactionError>)delegate(TransactionError result)
            {
                if (result == TransactionError.OK)
                {
                    this._CommonDialogWin.ShowDialogWin("Reject Place Succeed", "Infromation");
                }
            }, transactionError);
        }

        private void CancelTransactionCallback(TransactionError transactionError)
        {
            this.Dispatcher.BeginInvoke((Action<TransactionError>)delegate(TransactionError result)
            {
                if (result == TransactionError.OK)
                {
                    foreach (Order order in this._RejectTran.Orders)
                    {
                        order.ChangeStatus(OrderStatus.Canceled);

                        if (order.Transaction.OrderType == OrderType.Limit)
                        {
                            OrderTask orderTask = this._App.InitDataManager.OrderTaskModel.OrderTasks.SingleOrDefault(P => P.OrderId == order.Id);
                            this._App.InitDataManager.OrderTaskModel.RemoveOrderTask(orderTask);
                        }
                    }
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
            }, transactionError);
        }

        private void ExecuteCallback(TransactionError transactionError)
        {
            if (transactionError == TransactionError.OK)
            {
                var ss = "";
            }
        }
        #endregion
    }
}
