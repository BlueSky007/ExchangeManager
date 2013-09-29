using Manager.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ManagerService.Messages
{
    internal abstract class DispatchableMessage
    {
        internal virtual bool IsBroadcast
        {
            get { return false; }
        }

        protected internal Guid[] CustomerIds
        {
            get;
            protected set;
        }

        protected internal Guid[] AccountGroupIds
        {
            get;
            protected set;
        }

        protected internal ICollection<Guid> Accounts
        {
            get;
            protected set;
        }

        protected internal ICollection<Guid> Instruments
        {
            get;
            protected set;
        }

        internal protected abstract Message MessageToSend
        {
            get;
        }
    }
}
