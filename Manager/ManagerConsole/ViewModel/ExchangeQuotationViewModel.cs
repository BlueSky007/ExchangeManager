using iExchange.Common;
using Manager.Common.QuotationEntities;
using ManagerConsole.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Media;


namespace ManagerConsole.ViewModel
{
    public class ExchangeQuotationViewModel
    {
        
        public static ExchangeQuotationViewModel Instance = new ExchangeQuotationViewModel();

        public bool IsInitData = false;
        private ObservableCollection<InstrumentQuotation> _Exchanges = new ObservableCollection<InstrumentQuotation>();

        public ObservableCollection<UpdateHighLowBatchProcessInfo> HighLowBatchProcessInfos { get; set; }

        public event Action<string,Guid,Guid,DateTime,string,string> NotifyChartWindowRealTimeData;

        public ObservableCollection<InstrumentQuotation> Exchanges
        {
            get
            {
                if (!IsInitData)
                {
                    this.InitData();
                }
                return this._Exchanges;
            }
            set
            {
                this._Exchanges = value;
            }
        }

        public bool InitData()
        {
            if (this.IsInitData)
            {
                return true;
            }
            bool isSuccess = false;
            if (!App.MainFrameWindow.ExchangeDataManager.IsInitializeCompleted)
            {
                this.IsInitData = false;
                return false;
            }
            isSuccess = this.Convert(App.MainFrameWindow.ExchangeDataManager.ExchangeSettingManagers);

            this.IsInitData = isSuccess;
            return isSuccess;
        }

        public bool Load(string exchangeCode, List<OverridedQuotation> quotations)
        {
            bool isSuccess = true;
            if (!IsInitData)
            {
                isSuccess = this.InitData();
            }
            if (isSuccess)
            {
                isSuccess = this.Convert(exchangeCode, quotations);
            }
            return isSuccess;
        }

        public bool Convert(string exchangeCode, List<OverridedQuotation> quotations)
        {
            foreach (OverridedQuotation value in quotations)
            {
                InstrumentQuotation instrument = this._Exchanges.SingleOrDefault(i => i.ExchangeCode == exchangeCode && i.QuotationPolicyId == value.QuotePolicyID && i.InstruemtnId == value.InstrumentID);
                instrument.Ask = value.Ask;
                instrument.Bid = value.Bid;
                instrument.High = value.High;
                instrument.Low = value.Low;
                instrument.TimeSpan = value.Timestamp.ToLongTimeString();
                if (this.NotifyChartWindowRealTimeData!=null)
                {
                    this.NotifyChartWindowRealTimeData(exchangeCode, value.QuotePolicyID, value.InstrumentID, value.Timestamp, value.Ask, value.Bid);
                }
            }
            return true;
        }

        public bool Convert(Dictionary<string, ExchangeSettingManager> exchangeSettingManagers)
        {
            if (exchangeSettingManagers.Keys.Count == 0)
            {
                return false;
            }
            if (this._Exchanges.Count > 0)
            {
                return false;
            }
            foreach (string key in exchangeSettingManagers.Keys)
            {
                foreach (Guid quotepolicyId in exchangeSettingManagers[key].ExchangeQuotations.Keys)
                {
                    foreach (Guid instrumentId in exchangeSettingManagers[key].ExchangeQuotations[quotepolicyId].Keys)
                    {
                        InstrumentQuotation instrument = new InstrumentQuotation();
                        instrument = InstrumentQuotation.Convert(exchangeSettingManagers[key].ExchangeQuotations[quotepolicyId][instrumentId], key);
                        this._Exchanges.Add(instrument);
                    }
                }
            }
            if (this._Exchanges.Count == 0)
            {
                return false;
            }
            this.HighLowBatchProcessInfos = new ObservableCollection<UpdateHighLowBatchProcessInfo>();
            return true;
        }

