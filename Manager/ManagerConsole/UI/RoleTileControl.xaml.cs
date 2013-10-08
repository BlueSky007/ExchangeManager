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
using Manager.Common;
using Infragistics.Controls.Menus;
using ManagerConsole.Model;

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
        private RoleData _RoleData;
        private AccessPermissionTree _AccessPermissionEditTree;
        private DataPermissionTree _DataPermissionEditTree;
        private AccessPermissionTree _OriginAccessTree;
        private AccessPermissionTree _NewAccessTree;
        private DataPermissionTree _OriginDataTree;
        private DataPermissionTree _NewDataTree;
        private int _EditType;


        public RoleTileControl()
        {
            InitializeComponent();
        }

        public RoleTileControl(int roleId, string roleName, bool isNewRole, Action<bool> RoleMangerFunction)
        {
            InitializeComponent();
            this._roleId = roleId;
            this._isNewRole = isNewRole;
            this._AddNewRole = RoleMangerFunction;
            
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
            ConsoleClient.Instance.GetAccessPermissionTree(this._roleId, BuildAccessTree);
        }

        public void BuildAccessTree(AccessPermissionTree tree)
        {
            this.Dispatcher.BeginInvoke((Action<AccessPermissionTree>)delegate(AccessPermissionTree result)
            {
                this.AccessTree.ItemsSource = result.CategoryNodes;
            }, tree);
        }

        private void DataTree_Loaded(object sender, RoutedEventArgs e)
        {
            //ConsoleClient.Instance.GetDataPermissionTree(this._roleId, BuildDataTree);
        }

        public void BuildDataTree(DataPermissionTree tree)
        {
            this.Dispatcher.BeginInvoke((Action<DataPermissionTree>)delegate(DataPermissionTree result)
                {
                    this.DataTree.ItemsSource = result.DataPermissions;
                }, tree);
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            ConsoleClient.Instance.GetAccessPermissionTree(0, BuildAccessTree);
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
                        this._NewAccessTree.CategoryNodes.Add(categoryNode);
                    }
                }
                foreach (XamDataTreeNode data in this.DataTree.Nodes)
                {
                    DataPermission dataPermission = (DataPermission)data.Data;
                    dataPermission.ExChangeSystems.Clear();
                    foreach (XamDataTreeNode sys in data.Nodes)
                    {
                        ExChangeSystem exChange = (ExChangeSystem)sys.Data;
                        exChange.Targets.Clear();
                        foreach (XamDataTreeNode target in sys.Nodes)
                        {
                            if ((bool)target.IsChecked)
                            {
                                exChange.Targets.Add((Target)target.Data);
                            }
                        }
                        if (exChange.Targets.Count>0)
                        {
                            dataPermission.ExChangeSystems.Add(exChange);
                        }
                    }
                    if (dataPermission.ExChangeSystems.Count>0)
                    {
                        this._NewDataTree.DataPermissions.Add(dataPermission);
                    }
                }

                ConsoleClient.Instance.UpdateRolePermission(this._roleId, this._EditType, this.RoleName.Text, this._NewAccessTree, this._NewDataTree, EditResult);
            }
            else
            {
                ConsoleClient.Instance.UpdateRolePermission(this._roleId, this._EditType, this.RoleName.Text, new AccessPermissionTree(), new DataPermissionTree(),EditResult);
            }
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
