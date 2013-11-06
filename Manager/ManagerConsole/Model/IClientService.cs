﻿using Manager.Common;
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
        void SaveLayout(string layout,string content);  

        #region UserManager
        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginGetAccessPermissions(AsyncCallback callback, object asyncState);
        List<AccessPermission> EndGetAccessPermissions(IAsyncResult result);

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
        IAsyncResult BeginAcceptPlace(Guid transactionId,AsyncCallback callback, object asyncState);
        TransactionError EndAcceptPlace(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginCancelPlace(Guid transactionId, CancelReason cancelReason, AsyncCallback callback, object asyncState);
        TransactionError EndCancelPlace(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginExecute(Guid transactionId, string buyPrice, string sellPrice, decimal lot, Guid orderId, out XmlNode xmlNode, AsyncCallback callback, object asyncState);
        TransactionError EndExecute(IAsyncResult result);

        [OperationContract(IsInitiating = false)]
        void ResetHit(Guid[] orderIds);

        [OperationContract(IsInitiating = false)]
        AccountInformation GetAcountInfo(Guid transactionId);
        
        #endregion

        
       
      
    }

    [ServiceContract]
    public interface IClientProxy
    {
        [OperationContract(IsOneWay = true)]
        void SendMessage(Message message);
    }
}
