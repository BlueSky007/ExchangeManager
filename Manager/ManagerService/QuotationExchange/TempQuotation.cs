using iExchange.Common;
using Manager.Common.QuotationEntities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ManagerService.QuotationExchange
{
    public class CollectorQuotation
    {
        private string originCode;
        private DateTime timestamp;
        private string ask;
        private string bid;
        private string high;
        private string low;

        private string volume;
        private string totalVolume;

        #region Common public properties definition
        public string OriginCode
        {
            get { return this.originCode; }
        }
        public DateTime Timestamp
        {
            get { return this.timestamp; }
        }
        public string Ask
        {
            get { return this.ask; }
        }
        public string Bid
        {
            get { return this.bid; }
        }
        public string High
        {
            get { return this.high; }
        }
        public string Low
        {
            get { return this.low; }
        }
        public string Volume
        {
            get { return this.volume; }
        }
        public string TotalVolume
        {
            get { return this.totalVolume; }
        }
        #endregion Common public properties definition

        public CollectorQuotation(GeneralQuotation quotation)
        {
            this.originCode = quotation.OriginCode;
            this.timestamp = quotation.Timestamp == (new DateTime()) ? DateTime.Now : quotation.Timestamp;
            this.ask = quotation.Ask.ToString();
            this.bid = quotation.Bid.ToString();
            this.high = quotation.High.ToString();
            this.low = quotation.Low.ToString();
            this.volume = quotation.Volume;
            this.totalVolume = quotation.TotalVolume;
        }

        public CollectorQuotation(string originCode, DateTime timestamp, string ask, string bid, string high, string low)
        {
            this.originCode = originCode;
            this.timestamp = timestamp;
            this.ask = ask;
            this.bid = bid;
            this.high = high;
            this.low = low;
        }

        public void AdjustTimestamp(TimeSpan adjustTime)
        {
            this.timestamp = this.timestamp.Add(adjustTime);
        }

        public static CollectorQuotation CreateInstance(GeneralQuotation quotation)
        {
            CollectorQuotation cq = null;
            try
            {
                cq = new CollectorQuotation(quotation);
            }
            catch (Exception e)
            {
                AppDebug.LogEvent("CollectorQuotation", string.Format("CreateInstance'{0}',{1}", quotation, e.ToString()), EventLogEntryType.Error);
            }
            return cq;
        }

        public override string ToString()
        {
            return this.originCode + ","
                + this.timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff") + ","
                + this.ask + ","
                + this.bid + ","
                + this.high + ","
                + this.low
                + (this.volume == null ? "" : "," + this.volume + "," + this.totalVolume);
        }

        internal void EmptyZeroPrice()
        {
            this.ask = this.EmptyZeroPrice(this.ask);
            this.bid = this.EmptyZeroPrice(this.bid);
            this.high = this.EmptyZeroPrice(this.high);
            this.low = this.EmptyZeroPrice(this.low);
        }

        internal bool IsEmpty()
        {
            return string.IsNullOrEmpty(this.ask)
                && string.IsNullOrEmpty(this.bid)
                && string.IsNullOrEmpty(this.high)
                && string.IsNullOrEmpty(this.low);
        }

        private string EmptyZeroPrice(string price)
        {
            decimal price2;
            if (decimal.TryParse(price, out price2))
            {
                if (price2 == 0) return string.Empty;
            }

            return price;
        }
    }

    public class DealerQuotation
    {
        private Guid instrumentID;
        private DateTime timestamp;
        private string originString;
        private string highString;
        private string lowString;
        private string volume;
        private string totalVolume;
        private Price origin;
        private Price originHigh;
        private Price originLow;

        #region Common public properties definition
        public Guid InstrumentID
        {
            get { return this.instrumentID; }
        }
        public DateTime Timestamp
        {
            get { return this.timestamp; }
            set { this.timestamp = value; }
        }
        public Price Origin
        {
            get { return this.origin; }
        }
        public Price OriginHigh
        {
            get { return this.originHigh; }
        }
        public Price OriginLow
        {
            get { return this.originLow; }
        }

        public string Volume
        {
            get { return this.volume; }
        }
        public string TotalVolume
        {
            get { return this.totalVolume; }
        }
        #endregion Common public properties definition

        public DealerQuotation(GeneralQuotation quotation)
        {
            this.instrumentID = new Guid();
            this.timestamp = quotation.Timestamp;
            this.originString = quotation.OriginCode;
            this.highString = quotation.High.ToString();
            this.lowString = quotation.Low.ToString();
            this.volume = quotation.Volume;
            this.totalVolume = quotation.TotalVolume;
            this.origin = null;
            this.originHigh = null;
            this.originLow = null;
        }

        public static DealerQuotation CreateInstance(GeneralQuotation quotation)
        {
            DealerQuotation dq = null;
            try
            {
                dq = new DealerQuotation(quotation);
            }
            catch (Exception e)
            {
                AppDebug.LogEvent("DealerQuotation", e.ToString(), EventLogEntryType.Error);
            }
            return dq;
        }

        public void NormalizeOrigins(Instrument instrument)
        {
            this.origin = Price.CreateInstance(this.originString, instrument.NumeratorUnit, instrument.Denominator);
            this.originHigh = Price.CreateInstance(this.highString, instrument.NumeratorUnit, instrument.Denominator);
            this.originLow = Price.CreateInstance(this.lowString, instrument.NumeratorUnit, instrument.Denominator);
        }

        public override string ToString()
        {
            return this.instrumentID.ToString() + ","
                + this.timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff") + ","
                + this.originString + ","
                + this.highString + ","
                + this.lowString;
        }


        internal iExchange.Common.OriginQuotation ToLightVersion(Instrument instrument)
        {
            iExchange.Common.OriginQuotation lightOriginQ = new iExchange.Common.OriginQuotation();
            lightOriginQ.HasWatchOnlyQuotePolicies = instrument.HasWatchOnlyQuotePolicies;
            lightOriginQ.InstrumentID = this.InstrumentID;
            lightOriginQ.Timestamp = this.timestamp;
            lightOriginQ.Ask = (string)this.origin;
            lightOriginQ.Bid = (string)this.origin;
            lightOriginQ.High = (string)this.originHigh;
            lightOriginQ.Low = (string)this.originLow;
            lightOriginQ.Origin = (string)this.origin;
            lightOriginQ.Volume = this.volume;
            lightOriginQ.TotalVolume = this.totalVolume;

            return lightOriginQ;
        }
    }
}
