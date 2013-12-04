using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using Manager.Common;
using Manager.Common.QuotationEntities;
using ManagerConsole.Model;

namespace ManagerConsole.ViewModel
{
    public class VmQuotationManager
    {
        public static VmQuotationManager Instance = new VmQuotationManager();

        private bool _MetadataNotLoaded = true;
        private ObservableCollection<VmQuotationSource> _QuotationSources = new ObservableCollection<VmQuotationSource>();
        private ObservableCollection<VmInstrument> _Instruments = new ObservableCollection<VmInstrument>();
        private ObservableCollection<VmAbnormalQuotation> _AbnormalQuotations = new ObservableCollection<VmAbnormalQuotation>();

        private Timer _Timer;
        private bool _TimerStarted = false;

        private VmQuotationManager()
        {
            this._Timer = new Timer(this.CheckAbnormalQuotations, null, Timeout.Infinite, Timeout.Infinite);
        }

        public ObservableCollection<VmQuotationSource> QuotationSources
        {
            get
            {
                if (this._MetadataNotLoaded)
                {
                    this.LoadMetadata();
                }
                return this._QuotationSources;
            }
        }

        public ObservableCollection<VmInstrument> Instruments
        {
            get
            {
                if (this._MetadataNotLoaded)
                {
                    this.LoadMetadata();
                }
                return this._Instruments;
            }
        }

        public ObservableCollection<VmAbnormalQuotation> AbnormalQuotations { get { return this._AbnormalQuotations; } }

        public void AddAbnormalQuotation(AbnormalQuotationMessage message)
        {
            VmAbnormalQuotation abnormalQuotation = new VmAbnormalQuotation(message);
            lock(this._Timer)
            {
                this._AbnormalQuotations.Add(abnormalQuotation);
                if (!this._TimerStarted)
                {
                    this._Timer.Change(0, 1000);
                    this._TimerStarted = true;
                }
            }
        }

        private void CheckAbnormalQuotations(object state)
        {
            List<VmAbnormalQuotation> timeoutItems = new List<VmAbnormalQuotation>();
            lock (this._Timer)
            {
                foreach (VmAbnormalQuotation quotation in this._AbnormalQuotations)
                {
                    if (--quotation.RemainingSeconds == 0)
                    {
                        timeoutItems.Add(quotation);
                    }
                }
                App.MainWindow.Dispatcher.BeginInvoke((Action<List<VmAbnormalQuotation>>)delegate(List<VmAbnormalQuotation> items)
                {
                    foreach (VmAbnormalQuotation item in items)
                    {
                        this._AbnormalQuotations.Remove(item);
                    }
                    if (this._AbnormalQuotations.Count == 0)
                    {
                        this._Timer.Change(Timeout.Infinite, Timeout.Infinite);
                        this._TimerStarted = false;
                    }
                }, timeoutItems);
            }
        }

        private void LoadMetadata()
        {
            if (this._MetadataNotLoaded)
            {
                ConsoleClient.Instance.GetConfigMetadata(delegate(ConfigMetadata metadata)
                {
                    this._QuotationSources.Clear();
                    foreach (var source in metadata.QuotationSources.Values)
                    {
                        this._QuotationSources.Add(new VmQuotationSource(source));
                    }

                    this._Instruments.Clear();
                    foreach (var instrument in metadata.Instruments.Values)
                    {
                        VmInstrument vmInstrument = new VmInstrument(instrument);
                        if (instrument.IsDerivative)
                        {
                            vmInstrument.DerivativeRelation = new VmDerivativeRelation(metadata.DerivativeRelations[instrument.Id]);
                        }
                        else
                        {
                            foreach (Dictionary<string, InstrumentSourceRelation> dict in metadata.InstrumentSourceRelations.Values)
                            {
                                var relation = dict.Values.SingleOrDefault(r => r.InstrumentId == instrument.Id);
                                if (relation != null)
                                {
                                    VmQuotationSource vmQuotationSource = this._QuotationSources.Single(s => s.Id == relation.SourceId);
                                    vmInstrument.SourceRelations.Add(new VmInstrumentSourceRelation(relation, vmInstrument, vmQuotationSource));
                                }
                            }

                            if(!instrument.IsDerivative)
                            {
                                vmInstrument.VmPriceRangeCheckRule = new VmPriceRangeCheckRule(metadata.PriceRangeCheckRules[instrument.Id]);
                                if(metadata.WeightedPriceRules.ContainsKey(instrument.Id))
                                {
                                    vmInstrument.VmWeightedPriceRule = new VmWeightedPriceRule(metadata.WeightedPriceRules[instrument.Id]);
                                }
                                else
                                {
                                    vmInstrument.VmWeightedPriceRule = new VmWeightedPriceRule(new WeightedPriceRule());
                                }
                            }
                        }
                        this._Instruments.Add(vmInstrument);
                    }
                    this._MetadataNotLoaded = false;
                });
            }
        }

        public void Add(QuotationSource source)
        {
            this._QuotationSources.Add(new VmQuotationSource(source));
        }

        public void Add(Instrument instrument)
        {
            this._Instruments.Add(new VmInstrument(instrument));
        }

        public void Add(InstrumentSourceRelation relation)
        {
            VmInstrument vmInstrument = this._Instruments.Single(i => i.Id == relation.InstrumentId);
            VmQuotationSource vmQuotationSource = this._QuotationSources.Single(s => s.Id == relation.SourceId);
            vmInstrument.SourceRelations.Add(new VmInstrumentSourceRelation(relation, vmInstrument, vmQuotationSource));
        }

