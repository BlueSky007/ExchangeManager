using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manager.Common
{
    public class OrderRelation
    {
        public Guid OrderId1 { get; set; }

        public Guid OrderId2 { get; set; }

        public decimal Lot { get; set; }

        public OrderRelationType RelationType { get; set; }

        public decimal TradePL { get; set; }
        public decimal InterestPL { get; set; }
        public decimal StoragePL { get; set; }

        public override string ToString()
        {
            return string.Format("RelationType={0}, OrderId1={1}, OrderId2={2}, Lot={3}, TradePL={4}, InterestPL={5}, StoragePL={6}", RelationType, OrderId1, OrderId2, Lot, TradePL, InterestPL, StoragePL);
        }
    }
}
