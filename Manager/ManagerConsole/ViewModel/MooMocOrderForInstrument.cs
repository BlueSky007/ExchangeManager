using ManagerConsole.Helper;
using ManagerConsole.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Media;
using Scheduler = iExchange.Common.Scheduler;

namespace ManagerConsole.ViewModel
{
    public class MooMocOrderForInstrumentModel
    {
        public ObservableCollection<MooMocOrderForInstrument> MooMocOrderForInstruments { get; set; }

        public MooMocOrderForInstrumentModel()
        {
            MooMocOrderForInstruments = new ObservableCollection<MooMocOrderForInstrument>();
        }

        public void AddMooMocOrderForInstrument(OrderTask orderTask)
        {
            MooMocOrderForInstrument mooMocOrderForInstrument = null;
            mooMocOrderForInstrument = this.MooMocOrderForInstruments.SingleOrDefault(P => P.Instrument.Id == orderTask.Transaction.Instrument.Id);
            if (mooMocOrderForInstrument == null)
            {
                InstrumentClient instrument = orderTask.Transaction.Instrument;
                mooMocOrderForInstrument = new MooMocOrderForInstrument(instrument);
                this.MooMocOrderForInstruments.Add(mooMocOrderForInstrument);
            }

            mooMocOrderForInstrument.AddMooMocOrder(orderTask); 
        }

        public void RemoveMooMocOrderForInstrument(MooMocOrderForInstrument mooMocOrderForInstrument)
        {
            this.MooMocOrderForInstruments.Remove(mooMocOrderForInstrument);
        }
    }
    public class MooMocOrderForInstrument : PropertyChangedNotifier
    {
        public MooMocOrderForInstrument(InstrumentClient instrument)
        {
            this._Instrument = instrument;
            this._Ask = instrument.Ask;
            this._Bid = instrument.Bid;
            this._Origin = instrument.Origin;
            this._OrderTasks = new ObservableCollection<OrderTask>();
            this.BuySellList = new ObservableCollection<string>() { "All","Buy","Sell"};
            this.OpenCloseList = new ObservableCollection<string> { "All", "Open", "Close" };
        }

        #region Privete Property
        private InstrumentClient _Instrument;
        private ObservableCollection<OrderTask> _OrderTasks;
        private string _Ask;
        private string _Bid;
        private string _Origin;
        private decimal _SumSellLot = decimal.Zero;
        private decimal _SumBuyLot = decimal.Zero;
        
        private string _BuySellString = "All";
        private string _OpenCloseString = "All";

        private PriceTrend _PriceTrend;
        private string _PriceScheduleId;
        private Scheduler _Scheduler = new Scheduler();
        
        #endregion

        #region Public Property
        public InstrumentClient Instrument
        {
            get { return this._Instrument; }
            set { this._Instrument = value; }
        }

        public string InstrumentCode
        {
            get { return this._Instrument.Code; }
        }

        public string Bid
        {
            get { return this._Bid; }
            set { this._Bid = value; this.OnPropertyChanged("Bid"); }
        }

        public string Ask
        {
            get { return this._Ask; }
            set { this._Ask = value; this.OnPropertyChanged("Ask"); }
        }

        public string Origin
        {
            get { return this._Origin; }
            set { this._Origin = value; this.OnPropertyChanged("Origin"); }
        }

        public decimal SumBuyLot
        {
            get { return this._SumBuyLot; }
            set { this._SumBuyLot = value;}
        }

        public decimal SumSellLot
        {
            get { return this._SumSellLot; }
            set { this._SumSellLot = value; }
        }

        public string BuySellString
        {
            get { return this._BuySellString; }
            set { this._BuySellString = value; }
        }

        public string OpenCloseString
        {
            get { return this._OpenCloseString; }
            set { this._OpenCloseString = value; }
        }

        public ObservableCollection<string> BuySellList
        {
            get;
            set;
        }

        public ObservableCollection<string> OpenCloseList
        {
            get;
            set;
        }

        public ObservableCollection<OrderTask> OrderTasks
        {
            get { return this._OrderTasks; }
            set { this._OrderTasks = value; }
        }

        public PriceTrend PriceTrend
        {
            get { return this._PriceTrend; }
            set
            {
                if (this._PriceTrend != value)
                {
                    this._PriceTrend = value;
                    this.OnPropertyChanged("PriceTrend");
                    if (value != PriceTrend.NoChange)
                    {
                        if (this._PriceScheduleId != null)
                        {
                            this._Scheduler.Remove(this._PriceScheduleId);
                        }
                        this._PriceScheduleId = this._Scheduler.Add(this.ResetTrendState, "PriceTrend", DateTime.Now.AddSeconds(4));
                    }
                }
            }
        }

        public object MooMocHandle
        {
            get;
            set;
        }
        #endregion

        internal void UpdateOverridedQuotation(ExchangeQuotation exchangeQuotation)
        {
            if (exchangeQuotation.InstruemtnId == this.Instrument.Id)
            {
                this.PriceTrend = this.GetPriceTrend(double.Parse(exchangeQuotation.Ask), double.Parse(this.Ask));

                this.Ask = exchangeQuotation.Ask;
                this.Bid = exchangeQuotation.Bid;
                this.Origin = exchangeQuotation.Origin;
            }
        }

        private PriceTrend GetPriceTrend(double newPrice, double oldPrice)
        {
            if (newPrice > oldPrice)
            {
                return PriceTrend.Up;
            }
            else if (newPrice > oldPrice)
            {
                return PriceTrend.Down;
            }
            else
            {
                return PriceTrend.NoChange;
            }
        }

        private void ResetTrendState(object sender, object actionArgs)
        {
            if (actionArgs.Equals("PriceTrend"))
            {
                this._PriceScheduleId = null;
            }

            App.MainFrameWindow.Dispatcher.BeginInvoke((Action<string>)delegate(string propName)
            {
                if (propName.Equals("PriceTrend"))
                {
                    this.PriceTrend = PriceTrend.NoChange;
                }
            }, (string)actionArgs);
        }

        public void AddMooMocOrder(OrderTask orderTask)
        {
            this._OrderTasks.Add(orderTask);
            if (orderTask.IsBuy == BuySell.Buy)
            {
                this.SumBuyLot += orderTask.Lot.Value;
            }
            else
            {
                this.SumSellLot += orderTask.Lot.Value;
            }
        }

        public void RemoveMooMocOrder(List<OrderTask> orderTasks)
        {
            if (orderTasks.Count <= 0) return;
            foreach (OrderTask order in orderTasks)
            {
                this.OrderTasks.Remove(order);

            }
        }
    }
}
