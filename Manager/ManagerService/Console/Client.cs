using System;
using System.Collections.Generic;
using System.Linq;
using Manager.Common;
using System.Diagnostics;
using ManagerService.Exchange;
using ManagerService.DataAccess;
using Manager.Common.QuotationEntities;
using System.Threading;
using System.IO;
using Manager.Common.LogEntities;
using Manager.Common.ReportEntities;
using iExchange.Common.Manager;
using Account = Manager.Common.Settings.Account;
using System.Xml;
using System.Text;
using Manager.Common.Settings;
using System.Collections.ObjectModel;
using ManagerService.Quotation;
using TransactionError = iExchange.Common.TransactionError;
using OrderType = iExchange.Common.OrderType;
using CancelReason = iExchange.Common.CancelReason;
using ManagerService.Audit;

namespace ManagerService.Console
{
    public class Client
    {
        private string _SessionId;
        public  string _IP;
        private User _User;
        private IClientProxy _ClientProxy;
        private RelayEngine<Message> _MessageRelayEngine;
        private Language _Language;
        private List<DataPermission> _DataPermissions;

        // Map for ExchangeCode -> AccountId HashSet
        private Dictionary<string, HashSet<Guid>> _OwnAccounts;

        // Map for ExchangeCode -> InstrumentId HashSet
        private Dictionary<string, HashSet<Guid>> _OwnInstruments;
        
        private bool _IsInitialized = false;
        private ClientManager _ClientManager;
        private DateTime _ChannelBrokenTime;
        private ConnectionState _ConnectionState = ConnectionState.Unknown;

        public Client(string sessionId, User user, IClientProxy clientProxy, Language language, List<DataPermission> dataPermissions, ClientManager clientManager)
        {
            this._SessionId = sessionId;
            this._IP = ServiceHelper.GetIpAdreess();
            this._User = user;
            this._ClientProxy = clientProxy;
            this._Language = language;
            this._DataPermissions = dataPermissions;
            this._ClientManager = clientManager;
            this._MessageRelayEngine = new RelayEngine<Message>(this.SendMessage, this.HandlEngineException);
        }

        public string SessionId { get { return this._SessionId; } }

        public string IP { get { return this._IP; } }
        public Guid userId { get { return this._User.UserId; } }
        public User user { get { return this._User; } }
        public Language language { get { return this._Language; } }
        public List<DataPermission> DataPermissions { get { return this._DataPermissions; } }
        public DateTime ChannelBrokenTime { get { return this._ChannelBrokenTime; } }
        public ConnectionState ConnectionState { get { return this._ConnectionState; } }

        public void Replace(string sessionId, IClientProxy clientProxy)
        {
            this._SessionId = sessionId;
            this._ClientProxy = clientProxy;
            this._MessageRelayEngine.Resume();
            this._ConnectionState = ConnectionState.Connected;
        }

        public void UpdateDataPermission(string exchangeCode, GroupChangeType type, Guid groupId, List<Guid> memberIds)
        {
            bool hasPermission = UserDataAccess.CheckPermission(userId, groupId, type, exchangeCode);
            if (hasPermission)
            {
                if (type == GroupChangeType.Account)
                {
                    if (!this._OwnAccounts[exchangeCode].Contains(memberIds[0]))
                    {
                        this._OwnAccounts[exchangeCode].UnionWith(memberIds);
                        SettingSet set = ExchangeData.GetExchangeDataChange(exchangeCode, type, memberIds, this._OwnAccounts[exchangeCode].ToList(), this._OwnInstruments[exchangeCode].ToList());
                        this._MessageRelayEngine.AddItem(new UpdateMessage { AddSettingSets = set, ExchangeCode = exchangeCode });
                    }
                }
                else
                {
                    if (!this._OwnInstruments[exchangeCode].Contains(memberIds[0]))
                    {
                        this._OwnInstruments[exchangeCode].UnionWith(memberIds);
                        SettingSet set = ExchangeData.GetExchangeDataChange(exchangeCode, type, memberIds, this._OwnAccounts[exchangeCode].ToList(), this._OwnInstruments[exchangeCode].ToList());
                        this._MessageRelayEngine.AddItem(new UpdateMessage { AddSettingSets = set, ExchangeCode = exchangeCode });
                    }
                }
            }
            else
            {
                if (type == GroupChangeType.Account)
                {
                    if (this._OwnAccounts[exchangeCode].Contains(memberIds[0]))
                    {
                        foreach (Guid memberId in memberIds)
                        {
                            this._OwnAccounts[exchangeCode].Remove(memberId);
                        }
                    }
                }
                else
                {
                    if (this._OwnInstruments[exchangeCode].Contains(memberIds[0]))
                    {
                        foreach (Guid memberId in memberIds)
                        {
                            this._OwnInstruments[exchangeCode].Remove(memberId);
                        }
                    }
                }
            }
        }       

