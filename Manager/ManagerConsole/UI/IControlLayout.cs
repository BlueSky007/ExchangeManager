using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace ManagerConsole.UI
{
    public interface IControlLayout
    {
        string GetLayout();
        void SetLayout(XElement layout);
    }
}