        public bool Convert(Dictionary<string, OverridedQuotation[]> overridedQs)
        {
            foreach (string exchangeCode in overridedQs.Keys)
            {
                foreach (OverridedQuotation item in overridedQs[exchangeCode])
                {
                    InstrumentQuotation instrumentQuotation = this._Exchanges.SingleOrDefault(i => i.ExchangeCode == exchangeCode && i.QuotationPolicyId == item.QuotePolicyID && i.InstruemtnId == item.InstrumentID);
                    if (instrumentQuotation != null)
                    {
                        instrumentQuotation.Bid = item.Bid;
                        instrumentQuotation.Ask = item.Ask;
                        instrumentQuotation.High = item.High;
                        instrumentQuotation.Low = item.Low;
                        instrumentQuotation.TimeSpan = item.Timestamp.ToLongTimeString();
                        //instrumentQuotation.TotalVolume = item.TotalVolume;
                        //instrumentQuotation.Volume = item.Volume;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public void UpdateExchangeQuotationPolicy(List<InstrumentQuotationSet> setings)
        {
            // TODO: fix bug: SingleOrDefault return null, will cause exception.
            foreach (InstrumentQuotationSet set in setings)
            {
                if (this.Exchanges.SingleOrDefault(e => e.ExchangeCode == set.ExchangeCode && e.QuotationPolicyId == set.QoutePolicyId && e.InstruemtnId == set.InstrumentId) == null)
                {
                    return;
                }
                switch (set.type)
                {
                    case InstrumentQuotationEditType.PriceType:
                        this.Exchanges.SingleOrDefault(e => e.ExchangeCode == set.ExchangeCode && e.QuotationPolicyId == set.QoutePolicyId && e.InstruemtnId == set.InstrumentId).PriceType = (PriceType)set.Value;
                        break;
                    case InstrumentQuotationEditType.AutoAdjustPoints:
                        this.Exchanges.SingleOrDefault(e => e.ExchangeCode == set.ExchangeCode && e.QuotationPolicyId == set.QoutePolicyId && e.InstruemtnId == set.InstrumentId).AutoAdjustPoints = int.Parse(set.Value.ToString());
                        break;
                    case InstrumentQuotationEditType.AutoAdjustPoints2:
                        this.Exchanges.SingleOrDefault(e => e.ExchangeCode == set.ExchangeCode && e.QuotationPolicyId == set.QoutePolicyId && e.InstruemtnId == set.InstrumentId).AutoAdjustPoints2 = int.Parse(set.Value.ToString());
                        break;
                    case InstrumentQuotationEditType.AutoAdjustPoints3:
                        this.Exchanges.SingleOrDefault(e => e.ExchangeCode == set.ExchangeCode && e.QuotationPolicyId == set.QoutePolicyId && e.InstruemtnId == set.InstrumentId).AutoAdjustPoints3 = int.Parse(set.Value.ToString());
                        break;
                    case InstrumentQuotationEditType.AutoAdjustPoints4:
                        this.Exchanges.SingleOrDefault(e => e.ExchangeCode == set.ExchangeCode && e.QuotationPolicyId == set.QoutePolicyId && e.InstruemtnId == set.InstrumentId).AutoAdjustPoints4 = int.Parse(set.Value.ToString());
                        break;
                    case InstrumentQuotationEditType.SpreadPoints:
                        this.Exchanges.SingleOrDefault(e => e.ExchangeCode == set.ExchangeCode && e.QuotationPolicyId == set.QoutePolicyId && e.InstruemtnId == set.InstrumentId).SpreadPoints = int.Parse(set.Value.ToString());
                        break;
                    case InstrumentQuotationEditType.SpreadPoints2:
                        this.Exchanges.SingleOrDefault(e => e.ExchangeCode == set.ExchangeCode && e.QuotationPolicyId == set.QoutePolicyId && e.InstruemtnId == set.InstrumentId).SpreadPoints2 = int.Parse(set.Value.ToString());
                        break;
                    case InstrumentQuotationEditType.SpreadPoints3:
                        this.Exchanges.SingleOrDefault(e => e.ExchangeCode == set.ExchangeCode && e.QuotationPolicyId == set.QoutePolicyId && e.InstruemtnId == set.InstrumentId).SpreadPoints3 = int.Parse(set.Value.ToString());
                        break;
                    case InstrumentQuotationEditType.SpreadPoints4:
                        this.Exchanges.SingleOrDefault(e => e.ExchangeCode == set.ExchangeCode && e.QuotationPolicyId == set.QoutePolicyId && e.InstruemtnId == set.InstrumentId).SpreadPoints4 = int.Parse(set.Value.ToString());
                        break;
                    case InstrumentQuotationEditType.MaxAuotAutoAdjustPointsPoints:
                        this.Exchanges.SingleOrDefault(e => e.ExchangeCode == set.ExchangeCode && e.QuotationPolicyId == set.QoutePolicyId && e.InstruemtnId == set.InstrumentId).MaxAuotAdjustPoints = int.Parse(set.Value.ToString());
                        break;
                    case InstrumentQuotationEditType.MaxSpreadPointsPoints:
                        this.Exchanges.SingleOrDefault(e => e.ExchangeCode == set.ExchangeCode && e.QuotationPolicyId == set.QoutePolicyId && e.InstruemtnId == set.InstrumentId).MaxSpreadPoints = int.Parse(set.Value.ToString());
                        break;
                    case InstrumentQuotationEditType.IsOriginHiLo:
                        this.Exchanges.SingleOrDefault(e => e.ExchangeCode == set.ExchangeCode && e.QuotationPolicyId == set.QoutePolicyId && e.InstruemtnId == set.InstrumentId).IsOriginHiLo = (int.Parse(set.Value.ToString()) == 1);
                        break;
                    case InstrumentQuotationEditType.IsAutoFill:
                        this.Exchanges.SingleOrDefault(e => e.ExchangeCode == set.ExchangeCode && e.QuotationPolicyId == set.QoutePolicyId && e.InstruemtnId == set.InstrumentId).IsAutoFill = (int.Parse(set.Value.ToString()) == 1);
                        break;
                    case InstrumentQuotationEditType.IsPriceEnabled:
                        this.Exchanges.SingleOrDefault(e => e.ExchangeCode == set.ExchangeCode && e.QuotationPolicyId == set.QoutePolicyId && e.InstruemtnId == set.InstrumentId).IsPriceEnabled = (int.Parse(set.Value.ToString()) == 1);
                        break;
                    case InstrumentQuotationEditType.IsAutoEnablePrice:
                        this.Exchanges.SingleOrDefault(e => e.ExchangeCode == set.ExchangeCode && e.QuotationPolicyId == set.QoutePolicyId && e.InstruemtnId == set.InstrumentId).IsAutoEnablePrice = (int.Parse(set.Value.ToString()) == 1);
                        break;
                    case InstrumentQuotationEditType.OrderTypeMask:
                        this.Exchanges.SingleOrDefault(e => e.ExchangeCode == set.ExchangeCode && e.QuotationPolicyId == set.QoutePolicyId && e.InstruemtnId == set.InstrumentId).OrderTypeMask = int.Parse(set.Value.ToString());
                        break;
                    case InstrumentQuotationEditType.Resume:
                        break;
                    case InstrumentQuotationEditType.Suspend:
                        break;
                    case InstrumentQuotationEditType.High:
                        break;
                    case InstrumentQuotationEditType.Low:
                        break;
                    case InstrumentQuotationEditType.AcceptLmtVariation:
                        break;
                    case InstrumentQuotationEditType.AutoDQMaxLot:
                        break;
                    case InstrumentQuotationEditType.AlertVariation:
                        break;
                    case InstrumentQuotationEditType.DqQuoteMinLot:
                        break;
                    case InstrumentQuotationEditType.MaxDQLot:
                        break;
                    case InstrumentQuotationEditType.NormalWaitTime:
                        break;
                    case InstrumentQuotationEditType.AlertWaitTime:
                        break;
                    case InstrumentQuotationEditType.MaxOtherLot:
                        break;
                    case InstrumentQuotationEditType.CancelLmtVariation:
                        break;
                    case InstrumentQuotationEditType.MaxMinAdjust:
                        break;
                    case InstrumentQuotationEditType.PenetrationPoint:
                        break;
                    case InstrumentQuotationEditType.PriceValidTime:
                        break;
                    case InstrumentQuotationEditType.AutoCancelMaxLot:
                        break;
                    case InstrumentQuotationEditType.AutoAcceptMaxLot:
                        break;
                    case InstrumentQuotationEditType.HitPriceVariationForSTP:
                        break;
                    case InstrumentQuotationEditType.AutoDQDelay:
                        break;
                    default:
                        break;
                }
            }
        }
    }

    //public class VMExchangeQuotation
    //{
    //    public string ExchangeCode { get; set; }

    //    public ObservableCollection<QuotationPolicyView> QuotationPolicies { get; set; }

    //    public VMExchangeQuotation()
    //    {
    //        QuotationPolicies = new ObservableCollection<QuotationPolicyView>();
    //    }
    //}

    //public class QuotationPolicyView
    //{
    //    public Guid QuotationPolicyId { get; set; }
    //    public string QuotationPolicyCode { get; set; }

    //    public ObservableCollection<InstrumentQuotation> InstrumentQuotations { get; set; }

    //    public QuotationPolicyView()
    //    {
    //        InstrumentQuotations = new ObservableCollection<InstrumentQuotation>();
    //    }
    //}

    public class InstrumentQuotation:INotifyPropertyChanged
    {
        private string _ExchangeCode;
        private Guid _QuotationPolicyId;
        private string _QuotationPolicyCode;
        private Guid _InstruemtnId;
        private string _InstrumentCode;
        private string _InstrumentOriginCode;
        private string _Bid = "0";
        private string _Ask = "0";
        private string _High;
        private string _Low;
        private string _Origin;
        private string _TimeSpan;
        private PriceType _PriceType;
        private int _AutoAdjustPoints1;
        private int _AutoAdjustPoints2;
        private int _AutoAdjustPoints3;
        private int _AutoAdjustPoints4;
        private int _SpreadPoints1;
        private int _SpreadPoints2;
        private int _SpreadPoints3;
        private int _SpreadPoints4;
        private int _MaxAuotAdjustPoints;
        private int _MaxSpreadPoints;
        private bool _IsOriginHiLo;
        private PriceTrend _AskTrend;
        private PriceTrend _BidTrend;
        private string _BidSchedulerId;
        private string _AskSchedulerId;
        private bool _IsAutoFill;
        private bool _IsPriceEnabled;
        private bool _IsAutoEnablePrice;
        private int _OrderTypeMask;
        private bool _AllowLimit = false;

        public int _AcceptLmtVariation;
        public decimal _AutoDQMaxLot;
        public int _AlertVariation;
        public decimal _DqQuoteMinLot;
        public decimal _MaxDQLot;
        public int _NormalWaitTime;
        public int _AlertWaitTime;
        public decimal _MaxOtherLot;
        public int _CancelLmtVariation;
        public int _MaxMinAdjust;
        public int _PenetrationPoint;
        public int _PriceValidTime;
        public decimal _AutoCancelMaxLot;
        public decimal _AutoAcceptMaxLot;
        public int _HitPriceVariationForSTP;
        public int _AutoDQDelay;

        private Scheduler _Scheduler = new Scheduler();
        
        [Browsable(false)]
        public string ExchangeCode
        {
            get { return this._ExchangeCode; }
            set
            {
                this._ExchangeCode = value;
                NotifyPropertyChanged("ExchangeCode");
            }
        }
        [Browsable(false)]
        public Guid QuotationPolicyId
        {
            get { return _QuotationPolicyId; }
            set
            {
                _QuotationPolicyId = value;
                NotifyPropertyChanged("QuotationPolicyId");
            }
        }
        [Browsable(false)]
        public string QuotationPolicyCode
        {
            get { return _QuotationPolicyCode; }
            set
            {
                _QuotationPolicyCode = value;
                NotifyPropertyChanged("QuotationPolicyCode");
            }
        }
        [Browsable(false)]
        public Guid InstruemtnId
        {
            get { return _InstruemtnId; }
            set
            {
                _InstruemtnId = value;
                NotifyPropertyChanged("InstruemtnId");
            }
        }
        [Browsable(false)]
        public string InstrumentCode
        {
            get { return _InstrumentCode; }
            set
            {
                _InstrumentCode = value;
                NotifyPropertyChanged("InstrumentCode");
            }
        }
        [Browsable(false)]
        public string InstrumentOriginCode
        {
            get { return _InstrumentOriginCode; }
            set
            {
                _InstrumentOriginCode = value;
                NotifyPropertyChanged("InstrumentOriginCode");
            }
        }
        [Browsable(false)]
        public string Bid
        {
            get { return _Bid; }
            set
            {
                if (_Bid!=value)
                {
                    this.SetPriceTrend("bid", value);
                    _Bid = value;
                    NotifyPropertyChanged("Bid");
                }
                
            }
        }
        [Browsable(false)]
        public string Ask
        {
            get { return _Ask; }
            set
            {
                if (_Ask != value)
                {
                    this.SetPriceTrend("ask", value);
                    _Ask = value;
                    NotifyPropertyChanged("Ask");
                }
            }
        }
        [Browsable(false)]
        public string High
        {
            get { return _High; }
            set
            {
                _High = value;
                NotifyPropertyChanged("High");
            }
        }
        [Browsable(false)]
        public SolidColorBrush BidTrendBrush
        {
            get { return this.GetBrush(this._BidTrend); }
        }
        [Browsable(false)]
        public SolidColorBrush AskTrendBrush
        {
            get { return this.GetBrush(this._AskTrend); }
        }
        [Browsable(false)]
        public string Low
        {
            get { return _Low; }
            set
            {
                _Low = value;
                NotifyPropertyChanged("Low");
            }
        }
        [Browsable(false)]
        public string Origin
        {
            get { return _Origin; }
            set
            {
                _Origin = value;
                NotifyPropertyChanged("Origin");
            }
        }
        [Browsable(false)]
        public string TimeSpan
        {
            get { return _TimeSpan; }
            set
            {
                _TimeSpan = value;
                NotifyPropertyChanged("TimeSpan");
            }
        }

            
        //public string TotalVolume { get; set; }
        //public string Volume { get; set; }
        [Category("Quotation")]
        public PriceType PriceType
        {
            get { return _PriceType; }
            set
            {
                _PriceType = value;
                NotifyPropertyChanged("PriceType");
            }
        }
        [Category("Quotation")]
        [DisplayName("AutoAdjustPoints")]
        public int AutoAdjustPoints
        {
            get { return _AutoAdjustPoints1; }
            set
            {
                _AutoAdjustPoints1 = value;
                NotifyPropertyChanged("AutoAdjustPoints");
            }
        }
        [Category("Quotation")]
        public int AutoAdjustPoints2
        {
            get { return _AutoAdjustPoints2; }
            set
            {
                _AutoAdjustPoints2 = value;
                NotifyPropertyChanged("AutoAdjustPoints2");
            }
        }
        [Category("Quotation")]
        public int AutoAdjustPoints3
        {
            get { return _AutoAdjustPoints3; }
            set
            {
                _AutoAdjustPoints3 = value;
                NotifyPropertyChanged("AutoAdjustPoints3");
            }
        }
        [Category("Quotation")]
        public int AutoAdjustPoints4
        {
            get { return _AutoAdjustPoints4; }
            set
            {
                _AutoAdjustPoints4 = value;
                NotifyPropertyChanged("AutoAdjustPoints4");
            }
        }
        [Category("Quotation")]
        [DisplayName("SpreadPoints")]
        public int SpreadPoints
        {
            get { return _SpreadPoints1; }
            set
            {
                _SpreadPoints1 = value;
                NotifyPropertyChanged("SpreadPoints");
            }
        }
        [Category("Quotation")]
        public int SpreadPoints2
        {
            get { return _SpreadPoints2; }
            set
            {
                _SpreadPoints2 = value;
                NotifyPropertyChanged("SpreadPoints2");
            }
        }
        [Category("Quotation")]
        public int SpreadPoints3
        {
            get { return _SpreadPoints3; }
            set
            {
                _SpreadPoints3 = value;
                NotifyPropertyChanged("SpreadPoints3");
            }
        }
        [Category("Quotation")]
        public int SpreadPoints4
        {
            get { return _SpreadPoints4; }
            set
            {
                _SpreadPoints4 = value;
                NotifyPropertyChanged("SpreadPoints4");
            }
        }
        [Category("Quotation")]
        public int MaxAuotAdjustPoints
        {
            get { return _MaxAuotAdjustPoints; }
            set
            {
                _MaxAuotAdjustPoints = value;
                NotifyPropertyChanged("MaxAuotAdjustPoints");
            }
        }
        [Category("Quotation")]
        public int MaxSpreadPoints
        {
            get { return _MaxSpreadPoints; }
            set
            {
                _MaxSpreadPoints = value;
                NotifyPropertyChanged("MaxSpreadPoints");
            }
        }
        [Category("Quotation")]
        public bool IsOriginHiLo
        {
            get { return _IsOriginHiLo; }
            set
            {
                _IsOriginHiLo = value;
                NotifyPropertyChanged("IsOriginHiLo");
            }
        }

        public bool IsAutoFill
        {
            get { return _IsAutoFill; }
            set
            {
                _IsAutoFill = value;
                NotifyPropertyChanged("IsAutoFill");
            }
        }

        public bool IsPriceEnabled
        {
            get { return _IsPriceEnabled; }
            set
            {
                _IsPriceEnabled = value;
                NotifyPropertyChanged("IsPriceEnabled");
            }
        }

        public bool IsAutoEnablePrice
        {
            get { return _IsAutoEnablePrice; }
            set
            {
                _IsAutoEnablePrice = value;
                NotifyPropertyChanged("IsAutoEnablePrice");
            }
        }

        public int OrderTypeMask
        {
            get { return _OrderTypeMask; }
            set
            {
                this._OrderTypeMask = value;
                if ((this._OrderTypeMask & 2) == 2)
                {
                    this._AllowLimit = true;
                }
                else
                {
                    this._AllowLimit = false;
                }
                NotifyPropertyChanged("AllowLimit");
            }
        }

        public bool AllowLimit
        {
            get { return _AllowLimit; }
            set
            {
                this._AllowLimit = value;
                if (this._AllowLimit)
                {
                    if ((this._OrderTypeMask & 2) != 2)
                    this._OrderTypeMask += 2;
                }
                else
                {
                    if ((this._OrderTypeMask & 2) == 2)
                    this._OrderTypeMask -= 2;
                }
                NotifyPropertyChanged("AllowLimit");
            }
        }

        public string SuspendResume
        {
            get 
            {
                if (this.IsPriceEnabled&&this.IsAutoEnablePrice)
                {
                    return "Suspend";
                }
                else
                {
                    return "Resume";
                }
            }
        }

        public int AcceptLmtVariation
        {
            get { return _AcceptLmtVariation; }
            set
            {
                _AcceptLmtVariation = value;
                NotifyPropertyChanged("AcceptLmtVariation");
            }
        }
        public decimal AutoDQMaxLot
        {
            get { return _AutoDQMaxLot; }
            set
            {
                _AutoDQMaxLot = value;
                NotifyPropertyChanged("AutoDQMaxLot");
            }
        }
        public int AlertVariation
        {
            get { return _AlertVariation; }
            set
            {
                _AlertVariation = value;
                NotifyPropertyChanged("AlertVariation");
            }
        }
        public decimal DqQuoteMinLot
        {
            get { return _DqQuoteMinLot; }
            set
            {
                _DqQuoteMinLot = value;
                NotifyPropertyChanged("DqQuoteMinLot");
            }
        }
        public decimal MaxDQLot
        {
            get { return _MaxDQLot; }
            set
            {
                _MaxDQLot = value;
                NotifyPropertyChanged("MaxDQLot");
            }
        }
        public int NormalWaitTime
        {
            get { return _NormalWaitTime; }
            set
            {
                _NormalWaitTime = value;
                NotifyPropertyChanged("NormalWaitTime");
            }
        }
        public int AlertWaitTime
        {
            get { return _AlertWaitTime; }
            set
            {
                _AlertWaitTime = value;
                NotifyPropertyChanged("AlertWaitTime");
            }
        }
        public decimal MaxOtherLot
        {
            get { return _MaxOtherLot; }
            set
            {
                _MaxOtherLot = value;
                NotifyPropertyChanged("MaxOtherLot");
            }
        }
        public int CancelLmtVariation
        {
            get { return _CancelLmtVariation; }
            set
            {
                _CancelLmtVariation = value;
                NotifyPropertyChanged("CancelLmtVariation");
            }
        }
        public int MaxMinAdjust
        {
            get { return _MaxMinAdjust; }
            set
            {
                _MaxMinAdjust = value;
                NotifyPropertyChanged("MaxMinAdjust");
            }
        }
        public int PenetrationPoint
        {
            get { return _PenetrationPoint; }
            set
            {
                _PenetrationPoint = value;
                NotifyPropertyChanged("PenetrationPoint");
            }
        }
        public int PriceValidTime
        {
            get { return _PriceValidTime; }
            set
            {
                _PriceValidTime = value;
                NotifyPropertyChanged("PriceValidTime");
            }
        }
        public decimal AutoCancelMaxLot
        {
            get { return _AutoCancelMaxLot; }
            set
            {
                _AutoCancelMaxLot = value;
                NotifyPropertyChanged("AutoCancelMaxLot");
            }
        }
        public decimal AutoAcceptMaxLot
        {
            get { return _AutoAcceptMaxLot; }
            set
            {
                _AutoAcceptMaxLot = value;
                NotifyPropertyChanged("AutoAcceptMaxLot");
            }
        }
        public int HitPriceVariationForSTP
        {
            get { return _HitPriceVariationForSTP; }
            set
            {
                _HitPriceVariationForSTP = value;
                NotifyPropertyChanged("HitPriceVariationForSTP");
            }
        }
        public int AutoDQDelay
        {
            get { return _AutoDQDelay; }
            set
            {
                _AutoDQDelay = value;
                NotifyPropertyChanged("AutoDQDelay");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info)); 
            }
        }

        public static InstrumentQuotation Convert(ExchangeQuotation quote,string exchangeCode)
        {
            InstrumentQuotation instrument = new InstrumentQuotation();
            instrument.ExchangeCode = exchangeCode;
            instrument.QuotationPolicyId = quote.QuotationPolicyId;
            instrument.QuotationPolicyCode = quote.QuotationPolicyCode;
            instrument.Ask = quote.Ask;
            instrument.Bid = quote.Bid;
            instrument.High = quote.High;
            instrument.InstruemtnId = quote.InstruemtnId;
            instrument.InstrumentCode = quote.InstrumentCode;
            instrument.InstrumentOriginCode = quote.OriginInstrumentCode;
            instrument.Low = quote.Low;
            instrument.Origin = quote.Origin;
            instrument.TimeSpan = quote.Timestamp.ToShortTimeString();
            instrument.PriceType = quote.PriceType;
            instrument.AutoAdjustPoints = quote.AutoAdjustPoints1;
            instrument.AutoAdjustPoints2 = quote.AutoAdjustPoints2;
            instrument.AutoAdjustPoints3 = quote.AutoAdjustPoints3;
            instrument.AutoAdjustPoints4 = quote.AutoAdjustPoints4;
            instrument.SpreadPoints = quote.SpreadPoints1;
            instrument.SpreadPoints2 = quote.SpreadPoints2;
            instrument.SpreadPoints3 = quote.SpreadPoints3;
            instrument.SpreadPoints4 = quote.SpreadPoints4;
            instrument.MaxAuotAdjustPoints = quote.MaxAuotAdjustPoints;
            instrument.MaxSpreadPoints = quote.MaxSpreadPoints;
            instrument.IsOriginHiLo = quote.IsOriginHiLo;
            instrument.IsAutoFill = quote.IsAutoFill;
            instrument.IsPriceEnabled = quote.IsPriceEnabled;
            instrument.IsAutoEnablePrice = quote.IsAutoEnablePrice;
            instrument.OrderTypeMask = quote.OrderTypeMask;
            instrument.AcceptLmtVariation = quote.AcceptLmtVariation;
            instrument.AutoDQMaxLot = quote.AutoDQMaxLot;
            instrument.AlertVariation = quote.AlertVariation;
            instrument.DqQuoteMinLot = quote.DqQuoteMinLot;
            instrument.MaxDQLot = quote.MaxDQLot;
            instrument.NormalWaitTime = quote.NormalWaitTime;
            instrument.AlertWaitTime = quote.AlertWaitTime;
            instrument.MaxOtherLot = quote.MaxOtherLot;
            instrument.CancelLmtVariation = quote.CancelLmtVariation;
            instrument.MaxMinAdjust = quote.MaxMinAdjust;
            instrument.PenetrationPoint = quote.PenetrationPoint;
            instrument.PriceValidTime = quote.PriceValidTime;
            instrument.AutoCancelMaxLot = quote.AutoCancelMaxLot;
            instrument.AutoAcceptMaxLot = quote.AutoAcceptMaxLot;
            return instrument;
        }

        public bool IsDealerInput(InstrumentQuotationEditType type,decimal value)
        {
            switch (type)
            {
                case InstrumentQuotationEditType.PriceType:
                    break;
                case InstrumentQuotationEditType.AutoAdjustPoints:
                    break;
                case InstrumentQuotationEditType.AutoAdjustPoints2:
                    break;
                case InstrumentQuotationEditType.AutoAdjustPoints3:
                    break;
                case InstrumentQuotationEditType.AutoAdjustPoints4:
                    break;
                case InstrumentQuotationEditType.SpreadPoints:
                    break;
                case InstrumentQuotationEditType.SpreadPoints2:
                    break;
                case InstrumentQuotationEditType.SpreadPoints3:
                    break;
                case InstrumentQuotationEditType.SpreadPoints4:
                    break;
                case InstrumentQuotationEditType.MaxAuotAutoAdjustPointsPoints:
                    break;
                case InstrumentQuotationEditType.MaxSpreadPointsPoints:
                    break;
                case InstrumentQuotationEditType.IsOriginHiLo:
                    break;
                case InstrumentQuotationEditType.IsAutoFill:
                    break;
                case InstrumentQuotationEditType.IsPriceEnabled:
                    break;
                case InstrumentQuotationEditType.IsAutoEnablePrice:
                    break;
                case InstrumentQuotationEditType.Resume:
                    break;
                case InstrumentQuotationEditType.Suspend:
                    break;
                case InstrumentQuotationEditType.OrderTypeMask:
                    break;
                case InstrumentQuotationEditType.High:
                    decimal high;
                    decimal.TryParse(this._High, out high);
                    return high != value;
                case InstrumentQuotationEditType.Low:
                    decimal low;
                    decimal.TryParse(this._Low, out low);
                    return low != value;
                default:
                    break;
            }
            return false;
        }

        private void SetPriceTrend(string propertyName, string newValue)
        {
            double oldPrice;
            if (!string.IsNullOrEmpty(newValue))
            {
                double newPrice = double.Parse(newValue);
                if (propertyName == "bid")
                {
                    if (!string.IsNullOrEmpty(this._BidSchedulerId))
                    {
                        this._Scheduler.Remove(this._BidSchedulerId);
                    }
                    oldPrice = double.Parse(this._Bid);
                    if (newPrice > oldPrice)
                    {
                        this._BidTrend = PriceTrend.Up;
                        this._BidSchedulerId = this._Scheduler.Add(this.ResetTrendState, "Bid", DateTime.Now.AddSeconds(2));
                    }
                    else if (newPrice < oldPrice)
                    {
                        this._BidTrend = PriceTrend.Down;
                        this._BidSchedulerId = this._Scheduler.Add(this.ResetTrendState, "Bid", DateTime.Now.AddSeconds(2));
                    }
                    else
                    {
                        this._BidTrend = PriceTrend.NoChange;
                    }
                    this.NotifyPropertyChanged("BidTrendBrush");
                }
                else
                {
                    if (!string.IsNullOrEmpty(this._AskSchedulerId))
                    {
                        this._Scheduler.Remove(this._AskSchedulerId);
                    }
                    oldPrice = double.Parse(this._Ask);
                    if (newPrice > oldPrice)
                    {
                        this._AskTrend = PriceTrend.Up;
                        this._AskSchedulerId = this._Scheduler.Add(this.ResetTrendState, "Ask", DateTime.Now.AddSeconds(2));
                    }
                    else if (newPrice < oldPrice)
                    {
                        this._AskTrend = PriceTrend.Down;
                        this._AskSchedulerId = this._Scheduler.Add(this.ResetTrendState, "Ask", DateTime.Now.AddSeconds(2));
                    }
                    else
                    {
                        this._AskTrend = PriceTrend.NoChange;
                    }
                    this.NotifyPropertyChanged("AskTrendBrush");
                }
            }
        }

        private void ResetTrendState(object sender, object actionArgs)
        {
            App.MainFrameWindow.Dispatcher.BeginInvoke((Action<string>)delegate(string propName)
            {
                if (propName.Equals("Ask"))
                {
                    this._AskTrend = PriceTrend.NoChange;
                    this.NotifyPropertyChanged("AskTrendBrush");
                    //Logger.AddEvent(TraceEventType.Information, "Ask:{0}, this.AskTrend = PriceTrend.NoChange;", this.Ask);
                }
                else
                {
                    this._BidTrend = PriceTrend.NoChange;
                    this.NotifyPropertyChanged("BidTrendBrush");
                    //Logger.AddEvent(TraceEventType.Information, "Ask:{0}, this.BidTrend = PriceTrend.NoChange;", this.Ask);
                }
            }, (string)actionArgs);
        }

        private SolidColorBrush GetBrush(PriceTrend trend)
        {
            if (trend == PriceTrend.Up)
            {
                return Brushes.LimeGreen;
            }
            else if (trend == PriceTrend.Down)
            {
                return Brushes.OrangeRed;
            }
            else
            {
                return Brushes.Transparent;
            }
        }
    }
}
