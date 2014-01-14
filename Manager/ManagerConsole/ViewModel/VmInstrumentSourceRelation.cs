using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Manager.Common;
using Manager.Common.QuotationEntities;
using ManagerConsole.Helper;
using ManagerConsole.Model;
using System.Windows.Controls;

namespace ManagerConsole.ViewModel
{
    public class SwitchTimeoutRule : ValidationRule
    {
        private int _min;
        private int _max;

        public SwitchTimeoutRule()
        {
        }

        public int Min
        {
            get { return _min; }
            set { _min = value; }
        }

        public int Max
        {
            get { return _max; }
            set { _max = value; }
        }

        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            int switchTimeout = 0;

            try
            {
                if (((string)value).Length > 0)
                    switchTimeout = Int32.Parse((String)value);
            }
            catch (Exception e)
            {
                return new ValidationResult(false, "Illegal characters or " + e.Message);
            }

            if ((switchTimeout < Min) || (switchTimeout > Max))
            {
                return new ValidationResult(false,
                  "Please enter an age in the range: " + Min + " - " + Max + ".");
            }
            else
            {
                return new ValidationResult(true, null);
            }
        }
    }

    public class VmInstrumentSourceRelation : VmQuotationBase
    {
        private InstrumentSourceRelation _Relation;
        private VmInstrument _Instrument;
        private VmQuotationSource _QuotationSource;
        private int _ZIndex;

        public VmInstrumentSourceRelation(InstrumentSourceRelation relation, VmInstrument instrument, VmQuotationSource quotationSource)
            : base(relation)
        {
            this._Relation = relation;
            this._Instrument = instrument;
            this._QuotationSource = quotationSource;
            this.SourceQuotations = new ObservableCollection<VmSourceQuotation>();
        }

        public InstrumentSourceRelation InstrumentSourceRelation { get { return this._Relation; } }

        public ObservableCollection<VmSourceQuotation> SourceQuotations { get; set; }

        public VmInstrument vmInstrument { get { return this._Instrument; } }
        public VmQuotationSource QuotationSource { get { return this._QuotationSource; } }

        public int ZIndex
        {
            get { return this._ZIndex; }
            set
            {
                if(this._ZIndex != value)
                {
                    this._ZIndex = value;
                    this.OnPropertyChanged("ZIndex");
                }
            }
        }

        public int Id { get { return this._Relation.Id; } set { this._Relation.Id = value; } }
        public int SourceId
        {
            get
            {
                return this._Relation.SourceId;
            }
            set
            {
                if (this._Relation.SourceId != value)
                {
                    this._Relation.SourceId = value;
                    this.OnPropertyChanged(FieldSR.SourceId);
                }
            }
        }
        public string SourceSymbol
        {
            get
            {
                return this._Relation.SourceSymbol;
            }
            set
            {
                if (this._Relation.SourceSymbol != value)
                {
                    this._Relation.SourceSymbol = value;
                    this.OnPropertyChanged(FieldSR.SourceSymbol);
                }
            }
        }
        public int InstrumentId
        {
            get
            {
                return this._Relation.InstrumentId;
            }
            set
            {
                if (this._Relation.InstrumentId != value)
                {
                    this._Relation.InstrumentId = value;
                    this.OnPropertyChanged(FieldSR.InstrumentId);
                }
            }
        }
        public bool Inverted
        {
            get
            {
                return this._Relation.Inverted;
            }
            set
            {
                if (this._Relation.Inverted != value)
                {
                    this._Relation.Inverted = value;
                    this.OnPropertyChanged(FieldSR.Inverted);
                }
            }
        }

        public bool IsActive
        {
            get
            {
                return this._Relation.IsActive;
            }
            set
            {
                if (this._Relation.IsActive != value)
                {
                    this._Relation.IsActive = value;
                    this.OnPropertyChanged(FieldSR.IsActive);
                    this.OnPropertyChanged("ActiveState");
                }
            }
        }

        public string ActiveState
        {
            get
            {
                return this.IsActive ? "WORKING" : "IDLE";
            }
        }

        public bool IsDefault
        {
            get
            {
                return this._Relation.IsDefault;
            }
            set
            {
                if (this._Relation.IsDefault != value)
                {
                    bool switched = false;
                    if (value)
                    {
                        VmInstrumentSourceRelation oldDefaultReation = this._Instrument.SourceRelations.SingleOrDefault(r => r.IsDefault == true);
                        if (oldDefaultReation != null)
                        {
                            ConsoleClient.Instance.SwitchDefaultSource(
                                new SwitchRelationBooleanPropertyMessage()
                                {
                                    InstrumentId = this._Instrument.Id,
                                    PropertyName = FieldSR.IsDefault,
                                    OldRelationId = oldDefaultReation.Id,
                                    NewRelationId = this.Id
                                });
                            switched = true;
                            //oldDefaultReation.IsDefault = false;  -- change by server notify.
                        }
                    }
                    if(!switched)
                    {
                        this.SubmitChange(FieldSR.IsDefault, value);
                    }
                }
            }
        }
        public int Priority
        {
            get
            {
                return this._Relation.Priority;
            }
            set
            {
                if (this._Relation.Priority != value)
                {
                    this.SubmitChange(FieldSR.Priority, value);
                }
            }
        }
        public int SwitchTimeout
        {
            get
            {
                return this._Relation.SwitchTimeout;
            }
            set
            {
                if (this._Relation.SwitchTimeout != value)
                {
                    this.SubmitChange(FieldSR.SwitchTimeout, value);
                }
            }
        }
        public int AdjustPoints
        {
            get
            {
                return this._Relation.AdjustPoints;
            }
            set
            {
                if (this.AdjustPoints != value)
                {
                    this.SubmitChange(FieldSR.AdjustPoints, value);
                }
            }
        }
        public int AdjustIncrement
        {
            get
            {
                return this._Relation.AdjustIncrement;
            }
            set
            {
                if (this._Relation.AdjustIncrement != value)
                {
                    this.SubmitChange(FieldSR.AdjustIncrement, value);
                }
            }
        }

        public bool SetSourceQuotation(VmSourceQuotation quotation)
        {
            bool addedWithoutReplace = true;
            if (this.SourceQuotations.Count > 0)
            {
                VmSourceQuotation first = this.SourceQuotations[0];
                if (quotation.Timestamp == first.Timestamp && first.Ask == null)
                {
                    // last is empty quotation, just update
                    first.Ask = quotation.Ask;
                    first.Bid = quotation.Bid;
                    addedWithoutReplace = false;
                }
                else
                {
                    int keepCount = 20;
                    if (this.SourceQuotations.Count > keepCount)
                    {
                        this.SourceQuotations.RemoveAt(keepCount);
                    }
                    this.SourceQuotations.Insert(0, quotation);
                }
            }
            else
            {
                this.SourceQuotations.Add(quotation);
            }

            if (!string.IsNullOrEmpty(quotation.Ask))
            {
                this.Ask = quotation.Ask;
                this.Bid = quotation.Bid;
                this.Last = quotation.Last;
                this.Timestamp = quotation.Timestamp;
            }
            return addedWithoutReplace;
        }

        private void SubmitChange(string field, object value)
        {
            base.SubmitChange(MetadataType.InstrumentSourceRelation, field, value);
        }
    }
}
