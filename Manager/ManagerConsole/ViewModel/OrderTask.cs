using ManagerConsole.Helper;
using ManagerConsole.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using ManagerCommon = Manager.Common;

namespace ManagerConsole.ViewModel
{
    public class OrderTask : PropertyChangedNotifier
    {
        public OrderTask()
        { 
        }
        public OrderTask(Order order)
        {
            this._BaseOrder = order;
            this.Update(order);
        }


        #region Private Property
        private Order _BaseOrder;
        private bool _IsSelected = false;
        private string _ExchangeCode;
        private Account _Account;
        private Transaction _Transaction;
        private InstrumentClient _Instrument;
        private Guid _OrderId;
        private string _Code;
        private ManagerCommon.Phase _Phase;
        private OrderStatus _OrderStatus = OrderStatus.Placing;
        private string _OrderStatuString;
        private DateTime? _TimeStamp;
        private DateTime? _SubmitDateTime;
        private BuySell _IsBuy;
        private OpenClose _IsOPen;
        private decimal? _Lot;
        private ManagerCommon.TradeOption _TradeOption;
        private string _SetPrice;
        private string _BestPrice;
        private int _DiffPrice;
        private int _DQMaxMove;
        private int? _HitCount = 0;
        private DateTime? _BestTime;
        private int? _ContractSize;
        private ManagerCommon.OrderType _OrderType;
        private DateTime? _ExpireTime;
        private string _OpenPrice;
        private bool _IsExecutedStatus = false;

        private CellDataDefine _DQCellDataDefine1 = new CellDataDefine();
        private CellDataDefine _DQCellDataDefine2 = new CellDataDefine();
        private CellDataDefine _CellDataDefine1 = new CellDataDefine();
        private CellDataDefine _CellDataDefine2 = new CellDataDefine();
        private CellDataDefine _CellDataDefine3 = new CellDataDefine();
        private CellDataDefine _CellDataDefine4 = new CellDataDefine();

        #endregion

        #region Public Property
        public Order BaseOrder
        {
            get { return this._BaseOrder; }
            set 
            { 
                this._BaseOrder = value;
                this.OnPropertyChanged("BaseOrder");
                this._BaseOrder.OnOrderStatusChangedHandler += new Order.OrderStatusChangedHandler(Order_OnOrderStatusChangedHandler);
            }
        }

        void Order_OnOrderStatusChangedHandler(OrderStatus newOrderStatus)
        {
            this.ChangeOrderStatus(newOrderStatus);
        }

        public bool IsSelected
        {
            get { return this._IsSelected; }
            set { this._IsSelected = value; this.OnPropertyChanged("IsSelected"); }
        }

        public string ExchangeCode
        {
            get { return this._ExchangeCode; }
            set { this._ExchangeCode = value; }
        }

        public Account Account
        {
            get { return this._Account; }
        }

        public Transaction Transaction
        {
            get { return this._Transaction; }
        }
        public Guid OrderId
        {
            get { return this._OrderId; }
            set { this._OrderId = value; }
        }
        public string Code
        {
            get { return this._Code; }
            set { this._Code = value; }
        }
        public ManagerCommon.Phase Phase
        {
            get { return this._Phase; }
            set { this._Phase = value; }
        }

        public OrderStatus LastOrderStatus
        {
            get;
            set;
        }

        public OrderStatus OrderStatus
        {
            get { return this._OrderStatus; }
            set
            {
                if (this._OrderStatus != value)
                {
                    this._OrderStatus = value;
                    this.SetCellDataDefine(value);
                    this.OnPropertyChanged("OrderStatus");
                    this.OnPropertyChanged("OrderStatusString");
                    this.OnPropertyChanged("IsExecutedStatus");
                    if (this.OrderType == Manager.Common.OrderType.Limit && !this._IsExecutedStatus)
                    {
                        this.IsSelected = false;
                        this.OnPropertyChanged("IsSelected");
                    }
                    this.OnPropertyChanged("DQCellDataDefine1");
                    this.OnPropertyChanged("DQCellDataDefine1");
                    this.OnPropertyChanged("CellDataDefine1");
                    this.OnPropertyChanged("CellDataDefine2");
                    this.OnPropertyChanged("CellDataDefine3");
                    this.OnPropertyChanged("CellDataDefine4");
                }
            }
        }

