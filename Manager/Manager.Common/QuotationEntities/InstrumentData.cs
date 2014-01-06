using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manager.Common.QuotationEntities
{
    public class InstrumentData
    {
        public Instrument Instrument { get; set; }
        public PriceRangeCheckRule PriceRangeCheckRule { get; set; }
        public WeightedPriceRule WeightedPriceRule { get; set; }
        public DerivativeRelation DerivativeRelation { get; set; }

        public InstrumentData Clone()
        {
            return new InstrumentData()
            {
                Instrument = this.Instrument.Clone(),
                PriceRangeCheckRule = this.PriceRangeCheckRule.Clone(),
                WeightedPriceRule = this.WeightedPriceRule.Clone(),
                DerivativeRelation = this.DerivativeRelation.Clone()
            };
        }
    }
}
