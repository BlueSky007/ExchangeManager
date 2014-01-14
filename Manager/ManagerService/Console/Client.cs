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

namespace ManagerService.Console
{
    public class Client
    {
        private string _SessionId;
        private string _IP;
        private User _User;
        private IClientProxy _ClientProxy;
        private RelayEngine<Message> _MessageRelayEngine;
        private Language _Language;
        private Dictionary<string, List<Guid>> _AccountPermission;
        private Dictionary<string, List<Guid>> _InstrumentPermission;

        public Client(string sessionId, User user, IClientProxy clientProxy, Language language)
        {
            this._SessionId = sessionId;
            this._IP = ServiceHelper.GetIpAdreess();
            this._User = user;
            this._ClientProxy = clientProxy;
            this._Language = language;
            this._MessageRelayEngine = new RelayEngine<Message>(this.SendMessage, this.HandlEngineException);
        }

        public string SessionId { get { return this._SessionId; } }

        public string IP { get { return this._IP; } }
        public Guid userId { get { return this._User.UserId; } }
        public User user { get { return this._User; } }
        public Language language { get { return this._Language; } }

        public void Replace(string sessionId, IClientProxy clientProxy)
        {
            this._SessionId = sessionId;
            this._ClientProxy = clientProxy;
            this._MessageRelayEngine.Resume();
        }

        public void UpdateDataPermission(string exchangeCode, string type, Guid groupId, List<Guid> memberIds)
        {
            bool hasPermission = UserDataAccess.CheckPermission(userId,groupId, type, exchangeCode);
            if (hasPermission)
            {
                foreach (Guid memberId in memberIds)
                {
                    if (type == "Account")
                    {
                        if (!this._AccountPermission[exchangeCode].Contains(memberId))
                        {
                            this._AccountPermission[exchangeCode].Add(memberId);
                        }
                    }
                    else
                    {
                        if (!this._InstrumentPermission[exchangeCode].Contains(memberId))
                        {
                            this._InstrumentPermission[exchangeCode].Add(memberId);
                        }
                    }
                }
            }
            else
            {
                foreach (Guid memberId in memberIds)
                {
                    if (type == "Account")
                    {
                        if (this._AccountPermission[exchangeCode].Contains(memberId))
                        {
                            this._AccountPermission[exchangeCode].Remove(memberId);
                        }
                    }
                    else
                    {
                        if (this._InstrumentPermission[exchangeCode].Contains(memberId))
                        {
                            this._InstrumentPermission[exchangeCode].Remove(memberId);
                        }
                    }
                }
            }
        }

        public void UpdatePermission(Dictionary<string, List<Guid>> accountPermissions, Dictionary<string, List<Guid>> instrumentPermission)
        {
            this._AccountPermission = accountPermissions;
            this._InstrumentPermission = instrumentPermission;
        }

        public void InitPermissionData(List<DataPermission> dataPermission)
        {

        }

        public void AddMessage(Message message)
        {
            Message msg = this.Filter(message);

            if (msg != null)
            {
                this._MessageRelayEngine.AddItem(msg);
            }
        }

        public void Channel_Broken(object sender, EventArgs e)
        {
            this._MessageRelayEngine.Suspend();
            Logger.AddEvent(TraceEventType.Warning, "Client Channel_Broken of UserName:{0} sessionId:{1}", this.user.UserName, this._SessionId);
        }

        public void Close()
        {
            this._MessageRelayEngine.Stop();
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
            catch (Exception ex)
            {
                Logger.AddEvent(TraceEventType.Error, "Client.SendMessage error:\r\n{0}", ex.ToString());
            }
            return false;
        }

        private void HandlEngineException(Exception ex)
        {
            Logger.TraceEvent(TraceEventType.Error, "Client.HandlEngineException RelayEngine stopped:\r\n" + ex.ToString());
        }

        private Message Filter(Message message)
        {
            IFilterable filterableMessage = message as IFilterable;
            ExchangeMessage exchangeMessage = message as ExchangeMessage;
            if (filterableMessage != null && exchangeMessage != null)
            {
                List<Guid> accountPermissions = new List<Guid>();
                List<Guid> instrumentPermissions = new List<Guid>();
                this._AccountPermission.TryGetValue(exchangeMessage.ExchangeCode, out accountPermissions);
                this._InstrumentPermission.TryGetValue(exchangeMessage.ExchangeCode, out instrumentPermissions);

                if (filterableMessage.AccountId != null)
                {
                    if (!accountPermissions.Contains(filterableMessage.AccountId.Value))
                    {
                        message = null;
                    }
                }
                if (filterableMessage.InstrumentId != null)
                {
                    if (!instrumentPermissions.Contains(filterableMessage.InstrumentId.Value))
                    {
                        message = null;
                    }
                }
            }
            return message;
        }

