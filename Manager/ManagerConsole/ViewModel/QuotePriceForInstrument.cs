using Manager.Common;
using Manager.Common.Settings;
using ManagerConsole.Helper;
using ManagerConsole.Model;
using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows.Media;
using Price = iExchange.Common.Price;

namespace ManagerConsole.ViewModel
{
    public class QuotePriceForInstrument : PropertyChangedNotifier
    {
        public QuotePriceForInstrument(QuotePriceClient quotePriceClient)
        {
            this.FirstQuoteUpdate(quotePriceClient);
        }

        #region Privete Prperty
        private InstrumentClient _Instrument;
        private string _InstrumentCode;
        private Guid _QuoteId;
        private string _Origin;
        private string _Ask;
        private string _Bid;
        private string _BestSell;
        private string _BestBuy;
        private string _BestBuyString;
        private string _BestSellString;
        private decimal _Lot;
        private decimal _AnswerLot;
        private string _SetPrice;
        private string _AnswerPrice;
        private decimal _SumBuyLot;
        private decimal _SumSellLot;
        private BSStatus _BSStatus;
        private decimal _Adjust =decimal.Zero;
        private decimal _AdjustLot;
        private decimal _AdjustPoint = decimal.Zero;
        #endregion

        #region Public Property
        public string InstrumentCode
        {
            get { return this._InstrumentCode; }
            set 
            { 
                this._InstrumentCode = value; 
                this.OnPropertyChanged("InstrumentCode"); 
            }
        }

        public Guid QuoteId
        {
            get { return this._QuoteId; }
            set { this._QuoteId = value; }
        }

