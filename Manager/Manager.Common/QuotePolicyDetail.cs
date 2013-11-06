using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manager.Common
{
    public class QuotePolicyDetail
    {
        public Guid QuotePolicyId
        {
            get;
            set;
        }

        public Guid InstrumentId
        {
            get;
            set;
        }

        public PriceType PriceType
        {
            get;
            set;
        }

        public System.Int32 AutoAdjustPoints
        {
            get;
            set;
        }

        public string AutoAdjustPoints2
        {
            get;
            set;
        }

        public string AutoAdjustPoints3
        {
            get;
            set;
        }

        public string AutoAdjustPoints4
        {
            get;
            set;
        }

        public System.Int32 SpreadPoints
        {
            get;
            set;
        }

        public string SpreadPoints2
        {
            get;
            set;
        }

        public string SpreadPoints3
        {
            get;
            set;
        }

        public string SpreadPoints4
        {
            get;
            set;
        }

        public System.Int32 MaxAutoAdjustPoints
        {
            get;
            set;
        }

        public System.Int32 MaxSpreadPoints
        {
            get;
            set;
        }

        public bool IsOriginHiLo
        {
            get;
            set;
        }

        public decimal BuyLot
        {
            get;
            set;
        }

        public decimal SellLot
        {
            get;
            set;
        }
    }
}
