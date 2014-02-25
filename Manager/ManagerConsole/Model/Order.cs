using ManagerConsole.Helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Media;
using CommonOrder = iExchange.Common.Manager.Order;
using OrderType = iExchange.Common.OrderType;
using iExchange.Common;
using Phase = iExchange.Common.OrderPhase;

namespace ManagerConsole.Model
{
    public class OrderPhaseChangedEventArgs : EventArgs
    {
        public Phase Phase
        {
            get;
            private set;
        }

        public OrderPhaseChangedEventArgs(Phase phase)
        {
            this.Phase = phase;
        }
    }
    public delegate void OrderPhaseChangedEventHandler(Order order, OrderPhaseChangedEventArgs e);

    public class Order : PropertyChangedNotifier
    {
        public delegate void OrderStatusChangedHandler(OrderStatus orderStatus);
        public event OrderStatusChangedHandler OnOrderStatusChangedHandler;

        public event OrderPhaseChangedEventHandler OnOrderPhaseChanged;
        public static string GetOrderTypeString(OrderType orderType, TradeOption tradeOption, TransactionType transactionType, TransactionSubType transactionSubType)
        {
            //special process
            if (orderType == OrderType.Limit)
            {
                if (transactionType == TransactionType.OneCancelOther)
                {
                    if (tradeOption == TradeOption.Better)
                    {
                        return "LimitOfOCO";
                    }
                    else if (tradeOption == TradeOption.Stop)
                    {
                        return "StopOfOCO";
                    }
                    else
                    {
                        return "OCO";
                    }
                }
                else
                {
                    if (tradeOption == TradeOption.Better)
                    {
                        return "LMT";
                    }
                    else
                    {
                        return "STP";
                    }
                }
            }
            else if (orderType == OrderType.OneCancelOther)
            {
                if (tradeOption == TradeOption.Better)
                {
                    return "LimitOfOCO";
                }
                else if (tradeOption == TradeOption.Stop)
                {
                    return "StopOfOCO";
                }
                else
                {
                    return "OCO";
                }
            }
            else
            {
                return orderType.GetCaption();
            }
        }

        public Order(Transaction transaction,CommonOrder commonOrder)
        {
            this.Transaction = transaction;
            this.Phase = transaction.Phase;
            this.Id = commonOrder.Id;
            
            this.Update(commonOrder);
            this.Transaction.PropertyChanged += new PropertyChangedEventHandler(Transaction_PropertyChanged);
        }

        #region Property
        public Guid TransactionId
        {
            get;
            set;
        }

        public Transaction Transaction
        {
            get;
            private set;
        }

        public ObservableCollection<CloseOrder> CloseOrders
        {
            get;
            private set;
        }

        public Guid Id
        {
            get;
            private set;
        }

        private string _Code;
        public string Code
        {
            get
            {
                return this._Code;
            }
            set
            {
                string oldValue = this._Code;
                this._Code = value;
                if (this._Code != oldValue)
                {
                    this.OnPropertyChanged("Code");
                }
            }
        }

        public string ExchangeCode
        {
            get;
            set;
        }

        public Guid AccountId
        {
            get;
            set;
        }

        public Guid InstrumentId
        {
            get;
            set;
        }

        public TradeOption TradeOption
        {
            get;
            set;
        }

        public OpenClose OpenClose
        {
            get;
            set;
        }

        public BuySell BuySell
        {
            get;
            set;
        }

        public string BlotterCode
        {
            get;
            set;
        }

        private string _ExecutePrice;
        public string ExecutePrice
        {
            get { return this._ExecutePrice; }
            set
            {
                if (this._ExecutePrice != value)
                {
                    this._ExecutePrice = value;
                    this.OnPropertyChanged("ExecutePrice");
                }
            }
        }

        private string _SetPrice;
        public string SetPrice
        {
            get { return this._SetPrice; }
            set
            {
                if (this._SetPrice != value)
                {
                    this._SetPrice = value;
                    this.OnPropertyChanged("SetPrice");
                }
            }
        }

        private string _BestPrice;
        public string BestPrice
        {
            get { return this._BestPrice; }
            set
            {
                if (this._BestPrice != value)
                {
                    this._BestPrice = value;
                    this.OnPropertyChanged("BestPrice");
                }
            }
        }

