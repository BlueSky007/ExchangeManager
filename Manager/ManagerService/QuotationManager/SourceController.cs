using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Manager.Common.QuotationEntities;
using Manager.Common;
using ManagerService.DataAccess;
using Manager.Common.LogEntities;
using ManagerService.Audit;
using System.Diagnostics;

namespace ManagerService.Quotation
{
    public class AgioCalculator
    {
        private TimeSpan _MonitorTimeSpan;
        private int _LeastTicks;

        // Map for SourceId - AveragePrice( null 表示在_AgioSeconds 之内没有收到价格)
        private Dictionary<int, double?> _AveragePrices = new Dictionary<int, double?>();

        // Map for SourceId - Queue<Quotation>
        private Dictionary<int, Queue<SourceQuotation>> _QuotationSamples = new Dictionary<int, Queue<SourceQuotation>>();

        public AgioCalculator(int agioSeconds, int leastTicks, IEnumerable<int> sourceIds)
        {
            this._MonitorTimeSpan = TimeSpan.FromSeconds(agioSeconds);
            this._LeastTicks = leastTicks;
            foreach (int sourceId in sourceIds)
            {
                this._AveragePrices.Add(sourceId, null);
                this._QuotationSamples.Add(sourceId, new Queue<SourceQuotation>());
            }
        }

        public void QuotationArrived(SourceQuotation quotation)
        {
            // remove expired sample data
            DateTime cutOffTime = DateTime.Now - this._MonitorTimeSpan;
            Queue<SourceQuotation> sampleQueue = this._QuotationSamples[quotation.SourceId];
            while (sampleQueue.Count > 0)
            {
                SourceQuotation q = sampleQueue.Peek();
                if (q.Timestamp < cutOffTime)
                {
                    sampleQueue.Dequeue();
                }
                else
                {
                    break;
                }
            }

            // add new sample
            sampleQueue.Enqueue(quotation);

            // calculate average price
            lock (this._AveragePrices)
            {
                if (sampleQueue.Count > this._LeastTicks)
                {
                    this._AveragePrices[quotation.SourceId] = sampleQueue.Average(q => q.Bid);
                }
                else
                {
                    this._AveragePrices[quotation.SourceId] = null;
                }
            }
        }

        public bool CanSwith(int fromSourceId, int toSourceId, out double agio)
        {
            Logger.AddEvent(TraceEventType.Information, "[SourceController] Call CanSwith fromSourceId:{0}, toSourceId:{1}", fromSourceId, toSourceId);
            agio = 0;
            if (this._AveragePrices[fromSourceId].HasValue && this._AveragePrices[toSourceId].HasValue)
            {
                lock (this._AveragePrices)
                {
                    agio = this._AveragePrices[toSourceId].Value - this._AveragePrices[fromSourceId].Value;
                }
                return true;
            }
            Logger.AddEvent(TraceEventType.Information, "[SourceController] Call CanSwith return false");
            return false;
        }
    }

    public class ActiveSource
    {
        private int _Id;
        private TimeSpan _TimeoutSpan;
        private DateTime _TimeoutTime;
        private bool _IsTimeout;
        private SourceInstrument _SourceInstrument;

        public ActiveSource(int id, TimeSpan timeoutSpan, SourceInstrument sourceInstrument)
        {
            this._Id = id;
            this._TimeoutSpan = timeoutSpan;
            this._TimeoutTime = DateTime.Now.Add(this._TimeoutSpan);
            this._IsTimeout = false;
            this._SourceInstrument = sourceInstrument;
        }

        public int Id { get { return this._Id; } }
        public TimeSpan TimeoutSpan { get { return this._TimeoutSpan; } }
        public DateTime TimeoutTime { get { return this._TimeoutTime; } }
        public bool IsTimeout { get { return this._IsTimeout; } }

        public void QuotationArrived()
        {
            this.SetTimeout(false);
        }

        public void ChangeSource(InstrumentSourceRelation relation)
        {
            this._Id = relation.SourceId;
            this._TimeoutSpan = TimeSpan.FromSeconds(relation.SwitchTimeout);
            this.SetTimeout(true);
        }

        public void Timeout()
        {
            this._IsTimeout = true;
        }

        private void SetTimeout(bool forceCheckTimeoutTime)
        {
            bool prevStateIsTimeout = this._IsTimeout;
            this._IsTimeout = false;
            this._TimeoutTime = DateTime.Now + this._TimeoutSpan;
            if (prevStateIsTimeout || forceCheckTimeoutTime)
            {
                this._SourceInstrument.CheckTimeoutTime(this._TimeoutTime);
            }
            Logger.AddEvent(TraceEventType.Information, "[SourceController] ActiveSource QuotationArrived _IsTimeout set to false, sourceid:{0}", this.Id);
        }
    }

