using Infragistics;
using Infragistics.Controls.Grids;
using Infragistics.Windows.Reporting;
using ManagerConsole.Model;
using ManagerConsole.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;
using System.Linq;
using Logger = Manager.Common.Logger;

namespace ManagerConsole.UI
{
    /// <summary>
    /// Interaction logic for ExecutedOrders.xaml
    /// </summary>
    public partial class ExecutedOrders : UserControl, IControlLayout
    {
        private ManagerConsole.MainWindow _App;
        private ObservableCollection<AccountGroup> _AccountGroups = new ObservableCollection<AccountGroup>();
        private ExecuteOrderSummaryItemModel _Model;
        private bool isCompleted = false;
        private Style _BuyOrderRowStyle;
        private Style _SellOrderRowStyle;
        public ExecutedOrders()
        {
            InitializeComponent();
            this.GetStyles();
            this.BindingData();
            this.InitializeData();
            this.AttachEvent();
            this.isCompleted = true;
        }

        private void GetStyles()
        {
            this._BuyOrderRowStyle = this.Resources["BuyOrderRowStyle"] as Style;
            this._SellOrderRowStyle = this.Resources["SellOrderRowStyle"] as Style;
        }

        private void InitializeData()
        {
            RangeType rangeType = (this._TimeRangeRadio.IsChecked.Value) ? RangeType.Time : RangeType.Price;
            int interval = (rangeType == RangeType.Time) ? int.Parse(this._TimeRangeText.Text): int.Parse(this._PriceRangeText.Text);

            this._Model.InitializeExecuteOrderSummaryItems(this._App.ExchangeDataManager.ExecutedOrders, rangeType, interval);

            this._ExecutedOrderSummaryGrid.ItemsSource = this._Model.ExecuteOrderSummaryItems;
        }

        private OrderRange GetOrderRange(Order order,RangeType rangeType)
        {
            OrderRange orderRange = null;
            InstrumentClient instrument = order.Transaction.Instrument;
            int interval;
            
            if (rangeType == RangeType.Time)
            {
                DateTime executeTime = order.Transaction.ExecuteTime.Value;
                interval = int.Parse(this._TimeRangeText.Text);
                //orderRange = this._Model.GetTimeRange(executeTime, interval);
            }
            else
            {
                string setPrice = order.SetPrice;
                interval = int.Parse(this._PriceRangeText.Text);
               // orderRange = this._Model.GetExecutePriceRange(setPrice, interval, instrument);

            }
            return orderRange;
        }

        private void AttachEvent()
        {
            this._App.ExchangeDataManager.OnDeleteOrderNotifyEvent += new ExchangeDataManager.DeleteOrderNotifyHandler(this.DeleteOrderFromExecuteOrderGrid);
            this._App.ExchangeDataManager.OnExecutedOrderNotifyEvent += new ExchangeDataManager.ExecutedOrderNotifyHandler(this.AddExecutedOrder);
        }

        private void AddExecutedOrder(Order order)
        {
            RangeType rangeType = (this._TimeRangeRadio.IsChecked.Value) ? RangeType.Time : RangeType.Price;
            int interval = (rangeType == RangeType.Time) ? int.Parse(this._TimeRangeText.Text) : int.Parse(this._PriceRangeText.Text);
            this._Model.AddExecutedOrderToGrid(order, rangeType, interval);
        }

        private void BindingData()
        {
            this._App = (ManagerConsole.MainWindow)Application.Current.MainWindow;
            this._Model = this._App.ExchangeDataManager.ExecuteOrderSummaryItemModel;
            this._ExecutedOrderListGrid.ItemsSource = this._App.ExchangeDataManager.ExecutedOrders;
            this.GetComboListData();
        }

        private void GetComboListData()
        {
            AccountGroup allGroup = new AccountGroup();
            allGroup.Code = "All";

            foreach (string exchangeCode in this._App.ExchangeDataManager.ExchangeCodes)
            {
                ExchangeSettingManager settingManager = this._App.ExchangeDataManager.GetExchangeSetting(exchangeCode);

                foreach (AccountGroup group in settingManager.GetAccountGroups())
                {
                    this._AccountGroups.Add(group);
                }
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

        private void QuerySummaryBtn_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            this.InitializeData();
        }

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.isCompleted == false) return;
            e.Handled = true;
            RadioButton radioBtn = (RadioButton)sender;
            bool currenct = radioBtn.IsChecked.Value;
            switch (radioBtn.Name)
            {
                case "_TimeRangeRadio":
                    bool ischecked = this._TimeRangeRadio.IsChecked.Value;

                    break;
                case "_PriceRangeRadio":
                    bool ischecked2 = this._PriceRangeRadio.IsChecked.Value;
                    break;
            }
            this.InitializeData();
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

