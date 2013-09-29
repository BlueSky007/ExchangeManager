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
        public ChangePassword()
        {
            InitializeComponent();
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.newPassword.Password == this.confirmPassword.Password)
            {
                bool isSuccess = ConsoleClient.Instance.ChangePassword(this.currentPassword.Password, this.newPassword.Password);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.ChangePasswordDialog.Close();
        }
    }
}
