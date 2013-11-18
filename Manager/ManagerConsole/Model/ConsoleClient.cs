using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Xml;
using Manager.Common;
using ManagerConsole.ViewModel;
using Manager.Common.QuotationEntities;

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
        private Function _AccessPermissions;

        public MessageClient MessageClient
        {
            get { return this._MessageClient; }
        }

        public void Login(Action<LoginResult, string> endLogin, string server, int port, string userName, string password, Language language, string oldSessionId = null)
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
                try
                {
                    LoginResult result = this._ServiceProxy.EndLogin(ar);
                    if (result.Succeeded)
                    {
                        this._SessionId = result.SessionId;
                        this.GetAccessPermissions(this.EndGetPermissions);
                    }
                    endLogin(result, null);
                }
                catch (EndpointNotFoundException ex)
                {
                    endLogin(null, "Server not found: " + ex.Message);
                }
                catch (CommunicationException)
                {
                    endLogin(null, "Invalid user name or password.");
                }
                catch (Exception ex)
                {
                    endLogin(null, ex.Message);
                    Logger.TraceEvent(TraceEventType.Error, "Login failed: \r\n{0}", ex.Message);
                }
            }, null);
        }

        public bool HasPermission(AccessPermission function)
        {
            return this._AccessPermissions.HasPermission(function);
        }

        public FunctionTree GetFunctionTree()
        {
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

        public void SaveLayout(string layout, string content,string layoutName)
        {
            try
            {
                this._ServiceProxy.SaveLayout(layout, content, layoutName);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "SaveLayout.\r\n{0}", ex.ToString());
            }
        }

        public void LoadLayout(string layoutName,Action<List<string>> EndLoad)
        {
            try
            {
                this._ServiceProxy.BeginLoadLayout(layoutName, delegate(IAsyncResult ar)
                {
                    List<string> layouts = this._ServiceProxy.EndLoadLayout(ar);
                    EndLoad(layouts);
                }, null);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "LoadLayout.\r\n{0}", ex.ToString());
            }
        }

        #region UserManager
        public bool ChangePassword(string currentPassword, string newPassword,Guid userId )
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
            }, null);
        }

        private void GetAccessPermissions(Action<Dictionary<string, string>> endGetPermissions)
        {
            try
            {

                this._ServiceProxy.BeginGetAccessPermissions(delegate(IAsyncResult ar)
                {
                    Dictionary<string, string> permissions = this._ServiceProxy.EndGetAccessPermissions(ar);
                    endGetPermissions(permissions);
                }, null);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "GetAccessPermissions.\r\n{0}", ex.ToString());
            }
        }

        private void EndGetPermissions(Dictionary<string,string> permissions)
        {
            try
            {
                this._AccessPermissions = new Function();
                this._AccessPermissions.FunctionPermissions = permissions;
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "EndGetPermissions.\r\n{0}", ex.ToString());
            }
        }

        public List<RoleData> GetRoles()
        {
            return this._ServiceProxy.GetRoles();
        }

        public List<RoleFunctonPermission> GetAllFunctionPermission()
        {
            return this._ServiceProxy.GetAllFunctionPermission();
        }

        public List<RoleDataPermission> GetAllDataPermissions()
        {
            return this._ServiceProxy.GetAllDataPermission();
        }
       
        public void UpdateUser(UserData user, string password, bool isNewUser, Action<bool> EndUpdateUser)
        {
            this._ServiceProxy.BeginUpdateUsers(user, password, isNewUser, delegate(IAsyncResult ar)
            {
                bool isSuccess = this._ServiceProxy.EndUpdateUsers(ar);
                EndUpdateUser(isSuccess);
            }, null);
        }

        public void DeleteUser(Guid userId, Action<bool,Guid> EndDeleteUser)
        {
            this._ServiceProxy.BeginDeleteUser(userId, delegate(IAsyncResult ar)
            {
                bool isSuccess = this._ServiceProxy.EndDeleteUser(ar);
                EndDeleteUser(isSuccess,userId);
            }, null);
        }

        public void UpdateRole(RoleData role, bool isNewRole, Action<bool> endUpdateRole)
        {
            this._ServiceProxy.BeginUpdateRole(role, isNewRole, delegate(IAsyncResult ar)
            {
                bool isSuccess = this._ServiceProxy.EndUpdateRole(ar);
                endUpdateRole(isSuccess);
            }, null);
        }

        public void DeleteRole(int roleId, Action<bool> endDelete)
        {
            this._ServiceProxy.BeginDeleteRole(roleId, delegate(IAsyncResult ar)
            {
                bool isSuccess = this._ServiceProxy.EndDeleteRole(ar);
                endDelete(isSuccess);
            }, null);
        }
        #endregion

        #region DealingConsole
        public void AbandonQuote(List<Answer> abandonQuotePrices)
        {
            this._ServiceProxy.AbandonQuote(abandonQuotePrices);
        }

        public void SendQuotePrice(List<Answer> sendQuotePrices)
        {
            this._ServiceProxy.SendQuotePrice(sendQuotePrices);
        }

        public void AcceptPlace(Transaction tran,LogOrder logEntity,Action<Transaction, TransactionError> EndAcceptPlace)
        {
            this._ServiceProxy.BeginAcceptPlace(tran.Id,logEntity,delegate(IAsyncResult result)
            {
                TransactionError transactionError = this._ServiceProxy.EndAcceptPlace(result);
                EndAcceptPlace(tran, transactionError);
            }, null);
        }

        public void CancelPlace(Transaction tran, CancelReason cancelReason, Action<Transaction,TransactionError> EndCancelPlace)
        {
            this._ServiceProxy.BeginCancelPlace(tran.Id, cancelReason, delegate(IAsyncResult result) 
            {
                TransactionError transactionError = this._ServiceProxy.EndCancelPlace(result);
                EndCancelPlace(tran,transactionError);
            }, null);
        }

        public void Execute(Transaction tran, string buyPrice, string sellPrice, decimal lot, Guid orderId, LogOrder logEntity, Action<Transaction,TransactionError> EndExecute)
        {
            this._ServiceProxy.BeginExecute(tran.Id, buyPrice, sellPrice, lot, orderId, logEntity, delegate(IAsyncResult result)
            {
                TransactionError transactionError = this._ServiceProxy.EndExecute(result);
                EndExecute(tran,transactionError);
            }, null);
        }

        public void Cancel(Transaction tran, CancelReason cancelReason, LogOrder logEntity, Action<Transaction, TransactionError> EndCancel)
        {
            this._ServiceProxy.BeginCancel(tran.Id, cancelReason, logEntity, delegate(IAsyncResult result)
            {
                TransactionError transactionError = this._ServiceProxy.EndCancel(result);
                EndCancel(tran, transactionError);
            }, null);
        }

        public void ResetHit(Guid[] orderIds)
        {
            this._ServiceProxy.ResetHit(orderIds);
        }

        public AccountInformation GetAcountInfo(Guid transactionId)
        {
            return this._ServiceProxy.GetAcountInfo(transactionId);
        }
        #endregion

        #region Log Audit
        public void GetQuoteLogData(DateTime fromData, DateTime toData, LogType logType, Action<List<LogQuote>> EndGetQuoteLogData)
        {
            this._ServiceProxy.BeginGetQuoteLogData(fromData, toData, logType, delegate(IAsyncResult result)
            {
                List<LogQuote> logQuotes = this._ServiceProxy.EndGetQuoteLogData(result);
                EndGetQuoteLogData(logQuotes);
            }, null);
        }

        public void GetLogOrderData(DateTime fromDate, DateTime toDate, LogType logType, Action<List<LogOrder>> EndGetLogOrderData)
        {
            this._ServiceProxy.BeginGetLogOrderData(fromDate, toDate, logType, delegate(IAsyncResult result)
            {
                List<LogOrder> logOrders = this._ServiceProxy.EndGetLogOrderData(result);
                EndGetLogOrderData(logOrders);
            }, null);
        }

        public void GetLogPriceData(DateTime fromDate, DateTime toDate, LogType logType, Action<List<LogPrice>> EndGetLogPriceData)
        {
            this._ServiceProxy.BeginGetLogPriceData(fromDate, toDate, logType, delegate(IAsyncResult result)
            {
                List<LogPrice> logPrices = this._ServiceProxy.EndGetLogPriceData(result);
                EndGetLogPriceData(logPrices);
            }, null);
        }

        public void GetLogSourceChangeData(DateTime fromDate, DateTime toDate, LogType logType, Action<List<LogSourceChange>> EndGetLogSourceChangeData)
        {
            this._ServiceProxy.BeginGetLogSourceChangeData(fromDate, toDate, logType, delegate(IAsyncResult result)
            {
                List<LogSourceChange> logSourceChanges = this._ServiceProxy.EndGetLogSourceChangeData(result);
                EndGetLogSourceChangeData(logSourceChanges);
            }, null);
        }
        
        #endregion


        #region Quotation
        public void GetConfigMetadata(Action<ConfigMetadata> setMetadata)
        {
            this._ServiceProxy.BeginGetConfigMetadata(delegate(IAsyncResult ar)
            {
                ConfigMetadata metaData = this._ServiceProxy.EndGetConfigMetadata(ar);
                setMetadata(metaData);
            }, null);
        }
        #endregion

    }
}
