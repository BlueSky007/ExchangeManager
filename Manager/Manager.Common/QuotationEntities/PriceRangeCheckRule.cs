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

    public class PriceRangeCheckRule
    {
        public int InstrumentId;
        public bool DiscardOutOfRangePrice;
        public OutOfRangeType OutOfRangeType;
        public int ValidVariation;
        public int OutOfRangeWaitTime;
        public int OutOfRangeCount;
    }
}
