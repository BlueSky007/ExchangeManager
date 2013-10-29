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

        private ObservableCollection<OrderTaskDetail> _MooMocOrderTasks;
        public ObservableCollection<OrderTaskDetail> MooMocOrderTasks
        {
            get { return this._MooMocOrderTasks; }
            set { this._MooMocOrderTasks = value; }
        }


        #region Private Property
        private Order _BaseOrder;
        private bool _IsSelected = true;
        private string _ExchangeCode;
        private Account _Account;
        private Transaction _Transaction;
        private InstrumentClient _Instrument;
        private Guid _OrderId;
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
                this._OrderStatus = value;
                this.SetCellDataDefine(value);
                this.OnPropertyChanged("OrderStatus");
                this.OnPropertyChanged("DQCellDataDefine1");
                this.OnPropertyChanged("DQCellDataDefine2");
                this.OnPropertyChanged("CellDataDefine1");
                this.OnPropertyChanged("CellDataDefine2");
                this.OnPropertyChanged("CellDataDefine3");
                this.OnPropertyChanged("CellDataDefine4");
            }
        }

        public string OrderStatuString
        {
            get { return this._OrderStatuString; }
            set { this._OrderStatuString = value; }
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

        public CellDataDefine DQCellDataDefine1
        {
            get { return this._DQCellDataDefine1; }
            set { this._DQCellDataDefine1 = value; }
        }

        public CellDataDefine DQCellDataDefine2
        {
            get { return this._DQCellDataDefine2; }
            set { this._DQCellDataDefine2 = value; }
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
            this._DQCellDataDefine1.IsVisibility = Visibility.Collapsed;
            this._DQCellDataDefine2.IsVisibility = Visibility.Collapsed;
            this._CellDataDefine1.IsVisibility = Visibility.Collapsed;
            this._CellDataDefine2.IsVisibility = Visibility.Collapsed;
            this._CellDataDefine3.IsVisibility = Visibility.Collapsed;
            this._CellDataDefine4.IsVisibility = Visibility.Collapsed;
            if (this.OrderStatus == OrderStatus.WaitAcceptRejectPlace)
            {
                if (this.OrderType ==ManagerCommon.OrderType.SpotTrade)
                {
                    this._DQCellDataDefine1.Action = HandleAction.OnOrderAcceptPlace;
                    this._DQCellDataDefine1.IsVisibility = Visibility.Visible;
                    this._DQCellDataDefine2.Action = HandleAction.OnOrderRejectPlace;
                    this._DQCellDataDefine2.IsVisibility = Visibility.Visible;
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
                }
                return;
            }

            //DQ
            if (this.OrderType == ManagerCommon.OrderType.SpotTrade)
            {
                var btnAcceptIsEnable = (this.OrderStatus == OrderStatus.WaitAutoExecuteDQ) ? false : true;
                var btnRejectIsEnable = (this.OrderStatus == OrderStatus.WaitAutoExecuteDQ) ? false : true;
                this._DQCellDataDefine1.Action = HandleAction.OnOrderAccept;
                this._DQCellDataDefine1.IsVisibility = Visibility.Visible;
                this._DQCellDataDefine1.IsEnable = btnAcceptIsEnable;
                this._DQCellDataDefine2.Action = HandleAction.OnOrderReject;
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
                else if (this.OrderStatus == OrderStatus.WaitOutPriceLMT ||
                            this.OrderStatus == OrderStatus.WaitOutLotLMTOrigin ||
                            this.OrderStatus == OrderStatus.WaitOutLotLMT)//this.tran.subType == 3
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
                    this._CellDataDefine3.ColumnWidth = 240;
                    this._CellDataDefine3.Action = HandleAction.OnOrderCancel;
                    this._CellDataDefine3.Caption = "Cancel";
                    this._CellDataDefine3.IsEnable = true;
                    this._CellDataDefine3.IsVisibility = Visibility.Visible;
                }
                else if (this.OrderType == ManagerCommon.OrderType.Market)
                {

                }
            }
            else if (this.OrderType == ManagerCommon.OrderType.MarketOnOpen || this.OrderType == ManagerCommon.OrderType.MarketOnClose)
            {
                this._CellDataDefine4.ColumnWidth = 240;
                this._CellDataDefine4.Action = HandleAction.OnOrderDetail;
                this._CellDataDefine4.Caption = "Detail";
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
            this._MooMocOrderTasks = new ObservableCollection<OrderTaskDetail>();
            this.ChangeStatus(this._OrderStatus);
        }

    }

    public class OrderTaskDetail : PropertyChangedNotifier
    {
        public OrderTaskDetail(Order order)
        {
            this._AccountCode = order.Transaction.Account.Code;
            this._InstrumentClient = order.Transaction.Instrument;
            this._SubmitDateTime = order.Transaction.SubmitTime;
            this._OpenClose = order.OpenClose;
            this._BuyLot = order.BuySell == BuySell.Buy ? order.Lot : 0;
            this._SellLot = order.BuySell == BuySell.Sell ? order.Lot : 0;
        }
        #region Private Property
        private Guid? _AccountId;
        private string _AccountCode;
        private DateTime? _SubmitDateTime;
        private OpenClose _OpenClose;
        private InstrumentClient _InstrumentClient;
        private decimal? _BuyLot;
        private decimal? _SellLot;
        private string _QuotePolicyCode;

        #endregion

        #region Public Property
        public Guid? AccountId
        {
            get { return this._AccountId; }
            set { this._AccountId = value; }
        }
        public string AccountCode
        {
            get { return this._AccountCode; }
            set { this._AccountCode = value; this.OnPropertyChanged("AccountCode"); }
        }

        public Guid? InstrumentId
        {
            get { return this._InstrumentClient.Id; }
        }
        public string InstrumentCode
        {
            get { return this._InstrumentClient.Code; }
        }

        public DateTime? SubmitDateTime
        {
            get { return this._SubmitDateTime; }
            set { this._SubmitDateTime = value; this.OnPropertyChanged("SubmitDateTime"); }
        }

        public OpenClose OpenClose
        {
            get { return this._OpenClose; }
            set { this._OpenClose = value; }
        }

        public decimal? BuyLot
        {
            get { return this._BuyLot; }
            set { this._BuyLot = value; this.OnPropertyChanged("BuyLot"); }
        }

        public decimal? SellLot
        {
            get { return this._SellLot; }
            set { this._SellLot = value; this.OnPropertyChanged("SellLot"); }
        }

        public string QuotePolicyCode
        {
            get { return this._QuotePolicyCode; }
            set { this._QuotePolicyCode = value; }
        }
       
        #endregion
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
}
