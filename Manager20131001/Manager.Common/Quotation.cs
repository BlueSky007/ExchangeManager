using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Manager.Common
{
    public class Quotation
    {
        private Guid _InstrumentId;
        private string _Origin;
        private string _LastConfirmOrigin;
        private string _Bid;
        private string _Ask;
        private string _High;
        private string _Low;
        private DateTime _TimeStamp;

        public Guid InstrumentId
        {
            get { return this._InstrumentId; }
            set { this._InstrumentId = value; }
        }
        public string Origin
        {
            get { return this._Origin; }
            set { this._Origin = value; }
        }
        public string LastConfirmOrigin
        {
            get { return this._LastConfirmOrigin; }
            set { this._LastConfirmOrigin = value; }
        }
        public string Bid
        {
            get { return this._Bid; }
            set { this._Bid = value; }
        }
        public string Ask
        {
            get { return this._Ask; }
            set { this._Ask = value; }
        }
        public string High
        {
            get { return this._High; }
            set { this._High = value; }
        }
        public string Low
        {
            get { return this._Low; }
            set { this._Low = value; }
        }

        public DateTime TimeStamp
        {
            get { return this._TimeStamp; }
            set { this._TimeStamp = value; }
        }

        public static Quotation Create(double adjust, double origin, int numeratorUnit,
           int denominator, int autoPoint, int spread)
        {
            string validInt = @"^-?\d+$";
            Price originPrice;
            if (Regex.IsMatch(adjust.ToString(), validInt))
            {
                originPrice = Price.CreateInstance(origin, numeratorUnit, denominator);
                originPrice = Price.Adjust(originPrice, (int)adjust);
            }
            else
            {
                originPrice = Price.CreateInstance(adjust, numeratorUnit, denominator);
            }

            Quotation baseQuotation = new Quotation();
            if (originPrice != null)
            {
                baseQuotation.Origin = originPrice.ToPriceEntity().normalizedPrice;

                baseQuotation.Bid = (originPrice + autoPoint).ToPriceEntity().normalizedPrice;
                baseQuotation.Ask = (originPrice + autoPoint + spread).ToPriceEntity().normalizedPrice;
            }
            return baseQuotation;
        }

        public static Quotation Create(string quotationString)
        {
            Quotation baseQuotation = new Quotation();
            string[] contents = quotationString.Split('|');
            if (contents.Length == 6)
            {
                Guid instrumentId;
                if (Guid.TryParse(contents[0], out instrumentId))
                {
                    baseQuotation.InstrumentId = instrumentId;
                }
                baseQuotation.Origin = contents[1];
                baseQuotation.LastConfirmOrigin = contents[2];
                baseQuotation.Bid = contents[3];
                baseQuotation.Ask = contents[4];
                long ticks;
                if (long.TryParse(contents[5], out ticks))
                {
                    baseQuotation.TimeStamp = new DateTime(ticks);
                }
            }

            return baseQuotation;
        }
        public override string ToString()
        {
            return string.Format("{0}|{1}|{2}|{3}|{4}|{5}", this._InstrumentId, this._Origin, this._LastConfirmOrigin,
                this._Bid, this._Ask, this._TimeStamp.Ticks);
        }
    }

    public class QuoteQuotation : Quotation
    {
        private Guid _Id;
        private Guid _CustomerId;
        private string _CustomerCode;
        private decimal _QuoteLot;
        private bool _IsBuy;
        private TimeSpan _EnquiryOutTime;
        private string _ExchangeCode;

        public QuoteQuotation()
        {
            this._Id = Guid.NewGuid();
        }
        public QuoteQuotation(Quotation quotation)
            : this()
        {
            this.Ask = quotation.Ask;
            this.Bid = quotation.Bid;
            this.High = quotation.High;
            this.InstrumentId = quotation.InstrumentId;
            this.LastConfirmOrigin = quotation.LastConfirmOrigin;
            this.Low = quotation.Low;
            this.Origin = quotation.Origin;
        }
        public Guid Id
        {
            get { return this._Id; }
        }
        public Guid CustomerId
        {
            get { return this._CustomerId; }
            set { this._CustomerId = value; }
        }
        public string CustomerCode
        {
            get { return this._CustomerCode; }
            set { this._CustomerCode = value; }
        }
        public decimal QuoteLot
        {
            get { return this._QuoteLot; }
            set { this._QuoteLot = value; }
        }
        public bool IsBuy
        {
            get { return this._IsBuy; }
            set { this._IsBuy = value; }
        }
        public TimeSpan EnquiryOutTime
        {
            get { return this._EnquiryOutTime; }
            set
            {
                this._EnquiryOutTime = value;
                if (this._EnquiryOutTime.TotalSeconds < 0)
                {
                    this._EnquiryOutTime = TimeSpan.FromSeconds(0);
                }
            }
        }

        public string ExchangeCode
        {
            get { return this._ExchangeCode; }
            set { this._ExchangeCode = value; }
        }
    } 
}
