using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Diagnostics;
using Manager.Common;
using ManagerService.DataAccess;
using System.Xml;

namespace ManagerService.Console
{
    public class ClientManager
    {
        private ServiceHost _ConsoleServiceHost;

        // Map for sessionId -> Client
        private Dictionary<string, Client> _Clients = new Dictionary<string, Client>();
        private Queue<Client> _ChannelBrokenClients = new Queue<Client>();
        private Timer _ConnectionCheckTimer;
        private const int _ClientTimeoutSenconds = 30;

        public void Start(string serviceAddress)
        {
            this._ConsoleServiceHost = new ServiceHost(typeof(ClientService));
            NetTcpBinding binding = new NetTcpBinding(SecurityMode.None) { ReceiveTimeout = TimeSpan.FromHours(10) };
            this._ConsoleServiceHost.AddServiceEndpoint("ManagerService.Console.IClientService", binding, serviceAddress);
            this._ConsoleServiceHost.Open();
            this._ConnectionCheckTimer = new Timer(this.DisconnectedClientCheck);
        }

        public Client AddClient(string sessionId, User user, IClientProxy clientProxy, Language language, List<DataPermission> dataPermissions)
        {
            Client client;
            this.DispatchToUser(new NotifyLogoutMessage { UserId = user.UserId }, user.UserId);
            lock (this._Clients)
            {
                client = new Client(sessionId, user, clientProxy, language, dataPermissions, this);
                this._Clients.Add(client.SessionId, client);
            }
            return client;
        }
        public bool TryRecoverConnection(string oldSessionId, string sessionId, IClientProxy clientProxy, out Client client)
        {
            client = null;
            if (!string.IsNullOrEmpty(oldSessionId) && this._Clients.TryGetValue(oldSessionId, out client))
            {
                client.Replace(sessionId, clientProxy);
                this._Clients.Remove(oldSessionId);
                this._Clients.Add(client.SessionId, client);
                return true;
            }
            return false;
        }

        //XML <NotifyChangeGroup><Gorup Id='{0}' Type='{1}'/><MemberIDs><Member ID=''>...</MMemberIDs></NotifyChangeGroup>
        public void UpdateGroup(string exchangeCode,XmlNode node)
        {
            GroupChangeType type = (GroupChangeType)Enum.Parse(typeof(GroupChangeType), node.FirstChild.FirstChild.Attributes["Type"].Value);
            Guid groupId = Guid.Parse(node.FirstChild.FirstChild.Attributes["Id"].Value);
            List<Guid> memberIds = new List<Guid>();
            foreach (XmlNode child in node.FirstChild.LastChild.ChildNodes)
            {
                memberIds.Add(Guid.Parse(child.Attributes["Id"].Value));
            }
            foreach (Client client in this._Clients.Values)
            {
                client.UpdateDataPermission(exchangeCode, type, groupId, memberIds);
            }
        }

        public void Dispatch(Message message)
        {
            lock (this._Clients)
            {
                foreach (Client client in this._Clients.Values)
                {
                    client.AddMessage(message);
                }
            }
        }

        public void DispatchExcept(Message message, Client excludedClient)
        {
            lock (this._Clients)
            {
                foreach (Client client in this._Clients.Values)
                {
                    if (!object.ReferenceEquals(client, excludedClient))
                    {
                        client.AddMessage(message);
                    }
                }
            }
        }

        public void DispatchToUser(Message message, Guid userId)
        {
            lock (this._Clients)
            {
                foreach (Client client in this._Clients.Values)
                {
                    if (client.userId == userId)
                    {
                        client.AddMessage(message);
                    }
                }
            }
        }

        public void OnClientLogout(string sessionId)
        {
            lock (this._Clients)
            {
                this._Clients.Remove(sessionId);
            }
        }

        public void AddChannelBrokenClient(Client client)
        {
            lock (this._Clients)
            {
                this._ChannelBrokenClients.Enqueue(client);
                if (this._ChannelBrokenClients.Count == 1)
                {
                    this._ConnectionCheckTimer.Change(ClientManager._ClientTimeoutSenconds * 1000, Timeout.Infinite);
                }
            }
        }

        private void DisconnectedClientCheck(object state)
        {
            try
            {
                lock (this._Clients)
                {
                    Client client = this._ChannelBrokenClients.Dequeue();
                    bool duplicateAdded = false;
                    foreach(Client client2 in this._ChannelBrokenClients)
                    {
                        if(object.ReferenceEquals(client, client2))
                        {
                            duplicateAdded = true;
                            break;
                        }
                    }
                    if (!duplicateAdded && client.ConnectionState == ConnectionState.Disconnected)
                    {
                        if (this._Clients.ContainsKey(client.SessionId))
                        {
                            this._Clients.Remove(client.SessionId);
                            Logger.AddEvent(TraceEventType.Information, "ClientManager.DisconnectedClientCheck client UserName:{0},sessionId:{1} removed from ClientManager",
                                client.user.UserName, client.SessionId);
                        }
                    }
                }
                if (this._ChannelBrokenClients.Count > 0)
                {
                    Client client = this._ChannelBrokenClients.Peek();
                    while(client.ConnectionState == ConnectionState.Connected)
                    {
                        this._ChannelBrokenClients.Dequeue();
                        if (this._ChannelBrokenClients.Count == 0) return;
                        client = this._ChannelBrokenClients.Peek();
                    }
                    int leftSeconds = ClientManager._ClientTimeoutSenconds - (int)Math.Round((DateTime.Now - client.ChannelBrokenTime).TotalSeconds, 0);
                    if (leftSeconds < 0) leftSeconds = 0;
                    this._ConnectionCheckTimer.Change(leftSeconds * 1000, Timeout.Infinite);
                }
            }
            catch(Exception exception)
            {
                Logger.TraceEvent(TraceEventType.Error, "ClientManager.DisconnectedClientCheck error\r\n{0}", exception);
            }
        }
    }
}
