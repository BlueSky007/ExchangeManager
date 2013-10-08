using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manager.Common
{
    public class ShareFixedData
    {

        public static DateTime GetServiceTime(IServiceTimeProvider iServiceTimeProvider)
        {
            return iServiceTimeProvider.GetServiceTime();
        }
        public static DateTime GetServiceTime()
        {
            return new DateTime(DateTime.Now.Ticks, DateTimeKind.Utc);
        }
    }
    public interface IServiceTimeProvider
    {
        DateTime GetServiceTime();
    }
}
