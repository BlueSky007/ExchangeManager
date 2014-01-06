using iExchange.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manager.Common
{
    public class Transaction
    {
        public Guid Id
        {
            get;
            set;
        }

        public string Code
        {
            get;
            set;
        }

        public TransactionType Type
        {
            get;
            set;
        }

        public TransactionSubType SubType
        {
            get;
            set;
        }

        public Phase Phase
        {
            get;
            set;
        }

        public Guid AccountId
        {
            get;
            set;
        }

        public Guid InstrumentId
        {
            get;
            set;
        }

        public DateTime BeginTime
        {
            get;
            set;
        }

        public DateTime EndTime
        {
            get;
            set;
        }

        public ExpireType ExpireType
        {
            get;
            set;
        }

        public DateTime SubmitTime
        {
            get;
            set;
        }

        public DateTime? ExecuteTime
        {
            get;
            set;
        }

        public OrderType OrderType
        {
            get;
            set;
        }

        public decimal ContractSize
        {
            get;
            set;
        }

        public Guid SubmitorId
        {
            get;
            set;
        }

        public Guid? AssigningOrderId
        {
            get;
            set;
        }

        public TransactionError? Error
        {
            get;
            set;
        }

        public InstrumentCategory? InstrumentCategory
        {
            get;
            set;
        }

        public OrderRelation[] OrderRelations { get; set; }

        public Order[] Orders { get; set; }
    }
}
