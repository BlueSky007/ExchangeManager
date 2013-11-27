using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonAccount = Manager.Common.Settings.Account;

namespace ManagerConsole.Model
{
    public class Account
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public Guid TradePolicyId
        {
            get;
            private set;
        }

        public Guid CustomerId { get; set; }

        public Guid GroupId { get; set; }

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
            this.CustomerId = account.CustomerId;
            this.GroupId = account.GroupId;
            this.IsBlack = account.IsBlack;
        }
    }
}