        public string OrderStatusString
        {
            get { return this._OrderStatuString; }
            set 
            {
                this._OrderStatuString = value; 
                this.OnPropertyChanged("OrderStatuString");
            }
        }
        public InstrumentClient Instrument
        {
            get { return this._Instrument; }
            set
            {
                this._Instrument = value;
                this._Instrument.OnPriceChangedHandler += new InstrumentClient.PriceChangedHandler(Instrument_OnPriceChangedHandler);
            }
        }
        public Guid? InstrumentId
        {
            get { return this._Instrument.Id; }
        }
        public string InstrumentCode
        {
            get { return this._Instrument.Code; }
        }

        public DateTime? TimeStamp
        {
            get { return this._TimeStamp; }
            set { this._TimeStamp = value; this.OnPropertyChanged("TimeStamp"); }
        }
        public DateTime? SubmitDateTime
        {
            get { return this._SubmitDateTime; }
            set { this._SubmitDateTime = value; this.OnPropertyChanged("SubmitDateTime"); }
        }

        public string AccountCode
        {
            get { return this._Account.Code; }
        }

        public BuySell IsBuy
        {
            get { return this._IsBuy; }
            set { this._IsBuy = value; this.OnPropertyChanged("IsBuyString"); }
        }

        public string IsBuyString
        {
            get { return this._IsBuy == BuySell.Buy ? "B" : "S"; }
        }

        public OpenClose IsOpen
        {
            get { return this._IsOPen; }
            set { this._IsOPen = value; this.OnPropertyChanged("IsOpenString"); }
        }

        public string IsOpenString
        {
            get { return this._IsOPen == OpenClose.Open ? "N" : "C"; }
        }

        public SolidColorBrush IsOpenBrush
        {
            get
            {
                return this.IsOpen == OpenClose.Open ? new SolidColorBrush(Colors.Blue) : new SolidColorBrush(Colors.Red);
            }
        }
        public SolidColorBrush IsBuyBrush
        {
            get
            {
                return this.IsBuy == BuySell.Buy ? new SolidColorBrush(Colors.Blue) : new SolidColorBrush(Colors.Red);
            }
        }
        public SolidColorBrush RowBrush
        {
            get
            {
                if(this.OrderType == ManagerCommon.OrderType.MarketOnOpen|| this.OrderType == ManagerCommon.OrderType.MarketOnOpen)
                {
                    return new SolidColorBrush(Colors.Black);
                }
                else if (this.IsBuy == BuySell.Buy)
                {
                    return new SolidColorBrush(Colors.Blue);
                }
                else
                {
                    return new SolidColorBrush(Colors.Red);
                }
            }
        }
        public decimal? Lot
        {
            get { return this._Lot; }
            set { this._Lot = value; this.OnPropertyChanged("Lot"); }
        }

        public ManagerCommon.TradeOption TradeOption
        {
            get { return this._TradeOption; }
            set { this._TradeOption = value; this.OnPropertyChanged("TradeOption"); }
        }


        public string QuotePolicyCode
        {
            get { return "Default"; }
        }

        public string SetPrice
        {
            get { return this._SetPrice; }
            set
            {
                this._SetPrice = value;
                this.OnPropertyChanged("SetPrice");
            }
        }

        public string BestPrice
        {
            get { return this._BestPrice; }
            set { this._BestPrice = value; this.OnPropertyChanged("BestPrice"); }
        }

        public string ReferencePrice
        {
            get { return this._IsBuy == BuySell.Buy ? this._Instrument.Ask : this._Instrument.Bid; }
        }

        public int? DiffPrice
        {
            get { return 5; }
        }

        public int DQMaxMove
        {
            get { return this._DQMaxMove; }
            set { this._DQMaxMove = value; this.OnPropertyChanged("DQMaxMove"); }
        }

        public int? HitCount
        {
            get { return this._HitCount; }
            set
            {
                this._HitCount = value;
                this.OnPropertyChanged("HitCount");
                this.OnPropertyChanged("OrderStatus");
                this.OnPropertyChanged("IsExecutedStatus");
                this.SetCellDataDefine(this._OrderStatus);
                this.OnPropertyChanged("DQCellDataDefine1");
                this.OnPropertyChanged("DQCellDataDefine2");
                this.OnPropertyChanged("CellDataDefine1");
                this.OnPropertyChanged("CellDataDefine2");
                this.OnPropertyChanged("CellDataDefine3");
                this.OnPropertyChanged("CellDataDefine4");
            }
        }

