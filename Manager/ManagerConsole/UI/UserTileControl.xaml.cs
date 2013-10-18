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
        private List<RoleData> _roles;
        private Action<bool, UserData> _AddSuccessAction;
        private Action<bool, UserData> _DeleteUser;

        public UserTileControl()
        {
            InitializeComponent();
        }

        public UserTileControl(Guid userId, string userName, int roleId, string roleName, bool isNewUser,List<RoleData> role, Action<bool,UserData> AddSuccessAction)
        {
            this._isNewUser = isNewUser;
            this._user = new UserData();
            this._user.UserId = userId;
            this._user.UserName = userName;
            this._roles = role;
            this._AddSuccessAction = AddSuccessAction;
            InitializeComponent();
            this.RoleName_List.ItemsSource = this._roles;
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
                this.RoleName_List.SelectedItem = roleId;
                this.RoleName_List.ItemsSource = this._roles;
            }
        }

        public UserTileControl(UserData userData,List<RoleData> roles, bool isNewUser, Action<bool,UserData> DeleteUser)
        {
            try
            {
                InitializeComponent();
                this._isNewUser = isNewUser;
                this._user = userData;
                this._DeleteUser = DeleteUser;
                this._roles = roles;
                this.UserName.Text = userData.UserName;
                string text = string.Empty;
                foreach (RoleData role in userData.Roles)
                {
                    text += role.RoleName + ",";
                }
                this.RoleName_Text.Text = text;
                this.UserName.IsReadOnly = true;
                this.NewPassword.IsReadOnly = true;
                this.RoleName_List.ItemsSource = this._roles;
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "UserTileControl.\r\n{0}", ex.ToString());
            }
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
                this.UserName.Text = this._user.UserName;
                string text = string.Empty;
                foreach (RoleData role in this._user.Roles)
                {
                    text += role.RoleName + ",";
                }
                this.RoleName_Text.Text = text;
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
                    user.UserId = Guid.NewGuid();
                    user.UserName = this.UserName.Text;
                    foreach (var item in this.RoleName_List.SelectedItems)
                    {
                        user.Roles.Add((RoleData)item);
                    }
                    string text = string.Empty;
                    foreach (RoleData role in user.Roles)
                    {
                        text += role.RoleName + ",";
                    }
                    this.RoleName_Text.Text = text;
                    if (this._isNewUser)
                    {
                        this._user = user;
                        string password = this.NewPassword.Text;
                        ConsoleClient.Instance.UpdateUser(user, password, this._isNewUser, EndAddNewUser);
                    }
                    else
                    {
                        if (this._user.UserName == this.UserName.Text)
                        {
                            user.UserName = string.Empty;
                        }
                        if (this._user.Roles.SequenceEqual(user.Roles))
                        {
                            user.Roles.Clear();
                            string password = string.Empty;
                            if (this.ChangePassword.Visibility == System.Windows.Visibility.Hidden)
                            {
                                password = this.NewPassword.Text;
                            }
                            ConsoleClient.Instance.UpdateUser(user, password, false, EndAddNewUser);
                        }
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
            }, isSuccess);
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
                         this._AddSuccessAction(true,this._user);
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
