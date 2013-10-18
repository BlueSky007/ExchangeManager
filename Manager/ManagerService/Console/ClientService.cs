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
                User user = UserDataAccess.Login(userName, password,out dataPermissions);
                if (user.UserId != Guid.Empty)
                {
                    Dictionary<string, List<Guid>> accountPermissions = new Dictionary<string, List<Guid>>();
                    Dictionary<string, List<Guid>> instrumentPermissions = new Dictionary<string, List<Guid>>();
                    foreach (ExchangeSystemSetting item in Manager.ManagerSettings.ExchangeSystems)
                    {
                        List<Guid> accountMemberIds = new List<Guid>();
                        List<Guid> instrumentMemberIds = new List<Guid>();
                        List<DataPermission> systemPermissions = dataPermissions.FindAll(delegate(DataPermission data)
                        {
                            return data.ExchangeSystemCode == item.Code;
                        });
                        if (systemPermissions.Count>0)
                        {
                            foreach (DataPermission permission in systemPermissions)
                            {
                                List<Guid> memberIds = new List<Guid>();
                                memberIds.AddRange(ExchangeData.GetMemberIds(permission.ExchangeSystemCode, permission.DataObjectId));
                                if (permission.DataObjectType == DataObjectType.Account)
                                {
                                    accountMemberIds.AddRange(memberIds);
                                }
                                else if(permission.DataObjectType == DataObjectType.Instrument)
                                {
                                    instrumentMemberIds.AddRange(memberIds);
                                }
                            }
                            accountPermissions.Add(item.Code, accountMemberIds);
                            instrumentPermissions.Add(item.Code, instrumentMemberIds);
                        }
                    }
                    string sessionId = OperationContext.Current.SessionId;
                    IClientProxy clientProxy = OperationContext.Current.GetCallbackChannel<IClientProxy>();
                    this._Client = Manager.ClientManager.AddClient(oldSessionId, sessionId, user, clientProxy, language,accountPermissions,instrumentPermissions);

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
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "ManagerService.Console.ClientService/GetFunctionTree.\r\n{0}", ex.ToString());
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
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "ManagerService.Console.ClientService/ChangePassword.\r\n{0}", ex.ToString());
                return false;
            }
        }

        public List<AccessPermission> GetAccessPermissions()
        {
            try
            {
                List<AccessPermission> permissions = UserDataAccess.GetAccessPermissions(this._Client.userId, this._Client.language);
                return permissions;
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "ManagerService.Console.ClientService/GetAccessPermission.\r\n{0}", ex.ToString());
                return null;
            }
        }

        public List<UserData> GetUserData()
        {
            try
            {
                List<UserData> data = UserDataAccess.GetUserData();
                return data;
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "ManagerService.Console.ClientService/GetUserData.\r\n{0}", ex.ToString());
                return null;
            }
        }

        public List<RoleData> GetRoles()
        {
            try
            {
                List<RoleData> roles = UserDataAccess.GetRoles(this._Client.language.ToString());
                return roles;
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "ManagerService.Console.ClientService/GetRoles.\r\n{0}", ex.ToString());
                return null;
            }
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
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "ManagerService.Console.ClientService/UpdateUsers.\r\n{0}", ex.ToString());
                return false;
            }
        }

        public bool DeleteUser(Guid userId)
        {
            try
            {
                if (userId == this._Client.userId)
                {
                    return false;
                }
                else
                {
                    return UserDataAccess.DeleteUser(userId);
                }
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "ManagerService.Console.ClientService/DeleteUser.\r\n{0}", ex.ToString());
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
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "ManagerService.Console.ClientService/GetAllPermission.\r\n{0}", ex.ToString());
                return null;
            }
        }

        public bool UpdateRole(RoleData role, bool isNewRole)
        {
            try
            {
                bool isSuccess = false;
                if (isNewRole)
                {
                    isSuccess = UserDataAccess.AddNewRole(role);
                }
                else
                {
                    isSuccess = UserDataAccess.EditRole(role);
                }
                return isSuccess;
                
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "ManagerService.Console.ClientService/UpdateRole.\r\n{0}", ex.ToString());
                return false;
            }
        }

        public bool DeleteRole(int roleId)
        {
            try
            {
                if (this._Client.user.IsInRole(roleId))
                {
                    return false;
                }
                else
                {
                    return UserDataAccess.DeleteRole(roleId, this._Client.userId);
                }
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "ManagerService.Console.ClientService/DeleteRole.\r\n{0}", ex.ToString());
                return false;
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

        public TransactionError CancelPlace(Guid transactionId, CancelReason cancelReason)
        {
            TransactionError transactionError = TransactionError.OK;
            string exchangeCode = "WF01";
            try
            {
                ExchangeSystem exchangeSystem = Manager.ExchangeManager.GetExchangeSystem(exchangeCode);
                transactionError = exchangeSystem.CancelPlace(transactionId,cancelReason);
            }
            catch (Exception ex)
            {
                Logger.AddEvent(TraceEventType.Error, "ClientService.CancelPlace error:\r\n{0}", ex.ToString());
            }
            return transactionError;
        }
        #endregion


    }
}
