using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using Manager.Common.ExchangeEntities;
using Manager.Common;

namespace ManagerConsole.Model
{
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    [ComVisible(true)]
    public class ObjectForChartWindowJs
    {
        public ObjectForChartWindowJs(string exchangeCode, Guid quotePolicyId)
        {
            this.ExchangeCode = exchangeCode;
            this.QuotePolicyId = quotePolicyId;
        }

        public string ExchangeCode;

        public Guid QuotePolicyId;

        public string GetInitDataForChart()
        {
            try
            {
                InitDataForChart initData = new InitDataForChart();
                initData.CurrentDay = App.MainFrameWindow.ExchangeDataManager.ExchangeSettingManagers[this.ExchangeCode].TradeDay.CurrentDay;
                initData.BeginTime = App.MainFrameWindow.ExchangeDataManager.ExchangeSettingManagers[this.ExchangeCode].TradeDay.BeginTime;
                initData.EndTime = App.MainFrameWindow.ExchangeDataManager.ExchangeSettingManagers[this.ExchangeCode].TradeDay.EndTime;
                initData.EnableTrendSheetChart = false;
                initData.Language = "CHS";
                initData.Instruments = new List<InstrumentClient>();
                foreach (ExchangeQuotation exQuota in App.MainFrameWindow.ExchangeDataManager.ExchangeSettingManagers[this.ExchangeCode].ExchangeQuotations[this.QuotePolicyId].Values)
                {
                    initData.Instruments.Add(App.MainFrameWindow.ExchangeDataManager.ExchangeSettingManagers[this.ExchangeCode].Instruments[exQuota.InstruemtnId]);
                }
                initData.HighBid = App.MainFrameWindow.ExchangeDataManager.ExchangeSettingManagers[this.ExchangeCode].SystemParameter.HighBid;
                initData.LowBid = App.MainFrameWindow.ExchangeDataManager.ExchangeSettingManagers[this.ExchangeCode].SystemParameter.LowBid;
                string xml = initData.ConvertToXml();;
                return xml;
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "GetInitDataForChart\r\n{0}", ex.ToString());
                return string.Empty;
            }
        }

        public string GetChartQuotationInWpf(string instrumentId, string frequency, string fromTime, string toTime)
        {
            List<Manager.Common.ExchangeEntities.ChartQuotation> chartQuotations = new List<Manager.Common.ExchangeEntities.ChartQuotation>();
            // to server get chartquotations
            chartQuotations = ConsoleClient.Instance.GetChartQuotation(this.ExchangeCode, this.QuotePolicyId,Guid.Parse(instrumentId), frequency,DateTime.Parse(fromTime),DateTime.Parse( toTime));
            StringBuilder str = new StringBuilder();
            foreach (Manager.Common.ExchangeEntities.ChartQuotation item in chartQuotations)
            {
                str.AppendFormat("{0};", item.ToString());
            }
            return str.ToString();
        }

        public string GetLastQuotationsForTrendSheetFromWpf(string instrument,string frequency,string fromTime, string open)
        {
            Manager.Common.ExchangeEntities.ChartQuotation quotation = new Manager.Common.ExchangeEntities.ChartQuotation();
            quotation = ConsoleClient.Instance.GetLastQuotationsForTrendSheet(this.ExchangeCode, this.QuotePolicyId, Guid.Parse(instrument), frequency, DateTime.Parse(fromTime), decimal.Parse(open));
            return quotation.ToString();
        }

        public void WriteExperLog(string log)
        {
            Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "ChartQuotattionWindow\r\n{0}", log);
        }
    }
}
