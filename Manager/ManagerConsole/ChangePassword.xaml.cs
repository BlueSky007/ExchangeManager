using Infragistics.Controls.Interactions;
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
using System.Windows.Shapes;
using ManagerConsole.Model;

namespace ManagerConsole
{
    /// <summary>
    /// ChangePassword.xaml 的交互逻辑
    /// </summary>
    public partial class ChangePassword : XamDialogWindow
    {
        private bool _IsInUserManager;
        private bool _IsNewUser;
        private Action<string> _UpdateNewPassword;
        private Guid _UserId;
        public ChangePassword()
        {
            InitializeComponent();
            this._IsNewUser = false;
            this._IsInUserManager = false;
        }

        public ChangePassword(bool isNewUser,Guid userId,Action<string> UpdateNewPassword)
        {
            InitializeComponent();
            if (isNewUser)
            {
                this.currentPassword.Visibility = System.Windows.Visibility.Hidden;
            }
            this._UserId = userId;
            this._IsNewUser = isNewUser;
            this._IsInUserManager = true;
            this._UpdateNewPassword = UpdateNewPassword;
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.newPassword.Password == this.confirmPassword.Password)
            {
                if (this._IsInUserManager)
                {
                    if (this._IsNewUser)
                    {
                        this._UpdateNewPassword(this.newPassword.Password);
                    }
                    else
                    {
                        ConsoleClient.Instance.ChangePassword(this.currentPassword.Password, this.newPassword.Password, this._UserId);
                    }
                }
                else
                {
                    bool isSuccess = ConsoleClient.Instance.ChangePassword(this.currentPassword.Password, this.newPassword.Password, Guid.Empty);
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.ChangePasswordDialog.Close();
        }
    }
}
