using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ManagerConsole
{
    public enum OrderTypeCategory
    {
        DQ,
        Pending
    }
    public enum HandleAction
    {
        None = -1,
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
    public enum OrderStatus
    {
        Placing = 255,
        Placed = 0,
        Canceled = 1,
        Executed = 2,
        Completed = 3,
        Deleted = 4,

        WaitOutPriceDQ = 5,
        WaitOutPriceLMT = 6,
        WaitServerResponse = 7,
        Deleting = 8,
        WaitOutLotDQ = 9,
        WaitOutLotLMT = 10,
        WaitOutLotLMTOrigin = 11,
        SendFailed = 12,
        WaitNextPrice = 13,
        WaitTime = 14,
        TimeArrived = 15,
        WaitAcceptRejectPlace = 16,
        WaitAcceptRejectCancel = 17,

        WaitAutoExecuteDQ = 18,
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
    public enum ExecuteOrderSummaryType
    {
        Instrument = 0,
        Range = 1,
        Order = 2,
    }
    public enum RangeType
    {
        Time = 0,
        Price = 1,
    }

    public enum OpenInterestSummaryType
    {
        Empty = -1,
        Instrument = 0,
        Group = 1,
        Account = 2,
        Order = 3,
    }

    public enum BSStatus
    {
        None = -1,
        Sell = 0,
        Buy = 1,
        Both = 2,
    }

    public enum PriceTrend
    {
        NoChange,
        Up,
        Down
    }

    public enum SettingTaskType
    {
        InstrumentParameter = 3,
    }

    

}
