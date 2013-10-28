using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manager.Common
{
    public class TradePolicyDetail
    {
        public Guid TradePolicyId
        {
            get;
            set;
        }

        public Guid InstrumentId
        {
            get;
            set;
        }

        public Guid? VolumeNecessaryId
        {
            get;
            set;
        }

        public decimal ContractSize
        {
            get;
            set;
        }
    }
}
