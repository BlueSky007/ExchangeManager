using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Manager.Common.Entities;
using ManagerService.DataAccess;
using Manager.Common;

namespace ManagerService.Quotation
{
    public class ConfigMetadata
    {
        // Map for: SourceName - QuotationSource
        private Dictionary<string, QuotationSource> _QuotationSources = new Dictionary<string, QuotationSource>();

        // Map for: Code - Instrument
        private Dictionary<string, Instrument> _Instruments = new Dictionary<string, Instrument>();
        
        // Map for: SourceId - QuotationSourceRelation
        private Dictionary<int, InstrumentSourceRelation> _InstrumentSourceRelations = new Dictionary<int, InstrumentSourceRelation>();

        // Map for: InstrumentId - DerivativeRelation
        private Dictionary<int, DerivativeRelation> _DerivativeRelations = new Dictionary<int,DerivativeRelation>();
        // Map for: InstrumentId - PriceRangeCheckRule
        private Dictionary<int, PriceRangeCheckRule> _PriceRangeCheckRules = new Dictionary<int, PriceRangeCheckRule>();
        // Map for: InstrumentId - WeightedPriceRule
        private Dictionary<int, WeightedPriceRule> _WeightedPriceRules = new Dictionary<int, WeightedPriceRule>();

        Dictionary<SourceInstrumentKey, LastQuotation> _LastQuotations = new Dictionary<SourceInstrumentKey, LastQuotation>();

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

        public bool AuthenticateSource(string sourceName, string loginName, string password)
        {
            return this._QuotationSources.Values.Any(s => s.Name == sourceName && s.AuthName == loginName && s.Password == password);
        }

        public bool IsFromActiveSource(PrimitiveQuotation quotation)
        {
            int sourceId = this._QuotationSources[quotation.SourceName].Id;
            return this._InstrumentSourceRelations.Values.Any(isr => isr.SourceId == sourceId && isr.IsActive == true);
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
