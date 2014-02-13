using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manager.Common.LogEntities
{
    public class LogQuote:LogEntity
    {
        public LogQuote()
        {
            this.Id = Guid.NewGuid();
        }
        public LogQuote(LogEntity log)
            : this()
        {
            this.Id = log.Id;
            this.UserId = log.UserId;
            this.UserName = log.UserName;
            this.IP = log.IP;
            this.ExchangeCode = log.ExchangeCode;
            this.Event = log.Event;
            this.Timestamp = log.Timestamp;
        }
        public decimal Lot { get; set; }
        public decimal AnswerLot { get; set; }
        public string Ask { get; set; }
        public string Bid { get; set; }
        public bool IsBuy { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; }
        public Guid InstrumentId { get; set; }
        public string InstrumentCode { get; set; }
        public DateTime SendTime { get; set; }
    }
}
