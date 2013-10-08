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
using System.Windows;
using ManagerConsole.Model;
using Manager.Common;

namespace ManagerConsole.UI
{
    /// <summary>
    /// UserTileControl.xaml 的交互逻辑
    /// </summary>
    public partial class UserTileControl : UserControl
    {
        private bool _isNewUser;
        private UserData _user;
        private Action<UserData> _AddSuccessAction;
        private Action<bool,UserData> _DeleteUser;

        public UserTileControl()
        {
            InitializeComponent();
        }

        public UserTileControl(Guid userId, string userName, int roleId,string roleName,bool isNewUser,Action<UserData> AddSuccessAction)
        {
            this._isNewUser = isNewUser;
            this._user = new UserData();
            this._user.UserId = userId;
            this._user.RoleId = roleId;
            this._user.UserName = userName;
            this._user.RoleName = roleName;
            this._AddSuccessAction = AddSuccessAction;
            InitializeComponent();
            if (isNewUser)
            {
                this.Edit_Click(null, null);
                this.ChangePassword_Click(null, null);
                this.Cancel.Visibility = System.Windows.Visibility.Hidden;
            }
            else
            {
                this.UserName.Text = userName;
                this.RoleName_Text.Text = roleName;
                this.UserName.IsReadOnly = true;
                this.NewPassword.IsReadOnly = true;
                this.RoleName_List.SelectedValue = roleId;
            }
        }

        public UserTileControl(UserData userData, bool isNewUser, Action<bool,UserData> DeleteUser)
        {
            InitializeComponent();
            this._isNewUser = isNewUser;
            this._user = new UserData();
            this._user.UserId = userData.UserId;
            this._user.RoleId = userData.RoleId;
            this._user.UserName = userData.UserName;
            this._user.RoleName = userData.RoleName;
            this._DeleteUser = DeleteUser;
            
            this.UserName.Text = userData.UserName;
            this.RoleName_Text.Text = userData.RoleName;
            this.UserName.IsReadOnly = true;
            this.NewPassword.IsReadOnly = true;
            this.RoleName_List.SelectedValue = userData.RoleId;
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Edit.Visibility = Visibility.Hidden;
                this.Submit.Visibility = Visibility.Visible;
                this.Cancel.Visibility = Visibility.Visible;
                this.UserName.IsReadOnly = false;
                this.NewPassword.Visibility = System.Windows.Visibility.Hidden;
                this.RoleName_Text.Visibility = Visibility.Hidden;
                this.Role_View.Visibility = System.Windows.Visibility.Hidden;
                this.RoleName_List.Visibility = Visibility.Visible;
                this.Role_LabelEdit.Visibility = System.Windows.Visibility.Visible;
                this.ChangePassword.Visibility = System.Windows.Visibility.Visible;
                if (this._isNewUser)
                {
                    this.Delete.Visibility = System.Windows.Visibility.Hidden;
                }
                else
                {
                    this.Delete.Visibility = System.Windows.Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "Edit_Click.\r\n{0}", ex.ToString());
            }
        }

