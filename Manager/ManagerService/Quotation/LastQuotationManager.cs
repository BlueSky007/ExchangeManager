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
        private Dictionary<int, Dictionary<int, Quotation>> _LastReceivedQuotations = new Dictionary<int, Dictionary<int, Quotation>>();
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
        public void Set(Quotation quotation)
        {
            if (!this._LastReceivedQuotations.ContainsKey(quotation.SourceId))
            {
                this._LastReceivedQuotations.Add(quotation.SourceId, new Dictionary<int, Quotation>());
            }
            this._LastReceivedQuotations[quotation.SourceId][quotation.InstrumentId] = quotation;
        }
    }

    public class LastAcceptedQuotation
    {
        // Map for SourceId - [InstrumentId -  Quotation]
        private Dictionary<int, Dictionary<int, Quotation>> _LastAcceptedQuotations = new Dictionary<int, Dictionary<int, Quotation>>();

        private ReaderWriterLockSlim _LastQuotationLock = new ReaderWriterLockSlim();

        public bool TryGetLastQuotation(int sourceId, int instrumentId, out Quotation quotation)
        {
            this._LastQuotationLock.EnterReadLock();
            try
            {
                quotation = null;
                if (this._LastAcceptedQuotations.ContainsKey(sourceId))
                {
                    if (this._LastAcceptedQuotations[sourceId].ContainsKey(instrumentId))
                    {
                        quotation = this._LastAcceptedQuotations[sourceId][instrumentId];
                    }
                }
                return quotation != null;
            }
            finally
            {
                this._LastQuotationLock.ExitReadLock();
            }
        }
        public void Accept(Quotation quotation)
        {
            this._LastQuotationLock.EnterWriteLock();
            try
            {
                if (!this._LastAcceptedQuotations.ContainsKey(quotation.SourceId))
                {
                    this._LastAcceptedQuotations.Add(quotation.SourceId, new Dictionary<int, Quotation>());
                }
                this._LastAcceptedQuotations[quotation.SourceId][quotation.InstrumentId] = quotation;
                QuotationData.SaveLastQuotation(quotation.PrimitiveQuotation);
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

        public void Update(Quotation quotation, bool accepted)
        {
            this._LastReceivedQuotation.Set(quotation);
            if (accepted) this._LastAcceptedQuotation.Accept(quotation);
        }
    }
}
