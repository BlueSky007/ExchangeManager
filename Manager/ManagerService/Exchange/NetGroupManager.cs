using iExchange.Common;
using iExchange.Common.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

namespace ManagerService.Exchange
{
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

            List<AccountGroupGNP> accountGroups = new List<AccountGroupGNP>();


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
