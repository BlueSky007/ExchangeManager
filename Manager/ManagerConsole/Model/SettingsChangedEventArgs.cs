using ManagerConsole.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ManagerConsole.Model
{
    public class SettingsChangedEventArgs:EventArgs
    {
        public SettingsChangedEventArgs(Account[] changedAccounts, Account[] removedAccounts, InstrumentClient[] addedInstruments,
           InstrumentClient[] removedInstruments, InstrumentClient[] changedInstruments)
        {
            this.ChangedAccounts = changedAccounts;
            this.RemovedAccounts = removedAccounts;
            this.AddedInstruments = addedInstruments;
            this.RemovedInstruments = removedInstruments;
            this.ChangedInstruments = changedInstruments;
        }
        public Account[] ChangedAccounts
        {
            get;
            private set;
        }

        public Account[] RemovedAccounts
        {
            get;
            private set;
        }

        public InstrumentClient[] AddedInstruments
        {
            get;
            private set;
        }

        public InstrumentClient[] RemovedInstruments
        {
            get;
            private set;
        }

        public InstrumentClient[] ChangedInstruments
        {
            get;
            private set;
        }
    }
}
