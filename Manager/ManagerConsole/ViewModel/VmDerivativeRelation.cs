using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Manager.Common.QuotationEntities;

namespace ManagerConsole.ViewModel
{
    public class VmDerivativeRelation : VmBase
    {
        private DerivativeRelation _DerivativeRelation;
        public VmDerivativeRelation(DerivativeRelation derivativeRelation)
            : base(derivativeRelation)
        {
            this._DerivativeRelation = derivativeRelation;
        }

        public DerivativeRelation DerivativeRelation { get { return this._DerivativeRelation; } }

        public int Id { get { return this._DerivativeRelation.Id; } set { this._DerivativeRelation.Id = value; } }
        public int UnderlyingInstrument1Id
        {
            get { return this._DerivativeRelation.UnderlyingInstrument1Id; }
            set
            {
                if (this._DerivativeRelation.UnderlyingInstrument1Id != value)
                {
                    this.SubmitChange(FieldSR.UnderlyingInstrument1Id, value);
                }
            }
        }
        public bool UnderlyingInstrument1IdInverted
        {
            get { return this._DerivativeRelation.UnderlyingInstrument1IdInverted; }
            set
            {
                if (this._DerivativeRelation.UnderlyingInstrument1IdInverted != value)
                {
                    this.SubmitChange(FieldSR.UnderlyingInstrument1IdInverted, value);
                }
            }
        }
        public int? UnderlyingInstrument2Id
        {
            get { return this._DerivativeRelation.UnderlyingInstrument2Id; }
            set
            {
                if (this._DerivativeRelation.UnderlyingInstrument2Id != value)
                {
                    this.SubmitChange(FieldSR.UnderlyingInstrument2Id, value);
                }
            }
        }

        public OperandType AskOperand1Type
        {
            get { return this._DerivativeRelation.AskOperand1Type; }
            set
            {
                if (this._DerivativeRelation.AskOperand1Type != value)
                {
                    this.SubmitChange(FieldSR.AskOperand1Type, value);
                }
            }
        }

        public OperatorType? AskOperator1Type
        {
            get { return this._DerivativeRelation.AskOperator1Type; }
            set
            {
                if (this._DerivativeRelation.AskOperator1Type != value)
                {
                    this.SubmitChange(FieldSR.AskOperator1Type, value);
                }
            }
        }

        public OperandType? AskOperand2Type
        {
            get { return this._DerivativeRelation.AskOperand2Type; }
            set
            {
                if (this._DerivativeRelation.AskOperand2Type != value)
                {
                    this.SubmitChange(FieldSR.AskOperand2Type, value);
                }
            }
        }

        public OperatorType AskOperator2Type
        {
            get { return this._DerivativeRelation.AskOperator2Type; }
            set
            {
                if (this._DerivativeRelation.AskOperator2Type != value)
                {
                    this.SubmitChange(FieldSR.AskOperator2Type, value);
                }
            }
        }

        public decimal AskOperand3
        {
            get { return this._DerivativeRelation.AskOperand3; }
            set
            {
                if (this._DerivativeRelation.AskOperand3 != value)
                {
                    this.SubmitChange(FieldSR.AskOperand3, value);
                }
            }
        }

        public OperandType BidOperand1Type
        {
            get { return this._DerivativeRelation.BidOperand1Type; }
            set
            {
                if (this._DerivativeRelation.BidOperand1Type != value)
                {
                    this.SubmitChange(FieldSR.BidOperand1Type, value);
                }
            }
        }

        public OperatorType? BidOperator1Type
        {
            get { return this._DerivativeRelation.BidOperator1Type; }
            set
            {
                if (this._DerivativeRelation.BidOperator1Type != value)
                {
                    this.SubmitChange(FieldSR.BidOperator1Type, value);
                }
            }
        }

        public OperandType? BidOperand2Type
        {
            get { return this._DerivativeRelation.BidOperand2Type; }
            set
            {
                if (this._DerivativeRelation.BidOperand2Type != value)
                {
                    this.SubmitChange(FieldSR.BidOperand2Type, value);
                }
            }
        }

        public OperatorType BidOperator2Type
        {
            get { return this._DerivativeRelation.BidOperator2Type; }
            set
            {
                if (this._DerivativeRelation.BidOperator2Type != value)
                {
                    this.SubmitChange(FieldSR.BidOperator2Type, value);
                }
            }
        }

        public decimal BidOperand3
        {
            get { return this._DerivativeRelation.BidOperand3; }
            set
            {
                if (this._DerivativeRelation.BidOperand3 != value)
                {
                    this.SubmitChange(FieldSR.BidOperand3, value);
                }
            }
        }

        public OperandType LastOperand1Type
        {
            get { return this._DerivativeRelation.LastOperand1Type; }
            set
            {
                if (this._DerivativeRelation.LastOperand1Type != value)
                {
                    this.SubmitChange(FieldSR.LastOperand1Type, value);
                }
            }
        }

        public OperatorType? LastOperator1Type
        {
            get { return this._DerivativeRelation.LastOperator1Type; }
            set
            {
                if (this._DerivativeRelation.LastOperator1Type != value)
                {
                    this.SubmitChange(FieldSR.LastOperator1Type, value);
                }
            }
        }

        public OperandType? LastOperand2Type
        {
            get { return this._DerivativeRelation.LastOperand2Type; }
            set
            {
                if (this._DerivativeRelation.LastOperand2Type != value)
                {
                    this.SubmitChange(FieldSR.LastOperand2Type, value);
                }
            }
        }

        public OperatorType LastOperator2Type
        {
            get { return this._DerivativeRelation.LastOperator2Type; }
            set
            {
                if (this._DerivativeRelation.LastOperator2Type != value)
                {
                    this.SubmitChange(FieldSR.LastOperator2Type, value);
                }
            }
        }

        public decimal LastOperand3
        {
            get { return this._DerivativeRelation.LastOperand3; }
            set
            {
                if (this._DerivativeRelation.LastOperand3 != value)
                {
                    this.SubmitChange(FieldSR.LastOperand3, value);
                }
            }
        }

        public void SubmitChange(string filed, object value)
        {
            base.SubmitChange(MetadataType.DerivativeRelation, filed, value);
        }
    }
}
