using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Infragistics.Controls.Menus;
using ManagerConsole.Model;
using ManagerConsole.ViewModel;

namespace ManagerConsole.UI
{
    /// <summary>
    /// RoleTileControl.xaml 的交互逻辑
    /// </summary>
    public partial class RoleTileControl : UserControl
    {
        private int _roleId;
        private bool _isNewRole;
        private Action<bool> _AddNewRole;
        private Action<bool> _DeleteRole;
        private Manager.Common.RoleData _RoleData;
        private Manager.Common.RoleData _AllPermissions;
        private int _EditType;


        public RoleTileControl()
        {
            InitializeComponent();
        }

        public RoleTileControl(int roleId, string roleName, bool isNewRole, Manager.Common.RoleData roleData, Manager.Common.RoleData allPermissions, Action<bool> RoleMangerFunction)
        {
            InitializeComponent();
            this._roleId = roleId;
            this._isNewRole = isNewRole;
            this._RoleData = roleData;
            this._AddNewRole = RoleMangerFunction;
            this._AllPermissions = allPermissions;

            if (isNewRole)
            {
                
                Edit_Click(null, null);
            }
            else
            {
                this.RoleName.Text = roleName;
            }
        }


        private void AccessTree_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void DataTree_Loaded(object sender, RoutedEventArgs e)
        {
            //ConsoleClient.Instance.GetDataPermissionTree(this._roleId, BuildDataTree);
        }

        private void BuileAccessTree(List<Manager.Common.AccessPermission> permissions)
        {
            AccessPermissionTree accessTree = new AccessPermissionTree();
            foreach (Manager.Common.AccessPermission access in permissions)
            {
                CategoryNode category = new CategoryNode();
                //category.
            }
            
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            this.AccessTree.CheckBoxSettings.CheckBoxVisibility = System.Windows.Visibility.Visible;
            this._EditType = 0;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            if (this._EditType == 1)
            {
                foreach (XamDataTreeNode category in this.AccessTree.Nodes)
                {
                    CategoryNode categoryNode = (CategoryNode)category.Data;
                    categoryNode.ModuleNodes.Clear();
                    foreach (XamDataTreeNode module in category.Nodes)
                    {
                        ModuleNode moduleNode = (ModuleNode)module.Data;
                        moduleNode.OperationNodes.Clear();
                        foreach (XamDataTreeNode operation in module.Nodes)
                        {
                            if ((bool)operation.IsChecked)
                            {
                                moduleNode.OperationNodes.Add((OperationNode)operation.Data);
                            }
                        }
                        if (moduleNode.OperationNodes.Count > 0)
                        {
                            categoryNode.ModuleNodes.Add(moduleNode);
                        }
                    }
                    if (categoryNode.ModuleNodes.Count > 0)
                    {
                        //this._NewAccessTree.CategoryNodes.Add(categoryNode);
                    }
                }
            }
            //ConsoleClient.Instance.UpdateRolePermission(this._roleId, this._EditType, this.RoleName.Text, this._NewAccessTree, this._NewDataTree, EditResult);
        }
            
        

        private void EditResult(bool isSuccess)
        {
            if (this._isNewRole)
            {

            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {

        }

        private void NodeCheckedChanged(object sender, NodeEventArgs e)
        {
            this._EditType = 1;
        }
    }
}
