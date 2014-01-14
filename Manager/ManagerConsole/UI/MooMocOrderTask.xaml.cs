using Infragistics.Controls.Grids;
using ManagerConsole.Helper;
using ManagerConsole.Model;
using ManagerConsole.ViewModel;
using System;
using System.Collections.Generic;
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
using Logger = Manager.Common.Logger;

namespace ManagerConsole.UI
{
    /// <summary>
    /// Interaction logic for MooMocOrderTask.xaml
    /// </summary>
    public partial class MooMocOrderTask : UserControl
    {
        private CommonDialogWin _CommonDialogWin;
        private ConfirmDialogWin _ConfirmDialogWin;
        private ConfirmOrderDialogWin _ConfirmOrderDialogWin;
        private ManagerConsole.MainWindow _App;
        private ExcuteOrderConfirm _AccountInfoConfirm = null;
        private OrderHandle OrderHandle;
        public MooMocOrderTask()
        {
            InitializeComponent();

            this._App = ((ManagerConsole.MainWindow)Application.Current.MainWindow);
            this._AccountInfoConfirm = new ExcuteOrderConfirm();

            this._CommonDialogWin = this._App._CommonDialogWin;
            this._ConfirmDialogWin = this._App._ConfirmDialogWin;
            this._ConfirmOrderDialogWin = this._App._ConfirmOrderDialogWin;
            OrderHandle = new OrderHandle();
            //this._ConfirmOrderDialogWin.OnConfirmDialogResult += new ConfirmOrderDialogWin.ConfirmDialogResultHandle(ExcuteOrder);
            this.BindGridData();
        }

        private void BindGridData()
        {
            this.MooMocOrderTaskGrid.ItemsSource = this._App.InitDataManager.MooMocOrderForInstrumentModel.MooMocOrderForInstruments;
        }


        #region Action Event
        void OrderHandlerBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = sender as Button;
                MooMocOrderForInstrument mooMocOrderForInstrument;
                OrderTask orderTask;
                switch (btn.Name)
                {
                    case "AcceptBtn":
                        mooMocOrderForInstrument = ((UnboundColumnDataContext)btn.DataContext).RowData as MooMocOrderForInstrument;
                        //this.OrderHandle.OnLMTApply();
                        break;
                    case "RejectBtn":
                        MessageBox.Show("Reject");
                        break;
                    case "ApplyBtn":
                        MessageBox.Show("ApplyBtn");
                        break;
                    case "ExcuteBtn":
                        MessageBox.Show("ExcuteBtn");
                        break;
                    case "CancelBtn":
                        MessageBox.Show("CancelBtn");
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(TraceEventType.Error, "MooMocOrderTask.OrderHandlerBtn_Click error:\r\n{0}", ex.ToString());
            }
        }
        #endregion
    }
}
