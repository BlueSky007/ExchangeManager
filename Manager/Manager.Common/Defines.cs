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
        QueryReport = 5,
    }

    public enum ModuleType
    {
        UserManager = 6,
        RoleManager = 7,
        InstrumentManager = 8,
        QuoationSource = 9,
        QuotePolicy = 10,
        AbnormalQuotation = 11,
        Quote = 12,
        DQOrderTask = 13,
        LMTOrderTask = 14,
        MooMocTask = 15,

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
    public enum TransactionType
    {
        Single,
        Pair,
        OneCancelOther,
        Mapping,
        MultipleClose,
        Assign = 100,//AssigningOrderID == AssigningOrderID (the id of order been assigned from)
    }

    public enum TransactionSubType
    {
        None = 0,
        Amend,  //AssigningOrderID == AmendedOrderId (the id of order been amended)
        IfDone, //AssigningOrderID == IfOrderId (the id of order used as condition)
        Match,
        Assign, //AssigningOrderID == AssigningOrderID (id of the order been assigned from) //NotImplemented
        Mapping,
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
    public enum OrderType
    {
        SpotTrade,
        Limit,
        Market,
        MarketOnOpen,
        MarketOnClose,
        OneCancelOther,
        Risk,
        Stop,
        MultipleClose,
        MarketToLimit,
        StopLimit,
        FAK_Market
    }

    public enum OrderRelationType
    {
        Close,
        OCO,
        Assignment,
        IfDone,
        ChangeToOCO
    }

    public enum TransactionError
    {
        NoLinkedServer = -1,
        OK = 0,
        RuntimeError = 1,

        DbOperationFailed = 2,

        TransactionAlreadyExists = 3,
        HasNoOrders = 4,
        InvalidRelation = 5,
        InvalidLotBalance = 6,
        ExceedOpenLotBalance = 7,
        InvalidPrice = 8,
    }
    public enum CancelReason
    {
        CustomerCanceled,
        DealerCanceled,
        RiskMonitorCanceled,
        MooMocNewPositionNotAllowed,
        InitialOrderCanNotBeAmended,
        OrderExpired,
        InvalidPrice,

        RiskMonitorDelete,
        AccountResetFailed,
        //DealerCanceled,
        NecessaryIsNotWithinThreshold,
        MarginIsNotEnough,
        AccountIsNotTrading,
        InstrumentIsNotAccepting,
        TimingIsNotAcceptable,
        OrderTypeIsNotAcceptable,
        HasNoAccountsLocked,
        IsLockedByAgent,
        //InvalidPrice,
        LossExecutedOrderInOco,
        ExceedOpenLotBalance,
        OneCancelOther,
        //CustomerCanceled,
        AccountIsInAlerting,
        //RiskMonitorCanceled,
        //OrderExpired,
        LimitStopAddPositionNotAllowed,
        //MooMocNewPositionNotAllowed,
        TransactionCannotBeBooked,
        OnlySptMktIsAllowedForPreCheck,
        InvalidTransactionPhase,
        TransactionExpired,
        //InitialOrderCanNotBeAmended,
        OtherReason,
        PriceChanged,
        OpenOrderIsClosed,
        ReplacedWithMaxLot,
        ShortSellNotAllowed,
        ExceedMaxPhysicalValue,
        BalanceOrEquityIsShort,
        PrepaymentIsNotAllowed,
        ExistPendingLimitCloseOrder,

    }
    public enum ExpireType
    {
        Day,
        GTC,
        IOC,
        GTD,
        Session,
        FillOrKill,
        FillAndKill,

        //below are not realy expire type, all these will bo convert to GTD
        GoodTillMonthDay,
        GTF,
        GTM,
        GoodTillMonthSession
    }


}
