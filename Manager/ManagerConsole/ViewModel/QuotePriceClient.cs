using iExchange.Common.Manager;
using Manager.Common;
using ManagerConsole.Helper;
using ManagerConsole.Model;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Customer = ManagerConsole.Model.Customer;
using QuoteMessage = Manager.Common.QuoteMessage;

namespace ManagerConsole.ViewModel
{
    public class QuotePriceClientModel
    {
        public delegate void BindingQuotePriceUIHandle();
        public event BindingQuotePriceUIHandle OnBindingQuotePriceUIEvent;

        public ObservableCollection<QuotePriceClient> QuotePriceClients;
        public QuotePriceForInstrument QuotePriceForInstrument;

        public QuotePriceClientModel()
        {
            this.QuotePriceClients = new ObservableCollection<QuotePriceClient>();
        }

        public void RemoveSendQuotePrice(QuotePriceClient quotePriceClient)
        {
            bool isCurrentQuotePrice = (quotePriceClient.Id == this.QuotePriceForInstrument.QuoteId);
            this.QuotePriceClients.Remove(quotePriceClient);

            if (this.QuotePriceClients.Count == 0)
            {
                this.QuotePriceForInstrument = null;
                return;
            }

            bool isCurrentInstrument = (quotePriceClient.InstrumentId == this.QuotePriceClients[0].InstrumentId);
            if (isCurrentInstrument && this.QuotePriceClients.Count > 0)
            {
                this.QuotePriceForInstrument.SwitchNewInstrument(this.QuotePriceClients[0],false);
                this.QuotePriceForInstrument.SumBuyLot -= quotePriceClient.BuyLot;
                this.QuotePriceForInstrument.SumSellLot -= quotePriceClient.SellLot;
            }
            else
            {
                this.QuotePriceForInstrument.SwitchNewInstrument(this.QuotePriceClients[0], true);
                var quotePriceClients = this.QuotePriceClients.Where(P => P.InstrumentId == this.QuotePriceForInstrument.Instrument.Id);
                foreach (QuotePriceClient entity in quotePriceClients)
                {
                    this.QuotePriceForInstrument.SumBuyLot += entity.BuyLot;
                    this.QuotePriceForInstrument.SumSellLot += entity.SellLot;
                }
            }
            this.QuotePriceForInstrument.CreateBestPrice(true);
        }

        public void AddSendQuotePrice(QuotePriceClient quotePriceClient)
        {
            this.QuotePriceClients.Add(quotePriceClient);

            if (this.QuotePriceForInstrument == null)
            {
                this.QuotePriceForInstrument = new QuotePriceForInstrument(quotePriceClient);
                if (this.OnBindingQuotePriceUIEvent != null)
                {
                    this.OnBindingQuotePriceUIEvent();
                }
            }
            else
            {
                bool isCurrentInstrument = (quotePriceClient.InstrumentId == this.QuotePriceForInstrument.Instrument.Id);
                if (isCurrentInstrument)
                {
                    this.QuotePriceForInstrument.SumBuyLot += quotePriceClient.BuyLot;
                    this.QuotePriceForInstrument.SumSellLot += quotePriceClient.SellLot;
                }
                this.QuotePriceForInstrument.CreateBestPrice(true);
            }
        }

        public void AdjustLot(bool isIncrease)
        {
            if (this.QuotePriceForInstrument == null) return;
            var adjustQuoteEntities = this.QuotePriceClients.Where(P => P.InstrumentId == this.QuotePriceForInstrument.Instrument.Id);
            if (adjustQuoteEntities == null) return;
            if (isIncrease)
            {
                foreach (QuotePriceClient entity in adjustQuoteEntities)
                {
                    if (entity.Lot > entity.AnswerLot + 1)
                    {
                        entity.AnswerLot++;
                    }
                }
                this.QuotePriceForInstrument.AnswerLot++;
            }
            else
            {
                foreach (QuotePriceClient entity in adjustQuoteEntities)
                {
                    entity.AnswerLot--;
                }
                this.QuotePriceForInstrument.AnswerLot--;
            }
        }

        public void UpdateLot(decimal answerLot)
        {
            if (this.QuotePriceForInstrument == null) return;
            var adjustQuoteEntities = this.QuotePriceClients.Where(P => P.InstrumentId == this.QuotePriceForInstrument.Instrument.Id);

            foreach (QuotePriceClient entity in adjustQuoteEntities)
            {
                entity.AnswerLot = answerLot;
            }
        }

        public void AdjustPrice(bool isAdd)
        {
            this.QuotePriceForInstrument.AdjustPrice(isAdd);
        }
    }

