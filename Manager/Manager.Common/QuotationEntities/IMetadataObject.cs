using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manager.Common.QuotationEntities
{
    public interface IMetadataObject
    {
        int Id { get; set; }
        void Update(Dictionary<string, object> fieldAndValues);
        void Update(string field, object value);
    }
}
