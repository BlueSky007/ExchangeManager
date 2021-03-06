﻿using Manager.Common;
using Manager.Common.LogEntities;
using Manager.Common.QuotationEntities;
using Manager.Common.ReportEntities;
using Manager.Common.Settings;
using ManagerConsole.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Xml;
using TaskScheduler = Manager.Common.Settings.TaskScheduler;
using AccountGroupGNP = iExchange.Common.Manager.AccountGroupGNP;
using AccountType = iExchange.Common.AccountType;
using OpenInterestSummary = iExchange.Common.Manager.OpenInterestSummary;
using TransactionError = iExchange.Common.TransactionError;
using OrderQueryEntity = Manager.Common.ReportEntities.OrderQueryEntity;
using SoundSetting = Manager.Common.Settings.SoundSetting;
using BlotterSelection = Manager.Common.ReportEntities.BlotterSelection;
using InstrumentForFloatingPLCalc = Manager.Common.ReportEntities.InstrumentForFloatingPLCalc;
using System.Collections.ObjectModel;
using iExchange.Common.Manager;
using iExchange.Common;

namespace ManagerConsole.Model
{
    [ServiceKnownType("GetKnownTypes", typeof(KnownTypes))]
    [ServiceContract(CallbackContract = typeof(IClientProxy), SessionMode = SessionMode.Required)]
    public interface IClientService
    {
        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginLogin(string userName, string password, Language language, AsyncCallback callback, object asyncState);
        LoginResult EndLogin(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginRecoverConnection(string sessionId, AsyncCallback callback, object asyncState);
        bool EndRecoverConnection(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginGetInitializeData(AsyncCallback callback, object asyncState);
        InitializeData EndGetInitializeData(IAsyncResult result);


        [OperationContract(IsInitiating = false)]
        FunctionTree GetFunctionTree();

        [OperationContract(IsOneWay = true)]
        void Logout();
        //[OperationContract(AsyncPattern = true)]
        //IAsyncResult BeginLogout(AsyncCallback callback, object asyncState);
        //void EndLogout(IAsyncResult result);

        [OperationContract(IsInitiating = false)]
        void SaveLayout(string layout, string content, string layoutName);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginLoadLayout(string layoutName, AsyncCallback callback, object asyncState);
        List<string> EndLoadLayout(IAsyncResult result);

        #region UserManager
        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginGetAccessPermissions(AsyncCallback callback, object asyncState);
        Dictionary<string, List<FuncPermissionStatus>> EndGetAccessPermissions(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginGetUserData(AsyncCallback callback, object asyncState);
        List<UserData> EndGetUserData(IAsyncResult result);

        //[OperationContract(IsInitiating = false)]
        //List<RoleData> GetRoles();


        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginGetRoles(AsyncCallback callback, object asyncState);
        List<RoleData> EndGetRoles(IAsyncResult result);

        //[OperationContract(IsInitiating = false)]
        //List<RoleFunctonPermission> GetAllFunctionPermission();

        //[OperationContract(IsInitiating = false)]
        //List<RoleDataPermission> GetAllDataPermission();

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginGetAllFunctionPermission(AsyncCallback callback, object asyncState);
        List<RoleFunctonPermission> EndGetAllFunctionPermission(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginGetAllDataPermission(AsyncCallback callback, object asyncState);
        List<RoleDataPermission> EndGetAllDataPermission(IAsyncResult result);


        [OperationContract(IsInitiating = false)]
        bool ChangePassword(string currentPassword, string newPassword);


        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginUpdateUsers(UserData user, string password, bool isNewUser, AsyncCallback callback, object asyncState);
        bool EndUpdateUsers(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginDeleteUser(Guid userId, AsyncCallback callback, object asyncState);
        bool EndDeleteUser(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginUpdateRole(RoleData role, bool isNewRole, AsyncCallback callback, object asyncState);
        bool EndUpdateRole(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginDeleteRole(int roleId, AsyncCallback callback, object asyncState);
        bool EndDeleteRole(IAsyncResult result);

        #endregion

        #region DealingConsole
        [OperationContract(IsInitiating = false)]
        void AbandonQuote(List<Answer> abandonQuotePrices);

        [OperationContract(IsInitiating = false)]
        void SendQuotePrice(List<Answer> sendQuotePrices);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginAcceptPlace(Guid transactionId, LogOrder logEntity, AsyncCallback callback, object asyncState);
        TransactionError EndAcceptPlace(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginCancelPlace(Guid transactionId, CancelReason cancelReason, AsyncCallback callback, object asyncState);
        TransactionError EndCancelPlace(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginExecute(Guid transactionId, string buyPrice, string sellPrice, decimal lot, Guid orderId, LogOrder logEntity, AsyncCallback callback, object asyncState);
        TransactionResult EndExecute(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginCancel(Guid transactionId,CancelReason cancelReason, LogOrder logEntity, AsyncCallback callback, object asyncState);
        TransactionError EndCancel(IAsyncResult result);

        [OperationContract(IsInitiating = false)]
        void ResetHit(string exchangeCode, Guid[] orderIds);

        [OperationContract(IsInitiating = false)]
        AccountInformation GetAcountInfo(string exchangeCode, Guid transactionId);

        #endregion

        #region Setting Manager
        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginCopyFromSetting(Guid copyUserId, AsyncCallback callback, object asyncState);
        List<SoundSetting> EndCopyFromSetting(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginLoadSettingsParameters(AsyncCallback callback, object asyncState);
        SettingsParameter EndLoadSettingsParameters(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginLoadParameterDefine(AsyncCallback callback, object asyncState);
        List<ParameterDefine> EndLoadParameterDefine(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginCreateTaskScheduler(TaskScheduler taskScheduler, AsyncCallback callback, object asyncState);
        bool EndCreateTaskScheduler(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginEditorTaskScheduler(TaskScheduler taskScheduler, AsyncCallback callback, object asyncState);
        bool EndEditorTaskScheduler(IAsyncResult result);

        [OperationContract(IsInitiating = false)]
        void EnableTaskScheduler(TaskScheduler taskScheduler);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginStartRunTaskScheduler(TaskScheduler taskScheduler, AsyncCallback callback, object asyncState);
        void EndStartRunTaskScheduler(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginDeleteTaskScheduler(TaskScheduler taskScheduler, AsyncCallback callback, object asyncState);
        bool EndDeleteTaskScheduler(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginGetTaskSchedulersData(AsyncCallback callback, object asyncState);
        List<TaskScheduler> EndGetTaskSchedulersData(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginUpdateManagerSettings(Guid settingId, SettingParameterType type, Dictionary<string, object> fieldAndValues, AsyncCallback callback, object asyncState);
        bool EndUpdateManagerSettings(IAsyncResult result);
        #endregion

        #region Report
        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginGetOrderByInstrument(string exchangeCode,Guid instrumentId, Guid accountGroupId, OrderType orderType,
            bool isExecute, DateTime fromDate, DateTime toDate, AsyncCallback callback, object asyncState);
        List<OrderQueryEntity> EndGetOrderByInstrument(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginGetGroupNetPosition(string exchangeCode,bool showActualQuantity, string[] blotterCodeSelecteds,AsyncCallback callback, object asyncState);
        List<AccountGroupGNP> EndGetGroupNetPosition(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginGetOpenInterestInstrumentSummary(string exchangeCode,bool isGroupByOriginCode, string[] blotterCodeSelecteds, AsyncCallback callback, object asyncState);
        List<OpenInterestSummary> EndGetOpenInterestInstrumentSummary(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginGetOpenInterestAccountSummary(string exchangeCode,Guid instrumentId, string[] blotterCodeSelecteds, AsyncCallback callback, object asyncState);
        List<OpenInterestSummary> EndGetOpenInterestAccountSummary(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginGetOpenInterestOrderSummary(string exchangeCode,Guid instrumentId, Guid accountId, AccountType accountType, string[] blotterCodeSelecteds, AsyncCallback callback, object asyncState);
        List<OpenInterestSummary> EndGetOpenInterestOrderSummary(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginGetBlotterList(string exchangeCode, AsyncCallback callback, object asyncState);
        List<BlotterSelection> EndGetBlotterList(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginGetAccountReportData(string exchangeCode, string selectedPrice, Guid accountId, AsyncCallback callback, object asyncState);
        AccountStatusQueryResult EndGetAccountReportData(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginGetInstrumentForFloatingPLCalc(string exchangeCode, AsyncCallback callback, object asyncState);
        List<InstrumentForFloatingPLCalc> EndGetInstrumentForFloatingPLCalc(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginUpdateInstrumentForFloatingPLCalc(string exchangeCode, Guid instrumentId, string bid, int spreadPoint, AsyncCallback callback, object asyncState);
        bool EndUpdateInstrumentForFloatingPLCalc(IAsyncResult result);
        
        #endregion

        #region Log Audit
        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginGetQuoteLogData(DateTime fromDate, DateTime toDate, LogType logType, AsyncCallback callback, object asyncStatus);
        List<LogQuote> EndGetQuoteLogData(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginGetLogOrderData(DateTime fromDate, DateTime toDate, LogType logType, AsyncCallback callback, object asyncStatus);
        List<LogOrder> EndGetLogOrderData(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginGetLogSettingData(DateTime fromDate, DateTime toDate, LogType logType, AsyncCallback callback, object asyncStatus);
        List<LogSetting> EndGetLogSettingData(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginGetLogPriceData(DateTime fromDate, DateTime toDate, LogType logType, AsyncCallback callback, object asyncStatus);
        List<LogPrice> EndGetLogPriceData(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginGetLogSourceChangeData(DateTime fromDate, DateTime toDate, LogType logType, AsyncCallback callback, object asyncStatus);
        List<LogSourceChange> EndGetLogSourceChangeData(IAsyncResult result);

        #endregion


        #region Quotation
        [OperationContract(IsInitiating = false)]
        ConfigMetadata GetConfigMetadata();

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginAddMetadataObject(IMetadataObject metadataObject, AsyncCallback callback, object asyncState);
        int EndAddMetadataObject(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginAddInstrument(InstrumentData instrumentData, AsyncCallback callback, object asyncState);
        int EndAddInstrument(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginUpdateMetadataObject(MetadataType type, int objectId, Dictionary<string, object> fieldAndValues, AsyncCallback callback, object asyncState);
        bool EndUpdateMetadataObject(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginUpdateMetadataObjects(UpdateData[] updateDatas, AsyncCallback callback, object asyncState);
        bool EndUpdateMetadataObjects(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginUpdateMetadataObjectField(MetadataType type, int objectId, string field, object value, AsyncCallback callback, object asyncState);
        bool EndUpdateMetadataObjectField(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginDeleteMetadataObject(MetadataType type, int objectId, AsyncCallback callback, object asyncState);
        bool EndDeleteMetadataObject(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginSendQuotation(int instrumentSourceRelationId, double ask, double bid, AsyncCallback callback, object asyncState);
        void EndSendQuotation(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginSwitchDefaultSource(SwitchRelationBooleanPropertyMessage message, AsyncCallback callback, object asyncState);
        void EndSwitchDefaultSource(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginGetQuotePolicyRelation(AsyncCallback callback, object asyncState);
        List<QuotePolicyRelation> EndGetQuotePolicyRelation(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginAddNewRelation(Guid id, string code, List<int> instruments, AsyncCallback callback, object asyncState);
        bool EndAddNewRelation(IAsyncResult result);
        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginConfirmAbnormalQuotation(int instrumentId, int confirmId, bool accepted, AsyncCallback callback, object asyncState);
        void EndConfirmAbnormalQuotation(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginSuspendResume(int[] instrumentIds, bool resume, AsyncCallback callback, object asyncState);
        void EndSuspendResume(IAsyncResult result);
        #endregion

        [OperationContract(AsyncPattern = false)]
        void Updatetest();

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginUpdateQuotationPolicy(InstrumentQuotationSet set, AsyncCallback callback, object asyncState);
        void EndUpdateQuotationPolicy(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginUpdateInstrument(InstrumentQuotationSet set, AsyncCallback callback, object asyncState);
        void EndUpdateInstrument(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginSetQuotationPolicyDetail(Guid relationId, QuotePolicyDetailsSetAction action, int changeValue, AsyncCallback callback, object asyncState);
        void EndSetQuotationPolicyDetail(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginExchangeSuspendResume(Dictionary<string, List<Guid>> instruments, bool resume, AsyncCallback callback, object asyncState);
        bool EndExchangeSuspendResume(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginGetOriginQuotationForModifyAskBidHistory(string exchangeCode, Guid instrumentID, DateTime beginDateTime, string origin, AsyncCallback callback, object asyncState);
        List<HistoryQuotationData> EndGetOriginQuotationForModifyAskBidHistory(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginUpdateHighLow(string exchangeCode, Guid instrumentId, bool isOriginHiLo, string newInput, bool isUpdateHigh, AsyncCallback callback, object asyncState);
        UpdateHighLowBatchProcessInfo EndUpdateHighLow(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginFixOverridedQuotationHistory(Dictionary<string, string> quotations, bool needApplyAutoAdjustPoints, AsyncCallback callback, object asyncState);
        bool EndFixOverridedQuotationHistory(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginRestoreHighLow(string exchangeCode, int batchProcessId, AsyncCallback callback, object asyncState);
        bool EndRestoreHighLow(IAsyncResult result);

        [OperationContract(AsyncPattern = false)]
        List<Manager.Common.ExchangeEntities.ChartQuotation> GetChartQuotation(string exchangeCode, Guid quotePolicyId, Guid instrumentId, string frequency, DateTime fromTime, DateTime toTime);

        [OperationContract(AsyncPattern = false)]
        Manager.Common.ExchangeEntities.ChartQuotation GetLastQuotationsForTrendSheet(string exchangeCode, Guid quotePolicyId, Guid instrumentId, string frequency, DateTime fromTime, decimal open);
    }

    [ServiceContract]
    public interface IClientProxy
    {
        [OperationContract(IsOneWay = true)]
        void SendMessage(Message message);
    }
}
