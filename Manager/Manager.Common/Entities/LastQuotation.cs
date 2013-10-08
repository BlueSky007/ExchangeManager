using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manager.Common.Entities
{
    public class LastQuotation
    {
        public int SourceId;
        public int InstrumentId;
        public DateTime Timestamp;
        public string Ask;
        public string Bid;
        public string Last;
        public string High;
        public string Low;
    }
}
