using Manager.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ManagerConsole.Model
{
    public class UserPermission
    {
        //Map for parentCode -> ChildPermission
        public Dictionary<string, List<FuncPermissionStatus>> FunctionPermissions { get; set; }

        public UserPermission()
        {
            FunctionPermissions = new Dictionary<string, List<FuncPermissionStatus>>();
        }

        public bool HasPermission(ModuleCategoryType categoryType, ModuleType moduleType, string operationCode)
        {
            bool isOwn = false;
            if (FunctionPermissions.ContainsKey("admin"))
            {
                return true;
            }
            if (FunctionPermissions.Count == 0)
            {
                return false;
            }
            List<FuncPermissionStatus> category;
            if (FunctionPermissions.TryGetValue("root", out category))
            {
                foreach (FuncPermissionStatus item in category)
                {
                    if (item.Code == Enum.GetName(typeof(ModuleCategoryType), categoryType))
                    {
                        isOwn = item.HasPermission;
                    }
                }
            }
            List<FuncPermissionStatus> module;
            if (FunctionPermissions.TryGetValue(Enum.GetName(typeof(ModuleCategoryType), categoryType), out module))
            {
                foreach (FuncPermissionStatus item in module)
                {
                    if (item.Code == Enum.GetName(typeof(ModuleType), moduleType))
                    {
                        isOwn = item.HasPermission;
                    }
                }
            }
            List<FuncPermissionStatus> operation;
            if (FunctionPermissions.TryGetValue(Enum.GetName(typeof(ModuleType), moduleType), out operation))
            {
                foreach (FuncPermissionStatus item in operation)
                {
                    if (item.Code == operationCode)
                    {
                        isOwn = item.HasPermission;
                    }
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
