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
            LoginResult loginResult = new LoginResult();
            List<DataPermission> dataPermissions = new List<DataPermission>();
            try
            {
                Guid userId = UserDataAccess.Login(userName, password,out dataPermissions);
                if (userId != Guid.Empty)
                {
                    Dictionary<string, List<Guid>> accountPermissions = new Dictionary<string, List<Guid>>();
                    Dictionary<string, List<Guid>> instrumentPermissions = new Dictionary<string, List<Guid>>();
                    foreach (DataPermission data in dataPermissions)
                    {
                        List<Guid> memberIds = ExchangeData.GetMemberIds(data.ExchangeSystemCode, data.DataObjectId);
                        if (data.DataObjectType ==  DataObjectType.Account)
                        {
                            accountPermissions.Add(data.ExchangeSystemCode, memberIds);
                        }
                        else
                        {
                            instrumentPermissions.Add(data.ExchangeSystemCode, memberIds);
                        }
                    }
                    string sessionId = OperationContext.Current.SessionId;
                    IClientProxy clientProxy = OperationContext.Current.GetCallbackChannel<IClientProxy>();
                    this._Client = Manager.ClientManager.AddClient(oldSessionId, sessionId, userId, clientProxy, language,accountPermissions,instrumentPermissions);

                    OperationContext.Current.Channel.Faulted += this._Client.Channel_Broken;
                    OperationContext.Current.Channel.Closed += this._Client.Channel_Broken;
                    loginResult.SessionId = sessionId;
                }
                else
                {
                    OperationContext.Current.Channel.Abort();
                }
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(TraceEventType.Error, "userName:{0}, login failed:\r\n{1}", userName, ex.ToString());
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
            List<RoleData> roles = UserDataAccess.GetRoles(this._Client.language.ToString());
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

        public RoleData GetAllPermission()
        {
            try
            {
                RoleData data = UserDataAccess.GetAllPermissions(Manager.ManagerSettings.ExchangeSystems, this._Client.language.ToString());
                return data;
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "GetAllPermission.\r\n{0}", ex.ToString());
                return null;
            }
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

        public TransactionError AcceptPlace(Guid transactionId)
        {
            TransactionError transactionError = TransactionError.OK;
            string exchangeCode = "WF01";
            try
            {
                ExchangeSystem exchangeSystem = Manager.ExchangeManager.GetExchangeSystem(exchangeCode);
                transactionError = exchangeSystem.AcceptPlace(transactionId);

                //string objectIDs = "AcceptPlace";
                //Write Log
            }
            catch (Exception ex)
            {
                Logger.AddEvent(TraceEventType.Error, "ClientService.AcceptPlace error:\r\n{0}", ex.ToString());
            }
            return transactionError;
        }
        #endregion


    }
}