    /// <summary>
    /// Instrument with all source
    /// </summary>
    public class SourceInstrument
    {
        private Instrument _Instrument;
        private Timer _PriceInactiveCheckTimer;
        private DateTime _LastActiveTime;
        //private bool _IsPriceEnabled = true;
        private double _Agio = 0;
        private UpdateMetadataMessage _IsActiveUpdateMetadataMessage;

        // Map for: SourceId - InstrumentSourceRelation
        private Dictionary<int, InstrumentSourceRelation> _Relations = new Dictionary<int, InstrumentSourceRelation>();

        private ActiveSource _ActiveSource;

        private AgioCalculator _AgioCalculator;
        private SourceController _SourceController;


        // InstrumentId, Dictionary(SourceId - InstrumentSourceRelation)
        public SourceInstrument(int instrumentId, Dictionary<int, InstrumentSourceRelation> relations, SourceController sourceController)
        {
            this._SourceController = sourceController;
            this._Relations = relations;
            this.MakeActiveSource();

            this._Instrument = MainService.QuotationManager.ConfigMetadata.Instruments[instrumentId];
            if (this._Instrument.IsSwitchUseAgio.Value)
            {
                this._AgioCalculator = new AgioCalculator(this._Instrument.AgioSeconds.Value, this._Instrument.LeastTicks.Value, relations.Keys);
            }
            TimeSpan inactiveTimeSpan = TimeSpan.FromSeconds(this._Instrument.InactiveTime.Value);
            this._PriceInactiveCheckTimer = new Timer(this.CheckPriceInactiveTime, null, inactiveTimeSpan, inactiveTimeSpan);
            this._LastActiveTime = DateTime.Now;
        }

        public int InstrumentId { get { return this._Instrument.Id; } }
        public ActiveSource ActiveSource { get { return this._ActiveSource; } }

        private bool IsActive
        {
            get
            {
                return this._Instrument.IsActive;
            }
            set
            {
                this._Instrument.IsActive = value;
                this.NotifyClientsIsActiveChanged();
            }
        }

        private void MakeActiveSource()
        {
            InstrumentSourceRelation relation = this._Relations.Values.SingleOrDefault(s => s.IsActive);
            if (relation == null)
            {
                relation = this._Relations.Values.SingleOrDefault(r => r.IsDefault == true);
                if (relation == null)
                {
                    int maxPriority = this._Relations.Values.Max(r => r.Priority);
                    relation = this._Relations.Values.Single(r => r.Priority == maxPriority);
                }
                Dictionary<string, object> fieldsAndValues = new Dictionary<string, object>();
                fieldsAndValues.Add(FieldSR.IsActive, true);
                QuotationData.UpdateMetadataObject(MetadataType.InstrumentSourceRelation, relation.Id, fieldsAndValues);
            }
            this._ActiveSource = new ActiveSource(relation.SourceId, TimeSpan.FromSeconds(relation.SwitchTimeout), this);
        }

        public void AddInstrumentSourceRelation(InstrumentSourceRelation relation)
        {
            this._Relations.Add(relation.SourceId, relation);
        }

        public bool RemoveInstrumentSourceRelation(int relationId, out int sourceCount)
        {
            sourceCount = -1;
            bool removed = false;
            InstrumentSourceRelation relation = this._Relations.Values.SingleOrDefault(r => r.Id == relationId);
            if (relation != null)
            {
                this._Relations.Remove(relation.SourceId);
                sourceCount = this._Relations.Count;
                removed = true;
            }
            return removed;
        }

        //public TimeSpan ActiveSourceTimeoutSpan
        //{
        //    get
        //    {
        //        return this._ActiveSource.TimeoutSpan;
        //    }
        //}

        //public DateTime ActiveSourceTimeoutTime
        //{
        //    get
        //    {
        //        return this._ActiveSource.TimeoutTime;
        //    }
        //}

