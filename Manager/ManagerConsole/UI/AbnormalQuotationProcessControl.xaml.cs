using System;
using System.Collections;
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
    /// Interaction logic for AbnormalQuotationProcessControl.xaml
    /// </summary>
    public partial class AbnormalQuotationProcessControl : UserControl
    {
        public AbnormalQuotationProcessControl()
        {
            InitializeComponent();
        }

        public IEnumerable ItemsSource
        {
            get { return this.MainGrid.ItemsSource; }
            set { this.MainGrid.ItemsSource = value; }
        }
    }
}
