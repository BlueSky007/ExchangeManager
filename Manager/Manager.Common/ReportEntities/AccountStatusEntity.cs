using iExchange.Common;
using Manager.Common.ExchangeEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manager.Common.ReportEntities
{
    public class AccountStatusQueryResult
    {
        public List<AccountCurrency> AccountCurrencies { get; set; }
        public AccountStatusEntity AccountStatusEntity { get; set; }
        public TradingSummary AccountTradingSummary { get; set; }
        public AccountHedgingLevel AccountHedgingLevel { get; set; }

        public List<AccountStatusOrder> AccountOpenPostions { get; set; }
        public List<AccountStatusOrder> CurrentTradeDayOrders { get; set; }
        public List<AccountStatusOrder> LmtOrders { get; set; }
        public AccountStatusQueryResult()
        {
            this.AccountStatusEntity = new AccountStatusEntity();
            this.AccountCurrencies = new List<AccountCurrency>();
            this.AccountTradingSummary = new TradingSummary();
            this.AccountHedgingLevel = new AccountHedgingLevel();
        }
    }

    public class AccountStatusEntity
    {
        public string AccountCode { get; set; }
        public string AccountName { get; set; }
        public string TradeDay { get; set; }
        public string OrganizationCode { get; set; }
        public string CurrencyCode { get; set; }
        public bool IsMultiCurrency { get; set; }
        public decimal Credit { get; set; }
        public decimal CrLot { get; set; }
        public DateTime StartDate { get; set; }
        public string SaleCode { get; set; }
        public string AccountGroupCode { get; set; }
        public decimal SMAmount { get; set; }
        public string AccountDescription { get; set; }
        public decimal OverNightNecessary { get; set; }
        public decimal AccAdjustment { get; set; }
        public decimal AccDeposit { get; set; }
        public decimal Uncleared { get; set; }
    }

    public class AccountHedgingLevel
    {
        public string HedgingLevelString { get; set; }
        public Guid InstrumentID { get; set; }
        public string InstrumentCode { get; set; }
        public decimal PLTrade { get; set; }
        public decimal NetLot { get; set; }
        public decimal BuyLotBalance { get; set; }
        public decimal SellLotBalance { get; set; }
        public decimal AverageBuyPrice { get; set; }
        public decimal AverageSellPrice { get; set; }
        public string CallPriceString { get; set; }
        public string CutPriceString { get; set; }
        public decimal RateIn { get; set; }
        public decimal RateOut { get; set; }
    }

    public class AccountStatusOrder
    {
        public string TradeDay { get; set; }
        public string OrderCode { get; set; }
        public OrderPhase Phase { get; set; }
        public string InstrumentCode { get; set; }
        public string IsBuyString { get; set; }
        public string Lot { get; set; }
        public decimal Price { get; set; }
        public string MktPrice { get; set; }
        public decimal CommissionSum { get; set; }
        public decimal LevySum { get; set; }
        public decimal RateIn { get; set; }
        public decimal RateOut { get; set; }
        public decimal Interest { get; set; }
        public decimal Storage { get; set; }
        public decimal FloatTrade { get; set; }
        public short Decimals { get; set; }

        public decimal ExecutePrice { get; set; }
        public bool IsOpen { get; set; }
        public int ContractSize { get; set; }
        public string OpenPosition { get; set; }
        public decimal TradePL { get; set; }
        public string Dealer { get; set; }

        public DateTime ExecutedTime{get;set;}
        public DateTime SubmitTime { get; set; }
        public DateTime EndTime { get; set; }
        public string LotBalance { get; set; }

    }
}
