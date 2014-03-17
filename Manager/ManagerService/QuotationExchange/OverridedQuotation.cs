using iExchange.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace ManagerService.QuotationExchange
{
    public class OverridedQuotation : PersistObject
    {
        //use to update historyQuotation
        private static readonly string insHistorySqlFormat = "EXEC dbo.P_FixAddOverridedQuotationHistory '{0}','{1}','{2:yyyy-MM-dd HH:mm:ss.fff}','{3}','{4}','{5}','{6}','{7}','{8}'\n";
        private static readonly string delHistorySqlFormat = "EXEC dbo.P_FixRemoveOverridedQuotationHistory '{0}','{1}','{2:yyyy-MM-dd HH:mm:ss.fff}','{3}'\n";

        private QuotePolicyDetail quotePolicy;
        private Instrument instrument;
        private DateTime timestamp;
        private Price origin;
        private Price ask;
        private Price bid;
        private Price high;
        private Price low;
        private Guid dealerID;
        private string volume;
        private string totalVolume;

        #region Common public properties definition
        public Instrument Instrument
        {
            get { return this.instrument; }
        }
        public QuotePolicyDetail QuotePolicy
        {
            get { return this.quotePolicy; }
        }
        public DateTime Timestamp
        {
            get { return this.timestamp; }
        }
        public Price Ask
        {
            get { return this.ask; }
        }
        public Price Bid
        {
            get { return this.bid; }
        }
        public Price High
        {
            get { return this.high; }
        }
        public Price Low
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

        public Guid DealerID
        {
            get { return this.dealerID; }
        }

        public Price Origin
        {
            get { return this.origin; }
        }

        #endregion Common public properties definition
        #region PersistObject
        public override void Merge(PersistObject persistObject)
        {
            OverridedQuotation oq = (OverridedQuotation)persistObject;
            if (this.timestamp >= oq.timestamp) return;

            this.modifyState = ModifyState.Modified;
            this.dealerID = oq.dealerID;
            this.timestamp = oq.timestamp;
            if (oq.ask != null) { this.ask = oq.ask; }
            if (oq.bid != null) { this.bid = oq.bid; }
            if (oq.high != null) { this.high = oq.high; }
            if (oq.low != null) { this.low = oq.low; }
            if (oq.origin != null) { this.origin = oq.origin; }

            this.volume = oq.volume;
            this.totalVolume = oq.totalVolume;

            return;
        }

        //Add by Korn 2008-9-9
        private OverridedQuotation()
        {
        }

        public static OverridedQuotation CreateHistoryQuotation(Guid dealerID, Instrument instrument, QuotePolicyDetail quotePolicy, string timestamp, string origin, bool needApplyAutoAdjustPoints, bool highBid, bool lowBid)
        {
            OverridedQuotation overridedQuotation = new OverridedQuotation();
            overridedQuotation.modifyState = ModifyState.Added;

            overridedQuotation.quotePolicy = quotePolicy;
            overridedQuotation.instrument = instrument;
            overridedQuotation.timestamp = DateTime.Parse(timestamp);
            overridedQuotation.origin = Price.CreateInstance(origin, instrument.NumeratorUnit, instrument.Denominator);

            overridedQuotation.dealerID = dealerID;

            overridedQuotation.bid = needApplyAutoAdjustPoints ? overridedQuotation.origin + quotePolicy.AutoAdjustPoints : overridedQuotation.origin;
            overridedQuotation.ask = overridedQuotation.bid + quotePolicy.SpreadPoints;

            overridedQuotation.CalculateHiLo(highBid, lowBid);

            //overridedQuotation.high = highBid ? overridedQuotation.bid : overridedQuotation.ask;
            //overridedQuotation.low = lowBid ? overridedQuotation.bid : overridedQuotation.ask;

            return overridedQuotation;
        }

        public string GetInsertHistorySql()
        {
            return string.Format(OverridedQuotation.insHistorySqlFormat, this.quotePolicy.ID, this.instrument.ID, this.timestamp, this.origin, this.ask, this.bid, this.high, this.low, this.dealerID);
        }

        public string GetRemoveHistorySql()
        {
            return string.Format(OverridedQuotation.delHistorySqlFormat, this.quotePolicy.ID, this.instrument.ID, this.timestamp, this.dealerID);
        }

        //public string GetUpdateChartSqlFormat(bool needOutput)
        //{
        //    return string.Format(OverridedQuotation.updChartSqlFormat, this.instrument.ID, this.quotePolicy.ID, this.timestamp, (needOutput)?1:0);
        //}

        public override string GetUpdateSql(bool resetModifyState)
        {
            StringBuilder stringBuilder = new StringBuilder();
            switch (this.modifyState)
            {
                case ModifyState.Added:
                    stringBuilder.Append("EXEC dbo.P_AddOverridedQuotation ");
                    break;
                case ModifyState.Modified:
                    stringBuilder.Append("EXEC dbo.P_UpdateOverridedQuotation ");
                    break;
                default:
                    return string.Empty;
            }

            stringBuilder.Append("'").Append(this.quotePolicy.ID.ToString()).Append("',");
            stringBuilder.Append("'").Append(this.instrument.ID.ToString()).Append("',");
            stringBuilder.Append("'").Append(this.timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff")).Append("',");

            OverridedQuotation.AppendPrice(stringBuilder, this.origin);
            stringBuilder.Append(",");

            OverridedQuotation.AppendPrice(stringBuilder, this.ask);
            stringBuilder.Append(",");

            OverridedQuotation.AppendPrice(stringBuilder, this.bid);
            stringBuilder.Append(",");

            OverridedQuotation.AppendPrice(stringBuilder, this.high);
            stringBuilder.Append(",");

            OverridedQuotation.AppendPrice(stringBuilder, this.low);
            stringBuilder.Append(",");

            stringBuilder.Append("'").Append(this.dealerID.ToString()).Append("'");
            stringBuilder.Append(",");

            OverridedQuotation.AppendString(stringBuilder, this.volume);
            stringBuilder.Append(",");

            OverridedQuotation.AppendString(stringBuilder, this.totalVolume);
            stringBuilder.Append("\n");

            if (resetModifyState) this.modifyState = ModifyState.Unchanged;
            return stringBuilder.ToString();
        }

        public static void AppendPrice(StringBuilder stringBuilder, Price price)
        {
            OverridedQuotation.AppendString(stringBuilder, (string)price);
        }

        public static void AppendString(StringBuilder stringBuilder, string value)
        {
            if (value == null)
            {
                stringBuilder.Append("NULL");
            }
            else
            {
                stringBuilder.Append("'").Append(value).Append("'");
            }
        }

        #endregion PersistObject

        public OverridedQuotation(Instrument instrument, QuotePolicyDetail quotePolicy, DataRow overridedQuotation)
        {
            this.modifyState = ModifyState.Unchanged;

            this.quotePolicy = quotePolicy;
            this.instrument = instrument;
            this.timestamp = (DateTime)overridedQuotation["Timestamp"];
            if (overridedQuotation["Origin"] != DBNull.Value)
            {
                this.origin = Price.CreateInstance((string)overridedQuotation["Origin"], instrument.NumeratorUnit, instrument.Denominator);
            }
            if (overridedQuotation["Ask"] != DBNull.Value)
            {
                this.ask = Price.CreateInstance((string)overridedQuotation["Ask"], instrument.NumeratorUnit, instrument.Denominator);
            }
            if (overridedQuotation["Bid"] != DBNull.Value)
            {
                this.bid = Price.CreateInstance((string)overridedQuotation["Bid"], instrument.NumeratorUnit, instrument.Denominator);
            }
            if (overridedQuotation["High"] != DBNull.Value)
            {
                this.high = Price.CreateInstance((string)overridedQuotation["High"], instrument.NumeratorUnit, instrument.Denominator);
            }
            if (overridedQuotation["Low"] != DBNull.Value)
            {
                this.low = Price.CreateInstance((string)overridedQuotation["Low"], instrument.NumeratorUnit, instrument.Denominator);
            }

            this.dealerID = (Guid)overridedQuotation["DealerID"];

            if (overridedQuotation.Table.Columns.Contains("Volume") && overridedQuotation["Volume"] != DBNull.Value)
            {
                this.volume = (string)overridedQuotation["Volume"];
            }
            if (overridedQuotation.Table.Columns.Contains("TotalVolume") && overridedQuotation["TotalVolume"] != DBNull.Value)
            {
                this.totalVolume = (string)overridedQuotation["TotalVolume"];
            }
        }

        public OverridedQuotation(Instrument instrument, QuotePolicyDetail quotePolicy)
            : this(instrument, quotePolicy, (OriginQuotation)null)
        {
        }

        //Changed for emperor 2004-12-22
        public OverridedQuotation(Instrument instrument, QuotePolicyDetail quotePolicy, OriginQuotation originQuotation)
        {
            this.modifyState = ModifyState.Added;

            this.quotePolicy = quotePolicy;
            this.instrument = instrument;
            if (instrument.ExchangeSystem == ExchangeSystem.Bursa && originQuotation != null)
            {
                this.timestamp = originQuotation.Timestamp;
            }
            else
            {
                this.timestamp = DateTime.Now;
            }
            this.origin = instrument.LastOrigin;
            this.volume = instrument.LastVolume;
            this.totalVolume = instrument.LastTotalVolume;

            //this.dealerID=instrument.DealerID;
            this.dealerID = (originQuotation == null ? instrument.LastOfferDealerID : Guid.Empty);

            //Changed for emperor 2004-12-22
            if (quotePolicy.PriceType == PriceType.OriginEnable)
            {
                if (originQuotation != null) //Dealer not changed
                {
                    Price bidBase = originQuotation.Bid;
                    Price askBase = originQuotation.Ask;

                    if (instrument.OriginQProcessed != null)
                    {
                        if (bidBase == null) bidBase = instrument.OriginQProcessed.Bid;
                        if (askBase == null) askBase = instrument.OriginQProcessed.Ask;
                    }

                    if (bidBase == null)
                    {
                        bidBase = askBase;
                    }
                    else if (askBase == null)
                    {
                        askBase = bidBase;
                    }
                    else if (askBase < bidBase)
                    {
                        askBase = bidBase;
                    }

                    this.bid = bidBase + quotePolicy.AutoAdjustPoints;
                    this.ask = askBase + quotePolicy.AutoAdjustPoints;

                    this.volume = originQuotation.Volume;
                    this.totalVolume = originQuotation.TotalVolume;
                }
                else //Dealer changed
                {
                    this.bid = this.origin + quotePolicy.AutoAdjustPoints;
                    try
                    {
                        this.ask = this.bid + Math.Abs(instrument.OriginQProcessed.Ask - instrument.OriginQProcessed.Bid);
                    }
                    catch (Exception ex)
                    {
                        Manager.Common.Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "OverridedQuotation.OverridedQuotation Error\r\n{0}", ex.ToString());
                        this.ask = this.bid;
                    }
                }
                this.bid = this.bid - quotePolicy.SpreadPoints;
                this.ask = this.ask + quotePolicy.SpreadPoints;
            }
            else
            {
                this.bid = this.origin + quotePolicy.AutoAdjustPoints;
                this.ask = this.bid + quotePolicy.SpreadPoints;
            }
        }

        internal void CalculateHiLo(bool highBid, bool lowBid)
        {
            QuotePolicyDetail quotePolicy = this.quotePolicy;

            Price high = highBid ? this.bid : this.ask;
            Price low = lowBid ? this.bid : this.ask;

            if (quotePolicy.OverrideHigh == null || quotePolicy.OverrideHigh < high)
            {
                quotePolicy.OverrideHigh = high;
                quotePolicy.HighAsk = this.ask;
                quotePolicy.HighBid = this.bid;
            }
            if (quotePolicy.OverrideLow == null || quotePolicy.OverrideLow > low)
            {
                quotePolicy.OverrideLow = low;
                quotePolicy.LowAsk = this.ask;
                quotePolicy.LowBid = this.bid;
            }

            if (quotePolicy.IsOriginHiLo)
            {
                this.high = instrument.OriginHigh;
                this.low = instrument.OriginLow;
            }
            else
            {
                this.high = quotePolicy.OverrideHigh;
                this.low = quotePolicy.OverrideLow;
            }
        }

        public OverridedQuotation GetChanges(OverridedQuotation oq)
        {
            if (this.high != null && oq.high != null)
            {
                if ((double)this.high == (double)oq.high) this.high = null;
            }
            if (this.low != null && oq.low != null)
            {
                if ((double)this.low == (double)oq.low) this.low = null;
            }

            return this;
        }

        public iExchange.Common.OverridedQuotation ToLightVersion()
        {
            iExchange.Common.OverridedQuotation lightOverridedQ = new iExchange.Common.OverridedQuotation();

            lightOverridedQ.QuotePolicyID = this.quotePolicy.ID;
            lightOverridedQ.InstrumentID = this.instrument.ID;
            lightOverridedQ.Timestamp = this.timestamp;
            lightOverridedQ.Ask = (string)this.ask;
            lightOverridedQ.Bid = (string)this.bid;

            lightOverridedQ.High = (this.high == null) ? string.Empty : (string)this.high;
            lightOverridedQ.Low = (this.low == null) ? string.Empty : (string)this.low;

            lightOverridedQ.Volume = this.volume;
            lightOverridedQ.TotalVolume = this.totalVolume;

            lightOverridedQ.Origin = (this.origin == null) ? string.Empty : this.origin.ToString();
            lightOverridedQ.DealerID = this.dealerID;
            return lightOverridedQ;
        }

        public void ClearItems(bool isDayOpenTime)
        {
            if (isDayOpenTime)
            {
                this.high = null;
                this.low = null;
            }
            this.bid = null;
            this.ask = null;
        }

        public void UpdateHighLowWithHistoryQuotation(OverridedQuotation overridedQ)
        {
            if (this.high < overridedQ.high)
            {
                this.high = overridedQ.high;
                this.modifyState = ModifyState.Modified;
            }
            if (this.low > overridedQ.low)
            {
                this.low = overridedQ.low;
                this.modifyState = ModifyState.Modified;
            }
            //Update quotePolicy.high,low
            if (this.quotePolicy.OverrideHigh == null || (this.quotePolicy.OverrideHigh != null && this.quotePolicy.OverrideHigh < overridedQ.high))
            {
                this.quotePolicy.OverrideHigh = overridedQ.high;
            }
            if (this.quotePolicy.OverrideLow == null || (this.quotePolicy.OverrideLow != null && this.quotePolicy.OverrideLow > overridedQ.low))
            {
                this.quotePolicy.OverrideLow = overridedQ.low;
            }
        }

        public void UpdateHighLow(bool isUpdateHigh, Price highPrice, Price lowPrice)
        {
            //if (isUpdateHigh)
            //{
            if (highPrice != null)
            {
                this.high = highPrice;
                this.modifyState = ModifyState.Modified;
                this.quotePolicy.OverrideHigh = highPrice;
            }
            //}
            //else
            //{
            if (lowPrice != null)
            {
                this.low = lowPrice;
                this.modifyState = ModifyState.Modified;
                this.quotePolicy.OverrideLow = lowPrice;
            }
            //}
        }

        public void UpdateHighLow(OverridedQuotation overridedQ)
        {
            this.high = overridedQ.high;
            this.modifyState = ModifyState.Modified;

            this.low = overridedQ.low;
            this.modifyState = ModifyState.Modified;

            //Update quotePolicy.high,low

            this.quotePolicy.OverrideHigh = overridedQ.high;

            this.quotePolicy.OverrideLow = overridedQ.low;
        }

        public void UpdateHighLow(Price high, Price low)
        {
            this.high = high;
            this.low = low;

            this.quotePolicy.OverrideHigh = high;
            this.quotePolicy.OverrideLow = low;
        }

        public override string ToString()
        {
            return this.instrument.ID.ToString() + QuotationDelimiter.Col2 +
                   this.quotePolicy.ID.ToString() + QuotationDelimiter.Col2 +
                   this.Timestamp.ToString("yyyy-MM-dd HH:mm:ss") + QuotationDelimiter.Col2 +
                   this.Ask + QuotationDelimiter.Col2 + this.Bid + QuotationDelimiter.Col2 +
                   this.High + QuotationDelimiter.Col2 + this.Low +
                   (this.Volume == null ? "" : (QuotationDelimiter.Col2 + this.Volume + QuotationDelimiter.Col2 + this.TotalVolume));
        }
    }
}
