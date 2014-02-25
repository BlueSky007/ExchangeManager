using iExchange.Common;
using iExchange.Common.Manager;
using Manager.Common;
using Manager.Common.Settings;
using ManagerService.DataAccess;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml;
using ExchangeSystem = ManagerService.Exchange.ExchangeSystem;

namespace ManagerService.SettingsTaskManager
{
    public class SettingsTaskSchedulerManager
    {
        List<TaskScheduler> _TaskSchedulers;
        private TaskCheckManager _TaskCheckManager;

        private Scheduler Scheduler = new Scheduler();
        private Scheduler.Action _UpdateSettingAction;
        private Scheduler.Action _CheckTaskDateAction;

        public List<TaskScheduler> TaskSchedulers
        {
            get { return this._TaskSchedulers; }
            set{this._TaskSchedulers = value;}
        }

        public SettingsTaskSchedulerManager()
        {
            this._TaskCheckManager = new TaskCheckManager();
            this._TaskSchedulers = new List<TaskScheduler>();
            this._UpdateSettingAction = new Scheduler.Action(this.UpdateSettingAction);
            this._CheckTaskDateAction = new Scheduler.Action(this.CheckTaskDateAction);

            this.Scheduler.Add(this._CheckTaskDateAction, "CheckTask", DateTime.Now.AddSeconds(20));
        }

        public void Start()
        {
            this.InitailizedTaskSchedulerData();
            foreach (TaskScheduler taskScheduler in this._TaskSchedulers)
            {
                if (taskScheduler.RunTime <= DateTime.Now) continue;
                taskScheduler.ScheduleID = this.Scheduler.Add(this._UpdateSettingAction, taskScheduler, taskScheduler.RunTime);
            }
        }

        private void InitailizedTaskSchedulerData()
        {
            this.TaskSchedulers = SettingManagerData.InitailizedTaskSchedulers();
        }

        private TaskScheduler GetTaskScheduler(Guid taskSchedulerId)
        {
            TaskScheduler taskScheduler = this._TaskSchedulers.SingleOrDefault(P => P.Id == taskSchedulerId);
            return taskScheduler;
        }

        public void AddTaskScheduler(TaskScheduler taskScheduler)
        {
            try
            {
                TaskScheduler task = this.GetTaskScheduler(taskScheduler.Id);
                if (taskScheduler.RunTime <= DateTime.Now) return;

                switch (taskScheduler.ActionType)
                {
                    case ActionType.OneTime:
                        taskScheduler.ScheduleID = this.Scheduler.Add(this._UpdateSettingAction, taskScheduler, taskScheduler.RunTime);
                        break;
                    case ActionType.Daily:
                        taskScheduler.ScheduleID = this.Scheduler.Add(this._UpdateSettingAction, taskScheduler, taskScheduler.RunTime);
                        break;
                    case ActionType.Weekly:
                        DateTime startRunDate = taskScheduler.RunTime.Date;
                        TimeSpan startRunDiff = taskScheduler.RunTime - startRunDate;
                        string weekSN = taskScheduler.WeekDaySN;
                        int[] dayDiffs = TaskCheckManager.GetWeekTaskRunInterval(weekSN);

                        foreach (int dayDiff in dayDiffs)
                        {
                            DateTime runTaskTime = DateTime.Now + TimeSpan.FromDays(dayDiff) + startRunDiff;
                            string schedulerId = this.Scheduler.Add(this._UpdateSettingAction,taskScheduler, runTaskTime, DateTime.MaxValue, TimeSpan.FromDays(7));
                            taskScheduler.ScheduleIDs.Add(schedulerId);
                        }
                        break;
                    default:
                        taskScheduler.ScheduleID = this.Scheduler.Add(this._UpdateSettingAction, taskScheduler, taskScheduler.RunTime);
                        break;
                }

                if (task == null)
                {
                    this._TaskSchedulers.Add(taskScheduler);
                }
                else
                {
                    task.ScheduleID = taskScheduler.ScheduleID;
                }
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(TraceEventType.Error, "SettingsTaskSchedulerManager.AddTaskScheduler\r\n{0}", ex.ToString());
            }
        }

