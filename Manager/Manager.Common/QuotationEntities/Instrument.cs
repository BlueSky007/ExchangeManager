using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manager.Common.QuotationEntities
{
    public class Instrument : IMetadataObject
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public int AdjustPoints { get; set; }
        public int AdjustIncrement { get; set; }
        public bool IsDerivative { get; set; }
        public int DecimalPlace { get; set; }
        public int? InactiveTime { get; set; }
        public bool? UseWeightedPrice { get; set; }
        public bool? IsSwitchUseAgio { get; set; }
        public int? AgioSeconds { get; set; }
        public int? LeastTicks { get; set; }

        public Instrument Clone()
        {
            return new Instrument()
            {
                Id = this.Id,
                Code = this.Code,
                AdjustPoints = this.AdjustPoints,
                AdjustIncrement = this.AdjustIncrement,
                IsDerivative = this.IsDerivative,
                DecimalPlace = this.DecimalPlace,
                InactiveTime = this.InactiveTime,
                UseWeightedPrice = this.UseWeightedPrice,
                IsSwitchUseAgio = this.IsSwitchUseAgio,
                AgioSeconds = this.AgioSeconds,
                LeastTicks = this.LeastTicks
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
            if (field == FieldSR.Code)
            {
                this.Code = (string)value;
            }
            else if (field == FieldSR.AdjustPoints)
            {
                this.AdjustPoints = (int)value;
            }
            else if (field == FieldSR.AdjustIncrement)
            {
                this.AdjustIncrement = (int)value;
            }
            else if (field == FieldSR.DecimalPlace)
            {
                this.DecimalPlace = (int)value;
            }
            else if (field == FieldSR.InactiveTime)
            {
                this.InactiveTime = (int)value;
            }
            else if (field == FieldSR.UseWeightedPrice)
            {
                this.UseWeightedPrice = (bool)value;
            }
            else if (field == FieldSR.IsDerivative)
            {
                this.IsDerivative = (bool)value;
            }
            else if (field == FieldSR.IsSwitchUseAgio)
            {
                this.IsSwitchUseAgio = (bool)value;
            }
            else if (field == FieldSR.AgioSeconds)
            {
                this.AgioSeconds = (int?)value;
            }
            else if (field == FieldSR.LeastTicks)
            {
                this.LeastTicks = (int?)value;
            }
        }
    }
}
