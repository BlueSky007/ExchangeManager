using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Manager.Common;
using Manager.Common.QuotationEntities;

namespace ManagerService.Quotation
{
    public class SourceQuotation
    {
        private PrimitiveQuotation _PrimitiveQuotation;
        private GeneralQuotation _Quotation;

        public SourceQuotation(PrimitiveQuotation primitiveQuotation, double ask, double bid, string instrumentCode, bool inverted)
        {
            this._PrimitiveQuotation = primitiveQuotation;
            this._Quotation = new GeneralQuotation();
            this._Quotation.InstrumentId = primitiveQuotation.InstrumentId;
            this._Quotation.SourceId = primitiveQuotation.SourceId;
            this._Quotation.OriginCode = instrumentCode;
            this._Quotation.Timestamp = primitiveQuotation.Timestamp;

            this._Quotation.Ask = ask;
            this._Quotation.Bid = bid;

            double price;
            if (PrimitiveQuotation.TryGetPriceValue(primitiveQuotation.Last, out price)) this._Quotation.Last = price;
            if (PrimitiveQuotation.TryGetPriceValue(primitiveQuotation.High, out price)) this._Quotation.High = price;
            if (PrimitiveQuotation.TryGetPriceValue(primitiveQuotation.Low, out price)) this._Quotation.Low = price;

            if (inverted)
            {
                this._Quotation.Ask = 1 / this._Quotation.Ask;
                this._Quotation.Bid = 1 / this._Quotation.Bid;
                if (this._Quotation.Last.HasValue) this._Quotation.Last = 1 / this._Quotation.Last;
                if (this._Quotation.High.HasValue) this._Quotation.High = 1 / this._Quotation.High;
                if (this._Quotation.Low.HasValue) this._Quotation.Low = 1 / this._Quotation.Low;
            }
        }

        public PrimitiveQuotation PrimitiveQuotation { get { return this._PrimitiveQuotation; } }
        public GeneralQuotation Quotation { get { return this._Quotation; } }

        public int SourceId { get { return this._Quotation.SourceId; } }
        public int InstrumentId { get { return this._Quotation.InstrumentId; } }
        public string InstrumentCode { get { return this._Quotation.OriginCode; } }
        public double Ask { get { return this._Quotation.Ask; } set { this._Quotation.Ask = value; } }
        public double Bid { get { return this._Quotation.Bid; } set { this._Quotation.Bid = value; } }
        public double? Last { get { return this._Quotation.Last; } }
        public double? High { get { return this._Quotation.High; } }
        public double? Low { get { return this._Quotation.Low; } }
        public DateTime Timestamp { get { return this._Quotation.Timestamp; } }

        public bool IsAbnormal { get; set; }
        public string OutOfRangeType { get; set; }
        public int DiffPoints { get; set; }
        public int ConfirmId { get; set; }
        public int WaitSeconds { get; set; }
        public DateTime WaitEndTime { get; set; }
    }
}
