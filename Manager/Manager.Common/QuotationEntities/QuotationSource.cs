using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manager.Common.QuotationEntities
{
    public class QuotationSource : IMetadataObject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string AuthName { get; set; }
        public string Password { get; set; }

        public void Update(Dictionary<string, object> fieldAndValues)
        {
            foreach (string key in fieldAndValues.Keys)
            {
                this.Update(key, fieldAndValues[key]);
            }
        }

        public void Update(string field, object value)
        {
            if (field == FieldSR.Name)
            {
                this.Name = (string)value;
            }
            else if (field == FieldSR.AuthName)
            {
                this.AuthName = (string)value;
            }
            else if (field == FieldSR.Password)
            {
                this.Password = (string)value;
            }
        }

        public override string ToString()
        {
            return string.Format("Type:QuotationSource[Id:{0},Name:{1},AuthName:{2},Password:{3}]", this.Id, this.Name, this.AuthName, this.Password);
        }
    }
}
