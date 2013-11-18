using Manager.Common;
using ManagerConsole.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ManagerConsole.Model
{
    public class QuotePriceClient : PropertyChangedNotifier
    {
        private Guid _Id = Guid.NewGuid();
        private string _ExchangeCode;
        private Customer _CustomerClient;
        private bool _IsSelected = true;
        private int _BSStatus;
        private string _QuoteMessage = string.Empty;
        private Guid _InstrumentId;
        private string _InstrumentCode;
        private DateTime _TimeStamp;
        private int _WaitTimes;
        private string _Origin;
        private string _LastOrigin;
        private string _Diff;
        private string _Bid;
        private string _Ask;
        private decimal _QuoteLot;
        private decimal _AnswerLot;
        private Guid _CustomerId;
        private string _CustomerCode;
        private bool _IsBuy;
        private decimal _AdjustSingle;

        public QuotePriceClient(QuoteMessage quoteMessage,int waitTimes,Customer customer)
        {
            this._CustomerClient = customer;
            this._ExchangeCode = quoteMessage.ExchangeCode;
            this._CustomerId = quoteMessage.CustomerID;
            this._InstrumentId = quoteMessage.InstrumentID;
            this._QuoteLot = (decimal)quoteMessage.QuoteLot;
            this._AnswerLot = (decimal)quoteMessage.QuoteLot;
            this._BSStatus = quoteMessage.BSStatus;
            this._WaitTimes = waitTimes;
        }

        public QuotePriceClient(QuoteQuotation quoteQuotation, int waitTimes)
        {
            this._CustomerId = quoteQuotation.CustomerId;
            this._CustomerCode = quoteQuotation.CustomerCode;
            this._Ask = quoteQuotation.Ask;
            this._Bid = quoteQuotation.Bid;
            this._QuoteLot = quoteQuotation.QuoteLot;
            this._AnswerLot = quoteQuotation.QuoteLot;
            this._IsBuy = quoteQuotation.IsBuy;
            this._Origin = quoteQuotation.Origin;
            this._InstrumentId = quoteQuotation.InstrumentId;
            this._TimeStamp = new DateTime(DateTime.Now.Ticks, DateTimeKind.Utc);
            this._WaitTimes = waitTimes;
        }

        public Guid Id
        {
            get { return this._Id; }
        }

        public string ExchangeCode
        {
            get { return this._ExchangeCode; }
        }

        public Customer CustomerClient
        {
            get { return this._CustomerClient; }
            set { this._CustomerClient = value; }
        }
        public bool IsSelected
        {
            get { return this._IsSelected; }
            set { this._IsSelected = value; this.OnPropertyChanged("IsSelected"); }
        }

        public int BSStatus
        {
            get { return this._BSStatus; }
            set { this._BSStatus = value; this.OnPropertyChanged("BSStatus"); }
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
        public string InstrumentCode
        {
            get { return this._InstrumentCode; }
            set { this._InstrumentCode = value; this.OnPropertyChanged("InstrumentCode"); }
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

        public string Origin
        {
            get { return this._Origin; }
            set { this._Origin = value; this.OnPropertyChanged("Origin"); }
        }

        public string LastOrigin
        {
            get { return this._LastOrigin; }
            set { this._LastOrigin = value; this.OnPropertyChanged("LastOrigin"); }
        }

        public string Diff
        {
            get { return this._Diff; }
            set { this._Diff = value; }
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

        public decimal QuoteLot
        {
            get { return this._QuoteLot; }
            set { this._AnswerLot = value; }
        }

        public decimal AnswerLot
        {
            get { return this._AnswerLot; }
            set { this._AnswerLot = value; this.OnPropertyChanged("AnswerLot"); }
        }
        public Guid CustomerId
        {
            get { return this._CustomerId; }
            set { this._CustomerId = value; }
        }

        public string CustomerCode
        {
            get { return this._CustomerClient.Code; }
            set { this._CustomerCode = value; this.OnPropertyChanged("CustomerCode"); }
        }

        public decimal AdjustSingle
        {
            get { return this._AdjustSingle; }
            set
            {
                this._AdjustSingle = value;
            }
        }

        public Answer ToSendQutoPrice()
        {
            Answer sendQuote = new Answer();
            sendQuote.ExchangeCode = this._ExchangeCode;
            sendQuote.CustomerId = this._CustomerId;
            sendQuote.CustomerCode = this._CustomerCode;
            sendQuote.InstrumentId = this._InstrumentId;
            sendQuote.InstrumentCode = this.InstrumentCode;
            sendQuote.Origin = this._Origin;
            sendQuote.Ask = this._Ask;
            sendQuote.Bid = this._Bid;
            sendQuote.QuoteLot = this._QuoteLot;
            sendQuote.AnswerLot = this._AnswerLot;
            sendQuote.SendTime = DateTime.Now;

            return sendQuote;
        }
        //public QuoteQuotation ToQuoteQuotation()
        //{
        //    QuoteQuotation quoteQuotation = new QuoteQuotation();
        //    quoteQuotation.Origin = this._Origin;
        //    quoteQuotation.Ask = this._Ask;
        //    quoteQuotation.Bid = this._Bid;
        //    quoteQuotation.CustomerId = this.CustomerId;
        //    quoteQuotation.IsBuy = this._IsBuy;
        //    quoteQuotation.QuoteLot = this.QuoteLot;
        //    quoteQuotation.InstrumentId = this._InstrumentId;

        //    return quoteQuotation;
        //}
    }
}
