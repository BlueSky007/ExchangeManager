using Manager.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ManagerService.Messages
{
    internal class DispatchableQuote : DispatchableMessage
    {
        internal DispatchableQuote(string exchangeCode,Guid customerId, Guid instrumentId, double quoteLot,int bSStatus)
        {
            this.QuoteMessage = new QuoteMessage();

            this.QuoteMessage.ExchangeCode = exchangeCode;
            this.QuoteMessage.CustomerID = customerId;
            this.QuoteMessage.InstrumentID = instrumentId;
            this.QuoteMessage.QuoteLot = quoteLot;
            this.QuoteMessage.BSStatus = bSStatus;
        }

        internal QuoteMessage QuoteMessage
        {
            get;
            private set;
        }

        internal protected override Message MessageToSend
        {
            get { return this.QuoteMessage; }
        }
    }
}
