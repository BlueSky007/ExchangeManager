﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Manager.Common;
using Manager.Common.QuotationEntities;
using System.Diagnostics;
using ManagerInstrument = Manager.Common.QuotationEntities.Instrument;

namespace ManagerService.Quotation
{
    public class QuotationManager
    {
        private QuotationReceiver _QuotationReceiver;
        private ConfigMetadata _ConfigMetadata;
        private SourceController _SourceController;
        private LastQuotationManager _LastQuotationManager;
        private AbnormalQuotationManager _AbnormalQuotationManager;
        private DerivativeController _DerivativeController;
        private Dictionary<string, ConnectionState> _SourceConnectionStates = new Dictionary<string, ConnectionState>();

        public QuotationManager()
        {
            this._ConfigMetadata = new ConfigMetadata();
            this._SourceController = new SourceController();
            this._LastQuotationManager = new LastQuotationManager();
        }

        public void Start(int quotationListenPort)
        {
            this._ConfigMetadata.Load();
            this._AbnormalQuotationManager = new AbnormalQuotationManager(this._ConfigMetadata.PriceRangeCheckRules, this._LastQuotationManager);
            this._DerivativeController = new DerivativeController(this._ConfigMetadata.DerivativeRelations, this._LastQuotationManager);

            this._SourceController.Start();
            this._LastQuotationManager.LastAccepted.Initialize(this._ConfigMetadata.LastQuotations);
            this._QuotationReceiver = new QuotationReceiver(quotationListenPort);
            this._QuotationReceiver.Start(this);
        }

        public void Stop()
        {
            this._QuotationReceiver.Stop();
            this._SourceController.Stop();
            this._AbnormalQuotationManager.Stop();
        }

        public ConfigMetadata ConfigMetadata { get { return this._ConfigMetadata; } }

        public Dictionary<string, ConnectionState> SourceConnectionStates { get { return this._SourceConnectionStates; } }

        public AbnormalQuotationManager AbnormalQuotationManager { get { return this._AbnormalQuotationManager; } }

        public bool AuthenticateSource(string sourceName, string loginName, string password)
        {
            return this._ConfigMetadata.AuthenticateSource(sourceName, loginName, password);
        }

        public void NofitySourceConnectionStatus(string sourceName, ConnectionState state)
        {
            this._SourceConnectionStates[sourceName] = state;
            MainService.ClientManager.Dispatch(new SourceConnectionStatusMessage() { SouceName = sourceName, ConnectionState = state });
        }

        public void ProcessQuotation(PrimitiveQuotation primitiveQuotation)
        {
            bool inverted;
            if (this._ConfigMetadata.EnsureIsKnownQuotation(primitiveQuotation, out inverted))
            {
                double ask, bid;
                if (this._LastQuotationManager.LastReceived.Fix(primitiveQuotation, out ask, out bid))
                {
                    if (this._LastQuotationManager.LastReceived.IsNotSame(primitiveQuotation))
                    {
                        MainService.ClientManager.Dispatch(new PrimitiveQuotationMessage() { Quotation = primitiveQuotation });

                        string instrumentCode = this._ConfigMetadata.Instruments[primitiveQuotation.InstrumentId].Code;
                        SourceQuotation quotation = new SourceQuotation(primitiveQuotation, ask, bid, instrumentCode, inverted);
                        
                        if (this._SourceController.QuotationArrived(quotation))
                        {
                            // quotation come from Active Source and adjusted
                            if (this._AbnormalQuotationManager.SetQuotation(quotation))
                            {
                                // quotation is normal
                                this.ProcessNormalQuotation(quotation);
                            }
                        }
                        this._LastQuotationManager.LastReceived.Set(quotation);
                    }
                }
            }
            else
            {
                Logger.AddEvent(TraceEventType.Warning, "Discarded price:[ask={0}, bid={1}] got for {2} from {3}",
                    primitiveQuotation.Ask, primitiveQuotation.Bid, primitiveQuotation.Symbol, primitiveQuotation.SourceName);
            }
        }

