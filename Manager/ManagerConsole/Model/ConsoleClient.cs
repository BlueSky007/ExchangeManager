using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Xml;
using System.Threading;
using Manager.Common;
using Manager.Common.QuotationEntities;
using Manager.Common.LogEntities;
using Manager.Common.ReportEntities;
using iExchange.Common.Manager;
using Manager.Common.Settings;
using TransactionError = iExchange.Common.TransactionError;
using CancelReason = iExchange.Common.CancelReason;
using SoundSetting = Manager.Common.Settings.SoundSetting;
using BlotterSelection = Manager.Common.ReportEntities.BlotterSelection;
using iExchange.Common;

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

        private string _Server;
        private int _Port;
        private IClientService _ServiceProxy;

        private User _User;
        private MessageClient _MessageClient = null;
        private string _SessionId;
        private Timer _RecoverTimer;
        private int _ReocoverTimes;
        private bool _IsLoggedIn = false;

        public MessageClient MessageClient
        {
            get { return this._MessageClient; }
        }

        public bool IsLoggedIn { get { return this._IsLoggedIn; } }
        public User User { get { return this._User; } }

        public void Login(Action<LoginResult, string> endLogin, string server, int port, string userName, string password, Language language)
        {
            if (this._MessageClient == null)
            {
                this._MessageClient = new MessageClient();
            }

            this._Server = server;
            this._Port = port;
            this.CreateChannel();
            this._ServiceProxy.BeginLogin(userName, password, language, delegate(IAsyncResult ar)
            {
                try
                {
                    LoginResult result = this._ServiceProxy.EndLogin(ar);
                    this._User = result.User;
                    Principal.Instance.User = result.User;
                    if (result.Succeeded)
                    {
                        App.MainFrameWindow.StatusBar.ShowUserConnectionState(ConnectionState.Connected);
                        App.MainFrameWindow.StatusBar.ShowLoginUser(this._User.UserName);
                        this._IsLoggedIn = true;
                        ICommunicationObject communicationObject = this._ServiceProxy as ICommunicationObject;
                        communicationObject.Faulted += communicationObject_Faulted;
                        this._SessionId = result.SessionId;
                        if (this._User.IsAdmin)
                        {
                            Principal.Instance.UserPermission = new UserPermission();
                        }
                        else
                        {
                            this.GetAccessPermissions(this.EndGetPermissions);
                        }
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

        private void CreateChannel()
        {
            EndpointAddress address = new EndpointAddress(string.Format("net.tcp://{0}:{1}/Service", this._Server, this._Port));
            NetTcpBinding binding = new NetTcpBinding(SecurityMode.None) { MaxReceivedMessageSize = Int32.MaxValue };
            binding.OpenTimeout = TimeSpan.FromSeconds(20);
            binding.SendTimeout = TimeSpan.FromSeconds(60);
            binding.MaxReceivedMessageSize = int.MaxValue;
            DuplexChannelFactory<IClientService> factory = new DuplexChannelFactory<IClientService>(this._MessageClient, binding, address);

            foreach (OperationDescription operation in factory.Endpoint.Contract.Operations)
            {
                operation.Behaviors.Find<DataContractSerializerOperationBehavior>().MaxItemsInObjectGraph = int.MaxValue;
            }
            this._ServiceProxy = factory.CreateChannel();
            Logger.TraceEvent(TraceEventType.Information, "CreateChannel Created.");
        }

        private void communicationObject_Faulted(object sender, EventArgs e)
        {
            try
            {
                if (this._IsLoggedIn)
                {
                    this._ReocoverTimes = 1;
                    if (this._RecoverTimer == null)
                    {
                        this._RecoverTimer = new Timer(this.RecoverConnection);
                    }
                    App.MainFrameWindow.StatusBar.ShowUserConnectionState(ConnectionState.Connecting);
                    this._RecoverTimer.Change(50, Timeout.Infinite);
                    Logger.AddEvent(TraceEventType.Warning, "ConsoleClient.communicationObject_Faulted try recover.");
                }
                else
                {
                    App.MainFrameWindow.StatusBar.ShowUserConnectionState(ConnectionState.Disconnected);
                }
                this._IsLoggedIn = false;
            }
            catch (Exception exception)
            {
                Logger.TraceEvent(TraceEventType.Error, "ConsoleClient.communicationObject_Faulted exception\r\n{0}", exception);
            }
  
        }

        private void RecoverConnection(object state)
        {
            try
            {
                this.CreateChannel();
                this._ServiceProxy.BeginRecoverConnection(this._SessionId, delegate(IAsyncResult result)
                {
                    try
                    {
                        bool recovered = this._ServiceProxy.EndRecoverConnection(result);
                        this._IsLoggedIn = recovered;
                        if (recovered)
                        {
                            ICommunicationObject communicationObject = this._ServiceProxy as ICommunicationObject;
                            communicationObject.Faulted += communicationObject_Faulted;
                            Logger.AddEvent(TraceEventType.Information, "ConsoleClient.RecoverConnection Success");
                        }
                        else
                        {
                            Logger.AddEvent(TraceEventType.Information, "ConsoleClient.RecoverConnection Failed");
                        }
                        App.MainFrameWindow.StatusBar.ShowUserConnectionState(recovered ? ConnectionState.Connected : ConnectionState.Disconnected);

                    }
                    catch (Exception exception)
                    {
                        this.RetryRecoverConnection(exception);
                    }
                }, null);
            }
            catch(Exception exception)
            {
                this.RetryRecoverConnection(exception);
            }
        }

        private void RetryRecoverConnection(Exception exception)
        {
            if (this._ReocoverTimes++ < 20)
            {
                this._RecoverTimer.Change(1000, Timeout.Infinite);
                Logger.AddEvent(TraceEventType.Warning, "ConsoleClient.RecoverConnection failed, try again.\r\n{0}", exception);
                App.MainFrameWindow.StatusBar.ShowStatusText(string.Format("Recovering connection({0})......", this._ReocoverTimes));
            }
            else
            {
                App.MainFrameWindow.StatusBar.ShowUserConnectionState(ConnectionState.Disconnected);
                Logger.AddEvent(TraceEventType.Warning, "ConsoleClient.RecoverConnection failed\r\n{0}", exception);
                App.MainFrameWindow.StatusBar.ShowStatusText("Recover connection failed.");
            }
        }

        public void Logout()
        {
            if (this._IsLoggedIn)
            {
                this._IsLoggedIn = false;
                this._ServiceProxy.Logout();
                App.MainFrameWindow.StatusBar.ShowUserConnectionState(ConnectionState.Disconnected);
                this._MessageClient.Suspend();
            }
        }

        public void LoadSettingsParameters(Action<SettingsParameter> EndLoadSettingsParameters)
        {
            try
            {
                this._ServiceProxy.BeginLoadSettingsParameters(delegate(IAsyncResult result)
                {
                    SettingsParameter settingsParameter = this._ServiceProxy.EndLoadSettingsParameters(result);
                    EndLoadSettingsParameters(settingsParameter);
                }, null);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "LoadSettingsParameters.\r\n{0}", ex.ToString());
            }
        }

        public void GetInitializeData(Action<InitializeData> processInitializeData)
        {
            try
            {
                this._ServiceProxy.BeginGetInitializeData(delegate(IAsyncResult result)
                {
                    try
                    {
                        InitializeData initializeData = this._ServiceProxy.EndGetInitializeData(result);
                        processInitializeData(initializeData);
                        this._MessageClient.StartMessageProcess();
                    }
                    catch (Exception exception)
                    {
                        Logger.TraceEvent(TraceEventType.Error, "EndGetInitializeData exception\r\n{0}", exception);
                    }
  
                }, null);
            }
            catch(Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "GetInitializeData.\r\n{0}", ex.ToString());
            }
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

        public void GetAccessPermissions(Action<Dictionary<string,List<FuncPermissionStatus>>> endGetPermissions)
        {
            try
            {

                this._ServiceProxy.BeginGetAccessPermissions(delegate(IAsyncResult ar)
                {
                    Dictionary<string, List<FuncPermissionStatus>> permissions = this._ServiceProxy.EndGetAccessPermissions(ar);
                    endGetPermissions(permissions);
                }, null);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "GetAccessPermissions.\r\n{0}", ex.ToString());
            }
        }

        public void EndGetPermissions(Dictionary<string, List<FuncPermissionStatus>> permissions)
        {
            try
            {
                Principal.Instance.UserPermission = new UserPermission();
                Principal.Instance.UserPermission.FunctionPermissions = permissions;
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "EndGetPermissions.\r\n{0}", ex.ToString());
            }
        }

        public void GetRoles(Action<List<RoleData>> endGetRoles)
        {
            this._ServiceProxy.BeginGetRoles(delegate(IAsyncResult ar)
            {
                List<RoleData> rolesDatas = this._ServiceProxy.EndGetRoles(ar);
                App.MainFrameWindow.Dispatcher.BeginInvoke((Action<List<RoleData>>)delegate(List<RoleData> roles)
                {
                    endGetRoles(roles);
                }, rolesDatas);
            }, null);
        }

        public void GetAllFunctionPermission(Action<List<RoleFunctonPermission>> endGetAllFunctionPermission)
        {
            this._ServiceProxy.BeginGetAllFunctionPermission(delegate(IAsyncResult ar)
            {
                List<RoleFunctonPermission> functonPermissions = this._ServiceProxy.EndGetAllFunctionPermission(ar);
                App.MainFrameWindow.Dispatcher.BeginInvoke((Action<List<RoleFunctonPermission>>)delegate(List<RoleFunctonPermission> permissions)
                {
                    endGetAllFunctionPermission(permissions);
                }, functonPermissions);
            }, null);
        }

        public void GetAllDataPermissions(Action<List<RoleDataPermission>> endGetAllDataPermissions)
        {
            this._ServiceProxy.BeginGetAllDataPermission(delegate(IAsyncResult ar)
            {
                List<RoleDataPermission> dataPermissions = this._ServiceProxy.EndGetAllDataPermission(ar);
                App.MainFrameWindow.Dispatcher.BeginInvoke((Action<List<RoleDataPermission>>)delegate(List<RoleDataPermission> permissions)
                {
                    endGetAllDataPermissions(permissions);
                }, dataPermissions);
            }, null);
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

        public void Execute(Transaction tran, string buyPrice, string sellPrice, decimal lot, Guid orderId, LogOrder logEntity, Action<Transaction, TransactionResult> EndExecute)
        {
            this._ServiceProxy.BeginExecute(tran.Id, buyPrice, sellPrice, lot, orderId, logEntity, delegate(IAsyncResult result)
            {
                TransactionResult tranResult = this._ServiceProxy.EndExecute(result);
                EndExecute(tran, tranResult);
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

        public void ResetHit(string exchangeCode,Guid[] orderIds)
        {
            this._ServiceProxy.ResetHit(exchangeCode,orderIds);
        }

        public AccountInformation GetAcountInfo(string exchangeCode,Guid transactionId)
        {
            return this._ServiceProxy.GetAcountInfo(exchangeCode,transactionId);
        }

        #endregion

        #region Setting Manager
        public void CopyFromSetting(Guid copyFromUserId,Action<List<SoundSetting>> EndCopyFromSetting)
        {
            this._ServiceProxy.BeginCopyFromSetting(copyFromUserId, delegate(IAsyncResult result) 
            {
                List<SoundSetting> newSoundSettings = this._ServiceProxy.EndCopyFromSetting(result);
                EndCopyFromSetting(newSoundSettings);
            }, null);
        }

        public void LoadParameterDefine(Action<List<ParameterDefine>> EndLoadParameterDefine)
        {
            this._ServiceProxy.BeginLoadParameterDefine(delegate(IAsyncResult result)
            {
                List<ParameterDefine> parameters = this._ServiceProxy.EndLoadParameterDefine(result);
                EndLoadParameterDefine(parameters);
            }, null);
        }

        public void CreateTaskScheduler(TaskScheduler taskScheduler, Action<bool> EndCreateTaskScheduler)
        {
            this._ServiceProxy.BeginCreateTaskScheduler(taskScheduler, delegate(IAsyncResult result)
            {
                bool isCreateSucceed = this._ServiceProxy.EndCreateTaskScheduler(result);
                EndCreateTaskScheduler(isCreateSucceed);
            }, null);
        }

        public void EditorTaskScheduler(TaskScheduler taskScheduler, Action<bool> EndEditorTaskScheduler)
        {
            this._ServiceProxy.BeginEditorTaskScheduler(taskScheduler, delegate(IAsyncResult result)
            {
                bool isOk = this._ServiceProxy.EndEditorTaskScheduler(result);
                EndEditorTaskScheduler(isOk);
            }, null);
        }

        public void EnableTaskScheduler(TaskScheduler taskScheduler)
        {
            this._ServiceProxy.EnableTaskScheduler(taskScheduler);
        }

        public void StartRunTaskScheduler(TaskScheduler taskScheduler, Action EndStartRunTaskScheduler)
        {
            this._ServiceProxy.BeginStartRunTaskScheduler(taskScheduler, delegate(IAsyncResult result)
            {
                App.MainFrameWindow.Dispatcher.BeginInvoke((Action)delegate()
                {
                    this._ServiceProxy.EndStartRunTaskScheduler(result);
                    EndStartRunTaskScheduler();
                });
            }, null);
        }

        public void DeleteTaskScheduler(TaskScheduler taskScheduler, Action<Guid,bool> EndDeleteTaskScheduler)
        {
            this._ServiceProxy.BeginDeleteTaskScheduler(taskScheduler, delegate(IAsyncResult result)
            {
                App.MainFrameWindow.Dispatcher.BeginInvoke((Action)delegate()
                {
                    bool isOk = this._ServiceProxy.EndDeleteTaskScheduler(result);
                    EndDeleteTaskScheduler(taskScheduler.Id,isOk);
                });
            }, null);
        }

        public void GetTaskSchedulersData(Action<List<TaskScheduler>> EndGetTaskSchedulersData)
        {
            this._ServiceProxy.BeginGetTaskSchedulersData(delegate(IAsyncResult result)
            {
                App.MainFrameWindow.Dispatcher.BeginInvoke((Action)delegate()
                {
                    List<TaskScheduler> taskSchedulers = this._ServiceProxy.EndGetTaskSchedulersData(result);
                    EndGetTaskSchedulersData(taskSchedulers);
                });
            }, null);
        }

        public void UpdateManagerSettings(Guid settingId,SettingParameterType type,Dictionary<string, object> fieldAndValues, Action<bool> NotifyResult)
        {
            this._ServiceProxy.BeginUpdateManagerSettings(settingId,type, fieldAndValues, delegate(IAsyncResult ar)
            {
                bool result = this._ServiceProxy.EndUpdateManagerSettings(ar);
                App.MainFrameWindow.Dispatcher.BeginInvoke((Action<bool>)delegate(bool success)
                {
                    NotifyResult(success);
                }, result);
            }, null);
        }
        #endregion

        #region Report
        public void GetOrderByInstrument(string exchangeCode,Guid instrumentId, Guid accountGroupId,OrderType orderType,
            bool isExecute, DateTime fromDate, DateTime toDate,Action<List<OrderQueryEntity>> EndGetOrderByInstrument)
        {
            this._ServiceProxy.BeginGetOrderByInstrument(exchangeCode,instrumentId, accountGroupId, orderType, isExecute, fromDate, toDate, delegate(IAsyncResult result)
            {
                List<OrderQueryEntity> queryOrders = this._ServiceProxy.EndGetOrderByInstrument(result);
                EndGetOrderByInstrument(queryOrders);
            }, null);
        }

        public void GetGroupNetPosition(string exchangeCode,bool showActualQuantity, string[] blotterCodeSelecteds,Action<List<AccountGroupGNP>> EndGetGroupNetPosition)
        {
            this._ServiceProxy.BeginGetGroupNetPosition(exchangeCode,showActualQuantity, blotterCodeSelecteds, delegate(IAsyncResult result) 
            {
                List<iExchange.Common.Manager.AccountGroupGNP> accountGroupGNPs = this._ServiceProxy.EndGetGroupNetPosition(result);
                EndGetGroupNetPosition(accountGroupGNPs);
            }, null);
        }

        public void GetOpenInterestInstrumentSummary(string exchangeCode, bool isGroupByOriginCode, string[] blotterCodeSelecteds, Action<List<OpenInterestSummary>> EndGetOpenInterestInstrumentSummary)
        {
            this._ServiceProxy.BeginGetOpenInterestInstrumentSummary(exchangeCode,isGroupByOriginCode, blotterCodeSelecteds, delegate(IAsyncResult result)
            {
                List<iExchange.Common.Manager.OpenInterestSummary> openInterestSummarys = this._ServiceProxy.EndGetOpenInterestInstrumentSummary(result);
                EndGetOpenInterestInstrumentSummary(openInterestSummarys);
            }, null);
        }

        public void GetOpenInterestAccountSummary(string exchangeCode, Guid instrumentId, string[] blotterCodeSelecteds, Action<Guid, List<OpenInterestSummary>> EndGetOpenInterestAccountSummary)
        {
            this._ServiceProxy.BeginGetOpenInterestAccountSummary(exchangeCode,instrumentId,blotterCodeSelecteds, delegate(IAsyncResult result)
            {
                List<iExchange.Common.Manager.OpenInterestSummary> openInterestSummarys = this._ServiceProxy.EndGetOpenInterestAccountSummary(result);
                EndGetOpenInterestAccountSummary(instrumentId, openInterestSummarys);
            }, null);
        }

        public void GetOpenInterestOrderSummary(string exchangeCode,ManagerConsole.ViewModel.OpenInterestSummary accountSumamry, string[] blotterCodeSelecteds, Action<ManagerConsole.ViewModel.OpenInterestSummary, List<OpenInterestSummary>> EndGetOpenInterestOrderSummary)
        {
            Guid accountId = accountSumamry.Id;
            Guid instrumentId = accountSumamry.InstrumentId;
            this._ServiceProxy.BeginGetOpenInterestOrderSummary(exchangeCode, instrumentId,accountId, accountSumamry.AccountType, blotterCodeSelecteds, delegate(IAsyncResult result)
            {
                List<iExchange.Common.Manager.OpenInterestSummary> openInterestSummarys = this._ServiceProxy.EndGetOpenInterestOrderSummary(result);
                EndGetOpenInterestOrderSummary(accountSumamry,openInterestSummarys);
            }, null);
        }

        public void GetBlotterList(string exchangeCode, Action<List<BlotterSelection>> EndGetBlotterList)
        {
            this._ServiceProxy.BeginGetBlotterList(exchangeCode, delegate(IAsyncResult result)
            {
                List<BlotterSelection> blotterSelectionList = this._ServiceProxy.EndGetBlotterList(result);
                EndGetBlotterList(blotterSelectionList);
            }, null);
        }

        public void GetAccountReportData(string exchangeCode,string selectedPrice,Guid accountId, Action<AccountStatusQueryResult> EndGetAccountReportData)
        {
            this._ServiceProxy.BeginGetAccountReportData(exchangeCode, selectedPrice, accountId, delegate(IAsyncResult result)
            {
                AccountStatusQueryResult queryResult = this._ServiceProxy.EndGetAccountReportData(result);
                EndGetAccountReportData(queryResult);
            }, null);
        }

        public void GetInstrumentForFloatingPLCalc(string exchangeCode,Action<List<InstrumentForFloatingPLCalc>> EndGetInstrumentForFloatingPLCalc)
        {
            this._ServiceProxy.BeginGetInstrumentForFloatingPLCalc(exchangeCode,delegate(IAsyncResult result)
            {
                List<InstrumentForFloatingPLCalc> instrumentList = this._ServiceProxy.EndGetInstrumentForFloatingPLCalc(result);
                EndGetInstrumentForFloatingPLCalc(instrumentList);
            },null);
        }

        public void UpdateInstrumentForFloatingPLCalc(string exchangeCode, Guid instrumentId, string bid, int spreadPoint, Action<bool> EndUpdateInstrumentForFloatingPLCalc)
        {
            this._ServiceProxy.BeginUpdateInstrumentForFloatingPLCalc(exchangeCode,instrumentId,bid,spreadPoint,delegate(IAsyncResult result)
            {
                bool isSucceed = this._ServiceProxy.EndUpdateInstrumentForFloatingPLCalc(result);
                EndUpdateInstrumentForFloatingPLCalc(isSucceed);
            },null);
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
        public ConfigMetadata GetConfigMetadata()
        {
            return this._ServiceProxy.GetConfigMetadata();
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
                App.MainFrameWindow.Dispatcher.BeginInvoke((Action<bool>)delegate(bool success)
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
                App.MainFrameWindow.Dispatcher.BeginInvoke((Action<bool>)delegate(bool success)
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
                App.MainFrameWindow.Dispatcher.BeginInvoke((Action<bool>)delegate(bool success)
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

        public void UpdateExchangeQuotation(InstrumentQuotationSet set)
        {
            this._ServiceProxy.BeginUpdateQuotationPolicy(set, delegate(IAsyncResult ar)
            {
                this._ServiceProxy.EndUpdateQuotationPolicy(ar);
            }, null);
        }

        public void UpdateInstrument(InstrumentQuotationSet set)
        {
            this._ServiceProxy.BeginUpdateInstrument(set, delegate(IAsyncResult ar)
            {
                this._ServiceProxy.EndUpdateInstrument(ar);
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

        public void AddNewRelation(Guid id, string code, List<int> instruments, Action<bool> callBack)
        {
            this._ServiceProxy.BeginAddNewRelation(id, code, instruments, delegate(IAsyncResult ar)
            {
                bool result = this._ServiceProxy.EndAddNewRelation(ar);
                callBack(result);
            }, null);
        }

        public void SwitchDefaultSource(SwitchRelationBooleanPropertyMessage switchMessage)
        {
            this._ServiceProxy.BeginSwitchDefaultSource(switchMessage, delegate(IAsyncResult ar)
            {
                this._ServiceProxy.EndSwitchDefaultSource(ar);
            }, null);
        }

        public void ConfirmAbnormalQuotation(int instrumentId, int confirmId, bool accepted)
        {
            this._ServiceProxy.BeginConfirmAbnormalQuotation(instrumentId, confirmId, accepted, delegate(IAsyncResult ar)
            {
                this._ServiceProxy.EndConfirmAbnormalQuotation(ar);
            }, null);
        }

        public void SuspendResume(int[] instrumentIds, bool resume)
        {
            this._ServiceProxy.BeginSuspendResume(instrumentIds, resume, delegate(IAsyncResult ar)
            {
                this._ServiceProxy.EndSuspendResume(ar);
            }, null);
        }

        public void ExchangeSuspendResume(Dictionary<string, List<Guid>> instruments, bool resume)
        {
            this._ServiceProxy.BeginExchangeSuspendResume(instruments, resume, delegate(IAsyncResult ar)
            {
                this._ServiceProxy.EndExchangeSuspendResume(ar);
            }, null);
        }

        public void GetOriginQuotationForModifyAskBidHistory(string exchangeCode, Guid instrumentId, DateTime timeSpan, string origin, Action<List<HistoryQuotationData>> callBack)
        {
            this._ServiceProxy.BeginGetOriginQuotationForModifyAskBidHistory(exchangeCode, instrumentId, timeSpan, origin, delegate(IAsyncResult ar)
            {
                List<HistoryQuotationData> historyQuotations = this._ServiceProxy.EndGetOriginQuotationForModifyAskBidHistory(ar);
                callBack(historyQuotations);
            }, null);
        }

        public void UpdateHighLow(string exchangeCode, Guid instrumentId, bool isOriginHiLo, string newInput, bool isUpdateHigh,Action<UpdateHighLowBatchProcessInfo> callback)
        {
            this._ServiceProxy.BeginUpdateHighLow(exchangeCode, instrumentId, isOriginHiLo, newInput, isUpdateHigh, delegate(IAsyncResult ar)
            {
                UpdateHighLowBatchProcessInfo batchInfo = this._ServiceProxy.EndUpdateHighLow(ar);
                callback(batchInfo);
            }, null);
        }
        // -----------Dictionary<string,string> key =ExchangeCode, Value quotation
        public void FixOverridedQuotationHistory(Dictionary<string,string> quotations, bool needApplyAutoAdjustPoints, Action<bool> callBack)
        {
            this._ServiceProxy.BeginFixOverridedQuotationHistory(quotations, needApplyAutoAdjustPoints, delegate(IAsyncResult ar)
            {
                bool result = this._ServiceProxy.EndFixOverridedQuotationHistory(ar);
                callBack(result);
            }, null);
        }

        public void RestoreHighLow(string exchangeCode, int batchProcessId,Action<bool> callback)
        {
            this._ServiceProxy.BeginRestoreHighLow(exchangeCode, batchProcessId, delegate(IAsyncResult ar)
            {
                bool result = this._ServiceProxy.EndRestoreHighLow(ar);
                callback(result);
            }, null);
        }
        #endregion
    }
}
