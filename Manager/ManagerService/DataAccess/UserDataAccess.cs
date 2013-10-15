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
        public static Guid Login(string userName, string password, out List<DataPermission> dataPermissions)
        {
            Guid userId = Guid.Empty;
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
                            userId = (Guid)reader["Id"];
                            sqlPassword = (string)reader["Password"];
                        }
                        if (userId != Guid.Empty)
                        {
                            if (!UserDataAccess.CheckPassword(password, sqlPassword)) userId = Guid.Empty;
                        }
                        if (userId != Guid.Empty)
                        {
                            reader.NextResult();
                            while (reader.Read())
                            {
                                DataPermission permission = new DataPermission();
                                permission.ExchangeSystemCode = reader["IExchangeCode"].ToString();
                                permission.DataObjectType = (DataObjectType)reader["DataObjectType"];
                                permission.DataObjectId = (Guid)reader["DataObjectId"];
                                dataPermissions.Add(permission);
                            }
                        }
                    }
                    return userId;
                }
            }
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
                command.CommandText = "SELECT u.Id,u.[Name],r.Id,r.RoleName FROM dbo.Users u INNER JOIN dbo.UserInRole uir ON uir.UserId = u.Id INNER JOIN dbo.Roles r ON r.Id = uir.RoleId";
                command.CommandType = System.Data.CommandType.Text;
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    UserData user = new UserData();
                    user.UserId = (Guid)reader.GetValue(0);
                    user.UserName = reader.GetValue(1).ToString();
                    user.RoleId = (int)reader.GetValue(2);
                    user.RoleName = reader.GetValue(3).ToString();
                    data.Add(user);
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
                    roles[id].AccessPermissions.Add(access);
                }
                reader.NextResult();
                while (reader.Read())
                {
                    DataPermission data = new DataPermission();
                    int id = (int)reader["RoleId"];
                    data.ExchangeSystemCode = reader["IExchangeCode"].ToString();
                    data.DataObjectType = (DataObjectType)reader["DataObjectType"];
                    data.DataObjectId = (Guid)reader["DataObjectId"];
                    roles[id].DataPermissions.Add(data);
                }
            }, new SqlParameter("@language", language));
            return roles;
        }

        public static bool AddNewUser(UserData user, string password)
        {
            string encryptPassword = UserDataAccess.GetMd5EncryptPassword(password);
            using (SqlConnection sqlConnection = DataAccess.GetInstance().GetSqlConnection())
            {
                SqlCommand command = sqlConnection.CreateCommand();
                command.CommandText = string.Format("INSERT INTO dbo.Users(Id,[Name],[Password]) VALUES('{0}','{1}','{2}') INSERT INTO dbo.UserInRole(UserId,RoleId) VALUES ('{3}',{4})", user.UserId, user.UserName, encryptPassword, user.UserId, user.RoleId);
                command.CommandType = System.Data.CommandType.Text;
                command.ExecuteNonQuery();
            }
            return true;
        }

        public static bool EditUser(UserData user, string password)
        {
            string commandText = "UPDATE dbo.Users SET";
            if (!string.IsNullOrEmpty(user.UserName))
            {
                commandText += string.Format("[Name]='{0}'", user.UserName);
            }
            if (string.IsNullOrEmpty(password))
            {
                commandText += string.Format("[Password]=N'{0}'", UserDataAccess.GetMd5EncryptPassword(password));
            }
            commandText += string.Format("WHERE Id='{0}'", user.UserId);
            if (user.RoleId != 0)
            {
                commandText += string.Format("UPDATE dbo.UserInRole SET RoleId = {0} WHERE UserId='{1}'", user.RoleId, user.UserId);
            }
            using (SqlConnection sqlConnection = DataAccess.GetInstance().GetSqlConnection())
            {
                SqlCommand command = sqlConnection.CreateCommand();
                command.CommandText = commandText;
                command.CommandType = System.Data.CommandType.Text;
                command.ExecuteNonQuery();
            }
            return true;
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
    }
}