        private void ChangePassword_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                this.ChangePassword.Visibility = System.Windows.Visibility.Hidden;
                this.NewPassword.Visibility = System.Windows.Visibility.Visible;
                this.NewPassword.IsReadOnly = false;
                this.NewPassword.Text = string.Empty;
                this.ConfirmPassword.Visibility = System.Windows.Visibility.Visible;
                this.Confirm.Visibility = System.Windows.Visibility.Visible;
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "ChangePassword_Click.\r\n{0}", ex.ToString());
            }
        }

        private void Cancel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                this.Edit.Visibility = Visibility.Visible;
                this.NewPassword.Visibility = System.Windows.Visibility.Visible;
                this.RoleName_Text.Visibility = Visibility.Visible;
                this.Role_View.Visibility = System.Windows.Visibility.Visible;
                this.ChangePassword.Visibility = System.Windows.Visibility.Visible;
                this.Submit.Visibility = Visibility.Hidden;
                this.Cancel.Visibility = Visibility.Hidden;
                this.Delete.Visibility = System.Windows.Visibility.Hidden;
                this.UserName.IsReadOnly = true;
                this.RoleName_List.Visibility = Visibility.Hidden;
                this.Role_LabelEdit.Visibility = System.Windows.Visibility.Hidden;
                this.ChangePassword.Visibility = System.Windows.Visibility.Hidden;
                this.NewPassword.IsReadOnly = true;
                this.ConfirmPassword.Visibility = System.Windows.Visibility.Hidden;
                this.Confirm.Visibility = System.Windows.Visibility.Hidden;
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
                    RoleData role = (RoleData)this.RoleName_List.SelectedItem;

                    if (this._isNewUser)
                    {
                        user.UserId = Guid.NewGuid();
                        user.UserName = this.UserName.Text;
                        user.RoleId = role.RoleId;
                        user.RoleName = role.RoleName;
                        this._user = user;
                        string password = this.NewPassword.Text;
                        ConsoleClient.Instance.UpdateUser(user, password, this._isNewUser, EndAddNewUser);
                    }
                    else
                    {
                        user.UserId = this._user.UserId;
                        user.UserName = this.UserName.Text;
                        if (this._user.UserName == this.UserName.Text)
                        {
                            user.UserName = string.Empty;
                        }
                        if (role.RoleId != this._user.RoleId)
                        {
                            user.RoleId = role.RoleId;
                            user.RoleName = role.RoleName;
                        }
                        else
                        {
                            user.RoleId = 0;
                            user.RoleName = string.Empty;
                        }

                        string password = string.Empty;
                        if (this.ChangePassword.Visibility == System.Windows.Visibility.Hidden)
                        {
                            password = this.NewPassword.Text;
                        }
                        ConsoleClient.Instance.UpdateUser(user, password, false, EndAddNewUser);
                    }
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
            if (this.ChangePassword.Visibility == System.Windows.Visibility.Hidden)
            {
                if (this.NewPassword.Text != this.Confirm.Text)
                {
                    this.Message.Content = "两次填写密码不一致";
                    return false;
                }
            }
            if (this.RoleName_List.SelectedItem==null)
            {
                this.Message.Content = "请选择角色";
                return false;
            }
            if (string.IsNullOrEmpty(this.UserName.Text))
            {
                this.Message.Content = "请输入用户名";
                return false;
            }
            if (role.RoleId != this._user.RoleId && this._user.UserName == this.UserName.Text)
            {
                if (this.ChangePassword.Visibility == System.Windows.Visibility.Visible)
                {
                    return false;
                }
                else if (string.IsNullOrEmpty(this.NewPassword.Text))
                {
                    return false;
                }
            }
            return true;
        }

        private void EndAddNewUser(bool isSuccess)
        {
            try
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
                         this.UserName.Text = "";
                         this.NewPassword.Text = "";
                         this.Confirm.Text = "";
                         this.Message.Foreground = Brushes.Green;
                         this.Message.Content = "录入成功";
                         UserData user = new UserData();
                         this._AddSuccessAction(this._user);
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
                this.RoleName_List.ItemsSource = ConsoleClient.Instance.GetRoles();
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "RoleName_List_Loaded.\r\n{0}", ex.ToString());
            }
        }

        private void Delete_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                ConsoleClient.Instance.DeleteUser(this._user.UserId, DeleteResult);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "DeleteClick.\r\n{0}", ex.ToString());
                
            }
        }

        private void DeleteResult(bool isSuccess)
        {
            this.Dispatcher.BeginInvoke((Action<bool>)delegate(bool result)
            {
                if (!result)
                {
                    this.Message.Foreground = Brushes.Red;
                    this.Message.Content = "删除失败";
                }
                else
                {
                    this._DeleteUser(result, this._user);
                }
            }, isSuccess);
        }
    }
}
