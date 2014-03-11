using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using Manager.Common.QuotationEntities;
using Manager.Common.Settings;
using TransactionError = iExchange.Common.TransactionError;
using iExchange.Common;
using iExchange.Common.Manager;
using Manager.Common.ExchangeEntities;


namespace Manager.Common
{
    public interface IFilterable
    {
        Guid? AccountId { get; }
        Guid? InstrumentId { get; }
    }

    [KnownType(typeof(QuoteMessage)),
    KnownType(typeof(PlaceMessage)),
    KnownType(typeof(ExecuteMessage)),
    KnownType(typeof(HitMessage)),
    KnownType(typeof(UpdateMessage)),
    KnownType(typeof(DeleteMessage)),
    KnownType(typeof(PrimitiveQuotationMessage)),
    KnownType(typeof(AbnormalQuotationMessage)),
    KnownType(typeof(UpdateMetadataMessage)),
    KnownType(typeof(AddMetadataObjectMessage)),
    KnownType(typeof(AddMetadataObjectsMessage)),
    KnownType(typeof(DeleteMetadataObjectMessage)),
    KnownType(typeof(SwitchRelationBooleanPropertyMessage)),
    KnownType(typeof(QuotationsMessage)),
    KnownType(typeof(SourceConnectionStatusMessage)),
    KnownType(typeof(ExchangeConnectionStatusMessage)),
    KnownType(typeof(OverridedQuotationMessage)),
    KnownType(typeof(UpdateInstrumentQuotationMessage)),
    KnownType(typeof(AccessPermissionUpdateMessage)),
    KnownType(typeof(UpdateRoleMessage)),
    KnownType(typeof(NotifyLogoutMessage)),
    KnownType(typeof(TaskSchedulerRunMessage))]
    public class Message
    {
    }

    public class ExchangeMessage : Message
    {
        public string ExchangeCode { get; set; }
    }
    public class QuotationsMessage : Message
    {
        public List<GeneralQuotation> Quotations;
    }
    public class SwitchRelationBooleanPropertyMessage : Message
    {
        public int InstrumentId;
        public string PropertyName;
        public int OldRelationId;
        public int NewRelationId;
    }

    public class UpdateMetadataMessage : Message
    {
        public UpdateData[] UpdateDatas;
    }

    public class AddMetadataObjectMessage : Message
    {
        public IMetadataObject MetadataObject;
    }

    public class AddMetadataObjectsMessage : Message
    {
        public IMetadataObject[] MetadataObjects;
    }

    public class DeleteMetadataObjectMessage : Message
    {
        public int ObjectId;
        public MetadataType MetadataType;
    }

    public class QuoteMessage : ExchangeMessage, IFilterable
    {
        public Guid CustomerID { get; set; }
        public Guid InstrumentID { get; set; }
        public double QuoteLot { get; set; }
        public int BSStatus { get; set; }	//0:Sell 1:Buy 2:Mis
        public DateTime TimeStamp { get; set; }

        public QuoteMessage()
        { 
        }
        public QuoteMessage(string ExchangeCode,Guid customerId, Guid instrumentId, double quoteLot,int bSStatus)
        {
            this.ExchangeCode = ExchangeCode;
            this.CustomerID = customerId;
            this.InstrumentID = instrumentId;
            this.QuoteLot = quoteLot;
            this.BSStatus = bSStatus;
            this.TimeStamp = DateTime.Now;
        }

        #region IFilter
        public Guid? AccountId
        {
            get { return null; }
        }

        public Guid? InstrumentId
        {
            get { return this.InstrumentID; }
        }
        #endregion
    }

    public class PrimitiveQuotationMessage : Message
    {
        public PrimitiveQuotation Quotation;
    }

    public class AbnormalQuotationMessage : Message
    {
        public int ConfirmId;
        public int InstrumentId;
        public string InstrumentCode;
        public string NewPrice;
        public double OldPrice;
        public OutOfRangeType OutOfRangeType;
        public int DiffPoints;
        public DateTime Timestamp;
        public int WaitSeconds;
    }

    public class OverridedQuotationMessage : Message
    {
        public string ExchangeCode;
        public List<iExchange.Common.OverridedQuotation> OverridedQs;
    }

