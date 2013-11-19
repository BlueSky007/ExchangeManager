using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonQuotePolicyDetail = Manager.Common.Settings.QuotePolicyDetail;
using PriceType = Manager.Common.PriceType;

namespace ManagerConsole.Model
{
    public class QuotePolicyDetail
    {
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


        internal void Update(CommonQuotePolicyDetail quotePolicyDetail)
        {
            this.QuotePolicyId = quotePolicyDetail.QuotePolicyId;
            this.InstrumentId = quotePolicyDetail.InstrumentId;
            this.IsOriginHiLo = quotePolicyDetail.IsOriginHiLo;
        }
    }
}
