﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using Manager.Common;
using Manager.Common.QuotationEntities;

namespace ManagerService.Quotation
{
    public class AbnormalInstrument
    {
        private int _InstrumentId;
        private int _TimeoutCount;
        private Queue<Quotation> _WaitQueue;
        private AbnormalQuotationManager _AbnormalQuotationManager;
        private PriceRangeCheckRule _PriceRangeCheckRule;

        public AbnormalInstrument(Quotation firstAbnormalItem, AbnormalQuotationManager abnormalQuotationManager, PriceRangeCheckRule priceRangeCheckRule)
        {
            this._InstrumentId = firstAbnormalItem.InstrumentId;
            this._TimeoutCount = 0;
            this._WaitQueue = new Queue<Quotation>();
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

        public void AddPendingItem(Quotation quotation)
        {
            lock (this._WaitQueue)
            {
                this._WaitQueue.Enqueue(quotation);
            }
        }

        public void Confirm(int confirmId, bool accepted)
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
                    Quotation confirmedQuotation = this._WaitQueue.Dequeue();
                    if (accepted)
                    {
                        Manager.QuotationManager.ProcessNormalQuotation(confirmedQuotation);
                    }

                    // 重新确定后续价格是否异常
                    Quotation quotation;
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
                            // start confirm progress
                            this._AbnormalQuotationManager.StartConfirm(quotation);
                            break;
                        }
                        else
                        {
                            Manager.QuotationManager.ProcessNormalQuotation(this._WaitQueue.Dequeue());
                        }
                    }
                }
                else
                {
                    // quotation already timeout, do nothing.
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
                    this._WaitQueue.Dequeue();
                    if (this._TimeoutCount == this._PriceRangeCheckRule.OutOfRangeCount)
                    {
                        
                        this._TimeoutCount = 0;
                    }
                }
            }
        }
    }

    public class AbnormalQuotationManager
    {
        private int _LastConfirmId = 1;
        private object _ConfirmIdLock = new object();

        // Map for InstrumentId - Queue<Quotation>
        private Dictionary<int, AbnormalInstrument> _AbnormalInstruments = new Dictionary<int, AbnormalInstrument>();

        // Map for: InstrumentId - PriceRangeCheckRule
        private Dictionary<int, PriceRangeCheckRule> _PriceRangeCheckRules = new Dictionary<int, PriceRangeCheckRule>();

        private LastQuotationManager _LastQuotationManager;
        private Timer _Timer;
        private DateTime _WaitEndTime = DateTime.MaxValue;
        private object _WaitEndTimeLock = new object();

        public AbnormalQuotationManager(Dictionary<int, PriceRangeCheckRule> rangeCheckRules, LastQuotationManager lastQuotationManager)
        {
            this._PriceRangeCheckRules = rangeCheckRules;
            this._LastQuotationManager = lastQuotationManager;
            this._Timer = new Timer(this.CheckTimeout);
        }

        public bool SetQuotation(Quotation quotation)
        {
            bool isNormalAndNotWaiting = false;
            AbnormalInstrument abnormalInstrument;

            if (this._AbnormalInstruments.TryGetValue(quotation.InstrumentId, out abnormalInstrument))
            {
                abnormalInstrument.AddPendingItem(quotation);
            }
            else
            {
                this.SetAbnormalInfo(quotation);
                if (quotation.IsAbnormal)
                {
                    if (!this._PriceRangeCheckRules[quotation.InstrumentId].DiscardOutOfRangePrice)
                    {
                        abnormalInstrument = new AbnormalInstrument(quotation, this, this._PriceRangeCheckRules[quotation.InstrumentId]);
                        this._AbnormalInstruments.Add(quotation.InstrumentId, abnormalInstrument);
                        this.StartConfirm(quotation);
                    }
                }
                else
                {
                    isNormalAndNotWaiting = true;
                }
            }

            return isNormalAndNotWaiting;
        }

        public void SetAbnormalInfo(Quotation quotation)
        {
            Quotation last;
            quotation.IsAbnormal = false;
            if (this._LastQuotationManager.LastAccepted.TryGetLastQuotation(quotation.SourceId, quotation.InstrumentId, out last))
            {
                PriceRangeCheckRule rule = this._PriceRangeCheckRules[quotation.InstrumentId];
                double diff = 0;
                switch (rule.OutOfRangeType)
                {
                    case OutOfRangeType.Ask:
                        diff = Math.Abs(quotation.Ask - last.Ask);
                        break;
                    case OutOfRangeType.Bid:
                        diff = Math.Abs(quotation.Bid - last.Bid);
                        break;
                    default:
                        Logger.AddEvent(TraceEventType.Error, "AbnormalQuotationManager.IsNormalPrice unknown OutOfRangeType:{0}", rule.OutOfRangeType.ToString());
                        break;
                }

                // diff to diffPoints
                int decimalPlace = Manager.QuotationManager.ConfigMetadata.Instruments[quotation.PrimitiveQuotation.InstrumentCode].DecimalPlace;
                int diffPoints = (int)(diff * Math.Pow(10, decimalPlace));


                quotation.IsAbnormal = diffPoints > rule.ValidVariation;
                if (quotation.IsAbnormal)
                {
                    quotation.DiffPoints = diffPoints;
                    quotation.OutOfRangeType = rule.OutOfRangeType == OutOfRangeType.Ask ? "Ask" : "Bid";
                    quotation.WaitSeconds = rule.OutOfRangeWaitTime;
                    quotation.ConfirmId = this.GetNextConfirmId();
                }
            }
        }

        public void StartConfirm(Quotation quotation)
        {
            AbnormalQuotationMessage message = new AbnormalQuotationMessage();
            message.ConfirmId = quotation.ConfirmId;
            message.InstrumentId = quotation.InstrumentId;
            message.Ask = quotation.PrimitiveQuotation.Ask;
            message.Bid = quotation.PrimitiveQuotation.Bid;
            message.OutOfRangeType = quotation.OutOfRangeType;
            message.DiffPoints = quotation.DiffPoints;
            message.WaitSeconds = quotation.WaitSeconds;
            Manager.ClientManager.Dispatch(message);

            quotation.WaitEndTime = DateTime.Now.AddSeconds(quotation.WaitSeconds + 1);  // Add one sencond at server side.
            this.ChangeWaitTime(quotation.WaitEndTime);
        }

        public void Confirm(int instrumentId, int confirmId, bool accepted)
        {
            AbnormalInstrument abnormalInstrument;
            if (this._AbnormalInstruments.TryGetValue(instrumentId, out abnormalInstrument))
            {
                abnormalInstrument.Confirm(confirmId, accepted);
            }
            else
            {
                Logger.AddEvent(TraceEventType.Warning, "AbnormalQuotationManager.Confirm Cannot find wait queue for instrumentId={0}", instrumentId);
            }
        }

        private int GetNextConfirmId()
        {
            lock(this._ConfirmIdLock)
            {
                if (this._LastConfirmId++ == int.MaxValue) this._LastConfirmId = 1;
                return this._LastConfirmId;
            }
        }

        private void ChangeWaitTime(DateTime waitTime)
        {
            lock (this._WaitEndTimeLock)
            {
                if (waitTime < this._WaitEndTime)
                {
                    this._WaitEndTime = waitTime;
                    this._Timer.Change(this._WaitEndTime - DateTime.Now, TimeSpan.Zero);
                }
            }
        }

        private void CheckTimeout(object state)
        {
            DateTime? nextCheckTime = this._AbnormalInstruments.Values.Min(ai => ai.CheckTime);
            if (nextCheckTime == null)
            {
                this._WaitEndTime = DateTime.MaxValue;
            }
            this.ChangeWaitTime(nextCheckTime.Value);

            foreach (var item in this._AbnormalInstruments.Values)
            {
                item.CheckTimeout();
            }
        }
    }
}