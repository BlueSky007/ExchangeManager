using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manager.Common.QuotationEntities
{
    public class DerivativeRelation
    {
        public int InstrumentId;
        public int UnderlyingInstrument1Id;
        public bool UnderlyingInstrument1IdInverted;
        public int? UnderlyingInstrument2Id;
        public decimal AdjustPoints;
        public decimal AdjustIncrement;
        public byte AskOperand1Type;
        public byte? AskOperator1Type;
        public byte? AskOperand2Type;
        public byte AskOperator2Type;
        public decimal AskOperand3;
        public byte BidOperand1Type;
        public byte? BidOperator1Type;
        public byte? BidOperand2Type;
        public byte BidOperator2Type;
        public decimal BidOperand3;
        public byte LastOperand1Type;
        public byte? LastOperator1Type;
        public byte? LastOperand2Type;
        public byte LastOperator2Type;
        public decimal LastOperand3;
    }
}
