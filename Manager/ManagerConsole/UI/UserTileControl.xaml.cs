using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using ManagerConsole.Model;
using Manager.Common;
using Infragistics.Controls.Interactions;
using System.Collections.ObjectModel;

namespace ManagerConsole.UI
{
    /// <summary>
    /// UserTileControl.xaml 的交互逻辑
    /// </summary>
    public partial class UserTileControl : XamDialogWindow
    {
        private bool _IsNewUser;
        private UserData _user;
        private ObservableCollection<RoleData> _roles;
        private Action<bool,UserData> _UpdateSuccessAction;

        public UserTileControl()
        {
            InitializeComponent();
        }

        public UserTileControl(Guid userId, string userName, int roleId, string roleName, bool isNewUser, ObservableCollection<RoleData> role, Action<bool, UserData> AddSuccessAction)
        {
            this._IsNewUser = isNewUser;
            this._user = new UserData();
            this._user.UserId = userId;
            this._user.UserName = userName;
            this._roles = role;
            this._UpdateSuccessAction = AddSuccessAction;
            InitializeComponent();
            this.RoleName_List.ItemsSource = this._roles;
            if (isNewUser)
            {
                this.Cancel.Visibility = System.Windows.Visibility.Hidden;
            }
            else
            {
                this.UserName.Text = userName;

               
                this.RoleName_List.SelectedItem = roleId;
                this.RoleName_List.ItemsSource = this._roles;
            }
        }

        public UserTileControl(UserData userData, ObservableCollection<RoleData> roles, bool isNewUser, Action<bool, UserData> AddSuccessAction)
        {
            try
            {
                InitializeComponent();
                this._IsNewUser = isNewUser;
                this._user = userData;
                this._roles = roles;
                this.RoleName_List.ItemsSource = this._roles;
                this._UpdateSuccessAction = AddSuccessAction;
                if (isNewUser)
                {
                    this.UserName.Text = "";
                    this.NewPassword.Password = string.Empty;
                    this.Confirm.Password = string.Empty;
                }
                else
                {
                    this.NewPassword.Password = "12345678";
                    this.Confirm.Password = "12345678";
                    this.UserName.Text = userData.UserName;
                    foreach (RoleData item in userData.Roles)
                    {
                        this.RoleName_List.SelectedItems.Add(item);
                    }
                }
                
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "UserTileControl.\r\n{0}", ex.ToString());
            }
        }

        private void Cancel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                this.Close();
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "Cancel_Click.\r\n{0}", ex.ToString());
            }
        }

        private void Submit_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                if (this.CheckSubmitData())
                {
                    UserData user = new UserData();
                    string password = string.Empty;
                    foreach (var item in this.RoleName_List.SelectedItems)
                    {
                            user.Roles.Add((RoleData)item);
                    }
                    if (this._IsNewUser)
                    {
                        this._user.UserId = Guid.NewGuid();
                        password = this.NewPassword.Password;
                        this._user.Roles=user.Roles;
                        this._user.UserName = this.UserName.Text;
                    }
                    else
                    {
                        if (this.NewPassword.Password != "12345678")
                        {
                            password = this.NewPassword.Password;
                        }                       
                    }
                    user.UserId = this._user.UserId;
                    user.UserName = this._user.UserName;
                    ConsoleClient.Instance.UpdateUser(user, password, this._IsNewUser, EndUpdateUser);
                    
                }
            }

            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "Submit_Click.\r\n{0}", ex.ToString());
            }
        }

        private bool CheckSubmitData()
        {
            this.Message.Foreground = Brushes.Red;
            RoleData role = (RoleData)this.RoleName_List.SelectedItem;
            if (this.NewPassword.Password != this.Confirm.Password)
            {
                this.Message.Content = "两次填写密码不一致";
                return false;
            }
            if (this.RoleName_List.SelectedItems.Count == 0)
            {
                this.Message.Content = "请选择角色";
                return false;
            }
            if (string.IsNullOrEmpty(this.UserName.Text))
            {
                this.Message.Content = "请输入用户名";
                return false;
            }
            return true;
        }

        private void EndEidtUser(bool isSuccess)
        {
            this.Dispatcher.BeginInvoke((Action<bool>)delegate(bool result)
            {
                if (!result)
                {
                    this.Message.Foreground = Brushes.Red;
                    this.Message.Content = "录入失败";
                }
                else
                {
                   
                }
            }, isSuccess);
        }

        private void EndUpdateUser(bool isSuccess)
        {
            try
            {
                this.Dispatcher.BeginInvoke((Action<bool>)delegate(bool result)
                 {
                     if (result)
                     {
                         this.Message.Foreground = Brushes.Green;
                         if (this._IsNewUser)
                         {
                             this.Message.Content = "添加成功";
                             this.UserName.Text = string.Empty;
                             this.NewPassword.Password = string.Empty;
                             this.Confirm.Password = string.Empty;
                             this.RoleName_List.SelectedItems.Clear();
                         }
                         else
                         {
                             this.Message.Content = "修改成功";
                         }
                         this._UpdateSuccessAction(this._IsNewUser,this._user);
                     }
                     else
                     {
                         this.Message.Foreground = Brushes.Red;
                         if (this._IsNewUser)
                         {
                             this.Message.Content = "添加失败";
                         }
                         else
                         {
                             this.Message.Content = "修改失败";
                         }
                         
                     }
                 }, isSuccess);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "EndAddNewUser.\r\n{0}", ex.ToString());

            }
        }

        private void RoleName_List_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                this.RoleName_List.ItemsSource = this._roles;
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "RoleName_List_Loaded.\r\n{0}", ex.ToString());
            }
        }
    }
}
