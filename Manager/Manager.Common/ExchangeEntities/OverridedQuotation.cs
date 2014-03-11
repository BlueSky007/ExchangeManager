using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manager.Common.Settings
{
    public class OverridedQuotation
    {
        public Guid InstrumentId;
        public Guid QuotePolicyId;
        public string Origin;
        public string Ask;
        public string Bid;
        public string High;
        public string Low;
        public DateTime Timestamp;
    }
}