        public void ReplacePermissionData(Dictionary<string, List<Guid>> ownAccounts, Dictionary<string, List<Guid>> ownInstruments,List<DataPermission> dataPermission = null)
        {
             if (dataPermission!=null)
             {
                this._DataPermissions = dataPermission;
            }
            this._OwnAccounts = new Dictionary<string, HashSet<Guid>>();
            this._OwnInstruments = new Dictionary<string, HashSet<Guid>>();
            foreach (string exchangeCode in ownAccounts.Keys) this._OwnAccounts.Add(exchangeCode, new HashSet<Guid>(ownAccounts[exchangeCode]));
            foreach (string exchangeCode in ownInstruments.Keys) this._OwnInstruments.Add(exchangeCode, new HashSet<Guid>(ownInstruments[exchangeCode]));
            this._IsInitialized = true;
        }

        public void AddMessage(Message message)
        {
            if (this._IsInitialized)
            {
                UpdateMessage updateMessage = message as UpdateMessage;
                if (updateMessage != null)
                {
                    if (updateMessage.AddSettingSets != null)
                    {
                        if (updateMessage.AddSettingSets.Accounts != null && updateMessage.AddSettingSets.Accounts.Length > 0)
                        {

                        }
                        if (updateMessage.AddSettingSets.Instruments != null && updateMessage.AddSettingSets.Instruments.Length > 0)
                        {

                        }
                    }
                    if (updateMessage.DeletedSettings != null)
                    {
                        if (updateMessage.DeletedSettings.Accounts != null && updateMessage.DeletedSettings.Accounts.Length > 0)
                        {

                        }
                        if (updateMessage.DeletedSettings.Instruments != null && updateMessage.DeletedSettings.Instruments.Length > 0)
                        {

                        }
                    }
                }

                if (this.Require(message))
                {
                    this._MessageRelayEngine.AddItem(message);
                }
            }
        }

        public void Channel_Broken(object sender, EventArgs e)
        {
            this._MessageRelayEngine.Suspend();
            this._ChannelBrokenTime = DateTime.Now;
            this._ConnectionState = ConnectionState.Disconnected;
            this._ClientManager.AddChannelBrokenClient(this);
            Logger.AddEvent(TraceEventType.Warning, "Client Channel_Broken of UserName:{0} sessionId:{1}", this.user.UserName, this._SessionId);
        }

        public void HandleLogout()
        {
            this._MessageRelayEngine.Stop();
            this._ClientManager.OnClientLogout(this.SessionId);
            Logger.AddEvent(TraceEventType.Information, "Client.HandleLogout User logged out. UserName:{0} sessionId:{1}", this.user.UserName, this._SessionId);
        }

        public ConfigMetadata GetConfigMetadata()
        {
            return MainService.QuotationManager.ConfigMetadata;
        }

        private bool SendMessage(Message message)
        {
            try
            {
                this._ClientProxy.SendMessage(message);
                return true;
            }
            catch (Exception exception)
            {
                Logger.AddEvent(TraceEventType.Warning, "Client.SendMessage SendMessage error, MessageEngine suspended, waiting client recover connection.\r\nUserName:{0},sessionId:{1}\r\n{2}",
                    this.user.UserName, this._SessionId, exception);
            }
            return false;
        }

        private void HandlEngineException(Exception ex)
        {
            Logger.TraceEvent(TraceEventType.Error, "Client.HandlEngineException RelayEngine stopped:\r\n" + ex.ToString());
        }

        private bool Require(Message message)
        {
            bool isRequire = true;
            IFilterable filterableMessage = message as IFilterable;
            ExchangeMessage exchangeMessage = message as ExchangeMessage;
            if (filterableMessage != null && exchangeMessage != null)
            {
                if(string.IsNullOrEmpty(exchangeMessage.ExchangeCode))
                {
                    Logger.TraceEvent(TraceEventType.Error, "Client.Filter exchangeMessage.ExchangeCode is empty, type:{0}", exchangeMessage.GetType().Name);
                    throw new Exception("exchangeMessage.ExchangeCode is empty");
                }

                HashSet<Guid> accountPermissions;
                HashSet<Guid> instrumentPermissions;
                if (filterableMessage.AccountId.HasValue && this._OwnAccounts.TryGetValue(exchangeMessage.ExchangeCode, out accountPermissions))
                {
                    isRequire = accountPermissions.Contains(filterableMessage.AccountId.Value);
                }
                if (isRequire)
                {
                    if (filterableMessage.InstrumentId.HasValue && this._OwnInstruments.TryGetValue(exchangeMessage.ExchangeCode, out instrumentPermissions))
                {
                    isRequire = instrumentPermissions.Contains(filterableMessage.InstrumentId.Value);
                }
            }
            }
            return isRequire;
        }

        #region MainWindowFunction
        public string GetData(int value)
        {
            throw new NotImplementedException();
        }

        public FunctionTree GetFunctionTree()
        {
            FunctionTree tree = UserDataAccess.BuildFunctionTree(this.userId, this.language);
            return tree;
        }

