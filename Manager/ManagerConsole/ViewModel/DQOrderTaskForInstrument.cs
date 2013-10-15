using ManagerConsole.Helper;
using ManagerConsole.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace ManagerConsole.ViewModel
{
    public class DQOrderTaskForInstrumentModel
    {
        public ObservableCollection<DQOrderTaskForInstrument> DQOrderTaskForInstruments { get; set; }

        public DQOrderTaskForInstrumentModel()
        {
            DQOrderTaskForInstruments = new ObservableCollection<DQOrderTaskForInstrument>();
        }
    }
    public class DQOrderTaskForInstrument : PropertyChangedNotifier
    {
        public DQOrderTaskForInstrument()
        {
            this._Instrument = new InstrumentClient();
            this._OrderTasks = new ObservableCollection<OrderTask>();
            this.BuySellList = new ObservableCollection<string>() { "All","Buy","Sell"};
            this.OpenCloseList = new ObservableCollection<string> { "All", "Open", "Close" };
        }

        #region Privete Property
        private InstrumentClient _Instrument;
        private ObservableCollection<OrderTask> _OrderTasks;
        
        private string _BuySellString = "All";
        private string _IsOpenString = "All";
        private string _Origin;
        private int _Variation;
        public bool _SelectedAll = true;
        private bool _IsBuy = true;
        #endregion

        #region Public Property
        public string BuySellString
        {
            get { return this._BuySellString; }
            set { this._BuySellString = value; }
        }

        public string IsOpenString
        {
            get { return this._IsOpenString; }
            set { this._IsOpenString = value; }
        }

        public ObservableCollection<string> BuySellList
        {
            get;
            set;
        }

        public ObservableCollection<string> OpenCloseList
        {
            get;
            set;
        }
        public bool SelectedAll
        {
            get { return this._SelectedAll; }
            set { this._SelectedAll = value; this.OnPropertyChanged("SelectedAll"); }
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
        public int Variation
        {
            get { return this._Variation; }
            set 
            { 
                this._Variation = value; 
                this.OnPropertyChanged("Variation"); 
            }
        }

        public bool IsBuy
        {
            get { return this._IsBuy; }
            set { this._IsBuy = value; this.OnPropertyChanged("IsBuy"); }
        }

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
        public object DQHandle
        {
            get;
            set;
        }
        #endregion



        #region Function
        public void SelectAllOrderTask(bool isChecked)
        {
            foreach (OrderTask order in this.OrderTasks)
            {
                order.IsSelected = isChecked;
            }
        }

        public void FilterOrderTask(string isbuy, string isOpen)
        {
            switch (isbuy)
            {
                case "All":
                    foreach (OrderTask order in this.OrderTasks)
                    {
                        order.IsSelected = true;
                    }
                    if (isOpen == "Open")
                    {
                        this.Fileter(OpenClose.Open);
                    }
                    else if (isOpen == "Close")
                    {
                        this.Fileter(OpenClose.Close);
                    }
                    break;
                case "Buy":
                    this.Fileter(BuySell.Buy);
                    if (isOpen == "Open")
                    {
                        this.Fileter(OpenClose.Open);
                    }
                    else if (isOpen == "Close")
                    {
                        this.Fileter(OpenClose.Close);
                    }
                    break;
                case "Sell":
                    this.Fileter(BuySell.Sell);
                    if (isOpen == "Open")
                    {
                        this.Fileter(OpenClose.Open);
                    }
                    else if (isOpen == "Close")
                    {
                        this.Fileter(OpenClose.Close);
                    }
                    break;
            }
        }

        private void Fileter(BuySell isBuy)
        {
            foreach (OrderTask order in this.OrderTasks)
            {
                if (order.IsBuy == isBuy)
                {
                    order.IsSelected = true;
                }
                else
                {
                    order.IsSelected = false;
                }
            }
        }
        private void Fileter(OpenClose isOpen)
        {
            var orders = this.OrderTasks.Where(P => P.IsSelected == true);
            foreach (OrderTask order in orders)
            {
                if (order.IsOpen == isOpen)
                {
                    order.IsSelected = true;
                }
                else
                {
                    order.IsSelected = false;
                }
            }
        }
        #endregion
    }
}