    public class QuotePriceClient : PropertyChangedNotifier
    {
        #region Private Property
        private Guid _Id = Guid.NewGuid();
        private string _QuoteMessage = string.Empty;
        private string _ExchangeCode;
        private Guid _InstrumentId;
        private InstrumentClient _Instrument;
        private decimal _BuyLot = decimal.Zero;
        private decimal _SellLot = decimal.Zero;
        private decimal _Lot = decimal.Zero;
        private string _Origin;
        private int _WaitTimes;
        private decimal _AnswerLot = decimal.Zero;
        private BSStatus _BSStatus;
        private Customer _CustomerClient;
        private Guid _CustomerId;
        private DateTime _TimeStamp;
        #endregion

        public QuotePriceClient(QuoteMessage quoteMessage, int waitTimes, InstrumentClient instrument, Customer customer)
        {
            this._CustomerClient = customer;
            this._Instrument = instrument;
            this._ExchangeCode = quoteMessage.ExchangeCode;
            this._CustomerId = quoteMessage.CustomerID;
            this._InstrumentId = quoteMessage.InstrumentID;

            this._Lot = (decimal)quoteMessage.QuoteLot;
            this._AnswerLot = this._Lot;
            this._BSStatus = (BSStatus)quoteMessage.BSStatus;
            this._BuyLot = this._BSStatus == BSStatus.Buy ? this._Lot : decimal.Zero;
            this._SellLot = this._BSStatus == BSStatus.Sell ? this._Lot : decimal.Zero;
            if (this._BSStatus == BSStatus.Both)
            {
                this._BuyLot = this._Lot;
                this._SellLot = this._Lot;
            }
            this._WaitTimes = waitTimes;
            this._TimeStamp = quoteMessage.TimeStamp;
        }

        #region Public property
        public Guid Id
        {
            get { return this._Id; }
        }

        public string Origin
        {
            get { return this._Origin; }
            set { this._Origin = value; }
        }

        public string ExchangeCode
        {
            get { return this._ExchangeCode; }
        }

        public string QuoteMessage
        {
            get { return this._QuoteMessage; }
            set { this._QuoteMessage = value; }
        }

        public Guid InstrumentId
        {
            get { return this._InstrumentId; }
            set { this._InstrumentId = value; }
        }

        public InstrumentClient Instrument
        {
            get { return this._Instrument; }
            set
            {
                this._Instrument = value;
                this.OnPropertyChanged("Instrument");
                this.OnPropertyChanged("InstrumentCode");
            }
        }

        public string InstrumentCode
        {
            get { return this._Instrument.Code; }
        }

        public Customer CustomerClient
        {
            get { return this._CustomerClient; }
            set { this._CustomerClient = value; }
        }

        public Guid CustomerId
        {
            get { return this._CustomerId; }
            set { this._CustomerId = value; }
        }

        public string CustomerCode
        {
            get { return this._CustomerClient.Code; }
        }

        public BSStatus BSStatus
        {
            get { return this._BSStatus; }
            set { this._BSStatus = value; this.OnPropertyChanged("BSStatus"); }
        }

        public decimal BuyLot
        {
            get { return this._BuyLot; }
            set { this._BuyLot = value; }
        }

        public decimal SellLot
        {
            get { return this._SellLot; }
            set { this._SellLot = value; }
        }

        public decimal Lot
        {
            get { return this._Lot; }
            set { this._Lot = value; }
        }

        public decimal AnswerLot
        {
            get { return this._AnswerLot; }
            set { this._AnswerLot = value; this.OnPropertyChanged("AnswerLot"); }
        }


        public DateTime TimeStamp
        {
            get { return this._TimeStamp; }
            set { this._TimeStamp = value; this.OnPropertyChanged("TimeStamp"); }
        }

        public int WaitTimes
        {
            get { return this._WaitTimes; }
            set { this._WaitTimes = value; this.OnPropertyChanged("WaitTimes"); }
        }

        #endregion

        #region Relation property
        public bool IsSelected
        {
            get;
            set;
        }
        #endregion
        public Answer ToSendQutoPrice(string bestBuy,string bestSell)
        {
            Answer sendQuote = new Answer();
            sendQuote.ExchangeCode = this._ExchangeCode;
            sendQuote.CustomerId = this._CustomerId;
            sendQuote.CustomerCode = this.CustomerCode;
            sendQuote.InstrumentId = this._InstrumentId;
            sendQuote.InstrumentCode = this.InstrumentCode;
            sendQuote.Origin = this._Origin;
            sendQuote.Ask = bestBuy;
            sendQuote.Bid = bestSell;
            sendQuote.QuoteLot = this._Lot;
            sendQuote.AnswerLot = this._AnswerLot;
            sendQuote.SendTime = DateTime.Now;

            return sendQuote;
        }
    }
}
