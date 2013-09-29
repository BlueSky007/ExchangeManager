using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using iExchange.Common;
using System.Xml;
using Manager.Common;

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
        void GetInitData();

        [OperationContract]
        void Answer(QuoteQuotation quotation);
    }
}
