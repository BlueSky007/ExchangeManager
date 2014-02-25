using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Manager.Common;

namespace ManagerConsole.ViewModel
{
    public class QuotationMessageProcessor
    {
        public static QuotationMessageProcessor Instance = new QuotationMessageProcessor();

        private QuotationMessageProcessor() { }

        public void Process(PrimitiveQuotationMessage message)
        {
            VmQuotationManager.Instance.SetPrimitiveQuotation(message.Quotation);
        }

        public void Process(AbnormalQuotationMessage message)
        {
            VmQuotationManager.Instance.AddAbnormalQuotation(message);
            App.MainFrameWindow.ShowAbnormalQuotationPane();
        }

        public void Process(OverridedQuotationMessage message)
        {
            ExchangeQuotationViewModel.Instance.Load(message.ExchangeCode, message.OverridedQs);
        }

        public void Process(UpdateInstrumentQuotationMessage message)
        {
            ExchangeQuotationViewModel.Instance.UpdateExchangeQuotationPolicy(message.QuotePolicyChangeDetails.ToList());
        }

        public void Process(SourceConnectionStatusMessage message)
        {
            VmQuotationManager.Instance.QuotationSources.Single(s => s.Name == message.SouceName).ConnectionState = message.ConnectionState;
        }

        internal void Process(AddMetadataObjectMessage message)
        {
            VmQuotationManager.Instance.Add((dynamic)message.MetadataObject);
        }

        internal void Process(AddMetadataObjectsMessage message)
        {
            for (int i = 0; i < message.MetadataObjects.Length; i++)
            {
                VmQuotationManager.Instance.Add((dynamic)message.MetadataObjects[i]);
            }
        }

        internal void Process(UpdateMetadataMessage message)
        {
            VmQuotationManager.Instance.Update(message);
        }
        internal void Process(DeleteMetadataObjectMessage message)
        {
            VmQuotationManager.Instance.Delete(message);
        }

        internal void Process(SwitchRelationBooleanPropertyMessage message)
        {
            VmQuotationManager.Instance.SwitchActiveSource(message);
        }

        internal void Process(QuotationsMessage message)
        {
            VmQuotationManager.Instance.SetQuotation(message);
        }
    }
}
