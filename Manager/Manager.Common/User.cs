using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manager.Common
{
    public class UserData
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
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
        }
    }

    public class AccessPermission
    {
        public ModuleType Type { get; set; }
        public int OperationId { get; set; }
        public string OperationName { get; set; }

        public AccessPermission(ModuleType type, int Id)
        {
            Type = type;
            OperationId = Id;
        }
    }

    public class AccessPermissionTree
    {
        public List<CategoryNode> CategoryNodes { get; set; }

        public AccessPermissionTree()
        {
            CategoryNodes = new List<CategoryNode>();
        }
    }

    public class CategoryNode
    {
        public int Id { get; set; }
        public string CategoryDescription { get; set; }
        public List<ModuleNode> ModuleNodes { get; set; }

        public CategoryNode()
        {
            ModuleNodes = new List<ModuleNode>();
        }
    }

    public class ModuleNode
    {
        public int Id { get; set; }
        public string ModuleDescription { get; set; }
        public List<OperationNode> OperationNodes { get; set; }

        public ModuleNode()
        {
            OperationNodes = new List<OperationNode>();
        }
    }

    public class OperationNode
    {
        public Guid Id { get; set; }
        public string OperationDescription { get; set; }
    }

    public class DataPermissionTree
    {
        public List<DataPermission> DataPermissions { get; set; }

        public DataPermissionTree()
        {
            DataPermissions = new List<DataPermission>();
        }
    }

    public class DataPermission
    {
        public DataPermissionType Type { get; set; }
        public string Description { get; set; }
        public List<ExChangeSystem> ExChangeSystems { get; set; }

        public DataPermission()
        {
            ExChangeSystems = new List<ExChangeSystem>();
        }
    }

    public enum DataPermissionType
    {
        Account,
        Instrument
    }

    public class ExChangeSystem
    {
        public string ExchangeCode { get; set; }
        public List<Target> Targets { get; set; }

        public ExChangeSystem()
        {
            Targets = new List<Target>();
        }
    }

    public class Target
    {
        public Guid TargetId { get; set; }
        public string TargetDescription { get; set; }
    }
}