        public void ProcessNormalQuotation(SourceQuotation quotation)
        {
            List<GeneralQuotation> quotations = new List<GeneralQuotation>();
            this._DerivativeController.Derive(quotation.Quotation, quotations);
            for (int i = 0; i < quotations.Count; i++)
            {
                quotations[i].OriginCode = this._ConfigMetadata.Instruments[quotations[i].InstrumentId].Code;
            }
            quotations.Add(quotation.Quotation);

            // adjust quotation according to adjuestment on instrument
            foreach (GeneralQuotation gq in quotations)
            {
                Instrument instrument = this._ConfigMetadata.Instruments[gq.InstrumentId];
                gq.Ask += CommonHelper.GetAdjustValue(instrument.AdjustPoints, instrument.DecimalPlace);
                gq.Bid += CommonHelper.GetAdjustValue(instrument.AdjustPoints, instrument.DecimalPlace);
            }
            this._LastQuotationManager.LastAccepted.Accept(quotations);

            MainService.ExchangeManager.AddQuotations(quotations);
            MainService.ClientManager.Dispatch(new QuotationsMessage() { Quotations = quotations });
        }

        public void AddMetadataObject(QuotationSource quotationSource)
        {
            this._ConfigMetadata.QuotationSources.Add(quotationSource.Name, quotationSource);
        }

        public void AddMetadataObject(ManagerInstrument instrument)
        {
            this._ConfigMetadata.Instruments.Add(instrument.Id, instrument);
            this._SourceController.OnAddInstrument(instrument);
        }

        public void AddMetadataObject(InstrumentSourceRelation relation)
        {
            Dictionary<string, InstrumentSourceRelation> relations;
            if (!this._ConfigMetadata.InstrumentSourceRelations.TryGetValue(relation.SourceId, out relations))
            {
                relations = new Dictionary<string, InstrumentSourceRelation>();
                this._ConfigMetadata.InstrumentSourceRelations.Add(relation.SourceId, relations);
            }
            relations.Add(relation.SourceSymbol, relation);
            this._SourceController.OnAddInstrumentSourceRelation(relation);
        }

        public void AddMetadataObject(DerivativeRelation derivativeRelation)
        {
            this._ConfigMetadata.DerivativeRelations.Add(derivativeRelation.Id, derivativeRelation);
        }

        public void AddMetadataObject(PriceRangeCheckRule priceRangeCheckRule)
        {
            this._ConfigMetadata.PriceRangeCheckRules.Add(priceRangeCheckRule.Id, priceRangeCheckRule);
        }

        public void AddMetadataObject(WeightedPriceRule weightedPriceRule)
        {
            this._ConfigMetadata.WeightedPriceRules.Add(weightedPriceRule.Id, weightedPriceRule);
        }

        internal void DeleteMetadataObject(MetadataType type, int objectId)
        {
            switch (type)
            {
                case MetadataType.QuotationSource:
                    QuotationSource source = this._ConfigMetadata.QuotationSources.Values.SingleOrDefault(s => s.Id == objectId);
                    if (source != null) this._ConfigMetadata.QuotationSources.Remove(source.Name);
                    if (this._SourceConnectionStates.ContainsKey(source.Name)) this._SourceConnectionStates.Remove(source.Name);
                    break;
                case MetadataType.Instrument:
                    if (this._ConfigMetadata.Instruments[objectId].IsDerivative)
                    {
                        this._ConfigMetadata.DerivativeRelations.Remove(objectId);
                    }
                    else
                    {
                        this._ConfigMetadata.PriceRangeCheckRules.Remove(objectId);
                        this._ConfigMetadata.WeightedPriceRules.Remove(objectId);
                        foreach(Dictionary<string, InstrumentSourceRelation> relations in this._ConfigMetadata.InstrumentSourceRelations.Values)
                        {
                            InstrumentSourceRelation relation = relations.Values.SingleOrDefault(r => r.InstrumentId == objectId);
                            if(relation != null)
                            {
                                relations.Remove(relation.SourceSymbol);
                            }
                        }
                        this._ConfigMetadata.Instruments.Remove(objectId);
                        this._SourceController.OnRemoveInstrument(objectId);
                    }
                    break;
                case MetadataType.InstrumentSourceRelation:
                    foreach(Dictionary<string, InstrumentSourceRelation> relations in this._ConfigMetadata.InstrumentSourceRelations.Values)
                    {
                        InstrumentSourceRelation relation = relations.Values.SingleOrDefault(r => r.Id == objectId);
                        if(relation != null)
                        {
                            relations.Remove(relation.SourceSymbol);
                            break;
                        }
                    }
                    this._SourceController.OnRemoveInstrumentSourceRelation(objectId);
                    break;
                case MetadataType.DerivativeRelation:
                    this._ConfigMetadata.DerivativeRelations.Remove(objectId);
                    break;
                case MetadataType.PriceRangeCheckRule:
                    this._ConfigMetadata.PriceRangeCheckRules.Remove(objectId);
                    break;
                case MetadataType.WeightedPriceRule:
                    this._ConfigMetadata.WeightedPriceRules.Remove(objectId);
                    break;
                default:
                    break;
            }
        }

