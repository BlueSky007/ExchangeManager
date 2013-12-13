using Manager.Common;
using Manager.Common.Settings;
using ManagerConsole.Helper;
using ManagerConsole.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Media;

namespace ManagerConsole.ViewModel
{
    public class QuotePriceForInstrument : PropertyChangedNotifier
    {
        public QuotePriceForInstrument(QuotePriceClient quotePriceClient)
        {
            this.FirstQuoteUpdate(quotePriceClient);
        }

        internal void FirstQuoteUpdate(QuotePriceClient quotePriceClient)
        {
            this._QuoteId = quotePriceClient.Id;
            this._Instrument = quotePriceClient.Instrument;
            this._InstrumentCode = this._Instrument.Code;
            this._Origin = this._Instrument.Origin;
            this._Ask = this._Instrument.Ask;
            this._Bid = this._Instrument.Bid;
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
                this.SigleButtonBackgroundBrush = SolidColorBrushes.LightBlue;
            }
            else if (this._BSStatus == BSStatus.Sell)
            {
                this.SigleButtonBackgroundBrush = SolidColorBrushes.LightRed;
            }
            else
            {
                this.SigleButtonBackgroundBrush = SolidColorBrushes.Transparent;
            }
        }

        internal void SwitchNewInstrument(QuotePriceClient quotePriceClient,bool isResetSumLot)
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
                this.BestBuy = this.BestSell = this.GetBestBuySellString(this.Ask, this.Bid); 
            }
            else
            {
                if (this._Instrument.IsNormal ^ (this._BSStatus == BSStatus.Buy))
                    this.BestBuy = newQuotation.Bid;
                else
                    this.BestSell = newQuotation.Ask;
            }
        }

        internal void CreateBestPrice(bool isAdd)
        {
            int adjust = isAdd ? 1 : -1;

            if (this._BSStatus == BSStatus.Both)
            {
                this.BestBuy = this.BestSell = this.GetBestBuySellString(this.Ask, this.Bid); 
            }
            else
            {
                Price ask = new Price(string.IsNullOrEmpty(this.BestBuy)  ? this.BestBuy:this.Ask, this._Instrument.NumeratorUnit.Value, this._Instrument.Denominator.Value);
                Price bid = new Price(string.IsNullOrEmpty(this.BestSell) ? this.BestSell:this.Bid, this._Instrument.NumeratorUnit.Value, this._Instrument.Denominator.Value);
                if (this._Instrument.IsNormal ^ (this._BSStatus == BSStatus.Buy))
                {
                    this.BestBuy = (ask - adjust).ToString();
                    this.BestSell = (bid - adjust).ToString();
                }
                else
                {
                    this.BestBuy = (ask + adjust).ToString();
                    this.BestSell = (bid + adjust).ToString();
                }
            }
            this.SetPrice = string.IsNullOrEmpty(this.BestBuy) ? this.BestSell : this.BestBuy;   
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
        private decimal _Lot;
        private decimal _AnswerLot;
        private string _SetPrice;
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

        public SolidColorBrush BestBuyForegroundBrush
        {
            get
            {
                if (this._BestBuy != null)
                {
                    return SolidColorBrushes.Red;
                }
                else
                {
                    return SolidColorBrushes.Black; ;
                }
            }
        }

        private SolidColorBrush _SigleButtonBackgroundBrush;
        public SolidColorBrush SigleButtonBackgroundBrush
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
        //---Old object

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

        public bool? IsValidPrice(QuotePriceClient quotePriceClient, decimal adjust)
        {
            Price lastOriginPrice = Price.CreateInstance(quotePriceClient.Origin, this.Instrument.NumeratorUnit.Value, this.Instrument.Denominator.Value);
            string validInt = "^-?\\d+$";
            Price originPrice;
            if (Regex.IsMatch(adjust.ToString(), validInt))
            {
                originPrice = Price.Adjust(lastOriginPrice, (int)adjust);
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
                originPrice = Price.Adjust(lastOriginPrice, (int)adjust);
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

        #region Event
        public List<Answer> GetSelectedQuotePriceForAccount()
        {
            List<Answer> quoteQuotations = new List<Answer>();
            //List<QuotePriceClient> quotePrices = this.QuotePriceClients.Where(P => P.IsSelected).ToList();

            //foreach (QuotePriceClient entity in quotePrices)
            //{
            //    quoteQuotations.Add(this.GetSelectedQuotePriceForAccount(entity));
            //}
            return quoteQuotations;
        }
        private Answer GetSelectedQuotePriceForAccount(QuotePriceClient quotePrice)
        {
            Answer quoteQuotation = quotePrice.ToSendQutoPrice();
            quoteQuotation.InstrumentId = this.Instrument.Id;
            quoteQuotation.TimeStamp = ShareFixedData.GetServiceTime();

            return quoteQuotation;
        }

        //Update Price
        public void UpdateCurrentPrice()
        {
            this.Origin = this.Instrument.Origin;
            this.UpdateCurrentPrice(this.QuotePriceClients);
        }
        public void UpdateCurrentPrice(ObservableCollection<QuotePriceClient> quotePriceClients)
        {
            var adjustQuotePrices = quotePriceClients.Where(P => P.IsSelected);
            foreach (QuotePriceClient entity in adjustQuotePrices)
            {
                this.UpdateCurrentPrice(entity);
            }
        }
        public void UpdateCurrentPrice(QuotePriceClient quotePriceClient)
        {
            quotePriceClient.Origin = this.Instrument.Origin;
            //quotePriceClient.Ask = this.InstrumentClient.Ask;
            //quotePriceClient.Bid = this.InstrumentClient.Bid;
        }

        public void AddNewQuotePrice(QuotePriceClient quotePriceClient)
        {
            var existEntity = this.QuotePriceClients.SingleOrDefault(P => P.InstrumentId == quotePriceClient.InstrumentId && P.CustomerCode == quotePriceClient.CustomerCode);

            if (existEntity != null)
            {
                //existEntity.AdjustSingle = existEntity.AdjustSingle;
                //existEntity.CustomerId = existEntity.CustomerId;
                //existEntity.InstrumentId = existEntity.InstrumentId;
                //existEntity.QuoteLot = existEntity.QuoteLot;
                //existEntity.IsSelected = existEntity.IsSelected;
                //existEntity.TimeStamp = existEntity.TimeStamp;
                //existEntity.WaitTimes = existEntity.WaitTimes;
                //existEntity.Ask = this.InstrumentClient.Ask;
                //existEntity.Bid = this.InstrumentClient.Bid;
                //existEntity.Origin = this.InstrumentClient.Origin;
            }
            else
            {
                //quotePriceClient.Ask = this.InstrumentClient.Ask;
                //quotePriceClient.Bid = this.InstrumentClient.Bid;
                //quotePriceClient.Origin = this.InstrumentClient.Origin;
                //this._QuotePriceClients.Add(quotePriceClient);
            }
        }

        //Remove QuotePrice



        //Lot Limit
        public void OnEnquiryQuantity(bool isAbove)
        {
            //decimal newLot = this.AdjustLot;
            //foreach (QuotePriceClient entity in this.QuotePriceClients)
            //{
            //    bool isSelected = false;
            //    if(isAbove)
            //    {
            //        isSelected = (entity.QuoteLot >= newLot) ? true : false;
            //    }
            //    else
            //    {
            //        isSelected = (entity.QuoteLot >= newLot) ? false : true;
            //    }
            //    entity.IsSelected = isSelected;
            //}
        }
        #endregion

        public void AdjustCurrentPrice(decimal adjust, QuotePriceClient quotePriceClient, bool isAdjustInstrument)
        {
           // Quotation quotation = Quotation.Create((double)adjust,
           //// double.Parse(quotePriceClient.Origin),
           // this.InstrumentClient.NumeratorUnit.Value, this.InstrumentClient.Denominator.Value,
           // this.InstrumentClient.AutoPoint.Value, this.InstrumentClient.Spread.Value);

           // if (isAdjustInstrument)
           // {
           //     //this.Origin = quotation.Origin;
           //     this.Adjust = decimal.Parse(quotation.Origin);
           // }

           // quotePriceClient.Origin = quotation.Origin;
           // quotePriceClient.Ask = quotation.Ask;
           // quotePriceClient.Bid = quotation.Bid;
        }

        public void AdjustCurrentPrice(decimal adjust, bool isAdjustInstrument)
        {
            this.AdjustCurrentPrice(adjust, this.QuotePriceClients, isAdjustInstrument);
        }

        public void AdjustCurrentPrice(decimal adjust, ObservableCollection<QuotePriceClient> quotePriceClients, bool isAdjustInstrument)
        {
            //调整选中价格
            var adjustQuotePrices = quotePriceClients.Where(P => P.IsSelected);
            foreach (QuotePriceClient entity in adjustQuotePrices)
            {
                this.AdjustCurrentPrice(adjust, entity, isAdjustInstrument);
            }
        }

        //
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
