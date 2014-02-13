using iExchange.Common.Manager;
using Manager.Common;
using ManagerConsole.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonTransaction = iExchange.Common.Manager.Transaction;
using CommonOrder = iExchange.Common.Manager.Order;
using OrderType = iExchange.Common.OrderType;
using TransactionSubType = iExchange.Common.TransactionSubType;
using TransactionType = iExchange.Common.TransactionType;
using OrderPhase = iExchange.Common.OrderPhase;
using InstrumentCategory = iExchange.Common.InstrumentCategory;
using ExpireType = iExchange.Common.ExpireType;
using Price = iExchange.Common.Price;

namespace ManagerConsole.Model
{
    public class Transaction : PropertyChangedNotifier
    {
        private List<Order> _Orders = new List<Order>();
        private List<OrderRelation> _OrderRelations = new List<OrderRelation>();

        public Transaction(CommonTransaction tran,Account account,InstrumentClient instrument)
        {
            this.Account = account;
            this.Instrument = instrument;
            this.Update(tran);
        }

        public Transaction(CommonOrder commonOrder, Account account, InstrumentClient instrument)
        {
            this.Account = account;
            this.Instrument = instrument;
            this.Initialize(commonOrder);
        }

        #region Public Property
        public Guid Id
        {
            get;
            private set;
        }

        public string Code
        {
            get;
            set;
        }

        public TransactionType Type
        {
            get;
            set;
        }

        public TransactionSubType SubType
        {
            get;
            set;
        }

        private OrderPhase _Phase;
        public OrderPhase Phase
        {
            get { return this._Phase; }
            set
            {
                if (this._Phase != value)
                {
                    if (this._Phase == OrderPhase.Executed && (value == OrderPhase.Canceled || value == OrderPhase.Placed || value == OrderPhase.Placing)) return;

                    this._Phase = value;
                    this.OnPropertyChanged("Phase");

                    foreach (Order order in this.Orders)
                    {
                        order.Phase = value;
                    }
                }
            }
        }

        private string _CancelReason;
        public string CancelReason
        {
            get { return this._CancelReason; }
            set
            {
                if (this._CancelReason != value)
                {
                    this._CancelReason = value;
                    this.OnPropertyChanged("CancelReason");
                }
            }
        }

        public OrderType OrderType
        {
            get;
            set;
        }

        public Account Account
        {
            get;
            set;
        }

        public InstrumentClient Instrument
        {
            get;
            set;
        }

        public InstrumentCategory InstrumentCategory
        {
            get;
            set;
        }

        public DateTime BeginTime
        {
            get;
            set;
        }

        public DateTime EndTime
        {
            get;
            set;
        }

        public DateTime SubmitTime
        {
            get;
            set;
        }

        public DateTime? ExecuteTime
        {
            get;
            set;
        }

        public decimal ContractSize
        {
            get;
            set;
        }

        public Guid SubmitorId
        {
            get;
            set;
        }

        public Guid? AssigningOrderId
        {
            get;
            set;
        }

        public IList<Order> Orders
        {
            get { return this._Orders; }
        }

        public ExpireType ExpireType { get; set; }

        #endregion

        #region Order Update
        public void Add(Order order)
        {
            bool existed = false;
            foreach (Order item in this._Orders)
            {
                if (item.Id == order.Id)
                {
                    existed = true;
                    break;
                }
            }
            if (!existed) this._Orders.Add(order);
        }

        public void AddOrderRelation(OrderRelation orderRelation)
        {
            bool updated = false;
            foreach (OrderRelation item in this._OrderRelations)
            {
                if (item.Equals(orderRelation))
                {
                    item.Update(orderRelation);
                    updated = true;
                }
            }
            if (!updated)
            {
                this._OrderRelations.Add(orderRelation);
            }
        }

        public bool Remove(Order order)
        {
            return this._Orders.Remove(order);
        }
        #endregion

        internal void Update(CommonTransaction transaction)
        {
            this.Id = transaction.Id;
            this.BeginTime = transaction.BeginTime;
            this.Code = transaction.Code;
            this.ContractSize = transaction.ContractSize;
            this.EndTime = transaction.EndTime;
            this.ExecuteTime = transaction.ExecuteTime;
            this.ExpireType = transaction.ExpireType;
            this.OrderType = transaction.OrderType;
            this.Phase = transaction.Phase;
            this.SubmitorId = transaction.SubmitorId;
            this.SubmitTime = transaction.SubmitTime;
            this.Type = transaction.Type;
            this.SubType = transaction.SubType;
            this.AssigningOrderId = transaction.AssigningOrderId;
            this.InstrumentCategory = transaction.InstrumentCategory == null ? InstrumentCategory.Forex : transaction.InstrumentCategory.Value;
        }

        internal void Initialize(CommonOrder commonOrder)
         {
             this.Id = commonOrder.TransactionId;
             this.BeginTime = commonOrder.BeginTime;
             this.Code = commonOrder.Code;
             this.ContractSize = commonOrder.ContractSize;
             this.EndTime = commonOrder.EndTime;
             this.ExecuteTime = commonOrder.ExecuteTime;
             this.ExpireType = commonOrder.ExpireType;
             this.OrderType = commonOrder.OrderType;
             this.Phase = commonOrder.Phase;
             this.SubmitorId = commonOrder.SubmitorID;
             this.SubmitTime = commonOrder.SubmitTime;
             this.Type = commonOrder.TransactionType;
             this.SubType = commonOrder.TransactionSubType;
             this.AssigningOrderId = commonOrder.AssigningOrderID;
             this.InstrumentCategory = this.Instrument.Category;
         }

        internal string GetOpenOrderAvgPrice(Guid openOrderId)
        {
            var orderRelations = this._OrderRelations.Where(P => P.OrderId == openOrderId);

            decimal sumCloseLot = decimal.Zero;
            decimal sumAmount = decimal.Zero;
            foreach (OrderRelation relation in orderRelations)
            {
                sumCloseLot += relation.ClosedLot;
                sumAmount += relation.ClosedLot * decimal.Parse(relation.OpenOrderPrice);
            }

            if (sumCloseLot == 0)
            {
                return "-";
            }
            decimal avgPriceValue = sumAmount / sumCloseLot;
            return avgPriceValue.ToString();
        }
    }
}
