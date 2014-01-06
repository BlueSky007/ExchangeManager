using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;

namespace Manager.Common.Settings
{
    public class TaskScheduler
    {
        public TaskScheduler()
        {
            this.ParameterSettings = new ObservableCollection<ParameterSettingTask>();
            this.ParameterDefines = new ObservableCollection<ParameterDefine>();
        }

        public ObservableCollection<ParameterSettingTask> ParameterSettings { get; set; }
        public ObservableCollection<ParameterDefine> ParameterDefines { get; set; }
        public ObservableCollection<ExchangInstrument> ExchangInstruments { get; set; }
        public Guid Id { get; set; }
        public string Name{ get; set; }
        public string ExchangeCode { get; set; }
        public string Description { get; set; }
        public TaskType TaskType { get; set; }
        public TaskStatus TaskStatus { get; set; }
        public ActionType ActionType { get; set; }
        public DateTime RunTime { get; set; }
        public DateTime LastRunTime { get; set; }
        public int? Interval { get; set; }
        public DateTime CreateDate { get; set; }
        public Guid Creater { get; set; }
        public string ScheduleID { get; set; }
    }

    public class ParameterSettingTask
    {
        public Guid Id { get; set; }
        public Guid TaskSchedulerId { get; set; }
        public string ParameterKey { get; set; }
        public string ParameterValue { get; set; }
        public SettingParameterType SettingParameterType { get; set; }
        public SqlDbType SqlDbType { get; set; }
    }

    public class ParameterDefine
    {
        public string ParameterKey { get; set; }
        public SettingParameterType SettingParameterType { get; set; }
        public SqlDbType SqlDbType { get; set; }
    }
    public class ExchangInstrument
    {
        public Guid Id { get; set; }
        public string ExchangeCode { get; set; }
        public Guid InstrumentId { get; set; }
        public string InstrumentCode { get; set; }
    }
}
