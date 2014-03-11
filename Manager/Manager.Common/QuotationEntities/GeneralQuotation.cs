using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manager.Common.QuotationEntities
{
    public class GeneralQuotation
    {
        public int SourceId;
        public int InstrumentId;
        public string OriginCode;
        public DateTime Timestamp;
        public double Ask;
        public double Bid;
        public double? Last;
        public double? High;
        public double? Low;
        public string Volume;
        public string TotalVolume;
    }
}
