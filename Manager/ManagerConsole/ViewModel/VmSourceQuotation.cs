using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.ComponentModel;
using ManagerConsole.Helper;
using Manager.Common.QuotationEntities;

namespace ManagerConsole.ViewModel
{
    public class VmSourceQuotation : PropertyChangedNotifier
    {
        private PrimitiveQuotation _PrimitiveQuotation;

        //public VmSourceQuotation(PrimitiveQuotation primitiveQuotation, string instrumentCode)
        public VmSourceQuotation(PrimitiveQuotation primitiveQuotation)
        {
            this._PrimitiveQuotation = primitiveQuotation;
            //this.InstrumentCode = instrumentCode;
        }
        //public string InstrumentCode { get; private set; }

        public int SourceId { get { return this._PrimitiveQuotation.SourceId; } }
        public int InstrumentId { get { return this._PrimitiveQuotation.InstrumentId; } }
        public string SourceName { get { return this._PrimitiveQuotation.SourceName; } }
        public string Symbol { get { return this._PrimitiveQuotation.Symbol; } }
        public string Bid
        {
            get { return this._PrimitiveQuotation.Bid; }
            set
            {
                if (this._PrimitiveQuotation.Bid != value)
                {
                    this._PrimitiveQuotation.Bid = value;
                    this.OnPropertyChanged("Bid");
                }
            }
        }
        public string Ask
        {
            get { return this._PrimitiveQuotation.Ask; }
            set
            {
                if (this._PrimitiveQuotation.Ask != value)
                {
                    this._PrimitiveQuotation.Ask = value;
                    this.OnPropertyChanged("Ask");
                }
            }
        }
        public string Last { get { return this._PrimitiveQuotation.Last; } }
        public string High { get { return this._PrimitiveQuotation.High; } }
        public string Low { get { return this._PrimitiveQuotation.Low; } }
        public DateTime Timestamp { get { return this._PrimitiveQuotation.Timestamp; } }
    }

}
