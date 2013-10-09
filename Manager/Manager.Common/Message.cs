using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace Manager.Common
{
    public interface IFilterable
    {
        Guid? AccountId { get; }
        Guid? InstrumentId { get; }
    }

    [KnownType(typeof(PriceMessage)),
    KnownType(typeof(QuoteMessage)),
    KnownType(typeof(PrimitiveQuotationMessage))]
    public class Message
    {
        public string ExchangeCode { get; set; }
    }

    public class PriceMessage : Message
    {
    }

    public class QuoteMessage : Message, IFilterable
    {
        public Guid CustomerID { get; set; }
        public Guid InstrumentID { get; set; }
        public double QuoteLot { get; set; }
        public int BSStatus { get; set; }	//0:Sell 1:Buy 2:Mis

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

    public class PrimitiveQuotationMessage : Message, IFilterable
    {
        public PrimitiveQuotation Quotation;

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

    public class SourceStatusMessage : Message
    {
        public string SouceName;
        public ConnectionState ConnectionState;
    }

    public class UpdateMessage : Message,IFilterable
    {
        public UpdateAction UpdateAction { get; set; }

        public SettingSet AddSettingSets { get; set; }
        public SettingSet ModifySettings { get; set; }
        public SettingSet DeletedSettings { get; set; }

        public UpdateMessage(string exchagenCode,SettingSet addSettingSets, SettingSet modifySettings, SettingSet deletedSettings)
        {
            this.ExchangeCode = exchagenCode;
            this.AddSettingSets = addSettingSets;
            this.ModifySettings = modifySettings;
            this.DeletedSettings = deletedSettings;
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

    public class PlaceMessage : Message, IFilterable
    {
        public Transaction[] Transactions { get; set; }
        public Order[] Orders { get; set; }
        public OrderRelation[] OrderRelations { get; set; }

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
            get { return null; }
        }

        public Guid? InstrumentId
        {
            get { return null; }
        }
        #endregion
    }

    public class ExecuteMessage : Message, IFilterable
    {
        public Transaction[] Transactions { get; set; }
        public Order[] Orders { get; set; }
        public OrderRelation[] OrderRelations { get; set; }

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

    public class Execute2Message : Message, IFilterable
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

    public class CutMessage : Message, IFilterable
    {
        public Transaction[] Transactions { get; set; }
        public Order[] Orders { get; set; }
        public OrderRelation[] OrderRelations { get; set; }

        public CutMessage(string exchangeCode, Transaction[] transactions, Order[] orders, OrderRelation[] orderRelations)
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

    public class CancelMessage : Message, IFilterable
    {
        public Guid TransactionId { get; set; }
        public TransactionError ErrorCode { get; set; }
        public CancelReason CancelReason { get; set; }

        public CancelMessage(Guid transactionId,TransactionError errorCode,CancelReason cancelReason)
        {
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

    public class HitMessage : Message, IFilterable
    {
        public Order[] Orders { get; set; }

        public HitMessage(Order[] orders)
        {
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

    
   
}
