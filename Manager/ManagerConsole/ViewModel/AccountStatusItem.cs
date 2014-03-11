using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using AccountStatusItemType = Manager.Common.AccountStatusItemType;
using Currency = Manager.Common.ExchangeEntities.Currency;
using AccountStatusEntity = Manager.Common.ReportEntities.AccountStatusEntity;
using TradingSummary = Manager.Common.ExchangeEntities.TradingSummary;
using AccountCurrency = Manager.Common.ExchangeEntities.AccountCurrency;
using AccountHedgingLevel = Manager.Common.ReportEntities.AccountHedgingLevel;
using AccountOpenPostion = Manager.Common.ReportEntities.AccountStatusOrder;
using ManagerConsole.Helper;
using System.ComponentModel;
using System.Windows.Data;

namespace ManagerConsole.ViewModel
{
    public enum AccountItemPositon
    { 
        Left,
        LeftCenter,
        RightCenter,
        Right,
    }
    public class AccountStatusModel
    {
        public AccountStatusModel()
        {
            this.LeftAccountStatusItems = new ObservableCollection<AccountStatusItem>();
            this.LeftCenterAccountStatusItems = new ObservableCollection<AccountStatusItem>();
            this.RightCenterAccountStatusItems = new ObservableCollection<AccountStatusItem>();
            this.RightAccountStatusItems = new ObservableCollection<AccountStatusItem>();
        }

        public AccountStatusEntity AccountStatusEntity { get; set; }

        public AccountHedgingLevel AccountHedgingLevel { get; set; }
        public List<AccountCurrency> AccountCurrencies { get; set; }
        public List<AccountOpenPostion> AccountOpenPostions { get; set; }

        public ObservableCollection<AccountStatusItem> LeftAccountStatusItems
        {
            get;
            set;
        }
        public ObservableCollection<AccountStatusItem> LeftCenterAccountStatusItems
        {
            get;
            set;
        }
        public ObservableCollection<AccountStatusItem> RightCenterAccountStatusItems
        {
            get;
            set;
        }
        public ObservableCollection<AccountStatusItem> RightAccountStatusItems
        {
            get;
            set;
        }

        public TradingSummary AccountTradingSummary { get; set; }

        public void FillAccountItems()
        {
            this.LeftAccountStatusItems.Add(new AccountStatusItem(AccountStatusItemType.TelephonePin, this.GetValue(AccountStatusItemType.TelephonePin)));
            this.LeftAccountStatusItems.Add(new AccountStatusItem(AccountStatusItemType.Necessary, this.GetValue(AccountStatusItemType.Necessary)));
            this.LeftAccountStatusItems.Add(new AccountStatusItem(AccountStatusItemType.ONNecessary, this.GetValue(AccountStatusItemType.ONNecessary)));
            this.LeftCenterAccountStatusItems.Add(new AccountStatusItem(AccountStatusItemType.NotValue, this.GetValue(AccountStatusItemType.NotValue)));
            this.LeftCenterAccountStatusItems.Add(new AccountStatusItem(AccountStatusItemType.Usable, this.GetValue(AccountStatusItemType.Usable)));
            this.LeftCenterAccountStatusItems.Add(new AccountStatusItem(AccountStatusItemType.ONUsable, this.GetValue(AccountStatusItemType.ONUsable)));
            this.RightCenterAccountStatusItems.Add(new AccountStatusItem(AccountStatusItemType.Balance, this.GetValue(AccountStatusItemType.Balance)));
            this.RightCenterAccountStatusItems.Add(new AccountStatusItem(AccountStatusItemType.Floating, this.GetValue(AccountStatusItemType.Floating)));
            this.RightCenterAccountStatusItems.Add(new AccountStatusItem(AccountStatusItemType.Deposit, this.GetValue(AccountStatusItemType.Deposit)));
            this.RightAccountStatusItems.Add(new AccountStatusItem(AccountStatusItemType.Equity, this.GetValue(AccountStatusItemType.Equity)));
            this.RightAccountStatusItems.Add(new AccountStatusItem(AccountStatusItemType.Ratio, this.GetValue(AccountStatusItemType.Ratio)));
            this.RightAccountStatusItems.Add(new AccountStatusItem(AccountStatusItemType.Adjustment, this.GetValue(AccountStatusItemType.Adjustment)));

            this.SetSubItems();
        }

        public void SetSubItems()
        {
            foreach (AccountStatusItem item in this.LeftAccountStatusItems)
            {
                this.FillAccountCurrencyItem(item);
            }
            foreach (AccountStatusItem item in this.LeftCenterAccountStatusItems)
            {
                this.FillAccountCurrencyItem(item);
            }
            foreach (AccountStatusItem item in this.RightCenterAccountStatusItems)
            {
                this.FillAccountCurrencyItem(item);
            }
            foreach (AccountStatusItem item in this.RightAccountStatusItems)
            {
                this.FillAccountCurrencyItem(item);
            }
        }

        public void FillAccountCurrencyItem(AccountStatusItem item)
        {
            item.Value = this.GetValue(item.Type);
            if (!item.Type.HasDetail() || this.AccountCurrencies.Count == 0) return;

            foreach (AccountCurrency accountCurrency in this.AccountCurrencies)
            {
                SubCurrencyItem subItem = new SubCurrencyItem(accountCurrency.Currency, item.Type);
                subItem.Value = accountCurrency.GetValue(item.Type);
                item.SubItems.Add(subItem);
            }
        }
    }

    public class AccountStatusItem
    {
        private string _Value;

        public AccountStatusItem(AccountStatusItemType type, string value)
        {
            this._Value = value;
            this.Type = type;
            this.SubItems = new List<SubCurrencyItem>();
        }

        public AccountStatusItemType Type { get; private set; }

        public string Name
        {
            get
            {
                return this.Type.GetCaption();
            }
        }
        public string Value
        {
            get { return this._Value; }
            set { this._Value = value; }
        }

        private ICollectionView _SubItemsView = null;
        public ICollectionView SubItemsView
        {
            get
            {
                if (this._SubItemsView == null)
                {
                    CollectionViewSource viewSource = new CollectionViewSource();
                    viewSource.Source = this.SubItems;
                    this._SubItemsView = viewSource.View;
                    this._SubItemsView.Filter = delegate(object item)
                    {
                        decimal dvalue;
                        string value = ((SubCurrencyItem)item).Value;
                        return true;// (((SubCurrencyItem)item).Type == AccountStatusItemType.Adjustment && value != "-") || (decimal.TryParse(value, out dvalue) && dvalue != 0);
                    };
                }
                return this._SubItemsView;
            }
        }

        public List<SubCurrencyItem> SubItems
        {
            get;
            set;
        }
    }

    public class SubCurrencyItem : PropertyChangedNotifier
    {
        private string _Value;
        public SubCurrencyItem(Currency currency, AccountStatusItemType type)
        {
            this.Currency = currency;
            this.Type = type;
        }

        public Currency Currency { get; private set; }
        public string Value
        {
            get { return this._Value; }
            set
            {
                string oldValue = this._Value;
                this._Value = value;
                if (this._Value != oldValue)
                {
                    this.OnPropertyChanged("Value");
                }
            }
        }
        public AccountStatusItemType Type { get; private set; }
    }
}
