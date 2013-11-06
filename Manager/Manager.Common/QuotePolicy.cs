using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manager.Common
{
    public class QuotePolicy
    {
        public Guid Id { get; set; }

        public string Code { get; set; }

        public string Description { get; set; }

        public bool IsDefault { get; set; }
    }
}
