﻿using Infragistics.Controls.Layouts;
using Infragistics.Controls.Editors;
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
using System.Collections.ObjectModel;
using ManagerConsole.ViewModel;
using System.Xml.Linq;

namespace ManagerConsole
{
    /// <summary>
    /// Interaction logic for UserManagerControl.xaml
    /// </summary>
    public partial class UserManagerControl : UserControl, IControlLayout
    {
        public UserManagerControl()
        {
            InitializeComponent();
            //ResourceDictionary resource = new ResourceDictionary();
            //resource.Source = new Uri(@"D:\Program Files (x86)\Infragistics\NetAdvantage 2013.1\WPF\Themes\Office2010Blue\Office2010Blue.xamGrid.xaml", UriKind.Absolute);
            //this.UserManager.Resources.MergedDictionaries.Add(resource);
            Principal.Instance.PermissionChanged += Instance_PermissionChanged;
            this.Instance_PermissionChanged();
            
        }

        
        private ObservableCollection<UserData> _AllUserData = new ObservableCollection<UserData>();
        private ObservableCollection<UserModel> _users;
        private ObservableCollection<RoleData> _roles = new ObservableCollection<RoleData>();
        //private ObservableCollection<RoleData> _NewRole;
        private UserModel _NewUser = new UserModel();
        //private string _Password;

        public bool IsAllowEdit { get; set; }
        public bool IsAllowDelete { get; set; }
        public bool IsAllowAdd { get; set; }
        public void Instance_PermissionChanged()
        {
            IsAllowAdd = Principal.Instance.HasPermission(ModuleCategoryType.UserManager, ModuleType.UserManager, "AddUser");
            IsAllowEdit = Principal.Instance.HasPermission(ModuleCategoryType.UserManager, ModuleType.UserManager, "EditUser");
            IsAllowDelete = Principal.Instance.HasPermission(ModuleCategoryType.UserManager, ModuleType.UserManager, "DeleteUser");
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                
                    ConsoleClient.Instance.GetRoles(delegate(List<RoleData> roles)
                    {
                        if (this._roles.Count == 0)
                        {
                            foreach (RoleData roleData in roles)
                            {
                                if (!this._roles.Contains(roleData))
                                {
                                    this._roles.Add(roleData);
                                }
                            }
                        }
                    });

                    ConsoleClient.Instance.GetUserData(this.InitUserTile);
                    this.DataContext = this;
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

                     //foreach (UserData item in userData)
                     //{
                     //    XamTile tile = new XamTile();
                     //    tile.CloseAction = TileCloseAction.DoNothing;
                     //    tile.Header = item.UserName;
                     //    tile.Content = new UserTileControl(item, this._roles, false, DeleteUser);
                     //    //this.UserManager.Items.Add(tile);
                     //}
                     this._AllUserData = new ObservableCollection<UserData>(userData);
                     ObservableCollection<UserModel> users = new ObservableCollection<UserModel>();
                     foreach (UserData item in userData)
                     {
                         UserModel user = new UserModel();
                         user.UserId = item.UserId;
                         user.UserName = item.UserName;
                         user.Password = string.Empty;
                         user.Roles = "";
                         foreach (RoleData role in item.Roles)
                         {
                             user.Roles += role.RoleName + ";";
                         }
                         users.Add(user);
                     }
                     this._users = users;
                     this.UserManager.ItemsSource = this._users;

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
                UserData user = new UserData();
                UserTileControl newUserDialog = new UserTileControl(user, this._roles, true, AddUserSuccess);
                this.UseManagerFrame.Children.Add(newUserDialog);
                newUserDialog.IsModal = true;
                newUserDialog.Show();
                newUserDialog.BringToFront();

            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "AddUser_Click.\r\n{0}", ex.ToString());
            }
        }

        public void AddUserSuccess(bool isNewUser, UserData user)
        {
            try
            {
                UserModel newUser = new UserModel();
                newUser.UserId = user.UserId;
                newUser.UserName = user.UserName;
                foreach (RoleData item in user.Roles)
                {
                    newUser.Roles += item.RoleName + ";";
                }
                this._users.Add(newUser);
                //this.UserManager.Items.Add(tile);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "AddUserSuccess.\r\n{0}", ex.ToString());
            }
        }

        private void RoleName_List_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                ((XamComboEditor)sender).ItemsSource = this._roles;
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "RoleName_List_Loaded.\r\n{0}", ex.ToString());
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            try
            {
                Guid userId = (Guid)btn.Tag;
                UserModel userModel = this._users.Single(u => u.UserId == userId);
                if (userId != Guid.Empty)
                {
                    if (btn.Name == "Edit")
                    {
                        UserData user = new UserData();
                        user.UserId = userModel.UserId;
                        user.UserName = userModel.UserName;
                        string[] roles = userModel.Roles.Split(';');
                        foreach (string item in roles)
                        {
                            RoleData role = this._roles.SingleOrDefault(r => r.RoleName == item);
                            if (role != null)
                            {
                                user.Roles.Add(role);
                            }
                        }
                        UserTileControl tile = new UserTileControl(user, this._roles, false, this.AddUserSuccess);
                        this.UseManagerFrame.Children.Add(tile);
                        tile.IsModal = true;
                        tile.Show();
                        tile.BringToFront();
                    }
                    else if (btn.Name == "Delete")
                    {

                        if (MessageBox.Show(App.MainFrameWindow, string.Format("确认删除{0}用户吗", userModel.UserName), "Warnning", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
                        {
                            ConsoleClient.Instance.DeleteUser(userId, this.DeleteUser);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "UserManagerButton_Click.\r\n{0}{1}", btn.Name, ex.ToString());
            }
        }

        public void DeleteUser(bool isSuccess, Guid userId)
        {
            try
            {
                this.Dispatcher.BeginInvoke((Action<bool>)delegate(bool result)
                {
                    if (!result)
                    {
                        MessageBox.Show("删除失败");
                    }
                    else
                    {
                        this._users.Remove(this._users.SingleOrDefault(u => u.UserId == userId));
                        MessageBox.Show("删除成功");
                    }
                }, isSuccess);

            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "UserManager/DeleteUser.\r\n{0}", ex.ToString());
            }
        }

        public string GetLayout()
        {
            string layout = string.Empty;
            
            return layout;
        }

        public void SetLayout(XElement layout)
        {
        }
    }
}
