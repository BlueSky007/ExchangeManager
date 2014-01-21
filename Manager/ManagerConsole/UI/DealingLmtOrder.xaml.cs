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

namespace ManagerConsole.UI
{
    /// <summary>
    /// Interaction logic for DealingLmtOrder.xaml
    /// </summary>
    public partial class DealingLmtOrder : UserControl
    {
        private CommonDialogWin _CommonDialogWin;
        private ConfirmDialogWin _ConfirmDialogWin;
        private ManagerConsole.MainWindow _App;
        private ObservableCollection<InstrumentClient> _InstrumentList = new ObservableCollection<InstrumentClient>();
        private Style _ExecuteStatusStyle;
        private Style _NormalStyle;

        private ProcessLmtOrder _ProcessLmtOrder;
        public DealingLmtOrder()
        {
            InitializeComponent();

            this.InitializeData();
            this.BindGridData();
            this.GetComboBoxData();
            this.AttachEvent();
        }

        #region Event
        private void InitializeData()
        {
            this._App = ((ManagerConsole.MainWindow)Application.Current.MainWindow);
            this._ProcessLmtOrder = this._App.InitDataManager.ProcessLmtOrder;
            this._CommonDialogWin = this._App._CommonDialogWin;
            this._ConfirmDialogWin = this._App._ConfirmDialogWin;

            Color bgColor = Colors.Transparent;
            Style style = new Style(typeof(Infragistics.Controls.Grids.CellControl));
            style.Setters.Add(new Setter(BackgroundProperty, new SolidColorBrush(bgColor)));
            this._NormalStyle = this.Resources["CellStyle"] as Style;
            this._ExecuteStatusStyle = App.Current.Resources["ExecuteSatusCellStyle"] as Style;
        }
        private void AttachEvent()
        {
            this._ProcessLmtOrder.OnSettingFirstRowStyleEvent += new ProcessLmtOrder.SettingFirstRowStyleHandle(this.SettingFirstRowBackGround);
            this._App.InitDataManager.OnHitPriceReceivedRefreshUIEvent += new ExchangeDataManager.HitPriceReceivedRefreshUIEventHandler(this.ForwardHitOrder);
        }
        #endregion

