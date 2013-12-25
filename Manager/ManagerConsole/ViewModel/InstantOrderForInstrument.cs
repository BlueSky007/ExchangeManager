using ManagerConsole.Helper;
using ManagerConsole.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ManagerConsole.ViewModel
{
    public class InstantOrderForInstrument:PropertyChangedNotifier
    {
        #region Privete Property
        private Guid _OrderId;
        private InstrumentClient _Instrument;
        private string _InstrumentCode;
        private string _AccountCode;
        private decimal _Lot = decimal.Zero;
        private decimal _SumBuyLot = decimal.Zero;
        private decimal _SumSellLot = decimal.Zero;
        private int _BuyOrderCount;
        private int _SellOrderCount;
        private BuySell _BuySell;
        private string _Ask;
        private string _Bid;
        private string _CustomerPrice; //客定价
        private string _OpenAvgPrice;
        
        private int _BuyVariation;
        private int _SellVariation;
        private CellDataDefine _ExecuteAllCellDataDefine;
        #endregion

        public InstantOrderForInstrument()
        {
            this._Instrument = new InstrumentClient();
        }

        #region Public Property
        public Guid OrderId
        {
            get { return this._OrderId; }
            set { this._OrderId = value; this.OnPropertyChanged("OrderId"); }
        }

        public InstrumentClient Instrument
        {
            get { return this._Instrument; }
            set { this._Instrument = value; this.OnPropertyChanged("Instrument"); }
        }

        public string InstrumentCode
        {
            get { return this._InstrumentCode; }
            set { this._InstrumentCode = value; this.OnPropertyChanged("InstrumentCode"); }
        }

        public string AccountCode
        {
            get { return this._AccountCode; }
            set { this._AccountCode = value; this.OnPropertyChanged("AccountCode"); }
        }

        public decimal Lot
        {
            get { return this._Lot; }
            set { this._Lot = value; this.OnPropertyChanged("Lot"); }
        }

        public decimal SumBuyLot
        {
            get { return this._SumBuyLot; }
            set { this._SumBuyLot = value; this.OnPropertyChanged("SumBuyLot"); }
        }

        public decimal SumSellLot
        {
            get { return this._SumSellLot; }
            set { this._SumSellLot = value; this.OnPropertyChanged("SumSellLot"); }
        }

        public int BuyOrderCount
        {
            get { return this._BuyOrderCount; }
            set { this._BuyOrderCount = value; this.OnPropertyChanged("BuyOrderCount"); }
        }

        public int SellOrderCount
        {
            get { return this._SellOrderCount; }
            set { this._SellOrderCount = value; this.OnPropertyChanged("SellOrderCount"); }
        }

        public BuySell BuySell
        {
            get { return this._BuySell; }
            set { this._BuySell = value; this.OnPropertyChanged("BuySell"); }
        }

        public string Ask
        {
            get { return this._Ask; }
            set { this._Ask = value; this.OnPropertyChanged("Ask"); }
        }

        public string Bid
        {
            get { return this._Bid; }
            set { this._Bid = value; this.OnPropertyChanged("Bid"); }
        }

        public string CustomerPrice
        {
            get { return this._CustomerPrice; }
            set { this._CustomerPrice = value; this.OnPropertyChanged("CustomerPrice"); }
        }

        public string OpenAvgPrice
        {
            get { return this._OpenAvgPrice; }
            set { this._OpenAvgPrice = value; this.OnPropertyChanged("OpenAvgPrice"); }
        }

        public int BuyVariation
        {
            get { return this._BuyVariation; }
            set { this._BuyVariation = value;this.OnPropertyChanged("BuyVariation");}
        }

        public int SellVariation
        {
            get { return this._SellVariation; }
            set { this._SellVariation = value; this.OnPropertyChanged("SellVariation"); }
        }

        public CellDataDefine ExecuteAllCellDataDefine
        {
            get { return this._ExecuteAllCellDataDefine; }
            set { this._ExecuteAllCellDataDefine = value; }
        }

        #endregion

        internal void Update(OrderTask orderTask)
        {
            this.OrderId = orderTask.OrderId;
            this.Instrument = orderTask.Instrument;
            this.InstrumentCode = this._Instrument.Code;
            this.Ask = orderTask.Instrument.Ask;
            this.Bid = orderTask.Instrument.Bid;
            this.AccountCode = orderTask.AccountCode;
            this.CustomerPrice = orderTask.SetPrice;
            this.BuySell = orderTask.IsBuy;
            this.Lot = orderTask.Lot.Value;
            
            this.OpenAvgPrice = "1.568";
        }

        internal void UpdateSumBuySellLot(bool isAdd,OrderTask orderTask)
        {
            bool isBuy = orderTask.IsBuy == BuySell.Buy;
            if (isAdd)
            {
                if (isBuy)
                {
                    this.SumBuyLot += orderTask.Lot.Value;
                    this.BuyOrderCount++;
                }
                else
                {
                    this.SumSellLot += orderTask.Lot.Value;
                    this.SellOrderCount++;
                }
            }
            else
            {
                if (isBuy)
                {
                    this.SumBuyLot -= orderTask.Lot.Value;
                    this.BuyOrderCount++;
                }
                else
                {
                    this.SumSellLot -= orderTask.Lot.Value;
                    this.SellOrderCount++;
                }
            }
        }
    }
}
