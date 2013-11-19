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
    public class QuotationConfigData
    {
        public static QuotationConfigData Instance = new QuotationConfigData();

        private bool _NotLoaded = true;
        private ObservableCollection<QuotationSource> _QuotationSources = null;

        private QuotationConfigData() { }

        public ObservableCollection<QuotationSource> QuotationSources
        {
            get
            {
                if (this._NotLoaded)
                {
                    this.Load();
                    this._QuotationSources = new ObservableCollection<QuotationSource>();
                }
                return this._QuotationSources;
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
                        this._QuotationSources.Add(new QuotationSource
                        {
                            Id = source.Id,
                            Name = source.Name,
                            AuthName = source.AuthName,
                            Password = source.Password
                        });
                    }
                });
            }
        }

        internal void Update(Manager.Common.MetadataUpdateMessage message)
        {
            switch (message.MetadataType)
            {
                case MetadataType.QuotationSource:
                    if (message.UpdateAction == UpdateAction.Modify)
                    {
                        QuotationSource source = this._QuotationSources.SingleOrDefault(s => s.Id == message.ObjectId);
                        if (source != null)
                        {
                            foreach (string key in message.Fields.Keys)
                            {
                                if (key == FieldSR.Name)
                                {
                                    source.Name = message.Fields[key];
                                }
                                else if (key == FieldSR.AuthName)
                                {
                                    source.AuthName = message.Fields[key];
                                }
                                else if (key == FieldSR.Password)
                                {
                                    source.Password = message.Fields[key];
                                }
                            }
                        }
                    }
                    else if (message.UpdateAction == UpdateAction.Add)
                    {
                        QuotationSource source = new QuotationSource();
                        source.Id = message.ObjectId;
                        source.Name = message.Fields[FieldSR.Name];
                        source.AuthName = message.Fields[FieldSR.AuthName];
                        source.Password = message.Fields[FieldSR.Password];
                        this._QuotationSources.Add(source);
                    }
                    else if (message.UpdateAction == UpdateAction.Delete)
                    {
                        QuotationSource source = this._QuotationSources.SingleOrDefault(s => s.Id == message.ObjectId);
                        if (source != null)
                        {
                            this._QuotationSources.Remove(source);
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
    }
}
