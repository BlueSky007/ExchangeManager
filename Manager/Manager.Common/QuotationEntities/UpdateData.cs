using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manager.Common.QuotationEntities
{
    public class UpdateData
    {
        public MetadataType MetadataType;
        public int ObjectId;
        public Dictionary<string, object> FieldsAndValues;
    }
}
