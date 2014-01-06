using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Manager.Common.QuotationEntities;

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
            return knownTypes;
        }
    }
}
