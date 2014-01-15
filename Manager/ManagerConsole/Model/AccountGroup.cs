using Manager.Common.ExchangeEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonAccountGroup = Manager.Common.Settings.AccountGroup;

namespace ManagerConsole.Model
{
    public class AccountGroup
    {
        public Guid Id { get; set; }
        public string Code { get; set; }

        public AccountGroup() { }

        public AccountGroup(CommonAccountGroup accountGroup)
        {
            this.Initialize(accountGroup);
        }

        public void Update(Dictionary<string, object> fieldAndValues)
        {
            foreach (string key in fieldAndValues.Keys)
            {
                this.Update(key, fieldAndValues[key]);
            }
        }

        public void Update(string field, object value)
        {
            if (field == ExchangeFieldSR.Code)
            {
                this.Code = (string)value;
            }
        }

        internal void Initialize(CommonAccountGroup accountGroup)
        {
            this.Id = accountGroup.Id;
            this.Code = accountGroup.Code;
        }
    }
}
