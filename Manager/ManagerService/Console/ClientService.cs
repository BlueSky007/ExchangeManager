using System;
using System.Collections.Generic;
using System.ServiceModel;
using Manager.Common;
using ManagerService.DataAccess;
using System.Diagnostics;
using Manager.Common.QuotationEntities;
using Manager.Common.LogEntities;
using Manager.Common.ReportEntities;
using iExchange.Common.Manager;
using System.Xml;
using Manager.Common.Settings;
using System.Collections.ObjectModel;
using TransactionError = iExchange.Common.TransactionError;
using CancelReason = iExchange.Common.CancelReason;
using iExchange.Common;

namespace ManagerService.Console
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class ClientService : IClientService
    {
        private Client _Client;
        #region MainWindowFunction
        public LoginResult Login(string userName, string password, Language language)
        {
            LoginResult loginResult = new LoginResult();
            List<DataPermission> dataPermissions = new List<DataPermission>();
            try
            {
                User user = UserDataAccess.Login(userName, password, out dataPermissions);
                if (user.UserId != Guid.Empty)
                {
                    string sessionId = OperationContext.Current.SessionId;
                    IClientProxy clientProxy = OperationContext.Current.GetCallbackChannel<IClientProxy>();
                    this._Client = MainService.ClientManager.AddClient(sessionId, user, clientProxy, language, dataPermissions);

                    OperationContext.Current.Channel.Faulted += this._Client.Channel_Broken;
                    OperationContext.Current.Channel.Closed += this._Client.Channel_Broken;
                    loginResult.User = user;
                    loginResult.SessionId = sessionId;
                    loginResult.LayoutNames = UserDataAccess.GetAllLayoutName(userName);
                    string docklayout = string.Empty;
                    string content = string.Empty;
                    UserDataAccess.LoadLayout(userName, SR.LastClosed, out docklayout, out content);
                    loginResult.DockLayout = docklayout;
                    loginResult.ContentLayout = content;

                    loginResult.SourceConnectionStates = MainService.QuotationManager.SourceConnectionStates;
                    loginResult.ExchangeConnectionStates = MainService.ExchangeManager.ExchangeConnectionStates;
                }
                else
                {
                    OperationContext.Current.Channel.Abort();
                }
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(TraceEventType.Error, "Login userName:{0}, login failed:\r\n{1}", userName, ex.ToString());
            }
            return loginResult;
        }

        public bool RecoverConnection(string oldSessionId)
        {
            try
            {
                string sessionId = OperationContext.Current.SessionId;
                IClientProxy clientProxy = OperationContext.Current.GetCallbackChannel<IClientProxy>();
                Client client;
                if (MainService.ClientManager.TryRecoverConnection(oldSessionId, sessionId, clientProxy, out client))
                {
                    this._Client = client;
                    return true;
                }
                else
                {
                    OperationContext.Current.Channel.Abort();
                }
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(TraceEventType.Error, "RecoverConnection oldSessionId:{0}, login failed:\r\n{1}", oldSessionId, ex);
            }
            return false;
        }

        public InitializeData GetInitializeData()
        {
            InitializeData initializeData = new InitializeData();
            try
            {
                initializeData.ConfigParameter = new ConfigParameter()
                {
                    AllowModifyOrderLot = MainService.ManagerSettings.AllowModifyOrderLot,
                    IsTaskSchedulerRunNotify = MainService.ManagerSettings.IsTaskSchedulerRunNotify,
                };
                Dictionary<string, List<Guid>> accountPermissions = new Dictionary<string, List<Guid>>();
                Dictionary<string, List<Guid>> instrumentPermissions = new Dictionary<string, List<Guid>>();
                List<ExchangeInitializeData> exchangeInitializeDatas = new List<ExchangeInitializeData>();
                foreach (ExchangeSystemSetting exchangeSystemSetting in MainService.ManagerSettings.ExchangeSystems)
                {
                    List<Guid> accountMemberIds = new List<Guid>();
                    List<Guid> instrumentMemberIds = new List<Guid>();
                    bool accountDeafultStatus;
                    bool instrumentDeafultStatus;

                    List<DataPermission> systemPermissions;
                    if (this._Client.user.IsAdmin)
                    {
                        accountDeafultStatus = true;
                        instrumentDeafultStatus = true;
                        systemPermissions = new List<DataPermission>();
                    }
                    else
                    {
                        systemPermissions = this._Client.DataPermissions.FindAll(delegate(DataPermission data)
                        {
                            return data.ExchangeSystemCode == exchangeSystemSetting.Code;
                        });
                        UserDataAccess.GetGroupDeafultStatus(exchangeSystemSetting.Code, systemPermissions, out accountDeafultStatus, out instrumentDeafultStatus);
                    }
                    ExchangeInitializeData exchangeInitializeData = ExchangeData.GetInitData(exchangeSystemSetting.Code, this._Client.user.UserId, systemPermissions,
                        accountDeafultStatus, instrumentDeafultStatus, out accountMemberIds, out instrumentMemberIds);

                    exchangeInitializeData.ExchangeCode = exchangeSystemSetting.Code;
                    accountPermissions.Add(exchangeSystemSetting.Code, accountMemberIds);
                    instrumentPermissions.Add(exchangeSystemSetting.Code, instrumentMemberIds);

                    exchangeInitializeDatas.Add(exchangeInitializeData);

                }
                this._Client.ReplacePermissionData(accountPermissions, instrumentPermissions);
                initializeData.ExchangeInitializeDatas = exchangeInitializeDatas.ToArray();
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(TraceEventType.Error, "GetInitializeData Error:\r\n{0}", ex.ToString());
            }
            return initializeData;
        }

        public void Logout()
        {
            try
            {
                this._Client.HandleLogout();
            }
            catch (Exception ex)
            {
                Logger.AddEvent(TraceEventType.Error, "ClientService.SaveLayout userName:{0},IP:{1}\r\n{2}", this._Client.user.UserName,this._Client.IP, ex);
            }
        }

        public FunctionTree GetFunctionTree()
        {
            try
            {
                return this._Client.GetFunctionTree();
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(TraceEventType.Error, "ClientService.GetFunctionTree userName:{0},IP:{1}\r\n{2}", this._Client.user.UserName, this._Client.IP, ex);
                return new FunctionTree();
            }
        }

        public void SaveLayout(string layout, string content, string layoutName)
        {
            try
            {
                this._Client.SaveLayout(layout, content, layoutName);
            }
            catch (Exception ex)
            {
                Logger.AddEvent(TraceEventType.Error, "ClientService.SaveLayout userName:{0},IP:{1}\r\n{2}", this._Client.user.UserName, this._Client.IP, ex);
            }
        }

        public List<string> LoadLayout(string layoutName)
        {
            try
            {
                return this._Client.LoadLayout(layoutName);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(TraceEventType.Error, "ClientService.LoadLayout userName:{0},IP:{1}\r\n{2}", this._Client.user.UserName, this._Client.IP, ex);
                return new List<string>();
            }
        }
        #endregion

        #region UserAndRoleManager
        public bool ChangePassword(string currentPassword, string newPassword)
        {
            try
            {
                return this._Client.ChangePassword(currentPassword, newPassword);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(TraceEventType.Error, "ClientService.ChangePassword userName:{0},IP:{1}\r\n{2}", this._Client.user.UserName, this._Client.IP, ex);
                return false;
            }
        }

        public Dictionary<string, List<FuncPermissionStatus>> GetAccessPermissions()
        {
            try
            {
                return this._Client.GetAccessPermissions();
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(TraceEventType.Error, "ClientService.GetAccessPermission userName:{0},IP:{1}\r\n{2}", this._Client.user.UserName, this._Client.IP, ex);
                return null;
            }
        }

        public List<UserData> GetUserData()
        {
            return this._Client.GetUserData();
        }

        public List<RoleData> GetRoles()
        {
            return this._Client.GetRoles();
        }

        public bool UpdateUsers(UserData user, string password, bool isNewUser)
        {
            return this._Client.UpdateUsers(user, password, isNewUser);
        }

        public bool DeleteUser(Guid userId)
        {
            return this._Client.DeleteUser(userId);
        }

        public List<RoleFunctonPermission> GetAllFunctionPermission()
        {
            return this._Client.GetAllFunctionPermission();
        }

        public List<RoleDataPermission> GetAllDataPermission()
        {
            return this._Client.GetAllDataPermission();
        }

        public bool UpdateRole(RoleData role, bool isNewRole)
        {
            return this._Client.UpdateRole(role, isNewRole);
        }

        public bool DeleteRole(int roleId)
        {
            return this._Client.DeleteRole(roleId);
        }

        #endregion

        #region DealingConsole

        public void AbandonQuote(List<Answer> abandQuotations)
        {
            this._Client.AbandonQuote(abandQuotations);
        }

        public void SendQuotePrice(List<Answer> sendQuotations)
        {
            this._Client.SendQuotePrice(sendQuotations);
        }

        public TransactionError AcceptPlace(Guid transactionId, LogOrder logEntity)
        {
            return this._Client.AcceptPlace(transactionId, logEntity);
        }

        public TransactionError CancelPlace(Guid transactionId, CancelReason cancelReason)
        {
            return this._Client.CancelPlace(transactionId, cancelReason);
        }

        public TransactionResult Execute(Guid transactionId, string buyPrice, string sellPrice, decimal lot, Guid orderId, LogOrder logEntity)
        {
            return this._Client.Execute(transactionId, buyPrice, sellPrice, lot, orderId, logEntity);
        }

        public TransactionError Cancel(Guid transactionId, CancelReason cancelReason, LogOrder logEntity)
        {
            return this._Client.Cancel(transactionId, cancelReason, logEntity);
        }

        public void ResetHit(string exchangeCode,Guid[] orderIds)
        {
            this._Client.ResetHit(exchangeCode,orderIds);
        }

        public AccountInformation GetAcountInfo(string exchangeCode,Guid transactionId)
        {
            return this._Client.GetAcountInfo(exchangeCode,transactionId);
        }

        public SettingsParameter LoadSettingsParameters()
        {
            return this._Client.LoadSettingsParameters(this._Client.userId);
        }
        #endregion

        #region Setting Manager
        public List<SoundSetting> CopyFromSetting(Guid copyUserId)
        {
            return this._Client.CopyFromSetting(copyUserId);
        }

        public List<ParameterDefine> LoadParameterDefine()
        {
            return this._Client.LoadParameterDefine();
        }

        public bool CreateTaskScheduler(TaskScheduler taskScheduler)
        {
            taskScheduler.Creater = this._Client.userId;
            return this._Client.CreateTaskScheduler(taskScheduler);
        }

        public bool EditorTaskScheduler(TaskScheduler taskScheduler)
        {
            taskScheduler.Creater = this._Client.userId;
            return this._Client.EditorTaskScheduler(taskScheduler);
        }

        public void EnableTaskScheduler(TaskScheduler taskScheduler)
        {
            taskScheduler.Creater = this._Client.userId;
            this._Client.EnableTaskScheduler(taskScheduler);
        }

        public void StartRunTaskScheduler(TaskScheduler taskScheduler)
        {
            taskScheduler.Creater = this._Client.userId;
            this._Client.StartRunTaskScheduler(taskScheduler);
        }

        public bool DeleteTaskScheduler(TaskScheduler taskScheduler)
        {
            return this._Client.DeleteTaskScheduler(taskScheduler);
        }

        public List<TaskScheduler> GetTaskSchedulersData()
        {
            return this._Client.GetTaskSchedulersData();
        }

        public bool UpdateManagerSettings(Guid settingId,SettingParameterType type, Dictionary<string, object> fieldAndValues)
        {
            return this._Client.UpdateManagerSettings(settingId,type, fieldAndValues);
        }
        #endregion

        #region Report
        public List<OrderQueryEntity> GetOrderByInstrument(string exchangeCode,Guid instrumentId, Guid accountGroupId, OrderType orderType,
            bool isExecute, DateTime fromDate, DateTime toDate)
        {
            try
            {
                return this._Client.GetOrderByInstrument(exchangeCode,instrumentId, accountGroupId, orderType, isExecute, fromDate, toDate);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(TraceEventType.Error, "ClientService.GetOrderByInstrument userName:{0},IP:{1}\r\n{2}", this._Client.user.UserName, this._Client.IP, ex);
                return null;
            }
        }

        public List<AccountGroupGNP> GetGroupNetPosition(string exchangeCode, bool showActualQuantity,string[] blotterCodeSelecteds)
        {
            return this._Client.GetGroupNetPosition(exchangeCode,showActualQuantity, blotterCodeSelecteds);
        }

        public List<OpenInterestSummary> GetOpenInterestInstrumentSummary(string exchangeCode,bool isGroupByOriginCode, string[] blotterCodeSelecteds)
        {
            return this._Client.GetOpenInterestInstrumentSummary(exchangeCode,isGroupByOriginCode, blotterCodeSelecteds);
        }

        public List<OpenInterestSummary> GetOpenInterestAccountSummary(string exchangeCode,Guid instrumentId,string[] blotterCodeSelecteds)
        {
            Guid[] instruments = new Guid[] { instrumentId };
            return this._Client.GetOpenInterestAccountSummary(exchangeCode,null, instruments, blotterCodeSelecteds);
        }

        public List<OpenInterestSummary> GetOpenInterestOrderSummary(string exchangeCode,Guid instrumentId, Guid accountId,iExchange.Common.AccountType accountType, string[] blotterCodeSelecteds)
        {
            return this._Client.GetOpenInterestOrderSummary(exchangeCode,instrumentId, accountId, accountType, blotterCodeSelecteds);
        }

        public List<BlotterSelection> GetBlotterList(string exchangeCode)
        {
            return this._Client.GetBlotterList(exchangeCode);
        }

        public AccountStatusQueryResult GetAccountReportData(string exchangeCode, string selectedPrice, Guid accountId)
        {
            return this._Client.GetAccountReportData(exchangeCode, selectedPrice,accountId);
        }
        #endregion

        #region Log Audit

        public List<LogQuote> GetQuoteLogData(DateTime fromDate, DateTime toDate, LogType logType)
        {
            return this._Client.GetQuoteLogData(fromDate,toDate,logType);
        }

        public List<LogOrder> GetLogOrderData(DateTime fromDate, DateTime toDate, LogType logType)
        {
            return this._Client.GetLogOrderData(fromDate, toDate, logType);
        }

        public List<LogSetting> GetLogSettingData(DateTime fromDate, DateTime toDate, LogType logType)
        {
            return this._Client.GetLogSettingData(fromDate, toDate, logType);
        }

        public List<LogPrice> GetLogPriceData(DateTime fromDate, DateTime toDate, LogType logType)
        {
            return this._Client.GetLogPriceData(fromDate,toDate,logType);
        }

        public List<LogSourceChange> GetLogSourceChangeData(DateTime fromDate, DateTime toDate, LogType logType)
        {
            return this._Client.GetLogSourceChangeData(fromDate, toDate, logType);
        }
        #endregion

        public ConfigMetadata GetConfigMetadata()
        {
            try
            {
                return this._Client.GetConfigMetadata();
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(TraceEventType.Error, "ClientService.GetConfigMetadata userName:{0},IP:{1}\r\n{2}", this._Client.user.UserName, this._Client.IP, ex);
                return null;
            }
        }
        

        public List<LogQuote> GetQuoteLogData(DateTime fromDate, DateTime toDate)
        {
            throw new NotImplementedException();
        }

        public int AddMetadataObject(IMetadataObject metadataObject)
        {
            try
            {
                return this._Client.AddMetadataObject(metadataObject);
            }
            catch (Exception ex)
            {
                Logger.AddEvent(TraceEventType.Error, "[ClientService.GetQuoteLogData]{0}\r\n{1}", metadataObject, ex.ToString());
            }
            return 0;
        }

        public int AddInstrument(InstrumentData instrumentData)
        {
            try
            {
                return this._Client.AddInstrument(instrumentData);
            }
            catch (Exception ex)
            {
                Logger.AddEvent(TraceEventType.Error, "[ClientService.AddInstrument]{0}", ex.ToString());
            }
            return 0;
        }

        public bool UpdateMetadataObject(MetadataType type, int objectId, Dictionary<string, object> fieldAndValues)
        {
            try
            {
                this._Client.UpdateMetadataObject(type, objectId, fieldAndValues);
                return true;
            }
            catch (Exception ex)
            {
                Logger.AddEvent(TraceEventType.Error,
                    "[ClientService.UpdateMetadataObject]Type:{0}, Id:{1}\r\nfields:\r\n{2}{3}", type, objectId, ServiceHelper.DumpDictionary(fieldAndValues), ex.ToString());
            }
            return false;
        }
        public bool UpdateMetadataObjects(UpdateData[] updateDatas)
        {
            try
            {
                this._Client.UpdateMetadataObjects(updateDatas);
                return true;
            }
            catch (Exception ex)
            {
                string log = "[ClientService.UpdateMetadataObjects]\r\n";
                foreach (UpdateData item in updateDatas)
                {
                    log += string.Format("Type:{0}, Id:{1}\r\nfields:\r\n{2}\r\n", item.MetadataType, item.ObjectId, ServiceHelper.DumpDictionary(item.FieldsAndValues));
                }
                Logger.AddEvent(TraceEventType.Error, "{0}{1}", log, ex.ToString());
            }
            return false;
        }

        public bool UpdateMetadataObjectField(MetadataType type, int objectId, string field, object value)
        {
            try
            {
                this._Client.UpdateMetadataObject(type, objectId, field, value);
                return true;
            }
            catch (Exception ex)
            {
                Logger.AddEvent(TraceEventType.Error,
                    "[ClientService.UpdateMetadataObject]type:{0}, Id:{1}, field:{2}, value:{3}\r\n{4}", type, objectId, field, value, ex.ToString());
            }
            return false;
        }

        public bool DeleteMetadataObject(MetadataType type, int objectId)
        {
            try
            {
                this._Client.DeleteMetadataObject(type, objectId);
                return true;
            }
            catch (Exception ex)
            {
                Logger.AddEvent(TraceEventType.Error,
                    "[ClientService.DeleteMetadataObject]Type:{0}, Id:{1}\r\n{2}", type, objectId, ex.ToString());
            }
            return false;
        }


        public void SendQuotation(int instrumentSourceRelationId, double ask, double bid)
        {
            try
            {
                this._Client.SendQuotation(instrumentSourceRelationId, ask, bid);
            }
            catch (Exception ex)
            {
                Logger.AddEvent(TraceEventType.Error,
                    "[ClientService.SendQuotation]instrumentSourceRelationId:{0}, ask:{1}, bid:{2}\r\n{3}", instrumentSourceRelationId, ask, bid, ex.ToString());
            }
        }

        public void UpdateQuotationPolicy(InstrumentQuotationSet set)
        {
            try
            {
                this._Client.UpdateQuotationPolicy(set);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "UpdateQuotationPolicy.\r\n{0}", ex.ToString());
            }
        }

        public void UpdateInstrument(InstrumentQuotationSet set)
        {
            try
            {
                this._Client.UpdateInstrument(set);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(TraceEventType.Error, "UpdateInstrument\r\n{0}", ex.ToString());
            }
        }

        public bool ExchangeSuspendResume(Dictionary<string, List<Guid>> instruments, bool resume)
        {
            try
            {
                return this._Client.ExchangeSuspendResume(instruments, resume);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(TraceEventType.Error, "ClientService.ExchangeSuspendResume userName:{0},IP:{1}\r\n{2}", this._Client.user.UserName, this._Client.IP, ex);
                return false;
            }
        }

        public void SetQuotationPolicyDetail(Guid relationId, QuotePolicyDetailsSetAction action, int changeValue)
        {
            try
            {
                this._Client.SetQuotationPolicyDetail(relationId, action, changeValue);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(TraceEventType.Error, "ClientService.SetQuotePolicyDetail.\r\n{0}", ex.ToString());
            }
        }

        public bool AddNewRelation(Guid id, string code, List<int> instruments)
        {
            try
            {
                return this._Client.AddNewRelation(id, code, instruments);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(TraceEventType.Error, "ClientService.AddNewRelation.\r\n{0}", ex.ToString());
                return false;
            }
        }

        public void SwitchDefaultSource(SwitchRelationBooleanPropertyMessage message)
        {
            try
            {
                this._Client.SwitchDefaultSource(message);
            }
            catch (Exception ex)
            {
                Logger.AddEvent(TraceEventType.Error,
                    "ClientService.SwitchActiveSource InstrumentId:{0}, OldRelationId:{1}, NewRelationId:{2}\r\n{3}",
                    message.InstrumentId, message.OldRelationId, message.NewRelationId, ex.ToString());
            }
        }

        public List<QuotePolicyRelation> GetQuotePolicyRelation()
        {
            try
            {
                return this._Client.GetQuotePolicyRelation();
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(TraceEventType.Error, "ClientService.GetQuotePolicyRelation\r\n{0}", ex.ToString());
                return new List<QuotePolicyRelation>();
            }
        }

        public void ConfirmAbnormalQuotation(int instrumentId, int confirmId, bool accepted)
        {
            try
            {
                this._Client.ConfirmAbnormalQuotation(instrumentId, confirmId, accepted);
            }
            catch (Exception ex)
            {
                Logger.AddEvent(TraceEventType.Error,
                    "ClientService.ConfirmAbnormalQuotation instrumentId:{0}, confirmId:{1}, accepted:{2}\r\n{3}",
                    instrumentId, confirmId, accepted, ex.ToString());
            }
        }

        public void SuspendResume(int[] instrumentIds, bool resume)
        {
            try
            {
                this._Client.SuspendResume(instrumentIds, resume);
            }
            catch (Exception exception)
            {
                Logger.AddEvent(TraceEventType.Error,
                    "ClientService.SuspendResume instrumentIds:{0}, resume:{1}\r\n{2}",
                    string.Join(",", instrumentIds), resume, exception);
            }
        }

        public List<HistoryQuotationData> GetOriginQuotationForModifyAskBidHistory(string exchangeCode, Guid instrumentID, DateTime beginDateTime, string origin)
        {
            try
            {
                return this._Client.GetOriginQuotationForModifyAskBidHistory(exchangeCode, instrumentID, beginDateTime, origin);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "GetOriginQuotationForModifyAskBidHistory Error.\r\n{0}", ex.ToString());
                return new List<HistoryQuotationData>();
            }
        }

        public UpdateHighLowBatchProcessInfo UpdateHighLow(string exchangeCode, Guid instrumentId, bool isOriginHiLo, string newInput, bool isUpdateHigh)
        {
            try
            {
               return this._Client.UpdateHighLow(exchangeCode, instrumentId, isOriginHiLo, newInput, isUpdateHigh);
            }
            catch (Exception ex)
            {
                UpdateHighLowBatchProcessInfo info = new UpdateHighLowBatchProcessInfo();
                info.StateCode = -10;
                info.ErrorMessage = ex.Message;
                Logger.TraceEvent(TraceEventType.Error, "UpdateHighLow Failed\r\n{0}", ex.ToString());
                return info;
            }
        }

        public bool FixOverridedQuotationHistory(Dictionary<string, string> quotations, bool needApplyAutoAdjustPoints)
        {
            try
            {
                return this._Client.FixOverridedQuotationHistory(quotations, needApplyAutoAdjustPoints);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(TraceEventType.Error, "FixOverridedQuotationHistory\r\n{0}", ex.ToString());
                return false;
            }
        }

        public bool RestoreHighLow(string exchangeCode,int batchProcessId)
        {
            try
            {
                return this._Client.RestoreHighLow(exchangeCode, batchProcessId);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(TraceEventType.Error, "RestoreHighLow\r\n{0}", ex.ToString());
                return false;
            }
        }
    }
}
