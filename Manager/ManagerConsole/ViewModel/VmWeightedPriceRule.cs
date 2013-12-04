using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Manager.Common.QuotationEntities;
using ManagerConsole.Helper;

namespace ManagerConsole.ViewModel
{
    public class VmWeightedPriceRule : VmBase
    {
        private WeightedPriceRule _WeightedPriceRule;

        public VmWeightedPriceRule(WeightedPriceRule weightedPriceRule)
            : base(weightedPriceRule)
        {
            this._WeightedPriceRule = weightedPriceRule;
        }

        public WeightedPriceRule WeightedPriceRule { get { return this._WeightedPriceRule; } }

        public int Id { get { return this._WeightedPriceRule.Id; } set { this._WeightedPriceRule.Id = value; } }
        public decimal Multiplier
        {
            get
            {
                return this._WeightedPriceRule.Multiplier;
            }
            set
            {
                if (this._WeightedPriceRule.Multiplier != value)
                {
                    this._WeightedPriceRule.Multiplier = value;
                    this.OnPropertyChanged(FieldSR.Multiplier);
                }
            }
        }
        public int AskAskWeight
        {
            get
            {
                return this._WeightedPriceRule.AskAskWeight;
            }
            set
            {
                if (this._WeightedPriceRule.AskAskWeight != value)
                {
                    this._WeightedPriceRule.AskAskWeight = value;
                    this.OnPropertyChanged(FieldSR.AskAskWeight);
                }
            }
        }
        public int AskBidWeight
        {
            get
            {
                return this._WeightedPriceRule.AskBidWeight;
            }
            set
            {
                if (this._WeightedPriceRule.AskBidWeight != value)
                {
                    this._WeightedPriceRule.AskBidWeight = value;
                    this.OnPropertyChanged(FieldSR.AskBidWeight);
                }
            }
        }
        public int AskLastWeight
        {
            get
            {
                return this._WeightedPriceRule.AskLastWeight;
            }
            set
            {
                if (this._WeightedPriceRule.AskLastWeight != value)
                {
                    this._WeightedPriceRule.AskLastWeight = value;
                    this.OnPropertyChanged(FieldSR.AskLastWeight);
                }
            }
        }
        public int BidAskWeight
        {
            get
            {
                return this._WeightedPriceRule.BidAskWeight;
            }
            set
            {
                if (this._WeightedPriceRule.BidAskWeight != value)
                {
                    this._WeightedPriceRule.BidAskWeight = value;
                    this.OnPropertyChanged(FieldSR.BidAskWeight);
                }
            }
        }
        public int BidBidWeight
        {
            get
            {
                return this._WeightedPriceRule.BidBidWeight;
            }
            set
            {
                if (this._WeightedPriceRule.BidBidWeight != value)
                {
                    this._WeightedPriceRule.BidBidWeight = value;
                    this.OnPropertyChanged(FieldSR.BidBidWeight);
                }
            }
        }
        public int BidLastWeight
        {
            get
            {
                return this._WeightedPriceRule.BidLastWeight;
            }
            set
            {
                if (this._WeightedPriceRule.BidLastWeight != value)
                {
                    this._WeightedPriceRule.BidLastWeight = value;
                    this.OnPropertyChanged(FieldSR.BidLastWeight);
                }
            }
        }
        public int LastAskWeight
        {
            get
            {
                return this._WeightedPriceRule.LastAskWeight;
            }
            set
            {
                if (this._WeightedPriceRule.LastAskWeight != value)
                {
                    this._WeightedPriceRule.LastAskWeight = value;
                    this.OnPropertyChanged(FieldSR.LastAskWeight);
                }
            }
        }
        public int LastBidWeight
        {
            get
            {
                return this._WeightedPriceRule.LastBidWeight;
            }
            set
            {
                if (this._WeightedPriceRule.LastBidWeight != value)
                {
                    this._WeightedPriceRule.LastBidWeight = value;
                    this.OnPropertyChanged(FieldSR.LastBidWeight);
                }
            }
        }
        public int LastLastWeight
        {
            get
            {
                return this._WeightedPriceRule.LastLastWeight;
            }
            set
            {
                if (this._WeightedPriceRule.LastLastWeight != value)
                {
                    this._WeightedPriceRule.LastLastWeight = value;
                    this.OnPropertyChanged(FieldSR.LastLastWeight);
                }
            }
        }
        public int AskAvarageWeight
        {
            get
            {
                return this._WeightedPriceRule.AskAverageWeight;
            }
            set
            {
                if (this._WeightedPriceRule.AskAverageWeight != value)
                {
                    this._WeightedPriceRule.AskAverageWeight = value;
                    this.OnPropertyChanged(FieldSR.AskAvarageWeight);
                }
            }
        }
        public int BidAvarageWeight
        {
            get
            {
                return this._WeightedPriceRule.BidAverageWeight;
            }
            set
            {
                if (this._WeightedPriceRule.BidAverageWeight != value)
                {
                    this._WeightedPriceRule.BidAverageWeight = value;
                    this.OnPropertyChanged(FieldSR.BidAvarageWeight);
                }
            }
        }
        public int LastAvarageWeight
        {
            get
            {
                return this._WeightedPriceRule.LastAverageWeight;
            }
            set
            {
                if (this._WeightedPriceRule.LastAverageWeight != value)
                {
                    this._WeightedPriceRule.LastAverageWeight = value;
                    this.OnPropertyChanged(FieldSR.LastAvarageWeight);
                }
            }
        }
        public decimal AskAdjust
        {
            get
            {
                return this._WeightedPriceRule.AskAdjust;
            }
            set
            {
                if (this._WeightedPriceRule.AskAdjust != value)
                {
                    this._WeightedPriceRule.AskAdjust = value;
                    this.OnPropertyChanged(FieldSR.AskAdjust);
                }
            }
        }
        public decimal BidAdjust
        {
            get
            {
                return this._WeightedPriceRule.BidAdjust;
            }
            set
            {
                if (this._WeightedPriceRule.BidAdjust != value)
                {
                    this._WeightedPriceRule.BidAdjust = value;
                    this.OnPropertyChanged(FieldSR.BidAdjust);
                }
            }
        }
        public decimal LastAdjust
        {
            get
            {
                return this._WeightedPriceRule.LastAdjust;
            }
            set
            {
                if (this._WeightedPriceRule.LastAdjust != value)
                {
                    this._WeightedPriceRule.LastAdjust = value;
                    this.OnPropertyChanged(FieldSR.LastAdjust);
                }
            }
        }
    }
}
