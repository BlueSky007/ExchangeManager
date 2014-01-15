using Manager.Common.ExchangeEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonQuotePolicy = Manager.Common.Settings.QuotePolicy;

namespace ManagerConsole.Model
{
    public class QuotePolicy
    {
        public Guid Id { get; set; }

        public string Code { get; set; }

        public string Description { get; set; }

        public bool IsDefault { get; set; }

        public QuotePolicy()
        {
        }

        internal void Initialize(CommonQuotePolicy quotePolicy)
        {
            this.Id = quotePolicy.Id;
            this.Code = quotePolicy.Code;
            this.Description = quotePolicy.Description;
            this.IsDefault = quotePolicy.IsDefault;
        }

        public void Update(Dictionary<string, string> fieldAndValues)
        {
            foreach (string key in fieldAndValues.Keys)
            {
                this.Update(key, fieldAndValues[key]);
            }
        }

        public void Update(string field, string value)
        {
            if (field == ExchangeFieldSR.Code)
            {
                this.Code = (string)value;
            }
            else if (field == ExchangeFieldSR.Description)
            {
                if (value != null)
                {
                    this.Description = value;
                }
            }
            else if (field == ExchangeFieldSR.IsDefault)
            {
                if (value != null)
                {
                    this.IsDefault = bool.Parse(value);
                }
            }
        }

    }
}
