using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manager.Common
{
    public class SettingSet
    {
        public PrivateDailyQuotation PrivateDailyQuotation
        {
            get;
            set;
        }

        public SystemParameter SystemParameter
        {
            get;
            set;
        }

        public Instrument[] Instruments
        {
            get;
            set;
        }

        public Account[] Accounts
        {
            get;
            set;
        }

        public Customer Customer
        {
            get;
            set;
        }

        public Customer[] Customers
        {
            get;
            set;
        }

        public TradePolicy[] TradePolicies
        {
            get;
            set;
        }

        public TradePolicyDetail[] TradePolicyDetails
        {
            get;
            set;
        }

        public QuotePolicyDetail[] QuotePolicyDetails
        {
            get;
            set;
        }
    }
}