        public string Origin
        {
            get { return this._Origin; }
            set
            { 
                this._Origin = value;
                this.OnPropertyChanged("Origin");
            }
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

        public string BestBuy
        {
            get { return this._BestBuy == null ? "-" : this._BestBuy; }
            set
            {
                this._BestBuy = value;
                this.OnPropertyChanged("BestBuy");
            }
        }

        public string BestSell
        {
            get { return this._BestSell == null ? "-" : this._BestSell; }
            set
            {
                this._BestSell = value;
                this.OnPropertyChanged("BestSell");
            }
        }

        public SolidColorBrush BestBuySellForegroundBrush
        {
            get
            {
                if (this._Instrument.IsNormal ^ (this.BSStatus == BSStatus.Buy))
                {
                    return SolidColorBrushes.Red;
                }
                else
                {
                    return SolidColorBrushes.Blue; ;
                }
            }
        }

        private Brush _SigleButtonBackgroundBrush;
        public Brush SigleButtonBackgroundBrush
        {
            get { return this._SigleButtonBackgroundBrush; }
            set
            {
                this._SigleButtonBackgroundBrush = value;
                this.OnPropertyChanged("SigleButtonBackgroundBrush");
            }
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

        public string AnswerPrice
        {
            get { return this._AnswerPrice; }
            set
            {
                this._AnswerPrice = value;
                this.OnPropertyChanged("AnswerPrice");
            }
        }

        public decimal Lot
        {
            get { return this._Lot; }
            set 
            { 
                this._Lot = value; 
                this.OnPropertyChanged("Lot"); 
            }
        }

        public decimal AnswerLot
        {
            get{return this._AnswerLot;}
            set 
            { 
                this._AnswerLot = value;
                this.OnPropertyChanged("AnswerLot");
            }
        }

        public decimal SumBuyLot
        {
            get { return this._SumBuyLot; }
            set 
            { 
                this._SumBuyLot = value;
                this.OnPropertyChanged("SumBuyLot");
            }
        }

        public decimal SumSellLot
        {
            get { return this._SumSellLot; }
            set
            { 
                this._SumSellLot = value;
                this.OnPropertyChanged("SumSellLot");
            }
        }

        public BSStatus BSStatus
        {
            get { return this._BSStatus; }
            set 
            { 
                this._BSStatus = value;
                this.OnPropertyChanged("BSStatus");
                this.SettingBackGround();
                this.OnPropertyChanged("SigleButtonBackgroundBrush");
            }
        }

        public decimal AdjustPoint
        {
            get { return this._AdjustPoint; }
            set
            {
                this._AdjustPoint = value;
                this.OnPropertyChanged("AdjustPoint");
            }
        }

        //?
        public ObservableCollection<QuotePriceClient> QuotePriceClients
        {
            get;
            set;
        }

        public decimal Adjust
        {
            get { return this._Adjust; }
            set { this._Adjust = value; this.OnPropertyChanged("Adjust"); }
        }

        public decimal AdjustLot
        {
            get { return this._AdjustLot; }
            set { this._AdjustLot = value; this.OnPropertyChanged("AdjustLot"); }

        }

        public InstrumentClient Instrument
        {
            get { return this._Instrument; }
            set { this._Instrument = value; }
        }

        #endregion

        internal void FirstQuoteUpdate(QuotePriceClient quotePriceClient)
        {
            this._QuoteId = quotePriceClient.Id;
            this._Instrument = quotePriceClient.Instrument;
            this._InstrumentCode = this._Instrument.Code;
            this._Origin = quotePriceClient.ExchangeQuotation.Origin;
            this._Ask = quotePriceClient.ExchangeQuotation.Ask;
            this._Bid = quotePriceClient.ExchangeQuotation.Bid;
            this._SumBuyLot = quotePriceClient.BuyLot;
            this._SumSellLot = quotePriceClient.SellLot;
            this._Lot = quotePriceClient.Lot;
            this._AnswerLot = this._Lot;
            this._BSStatus = quotePriceClient.BSStatus;
            this._AdjustPoint = (decimal)this.Instrument.NumeratorUnit.Value / (decimal)this.Instrument.Denominator.Value;

            this.CreateBestPrice(true);
            this.SettingBackGround();
        }

        internal void SettingBackGround()
        {
            if (this._BSStatus == BSStatus.Buy)
            {
                this.SigleButtonBackgroundBrush = SolidColorBrushes.Buy;
                this._AnswerPrice = this._BestBuy;
            }
            else if (this._BSStatus == BSStatus.Sell)
            {
                this.SigleButtonBackgroundBrush = SolidColorBrushes.Sell;
                this._AnswerPrice = this._BestSell;
            }
            else
            {
                this.SigleButtonBackgroundBrush = SolidColorBrushes.Gray;
                this._AnswerPrice = this._BestBuy;
            }
        }

        internal void SwitchNewInstrument(QuotePriceClient quotePriceClient, bool isResetSumLot)
        {
            this.QuoteId = quotePriceClient.Id;
            this.Instrument = quotePriceClient.Instrument;
            this.InstrumentCode = this._Instrument.Code;
            this.Origin = this._Instrument.Origin;
            this.Ask = this._Instrument.Ask;
            this.Bid = this._Instrument.Bid;
            if (isResetSumLot)
            {
                this.SumBuyLot = decimal.Zero;
                this.SumSellLot = decimal.Zero;
            }
            this.Lot = quotePriceClient.Lot;
            this.AnswerLot = this.Lot;
            this.BSStatus = quotePriceClient.BSStatus;
            this.AdjustPoint = (decimal)this.Instrument.NumeratorUnit.Value / (decimal)this.Instrument.Denominator.Value;
        }

        internal void UpdateBestBuySell(Quotation newQuotation)
        {
            bool isNormal = this._Instrument.IsNormal;
            BSStatus bSStatus = this.BSStatus;

            this.Origin = newQuotation.Origin;
            if (this._BSStatus == BSStatus.Both)
            {
                this._BestBuyString = this.Ask;
                this._BestSellString = this.Bid;
                this.BestBuy = this.BestSell = this.GetBestBuySellString(this.Ask, this.Bid);
                this.AnswerPrice = this.BestBuy;
            }
            else
            {
                if (this._Instrument.IsNormal ^ (this._BSStatus == BSStatus.Buy))
                {
                    this.BestBuy = newQuotation.Bid;
                    this.AnswerPrice = this.BestSell;
                }
                else
                {
                    this.BestSell = newQuotation.Ask;
                    this.AnswerPrice = this.BestBuy;
                }
            }
        }

        internal void CreateBestPrice(bool isAdd)
        {
            int adjust = isAdd ? 1 : -1;

            if (this._BSStatus == BSStatus.Both)
            {
                this._BestBuyString = this.Ask;
                this._BestSellString = this.Bid;
                this.BestBuy = this.BestSell = this.GetBestBuySellString(this.Ask, this.Bid);
                this.AnswerPrice = this.BestBuy;
            }
            else
            {
                Price ask = new Price(this.Ask, this._Instrument.NumeratorUnit.Value, this._Instrument.Denominator.Value);
                Price bid = new Price(this.Bid, this._Instrument.NumeratorUnit.Value, this._Instrument.Denominator.Value);
                if (this._Instrument.IsNormal ^ (this._BSStatus == BSStatus.Buy))
                {
                    this.BestBuy = (ask - adjust).ToString();
                    this.BestSell = (bid - adjust).ToString();
                    this.AnswerPrice = this.BestSell;
                }
                else
                {
                    this.BestBuy = (ask + adjust).ToString();
                    this.BestSell = (bid + adjust).ToString();
                    this.AnswerPrice = this.BestBuy;
                }
            }
            this.SetPrice = string.IsNullOrEmpty(this.BestBuy) ? this.BestSell : this.BestBuy;
        }

        internal void AdjustPrice(bool isAdd)
        {
            int adjust = isAdd ? 1 : -1;
            Price ask;
            Price bid;

            if (this._BSStatus == BSStatus.Both)
            {
                ask = new Price(this._BestBuyString, this._Instrument.NumeratorUnit.Value, this._Instrument.Denominator.Value);
                bid = new Price(this._BestSellString, this._Instrument.NumeratorUnit.Value, this._Instrument.Denominator.Value);

                this._BestBuyString = (ask + adjust).ToString();
                this._BestSellString = (bid + adjust).ToString();
                this.BestBuy = this.BestSell = this.GetBestBuySellString(this._BestBuyString, this._BestSellString);
                this.AnswerPrice = this.BestBuy;
            }
            else
            {
                ask = new Price(this.BestBuy.Equals("-") ? this.Ask : this.BestBuy, this._Instrument.NumeratorUnit.Value, this._Instrument.Denominator.Value);
                bid = new Price(this.BestSell.Equals("-") ? this.Bid : this.BestSell, this._Instrument.NumeratorUnit.Value, this._Instrument.Denominator.Value);
                if (this._Instrument.IsNormal ^ (this._BSStatus == BSStatus.Buy))
                {
                    this.BestBuy = (ask - adjust).ToString();
                    this.BestSell = (bid - adjust).ToString();
                    this.AnswerPrice = this.BestSell;
                }
                else
                {
                    this.BestBuy = (ask + adjust).ToString();
                    this.BestSell = (bid + adjust).ToString();
                    this.AnswerPrice = this.BestBuy;
                }
            }
            this.SetPrice = string.IsNullOrEmpty(this.BestBuy) ? this.BestSell : this.BestBuy;
        }

        public bool? IsValidPrice(QuotePriceClient quotePriceClient, decimal adjust)
        {
            if (quotePriceClient.Origin == null) return false;
            Price lastOriginPrice = Price.CreateInstance(quotePriceClient.Origin, this.Instrument.NumeratorUnit.Value, this.Instrument.Denominator.Value);
            string validInt = "^-?\\d+$";
            Price originPrice;
            if (Regex.IsMatch(adjust.ToString(), validInt))
            {
                originPrice = lastOriginPrice + (int)adjust;
            }
            else
            {
                originPrice = Price.CreateInstance((double)adjust, this.Instrument.NumeratorUnit.Value, this.Instrument.Denominator.Value);
            }
            if (originPrice != null)
            {
                if (lastOriginPrice != null)
                {
                    return (Math.Abs(lastOriginPrice - originPrice) > this.Instrument.AlertPoint);
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return null;
            }
        }

        public bool? IsValidPrice(decimal adjust)
        {
            Price lastOriginPrice = Price.CreateInstance(this.Origin, this.Instrument.NumeratorUnit.Value, this.Instrument.Denominator.Value);
			string validInt = "^-?\\d+$";
            Price originPrice;
            if (Regex.IsMatch(adjust.ToString(), validInt))
            {
                originPrice = lastOriginPrice + (int)adjust;
            }
            else
            {
                originPrice = Price.CreateInstance((double)adjust, this.Instrument.NumeratorUnit.Value, this.Instrument.Denominator.Value);
            }
            if (originPrice != null)
            {
                if (lastOriginPrice != null)
                {
                    return (Math.Abs(lastOriginPrice - originPrice) > this.Instrument.AlertPoint);
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return null;
            }
        }

        private string GetBestBuySellString(string ask, string bid)
        {
            if (ask == null || bid == null) return string.Empty;

            int differenceIndex = -1;
            for (int index = 0; index < ask.Length; index++)
            {
                if (ask[index] != bid[index])
                {
                    differenceIndex = index;
                    break;
                }
            }

            if (differenceIndex == -1)
            {
                if (ask.Length > 2)
                {
                    differenceIndex = ask.Length > 2 ? ask.Length - 2 : 2;
                }
                else
                {
                    differenceIndex = 0;
                }
            }
            else
            {
                int activePartLength = ask.Length - differenceIndex;
                if (activePartLength < 2) activePartLength = 2;
                if (activePartLength > 4) activePartLength = 4;
                differenceIndex = ask.Length - activePartLength;
                if (differenceIndex < 0) differenceIndex = 0;
            }

            if (((ask[differenceIndex] == '.' || bid[differenceIndex] == '.')) && differenceIndex > 0)
            {
                differenceIndex--;
            }

            string _AskActivePart = ask.Substring(differenceIndex);
            string _BidActivePart = bid.Substring(differenceIndex);
            string _InactivePartOfPrice = bid.Substring(0, differenceIndex);


            return bid + "/" + _AskActivePart;
        }
    }
}
