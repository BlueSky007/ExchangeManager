using ManagerConsole.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonDealingPolicyDetail = Manager.Common.Settings.DealingPolicyDetail;

namespace ManagerConsole.Model
{
    public class DealingPolicyDetail : PropertyChangedNotifier
    {
        public DealingPolicyDetail(CommonDealingPolicyDetail dealingPolicyDetail)
        {
            this.Update(dealingPolicyDetail);
        }

        public Guid DealingPolicyId
        {
            get;
            private set;
        }

        public Guid InstrumentId
        {
            get;
            private set;
        }

        private decimal _MaxDQLot;
        public decimal MaxDQLot
        {
            get { return this._MaxDQLot; }
            private set
            {
                if (this._MaxDQLot != value)
                {
                    this._MaxDQLot = value;
                    this.OnPropertyChanged("MaxDQLot");
                }
            }
        }

        private decimal _MaxOtherLot;
        public decimal MaxOtherLot
        {
            get { return this._MaxOtherLot; }
            private set
            {
                if (this._MaxOtherLot != value)
                {
                    this._MaxOtherLot = value;
                    this.OnPropertyChanged("MaxOtherLot");
                }
            }
        }

        public decimal DQQuoteMinLot
        {
            get;
            private set;
        }

        public int AcceptDQVariation
        {
            get;
            private set;
        }

        public int AcceptLmtVariation
        {
            get;
            private set;
        }

        public int AcceptCloseLmtVariation
        {
            get;
            private set;
        }

        public int CancelLmtVariation
        {
            get;
            private set;
        }

        public int AllowedNewTradeSides
        {
            get;
            private set;
        }

        public TimeSpan PriceValidTime
        {
            get;
            private set;
        }

        internal void Update(CommonDealingPolicyDetail dealingPolicyDetail)
        {
            this.DealingPolicyId = dealingPolicyDetail.DealingPolicyId;
            this.InstrumentId = dealingPolicyDetail.InstrumentId;
            if (dealingPolicyDetail.MaxDQLot != null) this.MaxDQLot = dealingPolicyDetail.MaxDQLot.Value;
            if (dealingPolicyDetail.MaxOtherLot != null) this.MaxOtherLot = dealingPolicyDetail.MaxOtherLot.Value;
            if (dealingPolicyDetail.DQQuoteMinLot != null) this.DQQuoteMinLot = dealingPolicyDetail.DQQuoteMinLot.Value;
            if (dealingPolicyDetail.AcceptDQVariation != null) this.AcceptDQVariation = dealingPolicyDetail.AcceptDQVariation.Value;
            if (dealingPolicyDetail.AcceptLmtVariation != null) this.AcceptLmtVariation = dealingPolicyDetail.AcceptLmtVariation.Value;
            if (dealingPolicyDetail.CancelLmtVariation != null) this.CancelLmtVariation = dealingPolicyDetail.CancelLmtVariation.Value;
            if (dealingPolicyDetail.AllowedNewTradeSides != null) this.AllowedNewTradeSides = dealingPolicyDetail.AllowedNewTradeSides.Value;
            if (dealingPolicyDetail.AcceptCloseLmtVariation != null) this.AcceptCloseLmtVariation = dealingPolicyDetail.AcceptCloseLmtVariation.Value;
            if (dealingPolicyDetail.PriceValidTime != null) this.PriceValidTime = TimeSpan.FromSeconds(dealingPolicyDetail.PriceValidTime.Value);
        }
    }
}
