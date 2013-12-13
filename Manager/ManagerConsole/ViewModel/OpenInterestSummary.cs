using ManagerConsole.Helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using AccountType = iExchange.Common.AccountType;
using CommonOpenInterestSummary = iExchange.Common.Manager.OpenInterestSummary;
using Price = iExchange.Common.Price;

namespace ManagerConsole.ViewModel
{
    public class OpenInterestSummaryModel
    {
        public ObservableCollection<OpenInterestSummary> SummaryItems { get; set; }
        public OpenInterestSummaryModel()
        {
            this.SummaryItems = new ObservableCollection<OpenInterestSummary>();
        }
    }

    public class OpenInterestSummary : PropertyChangedNotifier
    {
        #region private property
        private Guid _SummaryItemId;
        private Guid _InstrumentId;
        private Guid _Id;
        private string _Code;
        private AccountType _AccountType;
        private Guid _GroupId;
        private string _GroupCode;
        private OpenInterestSummaryType _Type;
        private int _MinNumeratorUnit = 1;
        private int _MaxDenominator = 1;
        private decimal _BuyLot = decimal.Zero;
        private string _BuyAvgPrice = "0";
        private decimal _BuyContractSize = decimal.Zero;
        private decimal _SellLot = decimal.Zero;
        private string _SellAvgPrice = "0";
        private decimal _SellContractSize = 0;
        private decimal _NetLot = decimal.Zero;
        private string _NetAvgPrice = "0";
        private decimal _NetContractSize = decimal.Zero;

        private decimal _BuyLotMultiplyAvgPriceSum = decimal.Zero;
        private decimal _SellLotMultiplyAvgPriceSum = decimal.Zero;
        private decimal _NetLotMultiplyAvgPriceSum = decimal.Zero;
        private bool _IsExclude = false;
        private ObservableCollection<OpenInterestSummary> _ChildSummaryItems;
        #endregion

        public OpenInterestSummary(OpenInterestSummaryType type)
        {
            this._Type = type;
            this._ChildSummaryItems = new ObservableCollection<OpenInterestSummary>();
        }

        public OpenInterestSummary(CommonOpenInterestSummary openInterestSummary,OpenInterestSummaryType type)
        {
            this._ChildSummaryItems = new ObservableCollection<OpenInterestSummary>();
            this.Update(openInterestSummary,type);
        }

        #region public property
        public ObservableCollection<OpenInterestSummary> ChildSummaryItems
        {
            get { return this._ChildSummaryItems; }
            set { this._ChildSummaryItems = value; }
        }
        public Guid Id
        {
            get { return this._Id; }
            set { this._Id = value; }
        }
        public Guid InstrumentId
        {
            get { return this._InstrumentId; }
            set { this._InstrumentId = value; }
        }
        public Guid GroupId
        {
            get { return this._GroupId; }
            set { this._GroupId = value; }
        }

        public string GroupCode
        {
            get { return this._GroupCode; }
            set { this._GroupCode = value; }
        }
        public string Code
        {
            get { return this._Code; }
            set { this._Code = value; }
        }
        public AccountType AccountType
        {
            get { return this._AccountType; }
            set { this._AccountType = value; }
        }
        public OpenInterestSummaryType Type
        {
            get { return this._Type; }
            set { this._Type = value; }
        }
        public decimal BuyLot
        {
            get { return this._BuyLot; }
            set { this._BuyLot = value; }
        }
        public string BuyAvgPrice
        {
            get { return this._BuyAvgPrice; }
            set { this._BuyAvgPrice = value; }
        }
        public decimal BuyContractSize
        {
            get { return this._BuyContractSize; }
            set { this._BuyContractSize = value; }
        }
        public decimal SellLot
        {
            get { return this._SellLot; }
            set { this._SellLot = value; }
        }
        public string SellAvgPrice
        {
            get { return this._SellAvgPrice; }
            set { this._SellAvgPrice = value; }
        }
        public decimal SellContractSize
        {
            get { return this._SellContractSize; }
            set { this._SellContractSize = value; }
        }
        public decimal NetLot
        {
            get { return this._NetLot; }
            set { this._NetLot = value; }
        }
        public string NetAvgPrice
        {
            get { return this._NetAvgPrice; }
            set { this._NetAvgPrice = value; }
        }
        public decimal NetContractSize
        {
            get { return this._NetContractSize; }
            set { this._NetContractSize = value; }
        }
        public bool IsExclude
        {
            get { return this._IsExclude; }
            set { this._IsExclude = value; }
        }
        #endregion
        internal void CreateEmptySummary()
        {
            OpenInterestSummary emptyEntity = new OpenInterestSummary(OpenInterestSummaryType.Group);
            emptyEntity.Code = "Loading...";
            this.ChildSummaryItems.Add(emptyEntity);
        }
        
