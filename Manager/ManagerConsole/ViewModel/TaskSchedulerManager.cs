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

        public void UpdateTaskScheduler(TaskScheduler taskScheduler)
        {
            foreach (TaskScheduler task in this._TaskSchedulers)
            {
                if (task.Id == taskScheduler.Id)
                {
                    task.Update(taskScheduler);
                    return;
                }
            }
        }

        public void RemoveTaskScheduler(Guid taskSchedulerId)
        {
            foreach (TaskScheduler taskScheduler in this._TaskSchedulers)
            { 
                if(taskScheduler.Id == taskSchedulerId)
                {
                    this._TaskSchedulers.Remove(taskScheduler);
                    return;
                }
            }
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

        private int _RecurDay = 0;
        private int _Interval = 0;
        private string _WeekDaySN;
        private WeekDays _WeekDays;

        private ObservableCollection<ParameterSetting> _ParameterSettings;
        private ObservableCollection<ExchangInstrument> _ExchangInstruments { get; set; }
        #endregion

        public TaskScheduler() 
        {
            this._RunTime = DateTime.Now;
            this._LastRunTime = DateTime.MaxValue;
            this._Id = Guid.NewGuid();
            this._ParameterSettings = new ObservableCollection<ParameterSetting>();
            this._ExchangInstruments = new ObservableCollection<ExchangInstrument>();
            this._WeekDays = new WeekDays();
        }

        public TaskScheduler(CommonTaskScheduler commonTaskScheduler)
        {
            this._Id = commonTaskScheduler.Id;
            this._ParameterSettings = new ObservableCollection<ParameterSetting>();
            this._ExchangInstruments = new ObservableCollection<ExchangInstrument>();
            this.Update(commonTaskScheduler);
        }

        #region public property
        public ObservableCollection<ParameterSetting> ParameterSettings
        {
            get { return this._ParameterSettings; }
            set { this._ParameterSettings = value; this.OnPropertyChanged("ParameterSettings"); }
        }

        public ObservableCollection<ExchangInstrument> ExchangInstruments
        {
            get { return this._ExchangInstruments; }
            set { this._ExchangInstruments = value; }
        }

        public Guid Id
        {
            get { return this._Id; }
            set { this._Id = value; }
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
            set { this._Description = value; this.OnPropertyChanged("Description"); }
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
            set { this._RunTime = value; this.OnPropertyChanged("RunTime"); }
        }

        public DateTime LastRunTime
        {
            get { return this._LastRunTime; }
            set { this._LastRunTime = value; this.OnPropertyChanged("LastRunTime"); }
        }

        public int RecurDay
        {
            get { return this._RecurDay; }
            set { this._RecurDay = value; this.OnPropertyChanged("RecurDay"); }
        }

        public int Interval
        {
            get { return this._Interval; }
            set { this._Interval = value; this.OnPropertyChanged("Interval"); }
        }

        public string WeekDaySN
        {
            get { return this._WeekDaySN; }
            set { this._WeekDaySN = value; this.OnPropertyChanged("WeekDaySN"); }
        }
        public WeekDays WeekDays
        {
            get { return this._WeekDays; }
            set { this._WeekDays = value; this.OnPropertyChanged("WeekDays"); }
        }

        public string Creater
        {
            get { return "Admin";}
        }

        public DateTime CreateDateTime
        {
            get { return this._CreateDateTime; }
            set { this._CreateDateTime = value; this.OnPropertyChanged("CreateDateTime"); }
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
            commonTaskScheduler.RecurDay = this.RecurDay;
            commonTaskScheduler.Interval = this.Interval;
            commonTaskScheduler.TaskType = Manager.Common.TaskType.SetParameterTask;
            commonTaskScheduler.CreateDate = this.CreateDateTime;
            commonTaskScheduler.ActionType = this.ActionType;
            commonTaskScheduler.WeekDaySN = this.GetWeekString();

            if (this.ExchangInstruments.Count > 0)
            {
                foreach (ExchangInstrument entity in this.ExchangInstruments)
                {
                    commonTaskScheduler.ExchangInstruments.Add(entity);
                }
            }

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
            this._Interval = commonTaskScheduler.Interval;
            this._RecurDay = commonTaskScheduler.RecurDay;
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

        internal void Update(TaskScheduler newTaskScheduler)
        {
            this.ExchangeCode = newTaskScheduler.ExchangeCode;
            this.Name = newTaskScheduler.Name;
            this.Description = newTaskScheduler.Description;
            this.TaskStatus = newTaskScheduler.TaskStatus;
            this.RecurDay = newTaskScheduler.RecurDay;
            this.Interval = newTaskScheduler.Interval;
            this.ActionType = newTaskScheduler.ActionType;
            this.TaskType = newTaskScheduler.TaskType;
            this.CreateDateTime = newTaskScheduler.CreateDateTime;
            this.RunTime = newTaskScheduler.RunTime;
            this.LastRunTime = newTaskScheduler.LastRunTime;

            this.ParameterSettings = newTaskScheduler.ParameterSettings;
        }

        internal TaskScheduler Clone()
        {
            TaskScheduler taskScheduler = new TaskScheduler();

            taskScheduler.Id = this.Id;
            taskScheduler.ExchangeCode = this.ExchangeCode;
            taskScheduler.Name = this.Name;
            taskScheduler.Description = this.Description;
            taskScheduler.TaskStatus = this.TaskStatus;
            taskScheduler.ActionType = this.ActionType;
            taskScheduler.RecurDay = this.RecurDay;
            taskScheduler.Interval = this.Interval;
            taskScheduler.TaskType = this.TaskType;
            taskScheduler.CreateDateTime = this.CreateDateTime;
            taskScheduler.RunTime = this.RunTime;
            taskScheduler.LastRunTime = this.LastRunTime;
            taskScheduler.WeekDays = this.WeekDays;
            taskScheduler.WeekDaySN = this.WeekDaySN;

            foreach (ParameterSetting setting in this.ParameterSettings)
            {
                taskScheduler.ParameterSettings.Add(setting.Clone());
            }

            return taskScheduler;
        }

        internal string GetWeekString()
        {
            string weekStr = string.Empty;
            if (this._WeekDays == null) return weekStr;
            if (this.WeekDays.IsMonDay)
            {
                weekStr += "1" + ",";
            }
            if (this.WeekDays.IsThurDay)
            {
                weekStr += "2" + ",";
            }
            if (this.WeekDays.IsWedDay)
            {
                weekStr += "3" + ",";
            }
            if (this.WeekDays.IsThurDay)
            {
                weekStr += "4" + ",";
            }
            if (this.WeekDays.IsFriDay)
            {
                weekStr += "5" + ",";
            }
            if (this.WeekDays.IsSateDay)
            {
                weekStr += "6" + ",";
            }
            if (this.WeekDays.IsSunDay)
            {
                weekStr += "7" + ",";
            }

            return weekStr;
        }
    }

    public class ParameterSetting
    {
        public bool IsSelected { get; set; }
        public Guid TaskSchedulerId { get; set; }
        public string ParameterKey { get; set; }
        public string ParameterValue { get; set; }
        public SettingParameterType SettingParameterType { get; set; }
        public SqlDbType SqlDbType { get; set; }
        public int Interval { get; set; }
        public SqlDbTypeCellData SqlDbTypeCellData{get;set;}

        public object SetValue { get; set; }

        public ParameterSetting()
        {
            this.SqlDbTypeCellData = new SqlDbTypeCellData();
        }

        public ParameterSetting(ParameterDefine parameteDefine)
            : this()
        {
            this.Update(parameteDefine);
        }

        public ParameterSetting(ParameterSettingTask parameterSettingTask)
            : this()
        {
            this.Update(parameterSettingTask);
        }

        internal void Update(ParameterDefine parameteDefine)
        {
            this.ParameterKey = parameteDefine.ParameterKey;
            this.ParameterValue = string.Empty;
            this.SettingParameterType = parameteDefine.SettingParameterType;
            this.SqlDbType = parameteDefine.SqlDbType;
            this.SettingCellData(this.ParameterValue);
        }

        internal void Update(ParameterSettingTask parameterSettingTask)
        {
            this.ParameterKey = parameterSettingTask.ParameterKey;
            string parameterValue = parameterSettingTask.ParameterValue;
            this.SettingParameterType = parameterSettingTask.SettingParameterType;
            this.SqlDbType = parameterSettingTask.SqlDbType;
            this.SettingCellData(parameterValue);
        }

        internal ParameterSetting Clone()
        {
            ParameterSetting setting = new ParameterSetting();
            setting.IsSelected = this.IsSelected;
            setting.Interval = this.Interval;
            setting.TaskSchedulerId = this.TaskSchedulerId;
            setting.ParameterKey = this.ParameterKey;
            setting.SettingParameterType = this.SettingParameterType;
            setting.SqlDbType = this.SqlDbType;
            setting.SqlDbTypeCellData = this.SqlDbTypeCellData;
            setting.SettingCellData(string.Empty);

            return setting;
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

        internal void SettingCellData(string newParameterValue)
        {
            if (this.SqlDbType == SqlDbType.Int || this.SqlDbType == SqlDbType.Decimal)
            {
                this.SqlDbTypeCellData.IsNumberType = Visibility.Visible;
                if (string.IsNullOrEmpty(newParameterValue)) return;
                this.SqlDbTypeCellData.NumberSetValue = decimal.Parse(newParameterValue);
            }
            else if(this.SqlDbType == SqlDbType.Bit)
            {
                this.SqlDbTypeCellData.IsBoolType = Visibility.Visible;
                if (string.IsNullOrEmpty(newParameterValue)) return;
                this.SqlDbTypeCellData.BoolSetValue = bool.Parse(newParameterValue);
            }
            else if (this.SqlDbType == SqlDbType.DateTime)
            {
                this.SqlDbTypeCellData.IsDateTimeType = Visibility.Visible;
                if (string.IsNullOrEmpty(newParameterValue)) return;
                this.SqlDbTypeCellData.DateSetValue = DateTime.Parse(newParameterValue);
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

    public class WeekDays
    {
        public bool IsMonDay { get; set; }
        public bool IsTuesDay { get; set; }
        public bool IsWedDay { get; set; }
        public bool IsThurDay { get; set; }
        public bool IsFriDay { get; set; }
        public bool IsSateDay { get; set; }
        public bool IsSunDay { get; set; }
    }
}
