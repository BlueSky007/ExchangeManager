﻿using ManagerCommon = Manager.Common;
using ManagerConsole.Helper;
using ManagerConsole.Model;
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
using ManagerConsole.ViewModel;
using System.Collections.ObjectModel;
using CommonTransactionError = Manager.Common.TransactionError;
using CommonParameter = Manager.Common.SystemParameter;
using Logger = Manager.Common.Logger;
using Infragistics.Controls.Interactions;
using Infragistics.Controls.Grids;
using System.Diagnostics;
using Infragistics;

namespace ManagerConsole.UI
{
    /// <summary>
    /// Interaction logic for OrderTaskControl.xaml
    /// </summary>
    public partial class OrderTaskControl : UserControl
    {
        private CommonDialogWin _CommonDialogWin;
        private ConfirmDialogWin _ConfirmDialogWin;
        private ManagerConsole.MainWindow _App;
        private OrderHandle OrderHandle;
        private ObservableCollection<InstrumentClient> _InstrumentList = new ObservableCollection<InstrumentClient>();
        private Style _ExecuteStatusStyle; 
        public OrderTaskControl()
        {
            InitializeComponent();

            this._App = ((ManagerConsole.MainWindow)Application.Current.MainWindow);
            this._CommonDialogWin = this._App.CommonDialogWin;
            this._ConfirmDialogWin = this._App.ConfirmDialogWin;
            this._ExecuteStatusStyle = App.Current.Resources["ExecuteSatusCellStyle"] as Style;
            OrderHandle = new OrderHandle();
            this.BindGridData();
            this.GetComboBoxData();
            this.AttachEvent();
        }

        #region Event
        private void AttachEvent()
        {
            this._App.InitDataManager.OnHitPriceReceivedRefreshUIEvent += new InitDataManager.HitPriceReceivedRefreshUIEventHandler(this.RefreshUI);
        }
        #endregion

        private void GetComboBoxData()
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
            this._InstrumentCombo.SelectedIndex = 1;
            this._InstrumentCombo.SelectedValuePath = "Id";
        }

        private void BindGridData()
        {
            this.LayRootGrid.DataContext = this._App.InitDataManager.OrderTaskModel;//this._OrderTaskEntity;
            this._ConfirmDialogWin.OnConfirmDialogResult += new ConfirmDialogWin.ConfirmDialogResultHandle(ExcuteOrder);
            this._OrderTaskGrid.ItemsSource = this._App.InitDataManager.OrderTaskModel.OrderTasks;
            this._TopToolBar.DataContext = new TopToolBarImages();
        }

        void ExcuteOrder(bool yesOrNo, UIElement uIElement)
        {
            if (yesOrNo)
            {
                MessageBox.Show("你点击了确定按钮");
            }
        }

