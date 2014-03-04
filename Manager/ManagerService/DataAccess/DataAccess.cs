using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace ManagerService.DataAccess
{
    public class DataAccess
    {
        private const string ManagerDb = "ManagerDb";
        private static Dictionary<string, DataAccess> DataAccessInstances = new Dictionary<string, DataAccess>();
        public static DataAccess GetInstance(string exchangeCode = null)
        {
            string key = string.IsNullOrEmpty(exchangeCode) ? DataAccess.ManagerDb : exchangeCode;
            DataAccess instance;
            if (!DataAccess.DataAccessInstances.TryGetValue(key, out instance))
            {
                instance = new DataAccess(exchangeCode);
                DataAccess.DataAccessInstances.Add(key, instance);
            }
            return instance;
        }

        private string _ConnectionString;

        private DataAccess(string exchangeCode = null)
        {
            if (string.IsNullOrEmpty(exchangeCode))
            {
                this._ConnectionString = System.Configuration.ConfigurationManager.AppSettings["ManagerDBConnection"];
            }
            else
            {
                ExchangeSystemSetting exchangeSystemSetting = MainService.ManagerSettings.ExchangeSystems.Single(e => e.Code == exchangeCode);
                this._ConnectionString = exchangeSystemSetting.DbConnectionString;
            }
        }

        public SqlConnection GetSqlConnection()
        {
            SqlConnection sqlConnection = new SqlConnection(this._ConnectionString);
            sqlConnection.Open();
            return sqlConnection;
        }

        public object ExecuteScalar(string sql, CommandType commandType, params SqlParameter[] parameters)
        {
            using (SqlConnection connection = this.GetSqlConnection())
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = sql;
                    if (parameters != null)
                    {
                        foreach (SqlParameter parameter in parameters)
                        {
                            command.Parameters.Add(parameter);
                        }
                    }
                    command.CommandType = commandType;
                    return command.ExecuteScalar();
                }
            }
        }

        public int ExecuteNonQuery(string sql, CommandType commandType, params SqlParameter[] parameters)
        {
            using (SqlConnection connection = this.GetSqlConnection())
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = sql;
                    foreach (SqlParameter parameter in parameters)
                    {
                        command.Parameters.Add(parameter);
                    }
                    command.CommandType = commandType;
                    return command.ExecuteNonQuery();
                }
            }
        }

        public void ExecuteReader(string sql, CommandType commandType, Action<SqlDataReader> processData, params SqlParameter[] parameters)
        {
            using (SqlConnection connection = this.GetSqlConnection())
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = sql;
                    foreach (SqlParameter parameter in parameters)
                    {
                        command.Parameters.Add(parameter);
                    }
                    command.CommandType = commandType;
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        processData(reader);
                    }
                }
            }
        }
    }
}