        public void Add(PriceRangeCheckRule rule)
        {
            this._Instruments.Single(i => i.Id == rule.Id).VmPriceRangeCheckRule = new VmPriceRangeCheckRule(rule);
        }

        public void Add(WeightedPriceRule rule)
        {
            this._Instruments.Single(i => i.Id == rule.Id).VmWeightedPriceRule = new VmWeightedPriceRule(rule);
        }

        public void Update(Manager.Common.UpdateMetadataMessage message)
        {
            VmInstrument instrument;
            switch (message.MetadataType)
            {
                case MetadataType.QuotationSource:
                    VmQuotationSource source = this._QuotationSources.SingleOrDefault(s => s.Id == message.ObjectId);
                    if (source != null)
                    {
                        source.Update(message.FieldAndValues);
                    }
                    break;
                case MetadataType.Instrument:
                    instrument = this._Instruments.SingleOrDefault(i => i.Id == message.ObjectId);
                    if (instrument != null)
                    {
                        instrument.Update(message.FieldAndValues);
                    }
                    break;
                case MetadataType.InstrumentSourceRelation:
                    VmInstrumentSourceRelation relation;
                    if (this.FindVmInstrumentSourceRelation(message.ObjectId, out relation, out instrument))
                    {
                        relation.Update(message.FieldAndValues);
                    }
                    break;
                case MetadataType.DerivativeRelation:
                    instrument = this._Instruments.SingleOrDefault(i => i.Id == message.ObjectId);
                    if (instrument != null)
                    {
                        instrument.DerivativeRelation.Update(message.FieldAndValues);
                    }
                    break;
                case MetadataType.PriceRangeCheckRule:
                    instrument = this._Instruments.SingleOrDefault(i => i.Id == message.ObjectId);
                    if (instrument != null)
                    {
                        instrument.VmPriceRangeCheckRule.Update(message.FieldAndValues);
                    }
                    break;
                case MetadataType.WeightedPriceRule:
                    instrument = this._Instruments.SingleOrDefault(i => i.Id == message.ObjectId);
                    if (instrument != null)
                    {
                        instrument.VmWeightedPriceRule.Update(message.FieldAndValues);
                    }
                    break;
                default:
                    break;
            }
        }

        public void Delete(DeleteMetadataObjectMessage message)
        {
            VmInstrument instrument;
            switch (message.MetadataType)
            {
                case MetadataType.QuotationSource:
                    this.RemoveQuotationSource(message.ObjectId);
                    break;
                case MetadataType.Instrument:
                    instrument = this._Instruments.SingleOrDefault(i => i.Id == message.ObjectId);
                    if (instrument != null)
                    {
                        this._Instruments.Remove(instrument);
                    }
                    break;
                case MetadataType.InstrumentSourceRelation:
                    VmInstrumentSourceRelation relation;
                    if(this.FindVmInstrumentSourceRelation(message.ObjectId, out relation, out instrument))
                    {
                        instrument.SourceRelations.Remove(relation);
                    }
                    break;
                case MetadataType.DerivativeRelation:
                    // TODO: Check if this will happen. 
                    break;
                case MetadataType.PriceRangeCheckRule:
                    // TODO: Check if this will happen.
                    break;
                case MetadataType.WeightedPriceRule:
                    // TODO: Check if this will happen.
                    break;
                default:
                    break;
            }
        }

        public void RemoveQuotationSource(int id)
        {
            VmQuotationSource source = this._QuotationSources.SingleOrDefault(s => s.Id == id);
            if (source != null)
            {
                this._QuotationSources.Remove(source);
            }
        }

        public void SetPrimitiveQuotation(PrimitiveQuotation quotation)
        {
            VmInstrument vmInstrument  = this._Instruments.SingleOrDefault(i => i.Id == quotation.InstrumentId);
            if (vmInstrument != null)
            {
                VmInstrumentSourceRelation relation = vmInstrument.SourceRelations.SingleOrDefault(r => r.SourceId == quotation.SourceId);
                if (relation != null)
                {
                    string InstrumentCode = this._Instruments.Single(i => i.Id == quotation.InstrumentId).Code;   // TODO: performance can be improved by create a id=>instrumnet map.
                    relation.SetSourceQuotation(new VmSourceQuotation(quotation, InstrumentCode));
                }
            }
        }

        internal void SwitchActiveSource(SwitchRelationBooleanPropertyMessage message)
        {
            VmInstrument vmInstrument = this._Instruments.Single(i => i.Id == message.InstrumentId);
            vmInstrument.SourceRelations.Single(r => r.Id == message.NewRelationId).Update(message.PropertyName, true);
            vmInstrument.SourceRelations.Single(r => r.Id == message.OldRelationId).Update(message.PropertyName, false);
        }

        private bool FindVmInstrumentSourceRelation(int relationId, out VmInstrumentSourceRelation relation, out VmInstrument instrument)
        {
            relation = null;
            instrument = null;
            foreach (VmInstrument vmInstrument in this._Instruments)
            {
                relation = vmInstrument.SourceRelations.SingleOrDefault(r => r.Id == relationId);
                if (relation != null)
                {
                    instrument = vmInstrument;
                    break;
                }
            }
            return relation != null;
        }
    }
}
