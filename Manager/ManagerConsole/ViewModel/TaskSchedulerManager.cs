using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using ExchangInstrument = Manager.Common.Settings.ExchangInstrument;
using CommonTaskScheduler = Manager.Common.Settings.TaskScheduler;
using ParameterDefine = Manager.Common.Settings.ParameterDefine;
using ParameterSettingTask = Manager.Common.Settings.ParameterSettingTask;
using TaskStatus = Manager.Common.TaskStatus;
using SettingParameterType = Manager.Common.SettingParameterType;
using TaskType = Manager.Common.TaskType;
using ActionType = Manager.Common.ActionType;
using System.Data;
using System.Windows;
using ManagerConsole.Model;
using UpdateSettingParameterMessage = Manager.Common.UpdateSettingParameterMessage;
using ManagerConsole.Helper;

namespace ManagerConsole.ViewModel
{
    public class TaskSchedulerModel
    {
        public static TaskSchedulerModel Instance = new TaskSchedulerModel();

        public bool _IsGetTaskData = false;
        private ObservableCollection<TaskScheduler> _TaskSchedulers;

        public TaskSchedulerModel()
        {
            this._TaskSchedulers = new ObservableCollection<TaskScheduler>();
        }

        public ObservableCollection<TaskScheduler> TaskSchedulers
        {
            get
            {
                if (!this._IsGetTaskData)
                {
                    this.LoadTaskSchedulers();
                }
                return this._TaskSchedulers;
            }
        }

        public void LoadTaskSchedulers()
        {
            if (!this._IsGetTaskData)
            {
                ConsoleClient.Instance.GetTaskSchedulersData(delegate(List<CommonTaskScheduler> taskSchedulers)
                {
                    foreach (CommonTaskScheduler commonTaskScheduler in taskSchedulers)
                    {
                        TaskScheduler taskScheduler = new TaskScheduler(commonTaskScheduler);
                        this._TaskSchedulers.Add(taskScheduler);
                    }
                });
            }
        }

        public void TaskSchedulerStatusChangeNotify(UpdateSettingParameterMessage message)
        {
            Guid taskSchedulerId = message.TaskScheduler.Id;
            TaskScheduler currentTask = this._TaskSchedulers.SingleOrDefault(P => P.Id == taskSchedulerId);
            if (currentTask == null) return;
            currentTask.TaskStatus = TaskStatus.Run;
            currentTask.LastRunTime = currentTask.RunTime;
        }


        public void AddTaskScheduler(TaskScheduler taskScheduler)
        {
            this._TaskSchedulers.Add(taskScheduler);
        }

        public void RemoveTaskScheduler(TaskScheduler taskScheduler)
        {
            if (!this._TaskSchedulers.Contains(taskScheduler)) return;
            this._TaskSchedulers.Remove(taskScheduler);
        }

        public void ChangeTaskStatus(TaskScheduler taskScheduler,bool isDisable)
        {
            taskScheduler.TaskStatus = isDisable? TaskStatus.Disable:TaskStatus.Ready;
        }
    }

    public class TaskScheduler : PropertyChangedNotifier
    {
        #region private property
        private Guid _Id;
        private string _ExchangeCode;
        private string _Name;
        private string _Description;
        private TaskType _TaskType;
        private TaskStatus _TaskStatus;
        private ActionType _ActionType;
        private Guid _Creater;
        private DateTime _RunTime;
        private DateTime _LastRunTime;
        private DateTime _CreateDateTime;

        private ObservableCollection<ParameterSetting> _ParameterSettings;
        private ObservableCollection<ExchangInstrument> _ExchangInstruments { get; set; }
        #endregion

        public TaskScheduler() 
        {
            this._RunTime = DateTime.Now;
            this._LastRunTime = DateTime.MaxValue;
            this._Id = Guid.NewGuid();
            this._ParameterSettings = new ObservableCollection<ParameterSetting>();
        }

        public TaskScheduler(CommonTaskScheduler commonTaskScheduler)
        {
            this._Id = commonTaskScheduler.Id;
            this._ParameterSettings = new ObservableCollection<ParameterSetting>();
            this.Update(commonTaskScheduler);
        }

