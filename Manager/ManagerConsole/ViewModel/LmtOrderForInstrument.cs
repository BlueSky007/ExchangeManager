using ManagerConsole.Helper;
using ManagerConsole.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using Price = iExchange.Common.Price;
using Scheduler = iExchange.Common.Scheduler;

namespace ManagerConsole.ViewModel
{
    public class LmtOrderForInstrument : PropertyChangedNotifier
    {
        #region Privete Property
        private Guid _OrderId;
        private InstrumentClient _Instrument;
        private string _InstrumentCode;
        private string _AccountCode;
        private decimal _Lot = decimal.Zero;
        private decimal _SumBuyLot = decimal.Zero;
        private decimal _SumSellLot = decimal.Zero;
        private int _BuyOrderCount;
        private int _SellOrderCount;
        private BuySell _BuySell;
        private string _Ask;
        private string _Bid;
        private string _Origin;
        private string _CustomerBidPrice; //客定Bid价
        private string _CustomerAskPrice; //客定Ask价
        private int _BidDiff;
        private int _AskDiff;
        private string _OpenAvgPrice;
        private string _PriceFormat;
        private decimal _IncrementPoint;

        private PriceTrend _AskTrend;
        private PriceTrend _BidTrend;

        private Scheduler _Scheduler = new Scheduler();
        private string _BidScheduleId;
        private string _AskScheduleId;
        private SolidColorBrush _IsBuyBrush = SolidColorBrushes.BorderBrush;
        #endregion

        public LmtOrderForInstrument()
        {
            this._Instrument = new InstrumentClient();
        }

        #region Public Property
        public Guid OrderId
        {
            get { return this._OrderId; }
            set { this._OrderId = value; this.OnPropertyChanged("OrderId"); }
        }

        public InstrumentClient Instrument
        {
            get { return this._Instrument; }
            set { this._Instrument = value; this.OnPropertyChanged("Instrument"); }
        }

        public string InstrumentCode
        {
            get { return this._InstrumentCode; }
            set { this._InstrumentCode = value; this.OnPropertyChanged("InstrumentCode"); }
        }

        public string AccountCode
        {
            get { return this._AccountCode; }
            set { this._AccountCode = value; this.OnPropertyChanged("AccountCode"); }
        }

        public decimal Lot
        {
            get { return this._Lot; }
            set { this._Lot = value; this.OnPropertyChanged("Lot"); }
        }

        public decimal SumBuyLot
        {
            get { return this._SumBuyLot; }
            set { this._SumBuyLot = value; this.OnPropertyChanged("SumBuyLot"); }
        }

        public decimal SumSellLot
        {
            get { return this._SumSellLot; }
            set { this._SumSellLot = value; this.OnPropertyChanged("SumSellLot"); }
        }

        public int BuyOrderCount
        {
            get { return this._BuyOrderCount; }
            set { this._BuyOrderCount = value; this.OnPropertyChanged("BuyOrderCount"); }
        }

        public int SellOrderCount
        {
            get { return this._SellOrderCount; }
            set { this._SellOrderCount = value; this.OnPropertyChanged("SellOrderCount"); }
        }

        public BuySell BuySell
        {
            get { return this._BuySell; }
            set { this._BuySell = value; this.OnPropertyChanged("BuySell"); }
        }

        public string Origin
        {
            get { return this._Origin; }
            set { this._Origin = value; this.OnPropertyChanged("Origin"); }
        }

        public string Ask
        {
            get { return this._Ask; }
            set { this._Ask = value; this.OnPropertyChanged("Ask"); }
        }

        public string Bid
        {
            get { return this._Bid; }
            set { this._Bid = value; this.OnPropertyChanged("Bid"); }
        }

        public string CustomerBidPrice
        {
            get { return this._CustomerBidPrice; }
            set
            {
                this._CustomerBidPrice = value;
                this.OnPropertyChanged("CustomerBidPrice");
                this.OnPropertyChanged("Diff");
            }
        }

        public string CustomerAskPrice
        {
            get { return this._CustomerAskPrice; }
            set
            {
                this._CustomerAskPrice = value;
                this.OnPropertyChanged("CustomerAskPrice");
                this.OnPropertyChanged("Diff");
            }
        }

