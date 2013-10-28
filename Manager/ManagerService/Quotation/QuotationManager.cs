using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Manager.Common;
using Manager.Common.QuotationEntities;

namespace ManagerService.Quotation
{
    public class QuotationManager
    {
        private QuotationReceiver _QuotationReceiver;
        private ConfigMetadata _ConfigMetadata;
        private SourceController _SourceController;
        private AbnormalQuotationProcessor _AbnormalQuotationProcessor;
        private DerivativeController _DerivativeController;

        public QuotationManager()
        {
            this._ConfigMetadata = new ConfigMetadata();
            this._SourceController = new SourceController();
            this._AbnormalQuotationProcessor = new AbnormalQuotationProcessor(this._ConfigMetadata);
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
            int instrumentId, sourceId;
            if (this._ConfigMetadata.IsKnownQuotation(primitiveQuotation, out instrumentId, out sourceId))
            {
                Manager.ClientManager.Dispatch(new PrimitiveQuotationMessage() { Quotation = primitiveQuotation });

                if (this._ConfigMetadata.IsFromActiveSource(instrumentId, sourceId))
                {
                    Quotation quotation = Quotation.Create(instrumentId, sourceId, primitiveQuotation);
                    this._SourceController.QuotationArrived(quotation);

                    this._ConfigMetadata.Adjust(quotation);
                    if (this._AbnormalQuotationProcessor.IsWaitForPreOutOfRangeConfirmed(primitiveQuotation))
                    {
                        this._AbnormalQuotationProcessor.AddAndWait(primitiveQuotation);
                    }
                    else
                    {
                        if (this._AbnormalQuotationProcessor.IsNormalPrice(primitiveQuotation))
                        {
                            this.EnablePrice(primitiveQuotation.InstrumentCode);
                            List<PrimitiveQuotation> quotations = this._DerivativeController.Derive(primitiveQuotation);
                            Manager.ExchangeManager.ProcessQuotation(quotations);
                        }
                        else
                        {
                            this._AbnormalQuotationProcessor.StartProcessAbnormalQuotation(primitiveQuotation);
                        }
                    }
                }
            }
        }

        private void EnablePrice(string instrumentCode)
        {
            throw new NotImplementedException();
        }
    }
}
