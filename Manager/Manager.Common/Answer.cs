using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manager.Common
{
    public class Answer
    {
        public string ExchangeCode { get; set; }
        public Guid CustomerId { get; set; }
        public Guid InstrumentId { get; set; }
        public string Origin { get; set; }
        public string Ask { get; set; }
        public string Bid { get; set; }
        public decimal QuoteLot { get; set; }
        public decimal AnswerLot { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
