using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iExchange.Common;
using Manager.Common;
using System.Xml;
using ManagerService.Messages;

namespace ManagerService.Exchange
{
    public class CommandConvertor
    {
        public static Message Convert(Command command)
        {
            return CommandConvertor.Convert((dynamic)command);
        }

        private static Message Convert(QuoteCommand quoteCommand)
        {
            //XmlNode node = quoteCommand.Content.Attributes["Quote"];
            //Guid customerId = new Guid(node.Attributes["CustomerID"].Value);
            //Guid instrumentId = new Guid(node.Attributes["InstrumentID"].Value);
            //double quoteLot = double.Parse(node.Attributes["QuoteLot"].Value);
            //int bSStatus = int.Parse(node.Attributes["BSStatus"].Value);

            //QuoteMessage quoteMessage = new QuoteMessage(customerId,instrumentId,quoteLot,bSStatus);
            
            //QuoteMessage quoteMessage = new QuoteMessage(quoteCommand.CustomerID, quoteCommand.InstrumentID, quoteCommand.QuoteLot,quoteCommand.BSStatus);
            DispatchableQuote quoteMessage = new DispatchableQuote("WF01",quoteCommand.CustomerID, quoteCommand.InstrumentID, quoteCommand.QuoteLot, quoteCommand.BSStatus);

            return quoteMessage.MessageToSend;
        }
    }
}
