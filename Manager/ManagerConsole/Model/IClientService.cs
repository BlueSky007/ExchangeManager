using Manager.Common;
using Manager.Common.LogEntities;
using Manager.Common.QuotationEntities;
using Manager.Common.ReportEntities;
using ManagerConsole.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Xml;
using AccountGroupGNP = iExchange.Common.Manager.AccountGroupGNP;
using AccountType = iExchange.Common.AccountType;
using OpenInterestSummary = iExchange.Common.Manager.OpenInterestSummary;

namespace ManagerConsole.Model
{
    [ServiceKnownType("GetKnownTypes", typeof(KnownTypes))]
    [ServiceContract(CallbackContract = typeof(IClientProxy), SessionMode = SessionMode.Required)]
    public interface IClientService
    {
        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginLogin(string userName, string password, string oldSessionId, Language language, AsyncCallback callback, object asyncState);
        LoginResult EndLogin(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginGetInitializeData(AsyncCallback callback, object asyncState);
        List<InitializeData> EndGetInitializeData(IAsyncResult result);


        [OperationContract(IsInitiating = false)]
        FunctionTree GetFunctionTree();

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginLogout(AsyncCallback callback, object asyncState);
        void EndLogout(IAsyncResult result);

        [OperationContract(IsInitiating = false)]
        void SaveLayout(string layout, string content, string layoutName);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginLoadLayout(string layoutName, AsyncCallback callback, object asyncState);
        List<string> EndLoadLayout(IAsyncResult result);

        #region UserManager
        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginGetAccessPermissions(AsyncCallback callback, object asyncState);
        Dictionary<string, Tuple<string, bool>> EndGetAccessPermissions(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginGetUserData(AsyncCallback callback, object asyncState);
        List<UserData> EndGetUserData(IAsyncResult result);

        [OperationContract(IsInitiating = false)]
        List<RoleData> GetRoles();

        [OperationContract(IsInitiating = false)]
        List<RoleFunctonPermission> GetAllFunctionPermission();

        [OperationContract(IsInitiating = false)]
        List<RoleDataPermission> GetAllDataPermission();

        [OperationContract(IsInitiating = false)]
        bool ChangePassword(string currentPassword, string newPassword);


        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginUpdateUsers(UserData user, string password, bool isNewUser, AsyncCallback callback, object asyncState);
        bool EndUpdateUsers(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginDeleteUser(Guid userId, AsyncCallback callback, object asyncState);
        bool EndDeleteUser(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginUpdateRole(RoleData role, bool isNewRole, AsyncCallback callback, object asyncState);
        bool EndUpdateRole(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginDeleteRole(int roleId, AsyncCallback callback, object asyncState);
        bool EndDeleteRole(IAsyncResult result);

        #endregion

        #region QuotePrice
        [OperationContract(IsInitiating = false)]
        void AbandonQuote(List<Answer> abandonQuotePrices);

        [OperationContract(IsInitiating = false)]
        void SendQuotePrice(List<Answer> sendQuotePrices);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginAcceptPlace(Guid transactionId, LogOrder logEntity, AsyncCallback callback, object asyncState);
        TransactionError EndAcceptPlace(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginCancelPlace(Guid transactionId, CancelReason cancelReason, AsyncCallback callback, object asyncState);
        TransactionError EndCancelPlace(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginExecute(Guid transactionId, string buyPrice, string sellPrice, decimal lot, Guid orderId, LogOrder logEntity, AsyncCallback callback, object asyncState);
        TransactionError EndExecute(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginCancel(Guid transactionId, CancelReason cancelReason, LogOrder logEntity, AsyncCallback callback, object asyncState);
        TransactionError EndCancel(IAsyncResult result);

        [OperationContract(IsInitiating = false)]
        void ResetHit(Guid[] orderIds);

        [OperationContract(IsInitiating = false)]
        AccountInformation GetAcountInfo(Guid transactionId);

        #endregion

        #region Report
        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginGetOrderByInstrument(Guid instrumentId, Guid accountGroupId, OrderType orderType,
            bool isExecute, DateTime fromDate, DateTime toDate, AsyncCallback callback, object asyncState);
        List<OrderQueryEntity> EndGetOrderByInstrument(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginGetGroupNetPosition(AsyncCallback callback, object asyncState);
        List<AccountGroupGNP> EndGetGroupNetPosition(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginGetInstrumentSummary(bool isGroupByOriginCode, string[] blotterCodeSelecteds, AsyncCallback callback, object asyncState);
        List<OpenInterestSummary> EndGetInstrumentSummary(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginGetAccountSummary(Guid instrumentId,string[] blotterCodeSelecteds, AsyncCallback callback, object asyncState);
        List<OpenInterestSummary> EndGetAccountSummary(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginGetOrderSummary(Guid instrumentId,Guid accountId,AccountType accountType,string[] blotterCodeSelecteds, AsyncCallback callback, object asyncState);
        List<OpenInterestSummary> EndGetOrderSummary(IAsyncResult result);

        #endregion

        #region Log Audit
        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginGetQuoteLogData(DateTime fromDate, DateTime toDate, LogType logType, AsyncCallback callback, object asyncStatus);
        List<LogQuote> EndGetQuoteLogData(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginGetLogOrderData(DateTime fromDate, DateTime toDate, LogType logType, AsyncCallback callback, object asyncStatus);
        List<LogOrder> EndGetLogOrderData(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginGetLogSettingData(DateTime fromDate, DateTime toDate, LogType logType, AsyncCallback callback, object asyncStatus);
        List<LogSetting> EndGetLogSettingData(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginGetLogPriceData(DateTime fromDate, DateTime toDate, LogType logType, AsyncCallback callback, object asyncStatus);
        List<LogPrice> EndGetLogPriceData(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginGetLogSourceChangeData(DateTime fromDate, DateTime toDate, LogType logType, AsyncCallback callback, object asyncStatus);
        List<LogSourceChange> EndGetLogSourceChangeData(IAsyncResult result);

        #endregion


        #region Quotation
        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginGetConfigMetadata(AsyncCallback callback, object asyncState);
        ConfigMetadata EndGetConfigMetadata(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginAddMetadataObject(IMetadataObject metadataObject, AsyncCallback callback, object asyncState);
        int EndAddMetadataObject(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginAddInstrument(InstrumentData instrumentData, AsyncCallback callback, object asyncState);
        int EndAddInstrument(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginUpdateMetadataObject(MetadataType type, int objectId, Dictionary<string, object> fieldAndValues, AsyncCallback callback, object asyncState);
        bool EndUpdateMetadataObject(IAsyncResult result);
        
        [OperationContract(AsyncPattern=true)]
        IAsyncResult BeginUpdateMetadataObjects(UpdateData[] updateDatas, AsyncCallback callback, object asyncState);
        bool EndUpdateMetadataObjects(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginUpdateMetadataObjectField(MetadataType type, int objectId, string field, object value, AsyncCallback callback, object asyncState);
        bool EndUpdateMetadataObjectField(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginDeleteMetadataObject(MetadataType type, int objectId, AsyncCallback callback, object asyncState);
        bool EndDeleteMetadataObject(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginSendQuotation(int instrumentSourceRelationId, double ask, double bid, AsyncCallback callback, object asyncState);
        void EndSendQuotation(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginSwitchDefaultSource(SwitchRelationBooleanPropertyMessage message, AsyncCallback callback, object asyncState);
        void EndSwitchDefaultSource(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginGetQuotePolicyRelation(AsyncCallback callback, object asyncState);
        List<QuotePolicyRelation> EndGetQuotePolicyRelation(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginAddNewRelation(Guid id, string code, List<int> instruments, AsyncCallback callback, object asyncState);
        bool EndAddNewRelation(IAsyncResult result);
        #endregion
        [OperationContract(AsyncPattern = false)]
        void Updatetest();

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginUpdateQuotationPolicy(QuotePolicyDetailSet set, AsyncCallback callback, object asyncState);
        void EndUpdateQuotationPolicy(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginSetQuotationPolicyDetail(Guid relationId, QuotePolicyDetailsSetAction action, int changeValue, AsyncCallback callback, object asyncState);
        void EndSetQuotationPolicyDetail(IAsyncResult result);
    }

    [ServiceContract]
    public interface IClientProxy
    {
        [OperationContract(IsOneWay = true)]
        void SendMessage(Message message);
    }
}
