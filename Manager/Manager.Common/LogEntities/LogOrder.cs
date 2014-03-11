using iExchange.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manager.Common.LogEntities
{
    public class LogOrder : LogEntity
    {
        public LogOrder()
        {
            this.Id = Guid.NewGuid();
        }
        public LogOrder(LogEntity log)
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
        public OperationType OperationType { get; set; }
        public Guid OrderId { get; set; }
        public string OrderCode { get; set; }
        public string AccountCode { get; set; }
        public string InstrumentCode { get; set; }
        public bool IsBuy { get; set; }
        public bool IsOpen { get; set; }
        public decimal Lot { get; set; }
        public string SetPrice { get; set; }
        public OrderType OrderType { get; set; }
        public string OrderRelation { get; set; }
        public string TransactionCode { get; set; }
    }
}
