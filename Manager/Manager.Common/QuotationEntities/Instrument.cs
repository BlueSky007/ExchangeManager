using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manager.Common.QuotationEntities
{
    public class Instrument
    {
        public int Id;
        public string Code;
        public string MappingCode;
        public int DecimalPlace;
        public bool Inverted;
        public int InactiveTime;
        public bool UseWeightedPrice;
        public bool IsDerivative;
        public bool IsSwitchUseAgio;
        public int? AgioSeconds;
        public int? LeastTicks;
    }
}
