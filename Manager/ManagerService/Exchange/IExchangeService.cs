using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using iExchange.Common;
using System.Xml;
using Manager.Common;
using iExchange.Common.Manager;

namespace ManagerService.Exchange
{
    [ServiceContract(CallbackContract = typeof(IStateServer), SessionMode = SessionMode.Required)]
    public interface IExchangeService
    {
        [OperationContract]
        void Register(string iexchangeCode);

        [OperationContract(IsInitiating = false), XmlSerializerFormat]
        void AddCommand(Command command);
    }

    [ServiceContract]
    public interface IStateServer
    {
        [OperationContract]
        bool SwitchPriceState(string[] originCodes, bool enable, out Guid[] affectInstrumentIds);

        [OperationContract]
        bool SuspendResume(string[] originCodes, bool resume, out Guid[] affectInstrumentIds);

        [OperationContract]
        void Update(Token token, XmlNode udpateNode);

        [OperationContract(IsOneWay = true)]
        void BroadcastQuotation(Token token, OriginQuotation[] originQs, OverridedQuotation[] overridedQs);

        [OperationContract]
        void Answer(Token token,List<Answer> answerQutos);

        [OperationContract]
        TransactionError AcceptPlace(Token token,Guid tranID);

        [OperationContract]
        TransactionError Cancel(Token token, Guid tranID, CancelReason cancelReason);

        [OperationContract]
        TransactionError CancelPlace(Token token, Guid tranID);

        [OperationContract]
        TransactionResult Execute(Token token, Guid tranID, string buyPrice, string sellPrice, string lot, Guid executedOrderID);

        [OperationContract]
        void ResetHit(Token token, Guid[] orderIDs);

        [OperationContract]
        bool UpdateInstrument(Token token, ParameterUpdateTask parameterUpdateTask);

        [OperationContract]
        AccountInformation GetAcountInfo(Token token, Guid tranID);
    }
}
