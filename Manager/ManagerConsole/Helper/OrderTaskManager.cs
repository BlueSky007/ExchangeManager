using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ManagerConsole.Model;
using ManagerConsole.ViewModel;
using OrderType = Manager.Common.OrderType;
using Price = Manager.Common.Price;
using System.Text.RegularExpressions;
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

        public static bool CheckExecuteOrder(OrderTask order)
        {
            bool isOK = false;
            if (order.OrderType == OrderType.Limit || order.OrderType == OrderType.Market)
            {
                if (order.OrderStatus == OrderStatus.WaitOutPriceLMT
                    || order.OrderStatus == OrderStatus.WaitOutLotLMT
                    || order.OrderStatus == OrderStatus.WaitOutLotLMTOrigin) //order.tran.subType == 3 缺少这个条件
                {
                    isOK = true;
                }
            }
            return isOK;
        }

        public static bool CheckModifyPrice(OrderTask order, string newPrice)
        {
            bool result = false;

            InstrumentClient instrument = order.Transaction.Instrument;

            
            

            return result;
        }

        public static bool? IsValidPrice(InstrumentClient instrument, decimal adjust)
        {
            Price lastOriginPrice = Price.CreateInstance(instrument.Origin, instrument.NumeratorUnit.Value, instrument.Denominator.Value);
            string validInt = "^-?\\d+$";
            Price originPrice;
            if (Regex.IsMatch(adjust.ToString(), validInt))
            {
                originPrice = Price.Adjust(lastOriginPrice, (int)adjust);
            }
            else
            {
                originPrice = Price.CreateInstance((double)adjust, instrument.NumeratorUnit.Value, instrument.Denominator.Value);
            }
            if (originPrice != null)
            {
                if (lastOriginPrice != null)
                {
                    return (Math.Abs(lastOriginPrice - originPrice) > instrument.AlertPoint);
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return null;
            }
        }

        public static bool IsLimitedPriceByDailyMaxMove(string newPrice,InstrumentClient instrument)
        {
            bool isLimited = false;

            if (!string.IsNullOrEmpty(newPrice) && instrument.DailyMaxMove != 0)
            {
                Price adjustPrice = new Price(newPrice, instrument.NumeratorUnit.Value, instrument.Denominator.Value);
                Price previousClosePrice = new Price(instrument.PreviousClosePrice, instrument.NumeratorUnit.Value, instrument.Denominator.Value);

                if((adjustPrice > (previousClosePrice + instrument.DailyMaxMove)) 
                    || (adjustPrice < (previousClosePrice - instrument.DailyMaxMove)))
                {
                    isLimited = true;
                }
            }
            return isLimited;
        }

        private static bool IsNeedDQMaxMove(OrderTask orderTask)
        {
            return (orderTask.Transaction.OrderType == Manager.Common.OrderType.SpotTrade && orderTask.DQMaxMove > 0);
        }
    }
}
