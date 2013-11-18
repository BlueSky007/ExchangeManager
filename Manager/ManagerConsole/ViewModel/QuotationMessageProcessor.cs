using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Manager.Common;

namespace ManagerConsole.ViewModel
{
    public class QuotationMessageProcessor
    {
        public static QuotationMessageProcessor Instance = new QuotationMessageProcessor();

        private ObservableCollection<SourceQuotation> _SourceQuotations = null;

        private QuotationMessageProcessor() { }

        public ObservableCollection<SourceQuotation> QuotationSources
        {
            get
            {
                if (this._SourceQuotations == null)
                {
                    this._SourceQuotations = new ObservableCollection<SourceQuotation>();
                }
                return this._SourceQuotations;
            }
        }

        public void Process(PrimitiveQuotationMessage message)
        {
            if(this._SourceQuotations != null)
            {
                SourceQuotation source = this._SourceQuotations.SingleOrDefault(q => q.Id == message.Quotation.SourceId);
                if (source == null)
                {
                    source = new SourceQuotation(message.Quotation.SourceId, message.Quotation.SourceName);
                }

                if (source.Quotations.Count > 20) source.Quotations.RemoveAt(0);

                source.Quotations.Add(new PrimitiveQuotation
                {
                    InstrumentCode = message.Quotation.Symbol,
                    Bid = message.Quotation.Bid,
                    Ask = message.Quotation.Ask,
                    Last = message.Quotation.Last,
                    High = message.Quotation.High,
                    Low = message.Quotation.Low,
                    Timestamp = message.Quotation.Timestamp
                });
            }
        }

        public void Process(AbnormalQuotationMessage abnormalQuotationMessage)
        {

        }

        public void Process(SourceStatusMessage sourceStatusMessage)
        {

        }
    }
}
