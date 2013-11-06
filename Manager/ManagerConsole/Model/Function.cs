using Manager.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ManagerConsole.Model
{
    public class Function
    {
        public Dictionary<int, string> FunctionPermissions { get; set; }

        public Function()
        {
            FunctionPermissions = new Dictionary<int, string>();
        }

        public bool HasPermission(AccessPermission function)
        {
            bool isOwn = false;
            if (FunctionPermissions.ContainsValue(Enum.GetName(typeof(ModuleCategoryType),function.CategotyType)))
            {
                isOwn = true;
            }
            if (FunctionPermissions.ContainsValue(Enum.GetName(typeof(ModuleType),function.ModuleType)))
            {
                isOwn = true;
            }
            if (FunctionPermissions.ContainsValue(function.OperationName))
            {
                isOwn = true;
            }
            return isOwn;
        }
    }
}
