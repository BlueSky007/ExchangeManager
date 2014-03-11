using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manager.Common.ExchangeEntities
{
    public class AccountCurrency
    {
        public Currency Currency { get; set; }
        public TradingSummary CurrencyTradingSummary { get; set; }
    }
}
