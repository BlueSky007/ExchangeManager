using iExchange.Common.Manager;
using Manager.Common;
using Manager.Common.LogEntities;
using Manager.Common.QuotationEntities;
using Manager.Common.ReportEntities;
using Manager.Common.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Xml;
using AccountType = iExchange.Common.AccountType;
using TransactionError = iExchange.Common.TransactionError;
using CancelReason = iExchange.Common.CancelReason;
using OrderType = iExchange.Common.OrderType;

namespace ManagerService.Console
{
    [ServiceKnownType("GetKnownTypes", typeof(KnownTypes))]
    [ServiceContract(CallbackContract = typeof(IClientProxy), SessionMode = SessionMode.Required)]
    public interface IClientService
    {
        [OperationContract]
        LoginResult Login(string userName, string password, string oldSessionId, Language language);

        [OperationContract(IsTerminating = true)]
        void Logout();

        [OperationContract(IsInitiating = false)]
        InitializeData GetInitializeData();

        [OperationContract(IsInitiating = false)]
        FunctionTree GetFunctionTree();

        [OperationContract(IsInitiating = false)]
        void SaveLayout(string layout, string content,string layoutName);

        [OperationContract(IsInitiating = false)]
        List<string> LoadLayout(string layoutName);

        #region UserManager
        [OperationContract(IsInitiating = false)]
        bool ChangePassword(string currentPassword, string newPassword);

        [OperationContract(IsInitiating = false)]
        Dictionary<string, List<FuncPermissionStatus>> GetAccessPermissions();

        [OperationContract(IsInitiating = false)]
        List<UserData> GetUserData();

        [OperationContract(IsInitiating = false)]
        List<RoleData> GetRoles();

        [OperationContract(IsInitiating = false)]
        List<RoleFunctonPermission> GetAllFunctionPermission();

        [OperationContract(IsInitiating = false)]
        List<RoleDataPermission> GetAllDataPermission();

        [OperationContract(IsInitiating = false)]
        bool UpdateUsers(UserData user, string password, bool isNewUser);

        [OperationContract(IsInitiating = false)]
        bool DeleteUser(Guid userId);

        [OperationContract(IsInitiating = false)]
        bool UpdateRole(RoleData role, bool isNewRole);

        [OperationContract(IsInitiating = false)]
        bool DeleteRole(int roleId);
        #endregion

        #region DealingConsole
        [OperationContract(IsInitiating = false)]
        void AbandonQuote(List<Answer> abandonQuotePrices);

        [OperationContract(IsInitiating = false)]
        void SendQuotePrice(List<Answer> sendQuotePrices);

        [OperationContract(IsInitiating = true)]
        TransactionError AcceptPlace(Guid transactionId, LogOrder logEntity);

        [OperationContract(IsInitiating = false)]
        TransactionError CancelPlace(Guid transactionId, CancelReason cancelReason);

        [OperationContract(IsInitiating = false)]
        TransactionResult Execute(Guid transactionId, string buyPrice, string sellPrice, decimal lot, Guid orderId, LogOrder logEntity);

        [OperationContract(IsInitiating = false)]
        TransactionError Cancel(Guid transactionId,CancelReason cancelReason,LogOrder logEntity);

        [OperationContract(IsInitiating = false)]
        void ResetHit(string exchangeCode,Guid[] orderIds);

        [OperationContract(IsInitiating = false)]
        AccountInformation GetAcountInfo(string exchangeCode,Guid transactionId);
       
        #endregion 

        #region Setting Manager
        [OperationContract(IsInitiating = false)]
        SettingsParameter LoadSettingsParameters();

        [OperationContract(IsInitiating = false)]
        List<ParameterDefine> LoadParameterDefine();

        [OperationContract(IsInitiating = false)]
        bool CreateTaskScheduler(TaskScheduler taskScheduler);

        [OperationContract(IsInitiating = false)]
        bool EditorTaskScheduler(TaskScheduler taskScheduler);

        [OperationContract(IsInitiating = false)]
        void EnableTaskScheduler(TaskScheduler taskScheduler);

        [OperationContract(IsInitiating = false)]
        void StartRunTaskScheduler(TaskScheduler taskScheduler);

        [OperationContract(IsInitiating = false)]
        bool DeleteTaskScheduler(TaskScheduler taskScheduler);
        
        [OperationContract(IsInitiating = false)]
        List<TaskScheduler> GetTaskSchedulersData();

