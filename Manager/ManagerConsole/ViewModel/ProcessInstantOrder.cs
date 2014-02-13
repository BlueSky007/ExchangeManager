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
    public class ProcessInstantOrder : PropertyChangedNotifier
    {
        public delegate void SettingFirstRowStyleHandle();
        public event SettingFirstRowStyleHandle OnSettingFirstRowStyleEvent;
        #region Privete Property
        private ObservableCollection<OrderTask> _OrderTasks;
        private InstantOrderForInstrument _InstantOrderForInstrument;
        #endregion

        public ProcessInstantOrder()
        {
            this._OrderTasks = new ObservableCollection<OrderTask>();
            this._InstantOrderForInstrument = new InstantOrderForInstrument();
        }

        #region Public Property
        public InstantOrderForInstrument InstantOrderForInstrument
        {
            get { return this._InstantOrderForInstrument; }
            set { this._InstantOrderForInstrument = value; }
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
        public void AddInstanceOrder(OrderTask orderTask)
        {
            this.OrderTasks.Add(orderTask);
            orderTask.SetCellDataDefine(orderTask.OrderStatus);

            if (this.InstantOrderForInstrument.Instrument.Id == Guid.Empty)
            {
                this.InstantOrderForInstrument.Update(orderTask);
            }
            else
            {
                bool isCurrentInstrument = orderTask.InstrumentId == this.InstantOrderForInstrument.Instrument.Id;
            }

            this.InstantOrderForInstrument.UpdateSumBuySellLot(true, orderTask);
            if (this.OnSettingFirstRowStyleEvent != null && this.SelectedInstrumentId != null && (orderTask.InstrumentId == this.SelectedInstrumentId))
            {
                this.OnSettingFirstRowStyleEvent();
            }
        }

        public void AdjustPrice(bool upOrDown)
        {
            if (this._InstantOrderForInstrument == null) return;

            this._InstantOrderForInstrument.AdjustCustomerPrice(upOrDown);
        }

        public void AdjustAutoPointVariation(bool isBuy,bool upOrDown)
        {
            if (this._InstantOrderForInstrument == null) return;

            this._InstantOrderForInstrument.AdjustAutoPointVariation(isBuy,upOrDown);
        }

        public void RemoveInstanceOrder(OrderTask orderTask)
        {
            this.OrderTasks.Remove(orderTask);
            if (this.OrderTasks.Count <= 0)
            {
                this.InstantOrderForInstrument.CreateEmptyEntity();
                return;
            }
            this.InstantOrderForInstrument.UpdateSumBuySellLot(false, orderTask);

            this.UpdateFirstOrder();
        }

        public void RemoveInstanceOrder(List<OrderTask> orderTasks)
        {
            if (orderTasks.Count <= 0) return;
            foreach (OrderTask order in orderTasks)
            {
                this.OrderTasks.Remove(order);
                if (this.OrderTasks.Count <= 0)
                {
                    this.InstantOrderForInstrument.CreateEmptyEntity();
                    return;
                }
                this.InstantOrderForInstrument.UpdateSumBuySellLot(false, order);
            }

            this.UpdateFirstOrder();
        }

        private void UpdateFirstOrder()
        {
            if (this.OrderTasks.Count <= 0) return;
            OrderTask currentOrder = this.OrderTasks[0];
            this.InstantOrderForInstrument.Update(currentOrder);

            if (this.OnSettingFirstRowStyleEvent != null)
            {
                this.OnSettingFirstRowStyleEvent();
            }
        }

        public void InitializeBinding(Guid instrumentId)
        {
            this.SelectedInstrumentId = instrumentId;
            List<OrderTask> bindingOrders = (this._OrderTasks.Where(P => P.InstrumentId == instrumentId)).ToList();
            this.InstantOrderForInstrument = new InstantOrderForInstrument();
            if (bindingOrders.Count() <= 0) return;

            this.InstantOrderForInstrument.Update(bindingOrders[0]);

            foreach (OrderTask order in bindingOrders)
            {
                this.InstantOrderForInstrument.UpdateSumBuySellLot(true, order);
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
