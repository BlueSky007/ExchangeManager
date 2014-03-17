using iExchange.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml;

namespace ManagerService.QuotationExchange
{
    public class QuotePolicyDetail : IDisposable
    {
        private Guid id;
        private Instrument instrument;
        private PriceType priceType;
        private int autoAdjustPoints;
        private int spreadPoints;
        private bool isOriginHiLo;
        private Price overrideHigh;
        private Price overrideLow;

        #region Common public properties definition
        public Guid ID
        {
            get { return this.id; }
        }
        public Instrument Instrument
        {
            get { return this.instrument; }
        }
        public PriceType PriceType
        {
            get { return this.priceType; }
        }
        public int AutoAdjustPoints
        {
            get { return this.autoAdjustPoints; }
        }
        public int SpreadPoints
        {
            get { return this.spreadPoints; }
        }
        public bool IsOriginHiLo
        {
            get { return this.isOriginHiLo; }
        }
        public Price OverrideHigh
        {
            get { return this.overrideHigh; }
            set { this.overrideHigh = value; }
        }
        public Price OverrideLow
        {
            get { return this.overrideLow; }
            set { this.overrideLow = value; }
        }
        public Price HighAsk
        {
            get;
            set;
        }
        public Price HighBid
        {
            get;
            set;
        }
        public Price LowAsk
        {
            get;
            set;
        }
        public Price LowBid
        {
            get;
            set;
        }
        public string QuotePolicyCode
        {
            get;
            private set;
        }
        #endregion Common public properties definition

        public QuotePolicyDetail(Instrument instrument, XmlNode quotePolicy)
        {
            this.id = XmlConvert.ToGuid(quotePolicy.Attributes["QuotePolicyID"].Value);
            this.priceType = (PriceType)XmlConvert.ToByte(quotePolicy.Attributes["PriceType"].Value);
            this.autoAdjustPoints = XmlConvert.ToInt32(quotePolicy.Attributes["AutoAdjustPoints"].Value);
            this.spreadPoints = XmlConvert.ToInt32(quotePolicy.Attributes["SpreadPoints"].Value);
            this.isOriginHiLo = XmlConvert.ToBoolean(quotePolicy.Attributes["IsOriginHiLo"].Value);
            this.overrideHigh = null;
            this.overrideLow = null;

            this.SetCrossRef(instrument);
        }

        public QuotePolicyDetail(Instrument instrument, DataRow quotePolicy)
        {
            this.id = (Guid)quotePolicy["QuotePolicyID"];
            this.priceType = (PriceType)(byte)quotePolicy["PriceType"];
            this.autoAdjustPoints = (int)quotePolicy["AutoAdjustPoints"];
            this.spreadPoints = (int)quotePolicy["SpreadPoints"];
            this.isOriginHiLo = (bool)quotePolicy["IsOriginHiLo"];
            this.overrideHigh = null;
            this.overrideLow = null;

            this.SetCrossRef(instrument);
        }

        public bool Update(XmlNode quotePolicy)
        {
            return this.Update(quotePolicy, this.instrument);
        }
        public bool Update(XmlNode quotePolicy, Instrument instrument)
        {
            this.Dispose();

            foreach (XmlAttribute attribute in quotePolicy.Attributes)
            {
                switch (attribute.Name)
                {
                    case "PriceType":
                        this.priceType = (PriceType)XmlConvert.ToInt16(attribute.Value);
                        break;
                    case "AutoAdjustPoints":
                        this.autoAdjustPoints = XmlConvert.ToInt32(attribute.Value);
                        break;
                    case "SpreadPoints":
                        this.spreadPoints = XmlConvert.ToInt32(attribute.Value);
                        break;
                    case "IsOriginHiLo":
                        this.isOriginHiLo = XmlConvert.ToBoolean(attribute.Value);
                        break;
                }
            }

            this.SetCrossRef(instrument);
            return true;
        }

        public void Dispose()
        {
            this.instrument.QuotePolicyDetails.Remove(this.id);
        }

        private void SetCrossRef(Instrument instrument)
        {
            this.instrument = instrument;

            this.instrument.QuotePolicyDetails.Add(this);
        }

    }
}
