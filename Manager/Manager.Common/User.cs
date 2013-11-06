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
        public List<RoleFunctonPermission> FunctionPermissions { get; set; }
        public List<RoleDataPermission> DataPermissions { get; set; }

        public RoleData()
        {
            DataPermissions = new List<RoleDataPermission>();
            FunctionPermissions = new List<RoleFunctonPermission>();
        }

        public RoleData(int roleId, string roleName)
        {
            RoleId = roleId;
            RoleName = roleName;
            FunctionPermissions = new List<RoleFunctonPermission>();
            DataPermissions = new List<RoleDataPermission>(); ;
        }
    }

    public class AccessPermission
    {
        public ModuleCategoryType CategotyType { get; set; }
        public ModuleType ModuleType { get; set; }
        public int OperationId { get; set; }
        public string OperationName { get; set; }
        public bool IsAllow { get; set; }
        public AccessPermission()
        {

        }

        public AccessPermission(ModuleCategoryType category, ModuleType module, string operationName)
        {
            CategotyType = category;
            ModuleType = module;
            OperationName = operationName;
        }
    }

    public class DataPermission
    {
        public string ExchangeSystemCode { get; set; }
        public DataObjectType DataObjectType { get; set; }
        public Guid DataObjectId { get; set; }
        public bool IsAllow { get; set; }
        public string DataObjectDescription { get; set; }
    }

    public enum DataObjectType
    {
        Account = 0,
        Instrument = 1,
        None
    }

    public class RoleFunctonPermission
    {
        public int FunctionId { get; set; }
        public string Code { get; set; }
        public int ParentId { get; set; }
        public string Description { get; set; }
        public int Level { get; set; }
        public bool IsAllow { get; set; }

        
    }

    public class RoleDataPermission
    {
        public int PermissionId { get; set; }
        public string Code { get; set; }
        public string IExchangeCode { get; set; }
        public DataObjectType Type { get; set; }
        public int ParentId { get; set; }
        public int Level { get; set; }
        public Guid DataObjectId { get; set; }
        public bool IsAllow { get; set; }
    }
}
