using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using ChartQuotation = iExchange4.Chart.SilverlightExtension.Quotation;
using ChartFrequency = iExchange4.Chart.SilverlightExtension.Frequency;
using System.Threading;
using System.Collections.Generic;
using System.Windows.Browser;


namespace ManagerConsole.Chart
{
    public class QuotationProvider : iExchange4.Chart.SilverlightExtension.IQuotationProvider
    {
        public IAsyncResult BeginGetQuotations(Guid instrumentId, ChartFrequency frequency, DateTime fromTime, DateTime toTime, bool fixLastPeriod, AsyncCallback callback, object state)
        {
            try
            {
                AsyncGetQuotationResult result = new AsyncGetQuotationResult(callback,instrumentId, frequency.ToString(), fromTime, toTime, fixLastPeriod,state);
                ThreadPool.QueueUserWorkItem(GetChartQuotation, result);
                return result;
            }
            catch (Exception ex)
            {
                HtmlPage.Window.Invoke("WriteLog", ex.ToString());
                //Logger.TraceEvent(Omnicare.Diagnostics.TraceEventType.Error, "{0}", ex.ToString());
                return null;
            }
        }

        public void GetChartQuotation(object result)
        {
            try
            {
                AsyncGetQuotationResult quotationResult = (AsyncGetQuotationResult)result;
                Deployment.Current.Dispatcher.BeginInvoke((Action)delegate()
                {
                    var chartQuotationStr = HtmlPage.Window.Invoke("GetChartQuotation", quotationResult.InstrumentId, quotationResult.Frequency.ToString(), quotationResult.FromTime.ToString(), quotationResult.ToTime.ToString());
                    List<ChartQuotation> chartQuotes = new List<ChartQuotation>();
                    string[] strs = chartQuotationStr.ToString().Split(';');
                    foreach (string str in strs)
                    {
                        if (!string.IsNullOrEmpty(str))
                        {
                            ChartQuotation quotation = ChartQuotation.Parse(str);
                            chartQuotes.Add(quotation);
                        }
                    }
                    quotationResult.ResultData = chartQuotes;
                    quotationResult.CallBack(quotationResult);
                }, null);
            }
            catch (Exception ex)
            {
                HtmlPage.Window.Invoke("WriteLog", ex.ToString());
            }
        }

        public void GetLastQuotationsForTrendSheet(object obj)
        {
            AsyncGetLastQuotationsResult lastQuotation = (AsyncGetLastQuotationsResult)obj;
            Deployment.Current.Dispatcher.BeginInvoke((Action)delegate()
            {
                var lastTrendSheet = HtmlPage.Window.Invoke("GetLastQuotationsForTrendSheet");
            }, null);

        }

        public ICollection<ChartQuotation> EndGetQuotations(IAsyncResult result)
        {
            try
            {
                AsyncGetQuotationResult quotationResult = (AsyncGetQuotationResult)result;
                return quotationResult.ResultData;
            }
            catch (Exception ex)
            {
                HtmlPage.Window.Invoke("WriteLog", ex.ToString());
                //Trace.TraceEvent(Omnicare.Diagnostics.TraceEventType.Error, 0, exception.ToString());
                return new ChartQuotation[] { };
            }
        }

        public IAsyncResult BeginGetLastQuotationsForTrendSheet(Guid instrumentId, ChartFrequency frequency, DateTime fromTime, decimal open, AsyncCallback callback, object state)
        {
            AsyncGetLastQuotationsResult result = new AsyncGetLastQuotationsResult(callback, instrumentId, frequency.ToString(), fromTime, open, state);
            ThreadPool.QueueUserWorkItem(GetLastQuotationsForTrendSheet, result);
            return result;
        }

        public ChartQuotation EndGetLastQuotationsForTrendSheet(IAsyncResult result)
        {
            try
            {
                AsyncGetLastQuotationsResult lastQuotation = (AsyncGetLastQuotationsResult)result;
                return lastQuotation.ResultData;
            }
            catch (Exception ex)
            {
                HtmlPage.Window.Invoke("WriteLog", ex.ToString());
                //Trace.TraceEvent(Omnicare.Diagnostics.TraceEventType.Error, 0, exception.ToString());
                return null;
            }
        }
    }
}
