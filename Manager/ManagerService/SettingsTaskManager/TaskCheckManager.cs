using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ManagerService.SettingsTaskManager
{
    public class TaskCheckManager
    {
        private int DateDiff(DateTime DateTime1, DateTime DateTime2)
        {
            int dateDiff = 0;
            TimeSpan ts1 = new TimeSpan(DateTime1.Ticks);
            TimeSpan ts2 = new TimeSpan(DateTime2.Ticks);
            TimeSpan ts = ts1.Subtract(ts2).Duration();
            dateDiff = int.Parse(ts.Days.ToString());
            return dateDiff;
        }

        public bool IsAddDailyTask(DateTime beginTime, int recurDay)
        {
            if (recurDay == 0) return true;
            DateTime currentDate = DateTime.Now;

            int dateDiff = this.DateDiff(beginTime, currentDate);

            return (dateDiff % recurDay == 0);
        }
    }
}