        private DateTime _BestTime;
        public DateTime BestTime
        {
            get { return this._BestTime; }
            set
            {
                if (this._BestTime != value)
                {
                    this._BestTime = value;
                    this.OnPropertyChanged("BestTime");
                }
            }
        }

        private Phase _Phase;
        public Phase Phase
        {
            get { return this._Phase; }
            set
            {
                if (this._Phase != value)
                {
                    this._Phase = value;
                    this.OnPropertyChanged("Phase");
                    this.OnPropertyChanged("PhaseInString");
                    if (this.OnOrderPhaseChanged != null)
                    {
                        this.OnOrderPhaseChanged(this, new OrderPhaseChangedEventArgs(this._Phase));
                    }
                }
            }
        }

        public string PhaseInString
        {
            get
            {
                switch (this.Phase)
                {
                    case Phase.Canceled:
                        return "Canceled";
                    case Phase.Completed:
                        return "Completed";
                    case Phase.Deleted:
                        return "Deleted";
                    case Phase.Executed:
                        return "Confirmed";
                    case Phase.Placed:
                        if (this.Transaction.OrderType.GetCategory() == OrderTypeCategory.DQ)
                        {
                            return "In process";
                        }
                        else
                        {
                            return "Placed and wait";
                        }
                    case Phase.Placing:
                        if (this.OpenClose == OpenClose.Close && this.Transaction.SubType == TransactionSubType.IfDone)
                        {
                            return "Wait for activate";
                        }
                        else
                        {
                            return "Placing";
                        }
                    default:
                        throw new NotSupportedException(this.Phase.ToString());
                }
            }
        }

        private string _CancelReason;
        public string CancelReason
        {
            get { return string.IsNullOrEmpty(this._CancelReason) ? this.Transaction.Code : this._CancelReason; }
            set
            {
                if (this._CancelReason != value)
                {
                    this._CancelReason = value;
                    this.OnPropertyChanged("CancelReason");
                }
            }
        }

        private decimal _Lot;
        public decimal Lot
        {
            get { return this._Lot; }
            set
            {
                if (this._Lot != value)
                {
                    this._Lot = value;
                    this.OnPropertyChanged("Lot");
                    this.OnPropertyChanged("LotInFormat");
                }
            }
        }

        private decimal _LotBalance;
        public decimal LotBalance
        {
            get { return this._LotBalance; }
            set
            {
                if (this._LotBalance != value)
                {
                    this._LotBalance = value;
                    this.OnPropertyChanged("LotBalance");
                }
            }
        }

        private DateTime _SubmitDateTime;
        public DateTime SubmitDateTime
        {
            get { return this._SubmitDateTime; }
            set { this._SubmitDateTime = value; this.OnPropertyChanged("SubmitDateTime"); }
        }

        private int _HitCount;
        public int HitCount
        {
            get { return this._HitCount; }
            set
            {
                if (this._HitCount != value)
                {
                    this._HitCount = value;
                    this.OnPropertyChanged("HitCount");
                }
            }
        }

        public int DQMaxMove
        {
            get;
            set;
        }

        private OrderStatus _Status = OrderStatus.Placing;
        public OrderStatus Status
        {
            get{return this._Status;}
            set
            {
                if (this._Status != value)
                {
                    this._Status = value;
                    this.OnPropertyChanged("Status");
                    if (this.OnOrderStatusChangedHandler != null)
                    {
                        this.OnOrderStatusChangedHandler(value);
                    }
                }
            }
        }

        private OrderStatus _LastStatus = OrderStatus.Placing;
        public OrderStatus LastStatus
        {
            get { return this._LastStatus; }
            set
            {
                if (this._LastStatus != value)
                {
                    this._LastStatus = value;
                    this.OnPropertyChanged("LastStatus");
                }
            }
        }


        public string OpenPrice
        {
            get;
            set;
        }

        public string QuotePolicyCode
        {
            get { return "Default"; }
        }

        public string RelationString
        {
            get;
            set;
        }

        public string Dealer
        {
            get;
            set;
        }

        public string Type
        {
            get
            {
                return GetOrderTypeString(this.Transaction.OrderType, this.TradeOption, this.Transaction.Type, this.Transaction.SubType);
            }
        }

        public string IsBuyString
        {
            get { return this.BuySell == BuySell.Buy ? "B" : "S"; }
        }

