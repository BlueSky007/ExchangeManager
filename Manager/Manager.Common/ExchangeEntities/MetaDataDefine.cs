using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manager.Common.ExchangeEntities
{
    public enum ExchangeMetadataType
    {
        Instrument,
        Account,
        Customer,
        SystemParameter,
        QuotePolicy,
        QuotePolicyDetail,
        TradePolicy,
        TradePolicyDetail,
        DealingPolicyDetail,
        TradeDay,
    }
    public class ExchangeFieldSR
    {
        //Common
        public const string ID = "ID";
        public const string InstrumentId = "InstrumentId";
        public const string QuotePolicyId = "QuotePolicyId";
        public const string TradePolicyId = "TradePolicyId";
        public const string Name = "Name";
        public const string Code = "Code";
        public const string Description = "Description";
        //SystemParameter
        public const string IsCustomerVisibleToDealer = "IsCustomerVisibleToDealer";
        public const string CanDealerViewAccountInfo = "CanDealerViewAccountInfo";
        public const string DealerUsingAccountPermission = "DealerUsingAccountPermission";
        public const string MooMocAcceptDuration = "MooMocAcceptDuration";
        public const string LotDecimal = "LotDecimal";
        public const string MooMocCancelDuration = "MooMocCancelDuration";
        public const string QuotePolicyDetailID = "QuotePolicyDetailID";
        //Instrument
        public const string IsPriceEnabled = "IsPriceEnabled";
        public const string IsAutoEnablePrice = "IsAutoEnablePrice";
        public const string SummaryUnit = "SummaryUnit";
        public const string SummaryQuantity = "SummaryQuantity";
        //Account
        public const string GroupID = "GroupID";
        public const string GroupCode = "GroupCode";
        public const string Type = "Type";
        public const string CustomerID = "CustomerID";
        public const string TradePolicyID = "TradePolicyID";
        //Customer
        public const string PrivateQuotePolicyId = "PrivateQuotePolicyId";
        public const string PublicQuotePolicyId = "PublicQuotePolicyId";
        //QuotePolicy.QuotePolicyDetail
        public const string IsDefault = "IsDefault";
        //TradePolicy.TradePolicyDetail
        public const string ContractSize = "ContractSize";
        public const string QuotationMask = "QuotationMask";

    }
}
