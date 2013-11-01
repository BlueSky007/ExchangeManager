using ManagerConsole.Helper;
using ManagerConsole.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace ManagerConsole.ViewModel
{
    public class OrderTaskModel : PropertyChangedNotifier
    {
        public OrderTaskModel()
        {
            this._OrderTasks = new ObservableCollection<OrderTask>();
            this._Instrument = new InstrumentClient();
            this._TopToolBarImages = new TopToolBarImages();
            this.CreateCellDataDefine();
        }

        #region Privete Property
        private InstrumentClient _Instrument;
        private ObservableCollection<OrderTask> _OrderTasks;

        private string _Origin;
        private int _Variation;
        public bool _SelectedAll = true;
        private bool _IsBuy = true;
        private CellDataDefine _ExecuteAllCellDataDefine;
        public TopToolBarImages _TopToolBarImages { get; set; }
        #endregion

        #region Public Property

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

        public TopToolBarImages TopToolBarImages
        {
            get { return this._TopToolBarImages; }
            set { this._TopToolBarImages = value; }
        }

        public CellDataDefine ExecuteAllCellDataDefine
        {
            get { return this._ExecuteAllCellDataDefine; }
            set { this._ExecuteAllCellDataDefine = value; }
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

        //Remove
        public void RemoveOrderTask(OrderTask orderTask)
        {
            this._OrderTasks.Remove(orderTask);
        }

        #endregion

        internal void CreateCellDataDefine()
        {
            this._ExecuteAllCellDataDefine = new CellDataDefine();
            this._ExecuteAllCellDataDefine = new CellDataDefine();
            this._ExecuteAllCellDataDefine.Action = HandleAction.OnOrderAccept;
            this._ExecuteAllCellDataDefine.IsEnable = true;
            this._ExecuteAllCellDataDefine.IsVisibility = System.Windows.Visibility.Visible;
        }
    }
}
