using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ManagerConsole
{
    public class Message
    {
        private string RiskMonitorDelete = "The system has deleted the order. If you find query, please contact our Customer Service!";
        private string AccountResetFailed = "The system has failed to prepare the account for coming trade day. Please contact the system administrator";
        private string DealerCanceled = "The order has been cancelled by the Trading Desk!";
        private string RejectDQByDealer = "Price time out, please try again!";
        private string NecessaryIsNotWithinThreshold = "The trading quantity exceeds the limit allowed for the account, order was not accepted!";
        private string MarginIsNotEnough = "Account's usable margin is not sufficient, order was not accepted!";
        private string AccountIsNotTrading = "Account allowed for trading is not available, please contact our Customer Service for details!";
        private string InstrumentIsNotAccepting = "The instrument is not available for trading, please contact our Customer Service for details!";
        private string TimingIsNotAcceptable = "The order is out of the trading time accepted, order was cancelled!";
        private string OrderTypeIsNotAcceptable = "The select order type is not available for the instrument!";
        private string HasNoAccountsLocked = "The agent account has not been granted with control over the other accounts, please select appropriate accounts and try later!";
        private string IsLockedByAgent = "The account has been occupied by its Agent!";
        private string IsNotLockedByAgent = "The trading account has not been controlled by Agent!";
        private string InvalidPrice = "Price is not in a valid format, please try later!";
        private string LossExecutedOrderInOco = "The other end of the OCO has not been found!";
        private string ExceedOpenLotBalance = "Close order quantity exceeds the quantity of the open order";
        private string OneCancelOtherPrompt = "This is a cancel OCO order.";
        private string HasUnassignedOvernightOrders = "There are orders in the agent account, please assign the order to appropriate accounts!";
        private string CustomerCanceled = "The order has been cancelled by the Customer";
        private string DbOperationFailed = "Unexpected error has occured when locating the account's information";
        private string TransactionAlreadyExists = "Unexpected error has occured when locating the order";
        private string HasNoOrders = "The order does not exist";
        private string InvalidRelation = "Unexpected error has occured when locating the corresponding open order";
        private string InvalidLotBalance = "Unexpected error has occured with the open trading quantity";
        private string ExceedAssigningLotBalance = "The order has exceeded the quantity of the assigning order";
        private string OrderLotExceedMaxLot = "The order has exceeded the permitted trading quantity of the account";
        private string OpenOrderNotExists = "The corresponding open order does not exist, order cancelled.";
        private string AssigningOrderNotExists = "The assigning order does not exist any more, order cancelled";
        private string TransactionNotExists = "Unexpected error for the transaction has occured";
        private string TransactionCannotBeCanceled = "The order cannot be cancelled";
        private string TransactionCannotBeExecuted = "Unexpected error occurred, failed to executed the order";
        private string OrderCannotBeDeleted = "The order cannot be cancelled";
        private string IsNotAccountOwner = "User is not authorized to trade for the account";
        private string InvalidOrderRelation = "The system fails to execute the order due to the missing of open order";
        private string TradePolicyIsNotActive = "The trading policy for the account has been inactivated, please contact our Customer Service for details!";
        private string SetPriceTooCloseToMarket = "The order was rejected because being too close to the market!";
        private string HasNoQuotationExists = "There is no price available for the execution, please try again";
        private string AccountIsInAlerting = "The Account is in margin call position, the order was rejected";


        public  string GetMessageForOrder(string errorCode)
        {
            var message = errorCode;
            switch (errorCode)
            {
                case "RiskMonitorDelete":
                    message = this.RiskMonitorDelete;
                    break;
                case "AccountResetFailed":
                    message = this.AccountResetFailed;
                    break;
                case "DealerCanceled":
                    message = this.DealerCanceled;
                    break;
                case "RejectDQByDealer":
                    message = this.RejectDQByDealer;
                    break;
                case "NecessaryIsNotWithinThreshold":
                    message = this.NecessaryIsNotWithinThreshold;
                    break;
                case "MarginIsNotEnough":
                    message = this.MarginIsNotEnough;
                    break;
                case "AccountIsNotTrading":
                    message = this.AccountIsNotTrading;
                    break;
                case "InstrumentIsNotAccepting":
                    message = this.InstrumentIsNotAccepting;
                    break;
                case "TimingIsNotAcceptable":
                    message = this.TimingIsNotAcceptable;
                    break;
                case "OrderTypeIsNotAcceptable":
                    message = this.OrderTypeIsNotAcceptable;
                    break;
                case "HasNoAccountsLocked":
                    message = this.HasNoAccountsLocked;
                    break;
                case "IsLockedByAgent":
                    message = this.IsLockedByAgent;
                    break;
                case "IsNotLockedByAgent":
                    message = this.IsNotLockedByAgent;
                    break;
                case "InvalidPrice":
                    message = this.InvalidPrice;
                    break;
                case "LossExecutedOrderInOco":
                    message = this.LossExecutedOrderInOco;
                    break;
                case "ExceedOpenLotBalance":
                    message = this.ExceedOpenLotBalance;
                    break;
                case "OneCancelOther":
                    message = this.OneCancelOtherPrompt;
                    break;
                case "HasUnassignedOvernightOrders":
                    message = this.HasUnassignedOvernightOrders;
                    break;
                case "CustomerCanceled":
                    message = this.CustomerCanceled;
                    break;
                case "DbOperationFailed":
                    message = this.DbOperationFailed;
                    break;
                case "TransactionAlreadyExists":
                    message = this.TransactionAlreadyExists;
                    break;
                case "HasNoOrders":
                    message = this.HasNoOrders;
                    break;
                case "InvalidRelation":
                    message = this.InvalidRelation;
                    break;
                case "InvalidLotBalance":
                    message = this.InvalidLotBalance;
                    break;
                case "ExceedAssigningLotBalance":
                    message = this.ExceedAssigningLotBalance;
                    break;
                case "OrderLotExceedMaxLot":
                    message = this.OrderLotExceedMaxLot;
                    break;
                case "OpenOrderNotExists":
                    message = this.OpenOrderNotExists;
                    break;
                case "AssigningOrderNotExists":
                    message = this.AssigningOrderNotExists;
                    break;
                case "TransactionNotExists":
                    message = this.TransactionNotExists;
                    break;
                case "TransactionCannotBeCanceled":
                    message = this.TransactionCannotBeCanceled;
                    break;
                case "TransactionCannotBeExecuted":
                    message = this.TransactionCannotBeExecuted;
                    break;
                case "OrderCannotBeDeleted":
                    message = this.OrderCannotBeDeleted;
                    break;
                case "IsNotAccountOwner":
                    message = this.IsNotAccountOwner;
                    break;
                case "InvalidOrderRelation":
                    message = this.InvalidOrderRelation;
                    break;
                case "TradePolicyIsNotActive":
                    message = this.TradePolicyIsNotActive;
                    break;
                case "SetPriceTooCloseToMarket":
                    message = this.SetPriceTooCloseToMarket;
                    break;
                case "HasNoQuotationExists":
                    message = this.HasNoQuotationExists;
                    break;
                case "AccountIsInAlerting":
                    message = this.AccountIsInAlerting;
                    break;
            }
            return (message);
        }
    }
}
