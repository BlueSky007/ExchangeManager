using Infragistics.Controls.Editors;
using Infragistics.Controls.Grids;
using Infragistics.Controls.Layouts;
using Infragistics.Controls.Menus;
using Manager.Common;
using ManagerConsole.Model;
using ManagerConsole.ViewModel;
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
using System.Collections.ObjectModel;

namespace ManagerConsole.UI
{
    /// <summary>
    /// RoleManagerControl.xaml 的交互逻辑
    /// </summary>
    public partial class RoleManagerControl : UserControl
    {
        private ObservableCollection<RoleData> _roleDatas;
        private ObservableCollection<RoleGridData> _RoleGridDatas;
        private RoleData _AllRolePermissions;
        private RoleData _SelectRole;

        public static readonly DependencyProperty IsTeamLeaderProperty = DependencyProperty.Register("IsAllowEdit", typeof(bool), typeof(RoleManagerControl));

        public bool IsAllowEdit{ get;set;}
        public bool IsAllowDelete { get; set; }
        public bool IsAllowAdd { get; set; }
        public RoleManagerControl()
        {
            InitializeComponent();
        }

        private void AddRole_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this._SelectRole = new RoleData();
                this._SelectRole.RoleId = this._roleDatas.Count + 2;
                this.Submit.Visibility = System.Windows.Visibility.Visible;
                this.Cancel.Visibility = System.Windows.Visibility.Hidden;
                this.RoleName.IsReadOnly = false;
                this.RoleName.Text = string.Empty;
                DataPermissionTree allDataTree = this.BuildDataPermissionTree(this._AllRolePermissions.DataPermissions);
                AccessPermissionTree allAccessTree = this.BuileAccessTree(this._AllRolePermissions.AccessPermissions);
                this.DataTree.ItemsSource = allDataTree.ExChangeSystemNodes;
                this.DataTree.CheckBoxSettings.CheckBoxVisibility = System.Windows.Visibility.Visible;
                this.AccessTree.ItemsSource = allAccessTree.CategoryNodes;
                this.AccessTree.CheckBoxSettings.CheckBoxVisibility = System.Windows.Visibility.Visible;
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "AddRole_Click.\r\n{0}", ex.ToString());
            }
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.DataContext = this;
                this.IsAllowEdit = ConsoleClient.Instance.IsHasThisPermission(17);
                this.IsAllowDelete = ConsoleClient.Instance.IsHasThisPermission(18);
                this.IsAllowAdd = ConsoleClient.Instance.IsHasThisPermission(16);
                RoleData data = new RoleData();
                data = ConsoleClient.Instance.GetAllPermission();
                List<RoleData> roles = ConsoleClient.Instance.GetRoles();
                this._roleDatas = new ObservableCollection<RoleData>(roles);
                this._RoleGridDatas = new ObservableCollection<RoleGridData>();
                foreach (RoleData role in roles)
                {
                    if (role.RoleId != 1)
                    {
                        this._RoleGridDatas.Add(new RoleGridData(role, true, true, this.IsAllowEdit));
                    }
                }
                this._AllRolePermissions = data;
                this.RoleGrid.ItemsSource = this._RoleGridDatas;
                this.Submit.Visibility = System.Windows.Visibility.Hidden;
                this.Cancel.Visibility = System.Windows.Visibility.Hidden;
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "RoleManagerControl/Grid_Loaded.\r\n{0}", ex.ToString());
            }
        }
        private void AddNewRole(bool result,RoleData role)
        {
            try
            {
                XamTile tile = new XamTile();
                tile.Header = role.RoleName;
                tile.Content = new RoleTileControl(false, role, this._AllRolePermissions, AddNewRole,DeleteRole);
               // this.RoleManager.Items.Add(tile);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "AddNewRole.\r\n{0}", ex.ToString());                
            }
        }

        public void DeleteRole(RoleData role)
        {
            try
            {
                XamTile tile = new XamTile();
                tile.Header = role.RoleName;
                tile.Content = new RoleTileControl(false, role, this._AllRolePermissions, AddNewRole, DeleteRole);
               // this.RoleManager.Items.Remove(tile);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "DeleteRole.\r\n{0}", ex.ToString());
            }
        }

        private void RoleGrid_CellClicked(object sender, CellClickedEventArgs e)
        {
            try
            {
                string roleName = e.Cell.Value.ToString();
                RoleData role = this._roleDatas.SingleOrDefault(r => r.RoleName == roleName);
                this._SelectRole = role;
                this.RoleName.Text = this._SelectRole.RoleName;
                this.RoleName.IsReadOnly = true;
                DataPermissionTree dataTree = this.BuildDataPermissionTree(role.DataPermissions);
                AccessPermissionTree accessTree = this.BuileAccessTree(role.AccessPermissions);
                this.DataTree.ItemsSource = dataTree.ExChangeSystemNodes;
                this.DataTree.CheckBoxSettings.CheckBoxVisibility = System.Windows.Visibility.Hidden;
                this.AccessTree.ItemsSource = accessTree.CategoryNodes;
                this.AccessTree.CheckBoxSettings.CheckBoxVisibility = System.Windows.Visibility.Hidden;
                this.Submit.Visibility = System.Windows.Visibility.Hidden;
                this.Cancel.Visibility = System.Windows.Visibility.Hidden;
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "RoleManager/RoleGrid_CellClicked.\r\n{0}", ex.ToString());
            }
        }

        private AccessPermissionTree BuileAccessTree(List<AccessPermission> permissions)
        {
            AccessPermissionTree accessTree = new AccessPermissionTree();
            foreach (ModuleCategoryType catetype in Enum.GetValues(typeof(ModuleCategoryType)))
            {
                CategoryNode category = new CategoryNode();
                category.CategoryType = catetype;
                List<AccessPermission> moduleList = permissions.FindAll(delegate(AccessPermission access) { return access.CategotyType == catetype; });
                List<ModuleType> moduleInCategory = new List<ModuleType>();
                foreach (ModuleType moduleType in Enum.GetValues(typeof(ModuleType)))
                {
                    AccessPermission accesssss = moduleList.Find(delegate(AccessPermission per)
                    {
                        return per.ModuleType == moduleType;
                    });
                    if (moduleList.Find(delegate(AccessPermission per) { return per.ModuleType == moduleType; }) != null)
                    {
                        moduleInCategory.Add(moduleType);
                    }
                }
                foreach (ModuleType moduleType in moduleInCategory)
                {
                    ModuleNode module = new ModuleNode();
                    module.Type = moduleType;
                    foreach (AccessPermission access in moduleList)
                    {
                        if (access.ModuleType == moduleType)
                        {
                            OperationNode node = new OperationNode();
                            node.Id = access.OperationId;
                            node.OperationDescription = access.OperationName;
                            module.OperationNodes.Add(node);
                        }
                    }
                    category.ModuleNodes.Add(module);
                }
                if (category.ModuleNodes.Count != 0)
                {
                    accessTree.CategoryNodes.Add(category);
                }
            }
            return accessTree;
        }

        private DataPermissionTree BuildDataPermissionTree(List<DataPermission> permissions)
        {
            DataPermissionTree tree = new DataPermissionTree();
            List<string> ExchangeCodes = new List<string>();
            foreach (DataPermission item in permissions)
            {
                if (!ExchangeCodes.Contains(item.ExchangeSystemCode))
                {
                    ExchangeCodes.Add(item.ExchangeSystemCode);
                }
            }
            foreach (string item in ExchangeCodes)
            {
                ExchangeSystemNode systemNode = new ExchangeSystemNode();
                systemNode.ExChangeCode = item;
                foreach (DataObjectType type in Enum.GetValues(typeof(DataObjectType)))
                {
                    DataObjectTypeNode typeNode = new DataObjectTypeNode();
                    typeNode.Type = type;
                    foreach (DataPermission data in permissions)
                    {
                        if (data.DataObjectType == type && data.ExchangeSystemCode == item)
                        {
                            DataObjectNode node = new DataObjectNode();
                            node.DataObjectId = data.DataObjectId;
                            node.Decription = data.DataObjectDescription;
                            typeNode.DataObjectNodes.Add(node);
                        }
                    }
                    systemNode.DataObjectTypeNodes.Add(typeNode);
                }
                tree.ExChangeSystemNodes.Add(systemNode);
            }
            return tree;
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button edit = sender as Button;
                int roleId = (int)edit.Tag;
                this._SelectRole = this._roleDatas.SingleOrDefault(r => r.RoleId == roleId);
                this.RoleName.Text = this._SelectRole.RoleName;
                this.RoleName.IsReadOnly = false;
                DataPermissionTree allDataTree = this.BuildDataPermissionTree(this._AllRolePermissions.DataPermissions);
                AccessPermissionTree allAccessTree = this.BuileAccessTree(this._AllRolePermissions.AccessPermissions);
                this.DataTree.ItemsSource = allDataTree.ExChangeSystemNodes;
                this.DataTree.CheckBoxSettings.CheckBoxVisibility = System.Windows.Visibility.Visible;
                this.AccessTree.ItemsSource = allAccessTree.CategoryNodes;
                this.AccessTree.CheckBoxSettings.CheckBoxVisibility = System.Windows.Visibility.Visible;
                this.Submit.Visibility = System.Windows.Visibility.Visible;
                this.Cancel.Visibility = System.Windows.Visibility.Visible;
                this.SetPermissionNodeIsCheck(true);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "EditButton_Click.\r\n{0}", ex.ToString());

            }
        }

        private void SetPermissionNodeIsCheck(bool isChecked)
        {
            try
            {
                foreach (XamDataTreeNode exchange in this.DataTree.Nodes)
                {
                    foreach (XamDataTreeNode datatype in exchange.Nodes)
                    {
                        foreach (XamDataTreeNode dataNode in datatype.Nodes)
                        {
                            DataObjectNode data = (DataObjectNode)dataNode.Data;
                            if (this._SelectRole.DataPermissions.SingleOrDefault(d => d.DataObjectId == data.DataObjectId) != null)
                            {
                                dataNode.IsChecked = isChecked;
                                datatype.IsChecked = isChecked;
                                exchange.IsChecked = isChecked;
                            }
                        }
                    }
                }
                foreach (XamDataTreeNode category in this.AccessTree.Nodes)
                {
                    foreach (XamDataTreeNode module in category.Nodes)
                    {
                        foreach (XamDataTreeNode accessNode in module.Nodes)
                        {
                            OperationNode operationNode = (OperationNode)accessNode.Data;
                            if (this._SelectRole.AccessPermissions.SingleOrDefault(a => a.OperationId == operationNode.Id) != null)
                            {
                                accessNode.IsChecked = isChecked;
                                module.IsChecked = isChecked;
                                category.IsChecked = isChecked;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "SetPermissionNodeIsCheck.\r\n{0}", ex.ToString());
            }
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RoleData newRole = new RoleData();
                newRole.RoleId = this._SelectRole.RoleId;
                this._SelectRole.RoleName = this.RoleName.Text;
                newRole.RoleName = this.RoleName.Text;
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
                                AccessPermission access = new AccessPermission();
                                access.CategotyType = categoryNode.CategoryType;
                                access.ModuleType = moduleNode.Type;
                                OperationNode operationNode = (OperationNode)operation.Data;
                                access.OperationId = operationNode.Id;
                                access.OperationName = operationNode.OperationDescription;
                                newRole.AccessPermissions.Add(access);
                            }
                        }

                    }
                }
                foreach (XamDataTreeNode systemNode in this.DataTree.Nodes)
                {
                    ExchangeSystemNode system = (ExchangeSystemNode)systemNode.Data;
                    foreach (XamDataTreeNode typeNode in systemNode.Nodes)
                    {
                        DataObjectTypeNode dataType = (DataObjectTypeNode)typeNode.Data;
                        foreach (XamDataTreeNode dataNode in typeNode.Nodes)
                        {
                            if ((bool)dataNode.IsChecked)
                            {
                                DataPermission dataPermission = new DataPermission();
                                dataPermission.ExchangeSystemCode = system.ExChangeCode;
                                dataPermission.DataObjectType = dataType.Type;
                                DataObjectNode data = (DataObjectNode)dataNode.Data;
                                dataPermission.DataObjectId = data.DataObjectId;
                                dataPermission.DataObjectDescription = data.Decription;
                                newRole.DataPermissions.Add(dataPermission);
                            }
                        }
                    }
                }
                this._SelectRole = newRole;
                ConsoleClient.Instance.UpdateRole(newRole, (newRole.RoleId == this._roleDatas.Count + 2), EditResult);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "Submit_Click.\r\n{0}", ex.ToString());
            }
        }

        private void EditResult(bool isSuccess)
        {
            try
            {
                if (this._SelectRole.RoleId == (this._roleDatas.Count + 2))
                {
                    this.Dispatcher.BeginInvoke((Action<bool>)delegate(bool result)
                    {
                        this._roleDatas.Add(this._SelectRole);
                        this._RoleGridDatas.Add(new RoleGridData(this._SelectRole,this.IsAllowAdd,this.IsAllowDelete,this.IsAllowEdit));
                    }, isSuccess);
                }
                else
                {
                    this.Dispatcher.BeginInvoke((Action<bool>)delegate(bool result)
                    {
                        RoleData role = this._roleDatas.SingleOrDefault(r => r.RoleId == this._SelectRole.RoleId);
                        role = this._SelectRole;
                    }, isSuccess);
                }
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "RoleManager/EditResult.\r\n{0}", ex.ToString());
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.RoleName.Text = this._SelectRole.RoleName;
                DataPermissionTree dataTree = this.BuildDataPermissionTree(this._SelectRole.DataPermissions);
                AccessPermissionTree accessTree = this.BuileAccessTree(this._SelectRole.AccessPermissions);
                this.DataTree.ItemsSource = dataTree.ExChangeSystemNodes;
                this.AccessTree.ItemsSource = accessTree.CategoryNodes;
                this.DataTree.CheckBoxSettings.CheckBoxVisibility = System.Windows.Visibility.Hidden;
                this.AccessTree.CheckBoxSettings.CheckBoxVisibility = System.Windows.Visibility.Hidden;
                this.Submit.Visibility = System.Windows.Visibility.Hidden;
                this.Cancel.Visibility = System.Windows.Visibility.Hidden;
                this.RoleName.IsReadOnly = true;
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "RoleManager.Cancel_Click.\r\n{0}", ex.ToString());
            }
        }

        private void AccessExpandAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.AccessTree != null&&this.AccessTree.Nodes.Count>0)
                {
                    this.SetNodeExpandedState(this.AccessTree.Nodes, true);
                }
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "RoleManager.ExpandAll_Click.\r\n{0}", ex.ToString());
            }
        }

        private void DataExpandAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.DataTree != null && this.DataTree.Nodes.Count > 0)
                {
                    this.SetNodeExpandedState(this.DataTree.Nodes, true);
                }
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "RoleManager.ExpandAll_Click.\r\n{0}", ex.ToString());
            }
        }

        private void SetNodeExpandedState(IEnumerable<XamDataTreeNode> nodes, bool expandNode)
        {
            if (nodes.Count() > 0)
            {
                foreach (XamDataTreeNode item in nodes)
                {
                    item.IsExpanded = expandNode;
                    this.SetNodeExpandedState(item.Nodes, expandNode);
                }
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = sender as Button;
                int roleId = (int)btn.Tag;
                this._SelectRole = this._roleDatas.SingleOrDefault(r => r.RoleId == roleId);
                ConsoleClient.Instance.DeleteRole(roleId, DeleteResult);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "DeleteButton_Click.\r\n{0}", ex.ToString());
            }
        }

        private void DeleteResult(bool isSuccess)
        {
            try
            {
                this.Dispatcher.BeginInvoke((Action<bool>)delegate(bool result)
                {
                    if (result)
                    {
                        this._RoleGridDatas.Remove(this._RoleGridDatas.SingleOrDefault(r => r.RoleId == this._SelectRole.RoleId));
                    }
                }, isSuccess);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "RoleManager/DeleteResult.\r\n{0}", ex.ToString());
            }
        }
    }
}
