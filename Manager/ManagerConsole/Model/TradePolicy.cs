using Manager.Common.ExchangeEntities;
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
            this.Initialize(tradePolicy);
        }

        public Guid Id
        {
            get;
            private set;
        }

        public string Code { get; set; }

        public string Description { get; set; }

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

        internal void Initialize(CommonTradePolicy tradePolicy)
        {
            this.Id = tradePolicy.Id;
            this.AlertLevel1 = tradePolicy.AlertLevel1;
            this.AlertLevel2 = tradePolicy.AlertLevel2;
            this.AlertLevel3 = tradePolicy.AlertLevel3;
        }

        public void Update(Dictionary<string, string> fieldAndValues)
        {
            foreach (string key in fieldAndValues.Keys)
            {
                this.Update(key, fieldAndValues[key]);
            }
        }

        public void Update(string field, string value)
        {
            if (field == ExchangeFieldSR.Code)
            {
                this.Code = (string)value;
            }
            else if (field == ExchangeFieldSR.Description)
            {
                if (value != null)
                {
                    this.Description = value;
                }
            }
        }
    }
}
