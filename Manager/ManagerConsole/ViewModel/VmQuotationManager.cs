using Manager.Common;
using Manager.Common.QuotationEntities;
using ManagerConsole.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace ManagerConsole.ViewModel
{
    public class VmQuotationManager
    {
        public static VmQuotationManager Instance = new VmQuotationManager();

        private bool _NotLoaded = true;
        private ObservableCollection<VmQuotationSource> _QuotationSources = new ObservableCollection<VmQuotationSource>();
        private ObservableCollection<VmInstrument> _Instruments = new ObservableCollection<VmInstrument>();

        private VmQuotationManager() { }

        public ObservableCollection<VmQuotationSource> QuotationSources
        {
            get
            {
                if (this._NotLoaded)
                {
                    this.Load();
                }
                return this._QuotationSources;
            }
        }

        public ObservableCollection<VmInstrument> Instruments
        {
            get
            {
                if (this._NotLoaded)
                {
                    this.Load();
                }
                return this._Instruments;
            }
        }

        private void Load()
        {
            if (this._NotLoaded)
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
                        //metadata.InstrumentSourceRelations.
                        VmInstrument instrumentViewModel = new VmInstrument(instrument);
                        foreach (Dictionary<string, InstrumentSourceRelation> dict in metadata.InstrumentSourceRelations.Values)
                        {
                            var relation = dict.Values.SingleOrDefault(r => r.InstrumentId == instrument.Id);
                            if (relation != null)
                            {
                                instrumentViewModel.SourceRelations.Add(new VmInstrumentSourceRelation(relation));
                            }
                        }
                        this._Instruments.Add(instrumentViewModel);
                    }
                });
            }
        }

        public void Update(Manager.Common.UpdateMetadataMessage message)
        {
            switch (message.MetadataType)
            {
                case MetadataType.QuotationSource:
                    VmQuotationSource source = this._QuotationSources.SingleOrDefault(s => s.Id == message.ObjectId);
                    if (source != null)
                    {
                        foreach (string key in message.FieldAndValues.Keys)
                        {
                            if (key == FieldSR.Name)
                            {
                                source.Name = (string)message.FieldAndValues[key];
                            }
                            else if (key == FieldSR.AuthName)
                            {
                                source.AuthName = (string)message.FieldAndValues[key];
                            }
                            else if (key == FieldSR.Password)
                            {
                                source.Password = (string)message.FieldAndValues[key];
                            }
                        }
                    }
                    break;
                case MetadataType.Instrument:
                    break;
                case MetadataType.InstrumentSourceRelation:
                    break;
                case MetadataType.DerivativeRelation:
                    break;
                case MetadataType.PriceRangeCheckRule:
                    break;
                case MetadataType.WeightedPriceRule:
                    break;
                default:
                    break;
            }
        }

        public void Add(VmQuotationSource source)
        {
            this._QuotationSources.Add(source);
        }

        public void Delete(DeleteMetadataObjectMessage message)
        {
            switch (message.MetadataType)
            {
                case MetadataType.QuotationSource:
                    this.RemoveQuotationSource(message.ObjectId);
                    break;
                case MetadataType.Instrument:
                    break;
                case MetadataType.InstrumentSourceRelation:
                    break;
                case MetadataType.DerivativeRelation:
                    break;
                case MetadataType.PriceRangeCheckRule:
                    break;
                case MetadataType.WeightedPriceRule:
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
                    relation.SetPrimitiveQuotation(quotation);
                }
            }
        }
    }
}
