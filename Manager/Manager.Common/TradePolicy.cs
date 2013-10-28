﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manager.Common
{
    public class TradePolicy
    {
        public Guid Id
        {
            get;
            set;
        }

        public decimal AlertLevel1
        {
            get;
            set;
        }

        public decimal AlertLevel2
        {
            get;
            set;
        }

        public decimal AlertLevel3
        {
            get;
            set;
        }
    }
}
