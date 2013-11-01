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
        
        public LMTProcess()
        {
            InitializeComponent();
            this._App = ((ManagerConsole.MainWindow)Application.Current.MainWindow);
            this.GetLMTOrdersToExecute();
            this.BindGrid();
        }
        public void RefreshUI()
        {
            this.GetLMTOrdersToExecute();
            this.BindGrid();
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

                    //lMTProcessForInstrument.OnEmptyOrderTask += new OrderTaskForInstrument.EmptyOrderHandle(OrderTaskForInstrument_OnEmptyOrderTask);
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
 
        }

        #region Grid Event
        private void OrderTaskGrid_InitializeRow(object sender, Infragistics.Controls.Grids.InitializeRowEventArgs e)
        {
            Color bgColor = Colors.Transparent;
            Style style = new Style(typeof(Infragistics.Controls.Grids.CellControl));
            style.Setters.Add(new Setter(BackgroundProperty, new SolidColorBrush(bgColor)));
            e.Row.CellStyle = style;
        }
        #endregion
    }
}
