using Infragistics.Controls.Layouts;
using Manager.Common;
using ManagerConsole.Model;
using ManagerConsole.UI;
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

namespace ManagerConsole
{
    /// <summary>
    /// Interaction logic for UserManagerControl.xaml
    /// </summary>
    public partial class UserManagerControl : UserControl
    {
        public UserManagerControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.UserManager.Items.Count == 0)
                {
                    ConsoleClient.Instance.GetUserData(this.InitUserTile);
                }
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "UserControl_Loaded.\r\n{0}", ex.ToString());
            }
        }

        private void InitUserTile(List<UserData> data)
        {
            try
            {
                this.Dispatcher.BeginInvoke((Action<List<UserData>>)delegate(List<UserData> userData)
                 {
                     foreach (UserData item in userData)
                     {
                         XamTile tile = new XamTile();
                         tile.CloseAction = TileCloseAction.DoNothing;
                         tile.Header = item.UserName;
                         tile.Content = new UserTileControl(item, false, DeleteUser);
                         this.UserManager.Items.Add(tile);
                     }
                 }, data);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "InitUserTile.\r\n{0}", ex.ToString());
            }
        }

        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                XamTile tile = new XamTile();
                tile.CloseAction = TileCloseAction.RemoveItem;
                tile.Header = "Add New User";
                tile.Content = new UserTileControl(Guid.Empty, "", 0, "", true, AddUserSuccess);
                if (this.UserManager.Items.Contains(tile))
                {
                    this.UserManager.Items.Remove(tile);
                }
                tile.IsMaximized = true;
                this.UserManager.Items.Add(tile);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "AddUser_Click.\r\n{0}", ex.ToString());
            }
        }

        public void AddUserSuccess(UserData user)
        {
            try
            {
                XamTile tile = new XamTile();
                tile.CloseAction = TileCloseAction.DoNothing;
                tile.Header = user.UserName;
                tile.Content = new UserTileControl(user.UserId, user.UserName, user.RoleId, user.RoleName, false, AddUserSuccess);
                this.UserManager.Items.Add(tile);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "AddUserSuccess.\r\n{0}", ex.ToString());
            }
        }

        public void DeleteUser(bool isSuccess,UserData user)
        {
            try
            {
                if (isSuccess)
                {
                    this.UserManager.Items.Remove(new UserTileControl(user, false, DeleteUser));
                }
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "UserManager/DeleteUser.\r\n{0}", ex.ToString());
            }
        }
    }
}
