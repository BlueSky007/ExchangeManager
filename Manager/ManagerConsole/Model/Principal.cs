using Manager.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ManagerConsole.Model
{
    public class Principal 
    {
        public static Principal Instance = new Principal();

        public Principal()
        {
            this.UserPermission = new UserPermission();
        }

        public event Action PermissionChanged;

        public User User { get; set; }
        public UserPermission UserPermission { get; set; }

        public bool HasPermission(ModuleCategoryType categoryType, ModuleType moduleType, string operationCode)
        {
            if (User.IsAdmin)
            {
                return true;
            }
            return this.UserPermission.HasPermission(categoryType, moduleType, operationCode);
        }

        //public void RegistModule(string moduleName, Action callBack)
        //{
        //    if (this.PermissionChanged != null) this.PermissionChanged();
        //}

        public void ProcessUpdate(Dictionary<string, List<FuncPermissionStatus>> newPermission)
        {
            this.UserPermission.FunctionPermissions = newPermission;
            if (this.PermissionChanged != null) this.PermissionChanged();
        }

        public void ProcessUpdateRole(UpdateRoleType type, int roleId)
        {
            if (type == UpdateRoleType.Delete)
            {
                this.User.Roles.Remove(roleId);
                if (this.User.Roles.Count == 0)
                {
                    this.User.Roles.Add(2, "DefaultRole");
                }
            }
            ConsoleClient.Instance.GetAccessPermissions(this.ProcessUpdate);
        }
    }
}
