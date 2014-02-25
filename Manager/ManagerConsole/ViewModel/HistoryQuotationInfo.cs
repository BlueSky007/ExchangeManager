using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Manager.Common;
using Manager.Common.QuotationEntities;

namespace ManagerConsole.ViewModel
{
    public class HistoryQuotationInfo : INotifyPropertyChanged
    {
        public string ExchangeCode { get; set; }
        public Guid InstrumentId { get; set; }
        public string Origin { get; set; }
        public DateTime TimeSpan { get; set; }
        public int NumeratorUnit { get; set; }
        public int Denominator { get; set; }
        public string OriginMask { get { return GetMask(); } }

        public ObservableCollection<HistoryQuotationData> HistoryQuotationGridData { get; set; }

        public HistoryQuotationInfo()
        {
            this.HistoryQuotationGridData = new ObservableCollection<HistoryQuotationData>();
        }

        public bool IsHistoryQuotationEdited()
        {
            foreach (HistoryQuotationData item in this.HistoryQuotationGridData)
            {
                if (item.IsEdit)
                {
                    return true;
                }
            }
            return false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        private string GetMask()
        {
            try
            {
                string mask = "nnnnnnnnnn";
                if (this.Denominator != 0)
                {
                    decimal digits = this.NumeratorUnit / this.Denominator;
                    string[] strs = digits.ToString().Split('.');
                    int decimals = 0;
                    if (strs.Length > 1)
                    {
                        decimals = strs[1].Length;
                    }
                    if (decimals > 0)
                    {
                        mask += ".";
                        for (int i = 0; i < decimals; i++)
                        {
                            mask += "n";
                        }
                    }

                }
                return mask;
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "Get Price Mask Error.\r\nNumeratorUnit:{0}Denominator:{1}\r\n{2}", this.NumeratorUnit, this.Denominator, ex.ToString());
                return "nnnnnnnnnn.nnnn";
            }
        }

    }
}
