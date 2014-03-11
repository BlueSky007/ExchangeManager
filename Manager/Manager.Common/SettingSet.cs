using iExchange.Common.Manager;
using Manager.Common.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manager.Common
{
    public class SettingSet
    {
        public TradeDay TradeDay
        {
            get;
            set;
        }

        public DataPermission[] DataPermissions
        {
            get;
            set;
        }

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

        public AccountGroup[] AccountGroups
        {
            get;
            set;
        }

        public Customer[] Customers
        {
            get;
            set;
        }

        public QuotePolicy[] QuotePolicies
        {
            get;
            set;
        }

        public QuotePolicyDetail[] QuotePolicyDetails
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


        public Order[] Orders
        {
            get;
            set;
        }

        public OverridedQuotation[] OverridedQuotations
        {
            get;
            set;
        }
    }
}
