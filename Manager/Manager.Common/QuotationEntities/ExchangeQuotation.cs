using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manager.Common.QuotationEntities
{
    public class ExchangeQuotation
    {
        public Guid QuotationPolicyId;
        public string QuotationPolicyCode;
        public Guid InstruemtnId;

        public string InstrumentCode;
        public string OriginInstrumentCode;

        public string Bid;

        public string Ask;

        public string High;

        public string Low;

        public string Origin;

        public DateTime Timestamp;
        //public string TotalVolume { get; set; }
        //public string Volume { get; set; }

        public bool IsOriginHiLo;

        public PriceType PriceType;

        public int AutoAdjustPoints1;

        public int AutoAdjustPoints2;

        public int AutoAdjustPoints3;

        public int AutoAdjustPoints4;

        public int SpreadPoints1;

        public int SpreadPoints2;

        public int SpreadPoints3;

        public int SpreadPoints4;

        public int MaxAuotAdjustPoints;

        public int MaxSpreadPoints;

        public ExchangeQuotation()
        {
        }

        public ExchangeQuotation(Manager.Common.Settings.QuotePolicyDetail detail)
        {
            this.QuotationPolicyId = detail.QuotePolicyId;
            this.InstruemtnId = detail.InstrumentId;
            this.PriceType = detail.PriceType;
            this.AutoAdjustPoints1 = detail.AutoAdjustPoints;
            this.AutoAdjustPoints2 = int.Parse(detail.AutoAdjustPoints2);
            this.AutoAdjustPoints3 = int.Parse(detail.AutoAdjustPoints3);
            this.AutoAdjustPoints4 = int.Parse(detail.AutoAdjustPoints4);
            this.SpreadPoints1 = detail.SpreadPoints;
            this.SpreadPoints2 = int.Parse(detail.SpreadPoints2);
            this.SpreadPoints3 = int.Parse(detail.SpreadPoints3);
            this.SpreadPoints4 = int.Parse(detail.SpreadPoints4);
            this.MaxAuotAdjustPoints = detail.MaxAutoAdjustPoints;
            this.MaxSpreadPoints = detail.MaxSpreadPoints;
            this.IsOriginHiLo = detail.IsOriginHiLo;
        }
    }

    public class QuotePolicyDetailSet
    {
        public string ExchangeCode;
        public Guid QoutePolicyId;
        public Guid InstrumentId;
        public QuotePolicyEditType type;
        public int Value;
    }

    public enum QuotePolicyEditType
    {
        PriceType,
        AutoAdjustPoints,
        AutoAdjustPoints2,
        AutoAdjustPoints3,
        AutoAdjustPoints4,
        SpreadPoints,
        SpreadPoints2,
        SpreadPoints3,
        SpreadPoints4,
        MaxAuotAutoAdjustPointsPoints,
        MaxSpreadPointsPoints,
        IsOriginHiLo,
    }

    public class QuotePolicyRelation
    {
        public Guid RelationId;
        public string RelationCode;
        public List<int> instrumentIds;

        public QuotePolicyRelation()
        {
            instrumentIds = new List<int>();
        }
    }
}
