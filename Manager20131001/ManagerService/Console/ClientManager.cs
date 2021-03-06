﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using Manager.Common;

namespace ManagerService.Console
{
    public class ClientManager
    {
        private ServiceHost _ConsoleServiceHost;

        private Dictionary<string, Client> _Clients = new Dictionary<string, Client>();

        public void Start(string serviceAddress)
        {
            this._ConsoleServiceHost = new ServiceHost(typeof(ClientService));
            NetTcpBinding binding = new NetTcpBinding(SecurityMode.None) { ReceiveTimeout = TimeSpan.FromHours(10) };
            this._ConsoleServiceHost.AddServiceEndpoint("ManagerService.Console.IClientService", binding, serviceAddress);
            this._ConsoleServiceHost.Open();
        }

        public Client AddClient(string oldSessionId, string sessionId, Guid userId, IClientProxy clientProxy, Language language)
        {
            Client client;
            if (!string.IsNullOrEmpty(oldSessionId) && this._Clients.TryGetValue(oldSessionId, out client))
            {
                client.Replace(sessionId, clientProxy);
                this._Clients.Remove(oldSessionId);
            }
            else
            {
                client = new Client(sessionId, userId, clientProxy, language);
            }
            this._Clients.Add(client.SessionId, client);
            return client;
        }

        public void Dispatch(global::Manager.Common.Message message)
        {
            foreach (Client client in this._Clients.Values)
            {
                client.AddMessage(message);
            }
        }

        private void ConnectionCheck()
        {

        }
    }
}
