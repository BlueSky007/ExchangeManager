using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace ManagerConsole.ViewModel
{
    class Login : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string Server
        {
            get;
            set;
        }

        private void NotifyPropertyChanged(String server)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(server));
            }
        }
    }
}
