using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manager.Common
{
    public class Order
    {
        public Guid Id
        {
            get;
            set;
        }

        public Guid TransactionId
        {
            get;
            set;
        }

        public string ExchangeCode
        {
            get;
            set;
        }

        public string Code
        {
            get;
            set;
        }

        public decimal Lot
        {
            get;
            set;
        }

        public decimal? MinLot
        {
            get;
            set;
        }

        public bool IsOpen
        {
            get;
            set;
        }

        public bool IsBuy
        {
            get;
            set;
        }

        public string SetPrice
        {
            get;
            set;
        }

        public string ExecutePrice
        {
            get;
            set;
        }

        public string BestPrice
        {
            get;
            set;
        }

        public DateTime BestTime
        {
            get;
            set;
        }

        public DateTime? PriceTimestamp
        {
            get;
            set;
        }

        public TradeOption TradeOption
        {
            get;
            set;
        }

        public int DQMaxMove
        {
            get;
            set;
        }

        public int HitCount
        {
            get;
            set;
        }
    }
}