        public DateTime? BestTime
        {
            get { return this._BestTime; }
            set { this._BestTime = value; this.OnPropertyChanged("BestTime"); }
        }

        public string TransactionCode
        {
            get { return this.Transaction.Code; }
        }

        public int? ContractSize
        {
            get { return this._ContractSize; }
            set { this._ContractSize = value; this.OnPropertyChanged("ContractSize"); }
        }

        public ManagerCommon.OrderType OrderType
        {
            get { return this._OrderType; }
            set { this._OrderType = value; this.OnPropertyChanged("OrderType"); }
        }

        public DateTime? ExpireTime
        {
            get { return this._ExpireTime; }
            set { this._ExpireTime = value; this.OnPropertyChanged("ExpireTime"); }
        }

        public string OpenPrice
        {
            get { return this._OpenPrice; }
            set { this._OpenPrice = value; this.OnPropertyChanged("OpenPrice"); }
        }

        public bool IsExecutedStatus
        {
            get
            {
                this._IsExecutedStatus =  (this.OrderType == Manager.Common.OrderType.Limit) &&
                    (this.OrderStatus == OrderStatus.WaitOutPriceLMT
                    || this.OrderStatus == OrderStatus.WaitOutLotLMT
                    || this.OrderStatus == OrderStatus.WaitOutLotLMTOrigin);
                return this._IsExecutedStatus;
            }
            set
            {
                this._IsExecutedStatus = value; 
                this.OnPropertyChanged("IsExecutedStatus");
            }
        }

        //Handle Action
        public object DQHandle
        {
            get;
            set;
        }

        public object LMTHandle
        {
            get;
            set;
        }

        public CellDataDefine CellDataDefine1
        {
            get { return this._CellDataDefine1; }
            set { this._CellDataDefine1 = value; this.OnPropertyChanged("CellDataDefine1"); }
        }
        public CellDataDefine CellDataDefine2
        {
            get { return this._CellDataDefine2; }
            set { this._CellDataDefine2 = value; this.OnPropertyChanged("CellDataDefine2"); }
        }
        public CellDataDefine CellDataDefine3
        {
            get { return this._CellDataDefine3; }
            set { this._CellDataDefine3 = value; this.OnPropertyChanged("CellDataDefine3"); }
        }
        public CellDataDefine CellDataDefine4
        {
            get { return this._CellDataDefine4; }
            set { this._CellDataDefine4 = value; this.OnPropertyChanged("CellDataDefine4"); }
        }
        public CellDataDefine DQCellDataDefine1
        {
            get { return this._DQCellDataDefine1; }
            set { this._DQCellDataDefine1 = value; this.OnPropertyChanged("DQCellDataDefine1"); }
        }
        public CellDataDefine DQCellDataDefine2
        {
            get { return this._DQCellDataDefine2; }
            set { this._DQCellDataDefine2 = value; this.OnPropertyChanged("DQCellDataDefine2"); }
        }
        #endregion

        #region Change OrderStatus
        public void ChangeOrderStatus(OrderStatus newStatus)
        {
            this.LastOrderStatus = this.OrderStatus;
            this.OrderStatus = newStatus;
        }

        private void SetOrderStatus()
        {
            if (this.OrderType == ManagerCommon.OrderType.SpotTrade)
            {
                this.ChangeOrderStatus(OrderStatus.WaitAcceptRejectPlace);
            }
        }

