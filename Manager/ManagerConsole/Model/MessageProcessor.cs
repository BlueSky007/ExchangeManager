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
            ConsoleClient.Instance.MessageClient.ExecutedOrderToDealerEvent += this.MessageClient_ExecutedOrderReceived;
            ConsoleClient.Instance.MessageClient.HitPriceEvent += this.MessageClient_HitPriceReceived;
            ConsoleClient.Instance.MessageClient.DeletedOrderEvent += this.MessageClient_DeletedOrderReceived;
        }

        internal void MessageClient_QuotePriceReceived(QuoteMessage quoteMessage)
        {
            App.MainWindow.Dispatcher.BeginInvoke((Action)delegate()
            {
                MediaManager.PlayMedia(this._MediaElement, MediaManager._EnquiryMediaSource);
                this.InitDataManager.ProcessQuoteMessage(quoteMessage);
            });
        }

        internal void MessageClient_QuoteOrderReceived(PlaceMessage placeMessage)
        {
            App.MainWindow.Dispatcher.BeginInvoke((Action)delegate()
            {
                this.InitDataManager.ProcessPlaceMessage(placeMessage);
            });
        }

        internal void MessageClient_ExecutedOrderReceived(ExecuteMessage executedMessage)
        {
            App.MainWindow.Dispatcher.BeginInvoke((Action)delegate()
            {
                this.InitDataManager.ProcessExecuteMessage(executedMessage);
            });
        }

        internal void MessageClient_HitPriceReceived(HitMessage hitMessage)
        {
            App.MainWindow.Dispatcher.BeginInvoke((Action)delegate() 
            {
                this.InitDataManager.ProcessHitMessage(hitMessage);
            });
        }

        internal void MessageClient_DeletedOrderReceived(DeleteMessage deleteMessage)
        {
            App.MainWindow.Dispatcher.BeginInvoke((Action)delegate()
            {
                this.InitDataManager.ProcessDeleteMessage(deleteMessage);
            });
        }

        internal void Process(UpdateSettingParameterMessage message)
        {
            App.MainWindow.Dispatcher.BeginInvoke((Action)delegate()
            {
                TaskSchedulerModel.Instance.TaskSchedulerStatusChangeNotify(message);
            });
        }
    }
}
