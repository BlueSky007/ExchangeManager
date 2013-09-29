using ManagerConsole.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonEnquiry = Manager.Common.Enquiry;

namespace ManagerConsole.Model
{
    public class EnquiryClient : PropertyChangedNotifier
    {
        public EnquiryClient(Guid instrumentId, Guid customerId, decimal lot)
        {
            this.InstrumentId = instrumentId;
            this.CustomerId = customerId;
            this.Lot = lot;
            this.Timestamp = DateTime.Now;
        }

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

        internal void Update(CommonEnquiry enquiry)
        {
            this.InstrumentId = enquiry.InstrumentId;
            this.CustomerId = enquiry.CustomerId;
            this.Lot = enquiry.Lot;
            this.AnswerLot = enquiry.AnswerLot;
            this.Timestamp = enquiry.Timestamp;
            this.BSStatus = enquiry.BSStatus;
        }
    }
}