    public class SourceConnectionStatusMessage : Message
    {
        public string SouceName;
        public ConnectionState ConnectionState;
    }

    public class ExchangeConnectionStatusMessage : Message
    {
        public string ExchangeCode;
        public ConnectionState ConnectionState;
    }

    public class AccessPermissionUpdateMessage : Message
    {
        public User user;
        public Dictionary<string, List<FuncPermissionStatus>> NewPermission;

        public AccessPermissionUpdateMessage() { }

        #region IFilter
        public Guid? AccountId
        {
            get { return null; }
        }

        public Guid? InstrumentId
        {
            get { return null; }
        }
        #endregion
    }

    public class UpdateMessage : ExchangeMessage
    {
        public UpdateMessage() { }

        public SettingSet AddSettingSets;
        public ExchangeUpdateData[] ExchangeUpdateDatas;
        public SettingSet DeletedSettings;
    }

    public class PlaceMessage : ExchangeMessage, IFilterable
    {
        public Guid AccountID { get; set; }
        public Guid InstrumentID { get; set; }
        public Transaction[] Transactions { get; set; }
        public Order[] Orders { get; set; }
        public OrderRelation[] OrderRelations { get; set; }

        public PlaceMessage() { }
        public PlaceMessage(string exchangeCode,Transaction[] transactions, Order[] orders, OrderRelation[] orderRelations)
        {
            this.ExchangeCode = exchangeCode;
            this.Transactions = transactions;
            this.Orders = orders;
            this.OrderRelations = orderRelations;
        }

        #region IFilter
        public Guid? AccountId
        {
            get { return this.AccountID; }
        }

        public Guid? InstrumentId
        {
            get { return this.InstrumentID; }
        }
        #endregion
    }

    public class HitMessage : ExchangeMessage, IFilterable
    {
        public Order[] Orders { get; set; }

        public HitMessage()
        { 
        }

        public HitMessage(string exchangeCode,Order[] orders)
        {
            this.ExchangeCode = exchangeCode;
            this.Orders = orders;
        }

        #region IFilter
        public Guid? AccountId
        {
            get { return null; }
        }

        public Guid? InstrumentId
        {
            get { return null; }
        }
        #endregion
    }

    public class DeleteMessage : ExchangeMessage, IFilterable
    {
        public Guid DeletedOrderId { get; set; }
        public Guid InstrumentID { get; set; }
        public Guid AccountID { get; set; }
        public Account Account { get; set; }
        public Transaction[] Transactions { get; set; }
        public Order[] Orders { get; set; }
        public OrderRelation[] OrderRelations { get; set; }

        public DeleteMessage() { }
        public DeleteMessage(string exchangeCode, Guid deletedOrderId, Guid instrumentId, Transaction[] transactions, Order[] orders, OrderRelation[] orderRelations)
        {
            this.ExchangeCode = exchangeCode;
            this.DeletedOrderId = deletedOrderId;
            this.InstrumentID = instrumentId;
            this.Transactions = transactions;
            this.Orders = orders;
            this.OrderRelations = orderRelations;
        }

        #region IFilter
        public Guid? AccountId
        {
            get { return this.AccountID; }
        }

        public Guid? InstrumentId
        {
            get { return this.InstrumentID; }
        }
        #endregion
    }

    public class ExecuteMessage : ExchangeMessage, IFilterable
    {
        public Transaction[] Transactions { get; set; }
        public Order[] Orders { get; set; }
        public OrderRelation[] OrderRelations { get; set; }

        public ExecuteMessage() { }
        public ExecuteMessage(string exchangeCode, Transaction[] transactions, Order[] orders, OrderRelation[] orderRelations)
        {
            this.ExchangeCode = exchangeCode;
            this.Transactions = transactions;
            this.Orders = orders;
            this.OrderRelations = orderRelations;
        }

        #region IFilter
        public Guid? AccountId
        {
            get { return null; }
        }

        public Guid? InstrumentId
        {
            get { return null; }
        }
        #endregion
    }

