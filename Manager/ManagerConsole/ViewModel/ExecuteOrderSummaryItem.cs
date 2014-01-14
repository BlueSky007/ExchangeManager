using ManagerConsole.Helper;
using ManagerConsole.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Price = iExchange.Common.Price;

namespace ManagerConsole.ViewModel
{
    public class ExecuteOrderSummaryItemModel
    {
        public ObservableCollection<ExecuteOrderSummaryItem> ExecuteOrderSummaryItems { get; set; }
        public ExecuteOrderSummaryItemModel()
        {
            this.ExecuteOrderSummaryItems = new ObservableCollection<ExecuteOrderSummaryItem>();
        }

        public void InitializeExecuteOrderSummaryItems(ObservableCollection<Order> executedOrders, RangeType rangeType, int interval)
        {
            this.ExecuteOrderSummaryItems.Clear();
            IEnumerable<IGrouping<InstrumentClient, Order>> query = executedOrders.GroupBy(P => P.Transaction.Instrument, P => P);
            foreach (IGrouping<InstrumentClient, Order> group in query)
            {
                List<Order> instrumentOfOrders = group.ToList<Order>();
                Order order = group.First();

                ExecuteOrderSummaryItem summaryItem = this.AddInstrumentSummaryItem(order.Transaction.Instrument);
                string rangeValue = string.Empty;

                if (rangeType == RangeType.Time)
                {
                    rangeValue = order.Transaction.ExecuteTime.Value.ToShortTimeString();
                    instrumentOfOrders.OrderBy(P => P.Transaction.ExecuteTime);
                }
                else
                {
                    rangeValue = order.ExecutePrice;
                    instrumentOfOrders.OrderBy(P => P.ExecutePrice);
                }
                foreach (Order orderEntity in instrumentOfOrders)
                {
                    ExecuteOrderSummaryItem rangeSummaryItem = this.AddRangeSummaryItem(summaryItem, orderEntity, rangeType, interval, rangeValue);
                    summaryItem.Update(rangeSummaryItem, true);
                    summaryItem.SetAvgPrice();
                }
            }
        }

        //Executed Order Notify
        public void AddExecutedOrderToGrid(Order order,RangeType rangeType, int interval)
        {
            ExecuteOrderSummaryItem instrumentSummaryItem = this.AddInstrumentSummaryItem(order.Transaction.Instrument);
            string rangeValue = string.Empty;

            if (rangeType == RangeType.Time)
            {
                rangeValue = order.Transaction.ExecuteTime.Value.ToShortTimeString();
            }
            else
            {
                rangeValue = order.ExecutePrice;
            }
            ExecuteOrderSummaryItem rangeSummaryItem = this.AddRangeSummaryItem(instrumentSummaryItem, order, rangeType, interval, rangeValue);
            this.AddOrderSummaryItem(rangeSummaryItem, order);
            instrumentSummaryItem.Update(rangeSummaryItem, true);
            instrumentSummaryItem.SetAvgPrice();
        }
        
        //Deleted Order Notify
        public void DeletedExecutedOrderFromGrid(Order deletedOrder)
        {
            InstrumentClient instrument = deletedOrder.Transaction.Instrument;
            ExecuteOrderSummaryItem instrumentSummaryItem = this.ExecuteOrderSummaryItems.SingleOrDefault(P => P.Instrument.Id == instrument.Id);
            if (instrumentSummaryItem != null)
            {
                foreach (ExecuteOrderSummaryItem rangeSummaryItem in instrumentSummaryItem.ChildSummaryItems)
                {
                    ExecuteOrderSummaryItem orderSummaryItem = rangeSummaryItem.ChildSummaryItems.SingleOrDefault(P => P.Id == deletedOrder.Id.ToString());
                    if (orderSummaryItem != null)
                    {
                        rangeSummaryItem.ChildSummaryItems.Remove(orderSummaryItem);
                        instrumentSummaryItem.Update(rangeSummaryItem, true);
                        instrumentSummaryItem.SetAvgPrice();
                        return;
                    }
                }
            }
        }

