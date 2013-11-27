using iExchange.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml;

namespace ManagerService.QuotationExchange
{
    public enum PriceConvertFomulaType
    {
        None = 0,
        Multiply = 1,
        Divide = 2,
    }

    public class OriginInstrument
    {
        private string originCode;
        private bool isTrading;

        private PriceConvertFomulaType priceConvertFomulaType;
        private decimal priceConvertFactor;
        private string priceOriginCode1;
        private string priceOriginCode2;

        private CollectorQuotation lastQuotation;
        private List<Instrument> instruments;

        public OriginInstrument(XmlNode instrument)
        {
            this.originCode = instrument.Attributes["OriginCode"].Value;
            this.isTrading = false;
            this.instruments = new List<Instrument>();

            this.priceConvertFomulaType = (PriceConvertFomulaType)XmlConvert.ToInt32(instrument.Attributes["PriceConvertFomulaType"].Value);
            if (this.priceConvertFomulaType != PriceConvertFomulaType.None)
            {
                this.priceConvertFactor = XmlConvert.ToDecimal(instrument.Attributes["PriceConvertFactor"].Value);
                this.priceOriginCode1 = instrument.Attributes["PriceOriginCode1"].Value;
                this.priceOriginCode2 = instrument.Attributes["PriceOriginCode2"].Value;
            }
        }

        public OriginInstrument(DataRow instrument)
        {
            this.originCode = (string)instrument["OriginCode"];
            this.isTrading = false;
            this.instruments = new List<Instrument>();

            this.priceConvertFomulaType = (PriceConvertFomulaType)(int)instrument["PriceConvertFomulaType"];
            if (this.priceConvertFomulaType != PriceConvertFomulaType.None)
            {
                this.priceConvertFactor = (decimal)instrument["PriceConvertFactor"];
                this.priceOriginCode1 = (string)instrument["PriceOriginCode1"];
                this.priceOriginCode2 = (string)instrument["PriceOriginCode2"];
            }
        }

        public bool Update(XmlNode instrument)
        {
            foreach (XmlAttribute attribute in instrument.Attributes)
            {
                switch (attribute.Name)
                {
                    case "OriginCode":
                        this.originCode = attribute.Value;
                        break;
                    case "PriceConvertFomulaType":
                        this.priceConvertFomulaType = (PriceConvertFomulaType)XmlConvert.ToInt32(attribute.Value);
                        break;
                    case "PriceConvertFactor":
                        if (this.priceConvertFomulaType != PriceConvertFomulaType.None)
                        {
                            this.priceConvertFactor = XmlConvert.ToDecimal(attribute.Value);
                        }
                        break;
                    case "PriceOriginCode1":
                        this.priceOriginCode1 = attribute.Value;
                        break;
                    case "PriceOriginCode2":
                        this.priceOriginCode2 = attribute.Value;
                        break;
                }
            }

            return true;
        }


        public string OriginCode
        {
            get { return originCode; }
            set { originCode = value; }
        }

        public bool IsTrading
        {
            get { return this.isTrading; }
            set { this.isTrading = value; }
        }

        public List<Instrument> Instruments
        {
            get { return this.instruments; }
        }

        public bool HasPriceConverter
        {
            get { return this.priceConvertFomulaType != PriceConvertFomulaType.None; }
        }

        public CollectorQuotation LastQuotation
        {
            get { return this.lastQuotation; }
            set { this.lastQuotation = value; }
        }

        public string PriceOriginCode1
        {
            get { return this.priceOriginCode1; }
        }

        public string PriceOriginCode2
        {
            get { return this.priceOriginCode2; }
        }

