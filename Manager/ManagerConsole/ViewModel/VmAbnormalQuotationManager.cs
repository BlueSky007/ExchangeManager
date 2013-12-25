using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using Manager.Common;
using ManagerConsole.Helper;

namespace ManagerConsole.ViewModel
{
    public class VmAbnormalQuotationManager : PropertyChangedNotifier
    {
        private Timer _Timer;
        private bool _TimerStarted = false;
        private ObservableCollection<VmAbnormalQuotation> _AbnormalQuotations = new ObservableCollection<VmAbnormalQuotation>();

        public VmAbnormalQuotationManager()
        {
            this._Timer = new Timer(this.CheckAbnormalQuotations, null, Timeout.Infinite, Timeout.Infinite);
        }

        public VmAbnormalQuotation FirstItem
        {
            get
            {
                if (this._AbnormalQuotations.Count > 0)
                {
                    return this._AbnormalQuotations[0];
                }
                else
                {
                    return null;
                }
            }
        }
        public ObservableCollection<VmAbnormalQuotation> AbnormalQuotations { get { return this._AbnormalQuotations; } }

        public void RemoveFirstItem()
        {
            this._AbnormalQuotations.RemoveAt(0);
            this.OnPropertyChanged("FirstItem");
        }

        public void AddAbnormalQuotation(AbnormalQuotationMessage message)
        {
            VmAbnormalQuotation abnormalQuotation = new VmAbnormalQuotation(message);
            lock (this._Timer)
            {
                this._AbnormalQuotations.Add(abnormalQuotation);
                if(this._AbnormalQuotations.Count == 1)
                {
                    this.OnPropertyChanged("FirstItem");
                }
                if (!this._TimerStarted)
                {
                    this._Timer.Change(0, 1000);
                    this._TimerStarted = true;
                }
            }
        }

        private void CheckAbnormalQuotations(object state)
        {
            App.MainWindow.Dispatcher.BeginInvoke((Action)delegate()
            {
                List<VmAbnormalQuotation> timeoutItems = new List<VmAbnormalQuotation>();
                foreach (VmAbnormalQuotation quotation in this._AbnormalQuotations)
                {
                    if (--quotation.RemainingSeconds == 0)
                    {
                        timeoutItems.Add(quotation);
                    }
                }
                lock (this._Timer)
                {
                    bool firstItemChanged = timeoutItems.Any(ti => ti.ConfirmId == this._AbnormalQuotations[0].ConfirmId);
                    foreach (VmAbnormalQuotation item in timeoutItems)
                    {
                        this._AbnormalQuotations.Remove(item);
                    }
                    if (this._AbnormalQuotations.Count == 0)
                    {
                        this._Timer.Change(Timeout.Infinite, Timeout.Infinite);
                        this._TimerStarted = false;
                    }
                    if (firstItemChanged) this.OnPropertyChanged("FirstItem");
                }
            });
        }
    }
}
