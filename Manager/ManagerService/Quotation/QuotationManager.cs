using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Manager.Common;
using Manager.Common.QuotationEntities;
using System.Diagnostics;

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

        public QuotationManager()
        {
            this._ConfigMetadata = new ConfigMetadata();
            this._SourceController = new SourceController();
            this._LastQuotationManager = new LastQuotationManager();
            this._AbnormalQuotationManager = new AbnormalQuotationManager(this._ConfigMetadata.RangeCheckRules, this._LastQuotationManager);
            this._DerivativeController = new DerivativeController(this._ConfigMetadata.DerivativeRelations);
        }

        public void Start(int quotationListenPort)
        {
            this._QuotationReceiver = new QuotationReceiver(quotationListenPort);
            this._QuotationReceiver.Start(this);
        }

        public ConfigMetadata ConfigMetadata { get { return this._ConfigMetadata; } }

        public bool AuthenticateSource(string sourceName, string loginName, string password)
        {
            return this._ConfigMetadata.AuthenticateSource(sourceName, loginName, password);
        }

        public void QuotationSourceStatusChanged(string sourceName, ConnectionState state)
        {
            Manager.ClientManager.Dispatch(new SourceStatusMessage() { SouceName = sourceName, ConnectionState = state });
        }

        public void ProcessQuotation(PrimitiveQuotation primitiveQuotation)
        {
            if (this._ConfigMetadata.EnsureIsKnownQuotation(primitiveQuotation))
            {
                double ask, bid;
                if (this._LastQuotationManager.LastReceived.Fix(primitiveQuotation, out ask, out bid))
                {
                    if (this._LastQuotationManager.LastReceived.IsNotSame(primitiveQuotation))
                    {
                        Manager.ClientManager.Dispatch(new PrimitiveQuotationMessage() { Quotation = primitiveQuotation });

                        Quotation quotation = new Quotation(primitiveQuotation, ask, bid);
                        bool quotationAccepted = true;
                        if (this._SourceController.QuotationArrived(quotation))
                        {
                            // quotation come from Active Source
                            if (this._AbnormalQuotationManager.SetQuotation(quotation))
                            {
                                // quotation is normal
                                this.ProcessNormalQuotation(quotation);
                            }
                            else
                            {
                                quotationAccepted = false;
                            }
                        }
                        this._LastQuotationManager.Update(quotation, quotationAccepted);
                    }
                }
            }
            else
            {
                Logger.AddEvent(TraceEventType.Warning, "Discarded price:[ask={0}, bid={1}] got for {2} from {3}",
                    primitiveQuotation.Ask, primitiveQuotation.Bid, primitiveQuotation.InstrumentCode, primitiveQuotation.SourceName);
            }
        }

        public void ProcessNormalQuotation(Quotation quotation)
        {
            this.EnablePrice(quotation.PrimitiveQuotation.InstrumentCode);
            List<Quotation> quotations = this._DerivativeController.Derive(quotation);
            Manager.ExchangeManager.SetQuotation(quotations);
            this._LastQuotationManager.Update(quotation, true);
        }
   
        private void EnablePrice(string instrumentCode)
        {
            throw new NotImplementedException();
        }
    }
}
