using Manager.Common;
using Manager.Common.LogEntities;
using Manager.Common.QuotationEntities;
using Manager.Common.ReportEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Xml;

namespace ManagerService.Console
{
    [ServiceContract(CallbackContract = typeof(IClientProxy), SessionMode = SessionMode.Required)]
    public interface IClientService
    {
        [OperationContract]
        LoginResult Login(string userName, string password, string oldSessionId, Language language);

        [OperationContract(IsTerminating = true)]
        void Logout();

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
        Dictionary<string, string> GetAccessPermissions();

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

        [OperationContract(IsInitiating = false)]
        void AbandonQuote(List<Answer> abandonQuotePrices);

        [OperationContract(IsInitiating = false)]
        void SendQuotePrice(List<Answer> sendQuotePrices);

        [OperationContract(IsInitiating = true)]
        TransactionError AcceptPlace(Guid transactionId, LogOrder logEntity);

        [OperationContract(IsInitiating = false)]
        TransactionError CancelPlace(Guid transactionId, CancelReason cancelReason);

        [OperationContract(IsInitiating = false)]
        TransactionError Execute(Guid transactionId, string buyPrice, string sellPrice, decimal lot, Guid orderId, LogOrder logEntity);

        [OperationContract(IsInitiating = false)]
        TransactionError Cancel(Guid transactionId,CancelReason cancelReason,LogOrder logEntity);

        [OperationContract(IsInitiating = false)]
        void ResetHit(Guid[] orderIds);

        [OperationContract(IsInitiating = false)]
        AccountInformation GetAcountInfo(Guid transactionId);

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

        #region Report
        [OperationContract(IsInitiating = false)]
        List<OrderQueryEntity> GetOrderByInstrument(Guid instrumentId, Guid accountGroupId, OrderType orderType,
            bool isExecute, DateTime fromDate, DateTime toDate);
        #endregion

        #region QuotationManager
        [OperationContract(IsInitiating = false)]
        ConfigMetadata GetConfigMetadata();

        [OperationContract(IsInitiating = false)]
        int AddMetadataObject(MetadataType type, Dictionary<string, string> fields);

        [OperationContract(IsInitiating = false)]
        bool UpdateMetadataObject(MetadataType type, int objectId, Dictionary<string, string> fields);

        [OperationContract(IsInitiating = false)]
        bool DeleteMetadataObject(MetadataType type, int objectId);

        #endregion
    }

    [ServiceContract]
    public interface IClientProxy
    {
        [OperationContract(IsOneWay = true)]
        void SendMessage(Message message);
    }
}
