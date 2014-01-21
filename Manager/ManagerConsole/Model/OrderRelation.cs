using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OrderRelationType = iExchange.Common.OrderRelationType;
using CommonOrderRelation = iExchange.Common.Manager.OrderRelation;

namespace ManagerConsole.Model
{
    public class OrderRelation
    {
        public Guid OrderId { get; set; }

        public Guid OpenOrderId { get; set; }

        public decimal ClosedLot { get; set; }

        public OrderRelationType RelationType { get; set; }

        public string OpenOrderInfo{get;set;}

        public OrderRelation(CommonOrderRelation commonOrderRelation)
        {
            this.Intialize(commonOrderRelation);
        }

        public override bool Equals(object obj)
        {
            OrderRelation other = obj as OrderRelation;
            if (other == null) return false;
            return other.OpenOrderId == this.OpenOrderId
                && other.OrderId == this.OrderId;
        }

        public override int GetHashCode()
        {
            return this.RelationType.GetHashCode();
        }

    
        public void Update(OrderRelation orderRelation)
        {
            if (orderRelation != null)
            {
                this.OpenOrderId = orderRelation.OpenOrderId;
                this.OrderId = orderRelation.OrderId;
                this.ClosedLot = orderRelation.ClosedLot;
                this.RelationType = orderRelation.RelationType;
            }
        }

        public void Intialize(CommonOrderRelation commonOrderRelation)
        {
            this.OpenOrderId = commonOrderRelation.OpenOrderId;
            this.OrderId = commonOrderRelation.OrderId;
            this.ClosedLot = commonOrderRelation.ClosedLot;
            this.RelationType = commonOrderRelation.RelationType;
        }
    }
}
