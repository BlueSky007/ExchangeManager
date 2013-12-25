using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ManagerConsole.ViewModel
{
    public class AdjustRelationViewModel
    {
        private int _AdjustIncrement;
        private int _SpreadIncrement;
        private int _AdjustReplacement;
        private int _SpreadReplacement;

        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<int> InstrumentIds { get; set; }
        public int AdjustActionUp { get { return this._AdjustIncrement; } }
        public int AdjustActionDN { get { return this._AdjustIncrement; } }
        public int AdjustIncrement { get { return this._AdjustIncrement; } set { this._AdjustIncrement = value; } }
        public int AdjustReplacement { get { return this._AdjustReplacement; } set { this._AdjustReplacement = value; } }
        public int AdjustActionReplace { get { return this._AdjustReplacement; } }
        public int SpreadActionUp { get { return this._SpreadIncrement; } }
        public int SpreadActionDN { get { return this._SpreadIncrement; } }
        public int SpreadIncrement { get { return this._SpreadIncrement; } set { this._SpreadIncrement = value; } }
        public int SpreadReplacement { get { return this._SpreadReplacement; } set { this._SpreadReplacement = value; } }
        public int SpreadActionReplace { get { return this._SpreadReplacement; } }

        public AdjustRelationViewModel()
        {
            InstrumentIds = new List<int>();
        }

        public AdjustRelationViewModel(Guid id, string name,List<int> instrumentIds)
        {
            Id = id;
            Name = name;
            InstrumentIds = instrumentIds;
            AdjustIncrement = 1;
            AdjustReplacement = 1;
            SpreadIncrement = 1;
            SpreadReplacement = 1;
        }
    }

    public class IdCode
    {
        public Guid Id;
        public string Code;
    }
}
