using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Manager.Common.QuotationEntities;

namespace Manager.Common.LogEntities
{
    public class LogPrice : LogEntity
    {
        public LogPrice()
        {
        }
         public LogPrice(LogEntity log)
        {
            this.Id = log.Id;
            this.UserId = log.UserId;
            this.UserName = log.UserName;
            this.IP = log.IP;
            this.ExchangeCode = log.ExchangeCode;
            this.Event = log.Event;
            this.Timestamp = log.Timestamp;
        }

        public int InstrumentId { get; set; }
        public string InstrumentCode { get; set; }
        public PriceOperationType OperationType { get; set; }
        public OutOfRangeType? OutOfRangeType { get; set; }
        public string Bid { get; set; }
        public string Ask { get; set; }
        public string Diff { get; set; }
    }

    public class LogSourceChange : LogEntity
    {
        public LogSourceChange()
        {
            this.Id = Guid.NewGuid();
        }
        public LogSourceChange(LogEntity log)
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

        public bool IsDefault { get; set; }
        public int FromSourceId { get; set; }
        public string FromSourceName { get; set; }
        public int ToSourceId { get; set; }
        public string ToSourceName { get; set; }
        public byte Priority { get; set; }
    }
}
