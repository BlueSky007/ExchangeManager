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
using ManagerConsole.Model;
using Manager.Common.LogEntities;
using LogType = Manager.Common.LogType;
using System.Drawing.Printing;
using System.Printing;
using ManagerConsole.Helper;

namespace ManagerConsole.FramePages
{
    /// <summary>
    /// Interaction logic for SourceManagerLogControl.xaml
    /// </summary>
    public partial class SourceManagerLogControl : UserControl
    {
        private App _App = Application.Current as App;
        private LogType _LogType;
        private BusyDecorator _BusyDecorator;
        public SourceManagerLogControl(LogType logType)
        {
            InitializeComponent();
            this._LogType = logType;
            this.InitUI();
            this.ChangeGridVisibilityStatus();
        }

        private void InitUI()
        {
            this._FromDatePicker.SelectedDate = DateTime.Today.AddDays(-(DateTime.Now.Day) + 1);
            this._ToDatePicker.SelectedDate = DateTime.Today.AddDays(1).AddSeconds(-1);
            this.QueryImg.Children.Add((UIElement)this.Resources["LogAuditQuery"]);
            this._BusyDecorator = new BusyDecorator(this._BusyDecoratorGrid);
        }

        //Query
        private void ToolBar_Click(object sender, RoutedEventArgs e)
        {
            Button clickButton = (Button)sender;
            if (clickButton.Name == "_QueryButton")
            {
                this.QueryDealingLogData();
            }

            //LocalPrintServer localPrintServer = new LocalPrintServer();
            //PrintQueue defaultPrintQueue = localPrintServer.DefaultPrintQueue;

            //PageSettings currentPageSettings = this._LogSourceChangeGrid.getc .GetCurrentPageSettings();

            // Infragistics.Controls.Reports.PaperSettings paperSettings = 
            //     new Infragistics.Controls.Reports.PaperSettings currentPageSettings.PaperSize, currentPageSettings.PageOrientation); 
        }

        private void QueryDealingLogData()
        {
            DateTime fromData = new DateTime(this._FromDatePicker.SelectedDate.Value.Ticks, DateTimeKind.Utc);
            DateTime toData = new DateTime(this._ToDatePicker.SelectedDate.Value.Ticks, DateTimeKind.Utc);

            this._BusyDecorator.Start();
            switch (this._LogType)
            {
                case LogType.AdjustPrice:
                    ConsoleClient.Instance.GetLogPriceData(fromData, toData, this._LogType, this.GetLogPriceDataCallback);
                    break;
                case LogType.SourceChange:
                    ConsoleClient.Instance.GetLogSourceChangeData(fromData, toData, this._LogType, this.GetSourceChangeLogCallback);
                    break;
            }
        }

        private void GetSourceChangeLogCallback(List<LogSourceChange> logSourceChange)
        {
            this.Dispatcher.BeginInvoke((Action<List<LogSourceChange>>)delegate(List<LogSourceChange> result)
            {
                this._BusyDecorator.Stop();
                if (result.Count == 0) return;
                this._LogSourceChangeGrid.ItemsSource = result;
            }, logSourceChange);
        }

        private void GetLogPriceDataCallback(List<LogPrice> logPrice)
        {
            this.Dispatcher.BeginInvoke((Action<List<LogPrice>>)delegate(List<LogPrice> result)
            {
                this._BusyDecorator.Stop();
                if (result.Count == 0) return;
                this._LogPriceGrid.ItemsSource = result;
            }, logPrice);
        }

        private void ChangeGridVisibilityStatus()
        {
            this._LogPriceGrid.Visibility = Visibility.Collapsed;
            this._LogPriceGrid.ItemsSource = null;
            this._LogSourceChangeGrid.Visibility = Visibility.Collapsed;
            this._LogSourceChangeGrid.ItemsSource = null;

            switch (this._LogType)
            {
                case LogType.AdjustPrice:
                    this._LogPriceGrid.Visibility = Visibility.Visible;
                    break;
                case LogType.SourceChange:
                    this._LogSourceChangeGrid.Visibility = Visibility.Visible;
                    break;
            }
        }
    }
}
