using ManagerConsole.Helper;
using ManagerConsole.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Price = iExchange.Common.Price;
using PriceType = iExchange.Common.PriceType;

namespace ManagerConsole.ViewModel
{
    public class ProcessLmtOrder : PropertyChangedNotifier
    {
        public delegate void SettingFirstRowStyleHandle();
        public event SettingFirstRowStyleHandle OnSettingFirstRowStyleEvent;

        private ObservableCollection<OrderTask> _OrderTasks;
        private LmtOrderForInstrument _LmtOrderForInstrument;

        public ProcessLmtOrder()
        {
            this._OrderTasks = new ObservableCollection<OrderTask>();
            this._LmtOrderForInstrument = new LmtOrderForInstrument();
        }

        #region Public Property
        public LmtOrderForInstrument LmtOrderForInstrument
        {
            get { return this._LmtOrderForInstrument; }
            set { this._LmtOrderForInstrument = value; }
        }
        public ObservableCollection<OrderTask> OrderTasks
        {
            get { return this._OrderTasks; }
            set { this._OrderTasks = value; }
        }
        public Guid? SelectedInstrumentId
        {
            get;
            set;
        }

        #endregion

        public void AddLmtOrder(OrderTask orderTask)
        {
            this.OrderTasks.Add(orderTask);
            orderTask.SetCellDataDefine(orderTask.OrderStatus);

            if (this._LmtOrderForInstrument.Instrument.Id == Guid.Empty)
            {
                this.LmtOrderForInstrument.Update(orderTask);
            }
            else
            {
                bool isCurrentInstrument = orderTask.InstrumentId == this.LmtOrderForInstrument.Instrument.Id;
            }

            this.LmtOrderForInstrument.UpdateSumBuySellLot(true, orderTask);
            if (this.OnSettingFirstRowStyleEvent != null && this.SelectedInstrumentId != null && (orderTask.InstrumentId == this.SelectedInstrumentId))
            {
                this.OnSettingFirstRowStyleEvent();
            }
        }

        public void AdjustPrice(bool upOrDown)
        {
            if (this.LmtOrderForInstrument == null) return;

            this.LmtOrderForInstrument.AdjustCustomerPrice(upOrDown);
        }

        public void RemoveInstanceOrder(List<OrderTask> orderTasks)
        {
            if (orderTasks.Count <= 0) return;
            foreach (OrderTask order in orderTasks)
            {
                this.OrderTasks.Remove(order);
                if (this.OrderTasks.Count <= 0)
                {
                    this._LmtOrderForInstrument.CreateEmptyEntity();
                    return;
                }
                this._LmtOrderForInstrument.UpdateSumBuySellLot(false, order);
            }

            OrderTask currentOrder = this.OrderTasks[0];
            this._LmtOrderForInstrument.Update(currentOrder);

            if (this.OnSettingFirstRowStyleEvent != null)
            {
                this.OnSettingFirstRowStyleEvent();
            }
        }

        public void InitializeBinding(Guid instrumentId)
        {
            this.SelectedInstrumentId = instrumentId;
            List<OrderTask> bindingOrders = (this._OrderTasks.Where(P => P.InstrumentId == instrumentId)).ToList();
            this.LmtOrderForInstrument = new LmtOrderForInstrument();
            if (bindingOrders.Count() <= 0) return;

            this.LmtOrderForInstrument.Update(bindingOrders[0]);

            foreach (OrderTask order in bindingOrders)
            {
                this.LmtOrderForInstrument.UpdateSumBuySellLot(true, order);
            }
            
            if (this.OnSettingFirstRowStyleEvent != null)
            {
                this.OnSettingFirstRowStyleEvent();
            }
        }

        public int CheckDQVariation(InstrumentClient instrument,int adjustDQVariation)
        {
            if (instrument.CheckVariation(adjustDQVariation))
            {
                return adjustDQVariation;
            }
            else
            {
                return (0 - instrument.AcceptDQVariation);
            }
        }

        public bool AllowAccept(OrderTask orderTask,QuotePolicyDetail quotePolicyDetail, bool isBuy, string marketPrice, int acceptDQVariation)
        {
            InstrumentClient instrument = orderTask.Transaction.Instrument;

            Price marketPricePrice = Price.CreateInstance(marketPrice, instrument.NumeratorUnit.Value, instrument.Denominator.Value);
            marketPricePrice = marketPricePrice + acceptDQVariation;
            
            if (quotePolicyDetail.PriceType == PriceType.OriginEnable)
            {
                marketPricePrice = marketPricePrice + quotePolicyDetail.AutoAdjustPoints + (0 - quotePolicyDetail.SpreadPoints);
            }
            else
            {
                marketPricePrice = marketPricePrice + (quotePolicyDetail.AutoAdjustPoints);
            }

            Price setPrice = new Price(orderTask.SetPrice, instrument.NumeratorUnit.Value, instrument.Denominator.Value);
            if (instrument.IsNormal == isBuy)
            {
                if (marketPricePrice != null)
                {
                    return setPrice > marketPricePrice;
                }
            }
            else
            {
                if (marketPricePrice != null)
                {
                    return setPrice < marketPricePrice;
                }
            }
            return false;
        }
    }
}
