using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manager.Common.QuotationEntities
{
    public class InstrumentSourceRelation : IMetadataObject
    {
        public int Id { get; set; }
        public int SourceId { get; set; }
        public string SourceSymbol { get; set; }
        public int InstrumentId { get; set; }
        public bool Inverted { get; set; }
        public bool IsActive { get; set; }
        public bool IsDefault { get; set; }
        public int Priority { get; set; }
        public int SwitchTimeout { get; set; }
        public int AdjustPoints { get; set; }
        public int AdjustIncrement { get; set; }

        public override string ToString()
        {
            return string.Format("InstrumentSourceRelation Id:{0} SourceId:{1} SourceSymbol:{2} InstrumentId:{3} Inverted:{4} IsActive:{5} IsDefault:{6} Priority:{7} SwitchTimeout:{8} AdjustPoints:{9} AdjustIncrement:{10}",
                this.Id, this.SourceId, this.SourceSymbol, this.InstrumentId, this.Inverted, this.IsActive, this.IsDefault, this.Priority,
                this.SwitchTimeout, this.AdjustPoints, this.AdjustIncrement);
        }

        public InstrumentSourceRelation Clone()
        {
            InstrumentSourceRelation newRelation = new InstrumentSourceRelation()
            {
                Id = this.Id,
                SourceId = this.SourceId,
                SourceSymbol = this.SourceSymbol,
                InstrumentId = this.InstrumentId,
                Inverted = this.Inverted,
                IsActive = this.IsActive,
                IsDefault = this.IsDefault,
                Priority = this.Priority,
                SwitchTimeout = this.SwitchTimeout,
                AdjustPoints = this.AdjustPoints,
                AdjustIncrement = this.AdjustIncrement
            };
            return newRelation;
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
            if (field == FieldSR.SourceId)
            {
                this.SourceId = (int)value;
            }
            else if (field == FieldSR.SourceSymbol)
            {
                this.SourceSymbol = (string)value;
            }
            else if (field == FieldSR.InstrumentId)
            {
                this.InstrumentId = (int)value;
            }
            else if (field == FieldSR.Inverted)
            {
                this.Inverted = (bool)value;
            }
            else if (field == FieldSR.IsActive)
            {
                this.IsActive = (bool)value;
            }
            else if (field == FieldSR.IsDefault)
            {
                this.IsDefault = (bool)value;
            }
            else if (field == FieldSR.Priority)
            {
                this.Priority = (int)value;
            }
            else if (field == FieldSR.SwitchTimeout)
            {
                this.SwitchTimeout = (int)value;
            }
            else if (field == FieldSR.AdjustPoints)
            {
                this.AdjustPoints = (int)value;
            }
            else if (field == FieldSR.AdjustIncrement)
            {
                this.AdjustIncrement = (int)value;
            }
        }
    }
}
