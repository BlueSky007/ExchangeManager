using iExchange.Common;
using iExchange.Common.Manager;
using ManagerService.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

namespace ManagerService.Exchange
{
    internal static class NetGroupManagerHelper
    {
        internal static void UpdateByXmlRowAttribute(this OpenInterestSummary openInterestSummary, XmlNode xmlNode)
        {
            try
            {
                foreach (XmlAttribute attribute in xmlNode.Attributes)
                {
                    String nodeName = attribute.Name;
                    String nodeValue = attribute.Value;
                    if (nodeName == "ID")
                    {
                        openInterestSummary.Id = new Guid(nodeValue);
                        continue;
                    }
                    else if (nodeName == "Code")
                    {
                        openInterestSummary.Code = nodeValue;
                        continue;
                    }
                    else if (nodeName == "Type")
                    {
                        openInterestSummary.AccountType = (AccountType)(int.Parse(nodeValue));
                        continue;
                    }
                    else if (nodeName == "MinNumeratorUnit")
                    {
                        openInterestSummary.MinNumeratorUnit = int.Parse(nodeValue);
                        continue;
                    }
                    else if (nodeName == "MaxDenominator")
                    {
                        openInterestSummary.MaxDenominator = int.Parse(nodeValue); ;
                        continue;
                    }
                    else if (nodeName == "BuyLot")
                    {
                        openInterestSummary.BuyLot = decimal.Parse(nodeValue); ;
                        continue;
                    }
                    else if (nodeName == "AvgBuyPrice")
                    {
                        openInterestSummary.BuyAvgPrice = nodeValue; ;
                        continue;
                    }
                    else if (nodeName == "BuyContractSize")
                    {
                        openInterestSummary.BuyContractSize = decimal.Parse(nodeValue); ;
                        continue;
                    }
                    else if (nodeName == "SellLot")
                    {
                        openInterestSummary.SellLot = decimal.Parse(nodeValue); ;
                        continue;
                    }
                    else if (nodeName == "AvgSellPrice")
                    {
                        openInterestSummary.SellAvgPrice = nodeValue; ;
                        continue;
                    }
                    else if (nodeName == "SellContractSize")
                    {
                        openInterestSummary.SellContractSize = decimal.Parse(nodeValue); ;
                        continue;
                    }
                    else if (nodeName == "NetLot")
                    {
                        openInterestSummary.NetLot = decimal.Parse(nodeValue); ;
                        continue;
                    }
                    else if (nodeName == "AvgNetPrice")
                    {
                        openInterestSummary.NetAvgPrice = nodeValue; ;
                        continue;
                    }
                    else if (nodeName == "NetContractSize")
                    {
                        openInterestSummary.NetContractSize = decimal.Parse(nodeValue); ;
                        continue;
                    }
                }
            }
            catch (Exception ex)
            { 
            }
        }
    }
    public class NetGroupManager
    {
        public static List<AccountGroupGNP> GetGroupNetPosition()
        {
            List<AccountGroupGNP> accountGroups = new List<AccountGroupGNP>();
            try
            {
                string xmlPath = GetCommandXmlPath("NetPosition");
                XmlDocument doc = new XmlDocument();
                doc.Load(xmlPath);
                XmlNode netXmlNode = doc.ChildNodes[1].ChildNodes[0];

                foreach (XmlNode groupNode in netXmlNode.ChildNodes)
                {
                    Guid groupId = new Guid(groupNode.Attributes["ID"].Value);
                    string groupCode = groupNode.Attributes["Code"].Value;
                    AccountGroupGNP accountGroupGNP = new AccountGroupGNP(groupId, groupCode);

                    foreach (XmlNode accountNode in groupNode.ChildNodes[0].ChildNodes)
                    {
                        Guid accountId = new Guid(accountNode.Attributes["ID"].Value);
                        string accountCode = accountNode.Attributes["Code"].Value;
                        AccountType type = (AccountType)Enum.ToObject(typeof(AccountType), int.Parse(accountNode.Attributes["Type"].Value));
                        AccountGNP accountGNP = new AccountGNP(accountId, accountCode, type);

                        foreach (XmlNode instrumentNode in accountNode.ChildNodes[0].ChildNodes)
                        {
                            Guid instrumentId = new Guid(instrumentNode.Attributes["ID"].Value);
                            decimal lotBalance = decimal.Parse(instrumentNode.Attributes["LotBalance"].Value);
                            decimal quantity = decimal.Parse(instrumentNode.Attributes["Quantity"].Value);
                            decimal buyQuantity = decimal.Parse(instrumentNode.Attributes["BuyQuantity"].Value);
                            string buyAveragePrice = instrumentNode.Attributes["BuyAveragePrice"].Value;
                            decimal buyMultiplyValue = decimal.Parse(instrumentNode.Attributes["BuyMultiplyValue"].Value);
                            decimal sellQuantity = decimal.Parse(instrumentNode.Attributes["SellQuantity"].Value);
                            string sellAveragePrice = instrumentNode.Attributes["SellAveragePrice"].Value;
                            decimal sellMultiplyValue = decimal.Parse(instrumentNode.Attributes["SellMultiplyValue"].Value);

                            InstrumentGNP instrumentGNP = new InstrumentGNP(instrumentId);
                            instrumentGNP.LotBalance = lotBalance;
                            instrumentGNP.Quantity = quantity;
                            instrumentGNP.BuyQuantity = buyQuantity;
                            instrumentGNP.BuyAveragePrice = buyAveragePrice;
                            instrumentGNP.BuyMultiplyValue = buyMultiplyValue;
                            instrumentGNP.SellQuantity = sellQuantity;
                            instrumentGNP.SellAveragePrice = sellAveragePrice;
                            instrumentGNP.SellMultiplyValue = sellMultiplyValue;

                            accountGNP.InstrumentGNPs.Add(instrumentGNP);
                        }
                        accountGroupGNP.AccountGNPs.Add(accountGNP);
                    }
                    accountGroups.Add(accountGroupGNP);
                }
            }
            catch (Exception ex)
            { 
            }
            
            return accountGroups;
        }

