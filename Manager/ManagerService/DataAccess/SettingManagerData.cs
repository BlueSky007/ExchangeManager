using Manager.Common.Settings;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Manager.Common;
using System.Xml;
using System.Collections.ObjectModel;
using ExchangeIntrument = Manager.Common.Settings.ExchangInstrument;
using iExchange.Common.Manager;
using System.Diagnostics;

namespace ManagerService.DataAccess
{
    public class SettingManagerData
    {
        public static List<TaskScheduler> InitailizedTaskSchedulers()
        {
            List<TaskScheduler> taskSchedulers = new List<TaskScheduler>();
            string sql = "dbo.GetInitialDataForTaskScheduler";
            DataAccess.GetInstance().ExecuteReader(sql, CommandType.Text, delegate(SqlDataReader reader) {
                while (reader.Read())
                {
                    TaskScheduler taskScheduler = new TaskScheduler();
                    taskScheduler.Id = (Guid)reader["Id"];
                    taskScheduler.Name = (string)reader["Name"];
                    taskScheduler.Description = (string)reader["Description"];
                    taskScheduler.TaskStatus = reader["TaskStatus"].ConvertToEnumValue<TaskStatus>();
                    taskScheduler.RunTime = (DateTime)reader["RunTime"];
                    taskScheduler.LastRunTime = (reader["LastRunTime"] == DBNull.Value) ? DateTime.MaxValue : (DateTime)reader["LastRunTime"];
                    taskScheduler.TaskType = reader["TaskType"].ConvertToEnumValue<TaskType>();
                    taskScheduler.ActionType = reader["ActionType"].ConvertToEnumValue<ActionType>();
                    taskScheduler.RecurDay = reader["RecurDay"] == DBNull.Value ? 0 : (int)reader["RecurDay"];
                    taskScheduler.WeekDaySN = reader["WeekDaySN"] == DBNull.Value ? string.Empty : (string)reader["WeekDaySN"];
                    taskScheduler.Interval = reader["Interval"] == DBNull.Value ? 0 : (int)reader["Interval"];
                    taskScheduler.Creater = (Guid)reader["UserId"];
                    taskScheduler.CreateDate = (DateTime)reader["Timestamp"];

                    taskSchedulers.Add(taskScheduler);
                }
                reader.NextResult();
                while (reader.Read())
                {
                    ParameterSettingTask parameterSettingTask = new ParameterSettingTask();
                    parameterSettingTask.Id = (Guid)reader["Id"];
                    parameterSettingTask.TaskSchedulerId = (Guid)reader["TaskSchedulerId"];
                    parameterSettingTask.ParameterKey = (string)reader["ParameterKey"];
                    parameterSettingTask.ParameterValue = (string)reader["ParameterValue"];
                    parameterSettingTask.SettingParameterType = reader["SettingType"].ConvertToEnumValue<SettingParameterType>();
                    parameterSettingTask.SqlDbType = reader["SqlDbType"].ConvertToEnumValue<SqlDbType>();

                    TaskScheduler taskScheduler = taskSchedulers.SingleOrDefault(P => P.Id == parameterSettingTask.TaskSchedulerId);
                    if (taskScheduler == null) continue;
                    taskScheduler.ParameterSettings.Add(parameterSettingTask);
                }
                reader.NextResult();
                while (reader.Read())
                {
                    ExchangeIntrument instrument = new ExchangeIntrument();
                    //instrument.Id = (Guid)reader["Id"];
                    instrument.ExchangeCode = (string)reader["ExchangeCode"];
                    instrument.InstrumentId = (Guid)reader["InstrumentId"];
                    instrument.InstrumentCode = (string)reader["InstrumentCode"];
                    Guid taskId = (Guid)reader["TaskSchedulerId"];

                    TaskScheduler taskScheduler = taskSchedulers.SingleOrDefault(P => P.Id == taskId);
                    if (taskScheduler == null) continue;
                    taskScheduler.ExchangInstruments.Add(instrument);
                }
            });
            return taskSchedulers;
        }

