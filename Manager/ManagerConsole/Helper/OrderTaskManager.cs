using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Manager.Common;
using ManagerConsole.Model;
using ManagerConsole.ViewModel;

namespace ManagerConsole.Helper
{
    public class OrderTaskManager
    {
        public static bool CheckDQOrder(OrderTask order,SystemParameter parameter)
        {
            bool isOK = false;
            if((order.OrderType == OrderType.SpotTrade) && (order.OrderStatus == OrderStatus.WaitOutPriceDQ || order.OrderStatus == OrderStatus.WaitOutLotDQ))
            {
			    if (parameter.AutoConfirmOrder && ((IsNeedDQMaxMove(order) || parameter.CanDealerViewAccountInfo)==false))
                {
			        isOK = true;
                }
			}
            return isOK;
        }

        private static bool IsNeedDQMaxMove(OrderTask orderTask)
        {
            return (orderTask.Transaction.OrderType == OrderType.SpotTrade && orderTask.DQMaxMove > 0);
        }
    }
}
