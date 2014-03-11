using iExchange.Common;
using iExchange.Common.Manager;
using Manager.Common;
using Manager.Common.ExchangeEntities;
using Manager.Common.ReportEntities;
using Manager.Common.Settings;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using OrderType = iExchange.Common.OrderType;
using PriceType = iExchange.Common.PriceType;

namespace ManagerService.DataAccess
{
    internal static class InitializationHelper
    {
        internal delegate void Initiliaze<T>(T value, SqlDataReader reader);
        internal static T Create<T>(SqlDataReader reader, Initiliaze<T> initailize)
            where T : new()
        {
            T value = new T();
            while (reader.Read())
            {
                initailize(value, reader);
            }
            reader.NextResult();
            return value;
        }

        internal static T[] CreateArray<T>(SqlDataReader reader, Initiliaze<T> initailize)
            where T : new()
        {
            //T[] values = new T[reader.FieldCount];
            List<T> valueList = new List<T>();
            while (reader.Read())
            {
                T value = new T();
                initailize(value, reader);
                valueList.Add(value);
            }
            reader.NextResult();
            return valueList.ToArray();
        }

        internal static T GetItemValue<T>(this SqlDataReader reader, string name, T defaul)
        {
            return reader[name] == DBNull.Value ? defaul : (T)reader[name];
        }
    }
    public static class SettingsSetHelper
    {
        public static void Initialize(this SettingSet settingSet, SqlDataReader dr)
        {
            settingSet.TradeDay = InitializationHelper.Create<TradeDay>(dr, Initialize);
            settingSet.SystemParameter = InitializationHelper.Create<SystemParameter>(dr, Initialize);
            settingSet.Customers = InitializationHelper.CreateArray<Customer>(dr, Initialize);
            settingSet.AccountGroups = InitializationHelper.CreateArray<AccountGroup>(dr, Initialize);
            settingSet.Accounts = InitializationHelper.CreateArray<Account>(dr, Initialize);
            settingSet.QuotePolicies = InitializationHelper.CreateArray<QuotePolicy>(dr, Initialize);
            settingSet.QuotePolicyDetails = InitializationHelper.CreateArray<QuotePolicyDetail>(dr, Initialize);
            settingSet.TradePolicies = InitializationHelper.CreateArray<TradePolicy>(dr, Initialize);
            settingSet.TradePolicyDetails = InitializationHelper.CreateArray<TradePolicyDetail>(dr, Initialize);
            settingSet.Instruments = InitializationHelper.CreateArray<Instrument>(dr, Initialize);
            dr.NextResult();
            settingSet.OverridedQuotations = InitializationHelper.CreateArray<Manager.Common.Settings.OverridedQuotation>(dr, Initialize);
            dr.NextResult();
            settingSet.Orders = InitializationHelper.CreateArray<Order>(dr, Initialize);
        }

        public static SettingSet GetExchangeDataChangeByAccountChange(SqlDataReader dr)
        {
            SettingSet settingSet = new SettingSet();
            settingSet.TradeDay = InitializationHelper.Create<TradeDay>(dr, Initialize);
            settingSet.Customers = InitializationHelper.CreateArray<Customer>(dr, Initialize);
            settingSet.AccountGroups = InitializationHelper.CreateArray<AccountGroup>(dr, Initialize);
            settingSet.Accounts = InitializationHelper.CreateArray<Account>(dr, Initialize);
            settingSet.QuotePolicyDetails = InitializationHelper.CreateArray<QuotePolicyDetail>(dr, Initialize);
            return settingSet;
        }

        public static SettingSet GetExchangeDataChangeByInstrumentChange(SqlDataReader dr)
        {
            SettingSet settingSet = new SettingSet();
            settingSet.TradeDay = InitializationHelper.Create<TradeDay>(dr, Initialize);
            settingSet.QuotePolicies = InitializationHelper.CreateArray<QuotePolicy>(dr, Initialize);
            settingSet.QuotePolicyDetails = InitializationHelper.CreateArray<QuotePolicyDetail>(dr, Initialize);
            settingSet.TradePolicies = InitializationHelper.CreateArray<TradePolicy>(dr, Initialize);
            settingSet.TradePolicyDetails = InitializationHelper.CreateArray<TradePolicyDetail>(dr, Initialize);
            settingSet.Instruments = InitializationHelper.CreateArray<Instrument>(dr, Initialize);
            settingSet.OverridedQuotations = InitializationHelper.CreateArray<Manager.Common.Settings.OverridedQuotation>(dr, Initialize);
            return settingSet;
        }


