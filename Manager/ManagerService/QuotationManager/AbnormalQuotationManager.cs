using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using Manager.Common;
using Manager.Common.QuotationEntities;

namespace ManagerService.Quotation
{
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
        private DateTime _WaitingEndTime = DateTime.MaxValue;
        private object _WaitEndTimeLock = new object();

        public AbnormalQuotationManager(Dictionary<int, PriceRangeCheckRule> rangeCheckRules, LastQuotationManager lastQuotationManager)
        {
            this._PriceRangeCheckRules = rangeCheckRules;
            this._LastQuotationManager = lastQuotationManager;
            this._Timer = new Timer(this.CheckTimeout);
        }

        public bool SetQuotation(SourceQuotation quotation)
        {
            bool isNormalAndNotWaiting = false;
            AbnormalInstrument abnormalInstrument;

            bool hasAbnormalInstrument = this._AbnormalInstruments.TryGetValue(quotation.InstrumentId, out abnormalInstrument);

            if (hasAbnormalInstrument)
            {
                if (abnormalInstrument.AppendPendingItem(quotation))
                {
                    return false;
                }
            }

            this.SetAbnormalInfo(quotation);
            if (quotation.IsAbnormal)
            {
                if (!this._PriceRangeCheckRules[quotation.InstrumentId].DiscardOutOfRangePrice)
                {
                    MainService.ExchangeManager.SwitchPriceEnableState(quotation.InstrumentId, false);
                    if (hasAbnormalInstrument)
                    {
                        abnormalInstrument.AddPendingItem(quotation);
                    }
                    else
                    {
                        abnormalInstrument = new AbnormalInstrument(quotation, this, this._PriceRangeCheckRules[quotation.InstrumentId]);
                        this._AbnormalInstruments.Add(quotation.InstrumentId, abnormalInstrument);
                    }
                    this.StartConfirm(quotation);
                }
            }
            else
            {
                isNormalAndNotWaiting = true;
            }

            return isNormalAndNotWaiting;
        }

        public void SetAbnormalInfo(SourceQuotation quotation)
        {
            GeneralQuotation last;
            quotation.IsAbnormal = false;
            if (this._LastQuotationManager.LastAccepted.TryGetLastQuotation(quotation.InstrumentId, out last))
            {
                PriceRangeCheckRule rule = this._PriceRangeCheckRules[quotation.InstrumentId];
                double diff = 0;
                double oldPrice = 0;
                switch (rule.OutOfRangeType)
                {
                    case OutOfRangeType.Ask:
                        diff = Math.Abs(quotation.Ask - last.Ask);
                        oldPrice = last.Ask;
                        break;
                    case OutOfRangeType.Bid:
                        diff = Math.Abs(quotation.Bid - last.Bid);
                        oldPrice = last.Bid;
                        break;
                    default:
                        Logger.AddEvent(TraceEventType.Error, "AbnormalQuotationManager.IsNormalPrice unknown OutOfRangeType:{0}", rule.OutOfRangeType.ToString());
                        break;
                }

                // diff to diffPoints
                int decimalPlace = MainService.QuotationManager.ConfigMetadata.Instruments[quotation.PrimitiveQuotation.InstrumentId].DecimalPlace;
                int diffPoints = (int)(Math.Round(diff * Math.Pow(10, decimalPlace), 0));


                quotation.IsAbnormal = diffPoints > rule.ValidVariation;
                if (quotation.IsAbnormal)
                {
                    quotation.DiffPoints = diffPoints;
                    quotation.OldPrice = oldPrice;
                    quotation.OutOfRangeType = rule.OutOfRangeType;
                    quotation.WaitSeconds = rule.OutOfRangeWaitTime;
                    quotation.ConfirmId = this.GetNextConfirmId();
                }
            }
        }

        public void StartConfirm(SourceQuotation quotation)
        {
            string format = "F" + MainService.QuotationManager.ConfigMetadata.Instruments[quotation.InstrumentId].DecimalPlace;
            AbnormalQuotationMessage message = new AbnormalQuotationMessage();
            message.ConfirmId = quotation.ConfirmId;
            message.InstrumentId = quotation.InstrumentId;
            message.InstrumentCode = quotation.InstrumentCode;
            message.NewPrice = quotation.OutOfRangeType == OutOfRangeType.Ask ? quotation.Ask.ToString(format) : quotation.Bid.ToString(format);
            message.OldPrice = quotation.OldPrice;
            message.OutOfRangeType = quotation.OutOfRangeType;
            message.DiffPoints = quotation.DiffPoints;
            message.WaitSeconds = quotation.WaitSeconds;
            MainService.ClientManager.Dispatch(message);

            quotation.WaitEndTime = DateTime.Now.AddSeconds(quotation.WaitSeconds + 1);  // Add one sencond at server side.
            this.ChangeWaitTime(quotation.WaitEndTime);
        }

        public ConfirmResult Confirm(int instrumentId, int confirmId, bool accepted)
        {
            ConfirmResult confirmResult = new ConfirmResult() { Confirmed = false };
            AbnormalInstrument abnormalInstrument;
            if (this._AbnormalInstruments.TryGetValue(instrumentId, out abnormalInstrument))
            {
                abnormalInstrument.Confirm(confirmId, accepted, confirmResult);
            }
            else
            {
                Logger.AddEvent(TraceEventType.Warning, "AbnormalQuotationManager.Confirm Cannot find wait queue for instrumentId={0}", instrumentId);
            }
            return confirmResult;
        }

        private int GetNextConfirmId()
        {
            lock(this._ConfirmIdLock)
            {
                if (this._LastConfirmId++ == int.MaxValue) this._LastConfirmId = 1;
                return this._LastConfirmId;
            }
        }

        private void ChangeWaitTime(DateTime waitEndTime)
        {
            lock (this._WaitEndTimeLock)
            {
                if (waitEndTime < this._WaitingEndTime)
                {
                    this._WaitingEndTime = waitEndTime;
                    TimeSpan timeSpan = this._WaitingEndTime - DateTime.Now;
                    if (timeSpan < TimeSpan.Zero) timeSpan = TimeSpan.Zero;
                    this._Timer.Change(timeSpan, TimeSpan.Zero);
                }
            }
        }

        private void CheckTimeout(object state)
        {
            this.StopTimer();
            foreach (var item in this._AbnormalInstruments.Values)
            {
                item.CheckTimeout();
            }

            DateTime? nextCheckTime = this._AbnormalInstruments.Values.Min(ai => ai.CheckTime);
            if (nextCheckTime == null)
            {
                this.StopTimer();
            }
            else
            {
                this.ChangeWaitTime(nextCheckTime.Value);
            }
        }

        private void StopTimer()
        {
            lock(this._WaitEndTimeLock)
            {
                this._WaitingEndTime = DateTime.MaxValue;
                this._Timer.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }
    }
}
