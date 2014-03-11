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
        public object Value;
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
        OrderTypeMask,
        High,
        Low,
        AcceptLmtVariation,
        AutoDQMaxLot,
        AlertVariation,
        DqQuoteMinLot,
        MaxDQLot,
        NormalWaitTime,
        AlertWaitTime,
        MaxOtherLot,
        CancelLmtVariation,
        MaxMinAdjust,
        PenetrationPoint,
        PriceValidTime,
        AutoCancelMaxLot,
        AutoAcceptMaxLot,
        HitPriceVariationForSTP,
        AutoDQDelay
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

    public class HistoryQuotationData
    {
        public string ExchangeCode { get; set; }
        public Guid InstrumentId { get; set; }
        public string InstrumentCode { get; set; }
        public string Time { get; set; }
        public string Origin { get; set; }
        public string Status { get; set; }
    }

    public class UpdateHighLowBatchProcessInfo
    {
        public int BatchProcessId { get; set; }
        public bool IsHigh { get; set; }
        public string ExchangeCode { get; set; }
        public Guid InstrumentId { get; set; }
        public string InstrumentCode { get; set; }
        public DateTime UpdateTime { get; set; }
        public string NewInput { get; set; }
        public int StateCode { get; set; }
        public string ErrorMessage { get; set; }

        public string ProcessMessage
        {
            get
            {
                string message = string.Empty;
                if (StateCode == 0)
                {
                    message = string.Format("{0}:{1} Update {2} at {3}. NewInput:{4}(BatchProcessId:{5})", ExchangeCode, InstrumentCode, IsHigh ? "High" : "Low", UpdateTime, NewInput, BatchProcessId);
                }
                return message;
            }
        }
    }
}
