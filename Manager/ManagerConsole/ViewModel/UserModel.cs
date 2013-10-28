using Manager.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace ManagerConsole.ViewModel
{
    public class UserModel
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Roles { get; set; }
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
        public ModuleCategoryType CategoryType { get; set; }
        public List<ModuleNode> ModuleNodes { get; set; }

        public CategoryNode()
        {
            ModuleNodes = new List<ModuleNode>();
        }
    }

    public class ModuleNode
    {
        public ModuleType Type { get; set; }
        public List<OperationNode> OperationNodes { get; set; }

        public ModuleNode()
        {
            OperationNodes = new List<OperationNode>();
        }
    }

    public class OperationNode
    {
        public int Id { get; set; }
        public string OperationDescription { get; set; }
    }

    public class DataPermissionTree
    {
        public List<ExchangeSystemNode> ExChangeSystemNodes { get; set; }

        public DataPermissionTree()
        {
            ExChangeSystemNodes = new List<ExchangeSystemNode>();
        }
    }

    public class ExchangeSystemNode
    {
        public string ExChangeCode { get; set; }
        public List<DataObjectTypeNode> DataObjectTypeNodes { get; set; }

        public ExchangeSystemNode()
        {
            DataObjectTypeNodes = new List<DataObjectTypeNode>();
        }
    }

    public class DataObjectTypeNode
    {
        public DataObjectType Type { get; set; }
        public List<DataObjectNode> DataObjectNodes { get; set; }

        public DataObjectTypeNode()
        {
            DataObjectNodes = new List<DataObjectNode>();
        }
    }

    public class DataObjectNode
    {
        public Guid DataObjectId { get; set; }
        public string Decription { get; set; }
    }

    public class RoleGridData
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public bool IsAllowEdit { get; set; }
        public bool IsAllowDelete { get; set; }
        public bool IsAllowAdd { get; set; }

        public RoleGridData(RoleData role, bool isAllowAdd, bool isAllowDelete, bool isAllowEdit)
        {
            RoleId = role.RoleId;
            RoleName = role.RoleName;
            IsAllowAdd = isAllowAdd;
            IsAllowDelete = isAllowDelete;
            IsAllowEdit = isAllowEdit;
        }
    }
}
