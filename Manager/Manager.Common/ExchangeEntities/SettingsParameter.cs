using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Manager.Common.Settings
{
    public class SettingsParameter
    {
        public Guid SettingId { get; set; }
        public DealingOrderParameter DealingOrderParameter { get; set; }
        public SetValueSetting SetValueSetting { get; set; }
        public List<SoundSetting> SoundSettings { get; set; }
    }

    public class DealingOrderParameter
    {
        public int InactiveWaitTime { get; set; }
        public int EnquiryWaitTime { get; set; }
        public int PriceOrderSetting { get; set; }
        public bool DisablePopup { get; set; }
        public bool AutoConfirm { get; set; }
        public bool ConfirmRejectDQOrder { get; set; }
        public bool LimitStopSummaryIsTimeRange { get; set; }
        public int LimitStopSummaryTimeRangeValue { get; set; }
        public int LimitStopSummaryPriceRangeValue { get; set; }
    }

    public class SoundSetting
    {
        public SoundType SoundType { get; set; }
        public SoundOption SoundKey { get; set; }
        public string SoundPath { get; set; }
    }

    public class SetValueSetting
    { 
        public int OriginInactiveTime {get;set;} 
        public int AlertVariation {get;set;}  
        public int NormalWaitTime {get;set;}  
        public int AlertWaitTime {get;set;}
        public decimal MaxDQLot { get; set; }
        public decimal MaxOtherLot { get; set; }
        public decimal DQQuoteMinLot { get; set; }
        public decimal AutoDQMaxLot { get; set; }   
        public decimal AutoLmtMktMaxLot {get;set;}
        public int AcceptDQVariation { get; set; }
        public int AcceptLmtVariation { get; set; }
        public int AcceptCloseLmtVariation { get; set; }
        public int CancelLmtVariation { get; set; }   
        public int MaxMinAdjust {get;set;}  
        public bool  IsBetterPrice {get;set;}   
        public decimal AutoAcceptMaxLot {get;set;}   
        public decimal AutoCancelMaxLot {get;set;}   
        public int AllowedNewTradeSides {get;set;}   
        public int HitTimes {get;set;}  
        public int PenetrationPoint {get;set;}   
        public int PriceValidTime {get;set;}   
        public int AutoDQDelay {get;set;}
        public int HitPriceVariationForSTP { get; set; }   
    }
}
