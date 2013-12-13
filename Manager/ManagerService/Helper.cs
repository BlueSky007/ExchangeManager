using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Manager.Common.QuotationEntities;
using System.Reflection;

namespace ManagerService
{
    public class ServiceHelper
    {
        public static string DumpDictionary(IDictionary dict)
        {
            StringBuilder builder = new StringBuilder();
            foreach (var key in dict.Keys)
            {
                builder.AppendFormat("{0}:{1}\r\n", key, dict[key]);
            }
            return builder.ToString();
        }

        public static void GetTableName(MetadataType type, out string tableName, out string keyFieldName)
        {
            tableName = string.Empty;
            keyFieldName = "Id";
            switch (type)
            {
                case MetadataType.QuotationSource:
                    tableName = "QuotationSource";
                    break;
                case MetadataType.Instrument:
                    tableName = "Instrument";
                    break;
                case MetadataType.InstrumentSourceRelation:
                    tableName = "InstrumentSourceRelation";
                    break;
                case MetadataType.DerivativeRelation:
                    tableName = "DerivativeRelation";
                    keyFieldName = "InstrumentId";
                    break;
                case MetadataType.PriceRangeCheckRule:
                    tableName = "PriceRangeCheckRule";
                    keyFieldName = "InstrumentId";
                    break;
                case MetadataType.WeightedPriceRule:
                    tableName = "WeightedPriceRule";
                    keyFieldName = "InstrumentId";
                    break;
            }
        }

    }
}
