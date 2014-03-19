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
using ManagerConsole.ViewModel;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

namespace ManagerConsole.UI
{
    public class IdCode
    {
        public Guid QuoteId { get; set; }
        public string QuoteCode { get; set; }
    }
    /// <summary>
    /// QuotationChartWindow.xaml 的交互逻辑
    /// </summary>
    public partial class QuotationChartWindow : UserControl
    {
        public QuotationChartWindow()
        {
            InitializeComponent();
            this.Exchange.ItemsSource = App.MainFrameWindow.ExchangeDataManager.ExchangeCodes;
            ExchangeQuotationViewModel.Instance.NotifyChartWindowRealTimeData += Instance_NotifyChartWindowRealTimeData;
            this.SetZIndex();
        }

        private void Instance_NotifyChartWindowRealTimeData(string exchangeCode, Guid quotePolicyId, Guid instrumentId, DateTime timeSpan, string ask, string bid)
        {
            if (exchangeCode == this._ExchangeCode && quotePolicyId == this._QuotePolicyId)
            {
                this.SetRealTimeDataForChart(instrumentId, timeSpan, ask, bid);
            }
        }

        private ObjectForChartWindowJs _Helper;

        private string _ExchangeCode;
        private Guid _QuotePolicyId;

       

        public void SetRealTimeDataForChart(Guid instrumentId, DateTime timestamp, string ask, string bid)
        {
            this.ExchangeChart.InvokeScript("SetRealTimeData", new object[] { instrumentId.ToString(), timestamp.ToString(), ask, bid });
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            string exchangeCode = (string)cb.SelectedItem;
            this._ExchangeCode = exchangeCode;
            ObservableCollection<IdCode> quotes = new ObservableCollection<IdCode>();
            this.QuotePolicy.Items.Clear();
            foreach (Guid id in App.MainFrameWindow.ExchangeDataManager.ExchangeSettingManagers[exchangeCode].ExchangeQuotations.Keys)
            {
                IdCode quotepolicy = new IdCode();
                quotepolicy.QuoteId = id;
                quotepolicy.QuoteCode = App.MainFrameWindow.ExchangeDataManager.ExchangeSettingManagers[exchangeCode].QuotePolicies[id].Code;
                quotes.Add(quotepolicy);
            }
            this.QuotePolicy.ItemsSource = quotes;
            this.QuotePolicy.DisplayMemberPath = "QuoteCode";
        }

        private void QuotePolicy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            IdCode quotePolicy = cb.SelectedValue as IdCode;
            this._QuotePolicyId = quotePolicy.QuoteId;
            this._Helper = new ObjectForChartWindowJs(this._ExchangeCode, this._QuotePolicyId);
            this.ExchangeChart.ObjectForScripting = this._Helper;
            this.ExchangeChart.Navigate(new Uri("D:\\Teams\\iExchangeCollection\\iExchange3 Team\\Manager\\ManagerConsole.Chart.Web\\ManagerConsole.ChartTestPage.html", UriKind.Absolute));
            
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, int flags);

        IntPtr HWND_TOP = IntPtr.Zero;

        private void SetZIndex()
        {
            SetWindowPos(this.ExchangeChart.Handle, HWND_TOP, 0, 0, 0, 0, 3);
        }

    }
}
