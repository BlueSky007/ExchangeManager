using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ManagerConsole.Helper;

namespace ManagerConsole.ViewModel
{
    public class VmQuotationBase : PropertyChangedNotifier
    {
        private DateTime _Timestamp;
        private string _Ask;
        private string _Bid;
        private string _Last;

        public DateTime Timestamp
        {
            get
            {
                return this._Timestamp;
            }
            set
            {
                if (this._Timestamp != value)
                {
                    this._Timestamp = value;
                    this.OnPropertyChanged("Timestamp");
                }
            }
        }        
        public string Ask
        {
            get
            {
                return this._Ask;
            }
            set
            {
                if (this._Ask != value)
                {
                    this._Ask = value;
                    this.OnPropertyChanged("Ask");
                }
            }
        }
        public string Bid
        {
            get
            {
                return this._Bid;
            }
            set
            {
                if (this._Bid != value)
                {
                    this._Bid = value;
                    this.OnPropertyChanged("Bid");
                }
            }
        }
        public string Last
        {
            get
            {
                return this._Last;
            }
            set
            {
                if (this._Last != value)
                {
                    this._Last = value;
                    this.OnPropertyChanged("Last");
                }
            }
        }
    }
}
