using Manager.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

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
        IAsyncResult BeginGetAccessPermissionTree(int roleId, AsyncCallback callback, object asyncState);
        AccessPermissionTree EndGetAccessPermissionTree(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginGetDataPermissionTree(int roleId, AsyncCallback callback, object asyncState);
        DataPermissionTree EndGetDataPermissionTree(IAsyncResult result);


        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginUpdateRolePermission(int roleId, int editType, string roleName, AccessPermissionTree accessTree, DataPermissionTree dataTree, AsyncCallback callback, object asyncState);
        bool EndUpdateRolePermission(IAsyncResult result);
      

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginLogout(AsyncCallback callback, object asyncState);
        void EndLogout(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginGetAccessPermissions(AsyncCallback callback, object asyncState);
        List<AccessPermission> EndGetAccessPermissions(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginGetUserData(AsyncCallback callback, object asyncState);
        List<UserData> EndGetUserData(IAsyncResult result);

        [OperationContract(IsInitiating = false)]
        List<RoleData> GetRoles();

        [OperationContract(IsInitiating = false)]
        bool ChangePassword(string currentPassword, string newPassword);


        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginUpdateUsers(UserData user,string password,bool isNewUser, AsyncCallback callback, object asyncState);
        bool EndUpdateUsers(IAsyncResult result);

        #region QuotePrice
        [OperationContract(IsInitiating = false)]
        void AbandonQuote(List<Answer> abandonQuotePrices);

        [OperationContract(IsInitiating = false)]
        void SendQuotePrice(List<Answer> sendQuotePrices);
        #endregion

        
        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginDeleteUser(Guid userId, AsyncCallback callback, object asyncState);
        bool EndDeleteUser(IAsyncResult result);
      
    }

    [ServiceContract]
    public interface IClientProxy
    {
        [OperationContract(IsOneWay = true)]
        void SendMessage(Message message);

        [OperationContract(IsOneWay = true)]
        void Answer(Answer answer);
    }
}
