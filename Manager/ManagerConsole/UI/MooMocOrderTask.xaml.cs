using ManagerConsole.Helper;
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
        public MooMocOrderTask()
        {
            InitializeComponent();

            this._App = ((ManagerConsole.MainWindow)Application.Current.MainWindow);
            this._AccountInfoConfirm = new ExcuteOrderConfirm();

            this._CommonDialogWin = this._App.CommonDialogWin;
            this._ConfirmDialogWin = this._App.ConfirmDialogWin;
            this._ConfirmOrderDialogWin = this._App.ConfirmOrderDialogWin;
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
        }
        #endregion
    }
}
