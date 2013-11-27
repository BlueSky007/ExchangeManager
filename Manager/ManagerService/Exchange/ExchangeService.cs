using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using iExchange.Common;

namespace ManagerService.Exchange
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class ExchangeService : IExchangeService
    {
        private ExchangeSystem _ExchangeSystem;

        public void Register(string iexchangeCode)
        {
            string sessionId = OperationContext.Current.SessionId;
            IStateServer stateServer = OperationContext.Current.GetCallbackChannel<IStateServer>();
            if (MainService.ExchangeManager.TryRegister(iexchangeCode, sessionId, stateServer, out this._ExchangeSystem))
            {
                OperationContext.Current.Channel.Faulted += this._ExchangeSystem.Channel_Broken;
                OperationContext.Current.Channel.Closed += this._ExchangeSystem.Channel_Broken;
            }
            else
            {
                OperationContext.Current.Channel.Abort();
            }
        }

        public void AddCommand(Command command)
        {
            this._ExchangeSystem.AddCommand(command);
        }

       
    }
}
