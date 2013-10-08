using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iExchange.Common;
using Manager.Common;
using System.Xml;
using System.Collections;
using ManagerCommon = Manager.Common;

namespace ManagerService.Exchange
{
    public class CommandConvertor
    {
        public static Message Convert(string exchagenCode, Command command)
        {
            return CommandConvertor.Convert(exchagenCode, (dynamic)command);
        }

        private static Message Convert(string exchagenCode, QuoteCommand quoteCommand)
        {
            //XmlNode node = quoteCommand.Content.Attributes["Quote"];
            //Guid customerId = new Guid(node.Attributes["CustomerID"].Value);
            //Guid instrumentId = new Guid(node.Attributes["InstrumentID"].Value);
            //double quoteLot = double.Parse(node.Attributes["QuoteLot"].Value);
            //int bSStatus = int.Parse(node.Attributes["BSStatus"].Value);
            //QuoteMessage quoteMessage = new QuoteMessage(customerId,instrumentId,quoteLot,bSStatus);

            QuoteMessage quoteMessage = new QuoteMessage(exchagenCode, quoteCommand.CustomerID, quoteCommand.InstrumentID, quoteCommand.QuoteLot, quoteCommand.BSStatus);
            return quoteMessage;
        }

        private static Message Convert(string exchagenCode, UpdateCommand updateCommand)
        {
            XmlNode content = updateCommand.Content;
            XmlNode node = updateCommand.Content;

            XmlElement addElement = content["Add"];
            XmlElement modifyElement = content["Modify"];
            XmlElement deleteElement = content["Delete"];

            SettingSet addedSettings = addElement == null ? null : ToSettingSet(addElement, UpdateAction.Add);
            SettingSet modifySettings = modifyElement == null ? null : ToSettingSet(modifyElement, UpdateAction.Modify);
            SettingSet deletedSettings = deleteElement == null ? null : ToSettingSet(deleteElement, UpdateAction.Delete);

            UpdateMessage updateMessage = new UpdateMessage(exchagenCode,addedSettings, modifySettings, deletedSettings);

            return updateMessage;
        }

