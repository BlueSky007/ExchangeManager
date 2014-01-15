using ManagerConsole.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Quotation = Manager.Common.Settings.Quotation;
using Price = iExchange.Common.Price;
using CommonInstrument = Manager.Common.Settings.Instrument;
using ExchangeInstrument = Manager.Common.Settings.ExchangInstrument;
using InstrumentCategory = iExchange.Common.InstrumentCategory;
using OriginType = iExchange.Common.OriginType;
using Manager.Common.ExchangeEntities;

namespace ManagerConsole.Model
{
    public class InstrumentClient : PropertyChangedNotifier
    {
        public delegate void PriceChangedHandler(string priceType, string price);
        public event PriceChangedHandler OnPriceChangedHandler;

        public InstrumentClient() { }
        public InstrumentClient(CommonInstrument instrument)
        {
            this.Initialize(instrument);
        }

        #region Private Property
         // private OverrideQuotation
        private string _Origin;
        private string _LastOrigin;
        private InstrumentCategory _Category;
        private string _Code;
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

        private OriginType _OriginType ;
        private int _AllowedSpotTradeOrderSides ;
        private int _OriginInactiveTime ;
        private int _AlertVariation ;
        private int _NormalWaitTime ;
        private int _AlertWaitTime ;
        private decimal _MaxDQLot ;
        private decimal _MaxOtherLot ;
        private decimal? _DqQuoteMinLot ;
        private int _OrderTypeMask ;
        private decimal _AutoCancelMaxLot ;
        private decimal _AutoAcceptMaxLot ;
        private decimal _AutoDQMaxLot ;
        private decimal _AutoLmtMktMaxLot ;
        private int? _AcceptLmtVariation ;
        private int? _AcceptCloseLmtVariation ;
        private int? _AcceptIfDoneVariation ;
        private int? _CancelLmtVariation ;
        private int _MaxMinAdjust ;
        private bool _IsBetterPrice ;
        private int _HitTimes ;
        private int _PriceValidTime ;
        private TimeSpan _LastAcceptTimeSpan ;
        private DateTime? _NextDayOpenTime ;
        private DateTime? _DayOpenTime ;
        private DateTime? _DayCloseTime ;
        private DateTime? _LastDayCloseTime ;
        private DateTime? _LastTradeDay ;
        private DateTime? _MOCTime ;
        private bool _IsAutoEnablePrice ;
        private bool _IsAutoFill ;
        private bool _IsPriceEnabled ;
        private int _AutoDQDelay ;
        private int _HitPriceVariationForSTP ;
        private decimal _LastLot;
        private string _LastSales;
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
        public string ExchangeCode { get; set; }
        public string Code { get; set; }
        public string Description
        {
            get;
            set;
        }
        public int? Denominator { get; set; }
        public int? NumeratorUnit { get; set; }
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
        public InstrumentCategory Category
        {
            get { return this._Category; }
            set
            {
                this._Category = value;
                this.OnPropertyChanged("Category");

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

        public Guid? SummaryGroupId { get; set; }
        public string SummaryGroupCode { get; set; }
        public decimal SummaryUnit { get; set; }
        public decimal SummaryQuantity { get; set; }
        #endregion

        #region 
        public DateTime BeginTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsNormal { get; set; }
        public OriginType OriginType { get; set; }
        public int AllowedSpotTradeOrderSides { get; set; }
        public int OriginInactiveTime { get; set; }
        public int AcceptDQVariation { get; set; }
        public int AlertVariation { get; set; }
        public int NormalWaitTime { get; set; }
        public int AlertWaitTime { get; set; }
        public decimal MaxDQLot { get; set; }
        public decimal MaxOtherLot { get; set; }
        public decimal DqQuoteMinLot { get; set; }
        public decimal AutoDQMaxLot { get; set; }
        public decimal AutoLmtMktMaxLot { get; set; }
        public int AcceptLmtVariation { get; set; }
        public int AcceptCloseLmtVariation { get; set; }
        public int CancelLmtVariation { get; set; }
        public int MaxMinAdjust { get; set; }
        public bool IsBetterPrice { get; set; }
        public int HitTimes { get; set; }
        public int PriceValidTime { get; set; }
        public TimeSpan LastAcceptTimeSpan { get; set; }
        public int OrderTypeMask { get; set; }
        public decimal AutoCancelMaxLot { get; set; }
        public decimal AutoAcceptMaxLot { get; set; }
        public int AllowedNewTradeSides { get; set; }
        public DateTime? NextDayOpenTime { get; set; }
        public DateTime DayOpenTime { get; set; }
        public DateTime DayCloseTime { get; set; }
        public DateTime LastDayCloseTime { get; set; }
        public DateTime LastTradeDay { get; set; }
        public DateTime? MOCTime { get; set; }
        public bool IsAutoFill { get; set; }
        public bool IsPriceEnabled { get; set; }
        public bool IsAutoEnablePrice { get; set; }
        public int AutoDQDelay { get; set; }
        public int HitPriceVariationForSTP { get; set; }
        public int DailyMaxMove { get; set; }
        public int PenetrationPoint { get; set; }
        public string PreviousClosePrice { get; set; }
        public bool Mit { get; set; }
        public decimal BuyLot { get; set; }
        public decimal SellLot { get; set; }
        #endregion

        public void Update(Dictionary<string, string> fieldAndValues)
        {
            foreach (string key in fieldAndValues.Keys)
            {
                this.Update(key, fieldAndValues[key]);
            }
        }

        public void Update(string field, object value)
        {
            if (field == ExchangeFieldSR.Code)
            {
                this.Code = (string)value;
            }
            else if (field == ExchangeFieldSR.IsPriceEnabled)
            {
                this.IsPriceEnabled = (bool)value;
            }
            else if (field == ExchangeFieldSR.IsAutoEnablePrice)
            {
                this.IsAutoEnablePrice = (bool)value;
            }
        }


        internal void Initialize(CommonInstrument instrument)
        {
            this.Id = instrument.Id;
            this.ExchangeCode = instrument.ExchangeCode;
            this.OriginCode = instrument.OriginCode;
            this.Code = instrument.Code;
            this.Category = instrument.Category;
            this.IsActive = instrument.IsActive;
            this.BeginTime = instrument.BeginTime;
            this.EndTime = instrument.EndTime;
            this.Denominator = instrument.Denominator;
            this.NumeratorUnit = instrument.NumeratorUnit;
            this.IsNormal = instrument.IsNormal;
            this.OriginType = instrument.OriginType;
            this.AllowedSpotTradeOrderSides = instrument.AllowedSpotTradeOrderSides;
            this.OriginInactiveTime = instrument.OriginInactiveTime;
            this.AlertVariation = instrument.AlertVariation;
            this.NormalWaitTime = instrument.NormalWaitTime;
            this.AlertWaitTime = instrument.AlertWaitTime;
            this.AlertVariation = instrument.AlertVariation;
            this.MaxDQLot = instrument.MaxDQLot;
            this.MaxOtherLot = instrument.MaxOtherLot;
            this.DqQuoteMinLot = instrument.DqQuoteMinLot;
            this.AutoDQMaxLot = instrument.AutoDQMaxLot;
            this.AutoLmtMktMaxLot = instrument.AutoLmtMktMaxLot;
            this.AcceptDQVariation = instrument.AcceptDQVariation;
            this.AcceptLmtVariation = instrument.AcceptLmtVariation;
            this.AcceptCloseLmtVariation = instrument.AcceptCloseLmtVariation;
            this.CancelLmtVariation = instrument.CancelLmtVariation;
            this.MaxMinAdjust = instrument.MaxMinAdjust;
            this.IsBetterPrice = instrument.IsBetterPrice;
            this.HitTimes = instrument.HitTimes;
            this.PenetrationPoint = instrument.PenetrationPoint;
            this.PriceValidTime = instrument.PriceValidTime;
            this.DailyMaxMove = instrument.DailyMaxMove;
            this.LastAcceptTimeSpan = instrument.LastAcceptTimeSpan;
            this.OrderTypeMask = instrument.OrderTypeMask;
            this.PreviousClosePrice = instrument.PreviousClosePrice;
            this.AutoCancelMaxLot = instrument.AutoCancelMaxLot;
            this.AutoAcceptMaxLot = instrument.AutoAcceptMaxLot;
            this.AllowedNewTradeSides = instrument.AllowedNewTradeSides;
            this.Mit = instrument.Mit;
            this.IsAutoFill = instrument.IsAutoFill;
            this.IsPriceEnabled = instrument.IsPriceEnabled;
            this.IsAutoEnablePrice = instrument.IsAutoEnablePrice;
            this.NextDayOpenTime = instrument.NextDayOpenTime;
            this.MOCTime = instrument.MOCTime;
            this.DayOpenTime = instrument.DayOpenTime;
            this.DayCloseTime = instrument.DayCloseTime;
            this.AutoDQDelay = instrument.AutoDQDelay;
            this.SummaryGroupId = instrument.SummaryGroupId;
            this.SummaryGroupCode = instrument.SummaryGroupCode;
            this.SummaryUnit = instrument.SummaryUnit;
            this.SummaryQuantity = instrument.SummaryQuantity;
            this.BuyLot = instrument.BuyLot;
            this.SellLot = instrument.SellLot;
            this.HitPriceVariationForSTP = instrument.HitPriceVariationForSTP;

            this.Ask = this.PreviousClosePrice;
            this.Bid = this.PreviousClosePrice;
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
