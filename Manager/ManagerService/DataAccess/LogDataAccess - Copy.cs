using Manager.Common;
using ManagerService.DataAccess;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ManagerService.DataAccess
{
    internal static class LogHelper
    {
        internal delegate void GetLogEntity<T>(T value, SqlDataReader dr);
        internal static List<T> CreateLogEntity<T>(SqlDataReader dr, GetLogEntity<T> getLogEntity)
            where T : new()
        {
            List<T> values = new List<T>();
            while (dr.Read())
            {
                T value = new T();
                getLogEntity(value, dr);
                values.Add(value);
            }
            return values;
        }
    }
    public class LogDataAccess
    {
        private static LogDataAccess _Instance = new LogDataAccess();

        public static LogDataAccess Instance
        {
            get
            {
                return LogDataAccess._Instance;
            }
        }

        private SqlDataReader GetLogReader(DateTime fromDate,DateTime toDate,LogType logType)
        {
            SqlDataReader dr = null;
            try
            {
                using (SqlConnection sqlConnection = DataAccess.GetInstance().GetSqlConnection())
                {
                    SqlCommand command = sqlConnection.CreateCommand();
                    command.CommandText = "dbo.Log_Get";
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@fromDate", fromDate));
                    command.Parameters.Add(new SqlParameter("@toDate", toDate));
                    command.Parameters.Add(new SqlParameter("@logType", (byte)logType));

                    dr = command.ExecuteReader();
                }
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(TraceEventType.Error, "LogDataAccess.GetLogReader:{0}, Error:\r\n{1}", ex.ToString());
            }
            return dr;
        }

        public List<LogQuote> GetQuoteLogData(DateTime fromDate, DateTime toDate,LogType logType)
        {
            List<LogQuote> logQuotes = new List<LogQuote>();
            SqlDataReader dr = this.GetLogReader(fromDate,toDate,logType);
            try
            {
                logQuotes = LogHelper.CreateLogEntity<LogQuote>(dr, GetLogEntity); 
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(TraceEventType.Error, "LogDataAccess.GetQuoteLogData:{0}, Error:\r\n{1}", ex.ToString());
            }
            return logQuotes;
        }

        public List<LogOrder> GetLogOrderData(DateTime fromDate, DateTime toDate, LogType logType)
        {
            List<LogOrder> logOrders = new List<LogOrder>();
            SqlDataReader dr = this.GetLogReader(fromDate, toDate, logType);
            try
            {
                logOrders = LogHelper.CreateLogEntity<LogOrder>(dr, GetLogEntity);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(TraceEventType.Error, "LogDataAccess.GetLogOrderData:{0}, Error:\r\n{1}", ex.ToString());
            }
            return logOrders;
        }


        private static void GetLogEntity(LogQuote logQuote, SqlDataReader dr)
        {
            logQuote.Id = (Guid)dr["Id"];
            logQuote.IP = dr["IP"] == DBNull.Value ? null : (string)dr["IP"];
            logQuote.UserId = (int)dr["UserId"];
            logQuote.UserName = (string)dr["UserName"];
            logQuote.ExchangeCode = dr.GetItemValue<string>("ExchangeCode", null);
            logQuote.Event = (string)dr["Event"];
            logQuote.Timestamp = (DateTime)dr["Timestamp"];
            logQuote.Lot = dr.GetItemValue<decimal>("Lot", 0);
            logQuote.AnswerLot = dr.GetItemValue<decimal>("AnswerLot", 0);
            logQuote.Ask = (string)dr["Ask"];
            logQuote.Bid = (string)dr["Bid"];
            logQuote.IsBuy = (bool)dr["IsBuy"];
            logQuote.CustomerId = (Guid)dr["CustomerId"];
            logQuote.CustomerName = dr.GetItemValue<string>("CustomerName", null);
            logQuote.InstrumentId = (Guid)dr["InstrumentId"];
            logQuote.InstrumentCode = (string)dr["InstrumentCode"];
            logQuote.SendTime = (DateTime)dr["SendTime"];
        }

        private static void GetLogEntity(LogOrder logOrder, SqlDataReader dr)
        {
            logOrder.Id = (Guid)dr["Id"];
            logOrder.IP = dr["IP"] == DBNull.Value ? null : (string)dr["IP"];
            logOrder.UserId = (int)dr["UserId"];
            logOrder.UserName = (string)dr["UserName"];
            logOrder.ExchangeCode = dr.GetItemValue<string>("ExchangeCode", null);
            logOrder.Event = (string)dr["Event"];
            logOrder.Timestamp = (DateTime)dr["Timestamp"];
        }
    }
}