        private static SettingSet ToSettingSet(XmlNode content, UpdateAction updateAction)
        {
            SettingSet settingSet = new SettingSet();

            List<Instrument> instruments = null;
            List<Customer> customers = null;
            List<TradePolicyDetail> tradePolicyDetails = null;
            List<QuotePolicyDetail> quotePolicyDetails = null;

            foreach (XmlNode xmlNode in content.ChildNodes)
            {
                string name = xmlNode.Name;

                if (name == "PrivateDailyQuotation")
                {
                    settingSet.PrivateDailyQuotation = new PrivateDailyQuotation();
                    settingSet.PrivateDailyQuotation.UpdateAction = updateAction;
                    settingSet.PrivateDailyQuotation.Initialize(xmlNode);
                }
                else if (name == "SystemParameter")
                {
                    settingSet.SystemParameter = new SystemParameter();
                    settingSet.SystemParameter.Initialize(xmlNode);
                }
                else if (name == "Instruments")
                {
                    foreach (XmlNode instrumentNode in xmlNode.ChildNodes)
                    {
                        Instrument instrument = new Instrument();
                        instrument.Initialize(instrumentNode);
                        if (instruments == null) instruments = new List<Instrument>();
                        instruments.Add(instrument);
                    }
                }
                else if (name == "Instrument")
                {
                    Instrument instrument = new Instrument();
                    instrument.Initialize(xmlNode);
                    if (instruments == null) instruments = new List<Instrument>();
                    instruments.Add(instrument);
                }
                else if (name == "Account")
                {
                    Account account = new Account();
                    account.Initialize(xmlNode);
                    settingSet.Accounts = new Account[] { account };
                }
                else if (name == "Customer")
                {
                    Customer customer = new Customer();
                    customer.Initialize(xmlNode);
                    settingSet.Customer = customer;
                }
                else if (name == "Customers")
                {
                    foreach (XmlNode customerNode in xmlNode.ChildNodes)
                    {
                        Customer customer = new Customer();
                        customer.Initialize(customerNode);
                        if (customers == null) customers = new List<Customer>();
                        customers.Add(customer);
                    }
                }
                else if (name == "TradePolicyDetail")
                {
                    TradePolicyDetail tradePolicyDetail = new TradePolicyDetail();
                    tradePolicyDetail.Initialize(xmlNode);

                    if (tradePolicyDetails == null) tradePolicyDetails = new List<TradePolicyDetail>();
                    tradePolicyDetails.Add(tradePolicyDetail);
                }
                else if (name == "QuotePolicyDetail")
                {
                    QuotePolicyDetail quotePolicyDetail = new QuotePolicyDetail();
                    quotePolicyDetail.Initialize(xmlNode);

                    if (quotePolicyDetails == null) quotePolicyDetails = new List<QuotePolicyDetail>();
                    quotePolicyDetails.Add(quotePolicyDetail);
                }
                else if (name == "QuotePolicyDetails")
                {
                    foreach (XmlNode childNode in xmlNode.ChildNodes)
                    {
                        QuotePolicyDetail quotePolicyDetail = new QuotePolicyDetail();
                        quotePolicyDetail.Initialize(childNode);

                        if (quotePolicyDetails == null) quotePolicyDetails = new List<QuotePolicyDetail>();
                        quotePolicyDetails.Add(quotePolicyDetail);
                    }
                }
            }

            settingSet.Instruments = (instruments.IsNullOrEmpty() ? null : instruments.ToArray());
            settingSet.Customers = (customers.IsNullOrEmpty() ? null : customers.ToArray());
            settingSet.TradePolicyDetails = (tradePolicyDetails.IsNullOrEmpty() ? null : tradePolicyDetails.ToArray());
            settingSet.QuotePolicyDetails = (quotePolicyDetails.IsNullOrEmpty() ? null : quotePolicyDetails.ToArray());
            return settingSet;
        }
    }

    internal static class CommandConvertHelper
    {
        internal static void Initialize(this PrivateDailyQuotation privateDailyQuotation, XmlNode xmlNode)
        {
            foreach (XmlAttribute attribute in xmlNode.Attributes)
            {
                String nodeName = attribute.Name;
                String nodeValue = attribute.Value;
                if (nodeName == "InstrumentID")
                {
                    privateDailyQuotation.InstrumentId = new Guid(nodeValue);
                    continue;
                }
                else if (nodeName == "TradeDay")
                {
                    privateDailyQuotation.TradeDay = DateTime.Parse(nodeValue);
                    continue;
                }
                else if (nodeName == "Open")
                {
                    privateDailyQuotation.Open = nodeValue;
                    continue;
                }
                else if (nodeName == "Close")
                {
                    privateDailyQuotation.Close = nodeValue;
                    continue;
                }
                else if (nodeName == "Bid")
                {
                    privateDailyQuotation.Bid = nodeValue;
                    continue;
                }
                else if (nodeName == "Ask")
                {
                    privateDailyQuotation.Ask = nodeValue;
                    continue;
                }
                else if (nodeName == "DayCloseTime")
                {
                    privateDailyQuotation.DayCloseTime = DateTime.Parse(nodeValue);
                    continue;
                }
            }
        }

