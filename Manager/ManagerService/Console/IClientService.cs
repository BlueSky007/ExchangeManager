using Manager.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

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
        bool ChangePassword(string currentPassword, string newPassword);

        [OperationContract(IsInitiating = false)]
        FunctionTree GetFunctionTree();

        [OperationContract(IsInitiating = false)]
        List<AccessPermission> GetAccessPermissions();

        [OperationContract(IsInitiating = false)]
        List<UserData> GetUserData();

        [OperationContract(IsInitiating = false)]
        List<RoleData> GetRoles();

        [OperationContract(IsInitiating = false)]
        RoleData GetAllPermission();

        [OperationContract(IsInitiating = false)]
        bool UpdateUsers(UserData user, string password, bool isNewUser);

        [OperationContract(IsInitiating = false)]
        void AbandonQuote(List<Answer> abandonQuotePrices);

        [OperationContract(IsInitiating = false)]
        void SendQuotePrice(List<Answer> sendQuotePrices);

        [OperationContract(IsInitiating = true)]
        TransactionError AcceptPlace(Guid transactionId);
    }

    [ServiceContract]
    public interface IClientProxy
    {
        [OperationContract(IsOneWay = true)]
        void SendMessage(Message message);
    }
}
