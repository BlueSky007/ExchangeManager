using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ManagerConsole.Helper
{
    public class MessageHelper
    {
        private static MessageHelper _Instance = new MessageHelper();
        public static MessageHelper Instance
        {
            get
            {
                return MessageHelper._Instance;
            }
        }
        public string NoLinkedServer { get; set; }
        public string RiskMonitorDelete{get;set;}
        public string AccountResetFailed{get;set;}
        public string DealerCanceled{get;set;}
        public string RejectDQByDealer{get;set;}
        public string NecessaryIsNotWithinThreshold{get;set;}
        public string MarginIsNotEnough{get;set;}
        public string AccountIsNotTrading{get;set;}
        public string InstrumentIsNotAccepting{get;set;}
        public string TimingIsNotAcceptable{get;set;}
        public string OrderTypeIsNotAcceptable{get;set;}
        public string HasNoAccountsLocked{get;set;}
        public string IsLockedByAgent{get;set;}
        public string IsNotLockedByAgent{get;set;}
        public string InvalidPrice{get;set;}
        public string LossExecutedOrderInOco{get;set;}
        public string ExceedOpenLotBalance{get;set;}
        public string OneCancelOtherPrompt{get;set;}
        public string HasUnassignedOvernightOrders{get;set;}
        public string CustomerCanceled{get;set;}
        public string DbOperationFailed{get;set;}
        public string TransactionAlreadyExists{get;set;}
        public string HasNoOrders{get;set;}
        public string InvalidRelation{get;set;}
        public string InvalidLotBalance{get;set;}
        public string ExceedAssigningLotBalance{get;set;}
        public string OrderLotExceedMaxLot{get;set;}
        public string OpenOrderNotExists{get;set;}
        public string AssigningOrderNotExists{get;set;}
        public string TransactionNotExists{get;set;}
        public string TransactionCannotBeCanceled{get;set;}
        public string TransactionCannotBeExecuted{get;set;}
        public string OrderCannotBeDeleted{get;set;}
        public string IsNotAccountOwner{get;set;}
        public string InvalidOrderRelation{get;set;}
        public string TradePolicyIsNotActive{get;set;}
        public string SetPriceTooCloseToMarket{get;set;}
        public string HasNoQuotationExists{get;set;}
        public string AccountIsInAlerting{get;set;}
 
        public MessageHelper()
        {
            this.NoLinkedServer = "Not connect the server!";
            this.RiskMonitorDelete = "The system has deleted the order. If you find query, please contact our Customer Service!";
            this.AccountResetFailed = "The system has failed to prepare the account for coming trade day. Please contact the system administrator";
            this.DealerCanceled = "The order has been cancelled by the Trading Desk!";
            this.RejectDQByDealer = "Price time out, please try again!";
            this.NecessaryIsNotWithinThreshold = "The trading quantity exceeds the limit allowed for the account, order was not accepted!";
            this.MarginIsNotEnough = "Account's usable margin is not sufficient, order was not accepted!";
            this.AccountIsNotTrading = "Account allowed for trading is not available, please contact our Customer Service for details!";
            this.InstrumentIsNotAccepting = "The instrument is not available for trading, please contact our Customer Service for details!";
            this.TimingIsNotAcceptable = "The order is out of the trading time accepted, order was cancelled!";
            this.OrderTypeIsNotAcceptable = "The select order type is not available for the instrument!";
            this.HasNoAccountsLocked = "The agent account has not been granted with control over the other accounts, please select appropriate accounts and try later!";
            this.IsLockedByAgent = "The account has been occupied by its Agent!";
            this.IsNotLockedByAgent = "The trading account has not been controlled by Agent!";
            this.InvalidPrice = "Price is not in a valid format, please try later!";
            this.LossExecutedOrderInOco = "The other end of the OCO has not been found!";
            this.ExceedOpenLotBalance = "Close order quantity exceeds the quantity of the open order";
            this.OneCancelOtherPrompt = "This is a cancel OCO order.";
            this.HasUnassignedOvernightOrders = "There are orders in the agent account, please assign the order to appropriate accounts!";
            this.CustomerCanceled = "The order has been cancelled by the Customer";
            this.DbOperationFailed = "Unexpected error has occured when locating the account's information";
            this.TransactionAlreadyExists = "Unexpected error has occured when locating the order";
            this.HasNoOrders = "The order does not exist";
            this.InvalidRelation = "Unexpected error has occured when locating the corresponding open order";
            this.InvalidLotBalance = "Unexpected error has occured with the open trading quantity";
            this.ExceedAssigningLotBalance = "The order has exceeded the quantity of the assigning order";
            this.OrderLotExceedMaxLot = "The order has exceeded the permitted trading quantity of the account";
            this.OpenOrderNotExists = "The corresponding open order does not exist, order cancelled.";
            this.AssigningOrderNotExists = "The assigning order does not exist any more, order cancelled";
            this.TransactionNotExists = "Unexpected error for the transaction has occured";
            this.TransactionCannotBeCanceled = "The order cannot be cancelled";
            this.TransactionCannotBeExecuted = "Unexpected error occurred, failed to executed the order";
            this.OrderCannotBeDeleted = "The order cannot be cancelled";
            this.IsNotAccountOwner = "User is not authorized to trade for the account";
            this.InvalidOrderRelation = "The system fails to execute the order due to the missing of open order";
            this.TradePolicyIsNotActive = "The trading policy for the account has been inactivated, please contact our Customer Service for details!";
            this.SetPriceTooCloseToMarket = "The order was rejected because being too close to the market!";
            this.HasNoQuotationExists = "There is no price available for the execution, please try again";
            this.AccountIsInAlerting = "The Account is in margin call position, the order was rejected";
        }

        public string GetMessageForOrder(string errorCode)
        {
            string message = string.Empty;
            switch (errorCode)
            {
                case "NoLinkedServer":
                    message = this.NoLinkedServer;
                    break;
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
                default:
                    message = "Error";
                    break;
            }

            return message;
        }
    }
}
