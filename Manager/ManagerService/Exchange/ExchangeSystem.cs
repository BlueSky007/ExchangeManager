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
using Manager.Common.ExchangeEntities;

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
            this._CommandRelayEngine = new RelayEngine<Command>(this.DispatchCommand, this.HandleEngineException);
            this._QuotationRelayEngine = new RelayEngine<List<GeneralQuotation>>(this.SetQuotation, this.HandleQuotationRelayEngineException);
            this._QuotationServer = new QuotationServer(exchangeSystemSetting);
        }

        public string ExchangeCode { get { return this._ExchangeSystemSetting.Code; } }
        public string StateServerUrl { get { return this._ExchangeSystemSetting.StateServerUrl; } }
        public ConnectionState ConnectionState
        {
            get { return this._ConnectionState; }
            private set
            {
                this._ConnectionState = value;
                MainService.ExchangeManager.NotifyExchangeConnectionStatus(this.ExchangeCode, value);
            }
        }

        public void Channel_Broken(object sender, EventArgs e)
        {
            this.ConnectionState = ConnectionState.Disconnected;
            this._ConnectionManager.NotifyExchangeToConnect(this.StateServerUrl, this.ExchangeCode);
            Logger.AddEvent(TraceEventType.Warning, "ExchangeSystem Channel_Broken of ExchangeCode:{0} sessionId:{1}", ExchangeCode, this._SessionId);
        }

        public void Stop()
        {
            this._CommandRelayEngine.Stop();
            this._QuotationRelayEngine.Stop();
        }

        public void Replace(string sessionId, IStateServer stateServer)
        {
            this._SessionId = sessionId;
            this._StateServer = stateServer;
            this.ConnectionState = ConnectionState.Connected;
            this._QuotationServer.SetStateServer(stateServer);
            this._QuotationRelayEngine.Resume();
            Logger.AddEvent(TraceEventType.Information, "ExchangeCode:{0},Connection to StateSever established. sessionId:{1}", ExchangeCode, this._SessionId);
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
                //foreach (OverridedQuotation item in collection)
                //{
                    
                //}
            }
            catch (Exception e)
            {
                Logger.TraceEvent(TraceEventType.Error, "ExchangeSystem.SetQuotation\r\n{0}", e.ToString());
                return false;
            }
            return true;
        }

        public bool UpdateQuotationServer(string xmlUpdateStr)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlUpdateStr);
            return this._QuotationServer.UpdateQuotePolicy(doc.FirstChild);
        }

        public UpdateHighLowBatchProcessInfo UpdateHighLow(Guid instrumentId, bool isOriginHiLo, string newInput, bool isUpdateHigh)
        {
            UpdateHighLowBatchProcessInfo info = new UpdateHighLowBatchProcessInfo();
            int batchProcessId; 
            string instrumentCode; 
            bool highBid ;
            bool lowBid; 
            DateTime updateTime; 
            DateTime minTimestamp; 
            iExchange.Common.OverridedQuotation[] overridedQs; 
            int returnValue; 
            string errorMessage;
            this._QuotationServer.UpdateHighLow(new Token(), "", instrumentId, isOriginHiLo, newInput, isUpdateHigh, out batchProcessId, out instrumentCode, out highBid, out lowBid, out updateTime, out minTimestamp, out overridedQs, out returnValue, out errorMessage);
            this._StateServer.BroadcastQuotation(new Token(Guid.Empty, UserType.System, AppType.QuotationServer), null, overridedQs);
            info.BatchProcessId = batchProcessId;
            info.InstrumentId = instrumentId;
            info.InstrumentCode = instrumentCode;
            info.IsHigh = isUpdateHigh;
            info.NewInput = newInput;
            info.StateCode = returnValue;
            info.ErrorMessage = errorMessage;
            info.UpdateTime = updateTime;
            return info;
        }

        public bool FixOverridedQuotationHistory(Token token, string quotation, bool needApplyAutoAdjustPoints, out iExchange.Common.OriginQuotation[] originQs, out iExchange.Common.OverridedQuotation[] overridedQs, out bool needBroadcastQuotation, out XmlNode fixChartDatas)
        {
            return this._QuotationServer.FixOverridedQuotationHistory(token, quotation, needApplyAutoAdjustPoints, out originQs, out overridedQs, out needBroadcastQuotation, out fixChartDatas);
        }

        public bool RestoreHighLow(int batchProcessId, out string errorMessage)
        {
             Guid instrumentId ;  
             string instrumentCode ;  
             string newInput ;  
             bool isUpdateHigh ; 
             bool highBid ;  
             bool lowBid ;  
             DateTime minTimestamp ; 
             iExchange.Common.OverridedQuotation[] overridedQs ;  
             int returnValue ;  
             this._QuotationServer.RestoreHighLow(new Token(Guid.Empty, UserType.System, AppType.DealingConsole), "", batchProcessId, out instrumentId, out instrumentCode, out newInput, out isUpdateHigh, out highBid, out lowBid, out minTimestamp, out overridedQs, out returnValue, out errorMessage);
             return returnValue == 0;
        }

        public void SwitchPriceState(string[] originCodes, bool enable)
        {
            // Enable/Disable Instrument price state
            if (this._ConnectionState == Manager.Common.ConnectionState.Connected)
            {
                Guid[] affectInstrumentIds;
                if (this._StateServer.SwitchPriceState(originCodes, enable, out affectInstrumentIds))
                {
                    // TODO: Write Audit Log here.
                    if (affectInstrumentIds != null && affectInstrumentIds.Length > 0)
                    {
                        string state = enable.ToString();
                        Dictionary<string, string> fieldsAndValues = new Dictionary<string, string>();
                        fieldsAndValues.Add(ExchangeFieldSR.IsPriceEnabled, state);
                        this.NotifyInstrumentUpdate(ExchangeMetadataType.Instrument, fieldsAndValues, affectInstrumentIds);
                    }
                }
            }
            else
            {
                Logger.AddEvent(TraceEventType.Warning,
                    "ExchangeSystem.SwitchPriceState StateServer not connected,SwitchPriceState failed.\r\noriginCodes:{0}. enable:{1}",
                    string.Join(",", originCodes), enable);
            }
        }

        public void SuspendResume(string[] originCodes, bool resume)
        {
            if (this._ConnectionState == Manager.Common.ConnectionState.Connected)
            {
                Guid[] affectInstrumentIds;
                if (this._StateServer.SuspendResume(originCodes, resume, out affectInstrumentIds))
                {
                    // TODO: Write Audit Log here.

                    if (affectInstrumentIds != null && affectInstrumentIds.Length > 0)
                    {
                        string state = resume.ToString();
                        Dictionary<string, string> fieldsAndValues = new Dictionary<string, string>();
                        fieldsAndValues.Add(ExchangeFieldSR.IsPriceEnabled, state);
                        fieldsAndValues.Add(ExchangeFieldSR.IsAutoEnablePrice, state);
                        this.NotifyInstrumentUpdate(ExchangeMetadataType.Instrument, fieldsAndValues, affectInstrumentIds);
                    }
                }
            }
            else
            {
                Logger.AddEvent(TraceEventType.Warning,
                    "ExchangeSystem.SuspendResume StateServer not connected,SuspendResume failed.\r\noriginCodes:{0}. resume:{1}",
                    string.Join(",", originCodes), resume);
            }
        }

        private void NotifyInstrumentUpdate(ExchangeMetadataType metadataType, Dictionary<string, string> fieldsAndValues, Guid[] instrumentIds)
        {
            ExchangeUpdateData[] exchangeUpdateDatas = new ExchangeUpdateData[instrumentIds.Length];
            for (int i = 0; i < instrumentIds.Length; i++)
            {
                Dictionary<string, string> fieldsAndValues2 = new Dictionary<string, string>(fieldsAndValues);
                fieldsAndValues2.Add(ExchangeFieldSR.ID, instrumentIds[i].ToString());
                exchangeUpdateDatas[i] = new ExchangeUpdateData()
                {
                    ExchangeMetadataType = metadataType,
                    FieldsAndValues = fieldsAndValues2,
                };
            }
            UpdateMessage updateMessage = new UpdateMessage() { ExchangeCode = this.ExchangeCode, ExchangeUpdateDatas = exchangeUpdateDatas };
            MainService.ClientManager.Dispatch(updateMessage);
        }

        private bool DispatchCommand(Command command)
        {
            try
            {
                ChangeGroupCommand changeGroupCommand = command as ChangeGroupCommand;
                if (changeGroupCommand != null)
                {
                    MainService.ClientManager.UpdateGroup(this.ExchangeCode, changeGroupCommand.xmlNode);
                }
                else
                {
                    UpdateCommand updateCommand = command as UpdateCommand;
                    if (updateCommand !=null)
                    {
                        this._QuotationServer.Update(new Token(), updateCommand.Content);
                    }
                    Message message = CommandConvertor.Convert(this.ExchangeCode, command);
                    MainService.ClientManager.Dispatch(message);
                }
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
                Token token = new Token(userId, UserType.System, AppType.Manager);
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
                Token token = new Token(Guid.Empty, UserType.System, AppType.Manager);
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
                Token token = new Token(Guid.Empty, UserType.System, AppType.Manager);
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
                Token token = new Token(Guid.Empty, UserType.System, AppType.Manager);
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
                Token token = new Token(Guid.Empty, UserType.System, AppType.Manager);
                bool isOk = this._StateServer.UpdateInstrument(token, parameterUpdateTask);
                return isOk;
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
                Token token = new Token(Guid.Empty, UserType.System, AppType.Manager);
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
        public List<AccountGroupGNP> GetGroupNetPosition(bool showActualQuantity, string[] blotterCodeSelecteds)
        {
            try
            {
                Token token = new Token(Guid.Empty, UserType.System, AppType.DealingConsole);
                token.UserID = new Guid("525BBBC6-0E94-4991-BAC1-0CF1D31BBC17");
                return this._StateServer.GetGroupNetPosition(token, "Access1", null, null, showActualQuantity, blotterCodeSelecteds);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(TraceEventType.Error, "ExchangeSystem.GetGroupNetPosition Error:\r\n" + ex.ToString());
                return null;
            }
        }

        public List<OpenInterestSummary> GetOpenInterestInstrumentSummary(bool isGroupByOriginCode, string[] blotterCodeSelecteds)
        {
            try
            {
                Token token = new Token(Guid.Empty, UserType.System, AppType.DealingConsole);
                token.UserID = new Guid("525BBBC6-0E94-4991-BAC1-0CF1D31BBC17");
                return this._StateServer.GetOpenInterestInstrumentSummary(token, false, blotterCodeSelecteds);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(TraceEventType.Error, "ExchangeSystem.GetGroupNetPosition Error:\r\n" + ex.ToString());
                return null;
            }
        }

        public List<OpenInterestSummary> GetOpenInterestAccountSummary(Guid[] accountIDs, Guid[] instrumentIDs, string[] blotterCodeSelecteds)
        {
            try
            {
                Token token = new Token(Guid.Empty, UserType.System, AppType.DealingConsole);
                token.UserID = new Guid("525BBBC6-0E94-4991-BAC1-0CF1D31BBC17");
                return this._StateServer.GetOpenInterestAccountSummary(token, accountIDs, instrumentIDs, blotterCodeSelecteds);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(TraceEventType.Error, "ExchangeSystem.GetOpenInterestAccountSummary Error:\r\n" + ex.ToString());
                return null;
            }
        }

        public List<OpenInterestSummary> GetOpenInterestOrderSummary(Guid accountId,iExchange.Common.AccountType accountType, Guid instrumentId, string[] blotterCodeSelecteds)
        {
            try
            {
                Token token = new Token(Guid.Empty, UserType.System, AppType.DealingConsole);
                Guid[] instruments = new Guid[] { instrumentId };
                token.UserID = new Guid("525BBBC6-0E94-4991-BAC1-0CF1D31BBC17");
                return this._StateServer.GetOpenInterestOrderSummary(token, accountId, accountType, instruments, blotterCodeSelecteds);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(TraceEventType.Error, "ExchangeSystem.GetOpenInterestOrderSummary Error:\r\n" + ex.ToString());
                return null;
            }
        }

        public string GetStateServerAccount(Guid[] accountIds)
        {
            string outerXml = string.Empty;

            Token token = new Token();
            token.UserID = Guid.Empty;
            token.UserType = UserType.System;
            token.AppType = AppType.BackOffice;

            return this._StateServer.GetAccounts(token, accountIds, true);
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
