using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manager.Common.QuotationEntities
{
    public class SourceInstrumentKey
    {
        public SourceInstrumentKey(int sourceId, int instrumentId)
        {
            this.SourceId = sourceId;
            this.InstrumentId = instrumentId;
        }

        public int SourceId { get; private set; }
        public int InstrumentId { get; private set; }
        public override bool Equals(object obj)
        {
            SourceInstrumentKey other = obj as SourceInstrumentKey;
            if (other == null) return false;
            return this.SourceId == other.SourceId && this.InstrumentId == other.InstrumentId;
        }
        public override int GetHashCode()
        {
            return string.Format("{0}|{1}", this.SourceId, this.InstrumentId).GetHashCode();
        }
    }
}
