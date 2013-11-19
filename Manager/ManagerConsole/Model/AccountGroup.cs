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
            this.Update(accountGroup);
        }

        internal void Update(CommonAccountGroup accountGroup)
        {
            this.Id = accountGroup.Id;
            this.Code = accountGroup.Code;
        }
    }
}
