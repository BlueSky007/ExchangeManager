using Manager.Common;
using ManagerConsole.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace ManagerConsole.Helper
{
    public class LogEntityConvert
    {
        public static LogOrder GetLogOrderEntity(OrderTask orderTask,OperationType operationType,string objectId)
        {
            LogOrder logEntity = new LogOrder();

            string hostname = Dns.GetHostName();
            IPHostEntry localhost = Dns.GetHostEntry(hostname);
            IPAddress localaddr = localhost.AddressList[0];
            logEntity.IP = localaddr.ToString();

            //logEntity.UserId = client.userId;
            //logEntity.UserName = client.user.UserName;
            logEntity.Event = objectId;// "ExecuteOrder";
            logEntity.ExchangeCode = orderTask.ExchangeCode;
            logEntity.OperationType = operationType;
            logEntity.OrderId = orderTask.OrderId;
            logEntity.OrderCode = orderTask.Code;
            logEntity.AccountCode = orderTask.AccountCode;
            logEntity.InstrumentCode = orderTask.InstrumentCode;
            logEntity.IsBuy = orderTask.IsBuy == BuySell.Buy;
            logEntity.IsOpen = orderTask.IsOpen == OpenClose.Open;
            logEntity.Lot = orderTask.Lot.Value;
            logEntity.SetPrice = orderTask.SetPrice;
            logEntity.OrderType = orderTask.OrderType;
            logEntity.OrderRelation = null;
            logEntity.TransactionCode = orderTask.Transaction.Code;

            return logEntity;
        }
    }
}