        public static List<AccountGroupGNP> GetNetPosition()
        {
            return GetGroupNetPosition();
        }

        public static List<OpenInterestSummary> GetInstrumentSummary()
        {
            List<OpenInterestSummary> openInterestSummarys = new List<OpenInterestSummary>();

            string xmlPath = GetCommandXmlPath("InstrumentSummary");
            XmlDocument doc = new XmlDocument();
            doc.Load(xmlPath);
            XmlNodeList XmlNodeList = doc.ChildNodes[1].ChildNodes;

            foreach (XmlNode instrumentNode in XmlNodeList)
            {
                OpenInterestSummary instrumentSummary = new OpenInterestSummary();
                instrumentSummary.UpdateByXmlRowAttribute(instrumentNode);
                openInterestSummarys.Add(instrumentSummary);
            }
            return openInterestSummarys;
        }

        public static List<OpenInterestSummary> GetAccountSummary()
        {
            List<OpenInterestSummary> openInterestSummarys = new List<OpenInterestSummary>();
            
            string xmlPath = GetCommandXmlPath("AccountSummary");
            XmlDocument doc = new XmlDocument();
            doc.Load(xmlPath);
            XmlNodeList XmlNodeList = doc.ChildNodes[1].ChildNodes;

            foreach (XmlNode summaryNode in XmlNodeList)
            {
                OpenInterestSummary summaryItem = new OpenInterestSummary();
                summaryItem.UpdateByXmlRowAttribute(summaryNode);
                openInterestSummarys.Add(summaryItem);
            }

            int i = 1;
            foreach (OpenInterestSummary item in openInterestSummarys)
            {
                Tuple<string,Guid, string> group = ExchangeData.GetAccountGroup("WF01", item.Id);
                //Just test
                item.GroupId = Guid.NewGuid();
                item.GroupCode = "Demo";
                item.Code = "Account00" + i;
                i++;
            }
            return openInterestSummarys;
        }

