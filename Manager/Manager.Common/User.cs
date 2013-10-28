using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manager.Common
{
    public class User
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public Dictionary<int,string> Roles { get; set; }

        public User()
        {
            Roles = new Dictionary<int, string>();
        }
        public bool IsInRole(string role)
        {
            return Roles.ContainsValue(role);
        }

        public bool IsInRole(int roleId)
        {
            return Roles.ContainsKey(roleId);
        }
    }

    public class UserData
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public List<RoleData> Roles { get; set; }

        public UserData()
        {
            Roles = new List<RoleData>();
        }
    }

    public class RoleData
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public List<AccessPermission>  AccessPermissions { get; set; }
        public List<DataPermission> DataPermissions { get; set; }

        public RoleData()
        {
            AccessPermissions = new List<AccessPermission>();
            DataPermissions = new List<DataPermission>();
        }

        public RoleData(int roleId, string roleName)
        {
            RoleId = roleId;
            RoleName = roleName;
            AccessPermissions = new List<AccessPermission>();
            DataPermissions = new List<DataPermission>();
        }
    }

    public class AccessPermission
    {
        public ModuleCategoryType CategotyType { get; set; }
        public ModuleType ModuleType { get; set; }
        public int OperationId { get; set; }
        public string OperationName { get; set; }

        public AccessPermission()
        {
        }

        public AccessPermission(ModuleCategoryType category, ModuleType module, int operationId)
        {
            CategotyType = category;
            ModuleType = module;
            OperationId = operationId;
        }
    }

    public class DataPermission
    {
        public string ExchangeSystemCode { get; set; }
        public DataObjectType DataObjectType { get; set; }
        public Guid DataObjectId { get; set; }
        public string DataObjectDescription { get; set; }
    }

    public enum DataObjectType
    {
        Account = 0,
        Instrument = 1
    }
}
