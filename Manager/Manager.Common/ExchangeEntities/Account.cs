using iExchange.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manager.Common.Settings
{
    public class AccountGroup
    {
        public Guid Id { get; set; }

        public string Code { get; set; }
    }

    public class Account
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public bool IsBlack { get; set; }
        public AccountType AccountType { get; set; }
        public Guid GroupId { get; set; }
        public Guid CustomerId { get; set; }
        public string GroupCode { get; set; }
        public Guid TradePolicyId { get; set; }
    }
}
