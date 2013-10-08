using Manager.Common;
using ManagerConsole.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        private string _Code;
        public Guid Id
        {
            get;
            private set;
        }

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

        private decimal? _MinLot;
        public decimal? MinLot
        {
            get { return this._MinLot; }
            set
            {
                if (this._MinLot != value)
                {
                    this._MinLot = value;
                    this.OnPropertyChanged("MinLot");
                }
            }
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

        public TradeOption TradeOption
        {
            get;
            set;
        }

        public int DQMaxMove
        {
            get;
            set;
        }

        internal Manager.Common.Order ToCommonOrder()
        {
            return null;
        }

        internal void Update(Manager.Common.Order Order)
        { 

        }
    }
}
