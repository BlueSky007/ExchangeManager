using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manager.Common
{
    public class PrivateDailyQuotation
    {
        public Guid InstrumentId
        {
            get;
            set;
        }

        public UpdateAction UpdateAction
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

        public override string ToString()
        {
            return string.Format("InstrumentId={0}, UpdateAction={1}, TradeDay={2}, DayCloseTime={3}, Ask={4}, Bid={5}, Open={6}, PreClose={7}",
                InstrumentId, UpdateAction, TradeDay, DayCloseTime, Ask, Bid, Open, Close);
        }
    }
}
