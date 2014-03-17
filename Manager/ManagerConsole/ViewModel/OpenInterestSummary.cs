using ManagerConsole.Helper;
using ManagerConsole.Model;
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
        public ObservableCollection<OpenInterestSummary> InstrumentSummaryItems;

        public OpenInterestSummaryModel()
        {
            this.InstrumentSummaryItems = new ObservableCollection<OpenInterestSummary>();
        }

        public void Reset()
        {
            this.InstrumentSummaryItems.Clear();
        }

        public void ResetChildItem(OpenInterestSummary summayItem)
        {
            summayItem.ChildSummaryItems.Clear();
        }

        public void AddInstrumentItem(OpenInterestSummary SummaryItem)
        {
            this.InstrumentSummaryItems.Add(SummaryItem);
        }

        public OpenInterestSummary GetInstrumentSummaryItem(Guid instrumentId)
        {
            return this.InstrumentSummaryItems.SingleOrDefault(P => P.Id == instrumentId);
        }

        public OpenInterestSummary GetAccountGroupSummayItem(OpenInterestSummary parentSummayItem,Guid groupId)
        {
            return parentSummayItem.ChildSummaryItems.SingleOrDefault(P => P.GroupId == groupId);
        }

        public OpenInterestSummary GetAccountSummayItem(OpenInterestSummary groupSummaryItem, Guid accountId)
        {
            return groupSummaryItem.ChildSummaryItems.SingleOrDefault(P => P.Id == accountId);
        }

        public OpenInterestSummary GetOrderSummaryItem(Order order)
        {
            OpenInterestSummary instrumentSummaryItem = this.GetInstrumentSummaryItem(order.InstrumentId);
            if (instrumentSummaryItem == null) return null;
            OpenInterestSummary groupSummaryItem = this.GetAccountGroupSummayItem(instrumentSummaryItem,order.AccountId);
            if (groupSummaryItem == null) return null;
            OpenInterestSummary accountSummaryItem = this.GetAccountSummayItem(groupSummaryItem,order.AccountId);
            if (accountSummaryItem == null) return null;

            return accountSummaryItem.ChildSummaryItems.SingleOrDefault(P => P.Id == order.Id);
        }

        //账户排除汇总
        public void ExcludeAccountSummary(OpenInterestSummary accountSummaryItem, bool isExclude)
        {
            OpenInterestSummary instrumentSummaryItem = this.GetInstrumentSummaryItem(accountSummaryItem.InstrumentId);
            OpenInterestSummary groupSummaryItem = this.GetAccountGroupSummayItem(instrumentSummaryItem,accountSummaryItem.GroupId);

            if (isExclude)
            {
                groupSummaryItem.SetItem(accountSummaryItem, false);
                instrumentSummaryItem.SetItem(accountSummaryItem, false);
            }
            else
            {
                groupSummaryItem.SetItem(accountSummaryItem, true);
                instrumentSummaryItem.SetItem(accountSummaryItem, true);
            }
            groupSummaryItem.SetAvgPrice();
            instrumentSummaryItem.SetAvgPrice();
        }
        //账户组排除汇总
        public void ExcludeGroupSummary(OpenInterestSummary groupSummaryItem,bool isExclude)
        {
            OpenInterestSummary instrumentSummaryItem = this.GetInstrumentSummaryItem(groupSummaryItem.InstrumentId);
            foreach (OpenInterestSummary accountItem in groupSummaryItem.ChildSummaryItems)
            {
                accountItem.IsExclude = isExclude;
            }

            if (isExclude)
            {
                instrumentSummaryItem.SetItem(groupSummaryItem, false);
                groupSummaryItem.Exclude();
            }
            else
            {
                foreach (OpenInterestSummary accountItem in groupSummaryItem.ChildSummaryItems)
                {
                    groupSummaryItem.SetItem(accountItem, true);
                }
                groupSummaryItem.SetAvgPrice();
                instrumentSummaryItem.SetItem(groupSummaryItem, true);
            }
            instrumentSummaryItem.SetAvgPrice();
        }
        //账户单子汇总(删单)

        //删Open Order/Close Order
        public void DeleteOrderFromSummaryGrid(Order order,bool isAdd)
        {
            OpenInterestSummary instrumentSummaryItem = this.GetInstrumentSummaryItem(order.InstrumentId);
            if (instrumentSummaryItem == null) return;
            OpenInterestSummary groupSummaryItem = this.GetAccountGroupSummayItem(instrumentSummaryItem, order.Transaction.Account.GroupId);
            if (groupSummaryItem == null) return;
            OpenInterestSummary accountSummaryItem = this.GetAccountSummayItem(groupSummaryItem, order.AccountId);
            if (accountSummaryItem == null) return;
            OpenInterestSummary orderSummaryItem = accountSummaryItem.ChildSummaryItems.SingleOrDefault(P => P.Id == order.Id);
            if (orderSummaryItem == null) return;

            if (isAdd)
            {
                accountSummaryItem.ChildSummaryItems.Add(orderSummaryItem);
                instrumentSummaryItem.SetItem(orderSummaryItem, true);
                groupSummaryItem.SetItem(orderSummaryItem, true);
                accountSummaryItem.SetItem(orderSummaryItem, true);
            }
            else
            {
                accountSummaryItem.ChildSummaryItems.Remove(orderSummaryItem);
                instrumentSummaryItem.SetItem(orderSummaryItem, false);
                groupSummaryItem.SetItem(orderSummaryItem, false);
                accountSummaryItem.SetItem(orderSummaryItem, false);
            }
        }
    }

    public class OpenInterestSummary : PropertyChangedNotifier
    {
        #region private property
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

        public OpenInterestSummary(CommonOpenInterestSummary commonEntity,OpenInterestSummaryType type)
        {
            this._Type = type;
            this._ChildSummaryItems = new ObservableCollection<OpenInterestSummary>();
            if (commonEntity != null)
            {
                this.Update(commonEntity);
            }
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
            set { this._BuyLot = value; this.OnPropertyChanged("BuyLot"); }
        }
        public string BuyAvgPrice
        {
            get { return this._BuyAvgPrice; }
            set { this._BuyAvgPrice = value; this.OnPropertyChanged("BuyAvgPrice"); }
        }
        public decimal BuyContractSize
        {
            get { return this._BuyContractSize; }
            set { this._BuyContractSize = value; this.OnPropertyChanged("BuyContractSize"); }
        }
        public decimal SellLot
        {
            get { return this._SellLot; }
            set { this._SellLot = value; this.OnPropertyChanged("SellLot"); }
        }
        public string SellAvgPrice
        {
            get { return this._SellAvgPrice; }
            set { this._SellAvgPrice = value; this.OnPropertyChanged("SellAvgPrice"); }
        }
        public decimal SellContractSize
        {
            get { return this._SellContractSize; }
            set { this._SellContractSize = value; this.OnPropertyChanged("SellContractSize"); }
        }
        public decimal NetLot
        {
            get { return this._NetLot; }
            set { this._NetLot = value; this.OnPropertyChanged("NetLot"); }
        }
        public string NetAvgPrice
        {
            get { return this._NetAvgPrice; }
            set { this._NetAvgPrice = value; this.OnPropertyChanged("NetAvgPrice"); }
        }
        public decimal NetContractSize
        {
            get { return this._NetContractSize; }
            set { this._NetContractSize = value; this.OnPropertyChanged("NetContractSize"); }
        }
        public bool IsExclude
        {
            get { return this._IsExclude; }
            set { this._IsExclude = value; this.OnPropertyChanged("IsExclude");}
        }

        public object NullColumn { get; set; }
        #endregion
        internal void CreateEmptySummary()
        {
            OpenInterestSummary emptyEntity = new OpenInterestSummary(null,OpenInterestSummaryType.Group);
            emptyEntity.Code = "Loading...";
            this.ChildSummaryItems.Add(emptyEntity);
        }

        internal void Update(CommonOpenInterestSummary openInterestSummary)
        {
            this._Id = openInterestSummary.Id;
            this._InstrumentId = openInterestSummary.InstrumentId;
            this._Code = openInterestSummary.Code;
            this._GroupId = openInterestSummary.GroupId;
            this._GroupCode = openInterestSummary.GroupCode;
            this._AccountType = openInterestSummary.AccountType;
            this._MinNumeratorUnit = openInterestSummary.MinNumeratorUnit;
            this._MaxDenominator = openInterestSummary.MaxDenominator;
            this._BuyLot = openInterestSummary.BuyLot;
            this._BuyAvgPrice = openInterestSummary.BuyAvgPrice;
            this._BuyContractSize = openInterestSummary.BuyContractSize;
            this._SellLot = openInterestSummary.SellLot;
            this._SellAvgPrice = openInterestSummary.SellAvgPrice;
            this._SellContractSize = openInterestSummary.SellContractSize;
            this._NetLot = openInterestSummary.NetLot;
            this._NetAvgPrice = openInterestSummary.NetAvgPrice;
            this._NetContractSize = openInterestSummary.NetContractSize;

            this._BuyLotMultiplyAvgPriceSum = openInterestSummary.BuyLot * decimal.Parse(openInterestSummary.BuyAvgPrice);
            this._SellLotMultiplyAvgPriceSum = openInterestSummary.SellLot * decimal.Parse(openInterestSummary.SellAvgPrice);
            this._NetLotMultiplyAvgPriceSum = openInterestSummary.NetLot * decimal.Parse(openInterestSummary.NetAvgPrice);

            this.CreateEmptySummary();
        }

        internal void UpdateGroupFromAccountItem(OpenInterestSummary accountSumamryItem)
        {
            this._InstrumentId = accountSumamryItem.InstrumentId;
            this._GroupId = accountSumamryItem.GroupId;
            this._GroupCode = accountSumamryItem.GroupCode;
            this._MinNumeratorUnit = accountSumamryItem._MinNumeratorUnit;
            this._MaxDenominator = accountSumamryItem._MaxDenominator;
        }

        internal void UpdateOrderFromAccountItem(OpenInterestSummary accountSumamryItem)
        {
            this._GroupId = accountSumamryItem.GroupId;
            this._GroupCode = accountSumamryItem.GroupCode;
            this._MinNumeratorUnit = accountSumamryItem._MinNumeratorUnit;
            this._MaxDenominator = accountSumamryItem._MaxDenominator;
        }

        internal void Exclude()
        {
            this.BuyLot = decimal.Zero;
            this._BuyLotMultiplyAvgPriceSum = decimal.Zero;
            this.BuyAvgPrice = "-";
            this.BuyContractSize = decimal.Zero;
            this.SellLot = decimal.Zero;
            this._SellLotMultiplyAvgPriceSum = decimal.Zero;
            this.SellAvgPrice = "-";
            this.SellContractSize = decimal.Zero;
            this.NetLot = decimal.Zero;
            this._NetLotMultiplyAvgPriceSum = decimal.Zero;
            this.NetAvgPrice = "-";
            this.NetContractSize = decimal.Zero;
        }

        internal void SetItem(OpenInterestSummary childItem, bool inCrease)
        {
            if (inCrease)
            {
                this.BuyLot += childItem.BuyLot;
                this._BuyLotMultiplyAvgPriceSum += childItem.BuyLot * decimal.Parse(childItem.BuyAvgPrice);
                this.BuyContractSize += childItem.BuyContractSize;
                this.SellLot += childItem._SellLot;
                this._SellLotMultiplyAvgPriceSum += childItem.SellLot * decimal.Parse(childItem.SellAvgPrice);
                this.SellContractSize += childItem.SellContractSize;
                this.NetLot += childItem.NetLot;
                this._NetLotMultiplyAvgPriceSum += childItem.NetLot * decimal.Parse(childItem.NetAvgPrice);
                this.NetContractSize += childItem.NetContractSize;
            }
            else
            {
                this.BuyLot -= childItem.BuyLot;
                this._BuyLotMultiplyAvgPriceSum -= childItem.BuyLot * decimal.Parse(childItem.BuyAvgPrice);
                this.BuyContractSize -= childItem.BuyContractSize;
                this.SellLot -= childItem._SellLot;
                this._SellLotMultiplyAvgPriceSum -= childItem.SellLot * decimal.Parse(childItem.SellAvgPrice);
                this.SellContractSize -= childItem.SellContractSize;
                this.NetLot -= childItem.NetLot;
                this._NetLotMultiplyAvgPriceSum -= childItem.NetLot * decimal.Parse(childItem.NetAvgPrice);
                this.NetContractSize -= childItem.NetContractSize;
            }
        }

        internal void SetAvgPrice()
        {
            Price price = null;
            decimal avgBuyPriceValue = this._BuyLot != decimal.Zero ? this._BuyLotMultiplyAvgPriceSum / this._BuyLot : decimal.Zero;
            price = new Price(avgBuyPriceValue.ToString(), this._MinNumeratorUnit, this._MaxDenominator);
            this.BuyAvgPrice = price == null ? "0" : price.ToString();

            decimal avgSellPriceValue = this._SellLot != decimal.Zero ? this._SellLotMultiplyAvgPriceSum / this._SellLot : decimal.Zero;
            price = new Price(avgSellPriceValue.ToString(), this._MinNumeratorUnit, this._MaxDenominator);
            this.SellAvgPrice = price == null ? "0" : price.ToString();

            decimal avgNetPriceValue = this._NetLot != decimal.Zero ? this._NetLotMultiplyAvgPriceSum / this._NetLot : decimal.Zero;
            price = new Price(avgNetPriceValue.ToString(), this._MinNumeratorUnit, this._MaxDenominator);
            this.NetAvgPrice = price == null ? "0" : price.ToString();
        }
    }
}
