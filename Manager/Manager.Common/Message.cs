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

        public Guid? AccountId
        {
            get { return null; }
        }

        public Guid? InstrumentId
        {
            get { return this.InstrumentID; }
        }
    }

    public class PrimitiveQuotationMessage : Message
    {
        public PrimitiveQuotation Quotation;
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

        public Guid? AccountId
        {
            get { return null; }
        }

        public Guid? InstrumentId
        {
            get { return null; }
        }
    }

   
}
