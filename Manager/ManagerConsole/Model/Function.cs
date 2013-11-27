using Manager.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ManagerConsole.Model
{
    public class Function
    {
        public Dictionary<string, Tuple<string, bool>> FunctionPermissions { get; set; }

        public Function()
        {
            FunctionPermissions = new Dictionary<string, Tuple<string, bool>>();
        }

        public bool HasPermission(ModuleCategoryType categoryType, ModuleType moduleType, string operationCode)
        {
            bool isOwn = false;
            if (FunctionPermissions.ContainsKey("admin"))
            {
                return true;
            }
            Tuple<string, bool> category;
            
            if (FunctionPermissions.TryGetValue("root", out category))
            {
                if (category.Item1 == Enum.GetName(typeof(ModuleCategoryType), categoryType))
                {
                    isOwn = category.Item2;
                }
            }
            Tuple<string, bool> module;
            if (FunctionPermissions.TryGetValue(Enum.GetName(typeof(ModuleCategoryType), categoryType), out module))
            {
                if (module.Item1 == Enum.GetName(typeof(ModuleType), moduleType))
                {
                    isOwn = module.Item2;
                }
            }
            Tuple<string, bool> operation;
            if (FunctionPermissions.TryGetValue(Enum.GetName(typeof(ModuleType), moduleType), out operation))
            {
                if (operation.Item1 ==  operationCode)
                {
                    isOwn = operation.Item2;
                }
            }
            return isOwn;
        }

        //public bool HasPermission(AccessPermission function)
        //{
        //    bool isOwn = false;
        //    if (FunctionPermissions.ContainsKey("admin"))
        //    {
        //        return true;
        //    }
        //    string category = "";
        //    if (FunctionPermissions.TryGetValue("root",out category))
        //    {
        //        if (category == Enum.GetName(typeof(ModuleCategoryType),function.CategotyType))
        //        {
        //            isOwn = true;
        //        }
        //    }
        //    string module = "";
        //    if (FunctionPermissions.TryGetValue(Enum.GetName(typeof(ModuleCategoryType),function.CategotyType),out module))
        //    {
        //        if (module == Enum.GetName(typeof(ModuleType),function.ModuleType))
        //        {
        //            isOwn = true;
        //        }
        //    }
        //    string value = "";
        //    if (FunctionPermissions.TryGetValue(Enum.GetName(typeof(ModuleType), function.ModuleType), out value))
        //    {
        //        if (value==function.OperationName)
        //        {
        //            isOwn = true;
        //        }
        //    }
        //    return isOwn;
        //}
    }
}
