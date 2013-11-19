using ManagerConsole.Helper;
using ManagerConsole.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace ManagerConsole.ViewModel
{
    public class LMTProcessModel
    {
        public ObservableCollection<LMTProcessForInstrument> LMTProcessForInstruments { get; set; }

        public LMTProcessModel()
        {
            this.LMTProcessForInstruments = new ObservableCollection<LMTProcessForInstrument>();
        }
    }

    public class LMTProcessForInstrument : PropertyChangedNotifier
    {
        public delegate void OnEmptyLmtOrderTaskHandle(LMTProcessForInstrument lMTProcessForInstrument);
        public event OnEmptyLmtOrderTaskHandle OnEmptyLmtOrderTask;

        public LMTProcessForInstrument()
        {
            this._OrderTasks = new ObservableCollection<OrderTask>();
            this._Instrument = new InstrumentClient();
        }

        #region Privete Property
        private InstrumentClient _Instrument;
        private ObservableCollection<OrderTask> _OrderTasks;
        private string _Origin;
        private decimal _SumSellLot;
        private decimal _SumBuyLot;
        private string _AskPrice;
        private string _BidPrice;

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

        public string AskPrice
        {
            get { return this.Instrument.Ask; }
            set { this._AskPrice = value; }
        }

        public string BidPrice
        {
            get { return this.Instrument.Bid; }
            set { this._BidPrice = value; }
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

        //Apply Price
        public void ApplyPrice(string newAsk, string newBid)
        {
            foreach (OrderTask order in this._OrderTasks)
            {
                if (order.IsBuy == BuySell.Buy)
                {
                    order.Instrument.Ask = newAsk;
                }
                else
                {
                    order.Instrument.Bid = newBid;
                }
            }
        }

        private bool IsProblematicPrice()
        {
            return true;
        }


    }
}
