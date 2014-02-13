using Manager.Common.ExchangeEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PriceType = iExchange.Common.PriceType;

namespace Manager.Common.QuotationEntities
{
    public class InstrumentQuotationSet
    {
        public string ExchangeCode;
        public Guid QoutePolicyId;
        public Guid InstrumentId;
        public InstrumentQuotationEditType type;
        public int Value;
    }

    public enum InstrumentQuotationEditType
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
        IsAutoFill,
        IsPriceEnabled,
        IsAutoEnablePrice,
        Resume,
        Suspend,
        OrderTypeMask
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
