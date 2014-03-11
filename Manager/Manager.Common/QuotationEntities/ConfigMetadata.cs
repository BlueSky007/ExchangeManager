using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Manager.Common.QuotationEntities;

namespace Manager.Common.QuotationEntities
{
    public enum MetadataType
    {
        QuotationSource,
        Instrument,
        InstrumentSourceRelation,
        DerivativeRelation,
        PriceRangeCheckRule,
        WeightedPriceRule,
    }

    public class ConfigMetadata
    {
        // Map for: SourceName - QuotationSource
        public Dictionary<string, QuotationSource> QuotationSources { get; set; }

        // Map for: InstrumentId - Instrument
        public Dictionary<int, Instrument> Instruments { get; set; }

        // Map for: InstrumentId - (SourceId - InstrumentSourceRelation)
        //public Dictionary<int, Dictionary<int, InstrumentSourceRelation>> InstrumentSourceRelations { get; set; }

        // Map for: SourceId - (SourceSymbol - InstrumentSourceRelation)
        public Dictionary<int, Dictionary<string, InstrumentSourceRelation>> InstrumentSourceRelations { get; set; }

        // Map for: (Instrument)Id - DerivativeRelation
        public Dictionary<int, DerivativeRelation> DerivativeRelations { get; set; }

        // Map for: (Instrument)Id - PriceRangeCheckRule
        public Dictionary<int, PriceRangeCheckRule> PriceRangeCheckRules { get; set; }

        // Map for: (Instrument)Id - WeightedPriceRule
        public Dictionary<int, WeightedPriceRule> WeightedPriceRules { get; set; }

        // Map for: InstrumentId - LastQuotation
        public Dictionary<int, GeneralQuotation> LastQuotations { get; set; }

        public void GetDerivativeInstrumentCodes(int instrumentId, HashSet<string> derivativeInstrumentCodes)
        {
            IEnumerable<DerivativeRelation> derivativeRelations = this.DerivativeRelations.Values.Where(d => d.UnderlyingInstrument1Id == instrumentId || d.UnderlyingInstrument2Id == instrumentId);
            foreach (DerivativeRelation derivativeRelation in derivativeRelations)
            {
                derivativeInstrumentCodes.Add(this.Instruments[derivativeRelation.Id].Code);
                this.GetDerivativeInstrumentCodes(derivativeRelation.Id, derivativeInstrumentCodes);
            }
        }
    }
}
