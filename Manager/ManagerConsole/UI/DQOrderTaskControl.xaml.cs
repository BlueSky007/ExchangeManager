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
        public DQOrderTaskControl()
        {
            InitializeComponent();

            this._App = ((ManagerConsole.MainWindow)Application.Current.MainWindow);
            this._AccountInfoConfirm = new ExcuteOrderConfirm();

            this._CommonDialogWin = this._App.CommonDialogWin;
            this._ConfirmDialogWin = this._App.ConfirmDialogWin;
            this._ConfirmOrderDialogWin = this._App.ConfirmOrderDialogWin;
            this._ConfirmOrderDialogWin.OnConfirmDialogResult += new ConfirmOrderDialogWin.ConfirmDialogResultHandle(ExcuteOrder);
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
                        currentCellData = orderTask.DQCellDataDefine1;
                        this.ProcessPendingOrder(orderTask, currentCellData);
                        break;
                    case "DQRejectSigleBtn":
                        orderTask = btn.DataContext as OrderTask;
                        if (!orderTask.IsSelected) return;
                        currentCellData = orderTask.DQCellDataDefine2;
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
            CellDataDefine currentCellData;
            var executeAllOrder = dQOrderTaskForInstrument.OrderTasks.Where(P => P.IsSelected == true);
            foreach (OrderTask orderTask in executeAllOrder)
            {
                if (acceptOrCancel)
                {
                    currentCellData = orderTask.CellDataDefine1;
                    this.ProcessPendingOrder(orderTask, currentCellData);
                }
                else
                {
                    currentCellData = orderTask.CellDataDefine2;
                    this.ProcessPendingOrder(orderTask, currentCellData);
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
                        this.OnOrderAccept(orderTask); // DQ Order Accept
                    }
                    break;
                case HandleAction.OnOrderReject:
                    isEnabled = currentCellData.IsEnable;
                    if (isEnabled)
                    {
                        this.OnOrderReject(orderTask);
                    }
                    break;
            }
        }
        #endregion


        #region Quote Order
        private void OnOrderAccept(OrderTask order)
        {
            SystemParameter systemParameter = this._App.InitDataManager.SettingsManager.SystemParameter;
            systemParameter.CanDealerViewAccountInfo = true;
            bool isOK = OrderTaskManager.CheckDQOrder(order, systemParameter);
            isOK = false;

            if (!isOK)
            {
                if (systemParameter.CanDealerViewAccountInfo)
                {
                    this._ConfirmOrderDialogWin.ShowDialogWin("55", "Confrim",order);
                    return;
                }
                else
                {
                    if (MessageBox.Show("Accept the order?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        this.AcceptPlace(order.Transaction.Id);
                        ObservableCollection<DQOrderTaskForInstrument> dQOrderTaskForInstruments = this._App.InitDataManager.DQOrderTaskForInstrumentModel.DQOrderTaskForInstruments;
                        DQOrderTaskForInstrument dQOrderTaskForInstrument = dQOrderTaskForInstruments.SingleOrDefault(P => P.Instrument.Id == order.Instrument.Id);
                        dQOrderTaskForInstrument.OrderTasks.Remove(order); 
                    }
                }
            }
        }

        private void OnOrderReject(OrderTask order)
        {
            SystemParameter systemParameter = this._App.InitDataManager.SettingsManager.SystemParameter;

            systemParameter.ConfirmRejectDQOrder = true;//test Data from WebConfig

            if (systemParameter.ConfirmRejectDQOrder)
            {
                if (MessageBox.Show("Are you sure reject the order?", "Reject", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    if (order.OrderStatus == OrderStatus.WaitAcceptRejectPlace
                        || order.OrderStatus == OrderStatus.WaitAcceptRejectCancel
                        || order.OrderStatus == OrderStatus.WaitOutPriceDQ
                        || order.OrderStatus == OrderStatus.WaitOutLotDQ
                        || (order.OrderStatus == OrderStatus.WaitNextPrice && order.Transaction.OrderType == Manager.Common.OrderType.Limit))
                    {
                        foreach(Order orderEntity in order.Transaction.Orders)
                        {
                            orderEntity.Status = OrderStatus.Deleting;
                        }
                        this.RejectPlace(order.Transaction.Id);
                    }
                    else
                    {
                        string sMsg = "The order is canceled or executed already";
                        this._CommonDialogWin.ShowDialogWin(sMsg, "Alert");
                    }
                    ObservableCollection<DQOrderTaskForInstrument> dQOrderTaskForInstruments = this._App.InitDataManager.DQOrderTaskForInstrumentModel.DQOrderTaskForInstruments;
                    DQOrderTaskForInstrument dQOrderTaskForInstrument = dQOrderTaskForInstruments.SingleOrDefault(P => P.Instrument.Id == order.Instrument.Id);
                    dQOrderTaskForInstrument.OrderTasks.Remove(order); 
                }
            }
        }

        private void AcceptPlace(Guid transactionId)
        {
            ConsoleClient.Instance.AcceptPlace(transactionId, AcceptPlaceCallback);
        }

        private void RejectPlace(Guid transactionId)
        {
            ConsoleClient.Instance.CancelPlace(transactionId,Manager.Common.CancelReason.CustomerCanceled, RejectPlaceCallback);
        }
        //Call Back Event
        private void AcceptPlaceCallback(CommonTransactionError transactionError)
        {
            this.Dispatcher.BeginInvoke((Action<CommonTransactionError>)delegate(CommonTransactionError result)
            {
                if (result == CommonTransactionError.OK)
                {
                    this._CommonDialogWin.ShowDialogWin("Accept Place Succeed", "Infromation");
                }
            }, transactionError);
        }
        private void RejectPlaceCallback(CommonTransactionError transactionError)
        {
            this.Dispatcher.BeginInvoke((Action<CommonTransactionError>)delegate(CommonTransactionError result)
            {
                if (result == CommonTransactionError.OK)
                {
                    this._CommonDialogWin.ShowDialogWin("Reject Place Succeed", "Infromation");
                }
            }, transactionError);
        }
        #endregion

        void ExcuteOrder(bool yesOrNo, UIElement uIElement,OrderTask orderTask)
        {
            if (yesOrNo)
            {
                this.AcceptPlace(orderTask.Transaction.Id);
                ObservableCollection<DQOrderTaskForInstrument> dQOrderTaskForInstruments = this._App.InitDataManager.DQOrderTaskForInstrumentModel.DQOrderTaskForInstruments;
                DQOrderTaskForInstrument dQOrderTaskForInstrument = dQOrderTaskForInstruments.SingleOrDefault(P => P.Instrument.Id == orderTask.Instrument.Id);
                dQOrderTaskForInstrument.OrderTasks.Remove(orderTask); 
            }
        }
    }
}
