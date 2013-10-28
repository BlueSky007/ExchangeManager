using Manager.Common;
using ManagerConsole.Helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using CommonOrder = Manager.Common.Order;
using OrderType = Manager.Common.OrderType;

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
        public event OrderPhaseChangedEventHandler OnOrderPhaseChanged;

        public Order(Transaction transaction,CommonOrder commonOrder)
        {
            this.Transaction = transaction;
            this.Phase = transaction.Phase;
            this.Id = commonOrder.Id;
            
            this.Update(commonOrder);
            this.Transaction.PropertyChanged += new PropertyChangedEventHandler(Transaction_PropertyChanged);
        }

        #region Property
        public Transaction Transaction
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

        #endregion

        void Transaction_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CancelReason")
            {
                this.OnPropertyChanged("CancelReason");
            }
        }

        internal Manager.Common.Order ToCommonOrder()
        {
            return null;
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

            Price bestPrice = new Price(this.BestPrice, instrument.NumeratorUnit.Value, instrument.Denominator.Value);

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
            if (this.Transaction.OrderType == OrderType.Limit || this.Transaction.OrderType == OrderType.Market)
            {
                if (this.Transaction.OrderType == OrderType.Market && this.Transaction != null)
                {
                    this.ChangeStatus(OrderStatus.WaitOutLotLMT);
                }

                Price setPrice = new Price(this.SetPrice, instrument.NumeratorUnit.Value, instrument.Denominator.Value);
                if (this.BestPrice == null || this.SetPrice == null) return;
                if (this.HitCount > 0
                    || (this.Transaction.OrderType == OrderType.Limit 
                    && !instrument.Mit && instrument.PenetrationPoint >= 0
                    && Math.Abs(Price.Subtract(bestPrice, setPrice)) >= instrument.PenetrationPoint))
                {
                    this.ChangeStatus(OrderStatus.WaitOutLotLMT);
                    //this.mainWindow.oDealingConsole.PlaySound(SoundOption.LimitDealerIntervene);
                }
            }
            else if (this.IsNeedDQMaxMove())
            {
                var isExceed = true;// = this.IsPriceExceedMaxMin(this.tran.executePrice);
                if (isExceed == true)
                {
                    //Waiting for Dealer Accept/Reject
                    this.ChangeStatus(OrderStatus.WaitOutPriceDQ);
                }
                else
                {
                    //Waiting for Dealer Confirm/Reject
                    this.ChangeStatus(OrderStatus.WaitOutLotDQ);
                }
            }
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
            Price setPrice = Price.CreateInstance(double.Parse(this.SetPrice), instrument.NumeratorUnit.Value, instrument.Denominator.Value);
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


        #endregion
    }
}
