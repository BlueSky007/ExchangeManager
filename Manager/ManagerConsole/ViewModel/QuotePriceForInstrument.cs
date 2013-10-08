using Manager.Common;
using ManagerConsole.Helper;
using ManagerConsole.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace ManagerConsole.ViewModel
{
    public class QuotePriceForInstrument : PropertyChangedNotifier
    {
        public delegate void EnquiryTimeOutHandle(QuotePriceForInstrument quotePriceForInstrument);
        public event EnquiryTimeOutHandle OnEnquiryTimeOutEvent;

        public delegate void EmptyQuotePriceHandle(QuotePriceForInstrument quotePriceForInstrument);
        public event EmptyQuotePriceHandle OnEmptyQuotePriceClient;

        public QuotePriceForInstrument()
        {
            this.QuotePriceClients = new ObservableCollection<QuotePriceClient>();
        }

        #region Privete Prperty
        private InstrumentClient _InstrumentClient;
        private ObservableCollection<QuotePriceClient> _QuotePriceClients;
        private string _Origin;
        private decimal _Adjust =decimal.Zero;
        private decimal _AdjustLot;
        public bool _SelectedAll = true;
        #endregion

        #region Public Property
        public bool SelectedAll
        {
            get { return this._SelectedAll; }
            set { this._SelectedAll = value; this.OnPropertyChanged("SelectedAll"); }
        }
        public string InstrumentCode
        {
            get { return this._InstrumentClient.Code; }
        }
        public string Origin
        {
            get { return this._Origin; }
            set { this._Origin = value; this.OnPropertyChanged("Origin"); }
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

        public InstrumentClient InstrumentClient
        {
            get { return this._InstrumentClient; }
            set { this._InstrumentClient = value; }
        }

        public ObservableCollection<QuotePriceClient> QuotePriceClients
        {
            get { return this._QuotePriceClients; }
            set { this._QuotePriceClients = value; }
        }
        #endregion

        public bool? IsValidPrice(QuotePriceClient quotePriceClient, decimal adjust)
        {
            Price lastOriginPrice = Price.CreateInstance(quotePriceClient.Origin, this.InstrumentClient.NumeratorUnit.Value, this.InstrumentClient.Denominator.Value);
            string validInt = "^-?\\d+$";
            Price originPrice;
            if (Regex.IsMatch(adjust.ToString(), validInt))
            {
                originPrice = Price.Adjust(lastOriginPrice, (int)adjust);
            }
            else
            {
                originPrice = Price.CreateInstance((double)adjust, this.InstrumentClient.NumeratorUnit.Value, this.InstrumentClient.Denominator.Value);
            }
            if (originPrice != null)
            {
                if (lastOriginPrice != null)
                {
                    return (Math.Abs(lastOriginPrice - originPrice) > this.InstrumentClient.AlertPoint);
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
            Price lastOriginPrice = Price.CreateInstance(this.Origin, this.InstrumentClient.NumeratorUnit.Value, this.InstrumentClient.Denominator.Value);
			string validInt = "^-?\\d+$";
            Price originPrice;
            if (Regex.IsMatch(adjust.ToString(), validInt))
            {
                originPrice = Price.Adjust(lastOriginPrice, (int)adjust);
            }
            else
            {
                originPrice = Price.CreateInstance((double)adjust, this.InstrumentClient.NumeratorUnit.Value, this.InstrumentClient.Denominator.Value);
            }
            if (originPrice != null)
            {
                if (lastOriginPrice != null)
                {
                    return (Math.Abs(lastOriginPrice - originPrice) > this.InstrumentClient.AlertPoint);
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
            List<QuotePriceClient> quotePrices = this.QuotePriceClients.Where(P => P.IsSelected).ToList();

            foreach (QuotePriceClient entity in quotePrices)
            {
                quoteQuotations.Add(this.GetSelectedQuotePriceForAccount(entity));
            }
            return quoteQuotations;
        }
        private Answer GetSelectedQuotePriceForAccount(QuotePriceClient quotePrice)
        {
            Answer quoteQuotation = quotePrice.ToSendQutoPrice();
            quoteQuotation.InstrumentId = this.InstrumentClient.Id;
            quoteQuotation.TimeStamp = ShareFixedData.GetServiceTime();

            return quoteQuotation;
        }

        //Update Price
        public void UpdateCurrentPrice()
        {
            this.Origin = this.InstrumentClient.Origin;
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
            quotePriceClient.Origin = this.InstrumentClient.Origin;
            quotePriceClient.Ask = this.InstrumentClient.Ask;
            quotePriceClient.Bid = this.InstrumentClient.Bid;
        }

        public void AddNewQuotePrice(QuotePriceClient quotePriceClient)
        {
            var existEntity = this.QuotePriceClients.SingleOrDefault(P => P.InstrumentId == quotePriceClient.InstrumentId && P.CustomerId == quotePriceClient.CustomerId);

            if (existEntity != null)
            {
                existEntity.AdjustSingle = existEntity.AdjustSingle;
                existEntity.CustomerId = existEntity.CustomerId;
                existEntity.InstrumentId = existEntity.InstrumentId;
                existEntity.QuoteLot = existEntity.QuoteLot;
                existEntity.IsSelected = existEntity.IsSelected;
                existEntity.TimeStamp = existEntity.TimeStamp;
                existEntity.WaitTimes = existEntity.WaitTimes;
                existEntity.Ask = this.InstrumentClient.Ask;
                existEntity.Bid = this.InstrumentClient.Bid;
                existEntity.Origin = this.InstrumentClient.Origin;
            }
            else
            {
                quotePriceClient.Ask = this.InstrumentClient.Ask;
                quotePriceClient.Bid = this.InstrumentClient.Bid;
                quotePriceClient.Origin = this.InstrumentClient.Origin;
                this._QuotePriceClients.Add(quotePriceClient);
            }
        }

        //Remove QuotePrice
        public void RemoveSendQuotePrice(QuotePriceClient quotePriceClient)
        {
            this._QuotePriceClients.Remove(quotePriceClient);
            if (this._QuotePriceClients.Count == 0)
            {
                this.OnEmptyQuotePriceClient(this);
            }
        }
        public void RemoveSendQuotePrice(ObservableCollection<QuotePriceClient> quotePriceClients)
        {
            lock (this._QuotePriceClients)
            {
                foreach (QuotePriceClient entity in quotePriceClients)
                {
                    this._QuotePriceClients.Remove(entity);
                }
            }
            if (this._QuotePriceClients.Count == 0)
            {
                this.OnEmptyQuotePriceClient(this);
            }
        }

        //Lot Limit
        public void OnEnquiryQuantity(bool isAbove)
        {
            decimal newLot = this.AdjustLot;
            foreach (QuotePriceClient entity in this.QuotePriceClients)
            {
                bool isSelected = false;
                if(isAbove)
                {
                    isSelected = (entity.QuoteLot >= newLot) ? true : false;
                }
                else
                {
                    isSelected = (entity.QuoteLot >= newLot) ? false : true;
                }
                entity.IsSelected = isSelected;
            }
        }
        #endregion

        public void AdjustCurrentPrice(decimal adjust, QuotePriceClient quotePriceClient, bool isAdjustInstrument)
        {
            Quotation quotation = Quotation.Create((double)adjust,
                double.Parse(quotePriceClient.Origin),
                this.InstrumentClient.NumeratorUnit.Value, this.InstrumentClient.Denominator.Value,
                this.InstrumentClient.AutoPoint.Value, this.InstrumentClient.Spread.Value);

            if (isAdjustInstrument)
            {
                this.Origin = quotation.Origin;
                this.Adjust = decimal.Parse(quotation.Origin);
            }

            quotePriceClient.Origin = quotation.Origin;
            quotePriceClient.Ask = quotation.Ask;
            quotePriceClient.Bid = quotation.Bid;
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

        public void AdjustCurrentLot(decimal newLot, bool isAdjustInstrument)
        {
            var adjustQuotePrices = this.QuotePriceClients.Where(P => P.IsSelected);
            foreach (QuotePriceClient entity in adjustQuotePrices)
            {
                if (isAdjustInstrument)
                {
                    entity.AnswerLot = newLot;
                }
            }
        }

        public void SelectAllQuotePrice(bool isFullSelect)
        {
            foreach (QuotePriceClient entity in this.QuotePriceClients)
            {
                entity.IsSelected = isFullSelect;
            }
        }

        public void OnEmptyCheckBoxEvent()
        {
            int childCheckCount = this._QuotePriceClients.Where(P => P.IsSelected == true).Count();
            if (childCheckCount == 0)
            {
                this.SelectedAll = false;
            }
            else if(childCheckCount == this.QuotePriceClients.Count)
            {
                this.SelectedAll = true;
            }
        }
    }
}
