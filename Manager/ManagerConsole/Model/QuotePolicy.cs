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

        internal void Update(CommonQuotePolicy quotePolicy)
        {
            this.Id = quotePolicy.Id;
            this.Code = quotePolicy.Code;
            this.Description = quotePolicy.Description;
            this.IsDefault = quotePolicy.IsDefault;
        }

    }
}
