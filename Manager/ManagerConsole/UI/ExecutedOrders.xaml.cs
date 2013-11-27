using Infragistics.Windows.Reporting;
using ManagerConsole.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for ExecutedOrders.xaml
    /// </summary>
    public partial class ExecutedOrders : UserControl
    {
        private ManagerConsole.MainWindow _App;
        private ObservableCollection<AccountGroup> _AccountGroups = new ObservableCollection<AccountGroup>();
        public ExecutedOrders()
        {
            InitializeComponent();
            this.BindingData();
            this.AttachEvent();
        }

        private void AttachEvent()
        {
            this._App.InitDataManager.OnDeleteOrderSetRowBackColorEvent += new InitDataManager.DeleteOrderFromExecuteOrderGridHandler(this.DeleteOrderFromExecuteOrderGrid);
        }

        private void BindingData()
        {
            this._App = (ManagerConsole.MainWindow)Application.Current.MainWindow;
            this._ExecutedOrderListGrid.ItemsSource = this._App.InitDataManager.ExecutedOrders;
            this.GetComboListData();
        }

        private void GetComboListData()
        {
            AccountGroup allGroup = new AccountGroup();
            allGroup.Code = "All";
            foreach (AccountGroup group in this._App.InitDataManager.SettingsManager.GetAccountGroups())
            {
                this._AccountGroups.Add(group);
            }
            this._AccountGroups.Insert(0, allGroup);
            this._AccountGroupCombo.ItemsSource = this._AccountGroups;
            this._AccountGroupCombo.DisplayMemberPath = "Code";
            this._AccountGroupCombo.SelectedIndex = 0;
            this._AccountGroupCombo.SelectedValuePath = "Id";
            this._AccountGroupCombo.SelectedItem = allGroup;
        }

        void VariationText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Subtract)
            {
                TextBox textBox = sender as TextBox;
                string str = textBox.Text;
                if (str.Contains("-"))
                {
                    e.Handled = true;
                    return;
                }
            }

            if (!((e.Key >= Key.D0 && e.Key <= Key.D9) || (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)) && e.Key != Key.Subtract)
            {
                e.Handled = true;
            }
        }

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            Button btn = (Button)sender;
            switch (btn.Name)
            {
                case "_PrintSummaryOrderBtn":
                    this.PrintGrid(this._ExecutedOrderSummaryGrid);
                    break;
                case "_PrintListOrderBtn":
                    this.PrintGrid(this._ExecutedOrderListGrid);
                    break;
            }
        }

        private void ExecutedOrderTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabControl tab = sender as TabControl;
            if (tab.SelectedIndex == 0)
            {
                this._ListToolbar.Visibility = Visibility.Visible;
                this._SummaryToolbar.Visibility = Visibility.Collapsed;
            }
            else
            {
                this._ListToolbar.Visibility = Visibility.Collapsed;
                this._SummaryToolbar.Visibility = Visibility.Visible;
            }
        }

        private void PrintGrid(Infragistics.Controls.Grids.XamGrid printGrid)
        {
            Report reportObj = new Report();
            EmbeddedVisualReportSection section = new EmbeddedVisualReportSection(printGrid);
            reportObj.Sections.Add(section);
           // progressInfo.Report = reportObj;
            reportObj.Print(true, false);
        }

        void ExecutedOrderListGrid_InitializeRow(object sender, Infragistics.Controls.Grids.InitializeRowEventArgs e)
        {
            Order order = e.Row.Data as Order;
            Style style = new Style(typeof(Infragistics.Controls.Grids.CellControl));
            style.Setters.Add(new Setter(BackgroundProperty, order.IsBuyBrush));

            e.Row.CellStyle = style;
        }

        void DeleteOrderFromExecuteOrderGrid(Order deletedOrder)
        {
            Style style = new Style(typeof(Infragistics.Controls.Grids.CellControl));
            style.Setters.Add(new Setter(BackgroundProperty, new SolidColorBrush(Colors.Gray)));
            int index = this._App.InitDataManager.ExecutedOrders.IndexOf(deletedOrder);
            this._ExecutedOrderListGrid.Rows[index].CellStyle = style;
        }
    }
}
