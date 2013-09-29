using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ManagerConsole.Model
{
    public class EventLogEntity
    {
        private Guid _Id;
        public EventLogEntity(Guid id)
        {
            this._Id = id;
        }
    }
}
