using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Manager.Common.QuotationEntities;
using ManagerConsole.Helper;

namespace ManagerConsole.ViewModel
{
    public class VmPriceRangeCheckRule : VmBase
    {
        private PriceRangeCheckRule _PriceRangeCheckRule;
        public VmPriceRangeCheckRule(PriceRangeCheckRule priceRangeCheckRule)
            : base(priceRangeCheckRule)
        {
            this._PriceRangeCheckRule = priceRangeCheckRule;
        }

        public PriceRangeCheckRule PriceRangeCheckRule { get { return this._PriceRangeCheckRule; } }

        public int Id { get { return this._PriceRangeCheckRule.Id; } set { this._PriceRangeCheckRule.Id = value; } }
        public bool DiscardOutOfRangePrice
        {
            get
            {
                return this._PriceRangeCheckRule.DiscardOutOfRangePrice;
            }
            set
            {
                if (this._PriceRangeCheckRule.DiscardOutOfRangePrice != value)
                {
                    this._PriceRangeCheckRule.DiscardOutOfRangePrice = value;
                    this.OnPropertyChanged(FieldSR.DiscardOutOfRangePrice);
                }
            }
        }
        public OutOfRangeType OutOfRangeType
        {
            get
            {
                return this._PriceRangeCheckRule.OutOfRangeType;
            }
            set
            {
                if (this._PriceRangeCheckRule.OutOfRangeType != value)
                {
                    this._PriceRangeCheckRule.OutOfRangeType = value;
                    this.OnPropertyChanged(FieldSR.OutOfRangeType);
                }
            }
        }

        public int ValidVariation
        {
            get
            {
                return this._PriceRangeCheckRule.ValidVariation;
            }
            set
            {
                if (this._PriceRangeCheckRule.ValidVariation != value)
                {
                    this._PriceRangeCheckRule.ValidVariation = value;
                    this.OnPropertyChanged(FieldSR.ValidVariation);
                }
            }
        }

        public int OutOfRangeWaitTime
        {
            get
            {
                return this._PriceRangeCheckRule.OutOfRangeWaitTime;
            }
            set
            {
                if (this._PriceRangeCheckRule.OutOfRangeWaitTime != value)
                {
                    this._PriceRangeCheckRule.OutOfRangeWaitTime = value;
                    this.OnPropertyChanged(FieldSR.OutOfRangeWaitTime);
                }
            }
        }

        public int OutOfRangeCount
        {
            get
            {
                return this._PriceRangeCheckRule.OutOfRangeCount;
            }
            set
            {
                if (this._PriceRangeCheckRule.OutOfRangeCount != value)
                {
                    this._PriceRangeCheckRule.OutOfRangeCount = value;
                    this.OnPropertyChanged(FieldSR.OutOfRangeCount);
                }
            }
        }

    }
}
