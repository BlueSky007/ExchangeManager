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

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("Id=");
            stringBuilder.Append(Id);
            stringBuilder.Append(",");

            stringBuilder.Append("Code=");
            stringBuilder.Append(Code);
            stringBuilder.Append(",");

            stringBuilder.Append("Type=");
            stringBuilder.Append(Type);
            stringBuilder.Append(",");

            stringBuilder.Append("SubType=");
            stringBuilder.Append(SubType);
            stringBuilder.Append(",");

            stringBuilder.Append("InstrumentCategory=");
            stringBuilder.Append(InstrumentCategory);
            stringBuilder.Append(",");

            stringBuilder.Append("Phase=");
            stringBuilder.Append(Phase);
            stringBuilder.Append(",");

            stringBuilder.Append("AccountId=");
            stringBuilder.Append(AccountId);
            stringBuilder.Append(",");

            stringBuilder.Append("InstrumentId=");
            stringBuilder.Append(InstrumentId);
            stringBuilder.Append(",");

            stringBuilder.Append("BeginTime=");
            stringBuilder.Append(BeginTime);
            stringBuilder.Append(",");

            stringBuilder.Append("EndTime=");
            stringBuilder.Append(EndTime);
            stringBuilder.Append(",");

            stringBuilder.Append("ExpireType=");
            stringBuilder.Append(ExpireType);
            stringBuilder.Append(",");

            stringBuilder.Append("SubmitTime=");
            stringBuilder.Append(SubmitTime);
            stringBuilder.Append(",");

            if (ExecuteTime != null)
            {
                stringBuilder.Append("ExecuteTime=");
                stringBuilder.Append(ExecuteTime);
                stringBuilder.Append(",");
            }

            stringBuilder.Append("ContractSize=");
            stringBuilder.Append(ContractSize);
            stringBuilder.Append(",");

            stringBuilder.Append("SubmitorId=");
            stringBuilder.Append(SubmitorId);
            stringBuilder.Append(",");

            if (AssigningOrderId != null)
            {
                stringBuilder.Append("AssigningOrderId=");
                stringBuilder.Append(AssigningOrderId);
                stringBuilder.Append(",");
            }

            if (Error != null)
            {
                stringBuilder.Append("Error=");
                stringBuilder.Append(Error);
                stringBuilder.Append(",");
            }

            if (Orders != null && Orders.Length > 0)
            {
                stringBuilder.Append("Orders:");
                foreach (Order order in Orders)
                {
                    stringBuilder.Append(order);
                    stringBuilder.Append(";");
                }
            }

            if (OrderRelations != null && OrderRelations.Length > 0)
            {
                stringBuilder.Append("OrderRelations:");
                foreach (OrderRelation orderRelation in OrderRelations)
                {
                    stringBuilder.Append(orderRelation);
                    stringBuilder.Append(";");
                }
            }

            stringBuilder.Append("OrderType=");
            stringBuilder.Append(OrderType);
            stringBuilder.Append(",");

            return stringBuilder.ToString();
        }
    }
}
