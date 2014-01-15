using iExchange.Common;
using Manager.Common.QuotationEntities;
using ManagerService.Exchange;
using ManagerService.QuotationExchange;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Transactions;
using System.Xml;

namespace ManagerService.QuotationExchange
{
    public class QuotationServer
    {
        public static readonly Token Token = new Token(Guid.Empty, UserType.System, AppType.QuotationServer);
        private ExchangeSystemSetting _IExchangeSettion;

        private ReaderWriterLock rwLock = new ReaderWriterLock();
        private Scheduler scheduler = new Scheduler();
        private IStateServer _StateServer;
        //private string quotationServerID;
        private string connectionString;

        //private StateServer.Service2 stateServer;
        private TradeDay tradeDay;

        //Data Access (not through dataservice layer for performance)
        //this.originQuotations,this.overridedealerQuotations,this.quotePolicies is synchronizing with DB
        private Dictionary<Guids, QuotePolicyDetail> quotePolicyDetails;
        private Dictionary<string, Guid> quotePolicies;
        private Dictionary<Guid, OriginQuotation> originQuotations;
        private Dictionary<Guids, OverridedQuotation> overridedQuotations;

        //Instrument
        private Dictionary<string, OriginInstrument> originInstruments;
        private Dictionary<Guid, Instrument> instruments;

        //Received queue
        private Dictionary<Guid, OriginQuotation> originQReceivedList;
        //Sending queue
        private Dictionary<Guid, OriginQuotation> originQSendingList;
        private Dictionary<Guids, OverridedQuotation> overridedQSendingList;

        //used to fix the bug 
        private Dictionary<Guid, OriginQuotation> emptyOriginQuotaions;

        private bool isReplay = false;
        private string traceOriginCode;// = ConfigurationSettings.AppSettings["TraceOriginCode"];
        private bool acceptZeroPrice;// = string.IsNullOrEmpty(ConfigurationSettings.AppSettings["AcceptZeroPrice"]) ? false : bool.Parse(ConfigurationSettings.AppSettings["AcceptZeroPrice"]);
        private TimeSpan originPriceValidTime;// = string.IsNullOrEmpty(ConfigurationSettings.AppSettings["OriginPriceValidTime"]) ? TimeSpan.FromHours(1) : TimeSpan.Parse(ConfigurationSettings.AppSettings["OriginPriceValidTime"]);

        private Dictionary<Instrument, object> pendingOpenPriceInstrument;      // key:Instrument; value: null;        Instruments waiting to update PrivateDailyQuotation
        private Dictionary<string, object> pendingOpenPriceOriginCode;      // key:InstrumentOriginCode; value: null;        OriginCodes waiting to update PublicDailyQuotation
        private Guid defaultQuotePolicyId;
        private bool traceOpenPrice;// = (string.IsNullOrEmpty(ConfigurationSettings.AppSettings["TraceOpenPrice"]) ? false : bool.Parse(ConfigurationSettings.AppSettings["TraceOpenPrice"]));
        private bool enableGenerateDailyQuotation;          // Auto generate OpenPrice after first quotation arrived;

        private bool highBid;
        private bool lowBid;
        private QuotationFileCache _Cache;
        private QuotationCacheType _CacheType;
        private bool _EnableChartGeneration;
        private ChartQuotationManager _ChartQuotationManager;

        //private Test _Test;
        private TimeSpan _MinIntervalToGetPicesOfHiLo;
       
        public QuotationServer(ExchangeSystemSetting setting)
        {
            try
            {
                this._IExchangeSettion = setting;
                this.connectionString = setting.DbConnectionString;
                this.traceOriginCode = setting.TraceOriginCode;
                this.acceptZeroPrice = setting.AcceptZeroPricep;
                this.originPriceValidTime = string.IsNullOrEmpty(setting.OriginPriceValidTime) ? TimeSpan.Parse("1") : TimeSpan.Parse(setting.OriginPriceValidTime);
                this.traceOpenPrice = setting.TraceOpenPrice;
                this._MinIntervalToGetPicesOfHiLo = string.IsNullOrEmpty(setting.MinIntervalToGetPricesOfHiLo) ? TimeSpan.Parse("1") : TimeSpan.Parse(setting.MinIntervalToGetPricesOfHiLo);
                this._CacheType = string.IsNullOrEmpty(setting.CacheType) ? QuotationCacheType.None : (QuotationCacheType)Enum.Parse(typeof(QuotationCacheType), setting.CacheType);
                string quotationConnectionString = setting.QuotationConnectionString;
                TimeSpan commitFrequency = string.IsNullOrEmpty(setting.CommitFrequency) ? new TimeSpan(0, 0, 10) : TimeSpan.Parse(setting.CommitFrequency);
                if (this._CacheType == QuotationCacheType.File)
                {
                    int batchSize = string.IsNullOrEmpty(setting.CatchSize) ? 100 : int.Parse(setting.CatchSize);
                    string directoryName = string.IsNullOrEmpty(setting.QuotationFileCacheDirectory) ? "Quotations" : setting.QuotationFileCacheDirectory;
                    this._Cache = new QuotationFileCache(this, directoryName, quotationConnectionString, batchSize, commitFrequency);
                }

                //should initialize after cache construct completed.
                this.Initialize(this.GetData(connectionString));
                QuotaionExporterManager.Start(this.quotePolicies);

                if (this._EnableChartGeneration)
                {
                    this._ChartQuotationManager = new ChartQuotationManager(quotationConnectionString, TimeSpan.FromMilliseconds(1), this.highBid, this.lowBid, setting.ChartCommandTimeout);
                }

                if (this._CacheType == QuotationCacheType.File)
                {
                    this._Cache.Start();        // QuotationFileCache will start chartQuotationManager
                }
                else if (this._EnableChartGeneration)
                {
                    this._ChartQuotationManager.Start();
                }
            }
            catch (Exception ex)
            {
               Manager.Common.Logger.TraceEvent(TraceEventType.Error,"QuotationServer{0}",ex.ToString());
            }
        }

        public void SetStateServer(IStateServer stateServer)
        {
            this._StateServer = stateServer;
        }

        internal bool EnableChartGeneration
        {
            get { return this._EnableChartGeneration; }
        }

        internal ChartQuotationManager ChartQuotationManager
        {
            get { return this._ChartQuotationManager; }
        }

        public void Reset()
        {
            this.Initialize(this.GetData(connectionString));
        }

        public string GetQuotation(Token token)
        {
            return string.Empty;
        }

        public bool ReplayQuotation(Token token, List<GeneralQuotation> quotations, out iExchange.Common.OriginQuotation[] originQs, out iExchange.Common.OverridedQuotation[] overridedQs)
        {
            this.rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                this.isReplay = true;
                return this.SetQuotation(token, quotations, out originQs, out overridedQs);
            }
            finally
            {
                this.isReplay = false;
                this.rwLock.ReleaseWriterLock();
            }
        }

        public bool Flush()
        {
            if (this._Cache != null)
            {
                AppDebug.LogEvent("QuotationServer", "Begin QuotationFileCache.Flush", EventLogEntryType.Information);
                try
                {
                    return this._Cache.Flush();
                }
                finally
                {
                    AppDebug.LogEvent("QuotationServer", "End QuotationFileCache.Flush", EventLogEntryType.Information);
                }
            }
            return true;
        }

        public bool SetQuotation(Token token, List<GeneralQuotation> quotations, out iExchange.Common.OriginQuotation[] originQs, out iExchange.Common.OverridedQuotation[] overridedQs)
        {
            originQs = null;
            overridedQs = null;
            try
            {
                bool success = false;
                bool hasInvalidQuotation = false;
                int i;
                //Parse quotation row
                switch (token.AppType)
                {
                    case AppType.QuotationCollector:
                        CollectorQuotation[] collectorQ = new CollectorQuotation[quotations.Count];
                        i = 0;
                        foreach (GeneralQuotation quotation in quotations)
                        {
                            collectorQ[i] = CollectorQuotation.CreateInstance(quotation);
                            if (collectorQ[i++] == null) hasInvalidQuotation = true;
                        }
                        this.rwLock.AcquireWriterLock(Timeout.Infinite);
                        try
                        {
                            success = SetQuotation(token, collectorQ);
                            this.BuildLightQuotation(ref originQs, ref overridedQs);
                        }
                        finally
                        {
                            this.rwLock.ReleaseWriterLock();
                        }

                        break;
                    case AppType.DealingConsole:
                    case AppType.RiskMonitor:
                        DealerQuotation[] dealerQ = new DealerQuotation[quotations.Count];
                        i = 0;
                        foreach (GeneralQuotation quotation in quotations)
                        {
                            dealerQ[i] = DealerQuotation.CreateInstance(quotation);
                            if (dealerQ[i++] == null) hasInvalidQuotation = true;
                        }
                        this.rwLock.AcquireWriterLock(Timeout.Infinite);
                        try
                        {
                            success = SetQuotation(token, dealerQ);
                            if (AppType.RiskMonitor == token.AppType)
                            {
                                this.BuildLightQuotation(ref originQs, ref overridedQs);
                            }
                            else
                            {
                                this.BuildLightQuotation(dealerQ, ref originQs, ref overridedQs);
                            }
                        }
                        finally
                        {
                            this.rwLock.ReleaseWriterLock();
                        }

                        break;
                }

                if (hasInvalidQuotation)
                {
                    AppDebug.LogEvent("QuotationServer", string.Format("SetQuotation: has Invalid Quotation:{0}", quotations), EventLogEntryType.Warning);
                }

                //this.TraceQuotation(quotation, originQs, overridedQs);
                return success;
            }
            catch (Exception e)
            {
                AppDebug.LogEvent("QuotationServer", string.Format("|{0}|{1}", quotations, e.ToString()), EventLogEntryType.Error);
                return false;
            }
        }

        [Conditional("DEBUG")]
        private void TraceQuotation(string quotation, iExchange.Common.OriginQuotation[] originQs, iExchange.Common.OverridedQuotation[] overridedQs)
        {
            Debug.WriteLine(string.Format("QuotationServer.SetQuotation: quotation=={0}", AppDebug.OutputReplace(quotation)));
            Debug.WriteLine(string.Format("QuotationServer.SetQuotation: originQs=={0}", ToString<iExchange.Common.OriginQuotation>(originQs)));
            Debug.WriteLine(string.Format("QuotationServer.SetQuotation: overridedQs=={0}", ToString<iExchange.Common.OverridedQuotation>(overridedQs)));
        }

        private static string ToString<T>(T[] ts)
        {
            if (ts == null)
            {
                return "(NULL)";
            }
            else
            {
                StringBuilder stringBuilder = new StringBuilder();
                foreach (T t in ts)
                {
                    stringBuilder.AppendFormat("[{0}]", t);
                }
                return stringBuilder.ToString();
            }
        }


        public bool DiscardQuotation(Token token, Guid instrumentID, out iExchange.Common.OriginQuotation[] originQs, out iExchange.Common.OverridedQuotation[] overridedQs)
        {
            originQs = null;
            overridedQs = null;

            if (token.AppType != AppType.DealingConsole) return false;

            this.rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                Instrument instrument = this.instruments[instrumentID];
                if (!instrument.IsTrading) return false;

                if (instrument.ScheduleID != null)
                {
                    if (instrument.OriginCode == this.traceOriginCode)
                    {
                        AppDebug.LogEvent("QuotationServer", string.Format("DiscardQuotation--Id: {0}, IsTrading:{1}, DealerID: {2}, ScheduleID: {3}",
                            instrument.ID, instrument.IsTrading, instrument.LastOfferDealerID, instrument.ScheduleID), EventLogEntryType.Warning);
                    }
                    this.scheduler.Remove(instrument.ScheduleID);
                    instrument.ScheduleID = null;

                    this.OverrideQuotation();
                    this.BuildLightQuotation(ref originQs, ref overridedQs);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            finally
            {
                this.rwLock.ReleaseWriterLock();
            }
        }

        public bool UpdateQuotePolicy(Token token, XmlNode quotePolicy, out int error)
        {
            this.rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                //Save to DB
                string quotePolicyXml = iExchange.Common.DataAccess.ConvertToSqlXml(quotePolicy.OuterXml);
                string sql = string.Format("Exec dbo.P_UpdateQuotePolicyDetail '{0}','{1}'", token.UserID, quotePolicyXml);
                error = (int)iExchange.Common.DataAccess.ExecuteScalar(sql, this.connectionString);
                if (error != 0) return false;
                //if (!DataAccess.UpdateDB(sql, this.connectionString)) return false;

                //Modified by Michael on 2008-09-05
                Guid quotePolicyID = XmlConvert.ToGuid(quotePolicy.Attributes["QuotePolicyID"].Value);
                Guid instrumentID = XmlConvert.ToGuid(quotePolicy.Attributes["InstrumentID"].Value);
                QuotePolicyDetail quotePolicy2 = this.quotePolicyDetails[new Guids(quotePolicyID, instrumentID)];
                return quotePolicy2.Update(quotePolicy);
            }
            finally
            {
                this.rwLock.ReleaseWriterLock();
            }
        }

