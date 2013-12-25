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
                    base.SubmitChange(MetadataType.PriceRangeCheckRule, FieldSR.DiscardOutOfRangePrice, value);
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
                    base.SubmitChange(MetadataType.PriceRangeCheckRule, FieldSR.OutOfRangeType, value);
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
                    base.SubmitChange(MetadataType.PriceRangeCheckRule, FieldSR.ValidVariation, value);
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
                    base.SubmitChange(MetadataType.PriceRangeCheckRule, FieldSR.OutOfRangeWaitTime, value);
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
                    base.SubmitChange(MetadataType.PriceRangeCheckRule, FieldSR.OutOfRangeCount, value);
                }
            }
        }

    }
}
