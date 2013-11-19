using Infragistics.Windows.Reporting;
using Manager.Common.ReportEntities;
using ManagerConsole.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Printing;
using System.Linq;
using System.Printing;
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
using OrderType = Manager.Common.OrderType;

namespace ManagerConsole.UI
{
    /// <summary>
    /// Interaction logic for OrderSearchControl.xaml
    /// </summary>
    public partial class OrderSearchControl : UserControl
    {
        private ManagerConsole.MainWindow _App;
        private ObservableCollection<InstrumentClient> _InstrumentList = new ObservableCollection<InstrumentClient>();
        private ObservableCollection<AccountGroup> _AccountGroups = new ObservableCollection<AccountGroup>();
        public OrderSearchControl()
        {
            InitializeComponent();

            this.InilizeUI();
        }

        private void InilizeUI()
        {
            this._App = ((ManagerConsole.MainWindow)Application.Current.MainWindow);
            this._OrderTypeCombo.ItemsSource = System.Enum.GetNames(typeof(OrderType));
            this._OrderTypeCombo.SelectedIndex = 0;
            this.GetComboListData();
        }

        private void GetComboListData()
        {
            InstrumentClient allInstrument = new InstrumentClient();
            allInstrument.Code = "All";
            foreach (InstrumentClient instrument in this._App.InitDataManager.GetInstruments())
            {
                this._InstrumentList.Add(instrument);
            }

            this._InstrumentList.Insert(0, allInstrument);
            this._InstrumentCombo.ItemsSource = this._InstrumentList;
            this._InstrumentCombo.DisplayMemberPath = "Code";
            this._InstrumentCombo.SelectedIndex = 0;
            this._InstrumentCombo.SelectedValuePath = "Id";
            this._InstrumentCombo.SelectedItem = allInstrument;


            AccountGroup allGroup = new AccountGroup();
            allInstrument.Code = "All";
            foreach (AccountGroup group in this._App.InitDataManager.SettingsManager.GetAccountGroups())
            {
                this._AccountGroups.Add(group);
            }

            this._AccountGroupCombo.ItemsSource = this._AccountGroups;
            this._AccountGroupCombo.DisplayMemberPath = "Code";
            this._AccountGroupCombo.SelectedIndex = 0;
            this._AccountGroupCombo.SelectedValuePath = "Id";
            this._AccountGroupCombo.SelectedItem = allGroup;
        }

        private void ToolBar_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            switch (btn.Name)
            {
                case "_QueryButton":
                    this.QueryOrder();
                    break;
                case "_PrintBtn":
                    Report reportObj = new Report();
                    EmbeddedVisualReportSection section = new EmbeddedVisualReportSection(this._OrderSerchGrid);
                    reportObj.Sections.Add(section);

                    _OrderReportPreview.GeneratePreview(reportObj, false, false);

                    tbiPreview.IsSelected = true;
                    break;
            }
        }

        private void QueryOrder()
        {
            bool isExecute = this._OrderTypeCombo.SelectedIndex == 0 ? true:false;
            InstrumentClient instrument = (InstrumentClient)this._InstrumentCombo.SelectedItem;
            AccountGroup group = (AccountGroup)this._AccountGroupCombo.SelectedItem;
            DateTime fromDate = DateTime.Parse(this._FromDatePicker.Text);
            DateTime toDate = DateTime.Parse(this._ToDatePicker.Text);

            OrderType orderType = (OrderType)Enum.ToObject(typeof(OrderType), this._OrderTypeCombo.SelectedIndex);

            ConsoleClient.Instance.GetOrderByInstrument(instrument.Id, group.Id, orderType, isExecute, fromDate, toDate, GetOrderByInstrumentCallback);
        }

        private void GetOrderByInstrumentCallback(List<OrderQueryEntity> queryOrders)
        {
            this.Dispatcher.BeginInvoke((Action<List<OrderQueryEntity>>)delegate(List<OrderQueryEntity> result)
            {
                this._OrderSerchGrid.ItemsSource = result;
            }, queryOrders);
        }
    }
}