        public string IsOpenString
        {
            get { return this.OpenClose == OpenClose.Open ? "N" : "C"; }
        }

        public SolidColorBrush IsOpenBrush
        {
            get
            {
                return this.OpenClose == OpenClose.Open ? new SolidColorBrush(Colors.Blue) : new SolidColorBrush(Colors.Red);
            }
        }
        public SolidColorBrush IsBuyBrush
        {
            get
            {
                return this.BuySell == BuySell.Buy ? new SolidColorBrush(Colors.Blue) : new SolidColorBrush(Colors.Red);
            }
        }

        #endregion

        void Transaction_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CancelReason")
            {
                this.OnPropertyChanged("CancelReason");
            }
        }

        internal void Initialize(CommonOrder commonOrder)
        {
            this.Id = commonOrder.Id;

            this.ExchangeCode = commonOrder.ExchangeCode;
            this.AccountId = commonOrder.AccountId;
            this.InstrumentId = commonOrder.InstrumentId;
            this.Code = commonOrder.Code;
            this.DQMaxMove = commonOrder.DQMaxMove;
            this.BuySell = commonOrder.IsBuy ? BuySell.Buy : BuySell.Sell;
            this.OpenClose = commonOrder.IsOpen ? OpenClose.Open : OpenClose.Close;
            this.Lot = commonOrder.Lot;
            this.SetPrice = string.IsNullOrEmpty(commonOrder.SetPrice) ? commonOrder.SetPrice : commonOrder.SetPrice.Replace(".", NumberFormatInfo.CurrentInfo.NumberDecimalSeparator);
            this.ExecutePrice = string.IsNullOrEmpty(commonOrder.ExecutePrice) ? commonOrder.ExecutePrice : commonOrder.ExecutePrice.Replace(".", NumberFormatInfo.CurrentInfo.NumberDecimalSeparator);
            this.TradeOption = commonOrder.TradeOption;
            this._HitCount = commonOrder.HitCount;
            this._BestPrice = commonOrder.BestPrice;
            this._BestTime = commonOrder.BestTime;
        }

        internal void Update(CommonOrder commonOrder)
        {
            this.Id = commonOrder.Id;

            this.ExchangeCode = commonOrder.ExchangeCode;
            this.Code = commonOrder.Code;
            this.DQMaxMove = commonOrder.DQMaxMove;
            this.BuySell = commonOrder.IsBuy ? BuySell.Buy : BuySell.Sell;
            this.OpenClose = commonOrder.IsOpen ? OpenClose.Open : OpenClose.Close;
            this.Lot = commonOrder.Lot;
            this.SetPrice = string.IsNullOrEmpty(commonOrder.SetPrice) ? commonOrder.SetPrice : commonOrder.SetPrice.Replace(".", NumberFormatInfo.CurrentInfo.NumberDecimalSeparator);
            this.ExecutePrice = string.IsNullOrEmpty(commonOrder.ExecutePrice) ? commonOrder.ExecutePrice : commonOrder.ExecutePrice.Replace(".", NumberFormatInfo.CurrentInfo.NumberDecimalSeparator);
            this.TradeOption = commonOrder.TradeOption;
            this._HitCount = commonOrder.HitCount;
            this._BestPrice = commonOrder.BestPrice;
            this._BestTime = commonOrder.BestTime;
        }

        internal void HitUpdate(CommonOrder commonOrder)
        {
            InstrumentClient instrument = this.Transaction.Instrument;
            this.HitCount = commonOrder.HitCount;
            this.BestPrice = commonOrder.BestPrice;
            this.BestTime = commonOrder.BestTime;

            Price bestPrice = new Price(this.BestPrice, instrument.NumeratorUnit, instrument.Denominator);

            if (this.Transaction.OrderType == OrderType.Market)
            {
                this.SetPrice = this.BestPrice;
            }
            else if (this.IsNeedDQMaxMove())
            {
                Price newPrice = this.GetDQMaxMovePrice(instrument);
                if (instrument.IsNormal == (this.BuySell == ManagerConsole.BuySell.Buy))
                {
                    this.ExecutePrice = (bestPrice > newPrice) ? newPrice.ToString() : bestPrice.ToString();
                }
                else
                {
                    this.ExecutePrice = (bestPrice < newPrice) ? newPrice.ToString() : bestPrice.ToString();
                }
            }

            //Hit2
            this.Hit2();
        }