        private void ToolBar_Click(object sender, RoutedEventArgs e)
        {
            Button clickImg = (Button)sender;
            switch (clickImg.Name)
            {
                case "_UpdateBtn":
                    //orderTask = btn.DataContext as OrderTask;
                    //currentCellData = orderTask.CellDataDefine1;
                    break;
                case "_ModifyBtn":
                    MessageBox.Show("mody");
                    break;
                case "_CancelBtn":
                    MessageBox.Show("cace");
                    break;
                case "_ExecuteBtn":
                    MessageBox.Show("exe");
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
            LmtOrderTaskForInstrument lmtOrderForInstrument = null;
            CellDataDefine currentCellData = null;

            switch (btn.Name)
            {
                case "DQAcceptBtn":
                    orderTask = btn.DataContext as OrderTask;
                    currentCellData = orderTask.CellDataDefine1;
                    this.ProcessPendingOrder(orderTask, currentCellData);
                    break;
                case "DQRejectBtn":
                     orderTask = btn.DataContext as OrderTask;
                     currentCellData = orderTask.CellDataDefine2;
                        this.ProcessPendingOrder(orderTask, currentCellData);
                    break;
                case "ExecuteAllBtn":
                    lmtOrderForInstrument = ((UnboundColumnDataContext)btn.DataContext).RowData as LmtOrderTaskForInstrument;
                    this.OrderHandle.OnLMTExecute(lmtOrderForInstrument);
                    return;
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
                    break;
                case "ExcuteBtn":
                    orderTask = btn.DataContext as OrderTask;
                    currentCellData = orderTask.CellDataDefine4;
                    break;
            }
            this.ProcessPendingOrder(orderTask, currentCellData);
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
                        this.OrderHandle.OnOrderAccept(orderTask); 
                    }
                    break;
                case HandleAction.OnOrderReject:
                    isEnabled = currentCellData.IsEnable;
                    if (isEnabled)
                    {
                        this.OrderHandle.OnOrderReject(orderTask);
                    }
                    break;
                case HandleAction.OnOrderDetail:
                    //OnOrderDetail(row);
                    break;
                case HandleAction.OnOrderAcceptPlace:
                    this.OrderHandle.OnOrderAcceptPlace(orderTask);
                    break;
                case HandleAction.OnOrderRejectPlace:
                    this.OrderHandle.OnOrderRejectPlace(orderTask);
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
                        this.OrderHandle.OnOrderUpdate(orderTask);
                    }
                    break;
                case HandleAction.OnOrderModify:
                    isEnabled = currentCellData.IsEnable;
                    if (isEnabled)
                    {
                        this.OrderHandle.OnOrderModify(orderTask);
                    }
                    break;
                case HandleAction.OnOrderWait:
                    this.OrderHandle.OnOrderWait(orderTask);
                    break;
                case HandleAction.OnOrderExecute:
                    this.OrderHandle.OnOrderExecute(orderTask);
                    break;
                case HandleAction.OnOrderCancel:
                    this.OrderHandle.OnOrderCancel(orderTask);
                    break;
            }
        }

        #region Quote Order
        private void OnOrderAccept(OrderTask order)
        {
            SystemParameter systemParameter = this._App.InitDataManager.SettingsManager.SystemParameter;
            systemParameter.CanDealerViewAccountInfo = false;
            bool isOK = OrderTaskManager.CheckDQOrder(order, systemParameter);
            isOK = false;

            if (!isOK)
            {
                if (systemParameter.CanDealerViewAccountInfo)
                {
                    //Show Account Information
                }
                else
                {
                    if (MessageBox.Show("Accept the order?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        this.AcceptPlace(order.Transaction.Id);
                    }
                }
            }
        }

        

        private void AcceptPlace(Guid transactionId)
        {
            //ConsoleClient.Instance.AcceptPlace(transactionId, AcceptPlaceCallback);
        }

        private void ExecuteOrder()
        { 
        }
       
        #endregion

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

        void VariationText_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Subtract)
            {
                TextBox textBox = sender as TextBox;
                string str = textBox.Text;
                if(str.Contains("-"))
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

        void QueryOrder_Click(object sender, RoutedEventArgs e)
        {
            InstrumentClient currentInstrument = (InstrumentClient)this._InstrumentCombo.SelectedItem;
            decimal oldOrigin = string.IsNullOrEmpty(this._OriginLable.Text) ? 0 : decimal.Parse(this._OriginLable.Text);

            if (currentInstrument.Id == Guid.Empty)
            {
                this._CommonDialogWin.ShowDialogWin("Please select origin code to query.", "Alert");
                return;
            }

            decimal newOrigin = decimal.Parse(currentInstrument.Origin);
            this._OriginLable.Text = currentInstrument.Origin;
            string points = this._VariationText.Text;

            SolidColorBrush bgColor = new SolidColorBrush(Colors.Blue);
            if (newOrigin < oldOrigin)
            {
                bgColor = new SolidColorBrush(Colors.Red);
            }

            this._OriginLable.Foreground = bgColor;

            this.FilterInstantOrder();
        }

        void AdjustVariationText_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox variationText = sender as TextBox;
            InstrumentClient instrument = (InstrumentClient)this._InstrumentCombo.SelectedItem;
            if (instrument.Id == Guid.Empty) return;
            int acceptDQVariation = int.Parse(this._VariationText.Text);

            if (instrument.CheckVariation(acceptDQVariation))
            {
                this._VariationText.Text = acceptDQVariation.ToString();
            }
            else
            {
                this._VariationText.Text = (0 - instrument.AcceptDQVariation).ToString();
            }
        }

        void AdjustVariation_Click(object sender, RoutedEventArgs e)
        {
            InstrumentClient instrument = (InstrumentClient)this._InstrumentCombo.SelectedItem;
            if (instrument.Id == Guid.Empty) return;
            int variation;
            Button btn = sender as Button;
            int points = int.Parse(this._VariationText.Text);
            try
            {
                switch (btn.Name)
                {
                    case "UpButton":
                        variation = points + (int)instrument.NumeratorUnit;
                        if (instrument.CheckVariation(variation))
                        {
                            this._VariationText.Text = (points + (int)instrument.NumeratorUnit).ToString();
                        }
                        else
                        {
                            this._VariationText.Text = (0 - instrument.AcceptDQVariation).ToString();
                        }
                        break;
                    case "DownButton":
                        variation = points - (int)instrument.NumeratorUnit;
                        if (instrument.CheckVariation(variation))
                        {
                            this._VariationText.Text = (points - (int)instrument.NumeratorUnit).ToString();
                        }
                        else
                        {
                            this._VariationText.Text = (0 - instrument.AcceptDQVariation).ToString();
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(TraceEventType.Error, "OrderTaskControl.AdjustVariation_Click error:\r\n{0}", ex.ToString());
            }
        }

        private void FilterInstantOrder()
        {
            InstrumentClient instrument = (InstrumentClient)this._InstrumentCombo.SelectedItem;
            if (instrument == null) return;

            int buySellIndex = this._BuySellCombo.SelectedIndex;
            int openCloseIndex = this._OpenCloseCombo.SelectedIndex;
            if (buySellIndex == 1)
            {
                this.FilterOrderTask("B", "IsBuyString", ComparisonOperator.Equals);
            }
            else if (buySellIndex == 2)
            {
                this.FilterOrderTask("S", "IsBuyString", ComparisonOperator.Equals);
            }
            else
            {
                this.FilterOrderTask("", "IsBuyString", ComparisonOperator.NotEquals);
            }

            if (openCloseIndex == 1)
            {
                this.FilterOrderTask("N", "IsOpenString", ComparisonOperator.Equals);
            }
            else if (openCloseIndex == 2)
            {
                this.FilterOrderTask("C", "IsOpenString", ComparisonOperator.Equals);
            }
            else
            {
                this.FilterOrderTask("", "IsOpenString", ComparisonOperator.NotEquals);
            }

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
            this._VariationText.Text = "0";
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
            Color bgColor = Colors.Transparent;
            Style style = new Style(typeof(Infragistics.Controls.Grids.CellControl));
            style.Setters.Add(new Setter(BackgroundProperty, new SolidColorBrush(bgColor)));

            e.Row.CellStyle = style;
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

        //批量成交单
        private void ExecuteAllBtn_Click(object sender, RoutedEventArgs e)
        {
            
        }

        public void RefreshUI(int hitOrdersCount)
        {
            this._OrderTaskGrid.ItemsSource = null;

            this._OrderTaskGrid.ItemsSource = this._App.InitDataManager.OrderTaskModel.OrderTasks;
            for (int i = 0; i < hitOrdersCount; i++)
            {
                this._OrderTaskGrid.Rows[i].CellStyle = this._ExecuteStatusStyle;
            }
        }



    }
}
