using Manager.Common;
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

        public static InitializeData GetInitData(Guid userId, XmlNode permittedKeys,XmlNode selector, string exchangeCode)
        {
            InitializeData initializeData = new InitializeData();
            //XmlNode itemNode = GetPermissionItems();
            //return this._StateServer.GetInitData();
            string sql = "dbo.GetInitialDataForManager";
            SqlParameter[] parameters = new SqlParameter[3];
            //just test
            SqlParameter userIdParameter = new SqlParameter("@userId", new Guid("D8DCAF50-5021-41F2-A848-80A540F980C1"));
            SqlParameter instrumentKeyParameter;
            SqlParameter instrumentSelectParameter;
            if (permittedKeys != null)
            {
                instrumentKeyParameter = new SqlParameter("@permittedKeys", permittedKeys.OuterXml);
            }
            else
            {
                instrumentKeyParameter = new SqlParameter("@permittedKeys",null);
            }
            if (selector != null)
            {
                instrumentSelectParameter = new SqlParameter("@xmlInstruments", selector.OuterXml);
            }
            else
            {
                instrumentSelectParameter = new SqlParameter("@xmlInstruments", null);
            }

            parameters[0] = userIdParameter;
            parameters[1] = instrumentKeyParameter;
            parameters[2] = instrumentSelectParameter;
            DataAccess.GetInstance(exchangeCode).ExecuteReader(sql, CommandType.StoredProcedure, delegate(SqlDataReader reader)
            {
                try
                {
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
    }
}