        public static List<ParameterDefine> LoadParameterDefine()
        {
            List<ParameterDefine> parameterDefines = new List<ParameterDefine>();
            string sql = "SELECT * FROM dbo.FT_ParameterDefine()";
            DataAccess.GetInstance().ExecuteReader(sql, CommandType.Text, delegate(SqlDataReader reader) {
                while (reader.Read())
                {
                    ParameterDefine parameter = new ParameterDefine();
                    parameter.ParameterKey = reader["ParameterKey"].ToString();
                    parameter.SettingParameterType = reader["SettingType"].ConvertToEnumValue<SettingParameterType>();
                    parameter.SqlDbType = reader["SqlDbType"].ConvertToEnumValue<SqlDbType>();

                    parameterDefines.Add(parameter);
                }
            });
            return parameterDefines;
        }

        public static bool CreateTaskScheduler(TaskScheduler taskScheduler)
        {
            string instrumentSettings = string.Empty;
            if(taskScheduler.ExchangInstruments != null)
            {
                instrumentSettings = GetInstrumentSettings(taskScheduler);
            }
            string parameterSettings = GetParameterSettingTasks(taskScheduler);
            using (SqlConnection sqlConnection = DataAccess.GetInstance().GetSqlConnection())
            {
                SqlCommand sqlCommand = sqlConnection.CreateCommand();
                sqlCommand.CommandText = "dbo.P_TaskScheduler_Ins";
                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.CommandTimeout = 60 * 30;

                SqlParameter parameter = new SqlParameter("@id", SqlDbType.UniqueIdentifier);
                parameter.Value = taskScheduler.Id;
                sqlCommand.Parameters.Add(parameter);

                parameter = new SqlParameter("@name", SqlDbType.NVarChar);
                parameter.Value = taskScheduler.Name;
                sqlCommand.Parameters.Add(parameter);

                parameter = new SqlParameter("@description", SqlDbType.NVarChar);
                parameter.Value = taskScheduler.Description;
                sqlCommand.Parameters.Add(parameter);

                parameter = new SqlParameter("@taskStatus", SqlDbType.TinyInt);
                parameter.Value = (byte)taskScheduler.TaskStatus;
                sqlCommand.Parameters.Add(parameter);

                parameter = new SqlParameter("@runTime", SqlDbType.DateTime);
                parameter.Value = taskScheduler.RunTime;
                sqlCommand.Parameters.Add(parameter);

                parameter = new SqlParameter("@lastRunTime", SqlDbType.DateTime);
                parameter.Value = taskScheduler.LastRunTime;
                sqlCommand.Parameters.Add(parameter);

                parameter = new SqlParameter("@taskType", SqlDbType.TinyInt);
                parameter.Value = (byte)taskScheduler.TaskType;
                sqlCommand.Parameters.Add(parameter);

                parameter = new SqlParameter("@actionType", SqlDbType.TinyInt);
                parameter.Value = (byte)taskScheduler.ActionType;
                sqlCommand.Parameters.Add(parameter);

                parameter = new SqlParameter("@recurDay", SqlDbType.Int);
                parameter.Value = taskScheduler.RecurDay;
                sqlCommand.Parameters.Add(parameter);

                parameter = new SqlParameter("@weekDaySN", SqlDbType.NVarChar);
                parameter.Value = taskScheduler.WeekDaySN;
                sqlCommand.Parameters.Add(parameter);

                parameter = new SqlParameter("@interval", SqlDbType.Int);
                parameter.Value = taskScheduler.Interval;
                sqlCommand.Parameters.Add(parameter);

                parameter = new SqlParameter("@userId", SqlDbType.UniqueIdentifier);
                parameter.Value = taskScheduler.Creater;
                sqlCommand.Parameters.Add(parameter);

                parameter = new SqlParameter("@timestamp", SqlDbType.DateTime);
                parameter.Value = taskScheduler.CreateDate;
                sqlCommand.Parameters.Add(parameter);

                parameter = new SqlParameter("@parameterSettingsXml", SqlDbType.NText);
                parameter.Value = parameterSettings;
                sqlCommand.Parameters.Add(parameter);

                parameter = new SqlParameter("@instrumentSettingsXml", SqlDbType.NText);
                parameter.Value = instrumentSettings;
                sqlCommand.Parameters.Add(parameter);

                SqlParameter resultParameter = new SqlParameter("@result", SqlDbType.Bit);
                resultParameter.Direction = ParameterDirection.Output;
                sqlCommand.Parameters.Add(resultParameter);

                sqlCommand.ExecuteNonQuery();

                return (bool)sqlCommand.Parameters["@result"].Value;
            }
        }

