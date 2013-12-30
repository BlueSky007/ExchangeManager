using iExchange.Common;
using Manager.Common;
using Manager.Common.Settings;
using ManagerService.DataAccess;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace ManagerService.SettingScheduler
{
    public class SettingSchedulerManager
    {
        private SettingsForInstrument _SettingsForInstrument;
        private SettingsForSetValue _SettingsForSetValue;
        private SettingsForSystemParameter _SettingsForSystemParameter;

        ObservableCollection<TaskScheduler> _TaskSchedulers;

        private Scheduler Scheduler = new Scheduler();
        private Scheduler.Action _UpdateSettingAction;

        public ObservableCollection<TaskScheduler> TaskSchedulers
        {
            get { return this._TaskSchedulers; }
            set{this._TaskSchedulers = value;}
        }

        public SettingSchedulerManager()
        {
            this._SettingsForSystemParameter = new SettingsForSystemParameter();
            this._SettingsForInstrument = new SettingsForInstrument();
            this._SettingsForInstrument = new SettingsForInstrument();
            this.TaskSchedulers = new ObservableCollection<TaskScheduler>();
            this._UpdateSettingAction = new Scheduler.Action(this.UpdateSettingAction);
        }

        public void Start()
        {
            this.InitailizedTaskSchedulerData();
            foreach (TaskScheduler task in this._TaskSchedulers)
            {
                this.Scheduler.Add(this._UpdateSettingAction, task, task.RunTime);
            }
        }

        private void InitailizedTaskSchedulerData()
        {
            this.TaskSchedulers = SettingManagerData.InitailizedTaskSchedulers();
        }

        public void AddTaskScheduler(TaskScheduler taskScheduler)
        {
            TaskScheduler task = this._TaskSchedulers.SingleOrDefault(P => P.Id == taskScheduler.Id);
            if (task != null) return;
            this._TaskSchedulers.Add(task);

            this.Scheduler.Add(this._UpdateSettingAction, taskScheduler, taskScheduler.RunTime);
        }

        public void RemoveTaskScheduler(TaskScheduler taskScheduler)
        {
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

            UpdateSettingParameterMessage message = new UpdateSettingParameterMessage(taskScheduler.Id);
            MainService.ClientManager.Dispatch(message);
        }

        private void UpdateSettingParameter(TaskScheduler taskScheduler)
        {
            SettingManagerData.UpdateSettingsParameter(taskScheduler);
        }
        #endregion
    }
}
