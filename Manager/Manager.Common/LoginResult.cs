using System;
using System.Collections.Generic;
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

        public string DockLayout { get; set; }

        public string ContentLayout { get; set; }
    }
}
