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
using ManagerConsole.Model;
using ManagerConsole.ViewModel;

namespace ManagerConsole.UI
{
    /// <summary>
    /// Interaction logic for QuotationSourceControl.xaml
    /// </summary>
    public partial class QuotationSourceControl : UserControl
    {
        public QuotationSourceControl()
        {
            InitializeComponent();
            this.Loaded += QuotationSourceControl_Loaded;
        }

        private void QuotationSourceControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataGrid.ItemsSource = QuotationConfigData.Instance.QuotationSources;
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            this.AddNewDialog.Visibility = Visibility.Visible;
            //this.AddNewDialog.Show();
            //this.AddNewDialog.BringToFront();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            QuotationSource source = new QuotationSource();
            source.Name = this.NameTextBox.Text;
            source.AuthName = this.AuthNameTextBox.Text;
            source.Password = this.PasswordTextBox.Text;

            //ConsoleClient.Instance.
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.AddNewDialog.Visibility = Visibility.Collapsed;
            //this.AddNewDialog.Close();
        }
    }
}