        internal static void Initialize(this SystemParameter systemParameter, XmlNode xmlNode)
        {
            foreach (XmlAttribute attribut in xmlNode.Attributes)
            {
                String nodeName = attribut.Name;
                String nodeValue = attribut.Value;
                if (nodeName == "IsCustomerVisibleToDealer")
                {
                    systemParameter.IsCustomerVisibleToDealer = bool.Parse(nodeValue);
                    continue;
                }
                else if (nodeName == "CanDealerViewAccountInfo")
                {
                    systemParameter.CanDealerViewAccountInfo = bool.Parse(nodeValue);
                    continue;
                }
                else if (nodeName == "DealerUsingAccountPermission")
                {
                    systemParameter.DealerUsingAccountPermission = bool.Parse(nodeValue);
                    continue;
                }
                else if (nodeName == "MooMocAcceptDuration")
                {
                    systemParameter.MooMocAcceptDuration = int.Parse(nodeValue);
                    continue;
                }
                else if (nodeName == "LotDecimal")
                {
                    systemParameter.MooMocAcceptDuration = int.Parse(nodeValue);
                    continue;
                }
                else if (nodeName == "MooMocCancelDuration")
                {
                    systemParameter.MooMocAcceptDuration = int.Parse(nodeValue);
                    continue;
                }
                else if (nodeName == "QuotePolicyDetailID")
                {
                    systemParameter.QuotePolicyDetailID = new Guid(nodeValue);
                    continue;
                }
            }
        }

        internal static void Initialize(this Instrument instrument, XmlNode xmlNode)
        {
            foreach (XmlAttribute attribute in xmlNode.Attributes)
            {
                string nodeName = attribute.Name;
                string nodeValue = attribute.Value;
                if (nodeName == "ID")
                {
                    instrument.Id = new Guid(nodeValue);
                    continue;
                }
                else if (nodeName == "OriginCode")
                {
                    instrument.OriginCode = nodeValue;
                    continue;
                }
                else if (nodeName == "Code")
                {
                    instrument.Code = nodeValue;
                    continue;
                }
                else if (nodeName == "Description")
                {
                    instrument.Description = nodeValue;
                    continue;
                }
                else if (nodeName == "Denominator")
                {
                    instrument.Denominator = int.Parse(nodeValue);
                    continue;
                }
                else if (nodeName == "NumeratorUnit")
                {
                    instrument.NumeratorUnit = int.Parse(nodeValue);
                    continue;
                }
                else if (nodeName == "CommissionFormula")
                {
                    instrument.CommissionFormula = byte.Parse(nodeValue);
                    continue;
                }
                else if (nodeName == "TradePLFormula")
                {
                    instrument.TradePLFormula = byte.Parse(nodeValue);
                    continue;
                }
                //else if (nodeName == "CanPlacePendingOrderAtAnyTime")
                //{
                //    instrument.CanPlacePendingOrderAtAnyTime = bool.Parse(nodeValue);
                //    continue;
                //}
                else if (nodeName == "OrderTypeMask")
                {
                    instrument.OrderTypeMask = int.Parse(nodeValue);
                    continue;
                }
                else if (nodeName == "IsNormal")
                {
                    instrument.IsNormal = bool.Parse(nodeValue);
                    continue;
                }
                else if (nodeName == "MaxDQLot")
                {
                    instrument.MaxDQLot = decimal.Parse(nodeValue); 
                    continue;
                }
                else if (nodeName == "MaxOtherLot")
                {
                    instrument.MaxOtherLot = decimal.Parse(nodeValue); 
                    continue;
                }
                //else if (nodeName == "CurrencyID")
                //{
                //    instrument.CurrencyId = new Guid(nodeValue);
                //    continue;
                //}
                else if (nodeName == "PriceValidTime")
                {
                    instrument.PriceValidTime = int.Parse(nodeValue);
                    continue;
                }
                else if (nodeName == "DQQuoteMinLot")
                {
                    instrument.DqQuoteMinLot = decimal.Parse(nodeValue); //new BigDecimal(Double.valueOf(nodeValue).doubleValue());
                    continue;
                }
                else if (nodeName == "IsSinglePrice")
                {
                    instrument.IsSinglePrice = bool.Parse(nodeValue);
                    continue;
                }
                else if (nodeName == "BeginTime")
                {
                    instrument.BeginTime = DateTime.Parse(nodeValue);
                    continue;
                }
                else if (nodeName == "EndTime")
                {
                    instrument.EndTime = DateTime.Parse(nodeValue);
                    continue;
                }
                //else if (nodeName == "LastTradeDay")
                //{
                //    instrument.LastTradeDay = DateTime.Parse(nodeValue);
                //    continue;
                //}
                else if (nodeName == "AcceptLmtVariation")
                {
                    instrument.AcceptLmtVariation = int.Parse(nodeValue);
                    continue;
                }
                else if (nodeName == "AcceptCloseLmtVariation")
                {
                    instrument.AcceptCloseLmtVariation = int.Parse(nodeValue);
                    continue;
                }
                else if (nodeName == "CancelLmtVariation")
                {
                    instrument.CancelLmtVariation = int.Parse(nodeValue);
                    continue;
                }
                else if (nodeName == "AcceptIfDoneVariation")
                {
                    instrument.AcceptIfDoneVariation = int.Parse(nodeValue);
                    continue;
                }
                else if (nodeName == "PriceType")
                {
                    instrument.PriceType = (ManagerCommon.PriceType)(short.Parse(nodeValue));
                    continue;
                }
                //else if (nodeName == "LastAcceptTimeSpan")
                //{
                //    instrument.LastAcceptTimeSpan = TimeSpan.FromMinutes(int.Parse(nodeValue));
                //    continue;
                //}
                else if (nodeName == "DayOpenTime")
                {
                    instrument.DayOpenTime = DateTime.Parse(nodeValue);
                    continue;
                }
                //else if (nodeName == "DayCloseTime")
                //{
                //    instrument.DayCloseTime = DateTime.Parse(nodeValue);
                //    continue;
                //}
                //else if (nodeName == "LastDayCloseTime")
                //{
                //    instrument.LastDayCloseTime = DateTime.Parse(nodeValue);
                //    continue;
                //}
                else if (nodeName == "NextDayOpenTime")
                {
                    instrument.NextDayOpenTime = DateTime.Parse(nodeValue);
                    continue;
                }
                else if (nodeName == "MOCTime")
                {
                    if (string.IsNullOrEmpty(nodeValue))
                    {
                        instrument.MOCTime = null;
                    }
                    else
                    {
                        instrument.MOCTime = DateTime.Parse(nodeValue);
                    }
                    continue;
                }
                else if (nodeName == "IsActive")
                {
                    instrument.IsActive = bool.Parse(nodeValue);
                    continue;
                }
                else if (nodeName == "AcceptDQVariation")
                {
                    instrument.AcceptDQVariation = int.Parse(nodeValue);
                    continue;
                }
                else if (nodeName == "Category")
                {
                    instrument.Category = (ManagerCommon.InstrumentCategory)(int.Parse(nodeValue));
                    continue;
                }
                //else if (nodeName == "MarginFormula")
                //{
                //    instrument.MarginFormula = byte.Parse(nodeValue);
                //    continue;
                //}
                else if (nodeName == "AllowedNewTradeSides")
                {
                    instrument.AllowedNewTradeSides = int.Parse(nodeValue);
                    continue;
                }
            }
        }