        private static void Initialize(TradeDay tradeDay, SqlDataReader dr)
        {
            tradeDay.CurrentDay = (DateTime)dr["TradeDay"];
            tradeDay.BeginTime = (DateTime)dr["BeginTime"];
            tradeDay.EndTime = (DateTime)dr["EndTime"];
        }

        private static void Initialize(SystemParameter systemParameter, SqlDataReader dr)
        {
            systemParameter.IsCustomerVisibleToDealer = (bool)dr["IsCustomerVisibleToDealer"];
            systemParameter.MooMocAcceptDuration = (int)dr["MooMocAcceptDuration"];
            systemParameter.MooMocCancelDuration = (int)dr["MooMocCancelDuration"];
            systemParameter.QuotePolicyDetailID = (Guid)dr["QuotePolicyDetailID"];
            systemParameter.CanDealerViewAccountInfo = (bool)dr["CanDealerViewAccountInfo"];
            systemParameter.LotDecimal = (int)dr["LotDecimal"];
            systemParameter.DealerUsingAccountPermission = (bool)dr["DealerUsingAccountPermission"];
        }

        private static void Initialize(Customer customer, SqlDataReader dr)
        {
            customer.Id = (Guid)dr["ID"];
            customer.Code = (string)dr["Code"];
            if (dr["PrivateQuotePolicyID"] != DBNull.Value)
            {
                customer.PrivateQuotePolicyId = (Guid)dr["PrivateQuotePolicyID"];
            }
            if (dr["PublicQuotePolicyID"] != DBNull.Value)
            {
                customer.PublicQuotePolicyId = (Guid)dr["PublicQuotePolicyID"];
            }
            if (dr["DealingPolicyID"] != DBNull.Value)
            {
                customer.DealingPolicyId = (Guid)dr["DealingPolicyID"];
            }
        }

        private static void Initialize(AccountGroup accountGroup, SqlDataReader dr)
        {
            accountGroup.Id = (Guid)dr["GroupID"];
            accountGroup.Code = (string)dr["GroupCode"];
        }

        private static void Initialize(Account account, SqlDataReader dr)
        {
            account.Id = (Guid)dr["ID"];
            account.Code = (string)dr["Code"];
            account.Name = dr.GetItemValue<string>("Name", null);
            account.CustomerId = (Guid)dr["CustomerID"];
            account.TradePolicyId = (Guid)dr["TradePolicyID"];
            account.GroupId = (Guid)dr["GroupId"];
            account.GroupCode = (string)dr["GroupCode"];
            account.AccountType = (AccountType)dr["Type"];
        }

        private static void Initialize(QuotePolicy quotePolicy, SqlDataReader dr)
        {
            quotePolicy.Id = (Guid)dr["ID"];
            quotePolicy.Code = (string)dr["Code"];
            quotePolicy.Description = dr.GetItemValue<string>("Description", null);
            quotePolicy.IsDefault = (bool)dr["IsDefault"];
        }

        private static void Initialize(QuotePolicyDetail quotePolicyDetail, SqlDataReader dr)
        {
            quotePolicyDetail.InstrumentId = (Guid)dr["InstrumentID"];
            quotePolicyDetail.QuotePolicyId = (Guid)dr["QuotePolicyID"];
            quotePolicyDetail.PriceType = dr["PriceType"].ConvertToEnumValue<PriceType>();
            quotePolicyDetail.AutoAdjustPoints = (int)dr["AutoAdjustPoints"];
            quotePolicyDetail.AutoAdjustPoints2 = (string)dr["AutoAdjustPoints2"];
            quotePolicyDetail.AutoAdjustPoints3 = (string)dr["AutoAdjustPoints3"];
            quotePolicyDetail.AutoAdjustPoints4 = (string)dr["AutoAdjustPoints4"];
            quotePolicyDetail.SpreadPoints = (int)dr["SpreadPoints"];
            quotePolicyDetail.SpreadPoints2 = (string)dr["SpreadPoints2"];
            quotePolicyDetail.SpreadPoints3 = (string)dr["SpreadPoints3"];
            quotePolicyDetail.SpreadPoints4 = (string)dr["SpreadPoints4"];
            quotePolicyDetail.IsOriginHiLo = (bool)dr["IsOriginHiLo"];
            quotePolicyDetail.MaxAutoAdjustPoints = (int)dr["MaxAutoAdjustPoints"];
            quotePolicyDetail.MaxSpreadPoints = (int)dr["MaxSpreadPoints"];
            quotePolicyDetail.InstrumentId = (Guid)dr["InstrumentID"];
            quotePolicyDetail.BuyLot = (decimal)dr["BuyLot"];
            quotePolicyDetail.SellLot = (decimal)dr["SellLot"];
        }

