using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manager.Common
{
    public enum ConnectionState
    {
        Unknown,
        Disconnected,
        Connecting,
        Connected
    }

    public enum ModuleCategoryType
    {
        UserManager = 1,
        Configuration = 2,
        Quotation = 3,
        Dealing = 4,
        QueryReport = 5
    }

    public enum ModuleType
    {
        UserManager = 6,
        RoleManager = 7,
        InstrumentManager = 8,
        QuoationSource = 9,
        QuotePolicy = 10,
        AbnormalQuotation = 11,
        Quote = 12
    }
    public enum Language
    {
        CHT,
        CHS,
        ENG
    }
    public enum TradeOption
    {
        Invalid,
        Stop,
        Better
    }
}
