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
                    //Dictionary<string, List<Guid>> accountPermissions = new Dictionary<string, List<Guid>>();
                    //Dictionary<string, List<Guid>> instrumentPermissions = new Dictionary<string, List<Guid>>();
                    //foreach (ExchangeSystemSetting item in Manager.ManagerSettings.ExchangeSystems)
                    //{
                    //    List<Guid> accountMemberIds = new List<Guid>();
                    //    List<Guid> instrumentMemberIds = new List<Guid>();
                    //    bool accountDeafultStatus;
                    //    bool instrumentDeafultStatus;
                        
                    //    List<DataPermission> systemPermissions = dataPermissions.FindAll(delegate(DataPermission data)
                    //    {
                    //        return data.ExchangeSystemCode == item.Code;
                    //    });
                    //    UserDataAccess.GetGroupDeafultStatus(item.Code, systemPermissions, out accountDeafultStatus, out instrumentDeafultStatus);
                    //    if (user.IsInRole(1))
                    //    {
                    //        accountDeafultStatus = true;
                    //        instrumentDeafultStatus = true;
                    //        systemPermissions.Clear();
                    //    }
                    //    InitializeData initializeData = ExchangeData.GetInitData(item.Code, user.UserId, systemPermissions, accountDeafultStatus, instrumentDeafultStatus);
                    //    initializeData.SettingParameters.Add(item.Code, ExchangeData.GetSettingParameters(item));

                    //    initializeData.ValidAccounts.TryGetValue(item.Code, out accountMemberIds);
                    //    initializeData.ValidInstruments.TryGetValue(item.Code, out instrumentMemberIds);

                    //    accountPermissions.Add(item.Code, accountMemberIds);
                    //    instrumentPermissions.Add(item.Code, instrumentMemberIds);

                    //    loginResult.InitializeDatas.Add(initializeData);
                    //}
                    this._DataPermissions = dataPermissions;
                    string sessionId = OperationContext.Current.SessionId;
                    IClientProxy clientProxy = OperationContext.Current.GetCallbackChannel<IClientProxy>();
                    this._Client = MainService.ClientManager.AddClient(oldSessionId, sessionId, user, clientProxy, language);

                    OperationContext.Current.Channel.Faulted += this._Client.Channel_Broken;
                    OperationContext.Current.Channel.Closed += this._Client.Channel_Broken;
                    loginResult.SessionId = sessionId;
                    loginResult.LayoutNames = UserDataAccess.GetAllLayoutName(userName);
                    string docklayout = string.Empty;
                    string content = string.Empty;
                    UserDataAccess.LoadLayout(userName, "LastClosed", out docklayout, out content);
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

        public List<InitializeData> GetInitializeData()
        {
            List<InitializeData> initializeDatas = new List<InitializeData>();
            try
            {
                    Dictionary<string, List<Guid>> accountPermissions = new Dictionary<string, List<Guid>>();
                    Dictionary<string, List<Guid>> instrumentPermissions = new Dictionary<string, List<Guid>>();
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
                    if (this._Client.user.IsInRole(1))
                        {
                            accountDeafultStatus = true;
                            instrumentDeafultStatus = true;
                            systemPermissions.Clear();
                        }
                    InitializeData initializeData = ExchangeData.GetInitData(item.Code, this._Client.user.UserId, systemPermissions, accountDeafultStatus, instrumentDeafultStatus);
                        initializeData.SettingParameters.Add(item.Code, ExchangeData.GetSettingParameters(item));

                        initializeData.ValidAccounts.TryGetValue(item.Code, out accountMemberIds);
                        initializeData.ValidInstruments.TryGetValue(item.Code, out instrumentMemberIds);

                        accountPermissions.Add(item.Code, accountMemberIds);
                        instrumentPermissions.Add(item.Code, instrumentMemberIds);

                    initializeDatas.Add(initializeData);

                }
                this._Client.UpdatePermission(accountPermissions, instrumentPermissions);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(TraceEventType.Error, "GetInitializeData Error:\r\n{0}", ex.ToString());
            }
            return initializeDatas;
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

        public Dictionary<string, Tuple<string, bool>> GetAccessPermissions()
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

        public TransactionError Execute(Guid transactionId, string buyPrice, string sellPrice, decimal lot, Guid orderId, LogOrder logEntity)
        {
            return this._Client.Execute(transactionId,buyPrice,sellPrice,lot,orderId,logEntity);
            }

        public TransactionError Cancel(Guid transactionId, CancelReason cancelReason, LogOrder logEntity)
        {
            return this._Client.Cancel(transactionId, cancelReason, logEntity);
        }

        public void ResetHit(Guid[] orderIds)
        {
            this._Client.ResetHit(orderIds);
            }

        public AccountInformation GetAcountInfo(Guid transactionId)
        {
            return this._Client.GetAcountInfo(transactionId);
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
                    "[ManagerService.UpdateMetadataObject]type:{0}, objectId:{1}\r\nfields:\r\n{2}{3}", type, objectId, ServiceHelper.DumpDictionary(fieldAndValues), ex.ToString());
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
                    "[ManagerService.UpdateMetadataObject]type:{0}, objectId:{1}, field:{2}, value:{3}\r\n{4}", type, objectId, field, value, ex.ToString());
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
                    "[ManagerService.DeleteMetadataObject]type:{0}, objectId:{1}\r\n{2}", type, objectId, ex.ToString());
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
    }
}
