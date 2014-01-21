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

        public int QuotationMask
        {
            get;
            set;
        }

        internal void Update(CommonTradePolicyDetail tradePolicyDetail)
        {
            this.InstrumentId = tradePolicyDetail.InstrumentId;
            this.TradePolicyId = tradePolicyDetail.TradePolicyId;
            this.ContractSize = tradePolicyDetail.ContractSize;
            this.QuotationMask = tradePolicyDetail.QuotationMask;
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
            else if (field == ExchangeFieldSR.QuotationMask)
            {
                this.QuotationMask = int.Parse(value);
            }
        }
    }

}
