using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Manager.Common;
using ManagerConsole.Helper;

namespace ManagerConsole.ViewModel
{
    public class VmAbnormalQuotation : PropertyChangedNotifier
    {
        private AbnormalQuotationMessage _AbnormalQuotationMessage;
        private int _RemainingSeconds;

        public VmAbnormalQuotation(AbnormalQuotationMessage abnormalQuotationMessage)
        {
            this._AbnormalQuotationMessage = abnormalQuotationMessage;
            this._RemainingSeconds = abnormalQuotationMessage.WaitSeconds;
        }

        public int ConfirmId { get { return this._AbnormalQuotationMessage.ConfirmId; } }
        public int InstrumentId { get { return this._AbnormalQuotationMessage.InstrumentId; } }
        public string InstrumentCode { get { return this._AbnormalQuotationMessage.InstrumentCode; } }
        public string Ask { get { return this._AbnormalQuotationMessage.Ask; } }
        public string Bid { get { return this._AbnormalQuotationMessage.Bid; } }
        public string OutOfRangeType { get { return this._AbnormalQuotationMessage.OutOfRangeType; } }  // "Ask","Bid"
        public int DiffPoints { get { return this._AbnormalQuotationMessage.DiffPoints; } }
        public DateTime Timestamp { get { return this._AbnormalQuotationMessage.Timestamp; } }
        public int WaitSeconds { get { return this._AbnormalQuotationMessage.WaitSeconds; } }
        public int RemainingSeconds
        {
            get { return this._RemainingSeconds; }
            set
            {
                if(this._RemainingSeconds != value)
                {
                    this._RemainingSeconds = value;
                    this.OnPropertyChanged("RemainingSeconds");
                }
            }
        }
    }
}