        internal static void Initialize(this Account account, XmlNode xmlNode)
        {
            foreach (XmlAttribute attribute in xmlNode.Attributes)
            {
                string nodeName = attribute.Name;
                string nodeValue = attribute.Value;

                if (nodeName == "ID")
                {
                    account.Id = new Guid(nodeValue);
                    continue;
                }
                else if (nodeName == "Code")
                {
                    account.Code = nodeValue;
                    continue;
                }
                else if (nodeName == "Name")
                {
                    account.Name = nodeValue;
                    continue;
                }
                else if (nodeName == "Type")
                {
                    account.Type = (ManagerCommon.AccountType)(int.Parse(nodeValue));
                    continue;
                }
                else if (nodeName == "CustomerID")
                {
                    account.CustomerId = new Guid(nodeValue);
                    continue;
                }
                else if (nodeName == "TradePolicyID")
                {
                    account.TradePolicyId = new Guid(nodeValue);
                    continue;
                }
            }
        }

        internal static void Initialize(this Customer customer, XmlNode xmlNode)
        {
            foreach (XmlAttribute attribute in xmlNode.Attributes)
            {
                string nodeName = attribute.Name;
                string nodeValue = attribute.Value;

                if (nodeName == "ID")
                {
                    customer.Id = new Guid(nodeValue);
                    continue;
                }
                else if (nodeName == "Code")
                {
                    customer.Code = nodeValue;
                    continue;
                }
                else if (nodeName == "PublicQuotePolicyId")
                {
                    customer.PublicQuotePolicyId = new Guid(nodeValue);
                    continue;
                }
                else if (nodeName == "PrivateQuotePolicyId")
                {
                    customer.PrivateQuotePolicyId = new Guid(nodeValue);
                    continue;
                }
                else if (nodeName == "DealingPolicyId")
                {
                    customer.DealingPolicyId = new Guid(nodeValue);
                    continue;
                }
            }
        }

