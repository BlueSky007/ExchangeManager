﻿using iExchange.Common;
using Manager.Common;
using Manager.Common.ExchangeEntities;
using Manager.Common.ReportEntities;
using Manager.Common.Settings;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ManagerService.DataAccess
{
    public class ExchangeData
    {
        public static List<Guid> GetMemberIds(string exchangeCode, List<DataPermission> permissions, DataObjectType GroupType)
        {
            DataTable groupPermission = new DataTable();
            groupPermission.Columns.Add("GroupId", typeof(Guid));
            groupPermission.Columns.Add("Status", typeof(bool));
            foreach (DataPermission item in permissions)
            {
                if (item.DataObjectId != null && item.DataObjectType == GroupType)
                {
                    DataRow row = groupPermission.NewRow();
                    row["GroupId"] = item.DataObjectId;
                    row["Status"] = item.IsAllow;
                    groupPermission.Rows.Add(row);
                }
            }

            bool deafultStatus = false;
            DataPermission typePermission = permissions.SingleOrDefault(d => d.ExchangeSystemCode == exchangeCode && d.DataObjectType == GroupType && d.DataObjectId == null);
            if (typePermission != null)
            {
                deafultStatus = typePermission.IsAllow;
            }
            else
            {
                DataPermission exchangePermission = permissions.SingleOrDefault(d => d.ExchangeSystemCode == exchangeCode && d.DataObjectType == DataObjectType.Exchange);
                if (exchangePermission != null)
                {
                    deafultStatus = exchangePermission.IsAllow;
                }
            }
            List<Guid> memberIds = new List<Guid>();
            using (SqlConnection con = DataAccess.GetInstance(exchangeCode).GetSqlConnection())
            {
                using (SqlCommand command = con.CreateCommand())
                {
                    command.CommandText = "[dbo].[Manager_GetMemberIds]";
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@GroupType", GroupType.ToString()));
                    command.Parameters.Add(new SqlParameter("@deafultStatus", deafultStatus));
                    SqlParameter tableParameter = command.Parameters.AddWithValue("@dataPermissions", groupPermission);
                    tableParameter.SqlDbType = SqlDbType.Structured;
                    tableParameter.TypeName = "[dbo].[DataPermissions]";
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Guid memberId = (Guid)reader["MemberID"];
                            memberIds.Add(memberId);
                        }
                    }
                }
            }
            return memberIds;
        }

        public static List<Guid> GetNewMemberIds(string exchangeCode, bool deafultStatus, List<RoleDataPermission> permissions, DataObjectType GroupType)
        {
            List<Guid> memberIds = new List<Guid>();
            DataTable groupPermission = new DataTable();
            groupPermission.Columns.Add("GroupId", typeof(Guid));
            groupPermission.Columns.Add("Status", typeof(bool));
            foreach (RoleDataPermission item in permissions)
            {
                if (item.Level == 3)
                {
                    DataRow row = groupPermission.NewRow();
                    row["GroupId"] = item.DataObjectId;
                    row["Status"] = item.IsAllow;
                    groupPermission.Rows.Add(row);
                }
            }
            using (SqlConnection con = DataAccess.GetInstance(exchangeCode).GetSqlConnection())
            {
                using (SqlCommand command = con.CreateCommand())
                {
                    command.CommandText = "[dbo].[Manager_GetMemberIds]";
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@GroupType", GroupType.ToString()));
                    command.Parameters.Add(new SqlParameter("@deafultStatus", deafultStatus));
                    SqlParameter tableParameter = command.Parameters.AddWithValue("@dataPermissions", groupPermission);
                    tableParameter.SqlDbType = SqlDbType.Structured;
                    tableParameter.TypeName = "[dbo].[DataPermissions]";
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Guid memberId = (Guid)reader["MemberID"];
                            memberIds.Add(memberId);
                        }
                    }
                }
            }
            return memberIds;
        }

        public static List<RoleDataPermission> GetAllDataPermissions(string exchangeCode)
        {
            List<RoleDataPermission> dataPermissions = new List<RoleDataPermission>();
            string sql = "SELECT g.ID,g.Code,g.GroupType FROM dbo.[Group] g WHERE g.GroupType='Account' OR g.GroupType='Instrument'";
            DataAccess.GetInstance(exchangeCode).ExecuteReader(sql, CommandType.Text, delegate(SqlDataReader reader)
            {
                while (reader.Read())
                {
                    RoleDataPermission data = new RoleDataPermission();
                    data.ExchangeCode = exchangeCode;
                    data.Type = (DataObjectType)Enum.Parse(typeof(DataObjectType), reader["GroupType"].ToString());
                    data.DataObjectId = (Guid)reader["ID"];
                    data.Code = reader["Code"].ToString();
                    dataPermissions.Add(data);
                }
            });
            return dataPermissions;
        }

        public static AccountStatusQueryResult GetAccountReportData(string exchangeCode, Guid accountId, string selectedPrice, string accountXml, HashSet<Guid> instrumentIds)
        {
            string instrumentXmlString = GetInstrumentPermisstionString(instrumentIds);
            AccountStatusQueryResult queryResult = new AccountStatusQueryResult();
            string sql = string.Format("exec dbo.P_ReportAccountStatusAllDataForManager @accountId='{0}',@xmlAccounts='{1}',@xmlInstruments='{2}',@queryType='{3}'", accountId, accountXml, instrumentXmlString, selectedPrice);
            DataAccess.GetInstance(exchangeCode).ExecuteReader(sql, CommandType.Text, delegate(SqlDataReader reader)
            {
                while (reader.Read()) 
                {
                    decimal uncleared = Decimal.Zero;
                    Decimal.TryParse(reader["Uncleared"].ToString(), out uncleared);
                    queryResult.AccountStatusEntity.Uncleared = uncleared;
                }
                reader.NextResult();
                while (reader.Read())
                {
                    AccountReportDataHelper.ConvertReportEntity(queryResult.AccountStatusEntity, reader);
                }
                reader.NextResult();
                while (reader.Read())
                {
                    queryResult.AccountStatusEntity.TradeDay = (string)reader["TradeDay"];
                }
                reader.NextResult();
                int i = 0;
                while (reader.Read())
                {
                    if (i < 1)
                    {
                        AccountReportDataHelper.ConvertReportEntity(queryResult.AccountTradingSummary, queryResult.AccountCurrencies, reader);
                    }
                    else
                    {
                        AccountReportDataHelper.ConvertReportEntity(null, queryResult.AccountCurrencies, reader);
                    }
                    i++;
                }
                reader.NextResult();
                while (reader.Read())
                {
                    if (reader["OverNightNecessary"] == DBNull.Value)
                    {
                        queryResult.AccountStatusEntity.OverNightNecessary = decimal.Zero;
                    }
                    else
                    {
                        queryResult.AccountStatusEntity.OverNightNecessary = (decimal)(double)reader["OverNightNecessary"];
                    }
                }
                reader.NextResult();
                while (reader.Read()) 
                {
                    AccountReportDataHelper.ConvertReportEntity(queryResult.AccountHedgingLevel, reader);
                }
                reader.NextResult();
                while (reader.Read())
                {
                    queryResult.AccountStatusEntity.AccDeposit = (decimal)reader["AccDeposit"];
                    queryResult.AccountTradingSummary.Deposit = queryResult.AccountStatusEntity.AccDeposit;
                }
                reader.NextResult();
                while (reader.Read())
                {
                    queryResult.AccountStatusEntity.AccAdjustment = (decimal)reader["AccAdjustment"];
                    queryResult.AccountTradingSummary.Adjustment = queryResult.AccountStatusEntity.AccAdjustment;
                }
                reader.NextResult();
                //Order Data
                List<AccountStatusOrder> accountOpenLists = new List<AccountStatusOrder>();
                while (reader.Read())
                {
                    AccountReportDataHelper.ConvertReportEntity(accountOpenLists, reader);
                }
                queryResult.AccountOpenPostions = accountOpenLists;
                reader.NextResult();
                while (reader.Read())
                {
                    AccountReportDataHelper.ConvertReportEntity(accountOpenLists, reader);
                }
                reader.NextResult();
                while (reader.Read())
                {
                    
                }
                reader.NextResult();
            });
            
            return queryResult;
        }

        public static List<InstrumentForFloatingPLCalc> GetInstrumentForFloatingPLCalc(string exchangeCode,HashSet<Guid> accountIds,HashSet<Guid> instrumentIds)
        {
            string instrumentXmlString = GetInstrumentPermisstionString(instrumentIds);
            string accountXmlString = GetAccountPermisstionString(accountIds);
            List<InstrumentForFloatingPLCalc> instrumentForFloatingPLCalcList = new List<InstrumentForFloatingPLCalc>();
            string sql = string.Format("exec dbo.P_RptGetInstrumentForPLCalcForManager @xmlAccounts='{0}',@xmlInstruments='{1}'", accountXmlString, instrumentXmlString);
            DataAccess.GetInstance(exchangeCode).ExecuteReader(sql, CommandType.Text, delegate(SqlDataReader reader)
            {
                while (reader.Read())
                {
                    InstrumentForFloatingPLCalc entity = new InstrumentForFloatingPLCalc();
                    AccountReportDataHelper.ConvertReportEntity(entity, reader);
                    instrumentForFloatingPLCalcList.Add(entity);
                }
            });
            
            return instrumentForFloatingPLCalcList;
        }

        public static void UpdateInstrumentForFloatingPLCalc(string exchangeCode,Guid instrumentId,string bid, int spreadPoint)
        {
            DataAccess.GetInstance(exchangeCode).ExecuteNonQuery("dbo.P_RptInstrumentForFPLCalc_Upd", CommandType.StoredProcedure,
               new SqlParameter("@InstrumentID", instrumentId),
               new SqlParameter("@Bid", bid),
               new SqlParameter("@SpreadPoints", spreadPoint));
        }

        public static ExchangeInitializeData GetInitData(string exchangeCode, Guid userId, List<DataPermission> permissions,
            bool accountDeafultStatus, bool instrumentDeafuleStatus, out BlockingCollection<Guid> validAccounts, out BlockingCollection<Guid> validInstruments)
        {
            ExchangeInitializeData initializeData = new ExchangeInitializeData();
            validAccounts = new BlockingCollection<Guid>();
            validInstruments = new BlockingCollection<Guid>();

            DataTable groupPermission = new DataTable();
            groupPermission.Columns.Add("GroupId", typeof(Guid));
            groupPermission.Columns.Add("GroupType", typeof(string));
            groupPermission.Columns.Add("Status", typeof(bool));
            foreach (DataPermission item in permissions)
            {
                if (item.DataObjectId != null)
                {
                    DataRow row = groupPermission.NewRow();
                    row["GroupId"] = item.DataObjectId;
                    row["GroupType"] = item.DataObjectType;
                    row["Status"] = item.IsAllow;
                    groupPermission.Rows.Add(row);
                }
            }

            string sql = "dbo.GetInitialDataForManager";
            SqlParameter[] parameters = new SqlParameter[3];
            SqlParameter accountParameter = new SqlParameter("@accountDeafultStatus", accountDeafultStatus);
            SqlParameter instrumentParameter = new SqlParameter("@instrumentDeafuleStatus", instrumentDeafuleStatus);
            SqlParameter tableParameter = new SqlParameter("@dataPermissions", groupPermission);
            tableParameter.SqlDbType = SqlDbType.Structured;
            tableParameter.TypeName = "[dbo].[DataPermissions]";


            parameters[0] = accountParameter;
            parameters[1] = instrumentParameter;
            parameters[2] = tableParameter;

            using(SqlConnection connection = DataAccess.GetInstance(exchangeCode).GetSqlConnection())
            {
                using(SqlCommand command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = sql;
                    foreach (SqlParameter parameter in parameters)
                    {
                        command.Parameters.Add(parameter);
                    }
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        try
                        {
                            while (reader.Read())
                            {
                                validAccounts.Add((Guid)reader["ID"]);
                            }
                            reader.NextResult();
                            while (reader.Read())
                            {
                                validInstruments.Add((Guid)reader["ID"]);
                            }
                            reader.NextResult();

                            initializeData.SettingSet = new SettingSet();
                            initializeData.SettingSet.Initialize(reader);
                        }
                        catch (Exception ex)
                        {
                            Logger.TraceEvent(TraceEventType.Error, "GetInitData Error:\r\n{0}", ex.ToString());
                        }
                    }
                }
            }
            return initializeData;
        }

        public static List<Tuple<Guid, bool, bool>> GetInstrumentPriceEnableStates(string exchangeCode, string instrumentCode)
        {

            List<Tuple<Guid, bool, bool>> result = new List<Tuple<Guid, bool, bool>>();
            string sql = "SELECT ID,IsPriceEnabled,IsAutoEnablePrice FROM Instrument WHERE OriginCode=@originCode";
            DataAccess.GetInstance(exchangeCode).ExecuteReader(sql, CommandType.Text, delegate(SqlDataReader reader)
            {
                while (reader.Read())
                {
                    result.Add(new Tuple<Guid, bool, bool>((Guid)reader["ID"], (bool)reader["IsPriceEnabled"], (bool)reader["IsAutoEnablePrice"]));
                }
            }, new SqlParameter("@originCode", instrumentCode));
            return result;
        }

        public static List<OrderQueryEntity> GetOrderByInstrument(string exchangeCode, HashSet<Guid> accounts, HashSet<Guid> instruments,
                        Guid instrumentId, Guid accountGroupId, OrderType orderType, bool isExecute, DateTime fromDate, DateTime toDate)
        {
            List<OrderQueryEntity> queryOrders = new List<OrderQueryEntity>();
            string sql = string.Empty;

            try
            {
                string instrumentXmlString = GetInstrumentPermisstionString(instruments);
                string accountXmlString = GetAccountPermisstionString(accounts);
                using (SqlConnection sqlConnection = DataAccess.GetInstance(exchangeCode).GetSqlConnection())
                {
                    SqlCommand command = sqlConnection.CreateCommand();
                    command.CommandText = isExecute ? "dbo.P_GetExecutedOrder" : "dbo.P_GetCancelledOrder";
                    command.CommandType = CommandType.StoredProcedure;

                    SqlParameter parameter = new SqlParameter("@accountsXml", SqlDbType.NText);
                    parameter.Value = accountXmlString;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("@instrumentsXml", SqlDbType.NText);
                    parameter.Value = instrumentXmlString;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("@InstrumentID", SqlDbType.UniqueIdentifier);
                    parameter.Value = instrumentId;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("@OrderType", SqlDbType.Int);
                    parameter.Value = (byte)orderType;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("@FromDate", SqlDbType.DateTime);
                    parameter.Value = fromDate;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("@ToDate", SqlDbType.DateTime);
                    parameter.Value = toDate;
                    command.Parameters.Add(parameter);

                    parameter = new SqlParameter("@AccountGroupId", SqlDbType.UniqueIdentifier);
                    parameter.Value = accountGroupId;
                    command.Parameters.Add(parameter);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            OrderQueryEntity order = new OrderQueryEntity();
                            order.InstrumentCode = (string)reader["InstrumentCode"];
                            order.BuySell = (bool)reader["IsBuy"];
                            order.OpenClose = (string)reader["OpenClose"];
                            order.Lot = (decimal)reader["Lot"];
                            order.OrderCode = (string)reader["OrderCode"];
                            order.AccountCode = (string)reader["AccountCode"];
                            order.SetPrice = (string)reader["ExecutePrice"];
                            order.OrderType = reader["OrderType"].ConvertToEnumValue<OrderType>();
                            if (isExecute)
                            {
                                order.ExecuteTime = (DateTime)reader["ExecuteTime"];
                            }
                            order.Relation = (string)reader["Relation"];
                            //order.Dealer = (Guid)reader["approverID"] ==;
                            order.ExchangeCode = exchangeCode;
                            queryOrders.Add(order);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "ExchangeData.GetOrderByInstrument Error\r\n{0}", ex.ToString());
            }
            return queryOrders;
        }

        public static List<OrderQueryEntity> GetOrderByInstrument(string exchangeCode,Guid userId, Guid instrumentId, Guid accountGroupId, OrderType orderType,
           bool isExecute, DateTime fromDate, DateTime toDate)
        {
            List<OrderQueryEntity> queryOrders = new List<OrderQueryEntity>();
            string sql = string.Empty;

            using (SqlConnection sqlConnection = DataAccess.GetInstance(exchangeCode).GetSqlConnection())
            {
                SqlCommand command = sqlConnection.CreateCommand();
                command.CommandText = isExecute ? "dbo.P_GetExecutedOrderByInstrument" : "dbo.P_GetCancelledOrderByInstrument";
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@UserID", userId));
                command.Parameters.Add(new SqlParameter("@InstrumentID", instrumentId));
                command.Parameters.Add(new SqlParameter("@OrderType", (byte)orderType));
                command.Parameters.Add(new SqlParameter("@FromDate", fromDate));
                command.Parameters.Add(new SqlParameter("@ToDate", toDate));
                command.Parameters.Add(new SqlParameter("@AccountGroupId", accountGroupId));
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        OrderQueryEntity order = new OrderQueryEntity();
                        order.InstrumentCode = (string)reader["InstrumentCode"];
                        order.BuySell = (bool)reader["IsBuy"];
                        order.OpenClose = (string)reader["OpenClose"];
                        order.Lot = (decimal)reader["Lot"];
                        order.OrderCode = (string)reader["OrderCode"];
                        order.AccountCode = (string)reader["AccountCode"];
                        order.SetPrice = (string)reader["ExecutePrice"];
                        order.OrderType = reader["OrderType"].ConvertToEnumValue<OrderType>();
                        order.ExecuteTime = (DateTime)reader["ExecuteTime"];
                        order.Relation = (string)reader["Relation"];
                        order.Dealer = (Guid)reader["approverID"];

                        queryOrders.Add(order);
                    }
                }
            }
            return queryOrders;
        }

        public static string GetOrderRelationOpenPrice(string exchangeCode,string openOrderId)
        {
            string setPrice = string.Empty;

            string sql = string.Format("SELECT SetPrice FROM v_Order WHERE ID = N'{0}'", openOrderId);
            Object o = DataAccess.GetInstance(exchangeCode).ExecuteScalar(sql,CommandType.Text,null);
            if (o != null)
            {
                setPrice = (string)o;
            }
            return setPrice;
        }

        public static List<BlotterSelection> GetBlotterList(string exchangeCode)
        {
            List<BlotterSelection> blotterSelections = new List<BlotterSelection>();

            string sql = @"SELECT ID,Code FROM dbo.Blotter ORDER BY Code";
            DataAccess.GetInstance(exchangeCode).ExecuteReader(sql, CommandType.Text, delegate(SqlDataReader reader)
            {
                while (reader.Read())
                {
                    BlotterSelection blotter = new BlotterSelection();
                    blotter.Id = (Guid)reader["ID"];
                    blotter.Code = (string)reader["Code"];
                    blotterSelections.Add(blotter);
                }
            });
            return blotterSelections;
        }

        public static Tuple<string, Guid, string> GetAccountGroup(string exchangeCode, Guid accountId)
        {
            Tuple<string, Guid, string> group = null;
            try
            {
                string sql = @"SELECT a.Code,gm.GroupID, g.Code As GroupCode FROM Account a
		                    INNER JOIN GroupMembership gm ON gm.MemberID = a.ID
		                    INNER JOIN [Group] g ON g.[ID] = gm.GroupID
                            WHERE a.ID=@accountId";

                DataAccess.GetInstance(exchangeCode).ExecuteReader(sql, CommandType.Text, delegate(SqlDataReader reader)
                {
                    while (reader.Read())
                    {
                        group = new Tuple<string, Guid, string>((string)reader["Code"], (Guid)reader["GroupID"], (string)reader["GroupCode"]);
                    }
                }, new SqlParameter("@accountId", accountId));
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(TraceEventType.Error, "ExchangeData.GetAccountGroup Error:\r\n" + ex.ToString());
            }
            return group;
        }


        public static SettingSet GetExchangeDataChange(string exchangeCode, GroupChangeType type, List<Guid> memberIds, List<Guid> accounts, List<Guid> instruments)
        {
            SettingSet set = new SettingSet();
            string xmlMemberIds = ExchangeData.GetXmlIds(memberIds);
            string xmlAccountIds = ExchangeData.GetXmlIds(accounts);
            string xmlInstrumentIds = ExchangeData.GetXmlIds(instruments);
            using (SqlConnection con = DataAccess.GetInstance(exchangeCode).GetSqlConnection())
            {
                using (SqlCommand command = con.CreateCommand())
                {
                    command.CommandText = "[dbo].[GetInitDataChangeForManager]";
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@type", type));
                    command.Parameters.Add(new SqlParameter("@xmlMemberIds", xmlMemberIds));
                    command.Parameters.Add(new SqlParameter("@xmlAccountIds", xmlAccountIds));
                    command.Parameters.Add(new SqlParameter("@xmlInstrumentIds", xmlInstrumentIds));
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (type == GroupChangeType.Account)
                        {
                            set = SettingsSetHelper.GetExchangeDataChangeByAccountChange(reader);
                        }
                        else
                        {
                            set = SettingsSetHelper.GetExchangeDataChangeByInstrumentChange(reader);
                        }
                    }
                }
            }
            return set;
        }

        private static string GetXmlIds(List<Guid> memberIds)
        {
            StringBuilder xml = new StringBuilder();
            xml.Append("<Members>");
            foreach (Guid id in memberIds)
            {
                xml.AppendFormat("<Member Id='{0}'/>", id);
            }
            xml.Append("</Members>");
            return xml.ToString();
        }

        private static string GetAccountPermisstionString(HashSet<Guid> ids)
        {
            StringBuilder xml = new StringBuilder();
            xml.Append("<Accounts>");
            foreach (Guid id in ids)
            {
                xml.AppendFormat("<Account Id=\"{0}\"/>", id);
            }
            xml.Append("</Accounts>");
            return xml.ToString();
        }

        private static string GetInstrumentPermisstionString(HashSet<Guid> ids)
        {
            StringBuilder xml = new StringBuilder();
            xml.Append("<Instruments>");
            foreach (Guid id in ids)
            {
                xml.AppendFormat("<Instrument Id=\"{0}\"/>", id);
            }
            xml.Append("</Instruments>");
            return xml.ToString();
        }
    }
}
