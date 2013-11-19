using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ManagerConsole.Helper;

namespace ManagerConsole.ViewModel
{
    public class QuotationSource : PropertyChangedNotifier
    {
        //private int _Id;
        private string _Name;
        private string _AuthName;
        private string _Password;

        public string Delete { get { return "Delete"; } }

        public int Id { get; set; }
        public string Name
        {
            get { return this._Name; }
            set
            {
                if (this._Name != value)
                {
                    this._Name = value;
                    this.OnPropertyChanged("Name");
                }
            }
        }
        public string AuthName
        {
            get { return this._AuthName; }
            set
            {
                if (this._AuthName != value)
                {
                    this._AuthName = value;
                    this.OnPropertyChanged("AuthName");
                }
            }
        }
        public string Password
        {
            get { return this._Password; }
            set
            {
                if (this._Password != value)
                {
                    this._Password = value;
                    this.OnPropertyChanged("Password");
                }
            }
        }

    }
}
