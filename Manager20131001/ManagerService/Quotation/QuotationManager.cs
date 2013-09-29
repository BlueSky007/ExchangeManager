using Manager.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ManagerService.Quotation
{
    public class QuotationManager
    {
        private QuotationReceiver _QuotationReceiver;
        private ConfigMetadata _ConfigMetadata;
        private AbnormalQuotationProcessor _AbnormalQuotationProcessor;
        private DerivativeController _DerivativeController;

        public QuotationManager()
        {
            this._ConfigMetadata = new ConfigMetadata();
            this._AbnormalQuotationProcessor = new AbnormalQuotationProcessor(this._ConfigMetadata);
        }

        public void Start(int quotationListenPort)
        {
            this._QuotationReceiver = new QuotationReceiver(quotationListenPort);
            this._QuotationReceiver.Start(this);
        }

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
            Manager.ClientManager.Dispatch(new PrimitiveQuotationMessage() { Quotation = primitiveQuotation });

            if (this._ConfigMetadata.IsFromActiveSource(primitiveQuotation))
            {
                Quotation quotation = Quotation.Create(primitiveQuotation, this._ConfigMetadata);

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

        private void EnablePrice(string instrumentCode)
        {
            throw new NotImplementedException();
        }
    }
}
