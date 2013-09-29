using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Manager.Common
{
    [KnownType(typeof(PriceMessage)),
    KnownType(typeof(QuoteMessage)),
    KnownType(typeof(Answer)),
    KnownType(typeof(PrimitiveQuotationMessage))]
    public class Message
    {
    }

    public class PriceMessage : Message
    {
    }

    public class QuoteMessage : Message
    {
        public string ExchangeCode { get; set; }
        public Guid CustomerID { get; set; }
        public Guid InstrumentID { get; set; }
        public double QuoteLot { get; set; }
        public int BSStatus { get; set; }	//0:Sell 1:Buy 2:Mis

        public QuoteMessage()
        { 
        }
        public QuoteMessage(string ExchangeCode,Guid customerId, Guid instrumentId, double quoteLot,int bSStatus)
        {
            this.CustomerID = customerId;
            this.InstrumentID = instrumentId;
            this.QuoteLot = quoteLot;
            this.BSStatus = bSStatus;
        }
        public override string ToString()
        {
            return base.ToString() + string.Format("CustomerID {0}, InstrumentID {1}, QuoteLot {2}, BSStatus {3};\n",
                this.CustomerID, this.InstrumentID, this.QuoteLot, this.BSStatus);
        }
    }

    public class PrimitiveQuotationMessage : Message
    {
        public PrimitiveQuotation Quotation;
    }

    public class SourceStatusMessage : Message
    {
        public string SouceName;
        public ConnectionState ConnectionState;
    }

    public class Answer : Message
    {
        public string ExchangeCode { get; set; }
        public Guid CustomerId { get; set; }
        public Guid InstrumentId { get; set; }
        public string Origin { get; set; }
        public string Ask { get; set; }
        public string Bid { get; set; }
        public decimal QuoteLot { get; set; }
        public decimal AnswerLot { get; set; }
        public DateTime TimeStamp { get; set; }

        public override string ToString()
        {
            return string.Format("CustomerId={0}, InstrumentId={1},Origin={2}, Ask={3}, Bid={4}, QuoteLot={5}, AnswerLot={6}",
                CustomerId, InstrumentId,Origin, Ask, Bid, QuoteLot, AnswerLot);
        }
    }
}
