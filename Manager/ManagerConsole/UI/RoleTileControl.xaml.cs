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
using Manager.Common;

namespace ManagerConsole.UI
{
    /// <summary>
    /// RoleTileControl.xaml 的交互逻辑
    /// </summary>
    public partial class RoleTileControl : UserControl
    {
        public int TileId { get; set; }
        private Action<bool,RoleData> _AddNewRole;
        private Action<RoleData> _DeleteRole;
        private RoleData _RoleData;
        private RoleData _AllPermissions;
        private int _EditType;
        private bool _isNewRole;


        public RoleTileControl()
        {
            InitializeComponent();
        }

        public RoleTileControl( bool isNewRole,RoleData roleData,RoleData allPermissions, Action<bool,RoleData> RoleMangerFunction,Action<RoleData> DeleteRole)
        {
            InitializeComponent();
            this._RoleData = roleData;
            this._AddNewRole = RoleMangerFunction;
            this._DeleteRole = DeleteRole;
            this._AllPermissions = allPermissions;
            this._isNewRole = isNewRole;
            if (isNewRole)
            {
                Edit_Click(null, null);
                this.Delete.Visibility = System.Windows.Visibility.Hidden;
            }
            else
            {
                this.RoleName.Text = roleData.RoleName;
            }
        }


        private void AccessTree_Loaded(object sender, RoutedEventArgs e)
        {
            AccessPermissionTree tree = this.BuileAccessTree(this._RoleData.AccessPermissions);
            this.AccessTree.ItemsSource = tree.CategoryNodes;
            this.AccessTree.CheckBoxSettings.CheckBoxVisibility = System.Windows.Visibility.Hidden;
        }

        private void DataTree_Loaded(object sender, RoutedEventArgs e)
        {
            DataPermissionTree tree = this.BuildDataPermissionTree(this._RoleData.DataPermissions);
            this.DataTree.ItemsSource = tree.ExChangeSystemNodes;
            this.DataTree.CheckBoxSettings.CheckBoxVisibility = System.Windows.Visibility.Hidden;
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

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            this.Edit.Visibility = System.Windows.Visibility.Hidden;
            this.Submit.Visibility = System.Windows.Visibility.Visible;
            this.Cancel.Visibility = System.Windows.Visibility.Visible;
            this.RoleName.IsReadOnly = false;
            this.AccessTree.CheckBoxSettings.CheckBoxVisibility = System.Windows.Visibility.Visible;
            this.DataTree.CheckBoxSettings.CheckBoxVisibility = System.Windows.Visibility.Visible;
            this._EditType = 0;
            AccessPermissionTree accessTree = this.BuileAccessTree(this._AllPermissions.AccessPermissions);
            this.AccessTree.ItemsSource = accessTree.CategoryNodes;
            DataPermissionTree dataTree = this.BuildDataPermissionTree(this._AllPermissions.DataPermissions);
            this.DataTree.ItemsSource = dataTree.ExChangeSystemNodes;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Edit.Visibility = System.Windows.Visibility.Visible;
            this.Submit.Visibility = System.Windows.Visibility.Hidden;
            this.Cancel.Visibility = System.Windows.Visibility.Hidden;
            this.RoleName.IsReadOnly = true;
            this.AccessTree.CheckBoxSettings.CheckBoxVisibility = System.Windows.Visibility.Hidden;
            this.DataTree.CheckBoxSettings.CheckBoxVisibility = System.Windows.Visibility.Hidden;
            AccessPermissionTree accessTree = this.BuileAccessTree(this._RoleData.AccessPermissions);
            this.AccessTree.ItemsSource = accessTree.CategoryNodes;
            DataPermissionTree dataTree = this.BuildDataPermissionTree(this._RoleData.DataPermissions);
            this.DataTree.ItemsSource = dataTree.ExChangeSystemNodes;
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this._EditType != 0)
                {
                    RoleData newRole = new RoleData();
                    if (this._EditType > 1)
                    {
                        if (!this._isNewRole)
                        {
                            newRole.RoleId = this._RoleData.RoleId;
                        }
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
                    }
                    if (this._EditType % 2 == 1)
                    {
                        newRole.RoleName = this.RoleName.Text;
                    }
                    this._RoleData = newRole;
                    if (CheckNewRole(newRole))
                    {
                        ConsoleClient.Instance.UpdateRole(newRole, this._isNewRole, EditResult);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "Submit_Click.\r\n{0}", ex.ToString());
            }
            //ConsoleClient.Instance.UpdateRolePermission(this._roleId, this._EditType, this.RoleName.Text, this._NewAccessTree, this._NewDataTree, EditResult);
        }
            
        private bool CheckNewRole(RoleData role)
        {
            if (this._isNewRole)
            {
                if (string.IsNullOrEmpty(role.RoleName))
                {
                    return false;
                }
                if (role.AccessPermissions.Count == 0)
                {
                    return false;
                }
                if (role.DataPermissions.Count == 0)
                {
                    return false;
                }
            }
            return true;
        }

        private void EditResult(bool isSuccess)
        {
            if (this._isNewRole)
            {
                this.Dispatcher.BeginInvoke((Action<bool>)delegate(bool result)
                {
                    this._AddNewRole(result,this._RoleData);
                }, isSuccess);
            }
            else
            {
                this.Dispatcher.BeginInvoke((Action<bool>)delegate(bool result)
                {
                    this.Message.Content = "Edit Success";
                }, isSuccess);
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ConsoleClient.Instance.DeleteRole(this._RoleData.RoleId, DeleteResult);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "RoleManager/Delete_Click.\r\n{0}", ex.ToString());
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
                        this._DeleteRole(this._RoleData);
                    }
                }, isSuccess);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "RoleManager/DeleteResult.\r\n{0}", ex.ToString());
            }
        }

        private void NodeCheckedChanged(object sender, NodeEventArgs e)
        {
            if (this._EditType == 1)
            {
                this._EditType = 3;
            }
            else if(this._EditType == 0 )
            {
                this._EditType = 2;
            }
        }

        private void RoleName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this._EditType == 2)
            {
                this._EditType = 3;
            }
            else if (this._EditType == 0)
            {
            this._EditType = 1;
        }
    }
}
}
