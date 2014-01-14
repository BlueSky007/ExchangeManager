using ManagerConsole.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonCustomer = Manager.Common.Settings.Customer;

namespace ManagerConsole.Model
{
    public class Customer : PropertyChangedNotifier
    {
        private string _Name;
        public Customer()
        {
        }

        public Customer(CommonCustomer customer)
        {
            this.Update(customer);
        }
        public Guid Id
        {
            get;
            set;
        }

        public string Code
        {
            get;
            set;
        }

        public string Name
        {
            get { return this._Name; }
            private set
            {
                string oldName = this._Name;
                this._Name = value;
                if (this._Name != oldName)
                {
                    this.OnPropertyChanged("Name");
                }
            }
        }

        public string Email
        {
            get;
            private set;
        }

        public Guid PublicQuotePolicyId
        {
            get;
            private set;
        }

        public Guid PrivateQuotePolicyId
        {
            get;
            private set;
        }


        internal void Update(CommonCustomer customer)
        {
            this.Id = customer.Id;
            this.Code = customer.Code;
            this.Name = customer.Name;
            this.Email = customer.Email;
            this.Name = customer.Name;
            if (customer.PrivateQuotePolicyId != null) this.PrivateQuotePolicyId = customer.PrivateQuotePolicyId.Value;
            if (customer.PublicQuotePolicyId != null) this.PublicQuotePolicyId = customer.PublicQuotePolicyId.Value;
        }
    }
}
