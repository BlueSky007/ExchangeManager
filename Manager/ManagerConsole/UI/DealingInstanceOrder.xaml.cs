using Infragistics;
using Infragistics.Controls.Grids;
using ManagerConsole.Helper;
using ManagerConsole.Model;
using ManagerConsole.ViewModel;
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
using System.Xml.Linq;
using Logger = Manager.Common.Logger;

namespace ManagerConsole.UI
{
    /// <summary>
    /// Interaction logic for DealingInstanceOrder.xaml
    /// </summary>
    public partial class DealingInstanceOrder : UserControl, IControlLayout
    {
        private ManagerConsole.MainWindow _App;
        private ObservableCollection<InstrumentClient> _InstrumentList = new ObservableCollection<InstrumentClient>();
        private Style _ExecuteStatusStyle;
        private ProcessInstantOrder _ProcessInstantOrder;
        public DealingInstanceOrder()
        {
            InitializeComponent();
            this._App = ((ManagerConsole.MainWindow)Application.Current.MainWindow);
            this._ProcessInstantOrder = this._App.ExchangeDataManager.ProcessInstantOrder;
            this.InitializeData();
            this.BindGridData();
            this.GetComboBoxData();
            this.AttachEvent();
        }

        private void InitializeData()
        {
            Color bgColor = Colors.Transparent;
            Style style = new Style(typeof(Infragistics.Controls.Grids.CellControl));
            style.Setters.Add(new Setter(BackgroundProperty, new SolidColorBrush(bgColor)));
            this._ExecuteStatusStyle = App.Current.Resources["ExecuteSatusCellStyle"] as Style;
        }

        private void AttachEvent()
        {
            this._ProcessInstantOrder.OnSettingFirstRowStyleEvent += new ProcessInstantOrder.SettingFirstRowStyleHandle(SettingFirstRowBackGround);
        }

        private void GetComboBoxData()
        {
            InstrumentClient allInstrument = new InstrumentClient();
            allInstrument.Code = "All";

            foreach (string exchangeCode in this._App.ExchangeDataManager.ExchangeCodes)
            {
                ExchangeSettingManager settingManager = this._App.ExchangeDataManager.GetExchangeSetting(exchangeCode);

                foreach (InstrumentClient instrument in settingManager.Instruments.Values)
                {
                    this._InstrumentList.Add(instrument);
                }
            }
            
            this._InstrumentList.Insert(0, allInstrument);
            this._InstrumentCombo.ItemsSource = this._InstrumentList;
            this._InstrumentCombo.DisplayMemberPath = "Code";
            this._InstrumentCombo.SelectedIndex = 0;
            this._InstrumentCombo.SelectedValuePath = "Id";
            this._InstrumentCombo.SelectedItem = allInstrument;
        }

        private void BindGridData()
        {
            this.LayRootGrid.DataContext = this._ProcessInstantOrder.InstantOrderForInstrument;
            this._OrderTaskGrid.ItemsSource = this._ProcessInstantOrder.OrderTasks;
        }

        private void FilterOrderByInstrument(InstrumentClient instrument)
        {
            if (instrument.Id == Guid.Empty) //All
            {
                this._OrderTaskGrid.ItemsSource = this._ProcessInstantOrder.OrderTasks;
                return;
            }
            this._OrderTaskGrid.ItemsSource = this._ProcessInstantOrder.OrderTasks.Where(P => P.InstrumentId == instrument.Id);
            this._ProcessInstantOrder.InitializeBinding(instrument.Id);
            this.LayRootGrid.DataContext = this._ProcessInstantOrder.InstantOrderForInstrument;
        }

        #region Grid Event
        #endregion

        #region Dealing Order Event
        private void AdjustBtn_Click(object sender, RoutedEventArgs e)
        {
            if (this._ProcessInstantOrder.OrderTasks.Count == 0) return;
            e.Handled = true;
            Button btn = sender as Button;
            
            switch (btn.Name)
            {
                case "_UpPriceButton":
                    this._ProcessInstantOrder.AdjustPrice(true);
                    break;
                case "_DownPriceButton":
                    this._ProcessInstantOrder.AdjustPrice(false);
                    break;
                case "_IncreaseSellAutoPointsButton":
                    this._ProcessInstantOrder.AdjustAutoPointVariation(false, true);
                    break;
                case "_DecreaseSellAutoPointsButton":
                    this._ProcessInstantOrder.AdjustAutoPointVariation(false, false);
                    break;
                case "_IncreaseBuyAutoPointsButton":
                    this._ProcessInstantOrder.AdjustAutoPointVariation(true, true);
                    break;
                case "_DecreaseBuyAutoPointsButton":
                    this._ProcessInstantOrder.AdjustAutoPointVariation(true, false);
                    break;
                default: break;
            }
        }

