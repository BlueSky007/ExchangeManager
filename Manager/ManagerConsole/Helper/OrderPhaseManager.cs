using ManagerConsole.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phase = Manager.Common.Phase;
using OrderType = Manager.Common.OrderType;

namespace ManagerConsole.Helper
{
    public class TranPhaseManager
    {
        public static void SetOrderStatus(Transaction transaction,bool existAccount)
        {
            foreach (Order order in transaction.Orders)
            {
                switch (transaction.Phase)
                {
                    case Phase.Placing:
                        if (existAccount)
                        {
                            order.Status = OrderStatus.WaitAcceptRejectPlace;
                        }
                        else
                        {
                            //若Transaction中不存在账户，是否要重新获取
                        }
                        break;
                    case Phase.Placed:
                        if (existAccount)
                        {
                            //order.mainWindow.oPendingOrders.Item(order.id) = order;
                            CheckWhenOrderArrive(true,order);
                            if (order.Status == OrderStatus.WaitServerResponse)
                                return;
                        }

                        CheckWhenOrderArrive(true, order);
                        break;
                    case Phase.Executed:
                        order.Status = OrderStatus.Executed;
                        break;
                    case Phase.Canceled:
                        order.Status = OrderStatus.Canceled;
                        break;
                    case Phase.Deleted:
                        order.Status = OrderStatus.Deleted;
                        break;
                    case Phase.Completed:
                        break;
                }
            }
            
        }

        private static void CheckWhenOrderArrive(bool needPlaySound,Order order)
        {
            if (order.Transaction.OrderType == Manager.Common.OrderType.SpotTrade)
            {
               
                if (IsPriceExceedMaxMin(order.SetPrice) == true)
                {
                    //Waiting for Dealer Accept/Reject
                    order.Status = OrderStatus.WaitOutPriceDQ;
                }
                else
                {
                    //Waiting for Dealer Confirm/Reject
                    order.Status = OrderStatus.WaitOutLotDQ;
                }
            }
            else if (order.Transaction.OrderType == Manager.Common.OrderType.Limit)
            {
                order.Status = OrderStatus.WaitNextPrice;
            }
        }


        private static bool IsPriceExceedMaxMin(string bestPrice)
        {
            return true;
        }

    }

}
