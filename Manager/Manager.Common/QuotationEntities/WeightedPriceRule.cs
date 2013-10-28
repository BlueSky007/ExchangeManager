using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manager.Common.QuotationEntities
{
    public class WeightedPriceRule
    {
        public int InstrumentId;
        public int AskAskWeight;
        public int AskBidWeight;
        public int AskLastWeight;
        public int BidAskWeight;
        public int BidBidWeight;
        public int BidLastWeight;
        public int LastAskWeight;
        public int LastBidWeight;
        public int LastLastWeight;
        public int AskAvarageWeight;
        public int BidAvarageWeight;
        public int LastAvarageWeight;
        public decimal AskAdjust;
        public decimal BidAdjust;
        public decimal LastAdjust;
    }
}
