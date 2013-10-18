using ManagerConsole.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonTradePolicyDetail = Manager.Common.TradePolicyDetail;

namespace ManagerConsole.Model
{
    public class TradePolicyDetail : PropertyChangedNotifier
    {
        public TradePolicyDetail(CommonTradePolicyDetail tradePolicyDetail)
        {
            this.Update(tradePolicyDetail);
        }

        public Guid TradePolicyId
        {
            get;
            private set;
        }

        public Guid InstrumentId
        {
            get;
            private set;
        }

        public decimal ContractSize
        {
            get;
            private set;
        }


        internal void Update(CommonTradePolicyDetail tradePolicyDetail)
        {
            this.TradePolicyId = tradePolicyDetail.TradePolicyId;
        }
    }
}