        public int BidDiff
        {
            get { return this._BidDiff; }
            set { this._BidDiff = value; this.OnPropertyChanged("BidDiff"); }
        }

        public int AskDiff
        {
            get { return this._AskDiff; }
            set { this._AskDiff = value; this.OnPropertyChanged("AskDiff"); }
        }

        public string OpenAvgPrice
        {
            get { return this._OpenAvgPrice; }
            set { this._OpenAvgPrice = value; this.OnPropertyChanged("OpenAvgPrice"); }
        }

        public SolidColorBrush IsBuyBrush
        {
            get{return this._IsBuyBrush;}
            set{this._IsBuyBrush = value;this.OnPropertyChanged("IsBuyBrush"); }   
        }

        public PriceTrend AskTrend
        {
            get { return this._AskTrend; }
            set
            {
                if (this._AskTrend != value)
                {
                    this._AskTrend = value;
                    this.OnPropertyChanged("AskTrend");
                    if (value != PriceTrend.NoChange)
                    {
                        if (this._AskScheduleId != null)
                        {
                            this._Scheduler.Remove(this._AskScheduleId);
                        }
                        this._AskScheduleId = this._Scheduler.Add(this.ResetTrendState, "Ask", DateTime.Now.AddSeconds(4));
                    }
                }
            }
        }
        public PriceTrend BidTrend
        {
            get { return this._BidTrend; }
            set
            {
                if (this._BidTrend != value)
                {
                    this._BidTrend = value;
                    this.OnPropertyChanged("BidTrend");
                    if (value != PriceTrend.NoChange)
                    {
                        if (this._BidScheduleId != null)
                        {
                            this._Scheduler.Remove(this._BidScheduleId);
                        }
                        this._BidScheduleId = this._Scheduler.Add(this.ResetTrendState, "Bid", DateTime.Now.AddSeconds(4));
                    }
                }
            }
        }

        public string PriceFormat
        {
            get { return this._PriceFormat; }
            set { this._PriceFormat = value; this.OnPropertyChanged("PriceFormat"); }
        }

        public decimal IncrementPoint
        {
            get { return this._IncrementPoint; }
            set { this._IncrementPoint = value; this.OnPropertyChanged("IncrementPoint"); }
        }
        #endregion