        internal static void Initialize(this TradePolicyDetail tradePolicyDetail, XmlNode xmlNode)
        {
            foreach (XmlAttribute xmlAttribute in xmlNode.Attributes)
            {
                String nodeName = xmlAttribute.Name;
                String nodeValue = xmlAttribute.Value;
                if (nodeName == "TradePolicyID")
                {
                    tradePolicyDetail.TradePolicyId = new Guid(nodeValue);
                    continue;
                }
                else if (nodeName == "InstrumentId")
                {
                    tradePolicyDetail.InstrumentId = new Guid(nodeValue);
                    continue;
                }
                else if (nodeName == "VolumeNecessaryId")
                {
                    tradePolicyDetail.VolumeNecessaryId = new Guid(nodeValue);
                    continue;
                }
                else if (nodeName == "ContractSize")
                {
                    tradePolicyDetail.ContractSize = decimal.Parse(nodeValue);
                    continue;
                }
            }
        }

        internal static void Initialize(this QuotePolicyDetail quotePolicyDetail, XmlNode xmlNode)
        {
            foreach (XmlAttribute xmlAttribute in xmlNode.Attributes)
            {
                String nodeName = xmlAttribute.Name;
                String nodeValue = xmlAttribute.Value;
                if (nodeName == "TradePolicyID")
                {
                    quotePolicyDetail.QuotePolicyId = new Guid(nodeValue);
                    continue;
                }
                else if (nodeName == "InstrumentId")
                {
                    quotePolicyDetail.InstrumentId = new Guid(nodeValue);
                    continue;
                }
                else if (nodeName == "PriceType")
                {
                    quotePolicyDetail.PriceType = int.Parse(nodeValue);
                    continue;
                }
                else if (nodeName == "AutoAdjustPoints")
                {
                    quotePolicyDetail.AutoAdjustPoints = int.Parse(nodeValue);
                    continue;
                }
                else if (nodeName == "AutoAdjustPoints2")
                {
                    quotePolicyDetail.AutoAdjustPoints2 = nodeValue;
                    continue;
                }
                else if (nodeName == "AutoAdjustPoints3")
                {
                    quotePolicyDetail.AutoAdjustPoints3 = nodeValue;
                    continue;
                }
                else if (nodeName == "AutoAdjustPoints4")
                {
                    quotePolicyDetail.AutoAdjustPoints4 = nodeValue;
                    continue;
                }
                else if (nodeName == "SpreadPoints")
                {
                    quotePolicyDetail.SpreadPoints = int.Parse(nodeValue);
                    continue;
                }
                else if (nodeName == "SpreadPoints2")
                {
                    quotePolicyDetail.SpreadPoints2 = nodeValue;
                    continue;
                }
                else if (nodeName == "SpreadPoints3")
                {
                    quotePolicyDetail.SpreadPoints3 = nodeValue;
                    continue;
                }
                else if (nodeName == "SpreadPoints4")
                {
                    quotePolicyDetail.SpreadPoints4 = nodeValue;
                    continue;
                }
            }
        }

        internal static bool IsNullOrEmpty(this ICollection collection)
        {
            return collection == null || collection.Count == 0;
        }
    }


}
