using Manager.Common.ExchangeEntities;
using ManagerConsole.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonTradePolicyDetail = Manager.Common.Settings.TradePolicyDetail;

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

        public void Update(Dictionary<string, string> fieldAndValues)
        {
            foreach (string key in fieldAndValues.Keys)
            {
                this.Update(key, fieldAndValues[key]);
            }
        }

        public void Update(string field, string value)
        {
            if (field == ExchangeFieldSR.InstrumentId)
            {
                this.InstrumentId = Guid.Parse(value);
            }
        }
    }

}