        public static bool EditorTaskScheduler(TaskScheduler taskScheduler)
        {
            string instrumentSettings = string.Empty;
            if (taskScheduler.ExchangInstruments != null)
            {
                instrumentSettings = GetInstrumentSettings(taskScheduler);
            }
            string parameterSettings = GetParameterSettingTasks(taskScheduler);
            using (SqlConnection sqlConnection = DataAccess.GetInstance().GetSqlConnection())
            {
                SqlCommand sqlCommand = sqlConnection.CreateCommand();
                sqlCommand.CommandText = "dbo.P_TaskScheduler_Upd";
                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.CommandTimeout = 60 * 30;

                SqlParameter parameter = new SqlParameter("@id", SqlDbType.UniqueIdentifier);
                parameter.Value = taskScheduler.Id;
                sqlCommand.Parameters.Add(parameter);

                parameter = new SqlParameter("@name", SqlDbType.NVarChar);
                parameter.Value = taskScheduler.Name;
                sqlCommand.Parameters.Add(parameter);

                parameter = new SqlParameter("@description", SqlDbType.NVarChar);
                parameter.Value = taskScheduler.Description;
                sqlCommand.Parameters.Add(parameter);

                parameter = new SqlParameter("@taskStatus", SqlDbType.TinyInt);
                parameter.Value = (byte)taskScheduler.TaskStatus;
                sqlCommand.Parameters.Add(parameter);

                parameter = new SqlParameter("@runTime", SqlDbType.DateTime);
                parameter.Value = taskScheduler.RunTime;
                sqlCommand.Parameters.Add(parameter);

                parameter = new SqlParameter("@lastRunTime", SqlDbType.DateTime);
                parameter.Value = taskScheduler.LastRunTime;
                sqlCommand.Parameters.Add(parameter);

                parameter = new SqlParameter("@taskType", SqlDbType.TinyInt);
                parameter.Value = (byte)taskScheduler.TaskType;
                sqlCommand.Parameters.Add(parameter);

                parameter = new SqlParameter("@actionType", SqlDbType.TinyInt);
                parameter.Value = (byte)taskScheduler.ActionType;
                sqlCommand.Parameters.Add(parameter);

                parameter = new SqlParameter("@recurDay", SqlDbType.Int);
                parameter.Value = taskScheduler.RecurDay;
                sqlCommand.Parameters.Add(parameter);

                parameter = new SqlParameter("@weekDaySN", SqlDbType.NVarChar);
                parameter.Value = taskScheduler.WeekDaySN;
                sqlCommand.Parameters.Add(parameter);

                parameter = new SqlParameter("@interval", SqlDbType.Int);
                parameter.Value = taskScheduler.Interval;
                sqlCommand.Parameters.Add(parameter);

                parameter = new SqlParameter("@userId", SqlDbType.UniqueIdentifier);
                parameter.Value = taskScheduler.Creater;
                sqlCommand.Parameters.Add(parameter);

                parameter = new SqlParameter("@timestamp", SqlDbType.DateTime);
                parameter.Value = taskScheduler.CreateDate;
                sqlCommand.Parameters.Add(parameter);

                parameter = new SqlParameter("@parameterSettingsXml", SqlDbType.NText);
                parameter.Value = parameterSettings;
                sqlCommand.Parameters.Add(parameter);

                parameter = new SqlParameter("@instrumentSettingsXml", SqlDbType.NText);
                parameter.Value = instrumentSettings;
                sqlCommand.Parameters.Add(parameter);

                SqlParameter resultParameter = new SqlParameter("@result", SqlDbType.Bit);
                resultParameter.Direction = ParameterDirection.Output;
                sqlCommand.Parameters.Add(resultParameter);

                sqlCommand.ExecuteNonQuery();

                return (bool)sqlCommand.Parameters["@result"].Value;
            }
        }

