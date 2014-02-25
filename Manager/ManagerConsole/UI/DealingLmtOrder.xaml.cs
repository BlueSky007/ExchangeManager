using ManagerConsole.Helper;
using ManagerConsole.Model;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ManagerConsole.ViewModel;
using System.Collections.ObjectModel;
using ConfigParameter = Manager.Common.Settings.ConfigParameter;
using OrderType = iExchange.Common.OrderType;
using Logger = Manager.Common.Logger;
using Infragistics.Controls.Grids;
using System.Diagnostics;
using Infragistics;
using System.Linq;
using System.Windows.Media.Animation;
using System.Threading;
using System.Text;
using System.Xml.Linq;

namespace ManagerConsole.UI
{
    /// <summary>
    /// Interaction logic for DealingLmtOrder.xaml
    /// </summary>
    public partial class DealingLmtOrder : UserControl, IControlLayout
    {
        private CommonDialogWin _CommonDialogWin;
        private ConfirmDialogWin _ConfirmDialogWin;
        private ManagerConsole.MainWindow _App;
        private ObservableCollection<InstrumentClient> _InstrumentList = new ObservableCollection<InstrumentClient>();
        private Style _ExecuteStatusStyle;
        private Style _NormalStyle;
        private bool _IsProcessOrderPanelVisibility = true;
        private Storyboard _HiddenStoryboard;
        private Storyboard _ShowStoryboard;

        private ProcessLmtOrder _ProcessLmtOrder;
        public DealingLmtOrder()
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
        }

        #region Event
        private bool InilizeUI()
        {
            if (this._App.ExchangeDataManager.IsInitializeCompleted)
            {
                this.Dispatcher.BeginInvoke((Action)delegate()
                {
                    this.InitializeData();
                    this.BindGridData();
                    this.GetComboBoxData();
                    this.AttachEvent();
                    this.CheckOrderStatus();
                });
                return true;
            }
            else
            {
                return false;
            }
        }

