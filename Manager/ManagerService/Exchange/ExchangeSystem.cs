using System;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using iExchange.Common;
using Manager.Common;
using Manager.Common.QuotationEntities;
using TransactionError = iExchange.Common.TransactionError;
using ManagerService.QuotationExchange;
using iExchange.Common.Manager;

namespace ManagerService.Exchange
{
    public class ExchangeSystem
    {
        private string _SessionId;
        private IStateServer _StateServer;
        private ExchangeSystemSetting _ExchangeSystemSetting;
        private QuotationServer _QuotationServer;
        private RelayEngine<Command> _CommandRelayEngine;

        private ConnectionState _ConnectionState = ConnectionState.Unknown;

        public ExchangeSystem(ExchangeSystemSetting exchangeSystemSetting)
        {
            this._ExchangeSystemSetting = exchangeSystemSetting;
            this._CommandRelayEngine = new RelayEngine<Command>(this.Dispatch, this.HandlEngineException);
            this._QuotationServer = new QuotationServer(exchangeSystemSetting);
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
            this._QuotationServer.SetStateServer(stateServer);
        }

        public void AddCommand(Command command)
        {
            this._CommandRelayEngine.AddItem(command);
        }

        public void SetQuotation(List<GeneralQuotation> quotations)
        {
            // TODO: Override Quotation here.
            bool isSucceed = false;
            iExchange.Common.OriginQuotation[] originQs = null;
            iExchange.Common.OverridedQuotation[] overridedQs = null;
            Token token = new Token(Guid.Empty, UserType.System, AppType.QuotationCollector);
            try
            {
                isSucceed = this._QuotationServer.SetQuotation(token, quotations, out originQs, out overridedQs);
                if (overridedQs.Length>0)
                {
                    OverridedQuotationMessage message = new OverridedQuotationMessage();
                    message.ExchangeCode = this.ExchangeCode;
                    message.OverridedQs = overridedQs.ToList();
                    MainService.ClientManager.Dispatch(message);
                }
            }
            catch (Exception e)
            {
                AppDebug.LogEvent("StateServer", e.ToString(), EventLogEntryType.Error);
            }
            return;
        }

        public void SwitchPriceState(string instrumentCode, bool enable)
        {
            // Enable/Disable Instrument price state
            if(this._ConnectionState == Manager.Common.ConnectionState.Connected)
            {
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
        }

        private bool Dispatch(Command command)
        {
            try
            {
                Message message = CommandConvertor.Convert(this.ExchangeCode, command);
                MainService.ClientManager.Dispatch(message);
                return true;
            }
            catch (Exception ex)
            {
                Logger.AddEvent(TraceEventType.Error, "ExchangeSystem.Dispatch the exception cause _CommandRelayEngine suspended.\r\n{0}", ex.ToString());
            }
            return false;
        }


        #region DealingConsole
        public void AbandonQuote(List<Answer> abandonQutes)
        {
            return;
        }

        public void Answer(Guid userId,List<Answer> answerQutos)
        {
            try
            {
                Token token = new Token(userId, UserType.System, AppType.DealingConsole);
                this._StateServer.Answer(token,answerQutos);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(TraceEventType.Error, "ExchangeSystem.Answer Error:\r\n" + ex.ToString());
            }
        }

        public TransactionError AcceptPlace(Guid transactionId)
        {
            TransactionError errorCode = TransactionError.OK;
            Token token = new Token(Guid.Empty, UserType.System, AppType.DealingConsole);
            errorCode = this._StateServer.AcceptPlace(token,transactionId);
            return errorCode;
        }

        public TransactionError CancelPlace(Guid transactionId, CancelReason cancelReason)
        {
            TransactionError errorCode = TransactionError.OK;
            Token token = new Token(Guid.Empty, UserType.System, AppType.DealingConsole);
            errorCode = this._StateServer.CancelPlace(token,transactionId);
            return errorCode;
        }

        public TransactionError Cancel(Guid transactionId, CancelReason cancelReason)
        {
            TransactionError errorCode = TransactionError.OK;
            Token token = new Token(Guid.Empty, UserType.System, AppType.DealingConsole);
            TransactionError transactionError = this._StateServer.Cancel(token, transactionId, cancelReason);
            return errorCode;
        }

        public TransactionError Execute(Guid transactionId, string buyPrice, string sellPrice, decimal lot, Guid executedOrderGuid)
        {
            TransactionError errorCode = TransactionError.OK;
            Token token = new Token(Guid.Empty, UserType.System, AppType.DealingConsole);
            errorCode = this._StateServer.Execute(token, transactionId, buyPrice, sellPrice, lot.ToString(), executedOrderGuid);
            return errorCode;
        }

        public bool UpdateInstrument(XmlNode instruments)
        {
            //bool isOk = this._StateServer.UpdateInstrument(token, instruments);
            return true;
        }
        #endregion

        #region Report
        public List<AccountGroupGNP> GetGroupNetPosition()
        {
            return NetGroupManager.GetNetPosition();
        }

        public List<OpenInterestSummary> GetInstrumentSummary(bool isGroupByOriginCode, string[] blotterCodeSelecteds)
        {
            return NetGroupManager.GetInstrumentSummary();
        }

        public List<OpenInterestSummary> GetAccountSummary(Guid instrumentId,string[] blotterCodeSelecteds)
        {
            return NetGroupManager.GetAccountSummary();
        }

        public List<OpenInterestSummary> GetOrderSummary(Guid instrumentId, Guid accountId,iExchange.Common.AccountType accountType, string[] blotterCodeSelecteds)
        {
            return NetGroupManager.GetOrderSummary(accountType);
        }
        #endregion
        private void HandlEngineException(Exception ex)
        {
            Logger.TraceEvent(TraceEventType.Error, "ExchangeSystem.HandlEngineException RelayEngine stopped:\r\n" + ex.ToString());
        }
        
    }
}