        public bool QuotationArrived(SourceQuotation quotation)
        {
            if (this._Instrument.IsSwitchUseAgio.Value)
            {
                this._AgioCalculator.QuotationArrived(quotation);
            }

            if (quotation.SourceId == this._ActiveSource.Id)
            {
                this._ActiveSource.QuotationArrived();
            }
            else
            {
                if (this._Relations[quotation.SourceId].IsDefault || !this._Relations[this._ActiveSource.Id].IsDefault && this._Relations[quotation.SourceId].Priority > this._Relations[this._ActiveSource.Id].Priority)
                {
                    // 如果价格来自优先级高的源，则切换到该源；
                    this.SwitchActiveSource(quotation.SourceId);
                }
                else if (this._ActiveSource.IsTimeout)
                {
                    // 如果当前源Timeout，来一个价格就向该源切换；
                    this.SwitchActiveSource(quotation.SourceId);
                }
            }

            if (quotation.SourceId == this._ActiveSource.Id)
            {
                // calculate price and adjust
                if (this._Instrument.UseWeightedPrice.Value)
                {
                    WeightedPriceRule rule = MainService.QuotationManager.ConfigMetadata.WeightedPriceRules[this._Instrument.Id];
                    quotation.Ask = (quotation.Ask * rule.AskAskWeight + quotation.Bid * rule.BidAskWeight + (quotation.Last.HasValue ? quotation.Last.Value * rule.AskLastWeight : 0) + (quotation.Ask + quotation.Bid) / 2 * rule.AskAverageWeight + (double)rule.AskAdjust) * (double)rule.Multiplier;
                    quotation.Bid = (quotation.Ask * rule.BidAskWeight + quotation.Bid * rule.BidBidWeight + (quotation.Last.HasValue ? quotation.Last.Value * rule.BidLastWeight : 0) + (quotation.Ask + quotation.Bid) / 2 * rule.BidAverageWeight + (double)rule.BidAdjust) * (double)rule.Multiplier;
                }
                quotation.Ask += this._Agio + CommonHelper.GetAdjustValue(this._Relations[quotation.SourceId].AdjustPoints, this._Instrument.DecimalPlace);
                quotation.Bid += this._Agio + CommonHelper.GetAdjustValue(this._Relations[quotation.SourceId].AdjustPoints, this._Instrument.DecimalPlace);

                this._LastActiveTime = DateTime.Now;
                if (!this.IsActive)
                {
                    MainService.ExchangeManager.SwitchPriceEnableState(this._Instrument.Id, true);
                    this.IsActive = true;
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public void ActiveSourceTimeout()
        {
            if(this.InstrumentId == 1) Logger.AddEvent(TraceEventType.Information, "[SourceController] SourceInstrument.ActiveSourceTimeout InstrumentId:{0}, Source:{1} Timeout.", this.InstrumentId, this._ActiveSource.Id);
            this._ActiveSource.Timeout();
        }

        public void CheckTimeoutTime(DateTime timeoutTime)
        {
            this._SourceController.CheckActiveSourceTime(timeoutTime);
        }

        public void Stop()
        {
            this._PriceInactiveCheckTimer.Change(Timeout.Infinite, Timeout.Infinite);
            this._PriceInactiveCheckTimer.Dispose();
        }

        private void NotifyClientsIsActiveChanged()
        {
            if (this._IsActiveUpdateMetadataMessage == null)
            {
                this._IsActiveUpdateMetadataMessage = new UpdateMetadataMessage { UpdateDatas = new UpdateData[1] };
                this._IsActiveUpdateMetadataMessage.UpdateDatas[0] = new UpdateData
                {
                    FieldsAndValues = new Dictionary<string, object>(),
                    MetadataType = MetadataType.Instrument,
                    ObjectId = this._Instrument.Id
                };
            }
            this._IsActiveUpdateMetadataMessage.UpdateDatas[0].FieldsAndValues[FieldSR.IsActive] = this.IsActive;
            MainService.ClientManager.Dispatch(this._IsActiveUpdateMetadataMessage);
        }

        private void CheckPriceInactiveTime(object state)
        {
            try
            {
                this._PriceInactiveCheckTimer.Change(Timeout.Infinite, Timeout.Infinite);
                TimeSpan inactiveTimeSpan = TimeSpan.FromSeconds(this._Instrument.InactiveTime.Value);

                if (DateTime.Now - this._LastActiveTime > inactiveTimeSpan)
                {
                    if (this.IsActive)
                    {
                        try
                        {
                            MainService.ExchangeManager.SwitchPriceEnableState(this._Instrument.Id, false);
                        }
                        catch (Exception exception)
                        {
                            Logger.AddEvent(TraceEventType.Warning, "[SourceController] SourceInstrument.CheckInactiveTime try SwitchPriceEnableState\r\n{0}", exception);
                        }
                        this.IsActive = false;
                    }
                }
                this._PriceInactiveCheckTimer.Change(inactiveTimeSpan, inactiveTimeSpan);
            }
            catch (Exception exception)
            {
                Logger.TraceEvent(TraceEventType.Error, "[SourceController] SourceInstrument.CheckInactiveTime exception\r\n{0}", exception);
            }
        }

        private void SwitchActiveSource(int newSourceId)
        {
            double agio = 0;
            int oldSourceId = this._ActiveSource.Id;
            if (!this._Instrument.IsSwitchUseAgio.Value || this._AgioCalculator.CanSwith(oldSourceId, newSourceId, out agio))
            {
                this._Relations[oldSourceId].IsActive = false;
                this._Relations[newSourceId].IsActive = true;
                this._ActiveSource.ChangeSource(this._Relations[newSourceId]);
                this._Agio = agio;

                // Update database and notify Console
                int newRelationId = this._Relations[newSourceId].Id;
                int oldRelationId = this._Relations[oldSourceId].Id;
                DataAccess.QuotationData.SwitchActiveSource(newRelationId, oldRelationId);
                SwitchRelationBooleanPropertyMessage switchActiveSourceMessage = new SwitchRelationBooleanPropertyMessage()
                {
                    InstrumentId = this._Instrument.Id,
                    PropertyName = FieldSR.IsActive,
                    OldRelationId = oldRelationId,
                    NewRelationId = newRelationId
                };
                MainService.ClientManager.Dispatch(switchActiveSourceMessage);
                Logger.AddEvent(TraceEventType.Information, "SwitchActiveSource switched from {0} to {1}", oldRelationId, newSourceId);

                // write log
                WriteLogManager.WriteSourceChangeLog(LogManager.Instance.GetLogSourceChangeEntity(oldSourceId,newSourceId));
            }
        }
    }

    public class SourceController
    {
        private static TimeSpan Infinite = TimeSpan.FromMilliseconds(-1);

        // Map for: InstrumentId - SourceInstrument
        private Dictionary<int, SourceInstrument> _SourceInstruments;

        private Timer _ActiveSourceTimeoutCheckTimer;
        private DateTime _CheckTimerEndTime;

        public SourceController()
        {
            this._SourceInstruments = new Dictionary<int, SourceInstrument>();
            this._CheckTimerEndTime = DateTime.Now;
        }

        public void Start()
        {
            /* Convert: From [SourceId - (SourceSymbol - InstrumentSourceRelation)] To [InstrumentId - (SourceId - InstrumentSourceRelation)] */

            // Map for: InstrumentId - (SourceId - InstrumentSourceRelation)
            Dictionary<int, Dictionary<int, InstrumentSourceRelation>> instrumentSourceRelations = new Dictionary<int, Dictionary<int, InstrumentSourceRelation>>();

            foreach (Dictionary<string, InstrumentSourceRelation> symbolRelation in MainService.QuotationManager.ConfigMetadata.InstrumentSourceRelations.Values)
            {
                foreach (InstrumentSourceRelation relation in symbolRelation.Values)
                {
                    // Map for SourceId - InstrumentSourceRelation
                    Dictionary<int, InstrumentSourceRelation> sourceIdRelation;
                    if (!instrumentSourceRelations.TryGetValue(relation.InstrumentId, out sourceIdRelation))
                    {
                        sourceIdRelation = new Dictionary<int, InstrumentSourceRelation>();
                        instrumentSourceRelations.Add(relation.InstrumentId, sourceIdRelation);
                    }
                    sourceIdRelation.Add(relation.SourceId, relation);
                }
            }
            /* End Convert. */


            foreach (var pair in instrumentSourceRelations)
            {
                this._SourceInstruments.Add(pair.Key, new SourceInstrument(pair.Key, pair.Value, this));
            }

            if (this._SourceInstruments.Count > 0)
            {
                TimeSpan minTimeSpan = this._SourceInstruments.Values.Select(s => s.ActiveSource.TimeoutSpan).Min();
                this._ActiveSourceTimeoutCheckTimer = new Timer(this.ActiveSourceTimeoutCheck, null, minTimeSpan, SourceController.Infinite);
                Logger.AddEvent(TraceEventType.Information, "[SourceController] SourceController.Start minTimeSpan:{0}", minTimeSpan);
            }
        }

        public void Stop()
        {
            this._ActiveSourceTimeoutCheckTimer.Change(Timeout.Infinite, Timeout.Infinite);
            this._ActiveSourceTimeoutCheckTimer.Dispose();
            foreach (SourceInstrument sourceInstrument in this._SourceInstruments.Values)
            {
                sourceInstrument.Stop();
            }
        }

        public void OnAddInstrumentSourceRelation(InstrumentSourceRelation relation)
        {
            lock (this._SourceInstruments)
            {
                if (this._SourceInstruments.ContainsKey(relation.InstrumentId))
                {
                    this._SourceInstruments[relation.InstrumentId].AddInstrumentSourceRelation(relation);
                }
                else
                {
                    // Map for SourceId - InstrumentSourceRelation
                    Dictionary<int, InstrumentSourceRelation> relations = new Dictionary<int, InstrumentSourceRelation>();
                    relations.Add(relation.SourceId, relation);
                    SourceInstrument sourceInstrument = new SourceInstrument(relation.InstrumentId, relations, this);
                    this._SourceInstruments.Add(relation.InstrumentId, sourceInstrument);
                }
            }
        }

        public void OnRemoveInstrumentSourceRelation(int relationId)
        {
            lock (this._SourceInstruments)
            {
                foreach (SourceInstrument sourceInstrument in this._SourceInstruments.Values)
                {
                    int sourceCount;
                    if (sourceInstrument.RemoveInstrumentSourceRelation(relationId, out sourceCount))
                    {
                        if(sourceCount == 0)
                        {
                            sourceInstrument.Stop();
                            this._SourceInstruments.Remove(sourceInstrument.InstrumentId);
                        }
                        break;
                    }
                }
            }
        }

        public void OnAddInstrument(Instrument instrument)
        {
            // do nothing, SourceInstrument will be added while add first InstrumentSourceRelation.
            //lock(this._SourceInstruments)
            //{
            //    // while add instrument, there is not InstrumentSourceRelation yet.
            //    SourceInstrument sourceInstrument = new SourceInstrument(instrument.Id, new Dictionary<int,InstrumentSourceRelation>());
            //    this._SourceInstruments.Add(instrument.Id, sourceInstrument);
            //}
        }

        public void OnRemoveInstrument(int instrumentId)
        {
            if (this._SourceInstruments.ContainsKey(instrumentId))
            {
                this._SourceInstruments[instrumentId].Stop();
                lock (this._SourceInstruments)
                {
                    this._SourceInstruments.Remove(instrumentId);
                }
            }
        }

        public bool QuotationArrived(SourceQuotation quotation)
        {
            // do not lock _SourceInstruments here for performance, so we should judge if the instrument existed in _SourceInstruments. because of maybe in adding Instrument progress.
            if (this._SourceInstruments.ContainsKey(quotation.InstrumentId))
            {
                return this._SourceInstruments[quotation.InstrumentId].QuotationArrived(quotation);
            }
            else
            {
                return false;
            }
        }

        public void CheckActiveSourceTime(DateTime timeoutTime)
        {

            if (this._CheckTimerEndTime < timeoutTime)
            {
                TimeSpan timeSpan = timeoutTime - DateTime.Now;
                if(timeSpan < TimeSpan.Zero) timeSpan = TimeSpan.Zero;
                this._ActiveSourceTimeoutCheckTimer.Change(timeSpan, SourceController.Infinite); 
            }
        }

        private void ActiveSourceTimeoutCheck(object state)
        {
            try
            {
                lock (this._SourceInstruments)
                {
                    IEnumerable<SourceInstrument> activeSourceInstruments = this._SourceInstruments.Values.Where(si => !si.ActiveSource.IsTimeout);
                    if (activeSourceInstruments.Any())
                    {
                        DateTime now = DateTime.Now;
                        var timeoutSourceInstruments = activeSourceInstruments.Where(s => s.ActiveSource.TimeoutTime <= now);
                        foreach (SourceInstrument sourceInstrument in timeoutSourceInstruments)
                        {
                            sourceInstrument.ActiveSourceTimeout();
                        }
                        if (activeSourceInstruments.Any())
                        {
                            this._CheckTimerEndTime = activeSourceInstruments.Select(s => s.ActiveSource.TimeoutTime).Min();
                            TimeSpan timeSpen = this._CheckTimerEndTime - now;

                            this._ActiveSourceTimeoutCheckTimer.Change(timeSpen, SourceController.Infinite);
                            Logger.AddEvent(TraceEventType.Information, "[SourceController] SourceController.TimerCallback next call TimeSpan:{0}", timeSpen);
                        }
                    }
                    else
                    {
                        Logger.AddEvent(TraceEventType.Information, "[SourceController] SourceController.TimerCallback all source timedout.");
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.TraceEvent(TraceEventType.Error, "[SourceController] SourceController.TimerCallback exception\r\n{0}", exception);
            }
        }
    }
}
