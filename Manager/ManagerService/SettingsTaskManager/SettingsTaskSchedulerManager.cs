using iExchange.Common;
using iExchange.Common.Manager;
using Manager.Common;
using Manager.Common.Settings;
using ManagerService.DataAccess;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            TaskScheduler task = this.GetTaskScheduler(taskScheduler.Id);
            if (taskScheduler.RunTime <= DateTime.Now) return;

            switch (taskScheduler.ActionType)
            {
                case ActionType.OneTime:
                    taskScheduler.ScheduleID = this.Scheduler.Add(this._UpdateSettingAction, taskScheduler, taskScheduler.RunTime);
                    break;
                case ActionType.Daily:
                    taskScheduler.ScheduleID = this.Scheduler.Add(this._UpdateSettingAction, taskScheduler, taskScheduler.RunTime, DateTime.MaxValue, TimeSpan.FromSeconds(taskScheduler.Interval));
                    break;
                case ActionType.Weekly:
                    string weekSN = taskScheduler.WeekDaySN;
                    TimeSpan[] weekIntervals = TaskCheckManager.GetWeekTaskRunInterval(weekSN);

                    foreach (TimeSpan interval in weekIntervals)
                    {
                        string schedulerId = this.Scheduler.Add(this._UpdateSettingAction, taskScheduler, DateTime.Now + interval);
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
        }

        private void UpdateSettingParameter(TaskScheduler taskScheduler)
        {
            if (taskScheduler.ExchangInstruments.Count > 0)
            {
                ExchangeSystem exchangeSystem = MainService.ExchangeManager.GetExchangeSystem(taskScheduler.ExchangeCode);

                ParameterUpdateTask updateSettings = SettingManagerData.GetExchangeParametersTask(taskScheduler);
                bool isOk = exchangeSystem.UpdateInstrument(updateSettings);
            }
           
            bool isUpdate = SettingManagerData.UpdateSettingsParameter(taskScheduler);
            if (!isUpdate) return;
            UpdateSettingParameterMessage message = new UpdateSettingParameterMessage(taskScheduler);
            MainService.ClientManager.Dispatch(message);

            this._TaskSchedulers.Remove(taskScheduler);
        }
        #endregion
    }
}
