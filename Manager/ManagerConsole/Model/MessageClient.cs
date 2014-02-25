using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Manager.Common;
using ManagerConsole.ViewModel;
using System.Diagnostics;

namespace ManagerConsole.Model
{
    public class MessageClient : IClientProxy
    {
        //#region Message Event
        //public delegate void ExchangeQuotationUpdateHandler(OverridedQuotationMessage overidedQuotationMessage);
        //public event ExchangeQuotationUpdateHandler ExchangeQuotationUpdateEvent;

        //public delegate void QuotePriceToDealerEventHandler(QuoteMessage quoteMessage);
        //public event QuotePriceToDealerEventHandler QuotePriceToDealerEvent;

        //public delegate void QuoteOrderToDealerEventHandler(PlaceMessage placeMessage);
        //public event QuoteOrderToDealerEventHandler QuoteOrderToDealerEvent;

        //public delegate void ExecutedOrderEventHandler(ExecuteMessage executeMessage);
        //public event ExecutedOrderEventHandler ExecutedOrderToDealerEvent;

        //public delegate void HitPriceEventHandler(HitMessage hitMessage);
        //public event HitPriceEventHandler HitPriceEvent;

        //public delegate void DeletedOrderEventHandler(DeleteMessage deleteMessage);
        //public event DeletedOrderEventHandler DeletedOrderEvent;

        //public delegate void SettingTaskRunEventHandler(TaskSchedulerRunMessage message);
        //public event SettingTaskRunEventHandler OnSettingTaskRunEvent;

        //public delegate void UpdateExchangeSettingHandler(UpdateMessage updateMessage);
        //public event UpdateExchangeSettingHandler UpdateExchangeSettingEvent;

        //#endregion

        private RelayEngine<Message> _MessageRelayEngine;

        private QuotationMessageProcessor _QuotationMessageProcessor = QuotationMessageProcessor.Instance;
        private ExchangeDataManager _ExchangeDataManager;

        public MessageClient()
        {
            this._MessageRelayEngine = new RelayEngine<Message>(this.ProcessMessage, this.HandleException);
            this._MessageRelayEngine.Suspend();
            this._ExchangeDataManager = App.MainFrameWindow.ExchangeDataManager;
        }

        public void SendMessage(Message message)
        {
            this._MessageRelayEngine.AddItem(message);
        }

        public void StartMessageProcess()
        {
            this._MessageRelayEngine.Resume();
        }

        private bool ProcessMessage(Message message)
        {
            try
            {
                App.MainFrameWindow.Dispatcher.BeginInvoke((Action)delegate()
                {
                    try
                    {
                        this.Process((dynamic)message);
                    }
                    catch (Exception exception)
                    {
                        Logger.TraceEvent(TraceEventType.Error, "MessageClient Dispatcher Message type:{0}\r\n{1}", message.GetType().Name, exception);
                    }
                });
                
            }
            catch (Exception exception)
            {
                Logger.TraceEvent(TraceEventType.Error, "MessageClient.ProcessMessage type:{0}\r\n{1}", message.GetType().Name, exception);
            }
            return true;
        }

        private void Process(SourceConnectionStatusMessage message)
        {
            this._QuotationMessageProcessor.Process(message);
        }

        private void Process(ExchangeConnectionStatusMessage message)
        {
            App.MainFrameWindow.StatusBar.Process(message);
        }

        private void Process(QuotationsMessage message)
        {
            this._QuotationMessageProcessor.Process(message);
        }
        private void Process(SwitchRelationBooleanPropertyMessage message)
        {
            this._QuotationMessageProcessor.Process(message);
        }

        private void Process(PrimitiveQuotationMessage primitiveQuotationMessage)
        {
            this._QuotationMessageProcessor.Process(primitiveQuotationMessage);
        }

        private void Process(AbnormalQuotationMessage abnormalQuotationMessage)
        {
            this._QuotationMessageProcessor.Process(abnormalQuotationMessage);
        }

        private void Process(OverridedQuotationMessage overidedQuotationMessage)
        {
            this._QuotationMessageProcessor.Process(overidedQuotationMessage);
            this._ExchangeDataManager.ProcessOverridedQuotation(overidedQuotationMessage);
        }

        private void Process(UpdateInstrumentQuotationMessage quotePolicyDetailMessage)
        {
            this._QuotationMessageProcessor.Process(quotePolicyDetailMessage);
        }

        private void Process(AddMetadataObjectMessage message)
        {
            this._QuotationMessageProcessor.Process(message);
        }

        private void Process(AddMetadataObjectsMessage message)
        {
            this._QuotationMessageProcessor.Process(message);
        }

        private void Process(UpdateMetadataMessage message)
        {
            this._QuotationMessageProcessor.Process(message);
        }

        private void Process(DeleteMetadataObjectMessage message)
        {
            this._QuotationMessageProcessor.Process(message);
        }

        private void Process(TaskSchedulerRunMessage message)
        {
            TaskSchedulerModel.Instance.TaskSchedulerStatusChangeNotify(message);
        }

        private void Process(QuoteMessage quoteMessage)
        {
            this._ExchangeDataManager.ProcessQuoteMessage(quoteMessage);
        }

        private void Process(PlaceMessage placeMessage)
        {
            this._ExchangeDataManager.ProcessPlaceMessage(placeMessage);
        }

        private void Process(ExecuteMessage executeMessage)
        {
            this._ExchangeDataManager.ProcessExecuteMessage(executeMessage);
        }

        private void Process(DeleteMessage deleteMessage)
        {
            this._ExchangeDataManager.ProcessDeleteMessage(deleteMessage);
        }

        private void Process(HitMessage hitMessage)
        {
            this._ExchangeDataManager.ProcessHitMessage(hitMessage);
        }

        private void Process(UpdateMessage updateMessage)
        {
            this._ExchangeDataManager.ProcessUpdateMessage(updateMessage);
        }

        internal void Process(AccessPermissionUpdateMessage message)
        {
            if (message != null)
            {
                try
                {
                    if (Principal.Instance.User.UserId == message.user.UserId)
                    {
                        Principal.Instance.ProcessUpdate(message.NewPermission);
                    }
                }
                catch (Exception ex)
                {
                    HandleException(ex);

                }
            }
        }

        internal void Process(UpdateRoleMessage message)
        {
            if (message != null)
            {
                try
                {
                    if (Principal.Instance.User.IsInRole(message.RoleId))
                    {
                        Principal.Instance.ProcessUpdateRole(message.Type, message.RoleId);
                    }
                }
                catch (Exception ex)
                {
                    HandleException(ex);

                }
            }
        }

        internal void Process(NotifyLogoutMessage message)
        {
            if (message != null)
            {
                try
                {
                    if (Principal.Instance.User.UserId == message.UserId)
                    {
                        App.MainFrameWindow.KickOut();
                    }
                }
                catch (Exception ex)
                {
                    this.HandleException(ex);
                }
            }
        }

        private void HandleException(Exception exception)
        {
            Logger.AddEvent(TraceEventType.Error, exception.ToString());
        }
    }
}