        private ExecuteOrderSummaryItem AddInstrumentSummaryItem(InstrumentClient instrument)
        {
            ExecuteOrderSummaryItem summaryItem = this.ExecuteOrderSummaryItems.SingleOrDefault(P => P.Instrument == instrument);
            if (summaryItem == null)
            {
                summaryItem = new ExecuteOrderSummaryItem(instrument);
                this.ExecuteOrderSummaryItems.Add(summaryItem);
            }
            return summaryItem;
        }

        private ExecuteOrderSummaryItem AddRangeSummaryItem(ExecuteOrderSummaryItem instrumentSummaryItem, Order currentOrder, RangeType rangeType, int interval, string rangeValue)
        {
            ExecuteOrderSummaryItem rangeSummaryItem = null;
            InstrumentClient instrument = currentOrder.Transaction.Instrument;
            OrderRange orderRange;
            foreach (ExecuteOrderSummaryItem item in instrumentSummaryItem.ChildSummaryItems)
            {
                if (rangeType == item.OrderRange.RangeType && item.OrderRange.IsRange(interval, rangeValue))
                {
                    rangeSummaryItem = item;
                    break;
                }
            }
            if(rangeSummaryItem == null)
            {
                if(rangeType == RangeType.Time)
                {
                    orderRange = this.GetTimeRange(currentOrder.Transaction.ExecuteTime.Value,interval);
                }
                else
                {
                    orderRange = this.GetPriceRange(currentOrder.ExecutePrice,interval,instrument);
                }
                rangeSummaryItem = new ExecuteOrderSummaryItem(instrument, orderRange);
                this.AddOrderSummaryItem(rangeSummaryItem, currentOrder);

                instrumentSummaryItem.ChildSummaryItems.Add(rangeSummaryItem);
                //instrumentSummaryItem.Update(rangeSummaryItem, true);
                //instrumentSummaryItem.SetAvgPrice();
            }
            return rangeSummaryItem;
        }

        private void AddOrderSummaryItem(ExecuteOrderSummaryItem rangeSummaryItem, Order order)
        {
            ExecuteOrderSummaryItem orderSummaryItem = rangeSummaryItem.ChildSummaryItems.SingleOrDefault(P => P.Id == order.Id.ToString());
            if (orderSummaryItem == null)
            {
                orderSummaryItem = new ExecuteOrderSummaryItem(order);
                rangeSummaryItem.ChildSummaryItems.Add(orderSummaryItem);
                rangeSummaryItem.Update(orderSummaryItem, true);
                rangeSummaryItem.SetAvgPrice();
            }
        }

        private void RecalculateSummaryByOrders(ExecuteOrderSummaryItem rangeSummaryItem)
        {
            ExecuteOrderSummaryItem instrumentSummaryItem = this.ExecuteOrderSummaryItems.SingleOrDefault(P => P.Instrument.Id == rangeSummaryItem.Instrument.Id);

             instrumentSummaryItem.Update(rangeSummaryItem, false);

            foreach (ExecuteOrderSummaryItem orderSummaryItem in rangeSummaryItem.ChildSummaryItems)
            {
                if (rangeSummaryItem.MinNumeratorUnit > orderSummaryItem.MinNumeratorUnit)
                {
                    rangeSummaryItem.MinNumeratorUnit = orderSummaryItem.MinNumeratorUnit;
                }
                if (rangeSummaryItem.MaxDenominator < orderSummaryItem.MaxDenominator)
                {
                    rangeSummaryItem.MaxDenominator = orderSummaryItem.MaxDenominator;
                }
                rangeSummaryItem.Update(rangeSummaryItem, true);
            }
        }

        private OrderRange GetTimeRange(DateTime executeTime, int interval)
        {
            string beginTime = executeTime.ToShortTimeString();
            string EndTime = executeTime.AddMinutes(interval).ToShortTimeString();

            OrderRange orderRange = new OrderRange(RangeType.Time, interval, beginTime, EndTime);
            return orderRange;
        }

        private OrderRange GetPriceRange(string executePrice, int interval, InstrumentClient instrument)
        {
            Price rangeValue = new Price(executePrice, (int)instrument.NumeratorUnit.Value, (int)instrument.Denominator.Value);
            Price beginPrice = rangeValue;
            Price EndPrice = rangeValue + interval;

            OrderRange orderRange = new OrderRange(RangeType.Price, interval, beginPrice.ToString(), EndPrice.ToString());
            return orderRange;
        }
    }

