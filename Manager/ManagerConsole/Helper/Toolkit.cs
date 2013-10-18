using Manager.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ManagerConsole
{
    public static class Toolkit
    {
        public static SettingsManager SettingsManager;

        public static OrderTypeCategory GetCategory(this OrderType orderType)
        {
            if (orderType == OrderType.Market || orderType == OrderType.SpotTrade)
            {
                return OrderTypeCategory.DQ;
            }
            else
            {
                return OrderTypeCategory.Pending;
            }
        }
    }
}
