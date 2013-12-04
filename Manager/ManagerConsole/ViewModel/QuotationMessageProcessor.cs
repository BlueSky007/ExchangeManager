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
                VmQuotationManager.Instance.AddAbnormalQuotation(message);
                App.MainWindow.ShowAbnormalQuotation();
            });
        }

        public void Process(SourceStatusMessage sourceStatusMessage)
        {

        }

        internal void Process(AddMetadataObjectMessage message)
        {
            App.MainWindow.Dispatcher.BeginInvoke((Action)delegate()
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

        internal void Process(SwitchRelationBooleanPropertyMessage switchRelationPropertyMessage)
        {
            App.MainWindow.Dispatcher.BeginInvoke((Action)delegate()
            {
                VmQuotationManager.Instance.SwitchActiveSource(switchRelationPropertyMessage);
            });
        }
    }
}
