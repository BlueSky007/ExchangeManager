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

        public ICollection<Instrument> Instruments { get { return this._Instruments.Values; } }

        public bool AuthenticateSource(string sourceName, string loginName, string password)
        {
            return this._QuotationSources.Values.Any(s => s.Name == sourceName && s.AuthName == loginName && s.Password == password);
        }

        public bool IsKnownQuotation(PrimitiveQuotation quotation, out int instrumentId, out int sourceId)
        {
            instrumentId = sourceId = 0;
            Instrument instrument;
            QuotationSource quotationSource;
            if (this._Instruments.TryGetValue(quotation.InstrumentCode, out instrument) &&
                this._QuotationSources.TryGetValue(quotation.SourceName, out quotationSource))
            {
                instrumentId = instrument.Id;
                sourceId = quotationSource.Id;
                return true;
            }
            return false;
        }

        public bool IsFromActiveSource(int instrumentId, int sourceId)
        {
            return this._InstrumentSourceRelations[instrumentId][sourceId].IsActive;
        }

        public void Adjust(Quotation quotation)
        {
            throw new NotImplementedException();
        }

        public int GetSourceId(string sourceCode)
        {
            return this._QuotationSources[sourceCode].Id;
        }

        public int GetInstrumentId(string instrumentCode)
        {
            return this._Instruments[instrumentCode].Id;
        }
    }
}
