using ManagerConsole.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonBlotterSelection = Manager.Common.ReportEntities.BlotterSelection;

namespace ManagerConsole.ViewModel
{
    public class BlotterSelection : PropertyChangedNotifier
    {
        private bool _IsSelected = false;

        public BlotterSelection() { }

        public BlotterSelection(CommonBlotterSelection commonBlotterSelection)
        {
            this.Id = commonBlotterSelection.Id;
            this.Code = commonBlotterSelection.Code;
        }

        public bool IsSelected
        {
            get { return this._IsSelected; }
            set { this._IsSelected = value; this.OnPropertyChanged("IsSelected"); }
        }
        public Guid Id { get; set; }
        public string Code { get; set; }
    }
}
