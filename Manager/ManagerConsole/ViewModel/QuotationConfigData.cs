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
                            Passowrd = source.Password
                        });
                    }
                });
            }
        }
    }
}
