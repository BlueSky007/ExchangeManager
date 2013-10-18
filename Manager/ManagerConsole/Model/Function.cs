using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ManagerConsole.Model
{
    public class Function
    {
        public Dictionary<int, string> FunctionPermissions { get; set; }

        public Function()
        {
            FunctionPermissions = new Dictionary<int, string>();
        }

        public bool IsHasPermission(int functionId)
        {
            return FunctionPermissions.ContainsKey(functionId);
        }

        public bool IsHasPermission(string functionName)
        {
            return FunctionPermissions.ContainsValue(functionName);
        }
    }

    
}
