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
        #region Message Event
        public delegate void ExchangeQuotationUpdateHandler(OverridedQuotationMessage overidedQuotationMessage);
        public event ExchangeQuotationUpdateHandler ExchangeQuotationUpdateEvent;

        public delegate void QuotePriceToDealerEventHandler(QuoteMessage quoteMessage);
        public event QuotePriceToDealerEventHandler QuotePriceToDealerEvent;

        public delegate void QuoteOrderToDealerEventHandler(PlaceMessage placeMessage);
        public event QuoteOrderToDealerEventHandler QuoteOrderToDealerEvent;

        public delegate void ExecutedOrderEventHandler(ExecuteMessage executeMessage);
        public event ExecutedOrderEventHandler ExecutedOrderToDealerEvent;

        public delegate void HitPriceEventHandler(HitMessage hitMessage);
        public event HitPriceEventHandler HitPriceEvent;

        public delegate void DeletedOrderEventHandler(DeleteMessage deleteMessage);
        public event DeletedOrderEventHandler DeletedOrderEvent;

        public delegate void SettingTaskRunEventHandler(UpdateSettingParameterMessage message);
        public event SettingTaskRunEventHandler OnSettingTaskRunEvent;

        public delegate void UpdateExchangeSettingHandler(UpdateMessage updateMessage);
        public event UpdateExchangeSettingHandler UpdateExchangeSettingEvent;

        #endregion

        private RelayEngine<Message> _MessageRelayEngine;

        private QuotationMessageProcessor _QuotationMessageProcessor = QuotationMessageProcessor.Instance;
        private MessageProcessor _MessageProcessor = MessageProcessor.Instance;

        public MessageClient()
        {
            this._MessageRelayEngine = new RelayEngine<Message>(this.ProcessMessage, this.HandleException);
        }

        public void SendMessage(Message message)
        {
            this._MessageRelayEngine.AddItem(message);
        }

        private bool ProcessMessage(Message message)
        {
            try
            {
                this.Process((dynamic)message);
            }
            catch (Exception exception)
            {
                Logger.TraceEvent(TraceEventType.Error, "MessageClient.ProcessMessage type:{0}\r\n{1}", message.GetType().Name, exception);
            }
            return true;
        }

        private void Process(SourceStatusMessage message)
        {
            this._QuotationMessageProcessor.Process(message);
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
            this.ExchangeQuotationUpdateEvent(overidedQuotationMessage);
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

        private void Process(UpdateSettingParameterMessage message)
        {
            this._MessageProcessor.Process(message);
            this.OnSettingTaskRunEvent(message);
        }

        private void Process(QuoteMessage quoteMessage)
        {
            if (quoteMessage != null)
            {
                try
                {
                    this.QuotePriceToDealerEvent(quoteMessage);
                }
                catch (Exception ex)
                {
                    this.HandleException(ex);
                }
            }
        }

        private void Process(PlaceMessage placeMessage)
        {
            if (placeMessage != null)
            {
                try
                {
                    this.QuoteOrderToDealerEvent(placeMessage);
                }
                catch (Exception ex)
                {
                    this.HandleException(ex);
                }
            }
        }

        private void Process(ExecuteMessage executeMessage)
        {
            this.ExecutedOrderToDealerEvent(executeMessage);
        }

        private void Process(DeleteMessage deleteMessage)
        {
            this.DeletedOrderEvent(deleteMessage);
        }

        private void Process(HitMessage hitMessage)
        {
            if (hitMessage != null)
            {
                try
                {
                    this.HitPriceEvent(hitMessage);
                }
                catch (Exception ex)
                {
                    this.HandleException(ex);
                }
            }
        }

        private void Process(UpdateMessage updateMessage)
        {
            if (updateMessage != null)
            {
                try
                {
                    this.UpdateExchangeSettingEvent(updateMessage);
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
