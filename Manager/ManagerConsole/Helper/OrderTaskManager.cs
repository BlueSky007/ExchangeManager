using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ManagerConsole.Model;
using ManagerConsole.ViewModel;
using OrderType = iExchange.Common.OrderType;
using Price = iExchange.Common.Price;
using PriceType = iExchange.Common.PriceType;
using ConfigParameter = Manager.Common.Settings.ConfigParameter;
using System.Text.RegularExpressions;
namespace ManagerConsole.Helper
{
    public class OrderTaskManager
    {
        public static bool CheckDQOrder(OrderTask order, SystemParameter parameter, ConfigParameter configParameter)
        {
            bool isOK = false;
            if((order.OrderType == OrderType.SpotTrade) && (order.OrderStatus == OrderStatus.WaitOutPriceDQ || order.OrderStatus == OrderStatus.WaitOutLotDQ))
            {
                if (parameter.AutoConfirmOrder && ((IsNeedDQMaxMove(order) || (configParameter.AllowModifyOrderLot && parameter.CanDealerViewAccountInfo)) == false))
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
                originPrice = lastOriginPrice + (int)adjust;
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

        public static bool IsNeedDQMaxMove(OrderTask orderTask)
        {
            return (orderTask.Transaction.OrderType == OrderType.SpotTrade && orderTask.DQMaxMove > 0);
        }

        public static bool AllowAccept(OrderTask orderTask,QuotePolicyDetail quotePolicyDetail,string origin,int acceptDQVariation)
        {
            //Allow: (isNormal = IsBuy), SetPrice >= Calculated.Quotepolicy.Ask, SetPrice <= Calculated.Quotepolicy.Bid
            InstrumentClient instrument = orderTask.Transaction.Instrument;
            Price ask = null;
            Price bid = null;
            Price marketOriginPrice = new Price(origin, instrument.NumeratorUnit.Value, instrument.Denominator.Value);
            marketOriginPrice = marketOriginPrice + acceptDQVariation;
            if (quotePolicyDetail.PriceType == PriceType.WatchOnly)
            {
                int diffValue = instrument.GetSourceAskBidDiffValue();
                bid = marketOriginPrice;
                ask = bid + diffValue;
            }
            else if (quotePolicyDetail.PriceType == PriceType.OriginEnable)
            {
                bid = marketOriginPrice + quotePolicyDetail.AutoAdjustPoints + (0 - quotePolicyDetail.SpreadPoints);
                var diffValue = instrument.GetSourceAskBidDiffValue();
                ask = bid + (Math.Abs(diffValue)) + (quotePolicyDetail.SpreadPoints);
            }
            else
            {
                bid = marketOriginPrice + (quotePolicyDetail.AutoAdjustPoints);
                ask = bid + (quotePolicyDetail.SpreadPoints);
            }

            Price setPrice = new Price(orderTask.SetPrice, instrument.NumeratorUnit.Value, instrument.Denominator.Value);
            if(instrument.IsNormal == (orderTask.IsBuy == BuySell.Buy))
            {
                if (ask != null)
                {
                    return setPrice > ask;
                }
            }
            else
            {
                if (bid != null)
                {
                    return setPrice < bid;
                }
            }
            return false;
        }
    }
}
