using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manager.Common.QuotationEntities
{
    public enum OutOfRangeType
    {
        Ask,
        Bid
    }

    public class PriceRangeCheckRule : IMetadataObject
    {
        //public int InstrumentId;
        public int Id { get; set; }
        public bool DiscardOutOfRangePrice { get; set; }
        public OutOfRangeType OutOfRangeType { get; set; }
        public int ValidVariation { get; set; }
        public int OutOfRangeWaitTime { get; set; }
        public int OutOfRangeCount { get; set; }

        public PriceRangeCheckRule Clone()
        {
            return new PriceRangeCheckRule()
            {
                Id = this.Id,
                DiscardOutOfRangePrice = this.DiscardOutOfRangePrice,
                OutOfRangeType = this.OutOfRangeType,
                ValidVariation = this.ValidVariation,
                OutOfRangeWaitTime = this.OutOfRangeWaitTime,
                OutOfRangeCount = this.OutOfRangeCount
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
            if (field == FieldSR.DiscardOutOfRangePrice)
            {
                this.DiscardOutOfRangePrice = (bool)value;
            }
            else if (field == FieldSR.OutOfRangeType)
            {
                this.OutOfRangeType = (OutOfRangeType)(int)value;
            }
            else if (field == FieldSR.ValidVariation)
            {
                this.ValidVariation = (int)value;
            }
            else if (field == FieldSR.OutOfRangeWaitTime)
            {
                this.OutOfRangeWaitTime = (int)value;
            }
            else if (field == FieldSR.OutOfRangeCount)
            {
                this.OutOfRangeCount = (int)value;
            }
        }
    }
}
