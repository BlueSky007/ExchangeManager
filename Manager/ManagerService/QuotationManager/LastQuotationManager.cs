using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Manager.Common.QuotationEntities;
using ManagerService.DataAccess;
using System.Threading;

namespace ManagerService.Quotation
{
    public class LastReceivedQuotation
    {
        // Map for SourceId - [InstrumentId -  Quotation]
        private Dictionary<int, Dictionary<int, SourceQuotation>> _LastReceivedQuotations = new Dictionary<int, Dictionary<int, SourceQuotation>>();
        public bool IsNotSame(PrimitiveQuotation quotation)
        {
            PrimitiveQuotation last;
            if (this.TryGetLastQuotation(quotation.SourceId, quotation.InstrumentId, out last))
            {
                return quotation.Timestamp != last.Timestamp || quotation.Bid != last.Bid || quotation.Ask != last.Ask || quotation.Last != last.Last || quotation.High != last.High || quotation.Low != last.Low;
            }
            return true;
        }
        public bool TryGetLastQuotation(int sourceId, int instrumentId, out PrimitiveQuotation primitiveQuotation)
        {
            primitiveQuotation = null;
            if (this._LastReceivedQuotations.ContainsKey(sourceId))
            {
                if (this._LastReceivedQuotations[sourceId].ContainsKey(instrumentId))
                {
                    primitiveQuotation = this._LastReceivedQuotations[sourceId][instrumentId].PrimitiveQuotation;
                }
            }
            return primitiveQuotation != null;
        }
        public bool Fix(PrimitiveQuotation primitiveQuotation, out double ask, out double bid)
        {
            bool @fixed = false;
            bool fixAsk = !PrimitiveQuotation.TryGetPriceValue(primitiveQuotation.Ask, out ask);
            bool fixBid = !PrimitiveQuotation.TryGetPriceValue(primitiveQuotation.Bid, out bid);
            if (fixAsk || fixBid)
            {
                PrimitiveQuotation last;
                if (this.TryGetLastQuotation(primitiveQuotation.SourceId, primitiveQuotation.InstrumentId, out last))
                {
                    if (fixAsk) primitiveQuotation.Ask = last.Ask;
                    if (fixBid) primitiveQuotation.Bid = last.Bid;
                    @fixed = true;
                }
            }
            return !fixAsk && !fixBid || @fixed;
        }
        public void Set(SourceQuotation quotation)
        {
            Dictionary<int, SourceQuotation> sourceQuotations;
            if (!this._LastReceivedQuotations.TryGetValue(quotation.SourceId, out sourceQuotations))
            {
                sourceQuotations = new Dictionary<int, SourceQuotation>();
                this._LastReceivedQuotations.Add(quotation.SourceId, sourceQuotations);
            }
            sourceQuotations[quotation.InstrumentId] = quotation;
        }
    }

    public class LastAcceptedQuotation
    {
        // Map for InstrumentId -  GeneralQuotation
        private Dictionary<int, GeneralQuotation> _LastAcceptedQuotations;

        private ReaderWriterLockSlim _LastQuotationLock = new ReaderWriterLockSlim();

        public void Initialize(Dictionary<int, GeneralQuotation> lastQuotations)
        {
            this._LastAcceptedQuotations = new Dictionary<int, GeneralQuotation>();
            foreach (KeyValuePair<int, GeneralQuotation> pair in lastQuotations)
            {
                this._LastAcceptedQuotations.Add(pair.Key, (GeneralQuotation)pair.Value);
            }
        }
        public bool TryGetLastQuotation(int instrumentId, out GeneralQuotation quotation)
        {
            this._LastQuotationLock.EnterReadLock();
            try
            {
                quotation = null;
                if (this._LastAcceptedQuotations.ContainsKey(instrumentId))
                {
                    quotation = this._LastAcceptedQuotations[instrumentId];
                }
                return quotation != null;
            }
            finally
            {
                this._LastQuotationLock.ExitReadLock();
            }
        }
        public void Accept(GeneralQuotation quotation)
        {
            this._LastQuotationLock.EnterWriteLock();
            try
            {
                this._LastAcceptedQuotations[quotation.InstrumentId] = quotation;
                QuotationData.SaveLastQuotation(quotation);
            }
            finally
            {
                this._LastQuotationLock.ExitWriteLock();
            }
        }
    }

    public class LastQuotationManager
    {
        private LastReceivedQuotation _LastReceivedQuotation = new LastReceivedQuotation();
        private LastAcceptedQuotation _LastAcceptedQuotation = new LastAcceptedQuotation();

        public LastReceivedQuotation LastReceived { get { return this._LastReceivedQuotation; } }
        public LastAcceptedQuotation LastAccepted { get { return this._LastAcceptedQuotation; } }

        public void Update(SourceQuotation quotation, bool accepted)
        {
            this._LastReceivedQuotation.Set(quotation);
            if (accepted) this._LastAcceptedQuotation.Accept(quotation.Quotation);
        }

        public void UpdateDerivativeQuotations(List<GeneralQuotation> quotations)
        {
            foreach (var quotation in quotations)
            {
                this._LastAcceptedQuotation.Accept(quotation);
            }
        }
    }
}
