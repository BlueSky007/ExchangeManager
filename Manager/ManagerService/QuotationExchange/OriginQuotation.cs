using iExchange.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace ManagerService.QuotationExchange
{
    public class OriginQuotation : PersistObject, ICloneable
    {
        private Instrument instrument;
        private DateTime timestamp;
        private Price ask;
        private Price bid;
        private Price high;
        private Price low;
        private Price origin;

        private string volume;
        private string totalVolume;

        private bool isProblematic = false;

        #region Common public properties definition
        public Instrument Instrument
        {
            get { return this.instrument; }
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
        public Price Origin
        {
            get { return this.origin; }
        }

        public bool IsProblematic
        {
            get { return this.isProblematic; }
            set { this.isProblematic = value; }
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

        #region PersistObject
        public override void Merge(PersistObject persistObject)
        {
            OriginQuotation oq = (OriginQuotation)persistObject;
            if (this.timestamp >= oq.timestamp) return;

            this.modifyState = ModifyState.Modified;
            this.timestamp = oq.timestamp;
            if (oq.ask != null) { this.ask = oq.ask; }
            if (oq.bid != null) { this.bid = oq.bid; }
            if (oq.high != null) { this.high = oq.high; }
            if (oq.low != null) { this.low = oq.low; }
            this.origin = oq.origin;

            this.volume = oq.volume;
            this.totalVolume = oq.totalVolume;

            return;
        }

        public override string GetUpdateSql(bool resetModifyState)
        {
            StringBuilder stringBuilder = new StringBuilder();
            switch (this.modifyState)
            {
                case ModifyState.Added:
                    stringBuilder.Append("EXEC dbo.P_AddOriginQuotation ");
                    break;
                case ModifyState.Modified:
                    stringBuilder.Append("EXEC dbo.P_UpdateOriginQuotation ");
                    break;
                default:
                    return string.Empty;
            }

            stringBuilder.Append("'").Append(this.instrument.ID.ToString()).Append("',");
            stringBuilder.Append("'").Append(this.timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff")).Append("',");

            OverridedQuotation.AppendPrice(stringBuilder, this.ask);
            stringBuilder.Append(",");

            OverridedQuotation.AppendPrice(stringBuilder, this.bid);
            stringBuilder.Append(",");

            OverridedQuotation.AppendPrice(stringBuilder, this.high);
            stringBuilder.Append(",");

            OverridedQuotation.AppendPrice(stringBuilder, this.low);
            stringBuilder.Append(",");

            OverridedQuotation.AppendString(stringBuilder, this.volume);
            stringBuilder.Append(",");

            OverridedQuotation.AppendString(stringBuilder, this.totalVolume);
            stringBuilder.Append("\n");

            if (resetModifyState) this.modifyState = ModifyState.Unchanged;
            return stringBuilder.ToString();
        }
        #endregion PersistObject
        #region ICloneable
        public virtual object Clone()
        {
            OriginQuotation oq = (OriginQuotation)this.MemberwiseClone();
            oq.ask = (this.ask == null ? null : (Price)this.ask.Clone());
            oq.bid = (this.bid == null ? null : (Price)this.bid.Clone());
            oq.high = (this.high == null ? null : (Price)this.high.Clone());
            oq.low = (this.low == null ? null : (Price)this.low.Clone());
            oq.origin = (this.origin == null ? null : (Price)this.origin.Clone());
            oq.volume = this.volume;
            oq.totalVolume = this.totalVolume;
            oq.isProblematic = this.isProblematic;

            return oq;
        }
        #endregion ICloneable

        public OriginQuotation()
        {
            this.modifyState = ModifyState.Unchanged;
        }

        public OriginQuotation(Instrument instrument, DataRow originQuotation)
        {
            this.modifyState = ModifyState.Unchanged;

            this.instrument = instrument;
            this.timestamp = (DateTime)originQuotation["Timestamp"];
            if (originQuotation["Ask"] != DBNull.Value)
            {
                this.ask = Price.CreateInstance((string)originQuotation["Ask"], instrument.NumeratorUnit, instrument.Denominator);
            }
            if (originQuotation["Bid"] != DBNull.Value)
            {
                this.bid = Price.CreateInstance((string)originQuotation["Bid"], instrument.NumeratorUnit, instrument.Denominator);
            }
            if (originQuotation["High"] != DBNull.Value)
            {
                this.high = Price.CreateInstance((string)originQuotation["High"], instrument.NumeratorUnit, instrument.Denominator);
            }
            if (originQuotation["Low"] != DBNull.Value)
            {
                this.low = Price.CreateInstance((string)originQuotation["Low"], instrument.NumeratorUnit, instrument.Denominator);
            }
            if (originQuotation.Table.Columns.Contains("Volume") && originQuotation["Volume"] != DBNull.Value)
            {
                this.volume = (string)originQuotation["Volume"];
            }
            if (originQuotation.Table.Columns.Contains("TotalVolume") && originQuotation["TotalVolume"] != DBNull.Value)
            {
                this.totalVolume = (string)originQuotation["TotalVolume"];
            }
            this.origin = instrument.CalculateOrigin(this.ask, this.bid, false);
        }

        public OriginQuotation(Instrument instrument, CollectorQuotation cq)
        {
            this.modifyState = ModifyState.Added;
            this.instrument = instrument;
            this.timestamp = cq.Timestamp;
            this.ask = Price.CreateInstance(cq.Ask, instrument.NumeratorUnit, instrument.Denominator);
            this.bid = Price.CreateInstance(cq.Bid, instrument.NumeratorUnit, instrument.Denominator);

            this.high = Price.CreateInstance(cq.High, instrument.NumeratorUnit, instrument.Denominator);
            this.low = Price.CreateInstance(cq.Low, instrument.NumeratorUnit, instrument.Denominator);

            this.volume = cq.Volume;
            this.totalVolume = cq.TotalVolume;

            this.origin = instrument.CalculateOrigin(this.ask, this.bid, instrument.OriginQReceived != null);

            //Special handle, it's not so strict  
            //NOTE: Has problem for session clear !
            if (this.origin == null && instrument.OriginQReceived != null)
            {
                if (this.ask == null) this.ask = this.instrument.OriginQReceived.ask;
                if (this.bid == null) this.bid = this.instrument.OriginQReceived.bid;
                this.origin = instrument.CalculateOrigin(this.ask, this.bid, false);
            }

            this.FilterErrorHighLow(instrument, false);
        }

        public void FilterErrorHighLow(Instrument instrument, bool useLast)
        {
            if (useLast)
            {
                if (this.high == instrument.ErrorOriginHigh) this.high = instrument.OriginHigh;
                if (this.low == instrument.ErrorOriginLow) this.low = instrument.OriginLow;
            }
            else
            {
                if (this.high == instrument.ErrorOriginHigh) this.high = null;
                if (this.low == instrument.ErrorOriginLow) this.low = null;
            }
        }

        public bool IsEmpty()
        {
            return (this.origin == null || (this.ask == null && this.bid == null && this.high == null && this.low == null));
        }

        public void Merge(DateTime time, Price lastOrigin, Price originHigh, Price originLow)
        {
            //Note: there should be more accurate
            if (((TimeSpan)this.timestamp.Subtract(time)).TotalSeconds >= 1) return;

            this.modifyState = ModifyState.Modified;
            this.timestamp = time;
            if (lastOrigin != null) { this.origin = lastOrigin; }
            if (originHigh != null) { this.high = originHigh; }
            if (originLow != null) { this.low = originLow; }

            return;
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

        public void Rebuild(int numeratorUnit, int denominator)
        {
            if (this.ask != null) { this.ask = Price.CreateInstance((double)this.ask, numeratorUnit, denominator); }
            if (this.bid != null) { this.bid = Price.CreateInstance((double)this.bid, numeratorUnit, denominator); }
            if (this.high != null) { this.high = Price.CreateInstance((double)this.high, numeratorUnit, denominator); }
            if (this.low != null) { this.low = Price.CreateInstance((double)this.low, numeratorUnit, denominator); }
            if (this.origin != null) { this.origin = Price.CreateInstance((double)this.origin, numeratorUnit, denominator); }
        }

        public iExchange.Common.OriginQuotation ToLightVersion()
        {
            iExchange.Common.OriginQuotation lightOriginQ = new iExchange.Common.OriginQuotation();

            lightOriginQ.HasWatchOnlyQuotePolicies = this.instrument.HasWatchOnlyQuotePolicies;
            lightOriginQ.InstrumentID = this.instrument.ID;
            lightOriginQ.Timestamp = this.timestamp;
            lightOriginQ.Ask = (string)this.ask;
            lightOriginQ.Bid = (string)this.bid;
            lightOriginQ.High = (string)this.high;
            lightOriginQ.Low = (string)this.low;
            lightOriginQ.Origin = (string)this.origin;
            lightOriginQ.IsProblematic = this.IsProblematic;
            lightOriginQ.Volume = this.volume;
            lightOriginQ.TotalVolume = this.totalVolume;

            return lightOriginQ;
        }

        public override string ToString()
        {
            return string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8}", this.instrument.ID, this.timestamp, this.ask, this.bid, this.high, this.low, this.origin, this.volume, this.totalVolume);
        }
    }
}
