using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using ManagerConsole.Helper;
using Manager.Common.QuotationEntities;
using ManagerConsole.Model;

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
            this.SourceRelations.CollectionChanged += SourceRelations_CollectionChanged;
        }

        private void SourceRelations_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            int step = 20;
            int firstZIndex = (this.SourceRelations.Count - 1) * step;
            
            for (int i = 0; i < this.SourceRelations.Count; i++)
            {
                this.SourceRelations[i].ZIndex = firstZIndex - i * step;
            }
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

        public double AdjustPoints
        {
            get
            {
                return this._Instrument.AdjustPoints;
            }
            set
            {
                double newValue = double.Parse(value.ToString('F' + this.DecimalPlace.ToString()));   // 避免浮点数问题
                if (this._Instrument.AdjustPoints != newValue)
                {
                    ConsoleClient.Instance.UpdateMetadataObjectField(MetadataType.Instrument, this.Id, FieldSR.AdjustPoints, newValue, delegate(bool success)
                    {
                        if (success)
                        {
                            this._Instrument.AdjustPoints = newValue;
                            this.OnPropertyChanged(FieldSR.AdjustPoints);
                        }
                    });
                }
            }
        }
        public double AdjustIncrement
        {
            get
            {
                return this._Instrument.AdjustIncrement;
            }
            set
            {
                if (this._Instrument.AdjustIncrement != value)
                {
                    ConsoleClient.Instance.UpdateMetadataObjectField(MetadataType.Instrument, this.Id, FieldSR.AdjustIncrement, value, delegate(bool success)
                    {
                        if (success)
                        {
                            this._Instrument.AdjustIncrement = value;
                            this.OnPropertyChanged(FieldSR.AdjustIncrement);
                        }
                    });
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
                    ConsoleClient.Instance.UpdateMetadataObjectField(MetadataType.Instrument, this.Id, FieldSR.DecimalPlace, value, delegate(bool success)
                    {
                        if (success)
                        {
                            this._Instrument.DecimalPlace = value;
                            this.OnPropertyChanged(FieldSR.DecimalPlace);
                        }
                    });
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
                    ConsoleClient.Instance.UpdateMetadataObjectField(MetadataType.Instrument, this.Id, FieldSR.InactiveTime, value, delegate(bool success)
                    {
                        if (success)
                        {
                            this._Instrument.InactiveTime = value;
                            this.OnPropertyChanged(FieldSR.InactiveTime);
                        }
                    });
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

        public bool HasDefaultSourceRelation
        {
            get
            {
                return this.SourceRelations.Any(r => r.IsDefault == true);
            }
        }
    }
}