        internal void UpdateOverridedQuotation(ExchangeQuotation exchangeQuotation)
        {
            if (exchangeQuotation.InstruemtnId == this.Instrument.Id)
            {
                this.AskTrend = this.GetPriceTrend(double.Parse(exchangeQuotation.Ask),double.Parse(this.Ask));
                this.BidTrend = this.GetPriceTrend(double.Parse(exchangeQuotation.Bid),double.Parse(this.Bid));

                this.Ask = exchangeQuotation.Ask;
                this.Bid = exchangeQuotation.Bid;
                this.Origin = exchangeQuotation.Origin;

                this.UpdateDiff();
            }
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

        internal void Update(OrderTask orderTask)
        {
            this.OrderId = orderTask.OrderId;
            this.Instrument = orderTask.Instrument;
            this.InstrumentCode = this._Instrument.Code;
            this.Ask = orderTask.Instrument.Ask;
            this.Bid = orderTask.Instrument.Bid;
            this.AccountCode = orderTask.AccountCode;
            this.BuySell = orderTask.IsBuy;
            this.Lot = orderTask.Lot.Value;
            this.OpenAvgPrice = orderTask.Transaction.GetOpenOrderAvgPrice(this.OrderId);

            this.UpdateBrush();
            this.CustomerAskPrice = this.Ask;
            this.CustomerBidPrice = this.Bid;
            this.UpdateDiff();

            this.GetPriceFormating();
        }

        internal void GetPriceFormating()
        {
            this.IncrementPoint = (decimal)this.Instrument.NumeratorUnit.Value / (decimal)this.Instrument.Denominator.Value;

            if (this.IncrementPoint < 1)
            {
                int index = this.IncrementPoint.ToString().IndexOf(".");
                int length = this.IncrementPoint.ToString().Length;
                int decimalPlace = length - (index + 1);

                this.PriceFormat = "nnnnnnnn.";
                for (int i = 0; i < decimalPlace; i++)
                {
                    this.PriceFormat += "n";
                }
            }
            else
            {
                this.PriceFormat = "nnnnnnnn";
            }
        }

        internal void CreateEmptyEntity()
        {
            this.OrderId = Guid.Empty;
            this.Instrument = new InstrumentClient();
            this.InstrumentCode = string.Empty;
            this.Ask = "-";
            this.Bid = "-";
            this.AccountCode = string.Empty;
            this.BuySell = BuySell.Buy;
            this.Lot = decimal.Zero;
            this.OpenAvgPrice = string.Empty;
            this.CustomerAskPrice = string.Empty;
            this.CustomerBidPrice = string.Empty;
            this.BuyOrderCount = 0;
            this.SellOrderCount = 0;

            this.UpdateBrush();
            this.AskDiff = 0;
            this.BidDiff = 0;
        }

        internal void ApplyMaretPrice(bool isBuy)
        {
            if (isBuy)
            {
                this.CustomerBidPrice = this.Bid;
            }
            else
            {
                this.CustomerAskPrice = this.Ask;
            }
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
            App.MainFrameWindow.Dispatcher.BeginInvoke((Action<string>)delegate(string propName)
            {
                if (propName.Equals("Ask"))
                {
                    this.AskTrend = PriceTrend.NoChange;
                }
                else
                {
                    this.BidTrend = PriceTrend.NoChange;
                }
            }, (string)actionArgs);
        }

        internal void UpdateBrush()
        {
            this.IsBuyBrush = this.BuySell == BuySell.Buy ? SolidColorBrushes.LightBlue : new SolidColorBrush(Colors.Red);
        }

        internal void UpdateSumBuySellLot(bool isAdd, OrderTask orderTask)
        {
            bool isBuy = orderTask.IsBuy == BuySell.Buy;
            if (isAdd)
            {
                if (isBuy)
                {
                    this.SumBuyLot += orderTask.Lot.Value;
                    this.BuyOrderCount++;
                }
                else
                {
                    this.SumSellLot += orderTask.Lot.Value;
                    this.SellOrderCount++;
                }
            }
            else
            {
                if (isBuy)
                {
                    this.SumBuyLot -= orderTask.Lot.Value;
                    this.BuyOrderCount--;
                }
                else
                {
                    this.SumSellLot -= orderTask.Lot.Value;
                    this.SellOrderCount--;
                }
            }
        }

        internal void SetCustomerPrice(bool isBuy)
        {
            string pricString = isBuy ? this.CustomerBidPrice : this.CustomerAskPrice;
            Price price = new Price(pricString, this._Instrument.NumeratorUnit.Value, this._Instrument.Denominator.Value);

            Price ask = new Price(this.Ask, this._Instrument.NumeratorUnit.Value, this._Instrument.Denominator.Value);
            Price bid = new Price(this.Bid, this._Instrument.NumeratorUnit.Value, this._Instrument.Denominator.Value);

            Price marketPrice = isBuy ? ask : bid;
            if (this._Instrument.IsNormal ^ (this.BuySell == BuySell.Buy))
            {
                price = marketPrice + 1;
            }
            else
            {
                price = marketPrice - 1;
            }
        }

        internal void UpdateDiff()
        {
            if (this.CustomerBidPrice == null || this.CustomerAskPrice == null || this._Instrument.NumeratorUnit == null) return;
            Price customerAskPrice = new Price(this.CustomerAskPrice, this._Instrument.NumeratorUnit.Value, this._Instrument.Denominator.Value);
            Price customerBidPrice = new Price(this.CustomerBidPrice, this._Instrument.NumeratorUnit.Value, this._Instrument.Denominator.Value);
            Price ask = new Price(this.Ask, this._Instrument.NumeratorUnit.Value, this._Instrument.Denominator.Value);
            Price bid = new Price(this.Bid, this._Instrument.NumeratorUnit.Value, this._Instrument.Denominator.Value);
            this.AskDiff = ask - customerAskPrice;
            this.BidDiff = bid - customerBidPrice;
        }
    }
}