        private void InitializeData()
        {
            this._ProcessLmtOrder = this._App.ExchangeDataManager.ProcessLmtOrder;
            this._CommonDialogWin = this._App._CommonDialogWin;
            this._ConfirmDialogWin = this._App._ConfirmDialogWin;

            Color bgColor = Colors.Transparent;
            Style style = new Style(typeof(Infragistics.Controls.Grids.CellControl));
            style.Setters.Add(new Setter(BackgroundProperty, new SolidColorBrush(bgColor)));
            this._NormalStyle = this.Resources["CellStyle"] as Style;
            this._ExecuteStatusStyle = App.Current.Resources["ExecuteSatusCellStyle"] as Style;
            this._HiddenStoryboard = this.Resources["HidBorderStoryboard"] as Storyboard;
            this._ShowStoryboard = this.Resources["ShowBorderStoryboard"] as Storyboard;
        }
        private void AttachEvent()
        {
            this._ProcessLmtOrder.OnSettingFirstRowStyleEvent += new ProcessLmtOrder.SettingFirstRowStyleHandle(this.SettingFirstRowBackGround);
            this._App.ExchangeDataManager.OnHitPriceReceivedRefreshUIEvent += new ExchangeDataManager.HitPriceReceivedRefreshUIEventHandler(this.ForwardHitOrder);
        }
        #endregion

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
            this.LayRootGrid.DataContext = this._ProcessLmtOrder.LmtOrderForInstrument;
            this._OrderTaskGrid.ItemsSource = this._ProcessLmtOrder.OrderTasks;
            this._TopToolBar.DataContext = new TopToolBarImages();
        }

        //Initialize Data Check Set Style
        private void CheckOrderStatus() 
        {
            for (int i = 0; i < this._OrderTaskGrid.Rows.Count; i++)
            {
                OrderTask currentOrder = this._OrderTaskGrid.Rows[i].Data as OrderTask;
                if (OrderTaskManager.CheckExecuteOrder(currentOrder))
                {
                    this._OrderTaskGrid.Rows[i].CellStyle = this._ExecuteStatusStyle;
                }
            }
        }

        private void ToolBar_Click(object sender, RoutedEventArgs e)
        {
            Button clickImg = (Button)sender;
            if (this._OrderTaskGrid.Rows.Count == 0) return;
            OrderTask orderTask = this._OrderTaskGrid.Rows[0].Data as OrderTask;
            switch (clickImg.Name)
            {
                case "_UpdateBtn":
                    this._App.OrderHandle.OnOrderUpdate(orderTask,this._ProcessLmtOrder.LmtOrderForInstrument);
                    break;
                case "_ModifyBtn":
                    this._App.OrderHandle.OnOrderModify(orderTask,this._ProcessLmtOrder.LmtOrderForInstrument.Origin);
                    break;
                case "_CancelBtn":
                    this._App.OrderHandle.OnOrderWait(orderTask);
                    this._OrderTaskGrid.Rows[0].CellStyle = this._NormalStyle;
                    this._ProcessLmtOrder.SetOrderBottom(orderTask, 0);
                    this._OrderTaskGrid.ItemsSource = this._ProcessLmtOrder.OrderTasks;
                    break;
                case "_ExecuteBtn":
                    this._App.OrderHandle.OnOrderExecute(orderTask);
                    break;
                case "_ShowGroupPanelBtn":
                    this.ShowGroupPanel();
                    break;
                case "_ShowBorderButton":
                    this.ShowProcessPanel();
                    break;
            }
        }

        private void OrderHandlerBtn_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            OrderTask orderTask = null;
            CellDataDefine currentCellData = null;

            switch (btn.Name)
            {
                case "DQAcceptBtn":
                    orderTask = btn.DataContext as OrderTask;
                    currentCellData = orderTask.DQCellDataDefine1;
                    break;
                case "DQRejectBtn":
                     orderTask = btn.DataContext as OrderTask;
                     currentCellData = orderTask.DQCellDataDefine2;
                     break;
                case "UpdateBtn":
                    orderTask = btn.DataContext as OrderTask;
                    currentCellData = orderTask.CellDataDefine1;
                    break;
                case "ModiFyBtn":
                    orderTask = btn.DataContext as OrderTask;
                    currentCellData = orderTask.CellDataDefine2;
                    break;
                case "CancelBtn":
                    orderTask = btn.DataContext as OrderTask;
                    currentCellData = orderTask.CellDataDefine3;
                    int rowIndex = this._OrderTaskGrid.ActiveCell.Row.Index;
                    this._OrderTaskGrid.Rows[rowIndex].CellStyle = this._NormalStyle;
                    this._ProcessLmtOrder.SetOrderBottom(orderTask, rowIndex);
                    this._OrderTaskGrid.ItemsSource = this._ProcessLmtOrder.OrderTasks;
                    break;
                case "ExcuteBtn":
                    orderTask = btn.DataContext as OrderTask;
                    currentCellData = orderTask.CellDataDefine4;
                    break;
                default: break;
            }
            this.ProcessPendingOrder(orderTask, currentCellData);
        }

        private void BathOrderHandlerBtn_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            if (this._ProcessLmtOrder.OrderTasks.Count == 0) return;
            LmtOrderForInstrument lmtOrderForInstrument = this._ProcessLmtOrder.LmtOrderForInstrument;
            if (lmtOrderForInstrument == null) return;
            Button btn = sender as Button;
            switch (btn.Name)
            {
                case "_ExecuteAllSellOrderButton":
                    this._App.OrderHandle.OnLMTExecute(this._ProcessLmtOrder,false);
                    break;
                case "_WailtNextSellPriceButton":
                    this._App.OrderHandle.OnLMTOrderWait(this._ProcessLmtOrder, false);
                    this._ProcessLmtOrder.SetAllOrderBottom(false);
                    break;
                case "_ApplyAskPriceButton":
                    lmtOrderForInstrument.ApplyMaretPrice(false);
                    break;
                case "_ExecuteAllBuyOrderButton":
                    this._App.OrderHandle.OnLMTExecute(this._ProcessLmtOrder, true);
                    break;
                case "_WailtNextAskPriceButton":
                    this._App.OrderHandle.OnLMTOrderWait(this._ProcessLmtOrder, true);
                    this._ProcessLmtOrder.SetAllOrderBottom(true);
                    break;
                case "_ApplyBidPriceButton":
                    lmtOrderForInstrument.ApplyMaretPrice(true);
                    return;
                default:
                    break;
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
                case HandleAction.OnOrderDetail:
                    //OnOrderDetail(row);
                    break;
                case HandleAction.OnOrderAcceptPlace:
                    this._App.OrderHandle.OnOrderAcceptPlace(orderTask);
                    break;
                case HandleAction.OnOrderRejectPlace:
                    this._App.OrderHandle.OnOrderRejectPlace(orderTask);
                    break;
                case HandleAction.OnOrderAcceptCancel:
                    //OnOrderAcceptCancel(row);
                    break;
                case HandleAction.OnOrderRejectCancel:
                    //OnOrderRejectCancel(row);
                    break;
                case HandleAction.OnOrderUpdate:
                    isEnabled = currentCellData.IsEnable;
                    if (isEnabled)
                    {
                        this._App.OrderHandle.OnOrderUpdate(orderTask,this._ProcessLmtOrder.LmtOrderForInstrument);
                    }
                    break;
                case HandleAction.OnOrderModify:
                    isEnabled = currentCellData.IsEnable;
                    if (isEnabled)
                    {
                        this._App.OrderHandle.OnOrderModify(orderTask,this._ProcessLmtOrder.LmtOrderForInstrument.Origin);
                    }
                    break;
                case HandleAction.OnOrderWait:
                    this._App.OrderHandle.OnOrderWait(orderTask);
                    break;
                case HandleAction.OnOrderExecute:
                    this._App.OrderHandle.OnOrderExecute(orderTask);
                    break;
                case HandleAction.OnOrderCancel:
                    this._App.OrderHandle.OnOrderCancel(orderTask);
                    break;
            }
        }

        #region ToolBar Event
        void ShowGroupPanel()
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

        private void FilterInstantOrder()
        {
            InstrumentClient instrument = (InstrumentClient)this._InstrumentCombo.SelectedItem;
            if (instrument == null) return;

            if (instrument.Id == Guid.Empty)
            {
                this.FilterOrderTask("", "InstrumentCode", ComparisonOperator.NotEquals);
            }
            else
            {
                this.FilterOrderTask(instrument.Code, "InstrumentCode", ComparisonOperator.Equals);
            }
        }

        private void InstrumentCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            InstrumentClient instrument = (InstrumentClient)this._InstrumentCombo.SelectedItem;
            if (instrument == null) return;
            this.FilterOrderByInstrument(instrument);
        }

        private void FilterOrderByInstrument(InstrumentClient instrument)
        {
            if (instrument.Id == Guid.Empty) //All
            {
                this._OrderTaskGrid.ItemsSource = this._ProcessLmtOrder.OrderTasks;
                return;
            }
            this._OrderTaskGrid.ItemsSource = this._ProcessLmtOrder.OrderTasks.Where(P => P.InstrumentId == instrument.Id);
            this._ProcessLmtOrder.InitializeBinding(instrument.Id);
            this.LayRootGrid.DataContext = this._ProcessLmtOrder.LmtOrderForInstrument;
        }

        private void ShowProcessPanel()
        {
            if (this._IsProcessOrderPanelVisibility)
            {
                this._HiddenStoryboard.Begin();
                this._IsProcessOrderPanelVisibility = false;
                this._ShowBorderButton.Content = "Show Panel";
            }
            else 
            {
                this._ShowStoryboard.Begin();
                this._IsProcessOrderPanelVisibility = true;
                this._ShowBorderButton.Content = "Hidden Panel";
            }
        }
        #endregion

        #region Grid Event

        private void OrderTaskGrid_InitializeRow(object sender, Infragistics.Controls.Grids.InitializeRowEventArgs e)
        {
            //Color bgColor = Colors.Transparent;
            //Style style = new Style(typeof(Infragistics.Controls.Grids.CellControl));
            //style.Setters.Add(new Setter(BackgroundProperty, new SolidColorBrush(bgColor)));
            //e.Row.CellStyle = style;
        }

        void OrderTaskGrid_CellControlAttached(object sender, CellControlAttachedEventArgs e)
        {
            OrderTask orderTask = e.Cell.Row.Data as OrderTask;
            switch (e.Cell.Column.Key)
            {
                case "HitCount":
                    int hitCount = orderTask.HitCount.Value;
                    if (hitCount > 0)
                    {
                        //this.SetOrderTaskStatusStyle(e.Cell);
                    }
                    break;
            }
        }
        #endregion

        private void CustomerAskPriceInput_ValueChanged(object sender, EventArgs e)
        {
            if (this._ProcessLmtOrder.OrderTasks.Count == 0) return;
            this._ProcessLmtOrder.LmtOrderForInstrument.UpdateDiff();
        }

        private void CustomerBidPriceInput_ValueChanged(object sender, EventArgs e)
        {
            if (this._ProcessLmtOrder.OrderTasks.Count == 0) return;
            this._ProcessLmtOrder.LmtOrderForInstrument.UpdateDiff();
        }

        private void FilterOrderTask(string filterValue,string columnName,ComparisonOperator comparisonOperator)
        {
            this._OrderTaskGrid.FilteringSettings.AllowFiltering = Infragistics.Controls.Grids.FilterUIType.FilterMenu;
            this._OrderTaskGrid.FilteringSettings.FilteringScope = FilteringScope.ColumnLayout;

            this._OrderTaskGrid.Columns.DataColumns[columnName].FilterColumnSettings.FilterCellValue = filterValue;
            foreach (FilterOperand f in this._OrderTaskGrid.Columns.DataColumns[columnName].FilterColumnSettings.RowFilterOperands)
            {
                if (f.ComparisonOperatorValue == comparisonOperator)
                {
                    this._OrderTaskGrid.Columns.DataColumns[columnName].FilterColumnSettings.FilteringOperand = f;
                    break;
                }
            }
        }

        public void ForwardHitOrder(int hitOrdersCount)
        {
            for (int i = 0; i < hitOrdersCount; i++)
            {
                this._OrderTaskGrid.Rows[i].CellStyle = this._ExecuteStatusStyle;
            }
            //this.FilterInstantOrder();
        }

        private void SetAllOrderBottom(bool isBuy)
        {
            for (int i = 0; i < this._OrderTaskGrid.Rows.Count; i++)
            {
                this._OrderTaskGrid.Rows[i].CellStyle = this._NormalStyle;
            }
        }

        private void SettingFirstRowBackGround()
        {
            this._OrderTaskGrid.Rows[0].CellStyle = this._ExecuteStatusStyle;
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
                IRecordFilter rowsFilter = this._OrderTaskGrid.FilteringSettings.RowFiltersCollection[3];

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
