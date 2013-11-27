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
            App.MainWindow.Dispatcher.BeginInvoke((Action)delegate()
            {
                VmQuotationManager.Instance.SetPrimitiveQuotation(message.Quotation);
            });
        }

        public void Process(AbnormalQuotationMessage message)
        {
            App.MainWindow.Dispatcher.BeginInvoke((Action)delegate()
            {
                App.MainWindow.ShowAbnormalQuotation(message);
            });
        }

        public void Process(SourceStatusMessage sourceStatusMessage)
        {

        }

        internal void Process(AddMetadataObjectMessage message)
        {
            App.MainWindow.Dispatcher.BeginInvoke((Action)delegate()
            {
                throw new NotImplementedException();
                //QuotationConfigData.Instance.Add(message.MetadataObject);
            });
        }


        internal void Process(UpdateMetadataMessage message)
        {
            App.MainWindow.Dispatcher.BeginInvoke((Action)delegate()
            {
                VmQuotationManager.Instance.Update(message);
            });
        }
        internal void Process(DeleteMetadataObjectMessage message)
        {
            App.MainWindow.Dispatcher.BeginInvoke((Action)delegate()
            {
                VmQuotationManager.Instance.Delete(message);
            });
        }
    }
}
