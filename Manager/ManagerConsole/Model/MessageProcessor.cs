using ManagerConsole.Model;
using System;
using System.Windows.Controls;
using QuoteMessage = Manager.Common.QuoteMessage;
using PlaceMessage = Manager.Common.PlaceMessage;
using HitMessage = Manager.Common.HitMessage;
using ExecuteMessage = Manager.Common.ExecuteMessage;
using DeleteMessage = Manager.Common.DeleteMessage;
using Manager.Common;
using ManagerConsole.ViewModel;


namespace ManagerConsole
{
    public class MessageProcessor
    {
        public static MessageProcessor Instance = new MessageProcessor();

        private MediaElement _MediaElement;
        public MessageProcessor()
        { 
        }

        public MessageProcessor(MediaElement media,ExchangeDataManager dataManager)
        {
            this.AttachEvent();
            this._MediaElement = media;
            this.ExchangeDataManager = dataManager;
        }

        #region Property

        private ExchangeDataManager ExchangeDataManager
        {
            get;
            set;
        }
        #endregion

        private void AttachEvent()
        {
            ConsoleClient.Instance.MessageClient.ExchangeQuotationUpdateEvent += this.MessageClient_OverridedQuotationReceived;
            ConsoleClient.Instance.MessageClient.QuotePriceToDealerEvent += this.MessageClient_QuotePriceReceived;
            ConsoleClient.Instance.MessageClient.QuoteOrderToDealerEvent += this.MessageClient_QuoteOrderReceived;
            ConsoleClient.Instance.MessageClient.ExecutedOrderToDealerEvent += this.MessageClient_ExecutedOrderReceived;
            ConsoleClient.Instance.MessageClient.HitPriceEvent += this.MessageClient_HitPriceReceived;
            ConsoleClient.Instance.MessageClient.DeletedOrderEvent += this.MessageClient_DeletedOrderReceived;
            ConsoleClient.Instance.MessageClient.UpdateExchangeSettingEvent += this.MessageClient_UpdateMessageNotifyReceived;
        }

        internal void MessageClient_OverridedQuotationReceived(OverridedQuotationMessage overridedQuotationMessage)
        {
            App.Current.Dispatcher.BeginInvoke((Action)delegate() 
            {
                this.ExchangeDataManager.ProcessOverridedQuotation(overridedQuotationMessage);
            });
        }

        internal void MessageClient_QuotePriceReceived(QuoteMessage quoteMessage)
        {
            App.MainFrameWindow.Dispatcher.BeginInvoke((Action)delegate()
            {
                MediaManager.PlayMedia(this._MediaElement, MediaManager._EnquiryMediaSource);
                this.ExchangeDataManager.ProcessQuoteMessage(quoteMessage);
            });
        }

        internal void MessageClient_QuoteOrderReceived(PlaceMessage placeMessage)
        {
            App.MainFrameWindow.Dispatcher.BeginInvoke((Action)delegate()
            {
                this.ExchangeDataManager.ProcessPlaceMessage(placeMessage);
            });
        }

        internal void MessageClient_ExecutedOrderReceived(ExecuteMessage executedMessage)
        {
            App.MainFrameWindow.Dispatcher.BeginInvoke((Action)delegate()
            {
                this.ExchangeDataManager.ProcessExecuteMessage(executedMessage);
            });
        }

        internal void MessageClient_HitPriceReceived(HitMessage hitMessage)
        {
            App.MainFrameWindow.Dispatcher.BeginInvoke((Action)delegate() 
            {
                this.ExchangeDataManager.ProcessHitMessage(hitMessage);
            });
        }

        internal void MessageClient_DeletedOrderReceived(DeleteMessage deleteMessage)
        {
            App.MainFrameWindow.Dispatcher.BeginInvoke((Action)delegate()
            {
                this.ExchangeDataManager.ProcessDeleteMessage(deleteMessage);
            });
        }

        internal void MessageClient_UpdateMessageNotifyReceived(UpdateMessage message)
        {
            App.MainFrameWindow.Dispatcher.BeginInvoke((Action)delegate()
            {
                this.ExchangeDataManager.ProcessUpdateMessage(message);
            });
        }

        internal void Process(UpdateSettingParameterMessage message)
        {
            App.MainFrameWindow.Dispatcher.BeginInvoke((Action)delegate()
            {
                TaskSchedulerModel.Instance.TaskSchedulerStatusChangeNotify(message);
            });
        }

        
    }
}
