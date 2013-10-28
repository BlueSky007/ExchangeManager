using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manager.Common.QuotationEntities
{
    public class InstrumentSourceRelation
    {
        public int InstrumentId;
        public int SourceId;
        public bool IsActive;
        public bool IsDefault;
        public int Priority;
        public int SwitchTimeout;
        public decimal AdjustPoints;
        public decimal AdjustIncrement;
    }
}
