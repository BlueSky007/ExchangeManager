using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manager.Common
{
    public class Enquiry
    {
        public Guid InstrumentId
        {
            get;
            set;
        }

        public Guid CustomerId
        {
            get;
            set;
        }

        public decimal Lot
        {
            get;
            set;
        }

        public decimal AnswerLot
        {
            get;
            set;
        }

        public DateTime Timestamp
        {
            get;
            set;
        }

        public bool BSStatus
        {
            get;
            set;
        }
    }
}
