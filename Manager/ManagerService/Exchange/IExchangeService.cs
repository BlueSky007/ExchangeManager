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
        [OperationContract(IsOneWay = true)]
        void SetQuotation(string price);

        [OperationContract]
        bool SwitchPriceState(List<Tuple<Guid, bool?, bool?>> tuples);  // Tuple: InstrumentId,IsPriceEnabled,IsAutoEnablePrice

        [OperationContract]
        void Update(Token token, XmlNode udpateNode);

        [OperationContract]
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
        TransactionError Execute(Token token, Guid tranID, string buyPrice, string sellPrice, string lot, Guid executedOrderID);


    }
}
