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
using AccountOpenPostion = Manager.Common.ReportEntities.AccountStatusOrder;

namespace ManagerConsole.UI
{
    /// <summary>
    /// Interaction logic for AccountOrderStatus.xaml
    /// </summary>
    public partial class AccountOrderStatus : UserControl
    {
        public AccountOrderStatus()
        {
            InitializeComponent();
        }

        public void BindingGridData(List<AccountOpenPostion> accountOpenPostions)
        {
            this._OpenPositionGrid.ItemsSource = accountOpenPostions;
        }
    }
}
