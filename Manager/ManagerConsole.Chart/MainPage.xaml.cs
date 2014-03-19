using iExchange4.Chart.SilverlightExtension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Browser;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace ManagerConsole.Chart
{
    public partial class MainPage : UserControl
    {
        public MainPage()
        {
            InitializeComponent();
            RealInitailize();
            HtmlPage.RegisterScriptableObject("Page", this);
        }

        private QuotationProvider _QuotationProvider = new QuotationProvider();
        private ChartManager _ChartManager;

        public void RealInitailize()
        {
            try
            {
                var initDataNode = HtmlPage.Window.Invoke("GetInitData");
                SettingsProvider settingsProvider = new SettingsProvider(initDataNode.ToString()); //new SettingsProvider(data.Instruments,data.EnableTrendSheetChart,data.HighBid,data.LowBid);
                ChartManager chartManager = new ChartManager(this._QuotationProvider, settingsProvider, new ChartChildWindowManager());
                this._ChartManager = chartManager;
                iExchange4.Chart.SilverlightExtensionToolbar.Toolbar toolbar = new iExchange4.Chart.SilverlightExtensionToolbar.Toolbar(this._ChartManager);
                ChartPanel chartPanel = this._ChartManager.CreateChartPanel(null, Frequency.Minute);
                this.Chart.Content = chartPanel;
                toolbar.ChartPanel = chartPanel;
                chartPanel.RefreshChart(false);
                //this._ChartManager.RefreshBar(Guid.Parse("AF10D803-F811-45D1-A0E3-DDB5BAD79FF1"), DateTime.Now);
            }
            catch (Exception ex)
            {
                HtmlPage.Window.Invoke("WriteLog", ex.ToString());
            }
        }

        [ScriptableMember]
        public void SetRealTimeData(string instrumentId, string timestamp, string ask, string bid)
        {
            try
            {
                this._ChartManager.SetRealTimeData(Guid.Parse(instrumentId), DateTime.Parse(timestamp), double.Parse(ask), double.Parse(bid));
            }
            catch (Exception ex)
            {
                HtmlPage.Window.Invoke("WriteLog", ex.ToString());
            }
        }
    }
}