        [OperationContract(IsInitiating = false)]
        bool UpdateManagerSettings(Guid settingId,SettingParameterType type, Dictionary<string, object> fieldAndValues);
        #endregion

        #region LogAudit
        [OperationContract(IsInitiating = false)]
        List<LogQuote> GetQuoteLogData(DateTime fromDate, DateTime toDate, LogType logType);

        [OperationContract(IsInitiating = false)]
        List<LogOrder> GetLogOrderData(DateTime fromDate, DateTime toDate, LogType logType);

        [OperationContract(IsInitiating = false)]
        List<LogSetting> GetLogSettingData(DateTime fromDate, DateTime toDate, LogType logType);

        [OperationContract(IsInitiating = false)]
        List<LogPrice> GetLogPriceData(DateTime fromDate, DateTime toDate, LogType logType);

        [OperationContract(IsInitiating = false)]
        List<LogSourceChange> GetLogSourceChangeData(DateTime fromDate, DateTime toDate, LogType logType);
        #endregion

        #region Report
        [OperationContract(IsInitiating = false)]
        List<OrderQueryEntity> GetOrderByInstrument(Guid instrumentId, Guid accountGroupId, OrderType orderType,
            bool isExecute, DateTime fromDate, DateTime toDate);

        [OperationContract(IsInitiating = false)]
        List<AccountGroupGNP> GetGroupNetPosition();

        [OperationContract(IsInitiating = false)]
        List<OpenInterestSummary> GetInstrumentSummary(bool isGroupByOriginCode, string[] blotterCodeSelecteds);

        [OperationContract(IsInitiating = false)]
        List<OpenInterestSummary> GetAccountSummary(Guid instrumentId,string[] blotterCodeSelecteds);

        [OperationContract(IsInitiating = false)]
        List<OpenInterestSummary> GetOrderSummary(Guid instrumentId, Guid accountId,AccountType accountType, string[] blotterCodeSelecteds);
        #endregion

        #region QuotationManager
        [OperationContract(IsInitiating = false)]
        ConfigMetadata GetConfigMetadata();

        [OperationContract(IsInitiating = false)]
        int AddMetadataObject(IMetadataObject metadataObject);

        [OperationContract(IsInitiating = false)]
        int AddInstrument(InstrumentData instrumentData);

        [OperationContract(IsInitiating = false)]
        bool UpdateMetadataObject(MetadataType type, int objectId, Dictionary<string, object> fieldAndValues);

        [OperationContract(IsInitiating = false)]
        bool UpdateMetadataObjects(UpdateData[] updateDatas);

        [OperationContract(IsInitiating = false)]
        bool UpdateMetadataObjectField(MetadataType type, int objectId, string field, object value);

        [OperationContract(IsInitiating = false)]
        bool DeleteMetadataObject(MetadataType type, int objectId);

        [OperationContract(IsInitiating = false)]
        void SendQuotation(int instrumentSourceRelationId, double ask, double bid);

        [OperationContract(IsInitiating = false)]
        void SwitchDefaultSource(SwitchRelationBooleanPropertyMessage message);

        [OperationContract(IsInitiating = false)]
        void UpdateQuotationPolicy(InstrumentQuotationSet set);

        [OperationContract(IsInitiating = false)]
        void SetQuotationPolicyDetail(Guid relationId, QuotePolicyDetailsSetAction action, int changeValue);

        [OperationContract(IsInitiating = false)]
        List<QuotePolicyRelation> GetQuotePolicyRelation();

        [OperationContract(IsInitiating = false)]
        bool AddNewRelation(Guid id, string code, List<int> instruments);

        [OperationContract(IsInitiating = false)]
        void ConfirmAbnormalQuotation(int instrumentId, int confirmId, bool accepted);

        [OperationContract(IsInitiating = false)]
        void SuspendResume(int[] instrumentIds, bool resume);

        [OperationContract(IsInitiating = false)]
        void UpdateInstrument(InstrumentQuotationSet set);

        [OperationContract(IsInitiating = false)]
        bool ExchangeSuspendResume(Dictionary<string, List<Guid>> instruments, bool resume);
        #endregion
    }

    //[ServiceKnownType("GetCallbackKnownTypes", typeof(KnownTypes))]
    [ServiceContract]
    public interface IClientProxy
    {
        [OperationContract(IsOneWay = true)]
        void SendMessage(Message message);
    }
}
