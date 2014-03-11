using Infragistics.Windows.Reporting;
using Manager.Common.ReportEntities;
using ManagerConsole.Model;
using ManagerConsole.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Scheduler = iExchange.Common.Scheduler;

namespace ManagerConsole.UI
{
    /// <summary>
    /// Interaction logic for AccountStatusQuery.xaml
    /// </summary>
    public partial class AccountStatusQuery : UserControl
    {
        private ManagerConsole.MainWindow _App;
        private AccountStatusModel _AccountStatusModel;
        private ObservableCollection<Account> _AccountList;
        private Scheduler Scheduler = new Scheduler();
        private Scheduler.Action _QueryAccountStatusAction;
        private string ScheduleID;
        public AccountStatusQuery()
        {
            InitializeComponent();
            this._App = ((ManagerConsole.MainWindow)Application.Current.MainWindow);
            Thread thread = new Thread(new ThreadStart(delegate()
            {
                while (!this.InilizeUI())
                {
                    Thread.Sleep(800);
                }
            }));
            thread.IsBackground = true;
            thread.Start();
            this.AttachEvent();
        }

        private bool InilizeUI()
        {
            if (this._App.ExchangeDataManager.IsInitializeCompleted)
            {
                this.Dispatcher.BeginInvoke((Action)delegate()
                {
                    this.GetComboListData();
                });
                return true;
            }
            else
            {
                return false;
            }
        }

        private void GetComboListData()
        {
            this._AccountList = new ObservableCollection<Account>();
            this.ExchangeCmb.ItemsSource = this._App.ExchangeDataManager.ExchangeCodes;
            this.ExchangeCmb.SelectedItem = this._App.ExchangeDataManager.ExchangeCodes[0];

            string exchangeCode = this._App.ExchangeDataManager.ExchangeCodes[0];

            ExchangeSettingManager settingManager = this._App.ExchangeDataManager.GetExchangeSetting(exchangeCode);

            foreach (Account account in settingManager.GetAccounts())
            {
                this._AccountList.Add(account);
            }
            this._AccountList.OrderBy(P => P.Code);
            this.AccountCodeCmb.ItemsSource = this._AccountList;
            this.AccountCodeCmb.DisplayMemberPath = "Code";
            this.AccountCodeCmb.SelectedIndex = 0;
            this.AccountNameCmb.ItemsSource = this._AccountList;
            this.AccountNameCmb.DisplayMemberPath = "Code";
            this.AccountNameCmb.SelectedIndex = 0;

            //this.IsUseMarketPriceCmb.ItemsSource = Enum.GetNames(typeof(OrderTask));
        }
        private void AttachEvent()
        {
            this._QueryAccountStatusAction = new Scheduler.Action(this.QueryAccountStatusAction);
            this.LeftTradingSummaryControl.OnGridExpandChangeEvent += new TradingSummaryControl.GridExpandChangeHandler(this.OnGridExpandChange);
            this.LeftCenterTradingSummaryControl.OnGridExpandChangeEvent += new TradingSummaryControl.GridExpandChangeHandler(this.OnGridExpandChange);
            this.RightCenterTradingSummaryControl.OnGridExpandChangeEvent += new TradingSummaryControl.GridExpandChangeHandler(this.OnGridExpandChange);
            this.RightTradingSummaryControl.OnGridExpandChangeEvent += new TradingSummaryControl.GridExpandChangeHandler(this.OnGridExpandChange);
        }

        private void QueryButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.ScheduleID != null)
            {
                this.Scheduler.Remove(this.ScheduleID);
            }
            bool isRealTimeQuery = this.IsRealTimeQueryChk.IsChecked.Value;
            int interval = int.Parse(this.IntervalInput.Value.ToString());
            if (isRealTimeQuery)
            {
                this.ScheduleID = this.Scheduler.Add(this._QueryAccountStatusAction, null, DateTime.Now, DateTime.MaxValue, TimeSpan.FromSeconds(interval));
            }

