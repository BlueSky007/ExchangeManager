using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using Manager.Common;
using System.Xml;
using System.IO;

namespace ManagerService.DataAccess
{
    public class UserDataAccess
    {
        public static User Login(string userName, string password, out List<DataPermission> dataPermissions)
        {
            User user = new User();
            user.UserName = userName;
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
                                    user.Roles.Add(roleId, roleName.ToLower());
                                }
                            }
                            if (reader.NextResult())
                            {
                                while (reader.Read())
                                {
                                    DataPermission permission = new DataPermission();
                                    permission.ExchangeSystemCode = reader["IExchangeCode"].ToString();
                                    permission.DataObjectType = reader["DataType"] is DBNull? DataObjectType.None: (DataObjectType)Enum.Parse(typeof(DataObjectType), reader["DataType"].ToString());
                                    permission.DataObjectId = reader["DataObjectId"] is DBNull ? Guid.Empty : (Guid)reader["DataObjectId"];
                                    permission.IsAllow = (bool)reader["Status"];
                                    dataPermissions.Add(permission);
                                }
                            }
                        }
                    }
                    return user;
                }
            }
        }

        public static void LoadLayout(string userName,string layoutName,out string dockLayout, out string contentLayout)
        {
            dockLayout = string.Empty;
            contentLayout = string.Empty;
            string path = string.Format("../../Layout/{0}", userName);
            string layoutPath = string.Format("../../Layout/{0}/{1}_layout.xml", userName, layoutName);
            string contentPath = string.Format("../../Layout/{0}/{1}_content.xml", userName, layoutName);
            if (Directory.Exists(path))
            {
                if (File.Exists(layoutPath))
                {
                    using (var stream = new StreamReader(layoutPath))
                    {
                        dockLayout = stream.ReadToEnd();
                    }
                }
                if (File.Exists(contentPath))
                {
                    using (var stream = new StreamReader(contentPath))
                    {
                        contentLayout = stream.ReadToEnd();
                    }
                }
            }

        }

        public static Dictionary<string,Tuple<string,bool>> GetAccessPermissions(Guid userId, Language language)
        {
            Dictionary<string, Tuple<string, bool>> permissions = new Dictionary<string, Tuple<string, bool>>();
            string sql = "SELECT DISTINCT pt.Id,pt2.Code AS Parent,pt.Code,rp.[Status],pt.[Level] FROM dbo.RolePermission rp INNER JOIN dbo.UserInRole uir ON uir.RoleId = rp.RoleId INNER JOIN dbo.PermissionTarget pt ON pt.Id=rp.TargetId LEFT JOIN dbo.PermissionTarget pt2 ON pt2.Id = pt.ParentId WHERE pt.TargetType = 1 AND uir.UserId = @userId";
            DataAccess.GetInstance().ExecuteReader(sql, CommandType.Text, delegate(SqlDataReader reader)
            {
                while (reader.Read())
                {
                    if (int.Parse(reader["Level"].ToString()) == 1)
                    {
                        permissions.Add("root", Tuple.Create(reader["Code"].ToString(), (bool)reader["Status"]));
                    }
                    else
                    {
                        permissions.Add(reader["Parent"].ToString(),Tuple.Create(reader["Code"].ToString(),(bool)reader["Status"]));
                    }
                }
            }, new SqlParameter("@userId", userId));
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
                    category.CategoryType = (ModuleCategoryType)Enum.Parse(typeof(ModuleCategoryType),reader["CategoryCode"].ToString());
                    category.CategoryDescription = reader["Description"].ToString();
                    tree.Categories.Add(category);
                }
                reader.NextResult();
                while (reader.Read())
                {
                    Module module = new Module();
                    module.Type = (ModuleType)Enum.Parse(typeof(ModuleType), reader["ModuleCode"].ToString());
                    module.ModuleDescription = reader["Description"].ToString();
                    module.Category = (ModuleCategoryType)Enum.Parse(typeof(ModuleCategoryType), reader["parentCode"].ToString());
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
                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        RoleFunctonPermission functionPermission = new RoleFunctonPermission();
                        functionPermission.FunctionId = (int)reader["FunctionId"];
                        functionPermission.Code = reader["Code"].ToString();
                        if (!(reader["ParentId"] is DBNull))
                        {
                            functionPermission.ParentId = (int)reader["ParentId"];
                        }
                        functionPermission.Level = (int)reader["Level"];
                        functionPermission.Description = reader["Description"].ToString();
                        functionPermission.IsAllow = (bool)reader["IsAllow"];
                        int id = (int)reader["RoleId"];
                        roles.SingleOrDefault(r => r.RoleId == id).FunctionPermissions.Add(functionPermission);
                    }
                }
                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        RoleDataPermission dataPermission = new RoleDataPermission();
                        dataPermission.PermissionId = (int)reader["TargetId"];
                        dataPermission.Code = reader["Code"].ToString();
                        if (!(reader["ParentId"] is DBNull))
                        {
                            dataPermission.ParentId = (int)reader["ParentId"];
                        }
                        if (!(reader["DataObjectId"] is DBNull))
                        {
                            dataPermission.DataObjectId = (Guid)reader["DataObjectId"];
                        }
                        dataPermission.IsAllow = (bool)reader["IsAllow"];
                        dataPermission.IExchangeCode = reader["IExchangeCode"].ToString();
                        int id = (int)reader["RoleId"];
                        roles.SingleOrDefault(r => r.RoleId == id).DataPermissions.Add(dataPermission);
                    }
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
            string sql = "DELETE FROM dbo.UserInRole WHERE UserId = @userId DELETE FROM dbo.Users WHERE Id = @userId";
            DataAccess.GetInstance().ExecuteNonQuery(sql, CommandType.Text, new SqlParameter("@userId", userId));
            return true;
        }

        public static List<RoleFunctonPermission> GetAllFunctionPermissions(string language)
        {
            List<RoleFunctonPermission> allFunction = new List<RoleFunctonPermission>();
            using (SqlConnection con = DataAccess.GetInstance().GetSqlConnection())
            {
                using (SqlCommand command = con.CreateCommand())
                {
                    command.CommandText = "SELECT pt.Id, pt.ParentId, pt.[Level], pt.Code,(CASE @language WHEN 'CHT' THEN fd.NameCHT WHEN 'CHS' THEN fd.NameCHS ELSE fd.NameENG END) AS [Description] FROM dbo.PermissionTarget pt	INNER JOIN dbo.FunctionDescription fd ON fd.FunctionId = pt.Id WHERE pt.TargetType = 1";
                    command.CommandType = CommandType.Text;
                    command.Parameters.Add(new SqlParameter("@language", language));
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            RoleFunctonPermission function = new RoleFunctonPermission();
                            function.FunctionId = (int)reader["Id"];
                            function.ParentId = (int)reader["ParentId"];
                            function.Level = int.Parse(reader["Level"].ToString());
                            function.Description = reader["Description"].ToString();
                            allFunction.Add(function);
                        }
                    }
                }
            }
            return allFunction;
        }

        public static List<RoleDataPermission> GetAllDataPermissions(ExchangeSystemSetting[] ExchangeSystems, string language)
        {
            List<RoleDataPermission> allData = new List<RoleDataPermission>();
            foreach (ExchangeSystemSetting system in ExchangeSystems)
            {
                List<RoleDataPermission> dataPermissions = ExchangeData.GetAllDataPermissions(system.Code);
                allData.AddRange(dataPermissions);
            }
            return allData;
        }

        public static bool AddNewRole(RoleData role,out int roleId)
        {
            bool isSuccess = false;
            DataTable permission = new DataTable();
            permission.Columns.Add("Id", typeof(int));
            permission.Columns.Add("ParentId",typeof(int));
            permission.Columns.Add("Level",typeof(int));
            permission.Columns.Add("Code",typeof(string));
            permission.Columns.Add("ExchangeCode", typeof(string));
            permission.Columns.Add("TargetType",typeof(int));
            permission.Columns.Add("DataObjectType", typeof(string));
            permission.Columns.Add("DataObjectId",typeof(Guid));
            permission.Columns.Add("Status", typeof(bool));
            foreach (RoleFunctonPermission item in role.FunctionPermissions)
            {
                DataRow row = permission.NewRow();
                row["Id"] = item.FunctionId;
                row["ParentId"] = item.ParentId;
                row["Level"] = item.Level;
                row["Code"] = item.Code;
                row["TargetType"] = 1;
                row["Status"] = item.IsAllow;
                permission.Rows.Add(row);
            }
            foreach (RoleDataPermission item in role.DataPermissions)
            {
                DataRow row = permission.NewRow();
                row["Id"] = item.PermissionId;
                row["ParentId"] = item.ParentId;
                row["Level"] = item.Level;
                row["Code"] = item.Code;
                row["ExchangeCode"] = item.IExchangeCode;
                row["TargetType"] = 2;
                row["DataObjectType"] = Enum.GetName(typeof(DataObjectType), item.Type);
                row["DataObjectId"] = item.DataObjectId;
                row["Status"] = item.IsAllow;
                permission.Rows.Add(row);
            }
            
            using (SqlConnection con = DataAccess.GetInstance().GetSqlConnection())
            {
                using (SqlTransaction transaction = con.BeginTransaction())
                {
                    using (SqlCommand command = con.CreateCommand())
                    {
                        command.CommandText = "[dbo].[Roles_AddNew]";
                        command.Transaction = transaction;
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@roleName", role.RoleName));
                        command.Parameters.Add(new SqlParameter("@roleId", SqlDbType.Int) { Direction = ParameterDirection.Output });
                        SqlParameter parameter = command.Parameters.AddWithValue("@permissionTarget", permission);
                        parameter.SqlDbType = SqlDbType.Structured;
                        parameter.TypeName = "dbo.PermissionTargetType";
                        command.Parameters.Add(new SqlParameter("@RETURN_VALUE", SqlDbType.Int) { Direction = ParameterDirection.ReturnValue });
                        command.ExecuteNonQuery();
                        roleId = (int)command.Parameters["@roleId"].Value;
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
            DataTable permission = new DataTable();
            permission.Columns.Add("Id", typeof(int));
            permission.Columns.Add("ParentId", typeof(int));
            permission.Columns.Add("Level", typeof(int));
            permission.Columns.Add("Code", typeof(string));
            permission.Columns.Add("ExchangeCode", typeof(string));
            permission.Columns.Add("TargetType", typeof(int));
            permission.Columns.Add("DataObjectType", typeof(string));
            permission.Columns.Add("DataObjectId", typeof(Guid));
            permission.Columns.Add("Status", typeof(bool));
            foreach (RoleFunctonPermission item in role.FunctionPermissions)
            {
                DataRow row = permission.NewRow();
                row["Id"] = item.FunctionId;
                row["ParentId"] = item.ParentId;
                row["Level"] = item.Level;
                row["Code"] = item.Code;
                row["TargetType"] = 1;
                row["Status"] = item.IsAllow;
                permission.Rows.Add(row);
            }
            foreach (RoleDataPermission item in role.DataPermissions)
            {
                DataRow row = permission.NewRow();
                row["Id"] = item.PermissionId;
                row["ParentId"] = item.ParentId;
                row["Level"] = item.Level;
                row["Code"] = item.Code;
                row["DataObjectType"] = item.Type;
                row["ExchangeCode"] = item.IExchangeCode;
                row["TargetType"] = 2;
                row["DataObjectId"] = item.DataObjectId;
                row["Status"] = item.IsAllow;
                permission.Rows.Add(row);
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
                        command.Parameters.Add(new SqlParameter("@roleName", role.RoleName));
                        SqlParameter parameter = command.Parameters.AddWithValue("@permissionTarget", permission);
                        parameter.SqlDbType = SqlDbType.Structured;
                        parameter.TypeName = "dbo.PermissionTargetType";
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

        public static void GetGroupDeafultStatus(string exchangeCode, List<DataPermission> permissions,out bool accountDeafultStatus, out bool instruemntDeafultStatus)
        {
            accountDeafultStatus = false;
            instruemntDeafultStatus = false;
            DataPermission accountTypePermission = permissions.SingleOrDefault(d => d.ExchangeSystemCode == exchangeCode && d.DataObjectType == DataObjectType.Account && d.DataObjectId == null);
            if (accountTypePermission != null)
            {
                accountDeafultStatus = accountTypePermission.IsAllow;
            }
            else
            {
                DataPermission exchangePermission = permissions.SingleOrDefault(d => d.ExchangeSystemCode == exchangeCode && d.DataObjectType == DataObjectType.None);
                if (exchangePermission != null)
                {
                    accountDeafultStatus = exchangePermission.IsAllow;
                }
            }

            DataPermission instrumentTypePermission = permissions.SingleOrDefault(d => d.ExchangeSystemCode == exchangeCode && d.DataObjectType == DataObjectType.Instrument && d.DataObjectId == null);
            if (instrumentTypePermission != null)
            {
                instruemntDeafultStatus = instrumentTypePermission.IsAllow;
            }
            else
            {
                DataPermission exchangePermission = permissions.SingleOrDefault(d => d.ExchangeSystemCode == exchangeCode && d.DataObjectType == DataObjectType.None);
                if (exchangePermission != null)
                {
                    instruemntDeafultStatus = exchangePermission.IsAllow;
                }
            }
        }

        public static List<string> GetAllLayoutName(string userName)
        {
            List<string> layoutNames = new List<string>();
            string path = string.Format("../../Layout/{0}", userName);
            if (Directory.Exists(path))
            {
                DirectoryInfo dir = new DirectoryInfo(path);
                FileInfo[] files = dir.GetFiles();
                foreach (FileInfo file in files)
                {
                    string[] name = file.Name.Split('_');
                    if (!layoutNames.Contains(name[0]))
                    {
                        if (name[0] != "LastClosed" && name[0] != "SystemDeafult")
                        {
                            layoutNames.Add(name[0]);
                        }
                    }
                }
            }
            else
            {
                layoutNames.Add(string.Empty);
                layoutNames.Add(string.Empty);
            }
           return layoutNames;
        }
    }
}