        #region public property
        public ObservableCollection<ParameterSetting> ParameterSettings
        {
            get { return this._ParameterSettings; }
            set { this._ParameterSettings = value; }
        }

        public ObservableCollection<ExchangInstrument> ExchangInstruments
        {
            get { return this._ExchangInstruments; }
            set { this._ExchangInstruments = value; }
        }

        public Guid Id
        {
            get { return this._Id; }
        }

        public string ExchangeCode
        {
            get { return this._ExchangeCode; }
            set { this._ExchangeCode = value; this.OnPropertyChanged("ExchangeCode"); }
        }

        public string Name
        {
            get { return this._Name; }
            set { this._Name = value; }
        }

        public TaskStatus TaskStatus
        {
            get { return this._TaskStatus; }
            set { this._TaskStatus = value; this.OnPropertyChanged("TaskStatus"); }

        }

        public string Description
        {
            get { return this._Description; }
            set { this._Description = value; }
        }

        public TaskType TaskType
        {
            get { return this._TaskType; }
            set { this._TaskType = value; this.OnPropertyChanged("TaskType"); }
        }

        public ActionType ActionType
        {
            get { return this._ActionType; }
            set { this._ActionType = value; this.OnPropertyChanged("ActionType"); }
        }

        public DateTime RunTime
        {
            get { return this._RunTime; }
            set { this._RunTime = value; }
        }

        public DateTime LastRunTime
        {
            get { return this._LastRunTime; }
            set { this._LastRunTime = value; this.OnPropertyChanged("LastRunTime"); }
        }

        public string Creater
        {
            get { return "DEM"; }
        }

        public DateTime CreateDateTime
        {
            get { return this._CreateDateTime; }
            set { this._CreateDateTime = value; }
        }

        public bool IsEditorName
        {
            get;
            set;
        }

        #endregion

        internal CommonTaskScheduler ToCommonTaskScheduler()
        {
            CommonTaskScheduler commonTaskScheduler = new CommonTaskScheduler();

            commonTaskScheduler.Id = this.Id;
            commonTaskScheduler.ExchangeCode = this.ExchangeCode;
            commonTaskScheduler.Name = this.Name;
            commonTaskScheduler.Description = this.Description;
            commonTaskScheduler.TaskStatus = Manager.Common.TaskStatus.Ready;
            commonTaskScheduler.RunTime = this.RunTime;
            commonTaskScheduler.LastRunTime = this.LastRunTime;
            commonTaskScheduler.TaskType = Manager.Common.TaskType.SetParameterTask;
            commonTaskScheduler.CreateDate = this.CreateDateTime;
            commonTaskScheduler.ActionType = this.ActionType;
            commonTaskScheduler.ExchangInstruments = this.ExchangInstruments;

            IEnumerable<ParameterSetting> newParameterSettings = this.ParameterSettings.Where(P => P.IsSelected);

            this.ParameterSettings = new ObservableCollection<ParameterSetting>();

            foreach (ParameterSetting setting in newParameterSettings)
            {
                setting.TaskSchedulerId = this.Id;
                this.ParameterSettings.Add(setting);
                commonTaskScheduler.ParameterSettings.Add(setting.ToCommonParameterSettingTask());
            }
           
            return commonTaskScheduler;
        }

        internal void Update(CommonTaskScheduler commonTaskScheduler)
        {
            this._Id = commonTaskScheduler.Id;
            this._ExchangeCode = commonTaskScheduler.ExchangeCode;
            this._Name = commonTaskScheduler.Name;
            this._Description = commonTaskScheduler.Description;
            this._TaskStatus = commonTaskScheduler.TaskStatus;
            this._ActionType = commonTaskScheduler.ActionType;
            this._TaskType = commonTaskScheduler.TaskType;
            this._CreateDateTime = commonTaskScheduler.CreateDate;
            this._Creater = commonTaskScheduler.Creater;
            this._RunTime = commonTaskScheduler.RunTime;
            this._LastRunTime = commonTaskScheduler.LastRunTime;

            foreach (ParameterSettingTask setting in commonTaskScheduler.ParameterSettings)
            {
                ParameterSetting parameterSetting = new ParameterSetting(setting);
                this._ParameterSettings.Add(parameterSetting);
            }
        }
    }

