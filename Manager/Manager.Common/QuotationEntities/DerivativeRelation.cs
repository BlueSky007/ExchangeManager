using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manager.Common.QuotationEntities
{
    public enum OperandType
    {
        Ask,
        Bid,
        Last
    }

    public enum OperatorType
    {
        Multiply,
        Divide
    }

    public class DerivativeRelation : IMetadataObject
    {
        //public int InstrumentId;
        public int Id { get; set; }
        public int UnderlyingInstrument1Id { get; set; }
        public bool UnderlyingInstrument1IdInverted { get; set; }
        public int? UnderlyingInstrument2Id { get; set; }
        public OperandType AskOperand1Type { get; set; }
        public OperatorType? AskOperator1Type { get; set; }
        public OperandType? AskOperand2Type { get; set; }
        public OperatorType AskOperator2Type { get; set; }
        public decimal AskOperand3 { get; set; }
        public OperandType BidOperand1Type { get; set; }
        public OperatorType? BidOperator1Type { get; set; }
        public OperandType? BidOperand2Type { get; set; }
        public OperatorType BidOperator2Type { get; set; }
        public decimal BidOperand3 { get; set; }
        public OperandType LastOperand1Type { get; set; }
        public OperatorType? LastOperator1Type { get; set; }
        public OperandType? LastOperand2Type { get; set; }
        public OperatorType LastOperator2Type { get; set; }
        public decimal LastOperand3 { get; set; }

        public DerivativeRelation Clone()
        {
            return new DerivativeRelation()
            {
                Id = this.Id,
                UnderlyingInstrument1Id = this.UnderlyingInstrument1Id,
                UnderlyingInstrument1IdInverted = this.UnderlyingInstrument1IdInverted,
                UnderlyingInstrument2Id = this.UnderlyingInstrument2Id,
                AskOperand1Type = this.AskOperand1Type,
                AskOperator1Type = this.AskOperator1Type,
                AskOperand2Type = this.AskOperand2Type,
                AskOperator2Type = this.AskOperator2Type,
                AskOperand3 = this.AskOperand3,
                BidOperand1Type = this.BidOperand1Type,
                BidOperator1Type = this.BidOperator1Type,
                BidOperand2Type = this.BidOperand2Type,
                BidOperator2Type = this.BidOperator2Type,
                BidOperand3 = this.BidOperand3,
                LastOperand1Type = this.LastOperand1Type,
                LastOperator1Type = this.LastOperator1Type,
                LastOperand2Type = this.LastOperand2Type,
                LastOperator2Type = this.LastOperator2Type,
                LastOperand3 = this.LastOperand3
            };
        }

        public void Update(Dictionary<string, object> fieldAndValues)
        {
            foreach (string key in fieldAndValues.Keys)
            {
                this.Update(key, fieldAndValues[key]);
            }
        }

        public void Update(string field, object value)
        {
            if (field == FieldSR.UnderlyingInstrument1Id)
            {
                this.UnderlyingInstrument1Id = (int)value;
            }
            else if (field == FieldSR.UnderlyingInstrument1IdInverted)
            {
                this.UnderlyingInstrument1IdInverted = (bool)value;
            }
            else if (field == FieldSR.UnderlyingInstrument2Id)
            {
                this.UnderlyingInstrument2Id = (int?)value;
            }
            else if (field == FieldSR.AskOperand1Type)
            {
                this.AskOperand1Type = (OperandType)value;
            }
            else if (field == FieldSR.AskOperator1Type)
            {
                this.AskOperator1Type = (OperatorType?)value;
            }
            else if (field == FieldSR.AskOperand2Type)
            {
                this.AskOperand2Type = (OperandType?)value;
            }
            else if (field == FieldSR.AskOperator2Type)
            {
                this.AskOperator2Type = (OperatorType)value;
            }
            else if (field == FieldSR.AskOperand3)
            {
                this.AskOperand3 = (decimal)value;
            }
            else if (field == FieldSR.BidOperand1Type)
            {
                this.BidOperand1Type = (OperandType)value;
            }
            else if (field == FieldSR.BidOperator1Type)
            {
                this.BidOperator1Type = (OperatorType?)value;
            }
            else if (field == FieldSR.BidOperand2Type)
            {
                this.BidOperand2Type = (OperandType?)value;
            }
            else if (field == FieldSR.BidOperator2Type)
            {
                this.BidOperator2Type = (OperatorType)value;
            }
            else if (field == FieldSR.BidOperand3)
            {
                this.BidOperand3 = (decimal)value;
            }
            else if (field == FieldSR.LastOperand1Type)
            {
                this.LastOperand1Type = (OperandType)value;
            }
            else if (field == FieldSR.LastOperator1Type)
            {
                this.LastOperator1Type = (OperatorType?)value;
            }
            else if (field == FieldSR.LastOperand2Type)
            {
                this.LastOperand2Type = (OperandType?)value;
            }
            else if (field == FieldSR.LastOperator2Type)
            {
                this.LastOperator2Type = (OperatorType)value;
            }
            else if (field == FieldSR.LastOperand3)
            {
                this.LastOperand3 = (decimal)value;
            }
        }
    }
}
