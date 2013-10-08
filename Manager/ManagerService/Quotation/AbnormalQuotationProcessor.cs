using Manager.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ManagerService.Quotation
{
    public class AbnormalQuotationProcessor
    {
        private ConfigMetadata _QuotationConfig;

        public AbnormalQuotationProcessor(ConfigMetadata quotationConfig)
        {
            // TODO: Complete member initialization
            this._QuotationConfig = quotationConfig;
        }

        public bool IsWaitForPreOutOfRangeConfirmed(PrimitiveQuotation quotation)
        {
            throw new NotImplementedException();
        }

        internal void AddAndWait(PrimitiveQuotation quotation)
        {
            throw new NotImplementedException();
        }

        internal bool IsNormalPrice(PrimitiveQuotation quotation)
        {
            throw new NotImplementedException();
        }

        internal void StartProcessAbnormalQuotation(PrimitiveQuotation quotation)
        {
            throw new NotImplementedException();
        }
    }
}
