using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using Manager.Common;
using System.Xml;

namespace ManagerService.DataAccess
{
    public class UserDataAccess
    {
        public static User Login(string userName, string password, out List<DataPermission> dataPermissions)
        {
            User user = new User();
            string sqlPassword = null;
            dataPermissions = new List<DataPermission>();
            using (SqlConnection connection = DataAccess.GetInstance().GetSqlConnection())
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "[dbo].[Login]";
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@userName", userName));
                    command.Parameters.Add(new SqlParameter("@password", password));
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            user.UserId = (Guid)reader["Id"];
                            sqlPassword = (string)reader["Password"];
                        }
                        if (user.UserId != Guid.Empty)
                        {
                            if (!UserDataAccess.CheckPassword(password, sqlPassword))
                            {
                                user.UserId = Guid.Empty;
                            }
                        }
                        if (user.UserId != Guid.Empty)
                        {
                            if (reader.NextResult())
                            {
                                while (reader.Read())
                        {
                                    int roleId = (int)reader["Id"];
                                    string roleName = reader["RoleName"].ToString();
                                    user.Roles.Add(roleId, roleName);
                                }
                        }
                            if (reader.NextResult())
                        {
                            while (reader.Read())
                            {
                                DataPermission permission = new DataPermission();
                                permission.ExchangeSystemCode = reader["IExchangeCode"].ToString();
                                    permission.DataObjectType = (DataObjectType)(int.Parse(reader["DataObjectType"].ToString()));
                                permission.DataObjectId = (Guid)reader["DataObjectId"];
                                dataPermissions.Add(permission);
                            }
                        }
                    }
                    }
                    return user;
                }
            }
        }

        public static List<AccessPermission> GetAccessPermissions(Guid userId,Language language)
        {
            List<AccessPermission> permissions = new List<AccessPermission>();
            string sql = "SELECT DISTINCT rfp.FunctionId,f.ParentId AS Module,f2.ParentId AS Category, (CASE @language WHEN'CHT' THEN f.NameCHT ELSE(CASE @language WHEN 'CHS' THEN f.NameCHS ELSE f.NameENG END) END) AS FunctionName FROM dbo.RoleFunctionPermission rfp INNER JOIN dbo.[Function] f ON f.Id=rfp.FunctionId INNER JOIN dbo.[Function] f2 ON f2.Id=f.ParentId INNER JOIN dbo.UserInRole uir ON uir.RoleId = rfp.RoleId WHERE uir.UserId=@userId";
            DataAccess.GetInstance().ExecuteReader(sql, CommandType.Text, delegate(SqlDataReader reader)
            {
                while (reader.Read())
                {
                    AccessPermission access = new AccessPermission();
                    access.CategotyType = (ModuleCategoryType)reader["Category"];
                    access.ModuleType = (ModuleType)reader["Module"];
                    access.OperationId = (int)reader["FunctionId"];
                    access.OperationName = reader["FunctionName"].ToString();
                    permissions.Add(access);
                }
            }, new SqlParameter("@userId", userId), new SqlParameter("@language",language.ToString()));
            return permissions;
        }

        public static bool ChangePassword(Guid userId, string currentPassword, string newPassword)
        {
            string password = string.Empty;
            using (SqlConnection sqlConnection = DataAccess.GetInstance().GetSqlConnection())
            {
                SqlCommand command = sqlConnection.CreateCommand();
                command.CommandText = "SELECT u.[Password] FROM dbo.Users u WHERE u.Id=@userId";
                command.CommandType = System.Data.CommandType.Text;
                command.Parameters.Add(new SqlParameter("@userId", userId));
                SqlDataReader reader = command.ExecuteReader(System.Data.CommandBehavior.SingleRow);
                reader.Read();
                password = reader.GetValue(0).ToString();
            }
            if (UserDataAccess.CheckPassword(currentPassword, password))
            {
                string encryptNewPassword = UserDataAccess.GetMd5EncryptPassword(newPassword);
                using (SqlConnection sqlConnection = DataAccess.GetInstance().GetSqlConnection())
                {
                    SqlCommand command = sqlConnection.CreateCommand();
                    command.CommandText = "UPDATE dbo.Users SET [Password] = @newPassword WHERE Id= @userId";
                    command.CommandType = System.Data.CommandType.Text;
                    command.Parameters.Add(new SqlParameter("@newPassword", encryptNewPassword));
                    command.Parameters.Add(new SqlParameter("@userId", userId));
                    command.ExecuteNonQuery();
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public static string GetMd5EncryptPassword(string password)
        {
            string encryptedStr = string.Empty;
            using (MD5 md5 = MD5.Create())
            {
                byte[] data = md5.ComputeHash(Encoding.Unicode.GetBytes(password));
                StringBuilder strBuilder = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                {
                    strBuilder.Append(data[i].ToString("x2"));
                }
                encryptedStr = strBuilder.ToString();
            }
            return encryptedStr;
        }

        private static bool CheckPassword(string inputPassword, string currentPassword)
        {
            string encryptedStr = UserDataAccess.GetMd5EncryptPassword(inputPassword);
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;
            return (comparer.Compare(encryptedStr, currentPassword) == 0);
        }

        public static FunctionTree BuildFunctionTree(Guid userId, Language language)
        {
            FunctionTree tree = new FunctionTree();
            using (SqlConnection sqlConnection = DataAccess.GetInstance().GetSqlConnection())
            {
                SqlCommand command = sqlConnection.CreateCommand();
                command.CommandText = "[dbo].[FunctionTree_GetData]";
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@userId", userId));
                command.Parameters.Add(new SqlParameter("@language", language.ToString()));
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Category category = new Category();
                    category.CategoryType = (ModuleCategoryType)reader.GetValue(0);
                    category.CategoryDescription = reader.GetValue(1).ToString();
                    tree.Categories.Add(category);
                }
                reader.NextResult();
                while (reader.Read())
                {
                    Module module = new Module();
                    module.Type = (ModuleType)reader.GetValue(0);
                    module.ModuleDescription = reader.GetValue(1).ToString();
                    module.Category = (ModuleCategoryType)reader.GetValue(2);
                    tree.Modules.Add(module);
                }
            }
            return tree;
        }

        public static List<UserData> GetUserData()
        {
            List<UserData> data = new List<UserData>();
            using (SqlConnection sqlConnection = DataAccess.GetInstance().GetSqlConnection())
            {
                SqlCommand command = sqlConnection.CreateCommand();
                command.CommandText = "SELECT u.Id,u.[Name] FROM dbo.Users u SELECT uir.UserId,uir.RoleId,r.RoleName FROM dbo.UserInRole uir INNER JOIN dbo.Roles r ON r.Id = uir.RoleId";
                command.CommandType = System.Data.CommandType.Text;
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    UserData user = new UserData();
                    user.UserId = (Guid)reader["Id"];
                    user.UserName = reader["Name"].ToString();
                    data.Add(user);
                }
                reader.NextResult();
                while (reader.Read())
                {
                    RoleData role = new RoleData();
                    role.RoleId = (int)reader["RoleId"];
                    role.RoleName = reader["RoleName"].ToString();
                    data.Find(u=>u.UserId==(Guid)reader["UserId"]).Roles.Add(role);
            }
            }
            return data;
        }

        public static List<RoleData> GetRoles(string language)
        {
            List<RoleData> roles = new List<RoleData>();
            string sql = "[dbo].[GetAllRoleData]";
            DataAccess.GetInstance().ExecuteReader(sql, CommandType.StoredProcedure, delegate(SqlDataReader reader)
            {
                while (reader.Read())
                {
                    RoleData role = new RoleData();
                    role.RoleId = (int)reader.GetValue(0);
                    role.RoleName = reader.GetValue(1).ToString();
                    roles.Add(role);
                }
                reader.NextResult();
                while (reader.Read())
                {
                    AccessPermission access = new AccessPermission();
                    access.CategotyType = (ModuleCategoryType)reader["CategoryId"];
                    access.ModuleType = (ModuleType)reader["ModuleId"];
                    access.OperationId = (int)reader["OperationId"];
                    access.OperationName = reader["OperationName"].ToString();
                    int id = (int)reader["RoleId"];
                    roles[id-1].AccessPermissions.Add(access);
                }
                reader.NextResult();
                while (reader.Read())
                {
                    DataPermission data = new DataPermission();
                    int id = (int)reader["RoleId"];
                    data.ExchangeSystemCode = reader["IExchangeCode"].ToString();
                    data.DataObjectType = (DataObjectType)(int.Parse(reader["DataObjectType"].ToString()));
                    data.DataObjectId = (Guid)reader["DataObjectId"];
                    data.DataObjectDescription = reader["DataObjectDescription"].ToString();
                    roles[id-1].DataPermissions.Add(data);
                }
            }, new SqlParameter("@language", language));
            return roles;
        }

        public static bool AddNewUser(UserData user, string password)
        {
            string encryptPassword = UserDataAccess.GetMd5EncryptPassword(password);
            string roles = string.Empty;
            bool isSuccess = false;
            foreach (RoleData role in user.Roles)
            {
                roles += (role.RoleId + ",");
            }
            SqlParameter[] parameters = new SqlParameter[] { new SqlParameter("@userId", user.UserId), new SqlParameter("@userName", user.UserName), new SqlParameter("@password",encryptPassword), new SqlParameter("@roles",roles) };
            using (SqlConnection sqlConnection = DataAccess.GetInstance().GetSqlConnection())
            {
                using (SqlTransaction transaction = sqlConnection.BeginTransaction())
                {
                    using (SqlCommand command = sqlConnection.CreateCommand())
                    {
                        command.CommandText = "[dbo].[User_AddNew]";
                        command.Transaction = transaction;
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@userId", user.UserId));
                        command.Parameters.Add(new SqlParameter("@userName", user.UserName));
                        command.Parameters.Add(new SqlParameter("@password", encryptPassword));
                        command.Parameters.Add(new SqlParameter("@roles", roles));
                        command.Parameters.Add(new SqlParameter("@RETURN_VALUE", SqlDbType.Int) { Direction = ParameterDirection.ReturnValue });
                command.ExecuteNonQuery();
                        int returnValue = (int)command.Parameters["@RETURN_VALUE"].Value;
                        isSuccess = (returnValue == 0);
                        if (isSuccess)
                        {
                            transaction.Commit();
            }
        }
                }
            }
            return isSuccess;
        }

        public static bool EditUser(UserData user, string password)
        {
            string roles = string.Empty;
            bool isSuccess = false;
            foreach (RoleData role in user.Roles)
            {
                roles += (role.RoleId + ",");
            }
            using (SqlConnection sqlConnection = DataAccess.GetInstance().GetSqlConnection())
            {
                using (SqlTransaction transaction = sqlConnection.BeginTransaction())
                {
                    using (SqlCommand command = sqlConnection.CreateCommand())
                    {
                        command.CommandText = "[dbo].[Users_Update]";
                        command.Transaction = transaction;
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@userId", user.UserId));
                        if (!string.IsNullOrEmpty(user.UserName))
                        {
                            command.Parameters.Add(new SqlParameter("@userName", user.UserName));
                        }
                        if (!string.IsNullOrEmpty(password))
                        {
                            string encryptPassword = UserDataAccess.GetMd5EncryptPassword(password);
                            command.Parameters.Add(new SqlParameter("@password", encryptPassword));
                        }
                        if (user.Roles.Count != 0)
                        {
                            command.Parameters.Add(new SqlParameter("@roles", roles));
                        }
                        command.Parameters.Add(new SqlParameter("@RETURN_VALUE", SqlDbType.Int) { Direction = ParameterDirection.ReturnValue });
                        command.ExecuteNonQuery();
                        int returnValue = (int)command.Parameters["@RETURN_VALUE"].Value;
                        isSuccess = (returnValue == 0);
                        if (isSuccess)
                        {
                            transaction.Commit();
                        }
                    }

                }
            }
            return isSuccess;
        }

        public static bool DeleteUser(Guid userId)
        {
            using (SqlConnection sqlConnection = DataAccess.GetInstance().GetSqlConnection())
            {
                SqlCommand command = sqlConnection.CreateCommand();
                command.CommandText = string.Format("DELETE FROM dbo.UserInRole WHERE UserId = @userId DELETE FROM dbo.Users WHERE Id = '{1}'", userId, userId);
                command.CommandType = System.Data.CommandType.Text;
                command.ExecuteNonQuery();
            }
            return true;
        }

        public static RoleData GetAllPermissions(ExchangeSystemSetting[] ExchangeSystems,string language)
        {
            RoleData data = new RoleData();
            using (SqlConnection con = DataAccess.GetInstance().GetSqlConnection())
            {
                using (SqlCommand command = con.CreateCommand())
                {
                    command.CommandText = "SELECT f3.Id AS CategoryId,f2.Id AS ModuleId,f.Id AS OperationId,(CASE WHEN @language='CHT' THEN f.NameCHT ELSE (CASE WHEN @language='CHS' THEN f.NameCHS ELSE f.NameENG END) END) AS OperationName FROM  dbo.[Function] f INNER JOIN dbo.[Function] f2 ON f2.Id=f.ParentId INNER JOIN dbo.[Function] f3 ON f3.Id=f2.ParentId WHERE f3.ParentId=0";
                    command.CommandType = CommandType.Text;
                    command.Parameters.Add(new SqlParameter("@language", language));
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            AccessPermission access = new AccessPermission();
                            access.CategotyType = (ModuleCategoryType)reader["CategoryId"];
                            access.ModuleType = (ModuleType)reader["ModuleId"];
                            access.OperationId = (int)reader["OperationId"];
                            access.OperationName = reader["OperationName"].ToString();
                            data.AccessPermissions.Add(access);
                        }
                    }
                }
            }
            foreach (ExchangeSystemSetting system in ExchangeSystems)
            {
                List<DataPermission> dataPermissions = ExchangeData.GetAllDataPermissions(system.Code);
                data.DataPermissions.AddRange(dataPermissions);
            }
            return data;
        }

        public static bool AddNewRole(RoleData role)
        {
            bool isSuccess = false;
            string functionStr = string.Empty;
            foreach (AccessPermission item in role.AccessPermissions)
            {
                functionStr += (item.OperationId + ",");
            }
            DataTable dataPermissionTable = new DataTable();
            dataPermissionTable.Columns.Add("ExchangeCode", typeof(string));
            dataPermissionTable.Columns.Add("DataObjectType", typeof(int));
            dataPermissionTable.Columns.Add("DataObjectId", typeof(Guid));
            dataPermissionTable.Columns.Add("DataObjectDescription", typeof(string));
            foreach (DataPermission item in role.DataPermissions)
            {
                DataRow row = dataPermissionTable.NewRow();
                row["ExchangeCode"] = item.ExchangeSystemCode;
                row["DataObjectType"] = item.DataObjectType;
                row["DataObjectId"] = item.DataObjectId;
                row["DataObjectDescription"] = item.DataObjectDescription;
                dataPermissionTable.Rows.Add(row);
            }
            using (SqlConnection sqlConnection = DataAccess.GetInstance().GetSqlConnection())
            {
                using (SqlTransaction transaction = sqlConnection.BeginTransaction())
                {
                    using (SqlCommand command = sqlConnection.CreateCommand())
                    {
                        command.CommandText = "[dbo].[Roles_AddNew]";
                        command.Transaction = transaction;
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@roleName", role.RoleName));
                        command.Parameters.Add(new SqlParameter("@functionPermssions", functionStr));
                        SqlParameter parameter = command.Parameters.AddWithValue("@dataPermissions", dataPermissionTable);
                        parameter.SqlDbType = SqlDbType.Structured;
                        parameter.TypeName = "dbo.DataPermissionsTableType";
                        command.Parameters.Add(new SqlParameter("@RETURN_VALUE", SqlDbType.Int) { Direction = ParameterDirection.ReturnValue });
                        command.ExecuteNonQuery();
                        int returnValue = (int)command.Parameters["@RETURN_VALUE"].Value;
                        isSuccess = (returnValue == 0);
                        if (isSuccess)
                        {
                            transaction.Commit();
                        }
                    }
                }
            }
            return isSuccess;
        }

        public static bool EditRole(RoleData role)
        {
            bool isSuccess = false;
            DataTable dataPermissionTable = new DataTable();
            dataPermissionTable.Columns.Add("ExchangeCode", typeof(string));
            dataPermissionTable.Columns.Add("DataObjectType", typeof(int));
            dataPermissionTable.Columns.Add("DataObjectId", typeof(Guid));
            dataPermissionTable.Columns.Add("DataObjectDescription", typeof(string));
            foreach (DataPermission item in role.DataPermissions)
            {
                DataRow row = dataPermissionTable.NewRow();
                row["ExchangeCode"] = item.ExchangeSystemCode;
                row["DataObjectType"] = item.DataObjectType;
                row["DataObjectId"] = item.DataObjectId;
                row["DataObjectDescription"] = item.DataObjectDescription;
                dataPermissionTable.Rows.Add(row);
            }
            using (SqlConnection con = DataAccess.GetInstance().GetSqlConnection())
            {
                using (SqlTransaction transaction = con.BeginTransaction())
                {
                    using (SqlCommand command = con.CreateCommand())
                    {
                        command.CommandText = "[dbo].[Roles_Update]";
                        command.Transaction = transaction;
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@roleId", role.RoleId));
                        if (string.IsNullOrEmpty(role.RoleName))
                        {
                            command.Parameters.Add(new SqlParameter("@roleName", role.RoleName));
                        }
                        if (role.AccessPermissions.Count != 0)
                        {
                            string functionStr = string.Empty;
                            foreach (AccessPermission item in role.AccessPermissions)
                            {
                                functionStr += (item.OperationId + ",");
                            }
                            command.Parameters.Add(new SqlParameter("@functionPermssions", functionStr));
                        }
                        SqlParameter parameter = command.Parameters.AddWithValue("@dataPermissions", dataPermissionTable);
                        parameter.SqlDbType = SqlDbType.Structured;
                        parameter.TypeName = "dbo.DataPermissionsTableType";
                        command.Parameters.Add(new SqlParameter("@RETURN_VALUE", SqlDbType.Int) { Direction = ParameterDirection.ReturnValue });
                        command.ExecuteNonQuery();
                        int returnValue = (int)command.Parameters["@RETURN_VALUE"].Value;
                        isSuccess = (returnValue == 0);
                        if (isSuccess)
                        {
                            transaction.Commit();
                        }
                    }
                }
            }
            return isSuccess;
        }

        public static bool DeleteRole(int roleId,Guid userId)
        {
            string sql = "[dbo].[Role_Delete]";
            bool isSuccess = false;
            using (SqlConnection con = DataAccess.GetInstance().GetSqlConnection())
            {
                using (SqlTransaction transaction = con.BeginTransaction())
                {
                    using (SqlCommand command = con.CreateCommand())
                    {
                        command.CommandText = sql;
                        command.Transaction = transaction;
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@roleId", roleId));
                        command.Parameters.Add(new SqlParameter("@RETURN_VALUE", SqlDbType.Int) { Direction = ParameterDirection.ReturnValue });
                        command.ExecuteNonQuery();
                        int returnValue = (int)command.Parameters["@RETURN_VALUE"].Value;
                        isSuccess = (returnValue == 0);
                        if (isSuccess)
                        {
                            transaction.Commit();
                        }
                    }
                }
            }
            return isSuccess;
        }
    }
}
