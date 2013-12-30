using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using ManagerConsole.Helper;
using Manager.Common.QuotationEntities;
using iExchange.Common;
using Manager.Common;
using System.Diagnostics;

namespace ManagerConsole.ViewModel
{
    public class VmQuotationBase : VmBase
    {
        private double _RawAsk;
        private double _RawBid;
        private double? _RawLast;

        private DateTime _Timestamp;
        private string _Ask;
        private string _Bid;
        private string _Last;

        private PriceTrend _AskTrend;
        private PriceTrend _BidTrend;

        private Scheduler _Scheduler = new Scheduler();
        private string _BidScheduleId;
        private string _AskScheduleId;

        public VmQuotationBase(IMetadataObject metadataObject)
            : base(metadataObject)
        {

        }

        public DateTime Timestamp
        {
            get
            {
                return this._Timestamp;
            }
            set
            {
                if (this._Timestamp != value)
                {
                    this._Timestamp = value;
                    this.OnPropertyChanged("Timestamp");
                }
            }
        }        
        public string Ask
        {
            get
            {
                return this._Ask;
            }
            set
            {
                if (this._Ask != value)
                {
                    this._Ask = value;
                    this.OnPropertyChanged("Ask");
                }
            }
        }
        public string Bid
        {
            get
            {
                return this._Bid;
            }
            set
            {
                if (this._Bid != value)
                {
                    this._Bid = value;
                    this.OnPropertyChanged("Bid");
                }
            }
        }
        public string Last
        {
            get
            {
                return this._Last;
            }
            set
            {
                if (this._Last != value)
                {
                    this._Last = value;
                    this.OnPropertyChanged("Last");
                }
            }
        }

        public PriceTrend AskTrend
        {
            get { return this._AskTrend; }
            set
            {
                if(this._AskTrend != value)
                {
                    this._AskTrend = value;
                    this.OnPropertyChanged("AskTrendBrush");
                    if (value != PriceTrend.NoChange)
                    {
                        if (this._AskScheduleId != null)
                        {
                            this._Scheduler.Remove(this._AskScheduleId);
                            //Logger.AddEvent(TraceEventType.Information, "_AskScheduleId:[{0}] removed.", this._AskScheduleId);
                        }
                        this._AskScheduleId = this._Scheduler.Add(this.ResetTrendState, "Ask", DateTime.Now.AddSeconds(5));
                        //Logger.AddEvent(TraceEventType.Information, "new _AskScheduleId:[{0}] added.", this._AskScheduleId);
                    }
                }
            }
        }
        public PriceTrend BidTrend
        {
            get { return this._BidTrend; }
            set
            {
                if(this._BidTrend != value)
                {
                    this._BidTrend = value;
                    this.OnPropertyChanged("BidTrendBrush");
                    if (value != PriceTrend.NoChange)
                    {
                        if(this._BidScheduleId != null)
                        {
                            this._Scheduler.Remove(this._BidScheduleId);
                            //Logger.AddEvent(TraceEventType.Information, "_BidScheduleId:[{0}] removed.", this._BidScheduleId);
                        }
                        this._BidScheduleId = this._Scheduler.Add(this.ResetTrendState, "Bid", DateTime.Now.AddSeconds(5));
                        //Logger.AddEvent(TraceEventType.Information, "new _BidScheduleId:[{0}] added.", this._BidScheduleId);
                    }
                }
            }
        }

        public SolidColorBrush AskTrendBrush
        {
            get
            {
                return this.GetBrush(this._AskTrend);
            }
        }

        public SolidColorBrush BidTrendBrush
        {
            get
            {
                return this.GetBrush(this._BidTrend);
            }
        }

        public void SetQuotation(GeneralQuotation generalQuotation, int decimalPlace)
        {
            string format = "F" + decimalPlace.ToString();
            this.Timestamp = generalQuotation.Timestamp;
            this.Bid = generalQuotation.Bid.ToString(format);
            this.Ask = generalQuotation.Ask.ToString(format);
            if (generalQuotation.Last.HasValue) this.Last = generalQuotation.Last.Value.ToString(format);

            this.AskTrend = this.GetPriceTrend(generalQuotation.Ask, this._RawAsk);
            this.BidTrend = this.GetPriceTrend(generalQuotation.Bid, this._RawBid);

            this._RawAsk = generalQuotation.Ask;
            this._RawBid = generalQuotation.Bid;
            this._RawLast = generalQuotation.Last;
        }

        private void ResetTrendState(object sender, object actionArgs)
        {
            if (actionArgs.Equals("Ask"))
            {
                this._AskScheduleId = null;
            }
            else
            {
                this._BidScheduleId = null;
            }
            App.MainWindow.Dispatcher.BeginInvoke((Action<string>)delegate(string propName)
            {
                if(propName.Equals("Ask"))
                {
                    this.AskTrend = PriceTrend.NoChange;
                }
                else
                {
                    this.BidTrend = PriceTrend.NoChange;
                }
            }, (string)actionArgs);
        }

        private PriceTrend GetPriceTrend(double newPrice, double oldPrice)
        {
            if (newPrice > oldPrice)
            {
                return PriceTrend.Up;
            }
            else if (newPrice < oldPrice)
            {
                return PriceTrend.Down;
            }
            return PriceTrend.NoChange;
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
