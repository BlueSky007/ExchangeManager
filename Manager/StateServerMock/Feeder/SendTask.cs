using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace TestConsole.Feeder
{
    public class SendTask : INotifyPropertyChanged
    {
        private string _Status;

        public event PropertyChangedEventHandler PropertyChanged;
        public bool IsSelected { get; set; }
        public string SourceName { get; set; }
        public string LoginName { get; set; }
        public string Password { get; set; }
        public string DataFileName { get; set; }
        public bool IsRepeat { get; set; }

        public string Status
        {
            get
            {
                return this._Status;
            }
            set
            {
                if (this._Status != value)
                {
                    this._Status = value;
                    App.MainFrameWindow.Dispatcher.BeginInvoke((Action)delegate()
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("Status"));
                    });
                }
            }
        }
    }
}