        public static List<OpenInterestSummary> GetOrderSummary(AccountType accountType)
        {
            List<OpenInterestSummary> orderSummaryItems = new List<OpenInterestSummary>();

            string xmlPath = GetCommandXmlPath("OrderSummary");
            XmlDocument doc = new XmlDocument();
            doc.Load(xmlPath);
            XmlNodeList transactionNodes = doc.ChildNodes[1].ChildNodes;

            foreach (XmlNode tranNode in transactionNodes)
            {
                Guid transactionId = new Guid(tranNode.Attributes["ID"].Value);
                decimal contractSize = decimal.Parse(tranNode.Attributes["ContractSize"].Value);
                Guid instrumentId = new Guid(tranNode.Attributes["InstrumentID"].Value);
                string executeTime = tranNode.Attributes["ExecuteTime"].Value;
               
                foreach (XmlNode orderNode in tranNode.ChildNodes)
                {
                    Guid orderId = new Guid(orderNode.Attributes["ID"].Value);
                    bool isBuy = bool.Parse(orderNode.Attributes["IsBuy"].Value);
                    decimal lotBalance = decimal.Parse(orderNode.Attributes["LotBalance"].Value);
                    string executePrice = orderNode.Attributes["ExecutePrice"].Value;

                    OpenInterestSummary orderSummaryItem = OrderSummaryItemSetItem(accountType, executePrice, lotBalance, isBuy, contractSize, executeTime);
                    orderSummaryItems.Add(orderSummaryItem);
                }
            }
            return orderSummaryItems;
        }

        private static OpenInterestSummary OrderSummaryItemSetItem(AccountType accountType, string executePrice, decimal lotBalance, bool isBuy, decimal contractSize, string executeTime)
        {
            OpenInterestSummary orderSummaryItem = new OpenInterestSummary();
            var executePriceValue = XmlConvert.ToDecimal(executePrice);
            decimal buyLot = isBuy ? lotBalance : decimal.Zero;
            decimal sellLot = !isBuy ? lotBalance : decimal.Zero;
            orderSummaryItem.Code = executeTime;
            orderSummaryItem.BuyLot = buyLot;
            orderSummaryItem.BuyAvgPrice = isBuy ? executePrice : "0";
            orderSummaryItem.BuyContractSize = buyLot * contractSize;
            orderSummaryItem.SellLot = sellLot;
            orderSummaryItem.SellAvgPrice = !isBuy ? executePrice : "0";
            orderSummaryItem.SellContractSize = sellLot * contractSize;
            if (accountType == AccountType.Company)  //Company
            {
                orderSummaryItem.NetLot = sellLot - buyLot;
                orderSummaryItem.NetContractSize = sellLot * contractSize - buyLot * contractSize;
            }
            else
            {
                orderSummaryItem.NetLot = buyLot - sellLot;
                orderSummaryItem.NetContractSize = buyLot * contractSize - sellLot * contractSize;
            }
            orderSummaryItem.NetAvgPrice = isBuy ? executePrice : "-" + executePrice;

            return orderSummaryItem;
        }

        private static string GetCommandXmlPath(string xmlFileName)
        {
            string appDir = System.IO.Path.Combine(Assembly.GetEntryAssembly().Location.Substring(0, Assembly.GetEntryAssembly().Location.LastIndexOf(System.IO.Path.DirectorySeparatorChar)));
            string rootpath = GetDestDirName(appDir);
            string xmlPath = string.Empty;
            xmlPath = System.IO.Path.Combine(rootpath, "Xml\\" + xmlFileName + ".xml");
            return xmlPath;
        }

        private static string GetDestDirName(string appDir)
        {
            string desDirName = string.Empty;
            desDirName = appDir.Substring(0, appDir.LastIndexOf(System.IO.Path.DirectorySeparatorChar));
            desDirName = desDirName.Substring(0, desDirName.LastIndexOf(System.IO.Path.DirectorySeparatorChar));
            return desDirName;
        }
    }
}
