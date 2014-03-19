using iExchange4.Chart.SilverlightExtension;
using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace ManagerConsole.Chart
{
    public class AsyncGetQuotationResult : IAsyncResult
    {

        public object AsyncState
        {
            get { return this._AsyncState; }
        }

        public System.Threading.WaitHandle AsyncWaitHandle
        {
            get { return null; }
        }

        public bool CompletedSynchronously
        {
            get { return true; }
        }

        public bool IsCompleted
        {
            get { return true; }
        }
        private object _AsyncState;
        public Guid InstrumentId;
        public string Frequency;
        public DateTime FromTime;
        public DateTime ToTime;
        public bool FixLastPeriod;
        public ICollection<Quotation> ResultData;
        public AsyncCallback CallBack;

        public AsyncGetQuotationResult(AsyncCallback callBack, Guid instrumentId, string frequency, DateTime fromTime, DateTime toTime, bool fixLastPeriod, object asyncState)
        {
            this.CallBack = callBack;
            this.InstrumentId = instrumentId;
            this.Frequency = frequency;
            this.FromTime = fromTime;
            this.ToTime = toTime;
            this.FixLastPeriod = fixLastPeriod;
            this._AsyncState = asyncState;
        }
    }

    public class AsyncGetLastQuotationsResult : IAsyncResult
    {
        public object AsyncState
        {
            get { return this._AsyncState; }
        }

        public System.Threading.WaitHandle AsyncWaitHandle
        {
            get { return null; }
        }

        public bool CompletedSynchronously
        {
            get { return true; }
        }

        public bool IsCompleted
        {
            get { return true; }
        }
        private object _AsyncState;
        public Guid InstrumentId;
        public string Frequency;
        public DateTime FromTime;
        public decimal Open;
        public Quotation ResultData;
        public AsyncCallback CallBack;

        public AsyncGetLastQuotationsResult(AsyncCallback callBack, Guid instrumentId, string frequency, DateTime fromTime, decimal open,object state)
        {
            this.CallBack = callBack;
            this.InstrumentId = instrumentId;
            this.Frequency = frequency;
            this.FromTime = fromTime;
            this.Open = open;
            this._AsyncState = state;
        }
    }
}
