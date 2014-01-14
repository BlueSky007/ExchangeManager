using ManagerConsole.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonQuotePolicyDetail = Manager.Common.Settings.QuotePolicyDetail;
using PriceType = iExchange.Common.PriceType;

namespace ManagerConsole.Model
{
    public class QuotePolicyDetail : PropertyChangedNotifier
    {
        #region private Property
        private decimal _BuyLot;
        private decimal _SellLot;
        #endregion
        public QuotePolicyDetail(CommonQuotePolicyDetail quotePolicyDetail)
        {
            this.Update(quotePolicyDetail);
        }

        public Guid QuotePolicyId
        {
            get;
            private set;
        }

        public Guid InstrumentId
        {
            get;
            private set;
        }

        public bool IsOriginHiLo
        {
            get;
            private set;
        }

        public PriceType PriceType
        {
            get;
            set;
        }

        public int AutoAdjustPoints
        {
            get;
            set;
        }

        public int SpreadPoints
        {
            get;
            set;
        }

        public decimal BuyLot
        {
            get { return this._BuyLot; }
            set
            {
                this._BuyLot = value; this.OnPropertyChanged("BuyLot");
            }
        }
        public decimal SellLot
        {
            get { return this._SellLot; }
            set
            {
                this._SellLot = value; this.OnPropertyChanged("SellLot");
            }
        }


        internal void Update(CommonQuotePolicyDetail quotePolicyDetail)
        {
            this.QuotePolicyId = quotePolicyDetail.QuotePolicyId;
            this.InstrumentId = quotePolicyDetail.InstrumentId;
            this.IsOriginHiLo = quotePolicyDetail.IsOriginHiLo;
        }
    }
}
