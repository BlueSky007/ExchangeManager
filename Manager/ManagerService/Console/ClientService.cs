﻿using System;
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

        private List<DataPermission> _DataPermissions;
        #region MainWindowFunction
        
        public LoginResult Login(string userName, string password, string oldSessionId, Language language)
        {
            LoginResult loginResult = new LoginResult();
            List<DataPermission> dataPermissions = new List<DataPermission>();
            try
            {
                User user = UserDataAccess.Login(userName, password,out dataPermissions);
                if (user.UserId != Guid.Empty)
                {
                    this._DataPermissions = dataPermissions;
                    string sessionId = OperationContext.Current.SessionId;
                    IClientProxy clientProxy = OperationContext.Current.GetCallbackChannel<IClientProxy>();
                    this._Client = MainService.ClientManager.AddClient(oldSessionId, sessionId, user, clientProxy, language);

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

        public InitializeData GetInitializeData()
        {
            InitializeData initializeData = new InitializeData();
            initializeData.ConfigParameter = new ConfigParameter()
            {
                AllowModifyOrderLot = MainService.ManagerSettings.AllowModifyOrderLot,
                ConfirmRejectDQOrder = MainService.ManagerSettings.ConfirmRejectDQOrder
            };
            try
            {
                Dictionary<string, List<Guid>> accountPermissions = new Dictionary<string, List<Guid>>();
                Dictionary<string, List<Guid>> instrumentPermissions = new Dictionary<string, List<Guid>>();
                List<ExchangeInitializeData> exchangeInitializeDatas = new List<ExchangeInitializeData>();
                foreach (ExchangeSystemSetting item in MainService.ManagerSettings.ExchangeSystems)
                {
                    List<Guid> accountMemberIds = new List<Guid>();
                    List<Guid> instrumentMemberIds = new List<Guid>();
                    bool accountDeafultStatus;
                    bool instrumentDeafultStatus;

                    List<DataPermission> systemPermissions = this._DataPermissions.FindAll(delegate(DataPermission data)
                        {
                            return data.ExchangeSystemCode == item.Code;
                        });
                    UserDataAccess.GetGroupDeafultStatus(item.Code, systemPermissions, out accountDeafultStatus, out instrumentDeafultStatus);
                    if (this._Client.user.IsAdmin)
                    {
                        accountDeafultStatus = true;
                        instrumentDeafultStatus = true;
                        systemPermissions.Clear();
                    }
                    ExchangeInitializeData exchangeInitializeData = ExchangeData.GetInitData(item.Code, this._Client.user.UserId, systemPermissions,
                        accountDeafultStatus, instrumentDeafultStatus, out accountMemberIds, out instrumentMemberIds);

                    exchangeInitializeData.ExchangeCode = item.Code;
                    accountPermissions.Add(item.Code, accountMemberIds);
                    instrumentPermissions.Add(item.Code, instrumentMemberIds);

                    exchangeInitializeDatas.Add(exchangeInitializeData);

                }
                this._Client.UpdatePermission(accountPermissions, instrumentPermissions);
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
            this._Client.Close();
        }

        public FunctionTree GetFunctionTree()
        {
            return this._Client.GetFunctionTree();
        }

        public void SaveLayout(string layout, string content,string layoutName)
        {
            this._Client.SaveLayout(layout, content, layoutName);
        }

        public List<string> LoadLayout(string layoutName)
        {
            return this._Client.LoadLayout(layoutName);
        }
        #endregion

        #region UserAndRoleManager
        public bool ChangePassword(string currentPassword, string newPassword)
        {
            return this._Client.ChangePassword(currentPassword, newPassword);
        }

        public Dictionary<string, List<FuncPermissionStatus>> GetAccessPermissions()
        {
            return this._Client.GetAccessPermissions();
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
        public List<OrderQueryEntity> GetOrderByInstrument(Guid instrumentId, Guid accountGroupId, OrderType orderType,
            bool isExecute, DateTime fromDate, DateTime toDate)
        {
            return this._Client.GetOrderByInstrument(instrumentId, accountGroupId, orderType, isExecute, fromDate, toDate);
        }

        public List<AccountGroupGNP> GetGroupNetPosition()
        {
            return this._Client.GetGroupNetPosition();
        }

        public List<OpenInterestSummary> GetInstrumentSummary(bool isGroupByOriginCode, string[] blotterCodeSelecteds)
        {
            return this._Client.GetInstrumentSummary(isGroupByOriginCode, blotterCodeSelecteds);
        }

        public List<OpenInterestSummary> GetAccountSummary(Guid instrumentId,string[] blotterCodeSelecteds)
        {
            return this._Client.GetAccountSummary(instrumentId,blotterCodeSelecteds);
        }

        public List<OpenInterestSummary> GetOrderSummary(Guid instrumentId, Guid accountId,iExchange.Common.AccountType accountType, string[] blotterCodeSelecteds)
        {
            return this._Client.GetOrderSummary(instrumentId, accountId, accountType, blotterCodeSelecteds);
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
            return this._Client.GetConfigMetadata();
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
                Logger.AddEvent(TraceEventType.Error, "[ManagerService.AddMetadataObject]{0}\r\n{1}", metadataObject, ex.ToString());
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
                Logger.AddEvent(TraceEventType.Error, "[ManagerService.AddMetadataObjects]{0}", ex.ToString());
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
                    "[ManagerService.UpdateMetadataObject]Type:{0}, Id:{1}\r\nfields:\r\n{2}{3}", type, objectId, ServiceHelper.DumpDictionary(fieldAndValues), ex.ToString());
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
                string log = "[ManagerService.UpdateMetadataObjects]\r\n";
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
                    "[ManagerService.UpdateMetadataObject]type:{0}, Id:{1}, field:{2}, value:{3}\r\n{4}", type, objectId, field, value, ex.ToString());
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
                    "[ManagerService.DeleteMetadataObject]Type:{0}, Id:{1}\r\n{2}", type, objectId, ex.ToString());
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
                    "[ManagerService.SendQuotation]instrumentSourceRelationId:{0}, ask:{1}, bid:{2}\r\n{3}", instrumentSourceRelationId, ask, bid, ex.ToString());
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
            return this._Client.ExchangeSuspendResume(instruments, resume);
        }

        public void SetQuotationPolicyDetail(Guid relationId, QuotePolicyDetailsSetAction action, int changeValue)
        {
            try
            {
                this._Client.SetQuotationPolicyDetail(relationId, action, changeValue);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(TraceEventType.Error, "ManagerServer/SetQuotePolicyDetail.\r\n{0}", ex.ToString());
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
                Logger.TraceEvent(TraceEventType.Error, "ManagerServer.AddNewRelation.\r\n{0}", ex.ToString());
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
                    "[ManagerService.SwitchActiveSource] InstrumentId:{0}, OldRelationId:{1}, NewRelationId:{2}\r\n{3}",
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
                Logger.TraceEvent(TraceEventType.Error, "[ManagerService.GetQuotePolicyRelation\r\n{0}", ex.ToString());
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
                    "[ManagerService.ConfirmAbnormalQuotation] instrumentId:{0}, confirmId:{1}, accepted:{2}\r\n{3}",
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
                    "[ManagerService.SuspendResume] instrumentIds:{0}, resume:{1}\r\n{2}",
                    string.Join(",", instrumentIds), resume, exception);
            }
        }
    }
}
