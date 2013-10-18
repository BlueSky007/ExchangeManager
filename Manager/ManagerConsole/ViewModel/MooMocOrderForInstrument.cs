using ManagerConsole.Helper;
using ManagerConsole.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace ManagerConsole.ViewModel
{
    public class MooMocOrderForInstrumentModel
    {
        public ObservableCollection<MooMocOrderForInstrument> MooMocOrderForInstruments { get; set; }

        public MooMocOrderForInstrumentModel()
        {
            MooMocOrderForInstruments = new ObservableCollection<MooMocOrderForInstrument>();
        }
    }
    public class MooMocOrderForInstrument : PropertyChangedNotifier
    {
        public MooMocOrderForInstrument()
        {
            this._Instrument = new InstrumentClient();
            this._OrderTasks = new ObservableCollection<OrderTask>();
            this.BuySellList = new ObservableCollection<string>() { "All","Buy","Sell"};
            this.OpenCloseList = new ObservableCollection<string> { "All", "Open", "Close" };
        }

        #region Privete Property
        private InstrumentClient _Instrument;
        private ObservableCollection<OrderTask> _OrderTasks;
        private string _Origin;
        private decimal _SumSellLot;
        private decimal _SumBuyLot;
        
        private string _BuySellString = "All";
        private string _OpenCloseString = "All";
        
        private int _Variation;
        public bool _SelectedAll = true;
        #endregion

        #region Public Property
        public InstrumentClient Instrument
        {
            get { return this._Instrument; }
            set { this._Instrument = value; }
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
            set { this._SumBuyLot = value;}
        }

        public decimal SumSellLot
        {
            get { return this._SumSellLot; }
            set { this._SumSellLot = value; }
        }

        public string BuySellString
        {
            get { return this._BuySellString; }
            set { this._BuySellString = value; }
        }

        public string OpenCloseString
        {
            get { return this._OpenCloseString; }
            set { this._OpenCloseString = value; }
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

        

        public int Variation
        {
            get { return this._Variation; }
            set 
            { 
                this._Variation = value; 
                this.OnPropertyChanged("Variation"); 
            }
        }

        public ObservableCollection<OrderTask> OrderTasks
        {
            get { return this._OrderTasks; }
            set { this._OrderTasks = value; }
        }

        public object MooMocHandle
        {
            get;
            set;
        }
        #endregion
    }
}
