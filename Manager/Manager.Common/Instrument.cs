using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manager.Common
{
    public class Instrument
    {
        public Guid Id { get; set; }
        public string OriginCode { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public int? Denominator { get; set; }
        public int? NumeratorUnit { get; set; }
        public InstrumentCategory? Category { get; set; }
        public DateTime? BeginTime { get; set; }
        public DateTime? EndTime { get; set; }
        public byte? CommissionFormula { get; set; }
        public byte? TradePLFormula { get; set; }
        public Guid? SummaryGroupId { get; set; }
        public string SummaryGroupCode { get; set; }
        public decimal SummaryUnit { get; set; }
        public decimal SummaryQuantity { get; set; }
        public int AllowedNewTradeSides { get; set; }
        public bool IsActive { get; set; }
        public bool IsSinglePrice { get; set; }
        public bool IsNormal { get; set; }
        public PriceType PriceType { get; set; }
        public OriginType OriginType { get; set; }
        public int AllowedSpotTradeOrderSides { get; set; }
        public int OriginInactiveTime { get; set; }
        public int AlertVariation { get; set; }
        public int NormalWaitTime { get; set; }
        public int AlertWaitTime { get; set; }
        public decimal MaxDQLot { get; set; }
        public decimal MaxOtherLot { get; set; }
        public decimal? DqQuoteMinLot{ get; set; }
        public int OrderTypeMask { get; set; }
        public string PreviousClosePrice { get; set; }
        public decimal AutoCancelMaxLot { get; set; }
        public decimal AutoAcceptMaxLot { get; set; }
        public decimal AutoDQMaxLot { get; set; }
        public decimal AutoLmtMktMaxLot { get; set; }
        public int? AcceptDQVariation { get; set; }
        public int? AcceptLmtVariation { get; set; }
        public int? AcceptCloseLmtVariation { get; set; }
        public int? AcceptIfDoneVariation { get; set; }
        public int? CancelLmtVariation { get; set; }
        public int MaxMinAdjust { get; set; }
        public bool IsBetterPrice { get; set; }
        public int HitTimes { get; set; }
        public int PenetrationPoint { get; set; }
        public int PriceValidTime { get; set; }
        public int DailyMaxMove { get; set; }
        public TimeSpan LastAcceptTimeSpan { get; set; }
        public DateTime? NextDayOpenTime { get; set; }
        public DateTime? DayOpenTime { get; set; }
        public DateTime? DayCloseTime { get; set; }
        public DateTime? LastDayCloseTime { get; set; }
        public DateTime? LastTradeDay { get; set; }
        public DateTime? MOCTime { get; set; }
        public bool IsAutoEnablePrice { get; set; }
        public bool IsAutoFill { get; set; }
        public bool IsPriceEnabled { get; set; }
        public int AutoDQDelay { get; set; }
        public int HitPriceVariationForSTP { get; set; }
        public decimal BuyLot { get; set; }
        public decimal SellLot { get; set; }
        public decimal LastLot { get; set; }
        public bool Mit { get; set; }
    }
}