        private void OrderHandlerBtn_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            if (this._ProcessInstantOrder.OrderTasks.Count == 0) return;
            Button btn = sender as Button;
            OrderTask orderTask = null;
            CellDataDefine currentCellData = null;
            Guid orderId = this._ProcessInstantOrder.InstantOrderForInstrument.OrderId;
            switch (btn.Name)
            {
                case "_ExecutedButton":
                    orderTask = this._ProcessInstantOrder.OrderTasks.SingleOrDefault(P => P.OrderId == orderId);
                    currentCellData = orderTask.DQCellDataDefine1;
                    break;
                case "_RejectButton":
                    orderTask = this._ProcessInstantOrder.OrderTasks.SingleOrDefault(P => P.OrderId == orderId);
                    currentCellData = orderTask.DQCellDataDefine2;
                    break;
                case "DQAcceptBtn":
                    orderTask = btn.DataContext as OrderTask;
                    currentCellData = orderTask.DQCellDataDefine1;
                    break;
                case "DQRejectBtn":
                    orderTask = btn.DataContext as OrderTask;
                    currentCellData = orderTask.DQCellDataDefine2;
                    break;
                case "_ExecuteRejectSellButton":
                    this.ExcuteAllDQOrder(false);
                    return;
                case "_ExecuteRejectBuyButton":
                    this.ExcuteAllDQOrder(true);
                    return;
                default:
                    break;
            }
            try
            {
                this.ProcessPendingOrder(orderTask, currentCellData);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "DealingInstanceOrder.OrderHandlerBtn_Click Error\r\n{0}", ex.ToString());
                this._App._CommonDialogWin.ShowDialogWin("Dealing Instance Order Error", "Error");
            }
        }

        private void ExcuteAllDQOrder(bool isBuy)
        {
            for (int i = 0; i < this._OrderTaskGrid.Rows.Count; i++)
            {
                OrderTask order = this._OrderTaskGrid.Rows[i].Data as OrderTask;
                string exchangeCode = order.ExchangeCode;
                ExchangeSettingManager settingManager = this._App.ExchangeDataManager.GetExchangeSetting(exchangeCode);

                string ask = this._ProcessInstantOrder.InstantOrderForInstrument.Ask;
                string bid = this._ProcessInstantOrder.InstantOrderForInstrument.Bid;
                bool buySell = (order.IsBuy == BuySell.Buy);
                if (buySell != isBuy) continue;

                int currentDQVarition = isBuy ? this._ProcessInstantOrder.InstantOrderForInstrument.BuyVariation:this._ProcessInstantOrder.InstantOrderForInstrument.SellVariation;
                string marketPrice = isBuy ? ask : bid;
                InstrumentClient instrument = order.Transaction.Instrument;
                int acceptDQVarition = this._ProcessInstantOrder.CheckDQVariation(instrument, currentDQVarition);

                Customer customer = settingManager.GetCustomer(order.Transaction.Account.CustomerId);
                QuotePolicyDetail quotePolicyDetail = settingManager.GetQuotePolicyDetail(order.InstrumentId.Value, customer);

                bool isAllowed = this._ProcessInstantOrder.AllowAccept(order, quotePolicyDetail, isBuy, marketPrice, currentDQVarition);
                if (isAllowed)
                {
                    this._App.OrderHandle.OnOrderAccept(order);
                }
                else
                {
                    this._App.OrderHandle.OnOrderReject(order);
                }
            }
        }

        private void ProcessPendingOrder(OrderTask orderTask, CellDataDefine currentCellData)
        {
            HandleAction actionType = currentCellData.Action;
            bool isEnabled;
            switch (actionType)
            {
                case HandleAction.None:
                    break;
                case HandleAction.OnOrderAccept:
                    isEnabled = currentCellData.IsEnable;
                    if (isEnabled)
                    {
                        this._App.OrderHandle.OnOrderAccept(orderTask);
                    }
                    break;
                case HandleAction.OnOrderReject:
                    isEnabled = currentCellData.IsEnable;
                    if (isEnabled)
                    {
                        this._App.OrderHandle.OnOrderReject(orderTask);
                    }
                    break;
                default: break;
            }
        }

        #endregion

        private void ToolBar_Click(object sender, RoutedEventArgs e)
        {
            this.ShowGroupPanel();
        }

        private void InstrumentCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            InstrumentClient instrument = (InstrumentClient)this._InstrumentCombo.SelectedItem;
            if (instrument == null) return;
            this.FilterOrderByInstrument(instrument);
        }

        private void SettingFirstRowBackGround()
        {
            this._OrderTaskGrid.Rows[0].CellStyle = this._ExecuteStatusStyle;
        }

        private void ShowGroupPanel()
        {
            if (this._OrderTaskGrid.GroupBySettings.AllowGroupByArea == GroupByAreaLocation.Hidden)
            {
                this._OrderTaskGrid.GroupBySettings.AllowGroupByArea = Infragistics.Controls.Grids.GroupByAreaLocation.Top;
                this._OrderTaskGrid.GroupBySettings.EmptyGroupByAreaContent = "拖动列在此分组";
            }
            else
            {
                this._OrderTaskGrid.GroupBySettings.AllowGroupByArea = Infragistics.Controls.Grids.GroupByAreaLocation.Hidden;
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
            //InstrumentCode
            StringBuilder layoutBuilder = new StringBuilder();
            layoutBuilder.Append("<GridSettings>");
            if (this._OrderTaskGrid.FilteringSettings.RowFiltersCollection.Count > 0)
            { 
                IRecordFilter rowsFilter = this._OrderTaskGrid.FilteringSettings.RowFiltersCollection[0];

                if (rowsFilter.FieldName == "InstrumentCode")
                {
                     layoutBuilder.AppendFormat("<Fitler LogicalOperator=\"{0}\">", (int)rowsFilter.Conditions.LogicalOperator);
                    foreach (IFilterCondition condition in rowsFilter.Conditions)
                    {
                        ComparisonCondition comparisonCondition = condition as ComparisonCondition;
                        if (comparisonCondition != null)
                        {
                            layoutBuilder.AppendFormat("<Condition op=\"{0}\" val=\"{1}\"/>", (int)comparisonCondition.Operator, comparisonCondition.FilterValue);
                        }
                    }
                    layoutBuilder.Append("</Fitler>");
                }
            }

            layoutBuilder.Append(ColumnWidthPersistence.GetPersistentColumnsWidthString(this._OrderTaskGrid));
            layoutBuilder.Append("</GridSettings>");

            return layoutBuilder.ToString();
        }

        public void SetLayout(XElement layout)
        {
            try
            {
                if (layout.HasElements)
                {
                    XElement filterElement = layout.Element("Fitler");
                    if (filterElement != null)
                    {
                        RowsFilter rowsFilter = new RowsFilter(typeof(string), this._OrderTaskGrid.Columns.DataColumns["InstrumentCode"]);
                        rowsFilter.Conditions.LogicalOperator = (LogicalOperator)int.Parse(filterElement.Attribute("LogicalOperator").Value);

                        foreach (XElement element in filterElement.Elements("Condition"))
                        {
                            rowsFilter.Conditions.Add(new ComparisonCondition() { FilterValue = element.Attribute("val").Value, Operator = (ComparisonOperator)int.Parse(element.Attribute("op").Value) });
                        }
                        this._OrderTaskGrid.FilteringSettings.RowFiltersCollection.Add(rowsFilter);
                    }
                    //Grid Column Width
                    XElement columnWidthElement = layout.Element("GridSettings").Element("ColumnsWidth");
                    if (columnWidthElement != null)
                    {
                        ColumnWidthPersistence.LoadColumnsWidth(this._OrderTaskGrid, columnWidthElement);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.AddEvent(System.Diagnostics.TraceEventType.Error, "DealingInstanceOrder.SetLayout\r\n{0}", ex.ToString());
            }
        }
        #endregion


    }
}
