using ManagerConsole.Helper;
using ManagerConsole.Model;
using ManagerConsole.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
using CommonTransactionError = Manager.Common.TransactionError;
using Logger = Manager.Common.Logger;

namespace ManagerConsole.UI
{
    /// <summary>
    /// Interaction logic for DQOrderTaskControl.xaml
    /// </summary>
    public partial class DQOrderTaskControl : UserControl
    {
        private CommonDialogWin _CommonDialogWin;
        private ConfirmDialogWin _ConfirmDialogWin;
        private ConfirmOrderDialogWin _ConfirmOrderDialogWin;
        private ManagerConsole.MainWindow _App;
        private ExcuteOrderConfirm _AccountInfoConfirm = null;
        private OrderHandle OrderHandle;
        public DQOrderTaskControl()
        {
            InitializeComponent();

            this._App = ((ManagerConsole.MainWindow)Application.Current.MainWindow);
            this._AccountInfoConfirm = new ExcuteOrderConfirm();

            this._CommonDialogWin = this._App.CommonDialogWin;
            this._ConfirmDialogWin = this._App.ConfirmDialogWin;
            this._ConfirmOrderDialogWin = this._App.ConfirmOrderDialogWin;
            this.OrderHandle = this._App.OrderHandle;
            //this._ConfirmOrderDialogWin.OnConfirmDialogResult += new ConfirmOrderDialogWin.ConfirmDialogResultHandle(ExcuteOrder);
            this.BindGridData();
        }

        private void BindGridData()
        {
            this.DQOrderTaskGrid.ItemsSource = this._App.InitDataManager.DQOrderTaskForInstrumentModel.DQOrderTaskForInstruments;
        }

        #region Grid Event
        void AdjustVariationText_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox variationText = sender as TextBox;

            DQOrderTaskForInstrument dQOrderTaskForInstrument = variationText.DataContext as DQOrderTaskForInstrument;
            InstrumentClient instrument = dQOrderTaskForInstrument.Instrument;
            int variation = dQOrderTaskForInstrument.Variation;
            if (dQOrderTaskForInstrument == null) return;

            if (instrument.CheckVariation(variation))
            {
                dQOrderTaskForInstrument.Variation = variation;
            }
            else
            {
                dQOrderTaskForInstrument.Variation = 0 - instrument.AcceptDQVariation;
            }
        }

