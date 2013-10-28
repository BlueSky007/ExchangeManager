using ManagerConsole.Helper;
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

        public OrderHandle()
        {
            this._App = ((ManagerConsole.MainWindow)Application.Current.MainWindow);
            this._CommonDialogWin = this._App.CommonDialogWin;
            this._ConfirmDialogWin = this._App.ConfirmDialogWin;
            this.ConfirmOrderDialogWin = this._App.ConfirmOrderDialogWin;
            this.ConfirmOrderDialogWin.OnConfirmDialogResult += new ConfirmOrderDialogWin.ConfirmDialogResultHandle(ProcessHandle);
            this.ConfirmOrderDialogWin.OnModifyPriceDialogResult += new ConfirmOrderDialogWin.ConfirmModifyPriceResultHandle(ModifyPriceHandle);
        }

        #region Order Action

        public void OnOrderAccept(OrderTask order)
        {
            SystemParameter systemParameter = this._App.InitDataManager.SettingsManager.SystemParameter;
            systemParameter.CanDealerViewAccountInfo = true;
            bool isOK = OrderTaskManager.CheckDQOrder(order, systemParameter);
            isOK = true;

            if (isOK)
            {
                if (systemParameter.CanDealerViewAccountInfo)
                {
                    //just test data
                    AccountInfor accountInfor = ConsoleClient.Instance.GetAcountInfo(Guid.Empty);
                    this.ConfirmOrderDialogWin.ShowDialogWin(accountInfor, "Confrim", order, HandleAction.OnOrderAccept);
                    return;
                }
                else
                {
                    if (MessageBox.Show("Accept the order?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        this.AcceptDQPlace(order);
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
                    ObservableCollection<DQOrderTaskForInstrument> dQOrderTaskForInstruments = this._App.InitDataManager.DQOrderTaskForInstrumentModel.DQOrderTaskForInstruments;
                    DQOrderTaskForInstrument dQOrderTaskForInstrument = dQOrderTaskForInstruments.SingleOrDefault(P => P.Instrument.Id == order.Instrument.Id);
                    dQOrderTaskForInstrument.RemoveDQOrderTask(order);
                }
            }
        }

        private void AcceptDQPlace(OrderTask order)
        {
            ConsoleClient.Instance.AcceptPlace(order.Transaction.Id, AcceptPlaceCallback);

            ObservableCollection<DQOrderTaskForInstrument> dQOrderTaskForInstruments = this._App.InitDataManager.DQOrderTaskForInstrumentModel.DQOrderTaskForInstruments;
            DQOrderTaskForInstrument dQOrderTaskForInstrument = dQOrderTaskForInstruments.SingleOrDefault(P => P.Instrument.Id == order.Instrument.Id);
            dQOrderTaskForInstrument.RemoveDQOrderTask(order);
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
                    order.DoUpdate();
                }
            }
        }
        #endregion

        #region 辅助方法
        private void ShowConfirmOrderFrm(bool canDealerViewAccountInfo, OrderTask orderTask,HandleAction action)
        {
            if (canDealerViewAccountInfo)
            {
                //just test data
                Manager.Common.AccountInformation accountInfor = ConsoleClient.Instance.GetAcountInfo(Guid.Empty);
                this._App.ConfirmOrderDialogWin.ShowDialogWin(accountInfor, "Execute", orderTask, action);
            }
            else
            {
                if (MessageBox.Show("Execute the order?", "Execute", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
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
	    
	    //order.mainWindow.oIOProxy.ExecuteTransaction(this, order, buyPrice, sellPrice, lot);

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
        #endregion

        #region 批单回调事件
        //Call Back Event
        private void AcceptPlaceCallback(TransactionError transactionError)
        {
            this.Dispatcher.BeginInvoke((Action<TransactionError>)delegate(TransactionError result)
            {
                if (result == TransactionError.OK)
                {
                    this._CommonDialogWin.ShowDialogWin("Accept Place Succeed", "Infromation");
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
