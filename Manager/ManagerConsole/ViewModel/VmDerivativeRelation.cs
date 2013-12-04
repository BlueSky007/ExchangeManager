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

        public int Id { get { return this._DerivativeRelation.Id; } set { this._DerivativeRelation.Id = value; } }
        public int UnderlyingInstrument1Id
        {
            get { return this._DerivativeRelation.UnderlyingInstrument1Id; }
            set
            {
                if (this._DerivativeRelation.UnderlyingInstrument1Id != value)
                {
                    this._DerivativeRelation.UnderlyingInstrument1Id = value;
                    this.OnPropertyChanged(FieldSR.UnderlyingInstrument1Id);
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
                    this._DerivativeRelation.UnderlyingInstrument1IdInverted = value;
                    this.OnPropertyChanged(FieldSR.UnderlyingInstrument1IdInverted);
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
                    this._DerivativeRelation.UnderlyingInstrument2Id = value;
                    this.OnPropertyChanged(FieldSR.UnderlyingInstrument2Id);
                }
            }
        }

        public decimal AdjustPoints
        {
            get { return this._DerivativeRelation.AdjustPoints; }
            set
            {
                if (this._DerivativeRelation.AdjustPoints != value)
                {
                    this._DerivativeRelation.AdjustPoints = value;
                    this.OnPropertyChanged(FieldSR.AdjustPoints);
                }
            }
        }

        public decimal AdjustIncrement
        {
            get { return this._DerivativeRelation.AdjustIncrement; }
            set
            {
                if (this._DerivativeRelation.AdjustIncrement != value)
                {
                    this._DerivativeRelation.AdjustIncrement = value;
                    this.OnPropertyChanged(FieldSR.AdjustIncrement);
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
                    this._DerivativeRelation.AskOperand1Type = value;
                    this.OnPropertyChanged(FieldSR.AskOperand1Type);
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
                    this._DerivativeRelation.AskOperator1Type = value;
                    this.OnPropertyChanged(FieldSR.AskOperator1Type);
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
                    this._DerivativeRelation.AskOperand2Type = value;
                    this.OnPropertyChanged(FieldSR.AskOperand2Type);
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
                    this._DerivativeRelation.AskOperator2Type = value;
                    this.OnPropertyChanged(FieldSR.AskOperator2Type);
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
                    this._DerivativeRelation.AskOperand3 = value;
                    this.OnPropertyChanged(FieldSR.AskOperand3);
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
                    this._DerivativeRelation.BidOperand1Type = value;
                    this.OnPropertyChanged(FieldSR.BidOperand1Type);
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
                    this._DerivativeRelation.BidOperator1Type = value;
                    this.OnPropertyChanged(FieldSR.BidOperator1Type);
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
                    this._DerivativeRelation.BidOperand2Type = value;
                    this.OnPropertyChanged(FieldSR.BidOperand2Type);
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
                    this._DerivativeRelation.BidOperator2Type = value;
                    this.OnPropertyChanged(FieldSR.BidOperator2Type);
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
                    this._DerivativeRelation.BidOperand3 = value;
                    this.OnPropertyChanged(FieldSR.BidOperand3);
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
                    this._DerivativeRelation.LastOperand1Type = value;
                    this.OnPropertyChanged(FieldSR.LastOperand1Type);
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
                    this._DerivativeRelation.LastOperator1Type = value;
                    this.OnPropertyChanged(FieldSR.LastOperator1Type);
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
                    this._DerivativeRelation.LastOperand2Type = value;
                    this.OnPropertyChanged(FieldSR.LastOperand2Type);
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
                    this._DerivativeRelation.LastOperator2Type = value;
                    this.OnPropertyChanged(FieldSR.LastOperator2Type);
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
                    this._DerivativeRelation.LastOperand3 = value;
                    this.OnPropertyChanged(FieldSR.LastOperand3);
                }
            }
        }

    }
}
