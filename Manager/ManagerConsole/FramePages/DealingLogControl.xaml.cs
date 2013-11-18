using Manager.Common;
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

namespace ManagerConsole.FramePages
{
    /// <summary>
    /// Interaction logic for DealingLogControl.xaml
    /// </summary>
    public partial class DealingLogControl : UserControl
    {
        private App _App = Application.Current as App;
        private LogType _LogType;
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
            this.RetrieveStoredLinksMask.Visibility = Visibility.Visible;

            switch (this._LogType)
            { 
                case LogType.QuotePrice:
                    ConsoleClient.Instance.GetQuoteLogData(fromData, toData, this._LogType, this.GetQuoteLogDataCallback);
                    break;
                case LogType.QuoteOrder:
                    ConsoleClient.Instance.GetLogOrderData(fromData, toData, this._LogType, this.GetLogOrderDataCallback);
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
                this.RetrieveStoredLinksMask.Visibility = Visibility.Collapsed;
                this._LogQuoteGrid.ItemsSource = result;
            }, logQuotes);
        }

        private void GetLogOrderDataCallback(List<LogOrder> logOrders)
        {
            this.Dispatcher.BeginInvoke((Action<List<LogOrder>>)delegate(List<LogOrder> result)
            {
                this.RetrieveStoredLinksMask.Visibility = Visibility.Collapsed;
                this._LogOrderGrid.ItemsSource = result;
            }, logOrders);
        }

        private void ChangeGridVisibilityStatus()
        {
            this._LogQuoteGrid.Visibility = Visibility.Collapsed;
            this._LogQuoteGrid.ItemsSource = null;
            this._LogOrderGrid.Visibility = Visibility.Collapsed;
            this._LogOrderGrid.ItemsSource = null;

            switch (this._LogType)
            {
                case LogType.QuotePrice:
                    this._LogQuoteGrid.Visibility = Visibility.Visible;
                    break;
                case LogType.QuoteOrder:
                    this._LogOrderGrid.Visibility = Visibility.Visible;
                    break;
            }
        }
    }
}