            this.QueryAccountReportData();
        }

        private void QueryAccountStatusAction(object sender, object Args)
        {
            this.Dispatcher.BeginInvoke((Action)delegate()
            {
                this.QueryAccountReportData();
            }); 
        }

        //查询
        private void QueryAccountReportData()
        {
            this._BusyBusyDecorator.Visibility = Visibility.Visible;
            this.TradingSummaryPanel.Visibility = System.Windows.Visibility.Visible;
            string exchangeCode = this.ExchangeCmb.SelectedItem.ToString();
            string selectPrice = string.Empty;
            Account account = this.AccountCodeCmb.SelectedItem as Account;
            Guid accountId = account.Id;
            ConsoleClient.Instance.GetAccountReportData(exchangeCode, selectPrice, accountId, this.GetAccountReportDataCallback);
        }

        private void GetAccountReportDataCallback(AccountStatusQueryResult accountQueryData)
        {
            this.Dispatcher.BeginInvoke((Action<AccountStatusQueryResult>)delegate(AccountStatusQueryResult result)
            {
                this._BusyBusyDecorator.Visibility = Visibility.Collapsed;
                if (result == null) return;
                this._AccountStatusModel = new AccountStatusModel();
                this._AccountStatusModel.AccountStatusEntity = result.AccountStatusEntity;
                this._AccountStatusModel.AccountTradingSummary = result.AccountTradingSummary;
                this._AccountStatusModel.AccountCurrencies = result.AccountCurrencies;
                this._AccountStatusModel.AccountHedgingLevel = result.AccountHedgingLevel;
                this._AccountStatusModel.AccountOpenPostions = result.AccountOpenPostions;
                this._AccountStatusModel.FillAccountItems();

                this.SetBinding();
            }, accountQueryData);
        }

        private void SetBinding()
        {
            this.AccountBaseInforGrid.DataContext = this._AccountStatusModel.AccountStatusEntity;
            this.LeftTradingSummaryControl.ItemsSource = this._AccountStatusModel.LeftAccountStatusItems;
            this.LeftCenterTradingSummaryControl.ItemsSource = this._AccountStatusModel.LeftCenterAccountStatusItems;
            this.RightCenterTradingSummaryControl.ItemsSource = this._AccountStatusModel.RightCenterAccountStatusItems;
            this.RightTradingSummaryControl.ItemsSource = this._AccountStatusModel.RightAccountStatusItems;
            this._AccountHedgingLevel.SetBinding(this._AccountStatusModel.AccountHedgingLevel);
            
            this._AccountOrderStatusControl.BindingGridData(this._AccountStatusModel.AccountOpenPostions);
        }

        void OnGridExpandChange(bool isExpand, int rowIndex,int childItemCount)
        {
            this.LeftTradingSummaryControl.ExpandSubItemRow(isExpand, rowIndex, childItemCount);
            this.LeftCenterTradingSummaryControl.ExpandSubItemRow(isExpand, rowIndex, childItemCount);
            this.RightCenterTradingSummaryControl.ExpandSubItemRow(isExpand, rowIndex, childItemCount);
            this.RightTradingSummaryControl.ExpandSubItemRow(isExpand, rowIndex, childItemCount);
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void _PrintBtn_Click(object sender, RoutedEventArgs e)
        {
            this.PrintGrid();
        }

        private void PrintGrid()
        {
            Report reportObj = new Report();
            EmbeddedVisualReportSection section = new EmbeddedVisualReportSection(this.ReportContentGrid);
            reportObj.Sections.Add(section);

            section.PageFooter = "Manager";
            section.PageHeader = "Account Information";
            this._OrderReportPreview.Width = 1600;
            this._OrderReportPreview.GeneratePreview(reportObj, false, false);
            tbiPreview.IsSelected = true;
        }

        private void AccountCodeCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = this.AccountCodeCmb.SelectedIndex;
            this.AccountNameCmb.SelectedIndex = index;
        }

        private void AccountNameCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = this.AccountNameCmb.SelectedIndex;
            this.AccountCodeCmb.SelectedIndex = index;
        }

        private void ExchangeCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.GetComboListData();
        }

        private void ShowTimerPanelChk_Click(object sender, RoutedEventArgs e)
        {
            if (this.TimerQueryPanel == null) return;
            bool isCheck = this.ShowTimerPanelChk.IsChecked.Value;
            this.TimerQueryPanel.Visibility = isCheck ? Visibility.Visible : Visibility.Collapsed;
        }

    }
}