        public static bool DeleteTaskScheduler(TaskScheduler taskScheduler)
        {
            using (SqlConnection sqlConnection = DataAccess.GetInstance().GetSqlConnection())
            {
                SqlCommand sqlCommand = sqlConnection.CreateCommand();
                sqlCommand.CommandText = "dbo.P_TaskScheduler_Del";
                sqlCommand.CommandType = CommandType.StoredProcedure;

                SqlParameter parameter = new SqlParameter("@id", SqlDbType.UniqueIdentifier);
                parameter.Value = taskScheduler.Id;
                sqlCommand.Parameters.Add(parameter);

                SqlParameter resultParameter = new SqlParameter("@result", SqlDbType.Bit);
                resultParameter.Direction = ParameterDirection.Output;
                sqlCommand.Parameters.Add(resultParameter);


                sqlCommand.ExecuteNonQuery();

                return (bool)sqlCommand.Parameters["@result"].Value;
            }
        }

        private static string GetParameterSettingTasks(TaskScheduler taskScheduler)
        {
            XmlDocument doc = new XmlDocument();
            XmlElement settingRot = doc.CreateElement("ParameterSettings");

            foreach (ParameterSettingTask setting in taskScheduler.ParameterSettings)
            {
                XmlElement settingElement = doc.CreateElement("ParameterSetting");
                settingElement.SetAttribute("Id", Guid.NewGuid().ToString());
                settingElement.SetAttribute("TaskSchedulerId", setting.TaskSchedulerId.ToString());
                settingElement.SetAttribute("ParameterKey", setting.ParameterKey);
                settingElement.SetAttribute("ParameterValue", setting.ParameterValue);
                settingElement.SetAttribute("SettingParameterType", ((byte)setting.SettingParameterType).ToString());
                settingElement.SetAttribute("SqlDbType", ((byte)setting.SqlDbType).ToString());

                settingRot.AppendChild(settingElement);
            }
            doc.AppendChild(settingRot);
            return doc.OuterXml;
        }

        private static string GetInstrumentSettings(TaskScheduler taskScheduler)
        {
            XmlDocument doc = new XmlDocument();
            XmlElement settingRot = doc.CreateElement("InstrumentSettings");

            foreach (ExchangInstrument instrument in taskScheduler.ExchangInstruments)
            {
                XmlElement settingElement = doc.CreateElement("InstrumentSetting");
                settingElement.SetAttribute("Id", Guid.NewGuid().ToString());
                settingElement.SetAttribute("ExchangeCode", instrument.ExchangeCode);
                settingElement.SetAttribute("InstrumentId", instrument.InstrumentId.ToString());
                settingElement.SetAttribute("InstrumentCode", instrument.InstrumentCode);

                settingRot.AppendChild(settingElement);
            }
            doc.AppendChild(settingRot);
            return doc.OuterXml;
        }

        private static string GetSettingsDetails(TaskScheduler taskScheduler)
        {
            XmlDocument doc = new XmlDocument();
            XmlElement settingDetailRoot = doc.CreateElement("SettingDetails");

            foreach (ParameterSettingTask setting in taskScheduler.ParameterSettings)
            {
                XmlElement settingElement = doc.CreateElement("SettingDetail");
                settingElement.SetAttribute("UserId", taskScheduler.Creater.ToString());
                settingElement.SetAttribute("ParameterKey", setting.ParameterKey);
                settingElement.SetAttribute("ParameterValue", setting.ParameterValue);
                settingElement.SetAttribute("SettingParameterType", ((byte)setting.SettingParameterType).ToString());

                settingDetailRoot.AppendChild(settingElement);
            }
            doc.AppendChild(settingDetailRoot);
            return doc.OuterXml;
        }

