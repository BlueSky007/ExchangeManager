using ManagerConsole.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ManagerConsole.Model
{
    public class InstrumentClient : PropertyChangedNotifier
    {
        public delegate void PriceChangedHandler(string priceType, string price);
        public event PriceChangedHandler OnPriceChangedHandler;
        #region Private Property
        private string _Origin;
        private string _LastOrigin;
        private string _OriginCode = string.Empty;
        private string _Bid;
        private string _Ask;
        private int _AlertPoint;
        private int? _AutoPoint;
        private int? _MaxAutoPoint;
        private int? _Spread;
        private int? _MaxSpread;
        private int? _AcceptDQVariation;
        #endregion
        public Guid Id
        {
            get;
            set;
        }
        public string OriginCode
        {
            get;
            set;
        }
        public string Code
        {
            get;
            set;
        }
        public string Description
        {
            get;
            set;
        }
        public int? Denominator
        {
            get;
            set;
        }
        public int? NumeratorUnit
        {
            get;
            set;
        }
        public string Origin
        {
            get
            { return this._Origin; }
            set
            {
                this.LastOrigin = this._Origin;
                this._Origin = value;
                this.OnPropertyChanged("Origin");
            }
        }
        public string LastOrigin
        {
            get { return this._LastOrigin; }
            set
            {
                this._LastOrigin = value;
                this.OnPropertyChanged("LastOrigin");

            }
        }
        public string Bid
        {
            get { return this._Bid; }
            set
            {
                this._Bid = value; 
                this.OnPropertyChanged("Bid");
                if (this.OnPriceChangedHandler != null)
                {
                    this.OnPriceChangedHandler("Bid", value);
                }
            }
        }
        public string Ask
        {
            get { return this._Ask; }
            set
            {
                this._Ask = value;
                this.OnPropertyChanged("Ask");
                if (this.OnPriceChangedHandler != null)
                {
                    this.OnPriceChangedHandler("Ask", value);
                }
            }
        }
        public int AlertPoint
        {
            get { return this._AlertPoint; }
            set
            {
                if (Regex.IsMatch(value.ToString(), "(\\d+)"))
                {
                    this._AlertPoint = value; this.OnPropertyChanged("AlertPoint");
                }
                else
                {
                    throw new Exception("Alert Point must be int");
                }
            }
        }

        public int? AutoPoint
        {
            get { return this._AutoPoint; }
            set
            {
                if (value <= this._MaxAutoPoint && Regex.IsMatch(value.ToString(), "(-?\\d+)"))
                {
                    this._AutoPoint = value; this.OnPropertyChanged("AutoPoint");
                }
                else
                {
                    throw new Exception("Invalid Input Auto Point");
                }
            }
        }
        public int? MaxAutoPoint
        {
            get { return this._MaxAutoPoint; }
            set
            {
                if (Regex.IsMatch(value.ToString(), "(\\d+)"))
                {
                    this._MaxAutoPoint = value; this.OnPropertyChanged("MaxAutoPoint");
                }
                else
                {
                    throw new Exception("Invalid Input Max Auto Point");
                }
            }
        }
        public int? Spread
        {
            get { return this._Spread; }
            set
            {
                if (value <= this._MaxSpread && Regex.IsMatch(value.ToString(), "(\\d+)"))
                {
                    this._Spread = value; this.OnPropertyChanged("Spread");
                }
                else
                {
                    throw new Exception("Invalid Input Spread");
                }
            }
        }
        public int? MaxSpread
        {
            get { return this._MaxSpread; }
            set
            {
                if (Regex.IsMatch(value.ToString(), "(\\d+)"))
                {
                    this._MaxSpread = value; this.OnPropertyChanged("MaxSpread");
                }
                else
                {
                    throw new Exception("Invalid Input Max Spread");
                }
            }
        }

        public int AcceptDQVariation
        {
            get { return (int)this._AcceptDQVariation; }
            set{this._AcceptDQVariation = value;}
        }

        public bool CheckVariation(decimal variation)
        {
            if (variation < 0 && variation < (0 - this.AcceptDQVariation))
            {
                return false;
            }
            return true;
        }




    }
}