        internal void UpdateMetadataObject(MetadataType type, int objectId, Dictionary<string, object> fieldAndValues)
        {
            switch (type)
            {
                case MetadataType.QuotationSource:
                    QuotationSource quotationSource = this._ConfigMetadata.QuotationSources.Values.Single(s => s.Id == objectId);
                    quotationSource.Update(fieldAndValues);
                    break;
                case MetadataType.Instrument:
                    ManagerInstrument instrument = this._ConfigMetadata.Instruments[objectId];
                    instrument.Update(fieldAndValues);
                    break;
                case MetadataType.InstrumentSourceRelation:
                    foreach (Dictionary<string, InstrumentSourceRelation> relations in this._ConfigMetadata.InstrumentSourceRelations.Values)
                    {
                        InstrumentSourceRelation relation = relations.Values.SingleOrDefault(r => r.Id == objectId);
                        if (relation != null)
                        {
                            if (fieldAndValues.ContainsKey(FieldSR.SourceSymbol))
                            {
                                string oldSourceSymbol = relation.SourceSymbol;
                                relation.Update(fieldAndValues);
                                relations.Remove(oldSourceSymbol);
                                relations.Add(relation.SourceSymbol, relation);
                            }
                            else
                            {
                                relation.Update(fieldAndValues);
                            }
                            break;
                        }
                    }    
                    break;
                case MetadataType.DerivativeRelation:
                    this._ConfigMetadata.DerivativeRelations[objectId].Update(fieldAndValues);
                    break;
                case MetadataType.PriceRangeCheckRule:
                    this._ConfigMetadata.PriceRangeCheckRules[objectId].Update(fieldAndValues);
                    break;
                case MetadataType.WeightedPriceRule:
                    this._ConfigMetadata.WeightedPriceRules[objectId].Update(fieldAndValues);
                    break;
                default:
                    break;
            }
        }

        internal void SendQuotation(int instrumentSourceRelationId, double ask, double bid, out PrimitiveQuotation primitiveQuotation)
        {
            InstrumentSourceRelation relation = null;
            foreach (Dictionary<string, InstrumentSourceRelation> relations in this._ConfigMetadata.InstrumentSourceRelations.Values)
            {
                relation = relations.Values.SingleOrDefault(r => r.Id == instrumentSourceRelationId);
                if (relation != null) break;
            }
            primitiveQuotation = new PrimitiveQuotation();
            primitiveQuotation.SourceId = relation.SourceId;
            primitiveQuotation.InstrumentId = relation.InstrumentId;
            primitiveQuotation.SourceName = this._ConfigMetadata.QuotationSources.Values.Single(s=>s.Id == relation.SourceId).Name;
            primitiveQuotation.Symbol = relation.SourceSymbol;
            primitiveQuotation.Ask = ask.ToString();
            primitiveQuotation.Bid = bid.ToString();
            primitiveQuotation.Timestamp = DateTime.Now;

            this.ProcessQuotation(primitiveQuotation);
        }
    }
}
