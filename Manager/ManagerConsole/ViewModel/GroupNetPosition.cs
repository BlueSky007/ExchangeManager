using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using AccountType = iExchange.Common.AccountType;
using Price = Manager.Common.Price;
using CommonAccountGroupGNP = iExchange.Common.Manager.AccountGroupGNP;
using CommonAccountGNP = iExchange.Common.Manager.AccountGNP;
using CommonInstrumentGNP = iExchange.Common.Manager.InstrumentGNP;
using ManagerConsole.Model;
using System.Dynamic;
using System.ComponentModel;
using ManagerConsole.Helper;

namespace ManagerConsole.ViewModel
{
    public class RootGNP : InstrumentColumn
    {
        private Guid _Id = Guid.Empty;
        private string _Code = "Total";
        private ObservableCollection<AccountGroupGNP> _AccountGroupGNPs;

        public RootGNP()
        {
            this._AccountGroupGNPs = new ObservableCollection<AccountGroupGNP>();
        }

        public ObservableCollection<AccountGroupGNP> AccountGroupGNPs
        {
            get { return this._AccountGroupGNPs; }
            set { this._AccountGroupGNPs = value; }
        }

        public Guid Id
        {
            get { return this._Id; }
            set { this._Id = value; }
        }

        public string Code
        {
            get { return this._Code; }
            set { this._Code = value; }
        }
    }

    public class AccountGroupGNP : InstrumentColumn
    {
        private Guid _Id;
        private string _Code;
        private bool _IsSelected = true;
        private decimal _OIPercent = 100;
        private decimal _OldOIPercent = 100;
        private Dictionary<string, decimal> _GroupSummaryDic = new Dictionary<string, decimal>();

        private ObservableCollection<AccountGNP> _AccountGNPs;

        public AccountGroupGNP(CommonAccountGroupGNP accountGroupGNP)
        {
            this._AccountGNPs = new ObservableCollection<AccountGNP>();
            this._Id = accountGroupGNP.Id;
            this._Code = accountGroupGNP.Code;

            this.Update(accountGroupGNP.AccountGNPs);
        }

        public Guid Id
        {
            get { return this._Id; }
        }
        public string Code
        {
            get { return this._Code; }
        }
        public bool IsSelected
        {
            get { return this._IsSelected; }
            set
            { 
                this._IsSelected = value;
                this.OnPropertyChanged("IsSelected");
            }
        }
        public decimal OldOIPercent
        {
            get { return this._OldOIPercent; }
            set
            {
                this._OldOIPercent = value;
                this.OnPropertyChanged("OldOIPercent");
            }
        }
        public decimal OIPercent
        {
            get { return this._OIPercent; }
            set
            {
                this._OIPercent = value;
                this.OnPropertyChanged("OIPercent");
            }
        }
        public ObservableCollection<AccountGNP> AccountGNPs
        {
            get { return this._AccountGNPs; }
            set { this._AccountGNPs = value; }
        }

        public Dictionary<string, decimal> GroupSummaryDic
        {
            get { return this._GroupSummaryDic; }
            set { this._GroupSummaryDic = value; }
        }

