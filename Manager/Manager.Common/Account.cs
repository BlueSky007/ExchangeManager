﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manager.Common
{
    public class Account
    {
        public Guid Id { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public AccountType? Type { get; set; }

        public Guid? GroupId { get; set; }

        public Guid? CustomerId { get; set; }

        public Guid? TradePolicyId { get; set; }
    }
}