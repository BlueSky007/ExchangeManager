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

        public int HitCount
        {
            get;
            set;
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

            this.Code = commonOrder.Code;
            this.DQMaxMove = commonOrder.DQMaxMove;
            this.BuySell = commonOrder.IsBuy ? BuySell.Buy : BuySell.Sell;
            this.OpenClose = commonOrder.IsOpen ? OpenClose.Open : OpenClose.Close;
            this.Lot = commonOrder.Lot;
            this.SetPrice = string.IsNullOrEmpty(commonOrder.SetPrice) ? commonOrder.SetPrice : commonOrder.SetPrice.Replace(".", NumberFormatInfo.CurrentInfo.NumberDecimalSeparator);
            this.ExecutePrice = string.IsNullOrEmpty(commonOrder.ExecutePrice) ? commonOrder.ExecutePrice : commonOrder.ExecutePrice.Replace(".", NumberFormatInfo.CurrentInfo.NumberDecimalSeparator);
            this.TradeOption = commonOrder.TradeOption;
        }


        #region Change Order Status
        private void ChangeStatus(OrderStatus orderStatus)
        {
           // this.OrderStatus = OrderStatus.WaitNextPrice;
        }
        public void DoAcceptPlace()
        {
            //if (this.OrderStatus == OrderStatus.WaitAcceptRejectPlace)
            //{
            //    this.ChangeStatus(OrderStatus.WaitServerResponse);
            //}
        }
        #endregion
    }
}