        private static void Initialize(TradePolicy tradePolicy, SqlDataReader dr)
        {
            tradePolicy.Id = (Guid)dr["ID"];
            tradePolicy.Code = (string)dr["Code"];
            tradePolicy.Description = dr.GetItemValue<string>("Description", null);

            if(dr["AlertLevel1"] != null)
            {
                tradePolicy.AlertLevel1 = dr.GetItemValue<decimal>("AlertLevel1", 0);
            }
            if (dr["AlertLevel2"] != null)
            {
                tradePolicy.AlertLevel2 = dr.GetItemValue<decimal>("AlertLevel2", 0);
            }
            if (dr["AlertLevel3"] != null)
            {
                tradePolicy.AlertLevel3 = dr.GetItemValue<decimal>("AlertLevel3", 0);
            }
        }

        private static void Initialize(TradePolicyDetail tradePolicyDetail, SqlDataReader dr)
        {
            tradePolicyDetail.InstrumentId = (Guid)dr["InstrumentID"];
            tradePolicyDetail.TradePolicyId = (Guid)dr["TradePolicyID"];
            tradePolicyDetail.ContractSize = (decimal)dr["ContractSize"];
            tradePolicyDetail.QuotationMask = (int)dr["QuotationMask"];
        }

        private static void Initialize(Instrument instrument, SqlDataReader dr)
        {
            instrument.Id = (Guid)dr["ID"];
            instrument.OriginCode = (string)dr["OriginCode"];
            instrument.Code = (string)dr["Code"];
            instrument.Category = dr["Category"].ConvertToEnumValue<InstrumentCategory>();
            instrument.IsActive = (bool)dr["IsActive"];
            instrument.BeginTime = (DateTime)dr["BeginTime"];
            instrument.EndTime = (DateTime)dr["EndTime"];
            instrument.Denominator = (int)dr["Denominator"];
            instrument.NumeratorUnit = (int)dr["NumeratorUnit"];
            instrument.IsSinglePrice = (bool)dr["IsSinglePrice"];
            instrument.IsNormal = (bool)dr["IsNormal"];
            instrument.OriginType = dr["OriginType"].ConvertToEnumValue<OriginType>();
            instrument.AllowedSpotTradeOrderSides = (byte)dr["AllowedSpotTradeOrderSides"];
            instrument.OriginInactiveTime = (int)dr["OriginInactiveTime"];
            instrument.AlertVariation = (int)dr["AlertVariation"];
            instrument.NormalWaitTime = (int)dr["NormalWaitTime"];
            instrument.AlertWaitTime = (int)dr["AlertWaitTime"];
            instrument.AlertVariation = (int)dr["OriginInactiveTime"];
            instrument.AlertVariation = (int)dr["OriginInactiveTime"];
            instrument.MaxDQLot = dr.GetItemValue<decimal>("MaxDQLot", 0);
            instrument.MaxOtherLot = dr.GetItemValue<decimal>("MaxOtherLot", 0);
            instrument.DqQuoteMinLot = dr.GetItemValue<decimal>("DQQuoteMinLot", 0);
            instrument.AutoDQMaxLot = dr.GetItemValue<decimal>("AutoDQMaxLot", 0);
            instrument.AutoLmtMktMaxLot = dr.GetItemValue<decimal>("AutoLmtMktMaxLot", 0);
            instrument.AcceptDQVariation = (int)dr["AcceptDQVariation"];
            instrument.AcceptLmtVariation = (int)dr["AcceptLmtVariation"];
            instrument.AcceptCloseLmtVariation = (int)dr["AcceptCloseLmtVariation"];
            instrument.CancelLmtVariation = (int)dr["CancelLmtVariation"];
            instrument.MaxMinAdjust = (int)dr["MaxMinAdjust"];
            instrument.IsBetterPrice = (bool)dr["IsBetterPrice"];
            instrument.HitTimes = (short)dr["HitTimes"];
            instrument.PenetrationPoint = (int)dr["PenetrationPoint"];
            instrument.PriceValidTime = (int)dr["PriceValidTime"];
            instrument.DailyMaxMove = (int)dr["DailyMaxMove"];
            instrument.LastAcceptTimeSpan = TimeSpan.FromMinutes((int)dr["LastAcceptTimeSpan"]);
            instrument.OrderTypeMask = (int)dr["OrderTypeMask"];
            instrument.PreviousClosePrice = dr.GetItemValue<string>("Close", string.Empty); 
            instrument.AutoCancelMaxLot = dr.GetItemValue<decimal>("AutoCancelMaxLot", 0);
            instrument.AutoAcceptMaxLot = dr.GetItemValue<decimal>("AutoAcceptMaxLot", 0);
            instrument.AllowedNewTradeSides = Convert.ToInt16(dr["AllowedNewTradeSides"]);
            instrument.Mit = (bool)dr["Mit"];
            instrument.IsAutoEnablePrice = (bool)dr["IsAutoEnablePrice"];
            instrument.IsAutoFill = (bool)dr["IsAutoFill"];
            instrument.IsPriceEnabled = (bool)dr["IsPriceEnabled"];
            instrument.IsAutoEnablePrice = (bool)dr["IsAutoEnablePrice"];
            instrument.NextDayOpenTime = dr.GetItemValue<DateTime?>("NextDayOpenTime", null);
            instrument.MOCTime = dr.GetItemValue<DateTime?>("MOCTime", null);
            instrument.DayOpenTime = DateTime.Parse(dr["DayOpenTime"].ToString());
            instrument.DayCloseTime =  DateTime.Parse(dr["DayCloseTime"].ToString());
            instrument.AutoDQDelay = (short)dr["AutoDQDelay"];
            instrument.SummaryGroupId = dr.GetItemValue<Guid?>("SummaryGroupId", null);
            instrument.SummaryGroupCode = dr.GetItemValue<string>("SummaryGroupCode", null);
            instrument.SummaryUnit = (decimal)((double)dr["SummaryUnit"]);
            instrument.SummaryQuantity = (decimal)dr["SummaryQuantity"];
            instrument.BuyLot = (decimal)dr["BuyLot"];
            instrument.SellLot = (decimal)dr["SellLot"];
            instrument.HitPriceVariationForSTP = (int)dr["HitPriceVariationForSTP"];
        }