        public CollectorQuotation ConvertQuotation(CollectorQuotation collectorQuotation1, CollectorQuotation collectorQuotation2)
        {
            if (collectorQuotation1 == null || collectorQuotation2 == null) return null;

            string bid = null, ask = null, high = null, low = null;

            switch (this.priceConvertFomulaType)
            {
                case PriceConvertFomulaType.Multiply:
                    bid = OriginInstrument.Multiply(collectorQuotation1.Bid, collectorQuotation2.Bid, this.priceConvertFactor);
                    ask = OriginInstrument.Multiply(collectorQuotation1.Ask, collectorQuotation2.Ask, this.priceConvertFactor);
                    high = OriginInstrument.Multiply(collectorQuotation1.High, collectorQuotation2.High, this.priceConvertFactor);
                    low = OriginInstrument.Multiply(collectorQuotation1.Low, collectorQuotation2.Low, this.priceConvertFactor);
                    break;
                case PriceConvertFomulaType.Divide:
                    bid = OriginInstrument.Divide(collectorQuotation1.Bid, collectorQuotation2.Ask, this.priceConvertFactor);
                    ask = OriginInstrument.Divide(collectorQuotation1.Ask, collectorQuotation2.Bid, this.priceConvertFactor);
                    high = OriginInstrument.Divide(collectorQuotation1.High, collectorQuotation2.Low, this.priceConvertFactor);
                    low = OriginInstrument.Divide(collectorQuotation1.Low, collectorQuotation2.High, this.priceConvertFactor);
                    break;
            }

            if (bid == null && ask == null && high == null && low == null)
            {
                return null;
            }
            else
            {
                DateTime timestamp = collectorQuotation1.Timestamp > collectorQuotation2.Timestamp ? collectorQuotation1.Timestamp : collectorQuotation2.Timestamp;
                return new CollectorQuotation(this.originCode, timestamp, ask, bid, high, low);
            }
        }

        private static string Multiply(string price1, string price2, decimal priceConvertFactor)
        {
            try
            {
                decimal price = System.Convert.ToDecimal(price1) * System.Convert.ToDecimal(price2) * priceConvertFactor;
                return price.ToString();
            }
            catch (Exception exception)
            {
                return null;
            }
        }

