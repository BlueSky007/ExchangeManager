using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Diagnostics;
using Manager.Common;
using ManagerService.DataAccess;
using System.Xml;

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

        public Client AddClient(string oldSessionId, string sessionId, User user, IClientProxy clientProxy, Language language)
        {
            Client client;
            if (!string.IsNullOrEmpty(oldSessionId) && this._Clients.TryGetValue(oldSessionId, out client))
            {
                client.Replace(sessionId, clientProxy);
                this._Clients.Remove(oldSessionId);
            }
            else
            {
                client = new Client(sessionId, user, clientProxy, language);
            }
            this._Clients.Add(client.SessionId, client);
            return client;
        }

        public void UpdateGroup(string exchangeCode,XmlNode node)
        {
            string type = node.FirstChild.FirstChild.Attributes["Type"].Value;
            Guid id = Guid.Parse(node.FirstChild.FirstChild.Attributes["Id"].Value);
            List<Guid> memberIds = new List<Guid>();
            foreach (XmlNode child in node.FirstChild.LastChild.ChildNodes)
            {
                memberIds.Add(Guid.Parse(child.Attributes["Id"].Value));
            }
            foreach (Client client in this._Clients.Values)
            {
                client.UpdateDataPermission(exchangeCode, type, id, memberIds);
            }
        }

        public void UpdatePermission(RoleData role)
        {
            foreach (Client client in this._Clients.Values)
            {
                if (client.user.IsInRole(role.RoleId))
                {
                    List<DataPermission> dataPermissions = new List<DataPermission>();
                    Dictionary<string, List<Guid>> accountPermissions = new Dictionary<string, List<Guid>>();
                    Dictionary<string, List<Guid>> instrumentPermissions = new Dictionary<string, List<Guid>>();
                    foreach (ExchangeSystemSetting item in MainService.ManagerSettings.ExchangeSystems)
                    {
                        bool deafultStatus = false;
                        List<Guid> accountMemberIds = new List<Guid>();
                        List<Guid> instrumentMemberIds = new List<Guid>();
                        List<RoleDataPermission> systemPermissions = role.DataPermissions.FindAll(delegate(RoleDataPermission data)
                        {
                            return data.IExchangeCode == item.Code;
                        });
                        RoleDataPermission account = systemPermissions.SingleOrDefault(r => r.Type == DataObjectType.Account && r.Level == 2);
                        if (account != null)
                        {
                            deafultStatus = account.IsAllow;
                        }
                        else
                        {
                            RoleDataPermission exchange = systemPermissions.SingleOrDefault(r => r.Level == 1);
                            if (exchange != null)
                            {
                                deafultStatus = exchange.IsAllow;
                            }
                        }
                        accountMemberIds.AddRange(ExchangeData.GetNewMemberIds(item.Code, deafultStatus, systemPermissions, DataObjectType.Account));
                        RoleDataPermission instrument = systemPermissions.SingleOrDefault(r => r.Type == DataObjectType.Instrument && r.Level == 2);
                        if (instrument != null)
                        {
                            deafultStatus = instrument.IsAllow;
                        }
                        else
                        {
                            RoleDataPermission exchange = systemPermissions.SingleOrDefault(r => r.Level == 1);
                            if (exchange != null)
                            {
                                deafultStatus = exchange.IsAllow;
                            }
                        }
                        instrumentMemberIds.AddRange(ExchangeData.GetNewMemberIds(item.Code, deafultStatus, systemPermissions, DataObjectType.Instrument));
                        accountPermissions.Add(item.Code, accountMemberIds);
                        instrumentPermissions.Add(item.Code, instrumentMemberIds);
                    }
                    client.UpdatePermission(accountPermissions, instrumentPermissions);
                }
            }
        }

        public void Dispatch(Message message)
        {
            foreach (Client client in this._Clients.Values)
            {
                client.AddMessage(message);
            }
        }

        public void DispatchExcept(Message message, Client excludedClient)
        {
            foreach (Client client in this._Clients.Values)
            {
                if (!object.ReferenceEquals(client, excludedClient))
                {
                    client.AddMessage(message);
                }
            }
        }

        private void ConnectionCheck()
        {

        }
    }
}
