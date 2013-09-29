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
        public static SqlConnection GetSqlConnection()
        {
            SqlConnection sqlConnection = new SqlConnection(System.Configuration.ConfigurationManager.AppSettings["ManagerDBConnection"]);
            sqlConnection.Open();
            return sqlConnection;
        }

        public static object ExecuteScalar(string sql, CommandType commandType, params SqlParameter[] parameters)
        {
            using (SqlConnection connection = DataAccess.GetSqlConnection())
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = sql;
                    foreach (SqlParameter parameter in parameters)
                    {
                        command.Parameters.Add(parameter);
                    }
                    command.CommandType = commandType;
                    return command.ExecuteScalar();
                }
            }
        }

        public static int ExecuteNonQuery(string sql, CommandType commandType, params SqlParameter[] parameters)
        {
            using (SqlConnection connection = DataAccess.GetSqlConnection())
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

        public static void ExecuteReader(string sql, CommandType commandType, Action<SqlDataReader> processData, params SqlParameter[] parameters)
        {
            using (SqlConnection connection = DataAccess.GetSqlConnection())
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
