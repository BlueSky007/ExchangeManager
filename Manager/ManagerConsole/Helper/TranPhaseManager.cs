using ManagerConsole.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phase = iExchange.Common.OrderPhase;
using OrderType = iExchange.Common.OrderType;
using System.Collections.ObjectModel;
using ManagerConsole.ViewModel;
using System.Windows.Threading;

namespace ManagerConsole.Helper
{
    public class TranPhaseManager
    {
        private ExchangeDataManager _ExchangeDataManager;
        public TranPhaseManager(ExchangeDataManager initDataManager)
        {
            this._ExchangeDataManager = initDataManager;
        }

        public ExchangeDataManager InitDataManager
        {
            get { return this._ExchangeDataManager; }
            set { this._ExchangeDataManager = value; }
        }

        public void UpdateTransaction(Transaction transaction)
        {
            foreach (Order order in transaction.Orders)
            {
                switch (transaction.Phase)
                {
                    case Phase.Placing:
                        order.Status = OrderStatus.WaitAcceptRejectPlace;
                        break;
                    case Phase.Placed:
                        CheckWhenOrderArrive(true, order);
                        if (order.Status == OrderStatus.WaitServerResponse) return;
                        CheckWhenOrderArrive(true, order);
                        break;
                    case Phase.Executed:
                        order.Status = OrderStatus.Executed;
                        this.AddExecutedOrder(order);
                        this.AddOrderToGroupNetPosition(order);
                        break;
                    case Phase.Canceled:
                        order.Status = OrderStatus.Canceled;
                        break;
                    case Phase.Deleted:
                        order.Status = OrderStatus.Deleted;
                        break;
                    case Phase.Completed:
                        break;
                }
            }
        }

        private static void CheckWhenOrderArrive(bool needPlaySound,Order order)
        {
            if (order.Transaction.OrderType == iExchange.Common.OrderType.SpotTrade)
            {
               
                if (IsPriceExceedMaxMin(order.SetPrice) == true)
                {
                    //Waiting for Dealer Accept/Reject
                    order.Status = OrderStatus.WaitOutPriceDQ;
                }
                else
                {
                    //Waiting for Dealer Confirm/Reject
                    order.Status = OrderStatus.WaitOutLotDQ;
                }
            }
            else if (order.Transaction.OrderType == iExchange.Common.OrderType.Limit)
            {
                order.Status = OrderStatus.WaitNextPrice;
            }
        }

        private static bool IsPriceExceedMaxMin(string bestPrice)
        {
            return true;
        }

        #region Binding Data Update
        public void AddExecutedOrder(Order order)
        {
            this._ExchangeDataManager.ExecutedOrders.Add(order);
            this._ExchangeDataManager.AddExecutedOrderSummaryItem(order);
        }

        public void AddOrderToGroupNetPosition(Order executedOrder)
        {
            GroupNetPositionModel groupNetPostionModel = this._ExchangeDataManager.ReportDataManager.GetGroupNetPositionModel(executedOrder.ExchangeCode);
            groupNetPostionModel.AddOrderToGroupNetPosition(executedOrder);
        }

        public void DeletedExecutedOrderSummaryItem(Order order)
        {
            this._ExchangeDataManager.ExecuteOrderSummaryItemModel.DeletedExecutedOrderFromGrid(order);
        }

        #endregion

        #region Lot Changed
        public void DeleteOrderNotifyUpdateLot(Guid instrumentId, Order deletedOrder)
        {
            //InstrumentClient instrument = this._ExchangeDataManager.Instruments.SingleOrDefault(P => P.Id == instrumentId);
            //Customer customer = new Customer();
            //QuotePolicyDetail quotePolicyDetail = this._ExchangeDataManager.ExchangeSettingManagers.GetQuotePolicyDetail(instrumentId, customer);
            //decimal lotBalance = deletedOrder.LotBalance;
            //bool isBuy = (deletedOrder.BuySell == BuySell.Buy);
            //this.SubtractBuySellLot(instrument, quotePolicyDetail, isBuy, lotBalance);
        }
        private void SubtractBuySellLot(InstrumentClient instrument, QuotePolicyDetail quotePolicyDetail, bool isBuy, decimal lotBalance)
        {
            instrument.BuyLot -= isBuy ? lotBalance : 0;
            instrument.SellLot -= !isBuy ? lotBalance : 0;

            if (quotePolicyDetail != null)
            {
                quotePolicyDetail.BuyLot -= isBuy ? lotBalance : 0;
                quotePolicyDetail.SellLot -= !isBuy ? lotBalance : 0;
            }
        }
        private void AddBuySellLot(InstrumentClient instrument, QuotePolicyDetail quotePolicyDetail, bool isBuy, decimal lotBalance)
        {
            instrument.BuyLot += isBuy ? lotBalance : 0;
            instrument.SellLot += !isBuy ? lotBalance : 0;

            if (quotePolicyDetail != null)
            {
                quotePolicyDetail.BuyLot -= isBuy ? lotBalance : 0;
                quotePolicyDetail.SellLot -= !isBuy ? lotBalance : 0;
            }
        }

        public void AddOrderProcessBuySellLot(Guid instrumentId,Order deletedOrder)
        {
            //if (_RepairNewOrderMultiNotifyOrders.Exists(order.id)) ??
            //{
            //    return;
            //}
            //else
            //{
            //    _RepairNewOrderMultiNotifyOrders.Add(order.id, order.id);
            //}
            Phase phase = deletedOrder.Phase;
            decimal lotBalance = deletedOrder.LotBalance;
            bool isOpen = deletedOrder.OpenClose == OpenClose.Open;
            bool isBuy = deletedOrder.BuySell == BuySell.Buy;
            InstrumentClient instrument = this._ExchangeDataManager.GetExchangeSetting(deletedOrder.ExchangeCode).Instruments.Values.SingleOrDefault(P => P.Id == instrumentId);
            Customer customer = new Customer();
            QuotePolicyDetail quotePolicyDetail = this._ExchangeDataManager.ExchangeSettingManagers["WF01"].GetQuotePolicyDetail(instrumentId, customer);
            ObservableCollection<CloseOrder> closeOrders = deletedOrder.CloseOrders;

            if (phase == Phase.Executed || phase == Phase.Completed)
            {
                instrument.LastLot = deletedOrder.Lot;
                instrument.LastSales = (isBuy ? "B" : "S") + deletedOrder.ExecutePrice.ToString();
            }

            if (isOpen)
            {
                this.AddBuySellLot(instrument, quotePolicyDetail, isBuy, lotBalance);
            }
            else
            {
                foreach (CloseOrder closeOrder in closeOrders)
                {
                    bool closeOrderIsBuy = !isBuy;
                    this.SubtractBuySellLot(instrument, quotePolicyDetail, closeOrderIsBuy, lotBalance);
                }
            }
        }
        #endregion


    }

}
