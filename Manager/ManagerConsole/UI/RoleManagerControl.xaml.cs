using Infragistics.Controls.Layouts;
using Manager.Common;
using ManagerConsole.Model;
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

namespace ManagerConsole.UI
{
    /// <summary>
    /// RoleManagerControl.xaml 的交互逻辑
    /// </summary>
    public partial class RoleManagerControl : UserControl
    {
        private List<RoleData> _roleDatas;
        private RoleData _AllRolePermissions;
        public RoleManagerControl()
        {
            InitializeComponent();
        }

        private void AddRole_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                XamTile tile = new XamTile();
                tile.Header = "New Role";
                tile.CloseAction = TileCloseAction.RemoveItem;
                tile.Content = new RoleTileControl(true, this._AllRolePermissions, this._AllRolePermissions, AddNewRole,DeleteRole);
                if (this.RoleManager.Items.Contains(tile))
                {
                    this.RoleManager.Items.Remove(tile);
                }
                tile.IsMaximized = true;
                this.RoleManager.Items.Add(tile);
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
                if (this.RoleManager.Items.Count <= 0)
                {
                    RoleData data = new RoleData();
                    data = ConsoleClient.Instance.GetAllPermission();
                    List<RoleData> roles = ConsoleClient.Instance.GetRoles();
                    this._roleDatas = roles;
                    this._AllRolePermissions = data;
                    foreach (RoleData item in roles)
                    {
                        if (item.RoleId != 1)
                        {
                            XamTile tile = new XamTile();
                            tile.Header = item.RoleName;
                            tile.Content = new RoleTileControl(false, item, data, AddNewRole,DeleteRole);
                            this.RoleManager.Items.Add(tile);
                        }
                    }
                }
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
                this.RoleManager.Items.Add(tile);
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
                this.RoleManager.Items.Remove(tile);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "DeleteRole.\r\n{0}", ex.ToString());
            }
        }
    }
}
