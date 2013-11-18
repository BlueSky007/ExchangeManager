using Manager.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ManagerConsole.Model
{
    public class Function
    {
        public Dictionary<string, string> FunctionPermissions { get; set; }

        public Function()
        {
            FunctionPermissions = new Dictionary<string, string>();
        }

        public bool HasPermission(AccessPermission function)
        {
            bool isOwn = false;
            if (FunctionPermissions.ContainsKey("admin"))
            {
                return true;
            }
            string category = "";
            if (FunctionPermissions.TryGetValue("root",out category))
            {
                if (category == Enum.GetName(typeof(ModuleCategoryType),function.CategotyType))
                {
                    isOwn = true;
                }
            }
            string module = "";
            if (FunctionPermissions.TryGetValue(Enum.GetName(typeof(ModuleCategoryType),function.CategotyType),out module))
            {
                if (module == Enum.GetName(typeof(ModuleType),function.ModuleType))
                {
                    isOwn = true;
                }
            }
            string value = "";
            if (FunctionPermissions.TryGetValue(Enum.GetName(typeof(ModuleType), function.ModuleType), out value))
            {
                if (value==function.OperationName)
                {
                    isOwn = true;
                }
            }
            return isOwn;
        }
    }
}
