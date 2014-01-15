using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonAccount = Manager.Common.Settings.Account;
using AccountType = iExchange.Common.AccountType;
using Manager.Common.ExchangeEntities;

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
            this.Initialize(account);
        }

        internal void Initialize(CommonAccount account)
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

        public void Update(Dictionary<string, string> fieldAndValues)
        {
            foreach (string key in fieldAndValues.Keys)
            {
                this.Update(key, fieldAndValues[key]);
            }
        }

        public void Update(string field, string value)
        {
            if (field == ExchangeFieldSR.Code)
            {
                this.Code = (string)value;
            }
            else if (field == ExchangeFieldSR.GroupID)
            {
                this.GroupId = Guid.Parse(value);
            }
            else if (field == ExchangeFieldSR.GroupCode)
            {
                this.GroupCode = (string)value;
            }
        }
    }
}
