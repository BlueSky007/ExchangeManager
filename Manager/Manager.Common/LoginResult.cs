using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Manager.Common
{
    public class LoginResult
    {
        public LoginResult()
        {
            //this.InitializeDatas = new ObservableCollection<InitializeData>();
            this.LayoutNames = new List<string>();
        }
        public string SessionId { get; set; }
        public bool Succeeded
        {
            get { return !string.IsNullOrEmpty(this.SessionId); }
        }

        public List<string> LayoutNames { get; set; }

        public string DockLayout { get; set; }

        public string ContentLayout { get; set; }

        public User User { get; set; }

       // public ObservableCollection<InitializeData> InitializeDatas;
    }
}
