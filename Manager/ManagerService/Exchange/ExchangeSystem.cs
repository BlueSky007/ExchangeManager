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
        private RelayEngine<List<GeneralQuotation>> _QuotationRelayEngine;

        private ConnectionState _ConnectionState = ConnectionState.Unknown;
        private ConnectionManager _ConnectionManager;

        public ExchangeSystem(ExchangeSystemSetting exchangeSystemSetting, ConnectionManager connectionManager)
        {
            this._ExchangeSystemSetting = exchangeSystemSetting;
            this._ConnectionManager = connectionManager;
            this._CommandRelayEngine = new RelayEngine<Command>(this.Dispatch, this.HandleEngineException);
            this._QuotationRelayEngine = new RelayEngine<List<GeneralQuotation>>(this.SetQuotation, this.HandleQuotationRelayEngineException);
            this._QuotationServer = new QuotationServer(exchangeSystemSetting);
        }

        public string ExchangeCode { get { return this._ExchangeSystemSetting.Code; } }
        public string StateServerUrl { get { return this._ExchangeSystemSetting.StateServerUrl; } }
        public ConnectionState ConnectionState { get { return this._ConnectionState; } }

        public void Channel_Broken(object sender, EventArgs e)
        {
            this._ConnectionState = ConnectionState.Disconnected;
            this._ConnectionManager.NotifyExchangeToConnect(this.StateServerUrl, this.ExchangeCode);
            Logger.AddEvent(TraceEventType.Warning, "ExchangeSystem Channel_Broken of ExchangeCode:{0} sessionId:{1}", ExchangeCode, this._SessionId);
        }

        public void Replace(string sessionId, IStateServer stateServer)
        {
            this._SessionId = sessionId;
            this._StateServer = stateServer;
            this._ConnectionState = ConnectionState.Connected;
            this._QuotationServer.SetStateServer(stateServer);
            this._QuotationRelayEngine.Resume();
            Logger.AddEvent(TraceEventType.Warning, "ExchangeCode:{0},Connection to StateSever established. sessionId:{1}", ExchangeCode, this._SessionId);
        }

        public void AddCommand(Command command)
        {
            this._CommandRelayEngine.AddItem(command);
        }
        
        public void AddQuotation(List<GeneralQuotation> quotations)
        {
            this._QuotationRelayEngine.AddItem(quotations);
        }

        private bool SetQuotation(List<GeneralQuotation> quotations)
        {
            if (this._StateServer == null) return false;
            bool isSucceed = false;
            iExchange.Common.OriginQuotation[] originQs = null;
            iExchange.Common.OverridedQuotation[] overridedQs = null;
            Token token = new Token(Guid.Empty, UserType.System, AppType.QuotationCollector);
            try
            {
                isSucceed = this._QuotationServer.SetQuotation(token, quotations, out originQs, out overridedQs);
                if (isSucceed && (originQs != null || overridedQs != null))
                {
                    token.AppType = AppType.QuotationServer;
                    this._StateServer.BroadcastQuotation(token, originQs, overridedQs);
                }
                if (overridedQs != null && overridedQs.Length > 0)
                {
                    OverridedQuotationMessage message = new OverridedQuotationMessage();
                    message.ExchangeCode = this.ExchangeCode;
                    message.OverridedQs = overridedQs.ToList();
                    MainService.ClientManager.Dispatch(message);
                }
            }
            catch (Exception e)
            {
                Logger.TraceEvent(TraceEventType.Error, "ExchangeSystem.SetQuotation\r\n{0}", e.ToString());
                return false;
            }
            return true;
        }

        public void SwitchPriceState(List<string> originCodes, bool enable)
        {
            // Enable/Disable Instrument price state
            if (this._ConnectionState == Manager.Common.ConnectionState.Connected)
            {
                if (this._StateServer.SwitchPriceState(originCodes, enable))
                {
                    // TODO: Write Audit Log here.
                }
            }
        }

        private bool Dispatch(Command command)
        {
            try
            {
                if (command.GetType() == typeof(ChangeGroupCommand))
                {
                    ChangeGroupCommand changeGroupCommand = command as ChangeGroupCommand;
                    MainService.ClientManager.UpdateGroup(this.ExchangeCode, changeGroupCommand.xmlNode);
                }
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
            try
            {
                Token token = new Token(Guid.Empty, UserType.System, AppType.DealingConsole);
                errorCode = this._StateServer.AcceptPlace(token, transactionId);
            }
            catch (Exception ex)
            {
                errorCode = TransactionError.NoLinkedServer;
                Logger.TraceEvent(TraceEventType.Error, "ExchangeSystem.AcceptPlace Error:\r\n" + ex.ToString());
            }
            return errorCode;
        }

        public TransactionError CancelPlace(Guid transactionId, CancelReason cancelReason)
        {
            TransactionError errorCode = TransactionError.OK;
            try
            {
                Token token = new Token(Guid.Empty, UserType.System, AppType.DealingConsole);
                errorCode = this._StateServer.CancelPlace(token, transactionId);
            }
            catch (Exception ex)
            {
                errorCode = TransactionError.NoLinkedServer;
                Logger.TraceEvent(TraceEventType.Error, "ExchangeSystem.CancelPlace Error:\r\n" + ex.ToString());
            }
            return errorCode;
        }

        public TransactionError Cancel(Guid transactionId, CancelReason cancelReason)
        {
            TransactionError errorCode = TransactionError.OK;
            try
            {
            Token token = new Token(Guid.Empty, UserType.System, AppType.DealingConsole);
            TransactionError transactionError = this._StateServer.Cancel(token, transactionId, cancelReason);
            }
            catch (Exception ex)
            {
                errorCode = TransactionError.NoLinkedServer;
                Logger.TraceEvent(TraceEventType.Error, "ExchangeSystem.Cancel Error:\r\n" + ex.ToString());
            }
            return errorCode;
        }

        public TransactionResult Execute(Guid transactionId, string buyPrice, string sellPrice, decimal lot, Guid executedOrderGuid)
        {
            TransactionResult tranResult = null;
            try
            {
                Token token = new Token(Guid.Empty, UserType.System, AppType.DealingConsole);
                tranResult = this._StateServer.Execute(token, transactionId, buyPrice, sellPrice, lot.ToString(), executedOrderGuid);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(TraceEventType.Error, "ExchangeSystem.Execute Error:\r\n" + ex.ToString());
            }
            return tranResult;
        }

        public void ResetHit(Guid[] orderIDs)
        {
            try
            {
                Token token = new Token(Guid.Empty, UserType.System, AppType.DealingConsole);
                this._StateServer.ResetHit(token, orderIDs);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(TraceEventType.Error, "ExchangeSystem.ResetHit Error:\r\n" + ex.ToString());
            }
        }

        public bool UpdateInstrument(ParameterUpdateTask parameterUpdateTask)
        {
            try
            {
                Token token = new Token(Guid.Empty, UserType.System, AppType.DealingConsole);
                bool isOk = this._StateServer.UpdateInstrument(token, parameterUpdateTask);
                return true;
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(TraceEventType.Error, "ExchangeSystem.UpdateInstrument Error:\r\n" + ex.ToString());
                return false;
            }
        }

        public AccountInformation GetAcountInfo(Guid tranID)
        {
            try
            {
                Token token = new Token(Guid.Empty, UserType.System, AppType.DealingConsole);
                return this._StateServer.GetAcountInfo(token, tranID);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(TraceEventType.Error, "ExchangeSystem.GetAcountInfo Error:\r\n" + ex.ToString());
                return null;
            }
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
        private void HandleEngineException(Exception ex)
        {
            Logger.TraceEvent(TraceEventType.Error, "ExchangeSystem.HandlEngineException CommandRelayEngine stopped:\r\n" + ex.ToString());
        }

        private void HandleQuotationRelayEngineException(Exception ex)
        {
            Logger.TraceEvent(TraceEventType.Error, "ExchangeManager.HandleQuotationRelayEngineException QuotationRelayEngine stopped:\r\n" + ex.ToString());
        }
    }
}
