using iExchange.Common;
using Manager.Common.ExchangeEntities;
using ManagerConsole.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ManagerConsole.Model
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

        public bool IsAutoFill;

        public bool IsPriceEnabled;

        public bool IsAutoEnablePrice;

        public int OrderTypeMask;
        //        instrument
        public int AcceptLmtVariation;
        public decimal AutoDQMaxLot;
        public int AlertVariation;
        public decimal DqQuoteMinLot;
        public decimal MaxDQLot;
        public int NormalWaitTime;
        public int AlertWaitTime;
        public decimal MaxOtherLot;
        public int CancelLmtVariation;
        public int MaxMinAdjust;
        public int PenetrationPoint;
        public int PriceValidTime;
        public decimal AutoCancelMaxLot;
        public decimal AutoAcceptMaxLot;
        public int HitPriceVariationForSTP;
        public int AutoDQDelay;

        public ExchangeQuotation()
        {
        }

        public ExchangeQuotation(Manager.Common.Settings.QuotePolicyDetail detail, Manager.Common.Settings.Instrument instrument)
        {
            this.QuotationPolicyId = detail.QuotePolicyId;
            this.InstruemtnId = detail.InstrumentId;
            this.InstrumentCode = instrument.Code;
            this.OriginInstrumentCode = instrument.OriginCode;
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
            this.IsAutoFill = instrument.IsAutoFill;
            this.IsPriceEnabled = instrument.IsPriceEnabled;
            this.IsAutoEnablePrice = instrument.IsAutoEnablePrice;
            this.OrderTypeMask = instrument.OrderTypeMask;
            this.AcceptLmtVariation = instrument.AcceptLmtVariation;
            this.AutoDQMaxLot = instrument.AutoDQMaxLot;
            this.AlertVariation = instrument.AlertVariation;
            this.DqQuoteMinLot = instrument.DqQuoteMinLot;
            this.MaxDQLot = instrument.MaxDQLot;
            this.NormalWaitTime = instrument.NormalWaitTime;
            this.AlertWaitTime = instrument.AlertWaitTime;
            this.MaxOtherLot = instrument.MaxOtherLot;
            this.CancelLmtVariation = instrument.CancelLmtVariation;
            this.MaxMinAdjust = instrument.MaxMinAdjust;
            this.PenetrationPoint = instrument.PenetrationPoint;
            this.PriceValidTime = instrument.PriceValidTime;
            this.AutoCancelMaxLot = instrument.AutoCancelMaxLot;
            this.AutoAcceptMaxLot = instrument.AutoAcceptMaxLot; 
        }

        public void Update(Dictionary<string, string> fieldAndValues,string exchangeCode)
        {
            foreach (string key in fieldAndValues.Keys)
            {
                this.Update(key, fieldAndValues[key],exchangeCode);
            }
        }

        public void Update(string field, string value, string exchangeCode)
        {
            if (field == ExchangeFieldSR.IsPriceEnabled)
            {
                this.IsPriceEnabled = bool.Parse(value);
                ExchangeQuotationViewModel.Instance.Exchanges.SingleOrDefault(i => i.ExchangeCode == exchangeCode && i.QuotationPolicyId == this.QuotationPolicyId && i.InstruemtnId == this.InstruemtnId).IsPriceEnabled = bool.Parse(value);
            }
            else if (field == ExchangeFieldSR.IsAutoEnablePrice)
            {
                this.IsAutoEnablePrice = bool.Parse(value);
                ExchangeQuotationViewModel.Instance.Exchanges.SingleOrDefault(i => i.ExchangeCode == exchangeCode && i.QuotationPolicyId == this.QuotationPolicyId && i.InstruemtnId == this.InstruemtnId).IsAutoEnablePrice = bool.Parse(value);
            }
        }
    }
}
