using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manager.Common.Settings
{
    public class PrivateDailyQuotation
    {
        public Guid InstrumentId
        {
            get;
            set;
        }

        public DateTime TradeDay
        {
            get;
            set;
        }

        public DateTime? DayCloseTime
        {
            get;
            set;
        }

        public string Ask
        {
            get;
            set;
        }

        public string Bid
        {
            get;
            set;
        }

        public string Open
        {
            get;
            set;
        }

        public string Close
        {
            get;
            set;
        }
    }
}
