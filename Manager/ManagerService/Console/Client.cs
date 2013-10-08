using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Manager.Common;
using System.Diagnostics;

namespace ManagerService.Console
{
    public class Client
    {
        private string _SessionId;
        private Guid _UserId;
        private IClientProxy _ClientProxy;
        private RelayEngine<Message> _MessageRelayEngine;
        private Language _Language;

        public Client(string sessionId,Guid userId, IClientProxy clientProxy, Language language)
        {
            this._SessionId = sessionId;
            this._UserId = userId;
            this._ClientProxy = clientProxy;
            this._Language = language;
            this._MessageRelayEngine = new RelayEngine<Message>(this.SendMessage, this.HandlEngineException);
        }

        public string SessionId { get { return this._SessionId; } }
        public Guid userId { get { return this._UserId; } }
        public Language language { get { return this._Language; } }

        public void Replace(string sessionId, IClientProxy clientProxy)
        {
            this._SessionId = sessionId;
            this._ClientProxy = clientProxy;
            this._MessageRelayEngine.Resume();
        }

        public void AddMessage(Message message)
        {
            Message msg = this.Filter(message);

            if (msg != null)
            {
                this._MessageRelayEngine.AddItem(msg);
            }
        }

        public void Channel_Broken(object sender, EventArgs e)
        {
            this._MessageRelayEngine.Suspend();
        }

        public void Close()
        {
            this._MessageRelayEngine.Stop();
        }

        private bool SendMessage(Message message)
        {
            try
            {
                this._ClientProxy.SendMessage(message);
                return true;
            }
            catch (Exception ex)
            {
                Logger.AddEvent(TraceEventType.Error, "Client.SendMessage error:\r\n{0}", ex.ToString());
            }
            return false;
        }

        private void HandlEngineException(Exception ex)
        {
            Logger.TraceEvent(TraceEventType.Error, "ExchangeSystem.HandlEngineException RelayEngine stopped:\r\n" + ex.ToString());
        }

        private Message Filter(Message message)
        {
            IFilterable filterableMessage = message as IFilterable;
            // TODO: perform filter here
            if (filterableMessage.AccountId != null)
            { 
            }
            if (filterableMessage.InstrumentId != null)
            { 
            }

            return message;
        }
    }
}