        internal void Update(List<CommonAccountGNP> accountGNPs)
        {
            foreach (CommonAccountGNP item in accountGNPs)
            {
                AccountGNP accountGNP = new AccountGNP(item,this.Id);
                this._AccountGNPs.Add(accountGNP);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class AccountGNP : InstrumentColumn
    {
        private Guid _Id;
        private Guid _GroupId;
        private string _Code;
        private AccountType _Type;

        private ObservableCollection<InstrumentGNP> _InstrumentGNPs;
        private ObservableCollection<DetailGNP> _DetailGNPs;

        public AccountGNP(CommonAccountGNP accountGNP,Guid groupId)
        {
            this._GroupId = groupId;
            this._InstrumentGNPs = new ObservableCollection<InstrumentGNP>();
            this._DetailGNPs = new ObservableCollection<DetailGNP>();
            this.Update(accountGNP);
            this.Update(accountGNP.InstrumentGNPs);
        }

        #region Public Property
        public Guid Id
        {
            get { return this._Id; }
        }
        public Guid GroupId
        {
            get { return this._GroupId; }
        }
        public string Code
        {
            get { return this._Code; }
        }
        public AccountType Type
        {
            get { return this._Type; }
        }
        public ObservableCollection<InstrumentGNP> InstrumentGNPs
        {
            get { return this._InstrumentGNPs; }
            set { this._InstrumentGNPs = value; }
        }
        public ObservableCollection<DetailGNP> DetailGNPs
        {
            get { return this._DetailGNPs; }
            set { this._DetailGNPs = value; }
        }
        #endregion

        internal void Update(CommonAccountGNP accountGNP)
        {
            this._Id = accountGNP.Id;
            this._Code = accountGNP.Code;
            this._Type = accountGNP.Type;
        }
        internal void Update(List<CommonInstrumentGNP> instrumentGNPs)
        {
            foreach (CommonInstrumentGNP item in instrumentGNPs)
            {
                InstrumentGNP instrumentGNP = new InstrumentGNP(item,this.Id);
                this._InstrumentGNPs.Add(instrumentGNP);
            }
        }
    }

    public class InstrumentGNP
    {
        private Guid _Id;
        private Guid _AccountId;
        private InstrumentClient _Instrument;
        private decimal _LotBalance = decimal.Zero;
        private decimal _Quantity = decimal.Zero;
        private decimal _BuyQuantity = decimal.Zero;
        private decimal _BuyMultiplyValue = decimal.Zero;
        private decimal _SellQuantity = decimal.Zero;
        private decimal _SellMultiplyValue = decimal.Zero;
        private string _BuyAveragePrice;
        private string _SellAveragePrice;
        private bool _IsSummaryGroup = false;
        private string _SummaryGroupCode;
        private int _SortIndex = 0;

        public InstrumentGNP() { }

        public InstrumentGNP(CommonInstrumentGNP instrumentGNP,Guid accountId)
        {
            this._AccountId = accountId;
            this.Update(instrumentGNP);
        }

        #region public property
        public Guid Id
        {
            get { return this._Id; }
        }

        public int ColumnIndex
        {
            get;
            set;
        }
        public Guid AccountId
        {
            get { return this._AccountId; }
        }

        public InstrumentClient Instrument
        {
            get{return this._Instrument;}
            set{this._Instrument = value;}
        }

        public string InstrumentCode
        {
            get { return this._Instrument.Code; }
        }

        public string SummaryGroupCode
        {
            get { return this._SummaryGroupCode; }
            set { this._SummaryGroupCode = value; }
        }

        public bool IsSummaryGroup
        {
            get { return this._IsSummaryGroup; }
            set { this._IsSummaryGroup = value; }
        }

        public decimal LotBalance
        {
            get { return this._LotBalance; }
            set { this._LotBalance = value; }
        }

        public decimal Quantity
        {
            get { return this._Quantity; }
            set { this._Quantity = value; }
        }

        public decimal BuyQuantity
        {
            get { return this._BuyQuantity; }
            set { this._BuyQuantity = value; }
        }

        public decimal BuyMultiplyValue
        {
            get { return this._BuyMultiplyValue; }
            set { this._BuyMultiplyValue = value; }
        }

        public string BuyAveragePrice
        {
            get { return this._BuyAveragePrice; }
            set { this._BuyAveragePrice = value; }
        }

        public decimal SellQuantity
        {
            get { return this._SellQuantity; }
            set { this._SellQuantity = value; }
        }

        public string SellAveragePrice
        {
            get { return this._SellAveragePrice; }
            set { this._SellAveragePrice = value; }
        }

        public decimal SellMultiplyValue
        {
            get { return this._SellMultiplyValue; }
            set { this._SellMultiplyValue = value; }
        }

        public string Detail
        {
            get { return this.GetDetailDisPlay(); }
        }

        public int SortIndex
        {
            get { return this._SortIndex; }
            set { this._SortIndex = value; }
        }
        #endregion

        internal void Update(CommonInstrumentGNP instrumentGNP)
        { 
            this._Id = instrumentGNP.Id;
            this._LotBalance = instrumentGNP.LotBalance;
            this._Quantity = instrumentGNP.Quantity;
            this._BuyQuantity = instrumentGNP.BuyQuantity;
            this._BuyMultiplyValue = instrumentGNP.BuyMultiplyValue;
            this._SellQuantity = instrumentGNP.SellQuantity;
            this._SellMultiplyValue = instrumentGNP.SellMultiplyValue;
            this._BuyAveragePrice = instrumentGNP.BuyAveragePrice;
            this._SellAveragePrice = instrumentGNP.SellAveragePrice;
        }

        public void AddLotBalance(decimal value)
        {
            this._LotBalance += value;
        }

        public void AddQuantity(decimal value)
        {
            this._Quantity += value;
        }

        internal string GetDetailDisPlay()
        {
            string detailStr = string.Empty;
            if (this._SellQuantity != decimal.Zero)
            {
                detailStr = this._SellQuantity + "S x " + this._SellAveragePrice;
            }
            if (this._BuyQuantity != decimal.Zero)
            {
                detailStr += (string.IsNullOrEmpty(detailStr) ? string.Empty: "/") + this._BuyQuantity + "B x " + this._BuyAveragePrice;
            }
            if (string.IsNullOrEmpty(detailStr)) detailStr = "--";
            return detailStr;
        }
    }

    public class DetailGNP : InstrumentColumn
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string OrderRelation
        {
            get { return "Detail"; }
        }
    }

    public class InstrumentColumn
    {
        public object EmptyColumn1 { get; set; }
        public object EmptyColumn2 { get; set; }
        private ColumnKeys _Keys = new ColumnKeys();
        public ColumnKeys Columns { get { return _Keys; } }
    }

    public class ColumnKeys : DynamicObject, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private Dictionary<string, object> dict = new Dictionary<string, object>();
        public void Reset()
        {
            dict = new Dictionary<string, object>();
        }

        public Dictionary<string, object> ResourceDictionary
        {
            get
            {
                return dict;
            }
        }

        public object this[string index]
        {
            get
            {
                object result;
                dict.TryGetValue(index, out result);
                return result;
            }
            set
            {
                dict[index] = value;
                OnPropertyChanged(string.Format("Item[{0}]", index));
            }
        }

        public void AddResource(string key, object value)
        {
            dict[key] = value;
            OnPropertyChanged(string.Format("Item[{0}]", key));
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return dict.TryGetValue(binder.Name, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            dict[binder.Name] = value;
            OnPropertyChanged(string.Format("Item[{0}]", binder.Name));
            return true;
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

    }

    public class InstrumentGNPSummaryCompare : IComparer<InstrumentGNP>
    {
        #region IComparer<InstrumentGNP> Members

        int IComparer<InstrumentGNP>.Compare(InstrumentGNP x, InstrumentGNP y)
        {
            int result = x.SummaryGroupCode.CompareTo(y.SummaryGroupCode);
            return result;
        }
        #endregion
    }

    public class InstrumentGNPCompare : IComparer<InstrumentGNP>
    {
        #region IComparer<InstrumentGNP> Members

        int IComparer<InstrumentGNP>.Compare(InstrumentGNP x, InstrumentGNP y)
        {
            return (x.SortIndex - y.SortIndex);
        }
        #endregion
    }
}