    public class ParameterSetting
    {
        public bool IsSelected { get; set; }
        public Guid TaskSchedulerId { get; set; }
        public string ParameterKey { get; set; }
        public SettingParameterType SettingParameterType { get; set; }
        public SqlDbType SqlDbType { get; set; }
        public int Interval { get; set; }
        public SqlDbTypeCellData SqlDbTypeCellData{get;set;}

        public object SetValue { get; set; }

        public ParameterSetting(ParameterDefine parameteDefine)
        {
            this.SqlDbTypeCellData = new SqlDbTypeCellData();
            this.Update(parameteDefine);
        }

        public ParameterSetting(ParameterSettingTask parameterSettingTask)
        {
            this.SqlDbTypeCellData = new SqlDbTypeCellData();
            this.Update(parameterSettingTask);
        }

        internal void Update(ParameterDefine parameteDefine)
        {
            this.ParameterKey = parameteDefine.ParameterKey;
            this.SettingParameterType = parameteDefine.SettingParameterType;
            this.SqlDbType = parameteDefine.SqlDbType;
            this.SettingCellData();
        }

        internal void Update(ParameterSettingTask parameterSettingTask)
        {
            this.ParameterKey = parameterSettingTask.ParameterKey;
            this.SettingParameterType = parameterSettingTask.SettingParameterType;
            this.SqlDbType = parameterSettingTask.SqlDbType;
            this.SettingCellData();
        }

        internal ParameterSettingTask ToCommonParameterSettingTask()
        {
            ParameterSettingTask parameterSetting = new ParameterSettingTask();
            parameterSetting.TaskSchedulerId = this.TaskSchedulerId;
            parameterSetting.ParameterKey = this.ParameterKey;
            parameterSetting.SettingParameterType = this.SettingParameterType;
            parameterSetting.SqlDbType = this.SqlDbType;
            parameterSetting.ParameterValue = this.GetParameterValue();

            return parameterSetting;
        }

        internal string GetParameterValue()
        {
            string parameterValue = string.Empty;
            if (this.SqlDbType == SqlDbType.Int || this.SqlDbType == SqlDbType.Decimal)
            {
                parameterValue = this.SqlDbTypeCellData.NumberSetValue.ToString();
            }
            else if (this.SqlDbType == SqlDbType.Bit)
            {
                parameterValue = this.SqlDbTypeCellData.BoolSetValue.ToString();
            }
            else if (this.SqlDbType == SqlDbType.DateTime)
            {
                parameterValue = this.SqlDbTypeCellData.DateSetValue.ToString();
            }
            return parameterValue;
        }

        internal void SettingCellData()
        {
            if (this.SqlDbType == SqlDbType.Int || this.SqlDbType == SqlDbType.Decimal)
            {
                this.SqlDbTypeCellData.IsNumberType = Visibility.Visible;
            }
            else if(this.SqlDbType == SqlDbType.Bit)
            {
                this.SqlDbTypeCellData.IsBoolType = Visibility.Visible; ;
            }
            else if (this.SqlDbType == SqlDbType.DateTime)
            {
                this.SqlDbTypeCellData.IsDateTimeType = Visibility.Visible; ;
            }
        }
    }

    public class SqlDbTypeCellData
    {
        public bool BoolSetValue { get; set; }
        public decimal NumberSetValue { get; set; }
        public DateTime DateSetValue { get; set; }

        public Visibility IsBoolType { get; set; }
        public Visibility IsNumberType { get; set; }
        public Visibility IsDateTimeType { get; set; }

        public SqlDbTypeCellData()
        {
            this.IsBoolType = Visibility.Collapsed;
            this.IsNumberType = Visibility.Collapsed; ;
            this.IsDateTimeType = Visibility.Collapsed; ;
        } 
    }
}
