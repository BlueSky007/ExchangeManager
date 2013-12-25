using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Manager.Common;
using ManagerConsole.Helper;
using Manager.Common.QuotationEntities;

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
        public string NewPrice { get { return this._AbnormalQuotationMessage.NewPrice; } }
        public string OldPrice
        {
            get
            {
                VmInstrument vmInstrument = VmQuotationManager.Instance.Instruments.SingleOrDefault(i => i.Id == this._AbnormalQuotationMessage.InstrumentId);
                if (vmInstrument == null)
                {
                    return this._AbnormalQuotationMessage.OldPrice.ToString();
                }
                else
                {
                    return this._AbnormalQuotationMessage.OldPrice.ToString("F" + vmInstrument.DecimalPlace.ToString());
                }
            }
        }
        public string PriceOutOfRangeType
        {
            get
            {
                return this._AbnormalQuotationMessage.OutOfRangeType == OutOfRangeType.Ask ? "Ask" : "Bid";
            }
        }
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
