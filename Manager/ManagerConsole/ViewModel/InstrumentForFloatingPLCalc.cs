using ManagerConsole.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonInstrumentForFloatingPLCalc = Manager.Common.ReportEntities.InstrumentForFloatingPLCalc;

namespace ManagerConsole.ViewModel
{
    public class InstrumentForFloatingPLCalc : PropertyChangedNotifier
    {
        private string _Ask;
        private string _Bid;
        private int _SpreadPoint;
        public InstrumentForFloatingPLCalc(CommonInstrumentForFloatingPLCalc commonInstrumentForPL)
        {
            this.InstrumentId = commonInstrumentForPL.InstrumentId;
            this.InstrumentCode = commonInstrumentForPL.InstrumentCode;
            this.Bid = commonInstrumentForPL.Bid;
            this.Ask = commonInstrumentForPL.Ask;
            this.SpreadPoint = commonInstrumentForPL.SpreadPoint;
        }

        public Guid InstrumentId { get; set; }
        public string InstrumentCode { get; set; }
        public string Bid
        {
            get { return this._Bid; }
            set { this._Bid = value; this.OnPropertyChanged("Bid"); }
        }
        public string Ask
        {
            get { return this._Ask; }
            set { this._Ask = value; this.OnPropertyChanged("Ask"); }
        }
        public int SpreadPoint
        {
            get { return this._SpreadPoint; }
            set { this._SpreadPoint = value; this.OnPropertyChanged("SpreadPoint"); }
        }
    }
}
