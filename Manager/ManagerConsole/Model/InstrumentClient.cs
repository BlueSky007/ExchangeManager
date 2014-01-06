using ManagerConsole.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Quotation = Manager.Common.Settings.Quotation;
using Price = Manager.Common.Price;
using CommonInstrument = Manager.Common.Settings.Instrument;
using ExchangeInstrument = Manager.Common.Settings.ExchangInstrument;

namespace ManagerConsole.Model
{
    public class InstrumentClient : PropertyChangedNotifier
    {
        public delegate void PriceChangedHandler(string priceType, string price);
        public event PriceChangedHandler OnPriceChangedHandler;

        public InstrumentClient() { }
        public InstrumentClient(CommonInstrument instrument)
        {
            this.Update(instrument);
        }

        #region Private Property
        private string _Origin;
        private string _LastOrigin;
        private string _OriginCode = string.Empty;
        private bool _IsActive;
        private string _Bid;
        private string _Ask;
        private int _AlertPoint;
        private int? _AutoPoint;
        private int? _MaxAutoPoint;
        private int? _Spread;
        private int? _MaxSpread;
        private int? _AcceptDQVariation;
        private bool _IsNormal;
        private bool _Mit;
        private int _PenetrationPoint;
        private int _DailyMaxMove;
        private string _PreviousClosePrice;
        private decimal _BuyLot;
        private decimal _SellLot;
        private Guid? _SummaryGroupId;
        private string _SummaryGroupCode;
        #endregion

        #region Public Property
        public Guid Id
        {
            get;
            set;
        }
        public string OriginCode
        {
            get;
            set;
        }
        public string Code
        {
            get;
            set;
        }
        public string ExchangeCode
        {
            get;
            set;
        }
        public string Description
        {
            get;
            set;
        }
        public int? Denominator
        {
            get;
            set;
        }
        public int? NumeratorUnit
        {
            get;
            set;
        }
        public string Origin
        {
            get
            { return this._Origin; }
            set
            {
                this.LastOrigin = this._Origin;
                this._Origin = value;
                this.OnPropertyChanged("Origin");
            }
        }
        public string LastOrigin
        {
            get { return this._LastOrigin; }
            set
            {
                this._LastOrigin = value;
                this.OnPropertyChanged("LastOrigin");

            }
        }
        public bool IsActive
        {
            get { return this._IsActive; }
            set
            {
                this._IsActive = value;
                this.OnPropertyChanged("IsActive");

            }
        }

        public string Bid
        {
            get { return this._Bid; }
            set
            {
                this._Bid = value; 
                this.OnPropertyChanged("Bid");
                if (this.OnPriceChangedHandler != null)
                {
                    this.OnPriceChangedHandler("Bid", value);
                }
            }
        }
        public string Ask
        {
            get { return this._Ask; }
            set
            {
                this._Ask = value;
                this.OnPropertyChanged("Ask");
                if (this.OnPriceChangedHandler != null)
                {
                    this.OnPriceChangedHandler("Ask", value);
                }
            }
        }

        public Quotation LastQuotation
        {
            get;
            set;
        }

        public int AlertPoint
        {
            get { return this._AlertPoint; }
            set
            {
                if (Regex.IsMatch(value.ToString(), "(\\d+)"))
                {
                    this._AlertPoint = value; this.OnPropertyChanged("AlertPoint");
                }
                else
                {
                    throw new Exception("Alert Point must be int");
                }
            }
        }

        public int? AutoPoint
        {
            get { return this._AutoPoint; }
            set
            {
                if (value <= this._MaxAutoPoint && Regex.IsMatch(value.ToString(), "(-?\\d+)"))
                {
                    this._AutoPoint = value; this.OnPropertyChanged("AutoPoint");
                }
                else
                {
                    throw new Exception("Invalid Input Auto Point");
                }
            }
        }
        public int? MaxAutoPoint
        {
            get { return this._MaxAutoPoint; }
            set
            {
                if (Regex.IsMatch(value.ToString(), "(\\d+)"))
                {
                    this._MaxAutoPoint = value; this.OnPropertyChanged("MaxAutoPoint");
                }
                else
                {
                    throw new Exception("Invalid Input Max Auto Point");
                }
            }
        }
        public int? Spread
        {
            get { return this._Spread; }
            set
            {
                if (value <= this._MaxSpread && Regex.IsMatch(value.ToString(), "(\\d+)"))
                {
                    this._Spread = value; this.OnPropertyChanged("Spread");
                }
                else
                {
                    throw new Exception("Invalid Input Spread");
                }
            }
        }
        public int? MaxSpread
        {
            get { return this._MaxSpread; }
            set
            {
                if (Regex.IsMatch(value.ToString(), "(\\d+)"))
                {
                    this._MaxSpread = value; this.OnPropertyChanged("MaxSpread");
                }
                else
                {
                    throw new Exception("Invalid Input Max Spread");
                }
            }
        }

        public int AcceptDQVariation
        {
            get { return (int)this._AcceptDQVariation; }
            set{this._AcceptDQVariation = value;}
        }

        public bool IsNormal
        {
            get { return this._IsNormal; }
            set { this._IsNormal = value; }
        }

        public bool Mit
        {
            get { return this._Mit; }
            set { this._Mit = value; }
        }

        public int PenetrationPoint
        {
            get { return this._PenetrationPoint; }
            set { this._PenetrationPoint = value; }
        }

        public int DailyMaxMove
        {
            get { return this._DailyMaxMove; }
            set { this._DailyMaxMove = value; }
        }

        public string PreviousClosePrice
        {
            get { return this._PreviousClosePrice; }
            set { this._PreviousClosePrice = value; }
        }

        public decimal BuyLot
        {
            get { return this._BuyLot; }
            set
            {
                this._BuyLot = value; this.OnPropertyChanged("BuyLot");
            }
        }
        public decimal SellLot 
        {
            get { return this._SellLot; }
            set
            {
                this._SellLot = value; this.OnPropertyChanged("SellLot");
            }
        }

        public decimal LastLot
        {
            get;
            set;
        }

        public string LastSales
        {
            get;
            set;
        }

        public Guid? SummaryGroupId
        { 
            get{return this._SummaryGroupId;}
            set{this._SummaryGroupId = value;}
        }
            
        public string SummaryGroupCode
        {
            get { return this._SummaryGroupCode; }
            set { this._SummaryGroupCode = value; }
        }
        #endregion

        internal void Update(CommonInstrument instrument)
        {
            if (instrument.AcceptDQVariation != null) this.AcceptDQVariation = instrument.AcceptDQVariation.Value;
            this.Id = instrument.Id;
        }

        internal ExchangeInstrument ToExchangeInstrument()
        {
            ExchangeInstrument exchangeInstrument = new ExchangeInstrument();
            exchangeInstrument.ExchangeCode = this.ExchangeCode;
            exchangeInstrument.InstrumentId = this.Id;
            exchangeInstrument.InstrumentCode = this.Code;

            return exchangeInstrument;
        }

        public bool CheckVariation(decimal variation)
        {
            if (variation < 0 && variation < (0 - this.AcceptDQVariation))
            {
                return false;
            }
            return true;
        }

        public int GetSourceAskBidDiffValue()
        {
            int diffValue = 0;
            Price lastAsk = new Price(this.LastQuotation.Ask, this.NumeratorUnit.Value, this.Denominator.Value);
            Price lastBid = new Price(this.LastQuotation.Bid, this.NumeratorUnit.Value, this.Denominator.Value);

            diffValue = lastAsk - lastBid;
            return diffValue;
        }
    }
}
