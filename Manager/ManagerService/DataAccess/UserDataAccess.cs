using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using Manager.Common;

namespace ManagerService.DataAccess
{
    public class UserDataAccess:DataAccess
    {
        public static Guid LoginIn(string userName, string password)
        {
            Guid userId = Guid.Empty;
            using (SqlConnection sqlConnection = DataAccess.GetSqlConnection())
            {
                SqlCommand command = sqlConnection.CreateCommand();
                command.CommandText = "SELECT u.Id, u.[Password] FROM dbo.Users u WHERE u.[Name]=@userName";
                command.CommandType = System.Data.CommandType.Text;
                command.Parameters.Add(new SqlParameter("@userName", userName));
                SqlDataReader reader = command.ExecuteReader();
                reader.Read();
                userId = (Guid)reader.GetValue(0);
                string sqlPassword = reader.GetValue(1).ToString();
                if (UserDataAccess.CheckPassword(password, sqlPassword))
                {
                    return userId;
                }
                else
                {
                    return Guid.Empty;
                }

            }
        }

        public static bool ChangePassword(Guid userId, string currentPassword, string newPassword)
        {
            string password = string.Empty;
            using (SqlConnection sqlConnection = DataAccess.GetSqlConnection())
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
                using (SqlConnection sqlConnection = DataAccess.GetSqlConnection())
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
                for (int i = 0; i <data.Length; i++)
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

        public static FunctionTree BuildFunctionTree(Guid userId,Language language)
        {
            FunctionTree tree = new FunctionTree();
            using (SqlConnection sqlConnection = DataAccess.GetSqlConnection())
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
            using (SqlConnection sqlConnection = DataAccess.GetSqlConnection())
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

        public static List<RoleData> GetRoles()
        {
            List<RoleData> roles = new List<RoleData>();
            using (SqlConnection sqlConnection = DataAccess.GetSqlConnection())
            {
                SqlCommand command = sqlConnection.CreateCommand();
                command.CommandText = "SELECT r.Id,r.RoleName FROM dbo.Roles r";
                command.CommandType = System.Data.CommandType.Text;
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    RoleData role = new RoleData();
                    role.RoleId = (int)reader.GetValue(0);
                    role.RoleName = reader.GetValue(1).ToString();
                    roles.Add(role);
                }
            }
            return roles;
        }

        public static bool AddNewUser(UserData user, string password)
        {
            string encryptPassword = UserDataAccess.GetMd5EncryptPassword(password);
            using (SqlConnection sqlConnection = DataAccess.GetSqlConnection())
            {
                SqlCommand command = sqlConnection.CreateCommand();
                command.CommandText = string.Format("INSERT INTO dbo.Users(Id,[Name],[Password]) VALUES('{0}','{1}','{2}') INSERT INTO dbo.UserInRole(UserId,RoleId) VALUES ('{3}',{4})",user.UserId,user.UserName,encryptPassword,user.UserId,user.RoleId);
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
                commandText += string.Format("[Name]='{0}'",user.UserName);
            }
            if (string.IsNullOrEmpty(password))
            {
                commandText += string.Format("[Password]=N'{0}'", UserDataAccess.GetMd5EncryptPassword(password));
            }
            commandText += string.Format("WHERE Id='{0}'", user.UserId);
            if (user.RoleId!=0)
            {
                commandText += string.Format("UPDATE dbo.UserInRole SET RoleId = {0} WHERE UserId='{1}'", user.RoleId, user.UserId);
            }
            using (SqlConnection sqlConnection = DataAccess.GetSqlConnection())
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
            using (SqlConnection sqlConnection = DataAccess.GetSqlConnection())
            {
                SqlCommand command = sqlConnection.CreateCommand();
                command.CommandText = string.Format("DELETE FROM dbo.UserInRole WHERE UserId = '{0}' DELETE FROM dbo.Users WHERE Id = '{1}'", userId, userId);
                command.CommandType = System.Data.CommandType.Text;
                command.ExecuteNonQuery();
            }
            return true;
        }

        public static AccessPermissionTree GetAccessPermissionTree(int roleId)
        {
            AccessPermissionTree tree = new AccessPermissionTree();
            using (SqlConnection sqlConnection = DataAccess.GetSqlConnection())
            {
                SqlCommand command = sqlConnection.CreateCommand();
                command.CommandText = "dbo.InitAccessPermissionTree";
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@roleId", roleId));
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    CategoryNode node = new CategoryNode();
                    node.Id = (int)reader.GetValue(0);
                    node.CategoryDescription = reader.GetValue(1).ToString();
                    tree.CategoryNodes.Add(node);
                }
                reader.NextResult();
                while (reader.Read())
                {
                    ModuleNode node = new ModuleNode();
                    node.Id = (int)reader.GetValue(0);
                    node.ModuleDescription = reader.GetValue(2).ToString();
                    tree.CategoryNodes.Find(c => c.Id == (int)reader.GetValue(1)).ModuleNodes.Add(node);
                }
                reader.NextResult();
                while (reader.Read())
                {
                    OperationNode node = new OperationNode();
                    node.Id = (Guid)reader.GetValue(2);
                    node.OperationDescription = reader.GetValue(3).ToString();
                    tree.CategoryNodes.Find(c => c.Id == (int)reader.GetValue(0)).ModuleNodes.Find(m => m.Id == (int)reader.GetValue(1)).OperationNodes.Add(node);
                }
            }
            return tree;
        }

        public static void UpdateRolePermission(int roleId,int editType, string roleName, AccessPermissionTree accessTree, DataPermissionTree dataTree)
        {
            string xmlAccessTree = string.Empty;
            string xmlDataTree = string.Empty;
            if (editType == 1)
            {
                StringBuilder accessStr = new StringBuilder();
                accessStr.Append("<AccessTree>");
                foreach (CategoryNode cate in accessTree.CategoryNodes)
                {
                    foreach (ModuleNode module in cate.ModuleNodes)
                    {
                        foreach (OperationNode operation in module.OperationNodes)
                        {
                            accessStr.AppendFormat("<Access Id=\"{0}\"/>", operation.Id);
                        }
                    }
                }
                accessStr.Append("</AccessTree>");
                xmlAccessTree = accessStr.ToString();

                StringBuilder dataStr = new StringBuilder();
                dataStr.Append("<DataTree>");
                foreach (DataPermission data in dataTree.DataPermissions)
                {
                    foreach (ExChangeSystem sys in data.ExChangeSystems)
                    {
                        foreach (Target target in sys.Targets)
                        {
                            dataStr.AppendFormat("<Data Id=\"{0}\"/>", target.TargetId);
                        }
                    }
                }
                dataStr.Append("</DataTree>");
                xmlDataTree = dataStr.ToString();
            }
            using (SqlConnection sqlConnection = DataAccess.GetSqlConnection())
            {
                SqlCommand command = sqlConnection.CreateCommand();
                command.CommandText = "dbo.UpdateRolePermission";
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@roleId", roleId));
                if (editType ==0)
                {
                    command.Parameters.Add(new SqlParameter("@roleName", roleName));
                }
                if (editType == 1)
                {
                    command.Parameters.Add(new SqlParameter("@xmlAccessTree", xmlAccessTree));
                    command.Parameters.Add(new SqlParameter("@xmlDataTree", xmlDataTree));
                }
                command.ExecuteNonQuery();
            }
        }
    }
}