        #region MainWindowFunction
        public string GetData(int value)
        {
            throw new NotImplementedException();
        }

        public FunctionTree GetFunctionTree()
        {
            try
            {
                FunctionTree tree = UserDataAccess.BuildFunctionTree(this.userId, this.language);
                return tree;
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "ManagerService.Console.ClientService/GetFunctionTree.\r\n{0}", ex.ToString());
                return new FunctionTree();
            }
        }

        public void SaveLayout(string layout, string content, string layoutName)
        {
            if ( this.userId != null && this.userId != Guid.Empty)
            {
                string path = string.Format("../../Layout/{0}", this.user.UserName);
                string layoutPath = string.Format("../../Layout/{0}/{1}_layout.xml", this.user.UserName, layoutName);
                string contentPath = string.Format("../../Layout/{0}/{1}_content.xml", this.user.UserName, layoutName);
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
            try
            {
                List<string> layouts = new List<string>();
                string dockLayout;
                string contentLayout;
                UserDataAccess.LoadLayout(this.user.UserName, layoutName, out dockLayout, out contentLayout);
                layouts.Add(dockLayout);
                layouts.Add(contentLayout);
                return layouts;
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "ManagerService/LoadLayout.\r\n{0}", ex.ToString());
                return new List<string>();
            }
        }
        #endregion

