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
        private FunctionGridData _FunctionGridDatas;
        private DataPermissionGridData _DataPermissionGridDatas;
        private List<RoleFunctonPermission> _AllFunctions;
        private List<RoleDataPermission> _AllData;
        private RoleData _SelectRole;
        private bool _isNew;

        public static readonly DependencyProperty IsTeamLeaderProperty = DependencyProperty.Register("IsAllowAdd", typeof(bool), typeof(RoleManagerControl));

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
                int id = 0;
                foreach (RoleData item in this._roleDatas)
                {
                    if (item.RoleId >= id)
                    {
                        id = item.RoleId + 1;
                    }
                }
                this._SelectRole.RoleId = id;
                this.Submit.Visibility = System.Windows.Visibility.Visible;
                this.Cancel.Visibility = System.Windows.Visibility.Hidden;
                this.RoleName.IsReadOnly = false;
                this.RoleName.Text = string.Empty;
                FunctionGridData allfunctionData = new FunctionGridData();
                allfunctionData.CastFunctionToGridData(new List<RoleFunctonPermission>(), this._AllFunctions);
                this._FunctionGridDatas = allfunctionData;
                DataPermissionGridData allDataPermissions = new DataPermissionGridData();
                allDataPermissions.CastDataPermissionToGridData(new List<RoleDataPermission>(), this._AllData);
                this._DataPermissionGridDatas = allDataPermissions;
                this.FunctionPermission.ItemsSource = this._FunctionGridDatas.CategoryDatas;
                this.DataPermission.ItemsSource = this._DataPermissionGridDatas.IExchangeCodes;
                this.FunctionPermission.EditingSettings.AllowEditing = EditingType.Hover;
                this.DataPermission.EditingSettings.AllowEditing = EditingType.Hover;
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
                this.IsAllowAdd = ConsoleClient.Instance.HasPermission(new AccessPermission(ModuleCategoryType.UserManager,ModuleType.RoleManager,"Add"));
                this.IsAllowEdit = ConsoleClient.Instance.HasPermission(new AccessPermission(ModuleCategoryType.UserManager, ModuleType.RoleManager, "Edit"));
                this.IsAllowDelete = ConsoleClient.Instance.HasPermission(new AccessPermission(ModuleCategoryType.UserManager, ModuleType.RoleManager, "Delete"));
                List<RoleFunctonPermission> allFunction = ConsoleClient.Instance.GetAllFunctionPermission();
                List<RoleDataPermission> allData = ConsoleClient.Instance.GetAllDataPermissions();
                this._AllData = allData;
                this._AllFunctions = allFunction;
                List<RoleData> roles = ConsoleClient.Instance.GetRoles();
                this._roleDatas = new ObservableCollection<RoleData>(roles);
                this._RoleGridDatas = new ObservableCollection<RoleGridData>();
                foreach (RoleData role in roles)
                {
                    if (role.RoleId != 1)
                    {
                        this._RoleGridDatas.Add(new RoleGridData(role, this.IsAllowAdd, this.IsAllowDelete, this.IsAllowEdit));
                    }
                }
                this._AllFunctions = allFunction;
                this.RoleGrid.ItemsSource = this._RoleGridDatas;
                this.Submit.Visibility = System.Windows.Visibility.Hidden;
                this.Cancel.Visibility = System.Windows.Visibility.Hidden;
                this.FunctionPermission.EditingSettings.AllowEditing = EditingType.None;
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
                FunctionGridData functionGrid = new FunctionGridData();
                functionGrid.CastFunctionToGridData(role.FunctionPermissions, this._AllFunctions);
                this._FunctionGridDatas = functionGrid;
                DataPermissionGridData dataPermissionGrid = new DataPermissionGridData();
                dataPermissionGrid.CastDataPermissionToGridData(role.DataPermissions, this._AllData);
                this._DataPermissionGridDatas = dataPermissionGrid;
                this.FunctionPermission.ItemsSource = this._FunctionGridDatas.CategoryDatas;
                this.FunctionPermission.EditingSettings.AllowEditing = EditingType.None;
                this.DataPermission.ItemsSource = this._DataPermissionGridDatas.IExchangeCodes;
                this.DataPermission.EditingSettings.AllowEditing = EditingType.None;
                this.Submit.Visibility = System.Windows.Visibility.Hidden;
                this.Cancel.Visibility = System.Windows.Visibility.Hidden;
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "RoleManager/RoleGrid_CellClicked.\r\n{0}", ex.ToString());
            }
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
                this.Submit.Visibility = System.Windows.Visibility.Visible;
                this.Cancel.Visibility = System.Windows.Visibility.Visible;
                this.FunctionPermission.EditingSettings.AllowEditing = EditingType.Hover;
                this.DataPermission.EditingSettings.AllowEditing = EditingType.Hover;
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "EditButton_Click.\r\n{0}", ex.ToString());

            }
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RoleData newRole = new RoleData();
                if (!this._FunctionGridDatas.CheckData())
                {
                    MessageBox.Show("有部分操作权限没有赋值，请检查后再提交");
                    return;
                }
                if (!this._DataPermissionGridDatas.CheckData())
                {
                    MessageBox.Show("有部分数据权限没有赋值，请检查后再提交");
                    return;
                }
                if (string.IsNullOrEmpty(this.RoleName.Text))
                {
                    MessageBox.Show("请输入角色名！");
                    return;
                }
                newRole.FunctionPermissions = this._FunctionGridDatas.CastGridDataToFunction();
                newRole.DataPermissions = this._DataPermissionGridDatas.CastGridDataToDataPermission();
                newRole.RoleId = this._SelectRole.RoleId;
                this._SelectRole.RoleName = this.RoleName.Text;
                newRole.RoleName = this._SelectRole.RoleName;
                this._isNew = false;
                if (this._roleDatas.SingleOrDefault(r=>r.RoleId==this._SelectRole.RoleId)==null)
                {
                    this._isNew = true;
                }
                this._SelectRole = newRole;
                ConsoleClient.Instance.UpdateRole(newRole, this._isNew, EditResult);
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
                if (this._isNew)
                {
                    this.Dispatcher.BeginInvoke((Action<bool>)delegate(bool result)
                    {
                        if (result)
                        {
                            this._roleDatas.Add(this._SelectRole);
                            this._RoleGridDatas.Add(new RoleGridData(this._SelectRole, this.IsAllowAdd, this.IsAllowDelete, this.IsAllowEdit));
                            MessageBox.Show("添加成功");
                        }
                        else
                        {
                            MessageBox.Show("添加失败");
                        }
                    }, isSuccess);
                }
                else
                {
                    this.Dispatcher.BeginInvoke((Action<bool>)delegate(bool result)
                    {
                        if (result)
                        {
                            RoleData role = this._roleDatas.SingleOrDefault(r => r.RoleId == this._SelectRole.RoleId);
                            this._roleDatas.Remove(role);
                            this._roleDatas.Add(this._SelectRole);
                            role = this._SelectRole;
                            this._RoleGridDatas.SingleOrDefault(r => r.RoleId == this._SelectRole.RoleId).RoleName = this._SelectRole.RoleName;
                            MessageBox.Show("编辑成功");
                        }
                        else
                        {
                            MessageBox.Show("编辑失败");
                        }

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
                this.Submit.Visibility = System.Windows.Visibility.Hidden;
                this.Cancel.Visibility = System.Windows.Visibility.Hidden;
                this.RoleName.IsReadOnly = true;
                this.FunctionPermission.EditingSettings.AllowEditing = EditingType.None;
                this.DataPermission.EditingSettings.AllowEditing = EditingType.None;
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "RoleManager.Cancel_Click.\r\n{0}", ex.ToString());
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
                if (MessageBox.Show(App.MainWindow,string.Format("确认删除{0}角色吗？", this._SelectRole.RoleName), "", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No, MessageBoxOptions.DefaultDesktopOnly) == MessageBoxResult.Yes)
                {
                    ConsoleClient.Instance.DeleteRole(roleId, DeleteResult);
                }
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
                    else
                    {
                        MessageBox.Show("删除失败");
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
