﻿using Manager.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml;

namespace ManagerService.DataAccess
{
    public class ExchangeData
    {
        public static List<Guid> GetMemberIds(string exchangeCode, List<DataPermission> permissions,DataObjectType GroupType)
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
            DataPermission typePermission = permissions.SingleOrDefault(d => d.ExchangeSystemCode == exchangeCode && d.DataObjectType == GroupType && d.DataObjectId==null);
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
                    data.IExchangeCode = exchangeCode;
                    data.Type = (DataObjectType)Enum.Parse(typeof(DataObjectType), reader["GroupType"].ToString());
                    data.DataObjectId = (Guid)reader["ID"];
                    data.Code = reader["Code"].ToString();
                    dataPermissions.Add(data);
                }
            });
            return dataPermissions;
        }

        public static SettingParameters GetSettingParameters(ExchangeSystemSetting exchangeSystemSetting)
        {
            SettingParameters settings = new SettingParameters();
            settings.AllowModifyOrderLot = exchangeSystemSetting.AllowModifyOrderLot;
            settings.ConfirmRejectDQOrder = exchangeSystemSetting.ConfirmRejectDQOrder;
            return settings;
        }

        public static InitializeData GetInitData(string exchangeCode,Guid userId, List<DataPermission> permissions, bool accountDeafultStatus, bool instrumentDeafuleStatus)
        {
            InitializeData initializeData = new InitializeData();

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
            SqlParameter[] parameters = new SqlParameter[4];
            //just test
            SqlParameter userIdParameter = new SqlParameter("@userId", new Guid("D8DCAF50-5021-41F2-A848-80A540F980C1"));
            SqlParameter accountParameter =  new SqlParameter("@accountDeafultStatus",accountDeafultStatus);
            SqlParameter instrumentParameter = new SqlParameter("@instrumentDeafuleStatus", instrumentDeafuleStatus);
            SqlParameter tableParameter = new SqlParameter("@dataPermissions", groupPermission);
            tableParameter.SqlDbType = SqlDbType.Structured;
            tableParameter.TypeName = "[dbo].[DataPermissions]";
            

            parameters[0] = userIdParameter;
            parameters[1] = accountParameter;
            parameters[2] = instrumentParameter;
            parameters[3] = tableParameter;
            DataAccess.GetInstance(exchangeCode).ExecuteReader(sql, CommandType.StoredProcedure, delegate(SqlDataReader reader)
            {
                try
                {
                    List<Guid> accountMemberIds = new List<Guid>();
                    List<Guid> instrumentMemberIds = new List<Guid>();
                    while (reader.Read())
                    { 
                        accountMemberIds.Add((Guid)reader["ID"]); 
                    }
                    reader.NextResult();
                    while (reader.Read())
                    {
                        instrumentMemberIds.Add((Guid)reader["ID"]);
                    }
                    reader.NextResult();
                    initializeData.ValidAccounts.Add(exchangeCode, accountMemberIds);
                    initializeData.ValidInstruments.Add(exchangeCode, instrumentMemberIds);

                    initializeData.SettingSet = new SettingSet();
                    initializeData.SettingSet.Initialize(reader);
                }
                catch (Exception ex)
                {
                    Logger.TraceEvent(TraceEventType.Error, "GetInitData Error:\r\n{0}", ex.ToString());
                }
            }, parameters);
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
    }
}
