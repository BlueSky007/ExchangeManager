using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Manager.Common.QuotationEntities;
using ManagerConsole.Helper;

namespace ManagerConsole.ViewModel
{
    //public interface IMetadataViewModelObject { }

    public class VmQuotationSource : PropertyChangedNotifier//, IMetadataViewModelObject
    {
        private QuotationSource _QuotationSource;
        //private string _Name;
        //private string _AuthName;
        //private string _Password;

        public VmQuotationSource(QuotationSource quotationSource)
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

        public VmQuotationSource Clone()
        {
            QuotationSource source = new QuotationSource() { Id = this._QuotationSource.Id, Name = this._QuotationSource.Name, AuthName = this._QuotationSource.AuthName, Password = this._QuotationSource.Password };
            return new VmQuotationSource(source);
        }

        public void Update(Dictionary<string, object> fieldAndValues)
        {
            foreach (string key in fieldAndValues.Keys)
            {
                if (key == FieldSR.Name) this.Name = (string)fieldAndValues[key];
                else if (key == FieldSR.AuthName) this.AuthName = (string)fieldAndValues[key];
                else if (key == FieldSR.Password) this.Password = (string)fieldAndValues[key];
            }
        }
    }
}
