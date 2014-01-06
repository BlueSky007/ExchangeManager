using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manager.Common.Settings
{
    public class DealingPolicyDetail
    {
        public Guid DealingPolicyId
        {
            get;
            set;
        }

        public Guid InstrumentId
        {
            get;
            set;
        }

        public decimal? MaxDQLot
        {
            get;
            set;
        }

        public decimal? MaxOtherLot
        {
            get;
            set;
        }

        public decimal? DQQuoteMinLot
        {
            get;
            set;
        }

        public int? AcceptDQVariation
        {
            get;
            set;
        }

        public int? AcceptLmtVariation
        {
            get;
            set;
        }

        public int? AcceptCloseLmtVariation
        {
            get;
            set;
        }

        public int? CancelLmtVariation
        {
            get;
            set;
        }

        public int? AllowedNewTradeSides
        {
            get;
            set;
        }

        public int? PriceValidTime
        {
            get;
            set;
        }
    }
}
