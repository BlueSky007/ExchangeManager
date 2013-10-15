using Manager.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace ManagerService.DataAccess
{
    public class ExchangeData
    {
        public static List<Guid> GetMemberIds(string exchangeCode, Guid groupId)
        {
            List<Guid> memberIds = new List<Guid>();
            string sql = "SELECT MemberID FROM GroupMembership WHERE GroupID=@groupId";
            DataAccess.GetInstance(exchangeCode).ExecuteReader(sql, CommandType.Text, delegate(SqlDataReader reader)
            {
                while (reader.Read())
                {
                    memberIds.Add((Guid)reader["MemberID"]);
                }
            }, new SqlParameter("@groupId", groupId));
            return memberIds;
        }

        public static List<DataPermission> GetAllDataPermissions(string exchangeCode)
        {
            List<DataPermission> dataPermissions = new List<DataPermission>();
            string sql = "SELECT g.ID,g.Code,g.GroupType FROM dbo.[Group] g WHERE g.GroupType='Account' OR g.GroupType='Instrument'";
            DataAccess.GetInstance(exchangeCode).ExecuteReader(sql, CommandType.Text, delegate(SqlDataReader reader)
            {
                while (reader.Read())
                {
                    DataPermission data = new DataPermission();
                    data.ExchangeSystemCode = exchangeCode;
                    data.DataObjectType = (DataObjectType)Enum.Parse(typeof(DataObjectType), reader["GroupType"].ToString());
                    data.DataObjectId = (Guid)reader["ID"];
                    data.DataObjectDescription = reader["Code"].ToString();
                    dataPermissions.Add(data);
                }
            });
            return dataPermissions;
        }
    }
}