        public void ModifyTaskScheduler(TaskScheduler taskScheduler)
        {
            this.DeleteTaskScheduler(taskScheduler);

            this.AddTaskScheduler(taskScheduler);
        }

        public void StartRunTaskScheduler(TaskScheduler taskScheduler)
        {
            TaskScheduler task = this.GetTaskScheduler(taskScheduler.Id);
            if (taskScheduler.RunTime <= DateTime.Now) return;

            if (task != null && task.ScheduleID != null)
            {
                this.Scheduler.Remove(task.ScheduleID);
            }

            this.UpdateSettingParameter(taskScheduler);
        }

        public void DeleteTaskScheduler(TaskScheduler taskScheduler)
        {
            IEnumerable<TaskScheduler> task = this._TaskSchedulers.Where(P => P.Id == taskScheduler.Id);
            TaskScheduler taskSchedulerEntity = task.ToList()[0];
            if (taskScheduler.RunTime <= DateTime.Now) return;

            if (taskSchedulerEntity != null)
            {
                this.Scheduler.Remove(taskSchedulerEntity.ScheduleID);
                this._TaskSchedulers.Remove(taskSchedulerEntity);
            }
        }

        private void TaskSchedulerCompleted(TaskScheduler taskScheduler)
        {
            this._TaskSchedulers.Remove(taskScheduler);
            switch (taskScheduler.ActionType)
            {
                case ActionType.OneTime:
                    break;
                case ActionType.Daily:
                    taskScheduler.LastRunTime = taskScheduler.RunTime;
                    taskScheduler.RunTime = DateTime.Now + TimeSpan.FromDays(1);
                    this._TaskSchedulers.Add(taskScheduler);
                    break;
                case ActionType.Weekly:
                    taskScheduler.LastRunTime = DateTime.Now;
                    taskScheduler.RunTime = DateTime.Now + TimeSpan.FromDays(7);
                    this._TaskSchedulers.Add(taskScheduler);
                   
                    break;
                case ActionType.Monthly:
                    break;
                default:
                    break;
            }
            bool isSucceed = SettingManagerData.UpdateSettingsParameter(taskScheduler);
            TaskSchedulerRunMessage message = new TaskSchedulerRunMessage(taskScheduler, isSucceed);
            MainService.ClientManager.Dispatch(message);
        }

        #region Action 
        private void CheckTaskDateAction(object sender, object Args)
        {
            string str = Args as string;
            bool isAdd = false;

            foreach (TaskScheduler task in this._TaskSchedulers)
            {
                if (task.ActionType == ActionType.Daily)
                {
                    isAdd = this._TaskCheckManager.IsAddDailyTask(task.RunTime, task.RecurDay);
                }
            }
        }

        private void UpdateSettingAction(object sender,object Args)
        {
            TaskScheduler taskScheduler = Args as TaskScheduler;

            switch (taskScheduler.TaskType)
            {
                case TaskType.SetParameterTask:
                    this.UpdateSettingParameter(taskScheduler);
                    break;
                default:
                    break;
            }

            this.TaskSchedulerCompleted(taskScheduler);
        }

        private void UpdateSettingParameter(TaskScheduler taskScheduler)
        {
            if (taskScheduler.ExchangInstruments.Count > 0)
            {
                ExchangeSystem exchangeSystem = MainService.ExchangeManager.GetExchangeSystem(taskScheduler.ExchangeCode);

                ParameterUpdateTask updateSettings = SettingManagerData.GetExchangeParametersTask(taskScheduler);
                bool isOk = exchangeSystem.UpdateInstrument(updateSettings);
            }
        }
        #endregion
    }
}