    public class ExecuteOrderSummaryItem : PropertyChangedNotifier
    {
        public ExecuteOrderSummaryItem(InstrumentClient instrument)
        {
            this._ChildSummaryItems = new ObservableCollection<ExecuteOrderSummaryItem>();
            this._Id = instrument.Id.ToString();
            this._ExecuteOrderSummaryType = ExecuteOrderSummaryType.Instrument;
            this._Instrument = instrument;
        }

        public ExecuteOrderSummaryItem(InstrumentClient instrument, OrderRange orderRange)
        {
            this._ChildSummaryItems = new ObservableCollection<ExecuteOrderSummaryItem>();
            this._Code = orderRange.BeginRange + "~" + orderRange.EndRange;
            this._Id = instrument.Id + "_" + this._Code;
            this._ExecuteOrderSummaryType = ExecuteOrderSummaryType.Range;
            this._MinNumeratorUnit = instrument.NumeratorUnit.Value;
            this._MaxDenominator = instrument.Denominator.Value;
            this._Instrument = instrument;
            this._OrderRange = orderRange;
        }

        public ExecuteOrderSummaryItem(Order order)
        {
            this.Update(order);
        }

        #region private property
        private string _Id;
        private string _Code;
        private InstrumentClient _Instrument;
        private ExecuteOrderSummaryType _ExecuteOrderSummaryType;
        private OrderRange _OrderRange;
        private int _MinNumeratorUnit = 1;
        private int _MaxDenominator = 1;
        private int _OrderCount;
        private decimal _BuyLot;
        private decimal _SellLot;
        private string _BuyAvgPrice = "0";
        private string _SellAvgPrice = "0";
        private decimal _BuyLotMultiplyAvgPriceSum = 0;
        private decimal _SellLotMultiplyAvgPriceSum = 0;
        private ObservableCollection<ExecuteOrderSummaryItem> _ChildSummaryItems;
        private ObservableCollection<Order> _InstrumentOrders;
        
        #endregion

        #region public property

        public ObservableCollection<Order> InstrumentOrders
        {
            get { return this._InstrumentOrders; }
            set { this._InstrumentOrders = value; }
        }

        public ObservableCollection<ExecuteOrderSummaryItem> ChildSummaryItems
        {
            get { return this._ChildSummaryItems; }
            set { this._ChildSummaryItems = value; }
        }

        public InstrumentClient Instrument
        {
            get { return this._Instrument; }
            set { this._Instrument = value; this.OnPropertyChanged("Instrument"); }
        }

        public OrderRange OrderRange
        {
            get { return this._OrderRange; }
            set { this._OrderRange = value; this.OnPropertyChanged("OrderRange"); }
        }

        public string Id
        {
            get { return this._Id; }
            set { this._Id = value; }
        }

        public string Code
        {
            get { return this._Code; }
            set 
            {
                this._Code = value;
            }
        }

        public string InstrumentCode
        {
            get { return this._Instrument.Code; }
        }

        public string AccountCode
        {
            get;
            set;
        }

        public ExecuteOrderSummaryType ExecuteOrderSummaryType
        {
            get { return this._ExecuteOrderSummaryType; }
            set { this._ExecuteOrderSummaryType = value; }
        }

        public int OrderCount
        {
            get { return this._OrderCount; }
            set 
            { 
                this._OrderCount = value; 
                this.OnPropertyChanged("OrderCount"); 
            }
        }

        public decimal BuyLot
        {
            get { return this._BuyLot; }
            set 
            { 
                this._BuyLot = value; 
                this.OnPropertyChanged("BuyLot"); 
            }
        }

        public decimal SellLot
        {
            get { return this._SellLot; }
            set 
            { 
                this._SellLot = value; 
                this.OnPropertyChanged("SellLot"); 
            }
        }

        public string BuyAvgPrice
        {
            get { return this._BuyAvgPrice; }
            set { this._BuyAvgPrice = value; this.OnPropertyChanged("BuyAvgPrice"); }
        }

        public string SellAvgPrice
        {
            get { return this._SellAvgPrice; }
            set { this._SellAvgPrice = value; this.OnPropertyChanged("SellAvgPrice"); }
        }

