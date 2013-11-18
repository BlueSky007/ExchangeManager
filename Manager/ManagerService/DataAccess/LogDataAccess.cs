using Manager.Common;
using ManagerService.Console;
using ManagerService.DataAccess;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net;
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
            dr.Close();
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

        private SqlDataReader GetLogReader(DateTime fromDate, DateTime toDate, LogType logType)
        {
            SqlDataReader dr = null;
            try
            {
                SqlConnection sqlConnection = DataAccess.GetInstance().GetSqlConnection();
                SqlCommand command = sqlConnection.CreateCommand();
                command.CommandText = "dbo.Log_Get";
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@fromDate", fromDate));
                command.Parameters.Add(new SqlParameter("@toDate", toDate));
                command.Parameters.Add(new SqlParameter("@logType", (byte)logType));
                dr = command.ExecuteReader();
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(TraceEventType.Error, "LogDataAccess.GetLogReader:{0}, Error:\r\n{1}", ex.ToString());
            }
            return dr;
        }

        public List<LogQuote> GetQuoteLogData(DateTime fromDate, DateTime toDate, LogType logType)
        {
            List<LogQuote> logQuotes = new List<LogQuote>();
            SqlDataReader dr = this.GetLogReader(fromDate, toDate, logType);
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

        public List<LogPrice> GetLogPriceData(DateTime fromDate, DateTime toDate, LogType logType)
        {
            List<LogPrice> logPircies = new List<LogPrice>();
            SqlDataReader dr = this.GetLogReader(fromDate, toDate, logType);
            try
            {
                logPircies = LogHelper.CreateLogEntity<LogPrice>(dr, GetLogEntity);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(TraceEventType.Error, "LogDataAccess.GetLogPriceData:{0}, Error:\r\n{1}", ex.ToString());
            }
            return logPircies;
        }

        public List<LogSourceChange> GetLogSourceChangeData(DateTime fromDate, DateTime toDate, LogType logType)
        {
            List<LogSourceChange> logSourceChanges = new List<LogSourceChange>();
            SqlDataReader dr = this.GetLogReader(fromDate, toDate, logType);
            try
            {
                logSourceChanges = LogHelper.CreateLogEntity<LogSourceChange>(dr, GetLogEntity);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(TraceEventType.Error, "LogDataAccess.GetLogPriceData:{0}, Error:\r\n{1}", ex.ToString());
            }
            return logSourceChanges;
        }
        

        private static void GetLogEntity(LogQuote logQuote, SqlDataReader dr)
        {
            logQuote.Id = (Guid)dr["Id"];
            logQuote.IP = dr["IP"] == DBNull.Value ? null : (string)dr["IP"];
            logQuote.UserId = (Guid)dr["UserId"];
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
            logOrder.UserId = (Guid)dr["UserId"];
            logOrder.UserName = (string)dr["UserName"];
            logOrder.ExchangeCode = dr.GetItemValue<string>("ExchangeCode", null);
            logOrder.Event = (string)dr["Event"];
            logOrder.Timestamp = (DateTime)dr["Timestamp"];

            logOrder.OperationType = dr["OperationType"].ConvertToEnumValue<OperationType>();
            logOrder.OrderId = (Guid)dr["OrderId"];
            logOrder.OrderCode = (string)dr["OrderCode"];
            logOrder.AccountCode = (string)dr["AccountCode"];
            logOrder.InstrumentCode = (string)dr["InstrumentCode"];
            logOrder.IsBuy = (bool)dr["IsBuy"];
            logOrder.IsOpen = (bool)dr["IsOpen"];
            logOrder.Lot = (decimal)dr["Lot"];
            logOrder.SetPrice = (string)dr["SetPrice"];
            logOrder.OrderType = (OrderType)dr["OrderTypeId"];
            logOrder.OrderRelation = (string)dr["OrderRelation"];
            logOrder.TransactionCode = (string)dr["TransactionCode"];
        }

        private static void GetLogEntity(LogPrice logPrice, SqlDataReader dr)
        {
            logPrice.Id = (Guid)dr["Id"];
            logPrice.IP = dr["IP"] == DBNull.Value ? null : (string)dr["IP"];
            logPrice.UserId = (Guid)dr["UserId"];
            logPrice.UserName = (string)dr["UserName"];
            logPrice.ExchangeCode = dr.GetItemValue<string>("ExchangeCode", null);
            logPrice.Event = (string)dr["Event"];
            logPrice.Timestamp = (DateTime)dr["Timestamp"];

            logPrice.InstrumentId = (int)dr["InstrumentId"];
            logPrice.InstrumentCode = (string)dr["InstrumentCode"];
            logPrice.OperationType = dr["OperationType"].ConvertToEnumValue<OperationType>();// (OperationType)dr["OperationType"];
            logPrice.Price = (string)dr["Price"];
            logPrice.Diff = (string)dr["Diff"];
        }

        private static void GetLogEntity(LogSourceChange logSourceChange, SqlDataReader dr)
        {
            logSourceChange.Id = (Guid)dr["Id"];
            logSourceChange.IP = dr["IP"] == DBNull.Value ? null : (string)dr["IP"];
            logSourceChange.UserId = (Guid)dr["UserId"];
            logSourceChange.UserName = (string)dr["UserName"];
            logSourceChange.ExchangeCode = dr.GetItemValue<string>("ExchangeCode", null);
            logSourceChange.Event = (string)dr["Event"];
            logSourceChange.Timestamp = (DateTime)dr["Timestamp"];

            logSourceChange.IsDefault = (bool)dr["IsDefault"];
            logSourceChange.FromSourceId = (int)dr["FromSourceId"];
            logSourceChange.FromSourceName = (string)dr["FromSourceName"];
            logSourceChange.ToSourceId = (int)dr["ToSourceId"];
            logSourceChange.ToSourceName = (string)dr["ToSourceName"];
            logSourceChange.Priority = (byte)dr["Priority"];
        }
        
    }

    public class WriteLogManager
    {
        public static void WriteQuotePriceLog(Answer answer, Client client, string eventType)
        {
            LogQuote logQuote = new LogQuote();

            string hostname = Dns.GetHostName();
            IPHostEntry localhost = Dns.GetHostEntry(hostname);
            IPAddress localaddr = localhost.AddressList[0];
            logQuote.IP = localaddr.ToString();

            logQuote.UserId = client.userId;
            logQuote.UserName = client.user.UserName;
            logQuote.Event = eventType;
            logQuote.Timestamp = DateTime.Now;

            logQuote.AnswerLot = answer.AnswerLot;
            logQuote.Ask = answer.Ask;
            logQuote.Bid = answer.Bid;
            logQuote.CustomerId = answer.CustomerId;
            logQuote.CustomerName = answer.CustomerCode;
            logQuote.InstrumentId = answer.InstrumentId;
            logQuote.InstrumentCode = answer.InstrumentCode;
            logQuote.ExchangeCode = answer.ExchangeCode;
            logQuote.Lot = answer.QuoteLot;
            logQuote.SendTime = answer.SendTime;

            WriteLogManager.WriteQuotePriceLog(logQuote);
        }

        public static void WriteQuotePriceLog(LogQuote logEntity)
        {
            try
            {
                using (SqlConnection sqlConnection = DataAccess.GetInstance().GetSqlConnection())
                {
                    SqlCommand command = sqlConnection.CreateCommand();
                    command.CommandText = "dbo.P_LogQuote_Ins";
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@id", logEntity.Id));
                    command.Parameters.Add(new SqlParameter("@userId", logEntity.UserId));
                    command.Parameters.Add(new SqlParameter("@ip", logEntity.IP));
                    command.Parameters.Add(new SqlParameter("@exchangeCode", logEntity.ExchangeCode));
                    command.Parameters.Add(new SqlParameter("@event", logEntity.Event));
                    command.Parameters.Add(new SqlParameter("@timestamp", DateTime.Now));
                    command.Parameters.Add(new SqlParameter("@lot", logEntity.Lot));
                    command.Parameters.Add(new SqlParameter("@answerLot", logEntity.AnswerLot));
                    command.Parameters.Add(new SqlParameter("@ask", logEntity.Ask));
                    command.Parameters.Add(new SqlParameter("@bid", logEntity.Bid));
                    command.Parameters.Add(new SqlParameter("@isBuy", logEntity.IsBuy));
                    command.Parameters.Add(new SqlParameter("@customerId", logEntity.CustomerId));
                    command.Parameters.Add(new SqlParameter("@customerName", logEntity.CustomerName));
                    command.Parameters.Add(new SqlParameter("@instrumentId", logEntity.InstrumentId));
                    command.Parameters.Add(new SqlParameter("@instrumentCode", logEntity.InstrumentCode));
                    command.Parameters.Add(new SqlParameter("@sendTime", logEntity.SendTime));
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(TraceEventType.Error, "WriteLogManager.WriteQuotePriceLog:{0}, Error:\r\n{1}", ex.ToString());
            }
        }

        public static void WriteQuoteOrderLog(LogOrder logEntity)
        {
            try
            {
                using (SqlConnection sqlConnection = DataAccess.GetInstance().GetSqlConnection())
                {
                    SqlCommand command = sqlConnection.CreateCommand();
                    command.CommandText = "dbo.P_LogOrder_Ins";
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@id", logEntity.Id));
                    command.Parameters.Add(new SqlParameter("@userId", logEntity.UserId));
                    command.Parameters.Add(new SqlParameter("@ip", logEntity.IP));
                    command.Parameters.Add(new SqlParameter("@exchangeCode", logEntity.ExchangeCode));
                    command.Parameters.Add(new SqlParameter("@event", logEntity.Event));
                    command.Parameters.Add(new SqlParameter("@timestamp", DateTime.Now));
                    command.Parameters.Add(new SqlParameter("@operationType", (Byte)logEntity.OperationType));
                    command.Parameters.Add(new SqlParameter("@orderId", logEntity.OrderId));
                    command.Parameters.Add(new SqlParameter("@orderCode", logEntity.OrderCode));
                    command.Parameters.Add(new SqlParameter("@accountCode", logEntity.AccountCode));
                    command.Parameters.Add(new SqlParameter("@instrumentCode", logEntity.InstrumentCode));
                    command.Parameters.Add(new SqlParameter("@isBuy", logEntity.IsBuy));
                    command.Parameters.Add(new SqlParameter("@isOpen", logEntity.IsOpen));
                    command.Parameters.Add(new SqlParameter("@lot", logEntity.Lot));
                    command.Parameters.Add(new SqlParameter("@setPrice", logEntity.SetPrice));
                    command.Parameters.Add(new SqlParameter("@orderTypeId", (int)logEntity.OrderType));
                    command.Parameters.Add(new SqlParameter("@orderRelation", logEntity.OrderRelation));
                    command.Parameters.Add(new SqlParameter("@transactionCode", logEntity.TransactionCode));
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(TraceEventType.Error, "WriteLogManager.WriteQuoteOrderLog:{0}, Error:\r\n{1}", ex.ToString());
            }
        }




    }
}
