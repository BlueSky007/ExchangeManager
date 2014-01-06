using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manager.Common.LogEntities
{
    public class LogSetting : LogEntity
    {
        public LogSetting()
        {
            this.Id = Guid.NewGuid();
        }
        public LogSetting(LogEntity log)
            : this()
        {
            this.Id = log.Id;
            this.UserId = log.UserId;
            this.IP = log.IP;
            this.ExchangeCode = log.ExchangeCode;
            this.Event = log.Event;
            this.Timestamp = log.Timestamp;
        }

        public Guid Id { get; set; }
        public string ParameterName { get; set; }
        public string TableName { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
    }
}
