using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manager.Common.Entities
{
    public class PriceRangeCheckRule
    {
        public int InstrumentId;
        public bool DiscardOutOfRangePrice;
        public byte OutOfRangeType;
        public int ValidVariation;
        public int OutOfRangeWaitTime;
        public int OutOfRangeCount;
    }
}
