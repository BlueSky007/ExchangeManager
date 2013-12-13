using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceModel;
using Manager.Common;
using Manager.Common.QuotationEntities;
using Manager.Common.LogEntities;
using Manager.Common.ReportEntities;
using iExchange.Common.Manager;
using System.Xml;

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

        public void GetInitializeData(Action<List<InitializeData>> EndGetInitializeData)
        {
            try
            {
                this._ServiceProxy.BeginGetInitializeData(delegate(IAsyncResult result)
                { 
                    List<InitializeData> initializeDatas = this._ServiceProxy.EndGetInitializeData(result);
                    EndGetInitializeData(initializeDatas);
                }, null);
            }
            catch(Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "GetInitializeData.\r\n{0}", ex.ToString());
            }
        }

        public bool HasPermission(ModuleCategoryType category, ModuleType module, string operationCode)
        {
            return this._AccessPermissions.HasPermission(category,module,operationCode);
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

        public void Updatetest()
        {
            try
            {
                this._ServiceProxy.Updatetest();
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "ActionName.\r\n{0}", ex.ToString());
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

        private void GetAccessPermissions(Action<Dictionary<string, Tuple<string, bool>>> endGetPermissions)
        {
            try
            {

                this._ServiceProxy.BeginGetAccessPermissions(delegate(IAsyncResult ar)
                {
                    Dictionary<string, Tuple<string, bool>> permissions = this._ServiceProxy.EndGetAccessPermissions(ar);
                    endGetPermissions(permissions);
                }, null);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "GetAccessPermissions.\r\n{0}", ex.ToString());
            }
        }

        private void EndGetPermissions(Dictionary<string, Tuple<string, bool>> permissions)
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

        #region Report
        public void GetOrderByInstrument(Guid instrumentId, Guid accountGroupId,OrderType orderType,
            bool isExecute, DateTime fromDate, DateTime toDate,Action<List<OrderQueryEntity>> EndGetOrderByInstrument)
        {
            this._ServiceProxy.BeginGetOrderByInstrument(instrumentId, accountGroupId, orderType, isExecute, fromDate, toDate, delegate(IAsyncResult result)
            {
                List<OrderQueryEntity> queryOrders = this._ServiceProxy.EndGetOrderByInstrument(result);
                EndGetOrderByInstrument(queryOrders);
            }, null);
        }

        public void GetGroupNetPosition(Action<List<AccountGroupGNP>> EndGetGroupNetPosition)
        {
            this._ServiceProxy.BeginGetGroupNetPosition(delegate(IAsyncResult result) 
            {
                List<iExchange.Common.Manager.AccountGroupGNP> accountGroupGNPs = this._ServiceProxy.EndGetGroupNetPosition(result);
                EndGetGroupNetPosition(accountGroupGNPs);
            }, null);
        }

        public void GetInstrumentSummary(bool isGroupByOriginCode, string[] blotterCodeSelecteds, Action<List<OpenInterestSummary>> EndGetInstrumentSummary)
        {
            this._ServiceProxy.BeginGetInstrumentSummary(isGroupByOriginCode, blotterCodeSelecteds, delegate(IAsyncResult result)
            {
                List<iExchange.Common.Manager.OpenInterestSummary> openInterestSummarys = this._ServiceProxy.EndGetInstrumentSummary(result);
                EndGetInstrumentSummary(openInterestSummarys);
            }, null);
        }

        public void GetAccountSummary(Guid instrumentId,string[] blotterCodeSelecteds, Action<Guid,List<OpenInterestSummary>> EndGetAccountSummary)
        {
            this._ServiceProxy.BeginGetAccountSummary(instrumentId,blotterCodeSelecteds, delegate(IAsyncResult result)
            {
                List<iExchange.Common.Manager.OpenInterestSummary> openInterestSummarys = this._ServiceProxy.EndGetAccountSummary(result);
                EndGetAccountSummary(instrumentId,openInterestSummarys);
            }, null);
        }

        public void GetOrderSummary(ManagerConsole.ViewModel.OpenInterestSummary accountSumamry, string[] blotterCodeSelecteds, Action<ManagerConsole.ViewModel.OpenInterestSummary, List<OpenInterestSummary>> EndGetOrderSummary)
        {
            Guid accountId = accountSumamry.Id;
            Guid instrumentId = accountSumamry.InstrumentId;
            this._ServiceProxy.BeginGetOrderSummary(accountId,instrumentId,accountSumamry.AccountType,blotterCodeSelecteds, delegate(IAsyncResult result)
            {
                List<iExchange.Common.Manager.OpenInterestSummary> openInterestSummarys = this._ServiceProxy.EndGetOrderSummary(result);
                EndGetOrderSummary(accountSumamry,openInterestSummarys);
            }, null);
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

        public void GetLogSettingData(DateTime fromDate, DateTime toDate, LogType logType, Action<List<LogSetting>> EndGetLogSettingData)
        {
            this._ServiceProxy.BeginGetLogSettingData(fromDate, toDate, logType, delegate(IAsyncResult result)
            {
                List<LogSetting> logSettings = this._ServiceProxy.EndGetLogSettingData(result);
                EndGetLogSettingData(logSettings);
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
                App.MainWindow.Dispatcher.BeginInvoke((Action)delegate()
                {
                    ConfigMetadata metaData = this._ServiceProxy.EndGetConfigMetadata(ar);
                    setMetadata(metaData);
                });
            }, null);
        }


        public void AddMetadataObject(IMetadataObject metadataObject, Action<int> SetAddObjectId)
        {
            this._ServiceProxy.BeginAddMetadataObject(metadataObject, delegate(IAsyncResult ar)
            {
                int objectId = this._ServiceProxy.EndAddMetadataObject(ar);
                SetAddObjectId(objectId);
            }, null);
        }

        public void AddInstrument(InstrumentData instrumentData, Action<int> SetInstrumentId)
        {
            this._ServiceProxy.BeginAddInstrument(instrumentData, delegate(IAsyncResult ar)
            {
                int instrumentId = this._ServiceProxy.EndAddInstrument(ar);
                SetInstrumentId(instrumentId);
            }, null);
        }


        public void UpdateMetadataObject(MetadataType type, int objectId, Dictionary<string, object> fieldAndValues, Action<bool> NotifyResult)
        {
            this._ServiceProxy.BeginUpdateMetadataObject(type, objectId, fieldAndValues, delegate(IAsyncResult ar)
            {
                bool result = this._ServiceProxy.EndUpdateMetadataObject(ar);
                App.MainWindow.Dispatcher.BeginInvoke((Action<bool>)delegate(bool success)
                {
                    NotifyResult(success);
                }, result);
            }, null);
        }

        public void UpdateMetadataObjects(UpdateData[] updateDatas, Action<bool> NotifyResult)
        {
            this._ServiceProxy.BeginUpdateMetadataObjects(updateDatas, delegate(IAsyncResult ar)
            {
                bool result = this._ServiceProxy.EndUpdateMetadataObjects(ar);
                App.MainWindow.Dispatcher.BeginInvoke((Action<bool>)delegate(bool success)
                {
                    NotifyResult(success);
                }, result);
            }, null);
        }

        public void UpdateMetadataObjectField(MetadataType type, int objectId, string field, object value, Action<bool> NotifyResult)
        {
            this._ServiceProxy.BeginUpdateMetadataObjectField(type, objectId, field, value, delegate(IAsyncResult ar)
            {
                bool result = this._ServiceProxy.EndUpdateMetadataObjectField(ar);
                App.MainWindow.Dispatcher.BeginInvoke((Action<bool>)delegate(bool success)
                {
                    NotifyResult(success);
                }, result);
            }, null);
        }

        public void DeleteMetadataObject(MetadataType type, int objectId, Action<bool> NotifyResult)
        {
            this._ServiceProxy.BeginDeleteMetadataObject(type, objectId, delegate(IAsyncResult ar)
            {
                bool deleted = this._ServiceProxy.EndDeleteMetadataObject(ar);
                NotifyResult(deleted);
            }, null);
        }

        public void SendQuotation(int instrumentSourceRelationId, double ask, double bid)
        {
            this._ServiceProxy.BeginSendQuotation(instrumentSourceRelationId, ask, bid, delegate(IAsyncResult ar)
            {
                this._ServiceProxy.EndSendQuotation(ar);
            }, null);
        }

        public void UpdateExchangeQuotation(QuotePolicyDetailSet set)
        {
            this._ServiceProxy.BeginUpdateQuotationPolicy(set, delegate(IAsyncResult ar)
            {
                this._ServiceProxy.EndUpdateQuotationPolicy(ar);
            }, null);
        }

        public void SetQuotePolicyDetail(Guid relationId, QuotePolicyDetailsSetAction action, int changeValue)
        {
            this._ServiceProxy.BeginSetQuotationPolicyDetail(relationId, action, changeValue, delegate(IAsyncResult ar)
            {
                this._ServiceProxy.EndSetQuotationPolicyDetail(ar);
            }, null);
        }

        public void GetQuotePolicyRelation(Action<List<QuotePolicyRelation>> GetDataResult)
        {
            this._ServiceProxy.BeginGetQuotePolicyRelation(delegate(IAsyncResult ar)
            {
                List<QuotePolicyRelation> VMRelation = this._ServiceProxy.EndGetQuotePolicyRelation(ar);
                GetDataResult(VMRelation);
            }, null);
        }

        public void AddNewRelation(Guid id, string code, List<int> instruments,Action<bool> callBack)
        {
            this._ServiceProxy.BeginAddNewRelation(id, code, instruments, delegate(IAsyncResult ar)
            {
                bool result = this._ServiceProxy.EndAddNewRelation(ar);
                callBack(result);
            }, null);
        }
        #endregion


        public void SwitchDefaultSource(SwitchRelationBooleanPropertyMessage switchMessage)
        {
            this._ServiceProxy.BeginSwitchDefaultSource(switchMessage, delegate(IAsyncResult ar)
            {
                this._ServiceProxy.EndSwitchDefaultSource(ar);
            }, null);
        }
    }
}
