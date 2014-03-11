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
        //InstrumentManager,
        //QuotePolicy,
        SettingParameter,
        SettingScheduler,

        QuotationSource,
        SourceQuotation,
        QuotationMonitor,
        AbnormalQuotation,
        ExchangeQuotation,
        AdjustSpreadSetting,
        SourceRelation,

        Quote,
        //OrderProcess,
        LimitBatchProcess,
        MooMocProcess,
        DQOrderProcess,

        OrderSearch,
        ExecutedOrder,
        OpenInterest,
        AccountStatus,
        LogAuditQuery, 
    }
    public enum Language
    {
        CHT,
        CHS,
        ENG
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

    public enum SoundType
    {
        DQTrade = 0,
        LimitTrade = 1,
        SystemMessage = 2,
    }

    public enum GroupChangeType
    {
        Account,
        Instrument
    }

    public enum AccountStatusItemType
    {
        TelephonePin,
        Necessary,
        ONNecessary,
        NotValue,
        Usable,
        ONUsable,
        Balance,
        Floating,
        Deposit,
        Equity,
        Ratio,
        Adjustment,
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
        DealingOrderParameter = 1,
        SetValueParameter = 2,
        InstrumentParameter = 3,
        SoundParameter = 4,
    }

    public enum UpdateRoleType
    {
        Modify,
        Delete
    }
    #endregion

    #region Log
    public enum LogGroup
    {
        TradingLog = 0,
        QuotationLog = 1,
        PermisstionLog = 2,
        ReportLog = 3,
        RiskMonitorLog = 4,
    }

    public enum LogType
    {
        QuotePrice = 0,
        QuoteOrder = 1,
        AdjustPrice = 2,
        SourceChange = 3,
        SettingChange = 4,
        Permisstion = 5,
    }

    public enum PriceOperationType
    {
        SendPrice = 0,
        OutOfRangeAccept = 1,
        OutOfRangeReject = 2,
    }
    #endregion



    public enum SoundOption
     {
        DQNewOrder,
        DQDealerIntervene,
        DQCancelOrder,
        DQTradeSucceed,
        DQTradeFailed,
        DQAlertHiLo,
        LimitNewOrder,
        LimitDealerIntervene,
        LimitCancelOrderRequest,
        LimitCancelOrder,
        LimitTradeSucceed,
        LimitTradeFailed,
        LimitHit,
        OutOfRange,
        Inactive,
        Enquiry,
    }


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
        public const string Modify = "Modify";
    }

    public static class SR
    {
        public const string SystemDeafult = "SystemDeafult";
        public const string LastClosed = "LastClosed";
        public const string FirstItem = "FirstItem";
    }
}
