using Manager.Common.QuotationEntities;
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
    public class InstantOrderForInstrument:PropertyChangedNotifier
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
        private Price _CustomerPrice; //客定价
        private Price _MarketPrice;
        private int _Diff;
        private string _SetPrice;
        private bool _IsAllowAdjustPrice = true;
        private string _OpenAvgPrice;
        
        private int _BuyVariation = 0;
        private int _SellVariation = 0;
        private SolidColorBrush _IsBuyBrush = SolidColorBrushes.BorderBrush;

        private PriceTrend _MarketPriceTrend;
        private Scheduler _Scheduler = new Scheduler();
        private string _MarketPriceScheduleId;
        #endregion

        public InstantOrderForInstrument()
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

        public Price CustomerPrice
        {
            get { return this._CustomerPrice; }
            set
            { 
                this._CustomerPrice = value; 
                this.OnPropertyChanged("CustomerPrice");
                //this.UpdateDiff();
                this.OnPropertyChanged("Diff");
            }
        }

        public Price MarketPrice
        {
            get { return this._MarketPrice; }
            set { this._MarketPrice = value; this.OnPropertyChanged("MarketPrice"); }
        }

        public int Diff
        {
            get { return this._Diff; }
            set { this._Diff = value; this.OnPropertyChanged("Diff"); }
        }

        public bool IsAllowAdjustPrice
        {
            get { return this._IsAllowAdjustPrice; }
            set { this._IsAllowAdjustPrice = value; this.OnPropertyChanged("IsAllowAdjustPrice"); }
        }

        public string OpenAvgPrice
        {
            get { return this._OpenAvgPrice; }
            set { this._OpenAvgPrice = value; this.OnPropertyChanged("OpenAvgPrice"); }
        }

        public int BuyVariation
        {
            get { return this._BuyVariation; }
            set { this._BuyVariation = value;this.OnPropertyChanged("BuyVariation");}
        }

        public int SellVariation
        {
            get { return this._SellVariation; }
            set { this._SellVariation = value; this.OnPropertyChanged("SellVariation"); }
        }

        public SolidColorBrush IsBuyBrush
        {
            get{return this._IsBuyBrush;}
            set{this._IsBuyBrush = value;this.OnPropertyChanged("IsBuyBrush"); }   
        }

        
        public PriceTrend MarketPriceTrend
        {
            get { return this._MarketPriceTrend; }
            set
            {
                if (this._MarketPriceTrend != value)
                {
                    this._MarketPriceTrend = value;
                    this.OnPropertyChanged("MarketPriceTrend");
                    if (value != PriceTrend.NoChange)
                    {
                        if (this._MarketPriceScheduleId != null)
                        {
                            this._Scheduler.Remove(this._MarketPriceScheduleId);
                        }
                        this._MarketPriceScheduleId = this._Scheduler.Add(this.ResetTrendState, "MarketPriceTrend", DateTime.Now.AddSeconds(4));
                    }
                }
            }
        }

        #endregion

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

        private void ResetTrendState(object sender, object actionArgs)
        {
            if (actionArgs.Equals("MarketPriceTrend"))
            {
                this._MarketPriceScheduleId = null;
            }

            App.MainFrameWindow.Dispatcher.BeginInvoke((Action<string>)delegate(string propName)
            {
                if (propName.Equals("MarketPriceTrend"))
                {
                    this.MarketPriceTrend = PriceTrend.NoChange;
                }
            }, (string)actionArgs);
        }

        internal void UpdateOverridedQuotation(ExchangeQuotation exchangeQuotation)
        {
            if (exchangeQuotation.InstruemtnId == this.Instrument.Id)
            {
                if (this.BuySell == BuySell.Buy)
                {
                    this.MarketPriceTrend = this.GetPriceTrend(double.Parse(exchangeQuotation.Ask), double.Parse(this.Ask));
                }
                else
                {
                    this.MarketPriceTrend = this.GetPriceTrend(double.Parse(exchangeQuotation.Bid), double.Parse(this.Bid));
                }

                this.Ask = exchangeQuotation.Ask;
                this.Bid = exchangeQuotation.Bid;
                this.UpdateMarketPrice(this.BuySell == BuySell.Buy);
            }
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
            this._SetPrice = orderTask.SetPrice;
            this.OpenAvgPrice = orderTask.Transaction.GetOpenOrderAvgPrice(this.OrderId);

            this.UpdateBrush();
            this.UpdateMarketPrice(this.BuySell == BuySell.Buy);
            this.IsAllowAdjustPrice = true;// OrderTaskManager.IsNeedDQMaxMove(orderTask);
            this.UpdateCustomerPrice();
            this.UpdateDiff();

            if (this.BuySell == BuySell.Buy)
            {
                this.MarketPriceTrend = this.GetPriceTrend(double.Parse(this.Ask), double.Parse(this.Ask));
            }
            else
            {
                this.MarketPriceTrend = this.GetPriceTrend(double.Parse(this.Bid), double.Parse(this.Bid));
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
            this._SetPrice = string.Empty;
            this.OpenAvgPrice = string.Empty;

            this.UpdateBrush();
            this.IsAllowAdjustPrice = false;
            this.CustomerPrice = new Price(1, 1, 10);
            this.Diff = 0;
            this.SumBuyLot = decimal.Zero;
            this.SumSellLot = decimal.Zero;
            this.BuyOrderCount = 0;
            this.SellOrderCount = 0;
        }

        internal void UpdateBrush()
        {
            this.IsBuyBrush = this.BuySell == BuySell.Buy ? SolidColorBrushes.LightBlue: new SolidColorBrush(Colors.Red);
        }

        internal void UpdateSumBuySellLot(bool isAdd,OrderTask orderTask)
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

        internal void UpdateMarketPrice(bool isBuy)
        {
            //if (!this._Instrument.NumeratorUnit.HasValue) return;
            if (isBuy)
            {
                Price bid = new Price(this.Bid, this._Instrument.NumeratorUnit, this._Instrument.Denominator);
                this.MarketPrice = bid;
            }
            else
            {
                Price ask = new Price(this.Ask, this._Instrument.NumeratorUnit, this._Instrument.Denominator);
                this.MarketPrice = ask;
            }
        }

        internal void UpdateCustomerPrice()
        {
            if (this.IsAllowAdjustPrice)
            {
                this.SetCustomerPrice();
            }
            else
            {
                this.CustomerPrice = new Price(this._SetPrice, this._Instrument.NumeratorUnit, this._Instrument.Denominator);
            }
        }

        internal void SetCustomerPrice()
        {
            Price ask = new Price(this.Ask, this._Instrument.NumeratorUnit, this._Instrument.Denominator);
            Price bid = new Price(this.Bid, this._Instrument.NumeratorUnit, this._Instrument.Denominator);
            if (this._Instrument.IsNormal ^ (this.BuySell == BuySell.Buy))
            {
                this.CustomerPrice = this.MarketPrice + 1;
            }
            else
            {
                this.CustomerPrice = this.MarketPrice - 1;
            }
        }

        internal void AdjustCustomerPrice(bool upOrDown)
        {
            int adjust = upOrDown ? 1 : -1;
            if (this.Instrument.IsNormal ^ (this.BuySell == BuySell.Buy))
            {
                this.CustomerPrice += adjust;
            }
            else
            {
                this.CustomerPrice -= adjust;
            }
        }

        internal void AdjustAutoPointVariation(bool isBuy,bool upOrDown)
        {
            int adjust = upOrDown ? 1:-1;
            int variation = isBuy ? this.BuyVariation : this.SellVariation;
            variation += ((int)this.Instrument.NumeratorUnit) * adjust;
            if (this.Instrument.CheckVariation(variation))
            {
                if (isBuy)
                {
                    this.BuyVariation = variation;
                }
                else
                {
                    this.SellVariation = variation;
                }
            }
        }

        internal void UpdateDiff()
        {
            this.Diff = this.MarketPrice - this.CustomerPrice;
        }

        
               
    }
}
