﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ManagerConsole.Model
{
    public class Account
    {
        public Guid Id { get; private set; }
        public string Code { get; set; }
    }
}