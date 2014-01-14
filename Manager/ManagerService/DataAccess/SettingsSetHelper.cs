using iExchange.Common;
using iExchange.Common.Manager;
using Manager.Common;
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
            try
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
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(TraceEventType.Error, "Initialize.SettingSet:{0}, Error:\r\n{1}", ex.ToString());
            }
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
            order.SetPrice = (string)dr["SetPrice"];
            order.ExecutePrice = (string)dr["ExecutePrice"];
            order.Lot = (decimal)dr["Lot"];
            order.LotBalance = (decimal)dr["LotBalance"];
            order.MinLot = (decimal)dr["MinLot"];
            order.MaxShow = (string)dr["MaxShow"];
            order.BeginTime = (DateTime)dr["BeginTime"];
            order.EndTime = (DateTime)dr["EndTime"];
            order.SubmitTime = (DateTime)dr["SubmitTime"];
            order.ExecuteTime = (DateTime)dr["ExecuteTime"];
            order.HitCount = (int)dr["HitCount"];
            order.BestPrice = (string)dr["BestPrice"];
            order.BestTime = (DateTime)dr["BestTime"];
            order.ApproverID = (Guid)dr["ApproverID"];
            order.SubmitorID = (Guid)dr["SubmitorID"];
            order.DQMaxMove = (int)dr["DQMaxMove"];
            order.ExpireType = dr["ExpireType"].ConvertToEnumValue<ExpireType>();
            order.SetPrice2 = (string)dr["SetPrice2"];
            order.AssigningOrderID = (Guid)dr["AssigningOrderID"];
            order.BlotterCode = (string)dr["BlotterCode"];
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
        }
    }
}
