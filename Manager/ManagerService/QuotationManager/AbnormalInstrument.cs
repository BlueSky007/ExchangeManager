using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Manager.Common;
using Manager.Common.QuotationEntities;

namespace ManagerService.Quotation
{
    public class ConfirmResult
    {
        public bool Confirmed;
        public SourceQuotation SourceQuotation;
    }
    public class AbnormalInstrument
    {
        private int _InstrumentId;
        private int _TimeoutCount;
        private Queue<SourceQuotation> _WaitQueue;
        private AbnormalQuotationManager _AbnormalQuotationManager;
        private PriceRangeCheckRule _PriceRangeCheckRule;

        public AbnormalInstrument(SourceQuotation firstAbnormalItem, AbnormalQuotationManager abnormalQuotationManager, PriceRangeCheckRule priceRangeCheckRule)
        {
            this._InstrumentId = firstAbnormalItem.InstrumentId;
            this._TimeoutCount = 0;
            this._WaitQueue = new Queue<SourceQuotation>();
            this._WaitQueue.Enqueue(firstAbnormalItem);
            this._AbnormalQuotationManager = abnormalQuotationManager;
            this._PriceRangeCheckRule = priceRangeCheckRule;
        }

        public DateTime? CheckTime
        {
            get
            {
                lock (this._WaitQueue)
                {
                    if (this._WaitQueue.Count > 0)
                    {
                        return this._WaitQueue.Peek().WaitEndTime;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

        public bool AppendPendingItem(SourceQuotation quotation)
        {
            lock (this._WaitQueue)
            {
                if (this._WaitQueue.Count > 0)
                {
                    this._WaitQueue.Enqueue(quotation);
                    return true;
                }
            }
            return false;
        }

        public void AddPendingItem(SourceQuotation quotation)
        {
            lock (this._WaitQueue)
            {
                this._WaitQueue.Enqueue(quotation);
            }
        }

        public void Confirm(int confirmId, bool accepted, ConfirmResult confirmResult)
        {
            this._TimeoutCount = 0;
            lock (this._WaitQueue)
            {
                if (this._WaitQueue.Any(q => q.ConfirmId == confirmId))
                {
                    while (this._WaitQueue.Peek().ConfirmId != confirmId)
                    {
                        // Discard quotations before ConfirmId == confirmId
                        this._WaitQueue.Dequeue();
                    }
                    SourceQuotation confirmedQuotation = this._WaitQueue.Dequeue();
                    MainService.ExchangeManager.SwitchPriceEnableState(confirmedQuotation.InstrumentId, true);
                    if (accepted)
                    {
                        MainService.QuotationManager.ProcessNormalQuotation(confirmedQuotation);
                    }

                    confirmResult.Confirmed = true;
                    confirmResult.SourceQuotation = confirmedQuotation;

                    this.ProcessSubsequentQuotation();
                    Logger.AddEvent(TraceEventType.Information, "[AbnormalInstrument.Confirm] Confirmed. confirmId:{0}, accepted:{1}", confirmId, accepted);
                }
                else
                {
                    // quotation already timeout, do nothing.
                    Logger.AddEvent(TraceEventType.Information, "[AbnormalInstrument.Confirm] quotation already timeout, do nothing. confirmId:{0}, accepted:{1}", confirmId, accepted);
                }
            }
        }

        public void CheckTimeout()
        {
            lock (this._WaitQueue)
            {
                DateTime now = DateTime.Now;
                while (this._WaitQueue.Count > 0 && this._WaitQueue.Peek().WaitEndTime <= now)
                {
                    this._TimeoutCount++;
                    SourceQuotation quotation = this._WaitQueue.Dequeue();
                    if (this._TimeoutCount > this._PriceRangeCheckRule.OutOfRangeCount)
                    {
                        MainService.ExchangeManager.SwitchPriceEnableState(quotation.InstrumentId, true);
                        MainService.QuotationManager.ProcessNormalQuotation(quotation);
                        this.ProcessSubsequentQuotation();
                        this._TimeoutCount = 0;
                        break;
                    }
                }
            }
        }

        private void ProcessSubsequentQuotation()
        {
            // 重新确定后续价格是否异常
            SourceQuotation quotation;
            while (this._WaitQueue.Count > 0)
            {
                quotation = this._WaitQueue.Peek();
                if (this._PriceRangeCheckRule.DiscardOutOfRangePrice)
                {
                    quotation.IsAbnormal = false;
                }
                else
                {
                    this._AbnormalQuotationManager.SetAbnormalInfo(quotation);
                }

                if (quotation.IsAbnormal)
                {
                    MainService.ExchangeManager.SwitchPriceEnableState(quotation.InstrumentId, false);
                    this._AbnormalQuotationManager.StartConfirm(quotation);
                    break;
                }
                else
                {
                    MainService.QuotationManager.ProcessNormalQuotation(this._WaitQueue.Dequeue());
                }
            }
        }
    }

}
