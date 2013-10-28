using ManagerConsole.Helper;
using ManagerConsole.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace ManagerConsole.ViewModel
{
    public class LmtOrderTaskForInstrumentModel
    {
        public ObservableCollection<LmtOrderTaskForInstrument> LmtOrderTaskForInstruments { get; set; }

        public LmtOrderTaskForInstrumentModel()
        {
            this.LmtOrderTaskForInstruments = new ObservableCollection<LmtOrderTaskForInstrument>();
        }
    }

    public class LmtOrderTaskForInstrument : PropertyChangedNotifier
    {
        public delegate void EmptyLmtOrderHandle(LmtOrderTaskForInstrument lmtOrderTaskForInstrument);
        public event EmptyLmtOrderHandle OnEmptyLmtOrderTask;

        public LmtOrderTaskForInstrument()
        {
            this._OrderTasks = new ObservableCollection<OrderTask>();
        }

        #region Privete Property
        private InstrumentClient _Instrument;
        private ObservableCollection<OrderTask> _OrderTasks;
        private string _Origin;
        private decimal _SumSellLot;
        private decimal _SumBuyLot;
        #endregion

        #region Public Property
        public InstrumentClient Instrument
        {
            get { return this._Instrument; }
            set { this._Instrument = value; }
        }

        public ObservableCollection<OrderTask> OrderTasks
        {
            get { return this._OrderTasks; }
            set { this._OrderTasks = value; }
        }

        public string InstrumentCode
        {
            get { return this._Instrument.Code; }
        }
        public string Origin
        {
            get { return this._Origin; }
            set { this._Origin = value; this.OnPropertyChanged("Origin"); }
        }

        public decimal SumBuyLot
        {
            get { return this._SumBuyLot; }
            set { this._SumBuyLot = value; }
        }

        public decimal SumSellLot
        {
            get { return this._SumSellLot; }
            set { this._SumSellLot = value; }
        }

        
        public object DQHandle
        {
            get;
            set;
        }
        #endregion

        //Remove
        public void RemoveLmtOrderTask(OrderTask orderTask)
        {
            this.OrderTasks.Remove(orderTask);
            if (this._OrderTasks.Count == 0)
            {
                this.OnEmptyLmtOrderTask(this);
            }
        }
    }
}
