using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Manager.Common.QuotationEntities;
using ManagerConsole.Helper;
using Manager.Common;

namespace ManagerConsole.ViewModel
{
    public class VmQuotationSource : VmBase
    {
        private QuotationSource _QuotationSource;
        private ConnectionState _ConnectionState = ConnectionState.Unknown;

        public VmQuotationSource(QuotationSource quotationSource)
            : base(quotationSource)
        {
            this._QuotationSource = quotationSource;
        }

        public int Id { get { return this._QuotationSource.Id; } set { this._QuotationSource.Id = value; } }
        public string Name
        {
            get { return this._QuotationSource.Name; }
            set
            {
                if (this._QuotationSource.Name != value)
                {
                    this._QuotationSource.Name = value;
                    this.OnPropertyChanged("Name");
                }
            }
        }
        public string AuthName
        {
            get { return this._QuotationSource.AuthName; }
            set
            {
                if (this._QuotationSource.AuthName != value)
                {
                    this._QuotationSource.AuthName = value;
                    this.OnPropertyChanged("AuthName");
                }
            }
        }
        public string Password
        {
            get { return this._QuotationSource.Password; }
            set
            {
                if (this._QuotationSource.Password != value)
                {
                    this._QuotationSource.Password = value;
                    this.OnPropertyChanged("Password");
                }
            }
        }

        public ConnectionState ConnectionState
        {
            get { return this._ConnectionState; }
            set
            {
                if (this._ConnectionState != value)
                {
                    this._ConnectionState = value;
                    this.OnPropertyChanged("ConnectionState");
                }
            }
        }

        public VmQuotationSource Clone()
        {
            QuotationSource source = new QuotationSource() { Id = this._QuotationSource.Id, Name = this._QuotationSource.Name, AuthName = this._QuotationSource.AuthName, Password = this._QuotationSource.Password };
            return new VmQuotationSource(source);
        }
    }
}