        void Instrument_OnPriceChangedHandler(string askBid, string newPrice)
        {
            this.OnPropertyChanged("ReferencePrice");
            if ((this._IsBuy == BuySell.Buy && string.Equals(askBid, "Ask")
                || (this._IsBuy == BuySell.Sell && string.Equals(askBid, "Bid"))))
            {
                ManagerCommon.Price setPrice = ManagerCommon.Price.CreateInstance(this._SetPrice, this._Instrument.NumeratorUnit.Value, this._Instrument.Denominator.Value);
                ManagerCommon.Price refPrice = ManagerCommon.Price.CreateInstance(newPrice, this._Instrument.NumeratorUnit.Value, this._Instrument.Denominator.Value);
                if (refPrice != null && setPrice != null)
                {
                    this._DiffPrice = Math.Abs(refPrice - setPrice);
                    this.OnPropertyChanged("DiffPrice");
                }
            }
        }
        public void SetCellDataDefine(OrderStatus orderStatus)
        {
            this._OrderStatuString = OrderStatusHelper.GetOrderStatusString(orderStatus);
            this._CellDataDefine1.IsVisibility = Visibility.Collapsed;
            this._CellDataDefine2.IsVisibility = Visibility.Collapsed;
            this._CellDataDefine3.IsVisibility = Visibility.Collapsed;
            this._CellDataDefine4.IsVisibility = Visibility.Collapsed;
            if (this.OrderStatus == OrderStatus.WaitAcceptRejectPlace)
            {
                if (this.OrderType ==ManagerCommon.OrderType.SpotTrade)
                {
                    this._DQCellDataDefine1.ColumnWidth = 60;
                    this._DQCellDataDefine1.Action = HandleAction.OnOrderAccept;
                    this._DQCellDataDefine1.Caption = "Accept";
                    this._DQCellDataDefine1.IsVisibility = Visibility.Visible;
                    this._DQCellDataDefine2.ColumnWidth = 60;
                    this._DQCellDataDefine2.Action = HandleAction.OnOrderReject;
                    this._DQCellDataDefine2.Caption = "Reject";
                    this._DQCellDataDefine2.IsVisibility = Visibility.Visible;
                    this._DQCellDataDefine2.FontColor = new SolidColorBrush(Colors.Red);
                }
                else
                {
                    this._CellDataDefine1.ColumnWidth = 120;
                    this._CellDataDefine1.Action = HandleAction.OnOrderAcceptPlace;
                    this._CellDataDefine1.Caption = "Accept";
                    this._CellDataDefine1.IsVisibility = Visibility.Visible;

                    this._CellDataDefine2.ColumnWidth = 120;
                    this._CellDataDefine2.Action = HandleAction.OnOrderRejectPlace;
                    this._CellDataDefine2.Caption = "Reject";
                    this._CellDataDefine2.FontColor = new SolidColorBrush(Colors.Red);
                    this._CellDataDefine2.IsVisibility = Visibility.Visible;

                    this._DQCellDataDefine1.ColumnWidth = 60;
                    this._DQCellDataDefine1.Action = HandleAction.OnOrderAcceptPlace;
                    this._DQCellDataDefine1.Caption = "Accept";
                    this._DQCellDataDefine1.IsVisibility = Visibility.Visible;
                    this._DQCellDataDefine2.ColumnWidth = 60;
                    this._DQCellDataDefine2.Action = HandleAction.OnOrderRejectPlace;
                    this._DQCellDataDefine2.Caption = "Reject";
                    this._DQCellDataDefine2.FontColor = new SolidColorBrush(Colors.Red);
                    this._DQCellDataDefine2.IsVisibility = Visibility.Visible;
                }
                return;
            }

            //DQ
            if (this.OrderType == ManagerCommon.OrderType.SpotTrade)
            {
                var btnAcceptIsEnable = (this.OrderStatus == OrderStatus.WaitAutoExecuteDQ) ? false : true;
                var btnRejectIsEnable = (this.OrderStatus == OrderStatus.WaitAutoExecuteDQ) ? false : true;
                this._DQCellDataDefine1.ColumnWidth = 60;
                this._DQCellDataDefine1.Action = HandleAction.OnOrderAccept;
                this._DQCellDataDefine1.Caption = "Accept";
                this._DQCellDataDefine1.IsEnable = btnAcceptIsEnable;
                this._DQCellDataDefine1.IsVisibility = Visibility.Visible;
                this._DQCellDataDefine2.ColumnWidth = 60;
                this._DQCellDataDefine2.Action = HandleAction.OnOrderReject;
                this._DQCellDataDefine2.Caption = "Reject";
                this._DQCellDataDefine2.IsVisibility = Visibility.Visible;
                this._DQCellDataDefine2.IsEnable = btnRejectIsEnable;
            }
            else if (this.OrderType == ManagerCommon.OrderType.Limit || this.OrderType == ManagerCommon.OrderType.OneCancelOther || this.OrderType == ManagerCommon.OrderType.Market)
            {
                if ((this.OrderType == ManagerCommon.OrderType.Limit || this.OrderType == ManagerCommon.OrderType.OneCancelOther)
                            && this.OrderStatus == OrderStatus.WaitAcceptRejectCancel)
                {
                    this._CellDataDefine1.ColumnWidth = 120;
                    this._CellDataDefine1.IsVisibility = Visibility.Visible;
                    this._CellDataDefine2.ColumnWidth = 120;
                    this._CellDataDefine2.IsVisibility = Visibility.Visible;
                }
                else if (this.OrderStatus == OrderStatus.WaitOutPriceLMT 
                        || this.OrderStatus == OrderStatus.WaitOutLotLMTOrigin 
                        || this.OrderStatus == OrderStatus.WaitOutLotLMT 
                        || this.Transaction.SubType == ManagerCommon.TransactionSubType.IfDone)
                {
                    bool btnUpdateIsEnable = (this.OrderStatus == OrderStatus.WaitOutPriceLMT) ? false : true;
                    bool btnModifyIsEnable = (this.OrderStatus == OrderStatus.WaitOutLotLMT) ? true : false;
                    this._CellDataDefine1.ColumnWidth = 60;
                    this._CellDataDefine1.Action = HandleAction.OnOrderUpdate;
                    this._CellDataDefine1.Caption = "uPdate";
                    this._CellDataDefine1.IsEnable = btnUpdateIsEnable;
                    this._CellDataDefine1.IsVisibility = Visibility.Visible;

                    this._CellDataDefine2.ColumnWidth = 60;
                    this._CellDataDefine2.Action = HandleAction.OnOrderModify;
                    this._CellDataDefine2.Caption = "modiFy";
                    this._CellDataDefine2.IsEnable = btnModifyIsEnable;
                    this._CellDataDefine2.IsVisibility = Visibility.Visible;

                    this._CellDataDefine3.ColumnWidth = 60;
                    this._CellDataDefine3.Action = HandleAction.OnOrderWait;
                    this._CellDataDefine3.Caption = "waIt";
                    this._CellDataDefine3.IsEnable = true;
                    this._CellDataDefine3.IsVisibility = Visibility.Visible;

                    this._CellDataDefine4.ColumnWidth = 60;
                    this._CellDataDefine4.Action = HandleAction.OnOrderExecute;
                    this._CellDataDefine4.Caption = "Execute";
                    this._CellDataDefine3.IsEnable = true;
                    this._CellDataDefine4.IsVisibility = Visibility.Visible;

                }
                else if (this.OrderType == ManagerCommon.OrderType.Limit)
                {
                    this._DQCellDataDefine1.ColumnWidth = 120;
                    this._DQCellDataDefine1.Action = HandleAction.OnOrderCancel;
                    this._DQCellDataDefine1.Caption = "Cancel";
                    this._DQCellDataDefine1.IsEnable = true;
                    this._DQCellDataDefine1.IsVisibility = Visibility.Visible;
                }
                else if (this.OrderType == ManagerCommon.OrderType.Market)
                {

                }
            }
            else if (this.OrderType == ManagerCommon.OrderType.MarketOnOpen || this.OrderType == ManagerCommon.OrderType.MarketOnClose)
            {
                this._CellDataDefine4.ColumnWidth = 48;
                this._CellDataDefine4.Action = HandleAction.OnOrderDetail;
                this._CellDataDefine4.Caption = "Execute";
                this._CellDataDefine4.IsVisibility = Visibility.Visible;
            }
        }

