using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manager.Common.Settings
{
    public class QuotePolicyDetail
    {
        private string _AutoAdjustPoints2;
        private string _AutoAdjustPoints3;
        private string _AutoAdjustPoints4;
        private string _SpreadPoints2;
        private string _SpreadPoints3;
        private string _SpreadPoints4;

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
            get
            {
                if (string.IsNullOrEmpty(this._AutoAdjustPoints2))
                {
                    return "0";
                }
                else
                {
                    return this._AutoAdjustPoints2;
                }
            }
            set { this._AutoAdjustPoints2 = value; }
        }

        public string AutoAdjustPoints3
        {
            get
            {
                if (string.IsNullOrEmpty(this._AutoAdjustPoints3))
                {
                    return "0";
                }
                else
                {
                    return this._AutoAdjustPoints3;
                }
            }
            set { this._AutoAdjustPoints3 = value; }
        }

        public string AutoAdjustPoints4
        {
            get
            {
                if (string.IsNullOrEmpty(this._AutoAdjustPoints4))
                {
                    return "0";
                }
                else
                {
                    return this._AutoAdjustPoints4;
                }
            }
            set { this._AutoAdjustPoints4 = value; }
        }

        public System.Int32 SpreadPoints
        {
            get;
            set;
        }

        public string SpreadPoints2
        {
            get
            {
                if (string.IsNullOrEmpty(this._SpreadPoints2))
                {
                    return "0";
                }
                else
                {
                    return this._SpreadPoints2;
                }
            }
            set { this._SpreadPoints2 = value; }
        }

        public string SpreadPoints3
        {
            get
            {
                if (string.IsNullOrEmpty(this._SpreadPoints3))
                {
                    return "0";
                }
                else
                {
                    return this._SpreadPoints3;
                }
            }
            set { this._SpreadPoints3 = value; }
        }

        public string SpreadPoints4
        {
            get
            {
                if (string.IsNullOrEmpty(this._SpreadPoints4))
                {
                    return "0";
                }
                else
                {
                    return this._SpreadPoints4;
                }
            }
            set { this._SpreadPoints4 = value; }
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
