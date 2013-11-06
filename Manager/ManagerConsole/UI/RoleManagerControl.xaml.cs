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
                FunctionGridData allfunctionData = new FunctionGridData();
                allfunctionData.CastFunctionToGridData(new List<RoleFunctonPermission>(), this._AllFunctions);
                this._FunctionGridDatas = allfunctionData;
                DataPermissionGridData allDataPermissions = new DataPermissionGridData();
                allDataPermissions.CastDataPermissionToGridData(new List<RoleDataPermission>(), this._AllData);
                this._DataPermissionGridDatas = allDataPermissions;

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
                        this._RoleGridDatas.Add(new RoleGridData(role, true, true, true));
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
                dataPermissionGrid.CastDataPermissionToGridData(this._AllData, role.DataPermissions);
                this._DataPermissionGridDatas = dataPermissionGrid;
                this.FunctionPermission.ItemsSource = this._FunctionGridDatas.CategoryDatas;
                this.DataPermission.ItemsSource = this._DataPermissionGridDatas.IExchangeCodes;
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
                newRole.FunctionPermissions = this._FunctionGridDatas.CastGridDataToFunction();
                newRole.DataPermissions = this._DataPermissionGridDatas.CastGridDataToDataPermission();
                newRole.RoleId = this._SelectRole.RoleId;
                this._SelectRole.RoleName = this.RoleName.Text;
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

        private void AccessExpandAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //if (this.AccessTree != null&&this.AccessTree.Nodes.Count>0)
                //{
                //    this.SetNodeExpandedState(this.AccessTree.Nodes, true);
                //}
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

        private void FunctionPermission_RowExitedEditMode(object sender, EditingRowEventArgs e)
        {
            int level = e.Row.Level;
        }
    }
}
