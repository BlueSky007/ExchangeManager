using iExchange.Common;
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

namespace ManagerService.SettingScheduler
{
    public class SettingSchedulerManager
    {
        List<TaskScheduler> _TaskSchedulers;

        private Scheduler Scheduler = new Scheduler();
        private Scheduler.Action _UpdateSettingAction;

        public List<TaskScheduler> TaskSchedulers
        {
            get { return this._TaskSchedulers; }
            set{this._TaskSchedulers = value;}
        }

        public SettingSchedulerManager()
        {
            this._TaskSchedulers = new List<TaskScheduler>();
            this._UpdateSettingAction = new Scheduler.Action(this.UpdateSettingAction);
        }

        public void Start()
        {
            this.InitailizedTaskSchedulerData();
            foreach (TaskScheduler taskScheduler in this._TaskSchedulers)
            {
                if (taskScheduler.RunTime <= DateTime.Now) continue;
                this.Scheduler.Add(this._UpdateSettingAction, taskScheduler, taskScheduler.RunTime);
            }
        }

        private void InitailizedTaskSchedulerData()
        {
            this.TaskSchedulers = SettingManagerData.InitailizedTaskSchedulers();
        }

        public void AddTaskScheduler(TaskScheduler taskScheduler)
        {
            var task = this._TaskSchedulers.Where(P => P.Id == taskScheduler.Id);
            if (taskScheduler.RunTime <= DateTime.Now) return;
            if (task.Count() == 0)
            {
                this._TaskSchedulers.Add(taskScheduler);
            }

            taskScheduler.ScheduleID = this.Scheduler.Add(this._UpdateSettingAction, taskScheduler, taskScheduler.RunTime);
        }

        public void StartRunTaskScheduler(TaskScheduler taskScheduler)
        {
            IEnumerable<TaskScheduler> task = this._TaskSchedulers.Where(P => P.Id == taskScheduler.Id);
            TaskScheduler taskSchedulerEntity = task.ToList()[0];
            if (taskScheduler.RunTime <= DateTime.Now) return;

            if (taskSchedulerEntity != null)
            {
                this.Scheduler.Remove(taskSchedulerEntity.ScheduleID);
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
            }
        }

        #region Action 
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
            if (!string.IsNullOrEmpty(taskScheduler.ExchangeCode))
            {
                ExchangeSystem exchangeSystem = MainService.ExchangeManager.GetExchangeSystem(taskScheduler.ExchangeCode);
                XmlNode instrumentsXml = SettingManagerData.GetInstrumentParametersXml(taskScheduler);
                bool isOk = exchangeSystem.UpdateInstrument(instrumentsXml);
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
