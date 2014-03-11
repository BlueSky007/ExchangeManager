using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manager.Common
{
    public static class CommonHelper
    {
        private static string[] FixedPointFormats
            = new string[] { "F0", "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10" };

        public static string Format(decimal value, int decimals)
        {
            string format
                = decimals < CommonHelper.FixedPointFormats.Length ? CommonHelper.FixedPointFormats[decimals] : string.Format("F{0}", decimals);
            return value.ToString(format);
        }

        public static double GetAdjustValue(int adjustPoints, int decimalPlace)
        {
            return ((double)adjustPoints) / Math.Pow(10, decimalPlace);
        }
    }
}
