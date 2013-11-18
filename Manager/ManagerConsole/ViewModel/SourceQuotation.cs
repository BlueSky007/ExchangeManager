using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.ComponentModel;
using ManagerConsole.Helper;
using CommonPrimitiveQuotation = Manager.Common.QuotationEntities.PrimitiveQuotation;

namespace ManagerConsole.ViewModel
{
    public class SourceQuotation
    {
        private ObservableCollection<PrimitiveQuotation> _Quotations = new ObservableCollection<PrimitiveQuotation>();

        public SourceQuotation(int sourceId, string sourceName)
        {
            this.Id = sourceId;
            this.Name = sourceName;
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public ObservableCollection<PrimitiveQuotation> Quotations { get { return this._Quotations; } }
    }

    public class PrimitiveQuotation
    {
        public string InstrumentCode { get; set; }
        public string Bid { get; set; }
        public string Ask { get; set; }
        public string Last { get; set; }
        public string High { get; set; }
        public string Low { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
