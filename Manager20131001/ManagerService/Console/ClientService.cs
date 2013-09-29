using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using Manager.Common;
using ManagerService.DataAccess;
using ManagerService.Exchange;
using System.Diagnostics;

namespace ManagerService.Console
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class ClientService : IClientService
    {
        private Client _Client;

        public LoginResult Login(string userName, string password, string oldSessionId, Language language)
        {
            Guid userId = UserDataAccess.LoginIn(userName,password);
            LoginResult loginResult = new LoginResult();
            if (userId != Guid.Empty)
            {
                string sessionId = OperationContext.Current.SessionId;
                IClientProxy clientProxy = OperationContext.Current.GetCallbackChannel<IClientProxy>();
                this._Client = Manager.ClientManager.AddClient(oldSessionId, sessionId, userId, clientProxy, language);

                OperationContext.Current.Channel.Faulted += this._Client.Channel_Broken;
                OperationContext.Current.Channel.Closed += this._Client.Channel_Broken;
                loginResult.SessionId = sessionId;
            }           
            return loginResult;
        }

        public void Logout()
        {
            this._Client.Close();
        }

        public string GetData(int value)
        {
            throw new NotImplementedException();
        }

        public FunctionTree GetFunctionTree()
        {
            try
            {
                FunctionTree tree = UserDataAccess.BuildFunctionTree(this._Client.userId, this._Client.language);
                return tree;
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "GetFunctionTree.\r\n{0}", ex.ToString());
                return new FunctionTree();
            }
        }

        #region UserAndRoleManager
        public bool ChangePassword(string currentPassword, string newPassword)
        {
            try
            {
                bool isSuccess = UserDataAccess.ChangePassword(this._Client.userId, currentPassword, newPassword);
                return isSuccess;
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "ChangePassword.\r\n{0}", ex.ToString());
                return false;
            }
        }

        public List<AccessPermission> GetAccessPermissions()
        {
            List<AccessPermission> permissions = new List<AccessPermission>();
            return permissions;
        }

        public List<UserData> GetUserData()
        {
            List<UserData> data = UserDataAccess.GetUserData();
            return data;
        }

        public List<RoleData> GetRoles()
        {
            List<RoleData> roles = UserDataAccess.GetRoles();
            return roles;
        }

        public bool UpdateUsers(UserData user, string password, bool isNewUser)
        {
            try
            {
                if (isNewUser)
                {
                    UserDataAccess.AddNewUser(user, password);
                    return true;
                }
                else
                {
                    UserDataAccess.EditUser(user, password);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "UpdateUsers.\r\n{0}", ex.ToString());
                return false;
            }
        }

        public bool DeleteUser(Guid userId)
        {
            try
            {
                return UserDataAccess.DeleteUser(userId);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "ClientServer/DeleteUser.\r\n{0}", ex.ToString());
                return false;
            }
        }

        public AccessPermissionTree GetAccessPermissionTree(int roleId)
        {
            AccessPermissionTree tree = UserDataAccess.GetAccessPermissionTree(roleId);
            return tree;
        }

        public DataPermissionTree GetDataPermissionTree(int roleId)
        {
            DataPermissionTree tree = new DataPermissionTree();
            return tree;
        }

        public bool UpdateRolePermission(int roleId,int editType, string roleName, AccessPermissionTree accessTree, DataPermissionTree dataTree)
        {
            UserDataAccess.UpdateRolePermission(roleId,editType,roleName,accessTree,dataTree);
            return true;
        }
        #endregion

        #region DealingConsole
        public void AbandonQuote(List<Answer> abandQuotations)
        {
            try
            {
                IEnumerable<IGrouping<string, Answer>> query = abandQuotations.GroupBy(P => P.ExchangeCode, P => P);
                foreach (IGrouping<string, Answer> group in query)
                {
                    List<Answer> newAbandQuotations = group.ToList<Answer>();
                    ExchangeSystem exchangeSystem = Manager.ExchangeManager.GetExchangeSystem(newAbandQuotations[0].ExchangeCode);
                    exchangeSystem.AbandonQuote(newAbandQuotations);
            }
            }
            catch (Exception ex)
            {
                Logger.AddEvent(TraceEventType.Error, "ClientService.DealerAbandonQuoteBack error:\r\n{0}", ex.ToString());
            }
        }

        public void SendQuotePrice(List<Answer> sendQuotations)
        {
            try
            {
                IEnumerable<IGrouping<string, Answer>> query = sendQuotations.GroupBy(P => P.ExchangeCode, P => P);
                foreach (IGrouping<string, Answer> group in query)
                {
                    List<Answer> newSendQuotations = group.ToList<Answer>();
                    ExchangeSystem exchangeSystem = Manager.ExchangeManager.GetExchangeSystem(newSendQuotations[0].ExchangeCode);
                    exchangeSystem.Answer(newSendQuotations);
                }
            }
            catch (Exception ex)
            {
                Logger.AddEvent(TraceEventType.Error, "ClientService.DealerConfirmedQuoteBack error:\r\n{0}", ex.ToString());
            }
        }

        #endregion


    }
}
