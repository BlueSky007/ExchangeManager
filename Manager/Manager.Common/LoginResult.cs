using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Manager.Common
{
    public class LoginResult
    {
        public string SessionId { get; set; }
        public bool Succeeded
        {
            get { return !string.IsNullOrEmpty(this.SessionId); }
        }

        public List<string> LayoutNames { get; set; }

        public string DockLayout { get; set; }

        public string ContentLayout { get; set; }

        public User User { get; set; }

        public Dictionary<string, ConnectionState> SourceConnectionStates;
        public Dictionary<string, ConnectionState> ExchangeConnectionStates;
    }
}
