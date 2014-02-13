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
                instrument.TimeSpan = value.Timestamp.ToShortTimeString();
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
                        instrumentQuotation.TimeSpan = item.Timestamp.ToShortTimeString();
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
                        this.Exchanges.SingleOrDefault(e => e.ExchangeCode == set.ExchangeCode && e.QuotationPolicyId == set.QoutePolicyId && e.InstruemtnId == set.InstrumentId).AutoAdjustPoints = set.Value;
                        break;
                    case InstrumentQuotationEditType.AutoAdjustPoints2:
                        this.Exchanges.SingleOrDefault(e => e.ExchangeCode == set.ExchangeCode && e.QuotationPolicyId == set.QoutePolicyId && e.InstruemtnId == set.InstrumentId).AutoAdjustPoints2 = set.Value;
                        break;
                    case InstrumentQuotationEditType.AutoAdjustPoints3:
                        this.Exchanges.SingleOrDefault(e => e.ExchangeCode == set.ExchangeCode && e.QuotationPolicyId == set.QoutePolicyId && e.InstruemtnId == set.InstrumentId).AutoAdjustPoints3 = set.Value;
                        break;
                    case InstrumentQuotationEditType.AutoAdjustPoints4:
                        this.Exchanges.SingleOrDefault(e => e.ExchangeCode == set.ExchangeCode && e.QuotationPolicyId == set.QoutePolicyId && e.InstruemtnId == set.InstrumentId).AutoAdjustPoints4 = set.Value;
                        break;
                    case InstrumentQuotationEditType.SpreadPoints:
                        this.Exchanges.SingleOrDefault(e => e.ExchangeCode == set.ExchangeCode && e.QuotationPolicyId == set.QoutePolicyId && e.InstruemtnId == set.InstrumentId).SpreadPoints = set.Value;
                        break;
                    case InstrumentQuotationEditType.SpreadPoints2:
                        this.Exchanges.SingleOrDefault(e => e.ExchangeCode == set.ExchangeCode && e.QuotationPolicyId == set.QoutePolicyId && e.InstruemtnId == set.InstrumentId).SpreadPoints2 = set.Value;
                        break;
                    case InstrumentQuotationEditType.SpreadPoints3:
                        this.Exchanges.SingleOrDefault(e => e.ExchangeCode == set.ExchangeCode && e.QuotationPolicyId == set.QoutePolicyId && e.InstruemtnId == set.InstrumentId).SpreadPoints3 = set.Value;
                        break;
                    case InstrumentQuotationEditType.SpreadPoints4:
                        this.Exchanges.SingleOrDefault(e => e.ExchangeCode == set.ExchangeCode && e.QuotationPolicyId == set.QoutePolicyId && e.InstruemtnId == set.InstrumentId).SpreadPoints4 = set.Value;
                        break;
                    case InstrumentQuotationEditType.MaxAuotAutoAdjustPointsPoints:
                        this.Exchanges.SingleOrDefault(e => e.ExchangeCode == set.ExchangeCode && e.QuotationPolicyId == set.QoutePolicyId && e.InstruemtnId == set.InstrumentId).MaxAuotAdjustPoints = set.Value;
                        break;
                    case InstrumentQuotationEditType.MaxSpreadPointsPoints:
                        this.Exchanges.SingleOrDefault(e => e.ExchangeCode == set.ExchangeCode && e.QuotationPolicyId == set.QoutePolicyId && e.InstruemtnId == set.InstrumentId).MaxSpreadPoints = set.Value;
                        break;
                    case InstrumentQuotationEditType.IsOriginHiLo:
                        this.Exchanges.SingleOrDefault(e => e.ExchangeCode == set.ExchangeCode && e.QuotationPolicyId == set.QoutePolicyId && e.InstruemtnId == set.InstrumentId).IsOriginHiLo = (set.Value ==1);
                        break;
                    case InstrumentQuotationEditType.IsAutoFill:
                        this.Exchanges.SingleOrDefault(e => e.ExchangeCode == set.ExchangeCode && e.QuotationPolicyId == set.QoutePolicyId && e.InstruemtnId == set.InstrumentId).IsAutoFill = (set.Value ==1);
                        break;
                    case InstrumentQuotationEditType.IsPriceEnabled:
                        this.Exchanges.SingleOrDefault(e => e.ExchangeCode == set.ExchangeCode && e.QuotationPolicyId == set.QoutePolicyId && e.InstruemtnId == set.InstrumentId).IsPriceEnabled = (set.Value == 1);
                        break;
                    case InstrumentQuotationEditType.IsAutoEnablePrice:
                        this.Exchanges.SingleOrDefault(e => e.ExchangeCode == set.ExchangeCode && e.QuotationPolicyId == set.QoutePolicyId && e.InstruemtnId == set.InstrumentId).IsAutoEnablePrice = (set.Value == 1);
                        break;
                    case InstrumentQuotationEditType.OrderTypeMask:
                        this.Exchanges.SingleOrDefault(e => e.ExchangeCode == set.ExchangeCode && e.QuotationPolicyId == set.QoutePolicyId && e.InstruemtnId == set.InstrumentId).OrderTypeMask = set.Value;
                        break;
                    case InstrumentQuotationEditType.Resume:
                        break;
                    case InstrumentQuotationEditType.Suspend:
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
            return instrument;
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
