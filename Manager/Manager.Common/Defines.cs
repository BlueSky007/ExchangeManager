using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manager.Common
{
    public enum ConnectionState
    {
        Unknown,
        Disconnected,
        Connecting,
        Connected
    }

    public enum ModuleCategoryType
    {
        UserManager = 1,
        Configuration = 2,
        Quotation = 3,
        Dealing = 4,
        QueryReport = 5
    }

    public enum ModuleType
    {
        UserManager = 6,
        RoleManager = 7,
        InstrumentManager = 8,
        QuoationSource = 9,
        QuotePolicy = 10,
        AbnormalQuotation = 11,
        Quote = 12
    }
    public enum Language
    {
        CHT,
        CHS,
        ENG
    }
    public enum PriceType
    {
        WatchOnly,
        ReferenceOnly,
        DealingEnable,
        OriginEnable
    }
    public enum InstrumentCategory
    {
        Forex = 10,
        Physical = 20
    }
    public enum TradeOption
    {
        Invalid,
        Stop,
        Better
    }
    public enum Phase
    {
        Placing = 255,
        Placed = 0,
        Canceled,
        Executed,
        Completed,
        Deleted
    }
    public enum BuySell
    {
        Buy,
        Sell
    }

    public enum OpenClose
    {
        Open,
        Close
    }

    public enum BSStatus
    {
        None = -1,
        SellOnly = 0,
        BuyOnly = 1,
        Both = 2,
    }
    public enum UpdateAction
    {
        Add = 0,
        Modify = 1,
        Delete = 2,
    }
    public enum OriginType
    { 
        Ask = 0,
        Bid = 1,
        Avg = 2,
    }

    public enum AccountType
    {
        Common,
        Agent,
        Company,
        Transit,//used by trader, to hide history open order and disable close order
        BlackList//Is same as AccountType.Common except add notify message
    }
}
