using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Diagnostics;
using Manager.Common;
using ManagerService.DataAccess;

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

        public Client AddClient(string oldSessionId, string sessionId, User user, IClientProxy clientProxy, Language language, Dictionary<string, List<Guid>> accountPermissions, Dictionary<string, List<Guid>> instrumentPermissions)
        {
            Client client;
            if (!string.IsNullOrEmpty(oldSessionId) && this._Clients.TryGetValue(oldSessionId, out client))
            {
                client.Replace(sessionId, clientProxy, accountPermissions, instrumentPermissions);
                this._Clients.Remove(oldSessionId);
            }
            else
            {
                client = new Client(sessionId, user, clientProxy, language, accountPermissions, instrumentPermissions);
            }
            this._Clients.Add(client.SessionId, client);
            return client;
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
                    foreach (ExchangeSystemSetting item in Manager.ManagerSettings.ExchangeSystems)
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
