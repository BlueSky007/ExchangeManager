﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manager.Common.QuotationEntities
{
    public class PrimitiveQuotation
    {
        public int SourceId;
        public int InstrumentId;
        public string SourceName { get; set; }
        public string Symbol { get; set; }
        public string Bid { get; set; }
        public string Ask { get; set; }
        public string Last { get; set; }
        public string High { get; set; }
        public string Low { get; set; }
        public DateTime Timestamp { get; set; }

        public static bool TryGetPriceValue(string price, out double value)
        {
            value = 0;
            return !string.IsNullOrEmpty(price) && double.TryParse(price, out value);
        }
    }
}