        public static XmlNode GetInstrumentParametersXml(TaskScheduler taskScheduler)
        {
            XmlDocument exchangeDoc = new XmlDocument();
            XmlElement xmlInstrumentRoot = exchangeDoc.CreateElement("Instruments");

            foreach (ExchangeIntrument instrument in taskScheduler.ExchangInstruments)
            {
                XmlElement instrumentElement = exchangeDoc.CreateElement("Instrument");
                instrumentElement.SetAttribute("ID", instrument.InstrumentId.ToString());
                foreach (ParameterSettingTask setting in taskScheduler.ParameterSettings)
                {
                    if (setting.SettingParameterType == SettingParameterType.InstrumentParameter)
                    {
                        instrumentElement.SetAttribute(setting.ParameterKey, setting.ParameterValue);
                    }
                }
                xmlInstrumentRoot.AppendChild(instrumentElement);
            }
            exchangeDoc.AppendChild(xmlInstrumentRoot);
            return exchangeDoc.DocumentElement;
        }

        public static ParameterUpdateTask GetExchangeParametersTask(TaskScheduler taskScheduler)
        {
            ParameterUpdateTask task = new ParameterUpdateTask();
            List<Guid> instruments = new List<Guid>();
            foreach (ExchangeIntrument instrument in taskScheduler.ExchangInstruments)
            {
                instruments.Add(instrument.InstrumentId);
            }
            foreach (ParameterSettingTask setting in taskScheduler.ParameterSettings)
            {
                if (setting.SettingParameterType == SettingParameterType.InstrumentParameter)
                {
                    ExchangeSetting exchangeSetting = new ExchangeSetting();
                    exchangeSetting.ParameterKey = setting.ParameterKey;
                    exchangeSetting.ParameterValue = setting.ParameterValue;
                    task.ExchangeSettings.Add(exchangeSetting);
                }
            }
            task.Instruments = instruments.ToArray();
            return task;
        }

        public static bool UpdateSettingsParameter(TaskScheduler taskScheduler)
        {
            string settingDetails = GetSettingsDetails(taskScheduler);
            using (SqlConnection sqlConnection = DataAccess.GetInstance().GetSqlConnection())
            {
                SqlCommand sqlCommand = sqlConnection.CreateCommand();
                sqlCommand.CommandText = "dbo.P_SettingsDetail_Upd";
                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.CommandTimeout = 60 * 30;

                SqlParameter parameter = new SqlParameter("@taskSchedulerId", SqlDbType.UniqueIdentifier);
                parameter.Value = taskScheduler.Id;
                sqlCommand.Parameters.Add(parameter);

                parameter = new SqlParameter("@lastRunTime", SqlDbType.DateTime);
                parameter.Value = taskScheduler.RunTime;
                sqlCommand.Parameters.Add(parameter);

                parameter = new SqlParameter("@settingDetailsXml", SqlDbType.NText);
                parameter.Value = settingDetails;
                sqlCommand.Parameters.Add(parameter);

                SqlParameter resultParameter = new SqlParameter("@result", SqlDbType.Bit);
                resultParameter.Direction = ParameterDirection.Output;
                sqlCommand.Parameters.Add(resultParameter);

                sqlCommand.ExecuteNonQuery();

                return (bool)sqlCommand.Parameters["@result"].Value;
            }
        }

