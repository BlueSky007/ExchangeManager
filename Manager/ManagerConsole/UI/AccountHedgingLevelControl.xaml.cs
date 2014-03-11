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
using AccountHedgingLevel = Manager.Common.ReportEntities.AccountHedgingLevel;

namespace ManagerConsole.UI
{
    /// <summary>
    /// Interaction logic for AccountHedgingLevelControl.xaml
    /// </summary>
    public partial class AccountHedgingLevelControl : UserControl
    {
        public AccountHedgingLevelControl()
        {
            InitializeComponent();
        }

        public void SetBinding(AccountHedgingLevel accountHedgingLevel)
        {
            if (string.IsNullOrEmpty(accountHedgingLevel.HedgingLevelString))
            {
                this.HedgingLevelGrid.DataContext = accountHedgingLevel;
                this.HedgingLevelGrid.Visibility = Visibility.Visible;
                this.NoLevelGrid.Visibility = Visibility.Collapsed;
                this.Height = 86;
            }
            else
            {
                this.HedgingLevelGrid.Visibility = Visibility.Collapsed;
                this.NoLevelGrid.Visibility = Visibility.Visible;
                this.Height = 50;
                this.NoLevelBorder.Height = 50;
            }
        }
    }
}
