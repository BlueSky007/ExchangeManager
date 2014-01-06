using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
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
        LogAudit = 6,
    }

    public enum ModuleType
    {
        UserManager,
        RoleManager,
        InstrumentManager,
        QuotePolicy,
        SettingParameter,
        SettingScheduler,

        QuotationSource,
        SourceQuotation,
        QuotationMonitor,
        AbnormalQuotation,
        IExchangeQuotation,
        AdjustSpreadSetting,
        SourceRelation,

        Quote,
        OrderProcess,
        LimitBatchProcess,
        MooMocProcess,
        DQOrderProcess,

        OrderSearch,
        ExecutedOrder,
        OpenInterest,
        LogAuditQuery,
    }
    public enum Language
    {
        CHT,
        CHS,
        ENG
    }
    public enum PriceType
    {
        WatchOnly = 0,
        ReferenceOnly = 1,
        DealingEnable = 2,
        OriginEnable = 3
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
        Initialize = 3,
        ChangeGroup = 4
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

    public enum OrderRelationType
    {
        Close,
        OCO,
        Assignment,
        IfDone,
        ChangeToOCO
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

    public enum LogType
    {
        QuotePrice = 1,
        QuoteOrder = 2,
        AdjustPrice = 3,
        SourceChange = 4,
        SettingChange = 5,
        Permisstion = 6,
    }

    public enum OperationType
    {
        OnOrderAccept = 0,
        OnOrderReject = 1,
        OnOrderDetail = 2,
        OnOrderAcceptPlace = 3,
        OnOrderRejectPlace = 4,
        OnOrderAcceptCancel = 5,
        OnOrderRejectCancel = 6,
        OnOrderUpdate = 7,
        OnOrderModify = 8,
        OnOrderWait = 9,
        OnOrderExecute = 10,
        OnOrderCancel = 11,
        OnLMTExecute = 12,
    }

    public enum PriceOperationType
    {
        SendPrice = 0,
        OutOfRangeAccept = 1,
        OutOfRangeReject = 2,
    }

    public enum SoundType
    {
        DQTrade = 0,
        LimitTrade = 1,
        SystemMessage = 2,
    }

    #region Task Scheduler
    public enum QuotePolicyDetailsSetAction
    {
        AdjustUp,
        AdjustDn,
        AdjustReplace,
        SpreadUp,
        SpreadDn,
        SpreadReplace
    }

    public enum TaskStatus
    {
        Ready = 0,
        Run = 1,
        Disable = 2,
    }

    public enum ActionType
    {
        Daily = 0,
        Weekly = 1,
        Monthly = 2,
        OneTime = 3,
    }

    public enum TaskType
    {
        SetParameterTask = 0,
        Other = 1,
    }

    public enum SettingParameterType
    {
        All = 0,
        SystemParameter = 1,
        SetValueParameter = 2,
        InstrumentParameter = 3,
    }
    #endregion

    public static class EnumHelper
    {
        public static TEnum ConvertToEnumValue<TEnum>(this object value)
        {
            int underlyVlaue = 0;
            if (!int.TryParse(value.ToString(), out underlyVlaue)
                || !Enum.IsDefined(typeof(TEnum), underlyVlaue))
            {
                string info = string.Format("{0} is not a valid value of {1}", value, typeof(TEnum));
                throw new InvalidCastException(info);
            }
            else
            {
                return (TEnum)Enum.ToObject(typeof(TEnum), underlyVlaue);
            }
        }
    }


    public static class OperationCode
    {
        public const string Add = "Add";
        public const string Edit = "Edit";
        public const string Delete = "Delete";
    }
}