        #region Change Order Status
        public void ChangeStatus(OrderStatus newStatus)
        {
            this.Status = newStatus;
        }

        private bool IsNeedDQMaxMove()
        {
            return (this.Transaction.OrderType == OrderType.SpotTrade && this.DQMaxMove > 0);
        }

        private Price GetDQMaxMovePrice(InstrumentClient instrument)
        {
            Price setPrice = Price.CreateInstance(double.Parse(this.SetPrice), instrument.NumeratorUnit, instrument.Denominator);
            bool isBuy = this.BuySell == ManagerConsole.BuySell.Buy;
            if (instrument.IsNormal == isBuy)
            {
                
                return (setPrice + this.DQMaxMove);
            }
            else
            {
                return (setPrice - this.DQMaxMove);
            }
        }

        internal void Hit2()
        {
            try
            {
                InstrumentClient instrument = this.Transaction.Instrument;
                if (this.Transaction.OrderType == OrderType.Limit || this.Transaction.OrderType == OrderType.Market)
                {
                    if (this.Transaction.OrderType == OrderType.Market && this.BestPrice != null)
                    {
                        this.ChangeStatus(OrderStatus.WaitOutLotLMT);
                    }

                    if (string.IsNullOrEmpty(this.BestPrice) || string.IsNullOrEmpty(this.BestPrice)) return;
                    Price bestPrice = new Price(this.BestPrice, instrument.NumeratorUnit, instrument.Denominator);
                    Price setPrice = new Price(this.SetPrice, instrument.NumeratorUnit, instrument.Denominator);
                    decimal diff = decimal.Parse(this.BestPrice) - decimal.Parse(this.SetPrice);

                    if (this.HitCount > 0 || (this.Transaction.OrderType == OrderType.Limit
                        && !this.Transaction.Instrument.Mit
                        && instrument.PenetrationPoint >= 0
                        && Math.Abs(diff) >= instrument.PenetrationPoint)) this.ChangeStatus(OrderStatus.WaitOutLotLMT);
                }
                else if (this.IsNeedDQMaxMove())
                {
                    if (instrument == null) return;
                    var isExceed = this.IsPriceExceedMaxMin(this.ExecutePrice);
                    if (isExceed == true)
                    {
                        this.ChangeStatus(OrderStatus.WaitOutPriceDQ);
                    }
                    else
                    {
                        this.ChangeStatus(OrderStatus.WaitOutLotDQ);
                    }
                    //this.mainWindow.oDealingConsole.PlaySound(SoundOption.LimitDealerIntervene);
                }
            }
            catch (Exception ex)
            {
                Manager.Common.Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "Order.Hit2 Error:.\r\n{0}", ex.ToString());
            }
        }



        #endregion

        private bool IsPriceExceedMaxMin(string executedPrice)
        {
            return true;
        }
    }

    public class OrderComparerByCode : IComparer<Order>
    {
        int IComparer<Order>.Compare(Order left, Order right)
        {
            return left.Code.CompareTo(right.Code);
        }
    }

    public class CloseOrder
    {
        public CloseOrder(Order order, decimal closeLot)
        {
            this.Order = order;
            this.CloseLot = closeLot;
        }

        public Order Order
        {
            get;
            private set;
        }

        public decimal CloseLot
        {
            get;
            private set;
        }
    }

    public static class OrderTypeHelper
    {
        public static string GetCaption(this OrderType orderType)
        {
            switch (orderType)
            {
                case OrderType.Limit:
                    return "LMT";
                case OrderType.Market:
                    return "MKT";
                case OrderType.MarketOnClose:
                    return "MOC";
                case OrderType.MarketOnOpen:
                    return "MOO";
                case OrderType.MarketToLimit:
                    return "MarketToLimit";
                case OrderType.MultipleClose:
                    return "MPC";
                case OrderType.OneCancelOther:
                    return "OCO";
                case OrderType.Risk:
                    return "SYS";
                case OrderType.SpotTrade:
                    return "SPT";
                case OrderType.Stop:
                    return "STP";
                case OrderType.StopLimit:
                    return "StopLimit";
                default:
                    return orderType.ToString();
            }
        }
    }
}