        public void SaveLayout(string layout, string content, string layoutName)
        {
            if ( this.userId != null && this.userId != Guid.Empty)
            {
                string path = string.Format("./Layout/{0}", this.user.UserName);
                string layoutPath = string.Format("./Layout/{0}/{1}_layout.xml", this.user.UserName, layoutName);
                string contentPath = string.Format("./Layout/{0}/{1}_content.xml", this.user.UserName, layoutName);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                using (var stream = new StreamWriter(layoutPath))
                {
                    stream.Write(layout);
                }
                using (var stream = new StreamWriter(contentPath))
                {
                    stream.Write(content);
                }
            }
        }

        public List<string> LoadLayout(string layoutName)
        {
            List<string> layouts = new List<string>();
            string dockLayout;
            string contentLayout;
            UserDataAccess.LoadLayout(this.user.UserName, layoutName, out dockLayout, out contentLayout);
            layouts.Add(dockLayout);
            layouts.Add(contentLayout);
            return layouts;
        }
        #endregion

        #region UserAndRoleManager
        public bool ChangePassword(string currentPassword, string newPassword)
        {
            bool isSuccess = UserDataAccess.ChangePassword(this.userId, currentPassword, newPassword);
            return isSuccess;
        }

        public Dictionary<string, List<FuncPermissionStatus>> GetAccessPermissions()
        {
            Dictionary<string, List<FuncPermissionStatus>> permissions = UserDataAccess.GetAccessPermissions(this.userId, this.language);
            return permissions;
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
                Logger.AddEvent(TraceEventType.Information, "Client.GetRoles begin{0}", this.SessionId);
                List<RoleData> roles = UserDataAccess.GetRoles(this.language.ToString());
                Logger.AddEvent(TraceEventType.Information, "Client.GetRoles end{0}", this.SessionId);
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
                bool isSuccess;
                if (isNewUser)
                {
                    isSuccess = UserDataAccess.AddNewUser(user, password);
                }
                else
                {
                   
                    Dictionary<string, List<FuncPermissionStatus>> permissions = new Dictionary<string, List<FuncPermissionStatus>>();
                    isSuccess = UserDataAccess.EditUser(user, password,out permissions);
                    if (isSuccess)
                    {
                        User editUser = new User();
                        editUser.UserId = user.UserId;
                        editUser.UserName = user.UserName;
                        foreach (RoleData role in user.Roles)
                        {
                            editUser.Roles.Add(role.RoleId, role.RoleName);
                        }
                        MainService.ClientManager.Dispatch(new AccessPermissionUpdateMessage { user = editUser, NewPermission = permissions });
                    }
                }
                return isSuccess;
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
                if (userId == this.userId)
                {
                    return false;
                }
                else
                {
                    bool isSuccess = UserDataAccess.DeleteUser(userId);
                    if (isSuccess)
                    {
                        MainService.ClientManager.DispatchToUser(new NotifyLogoutMessage { UserId = userId }, userId);
                    }
                    return isSuccess;
                }
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "ManagerService.Console.ClientService/DeleteUser.\r\n{0}", ex.ToString());
                return false;
            }
        }

        public List<RoleFunctonPermission> GetAllFunctionPermission()
        {
            try
            {
                List<RoleFunctonPermission> data = UserDataAccess.GetAllFunctionPermissions(this.language.ToString());
                return data;
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "ManagerService.Console.ClientService/GetAllPermission.\r\n{0}", ex.ToString());
                return null;
            }
        }

        public List<RoleDataPermission> GetAllDataPermission()
        {
            try
            {
                List<RoleDataPermission> allData = UserDataAccess.GetAllDataPermissions(MainService.ManagerSettings.ExchangeSystems, this.language.ToString());
                return allData;
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "GetAllDataPermissions.\r\n{0}", ex.ToString());
                return null;
            }
        }

        public bool UpdateRole(RoleData role, bool isNewRole)
        {
            try
            {
                bool isSuccess = false;
                int roleId = role.RoleId;
                if (isNewRole)
                {
                    isSuccess = UserDataAccess.AddNewRole(role, out roleId);
                }
                else
                {
                    isSuccess = UserDataAccess.EditRole(role);
                    if (isSuccess)
                    {
                        //MainService.ClientManager.UpdatePermission(role);
                        MainService.ClientManager.Dispatch(new UpdateRoleMessage { RoleId = roleId, Type= UpdateRoleType.Modify });
                    }
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
                if (this.user.IsInRole(roleId))
                {
                    return false;
                }
                else
                {
                    bool isSuccess = UserDataAccess.DeleteRole(roleId, this.userId);
                    if (isSuccess)
                    {
                        MainService.ClientManager.Dispatch(new UpdateRoleMessage { RoleId = roleId,Type = UpdateRoleType.Delete });
                    }
                    return isSuccess;
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
        internal void AbandonQuote(List<Answer> abandQuotations)
        {
            try
            {
                IEnumerable<IGrouping<string, Answer>> query = abandQuotations.GroupBy(P => P.ExchangeCode, P => P);
                foreach (IGrouping<string, Answer> group in query)
                {
                    List<Answer> newAbandQuotations = group.ToList<Answer>();
                    ExchangeSystem exchangeSystem = MainService.ExchangeManager.GetExchangeSystem(newAbandQuotations[0].ExchangeCode);
                    exchangeSystem.AbandonQuote(newAbandQuotations);

                    //Write Log QuotePrice
                    foreach (Answer answer in newAbandQuotations)
                    {
                        WriteLogManager.WriteQuotePriceLog(answer, this, "AbandQuotePrice");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.AddEvent(TraceEventType.Error, "Client.AbandonQuote error:\r\n{0}", ex.ToString());
            }
        }

        internal void SendQuotePrice(List<Answer> sendQuotations)
        {
            try
            {
                IEnumerable<IGrouping<string, Answer>> query = sendQuotations.GroupBy(P => P.ExchangeCode, P => P);
                foreach (IGrouping<string, Answer> group in query)
                {
                    List<Answer> newSendQuotations = group.ToList<Answer>();
                    ExchangeSystem exchangeSystem = MainService.ExchangeManager.GetExchangeSystem(newSendQuotations[0].ExchangeCode);
                    exchangeSystem.Answer(this._User.UserId,newSendQuotations);

                    //Write Log QuotePrice
                    foreach (Answer answer in newSendQuotations)
                    {
                        WriteLogManager.WriteQuotePriceLog(answer, this, "SendQuotePrice");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.AddEvent(TraceEventType.Error, "Client.SendQuotePrice error:\r\n{0}", ex.ToString());
            }
        }

        internal TransactionError AcceptPlace(Guid transactionId, LogOrder logEntity)
        {
            TransactionError transactionError = TransactionError.OK;
            string exchangeCode = logEntity.ExchangeCode;
            try
            {
                ExchangeSystem exchangeSystem = MainService.ExchangeManager.GetExchangeSystem(exchangeCode);
                transactionError = exchangeSystem.AcceptPlace(transactionId);
                //Write Log
                if (transactionError == TransactionError.OK)
                {
                    logEntity.IP = this._IP;
                    WriteLogManager.WriteQuoteOrderLog(logEntity);
                }
            }
            catch (Exception ex)
            {
                Logger.AddEvent(TraceEventType.Error, "Client.AcceptPlace error:\r\n{0}", ex.ToString());
            }
            return transactionError;
        }

        internal TransactionError CancelPlace(Guid transactionId, CancelReason cancelReason)
        {
            TransactionError transactionError = TransactionError.OK;
            string exchangeCode = "WF01";
            try
            {
                ExchangeSystem exchangeSystem = MainService.ExchangeManager.GetExchangeSystem(exchangeCode);
                transactionError = exchangeSystem.CancelPlace(transactionId, cancelReason);
            }
            catch (Exception ex)
            {
                Logger.AddEvent(TraceEventType.Error, "Client.CancelPlace error:\r\n{0}", ex.ToString());
            }
            return transactionError;
        }

        internal TransactionResult Execute(Guid transactionId, string buyPrice, string sellPrice, decimal lot, Guid orderId, LogOrder logEntity)
        {
            TransactionResult transactionResult = null;
            string exchangeCode = logEntity.ExchangeCode;

            try
            {
                ExchangeSystem exchangeSystem = MainService.ExchangeManager.GetExchangeSystem(exchangeCode);
                transactionResult = exchangeSystem.Execute(transactionId, buyPrice, sellPrice, lot, orderId);

                if (transactionResult.TransactionError == TransactionError.OK)
                {
                    logEntity.IP = this._IP;
                    WriteLogManager.WriteQuoteOrderLog(logEntity);
                }
            }
            catch (Exception ex)
            {
                Logger.AddEvent(TraceEventType.Error, "Client.Execute error:\r\n{0}", ex.ToString());
            }
            return transactionResult;
        }

        internal TransactionError Cancel(Guid transactionId, CancelReason cancelReason, LogOrder logEntity)
        {
            TransactionError transactionError = TransactionError.OK;
            string exchangeCode = logEntity.ExchangeCode;
            try
            {
                ExchangeSystem exchangeSystem = MainService.ExchangeManager.GetExchangeSystem(exchangeCode);
                transactionError = exchangeSystem.Cancel(transactionId, cancelReason);

                if (transactionError == TransactionError.OK)
                {
                    logEntity.IP = this._IP;
                    WriteLogManager.WriteQuoteOrderLog(logEntity);
                }
            }
            catch (Exception ex)
            {
                Logger.AddEvent(TraceEventType.Error, "Client.Cancel error:\r\n{0}", ex.ToString());
            }
            return transactionError;
        }

        internal void ResetHit(string exchangeCode,Guid[] orderIds)
        {
            try
            {
                ExchangeSystem exchangeSystem = MainService.ExchangeManager.GetExchangeSystem(exchangeCode);
                exchangeSystem.ResetHit(orderIds);
            }
            catch (Exception ex)
            {
                Logger.AddEvent(TraceEventType.Error, "Client.ResetHit error:\r\n{0}", ex.ToString());
            }
        }

        internal AccountInformation GetAcountInfo(string exchangeCode,Guid transactionId)
        {
            AccountInformation accountInfor = new AccountInformation();
            try
            {
                ExchangeSystem exchangeSystem = MainService.ExchangeManager.GetExchangeSystem(exchangeCode);
                accountInfor = exchangeSystem.GetAcountInfo(transactionId);
            }
            catch (Exception ex)
            {
                Logger.AddEvent(TraceEventType.Error, "ClientService.GetAcountInfo error:\r\n{0}", ex.ToString());
            }
            return accountInfor;
        }

        internal SettingsParameter LoadSettingsParameters(Guid userId)
        {
            SettingsParameter settingsParameter = null;
            try
            {
                settingsParameter = SettingManagerData.LoadSettingsParameter(userId);
            }
            catch (Exception ex)
            {
                Logger.AddEvent(TraceEventType.Error, "ClientService.LoadSettingsParameters error:\r\n{0}", ex.ToString());
            }
            return settingsParameter;
        }
        #endregion


        #region Setting Manager
        public List<SoundSetting> CopyFromSetting(Guid copyUserId)
        {
            List<SoundSetting> newSoundSettings = new List<SoundSetting>();
            try
            {
                newSoundSettings = SettingManagerData.CopyFromSetting(this.userId,copyUserId);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "ManagerService.Console.Client/CopyFromSetting.\r\n{0}", ex.ToString());
            }
            return newSoundSettings;
        }

        public List<ParameterDefine> LoadParameterDefine()
        {
            List<ParameterDefine> parameterDefines = new List<ParameterDefine>();
            try
            {
                parameterDefines = SettingManagerData.LoadParameterDefine();
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "ManagerService.Console.Client/LoadParameterDefine.\r\n{0}", ex.ToString());
            }
            return parameterDefines;
        }

        public bool CreateTaskScheduler(TaskScheduler taskScheduler)
        {
            bool result = false;
            try
            {
                result = SettingManagerData.CreateTaskScheduler(taskScheduler);

                if (result)
                {
                    MainService.SettingsTaskSchedulerManager.AddTaskScheduler(taskScheduler);
                }
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "ManagerService.Console.Client/CreateTaskScheduler.\r\n{0}", ex.ToString());
            }
            return result;
        }

        public bool EditorTaskScheduler(TaskScheduler taskScheduler)
        {
            bool result = false;
            try
            {
                result = SettingManagerData.EditorTaskScheduler(taskScheduler);

                if (result)
                {
                    MainService.SettingsTaskSchedulerManager.ModifyTaskScheduler(taskScheduler);
                }
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "ManagerService.Console.Client/EditorTaskScheduler.\r\n{0}", ex.ToString());
            }
            return result;
        }

        public void EnableTaskScheduler(TaskScheduler taskScheduler)
        {
            try
            {
                MainService.SettingsTaskSchedulerManager.AddTaskScheduler(taskScheduler);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "ManagerService.Console.Client/EnableTaskScheduler.\r\n{0}", ex.ToString());
            }
        }

        public void StartRunTaskScheduler(TaskScheduler taskScheduler)
        {
            try
            {
                MainService.SettingsTaskSchedulerManager.StartRunTaskScheduler(taskScheduler);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "ManagerService.Console.Client/StartRunTaskScheduler.\r\n{0}", ex.ToString());
            }
        }

        public bool DeleteTaskScheduler(TaskScheduler taskScheduler)
        {
            bool result = false;
            try
            {
                result = SettingManagerData.DeleteTaskScheduler(taskScheduler);
                if (result)
                {
                    MainService.SettingsTaskSchedulerManager.DeleteTaskScheduler(taskScheduler);
                }
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "ManagerService.Console.Client/DeleteTaskScheduler.\r\n{0}", ex.ToString());
            }
            return result;
        }

        public List<TaskScheduler> GetTaskSchedulersData()
        {
            try
            {
                return MainService.SettingsTaskSchedulerManager.TaskSchedulers;
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "ManagerService.Console.Client/GetTaskSchedulersData.\r\n{0}", ex.ToString());
                return null;
            }
        }

        public bool UpdateManagerSettings(Guid settingId,SettingParameterType type, Dictionary<string, object> fieldAndValues)
        {
            try
            {
                return SettingManagerData.UpdateManagerSettings(this.userId,settingId,type, fieldAndValues);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "ManagerService.Console.Client/UpdateManagerSettings.\r\n{0}", ex.ToString());
                return false;
            }
        }
        #endregion

        #region Report
        internal List<OrderQueryEntity> GetOrderByInstrument(string exchangeCode,Guid instrumentId, Guid accountGroupId, OrderType orderType,
           bool isExecute, DateTime fromDate, DateTime toDate)
        {
            HashSet<Guid> accounts = this._OwnAccounts[exchangeCode];
            HashSet<Guid> instruments = this._OwnInstruments[exchangeCode];
            return ExchangeData.GetOrderByInstrument(exchangeCode, accounts,instruments, instrumentId, accountGroupId, orderType, isExecute, fromDate, toDate);
        }

        internal List<AccountGroupGNP> GetGroupNetPosition(string exchangeCode,bool showActualQuantity, string[] blotterCodeSelecteds)
        {
            List<AccountGroupGNP> accountGroupGNPs = null;
            try
            {
                ExchangeSystem exchangeSystem = MainService.ExchangeManager.GetExchangeSystem(exchangeCode);
                accountGroupGNPs = exchangeSystem.GetGroupNetPosition(showActualQuantity, blotterCodeSelecteds);

            }
            catch (Exception ex)
            {
                Logger.AddEvent(TraceEventType.Error, "Client.GetGroupNetPosition error:\r\n{0}", ex.ToString());
            }
            return accountGroupGNPs;
        }

        internal List<OpenInterestSummary> GetOpenInterestInstrumentSummary(string exchangeCode,bool isGroupByOriginCode, string[] blotterCodeSelecteds)
        {
            List<OpenInterestSummary> openInterestSummarys = null;
            try
            {
                ExchangeSystem exchangeSystem = MainService.ExchangeManager.GetExchangeSystem(exchangeCode);
                openInterestSummarys = exchangeSystem.GetOpenInterestInstrumentSummary(isGroupByOriginCode, blotterCodeSelecteds);
            }
            catch (Exception ex)
            {
                Logger.AddEvent(TraceEventType.Error, "Client.GetOpenInterestInstrumentSummary error:\r\n{0}", ex.ToString());
            }
            return openInterestSummarys;
        }

        internal List<OpenInterestSummary> GetOpenInterestAccountSummary(string exchangeCode, Guid[] accountIDs, Guid[] instrumentIDs, string[] blotterCodeSelecteds)
        {
            List<OpenInterestSummary> openInterestSummarys = null;
            try
            {
                ExchangeSystem exchangeSystem = MainService.ExchangeManager.GetExchangeSystem(exchangeCode);
                openInterestSummarys = exchangeSystem.GetOpenInterestAccountSummary(accountIDs, instrumentIDs, blotterCodeSelecteds);
            }
            catch (Exception ex)
            {
                Logger.AddEvent(TraceEventType.Error, "Client.GetOpenInterestAccountSummary error:\r\n{0}", ex.ToString());
            }
            return openInterestSummarys;
        }

        internal List<OpenInterestSummary> GetOpenInterestOrderSummary(string exchangeCode,Guid instrumentId, Guid accountId, iExchange.Common.AccountType accountType, string[] blotterCodeSelecteds)
        {
            List<OpenInterestSummary> openInterestSummarys = null;
            try
            {
                ExchangeSystem exchangeSystem = MainService.ExchangeManager.GetExchangeSystem(exchangeCode);
                openInterestSummarys = exchangeSystem.GetOpenInterestOrderSummary(accountId, accountType, instrumentId, blotterCodeSelecteds);
            }
            catch (Exception ex)
            {
                Logger.AddEvent(TraceEventType.Error, "Client.GetOpenInterestOrderSummary error:\r\n{0}", ex.ToString());
            }
            return openInterestSummarys;
        }
        #endregion

        #region Log Audit

        internal List<LogQuote> GetQuoteLogData(DateTime fromDate, DateTime toDate, LogType logType)
        {
            List<LogQuote> logQuotes = new List<LogQuote>();
            try
            {
                logQuotes = LogDataAccess.Instance.GetQuoteLogData(fromDate, toDate, logType);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "ManagerService.Console.Client/GetQuoteLogData.\r\n{0}", ex.ToString());
            }
            return logQuotes;
        }

        internal List<LogOrder> GetLogOrderData(DateTime fromDate, DateTime toDate, LogType logType)
        {
            List<LogOrder> logOrders = new List<LogOrder>();
            try
            {
                logOrders = LogDataAccess.Instance.GetLogOrderData(fromDate, toDate, logType);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "ManagerService.Console.Client/GetLogOrderData.\r\n{0}", ex.ToString());
            }
            return logOrders;
        }

        internal List<LogSetting> GetLogSettingData(DateTime fromDate, DateTime toDate, LogType logType)
        {
            List<LogSetting> logSettings = new List<LogSetting>();
            try
            {
                logSettings = LogDataAccess.Instance.GetLogSettingData(fromDate, toDate, logType);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "ManagerService.Console.Client/GetLogSettingData.\r\n{0}", ex.ToString());
            }
            return logSettings;
        }


        internal List<LogPrice> GetLogPriceData(DateTime fromDate, DateTime toDate, LogType logType)
        {
            List<LogPrice> logPricies = new List<LogPrice>();
            try
            {
                logPricies = LogDataAccess.Instance.GetLogPriceData(fromDate, toDate, logType);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "ManagerService.Console.Client/GetLogPriceData.\r\n{0}", ex.ToString());
            }
            return logPricies;
        }

        internal List<LogSourceChange> GetLogSourceChangeData(DateTime fromDate, DateTime toDate, LogType logType)
        {
            List<LogSourceChange> logSourceChanges = new List<LogSourceChange>();
            try
            {
                logSourceChanges = LogDataAccess.Instance.GetLogSourceChangeData(fromDate, toDate, logType);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "ManagerService.Console.Client/GetLogSourceChangeData.\r\n{0}", ex.ToString());
            }
            return logSourceChanges;
        }
        #endregion

        public void SetQuotationPolicyDetail(Guid relationId, QuotePolicyDetailsSetAction action, int changeValue)
        {
            List<string> originCodes = QuotationData.GetRelationInstrumentOriginCodes(relationId);
            StringBuilder str = new StringBuilder();
            str.Append("<Instruments>");
            foreach (string code in originCodes)
            {
                str.AppendFormat("<Instrument OriginCode=\"{0}\"/>", code);
            }
            str.Append("</Instruments>");
            List<InstrumentQuotationSet> quotePolicyChangeDetails = new List<InstrumentQuotationSet>();
            foreach (ExchangeSystemSetting set in MainService.ManagerSettings.ExchangeSystems)
            {
                List<InstrumentQuotationSet> quotePolicyDetails = QuotationData.UpdateQuotePolicyDetails(set.Code, str.ToString(), action.ToString(), changeValue);
                if (quotePolicyDetails.Count > 0)
                {
                    quotePolicyChangeDetails.AddRange(quotePolicyDetails);
                }
            }
            Logger.TraceEvent(TraceEventType.Information, "SetQuotationPolicyDetail.\r\n{0},{1},{2}", str.ToString(), action.ToString(), changeValue);
            UpdateInstrumentQuotationMessage message = new UpdateInstrumentQuotationMessage(quotePolicyChangeDetails);
            MainService.ClientManager.Dispatch(message);
        }

        public List<QuotePolicyRelation> GetQuotePolicyRelation()
        {
            List<QuotePolicyRelation> relations = QuotationData.GetQuotePolicyRelation();
            return relations;
        }
        public bool UpdateQuotationPolicy(InstrumentQuotationSet set)
        {
            bool isSuccess = false;
            string xmlUpdateStr = string.Format("<QuotePolicyDetail QuotePolicyID=\"{0}\" InstrumentID=\"{1}\" {2}=\"{3}\" xmlns=\"\" />", set.QoutePolicyId, set.InstrumentId, Enum.GetName(typeof(InstrumentQuotationEditType), set.type), set.Value);
            isSuccess = QuotationData.UpdateQuotationPolicy(set.ExchangeCode, xmlUpdateStr);            
            if (isSuccess)
            {
                if (set.type == InstrumentQuotationEditType.AutoAdjustPoints || set.type == InstrumentQuotationEditType.SpreadPoints || set.type == InstrumentQuotationEditType.PriceType || set.type == InstrumentQuotationEditType.IsOriginHiLo)
                {
                    MainService.ExchangeManager.UpdateQuotationServer(set.ExchangeCode, xmlUpdateStr);
                }
                List<InstrumentQuotationSet> quotePolicyDetails = new List<InstrumentQuotationSet>();
                quotePolicyDetails.Add(set);
                UpdateInstrumentQuotationMessage message = new UpdateInstrumentQuotationMessage(quotePolicyDetails);
                MainService.ClientManager.DispatchExcept(message,this);
            }
            return isSuccess;
        }

        public void UpdateInstrument(InstrumentQuotationSet set)
        {
            bool isSuccess = MainService.ExchangeManager.UpdateInstrument(set);
            if (isSuccess)
            {
                List<InstrumentQuotationSet> quotePolicyDetails = new List<InstrumentQuotationSet>();
                quotePolicyDetails.Add(set);
                UpdateInstrumentQuotationMessage message = new UpdateInstrumentQuotationMessage(quotePolicyDetails);
                MainService.ClientManager.DispatchExcept(message, this);
            }
        }

        public bool ExchangeSuspendResume(Dictionary<string, List<Guid>> instruments, bool resume)
        {
            return MainService.ExchangeManager.ExchangeSuspendResume(instruments, resume);   
        }

        public List<HistoryQuotationData> GetOriginQuotationForModifyAskBidHistory(string exchangeCode, Guid instrumentID, DateTime beginDateTime, string origin)
        {
            return QuotationData.GetOriginQuotationForModifyAskBidHistory(exchangeCode, instrumentID, beginDateTime, origin);
        }

        public UpdateHighLowBatchProcessInfo UpdateHighLow(string exchangeCode, Guid instrumentId, bool isOriginHiLo, string newInput, bool isUpdateHigh)
        {
            UpdateHighLowBatchProcessInfo info = new UpdateHighLowBatchProcessInfo();
            info = MainService.ExchangeManager.UpdateHighLow(exchangeCode, instrumentId, isOriginHiLo, newInput, isUpdateHigh);
            info.ExchangeCode = exchangeCode;
            return info;
        }

        public bool AddNewRelation(Guid id, string code, List<int> instruments)
        {
            bool isSuccess = false;
            isSuccess = QuotationData.AddNewRelation(id, code, instruments);
            return isSuccess;
        }

        internal int AddMetadataObject(IMetadataObject metadataObject)
        {
            int objectId = QuotationData.AddMetadataObject((dynamic)metadataObject);
            if (objectId > 0)
            {
                metadataObject.Id = objectId;
                MainService.QuotationManager.AddMetadataObject((dynamic)metadataObject);
                AddMetadataObjectMessage message = new AddMetadataObjectMessage() { MetadataObject = metadataObject };
                MainService.ClientManager.DispatchExcept(message, this);
            }
            return objectId;
        }

        internal int AddInstrument(InstrumentData instrumentData)
        {
            instrumentData.Instrument.Id = QuotationData.AddInstrument(instrumentData);

            List<IMetadataObject> metadataObjects = new List<IMetadataObject>();

            metadataObjects.Add(instrumentData.Instrument);
            if (instrumentData.Instrument.IsDerivative)
            {
                MainService.QuotationManager.AddMetadataObject(instrumentData.DerivativeRelation);
                metadataObjects.Add(instrumentData.DerivativeRelation);
            }
            else
            {
                MainService.QuotationManager.AddMetadataObject(instrumentData.PriceRangeCheckRule);
                metadataObjects.Add(instrumentData.PriceRangeCheckRule);
                if(instrumentData.WeightedPriceRule != null)
                {
                    MainService.QuotationManager.AddMetadataObject(instrumentData.WeightedPriceRule);
                    metadataObjects.Add(instrumentData.WeightedPriceRule);
                }
            }
            MainService.QuotationManager.AddMetadataObject(instrumentData.Instrument);

            AddMetadataObjectsMessage message = new AddMetadataObjectsMessage() { MetadataObjects = metadataObjects.ToArray() };
            MainService.ClientManager.DispatchExcept(message, this);

            return instrumentData.Instrument.Id;
        }

        internal void UpdateMetadataObject(MetadataType type, int objectId, Dictionary<string, object> fieldAndValues)
        {
            QuotationData.UpdateMetadataObject(type, objectId, fieldAndValues);
            MainService.QuotationManager.UpdateMetadataObject(type, objectId, fieldAndValues);
            UpdateData updateData = new UpdateData() { MetadataType = type, ObjectId = objectId, FieldsAndValues = fieldAndValues };
            UpdateMetadataMessage message = new UpdateMetadataMessage() { UpdateDatas = new UpdateData[] { updateData } };
            MainService.ClientManager.DispatchExcept(message, this);
        }
        internal void UpdateMetadataObjects(UpdateData[] updateDatas)
        {
            QuotationData.UpdateMetadataObjects(updateDatas);
            foreach (UpdateData item in updateDatas)
            {
                MainService.QuotationManager.UpdateMetadataObject(item.MetadataType, item.ObjectId, item.FieldsAndValues);
            }
            UpdateMetadataMessage message = new UpdateMetadataMessage() { UpdateDatas = updateDatas };
            MainService.ClientManager.DispatchExcept(message, this);
        }

        internal void UpdateMetadataObject(MetadataType type, int objectId, string field, object value)
        {
            Dictionary<string, object> fieldAndValues = new Dictionary<string, object>();
            fieldAndValues.Add(field, value);
            this.UpdateMetadataObject(type, objectId, fieldAndValues);
        }

        internal void DeleteMetadataObject(MetadataType type, int objectId)
        {
            QuotationData.DeleteMetadataObject(type, objectId);
            MainService.QuotationManager.DeleteMetadataObject(type, objectId);
            DeleteMetadataObjectMessage message = new DeleteMetadataObjectMessage() { ObjectId = objectId, MetadataType = type };
            MainService.ClientManager.DispatchExcept(message, this);
        }

        internal void SendQuotation(int instrumentSourceRelationId, double ask, double bid)
        {
            PrimitiveQuotation primitiveQuotation;
            MainService.QuotationManager.SendQuotation(instrumentSourceRelationId, ask, bid, out primitiveQuotation);

            LogPrice logPrice = LogManager.Instance.GetLogPriceEntity(this,ask, bid, primitiveQuotation.InstrumentId);
            WriteLogManager.WritePriceLog(logPrice);
        }

        internal void SwitchDefaultSource(SwitchRelationBooleanPropertyMessage message)
        {
            QuotationData.SwitchDefaultSource(message.OldRelationId, message.NewRelationId);
            MainService.ClientManager.Dispatch(message);
        }

        internal void ConfirmAbnormalQuotation(int instrumentId, int confirmId, bool accepted)
        {
            ConfirmResult confirmResult = MainService.QuotationManager.AbnormalQuotationManager.Confirm(instrumentId, confirmId, accepted);
            if (confirmResult.Confirmed)
            {
                LogPrice logPrice = LogManager.Instance.GetLogPriceEntity(this, instrumentId, confirmId, accepted, confirmResult);
                WriteLogManager.WritePriceLog(logPrice);
            }
        }

        internal void SuspendResume(int[] instrumentIds, bool resume)
        {
            MainService.ExchangeManager.SuspendResume(instrumentIds, resume);
            // TODO: Write audit log here

        }

        
    }
}