        private static void Initialize(Order order, SqlDataReader dr)
        {
            order.Id = (Guid)dr["ID"];
            order.Code = (string)dr["Code"];
            order.Phase = dr["Phase"].ConvertToEnumValue<OrderPhase>();
            order.TransactionId = (Guid)dr["TransactionId"];
            order.TransactionCode = (string)dr["TransactionCode"];
            order.TransactionType = dr["TransactionType"].ConvertToEnumValue<TransactionType>();
            order.TransactionSubType = dr["TransactionSubType"].ConvertToEnumValue<TransactionSubType>();
            order.TradeOption = dr["TradeOption"].ConvertToEnumValue<TradeOption>();
            order.OrderType = dr["OrderTypeID"].ConvertToEnumValue<OrderType>();
            order.IsOpen = (bool)dr["IsOpen"];
            order.IsBuy = (bool)dr["IsBuy"];
            order.AccountId = (Guid)dr["AccountID"];
            order.InstrumentId = (Guid)dr["InstrumentID"];
            order.ContractSize = (decimal)dr["ContractSize"];
            order.SetPrice = dr.GetItemValue<string>("SetPrice", string.Empty); 
            order.ExecutePrice = dr.GetItemValue<string>("ExecutePrice", string.Empty);
            order.Lot = (decimal)dr["Lot"];
            order.LotBalance = (decimal)dr["LotBalance"];
            order.MinLot = dr.GetItemValue<decimal>("MinLot", decimal.Zero);
            order.MaxShow = dr.GetItemValue<string>("MaxShow", string.Empty);
            order.BeginTime = (DateTime)dr["BeginTime"];
            order.EndTime = (DateTime)dr["EndTime"];
            order.SubmitTime = (DateTime)dr["SubmitTime"];
            order.ExecuteTime = dr.GetItemValue<DateTime>("ExecuteTime", DateTime.MaxValue);
            order.HitCount = (short)dr["HitCount"];
            order.BestPrice = dr.GetItemValue<string>("BestPrice", string.Empty);
            order.BestTime = dr.GetItemValue<DateTime>("BestTime", DateTime.MaxValue);
            order.ApproverID = dr.GetItemValue<Guid>("ApproverID", Guid.Empty); 
            order.SubmitorID = (Guid)dr["SubmitorID"];
            order.DQMaxMove = (int)dr["DQMaxMove"];
            order.ExpireType = dr["ExpireType"].ConvertToEnumValue<ExpireType>();
            order.SetPrice2 = dr.GetItemValue<string>("SetPrice2", string.Empty);
            order.AssigningOrderID = dr.GetItemValue<Guid>("AssigningOrderID", Guid.Empty);
            order.BlotterCode = dr.GetItemValue<string>("BlotterCode", string.Empty);
        }

