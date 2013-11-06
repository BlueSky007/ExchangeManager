using Manager.Common;
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
        void SaveLayout(string layout, string content);

        #region UserManager
        [OperationContract(IsInitiating = false)]
        bool ChangePassword(string currentPassword, string newPassword);

        [OperationContract(IsInitiating = false)]
        List<AccessPermission> GetAccessPermissions();

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
        TransactionError AcceptPlace(Guid transactionId);

        [OperationContract(IsInitiating = false)]
        TransactionError CancelPlace(Guid transactionId, CancelReason cancelReason);

        [OperationContract(IsInitiating = false)]
        TransactionError Execute(Guid transactionId, string buyPrice, string sellPrice, decimal lot, Guid orderId, out XmlNode xmlNode);

        [OperationContract(IsInitiating = false)]
        void ResetHit(Guid[] orderIds);

        [OperationContract(IsInitiating = false)]
        AccountInformation GetAcountInfo(Guid transactionId);
    }

    [ServiceContract]
    public interface IClientProxy
    {
        [OperationContract(IsOneWay = true)]
        void SendMessage(Message message);
    }
}