        public decimal BuyLotMultiplyAvgPriceSum
        {
            get { return this._BuyLotMultiplyAvgPriceSum; }
            set { this._BuyLotMultiplyAvgPriceSum = value; }
        }

        public decimal SellLotMultiplyAvgPriceSum
        {
            get { return this._SellLotMultiplyAvgPriceSum; }
            set { this._SellLotMultiplyAvgPriceSum = value; }
        }

        public int MinNumeratorUnit
        {
            get { return this._MinNumeratorUnit; }
            set { this._MinNumeratorUnit = value; }
        }

        public int MaxDenominator
        {
            get { return this._MaxDenominator; }
            set { this._MaxDenominator = value; }
        }
        #endregion

        internal void Update(ExecuteOrderSummaryItem childItem, bool increase)
        {
            if (increase)
            {
                this._OrderCount++;
                this._BuyLot += childItem._BuyLot;
                this._SellLot += childItem._SellLot;
                this._BuyLotMultiplyAvgPriceSum += childItem._BuyLot * decimal.Parse(childItem._BuyAvgPrice);
                this._SellLotMultiplyAvgPriceSum += childItem._SellLot * decimal.Parse(childItem._SellAvgPrice);
            }
            else
            {
                this._OrderCount--;
                this._BuyLot -= childItem._BuyLot;
                this._SellLot -= childItem._SellLot;
            }
        }

        internal void Update(Order order)
        {
            InstrumentClient instrument = order.Transaction.Instrument;
            this._ExecuteOrderSummaryType = ExecuteOrderSummaryType.Order;
            this._MinNumeratorUnit = instrument.NumeratorUnit.Value;
            this._MaxDenominator = instrument.Denominator.Value;
            this._Instrument = instrument;

            bool isBuy = order.BuySell == BuySell.Buy;
            decimal buyLot = isBuy ? order.Lot:0;
            decimal sellLot = !isBuy ? order.Lot:0;

            
            this._Id = order.Id.ToString();
            this._Code = order.Transaction.ExecuteTime.Value.ToString();
            this._BuyLot = buyLot;
            this._SellLot = sellLot;
            this._BuyAvgPrice = isBuy ? order.ExecutePrice : "0";
            this._SellAvgPrice = !isBuy ? order.ExecutePrice : "0";
            this.AccountCode = order.Transaction.Account.Code;
        }

        internal void SetAvgPrice()
        {
            Price price = null;
            decimal avgBuyPriceValue = this._BuyLot != 0 ? this._BuyLotMultiplyAvgPriceSum / this._BuyLot : 0;
            price = new Price(avgBuyPriceValue.ToString(), this._MinNumeratorUnit, this._MaxDenominator);
            this._BuyAvgPrice = price == null ? "0" : price.ToString();

            decimal avgSellPriceValue = this._SellLot != 0 ? this._SellLotMultiplyAvgPriceSum / this._SellLot : 0;
            price = new Price(avgSellPriceValue.ToString(), this._MinNumeratorUnit, this._MaxDenominator);
            this._SellAvgPrice = price == null ? "0" : price.ToString();
        }

        
    }

    public class OrderRange
    {
        public OrderRange(RangeType type, int interval, string beginRange, string endRange)
        {
            this._RangeType = type;
            this._Interval = interval;
            this._BeginRange = beginRange;
            this._EndRange = endRange;
        }

        #region private property
        private RangeType _RangeType;
        private int _Interval;
        private string _BeginRange;
        private string _EndRange;
        #endregion

        public RangeType RangeType
        {
            get { return this._RangeType; }
            set { this._RangeType = value; }
        }
        public int Interval
        {
            get { return this._Interval; }
            set { this._Interval = value; }
        }
        public string BeginRange
        {
            get { return this._BeginRange; }
            set { this._BeginRange = value; }
        }
        public string EndRange
        {
            get { return this._EndRange; }
            set { this._EndRange = value; }
        }

        public bool IsRange(int interval, string rangeValue)
        {
            if (this.RangeType == ManagerConsole.RangeType.Time)
            {
                return DateTime.Parse(rangeValue) >= Convert.ToDateTime(this.BeginRange);
            }
            else
            {
                return double.Parse(rangeValue) > double.Parse(this.BeginRange) && double.Parse(rangeValue) < double.Parse(this.EndRange);
            }
        }


    }
}