        #region Handle Action
        public void ResetHit()
        {
            this._HitCount = 0;
        }
        public void ChangeStatus(OrderStatus orderStatus)
        {
            this.OrderStatus = orderStatus;
            this.SetCellDataDefine(this.OrderStatus);
        }
        public void DoAcceptPlace()
        {
            if (this.OrderStatus == OrderStatus.WaitAcceptRejectPlace)
            {
                this.ChangeStatus(OrderStatus.WaitServerResponse);
            }
        }
        public void DoWait()
        {
            if (this.OrderStatus == OrderStatus.WaitOutPriceLMT || this.OrderStatus == OrderStatus.WaitOutLotLMT
           || this.OrderStatus == OrderStatus.WaitOutLotLMTOrigin)
            {
                this.ChangeStatus(OrderStatus.WaitNextPrice);
            }
        }
        public void DoReject()
        {
            this.ChangeStatus(OrderStatus.Deleting);
        }
        #endregion
        #endregion

        internal void Update(Order order)
        {
            this._ExchangeCode = order.ExchangeCode;
            this._OrderId = order.Id;
            this._Code = order.Code;
            this._Phase = order.Phase;
            this._OrderStatus = order.Status;
            this._IsBuy = order.BuySell;
            this._IsOPen = order.OpenClose;
            this._Lot = order.Lot;
            this._OrderType = order.Transaction.OrderType;
            this._SetPrice = order.SetPrice;
            this._SubmitDateTime = order.Transaction.SubmitTime;
            this._ExpireTime = order.Transaction.SubmitTime;
            this._Account = order.Transaction.Account;
            this._Transaction = order.Transaction;
            this._Instrument = order.Transaction.Instrument;
            this._HitCount = order.HitCount;
            this._BestPrice = order.BestPrice;
            this._BestTime = order.BestTime;
            this.ChangeStatus(this._OrderStatus);
        }

    }

