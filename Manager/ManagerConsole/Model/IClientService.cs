using Manager.Common;
using Manager.Common.QuotationEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Xml;

namespace ManagerConsole.Model
{
    [ServiceContract(CallbackContract = typeof(IClientProxy), SessionMode = SessionMode.Required)]
    public interface IClientService
    {
        //[OperationContract]
        //LoginResult Login(string userName, string password, string oldSessionId, Language language);

       

        
        [OperationContract(AsyncPattern=true)]
        IAsyncResult BeginLogin(string userName, string password, string oldSessionId, Language language, AsyncCallback callback, object asyncState);
        LoginResult EndLogin(IAsyncResult result);
      

        [OperationContract(IsInitiating = false)]
        FunctionTree GetFunctionTree();      

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginLogout(AsyncCallback callback, object asyncState);
        void EndLogout(IAsyncResult result);

        [OperationContract(IsInitiating = false)]
        void SaveLayout(string layout,string content,string layoutName);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginLoadLayout(string layoutName, AsyncCallback callback, object asyncState);
        List<string> EndLoadLayout(IAsyncResult result);

        #region UserManager
        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginGetAccessPermissions(AsyncCallback callback, object asyncState);
        Dictionary<string, string> EndGetAccessPermissions(IAsyncResult result);

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
        IAsyncResult BeginUpdateRole(RoleData role,bool isNewRole, AsyncCallback callback, object asyncState);
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
        IAsyncResult BeginAcceptPlace(Guid transactionId,LogOrder logEntity,AsyncCallback callback, object asyncState);
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

        #region Log Audit
        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginGetQuoteLogData(DateTime fromDate, DateTime toDate, LogType logType, AsyncCallback callback, object asyncStatus);
        List<LogQuote> EndGetQuoteLogData(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginGetLogOrderData(DateTime fromDate, DateTime toDate, LogType logType, AsyncCallback callback, object asyncStatus);
        List<LogOrder> EndGetLogOrderData(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginGetLogPriceData(DateTime fromDate, DateTime toDate, LogType logType, AsyncCallback callback, object asyncStatus);
        List<LogPrice> EndGetLogPriceData(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginGetLogSourceChangeData(DateTime fromDate, DateTime toDate, LogType logType, AsyncCallback callback, object asyncStatus);
        List<LogSourceChange> EndGetLogSourceChangeData(IAsyncResult result);

        #endregion


        #region Quotation
        [OperationContract(AsyncPattern=true)]
        IAsyncResult BeginGetConfigMetadata(AsyncCallback callback, object asyncState);
        ConfigMetadata EndGetConfigMetadata(IAsyncResult result);
        #endregion
    }

    [ServiceContract]
    public interface IClientProxy
    {
        [OperationContract(IsOneWay = true)]
        void SendMessage(Message message);
    }
}
