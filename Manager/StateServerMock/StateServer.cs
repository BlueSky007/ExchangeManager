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

        internal void ResetHit(Token token, Guid[] orderIDs)
        {
            throw new NotImplementedException();
        }

        internal XmlNode GetAcountInfo(Token token, Guid tranID)
        {
            throw new NotImplementedException();
        }


        internal void Update(Token token, XmlNode udpateNode)
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

        public static XmlNode GetInstrumentParametersXml(ParameterUpdateTask settingTask)
        {
            XmlDocument exchangeDoc = new XmlDocument();
            XmlElement xmlInstrumentRoot = exchangeDoc.CreateElement("Instruments");

            foreach (Guid instrumentId in settingTask.Instruments)
            {
                XmlElement instrumentElement = exchangeDoc.CreateElement("Instrument");
                instrumentElement.SetAttribute("ID", instrumentId.ToString());
                foreach (ExchangeSetting setting in settingTask.ExchangeSettings)
                {
                    instrumentElement.SetAttribute(setting.ParameterKey, setting.ParameterValue);
                }
                xmlInstrumentRoot.AppendChild(instrumentElement);
            }
            exchangeDoc.AppendChild(xmlInstrumentRoot);
            return exchangeDoc.DocumentElement;
        }

        public static AccountInformation GetAcountInfo(XmlNode accountInforNode)
        {
            AccountInformation accountInfor = new AccountInformation();

            XmlNode accountNode = accountInforNode.ChildNodes[0];

            foreach (XmlAttribute attribute in accountNode)
            {
                string nodeName = attribute.Name;
                string nodeValue = attribute.Value;
                if (nodeName == "ID")
                {
                    accountInfor.AccountId = new Guid(nodeValue);
                    continue;
                }
                else if (nodeName == "Balance")
                {
                    accountInfor.Balance = decimal.Parse(nodeValue);
                    continue;
                }
                else if (nodeName == "Equity")
                {
                    accountInfor.Equity = decimal.Parse(nodeValue);
                    continue;
                }
                else if (nodeName == "Necessary")
                {
                    accountInfor.Necessary = decimal.Parse(nodeValue);
                    continue;
                }
            }
            XmlNode instrumentNode = accountNode.ChildNodes[0];
            accountInfor.InstrumentId = new Guid(instrumentNode.Attributes["ID"].Value);
            accountInfor.BuyLotBalanceSum = decimal.Parse(instrumentNode.Attributes["BuyLotBalanceSum"].Value);
            accountInfor.SellLotBalanceSum = decimal.Parse(instrumentNode.Attributes["SellLotBalanceSum"].Value);
            return accountInfor;
        }

        internal static ExecutedTransaction GetExecutedTransaction(XmlNode xmlTran)
        {
            return null;
        }
    }
}
