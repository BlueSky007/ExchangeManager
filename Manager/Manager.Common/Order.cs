using iExchange.Common;
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

        public Phase Phase
        {
            get;
            set;
        }

        public Guid TransactionId
        {
            get;
            set;
        }

        public string TransactionCode
        {
            get;
            set;
        }

        public TransactionType TransactionType
        {
            get;
            set;
        }

        public TransactionSubType TransactionSubType
        {
            get;
            set;
        }

        public TradeOption TradeOption
        {
            get;
            set;
        }

        public OrderType OrderType
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

        public Guid AccountId
        {
            get;
            set;
        }

        public Guid InstrumentId
        {
            get;
            set;
        }

        public decimal ContractSize
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


        public decimal Lot
        {
            get;
            set;
        }

        public decimal LotBalance
        {
            get;
            set;
        }

        public decimal? MinLot
        {
            get;
            set;
        }

        public string MaxShow
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

        public DateTime ExecuteTime
        {
            get;
            set;
        }

        public int HitCount
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

        public Guid ApproverID
        {
            get;
            set;
        }

        public Guid SubmitorID
        {
            get;
            set;
        }

        public int DQMaxMove
        {
            get;
            set;
        }

        public ExpireType ExpireType
        {
            get;
            set;
        }

        public string SetPrice2
        {
            get;
            set;
        }

        public Guid AssigningOrderID
        {
            get;
            set;
        }

        public string BlotterCode
        {
            get;
            set;
        }
 

       
    }
}
