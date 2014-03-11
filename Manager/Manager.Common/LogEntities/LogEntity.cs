using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manager.Common.LogEntities
{
    public class LogEntity
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string IP { get; set; }
        public string ExchangeCode { get; set;}
        public string Event { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
