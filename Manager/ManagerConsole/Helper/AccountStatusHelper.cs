using ManagerConsole.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AccountStatusItemType = Manager.Common.AccountStatusItemType;
using CommonHelper = Manager.Common.CommonHelper;
using AccountCurrency = Manager.Common.ExchangeEntities.AccountCurrency;

namespace ManagerConsole.Helper
{
    public static class AccountStatusHelper
    {
        public static string GetValue(this AccountStatusModel accountInfor, AccountStatusItemType type)
        {
            switch (type)
            {
                case AccountStatusItemType.TelephonePin:
                    return accountInfor.AccountStatusEntity.AccountDescription;
                case AccountStatusItemType.Necessary:
                    return CommonHelper.Format(accountInfor.AccountTradingSummary.Necessary, 2);
                case AccountStatusItemType.ONNecessary:
                    return CommonHelper.Format(accountInfor.AccountTradingSummary.OverNightNecessary, 2);
                case AccountStatusItemType.NotValue:
                    return CommonHelper.Format(accountInfor.AccountTradingSummary.NotValue, 2);
                case AccountStatusItemType.Usable:
                    return CommonHelper.Format(accountInfor.AccountTradingSummary.Usable, 2);
                case AccountStatusItemType.ONUsable:
                    return CommonHelper.Format(accountInfor.AccountTradingSummary.OverNightUsable, 2);
                case AccountStatusItemType.Balance:
                    return CommonHelper.Format(accountInfor.AccountTradingSummary.Balance, 2);
                case AccountStatusItemType.Floating:
                    return CommonHelper.Format(accountInfor.AccountTradingSummary.Floating, 2);
                case AccountStatusItemType.Deposit:
                    return CommonHelper.Format(accountInfor.AccountTradingSummary.Deposit, 2);
                case AccountStatusItemType.Equity:
                    return CommonHelper.Format(accountInfor.AccountTradingSummary.Equity, 2);
                case AccountStatusItemType.Ratio:
                    return CommonHelper.Format(accountInfor.AccountTradingSummary.Ratio, 2);
                case AccountStatusItemType.Adjustment:
                    return CommonHelper.Format(accountInfor.AccountTradingSummary.Adjustment, 2);
                default:
                    throw new NotSupportedException(string.Format("{0}", type));
            }
        }

        public static string GetValue(this AccountCurrency accountCurrency, AccountStatusItemType type)
        {
            switch (type)
            {
                case AccountStatusItemType.Necessary:
                    return CommonHelper.Format(accountCurrency.CurrencyTradingSummary.Necessary, 2);
                case AccountStatusItemType.ONNecessary:
                    return CommonHelper.Format(accountCurrency.CurrencyTradingSummary.OverNightNecessary, 2);
                case AccountStatusItemType.NotValue:
                    return CommonHelper.Format(accountCurrency.CurrencyTradingSummary.NotValue, 2);
                case AccountStatusItemType.Usable:
                    return CommonHelper.Format(accountCurrency.CurrencyTradingSummary.Usable, 2);
                case AccountStatusItemType.ONUsable:
                    return CommonHelper.Format(accountCurrency.CurrencyTradingSummary.OverNightUsable, 2);
                case AccountStatusItemType.Balance:
                    return CommonHelper.Format(accountCurrency.CurrencyTradingSummary.Balance, 2);
                case AccountStatusItemType.Floating:
                    return CommonHelper.Format(accountCurrency.CurrencyTradingSummary.Floating, 2);
                case AccountStatusItemType.Deposit:
                    return CommonHelper.Format(accountCurrency.CurrencyTradingSummary.Deposit, 2);
                case AccountStatusItemType.Equity:
                    return CommonHelper.Format(accountCurrency.CurrencyTradingSummary.Equity, 2);
                case AccountStatusItemType.Ratio:
                    return CommonHelper.Format(accountCurrency.CurrencyTradingSummary.Ratio, 2);
                case AccountStatusItemType.Adjustment:
                    return CommonHelper.Format(accountCurrency.CurrencyTradingSummary.Adjustment, 2);
                default:
                    throw new NotSupportedException(string.Format("{0}", type));
            }
        }

        public static string GetCaption(this AccountStatusItemType type)
        {
            switch (type)
            {
                case AccountStatusItemType.TelephonePin:
                    return "Telephone Pin";
                case AccountStatusItemType.Necessary:
                    return "Necessary";
                case AccountStatusItemType.ONNecessary:
                    return "ON Necessary";
                case AccountStatusItemType.NotValue:
                    return "Not Valued Bal";
                case AccountStatusItemType.Usable:
                    return "Usable";
                case AccountStatusItemType.ONUsable:
                    return "ON Usable";
                case AccountStatusItemType.Balance:
                    return "Balance";
                case AccountStatusItemType.Floating:
                    return "Floating";
                case AccountStatusItemType.Deposit:
                    return "Deposit";
                case AccountStatusItemType.Equity:
                    return "Equity";
                case AccountStatusItemType.Ratio:
                    return "Ratio";
                case AccountStatusItemType.Adjustment:
                    return "Adjustment";
                default:
                    throw new NotSupportedException(string.Format("{0}", type));
            }
        }

        public static bool HasDetail(this AccountStatusItemType type)
        {
            return type == AccountStatusItemType.Necessary
                || type == AccountStatusItemType.ONNecessary
                || type == AccountStatusItemType.NotValue
                || type == AccountStatusItemType.Usable
                || type == AccountStatusItemType.ONUsable
                || type == AccountStatusItemType.Balance
                || type == AccountStatusItemType.Floating
                || type == AccountStatusItemType.Deposit
                || type == AccountStatusItemType.Equity
                || type == AccountStatusItemType.Adjustment;
        }
    }
}
