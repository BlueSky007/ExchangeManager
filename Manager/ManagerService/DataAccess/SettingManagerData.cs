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

namespace ManagerService.DataAccess
{
    public class SettingManagerData
    {
        public static ObservableCollection<TaskScheduler> InitailizedTaskSchedulers()
        {
            ObservableCollection<TaskScheduler> taskSchedulers = new ObservableCollection<TaskScheduler>();
            string sql = "dbo.GetInitialDataForTaskScheduler";
            DataAccess.GetInstance().ExecuteReader(sql, CommandType.Text, delegate(SqlDataReader reader) {
                while (reader.Read())
                {
                    TaskScheduler taskScheduler = new TaskScheduler();
                    taskScheduler.Id = (Guid)reader["Id"];
                    taskScheduler.Name = (string)reader["Name"];
                    taskScheduler.Description = (string)reader["Description"];
                    taskScheduler.TaskStatus = reader["TaskStatus"].ConvertToEnumValue<TaskStatus>();
                    taskScheduler.TaskType = reader["TaskType"].ConvertToEnumValue<TaskType>();
                    taskScheduler.ActionType = reader["ActionType"].ConvertToEnumValue<ActionType>();
                    taskScheduler.Interval = reader["Interval"] == DBNull.Value ? null : (int?)reader["Interval"];
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
                if (taskScheduler.LastRunTime == null)
                {
                    parameter.Value = DBNull.Value;
                }
                else
                {
                    parameter.Value = taskScheduler.LastRunTime.Value;
                }
                sqlCommand.Parameters.Add(parameter);

                parameter = new SqlParameter("@taskType", SqlDbType.TinyInt);
                parameter.Value = (byte)taskScheduler.TaskType;
                sqlCommand.Parameters.Add(parameter);

                parameter = new SqlParameter("@actionType", SqlDbType.TinyInt);
                parameter.Value = (byte)taskScheduler.ActionType;
                sqlCommand.Parameters.Add(parameter);

                parameter = new SqlParameter("@interval", SqlDbType.Int);
                parameter.Value = taskScheduler.Interval;
                sqlCommand.Parameters.Add(parameter);

                parameter = new SqlParameter("@userId", SqlDbType.UniqueIdentifier);
                parameter.Value = taskScheduler.Creater;
                sqlCommand.Parameters.Add(parameter);

                parameter = new SqlParameter("@timestamp", SqlDbType.DateTime);
                parameter.Value = DateTime.Now;
                sqlCommand.Parameters.Add(parameter);

                parameter = new SqlParameter("@parameterSettingsXml", SqlDbType.NText);
                parameter.Value = parameterSettings;
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

        public static bool UpdateSettingsParameter(TaskScheduler taskScheduler)
        {
            string settingDetails = GetSettingsDetails(taskScheduler);
            using (SqlConnection sqlConnection = DataAccess.GetInstance().GetSqlConnection())
            {
                SqlCommand sqlCommand = sqlConnection.CreateCommand();
                sqlCommand.CommandText = "dbo.P_SettingsDetail_Upd";
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
}
