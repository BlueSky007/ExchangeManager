using System;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using iExchange.Common;
using Manager.Common;
using Manager.Common.QuotationEntities;
using CommonTransactionError = Manager.Common.TransactionError;
using CommonCancelReason = Manager.Common.CancelReason;

namespace ManagerService.Exchange
{
    public class ExchangeSystem
    {
        private string _SessionId;
        private IStateServer _StateServer;
        private ExchangeSystemSetting _ExchangeSystemSetting;

        private RelayEngine<Command> _CommandRelayEngine;

        private ConnectionState _ConnectionState = ConnectionState.Unknown;

        public ExchangeSystem(ExchangeSystemSetting exchangeSystemSetting)
        {
            this._ExchangeSystemSetting = exchangeSystemSetting;
            this._CommandRelayEngine = new RelayEngine<Command>(this.Dispatch, this.HandlEngineException);
        }

        public string ExchangeCode { get { return this._ExchangeSystemSetting.Code; } }
        public string StateServerUrl { get { return this._ExchangeSystemSetting.StateServerUrl; } }
        public ConnectionState ConnectionState { get { return this._ConnectionState; } }

        public void Channel_Broken(object sender, EventArgs e)
        {
            this._ConnectionState = ConnectionState.Disconnected;
        }

        public void Initailize()
        {
        }

        public void Replace(string sessionId, IStateServer stateServer)
        {
            this._SessionId = sessionId;
            this._StateServer = stateServer;
            this._ConnectionState = ConnectionState.Connected;
        }

        public void AddCommand(Command command)
        {
            this._CommandRelayEngine.AddItem(command);
        }

        public void SetQuotation(List<GeneralQuotation> quotations)
        {
            // TODO: Override Quotation here.
            return;
        }

        public void SwitchPriceState(string instrumentCode, bool enable)
        {
            // Enable/Disable Instrument price state
            List<Tuple<Guid, bool?, bool?>> updatedStates = new List<Tuple<Guid, bool?, bool?>>();
            List<Tuple<Guid, bool, bool>> states = DataAccess.ExchangeData.GetInstrumentPriceEnableStates(this.ExchangeCode, instrumentCode);
            foreach (var state in states)
            {
                if (state.Item2 != enable || state.Item3 != enable)
                {
                    updatedStates.Add(new Tuple<Guid, bool?, bool?>(state.Item1, state.Item2 == enable ? null : (bool?)enable, state.Item3 == enable ? null : (bool?)enable));
                }
            }
            if (updatedStates.Count > 0)
            {
                if (this._StateServer.SwitchPriceState(updatedStates))
                {
                    // TODO: Write Audit Log here.
                }
            }
        }

        private bool Dispatch(Command command)
        {
            try
            {
                Message message = CommandConvertor.Convert(this.ExchangeCode, command);
                Manager.ClientManager.Dispatch(message);
                return true;
            }
            catch (Exception ex)
            {
                Logger.AddEvent(TraceEventType.Error, "ExchangeSystem.Dispatch the exception cause _CommandRelayEngine suspended.\r\n{0}", ex.ToString());
            }
            return false;
        }

        private void HandlEngineException(Exception ex)
        {
            Logger.TraceEvent(TraceEventType.Error, "ExchangeSystem.HandlEngineException RelayEngine stopped:\r\n" + ex.ToString());
        }

        public void AbandonQuote(List<Answer> abandonQutes)
        {
            try
            {
                foreach (Answer quotes in abandonQutes)
                {
                    //this._StateServer.Answer(
                }
                //this._StateServer.AbandonQuote(quoteQuotation);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(TraceEventType.Error, "ExchangeSystem.Answer Error:\r\n" + ex.ToString());
            }
        }
        public void Answer(List<Answer> answerQutos)
        {
            try
            {
                //this._StateServer.Answer(answerQutos);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(TraceEventType.Error, "ExchangeSystem.Answer Error:\r\n" + ex.ToString());
            }
        }

        public CommonTransactionError AcceptPlace(Guid transactionId)
        {
            CommonTransactionError errorCode = CommonTransactionError.OK;

            //errorCode = this._StateServer.AcceptPlace(transactionId);
            return errorCode;
        }

        public CommonTransactionError CancelPlace(Guid transactionId, CommonCancelReason cancelReason)
        {
            CommonTransactionError errorCode = CommonTransactionError.OK;

            //errorCode = this._StateServer.AcceptPlace(transactionId);
            return errorCode;
        }

        public CommonTransactionError Cancel(Guid transactionId, CommonCancelReason cancelReason)
        {
            CommonTransactionError errorCode = CommonTransactionError.OK;

            //TransactionError transactionError = this.StateServer.Cancel(token, tranGuid, cancelReason);
            return errorCode;
        }

        public CommonTransactionError Execute(Guid transactionId, string buyPrice, string sellPrice, decimal lot, Guid orderId)
        {
            CommonTransactionError errorCode = CommonTransactionError.OK;

            //errorCode = this._StateServer.AcceptPlace(transactionId);
            return errorCode;
        }
        
    }
}
