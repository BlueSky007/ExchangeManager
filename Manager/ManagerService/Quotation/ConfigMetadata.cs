using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Manager.Common.QuotationEntities;
using ManagerService.DataAccess;

namespace ManagerService.Quotation
{
    public class ConfigMetadata
    {
        // Map for: SourceName - QuotationSource
        private Dictionary<string, QuotationSource> _QuotationSources = new Dictionary<string, QuotationSource>();

        // Map for: Code - Instrument
        private Dictionary<string, Instrument> _Instruments = new Dictionary<string, Instrument>();
        
        //// Map for: SourceId - QuotationSourceRelation
        //private Dictionary<SourceInstrumentKey, InstrumentSourceRelation> _InstrumentSourceRelations = new Dictionary<SourceInstrumentKey, InstrumentSourceRelation>();

        // Map for: InstrumentId - (SourceId - QuotationSourceRelation)
        private Dictionary<int, Dictionary<int, InstrumentSourceRelation>> _InstrumentSourceRelations = new Dictionary<int, Dictionary<int, InstrumentSourceRelation>>();

        // Map for: InstrumentId - DerivativeRelation
        private Dictionary<int, DerivativeRelation> _DerivativeRelations = new Dictionary<int,DerivativeRelation>();
        // Map for: InstrumentId - PriceRangeCheckRule
        private Dictionary<int, PriceRangeCheckRule> _PriceRangeCheckRules = new Dictionary<int, PriceRangeCheckRule>();
        // Map for: InstrumentId - WeightedPriceRule
        private Dictionary<int, WeightedPriceRule> _WeightedPriceRules = new Dictionary<int, WeightedPriceRule>();

        private Dictionary<SourceInstrumentKey, LastQuotation> _LastQuotations = new Dictionary<SourceInstrumentKey, LastQuotation>();

        public ConfigMetadata()
        {
            QuotationData.GetQuotationMetadata(
                this._QuotationSources,
                this._Instruments,
                this._InstrumentSourceRelations,
                this._DerivativeRelations,
                this._PriceRangeCheckRules,
                this._WeightedPriceRules,
                this._LastQuotations);
        }

        public Dictionary<int, Dictionary<int, InstrumentSourceRelation>> InstrumentSourceRelations { get { return this._InstrumentSourceRelations; } }
        public Dictionary<string, Instrument> Instruments { get { return this._Instruments; } }
        public Dictionary<int, PriceRangeCheckRule> RangeCheckRules { get { return this._PriceRangeCheckRules; } }
        public Dictionary<int, WeightedPriceRule> WeightedPriceRules { get { return this._WeightedPriceRules; } }
        public Dictionary<int, DerivativeRelation> DerivativeRelations { get { return this._DerivativeRelations; } }

        public bool AuthenticateSource(string sourceName, string loginName, string password)
        {
            return this._QuotationSources.Values.Any(s => s.Name == sourceName && s.AuthName == loginName && s.Password == password);
        }

        public bool EnsureIsKnownQuotation(PrimitiveQuotation quotation)
        {
            Instrument instrument;
            QuotationSource quotationSource;
            if (this._Instruments.TryGetValue(quotation.InstrumentCode, out instrument) &&
                this._QuotationSources.TryGetValue(quotation.SourceName, out quotationSource))
            {
                quotation.SourceId = quotationSource.Id;
                quotation.InstrumentId = instrument.Id;
                return true;
            }
            return false;
        }
    }
}
