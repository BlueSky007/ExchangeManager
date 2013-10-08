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
        public RoleManagerControl()
        {
            InitializeComponent();
        }

        private void AddRole_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.RoleManager.Items.Count <= 0)
            {
                List<RoleData> roles = ConsoleClient.Instance.GetRoles();
                foreach (RoleData item in roles)
                {
                    XamTile tile = new XamTile();
                    tile.Header = item.RoleName;
                    tile.Content = new RoleTileControl(item.RoleId, item.RoleName, false, AddNewRole);
                    this.RoleManager.Items.Add(tile);
                }
            }
        }
        private void AddNewRole(bool result)
        {
        }
    }
}
