using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manager.Common.QuotationEntities
{
    public interface IMetadataObject
    {
        int Id { get; set; }
        //MetadataType Type { get; }
        void Update(Dictionary<string, object> fieldAndValues);
        void Update(string field, object value);
    }

    //[
    //public class MetadataObject
    //{
    //    public int Id;
    //}
}
