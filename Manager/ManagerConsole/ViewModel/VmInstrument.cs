using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using ManagerConsole.Helper;
using Manager.Common.QuotationEntities;

namespace ManagerConsole.ViewModel
{
    public class VmInstrument : VmQuotationBase
    {
        private Instrument _Instrument;
        public VmInstrument(Instrument instrument)
            : base(instrument)
        {
            this._Instrument = instrument;
            this.SourceRelations = new ObservableCollection<VmInstrumentSourceRelation>();
        }

        public ObservableCollection<VmInstrumentSourceRelation> SourceRelations { get; set; }
        public VmPriceRangeCheckRule VmPriceRangeCheckRule { get; set; }
        public VmWeightedPriceRule VmWeightedPriceRule { get; set; }

        public VmDerivativeRelation DerivativeRelation { get; set; }

        public Instrument Instrument { get { return this._Instrument; } }
        public int Id { get { return this._Instrument.Id; } set { this._Instrument.Id = value; } }

        public string Code
        {
            get
            {
                return this._Instrument.Code;
            }
            set
            {
                if (this._Instrument.Code != value)
                {
                    this._Instrument.Code = value;
                    this.OnPropertyChanged(FieldSR.Code);
                }
            }
        }

        public bool IsDerivative
        {
            get
            {
                return this._Instrument.IsDerivative;
            }
            set
            {
                if (this._Instrument.IsDerivative != value)
                {
                    this._Instrument.IsDerivative = value;
                    this.OnPropertyChanged(FieldSR.IsDerivative);
                }
            }
        }

        public int DecimalPlace
        {
            get
            {
                return this._Instrument.DecimalPlace;
            }
            set
            {
                if (this._Instrument.DecimalPlace != value)
                {
                    this._Instrument.DecimalPlace = value;
                    this.OnPropertyChanged(FieldSR.DecimalPlace);
                }
            }
        }

        public int? InactiveTime
        {
            get
            {
                return this._Instrument.InactiveTime;
            }
            set
            {
                if (this._Instrument.InactiveTime != value)
                {
                    this._Instrument.InactiveTime = value;
                    this.OnPropertyChanged(FieldSR.InactiveTime);
                }
            }
        }

        public bool? UseWeightedPrice
        {
            get
            {
                return this._Instrument.UseWeightedPrice;
            }
            set
            {
                if (this._Instrument.UseWeightedPrice != value)
                {
                    this._Instrument.UseWeightedPrice = value;
                    this.OnPropertyChanged(FieldSR.UseWeightedPrice);
                }
            }
        }

        public bool? IsSwitchUseAgio
        {
            get
            {
                return this._Instrument.IsSwitchUseAgio;
            }
            set
            {
                if (this._Instrument.IsSwitchUseAgio != value)
                {
                    this._Instrument.IsSwitchUseAgio = value;
                    this.OnPropertyChanged(FieldSR.IsSwitchUseAgio);
                }
            }
        }

        public int? AgioSeconds
        {
            get
            {
                return this._Instrument.AgioSeconds;
            }
            set
            {
                if (this._Instrument.AgioSeconds != value)
                {
                    this._Instrument.AgioSeconds = value;
                    this.OnPropertyChanged(FieldSR.AgioSeconds);
                }
            }
        }

        public int? LeastTicks
        {
            get
            {
                return this._Instrument.LeastTicks;
            }
            set
            {
                if (this._Instrument.LeastTicks != value)
                {
                    this._Instrument.LeastTicks = value;
                    this.OnPropertyChanged(FieldSR.LeastTicks);
                }
            }
        }
    }
}
