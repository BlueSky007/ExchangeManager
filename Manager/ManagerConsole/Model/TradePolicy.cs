using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonTradePolicy = Manager.Common.Settings.TradePolicy;

namespace ManagerConsole.Model
{
    public class TradePolicy
    {
        public TradePolicy(CommonTradePolicy tradePolicy)
        {
            this.Update(tradePolicy);
        }

        public Guid Id
        {
            get;
            private set;
        }

        public decimal AlertLevel1
        {
            get;
            private set;
        }

        public decimal AlertLevel2
        {
            get;
            private set;
        }

        public decimal AlertLevel3
        {
            get;
            private set;
        }

        internal void Update(CommonTradePolicy tradePolicy)
        {
            this.Id = tradePolicy.Id;
            this.AlertLevel1 = tradePolicy.AlertLevel1;
            this.AlertLevel2 = tradePolicy.AlertLevel2;
            this.AlertLevel3 = tradePolicy.AlertLevel3;
        }
    }
}
