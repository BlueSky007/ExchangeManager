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

            //int dateDiff = this.DateDiff(beginTime, currentDate);
            int dateDiff = (int)(beginTime - currentDate).TotalDays;

            return (dateDiff % recurDay == 0);
        }

        public static int[] GetWeekTaskRunInterval(string weekSN)
        {
            weekSN = weekSN.Remove(weekSN.LastIndexOf(","), 1);
            List<int> weekIntervals = new List<int>();
            string[] weeks = weekSN.Split(',');

            foreach(string week in weeks)
            {
                int currentWeek = GetCurrentDayOfWeek(DateTime.Now);
                int diffWeek = int.Parse(week) - currentWeek;
                int interval = diffWeek > 0 ? diffWeek : diffWeek + 7;

                weekIntervals.Add(interval);
            }
            return weekIntervals.ToArray();
        }

        private static int GetCurrentDayOfWeek(DateTime dateTime)
        {
            switch (dateTime.DayOfWeek)
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
