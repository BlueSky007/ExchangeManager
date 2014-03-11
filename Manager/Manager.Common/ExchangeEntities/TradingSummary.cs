using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manager.Common.ExchangeEntities
{
    public class TradingSummary
    {
        public decimal Floating { get; set; }
        public decimal Balance { get; set; }
        public decimal NotValue { get; set; }
        public decimal Equity { get; set; }
        public decimal Necessary { get; set; }
        public decimal Usable { get; set; }
        public decimal Ratio { get; set; }
        public decimal OverNightNecessary { get; set; }
        public decimal OverNightUsable { get; set; }
        public decimal Deposit { get; set; }
        public decimal Adjustment { get; set; }
        public decimal ValueAsMargin { get; set; }
        public decimal TotalPaidAmount { get; set; }
        public decimal PartialPaymentPhysicalNecessary { get; set; }
    }
}