        void AdjustVariation_Click(object sender, RoutedEventArgs e)
        {
            int variation;
            Button btn = sender as Button;

            DQOrderTaskForInstrument dQOrderTaskForInstrument = btn.DataContext as DQOrderTaskForInstrument;
            InstrumentClient instrument = dQOrderTaskForInstrument.Instrument;
            int points = dQOrderTaskForInstrument.Variation;
            if (dQOrderTaskForInstrument == null) return;
            try
            {
                switch (btn.Name)
                {
                    case "UpButton":
                        variation = points + (int)instrument.NumeratorUnit;
                        if (instrument.CheckVariation(variation))
                        {
                            dQOrderTaskForInstrument.Variation = points + (int)instrument.NumeratorUnit;
                        }
                        else
                        {
                            dQOrderTaskForInstrument.Variation = 0 - instrument.AcceptDQVariation;
                        }
                        break;
                    case "DownButton":
                        variation = points - (int)instrument.NumeratorUnit;
                        if (instrument.CheckVariation(variation))
                        {
                            dQOrderTaskForInstrument.Variation = points - (int)instrument.NumeratorUnit;
                        }
                        else
                        {
                            dQOrderTaskForInstrument.Variation = 0 - instrument.AcceptDQVariation;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(TraceEventType.Error, "DQOrderTaskControl.AdjustVariation_Click error:\r\n{0}", ex.ToString());
            }
        }

        void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            this.DQOrderTaskGrid.ExitEditMode(true);
            CheckBox chk = sender as CheckBox;
            //DQOrderTaskForInstrument dQOrderTaskForInstrument;
            try
            {
                switch (chk.Name)
                {
                }
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(TraceEventType.Error, "DQOrderTaskControl.CheckBox_Click error:\r\n{0}", ex.ToString());
            }
        }

        private void IsOPenCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ComboBox cmb = sender as ComboBox;

                if (e.AddedItems.Count <= 0) return;
                string isOpenString = e.AddedItems[0].ToString();
                DQOrderTaskForInstrument dQOrderTaskForInstrument = cmb.DataContext as DQOrderTaskForInstrument;
                string buySellString = dQOrderTaskForInstrument.BuySellString;

                if (dQOrderTaskForInstrument != null)
                {
                    dQOrderTaskForInstrument.FilterOrderTask(buySellString, isOpenString);
                }
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(TraceEventType.Error, "DQOrderTaskControl.IsOPenCombo_SelectionChanged error:\r\n{0}", ex.ToString());
            }
        }

        private void BuySellCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ComboBox cmb = sender as ComboBox;

                if (e.AddedItems.Count <= 0) return;
                string buySellString = e.AddedItems[0].ToString();

                DQOrderTaskForInstrument dQOrderTaskForInstrument = cmb.DataContext as DQOrderTaskForInstrument;
                string isOpenString = dQOrderTaskForInstrument.IsOpenString;
                if (dQOrderTaskForInstrument != null)
                {
                    dQOrderTaskForInstrument.FilterOrderTask(buySellString, isOpenString);
                }
            }
            catch (Exception ex)
            {
                Manager.Common.Logger.TraceEvent(TraceEventType.Error, "DQOrderTaskControl.BuySellCombo_SelectionChanged error:\r\n{0}", ex.ToString());
            }
        }

        private void DQOrderTaskGrid_InitializeRow(object sender, Infragistics.Controls.Grids.InitializeRowEventArgs e)
        {
            Color bgColor = Colors.Transparent;
            Style style = new Style(typeof(Infragistics.Controls.Grids.CellControl));
            style.Setters.Add(new Setter(BackgroundProperty, new SolidColorBrush(bgColor)));

            e.Row.CellStyle = style;
        }
        #endregion

        #region Order Action Event
        private void OrderHandlerBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = sender as Button;
                DQOrderTaskForInstrument dQOrderTaskForInstrument; 
                OrderTask orderTask;
                CellDataDefine currentCellData;
                switch (btn.Name)
                {
                    case "DQAcceptBtn":
                        dQOrderTaskForInstrument = btn.DataContext as DQOrderTaskForInstrument;
                        this.ProcessAllOrder(dQOrderTaskForInstrument,true);
                        break;
                    case "DQRejecBtn":
                        dQOrderTaskForInstrument = btn.DataContext as DQOrderTaskForInstrument;
                        this.ProcessAllOrder(dQOrderTaskForInstrument, false);
                        break;
                    case "DQAcceptSigleBtn":
                        orderTask = btn.DataContext as OrderTask;
                        if (!orderTask.IsSelected) return;
                        currentCellData = null;// orderTask.DQCellDataDefine1;
                        this.ProcessPendingOrder(orderTask, currentCellData);
                        break;
                    case "DQRejectSigleBtn":
                        orderTask = btn.DataContext as OrderTask;
                        if (!orderTask.IsSelected) return;
                        currentCellData =null;// orderTask.DQCellDataDefine2;
                        this.ProcessPendingOrder(orderTask, currentCellData);
                        break;
                }
            }
            catch (Exception ex)
            {
                Manager.Common.Logger.TraceEvent(TraceEventType.Error, "DQOrderTaskControl.OrderHandlerBtn_Click error:\r\n{0}", ex.ToString());
            }
        }

        private void ProcessAllOrder(DQOrderTaskForInstrument dQOrderTaskForInstrument, bool acceptOrCancel)
        {
            var executeAllOrder = dQOrderTaskForInstrument.OrderTasks.Where(P => P.IsSelected == true);
            foreach (OrderTask orderTask in executeAllOrder)
            {
                if (acceptOrCancel)
                {
                    this.ProcessPendingOrder(orderTask, dQOrderTaskForInstrument.DQAcceptCellDataDefine);
                }
                else
                {
                    this.ProcessPendingOrder(orderTask, dQOrderTaskForInstrument.DQRejectCellDataDefine);
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
                        this.OrderHandle.OnOrderAccept(orderTask); // DQ Order Accept
                    }
                    break;
                case HandleAction.OnOrderReject:
                    isEnabled = currentCellData.IsEnable;
                    if (isEnabled)
                    {
                        this.OrderHandle.OnOrderReject(orderTask);
                    }
                    break;
            }
        }
        #endregion


    }
}
