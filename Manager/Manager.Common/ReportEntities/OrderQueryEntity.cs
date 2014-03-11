using iExchange.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manager.Common.ReportEntities
{
    public class OrderQueryEntity
    {
        public string OrderCode { get; set; }
        public string InstrumentCode { get; set; }
        public string ExchangeCode { get; set; }
        public bool BuySell { get; set; }
        public string OpenClose { get; set; }
        public decimal Lot { get; set; }
        public string AccountCode { get; set; }
        public string SetPrice { get; set; }
        public OrderType OrderType { get; set; }
        public DateTime ExecuteTime { get; set; }
        public string Relation { get; set; }
        public Guid Dealer { get; set; }
    }
}
