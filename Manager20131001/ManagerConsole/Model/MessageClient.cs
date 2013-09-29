using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Manager.Common;

namespace ManagerConsole.Model
{
    public class MessageClient : IClientProxy
    {
        #region Event
        public delegate void QuotePriceToDealerEventHandler(QuoteMessage quoteMessage);
        public event QuotePriceToDealerEventHandler QuotePriceToDealerEvent;
        #endregion

        private RelayEngine<Message> _MessageRelayEngine;

        public MessageClient()
        {
            this._MessageRelayEngine = new RelayEngine<Message>(this.ProcessMessage, this.HandleException);
        }

        public void SendMessage(Message message)
        {
            this._MessageRelayEngine.AddItem(message);
        }
        public void Answer(Answer answerMessage)
        {
            this._MessageRelayEngine.AddItem(answerMessage);
        }

        private bool ProcessMessage(Message message)
        {
            return this.Process((dynamic)message);
        }

        private bool Process(PriceMessage priceMessage)
        {
            return true;
        }

        private bool Process(PrimitiveQuotationMessage primitiveQuotationMessage)
        {
            return true;
        }

        private bool Process(QuoteMessage quoteMessage)
        {
            if (quoteMessage != null)
            {
                try
                {
                    this.QuotePriceToDealerEvent(quoteMessage);
                    return true;
                }
                catch (Exception ex)
                {
                    this.HandleException(ex);
                }
            }
            return true;
        }

        private void HandleException(Exception exception)
        {
            Logger.AddEvent(System.Diagnostics.TraceEventType.Error, exception.ToString());
        }
    }
}
