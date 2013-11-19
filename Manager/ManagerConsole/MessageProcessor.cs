using ManagerConsole.Model;
using System;
using System.Windows.Controls;
using QuoteMessage = Manager.Common.QuoteMessage;
using PlaceMessage = Manager.Common.PlaceMessage;
using HitMessage = Manager.Common.HitMessage;


namespace ManagerConsole
{
    public class MessageProcessor
    {
        private MediaElement _MediaElement;
        public MessageProcessor(MediaElement media,InitDataManager dataManager)
        {
            this.AttachEvent();
            this._MediaElement = media;
            this.InitDataManager = dataManager;
        }

        #region Property

        private InitDataManager InitDataManager
        {
            get;
            set;
        }
        #endregion

        private void AttachEvent()
        {
            ConsoleClient.Instance.MessageClient.QuotePriceToDealerEvent += this.MessageClient_QuotePriceReceived;
            ConsoleClient.Instance.MessageClient.QuoteOrderToDealerEvent += this.MessageClient_QuoteOrderReceived;
            ConsoleClient.Instance.MessageClient.HitPriceEvent += this.MessageClient_HitPriceReceived;
        }

        void MessageClient_QuotePriceReceived(QuoteMessage quoteMessage)
        {
            App.MainWindow.Dispatcher.BeginInvoke((Action)delegate()
            {
                //声音处理
                MediaManager.PlayMedia(this._MediaElement, MediaManager._EnquiryMediaSource);
                this.InitDataManager.ProcessQuoteMessage(quoteMessage);
            });
        }

        void MessageClient_QuoteOrderReceived(PlaceMessage placeMessage)
        {
            App.MainWindow.Dispatcher.BeginInvoke((Action)delegate()
            {
                this.InitDataManager.ProcessPlaceMessage(placeMessage);
            });
        }

        void MessageClient_HitPriceReceived(HitMessage hitMessage)
        {
            App.MainWindow.Dispatcher.BeginInvoke((Action)delegate() {
                this.InitDataManager.ProcessHitMessage(hitMessage);
            });
        }
    }
}