        private static void Initialize(Manager.Common.Settings.OverridedQuotation overridedQuotations, SqlDataReader dr)
        {
            overridedQuotations.InstrumentId = (Guid)dr["InstrumentID"];
            overridedQuotations.QuotePolicyId = (Guid)dr["QuotePolicyID"];
            overridedQuotations.Timestamp = (DateTime)dr["Timestamp"];
            overridedQuotations.Ask = dr["Ask"].ToString();
            overridedQuotations.Bid = dr["Bid"].ToString();
            overridedQuotations.High = dr["High"].ToString();
            overridedQuotations.Low = dr["Low"].ToString();
            overridedQuotations.Origin = dr["Origin"].ToString();
        }
    }

    public class AccountReportDataHelper
    {
        public static void ConvertReportEntity(AccountStatusEntity entity, SqlDataReader dr)
        {
            entity.OrganizationCode = (string)dr["OrganizationCode"];
            entity.AccountCode = (string)dr["AccountCode"];
            entity.AccountName = (string)dr["AaccountName"];
            entity.CurrencyCode = (string)dr["CurrencyCode"];
            entity.IsMultiCurrency = (bool)dr["IsMultiCurrency"];
            entity.Credit = (decimal)dr["Credit"];
            entity.CrLot = (decimal)dr["CrLot"];
            entity.StartDate = (DateTime)dr["StartDate"];
            entity.SaleCode = (string)dr["SaleCode"];
            entity.AccountGroupCode = (string)dr["AccountGroupCode"];
            entity.SMAmount = (decimal)dr["SMAmount"];
            entity.AccountDescription = (string)dr["AccountDescription"];
        }

        public static void ConvertReportEntity(AccountHedgingLevel entity, SqlDataReader dr)
        {
            if (dr.FieldCount == 1)
            {
                entity.HedgingLevelString = (string)dr["HedgingLevel"];
                return;
            }
            entity.InstrumentID = (Guid)dr["InstrumentID"];
            entity.InstrumentCode = (string)dr["InstrumentCode"];
            entity.PLTrade = (decimal)dr["PLTrade"];
            entity.NetLot = (decimal)dr["NetLot"];
            entity.BuyLotBalance = (decimal)dr["BuyLotBalance"];
            entity.SellLotBalance = (decimal)dr["SellLotBalance"];
            entity.AverageBuyPrice = (decimal)dr["AverageBuyPrice"];
            entity.AverageSellPrice = (decimal)dr["AverageSellPrice"];
            entity.CallPriceString = (string)dr["CallPriceString"];
            entity.CutPriceString = (string)dr["CutPriceString"];
            entity.RateIn = (decimal)(double)dr["RateIn"];
            entity.RateOut = (decimal)(double)dr["RateOut"];
        }

