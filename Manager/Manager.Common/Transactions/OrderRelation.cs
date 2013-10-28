using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonOrderRelation = Manager.Common.OrderRelation;

namespace Manager.Common.Transactions
{
    public abstract class OrderRelation
    {
        protected OrderRelation(OrderRelationType relationType, Guid orderId1, Guid orderId2)
            : this(relationType, orderId1, orderId2, 0)
        {
        }

        protected OrderRelation(OrderRelationType relationType, Guid orderId1, Guid orderId2, decimal lot)
        {
            this.RelationType = relationType;
            this.OrderId1 = orderId1;
            this.OrderId2 = orderId2;
            this.Lot = lot;
        }

        protected Guid OrderId1 { get; set; }
        protected Guid OrderId2 { get; set; }
        protected decimal Lot { get; set; }

        public OrderRelationType RelationType
        {
            get;
            protected set;
        }

        public abstract CommonOrderRelation ToCommonOrderRelation();
        public virtual void Update(OrderRelation orderRelation) { }

        public override bool Equals(object obj)
        {
            OrderRelation other = obj as OrderRelation;
            if (other == null) return false;
            return other.RelationType == this.RelationType && other.OrderId1 == this.OrderId1
                && other.OrderId2 == this.OrderId2 && other.Lot == this.Lot;
        }

        public override int GetHashCode()
        {
            return this.RelationType.GetHashCode() ^ this.OrderId1.GetHashCode()
                ^ this.OrderId2.GetHashCode() ^ this.Lot.GetHashCode();
        }
    }

    public class OpenCloseRelation : OrderRelation
    {
        public OpenCloseRelation(Guid openOrderId, Guid closeOrderId, decimal closeLot)
            : this(openOrderId, closeOrderId, closeLot, 0, 0, 0)
        {
        }

        public OpenCloseRelation(Guid openOrderId, Guid closeOrderId, decimal closeLot, decimal tradePL, decimal interestPL, decimal storagePL)
            : base(OrderRelationType.Close, openOrderId, closeOrderId, closeLot)
        {
            this.TradePL = tradePL;
            this.InterestPL = interestPL;
            this.StoragePL = storagePL;
        }

        public OpenCloseRelation(Guid openOrderId, Guid closeOrderId, decimal closeLot, string openOrderInfo)
            : this(openOrderId, closeOrderId, closeLot)
        {
            this.OpenOrderInfo = openOrderInfo;
        }

        public Guid OpenOrderId
        {
            get { return base.OrderId1; }
        }

        public Guid CloseOrderId
        {
            get { return base.OrderId2; }
        }

        public decimal TradePL
        {
            get;
            private set;
        }

        public decimal InterestPL
        {
            get;
            private set;
        }

        public decimal StoragePL
        {
            get;
            private set;
        }

        public decimal CloseLot
        {
            get { return base.Lot; }
        }

        public string OpenOrderInfo
        {
            get;
            set;
        }

        public void AdjustLotTo(decimal lot)
        {
            base.Lot = lot;
        }

        public static OpenCloseRelation FromOrderRelation(CommonOrderRelation orderRelation)
        {
            if (orderRelation.RelationType != OrderRelationType.Close) throw new InvalidOperationException("The relation is not a open-close relation");
            return new OpenCloseRelation(orderRelation.OrderId1, orderRelation.OrderId2, orderRelation.Lot, orderRelation.TradePL, orderRelation.InterestPL, orderRelation.StoragePL);
        }

        public override CommonOrderRelation ToCommonOrderRelation()
        {
            CommonOrderRelation orderRelation = new CommonOrderRelation();

            orderRelation.RelationType = OrderRelationType.Close;
            orderRelation.OrderId1 = this.OpenOrderId;
            orderRelation.OrderId2 = this.CloseOrderId;
            orderRelation.Lot = this.CloseLot;

            orderRelation.TradePL = this.TradePL;
            orderRelation.InterestPL = this.InterestPL;
            orderRelation.StoragePL = this.StoragePL;

            return orderRelation;
        }

        public override void Update(OrderRelation orderRelation)
        {
            base.Update(orderRelation);

            OpenCloseRelation openCloseRelation = orderRelation as OpenCloseRelation;
            if (openCloseRelation != null)
            {
                this.Lot = openCloseRelation.Lot;
                this.TradePL = openCloseRelation.TradePL;
                this.InterestPL = openCloseRelation.InterestPL;
                this.StoragePL = openCloseRelation.StoragePL;
                this.OpenOrderInfo = openCloseRelation.OpenOrderInfo;
            }
        }

        public override bool Equals(object obj)
        {
            OpenCloseRelation other = obj as OpenCloseRelation;
            if (other == null) return false;
            return other.OpenOrderId == this.OpenOrderId
                && other.CloseOrderId == this.CloseOrderId;
        }

        public override int GetHashCode()
        {
            return this.RelationType.GetHashCode() ^ this.OrderId1.GetHashCode()
                ^ this.OrderId2.GetHashCode();
        }
    }

    public class AssignmentRelation : OrderRelation
    {
        public AssignmentRelation(Guid assignedOrderId, Guid assigningOrderId, decimal assignLot)
            : base(OrderRelationType.Assignment, assignedOrderId, assigningOrderId, assignLot)
        {
        }

        public Guid AssignedOrderId
        {
            get { return base.OrderId1; }
        }

        public Guid AssigningOrderId
        {
            get { return base.OrderId2; }
        }

        public decimal AssignLot
        {
            get { return base.Lot; }
        }

        public static AssignmentRelation FromOrderRelation(CommonOrderRelation orderRelation)
        {
            if (orderRelation.RelationType != OrderRelationType.Assignment) throw new InvalidOperationException("The relation is not a assignment relation");
            return new AssignmentRelation(orderRelation.OrderId1, orderRelation.OrderId2, orderRelation.Lot);
        }

        public override CommonOrderRelation ToCommonOrderRelation()
        {
            CommonOrderRelation orderRelation = new CommonOrderRelation();

            orderRelation.RelationType = OrderRelationType.Assignment;
            orderRelation.OrderId1 = this.AssignedOrderId;
            orderRelation.OrderId2 = this.AssigningOrderId;
            orderRelation.Lot = this.AssignLot;

            return orderRelation;
        }
    }
}
