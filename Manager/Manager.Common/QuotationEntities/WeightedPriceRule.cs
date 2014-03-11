using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manager.Common.QuotationEntities
{
    public class WeightedPriceRule : IMetadataObject
    {
        //public int InstrumentId;
        public int Id { get; set; }
        public decimal Multiplier { get; set; }
        public int AskAskWeight { get; set; }
        public int AskBidWeight { get; set; }
        public int AskLastWeight { get; set; }
        public int BidAskWeight { get; set; }
        public int BidBidWeight { get; set; }
        public int BidLastWeight { get; set; }
        public int LastAskWeight { get; set; }
        public int LastBidWeight { get; set; }
        public int LastLastWeight { get; set; }
        public int AskAverageWeight { get; set; }
        public int BidAverageWeight { get; set; }
        public int LastAverageWeight { get; set; }
        public decimal AskAdjust { get; set; }
        public decimal BidAdjust { get; set; }
        public decimal LastAdjust { get; set; }

        public WeightedPriceRule Clone()
        {
            return new WeightedPriceRule()
            {
                Id = this.Id,
                Multiplier = this.Multiplier,
                AskAskWeight = this.AskAskWeight,
                AskBidWeight = this.AskBidWeight,
                AskLastWeight = this.AskLastWeight,
                BidAskWeight = this.BidAskWeight,
                BidBidWeight = this.BidBidWeight,
                BidLastWeight = this.BidLastWeight,
                LastAskWeight = this.LastAskWeight,
                LastBidWeight = this.LastBidWeight,
                LastLastWeight = this.LastLastWeight,
                AskAverageWeight = this.AskAverageWeight,
                BidAverageWeight = this.BidAverageWeight,
                LastAverageWeight = this.LastAverageWeight,
                AskAdjust = this.AskAdjust,
                BidAdjust = this.BidAdjust,
                LastAdjust = this.LastAdjust
            };
        }
        public void Update(Dictionary<string, object> fieldAndValues)
        {
            foreach (string key in fieldAndValues.Keys)
            {
                this.Update(key, fieldAndValues[key]);
            }
        }

        public void Update(string field, object value)
        {
            if (field == FieldSR.Multiplier)
            {
                this.Multiplier = (decimal)value;
            }
            else if (field == FieldSR.AskAskWeight)
            {
                this.AskAskWeight = (int)value;
            }
            else if (field == FieldSR.AskBidWeight)
            {
                this.AskBidWeight = (int)value;
            }
            else if (field == FieldSR.AskLastWeight)
            {
                this.AskLastWeight = (int)value;
            }
            else if (field == FieldSR.BidAskWeight)
            {
                this.BidAskWeight = (int)value;
            }
            else if (field == FieldSR.BidBidWeight)
            {
                this.BidBidWeight = (int)value;
            }
            else if (field == FieldSR.BidLastWeight)
            {
                this.BidLastWeight = (int)value;
            }
            else if (field == FieldSR.LastAskWeight)
            {
                this.LastAskWeight = (int)value;
            }
            else if (field == FieldSR.LastBidWeight)
            {
                this.LastBidWeight = (int)value;
            }
            else if (field == FieldSR.LastLastWeight)
            {
                this.LastLastWeight = (int)value;
            }
            else if (field == FieldSR.AskAvarageWeight)
            {
                this.AskAverageWeight = (int)value;
            }
            else if (field == FieldSR.BidAvarageWeight)
            {
                this.BidAverageWeight = (int)value;
            }
            else if (field == FieldSR.LastAvarageWeight)
            {
                this.LastAverageWeight = (int)value;
            }
            else if (field == FieldSR.AskAdjust)
            {
                this.AskAdjust = (decimal)value;
            }
            else if (field == FieldSR.BidAdjust)
            {
                this.BidAdjust = (decimal)value;
            }
            else if (field == FieldSR.LastAdjust)
            {
                this.LastAdjust = (decimal)value;
            }
        }
    }
}
