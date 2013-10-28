using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Manager.Common;
using Manager.Common.QuotationEntities;

namespace ManagerService.Quotation
{
    public class Quotation
    {
        private PrimitiveQuotation _PrimitiveQuotation;
        private SourceInstrumentKey _Key;


        public int SourceId { get; private set; }
        public int InstrumentId { get; private set; }

        public double? Ask { get; private set; }
        public double? Bid { get; private set; }
        public double? Last { get; private set; }
        public double? High { get; private set; }
        public double? Low { get; private set; }

        public DateTime Timestamp { get { return this._PrimitiveQuotation.Timestamp; } }

        public SourceInstrumentKey Key
        {
            get
            {
                if (this._Key == null)
                {
                    this._Key = new SourceInstrumentKey(this.SourceId, this.InstrumentId);
                }
                return this._Key;
            }
        }

        private Quotation(PrimitiveQuotation primitiveQuotation)
        {
            this._PrimitiveQuotation = primitiveQuotation;

        }

        public static Quotation Create(int instrumentId, int sourceId, PrimitiveQuotation primitiveQuotation)
        {
            Quotation quotation = new Quotation(primitiveQuotation);
            quotation.InstrumentId = instrumentId;
            quotation.SourceId = sourceId;
            return quotation;
        }
    }
}
