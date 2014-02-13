using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Manager.Common.QuotationEntities;
//using Manager.Common.Settings;

namespace Manager.Common
{
    public static class KnownTypes
    {
        public static IEnumerable<Type> GetKnownTypes(ICustomAttributeProvider provider)
        {
            List<System.Type> knownTypes = new List<Type>();
            knownTypes.Add(typeof(QuotationSource));
            knownTypes.Add(typeof(Instrument));
            knownTypes.Add(typeof(InstrumentSourceRelation));
            knownTypes.Add(typeof(DerivativeRelation));
            knownTypes.Add(typeof(PriceRangeCheckRule));
            knownTypes.Add(typeof(WeightedPriceRule));
            knownTypes.Add(typeof(OperandType));
            knownTypes.Add(typeof(OperatorType));
            knownTypes.Add(typeof(OutOfRangeType));
            return knownTypes;
        }
        //public static IEnumerable<Type> GetCallbackKnownTypes(ICustomAttributeProvider provider)
        //{
        //    List<System.Type> knownTypes = new List<Type>();
        //    knownTypes.Add(typeof(QuoteMessage));
        //    knownTypes.Add(typeof(PlaceMessage));
        //    knownTypes.Add(typeof(ExecuteMessage));
        //    knownTypes.Add(typeof(HitMessage));
        //    knownTypes.Add(typeof(UpdateMessage));
        //    knownTypes.Add(typeof(DeleteMessage));
        //    knownTypes.Add(typeof(PrimitiveQuotationMessage));
        //    knownTypes.Add(typeof(AbnormalQuotationMessage));
        //    knownTypes.Add(typeof(UpdateMetadataMessage));
        //    knownTypes.Add(typeof(AddMetadataObjectMessage));
        //    knownTypes.Add(typeof(AddMetadataObjectsMessage));
        //    knownTypes.Add(typeof(DeleteMetadataObjectMessage));
        //    knownTypes.Add(typeof(SwitchRelationBooleanPropertyMessage));
        //    knownTypes.Add(typeof(QuotationsMessage));
        //    knownTypes.Add(typeof(OverridedQuotationMessage));
        //    knownTypes.Add(typeof(UpdateQuotePolicyDetailMessage));
        //    knownTypes.Add(typeof(UpdateSettingParameterMessage));
        //    knownTypes.Add(typeof(TradeDay));
        //    knownTypes.Add(typeof(DataPermission));
        //    knownTypes.Add(typeof(PrivateDailyQuotation));
        //    knownTypes.Add(typeof(SystemParameter));
        //    knownTypes.Add(typeof(Manager.Common.Settings.Instrument));
        //    knownTypes.Add(typeof(Account));
        //    knownTypes.Add(typeof(AccountGroup));
        //    knownTypes.Add(typeof(Customer));
        //    knownTypes.Add(typeof(QuotePolicy));
        //    knownTypes.Add(typeof(QuotePolicyDetail));
        //    knownTypes.Add(typeof(SettingSet));
        //    knownTypes.Add(typeof(UpdateAction));
        //    return knownTypes;
        //}
    }
}
