using iExchange.Common;
using Manager.Common;
using Manager.Common.ReportEntities;
using Manager.Common.Settings;
using System;
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
                DataPermission exchangePermission = permissions.SingleOrDefault(d => d.ExchangeSystemCode == exchangeCode && d.DataObjectType == DataObjectType.None);
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

        //public static ConfigParameters GetSettingParameters(ExchangeSystemSetting exchangeSystemSetting)
        //{
        //    ConfigParameters settings = new ConfigParameters();
        //    settings.AllowModifyOrderLot = exchangeSystemSetting.AllowModifyOrderLot;
        //    settings.ConfirmRejectDQOrder = exchangeSystemSetting.ConfirmRejectDQOrder;
        //    return settings;
        //}

        public static ExchangeInitializeData GetInitData(string exchangeCode, Guid userId, List<DataPermission> permissions,
            bool accountDeafultStatus, bool instrumentDeafuleStatus, out List<Guid> validAccounts, out List<Guid> validInstruments)
        {
            ExchangeInitializeData initializeData = new ExchangeInitializeData();
            validAccounts = new List<Guid>();
            validInstruments = new List<Guid>();

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

        public static List<OrderQueryEntity> GetOrderByInstrument(Guid userId, Guid instrumentId, Guid accountGroupId, OrderType orderType,
           bool isExecute, DateTime fromDate, DateTime toDate)
        {
            List<OrderQueryEntity> queryOrders = new List<OrderQueryEntity>();
            string sql = string.Empty;
            if (isExecute)
            {
                sql = string.Format("exec dbo.P_GetExecutedOrderByInstrument '{0}','{1}',{2},'{3}','{4}','{5}'", userId, instrumentId, orderType, fromDate, toDate, accountGroupId);
            }
            else
            {
                sql = string.Format("exec dbo.P_GetCancelledOrderByInstrument '{0}','{1}',{2},'{3}','{4}','{5}'", userId, instrumentId, orderType, fromDate, toDate, accountGroupId);
            }

            string exhcangeCode = "WF01";
            using (SqlConnection sqlConnection = DataAccess.GetInstance(exhcangeCode).GetSqlConnection())
            {
                SqlCommand command = sqlConnection.CreateCommand();
                command.CommandText = sql;
                command.CommandType = System.Data.CommandType.Text;
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
                        order.Dealer = (string)reader["approverID"];

                        queryOrders.Add(order);
                    }
                }
            }
            return queryOrders;
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


        public static SettingSet GetExchangeDataChange(string exchangeCode, string type, List<Guid> memberIds, List<Guid> accounts, List<Guid> instruments)
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
                        if (type =="Account")
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
    }
}
