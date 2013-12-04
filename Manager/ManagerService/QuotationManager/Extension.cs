using Manager.Common.QuotationEntities;
using ManagerService.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ManagerService.Quotation
{
    public static class Extension
    {
        public static void Load(this ConfigMetadata metaData)
        {
            metaData.QuotationSources = new Dictionary<string, QuotationSource>();
            metaData.Instruments = new Dictionary<int, Instrument>();
            metaData.InstrumentSourceRelations = new Dictionary<int, Dictionary<string, InstrumentSourceRelation>>();
            metaData.DerivativeRelations = new Dictionary<int, DerivativeRelation>();
            metaData.PriceRangeCheckRules = new Dictionary<int, PriceRangeCheckRule>();
            metaData.WeightedPriceRules = new Dictionary<int, WeightedPriceRule>();
            metaData.LastQuotations = new Dictionary<int, GeneralQuotation>();

            QuotationData.GetQuotationMetadata(
                metaData.QuotationSources,
                metaData.Instruments,
                metaData.InstrumentSourceRelations,
                metaData.DerivativeRelations,
                metaData.PriceRangeCheckRules,
                metaData.WeightedPriceRules,
                metaData.LastQuotations);
        }

        public static bool AuthenticateSource(this ConfigMetadata metaData, string sourceName, string loginName, string password)
        {
            return metaData.QuotationSources.Values.Any(s => s.Name == sourceName && s.AuthName == loginName && s.Password == password);
        }

        public static bool EnsureIsKnownQuotation(this ConfigMetadata metaData, PrimitiveQuotation quotation, out bool inverted)
        {
            inverted = false;
            QuotationSource quotationSource;
            if (metaData.QuotationSources.TryGetValue(quotation.SourceName, out quotationSource))
            {
                quotation.SourceId = quotationSource.Id;
                Dictionary<string, InstrumentSourceRelation> relations;
                if (metaData.InstrumentSourceRelations.TryGetValue(quotationSource.Id, out relations))
                {
                    InstrumentSourceRelation relation;
                    if (relations.TryGetValue(quotation.Symbol, out relation))
                    {
                        quotation.InstrumentId = relation.InstrumentId;
                        inverted = relation.Inverted;
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