    public class CellDataDefine
    {
        public CellDataDefine()
        {
            this.ColumnWidth = 60;
            this.FontColor = new SolidColorBrush(Colors.Blue);
            this.IsEnable = true;
            this.IsVisibility = Visibility.Collapsed;
        }
        public string Caption { get; set; }
        public int ColumnWidth { get; set; }
        public SolidColorBrush FontColor { get; set; }
        public bool IsEnable { get; set; }
        public Visibility IsVisibility { get; set; }
        public HandleAction Action { get; set; }
    }

    public static class OrderStatusHelper
    {
        public static string GetOrderStatusString(this OrderStatus orderStatus)
        {
            var statusStr = "";
            switch (orderStatus)
            {
                case OrderStatus.Placing:
                    statusStr = "Placing";
                    break;
                case OrderStatus.Placed:
                    statusStr = "Pending";
                    break;
                case OrderStatus.Canceled:
                    statusStr = "Canceled";
                    break;
                case OrderStatus.Executed:
                    statusStr = "Executed";
                    break;
                case OrderStatus.Completed:
                    statusStr = "Completed";
                    break;
                case OrderStatus.Deleted:
                    statusStr = "Deleted";
                    break;
                case OrderStatus.WaitServerResponse:
                    statusStr = "Wait server response";
                    break;
                case OrderStatus.Deleting:
                    statusStr = "Deleting";
                    break;
                case OrderStatus.WaitOutPriceDQ:
                    statusStr = "Out of HiLo, Accept or Reject?";
                    break;
                case OrderStatus.WaitOutLotDQ:
                    statusStr = "Accept or Reject?";
                    break;
                case OrderStatus.WaitOutPriceLMT:
                    statusStr = "Out of HiLo, Wait or Execute?";
                    break;
                case OrderStatus.WaitOutLotLMTOrigin:
                    statusStr = "Update, Wait or Execute?";
                    break;
                case OrderStatus.WaitOutLotLMT:
                    statusStr = "Update, Modify, Wait or Execute?";
                    break;
                case OrderStatus.SendFailed:
                    statusStr = "Send failed.";
                    break;
                case OrderStatus.WaitNextPrice:
                    statusStr = "Wait for price.";
                    break;
                case OrderStatus.WaitTime:
                    statusStr = "Wait time arrive.";
                    break;
                case OrderStatus.TimeArrived:
                    statusStr = "Time arrived, wait response.";
                    break;
                case OrderStatus.WaitAcceptRejectPlace:
                    statusStr = "Accept or Reject Place?";
                    break;
                case OrderStatus.WaitAcceptRejectCancel:
                    statusStr = "Accept or Reject Cancel?";
                    break;
                case OrderStatus.WaitAutoExecuteDQ:
                    statusStr = "Wait for price.";
                    break;
            }
            return statusStr;
        }
    }
}
