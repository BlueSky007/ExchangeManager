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
using System.Xml.Linq;
using Logger = Manager.Common.Logger;

namespace ManagerConsole.UI
{
    /// <summary>
    /// Interaction logic for MooMocOrderTask.xaml
    /// </summary>
    public partial class MooMocOrderTask : UserControl,IControlLayout
    {
        private CommonDialogWin _CommonDialogWin;
        private ConfirmDialogWin _ConfirmDialogWin;
        private ConfirmOrderDialogWin _ConfirmOrderDialogWin;
        private ManagerConsole.MainWindow _App;
        private ExcuteOrderConfirm _AccountInfoConfirm = null;
        private OrderHandle _OrderHandle;
        public MooMocOrderTask()
        {
            InitializeComponent();

            this._App = ((ManagerConsole.MainWindow)Application.Current.MainWindow);
            this._AccountInfoConfirm = new ExcuteOrderConfirm();

            this._CommonDialogWin = this._App._CommonDialogWin;
            this._ConfirmDialogWin = this._App._ConfirmDialogWin;
            this._ConfirmOrderDialogWin = this._App._ConfirmOrderDialogWin;
            this._OrderHandle = this._App.OrderHandle;
            this.BindGridData();
        }

        private void BindGridData()
        {
            this.MooMocOrderTaskGrid.ItemsSource = this._App.ExchangeDataManager.MooMocOrderForInstrumentModel.MooMocOrderForInstruments;
        }

        #region Action Event
        void OrderHandlerBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = sender as Button;
                MooMocOrderForInstrument mooMocOrderForInstrument = ((UnboundColumnDataContext)btn.DataContext).RowData as MooMocOrderForInstrument;
                if (mooMocOrderForInstrument == null) return;
                switch (btn.Name)
                {
                    case "AcceptBtn":
                        this._OrderHandle.OnMooMocAccept(mooMocOrderForInstrument);
                        break;
                    case "RejectBtn":
                        this._OrderHandle.OnMooMocReject(mooMocOrderForInstrument);
                        break;
                    case "ApplyBtn":
                        this._OrderHandle.OnMooMocApply(mooMocOrderForInstrument);
                        break;
                    case "ExcuteBtn":
                        this._OrderHandle.OnMooMocExecute(mooMocOrderForInstrument);
                        break;
                    case "CancelBtn":
                        this._OrderHandle.OnMooMocCancel(mooMocOrderForInstrument);
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(TraceEventType.Error, "MooMocOrderTask.OrderHandlerBtn_Click error:\r\n{0}", ex.ToString());
            }
        }

        
        #endregion


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
            layoutBuilder.Append(ColumnWidthPersistence.GetPersistentColumnsWidthString(this.MooMocOrderTaskGrid));
            layoutBuilder.Append("</GridSettings>");
            return layoutBuilder.ToString();
        }

        public void SetLayout(XElement layout)
        {
            try
            {
                if (layout.HasElements)
                {
                    XElement columnWidthElement = layout.Element("GridSettings").Element("ColumnsWidth");
                    if (columnWidthElement != null)
                    {
                        ColumnWidthPersistence.LoadColumnsWidth(this.MooMocOrderTaskGrid, columnWidthElement);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.AddEvent(System.Diagnostics.TraceEventType.Error, "MooMocOrderTask.SetLayout\r\n{0}", ex.ToString());
            }
        }
        #endregion
    }
}
