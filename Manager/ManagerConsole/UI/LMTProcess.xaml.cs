using ManagerConsole.Helper;
using ManagerConsole.Model;
using ManagerConsole.ViewModel;
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
using OrderType = Manager.Common.OrderType;

namespace ManagerConsole.UI
{
    /// <summary>
    /// Interaction logic for LMTProcess.xaml
    /// </summary>
    public partial class LMTProcess : UserControl
    {
        private ManagerConsole.MainWindow _App;
        private CommonDialogWin _CommonDialogWin;
        private ConfirmDialogWin _ConfirmDialogWin;
        public LMTProcess()
        {
            InitializeComponent();

            this.InitializeData();
            this._App = ((ManagerConsole.MainWindow)Application.Current.MainWindow);
            this.GetLMTOrdersToExecute();
            this.BindGrid();
            this.AttachEvent();
        }

        private void AttachEvent()
        {
            this._App.InitDataManager.OnOrderHitPriceNotifyEvent += new InitDataManager.OrderHitPriceNotifyHandler(this.ReceivedNotifyOnOrderHitPrice);
            this._App.OrderHandle.OnOrderWaitNofityEvent += new OrderHandle.WaitOrderNotifyHandler(this.ReceivedNotifyOnOrderWaite);
            this._App.OrderHandle.OnExecuteOrderNotifyEvent += new OrderHandle.ExecuteOrderNotifyHandler(this.ReceivedNotifyOnOrderExecute);
        }

        private void InitializeData()
        {
            this._App = ((ManagerConsole.MainWindow)Application.Current.MainWindow);
            this._CommonDialogWin = this._App.CommonDialogWin;
            this._ConfirmDialogWin = this._App.ConfirmDialogWin;
        }

        public void RefreshUI()
        {
            this.GetLMTOrdersToExecute();
            this.BindGrid();
        }

        private void ToolBar_Click(object sender, RoutedEventArgs e)
        {
            this.GetLMTOrdersToExecute();
        }

        private void ReceivedNotifyOnOrderWaite(OrderTask orderTask)
        {
            LMTProcessForInstrument lMTProcessForInstrument = null;
            lMTProcessForInstrument = this._App.InitDataManager.LMTProcessModel.LMTProcessForInstruments.SingleOrDefault(P => P.Instrument.Code == orderTask.Transaction.Instrument.Code);
            if (lMTProcessForInstrument == null) return;
            lMTProcessForInstrument.RemoveLmtOrderTask(orderTask);
        }

        private void ReceivedNotifyOnOrderExecute(OrderTask orderTask)
        {
            LMTProcessForInstrument lMTProcessForInstrument = null;
            lMTProcessForInstrument = this._App.InitDataManager.LMTProcessModel.LMTProcessForInstruments.SingleOrDefault(P => P.Instrument.Code == orderTask.Transaction.Instrument.Code);
            if (lMTProcessForInstrument == null) return;
            lMTProcessForInstrument.RemoveLmtOrderTask(orderTask);
        }

        private void ReceivedNotifyOnOrderHitPrice(OrderTask orderTask)
        {
            LMTProcessForInstrument lMTProcessForInstrument = null;
            lMTProcessForInstrument = this._App.InitDataManager.LMTProcessModel.LMTProcessForInstruments.SingleOrDefault(P => P.Instrument.Code == orderTask.Transaction.Instrument.Code);
            if (lMTProcessForInstrument == null)
            {
                lMTProcessForInstrument = new LMTProcessForInstrument();
                InstrumentClient instrument = orderTask.Transaction.Instrument;
                lMTProcessForInstrument.Instrument = instrument;
                lMTProcessForInstrument.Origin = instrument.Origin;

                this._App.InitDataManager.LMTProcessModel.LMTProcessForInstruments.Add(lMTProcessForInstrument);
            }
            lMTProcessForInstrument.OnEmptyLmtOrderTask += new LMTProcessForInstrument.OnEmptyLmtOrderTaskHandle(LmtOrderTaskForInstrument_OnEmptyLmtOrderTask);
            lMTProcessForInstrument.OrderTasks.Add(orderTask);
        }

        private void GetLMTOrdersToExecute()
        {
            foreach (Order order in this._App.InitDataManager.GetOrders())
            {
                if (order.Transaction.OrderType == OrderType.Limit &&
                    (order.Status == OrderStatus.WaitOutPriceLMT
                    || order.Status == OrderStatus.WaitOutLotLMT
                    || order.Status == OrderStatus.WaitOutLotLMTOrigin))
                {
                    OrderTask orderTask = new OrderTask(order);
                    orderTask.BaseOrder = order;

                    LMTProcessForInstrument lMTProcessForInstrument = null;
                    lMTProcessForInstrument = this._App.InitDataManager.LMTProcessModel.LMTProcessForInstruments.SingleOrDefault(P => P.Instrument.Code == order.Transaction.Instrument.Code);
                    if (lMTProcessForInstrument == null)
                    {
                        lMTProcessForInstrument = new LMTProcessForInstrument();
                        InstrumentClient instrument = order.Transaction.Instrument;
                        lMTProcessForInstrument.Instrument = instrument;
                        lMTProcessForInstrument.Origin = instrument.Origin;

                        this._App.InitDataManager.LMTProcessModel.LMTProcessForInstruments.Add(lMTProcessForInstrument);
                    }

                    lMTProcessForInstrument.OnEmptyLmtOrderTask += new LMTProcessForInstrument.OnEmptyLmtOrderTaskHandle(LmtOrderTaskForInstrument_OnEmptyLmtOrderTask);
                    orderTask.SetCellDataDefine(orderTask.OrderStatus);
                    lMTProcessForInstrument.OrderTasks.Add(orderTask);
                }
            }
        }

        private void BindGrid()
        {
            this.LMTProcessOrderGrid.ItemsSource = this._App.InitDataManager.LMTProcessModel.LMTProcessForInstruments;
        }

        private void OrderHandlerBtn_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            LMTProcessForInstrument lMTProcessForInstrument = null;
            switch (btn.Name)
            {
                case "ApplyBtn":
                    lMTProcessForInstrument = btn.DataContext as LMTProcessForInstrument;
                    this._App.OrderHandle.OnLMTApply(lMTProcessForInstrument);
                    break;
                case "ExecuteBtn":
                    lMTProcessForInstrument = btn.DataContext as LMTProcessForInstrument;
                    this._App.OrderHandle.OnLMTExecute(lMTProcessForInstrument);
                    break;
            }
        }

        #region Grid Event
        private void OrderTaskGrid_InitializeRow(object sender, Infragistics.Controls.Grids.InitializeRowEventArgs e)
        {
            //Color bgColor = Colors.Transparent;
            //Style style = new Style(typeof(Infragistics.Controls.Grids.CellControl));
            //style.Setters.Add(new Setter(BackgroundProperty, new SolidColorBrush(bgColor)));
            //e.Row.CellStyle = style;
        }
        #endregion

        #region Empty OrderTask Event
        void LmtOrderTaskForInstrument_OnEmptyLmtOrderTask(LMTProcessForInstrument lMTProcessForInstrument)
        {
            this._App.InitDataManager.LMTProcessModel.LMTProcessForInstruments.Remove(lMTProcessForInstrument);
        }

        #endregion
    }
}
