using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manager.Common
{
    public class Customer
    {
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
            get;
            set;
        }

        public string Email
        {
            get;
            set;
        }

        public Guid? PublicQuotePolicyId
        {
            get;
            set;
        }

        public Guid? PrivateQuotePolicyId
        {
            get;
            set;
        }

        public Guid? DealingPolicyId
        {
            get;
            set;
        }
    }
}
