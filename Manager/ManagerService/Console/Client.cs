using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Manager.Common;
using System.Diagnostics;
using ManagerService.Exchange;
using ManagerService.DataAccess;
using Manager.Common.QuotationEntities;
using System.Threading;
using System.IO;
using System.ServiceModel;

namespace ManagerService.Console
{
    public class Client
    {
        private string _SessionId;
        private User _User;
        private IClientProxy _ClientProxy;
        private RelayEngine<Message> _MessageRelayEngine;
        private Language _Language;
        private Dictionary<string, List<Guid>> _AccountPermission;
        private Dictionary<string, List<Guid>> _InstrumentPermission;

        public Client(string sessionId, User user, IClientProxy clientProxy, Language language, Dictionary<string, List<Guid>> accountPermissions, Dictionary<string, List<Guid>> instrumentPermissions)
        {
            this._SessionId = sessionId;
            this._User = user;
            this._ClientProxy = clientProxy;
            this._Language = language;
            this._AccountPermission = accountPermissions;
            this._InstrumentPermission = instrumentPermissions;
            this._MessageRelayEngine = new RelayEngine<Message>(this.SendMessage, this.HandlEngineException);
        }

        public string SessionId { get { return this._SessionId; } }
        public Guid userId { get { return this._User.UserId; } }
        public User user { get { return this._User; } }
        public Language language { get { return this._Language; } }

        public void Replace(string sessionId, IClientProxy clientProxy, Dictionary<string, List<Guid>> accountPermissions, Dictionary<string, List<Guid>> instrumentPermissions)
        {
            this._SessionId = sessionId;
            this._ClientProxy = clientProxy;
            this._AccountPermission = accountPermissions;
            this._InstrumentPermission = instrumentPermissions;
            this._MessageRelayEngine.Resume();
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
        }

        public void Close()
        {
            this._MessageRelayEngine.Stop();
        }

        public ConfigMetadata GetConfigMetadata()
        {
            return Manager.QuotationManager.ConfigMetadata;
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
            Logger.TraceEvent(TraceEventType.Error, "ExchangeSystem.HandlEngineException RelayEngine stopped:\r\n" + ex.ToString());
        }

        private Message Filter(Message message)
        {
            IFilterable filterableMessage = message as IFilterable;
            // TODO: perform filter here
            if (filterableMessage.AccountId != null)
            {
            }
            if (filterableMessage.InstrumentId != null)
            {
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

        public Dictionary<string, string> GetAccessPermissions()
        {
            try
            {
                Dictionary<string, string> permissions = UserDataAccess.GetAccessPermissions(this.userId, this.language);
                if (this.user.IsInRole("admin"))
                {
                    permissions.Add("admin", "admin");
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
                List<RoleDataPermission> allData = UserDataAccess.GetAllDataPermissions(Manager.ManagerSettings.ExchangeSystems, this.language.ToString());
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
                        Manager.ClientManager.UpdatePermission(role);
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
                    foreach (ExchangeSystemSetting item in Manager.ManagerSettings.ExchangeSystems)
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

        public TransactionError AcceptPlace(Guid transactionId, LogOrder logEntity)
        {
            TransactionError transactionError = TransactionError.OK;
            string exchangeCode = "WF01";
            try
            {
                ExchangeSystem exchangeSystem = Manager.ExchangeManager.GetExchangeSystem(exchangeCode);
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

        public TransactionError CancelPlace(Guid transactionId, CancelReason cancelReason)
        {
            TransactionError transactionError = TransactionError.OK;
            string exchangeCode = "WF01";
            try
            {
                ExchangeSystem exchangeSystem = Manager.ExchangeManager.GetExchangeSystem(exchangeCode);
                transactionError = exchangeSystem.CancelPlace(transactionId, cancelReason);
            }
            catch (Exception ex)
            {
                Logger.AddEvent(TraceEventType.Error, "Client.CancelPlace error:\r\n{0}", ex.ToString());
            }
            return transactionError;
        }

        public TransactionError Execute(Guid transactionId, string buyPrice, string sellPrice, decimal lot, Guid orderId, LogOrder logEntity)
        {
            TransactionError transactionError = TransactionError.OK;
            string exchangeCode = logEntity.ExchangeCode;
            try
            {
                ExchangeSystem exchangeSystem = Manager.ExchangeManager.GetExchangeSystem(exchangeCode);
                transactionError = exchangeSystem.Execute(transactionId, buyPrice, sellPrice, lot, orderId);

                if (transactionError == TransactionError.OK)
                {
                    WriteLogManager.WriteQuoteOrderLog(logEntity);
                }
            }
            catch (Exception ex)
            {
                Logger.AddEvent(TraceEventType.Error, "Client.Execute error:\r\n{0}", ex.ToString());
            }
            return transactionError;
        }

        public TransactionError Cancel(Guid transactionId, CancelReason cancelReason, LogOrder logEntity)
        {
            TransactionError transactionError = TransactionError.OK;
            string exchangeCode = logEntity.ExchangeCode;
            try
            {
                ExchangeSystem exchangeSystem = Manager.ExchangeManager.GetExchangeSystem(exchangeCode);
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

        public void ResetHit(Guid[] orderIds)
        {
            try
            {
                //this.StateServer.ResetHit(token, orderIDs);
            }
            catch (Exception ex)
            {
                Logger.AddEvent(TraceEventType.Error, "Client.ResetHit error:\r\n{0}", ex.ToString());
            }
        }

        public AccountInformation GetAcountInfo(Guid transactionId)
        {
            AccountInformation accountInfor = new AccountInformation();
            try
            {
                //just test data
                accountInfor.AccountId = new Guid("9538eb6e-57b1-45fa-8595-58df7aabcfc9");
                accountInfor.InstrumentId = new Guid("66adc06c-c5fe-4428-867f-be97650eb3b1");
                accountInfor.Balance = 88888888;
                accountInfor.Equity = 10000000;
                accountInfor.Necessary = 99999999;
                accountInfor.BuyLotBalanceSum = 100;
                accountInfor.SellLotBalanceSum = 200;
                accountInfor.Usable = accountInfor.Equity - accountInfor.Necessary;
            }
            catch (Exception ex)
            {
                Logger.AddEvent(TraceEventType.Error, "ClientService.GetAcountInfo error:\r\n{0}", ex.ToString());
            }
            return accountInfor;
        }
        #endregion

        #region Log Audit

        public List<LogQuote> GetQuoteLogData(DateTime fromDate, DateTime toDate, LogType logType)
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

        public List<LogOrder> GetLogOrderData(DateTime fromDate, DateTime toDate, LogType logType)
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

        public List<LogPrice> GetLogPriceData(DateTime fromDate, DateTime toDate, LogType logType)
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

        public List<LogSourceChange> GetLogSourceChangeData(DateTime fromDate, DateTime toDate, LogType logType)
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

        internal bool AddQuotationSource(QuotationSource quotationSource)
        {
            if (QuotationData.AddQuotationSource(quotationSource))
            {
                // TODO: Update memory and notify here
                Manager.QuotationManager.UpdateMetadata(quotationSource);
                return true;
            }
            return false;
        }
    }
}
