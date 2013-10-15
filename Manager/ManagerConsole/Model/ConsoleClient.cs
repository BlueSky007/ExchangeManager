using Manager.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace ManagerConsole.Model
{
    public class ConsoleClient
    {
        private static ConsoleClient _Instance = new ConsoleClient();

        public static ConsoleClient Instance
        {
            get
            {
                return ConsoleClient._Instance;
            }
        }

        private IClientService _ServiceProxy;
        private MessageClient _MessageClient = null;
        private string _SessionId;
        private List<AccessPermission> _AccessPermissions;

        public MessageClient MessageClient
        {
            get { return this._MessageClient; }
        }

        public void Login(Action<LoginResult> endLogin, string server, int port, string userName, string password, Language language, string oldSessionId = null)
        {
            if (this._MessageClient == null)
            {
                this._MessageClient = new MessageClient();
            }

            EndpointAddress address = new EndpointAddress(string.Format("net.tcp://{0}:{1}/Service", server, port));
            NetTcpBinding binding = new NetTcpBinding(SecurityMode.None) { MaxReceivedMessageSize = Int32.MaxValue };
            DuplexChannelFactory<IClientService> factory = new DuplexChannelFactory<IClientService>(this._MessageClient, binding, address);
            this._ServiceProxy = factory.CreateChannel();
            this._ServiceProxy.BeginLogin(userName, password, oldSessionId, language, delegate(IAsyncResult ar)
            {
                LoginResult result = this._ServiceProxy.EndLogin(ar);
                if (result.Succeeded)
                {
                    this._SessionId = result.SessionId;
                    //this.GetAccessPermissions(this.EndGetPermissions);
                }
                endLogin(result);
            }, null);
        }

        public FunctionTree GetFunctionTree()
        {
            return ConsoleClientMock.GetFunctionTree();
            try
            {
                return this._ServiceProxy.GetFunctionTree();
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "GetFunctionTree.\r\n{0}", ex.ToString());
                return null;
            }
        }

        public bool ChangePassword(string currentPassword, string newPassword)
        {
            //bool isSuccess = this._ServiceProxy.ChangePassword(currentPassword, newPassword);
            return false;
        }

        public void GetUserData(Action<List<UserData>> InitUserTile)
        {
            this._ServiceProxy.BeginGetUserData(delegate(IAsyncResult ar)
            {
                List<UserData> data = this._ServiceProxy.EndGetUserData(ar);
                InitUserTile(data);
            },null);
        }

        private void GetAccessPermissions(Action<List<AccessPermission>> endGetPermissions)
        {
            this._ServiceProxy.BeginGetAccessPermissions(delegate(IAsyncResult ar)
            {
                List<AccessPermission> permissions = this._ServiceProxy.EndGetAccessPermissions(ar);
                endGetPermissions(permissions);
            }, null);
        }

        private void EndGetPermissions(List<AccessPermission> permissions)
        {
            this._AccessPermissions = permissions;
        }

        public List<RoleData> GetRoles()
        {
            return this._ServiceProxy.GetRoles();
        }

        public RoleData GetAllPermission()
        {
            return this._ServiceProxy.GetAllPermission();
        }

        public void UpdateUser(UserData user, string password,bool isNewUser, Action<bool> EndUpdateUser)
        {
            this._ServiceProxy.BeginUpdateUsers(user, password,isNewUser, delegate(IAsyncResult ar)
            {
                bool isSuccess = this._ServiceProxy.EndUpdateUsers(ar);
                EndUpdateUser(isSuccess);
            }, null);
        }

        #region DealingConsole
        public void AbandonQuote(List<Answer> abandonQuotePrices)
        {
            this._ServiceProxy.AbandonQuote(abandonQuotePrices);
        }

        public void SendQuotePrice(List<Answer> sendQuotePrices)
        {
            this._ServiceProxy.SendQuotePrice(sendQuotePrices);
        }

        public void AcceptPlace(Guid transactionId, Action<TransactionError> EndAcceptPlace)
        {
            this._ServiceProxy.BeginAcceptPlace(transactionId, delegate(IAsyncResult result)
            {
                TransactionError transactionError = this._ServiceProxy.EndAcceptPlace(result);
                EndAcceptPlace(transactionError);
            }, null);
        }
        #endregion

        public void DeleteUser(Guid userId, Action<bool> EndDeleteUser)
        {
            this._ServiceProxy.BeginDeleteUser(userId, delegate(IAsyncResult ar)
            {
                bool isSuccess = this._ServiceProxy.EndDeleteUser(ar);
                EndDeleteUser(isSuccess);
            }, null);
        }

        
    }
}