        private static string Divide(string price1, string price2, decimal priceConvertFactor)
        {
            try
            {
                decimal price = System.Convert.ToDecimal(price1) / System.Convert.ToDecimal(price2) * priceConvertFactor;
                return price.ToString();
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Summary description for Instrument.
    /// </summary>
    public class Instrument : IDisposable
    {
        private Guid id;
        private string originCode;
        private string code;
        private int numeratorUnit;
        private int denominator;
        private bool isSinglePrice;
        private OriginType originType;
        private int alertVariation;
        private int normalWaitTime;
        private int alertWaitTime;

        private OriginQuotation originQReceived;
        private OriginQuotation originQProcessed;
        private ArrayList quotePolicyDetails;
        private Guid lastOfferDealerID;
        private List<Guid> dealers = new List<Guid>();
        private bool isActive;
        private bool isTrading;
        private string scheduleID;

        private int originInactiveTime;
        private bool isPriceEnabled;
        private bool isAutoEnablePrice;
        private ExchangeSystem exchangeSystem;
        private DateTime lastPriceEnabledTime;

        //Note: the following three varaible are members of the last origin who has been used to generate overrided quotation. 
        private Price lastOrigin, originHigh, originLow;
        private string lastVolume, lastTotalVolume;

        //Note: used to filter the error origin High, low
        private Price errorOriginHigh, errorOriginLow;

        private DateTime dayOpenTime = DateTime.Now;
        private DateTime dayCloseTime = DateTime.Now;

        #region Properties
        public Guid ID
        {
            get { return this.id; }
        }
        public string OriginCode
        {
            get { return this.originCode; }
        }
        public string Code
        {
            get { return code; }
        }
        public int NumeratorUnit
        {
            get { return this.numeratorUnit; }
        }
        public int Denominator
        {
            get { return this.denominator; }
        }

        //BEGIN Added for emperor 2004-12-22
        public OriginType OriginType
        {
            get { return this.originType; }
        }
        //END Added for emperor 2004-12-22

        //The last received OriginQuotation
        public OriginQuotation OriginQReceived
        {
            get { return this.originQReceived; }
            set { this.originQReceived = value; }
        }

        //The last processed OriginQuotation
        public OriginQuotation OriginQProcessed
        {
            get { return this.originQProcessed; }
            set { this.originQProcessed = value; }
        }

        public ArrayList QuotePolicyDetails
        {
            get { return this.quotePolicyDetails; }
        }

        public bool HasWatchOnlyQuotePolicies
        {
            get
            {
                foreach (QuotePolicyDetail quotePolicy in this.quotePolicyDetails)
                {
                    if (quotePolicy.PriceType == PriceType.WatchOnly) return true;
                }
                return false;
            }
        }

        public Guid LastOfferDealerID
        {
            get { return this.lastOfferDealerID; }
            set { this.lastOfferDealerID = value; }
        }
        public bool IsTrading
        {
            get { return this.isTrading && this.isActive; }
            set { this.isTrading = value; }
        }
        public string ScheduleID
        {
            get { return this.scheduleID; }
            set { this.scheduleID = value; }
        }
        public Price LastOrigin
        {
            get { return this.lastOrigin; }
            internal set { this.lastOrigin = value; }
        }
        public string LastVolume
        {
            get { return this.lastVolume; }
        }
        public string LastTotalVolume
        {
            get { return this.lastTotalVolume; }
        }

        public Price OriginHigh
        {
            get { return this.originHigh; }
        }
        public Price OriginLow
        {
            get { return this.originLow; }
        }

        public Price ErrorOriginHigh
        {
            get { return this.errorOriginHigh; }
        }
        public Price ErrorOriginLow
        {
            get { return this.errorOriginLow; }
        }

        public ExchangeSystem ExchangeSystem
        {
            get { return this.exchangeSystem; }
        }

        public int CountOfDealer
        {
            get { return this.dealers.Count; }
        }

        public DateTime DayOpenTime
        {
            get { return this.dayOpenTime; }
            set { this.dayOpenTime = value; }
        }

        public DateTime DayCloseTime
        {
            get { return this.dayCloseTime; }
            set { this.dayCloseTime = value; }
        }

        #endregion Properties
        #region IDisposable
        public void Dispose()
        {
            QuotePolicyDetail[] quotePolicys = new QuotePolicyDetail[this.quotePolicyDetails.Count];
            this.quotePolicyDetails.CopyTo(quotePolicys);

            //Remove all cross references
            foreach (QuotePolicyDetail quotePolicy in quotePolicys)
            {
                quotePolicy.Dispose();
            }
        }
        #endregion IDisposable

        public Instrument(XmlNode instrument)
        {
            this.id = XmlConvert.ToGuid(instrument.Attributes["ID"].Value);
            this.originCode = instrument.Attributes["OriginCode"].Value;
            this.code = instrument.Attributes["Code"].Value;
            this.numeratorUnit = XmlConvert.ToInt32(instrument.Attributes["NumeratorUnit"].Value);
            this.denominator = XmlConvert.ToInt32(instrument.Attributes["Denominator"].Value);
            this.isSinglePrice = XmlConvert.ToBoolean(instrument.Attributes["IsSinglePrice"].Value);
            this.originType = (OriginType)XmlConvert.ToByte(instrument.Attributes["OriginType"].Value);
            this.alertVariation = XmlConvert.ToInt32(instrument.Attributes["AlertVariation"].Value);
            this.normalWaitTime = XmlConvert.ToInt32(instrument.Attributes["NormalWaitTime"].Value);
            this.alertWaitTime = XmlConvert.ToInt32(instrument.Attributes["AlertWaitTime"].Value);
            this.isActive = XmlConvert.ToBoolean(instrument.Attributes["IsActive"].Value);

            this.originInactiveTime = XmlConvert.ToInt32(instrument.Attributes["OriginInactiveTime"].Value);
            this.isPriceEnabled = XmlConvert.ToBoolean(instrument.Attributes["IsPriceEnabled"].Value);
            this.isAutoEnablePrice = XmlConvert.ToBoolean(instrument.Attributes["IsAutoEnablePrice"].Value);
            if (instrument.Attributes["ExternalExchangeCode"] != null)
            {
                this.exchangeSystem = (ExchangeSystem)Enum.Parse(typeof(ExchangeSystem), instrument.Attributes["ExternalExchangeCode"].Value);
            }
            this.lastPriceEnabledTime = this.isPriceEnabled ? DateTime.Now : DateTime.MinValue;

            this.originQReceived = null;
            this.originQProcessed = null;
            this.quotePolicyDetails = new ArrayList();
            this.isTrading = false;
            this.scheduleID = null;
            this.lastOrigin = null;
        }

        public Instrument(DataRow instrumentRow)
        {
            this.id = (Guid)instrumentRow["ID"];
            this.code = (string)instrumentRow["Code"];
            this.originCode = (string)instrumentRow["OriginCode"];
            this.numeratorUnit = (int)instrumentRow["NumeratorUnit"];
            this.denominator = (int)instrumentRow["Denominator"];
            this.isSinglePrice = (bool)instrumentRow["IsSinglePrice"];
            this.originType = (OriginType)(byte)instrumentRow["OriginType"];
            this.alertVariation = (int)instrumentRow["AlertVariation"];
            this.normalWaitTime = (int)instrumentRow["NormalWaitTime"];
            this.alertWaitTime = (int)instrumentRow["AlertWaitTime"];
            this.isActive = (bool)instrumentRow["IsActive"];

            this.originInactiveTime = (int)instrumentRow["OriginInactiveTime"];
            this.isPriceEnabled = (bool)instrumentRow["IsPriceEnabled"];
            this.isAutoEnablePrice = (bool)instrumentRow["IsAutoEnablePrice"];
            if (instrumentRow["ExternalExchangeCode"] != DBNull.Value)
            {
                this.exchangeSystem = (ExchangeSystem)Enum.Parse(typeof(ExchangeSystem), (string)(instrumentRow["ExternalExchangeCode"]), true);
            }
            else
            {
                this.exchangeSystem = ExchangeSystem.Local;
            }
            this.lastPriceEnabledTime = this.isPriceEnabled ? DateTime.Now : DateTime.MinValue;

            this.originQReceived = null;
            this.originQProcessed = null;
            this.quotePolicyDetails = new ArrayList();
            this.isTrading = false;
            this.scheduleID = null;
            this.lastOrigin = null;
        }

        public void AddDealer(Guid dealerId)
        {
            if (!this.dealers.Contains(dealerId)) this.dealers.Add(dealerId);
        }

        public void RemoveDealer(Guid dealerId)
        {
            this.dealers.Remove(dealerId);
        }

        public bool TryChangePriceActive(DateTime baseTime)
        {
            bool isTimeout = baseTime.Subtract(this.lastPriceEnabledTime).TotalSeconds >= this.originInactiveTime;
            //AppDebug.LogEvent("QuotationServer.TryChangePriceActive", string.Format("{0}, baseTime={1}, lastPriceEnabledTime={2}, originInactiveTime={3}", this.id, baseTime, this.lastPriceEnabledTime, this.originInactiveTime), EventLogEntryType.Information);

            if (isTimeout)
            {
                if (this.isPriceEnabled)
                {
                    this.isPriceEnabled = false;

                    return true;
                }
            }
            else
            {
                if (this.isAutoEnablePrice && !this.isPriceEnabled)
                {
                    this.isPriceEnabled = true;
                    this.lastPriceEnabledTime = baseTime; //Why you delete this line?
                    return true;
                }
            }

            return false;
        }

        public string GetUpdateSql()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("EXEC dbo.QuotationServer_Instrument_Update ");
            stringBuilder.Append("'").Append(this.ID.ToString()).Append("',");
            stringBuilder.Append(this.isPriceEnabled ? 1 : 0);
            stringBuilder.Append("\n");

            return stringBuilder.ToString();
        }

        public XmlNode GetUpdateNode(XmlDocument xmlDoc)
        {
            XmlElement instrumentNode = xmlDoc.CreateElement("Instrument");
            instrumentNode.SetAttribute("ID", XmlConvert.ToString(this.id));
            instrumentNode.SetAttribute("IsPriceEnabled", XmlConvert.ToString(this.isPriceEnabled));

            return instrumentNode;
        }

        public Price CalculateOrigin(Price ask, Price bid, bool suppressLog)
        {
            Price origin = null;
            if (this.isSinglePrice)
            {
                origin = (ask == null ? bid : ask);
            }
            else
            {
                switch (this.originType)
                {
                    case OriginType.Ask:
                        origin = ask;
                        break;
                    case OriginType.Bid:
                        origin = bid;
                        break;
                    case OriginType.Avg:
                        if (ask != null && bid != null)
                        {
                            origin = Price.Avg(ask, bid);
                        }
                        else if (!suppressLog)
                        {
                            AppDebug.LogEvent("QuotationServer", string.Format("Instrument.CalculateOrigin: OriginType.Avg, Instrument == {0}, ask == {1},bid == {2}", this.id, (ask == null ? "null" : (string)ask), (bid == null ? "null" : (string)bid)), EventLogEntryType.Warning);
                        }

                        break;
                }
            }
            return origin;
        }

        public bool IsProblematic(Price origin)
        {
            return ((origin == null || this.lastOrigin == null) ? false : Math.Abs(origin - this.lastOrigin) >= this.alertVariation);
        }

        public int GetWaitTime(Price origin)
        {
            return (IsProblematic(origin) ? this.alertWaitTime : this.normalWaitTime);
        }

        public void RefreshOrigins(DateTime time, Price lastOrigin, string volume, string totalVolume, Price originHigh, Price originLow, bool updateLastOriginQuotation)
        {
            if (lastOrigin != null)
            {
                //this.lastPriceEnabledTime = time;
                //this.lastPriceEnabledTime = DateTime.Now;
                if (updateLastOriginQuotation || !this.IsProblematic(lastOrigin)) this.lastPriceEnabledTime = DateTime.Now;
                this.lastOrigin = lastOrigin;

                this.lastVolume = volume;
                this.lastTotalVolume = totalVolume;
            }

            if (originHigh != null) this.originHigh = originHigh;
            if (originLow != null) this.originLow = originLow;

            if (updateLastOriginQuotation && this.originQReceived != null) this.originQReceived.Merge(time, lastOrigin, originHigh, originLow);
        }

        public void UpdateErrorHighLow(Price originHigh, Price originLow)
        {
            if (this.originHigh == originHigh)
            {
                this.errorOriginHigh = null;
            }
            else if (this.originQProcessed != null)
            {
                this.errorOriginHigh = this.originQProcessed.High;
            }

            if (this.originLow == originLow)
            {
                this.errorOriginLow = null;
            }
            else if (this.originQProcessed != null)
            {
                this.errorOriginLow = this.originQProcessed.Low;
            }
        }

        public bool Update(XmlNode instrument)
        {
            bool priceMustRebuild = false;
            foreach (XmlAttribute attribute in instrument.Attributes)
            {
                switch (attribute.Name)
                {
                    case "OriginCode":
                        this.originCode = attribute.Value;
                        break;
                    case "Code":
                        this.code = attribute.Value;
                        break;
                    case "NumeratorUnit":
                        this.numeratorUnit = XmlConvert.ToInt32(attribute.Value);
                        priceMustRebuild = true;
                        break;
                    case "Denominator":
                        this.denominator = XmlConvert.ToInt32(attribute.Value);
                        priceMustRebuild = true;
                        break;
                    case "IsSinglePrice":
                        this.isSinglePrice = XmlConvert.ToBoolean(attribute.Value);
                        break;
                    case "OriginType":
                        this.originType = (OriginType)XmlConvert.ToInt32(attribute.Value);
                        break;
                    case "AlertVariation":
                        this.alertVariation = XmlConvert.ToInt32(attribute.Value);
                        break;
                    case "NormalWaitTime":
                        this.ClearScheduleIDIfNeed(attribute.Value);
                        this.normalWaitTime = XmlConvert.ToInt32(attribute.Value);
                        break;
                    case "AlertWaitTime":
                        this.alertWaitTime = XmlConvert.ToInt32(attribute.Value);
                        break;
                    case "IsActive":
                        this.isActive = XmlConvert.ToBoolean(attribute.Value);
                        break;

                    case "OriginInactiveTime":
                        this.originInactiveTime = XmlConvert.ToInt32(attribute.Value);
                        break;
                    case "IsPriceEnabled":
                        this.isPriceEnabled = XmlConvert.ToBoolean(attribute.Value);
                        if (this.isPriceEnabled)
                        {
                            this.lastPriceEnabledTime = DateTime.Now;
                        }
                        else
                        {
                            this.lastPriceEnabledTime = DateTime.MinValue;
                        }
                        break;
                    case "IsAutoEnablePrice":
                        this.isAutoEnablePrice = XmlConvert.ToBoolean(attribute.Value);
                        break;
                }
            }

            if (priceMustRebuild)
            {
                if (this.lastOrigin != null) this.lastOrigin = Price.CreateInstance((double)this.lastOrigin, this.numeratorUnit, this.denominator);
                this.RebuildQuotation();
            }
            return true;
        }

        internal bool ChangePriceEnable(bool value)
        {
            if (this.isPriceEnabled != value)
            {
                this.isPriceEnabled = value;
                if (this.isPriceEnabled)
                {
                    this.lastPriceEnabledTime = DateTime.Now;
                }
                else
                {
                    this.lastPriceEnabledTime = DateTime.MinValue;
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        private void RebuildQuotation()
        {
            if (this.originQReceived != null) this.originQReceived.Rebuild(this.numeratorUnit, this.denominator);
            if (this.originQProcessed != null) this.originQProcessed.Rebuild(this.numeratorUnit, this.denominator);
            if (this.lastOrigin != null) this.lastOrigin = Price.CreateInstance((double)this.lastOrigin, this.numeratorUnit, this.denominator);
            if (this.originHigh != null) this.originHigh = Price.CreateInstance((double)this.originHigh, this.numeratorUnit, this.denominator);
            if (this.originLow != null) this.originLow = Price.CreateInstance((double)this.originLow, this.numeratorUnit, this.denominator);
            if (this.errorOriginHigh != null) this.errorOriginHigh = Price.CreateInstance((double)this.errorOriginHigh, this.numeratorUnit, this.denominator);
            if (this.errorOriginLow != null) this.errorOriginLow = Price.CreateInstance((double)this.errorOriginLow, this.numeratorUnit, this.denominator);

            foreach (QuotePolicyDetail quotePolicy in this.quotePolicyDetails)
            {
                if (quotePolicy.OverrideHigh != null) quotePolicy.OverrideHigh = Price.CreateInstance((double)quotePolicy.OverrideHigh, this.numeratorUnit, this.denominator);
                if (quotePolicy.OverrideLow != null) quotePolicy.OverrideLow = Price.CreateInstance((double)quotePolicy.OverrideLow, this.numeratorUnit, this.denominator);
                if (quotePolicy.HighAsk != null) quotePolicy.HighAsk = Price.CreateInstance((double)quotePolicy.HighAsk, this.numeratorUnit, this.denominator);
                if (quotePolicy.HighBid != null) quotePolicy.HighBid = Price.CreateInstance((double)quotePolicy.HighBid, this.numeratorUnit, this.denominator);
                if (quotePolicy.LowAsk != null) quotePolicy.LowAsk = Price.CreateInstance((double)quotePolicy.LowAsk, this.numeratorUnit, this.denominator);
                if (quotePolicy.LowBid != null) quotePolicy.LowBid = Price.CreateInstance((double)quotePolicy.LowBid, this.numeratorUnit, this.denominator);
            }
        }

        //add by Korn 2008-10-16 When dealer set normalwaitTime=0,should remove instrument.Schedule
        private void ClearScheduleIDIfNeed(string normalWaitTime)
        {
            int newNormalWaitTime = XmlConvert.ToInt32(normalWaitTime);
            if (newNormalWaitTime == 0 && this.normalWaitTime != newNormalWaitTime)
            {
                this.scheduleID = null;
            }
        }
    }
}