        public static SettingsParameter LoadSettingsParameter(Guid userId)
        {
            SettingsParameter settingsParameter = new SettingsParameter();
            DealingOrderParameter dealingOrderParameter = new DealingOrderParameter();
            SetValueSetting setValueSetting = new SetValueSetting();
            List<SoundSetting> soundSettings = new List<SoundSetting>();
            List<string> soundKeys = new List<string>();
            try
            {
                using (SqlConnection con = DataAccess.GetInstance().GetSqlConnection())
                {
                    using (SqlCommand command = con.CreateCommand())
                    {
                        command.CommandText = "[dbo].[P_GetSettingParameter]";
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@userID", userId));
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                //Guid id = (Guid)reader["Id"];
                                if (settingsParameter.SettingId == Guid.Empty)
                                {
                                    Guid settingId = (Guid)reader["SettingsId"];
                                    settingsParameter.SettingId = settingId;
                                }
                                SettingParameterType SettingType = reader["SettingType"].ConvertToEnumValue<SettingParameterType>();
                                string parameterKey = (string)reader["ParameterKey"];
                                string parameterValue = (string)reader["ParameterValue"];

                                if (SettingType == SettingParameterType.DealingOrderParameter)
                                {
                                    SettingParameterHelper.UpdateSettingValue(dealingOrderParameter, parameterKey, parameterValue);
                                }
                                else if (SettingType == SettingParameterType.SoundParameter)
                                {
                                    SoundSetting setting = new SoundSetting();
                                    SoundType soundType = reader["SoundType"].ConvertToEnumValue<SoundType>();
                                    setting.SoundKey = parameterKey;
                                    setting.SoundPath = parameterValue;
                                    setting.SoundType = soundType;
                                    soundSettings.Add(setting);
                                    soundKeys.Add(parameterKey);
                                }
                                else if (SettingType == SettingParameterType.SetValueParameter)
                                {
                                    SettingParameterHelper.UpdateSettingValue(setValueSetting, parameterKey, parameterValue);
                                }
                            }
                            reader.NextResult();
                            while (reader.Read())
                            {
                                string soundKey = (string)reader["SoundKey"];
                                if (soundKeys.Contains(soundKey)) continue;
                                SoundType soundType = reader["SoundType"].ConvertToEnumValue<SoundType>();

                                SoundSetting setting = new SoundSetting();

                                setting.SoundKey = soundKey;
                                setting.SoundPath = string.Empty;
                                setting.SoundType = soundType;
                                soundSettings.Add(setting);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(TraceEventType.Error, "ExchangeData.LoadSettingsParameter Error:\r\n" + ex.ToString());
            }
            settingsParameter.DealingOrderParameter = dealingOrderParameter;
            settingsParameter.SetValueSetting = setValueSetting;
            settingsParameter.SoundSettings = soundSettings;
            return settingsParameter;
        }

        public static bool UpdateManagerSettings(Guid settingId,SettingParameterType type, Dictionary<string, object> fieldAndValues)
        {
            string settingDetails = SettingParameterHelper.GetSettingDetailsUpdateXml(settingId, type, fieldAndValues);
            using (SqlConnection sqlConnection = DataAccess.GetInstance().GetSqlConnection())
            {
                SqlCommand sqlCommand = sqlConnection.CreateCommand();
                sqlCommand.CommandText = "dbo.P_SettingsDetailParameter_Upd";
                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.CommandTimeout = 60 * 30;

                SqlParameter parameter = new SqlParameter("@settingDetailsXml", SqlDbType.NText);
                parameter.Value = settingDetails;
                sqlCommand.Parameters.Add(parameter);

                SqlParameter resultParameter = new SqlParameter("@result", SqlDbType.Bit);
                resultParameter.Direction = ParameterDirection.Output;
                sqlCommand.Parameters.Add(resultParameter);

                sqlCommand.ExecuteNonQuery();

                return (bool)sqlCommand.Parameters["@result"].Value;
            }
        }
    }

    public static class SettingParameterHelper
    {
        public static void UpdateSettingValue(this DealingOrderParameter setting, string key, string value)
        {
            try
            {
                switch (key)
                {
                    case FieldSetting.InactiveWaitTime:
                        setting.InactiveWaitTime = int.Parse(value);
                        return;
                    case FieldSetting.EnquiryWaitTime:
                        setting.EnquiryWaitTime = int.Parse(value);
                        return;
                    case FieldSetting.PriceOrderSetting:
                        setting.PriceOrderSetting = int.Parse(value);
                        return;
                    case FieldSetting.DisablePopup:
                        setting.DisablePopup = bool.Parse(value);
                        return;
                    case FieldSetting.AutoConfirm:
                        setting.AutoConfirm = bool.Parse(value);
                        return;
                    case FieldSetting.LimitStopSummaryIsTimeRange:
                        setting.LimitStopSummaryIsTimeRange = bool.Parse(value);
                        return;
                    case FieldSetting.LimitStopSummaryTimeRangeValue:
                        setting.LimitStopSummaryTimeRangeValue = int.Parse(value);
                        return;
                    case FieldSetting.LimitStopSummaryPriceRangeValue:
                        setting.LimitStopSummaryPriceRangeValue = int.Parse(value);
                        return;
                    default: break;
                }
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(TraceEventType.Error, "SettingManagerData.SettingParameterHelper:{0}, Error:\r\n{1}", ex.ToString());
            }
        }

        public static void UpdateSettingValue(this SetValueSetting setting, string key, string value)
        {
            try
            {
                switch (key)
                {
                    case FieldSetting.OriginInactiveTime:
                        setting.OriginInactiveTime = int.Parse(value);
                        return;
                    case FieldSetting.AlertVariation:
                        setting.AlertVariation = int.Parse(value);
                        return;
                    case FieldSetting.NormalWaitTime:
                        setting.NormalWaitTime = int.Parse(value);
                        return;
                    case FieldSetting.AlertWaitTime:
                        setting.AlertWaitTime = int.Parse(value);
                        return;
                    case FieldSetting.MaxDQLot:
                        setting.MaxDQLot = decimal.Parse(value);
                        return;
                    case FieldSetting.MaxOtherLot:
                        setting.MaxOtherLot = decimal.Parse(value);
                        return;
                    case FieldSetting.CancelLmtVariation:
                        setting.CancelLmtVariation = int.Parse(value);
                        return;
                    case FieldSetting.DQQuoteMinLot:
                        setting.DQQuoteMinLot = decimal.Parse(value);
                        return;
                    case FieldSetting.AutoDQMaxLot:
                        setting.AutoDQMaxLot = decimal.Parse(value);
                        return;
                    case FieldSetting.AutoLmtMktMaxLot:
                        setting.AutoLmtMktMaxLot = decimal.Parse(value);
                        return;
                    case FieldSetting.AcceptDQVariation:
                        setting.AcceptDQVariation = int.Parse(value);
                        return;
                    case FieldSetting.AcceptLmtVariation:
                        setting.AcceptLmtVariation = int.Parse(value);
                        return;
                    case FieldSetting.AcceptCloseLmtVariation:
                        setting.AcceptCloseLmtVariation = int.Parse(value);
                        return;
                    case FieldSetting.MaxMinAdjust:
                        setting.MaxMinAdjust = int.Parse(value);
                        return;
                    case FieldSetting.IsBetterPrice:
                        setting.IsBetterPrice = bool.Parse(value);
                        return;
                    case FieldSetting.AutoAcceptMaxLot:
                        setting.AutoAcceptMaxLot = decimal.Parse(value);
                        return;
                    case FieldSetting.AutoCancelMaxLot:
                        setting.AutoCancelMaxLot = decimal.Parse(value);
                        return;
                    case FieldSetting.AllowedNewTradeSides:
                        setting.AllowedNewTradeSides = int.Parse(value);
                        return;
                    case FieldSetting.HitTimes:
                        setting.HitTimes = int.Parse(value);
                        return;
                    case FieldSetting.PenetrationPoint:
                        setting.PenetrationPoint = int.Parse(value);
                        return;
                    case FieldSetting.PriceValidTime:
                        setting.PriceValidTime = int.Parse(value);
                        return;
                    case FieldSetting.AutoDQDelay:
                        setting.AutoDQDelay = int.Parse(value);
                        return;
                    case FieldSetting.HitPriceVariationForSTP:
                        setting.HitPriceVariationForSTP = int.Parse(value);
                        return;
                    default: break;
                }
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(TraceEventType.Error, "SettingManagerData.SettingParameterHelper:{0}, Error:\r\n{1}", ex.ToString());
            }
        }

        public static string GetSettingDetailsUpdateXml(Guid settingId, SettingParameterType type, Dictionary<string, object> fieldAndValues)
        {
            XmlDocument doc = new XmlDocument();
            XmlElement settingDetailRoot = doc.CreateElement("SettingDetails");

            foreach (string parameterKey in fieldAndValues.Keys)
            {
                object parameterValue = fieldAndValues[parameterKey];
                XmlElement settingElement = doc.CreateElement("SettingDetail");
                settingElement.SetAttribute("SettingId", settingId.ToString());
                settingElement.SetAttribute("ParameterKey", parameterKey);
                settingElement.SetAttribute("ParameterValue", parameterValue.ToString());
                settingElement.SetAttribute("SettingParameterType", ((byte)type).ToString());
                settingDetailRoot.AppendChild(settingElement);
            }
            doc.AppendChild(settingDetailRoot);
            return doc.OuterXml;
        }
    }
}
