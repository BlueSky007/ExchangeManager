﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Manager.Common.QuotationEntities;
using Manager.Common;
using ManagerService.DataAccess;

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
            agio = 0;
            if (this._AveragePrices[fromSourceId].HasValue && this._AveragePrices[toSourceId].HasValue)
            {
                lock (this._AveragePrices)
                {
                    agio = this._AveragePrices[toSourceId].Value - this._AveragePrices[fromSourceId].Value;
                }
                return true;
            }
            return false;
        }
    }

    public class ActiveSource
    {
        private int _Id;
        private TimeSpan _TimeoutSpan;
        private DateTime _TimeoutTime;
        private bool _IsTimeout;

        public ActiveSource(int id, TimeSpan timeoutSpan)
        {
            this._Id = id;
            this._TimeoutSpan = timeoutSpan;
            this._TimeoutTime = DateTime.Now.Add(this._TimeoutSpan).AddMinutes(10);  // 系统启动时，未收到价格前的TimeoutTime. 暂时多加10分钟。
            this._IsTimeout = false;
        }

        public int Id { get { return this._Id; } }
        public TimeSpan TimeoutSpan { get { return this._TimeoutSpan; } }
        public DateTime TimeoutTime { get { return this._TimeoutTime; } }
        public bool IsTimeout { get { return this._IsTimeout; } }

        public void QuotationArrived()
        {
            this._IsTimeout = false;
            this._TimeoutTime = DateTime.Now + this._TimeoutSpan;
        }

        public void ChangeSource(InstrumentSourceRelation relation)
        {
            this._Id = relation.SourceId;
            this._TimeoutSpan = TimeSpan.FromSeconds(relation.SwitchTimeout);
            this.QuotationArrived();
        }

        public void Timeout()
        {
            this._IsTimeout = true;
            this._TimeoutTime = DateTime.Now.AddDays(10);  // Add 10 days to avoid be selected by SourceController Timer.
        }
    }

    /// <summary>
    /// Instrument with all source
    /// </summary>
    public class SourceInstrument
    {
        private Instrument _Instrument;
        private Timer _Timer;
        private DateTime _LastActiveTime;
        private bool _IsPriceEnabled = true;

        private double _Agio = 0;

        // Map for: SourceId - InstrumentSourceRelation
        private Dictionary<int, InstrumentSourceRelation> _Relations = new Dictionary<int, InstrumentSourceRelation>();

        private ActiveSource _ActiveSource;

        private AgioCalculator _AgioCalculator;

        // InstrumentId, Dictionary(SourceId - InstrumentSourceRelation)
        public SourceInstrument(int instrumentId, Dictionary<int, InstrumentSourceRelation> relations)
        {
            this._Relations = relations;

            int activeSourceId;
            InstrumentSourceRelation relation = relations.Values.SingleOrDefault(s => s.IsActive);
            if (relation == null)
            {
                relation = relations.Values.SingleOrDefault(r => r.IsDefault == true);
                if(relation == null)
                {
                    int maxPriority = relations.Values.Max(r => r.Priority);
                    relation = relations.Values.Single(r => r.Priority == maxPriority);
                }
                Dictionary<string, object> fieldsAndValues = new Dictionary<string,object>();
                fieldsAndValues.Add(FieldSR.IsActive, true);
                QuotationData.UpdateMetadataObject(MetadataType.InstrumentSourceRelation, relation.Id, fieldsAndValues);
            }
            activeSourceId = relation.SourceId;
            TimeSpan timeoutTimeSpan = TimeSpan.FromSeconds(this._Relations[activeSourceId].SwitchTimeout);
            this._ActiveSource = new ActiveSource(activeSourceId, timeoutTimeSpan);

            this._Instrument = MainService.QuotationManager.ConfigMetadata.Instruments[instrumentId];
            if (this._Instrument.IsSwitchUseAgio.Value)
            {
                this._AgioCalculator = new AgioCalculator(this._Instrument.AgioSeconds.Value, this._Instrument.LeastTicks.Value, relations.Keys);
            }
            TimeSpan inactiveTimeSpan = TimeSpan.FromSeconds(this._Instrument.InactiveTime.Value);
            this._Timer = new Timer(this.CheckInactiveTime, null, inactiveTimeSpan, inactiveTimeSpan);
            this._LastActiveTime = DateTime.Now;
        }

        public void AddInstrumentSourceRelation(InstrumentSourceRelation relation)
        {
            this._Relations.Add(relation.SourceId, relation);
        }

        public void RemoveInstrumentSourceRelation(int relationId)
        {
            InstrumentSourceRelation relation = this._Relations.Values.SingleOrDefault(r => r.Id == relationId);
            if (relation != null)
            {
                this._Relations.Remove(relation.SourceId);
            }
        }

        public TimeSpan ActiveSourceTimeoutSpan
        {
            get
            {
                return this._ActiveSource.TimeoutSpan;
            }
        }

        public DateTime ActiveSourceTimeoutTime
        {
            get
            {
                return this._ActiveSource.TimeoutTime;
            }
        }

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
                quotation.Ask += this._Agio + this._Relations[quotation.InstrumentId].AdjustPoints;
                quotation.Bid += this._Agio + this._Relations[quotation.InstrumentId].AdjustPoints;

                this._LastActiveTime = DateTime.Now;
                if (!this._IsPriceEnabled)
                {
                    MainService.ExchangeManager.SwitchPriceEnableState(this._Instrument.Id, true);
                    this._IsPriceEnabled = true;
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
            this._ActiveSource.Timeout();
        }

        private void CheckInactiveTime(object state)
        {
            this._Timer.Change(Timeout.Infinite, Timeout.Infinite);
            TimeSpan inactiveTimeSpan = TimeSpan.FromSeconds(this._Instrument.InactiveTime.Value);

            if(DateTime.Now - this._LastActiveTime > inactiveTimeSpan)
            {
                if (this._IsPriceEnabled)
                {
                    MainService.ExchangeManager.SwitchPriceEnableState(this._Instrument.Id, false);
                    this._IsPriceEnabled = false;
                }
            }

            this._Timer.Change(inactiveTimeSpan, inactiveTimeSpan);
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
                DataAccess.QuotationData.SwitchActiveSource(oldRelationId, newRelationId);
                SwitchRelationBooleanPropertyMessage switchActiveSourceMessage = new SwitchRelationBooleanPropertyMessage()
                {
                    InstrumentId = this._Instrument.Id,
                    PropertyName = FieldSR.IsActive,
                    OldRelationId = oldRelationId,
                    NewRelationId = newRelationId
                };
                MainService.ClientManager.Dispatch(switchActiveSourceMessage);
            }
        }
    }

    public class SourceController
    {
        private static TimeSpan Infinite = TimeSpan.FromMilliseconds(-1);

        // Map for: InstrumentId - SourceInstrument
        private Dictionary<int, SourceInstrument> _SourceInstruments = new Dictionary<int, SourceInstrument>();

        private Timer _Timer;

        public SourceController()
        {
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
                this._SourceInstruments.Add(pair.Key, new SourceInstrument(pair.Key, pair.Value));
            }

            if (this._SourceInstruments.Count > 0)
            {
                TimeSpan minTimeSpan = this._SourceInstruments.Values.Select(s => s.ActiveSourceTimeoutSpan).Min();
                this._Timer = new Timer(this.TimerCallback, null, minTimeSpan, SourceController.Infinite);
            }
        }

        public void OnAddInstrumentSourceRelation(InstrumentSourceRelation relation)
        {
            if(this._SourceInstruments.ContainsKey(relation.InstrumentId))
            {
                this._SourceInstruments[relation.InstrumentId].AddInstrumentSourceRelation(relation);
            }
            else
            {
                // SourceId - InstrumentSourceRelation
                Dictionary<int, InstrumentSourceRelation> relations = new Dictionary<int,InstrumentSourceRelation>();
                relations.Add(relation.SourceId, relation);
                SourceInstrument sourceInstrument = new SourceInstrument(relation.InstrumentId, relations);
                this._SourceInstruments.Add(relation.InstrumentId, sourceInstrument);
            }
        }

        public void OnRemoveInstrumentSourceRelation(int relationId)
        {
            foreach(SourceInstrument sourceInstrument in this._SourceInstruments.Values)
            {
                sourceInstrument.RemoveInstrumentSourceRelation(relationId);
            }
        }

        public bool QuotationArrived(SourceQuotation quotation)
        {
            return this._SourceInstruments[quotation.InstrumentId].QuotationArrived(quotation);
        }

        private void TimerCallback(object state)
        {
            DateTime now = DateTime.Now;
            var timeoutSourceInstruments = this._SourceInstruments.Values.Where(s => s.ActiveSourceTimeoutTime <= now);
            foreach (SourceInstrument sourceInstrument in timeoutSourceInstruments)
            {
                sourceInstrument.ActiveSourceTimeout();
            }
            DateTime minTimeoutTime = this._SourceInstruments.Values.Select(s => s.ActiveSourceTimeoutTime).Min();
            this._Timer.Change(minTimeoutTime - now, SourceController.Infinite);
        }
    }
}
