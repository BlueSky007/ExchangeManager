using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manager.Common.Entities
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
        public decimal? Multiplier;    // 当UseWeightedPrice为true时有值
        public bool IsDerivative;
    }
}
