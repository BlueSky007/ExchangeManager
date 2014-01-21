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

        public static TimeSpan[] GetWeekTaskRunInterval(string weekSN)
        {
            List<TimeSpan> weekIntervals = new List<TimeSpan>();
            string[] weeks = weekSN.Split(',');

            foreach(string week in weeks)
            {
                int currentWeek = GetCurrentDayOfWeek();
                TimeSpan interval = TimeSpan.FromDays((Math.Abs(currentWeek - int.Parse(week))));

                weekIntervals.Add(interval);
            }
            return weekIntervals.ToArray();
        }

        private static int GetCurrentDayOfWeek()
        {
            DateTime now = DateTime.Now;
            switch (now.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    return 1;
                case DayOfWeek.Tuesday:
                    return 2;
                case DayOfWeek.Wednesday:
                    return 3;
                case DayOfWeek.Thursday:
                    return 4;
                case DayOfWeek.Friday:
                    return 5;
                case DayOfWeek.Saturday:
                    return 6;
                case DayOfWeek.Sunday:
                    return 7;
            }
            return -1;
        }
    }
}