    public class Execute2Message : ExchangeMessage, IFilterable
    {
        public Transaction[] Transactions { get; set; }
        public Order[] Orders { get; set; }
        public OrderRelation[] OrderRelations { get; set; }

        public Execute2Message(string exchangeCode, Transaction[] transactions, Order[] orders, OrderRelation[] orderRelations)
        {
            this.ExchangeCode = exchangeCode;
            this.Transactions = transactions;
            this.Orders = orders;
            this.OrderRelations = orderRelations;
        }

        #region IFilter
        public Guid? AccountId
        {
            get { return null; }
        }

        public Guid? InstrumentId
        {
            get { return null; }
        }
        #endregion
    }

    public class CutMessage : ExchangeMessage, IFilterable
    {
        public Transaction[] Transactions { get; set; }
        public Order[] Orders { get; set; }
        public OrderRelation[] OrderRelations { get; set; }

        #region IFilter
        public Guid? AccountId
        {
            get { return null; }
        }

        public Guid? InstrumentId
        {
            get { return null; }
        }
        #endregion
    }

    public class CancelMessage : ExchangeMessage, IFilterable
    {
        public Guid TransactionId { get; set; }
        public TransactionError ErrorCode { get; set; }
        public CancelReason CancelReason { get; set; }

        public CancelMessage(string exchangeCode, Guid transactionId, TransactionError errorCode, CancelReason cancelReason)
        {
            this.ExchangeCode = exchangeCode;
            this.TransactionId = transactionId;
            this.ErrorCode = errorCode;
            this.CancelReason = cancelReason;
        }

        #region IFilter
        public Guid? AccountId
        {
            get { return null; }
        }

        public Guid? InstrumentId
        {
            get { return null; }
        }
        #endregion
    }

   

    //OrderTask
    public class AcceptPlaceMessage : ExchangeMessage, IFilterable
    {
        public Guid InstrumentID { get; set; }
        public Guid AccountID { get; set; }
        public Guid TransactionID { get; set; }
        public TransactionError ErrorCode = TransactionError.OK;

        public AcceptPlaceMessage()
        { 
        }
        public AcceptPlaceMessage(string exchangeCode,Guid instrumentID, Guid accountID, Guid transactionID, TransactionError errorCode)
        {
            this.ExchangeCode = exchangeCode;
            this.InstrumentID = instrumentID;
            this.AccountID = accountID;
            this.TransactionID = transactionID;
            this.ErrorCode = errorCode;
        }

        #region IFilter
        public Guid? AccountId
        {
            get { return this.AccountID; }
        }

        public Guid? InstrumentId
        {
            get { return this.InstrumentID; }
        }
        #endregion
    }

    public class UpdateInstrumentQuotationMessage : Message, IFilterable
    {
        public InstrumentQuotationSet[] QuotePolicyChangeDetails { get; set; }
    
        public UpdateInstrumentQuotationMessage()
        {
        }

        public UpdateInstrumentQuotationMessage(List<InstrumentQuotationSet> quotePolicyChangeDetail)
        {
            this.QuotePolicyChangeDetails = quotePolicyChangeDetail.ToArray();
        }

        #region IFilter
        public Guid? AccountId
        {
            get { return null; }
        }

        public Guid? InstrumentId
        {
            get { return null; }
        }
        #endregion
    }

    public class TaskSchedulerRunMessage : Message, IFilterable
    {
        public bool IsSucceed { get; set; }
        public TaskScheduler TaskScheduler { get; set; }
        public TaskSchedulerRunMessage()
        { 
        }
        public TaskSchedulerRunMessage(TaskScheduler taskScheduler, bool isSucceed)
        {
            this.IsSucceed = isSucceed;
            this.TaskScheduler = taskScheduler;
        }
        #region IFilter
        public Guid? AccountId
        {
            get { return null; }
        }

        public Guid? InstrumentId
        {
            get { return null; }
        }
        #endregion
    }

    public class UpdateRoleMessage : Message
    {
        public UpdateRoleType Type;
        public int RoleId;

        public UpdateRoleMessage() { }
    }

    public class NotifyLogoutMessage:Message
    {
        public Guid UserId;

        public NotifyLogoutMessage() { }
    }
}
