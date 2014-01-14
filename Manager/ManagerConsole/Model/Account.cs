using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonAccount = Manager.Common.Settings.Account;
using AccountType = iExchange.Common.AccountType;

namespace ManagerConsole.Model
{
    public class Account
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public AccountType AccountType { get; set; }
        public Guid TradePolicyId { get; set; }
        public Guid CustomerId { get; set; }
        public Guid GroupId { get; set; }
        public string GroupCode { get; set; }
        public bool IsBlack { get; set; }

        public Account() { }

        public Account(CommonAccount account)
        {
            this.Update(account);
        }

        internal void Update(CommonAccount account)
        {
            this.Id = account.Id;
            this.Code = account.Code;
            this.AccountType = account.AccountType;
            this.CustomerId = account.CustomerId;
            this.TradePolicyId = account.TradePolicyId;
            this.GroupId = account.GroupId;
            this.GroupCode = account.GroupCode;
            this.IsBlack = account.IsBlack;
        }
    }
}
