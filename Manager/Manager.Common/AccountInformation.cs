using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manager.Common
{
    public class AccountInformation
    {
        public Guid AccountId { get; set; }
        public Guid InstrumentId { get; set; }
        public decimal Balance { get; set; }
        public decimal Equity { get; set; }
        public decimal Necessary { get; set; }
        public decimal Usable { get; set; }
        public decimal BuyLotBalanceSum { get; set; }
        public decimal SellLotBalanceSum { get; set; }
    }
}
