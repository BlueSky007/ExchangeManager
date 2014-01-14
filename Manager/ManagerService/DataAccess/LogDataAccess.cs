using iExchange.Common;
using iExchange.Common.Manager;
using Manager.Common;
using Manager.Common.LogEntities;
using Manager.Common.QuotationEntities;
using ManagerService.Console;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;

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
                Logger.TraceEvent(TraceEventType.Error, "LogDataAccess.GetLogReader:\r\n{0}", ex.ToString());
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
                Logger.TraceEvent(TraceEventType.Error, "LogDataAccess.GetQuoteLogData:\r\n{0}", ex.ToString());
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
                Logger.TraceEvent(TraceEventType.Error, "LogDataAccess.GetLogOrderData:\r\n{0}", ex.ToString());
            }
            return logOrders;
        }

        public List<LogSetting> GetLogSettingData(DateTime fromDate, DateTime toDate, LogType logType)
        {
            List<LogSetting> logSettings = new List<LogSetting>();
            SqlDataReader dr = this.GetLogReader(fromDate, toDate, logType);
            try
            {
                logSettings = LogHelper.CreateLogEntity<LogSetting>(dr, GetLogEntity);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(TraceEventType.Error, "LogDataAccess.GetLogSettingData:\r\n{0}", ex.ToString());
            }
            return logSettings;
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
                Logger.TraceEvent(TraceEventType.Error, "LogDataAccess.GetLogPriceData:\r\n{0}", ex.ToString());
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
                Logger.TraceEvent(TraceEventType.Error, "LogDataAccess.GetLogPriceData:\r\n{0}", ex.ToString());
            }
            return logSourceChanges;
        }


        private static void GetLogEntity(LogQuote entity, SqlDataReader dr)
        {
            entity.Id = (Guid)dr["Id"];
            entity.IP = dr["IP"] == DBNull.Value ? null : (string)dr["IP"];
            entity.UserId = (Guid)dr["UserId"];
            entity.UserName = (string)dr["UserName"];
            entity.ExchangeCode = dr.GetItemValue<string>("ExchangeCode", null);
            entity.Event = (string)dr["Event"];
            entity.Timestamp = (DateTime)dr["Timestamp"];
            entity.Lot = dr.GetItemValue<decimal>("Lot", 0);
            entity.AnswerLot = dr.GetItemValue<decimal>("AnswerLot", 0);
            entity.Ask = (string)dr["Ask"];
            entity.Bid = (string)dr["Bid"];
            entity.IsBuy = (bool)dr["IsBuy"];
            entity.CustomerId = (Guid)dr["CustomerId"];
            entity.CustomerName = dr.GetItemValue<string>("CustomerName", null);
            entity.InstrumentId = (Guid)dr["InstrumentId"];
            entity.InstrumentCode = (string)dr["InstrumentCode"];
            entity.SendTime = (DateTime)dr["SendTime"];
        }

        private static void GetLogEntity(LogOrder entity, SqlDataReader dr)
        {
            entity.Id = (Guid)dr["Id"];
            entity.IP = dr["IP"] == DBNull.Value ? null : (string)dr["IP"];
            entity.UserId = (Guid)dr["UserId"];
            entity.UserName = (string)dr["UserName"];
            entity.ExchangeCode = dr.GetItemValue<string>("ExchangeCode", null);
            entity.Event = (string)dr["Event"];
            entity.Timestamp = (DateTime)dr["Timestamp"];

            entity.OperationType = dr["OperationType"].ConvertToEnumValue<OperationType>();
            entity.OrderId = (Guid)dr["OrderId"];
            entity.OrderCode = (string)dr["OrderCode"];
            entity.AccountCode = (string)dr["AccountCode"];
            entity.InstrumentCode = (string)dr["InstrumentCode"];
            entity.IsBuy = (bool)dr["IsBuy"];
            entity.IsOpen = (bool)dr["IsOpen"];
            entity.Lot = (decimal)dr["Lot"];
            entity.SetPrice = (string)dr["SetPrice"];
            entity.OrderType = (OrderType)dr["OrderTypeId"];
            entity.OrderRelation = (string)dr["OrderRelation"];
            entity.TransactionCode = (string)dr["TransactionCode"];
        }

        private static void GetLogEntity(LogSetting entity, SqlDataReader dr)
        {
            entity.Id = (Guid)dr["Id"];
            entity.IP = dr["IP"] == DBNull.Value ? null : (string)dr["IP"];
            entity.UserId = (Guid)dr["UserId"];
            entity.UserName = (string)dr["UserName"];
            entity.ExchangeCode = dr.GetItemValue<string>("ExchangeCode", null);
            entity.Event = (string)dr["Event"];
            entity.Timestamp = (DateTime)dr["Timestamp"];

            entity.ParameterName = dr.GetItemValue<string>("ParameterName", null);
            entity.TableName = dr.GetItemValue<string>("TableName", null);
            entity.OldValue = dr.GetItemValue<string>("OldValue", null);
            entity.NewValue = dr.GetItemValue<string>("NewValue", null);
        }

        private static void GetLogEntity(LogPrice entity, SqlDataReader dr)
        {
            entity.Id = (Guid)dr["Id"];
            entity.IP = dr["IP"] == DBNull.Value ? null : (string)dr["IP"];
            entity.UserId = (Guid)dr["UserId"];
            entity.UserName = (string)dr["UserName"];
            entity.ExchangeCode = dr.GetItemValue<string>("ExchangeCode", null);
            entity.Event = (string)dr["Event"];
            entity.Timestamp = (DateTime)dr["Timestamp"];

            entity.InstrumentId = (int)dr["InstrumentId"];
            entity.InstrumentCode = (string)dr["InstrumentCode"];
            entity.OperationType = (PriceOperationType)(byte)dr["OperationType"];
            if (dr["OperationType"] != DBNull.Value) entity.OutOfRangeType = (OutOfRangeType)(byte)dr["OperationType"];
            entity.Bid = (string)dr["Bid"];
            entity.Ask = (string)dr["Ask"];
            entity.Diff = (string)dr["Diff"];
        }

        private static void GetLogEntity(LogSourceChange entity, SqlDataReader dr)
        {
            entity.Id = (Guid)dr["Id"];
            entity.IP = dr["IP"] == DBNull.Value ? null : (string)dr["IP"];
            entity.UserId = (Guid)dr["UserId"];
            entity.UserName = (string)dr["UserName"];
            entity.ExchangeCode = dr.GetItemValue<string>("ExchangeCode", null);
            entity.Event = (string)dr["Event"];
            entity.Timestamp = (DateTime)dr["Timestamp"];

            entity.IsDefault = (bool)dr["IsDefault"];
            entity.FromSourceId = (int)dr["FromSourceId"];
            entity.FromSourceName = (string)dr["FromSourceName"];
            entity.ToSourceId = (int)dr["ToSourceId"];
            entity.ToSourceName = (string)dr["ToSourceName"];
            entity.Priority = (byte)dr["Priority"];
        }
    }

    public class WriteLogManager
    {
        public static void WriteQuotePriceLog(Answer answer, Client client, string eventType)
        {
            LogQuote logQuote = new LogQuote();
            logQuote.IP = client.IP;
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
                Logger.TraceEvent(TraceEventType.Error, "WriteLogManager.WriteQuotePriceLog\r\n{0}", ex.ToString());
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
                Logger.TraceEvent(TraceEventType.Error, "WriteLogManager.WriteQuoteOrderLog\r\n{0}", ex.ToString());
            }
        }

        public static void WriteSettingChangeLog(LogSetting logEntity)
        {
            try
            {
                using (SqlConnection sqlConnection = DataAccess.GetInstance().GetSqlConnection())
                {
                    SqlCommand command = sqlConnection.CreateCommand();
                    command.CommandText = "P_LogSetting_Ins";
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@id", logEntity.Id));
                    command.Parameters.Add(new SqlParameter("@userId", logEntity.UserId));
                    command.Parameters.Add(new SqlParameter("@ip", logEntity.IP));
                    command.Parameters.Add(new SqlParameter("@exchangeCode", logEntity.ExchangeCode));
                    command.Parameters.Add(new SqlParameter("@event", logEntity.Event));
                    command.Parameters.Add(new SqlParameter("@timestamp", DateTime.Now));
                    command.Parameters.Add(new SqlParameter("@parameterName", logEntity.ParameterName));
                    command.Parameters.Add(new SqlParameter("@tableName", logEntity.TableName));
                    command.Parameters.Add(new SqlParameter("@oldValue", logEntity.OldValue));
                    command.Parameters.Add(new SqlParameter("@newValue", logEntity.NewValue));
     
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(TraceEventType.Error, "WriteLogManager.WriteSettingChangeLog\r\n{0}", ex.ToString());
            }
        }

        public static void WritePriceLog(LogPrice logEntity)
        {
            try
            {
                using (SqlConnection sqlConnection = DataAccess.GetInstance().GetSqlConnection())
                {
                    SqlCommand command = sqlConnection.CreateCommand();
                    command.CommandText = "P_LogPrice_Ins";
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@id", logEntity.Id));
                    command.Parameters.Add(new SqlParameter("@userId", logEntity.UserId));
                    command.Parameters.Add(new SqlParameter("@ip", logEntity.IP));
                    command.Parameters.Add(new SqlParameter("@exchangeCode", logEntity.ExchangeCode));
                    command.Parameters.Add(new SqlParameter("@event", logEntity.Event));
                    command.Parameters.Add(new SqlParameter("@timestamp", DateTime.Now));
                    command.Parameters.Add(new SqlParameter("@instrumentId", logEntity.InstrumentId));
                    command.Parameters.Add(new SqlParameter("@instrumentCode", logEntity.InstrumentCode));
                    command.Parameters.Add(new SqlParameter("@operationType", (byte)logEntity.OperationType));
                    if (logEntity.OutOfRangeType.HasValue) command.Parameters.Add(new SqlParameter("@outOfRangeType", (byte)logEntity.OutOfRangeType));
                    if (logEntity.Ask != null) command.Parameters.Add(new SqlParameter("@ask", logEntity.Ask));
                    if (logEntity.Bid != null) command.Parameters.Add(new SqlParameter("@bid", logEntity.Bid));
                    if (logEntity.Diff != null) command.Parameters.Add(new SqlParameter("@diff", logEntity.Diff));

                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(TraceEventType.Error, "WriteLogManager.WritePriceLog\r\n{0}", ex.ToString());
            }
        }
    
        public static void WriteSourceChangeLog(LogSourceChange logEntity)
        {
            try
            {
                using (SqlConnection sqlConnection = DataAccess.GetInstance().GetSqlConnection())
                {
                    SqlCommand command = sqlConnection.CreateCommand();
                    command.CommandText = "P_LogSourceChange";
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@id", logEntity.Id));
                    command.Parameters.Add(new SqlParameter("@userId", logEntity.UserId));
                    command.Parameters.Add(new SqlParameter("@ip", logEntity.IP));
                    command.Parameters.Add(new SqlParameter("@exchangeCode", logEntity.ExchangeCode));
                    command.Parameters.Add(new SqlParameter("@event", logEntity.Event));
                    command.Parameters.Add(new SqlParameter("@timestamp", DateTime.Now));
                    command.Parameters.Add(new SqlParameter("@fromSourceId", logEntity.FromSourceId));
                    command.Parameters.Add(new SqlParameter("@toSourceId", logEntity.ToSourceId));

                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(TraceEventType.Error, "WriteLogManager.WriteSourceChangeLog\r\n{0}", ex.ToString());
            }
        }
    }
}
