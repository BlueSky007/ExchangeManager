using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Manager.Common.QuotationEntities;
using ManagerConsole.Helper;

namespace ManagerConsole.ViewModel
{
    public class VmInstrumentSourceRelation : VmQuotationBase
    {
        private InstrumentSourceRelation _Relation;

        public VmInstrumentSourceRelation(InstrumentSourceRelation relation)
        {
            this._Relation = relation;
            this.PrimitiveQuotations = new ObservableCollection<PrimitiveQuotation>();
        }

        public ObservableCollection<PrimitiveQuotation> PrimitiveQuotations { get; set; }

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
                }
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
                    this._Relation.IsDefault = value;
                    this.OnPropertyChanged(FieldSR.IsDefault);
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
                    this._Relation.Priority = value;
                    this.OnPropertyChanged(FieldSR.Priority);
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
                    this._Relation.SwitchTimeout = value;
                    this.OnPropertyChanged(FieldSR.SwitchTimeout);
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
                if (this._Relation.AdjustPoints != value)
                {
                    this._Relation.AdjustPoints = value;
                    this.OnPropertyChanged(FieldSR.AdjustPoints);
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
                    this._Relation.AdjustIncrement = value;
                    this.OnPropertyChanged(FieldSR.AdjustIncrement);
                }
            }
        }

        public void SetPrimitiveQuotation(PrimitiveQuotation quotation)
        {
            int keepCount = 20;
            if (this.PrimitiveQuotations.Count > keepCount)
            {
                this.PrimitiveQuotations.RemoveAt(keepCount - 1);
            }
            this.PrimitiveQuotations.Insert(0, quotation);
        }
    }
}
