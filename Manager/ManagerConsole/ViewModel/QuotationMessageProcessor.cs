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

        //private ObservableCollection<VmSourceQuotation> _SourceQuotations = null;

        private QuotationMessageProcessor() { }

        //public ObservableCollection<VmSourceQuotation> QuotationSources
        //{
        //    get
        //    {
        //        if (this._SourceQuotations == null)
        //        {
        //            this._SourceQuotations = new ObservableCollection<VmSourceQuotation>();
        //        }
        //        return this._SourceQuotations;
        //    }
        //}

        public void Process(PrimitiveQuotationMessage message)
        {
            App.MainFrameWindow.Dispatcher.BeginInvoke((Action)delegate()
            {
                VmQuotationManager.Instance.SetPrimitiveQuotation(message.Quotation);
            });
        }

        public void Process(AbnormalQuotationMessage message)
        {
            App.MainFrameWindow.Dispatcher.BeginInvoke((Action)delegate()
            {
                VmQuotationManager.Instance.AddAbnormalQuotation(message);
                App.MainFrameWindow.ShowAbnormalQuotation();
            });
        }

        public void Process(OverridedQuotationMessage message)
        {
            App.MainFrameWindow.Dispatcher.BeginInvoke((Action)delegate()
            {
                ExchangeQuotationViewModel.Instance.Load(message.ExchangeCode, message.OverridedQs);
            });
        }

        public void Process(UpdateInstrumentQuotationMessage message)
        {
            App.MainFrameWindow.Dispatcher.BeginInvoke((Action)delegate()
            {
                ExchangeQuotationViewModel.Instance.UpdateExchangeQuotationPolicy(message.QuotePolicyChangeDetails.ToList());
            });
        }

        public void Process(SourceStatusMessage sourceStatusMessage)
        {
            // TODO: handle SourceStatusMessage here
            throw new NotImplementedException();
        }

        internal void Process(AddMetadataObjectMessage message)
        {
            App.MainFrameWindow.Dispatcher.BeginInvoke((Action)delegate()
            {
                VmQuotationManager.Instance.Add((dynamic)message.MetadataObject);
            });
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
            App.MainFrameWindow.Dispatcher.BeginInvoke((Action)delegate()
            {
                VmQuotationManager.Instance.Update(message);
            });
        }
        internal void Process(DeleteMetadataObjectMessage message)
        {
            App.MainFrameWindow.Dispatcher.BeginInvoke((Action)delegate()
            {
                VmQuotationManager.Instance.Delete(message);
            });
        }

        internal void Process(SwitchRelationBooleanPropertyMessage message)
        {
            App.MainFrameWindow.Dispatcher.BeginInvoke((Action)delegate()
            {
                VmQuotationManager.Instance.SwitchActiveSource(message);
            });
        }

        internal void Process(QuotationsMessage message)
        {
            App.MainFrameWindow.Dispatcher.BeginInvoke((Action)delegate()
            {
                VmQuotationManager.Instance.SetQuotation(message);
            });
        }
    }
}
