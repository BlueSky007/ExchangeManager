using System;
using iExchange.Common;

namespace ManagerService.QuotationExchange
{
    public interface IQuotation
    {
        string GetQuotation(Token token);
        bool SetQuotation(Token token, string quotation);
        bool UpdateQuotePolicy(Token token, string quotationPolicy);
    }

    public abstract class PersistObject
    {
        protected ModifyState modifyState = ModifyState.Unchanged;
        public abstract void Merge(PersistObject persistObject);
        public abstract string GetUpdateSql(bool resetModifyState);
        public bool Saved { get; set; }
        public ModifyState ModifyState
        {
            get { return this.modifyState; }
            set { this.modifyState = value; }
        }
    }

    public enum ModifyState
    {
        Unchanged,
        Added,
        Modified
    }
}