        public bool UpdateQuotePolicies(Token token, XmlNode quotePolicies, out int error)
        {
            this.rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                //Save to DB
                string quotePolicyXml = iExchange.Common.DataAccess.ConvertToSqlXml(quotePolicies.OuterXml);
                string sql = string.Format("Exec dbo.P_UpdateQuotePolicyDetail '{0}','{1}'", token.UserID, quotePolicyXml);
                error = (int)iExchange.Common.DataAccess.ExecuteScalar(sql, this.connectionString);
                if (error != 0) return false;
                //if (!DataAccess.UpdateDB(sql, this.connectionString)) return false;

                //Modified by Michael on 2008-09-05
                foreach (XmlNode quotePolicy in quotePolicies.ChildNodes)
                {
                    Guid quotePolicyID = XmlConvert.ToGuid(quotePolicy.Attributes["QuotePolicyID"].Value);
                    Guid instrumentID = XmlConvert.ToGuid(quotePolicy.Attributes["InstrumentID"].Value);
                    QuotePolicyDetail quotePolicy2 = this.quotePolicyDetails[new Guids(quotePolicyID, instrumentID)];
                    bool passed = quotePolicy2.Update(quotePolicy);
                    if (!passed) return false;
                }
                return true;
            }
            finally
            {
                this.rwLock.ReleaseWriterLock();
            }
        }

        public void UpdateInstrumentDealer(Token token, DataSet instrumentSettingChanges)
        {
            this.rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                foreach (DataRow instrumentSetting in instrumentSettingChanges.Tables[0].Rows)
                {
                    Guid instrumentID = (Guid)instrumentSetting["InstrumentID"];
                    if (instrumentSetting["UserID"] != DBNull.Value)
                    {
                        Guid dealerID = (Guid)instrumentSetting["UserID"];
                        (this.instruments[instrumentID]).AddDealer(dealerID);
                    }
                    else
                    {
                        (this.instruments[instrumentID]).RemoveDealer(token.UserID);
                    }
                }
            }
            finally
            {
                this.rwLock.ReleaseWriterLock();
            }
        }

        //private void TryChangePriceActive(object sender, object Args)
        //{
        //    this.rwLock.AcquireWriterLock(Timeout.Infinite);

        //    bool hasChanges = false;
        //    XmlDocument xmlDoc = new XmlDocument();   
        //    XmlElement updateNode = xmlDoc.CreateElement("Update");
        //    StringBuilder sqlBuilder = new StringBuilder();

        //    try
        //    {                
        //        XmlElement modifyNode = xmlDoc.CreateElement("Modify");
        //        updateNode.AppendChild(modifyNode);
                
        //        DateTime baseTime = DateTime.Now;
        //        foreach (Instrument instrument in this.instruments.Values)
        //        {
        //            if (instrument.TryChangePriceActive(baseTime))
        //            {
        //                hasChanges = true;

        //                sqlBuilder.Append(instrument.GetUpdateSql());
        //                modifyNode.AppendChild(instrument.GetUpdateNode(xmlDoc));
        //            }
        //        }
        //    }
        //    catch (Exception exception)
        //    {
        //        AppDebug.LogEvent("QuotationServer", exception.ToString(), EventLogEntryType.Error);
        //    }
        //    finally
        //    {
        //        this.rwLock.ReleaseWriterLock();
        //    }

        //    if (hasChanges)
        //    {
        //        try
        //        {
        //            iExchange.Common.DataAccess.UpdateDB(sqlBuilder.ToString(), this.connectionString);
        //            this._StateServer.Update(QuotationServer.Token, updateNode);
        //        }
        //        catch (Exception exception)
        //        {
        //            AppDebug.LogEvent("QuotationServer", exception.ToString(), EventLogEntryType.Error);
        //        }
        //    }

        //    Scheduler.Action action = new Scheduler.Action(this.TryChangePriceActive);
        //    this.scheduler.Add(action, null, DateTime.Now.AddSeconds(2));
        //}

        //called by timer
        private void SetTradingState(object sender, object Args)
        {
            TradingTimeArgs tradingTime = (TradingTimeArgs)Args;
            AppDebug.LogEvent("QuotationServer", string.Format("SetTradingState({0}) start", tradingTime), EventLogEntryType.Information);

            this.rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                if (this.tradeDay.BeginTime > tradingTime.EndTime)
                {
                    AppDebug.LogEvent("QuotationServer", string.Format("SetTradingState({0}) is triggered in yesterday", tradingTime), EventLogEntryType.Warning);
                    return;
                }

                Instrument instrument = this.instruments[tradingTime.InstrumentID];
                instrument.IsTrading = tradingTime.IsTrading;

                OriginInstrument originInstrument = this.originInstruments[instrument.OriginCode];
                if (tradingTime.IsTrading)
                {
                    originInstrument.IsTrading = tradingTime.IsTrading;
                    try
                    {
                        bool isDayOpenTime = this.ProcessSessionLastQuotations(instrument.ID, tradingTime.BeginTime);
                        this.ClearSessionQuotationItems(isDayOpenTime, tradingTime.InstrumentID);
                    }
                    catch (Exception exception)
                    {
                        AppDebug.LogEvent("QuotationServer", exception.ToString(), EventLogEntryType.Error);
                        this.Reset();
                    }
                }
                else
                {
                    bool hasOneIDTrading = false;
                    List<Instrument> instruments = originInstrument.Instruments;
                    foreach (Instrument instrument2 in instruments)
                    {
                        hasOneIDTrading = instrument2.IsTrading;
                        if (hasOneIDTrading) break;
                    }
                    originInstrument.IsTrading = hasOneIDTrading;
                }
            }
            catch (Exception exception)
            {
                AppDebug.LogEvent("QuotationServer", string.Format("SetTradingState({0}) failed at\r\n{1}", tradingTime, exception), EventLogEntryType.Error);
            }
            finally
            {
                this.rwLock.ReleaseWriterLock();
            }
        }

        private void ClearSessionQuotationItems(bool isDayOpenTime, Guid instrumentId)
        {
            OriginQuotation originQ = null;;
            Instrument instrument = null;
            if (!this.instruments.TryGetValue(instrumentId, out instrument))
            {
                AppDebug.LogEvent("QuotationServer", string.Format("ClearSessionQuotationItems,cann't find Instrument for instrumentId:{0}", instrumentId), EventLogEntryType.Error);
                return;
            }
            if (this.originQuotations.TryGetValue(instrumentId, out originQ))
            {
                originQ.ClearItems(isDayOpenTime);

                instrument.OriginQReceived = originQ;
                instrument.OriginQProcessed = originQ;

            }
            else
            {
                AppDebug.LogEvent("QuotationServer", string.Format("ClearSessionQuotationItems,cann't find OriginQuotation for instrumentId:{0}", instrumentId), EventLogEntryType.Warning);
            }

            foreach (QuotePolicyDetail quotePolicy in instrument.QuotePolicyDetails)
            {
                Guids key = new Guids(quotePolicy.ID, instrument.ID);
                OverridedQuotation overridedQ = null;
                if (this.overridedQuotations.TryGetValue(key, out overridedQ))
                {
                    overridedQ.ClearItems(isDayOpenTime);

                    quotePolicy.OverrideHigh = overridedQ.High;
                    quotePolicy.OverrideLow = overridedQ.Low;
                }
                else
                {
                    AppDebug.LogEvent("QuotationServer", string.Format("ClearSessionQuotationItems,cann't find OverridedQuotation for instrumentId:{0}; quotePolicy.ID:{1}", instrumentId, quotePolicy.ID), EventLogEntryType.Warning);
                }
            }
        }

        //Add by Korn 2008-04-15
        //When session begin, Merge LastOriginQuotation and LastOverrideQuotation and clear lastSessionLastOriginQuotation, lastSessionLastOverrideQuotation
        private bool ProcessSessionLastQuotations(Guid instrumentId, DateTime beginTime)
        {
            if (beginTime == default(DateTime))
            {
                throw new ArgumentOutOfRangeException("beginTime", "Session beginTime cann't be zero.");
            }
            string sql = string.Format("EXEC dbo.P_ProcessSessionLastQuotations '{0}','{1:yyyy-MM-dd HH:mm:ss.fff}'", instrumentId, beginTime);
            bool result = false;
            try
            {
                result = (bool)iExchange.Common.DataAccess.ExecuteScalar(sql, this.connectionString);
                return result;
            }
            catch (Exception exception)
            {
                AppDebug.LogEvent("QuotationServer", string.Format("ProcessSessionLastQuotations failed.{0}", exception.ToString()), EventLogEntryType.Error);
                throw exception;
            }
        }

        //called by timer
        private void QuotationWaitTimeout(object sender, object Args)
        {
            iExchange.Common.OriginQuotation[] originQs = null;
            iExchange.Common.OverridedQuotation[] overridedQs = null;

            this.rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                OriginQuotation originQ = (OriginQuotation)Args;
                Instrument instrument = originQ.Instrument;

                DateTime baseTime = DateTime.Now;

                Trace.TraceInformation("QuotationWaitTimeout: Id=={0}, IsTrading=={1}, DealerID=={2}, ScheduleID=={3}, bseTime=={4:yyyy-MM-dd HH:mm:ss.fff}, originQ=={5}, LastOrigin=={6}",
                        instrument.ID, instrument.IsTrading, instrument.LastOfferDealerID, instrument.ScheduleID, baseTime, originQ, instrument.LastOrigin);

                if (instrument.OriginCode == this.traceOriginCode)
                {
                    AppDebug.LogEvent("QuotationServer", string.Format("QuotationWaitTimeout: Id=={0}, IsTrading=={1}, DealerID=={2}, ScheduleID=={3}, bseTime=={4:yyyy-MM-dd HH:mm:ss.fff}, originQ=={5}, LastOrigin=={6}",
                        instrument.ID, instrument.IsTrading, instrument.LastOfferDealerID, instrument.ScheduleID, baseTime, originQ, instrument.LastOrigin), EventLogEntryType.Warning);
                }
                instrument.ScheduleID = null;

                if (originQ.Bid != null && originQ.Ask != null && originQ.Bid > originQ.Ask)
                {
                    AppDebug.LogEvent("QuotationServer", string.Format("QuotationWaitTimeout--originQ.Bid > originQ.Ask [originQ:{0},LastOrigin:{1}]", originQ, instrument.LastOrigin), EventLogEntryType.Warning);
                }

                if(originQ.IsProblematic)
                {
                    OriginQuotation originQ2 = null;
                    if (this.originQReceivedList.TryGetValue(instrument.ID, out originQ2))
                    {
                        if (originQ2.Timestamp > originQ.Timestamp)
                        {
                            ProcessInstrument(originQ2, DateTime.Now);
                        }

                        this.originQReceivedList.Remove(instrument.ID);
                    }
                }
                else
                {
                    //loop all instrument's QuotePolicy
                    ProcessQuotePolicy(originQ);
                }

                if (QuotaionExporterManager.HasExporter)
                {
                    QuotaionExporterManager.AddQuotation(this.overridedQuotations.Values);
                }
                //Save overrided quotation
                switch (this._CacheType)
                {
                    case QuotationCacheType.None:
                        UpdateDB<Guids, OverridedQuotation>(this.overridedQuotations, this.overridedQSendingList);
                        break;
                    case QuotationCacheType.File:
                        this._Cache.Add(null, this.overridedQuotations.Values);
                        break;
                }

                //send to state server
                this.BuildLightQuotation(ref originQs, ref overridedQs);

                if (originQs != null || overridedQs != null)
                {
                    try
                    {                        
                        //?? Object reference not set to an instance of an object.
                        Debugger.Break();  // this will never happen.
                        this._StateServer.BroadcastQuotation(QuotationServer.Token, originQs, overridedQs);
                        //IAsyncResult asyncResult = this._StateServer.BeginBroadcastQuotation(QuotationServer.Token, originQs, overridedQs, null, null);
                        //asyncResult.AsyncWaitHandle.WaitOne();
                    }
                    catch (Exception e)
                    {
                        AppDebug.LogEvent("QuotationServer", e.ToString(), EventLogEntryType.Error);
                    }

                }
            }
            finally
            {
                this.rwLock.ReleaseWriterLock();
            }
        }

        //called by timer
        private void OnTradeDayClose(object sender, object Args)
        {
            TradeDay tradeDay = (TradeDay)Args;

            this.rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                AppDebug.LogEvent("QuotationnServer", string.Format("OnTradeDayClose: WaitTillReset {0}", tradeDay.Day), EventLogEntryType.Information);
                iExchange.Common.DataAccess.WaitTillReset(tradeDay.Day, this.connectionString);

                DateTime nextDay = tradeDay.Day.AddDays(1);
                AppDebug.LogEvent("QuotationnServer", string.Format("OnTradeDayClose: TradeDay {0:yyyy-MM-dd}) Initialize Begin", nextDay), EventLogEntryType.Information);
                this.Initialize(this.GetData( this.connectionString, nextDay));
                AppDebug.LogEvent("QuotationnServer", string.Format("OnTradeDayClose: TradeDay {0:yyyy-MM-dd} Initialize End", nextDay), EventLogEntryType.Information);
            }
            catch (Exception e)
            {
                AppDebug.LogEvent("QuotationnServer", e.ToString(), EventLogEntryType.Error);
            }
            finally
            {
                this.rwLock.ReleaseWriterLock();
            }
        }

        private DataSet GetData(string connectionString)
        {
            return this.GetData( connectionString, DateTime.MinValue);
        }

        private DataSet GetData(string connectionString, DateTime tradeDay)
        {
            //QuotationServer initCommand
            InitCommand initCommand;
            initCommand = new InitCommand();
            initCommand.Command = new SqlCommand("dbo.P_GetInitDataForQuotationServer");
            initCommand.Command.CommandType = System.Data.CommandType.StoredProcedure;
            initCommand.Parameters = new String[] { "@quotationServerID", "@tradeDay" };
            initCommand.Command.Parameters.Add(new SqlParameter("@quotationServerID", null));
            initCommand.Command.Parameters.Add( new SqlParameter("@tradeDay", null));

            initCommand.TableNames = new string[]{
												   "TradeDay",
												   "Instrument",
                                                   "InstrumentToDealers",
                                                   "DefaultQuotePolicyID",
                                                   "QuotePolicy",
												   "QuotePolicyDetail",
												   "TradingTime",
												   "OriginQuotation",
												   "OverridedQuotation",
                                                   "InstrumentDayOpenCloseHistory",
                                                   "SystemParameter"
			};

            //data set
            SqlCommand command = initCommand.Command;
            command.Parameters[initCommand.Parameters[0]].Value = string.Empty;
            if (tradeDay != DateTime.MinValue)
                command.Parameters[initCommand.Parameters[1]].Value = tradeDay;
            command.Connection = new SqlConnection(connectionString);
            command.CommandTimeout = 0;

            SqlDataAdapter dataAdapter = new SqlDataAdapter();
            dataAdapter.SelectCommand = command;
            DataSet dataSet = new DataSet();
            dataAdapter.Fill(dataSet);

            if (dataSet.Tables.Count != initCommand.TableNames.Length)
                throw new ApplicationException("Get initial data failed");

            //modify table name
            string[] tableNames = initCommand.TableNames;
            for (int i = 0; i < dataSet.Tables.Count; i++)
            {
                dataSet.Tables[i].TableName = tableNames[i];
            }

            return dataSet;
        }

        private void Initialize(DataSet dataSet)
        {
            this.rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                this.scheduler.Reset();

                this.originQuotations = new Dictionary<Guid, OriginQuotation>();
                this.overridedQuotations = new Dictionary<Guids,OverridedQuotation>();
                this.quotePolicies = new Dictionary<string, Guid>();
                this.quotePolicyDetails = new Dictionary<Guids,QuotePolicyDetail>();

                this.originInstruments = new Dictionary<string,OriginInstrument>();
                this.instruments = new Dictionary<Guid,Instrument>();

                this.originQReceivedList = new Dictionary<Guid,OriginQuotation>();
                this.originQSendingList = new Dictionary<Guid,OriginQuotation>();
                this.overridedQSendingList = new Dictionary<Guids,OverridedQuotation>();

                DataRowCollection dataRows;
                //Instrument
                dataRows = dataSet.Tables["Instrument"].Rows;
                foreach (DataRow instrumentRow in dataRows)
                {
                    Instrument instrument = new Instrument(instrumentRow);
                    this.instruments.Add(instrument.ID, instrument);

                    string originCode = instrument.OriginCode;
                    if (!this.originInstruments.ContainsKey(originCode))
                    {
                        this.originInstruments.Add(originCode, new OriginInstrument(instrumentRow));
                    }
                    (this.originInstruments[originCode]).Instruments.Add(instrument);
                }

                //InstrumentToDealers
                dataRows = dataSet.Tables["InstrumentToDealers"].Rows;
                foreach (DataRow instrumentToDealerRow in dataRows)
                {
                    if (instrumentToDealerRow["DealerID"] != DBNull.Value)
                    {
                        Guid instrumentID = (Guid)instrumentToDealerRow["ID"];
                        Instrument instrument = this.instruments[instrumentID];
                        instrument.AddDealer((Guid)instrumentToDealerRow["DealerID"]);
                    }
                }

                this.defaultQuotePolicyId = (Guid)dataSet.Tables["DefaultQuotePolicyID"].Rows[0]["ID"];

                dataRows = dataSet.Tables["QuotePolicy"].Rows;
                foreach (DataRow row in dataRows)
                {
                    string code = (string)row["Code"];
                    Guid id = (Guid)row["Id"];

                    this.quotePolicies.Add(code.ToLower().Trim(), id);
                }

                //QuotePolicy & Instrument
                dataRows = dataSet.Tables["QuotePolicyDetail"].Rows;
                foreach (DataRow quotePolicyRow in dataRows)
                {
                    Instrument instrument = this.instruments[(Guid)quotePolicyRow["InstrumentID"]];

                    QuotePolicyDetail quotePolicy = new QuotePolicyDetail(instrument, quotePolicyRow);
                    Guids key = new Guids(quotePolicy.ID, instrument.ID);
                    this.quotePolicyDetails.Add(key, quotePolicy);
                }

                //Origin Quotation & Instrument
                dataRows = dataSet.Tables["OriginQuotation"].Rows;
                foreach (DataRow originQRow in dataRows)
                {
                    Guid instrumentID = (Guid)originQRow["InstrumentID"];
                    Instrument instrument = this.instruments[instrumentID];

                    OriginQuotation originQ = new OriginQuotation(instrument, originQRow);

                    this.originQuotations.Add(instrumentID, originQ);
                    instrument.OriginQReceived = originQ;
                    instrument.OriginQProcessed = originQ;

                    DateTime timestamp = (DateTime)originQRow["Timestamp"];
                    if ((DateTime.Now - timestamp) < originPriceValidTime)
                    {
                        Price acceptedOrigin = null;
                        if (originQRow["AcceptedOrigin"] != DBNull.Value)
                        {
                            acceptedOrigin = Price.CreateInstance((string)originQRow["AcceptedOrigin"], instrument.NumeratorUnit, instrument.Denominator);
                        }
                        instrument.LastOrigin = acceptedOrigin;
                    }
                }

                //Overrided Quotation & QuotePolicy
                dataRows = dataSet.Tables["OverridedQuotation"].Rows;
                foreach (DataRow overridedQRow in dataRows)
                {
                    Guid instrumentID = (Guid)overridedQRow["InstrumentID"];
                    Guid quotePolicyID = (Guid)overridedQRow["QuotePolicyID"];
                    Guids key = new Guids(quotePolicyID, instrumentID);

                    Instrument instrument = this.instruments[instrumentID];
                    QuotePolicyDetail quotePolicy = this.quotePolicyDetails[key];

                    OverridedQuotation overridedQ = new OverridedQuotation(instrument, quotePolicy, overridedQRow);
                    this.overridedQuotations.Add(key, overridedQ);

                    quotePolicy.OverrideHigh = overridedQ.High;
                    quotePolicy.OverrideLow = overridedQ.Low;
                }

                //TradeDay
                dataRows = dataSet.Tables["TradeDay"].Rows;
                this.tradeDay = new TradeDay(dataRows[0]);

                Scheduler.Action action2 = new Scheduler.Action(this.OnTradeDayClose);
                this.scheduler.Add(action2, this.tradeDay, this.tradeDay.EndTime);

                //TradingTime 
                dataRows = dataSet.Tables["TradingTime"].Rows;
                foreach (DataRow tradingTime in dataRows)
                {
                    Guid instrumentID = (Guid)tradingTime["InstrumentID"];
                    DateTime beginTime = (DateTime)tradingTime["BeginTime"];
                    DateTime endTime = (DateTime)tradingTime["EndTime"];

                    Instrument instrument = this.instruments[instrumentID];
                    DateTime now = DateTime.Now;
                    if (endTime > now)
                    {
                        if (beginTime <= now && now < endTime)
                        {
                            (this.originInstruments[instrument.OriginCode]).IsTrading = true;
                            instrument.IsTrading = true;
                        }

                        if (beginTime > now && beginTime < tradeDay.EndTime)
                        {
                            //Add to Scheduler
                            Scheduler.Action action = new Scheduler.Action(this.SetTradingState);
                            this.scheduler.Add(action, new TradingTimeArgs(instrumentID, true, beginTime, endTime), beginTime);
                        }
                        if (endTime > now && endTime < tradeDay.EndTime)
                        {
                            //Add to Scheduler
                            Scheduler.Action action = new Scheduler.Action(this.SetTradingState);
                            this.scheduler.Add(action, new TradingTimeArgs(instrumentID, false, beginTime, endTime), endTime);
                        }
                    }
                }

                if (this.pendingOpenPriceInstrument == null)
                {
                    this.pendingOpenPriceInstrument = new Dictionary<Instrument,object>();
                }
                else
                {
                    this.pendingOpenPriceInstrument.Clear();
                }

                if (this.pendingOpenPriceOriginCode == null)
                {
                    this.pendingOpenPriceOriginCode = new Dictionary<string,object>();
                }
                else
                {
                    this.pendingOpenPriceOriginCode.Clear();
                }

                //EnableGenerateDailyQuotation
                this.enableGenerateDailyQuotation = (bool)dataSet.Tables["SystemParameter"].Rows[0]["EnableGenerateDailyQuotation"];
                this.highBid = (bool)dataSet.Tables["SystemParameter"].Rows[0]["HighBid"];
                this.lowBid = (bool)dataSet.Tables["SystemParameter"].Rows[0]["LowBid"];
                this._EnableChartGeneration = (bool)dataSet.Tables["SystemParameter"].Rows[0]["EnableQuotationServerChartGeneration"];
                // InstrumentDayOpenCloseHistory
                dataRows = dataSet.Tables["InstrumentDayOpenCloseHistory"].Rows;
                foreach (DataRow instrumentDayOpenClose in dataRows)
                {
                    Guid instrumentID = (Guid)instrumentDayOpenClose["InstrumentID"];

                    Instrument instrument = this.instruments[instrumentID];
                    instrument.DayOpenTime = (DateTime)instrumentDayOpenClose["DayOpenTime"];
                    instrument.DayCloseTime = (DateTime)instrumentDayOpenClose["DayCloseTime"];

                    if (this.enableGenerateDailyQuotation && !this.pendingOpenPriceInstrument.ContainsKey(instrument))
                    {
                        this.pendingOpenPriceInstrument.Add(instrument, null);
                    }
                }
                if (this.enableGenerateDailyQuotation)
                {
                    // Waiting first price to update Private/PublicDailyQuotation.Open
                    foreach (Instrument instrument in this.pendingOpenPriceInstrument.Keys)
                    {
                        if (!this.pendingOpenPriceOriginCode.ContainsKey(instrument.OriginCode))
                        {
                            bool hasDifferentDayCloseTime = false;
                            foreach (Instrument instrument2 in this.pendingOpenPriceInstrument.Keys)
                            {
                                if (instrument2.OriginCode == instrument.OriginCode && instrument2.DayCloseTime != instrument.DayCloseTime)
                                {
                                    hasDifferentDayCloseTime = true;
                                    break;
                                }
                            }
                            if (!hasDifferentDayCloseTime)
                            {
                                this.pendingOpenPriceOriginCode.Add(instrument.OriginCode, null);
                            }
                        }
                    }

                    foreach (string originCode in this.pendingOpenPriceOriginCode.Keys)
                    {
                        foreach (Instrument instrument in (this.originInstruments[originCode]).Instruments)
                        {
                            if (this.pendingOpenPriceInstrument.ContainsKey(instrument))
                            {
                                this.pendingOpenPriceInstrument.Remove(instrument);
                            }
                        }
                    }

                    // remove from pending list those instruments which has quotations after dayOpenTime
                    foreach (OverridedQuotation overridedQ in this.overridedQuotations.Values)
                    {
                        if (overridedQ.Timestamp > overridedQ.Instrument.DayOpenTime)
                        {
                            if (this.pendingOpenPriceInstrument.ContainsKey(overridedQ.Instrument))
                            {
                                this.pendingOpenPriceInstrument.Remove(overridedQ.Instrument);
                            }

                            if (this.pendingOpenPriceOriginCode.ContainsKey(overridedQ.Instrument.OriginCode))
                            {
                                this.pendingOpenPriceOriginCode.Remove(overridedQ.Instrument.OriginCode);
                            }
                        }
                    }
                }

                ////Check InactiveTime
                //Scheduler.Action action3 = new Scheduler.Action(this.TryChangePriceActive);
                //this.scheduler.Add(action3, null, DateTime.Now.AddSeconds(10));
            }
            finally
            {
                this.rwLock.ReleaseWriterLock();
            }
        }

        private CollectorQuotation[] PreProcess(CollectorQuotation[] quotations)
        {
            //Update Last Quotation
            foreach (CollectorQuotation collectorQ in quotations)
            {
                if (collectorQ == null) continue;
                if (!this.originInstruments.ContainsKey(collectorQ.OriginCode)) continue;

                OriginInstrument originInstrument = this.originInstruments[collectorQ.OriginCode];
                originInstrument.LastQuotation = collectorQ;
            }

            //Generate new Quotaitons
            ArrayList collectorQs = new ArrayList();
            foreach (OriginInstrument instrumentOrigin in this.originInstruments.Values)
            {
                if (instrumentOrigin.HasPriceConverter)
                {
                    OriginInstrument originInstrument1 = this.originInstruments[instrumentOrigin.PriceOriginCode1];
                    OriginInstrument originInstrument2 = this.originInstruments[instrumentOrigin.PriceOriginCode2];

                    CollectorQuotation collectorQ = instrumentOrigin.ConvertQuotation(originInstrument1.LastQuotation, originInstrument2.LastQuotation);
                    if (collectorQ != null)
                    {
                        collectorQs.Add(collectorQ);
                    }
                }
            }

            if (collectorQs.Count == 0)
            {
                return quotations;
            }
            else
            {
                CollectorQuotation[] quotations2 = new CollectorQuotation[quotations.Length + collectorQs.Count];
                Array.Copy(quotations, quotations2, quotations.Length);

                for (int i = 0; i < collectorQs.Count; i++)
                {
                    CollectorQuotation collectorQ = (CollectorQuotation)collectorQs[i];
                    quotations2[quotations.Length + i] = collectorQ;
                }

                return quotations2;
            }
        }

        private void AdjustTimestampOfRepeatQuotation(CollectorQuotation[] quotations)
        {
            Dictionary<string, object> handledInstuments = new Dictionary<string, object>(quotations.Length / 2);

            foreach (CollectorQuotation quotation in quotations)
            {
                TimeSpan adjustTime = TimeSpan.FromMilliseconds(4);
                foreach (CollectorQuotation quotation2 in quotations)
                {
                    if (quotation == quotation2 || handledInstuments.ContainsKey(quotation2.OriginCode))
                    {
                        continue;
                    }
                    if (quotation2.OriginCode == quotation.OriginCode)
                    {
                        quotation2.AdjustTimestamp(adjustTime);
                        adjustTime = TimeSpan.FromMilliseconds(adjustTime.TotalMilliseconds + 4);
                    }
                }
                if(!handledInstuments.ContainsKey(quotation.OriginCode)) handledInstuments.Add(quotation.OriginCode, null);
            }
        }

        private bool SetQuotation(Token token, CollectorQuotation[] quotations)
        {
            if (!acceptZeroPrice)
            {
                quotations = this.FilterZeroPrice(quotations);
            }            

            quotations = this.PreProcess(quotations);
            this.AdjustTimestampOfRepeatQuotation(quotations);

            foreach (CollectorQuotation collectorQ in quotations)
            {
                if (collectorQ == null) continue;

                //Filter Quotation
                if (!this.originInstruments.ContainsKey(collectorQ.OriginCode)) continue;
                if (!(this.originInstruments[collectorQ.OriginCode]).IsTrading) continue;

                //Integration Quotation--not implemented

                //Normalize Quotation--cast to originQuotation
                List<Instrument> instruments = (this.originInstruments[collectorQ.OriginCode]).Instruments;
                foreach (Instrument instrument in instruments)
                {
                    if (instrument.OriginCode == this.traceOriginCode)
                    {
                        AppDebug.LogEvent("QuotationServer", string.Format("SetQuotation--Id: {0}, IsTrading:{1}, DealerID: {2}, ScheduleID: {3}",
                            instrument.ID, instrument.IsTrading, instrument.LastOfferDealerID, instrument.ScheduleID), EventLogEntryType.Warning);
                    }

                    if (!instrument.IsTrading) continue;
                    //if no dealer controll the instrument
                    if (instrument.CountOfDealer == 0) continue;

                    //cast Collector Quotation to Origin Quotation
                    OriginQuotation originQ = new OriginQuotation(instrument, collectorQ);

                    //Changed to Fix the bugs (when day open, all quotaions should be reset But keep the first high-low in memory)
                    if (originQ.IsEmpty())
                    {
                        //Cache the empty OriginQuotation
                        if (this.emptyOriginQuotaions == null)
                        {
                            this.emptyOriginQuotaions = new Dictionary<Guid,OriginQuotation>();
                        }

                        OriginQuotation emptyOriginQuotation = null;
                        if (this.emptyOriginQuotaions.TryGetValue(instrument.ID, out emptyOriginQuotation))
                        {
                            emptyOriginQuotation.Merge(originQ);
                        }
                        else
                        {
                            this.emptyOriginQuotaions[instrument.ID] = originQ;
                        }

                        continue;
                    }
                    else if (this.emptyOriginQuotaions != null && this.emptyOriginQuotaions.ContainsKey(instrument.ID))
                    {
                        //Complete the quotation with the received price items before
                        OriginQuotation emptyOriginQuotaion = this.emptyOriginQuotaions[instrument.ID];
                        this.emptyOriginQuotaions.Remove(instrument.ID);

                        emptyOriginQuotaion.Merge(originQ);
                        originQ = emptyOriginQuotaion;
                    }

                    //merge to origin quotation receive list
                    OriginQuotation oldReceivedOriginQuotation = null;
                    if (this.originQReceivedList.TryGetValue(instrument.ID, out oldReceivedOriginQuotation))
                    {
                        oldReceivedOriginQuotation.Merge(originQ);
                    }
                    else
                    {
                        this.originQReceivedList[instrument.ID] = originQ;
                    }

                    //merge to last originQuotation
                    OriginQuotation oldOriginQuotation = null;
                    if (this.originQuotations.TryGetValue(instrument.ID, out oldOriginQuotation))
                    {
                        oldOriginQuotation.Merge(originQ);
                    }
                    else
                    {
                        OriginQuotation originQ2 = (OriginQuotation)originQ.Clone();

                        //Note this.originQuotations and instrument reference to the same object!
                        this.originQuotations[instrument.ID] = originQ2;
                        instrument.OriginQReceived = originQ2;
                    }
                }
            }

            //Override 
            //Note: OverrideQuotation may change this.originQuotations so it should place before. 
            this.OverrideQuotation();

            if (QuotaionExporterManager.HasExporter)
            {
                QuotaionExporterManager.AddQuotation(this.overridedQuotations.Values);
            }
            switch (this._CacheType)
            {
                case QuotationCacheType.None:
                    //Save quotation
                    this.UpdateDB<Guid, OriginQuotation>(this.originQuotations, this.originQSendingList);
                    this.UpdateDB<Guids, OverridedQuotation>(this.overridedQuotations, this.overridedQSendingList);
                    break;
                case QuotationCacheType.File:
                    this._Cache.Add(this.originQuotations.Values, this.overridedQuotations.Values);
                    break;
            }            

            if (this.enableGenerateDailyQuotation)
            {
                this.UpdateOpenPrice();
            }
            return true;
        }

        /// <summary>
        /// Update PublicDailyQuotation/PrivateDailyQuotation.Open when first quotation arrived
        /// </summary>
        /// <returns></returns>
        private void UpdateOpenPrice()
        {
            this.rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {                
                if (this.pendingOpenPriceInstrument != null && this.pendingOpenPriceInstrument.Count > 0)
                {
                    XmlDocument xmlDocument = null;
                    foreach (OverridedQuotation overridedQuotation in this.overridedQuotations.Values)
                    {
                        if (overridedQuotation.Saved && overridedQuotation.Instrument.IsTrading
                            && overridedQuotation.QuotePolicy.ID == this.defaultQuotePolicyId
                            && overridedQuotation.Timestamp > overridedQuotation.Instrument.DayOpenTime
                            && this.pendingOpenPriceInstrument.ContainsKey(overridedQuotation.Instrument))
                        {
                            // update PrivateDailyQuotation, delete instrument from PendingOpenPrice
                            double openPriceInNumeric = (Convert.ToDouble((string)overridedQuotation.Bid) + Convert.ToDouble((string)overridedQuotation.Ask)) / 2;
                            if (openPriceInNumeric != 0)
                            {
                                string openPrice = (string)Price.CreateInstance(openPriceInNumeric, overridedQuotation.Instrument.NumeratorUnit, overridedQuotation.Instrument.Denominator);
                                SqlCommand sqlCommand = iExchange.Common.DataAccess.GetCommand(this.connectionString, CommandType.StoredProcedure, "dbo.PrivateDailyQuotation_UpdateOpenPrice");
                                sqlCommand.Parameters["@tradeDay"].Value = this.tradeDay.Day;
                                sqlCommand.Parameters["@instrumentId"].Value = overridedQuotation.Instrument.ID;
                                sqlCommand.Parameters["@open"].Value = openPrice;
                                sqlCommand.Parameters["@openPriceExisted"].Value = DBNull.Value;

                                if (0 == (int)iExchange.Common.DataAccess.Execute(sqlCommand) && !(bool)sqlCommand.Parameters["@openPriceExisted"].Value)
                                {
                                    if (xmlDocument == null)
                                    {
                                        xmlDocument = new XmlDocument();
                                        xmlDocument.LoadXml("<Update xmlns=\"\"><Add></Add></Update>");
                                    }
                                    this.AppendOpenPriceNotifyItem(xmlDocument, openPrice, overridedQuotation.Instrument, false);
                                }
                                this.pendingOpenPriceInstrument.Remove(overridedQuotation.Instrument);
                            }
                        }
                    }
                    if (xmlDocument != null)
                    {
                        this.scheduler.Add(new Scheduler.Action(this.NotifyOpenPrice), xmlDocument, DateTime.Now.AddSeconds(10), true);
                    }

                    if (this.pendingOpenPriceInstrument.Count == 0)
                    {
                        this.pendingOpenPriceInstrument = null;
                    }
                }

                if (this.pendingOpenPriceOriginCode != null && this.pendingOpenPriceOriginCode.Count > 0)
                {
                    XmlDocument xmlDocument = null;
                    foreach (OverridedQuotation overridedQuotation in this.overridedQuotations.Values)
                    {
                        if (overridedQuotation.Saved && overridedQuotation.Instrument.IsTrading
                            && overridedQuotation.QuotePolicy.ID == this.defaultQuotePolicyId
                            && overridedQuotation.Timestamp > overridedQuotation.Instrument.DayOpenTime
                            && this.pendingOpenPriceOriginCode.ContainsKey(overridedQuotation.Instrument.OriginCode))
                        {
                            // update PublicDailyquotation , delete all instruments which has same OriginCode from PendingOpenPrice
                            double openPriceInNumeric = (Convert.ToDouble((string)overridedQuotation.Bid) + Convert.ToDouble((string)overridedQuotation.Ask)) / 2;
                            if (openPriceInNumeric != 0)
                            {
                                string openPrice = (string)Price.CreateInstance(openPriceInNumeric, overridedQuotation.Instrument.NumeratorUnit, overridedQuotation.Instrument.Denominator);
                                SqlCommand sqlCommand = iExchange.Common.DataAccess.GetCommand(this.connectionString, CommandType.StoredProcedure, "dbo.PublicDailyQuotation_UpdateOpenPrice");
                                sqlCommand.Parameters["@tradeDay"].Value = this.tradeDay.Day;
                                sqlCommand.Parameters["@originCode"].Value = overridedQuotation.Instrument.OriginCode;
                                sqlCommand.Parameters["@open"].Value = openPrice;
                                sqlCommand.Parameters["@openPriceExisted"].Value = DBNull.Value;

                                if (0 == (int)iExchange.Common.DataAccess.Execute(sqlCommand) && !(bool)sqlCommand.Parameters["@openPriceExisted"].Value)
                                {
                                    if (xmlDocument == null)
                                    {
                                        xmlDocument = new XmlDocument();
                                        xmlDocument.LoadXml("<Update xmlns=\"\"><Add></Add></Update>");
                                    }
                                    this.AppendOpenPriceNotifyItem(xmlDocument, openPrice, overridedQuotation.Instrument, true);
                                }
                                this.pendingOpenPriceOriginCode.Remove(overridedQuotation.Instrument.OriginCode);
                            }
                        }
                    }

                    if (xmlDocument != null)
                    {
                        this.scheduler.Add(new Scheduler.Action(this.NotifyOpenPrice), xmlDocument, DateTime.Now.AddSeconds(10), true);
                    }

                    if (this.pendingOpenPriceOriginCode.Count == 0)
                    {
                        this.pendingOpenPriceOriginCode = null;
                    }
                }
            }
            catch (Exception exception)
            {
                AppDebug.LogEvent("QuotationServer", string.Format("UpdateOpenPrice failed:{0}", exception), EventLogEntryType.Error);
            }

            finally
            {
                this.rwLock.ReleaseWriterLock();
            }
        }

        private void AppendOpenPriceNotifyItem(XmlDocument xmlDocument, string openPrice, Instrument instrument, bool isPublicDailyQuotation)
        {
            XmlNode xmlNode = null;
            if (isPublicDailyQuotation)
            {
                xmlNode = xmlDocument.CreateNode(XmlNodeType.Element, "PublicDailyQuotation", null);
            }
            else
            {
                xmlNode = xmlDocument.CreateNode(XmlNodeType.Element, "PrivateDailyQuotation", null);
            }

            XmlAttribute attribute = xmlDocument.CreateAttribute("Open");
            attribute.Value = openPrice;
            xmlNode.Attributes.Append(attribute);

            if (isPublicDailyQuotation)
            {
                attribute = xmlDocument.CreateAttribute("InstrumentOriginCode");
                attribute.Value = instrument.OriginCode;
                xmlNode.Attributes.Append(attribute);

                OriginInstrument instrumentOrigin = null;
                if (this.originInstruments.TryGetValue(instrument.OriginCode, out instrumentOrigin))
                {
                    foreach (Instrument instrumentItem in instrumentOrigin.Instruments)
                    {
                        XmlNode instrumentNode = xmlDocument.CreateNode(XmlNodeType.Element, "Instrument", null);

                        attribute = xmlDocument.CreateAttribute("ID");
                        attribute.Value = XmlConvert.ToString(instrumentItem.ID);
                        instrumentNode.Attributes.Append(attribute);

                        attribute = xmlDocument.CreateAttribute("DayCloseTime");
                        attribute.Value = instrumentItem.DayCloseTime.ToString("yyyy-MM-dd HH:mm:ss");
                        instrumentNode.Attributes.Append(attribute);

                        xmlNode.AppendChild(instrumentNode);
                    }
                }
            }
            else
            {
                attribute = xmlDocument.CreateAttribute("InstrumentID");
                attribute.Value = XmlConvert.ToString(instrument.ID);
                xmlNode.Attributes.Append(attribute);

                attribute = xmlDocument.CreateAttribute("DayCloseTime");
                attribute.Value = instrument.DayCloseTime.ToString("yyyy-MM-dd HH:mm:ss");
                xmlNode.Attributes.Append(attribute);
            }

            attribute = xmlDocument.CreateAttribute("TradeDay");
            attribute.Value = this.tradeDay.Day.ToString("yyyy-MM-dd HH:mm:ss");
            xmlNode.Attributes.Append(attribute);

            xmlDocument.FirstChild.FirstChild.AppendChild(xmlNode);
        }

        private void NotifyOpenPrice(object sender, object actionArgs)
        {
            XmlDocument xmlDocument = (XmlDocument)actionArgs;
            Token token = new Token();
            token.SessionID = "NotifyOpenPrice";
            token.UserID = Guid.Empty;
            token.AppType = AppType.QuotationServer;
            token.UserType = UserType.System;
            try
            {
                this._StateServer.Update(token, xmlDocument.DocumentElement);

                if (this.traceOpenPrice)
                {
                    AppDebug.LogEvent("QuotationServer", string.Format("NotifyOpenPrice:{0}", xmlDocument.OuterXml), EventLogEntryType.Information);
                }
            }
            catch (Exception exception)
            {
                this.scheduler.Add(new Scheduler.Action(this.NotifyOpenPrice), xmlDocument, DateTime.Now.AddSeconds(10), true);
                AppDebug.LogEvent("QuotationServer", string.Format("NotifyOpenPrice:{0}\r\n {1}", exception, xmlDocument.OuterXml), EventLogEntryType.Warning);
            }
        }

        private CollectorQuotation[] FilterZeroPrice(CollectorQuotation[] quotations)
        {
            if (quotations != null && quotations.Length > 0)
            {
                List<CollectorQuotation> quotations2 = new List<CollectorQuotation>();
                StringBuilder discardQuotation = new StringBuilder();
                foreach (CollectorQuotation quotation in quotations)
                {
                    quotation.EmptyZeroPrice();
                    if (!quotation.IsEmpty())
                    {
                        quotations2.Add(quotation);
                    }
                    else
                    {
                        if (discardQuotation.Length > 0) discardQuotation.Append(",");
                        discardQuotation.Append(quotation.OriginCode);
                    }
                }
                //Debug.WriteLine(string.Format("QuotationServer.FilterZeroPrice={0}", discardQuotation));
                return quotations2.ToArray();
            }
            else
            {
                return quotations;
            }
        }

        private bool SetQuotation(Token token, DealerQuotation[] quotations)
        {
            foreach (DealerQuotation dealerQ in quotations)
            {
                if (dealerQ == null) continue;

                Instrument instrument = this.instruments[dealerQ.InstrumentID];
                if (!instrument.IsTrading)
                {
                    AppDebug.LogEvent("QuotationServer.SetQuotation", string.Format("Quotation: {0} is ingored for the instrument is not in trading", dealerQ), EventLogEntryType.Warning);
                    continue;
                }

                dealerQ.NormalizeOrigins(instrument);

                if (dealerQ.Origin == null) continue;

                if (instrument.ScheduleID != null)
                {
                    if (this.scheduler[instrument.ScheduleID] != null)
                    {
                        //if dealerQ and the waiting originQ has same timestamp then remove server's override
                        OriginQuotation originQ = (OriginQuotation)this.scheduler[instrument.ScheduleID].ActionArgs;
                        if (((TimeSpan)(dealerQ.Timestamp - originQ.Timestamp)).TotalSeconds < 1)
                        {
                            if (instrument.OriginCode == this.traceOriginCode)
                            {
                                AppDebug.LogEvent("QuotationServer", string.Format("SetQuotation Dealer Right--Id: {0}, IsTrading:{1}, DealerID: {2}, ScheduleID: {3}",
                                    instrument.ID, instrument.IsTrading, instrument.LastOfferDealerID, instrument.ScheduleID), EventLogEntryType.Warning);
                            }

                            this.scheduler.Remove(instrument.ScheduleID);
                            instrument.ScheduleID = null;
                        }
                    }
                    else
                    {
                        if (instrument.OriginCode == this.traceOriginCode)
                        {
                            AppDebug.LogEvent("QuotationServer", string.Format("SetQuotation Dealer Error--Id: {0}, IsTrading:{1}, DealerID: {2}, ScheduleID: {3}",
                                instrument.ID, instrument.IsTrading, instrument.LastOfferDealerID, instrument.ScheduleID), EventLogEntryType.Warning);
                        }

                        instrument.ScheduleID = null;
                    }
                }
                else
                {
                    dealerQ.Timestamp = DateTime.Now;
                }

                //process dealer's override
                instrument.LastOfferDealerID = token.UserID;
                ProcessQuotePolicy(dealerQ);
            }

            if (QuotaionExporterManager.HasExporter)
            {
                QuotaionExporterManager.AddQuotation(this.overridedQuotations.Values);
            }
            switch (this._CacheType)
            {
                case QuotationCacheType.None:
                    //Save quotation
                    this.UpdateDB<Guid, OriginQuotation>(this.originQuotations, this.originQSendingList);
                    this.UpdateDB<Guids, OverridedQuotation>(this.overridedQuotations, this.overridedQSendingList);
                    break;
                case QuotationCacheType.File:
                    this._Cache.Add(this.originQuotations.Values, this.overridedQuotations.Values);
                    break;
            }

            if (this.enableGenerateDailyQuotation)
            {
                this.UpdateOpenPrice();
            }
            return true;
        }

        private void OverrideQuotation()
        {
            ArrayList processedEntries = new ArrayList();

            DateTime baseTime = DateTime.Now;
            foreach (KeyValuePair<Guid, OriginQuotation> de in this.originQReceivedList)
            {
                OriginQuotation originQ = de.Value;

                Instrument instrument = originQ.Instrument;
                if (instrument.ScheduleID != null) continue;

                //if (instrument.HasWatchOnlyQuotePolicies)//WatchOnly is not supported
                //{
                //    this.AddToSendingList(instrument, originQ);
                //}
                //else
                {
                    //Process origin quotation
                    ProcessInstrument(originQ, baseTime);
                }

                processedEntries.Add((Guid)de.Key);
            }

            //Remove all processed Entries
            foreach (Guid instrumentID in processedEntries)
            {
                this.originQReceivedList.Remove(instrumentID);
            }
        }

        //private void ChangePriceEnable(Instrument instrument, bool priceEnable)
        //{
        //    if (instrument.ChangePriceEnable(priceEnable))
        //    {
        //        XmlDocument xmlDoc = new XmlDocument();
        //        XmlElement updateNode = xmlDoc.CreateElement("Update");
        //        XmlElement modifyNode = xmlDoc.CreateElement("Modify");
        //        updateNode.AppendChild(modifyNode);

        //        string sql = instrument.GetUpdateSql();
        //        modifyNode.AppendChild(instrument.GetUpdateNode(xmlDoc));
        //        try
        //        {
        //            iExchange.Common.DataAccess.UpdateDB(sql, this.connectionString);

        //            this._StateServer.Update(QuotationServer.Token, updateNode);
        //        }
        //        catch (Exception exception)
        //        {
        //            AppDebug.LogEvent("QuotationServer.ChangePriceEnable", exception.ToString(), EventLogEntryType.Error);
        //        }
        //    }
        //}

        private void ProcessInstrument(OriginQuotation originQ, DateTime baseTime)
        {
            Instrument instrument = originQ.Instrument;

            //Must have a origin
            if (originQ.Origin != null)
            {
                //Process
                originQ.IsProblematic = instrument.IsProblematic(originQ.Origin);
                //if (originQ.IsProblematic)
                //{
                //    AppDebug.LogEvent("QuotationServer", string.Format("ProcessInstrument, price is out range: {0}", originQ), EventLogEntryType.Information);
                //    this.ChangePriceEnable(instrument, false);
                //}

                int waitTime = instrument.GetWaitTime(originQ.Origin);

                if (this.isReplay || waitTime == 0)
                {
                    // loop all instrument's QuotePolicy
                    ProcessQuotePolicy(originQ);
                }
                else
                {
                    OriginQuotation originQ2 = (OriginQuotation)originQ.Clone();

                    //Add to Scheduler
                    Scheduler.Action action = new Scheduler.Action(this.QuotationWaitTimeout);
                    instrument.ScheduleID = this.scheduler.Add(action, originQ2, baseTime, new TimeSpan(0, 0, waitTime));

                    Trace.TraceInformation("ProcessInstrument: Id=={0}, IsTrading=={1}, DealerID=={2}, ScheduleID=={3}, WaitTime=={4}, baseTime=={5:yyyy-MM-dd HH:mm:ss.fff}, originQ=={6}, LastOrigin=={7}",
                            instrument.ID, instrument.IsTrading, instrument.LastOfferDealerID, instrument.ScheduleID, waitTime, baseTime, originQ, instrument.LastOrigin);

                    if (instrument.OriginCode == this.traceOriginCode)
                    {
                        AppDebug.LogEvent("QuotationServer", string.Format("ProcessInstrument: Id=={0}, IsTrading=={1}, DealerID=={2}, ScheduleID=={3}, WaitTime=={4}, baseTime=={5:yyyy-MM-dd HH:mm:ss.fff}, originQ=={6}, LastOrigin=={7}",
                            instrument.ID, instrument.IsTrading, instrument.LastOfferDealerID, instrument.ScheduleID, waitTime, baseTime, originQ, instrument.LastOrigin), EventLogEntryType.Warning);
                    }
                }
            }

            this.AddToSendingList(instrument, originQ);
        }

        private void AddToSendingList(Instrument instrument, OriginQuotation originQ)
        {
            //Add to Sending Queue of Origin
            this.originQSendingList[instrument.ID] = originQ;

            //2004-12-30
            if (instrument.OriginQProcessed == null)
            {
                instrument.OriginQProcessed = originQ;
            }
            else
            {
                instrument.OriginQProcessed.Merge(originQ);
            }
        }

        // server's process
        private void ProcessQuotePolicy(OriginQuotation originQ)
        {
            Instrument instrument = originQ.Instrument;
            if (instrument.IsTrading && (this.isReplay || !instrument.IsProblematic(originQ.Origin)))
            {
                instrument.RefreshOrigins(originQ.Timestamp, originQ.Origin, originQ.Volume, originQ.TotalVolume, originQ.High, originQ.Low, false);

                //loop all quote policy
                foreach (QuotePolicyDetail quotePolicy in instrument.QuotePolicyDetails)
                {
                    Guids key = new Guids(quotePolicy.ID, instrument.ID);

                    //Calculate OverrideQuotaton
                    OverridedQuotation OverridedQ = new OverridedQuotation(instrument, quotePolicy, originQ);
                    OverridedQ.CalculateHiLo(this.highBid, this.lowBid);

                    //Add to Sending Queue of Overrided
                    this.overridedQSendingList[key] = OverridedQ;

                    //update last OverridedQuotation
                    OverridedQuotation oldOverridedQuotation = null;
                    if (this.overridedQuotations.TryGetValue(key, out oldOverridedQuotation))
                    {
                        oldOverridedQuotation.Merge(OverridedQ);
                    }
                    else
                    {
                        this.overridedQuotations[key] = OverridedQ;
                    }

                }
            }

            //??? if queued new origin quotation then process it directly
            //NOTE: the following condition is always true ,becase this method is in enumurating originQReceivedList
            OriginQuotation originQuotationReceived = null;
            if (this.originQReceivedList.TryGetValue(instrument.ID, out originQuotationReceived))
            {
                if (originQuotationReceived.Timestamp > originQ.Timestamp)
                {
                    ProcessInstrument(originQuotationReceived, DateTime.Now);
                }

                //Will be removed in OverrideQuotation
                //this.originQReceivedList.Remove(instrument.ID);
            }
        }

        // Dealer's process
        private void ProcessQuotePolicy(DealerQuotation dealerQ)
        {
            Instrument instrument = this.instruments[dealerQ.InstrumentID];

            Trace.TraceInformation("ProcessQuotePolicy: dealerQ=={0}", dealerQ);
            if (dealerQ.OriginHigh != instrument.OriginHigh || dealerQ.OriginLow != instrument.OriginLow)
            {
                AppDebug.LogEvent("QuotationServer", string.Format("Dealer change High, low: dealerQ=={0}", dealerQ), EventLogEntryType.Warning);
            }

            //Filter error High,Low
            instrument.UpdateErrorHighLow(dealerQ.OriginHigh, dealerQ.OriginLow);
            instrument.RefreshOrigins(dealerQ.Timestamp, dealerQ.Origin, dealerQ.Volume, dealerQ.TotalVolume, dealerQ.OriginHigh, dealerQ.OriginLow, true);

            //loop all quote policy
            foreach (QuotePolicyDetail quotePolicy in instrument.QuotePolicyDetails)
            {
                Guids key = new Guids(quotePolicy.ID, instrument.ID);

                //Calculate OverrideQuotaton
                OverridedQuotation OverridedQ = new OverridedQuotation(instrument, quotePolicy);
                OverridedQ.CalculateHiLo(this.highBid, this.lowBid);
                
                //update last OverridedQuotation
                OverridedQuotation oldOverridedQuotation = null;
                if (this.overridedQuotations.TryGetValue(key, out oldOverridedQuotation))
                {
                    OverridedQ = OverridedQ.GetChanges(oldOverridedQuotation);
                    oldOverridedQuotation.Merge(OverridedQ);
                }
                else
                {
                    this.overridedQuotations[key] = OverridedQ;
                }

                //Add to Sending Queue of Overrided
                this.overridedQSendingList[key] = OverridedQ;
            }

            //??? new origin quotation
            OriginQuotation originQ = null;
            if (instrument.ScheduleID == null && this.originQReceivedList.TryGetValue(instrument.ID, out originQ))
            {
                if (originQ.Timestamp > dealerQ.Timestamp)
                {
                    originQ.FilterErrorHighLow(instrument, true);

                    ProcessInstrument(originQ, DateTime.Now);
                }
                this.originQReceivedList.Remove(instrument.ID);
            }
        }

        int originCount = 0;
        private void UpdateDB<KType, VType>(Dictionary<KType, VType> quotations, Dictionary<KType, VType> sendingList)
            where VType : PersistObject
        {
            using (SqlConnection connection = new SqlConnection(this.connectionString))
            {
                try
                {
                    connection.Open();
                }
                catch (Exception exception)
                {
                    AppDebug.LogEvent("QuotationServer", exception.ToString(), EventLogEntryType.Error);
                    this.overridedQSendingList.Clear();
                    return;
                }

                foreach (KeyValuePair<KType, VType> quotation in quotations)
                {
                    if (quotation.Value.ModifyState == ModifyState.Unchanged) continue;

                    int result = -1;
                    try
                    {
                        string sqlString = quotation.Value.GetUpdateSql(true);
                        if (!string.IsNullOrEmpty(sqlString))
                        {
                            SqlCommand sqlCommand = new SqlCommand(sqlString, connection);
                            SqlParameter sqlParameter = sqlCommand.Parameters.Add("@RETURN_VALUE", SqlDbType.Int);
                            sqlParameter.Direction = ParameterDirection.ReturnValue;
                            sqlCommand.ExecuteNonQuery();
                            result = (int)sqlCommand.Parameters["@RETURN_VALUE"].Value;
                        }
                        if (quotation.Value is OriginQuotation)
                        {
                            Debug.WriteLine("origin saved :{0}", originCount++);
                        }
                    }
                    catch (Exception exception)
                    {
                        AppDebug.LogEvent("QuotationServer", exception.ToString() + Environment.NewLine + 
                        quotation.Value.ToString(), EventLogEntryType.Error);
                    }

                    quotation.Value.Saved = (result == 0);
                    if (!quotation.Value.Saved)
                    {
                        sendingList.Remove(quotation.Key);
                    }
                }
            }
        }

        private void BuildLightQuotation(ref iExchange.Common.OriginQuotation[] originQs, ref iExchange.Common.OverridedQuotation[] overridedQs)
        {
            this.BuildLightQuotation(null, ref originQs, ref overridedQs);
        }

        private void BuildLightQuotation(DealerQuotation[] dealerQs, ref iExchange.Common.OriginQuotation[] originQs, ref iExchange.Common.OverridedQuotation[] overridedQs)
        {
            if (this.originQSendingList.Count > 0 || (dealerQs != null && dealerQs.Length > 0))
            {
                originQs = new iExchange.Common.OriginQuotation[this.originQSendingList.Count + (dealerQs == null ? 0 : dealerQs.Length)];

                int i = 0;
                foreach (OriginQuotation oq in this.originQSendingList.Values)
                {
                    originQs[i++] = oq.ToLightVersion();
                }

                if (dealerQs != null)
                {
                    foreach (DealerQuotation dq in dealerQs)//Broadcast dealer input price as origin price to all other dealers
                    {
                        Instrument instrument = this.instruments[dq.InstrumentID];
                        originQs[i++] = dq.ToLightVersion(instrument);
                    }
                }
            }

            if (this.overridedQSendingList.Count > 0)
            {
                overridedQs = new iExchange.Common.OverridedQuotation[this.overridedQSendingList.Count];
                int i = 0;
                foreach (OverridedQuotation oq in this.overridedQSendingList.Values)
                {
                    overridedQs[i++] = oq.ToLightVersion();
                }
            }

            this.originQSendingList.Clear();
            this.overridedQSendingList.Clear();

            if (this._ChartQuotationManager != null && overridedQs != null && overridedQs.Length > 0)
            {
                this._ChartQuotationManager.Add(overridedQs);
            }
        }

        public bool Update(Token token, XmlNode update)
        {
            AppDebug.LogEvent("QuotationServer", "QuotationServer.Update(Token token, XmlNode update) \r\n" + update.OuterXml, EventLogEntryType.Information);

            this.rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                foreach (XmlNode method in update.ChildNodes)
                {
                    foreach (XmlNode row in method.ChildNodes)
                    {
                        switch (row.Name)
                        {
                            case "Instruments":
                                foreach (XmlNode row2 in row.ChildNodes)
                                {
                                    this.UpdateInstrument(method, row2);
                                }
                                break;
                            case "Instrument":
                                this.UpdateInstrument(method, row);
                                break;
                            case "QuotePolicyDetail":
                                if (!UpdateQuotePolicyDetail(method, row))
                                {
                                    AppDebug.LogEvent("QuotationServer", string.Format("QuotationServer.Update QuotePolicyDetail--> not found: {0},{1}\r\n{2}", method.Name, row.Name, update.OuterXml), EventLogEntryType.Warning);
                                }
                                break;
                            case "QuotePolicyDetails":
                                foreach (XmlNode row2 in row.ChildNodes)
                                {
                                    if (!this.UpdateQuotePolicyDetail(method, row2))
                                    {
                                        AppDebug.LogEvent("QuotationServer", string.Format("QuotationServer.Update QuotePolicyDetails--> not found: {0},{1}\r\n{2}", method.Name, row2.OuterXml, update.OuterXml), EventLogEntryType.Warning);
                                    }
                                }
                                break;
                            case "SystemParameter":
                                foreach (XmlAttribute attribut in row.Attributes)
                                {
                                    String nodeName = attribut.Name;
                                    String nodeValue = attribut.Value;
                                    if (nodeName == "HighBid")
                                    {
                                        this.highBid = bool.Parse(nodeValue);
                                    }
                                    else if (nodeName == "LowBid")
                                    {
                                        this.lowBid = bool.Parse(nodeValue);
                                    }
                                }
                                break;
                            default:
                                AppDebug.LogEvent("QuotationServer", string.Format("QuotationServer.Update --> not found: {0},{1}\r\n{2}", method.Name, row.Name, update.OuterXml), EventLogEntryType.Warning);
                                break;

                        }
                    }
                }
            }
            catch (Exception e)
            {
                AppDebug.LogEvent("QuotationServer", e.ToString(), EventLogEntryType.Error);
            }
            finally
            {
                this.rwLock.ReleaseWriterLock();
            }

            return true;
        }

        private bool UpdateQuotePolicyDetail(XmlNode method, XmlNode row)
        {
            if (method.Name == "Modify")
            {
                Guid quotePolicyID = XmlConvert.ToGuid(row.Attributes["QuotePolicyID"].Value);
                Guid instrumentID = XmlConvert.ToGuid(row.Attributes["InstrumentID"].Value);

                Guid oldQuotePolicyID, oldInstrumentID;
                if (row["OldPrimaryKey"] == null)
                {
                    oldQuotePolicyID = quotePolicyID;
                    oldInstrumentID = instrumentID;
                }
                else
                {
                    oldQuotePolicyID = XmlConvert.ToGuid(row["OldPrimaryKey"].Attributes["QuotePolicyID"].Value);
                    oldInstrumentID = XmlConvert.ToGuid(row["OldPrimaryKey"].Attributes["InstrumentID"].Value);
                }
                QuotePolicyDetail quotePolicyDetail = null;
                if (!this.quotePolicyDetails.TryGetValue(new Guids(oldQuotePolicyID, oldInstrumentID), out quotePolicyDetail))
                {                    
                    return false;
                }

                quotePolicyDetail.Update(row, this.instruments[instrumentID]);
            }
            else if (method.Name == "Add")
            {
                Guid instrumentID = XmlConvert.ToGuid(row.Attributes["InstrumentID"].Value);
                Instrument instrument = this.instruments[instrumentID];

                QuotePolicyDetail quotePolicy = new QuotePolicyDetail(instrument, row);
                Guids key = new Guids(quotePolicy.ID, instrument.ID);
                this.quotePolicyDetails.Add(key, quotePolicy);
            }
            else if (method.Name == "Delete")
            {
                Guid quotePolicyID = XmlConvert.ToGuid(row.Attributes["QuotePolicyID"].Value);
                Guid instrumentID = XmlConvert.ToGuid(row.Attributes["InstrumentID"].Value);
                Guids key = new Guids(quotePolicyID, instrumentID);
                QuotePolicyDetail quotePolicy = null;

                if (this.quotePolicyDetails.TryGetValue(key, out quotePolicy))
                {
                    this.quotePolicyDetails.Remove(key);
                    quotePolicy.Dispose();
                }
            }
            return true;
        }

        private void UpdateInstrument(XmlNode method, XmlNode row)
        {
            if (method.Name == "Delete")
            {
                Guid id = XmlConvert.ToGuid(row.Attributes["ID"].Value);
                Instrument instrument = null;                
                if (this.instruments.TryGetValue(id, out instrument))
                {
                    this.instruments.Remove(id);
                    string originCode = instrument.OriginCode;
                    if (this.originInstruments.ContainsKey(originCode))
                    {
                        (this.originInstruments[originCode]).Instruments.Remove(instrument);
                    }
                    instrument.Dispose();
                }
            }
            else if (method.Name == "Modify")
            {
                Guid id = XmlConvert.ToGuid(row.Attributes["ID"].Value);
                Instrument instrument = null;
                if (this.instruments.TryGetValue(id, out instrument))
                {
                    OriginInstrument oldOriginInstrument = this.originInstruments[instrument.OriginCode];
                    instrument.Update(row);

                    if (instrument.OriginCode == oldOriginInstrument.OriginCode)
                    {
                        oldOriginInstrument.Update(row);
                    }
                    else //OriginCode changed
                    {
                        //old
                        oldOriginInstrument.Instruments.Remove(instrument);

                        //new
                        OriginInstrument newOriginInstrument = null;
                        if(!this.originInstruments.TryGetValue(instrument.OriginCode, out newOriginInstrument))
                        {
                            newOriginInstrument = new OriginInstrument(row);
                            this.originInstruments.Add(newOriginInstrument.OriginCode, newOriginInstrument);
                        }

                        newOriginInstrument.Instruments.Add(instrument);
                    }
                }
            }
            else if (method.Name == "Add") //can be ignored
            {
                Instrument instrument = new Instrument(row);
                this.instruments.Add(instrument.ID, instrument);

                OriginInstrument instrumentOrigin = null; 
                if (!this.originInstruments.TryGetValue(instrument.OriginCode, out instrumentOrigin))
                {
                    instrumentOrigin = new OriginInstrument(row);
                    this.originInstruments.Add(instrumentOrigin.OriginCode, instrumentOrigin);
                }
                else
                {
                    instrumentOrigin.Update(row);
                }

                instrumentOrigin.Instruments.Add(instrument);
            }
        }

        #region HistoryQuotationProcess

        public void UpdateHighLow(Token token, string ip, Guid instrumentId, bool isOriginHiLo, string newInput, bool isUpdateHigh, out int batchProcessId, out string instrumentCode, out bool highBid, out bool lowBid, out DateTime updateTime, out DateTime minTimestamp, out iExchange.Common.OverridedQuotation[] overridedQs, out int returnValue, out string errorMessage)
        {
            //--0: Succeed
            //---1: FailedOnDBQuotation
            //---2: InvalidNewInput
            //---3: NotEffectedRecord
            //---4: FailedDeleteChartData
            //---5: FailedOnDBV3
            //-10: FailedCallOnQuotationServer
            //-11: FailedCallOnStateServer
            returnValue = -10;
            errorMessage = "FailedCallOnQuotationServer";
            batchProcessId = -1;
            instrumentCode = string.Empty;
            highBid = true;
            lowBid = true;
            updateTime = DateTime.MinValue;
            minTimestamp = DateTime.MinValue;
            overridedQs = null;
            if (!isOriginHiLo)
            {
                string message = string.Format("Token={0},IP={1},InstrumentId={2},IsOriginHiLo={3},NewInput={4},IsUpdateHigh={5}", token.ToString(), ip, instrumentId, isOriginHiLo, newInput, isUpdateHigh);
                AppDebug.LogEvent("QuotationServer.UpdateHighLow.Begin", message, EventLogEntryType.Information);

                this.rwLock.AcquireWriterLock(Timeout.Infinite);
                try
                {
                    TransactionOptions transactionOption = new TransactionOptions();
                    transactionOption.IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted;
                    transactionOption.Timeout = new TimeSpan(0, 3, 0);
                    this.Flush();
                    using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.Required, transactionOption))
                    {
                        try
                        {
                            if (this._ChartQuotationManager != null)
                            {
                                this._ChartQuotationManager.Suspend();
                            }
                            SqlConnection sqlConnection = new SqlConnection(this.connectionString);

                            SqlCommand sqlCommand = sqlConnection.CreateCommand();
                            sqlCommand.CommandTimeout = 0;
                            sqlCommand.CommandType = CommandType.StoredProcedure;
                            sqlCommand.CommandText = "P_UpdateHighLow";

                            SqlParameter parameter = sqlCommand.Parameters.Add("@instrumentId", SqlDbType.UniqueIdentifier);
                            parameter.Value = @instrumentId;

                            parameter = sqlCommand.Parameters.Add("@newInput", SqlDbType.VarChar, 10);
                            parameter.Value = newInput;

                            parameter = sqlCommand.Parameters.Add("@isUpdateHigh", SqlDbType.Bit);
                            parameter.Value = isUpdateHigh;

                            parameter = sqlCommand.Parameters.Add("@dealerID", SqlDbType.UniqueIdentifier);
                            parameter.Value = token.UserID;

                            parameter = sqlCommand.Parameters.Add("@ip", SqlDbType.NVarChar, 15);
                            parameter.Value = ip;

                            parameter = sqlCommand.Parameters.Add("@batchProcessId", SqlDbType.Int);
                            parameter.Direction = ParameterDirection.Output;

                            parameter = sqlCommand.Parameters.Add("@instrumentCode", SqlDbType.NVarChar, 20);
                            parameter.Direction = ParameterDirection.Output;

                            parameter = sqlCommand.Parameters.Add("@highBid", SqlDbType.Bit);
                            parameter.Direction = ParameterDirection.Output;

                            parameter = sqlCommand.Parameters.Add("@lowBid", SqlDbType.Bit);
                            parameter.Direction = ParameterDirection.Output;

                            parameter = sqlCommand.Parameters.Add("@updateTime", SqlDbType.DateTime);
                            parameter.Direction = ParameterDirection.Output;

                            parameter = sqlCommand.Parameters.Add("@minTimestamp", SqlDbType.DateTime);
                            parameter.Direction = ParameterDirection.Output;

                            parameter = sqlCommand.Parameters.Add("@openPrice", SqlDbType.VarChar, 10);
                            parameter.Direction = ParameterDirection.Output;

                            parameter = sqlCommand.Parameters.Add("@returnValue", SqlDbType.Int);
                            parameter.Direction = ParameterDirection.ReturnValue;

                            //sqlConnection.Open();
                            //sqlCommand.ExecuteNonQuery();

                            SqlDataAdapter dataAdapter = new SqlDataAdapter();
                            dataAdapter.SelectCommand = sqlCommand;
                            DataSet dataSet = new DataSet();
                            
                            dataAdapter.Fill(dataSet);

                            returnValue = (int)sqlCommand.Parameters["@returnValue"].Value;
                            if (returnValue != 0)
                            {
                                if (returnValue == -1)
                                {
                                    errorMessage = "FailedOnDBQuotation";
                                }
                                else if (returnValue == -2)
                                {
                                    errorMessage = "InvalidNewInput";
                                    if (sqlCommand.Parameters["@openPrice"].Value != DBNull.Value)
                                    {
                                        var openPrice = (string)sqlCommand.Parameters["@openPrice"].Value;
                                        errorMessage += " OpenPrice:" + openPrice;
                                    }
                                }
                                else if (returnValue == -3)
                                {
                                    errorMessage = "NotEffectedRecord";
                                }
                                else if (returnValue == -4)
                                {
                                    errorMessage = "FailedDeleteChartData";
                                }
                                else if (returnValue == -5)
                                {
                                    errorMessage = "FailedOnDBV3";
                                }
                                else
                                {
                                    errorMessage = "DB_ReturnValue:" + returnValue.ToString();
                                }

                                AppDebug.LogEvent("QuotationServer.UpdateHighLow.EXEC P_UpdateHighLow(Failed)", message + "\r\n" + string.Format("Return {0}", returnValue), EventLogEntryType.Warning);
                            }
                            else
                            {
                                batchProcessId = (int)sqlCommand.Parameters["@batchProcessId"].Value;
                                instrumentCode = (string)sqlCommand.Parameters["@instrumentCode"].Value;
                                highBid = (bool)sqlCommand.Parameters["@highBid"].Value;
                                lowBid = (bool)sqlCommand.Parameters["@lowBid"].Value;
                                updateTime = (DateTime)sqlCommand.Parameters["@updateTime"].Value;
                                minTimestamp = (DateTime)sqlCommand.Parameters["@minTimestamp"].Value;

                                this.UpdateHighLows(token.UserID, instrumentId, isUpdateHigh, dataSet, out overridedQs);

                                transactionScope.Complete();
                                errorMessage = "Succeed";

                                AppDebug.LogEvent("QuotationServer.UpdateHighLow.EXEC P_UpdateHighLow(Succeed)", message + "\r\n" + string.Format("Return {0}", returnValue), EventLogEntryType.Information);
                            }
                            if (dataSet.Tables.Count >= 2)
                            {
                                if (this._ChartQuotationManager != null)
                                {
                                    this._ChartQuotationManager.UpdateHighLow(instrumentId, dataSet.Tables[1], isUpdateHigh);
                                }
                            }
                            return;
                        }
                        catch (Exception exception)
                        {
                            returnValue = -10;
                            errorMessage += " [Exception:" + exception.ToString() + "]";
                            AppDebug.LogEvent("QuotationServer.UpdateHighLow(Exception)", message + "\r\n" + string.Format("Failed:{0}", errorMessage), EventLogEntryType.Error);
                            return;
                        }
                        finally
                        {
                            if (this._ChartQuotationManager != null)
                            {
                                this._ChartQuotationManager.Start();
                            }
                            transactionScope.Dispose();
                            AppDebug.LogEvent("QuotationServer.UpdateHighLow.End", message, EventLogEntryType.Information);
                        }
                    }
                }
                finally
                {
                    this.rwLock.ReleaseLock();
                }
            }
        }

        public void RestoreHighLow(Token token, string ip, int batchProcessId, out Guid instrumentId, out string instrumentCode, out string newInput, out bool isUpdateHigh, out bool highBid, out bool lowBid, out DateTime minTimestamp, out iExchange.Common.OverridedQuotation[] overridedQs, out int returnValue, out string errorMessage)
        {
            //0: Succeed
            //-1: FailedOnDBQuotation
            //-2: ExitsNewestBatchUpdate
            //-3: NotEffectedRecord
            //-4: FailedDeleteChartData
            //-5: FailedOnDBV3
            //-10: FailedCallOnQuotationServer
            //-11: FailedCallOnStateServer
            returnValue = -10;
            errorMessage = "FailedCallOnQuotationServer";
            instrumentId = Guid.Empty;
            instrumentCode = string.Empty;
            newInput = string.Empty;
            isUpdateHigh = true;
            highBid = true;
            lowBid = true;
            minTimestamp = DateTime.MinValue;
            overridedQs = null;

            this.rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                TransactionOptions transactionOption = new TransactionOptions();
                transactionOption.IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted;
                transactionOption.Timeout = new TimeSpan(0, 3, 0);
                string message = string.Format("Token={0},IP={1},BatchProcessId={2}", token.ToString(), ip, batchProcessId);
                AppDebug.LogEvent("QuotationServer.RestoreHighLow.Begin", message, EventLogEntryType.Information);
                this.Flush();
                using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.Required, transactionOption))
                {
                    try
                    {
                        SqlConnection sqlConnection = new SqlConnection(this.connectionString);
                        SqlCommand sqlCommand = sqlConnection.CreateCommand();
                        sqlCommand.CommandTimeout = 0;
                        sqlCommand.CommandType = CommandType.StoredProcedure;
                        sqlCommand.CommandText = "P_RestoreHighLow";

                        SqlParameter parameter = sqlCommand.Parameters.Add("@dealerID", SqlDbType.UniqueIdentifier);
                        parameter.Value = token.UserID;

                        parameter = sqlCommand.Parameters.Add("@batchProcessId", SqlDbType.Int);
                        parameter.Value = batchProcessId;

                        parameter = sqlCommand.Parameters.Add("@ip", SqlDbType.NVarChar, 15);
                        parameter.Value = ip;

                        parameter = sqlCommand.Parameters.Add("@instrumentId", SqlDbType.UniqueIdentifier);
                        parameter.Direction = ParameterDirection.Output;

                        parameter = sqlCommand.Parameters.Add("@instrumentCode", SqlDbType.NVarChar, 20);
                        parameter.Direction = ParameterDirection.Output;

                        parameter = sqlCommand.Parameters.Add("@newInput", SqlDbType.VarChar, 10);
                        parameter.Direction = ParameterDirection.Output;

                        parameter = sqlCommand.Parameters.Add("@isUpdateHigh", SqlDbType.Bit);
                        parameter.Direction = ParameterDirection.Output;

                        parameter = sqlCommand.Parameters.Add("@highBid", SqlDbType.Bit);
                        parameter.Direction = ParameterDirection.Output;

                        parameter = sqlCommand.Parameters.Add("@lowBid", SqlDbType.Bit);
                        parameter.Direction = ParameterDirection.Output;

                        parameter = sqlCommand.Parameters.Add("@minTimestamp", SqlDbType.DateTime);
                        parameter.Direction = ParameterDirection.Output;

                        parameter = sqlCommand.Parameters.Add("@returnValue", SqlDbType.Int);
                        parameter.Direction = ParameterDirection.ReturnValue;

                        //sqlConnection.Open();
                        //sqlCommand.ExecuteNonQuery();

                        SqlDataAdapter dataAdapter = new SqlDataAdapter();
                        dataAdapter.SelectCommand = sqlCommand;
                        DataSet dataSet = new DataSet();
                        
                        dataAdapter.Fill(dataSet);

                        returnValue = (int)sqlCommand.Parameters["@returnValue"].Value;
                        if (returnValue != 0)
                        {
                            if (returnValue == -1)
                            {
                                errorMessage = "FailedOnDBQuotation";
                            }
                            else if (returnValue == -2)
                            {
                                errorMessage = "ExitsNewestBatchUpdate";
                            }
                            else if (returnValue == -3)
                            {
                                errorMessage = "NotEffectedRecord";
                            }
                            else if (returnValue == -4)
                            {
                                errorMessage = "FailedDeleteChartData";
                            }
                            else if (returnValue == -5)
                            {
                                errorMessage = "FailedOnDBV3";
                            }
                            else
                            {
                                errorMessage = "DB_ReturnValue:" + returnValue.ToString();
                            }

                            AppDebug.LogEvent("QuotationServer.RestoreHighLow.EXEC P_RestoreHighLow(Failed)", message + "\r\n" + string.Format("Return {0}", returnValue), EventLogEntryType.Warning);
                        }
                        else
                        {
                            instrumentId = (Guid)sqlCommand.Parameters["@instrumentId"].Value;
                            instrumentCode = (string)sqlCommand.Parameters["@instrumentCode"].Value;
                            newInput = (string)sqlCommand.Parameters["@newInput"].Value;
                            isUpdateHigh = (bool)sqlCommand.Parameters["@isUpdateHigh"].Value;
                            batchProcessId = (int)sqlCommand.Parameters["@batchProcessId"].Value;
                            highBid = (bool)sqlCommand.Parameters["@highBid"].Value;
                            lowBid = (bool)sqlCommand.Parameters["@lowBid"].Value;
                            minTimestamp = (DateTime)sqlCommand.Parameters["@minTimestamp"].Value;

                            this.UpdateHighLows(token.UserID, instrumentId, isUpdateHigh, dataSet, out overridedQs);

                            transactionScope.Complete();
                            errorMessage = "Succeed";

                            AppDebug.LogEvent("QuotationServer.RestoreHighLow.EXEC P_RestoreHighLow(Succeed)", message + "\r\n" + string.Format("Return {0}", returnValue), EventLogEntryType.Information);
                        }
                        return;
                    }
                    catch (Exception exception)
                    {
                        returnValue = -10;
                        errorMessage += " [Exception:" + exception.ToString() + "]";
                        AppDebug.LogEvent("QuotationServer.RestoreHighLow(Exception)", message + "\r\n" + string.Format("Failed:{0}", errorMessage), EventLogEntryType.Error);
                        return;
                    }
                    finally
                    {
                        transactionScope.Dispose();
                        AppDebug.LogEvent("QuotationServer.RestoreHighLow.End", message, EventLogEntryType.Information);
                    }
                }
            }
            finally
            {
                this.rwLock.ReleaseLock();
            }
        }
                
        //Temp
        public bool SetHistoryQuotation(Token token, DateTime tradeDay, string quotation, bool needApplyAutoAdjustPoints, out iExchange.Common.OriginQuotation[] originQs, out iExchange.Common.OverridedQuotation[] overridedQs, out bool needBroadcastQuotation)
        {
            XmlNode fixChartDatas;
            return this.FixOverridedQuotationHistory(token, quotation,needApplyAutoAdjustPoints, out originQs, out overridedQs, out needBroadcastQuotation, out fixChartDatas);
        }

        public bool FixOverridedQuotationHistory(Token token, string quotation, bool needApplyAutoAdjustPoints, out iExchange.Common.OriginQuotation[] originQs, out iExchange.Common.OverridedQuotation[] overridedQs, out bool needBroadcastQuotation, out XmlNode fixChartDatas)
        {
            originQs = null;
            overridedQs = null;
            needBroadcastQuotation = false;
            fixChartDatas = null;

            this.rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                ArrayList mergedOverridedQuotationList = new ArrayList();

                Hashtable updateCharts = new Hashtable();

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(quotation);
                XmlNode root = xmlDoc.DocumentElement;

                TransactionOptions transactionOption = new TransactionOptions();
                transactionOption.IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted;
                transactionOption.Timeout = new TimeSpan(0, 3, 0);
                using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.Required, transactionOption))
                {
                    try
                    {
                        foreach (XmlNode childNode in root.ChildNodes)
                        {
                            Guid instrumentID = new Guid(childNode.Attributes["InstrumentID"].Value);
                            string timestamp = childNode.Attributes["Timestamp"].Value;
                            string origin = childNode.Attributes["Origin"].Value;
                            string status = childNode.Attributes["Status"].Value;

                            Instrument instrument = this.instruments[instrumentID];
                            foreach (QuotePolicyDetail quotePolicy in instrument.QuotePolicyDetails)
                            {
                                Guids key = new Guids(quotePolicy.ID, instrument.ID);
                                //Calculate OverrideQuotaton
                                OverridedQuotation overridedQ = OverridedQuotation.CreateHistoryQuotation(token.UserID, instrument, quotePolicy, timestamp, origin, needApplyAutoAdjustPoints, this.highBid, this.lowBid);

                                DateTime tradeDay = this.GetTradeDay(DateTime.Parse(timestamp));
                                if (status == "Inserted" || status == "Modified")
                                {
                                    if (tradeDay == this.tradeDay.BeginTime.Date && !overridedQ.QuotePolicy.IsOriginHiLo)
                                    {
                                        OverridedQuotation overrideQ2 = this.GetMergeOverriedQuotationsWithHistory(overridedQ);
                                        if (overrideQ2 != null)
                                        {
                                            mergedOverridedQuotationList.Add(overrideQ2);
                                        }
                                    }
                                    //Save quotation
                                    if (!this.UpdateHistoryDB(overridedQ))
                                    {
                                        originQs = null;
                                        overridedQs = null;
                                        needBroadcastQuotation = false;
                                        fixChartDatas = null;
                                        return false;
                                    }
                                }
                                else if (status == "Deleted")
                                {
                                    if (tradeDay == this.tradeDay.BeginTime.Date)
                                    {
                                        //...How to do?
                                    }
                                    //How to process that effected overridedQuotation
                                    //Save quotation
                                    if (!this.RemoveHistoryDB(overridedQ))
                                    {
                                        originQs = null;
                                        overridedQs = null;
                                        needBroadcastQuotation = false;
                                        fixChartDatas = null;
                                        return false;
                                    }
                                }

                                UpdateChartData updateChart = new UpdateChartData(instrumentID, quotePolicy.ID, DateTime.Parse(timestamp));
                                if (!updateCharts.ContainsKey(updateChart.ID))
                                {
                                    updateCharts.Add(updateChart.ID, updateChart);
                                }
                            }
                        }

                        //Save Chart Data and Create Notify xml
                        if (updateCharts.Count > 0)
                        {
                            XmlDocument xmlDoc2 = new XmlDocument();
                            XmlElement fixChartNode = xmlDoc2.CreateElement("ChartDatas");
                            //xmlDoc2.AppendChild(fixChartNode);
                            foreach (UpdateChartData updateChartData in updateCharts.Values)
                            {
                                XmlNode chartNode = updateChartData.ToXmlNode(this.connectionString);
                                if (chartNode != null)
                                {
                                    fixChartNode.AppendChild(xmlDoc2.ImportNode(chartNode, true));
                                }
                            }
                            if (!fixChartNode.HasChildNodes) fixChartNode = null;
                            fixChartDatas = fixChartNode;
                        }

                        if (mergedOverridedQuotationList.Count > 0)
                        {
                            ArrayList outputQuotations = new ArrayList();
                            foreach (OverridedQuotation oq in mergedOverridedQuotationList)
                            {
                                iExchange.Common.OverridedQuotation overridedQuotation = oq.ToLightVersion();
                                if (!string.IsNullOrEmpty(overridedQuotation.High) || !string.IsNullOrEmpty(overridedQuotation.Low))
                                {
                                    overridedQuotation.Ask = null;
                                    overridedQuotation.Bid = null;
                                    outputQuotations.Add(overridedQuotation);
                                }
                            }
                            if (outputQuotations.Count > 0)
                            {
                                needBroadcastQuotation = true;
                                overridedQs = new iExchange.Common.OverridedQuotation[outputQuotations.Count];
                                outputQuotations.CopyTo(overridedQs);
                            }

                            this.UpdateCurrentDB(mergedOverridedQuotationList);
                        }

                        transactionScope.Complete();
                        return true;
                    }
                    catch (Exception exception)
                    {
                        AppDebug.LogEvent("QuotationServer", string.Format("SetHistoryQuotation failed:{0},{1}", quotation, exception), EventLogEntryType.Error);
                        return false;
                    }
                    finally
                    {
                        transactionScope.Dispose();
                    }
                }
            }
            finally
            {
                this.rwLock.ReleaseLock();
            }
        }

        public class UpdateChartData
        {
            private static readonly string updChartSqlFormat = "EXEC dbo.P_FixChart '{0}','{1}','{2:yyyy-MM-dd HH:mm:ss.fff}',{3}\n";
            private Guid _InstrumentID;
            private Guid _QuotePolicyID;
            private DateTime _Timestamp;

            public string ID
            {
                get { return string.Format("{0}{1}{2:yyyy-MM-dd HH:mm:ss.fff}", this._InstrumentID, this._QuotePolicyID, this._Timestamp); }
            }


            public UpdateChartData(Guid instrumentID, Guid quotePolicyID, DateTime timestamp)
            {
                this._InstrumentID = instrumentID;
                this._QuotePolicyID = quotePolicyID;
                this._Timestamp = timestamp;
            }

            private string GetUpdateChartSqlFormat()
            {
                return string.Format(UpdateChartData.updChartSqlFormat, this._InstrumentID, this._QuotePolicyID, this._Timestamp, 1);
            }

            private DataSet UpdateDB(string connectionString)
            {
                return iExchange.Common.DataAccess.GetData(this.GetUpdateChartSqlFormat(), connectionString, TimeSpan.FromMinutes(3));
            }

            public XmlNode ToXmlNode(string connectionString)
            {
                DataSet dataSet = this.UpdateDB(connectionString);
                if (dataSet == null || dataSet.Tables.Count <= 0) return null;

                XmlDocument xmlDoc = new XmlDocument();
                XmlElement fixChartsNode = xmlDoc.CreateElement("FixChartDatas");
                foreach (DataTable dataTable in dataSet.Tables)
                {
                    if (dataTable.Rows.Count > 0)
                    {
                        foreach (DataRow dataRow in dataTable.Rows)
                        {
                            fixChartsNode.SetAttribute("InstrumentID", XmlConvert.ToString(this._InstrumentID));
                            fixChartsNode.SetAttribute("QuotePolicyID", XmlConvert.ToString(this._QuotePolicyID));

                            //XmlNode fixChartNode = xmlDoc.ImportNode(this.ToXmlNode(dataRow), true);
                            fixChartsNode.AppendChild(xmlDoc.ImportNode(this.ToXmlNode(dataRow), true));
                        }
                    }
                }
                if (!fixChartsNode.HasChildNodes) fixChartsNode = null;
                return fixChartsNode;
            }

            private XmlNode ToXmlNode(DataRow dataRow)
            {
                XmlDocument xmlFixChart = new XmlDocument();

                XmlElement fixChartNode = xmlFixChart.CreateElement("FixChartData");
                xmlFixChart.AppendChild(fixChartNode);

                //fixChartNode.SetAttribute("InstrumentID",XmlConvert.ToString((Guid)dataRow["InstrumentID"]));
                //fixChartNode.SetAttribute("QuotePolicyID", XmlConvert.ToString((Guid)dataRow["QuotePolicyID"]));

                fixChartNode.SetAttribute("Date", XmlConvert.ToString((DateTime)dataRow["Date"], DateTimeFormat.Xml));
                fixChartNode.SetAttribute("Minutes", XmlConvert.ToString((int)dataRow["Minutes"]));
                fixChartNode.SetAttribute("Open", (string)dataRow["Open"]);
                fixChartNode.SetAttribute("Close", (string)dataRow["Close"]);
                fixChartNode.SetAttribute("High", (string)dataRow["High"]);
                fixChartNode.SetAttribute("Low", (string)dataRow["Low"]);
                if (dataRow["Volume"] == DBNull.Value)
                {
                    fixChartNode.SetAttribute("Volume", "0");
                }
                else
                {
                    fixChartNode.SetAttribute("Volume", XmlConvert.ToString((double)dataRow["Volume"]));
                }
                fixChartNode.SetAttribute("Status", (string)dataRow["Status"]);

                return fixChartNode;
            }
        }

        private bool UpdateHistoryDB(OverridedQuotation overridedQ)
        {
            int error = (int)iExchange.Common.DataAccess.ExecuteScalar(overridedQ.GetInsertHistorySql(), this.connectionString);
            return error == 0;
        }

        private bool RemoveHistoryDB(OverridedQuotation overridedQ)
        {
            int error = (int)iExchange.Common.DataAccess.ExecuteScalar(overridedQ.GetRemoveHistorySql(), this.connectionString);
            return error == 0;
        }
        
        private DateTime GetTradeDay(DateTime timestamp)
        {
            DateTime tradeDay = DateTime.MaxValue;
            string sql = string.Format("SELECT dbo.FV_GetTradeDay('{0}')", timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            tradeDay = (DateTime)iExchange.Common.DataAccess.ExecuteScalar(sql, this.connectionString);
            return tradeDay;
        }

        private OverridedQuotation GetMergeOverriedQuotationsWithHistory(OverridedQuotation overrideQ)
        {
            this.rwLock.AcquireWriterLock(Timeout.Infinite);
            try
            {
                Guids key = new Guids(overrideQ.QuotePolicy.ID, overrideQ.Instrument.ID);
                OverridedQuotation oldOverrideQuotation = null;
                if (this.overridedQuotations.TryGetValue(key, out oldOverrideQuotation))
                {
                    string sql = string.Format("dbo.OverridedQuotation_GetDailyHighLow '{0}', '{1}', '{2:yyyy-MM-dd HH:mm:ss.fff}'", overrideQ.Instrument.ID, overrideQ.QuotePolicy.ID, overrideQ.Timestamp);
                    DataSet dataSet = iExchange.Common.DataAccess.GetData(sql, this.connectionString);
                    if (dataSet.Tables[0].Rows.Count > 0)
                    {
                        Price high = new Price(dataSet.Tables[0].Rows[0]["High"].ToString(), overrideQ.Instrument.NumeratorUnit, overrideQ.Instrument.Denominator);
                        Price low = new Price(dataSet.Tables[0].Rows[0]["Low"].ToString(), overrideQ.Instrument.NumeratorUnit, overrideQ.Instrument.Denominator);
                        high = (overrideQ.High > high ? overrideQ.High : high);
                        low = (overrideQ.Low < low ? overrideQ.Low : low);
                        overrideQ.UpdateHighLow(high, low);
                    }
                    
                    oldOverrideQuotation.UpdateHighLow(overrideQ);
                    //overrideQ2.UpdateHighLowWithHistoryQuotation(overrideQ);
                }
                return oldOverrideQuotation;
            }
            finally
            {
                this.rwLock.ReleaseLock();
            }
        }

        private void UpdateCurrentDB(ArrayList overridedQuotationList)
        {
            if (overridedQuotationList != null && overridedQuotationList.Count > 0)
            {
                StringBuilder builder = new StringBuilder();
                foreach (OverridedQuotation overridedQ in overridedQuotationList)
                {
                    builder.Append(overridedQ.GetUpdateSql(true));
                }
                iExchange.Common.DataAccess.UpdateDB(builder.ToString(), this.connectionString);
            }
        }

        private void UpdateHighLows(Guid dealerID, Guid instrumentId, bool isUpdateHigh, DataSet dataSet, out iExchange.Common.OverridedQuotation[] overridedQs)
        {
            overridedQs = null;
            if (dataSet == null || dataSet.Tables.Count <= 0) return;
            Instrument instrument = this.instruments[instrumentId];
            ArrayList outputQuotations = new ArrayList();
            DateTime timestamp = DateTime.Now;
            //this.rwLock.AcquireWriterLock(Timeout.Infinite);
            //try
            //{
                foreach (DataRow dataRow in dataSet.Tables[0].Rows)
                {
                    Guid quotePolicyId = (Guid)dataRow["QuotePolicyId"];
                    string high = null;
                    string low = null;
                    Price highPrice = null;
                    Price lowPrice = null;
                    if (dataRow["High"] != DBNull.Value)
                    {
                        high = (string)dataRow["High"];
                        highPrice = Price.CreateInstance(high, instrument.NumeratorUnit, instrument.Denominator);
                    }
                    if (dataRow["Low"] != DBNull.Value)
                    {
                        low = (string)dataRow["Low"];
                        lowPrice = Price.CreateInstance(low, instrument.NumeratorUnit, instrument.Denominator);
                    }
                    if (highPrice == null && lowPrice == null) continue;
                    foreach (OverridedQuotation overrideQ in this.overridedQuotations.Values)
                    {
                        if (overrideQ.Instrument.ID.Equals(instrumentId) && overrideQ.QuotePolicy.ID.Equals(quotePolicyId))
                        {
                            overrideQ.UpdateHighLow(isUpdateHigh, highPrice, lowPrice);

                            iExchange.Common.OverridedQuotation overridedQuotation = overrideQ.ToLightVersion();
                            if (!string.IsNullOrEmpty(overridedQuotation.High) || !string.IsNullOrEmpty(overridedQuotation.Low))
                            {
                                overridedQuotation.Ask = null;
                                overridedQuotation.Bid = null;
                                outputQuotations.Add(overridedQuotation);
                            }
                            break;
                        }
                    }
                }
                if (outputQuotations.Count > 0)
                {
                    overridedQs = new iExchange.Common.OverridedQuotation[outputQuotations.Count];
                    outputQuotations.CopyTo(overridedQs);
                }
            //}
            //finally
            //{
            //    this.rwLock.ReleaseLock();
            //}
        }

        #region Maybe Unused
        ////and by Korn 2008-9-4
        //private ArrayList GetMergeOverriedQuotationsWithHistory(ArrayList overridedQuotationList)
        //{
        //    ArrayList mergedOverridedQuotationList = new ArrayList();
        //    this.rwLock.AcquireWriterLock(Timeout.Infinite);
        //    try
        //    {
        //        for (int i = 0; i < overridedQuotationList.Count; i++)
        //        {
        //            OverridedQuotation overrideQ = (OverridedQuotation)overridedQuotationList[i];
        //            QuotePolicyDetail quotePolicy = overrideQ.QuotePolicy;
        //            Guid quotePolicyID = quotePolicy.ID;
        //            Guids key = new Guids(overrideQ.QuotePolicy.ID, overrideQ.Instrument.ID);
        //            OverridedQuotation overrideQ2 = (OverridedQuotation)this.overridedQuotations[key];
        //            if (overrideQ2 != null)
        //            {
        //                overrideQ2.UpdateHighLowWithHistoryQuotation(overrideQ);
        //                mergedOverridedQuotationList.Add(overrideQ2);
        //            }
        //        }
        //        return mergedOverridedQuotationList;
        //    }
        //    finally
        //    {
        //        this.rwLock.ReleaseLock();
        //    }
        //}

        //private void UpdateHistoryDB(ArrayList overridedQuotationList)
        //{
        //    if (overridedQuotationList != null && overridedQuotationList.Count > 0)
        //    {
        //        StringBuilder builder = new StringBuilder();

        //        foreach (OverridedQuotation overridedQ in overridedQuotationList)
        //        {
        //            builder.Append(overridedQ.GetInsertHistorySql());
        //        }
        //        DataAccess.UpdateDB(builder.ToString(), this.connectionString);
        //    }
        //}

        //private void UpdateChartDB(ArrayList overridedQuotationList)
        //{
        //    if (overridedQuotationList != null && overridedQuotationList.Count > 0)
        //    {
        //        StringBuilder builder = new StringBuilder();
        //        foreach (OverridedQuotation overridedQ in overridedQuotationList)
        //        {
        //            builder.Append(overridedQ.GetUpdateChartSqlFormat(false));
        //        }
        //        DataAccess.UpdateDB(builder.ToString(), this.connectionString);
        //    }
        //}

        //maybe Unused
        //Add by Korn 2008-10-14,When QuotationRectification updata overridedQuotationHistory,should update QuotationServer's high,low in iExchange.
        public bool UpdateOverridedQuotationHighLow(Token token, Guid instrumentID, string quotation, out iExchange.Common.OverridedQuotation[] overridedQs)
        {
            overridedQs = null;
            if (token.AppType == AppType.DealingConsole)
            {
                try
                {
                    Instrument instrument = this.instruments[instrumentID];
                    string[] quotationRows = quotation.Split(QuotationDelimiter.Row);
                    ArrayList overridedQuotationList = new ArrayList();
                    for (int i = 0; i < quotationRows.Length; i++)
                    {
                        string[] qs = quotationRows[i].Split(QuotationDelimiter.Col);
                        Guid quotePolicyID = new Guid(qs[0]);
                        Price high = Price.CreateInstance(qs[1], instrument.NumeratorUnit, instrument.Denominator);
                        Price low = Price.CreateInstance(qs[2], instrument.NumeratorUnit, instrument.Denominator);
                        this.rwLock.AcquireWriterLock(Timeout.Infinite);
                        try
                        {
                            Guids key = new Guids(quotePolicyID, instrumentID);
                            OverridedQuotation overridedQ = null; 
                            if (this.overridedQuotations.TryGetValue(key, out overridedQ))
                            {
                                overridedQ.UpdateHighLow(high, low);
                                overridedQuotationList.Add(overridedQ);
                            }
                        }
                        finally
                        {
                            this.rwLock.ReleaseWriterLock();
                        }
                    }
                    this.UpdateCurrentDB(overridedQuotationList);

                    if (instrument.IsTrading)
                    {
                        overridedQs = new iExchange.Common.OverridedQuotation[overridedQuotationList.Count];
                        int i = 0;
                        foreach (OverridedQuotation oq in overridedQuotationList)
                        {
                            overridedQs[i++] = oq.ToLightVersion();
                        }
                    }
                    return true;
                }
                catch (Exception exception)
                {
                    AppDebug.LogEvent("QuotationServer", string.Format("UpdateOverridedQuotationHighLow failed:{0}", exception), EventLogEntryType.Error);
                    return false;
                }
            }
            return true;
        }

        ////Add by Korn 2008-9-4
        ////create overridedQutation and update database,if quotation is in current tradeday,should update high,low in memory.
        //public bool SetHistoryQuotation(Token token, DateTime tradeDay, string quotation, out Common.OriginQuotation[] originQs, out Common.OverridedQuotation[] overridedQs, out bool needBroadcast)
        //{
        //    originQs = null;
        //    overridedQs = null;
        //    needBroadcast = false;

        //    ArrayList overridedQuotationList = new ArrayList();
        //    ArrayList mergedOverridedQuotationList = null;

        //    XmlDocument xmlDoc = new XmlDocument();
        //    xmlDoc.LoadXml(quotation);
        //    XmlNode root = xmlDoc.DocumentElement;
        //    try
        //    {
        //        foreach (XmlNode childNode in root.ChildNodes)
        //        {
        //            Guid instrumentID = new Guid(childNode.Attributes["InstrumentID"].Value);
        //            string timestamp = childNode.Attributes["Timestamp"].Value;
        //            string origin = childNode.Attributes["Origin"].Value;

        //            Instrument instrument = (Instrument)this.instruments[instrumentID];

        //            foreach (QuotePolicyDetail quotePolicy in instrument.QuotePolicyDetails)
        //            {
        //                Guids key = new Guids(quotePolicy.ID, instrument.ID);
        //                //Calculate OverrideQuotaton
        //                OverridedQuotation overridedQ = OverridedQuotation.CreateHistoryQuotation(token.UserID, instrument, quotePolicy, timestamp, origin);
        //                overridedQuotationList.Add(overridedQ);
        //            }
        //        }
        //        if (tradeDay == this.tradeDay.BeginTime.Date)
        //        {
        //            needBroadcast = true;
        //            mergedOverridedQuotationList = this.GetMergeOverriedQuotationsWithHistory(overridedQuotationList);
        //            //
        //            overridedQs = new Common.OverridedQuotation[mergedOverridedQuotationList.Count];
        //            int i = 0;
        //            foreach (OverridedQuotation oq in mergedOverridedQuotationList)
        //            {
        //                overridedQs[i++] = oq.ToLightVersion();
        //            }
        //        }
        //        //Save quotation
        //        if (overridedQuotationList.Count > 0)
        //        {
        //            UpdateHistoryDB(overridedQuotationList);
        //            UpdateChartDB(overridedQuotationList);
        //        }
        //        if (overridedQs != null && overridedQs.Length > 0)
        //        {
        //            UpdateCurrentDB(mergedOverridedQuotationList);
        //        }

        //        return true;
        //    }
        //    catch (Exception exception)
        //    {
        //        AppDebug.LogEvent("QuotationServer", string.Format("SetHistoryQuotation failed:{0},{1}", quotation, exception), EventLogEntryType.Error);
        //        return false;
        //    }
        //}

        //end by Korn 2008-9-6

        #endregion

        #endregion


        internal bool SetBestLimit(Token token, BestLimit[] bestLimits, out DateTime timeStamp)
        {
            timeStamp = DateTime.Now;
            foreach (BestLimit bestLimit in bestLimits)
            {
                bestLimit.Timestamp = timeStamp;
            }
            return iExchange.Common.DataAccess.UpdateDB(bestLimits, this.connectionString);
        }

        private DateTime _LastStampToGetPricesOfHiLo = DateTime.MinValue;
        internal string[] GetPricesOfHiLo(string quotePolicyCode)
        {
            DateTime now = DateTime.Now;
            if ((now - this._LastStampToGetPricesOfHiLo) < this._MinIntervalToGetPicesOfHiLo) throw new InvalidOperationException("The interval between calling GetPricesOfHiLo must large than " + this._MinIntervalToGetPicesOfHiLo);
            this._LastStampToGetPricesOfHiLo = now;
            if(string.IsNullOrEmpty(quotePolicyCode)) throw new ArgumentNullException();

            quotePolicyCode = quotePolicyCode.ToLower().Trim();
            Guid quotePolicyId = Guid.Empty;
            if(!this.quotePolicies.TryGetValue(quotePolicyCode, out quotePolicyId))
            {
                throw new ArgumentException(quotePolicyCode + " is not a valid quote policy");
            }
            else
            {
                

                InitCommand initCommand;
                initCommand = new InitCommand();
                initCommand.Command = new SqlCommand("dbo.P_GetFirstHighLowPrice");
                initCommand.Command.CommandType = System.Data.CommandType.StoredProcedure;
                initCommand.Command.Parameters.Add(new SqlParameter("@quotePolicyId", quotePolicyId));

                //data set
                SqlCommand command = initCommand.Command;
                command.Connection = new SqlConnection(this.connectionString);
                command.CommandTimeout = 0;

                SqlDataAdapter dataAdapter = new SqlDataAdapter();
                dataAdapter.SelectCommand = command;
                DataSet dataSet = new DataSet();
                dataAdapter.Fill(dataSet);

                DataRowCollection rows = dataSet.Tables[0].Rows;
                string[] prices = new string[rows.Count];
                int index = 0;
                foreach (DataRow row in rows)
                {
                    prices[index++] = string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}", row["Code"],
                        row["Bid"] == DBNull.Value ? string.Empty : row["Bid"],
                        row["Ask"] == DBNull.Value ? string.Empty : row["Ask"],
                        row["HighBid"] == DBNull.Value ? string.Empty : row["HighBid"],
                        row["HighAsk"] == DBNull.Value ? string.Empty : row["HighAsk"],
                        row["LowBid"] == DBNull.Value ? string.Empty : row["LowBid"],
                        row["LowAsk"] == DBNull.Value ? string.Empty : row["LowAsk"]);
                }
                return prices;
            }
        }
    }
}
