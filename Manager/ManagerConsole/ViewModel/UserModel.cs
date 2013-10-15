using Manager.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ManagerConsole.ViewModel
{
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
    }

    public class ExchangeSystemNode
    {
        public string ExChangeCode { get; set; }
        public List<DataObjectTypeNode> DataObjectTypeNodes { get; set; }
    }

    public class DataObjectTypeNode
    {
        public DataObjectType Type { get; set; }
        public List<DataObjectNode> DataObjectNodes { get; set; }
    }

    public class DataObjectNode
    {
        public Guid DataObjectId { get; set; }
        public string Decription { get; set; }
    }
}
