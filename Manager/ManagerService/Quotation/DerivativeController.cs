using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Manager.Common.QuotationEntities;

namespace ManagerService.Quotation
{
    public class DerivativeController
    {
        private Dictionary<int, DerivativeRelation> _DerivativeRelations;

        public DerivativeController(Dictionary<int, DerivativeRelation> derivativeRelations)
        {
            this._DerivativeRelations = derivativeRelations;
        }

        public List<Quotation> Derive(Quotation quotation)
        {
            List<Quotation> quotations = new List<Quotation>();
            quotations.Add(quotation);

            


            return quotations;
        }
    }
}