            if (order.BuySell == BuySell.Buy)
            {
                e.Row.CellStyle =  this._BuyOrderRowStyle;
            }
            else
            {
                e.Row.CellStyle = this._SellOrderRowStyle;
            }
        }

        void ExecutedOrderSummaryGrid_InitializeRow(object sender, Infragistics.Controls.Grids.InitializeRowEventArgs e)
        {
            ExecuteOrderSummaryItem summaryItem = e.Row.Data as ExecuteOrderSummaryItem;

            Style style = App.Current.Resources["InstrumentExecutedOrderSummaryRowStyle"] as Style;
            Style style2 = App.Current.Resources["RangeExecutedOrderSummaryRowStyle"] as Style;

            if(summaryItem.ExecuteOrderSummaryType == ExecuteOrderSummaryType.Instrument)
            {
                e.Row.CellStyle = style;
            }
            else if (summaryItem.ExecuteOrderSummaryType == ExecuteOrderSummaryType.Range)
            {
                e.Row.CellStyle = style2;
            }
        }

        void DeleteOrderFromExecuteOrderGrid(Order deletedOrder)
        {
            Style style = new Style(typeof(Infragistics.Controls.Grids.CellControl));
            style.Setters.Add(new Setter(BackgroundProperty, new SolidColorBrush(Colors.Gray)));
            style.Setters.Add(new Setter(ForegroundProperty, deletedOrder.IsBuyBrush));
            int index = this._App.ExchangeDataManager.ExecutedOrders.IndexOf(deletedOrder);
            this._ExecutedOrderListGrid.Rows[index].CellStyle = style;
        }

        private void _AccountGroupCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AccountGroup group = (AccountGroup)this._AccountGroupCombo.SelectedItem;
            if (group == null) return;

            this.FilterExecutedOrder(group.Id, "Transaction.Account.GroupId", ComparisonOperator.Equals);
        }

        private void FilterExecutedOrder(Guid filterValue, string columnName, ComparisonOperator comparisonOperator)
        {
            this._ExecutedOrderListGrid.FilteringSettings.AllowFiltering = Infragistics.Controls.Grids.FilterUIType.FilterMenu;
            this._ExecutedOrderListGrid.FilteringSettings.FilteringScope = FilteringScope.ColumnLayout;

            this._ExecutedOrderListGrid.Columns.DataColumns[columnName].FilterColumnSettings.FilterCellValue = filterValue;
            foreach (FilterOperand f in this._ExecutedOrderListGrid.Columns.DataColumns[columnName].FilterColumnSettings.RowFilterOperands)
            {
                if (f.ComparisonOperatorValue == comparisonOperator)
                {
                    this._ExecutedOrderListGrid.Columns.DataColumns[columnName].FilterColumnSettings.FilteringOperand = f;
                    break;
                }
            }
        }

        private void _ExecutedOrderTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;

            if (this._ExecutedOrderListItem.IsSelected)
            {
                this._SummaryToolbar.Visibility = System.Windows.Visibility.Collapsed;
                this._ListToolbar.Visibility = System.Windows.Visibility.Visible;
            }
            if (this._OrderSummaryItem.IsSelected)
            {
                this._SummaryToolbar.Visibility = System.Windows.Visibility.Visible;
                this._ListToolbar.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        #region 布局
        /// <summary>
        /// Layout format:
        /// <GridSettings>
        ///    <ColumnsWidth Data="53,0,194,70,222,60,89,60,80,80,80,70,80,80,80,60,60,59,80,80,80,100,80,150,80,"/>
        /// </GridSettings>

        public string GetLayout()
        {
            return ColumnWidthPersistence.GetGridColumnsWidthString(new List<XamGrid> { this._ExecutedOrderListGrid, this._ExecutedOrderSummaryGrid });
        }

        public void SetLayout(XElement layout)
        {
            try
            {
                if (layout.HasElements)
                {
                    ColumnWidthPersistence.LoadGridColumnsWidth(new ObservableCollection<XamGrid> { this._ExecutedOrderListGrid, this._ExecutedOrderSummaryGrid }, layout);
                }
            }
            catch (Exception ex)
            {
                Manager.Common.Logger.AddEvent(System.Diagnostics.TraceEventType.Error, "ExecutedOrders.SetLayout\r\n{0}", ex.ToString());
            }
        }
        #endregion

        
    }
}