        internal void Update(CommonOpenInterestSummary openInterestSummary,OpenInterestSummaryType type)
        {
            this._Id = openInterestSummary.Id;
            this._Code = openInterestSummary.Code;
            this._GroupId = openInterestSummary.GroupId;
            this._GroupCode = openInterestSummary.GroupCode;
            this._AccountType = openInterestSummary.AccountType;
            this._Type = type;
            this._MinNumeratorUnit = openInterestSummary.MinNumeratorUnit;
            this._MaxDenominator = openInterestSummary.MaxDenominator;
            this._BuyLot = openInterestSummary.BuyLot;
            this._BuyAvgPrice = openInterestSummary.BuyAvgPrice;
            this._BuyContractSize = openInterestSummary.BuyContractSize;
            this._SellLot = openInterestSummary.SellLot;
            this._SellAvgPrice = openInterestSummary.SellAvgPrice;
            this._SellContractSize = openInterestSummary.SellContractSize;

            this.CreateEmptySummary();
        }

        internal void SetItem(OpenInterestSummary childItem, bool inCrease)
        {
            if (inCrease)
            {
                this._BuyLot += childItem.BuyLot;
                this._BuyLotMultiplyAvgPriceSum += childItem.BuyLot * decimal.Parse(childItem.BuyAvgPrice);
                this._BuyContractSize += childItem.BuyContractSize;
                this._SellLot += childItem._SellLot;
                this._SellLotMultiplyAvgPriceSum += childItem.SellLot * decimal.Parse(childItem.SellAvgPrice);
                this._SellContractSize += childItem.SellContractSize;
                this._NetLot += childItem.NetLot;
                this._NetLotMultiplyAvgPriceSum += childItem.NetLot * decimal.Parse(childItem.NetAvgPrice);
                this._NetContractSize += childItem.NetContractSize;
            }
            else
            {
                this._BuyLot -= childItem.BuyLot;
                this._BuyLotMultiplyAvgPriceSum -= childItem.BuyLot * decimal.Parse(childItem.BuyAvgPrice);
                this._BuyContractSize -= childItem.BuyContractSize;
                this._SellLot -= childItem._SellLot;
                this._SellLotMultiplyAvgPriceSum -= childItem.SellLot * decimal.Parse(childItem.SellAvgPrice);
                this._SellContractSize -= childItem.SellContractSize;
                this._NetLot -= childItem.NetLot;
                this._NetLotMultiplyAvgPriceSum -= childItem.NetLot * decimal.Parse(childItem.NetAvgPrice);
                this._NetContractSize -= childItem.NetContractSize;
            }
        }

        internal void SetAvgPrice()
        {
            Price price = null;
            decimal avgBuyPriceValue = this._BuyLot != decimal.Zero ? this._BuyLotMultiplyAvgPriceSum / this._BuyLot : decimal.Zero;
            price = new Price(avgBuyPriceValue.ToString(), this._MinNumeratorUnit, this._MaxDenominator);
            this._BuyAvgPrice = price == null ? "0" : price.ToString();

            decimal avgSellPriceValue = this._SellLot != decimal.Zero ? this._SellLotMultiplyAvgPriceSum / this._SellLot : decimal.Zero;
            price = new Price(avgSellPriceValue.ToString(), this._MinNumeratorUnit, this._MaxDenominator);
            this._SellAvgPrice = price == null ? "0" : price.ToString();

            decimal avgNetPriceValue = this._NetLot != decimal.Zero ? this._NetLotMultiplyAvgPriceSum / this._NetLot : decimal.Zero;
            price = new Price(avgNetPriceValue.ToString(), this._MinNumeratorUnit, this._MaxDenominator);
            this._NetAvgPrice = price == null ? "0" : price.ToString();
        }
    }
}