        public static void ConvertReportEntity(TradingSummary accountTradingSummary,List<AccountCurrency> accountCurrencies, SqlDataReader dr)
        {
            Currency currency = new Currency();
            currency.Id = (Guid)dr["CurrencyID"];
            currency.Code = (string)dr["CurrencyCode"];

            if (accountTradingSummary != null)
            {
                accountTradingSummary.Floating = (decimal)dr["AccFloating"];
                accountTradingSummary.Balance = (decimal)dr["AccBalance"];
                accountTradingSummary.NotValue = (decimal)dr["AccNotValue"];
                accountTradingSummary.Equity = (decimal)dr["AccEquity"];
                accountTradingSummary.Necessary = (decimal)dr["AccNecessary"];
                accountTradingSummary.Usable = (decimal)dr["AccUsable"];
                accountTradingSummary.Ratio = (decimal)dr["AccRatio"];
                if (dr["AccOverNightNecessary"] == DBNull.Value)
                {
                    accountTradingSummary.OverNightNecessary = decimal.Zero;
                }
                else
                {
                    accountTradingSummary.OverNightNecessary = (decimal)(double)dr["AccOverNightNecessary"];
                }
                accountTradingSummary.OverNightUsable = (decimal)(double)dr["AccOverNightUsable"];
                // accountTradingSummary.Deposit = (decimal)dr["AccDeposit"];
                accountTradingSummary.ValueAsMargin = (decimal)dr["AccValueAsMargin"];
                accountTradingSummary.TotalPaidAmount = (decimal)dr["AccTotalPaidAmount"];
                accountTradingSummary.PartialPaymentPhysicalNecessary = (decimal)dr["AccPartialPaymentPhysicalNecessary"];
            }

            TradingSummary currencyTradingSummary = new TradingSummary();
            currencyTradingSummary.Floating = (decimal)dr["CurFloating"];
            currencyTradingSummary.Balance = (decimal)dr["CurBalance"];
            currencyTradingSummary.NotValue = (decimal)dr["CurNotValue"];
            currencyTradingSummary.Equity = (decimal)dr["CurEquity"];
            currencyTradingSummary.Necessary = (decimal)dr["CurNecessary"];
            currencyTradingSummary.Usable = (decimal)dr["CurUsable"];
            currencyTradingSummary.Ratio = (decimal)dr["CurRatio"];
            currencyTradingSummary.OverNightNecessary = (decimal)dr["CurOverNightNecessary"];
            currencyTradingSummary.OverNightUsable = (decimal)dr["CurOverNightUsable"];
            currencyTradingSummary.Deposit = (decimal)dr["CurDeposit"];
            currencyTradingSummary.Adjustment = dr.GetItemValue<decimal>("CurAdjustment", decimal.Zero);
            currencyTradingSummary.ValueAsMargin = (decimal)dr["CurValueAsMargin"];
            currencyTradingSummary.TotalPaidAmount = (decimal)dr["CurTotalPaidAmount"];

            AccountCurrency accountCurrency = new AccountCurrency() { Currency = currency,CurrencyTradingSummary = currencyTradingSummary};
            accountCurrencies.Add(accountCurrency);
        }

        public static void ConvertReportEntity(List<AccountStatusOrder> accountOpenPostions, SqlDataReader dr)
        {
            AccountStatusOrder entity = new AccountStatusOrder();
            entity.TradeDay = (string)dr["TradeDay"];
            entity.OrderCode = (string)dr["OrderCode"];
            //entity.Phase = (decimal)dr["Phase"];
            entity.InstrumentCode = (string)dr["Item"];
            entity.IsBuyString = (string)dr["BS"];
            entity.Lot = (string)dr["Lot"];
            entity.Price = (decimal)(double)dr["Price"];
            entity.MktPrice = (string)dr["MktPrice"];
            entity.CommissionSum = (decimal)dr["CommissionSum"];
            entity.LevySum = (decimal)dr["LevySum"];
            entity.RateIn = (decimal)(double)dr["RateIn"];
            entity.RateOut = (decimal)(double)dr["RateOut"];
            entity.Interest = (decimal)dr["Interest"];
            entity.Storage = (decimal)dr["Storage"];
            entity.FloatTrade = (decimal)dr["FloatTrade"];
            entity.Decimals = (short)dr["Decimals"];

            accountOpenPostions.Add(entity);
        }
    }
}
