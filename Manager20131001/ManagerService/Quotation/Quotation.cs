using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Manager.Common;

namespace ManagerService.Quotation
{
    public class Quotation
    {
        private PrimitiveQuotation _PrimitiveQuotation;
        public int SourceId { get; private set; }
        public int InstrumentId { get; private set; }

        public double? Ask { get; private set; }
        public double? Bid { get; private set; }
        public double? Last { get; private set; }
        public double? High { get; private set; }
        public double? Low { get; private set; }

        private Quotation(PrimitiveQuotation primitiveQuotation)
        {
            this._PrimitiveQuotation = primitiveQuotation;

        }

        public static Quotation Create(PrimitiveQuotation primitiveQuotation, ConfigMetadata metadata)
        {
            Quotation quotation = new Quotation(primitiveQuotation);
            quotation.SourceId = metadata.GetSourceId(primitiveQuotation.SourceName);
            quotation.InstrumentId = metadata.GetInstrumentId(primitiveQuotation.InstrumentCode);

            return quotation;
        }
    }
}
