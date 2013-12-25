using ManagerConsole.Helper;
using ManagerConsole.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

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

            if (this.SelectedInstrumentId != null && this.SelectedInstrumentId != orderTask.InstrumentId) return;

            if (this.InstantOrderForInstrument.Instrument.Id == Guid.Empty)
            {
                this.InstantOrderForInstrument.Update(orderTask);
            }
            else
            {
                bool isCurrentInstrument = orderTask.InstrumentId == this.InstantOrderForInstrument.Instrument.Id;
            }

            this.InstantOrderForInstrument.UpdateSumBuySellLot(true, orderTask);
            if (this.OnSettingFirstRowStyleEvent != null)
            {
                this.OnSettingFirstRowStyleEvent();
            }
        }

        public void RemoveInstanceOrder(List<OrderTask> orderTasks)
        {
            foreach (OrderTask order in orderTasks)
            {
                this.OrderTasks.Remove(order);
                this.InstantOrderForInstrument.UpdateSumBuySellLot(false, order);
            }

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
            if (bindingOrders.Count() <= 0) return;

            this.InstantOrderForInstrument = new InstantOrderForInstrument();
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
    }
}
