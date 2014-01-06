using iExchange.Common;
using iExchange.Common.Manager;
using System;
using System.Collections.Generic;
using System.Xml;
using TransactionError = iExchange.Common.TransactionError;

namespace iExchange.StateServer.Manager
{
    public class StateServer
    {
        internal bool UpdateInstrument(Common.Token token, System.Xml.XmlNode instruments)
        {
            throw new NotImplementedException();
        }

        internal void Broadcast(Common.Token token, OriginQuotation[] originQs, OverridedQuotation[] overridedQs)
        {
        }

        internal void Answer(Token token, XmlNode quotation)
        {
            throw new NotImplementedException();
        }

        internal TransactionError AcceptPlace(Token token, Guid tranID)
        {
            throw new NotImplementedException();
        }

        internal TransactionError Cancel(Token token, Guid tranID, CancelReason cancelReason)
        {
            return Global.StateServer.Cancel(token, tranID, cancelReason);
        }

        internal TransactionError CancelPlace(Token token, Guid tranID)
        {
            return TransactionError.OK;
        }

        internal TransactionError Execute(Token token, Guid tranID, string buyPrice, string sellPrice, string lot, Guid executedOrderID,out XmlNode xmlTran)
        {
            throw new NotImplementedException();
        }
    }

    public class ManagerHelper
    {
        public static XmlNode ConvertQuotationXml(List<Answer> answerQutos)
        {
            XmlDocument doc = new XmlDocument();
            XmlElement root = doc.CreateElement("Instrument");
            root.SetAttribute("ID", "");
            root.SetAttribute("Origin", "");
            foreach (Answer answer in answerQutos)
            {
                XmlElement answerNode = doc.CreateElement("Customer");
                answerNode.SetAttribute("ID", answer.CustomerId.ToString());
                answerNode.SetAttribute("Ask", answer.Ask);
                answerNode.SetAttribute("Bid", answer.Bid);
                answerNode.SetAttribute("QuoteLot", answer.AnswerLot.ToString());
                root.AppendChild(answerNode);
            }
            doc.AppendChild(root);
            return doc.DocumentElement;
        }
    }
}
