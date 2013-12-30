using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ManagerConsole.UI
{
    public interface IControlLayout
    {
        string GetLayout();
        void SetLayout(string layout);
    }
}
