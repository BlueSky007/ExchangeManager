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
        //private double _Ask;
        //private double _Bid;

        public Quotation(PrimitiveQuotation primitiveQuotation, double ask, double bid)
        {
            this._PrimitiveQuotation = primitiveQuotation;
            this.Ask = ask;
            this.Bid = bid;
        }

        public int SourceId { get { return this._PrimitiveQuotation.SourceId; } }
        public int InstrumentId { get { return this._PrimitiveQuotation.InstrumentId; } }
        public PrimitiveQuotation PrimitiveQuotation { get { return this._PrimitiveQuotation; } }
        //public double Ask { get { return this._Ask; } }
        //public double Bid { get { return this._Bid; } }
        public double Ask { get; set; }
        public double Bid { get; set; }
        public DateTime Timestamp { get { return this._PrimitiveQuotation.Timestamp; } }

        public bool IsAbnormal { get; set; }
        public string OutOfRangeType { get; set; }
        public int DiffPoints { get; set; }
        public int ConfirmId { get; set; }
        public int WaitSeconds { get; set; }
        public DateTime WaitEndTime { get; set; }
    }
}
