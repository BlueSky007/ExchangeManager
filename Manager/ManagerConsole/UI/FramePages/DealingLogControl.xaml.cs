using Manager.Common.LogEntities;
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
using LogType = Manager.Common.LogType;

namespace ManagerConsole.FramePages
{
    /// <summary>
    /// Interaction logic for DealingLogControl.xaml
    /// </summary>
    public partial class DealingLogControl : UserControl
    {
        private App _App = Application.Current as App;
        private LogType _LogType;
        private BusyDecorator _BusyDecorator;
        public DealingLogControl(LogType logType)
        {
            InitializeComponent();
            this._LogType = logType;
            this.InitUI();
            this.AttachEvents();
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
        }

        private void QueryDealingLogData()
        {
            DateTime fromData = new DateTime(this._FromDatePicker.SelectedDate.Value.Ticks, DateTimeKind.Utc);
            DateTime toData = new DateTime(this._ToDatePicker.SelectedDate.Value.Ticks, DateTimeKind.Utc);

            //this.RetrievalCircleAnimation.Begin();
            this._BusyDecorator.Start();

            switch (this._LogType)
            { 
                case LogType.QuotePrice:
                    ConsoleClient.Instance.GetQuoteLogData(fromData, toData, this._LogType, this.GetQuoteLogDataCallback);
                    break;
                case LogType.QuoteOrder:
                    ConsoleClient.Instance.GetLogOrderData(fromData, toData, this._LogType, this.GetLogOrderDataCallback);
                    break;
                case LogType.SettingChange:
                    ConsoleClient.Instance.GetLogSettingData(fromData, toData, this._LogType, this.GetLogSettingDataCallback);
                    break;
            }
        }

        private void AttachEvents()
        { 
        }

        private void GetQuoteLogDataCallback(List<LogQuote> logQuotes)
        {
            this.Dispatcher.BeginInvoke((Action<List<LogQuote>>)delegate(List<LogQuote> result)
            {
                this._BusyDecorator.Stop();
                this._LogQuoteGrid.ItemsSource = result;
            }, logQuotes);
        }

        private void GetLogOrderDataCallback(List<LogOrder> logOrders)
        {
            this.Dispatcher.BeginInvoke((Action<List<LogOrder>>)delegate(List<LogOrder> result)
            {
                this._BusyDecorator.Stop();
                this._LogOrderGrid.ItemsSource = result;
            }, logOrders);
        }

        private void GetLogSettingDataCallback(List<LogSetting> logSettings)
        {
            this.Dispatcher.BeginInvoke((Action<List<LogSetting>>)delegate(List<LogSetting> result)
            {
                this._BusyDecorator.Stop();
                this._LogSettingGrid.ItemsSource = result;
            }, logSettings);
        }

        private void ChangeGridVisibilityStatus()
        {
            this._LogQuoteGrid.Visibility = Visibility.Collapsed;
            this._LogQuoteGrid.ItemsSource = null;
            this._LogOrderGrid.Visibility = Visibility.Collapsed;
            this._LogOrderGrid.ItemsSource = null;
            this._LogSettingGrid.Visibility = Visibility.Collapsed;
            this._LogSettingGrid.ItemsSource = null;

            switch (this._LogType)
            {
                case LogType.QuotePrice:
                    this._LogQuoteGrid.Visibility = Visibility.Visible;
                    break;
                case LogType.QuoteOrder:
                    this._LogOrderGrid.Visibility = Visibility.Visible;
                    break;
                case LogType.SettingChange:
                    this._LogSettingGrid.Visibility = Visibility.Visible;
                    break;
            }
        }
    }
}