        #region UserAndRoleManager
        public bool ChangePassword(string currentPassword, string newPassword)
        {
            try
            {
                bool isSuccess = UserDataAccess.ChangePassword(this.userId, currentPassword, newPassword);
                return isSuccess;
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "ManagerService.Console.ClientService/ChangePassword.\r\n{0}", ex.ToString());
                return false;
            }
        }

        public Dictionary<string, Tuple<string,bool>> GetAccessPermissions()
        {
            try
            {
                Dictionary<string, Tuple<string, bool>> permissions = UserDataAccess.GetAccessPermissions(this.userId, this.language);
                if (this.user.IsInRole("admin"))
                {
                    permissions.Clear();
                    permissions.Add("admin", Tuple.Create("admin",true));
                }
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
                List<RoleData> roles = UserDataAccess.GetRoles(this.language.ToString());
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
                if (userId == this.userId)
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
                }
                if (isSuccess)
                {

                    Thread updatePermission = new Thread(delegate()
                    {
                        MainService.ClientManager.UpdatePermission(role);
                        return;
                    });
                    updatePermission.IsBackground = true;
                    updatePermission.Start();
                }
                if (this.user.IsInRole(role.RoleId) && isSuccess)
                {
                    List<DataPermission> dataPermissions = new List<DataPermission>();
                    Dictionary<string, List<Guid>> accountPermissions = new Dictionary<string, List<Guid>>();
                    Dictionary<string, List<Guid>> instrumentPermissions = new Dictionary<string, List<Guid>>();
                    foreach (ExchangeSystemSetting item in MainService.ManagerSettings.ExchangeSystems)
                    {
                        bool deafultStatus = false;
                        List<Guid> accountMemberIds = new List<Guid>();
                        List<Guid> instrumentMemberIds = new List<Guid>();
                        List<RoleDataPermission> systemPermissions = role.DataPermissions.FindAll(delegate(RoleDataPermission data)
                        {
                            return data.IExchangeCode == item.Code;
                        });
                        RoleDataPermission account = systemPermissions.SingleOrDefault(r => r.Type == DataObjectType.Account && r.Level == 2);
                        if (account != null)
                        {
                            deafultStatus = account.IsAllow;
                        }
                        else
                        {
                            RoleDataPermission exchange = systemPermissions.SingleOrDefault(r => r.Level == 1);
                            if (exchange != null)
                            {
                                deafultStatus = exchange.IsAllow;
                            }
                        }
                        accountMemberIds.AddRange(ExchangeData.GetNewMemberIds(item.Code, deafultStatus, systemPermissions, DataObjectType.Account));
                        RoleDataPermission instrument = systemPermissions.SingleOrDefault(r => r.Type == DataObjectType.Instrument && r.Level == 2);
                        if (instrument != null)
                        {
                            deafultStatus = instrument.IsAllow;
                        }
                        else
                        {
                            RoleDataPermission exchange = systemPermissions.SingleOrDefault(r => r.Level == 1);
                            if (exchange != null)
                            {
                                deafultStatus = exchange.IsAllow;
                            }
                        }
                        instrumentMemberIds.AddRange(ExchangeData.GetNewMemberIds(item.Code, deafultStatus, systemPermissions, DataObjectType.Instrument));
                        accountPermissions.Add(item.Code, accountMemberIds);
                        instrumentPermissions.Add(item.Code, instrumentMemberIds);
                    }

                    this.UpdatePermission(accountPermissions, instrumentPermissions);
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
                    return UserDataAccess.DeleteRole(roleId, this.userId);
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
                    WriteLogManager.WriteQuoteOrderLog(logEntity);
                }
            }
            catch (Exception ex)
            {
                Logger.AddEvent(TraceEventType.Error, "Client.Cancel error:\r\n{0}", ex.ToString());
            }
            return transactionError;
        }

        internal void ResetHit(Guid[] orderIds)
        {
            string exchangeCode = "WF01";
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

        internal AccountInformation GetAcountInfo(Guid transactionId)
        {
            AccountInformation accountInfor = new AccountInformation();
            string exchangeCode = "WF01";
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

        internal List<string> LoadSettingsParameters(Guid userId)
        {
            List<string> parameters = new List<string>();
            try
            {
                foreach (ExchangeSystem exchangeSystem in MainService.ExchangeManager.GetExchangeSystems())
                {
                    string parameterStr = string.Empty;
                    string exchangeCode = exchangeSystem.ExchangeCode;
                    parameterStr = ExchangeData.LoadSettingsParameter(exchangeCode,userId);
                    parameters.Add(parameterStr);
                }
            }
            catch (Exception ex)
            {
                Logger.AddEvent(TraceEventType.Error, "ClientService.LoadSettingsParameters error:\r\n{0}", ex.ToString());
            }
            return parameters;
        }
        #endregion


        #region Setting Manager
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
        #endregion

        #region Report
        internal List<OrderQueryEntity> GetOrderByInstrument(Guid instrumentId, Guid accountGroupId, OrderType orderType,
           bool isExecute, DateTime fromDate, DateTime toDate)
        {
            return ExchangeData.GetOrderByInstrument(this.userId,instrumentId, accountGroupId, orderType, isExecute, fromDate, toDate);
        }

        internal List<AccountGroupGNP> GetGroupNetPosition()
        {
            string exchangeCode = "WF01";
            List<AccountGroupGNP> accountGroupGNPs = null;
            try
            {
                ExchangeSystem exchangeSystem = MainService.ExchangeManager.GetExchangeSystem(exchangeCode);
                accountGroupGNPs = exchangeSystem.GetGroupNetPosition();

            }
            catch (Exception ex)
            {
                Logger.AddEvent(TraceEventType.Error, "Client.GetGroupNetPosition error:\r\n{0}", ex.ToString());
            }
            return accountGroupGNPs;
        }

        internal List<OpenInterestSummary> GetInstrumentSummary(bool isGroupByOriginCode, string[] blotterCodeSelecteds)
        {
            string exchangeCode = "WF01";
            List<OpenInterestSummary> openInterestSummarys = null;
            try
            {
                ExchangeSystem exchangeSystem = MainService.ExchangeManager.GetExchangeSystem(exchangeCode);
                openInterestSummarys = exchangeSystem.GetInstrumentSummary(isGroupByOriginCode, blotterCodeSelecteds);
            }
            catch (Exception ex)
            {
                Logger.AddEvent(TraceEventType.Error, "Client.GetInstrumentSummary error:\r\n{0}", ex.ToString());
            }
            return openInterestSummarys;
        }

        internal List<OpenInterestSummary> GetAccountSummary(Guid instrumentId, string[] blotterCodeSelecteds)
        {
            string exchangeCode = "WF01";
            List<OpenInterestSummary> openInterestSummarys = null;
            try
            {
                ExchangeSystem exchangeSystem = MainService.ExchangeManager.GetExchangeSystem(exchangeCode);
                openInterestSummarys = exchangeSystem.GetAccountSummary(instrumentId,blotterCodeSelecteds);
            }
            catch (Exception ex)
            {
                Logger.AddEvent(TraceEventType.Error, "Client.GetAccountSummary error:\r\n{0}", ex.ToString());
            }
            return openInterestSummarys;
        }

        internal List<OpenInterestSummary> GetOrderSummary(Guid instrumentId, Guid accountId, iExchange.Common.AccountType accountType, string[] blotterCodeSelecteds)
        {
            string exchangeCode = "WF01";
            List<OpenInterestSummary> openInterestSummarys = null;
            try
            {
                ExchangeSystem exchangeSystem = MainService.ExchangeManager.GetExchangeSystem(exchangeCode);
                openInterestSummarys = exchangeSystem.GetOrderSummary(instrumentId, accountId, accountType, blotterCodeSelecteds);
            }
            catch (Exception ex)
            {
                Logger.AddEvent(TraceEventType.Error, "Client.GetOrderSummary error:\r\n{0}", ex.ToString());
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
            List<QuotePolicyDetailSet> quotePolicyChangeDetails = new List<QuotePolicyDetailSet>();
            foreach (ExchangeSystemSetting set in MainService.ManagerSettings.ExchangeSystems)
            {
                List<QuotePolicyDetailSet> quotePolicyDetails = QuotationData.UpdateQuotePolicyDetails(set.Code, str.ToString(), action.ToString(), changeValue);
                if (quotePolicyDetails.Count > 0)
                {
                    quotePolicyChangeDetails.AddRange(quotePolicyDetails);
                }
            }
            Logger.TraceEvent(TraceEventType.Information, "SetQuotationPolicyDetail.\r\n{0},{1},{2}", str.ToString(), action.ToString(), changeValue);
            UpdateQuotePolicyDetailMessage message = new UpdateQuotePolicyDetailMessage(quotePolicyChangeDetails);
            MainService.ClientManager.Dispatch(message);
        }

        public List<QuotePolicyRelation> GetQuotePolicyRelation()
        {
            List<QuotePolicyRelation> relations = QuotationData.GetQuotePolicyRelation();
            return relations;
        }
        public bool UpdateQuotationPolicy(QuotePolicyDetailSet set)
        {
            bool isSuccess = false;
            string xmlUpdateStr = string.Format("<QuotePolicyDetail QuotePolicyID=\"{0}\" InstrumentID=\"{1}\" {2}=\"{3}\" xmlns=\"\" />", set.QoutePolicyId, set.InstrumentId, Enum.GetName(typeof(QuotePolicyEditType), set.type), set.Value);
            isSuccess = QuotationData.UpdateQuotationPolicy(set.ExchangeCode, xmlUpdateStr);            
            if (isSuccess)
            {
                List<QuotePolicyDetailSet> quotePolicyDetails = new List<QuotePolicyDetailSet>();
                quotePolicyDetails.Add(set);
                UpdateQuotePolicyDetailMessage message = new UpdateQuotePolicyDetailMessage(quotePolicyDetails);
                MainService.ClientManager.DispatchExcept(message,this);
            }
            return isSuccess;
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

            LogEntity logEntity = new LogEntity() { Id = Guid.NewGuid(), UserId = this.userId, UserName = this.user.UserName, IP = this._IP, ExchangeCode = string.Empty, Event = "SendQuotation", Timestamp = DateTime.Now };
            LogPrice logPrice = new LogPrice(logEntity);
            logPrice.InstrumentId = primitiveQuotation.InstrumentId;
            logPrice.InstrumentCode = MainService.QuotationManager.ConfigMetadata.Instruments[primitiveQuotation.InstrumentId].Code;
            logPrice.OperationType = PriceOperationType.SendPrice;
            string format = string.Format("F{0}", MainService.QuotationManager.ConfigMetadata.Instruments[primitiveQuotation.InstrumentId].DecimalPlace);
            logPrice.Ask = ask.ToString(format);
            logPrice.Bid = bid.ToString(format);
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
                LogEntity logEntity = new LogEntity() { Id = Guid.NewGuid(), UserId = this.userId, UserName = this.user.UserName, IP = this._IP, ExchangeCode = string.Empty, Event = "SendQuotation", Timestamp = DateTime.Now };
                LogPrice logPrice = new LogPrice(logEntity);
                logPrice.InstrumentId = confirmResult.SourceQuotation.InstrumentId;
                logPrice.InstrumentCode = confirmResult.SourceQuotation.InstrumentCode;
                logPrice.OperationType = accepted ? PriceOperationType.OutOfRangeAccept : PriceOperationType.OutOfRangeReject;
                logPrice.OutOfRangeType = confirmResult.SourceQuotation.OutOfRangeType;
                logPrice.Bid = confirmResult.SourceQuotation.PrimitiveQuotation.Ask;
                logPrice.Ask = confirmResult.SourceQuotation.PrimitiveQuotation.Bid;
                logPrice.Diff = confirmResult.SourceQuotation.DiffPoints.ToString();
                WriteLogManager.WritePriceLog(logPrice);
            }
        }
    }
}