        private void GetComboBoxData()
        {
            InstrumentClient allInstrument = new InstrumentClient();
            allInstrument.Code = "All";
            foreach (string exchangeCode in this._App.InitDataManager.ExchangeCodes)
            {
                ExchangeSettingManager settingManager = this._App.InitDataManager.GetExchangeSetting(exchangeCode);

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

        private void ToolBar_Click(object sender, RoutedEventArgs e)
        {
            Button clickImg = (Button)sender;
            if (this._OrderTaskGrid.Rows.Count == 0) return;
            OrderTask orderTask = this._OrderTaskGrid.Rows[0].Data as OrderTask;
            switch (clickImg.Name)
            {
                case "_UpdateBtn":
                    if (orderTask == null || orderTask.OrderType != OrderType.Limit) return;
                    this._App.OrderHandle.OnOrderUpdate(orderTask);
                    break;
                case "_ModifyBtn":
                    if (orderTask == null || orderTask.OrderType != OrderType.Limit) return;
                    this._App.OrderHandle.OnOrderModify(orderTask);
                    break;
                case "_CancelBtn":
                    if (orderTask == null || orderTask.OrderType != OrderType.Limit) return;
                    this._App.OrderHandle.OnOrderWait(orderTask);
                    this.BackHitOrder(orderTask, 0);
                    break;
                case "_ExecuteBtn":
                    if (orderTask == null || orderTask.OrderType != OrderType.Limit) return;
                    this._App.OrderHandle.OnOrderExecute(orderTask);
                    break;
                case "_ShowGroupPanelBtn":
                    this.ShowGroupPanel();
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
                    this.BackHitOrder(orderTask, rowIndex);
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
            Button btn = sender as Button;
            OrderTask orderTask = null;
            CellDataDefine currentCellData = null;
            Guid orderId = this._ProcessLmtOrder.LmtOrderForInstrument.OrderId;
            switch (btn.Name)
            {
                case "_ExecuteAllSellOrderButton":
                    orderTask = this._ProcessLmtOrder.OrderTasks.SingleOrDefault(P => P.OrderId == orderId);
                    currentCellData = orderTask.DQCellDataDefine1;
                    break;
                case "_WailtNextSellPriceButton":
                    orderTask = this._ProcessLmtOrder.OrderTasks.SingleOrDefault(P => P.OrderId == orderId);
                    currentCellData = orderTask.DQCellDataDefine2;
                    break;
                case "_ApplyAskPriceButton":
                    orderTask = btn.DataContext as OrderTask;
                    currentCellData = orderTask.DQCellDataDefine1;
                    break;
                case "_ExecuteAllBuyOrderButton":
                    orderTask = btn.DataContext as OrderTask;
                    currentCellData = orderTask.DQCellDataDefine2;
                    break;
                case "_WailtNextAskPriceButton":
                    this.ExcuteAllLMTOrder(false);
                    return;
                case "_ApplyBidPriceButton":
                    this.ExcuteAllLMTOrder(true);
                    return;
                default:
                    break;
            }
        }

        private void ExcuteAllLMTOrder(bool isBuy)
        {
            for (int i = 0; i < this._OrderTaskGrid.Rows.Count; i++)
            {
               
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
                        this._App.OrderHandle.OnOrderUpdate(orderTask);
                    }
                    break;
                case HandleAction.OnOrderModify:
                    isEnabled = currentCellData.IsEnable;
                    if (isEnabled)
                    {
                        this._App.OrderHandle.OnOrderModify(orderTask);
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
            this.FilterInstantOrder();
        }

        private void IsOPenCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ComboBox cmb = sender as ComboBox;

                //if (e.AddedItems.Count <= 0) return;
                //string isOpenString = e.AddedItems[0].ToString();
                //OrderTaskForInstrument orderTaskForInstrument = cmb.DataContext as OrderTaskForInstrument;
                //string buySellString = orderTaskForInstrument.BuySellString;

                //if (orderTaskForInstrument != null)
                //{
                //    orderTaskForInstrument.FilterOrderTask(buySellString, isOpenString);
                //}
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(TraceEventType.Error, "OrderTaskControl.IsOPenCombo_SelectionChanged error:\r\n{0}", ex.ToString());
            }
        }

        private void BuySellCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ComboBox cmb = sender as ComboBox;

                //if (e.AddedItems.Count <= 0) return;
                //string buySellString = e.AddedItems[0].ToString();

                //OrderTaskForInstrument orderTaskForInstrument = cmb.DataContext as OrderTaskForInstrument;
                //string isOpenString = orderTaskForInstrument.IsOpenString;
                //if (orderTaskForInstrument != null)
                //{
                //    orderTaskForInstrument.FilterOrderTask(buySellString, isOpenString);
                //}
            }
            catch (Exception ex)
            {
                Manager.Common.Logger.TraceEvent(TraceEventType.Error, "OrderTaskControl.BuySellCombo_SelectionChanged error:\r\n{0}", ex.ToString());
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

        private void SetOrderTaskStatusStyle(Cell cell)
        {
            if (cell == null)
            {
                return;
            }
            Row row = (Row)cell.Row;
            OrderTask orderTask = row.Data as OrderTask;
            if (orderTask != null)
            {
                row.CellStyle = App.Current.Resources["ExecuteSatusCellStyle"] as Style;
                this._OrderTaskGrid.Columns["DQHandle"].CellStyle = App.Current.Resources["OrangeColumnStyle"] as Style;
                this._OrderTaskGrid.Columns["LMTHandle"].CellStyle = App.Current.Resources["OrangeColumnStyle"] as Style;
            }
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
            this._OrderTaskGrid.ItemsSource = null;
            this._OrderTaskGrid.ItemsSource = this._App.InitDataManager.OrderTaskModel.OrderTasks;

            for (int i = 0; i < hitOrdersCount; i++)
            {
                this._OrderTaskGrid.Rows[i].CellStyle = this._ExecuteStatusStyle;
            }
            this.FilterInstantOrder();
        }

        public void BackHitOrder(OrderTask orderTask,int rowIndex)
        {
            this._OrderTaskGrid.Rows[rowIndex].CellStyle = this._NormalStyle;
            this._App.InitDataManager.OrderTaskModel.OrderTasks.Remove(orderTask);
            int index = this._App.InitDataManager.OrderTaskModel.OrderTasks.Count - 1;
            this._App.InitDataManager.OrderTaskModel.OrderTasks.Insert(index, orderTask);
            this._OrderTaskGrid.ItemsSource = this._App.InitDataManager.OrderTaskModel.OrderTasks;
        }

        private void AdjustBtn_Click(object sender, RoutedEventArgs e)
        { 
        }

        private void SettingFirstRowBackGround()
        {
            this._OrderTaskGrid.Rows[0].CellStyle = this._ExecuteStatusStyle;
        }
    }
}
