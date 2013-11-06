using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manager.Common
{
    public class Module
    {
        public ModuleType Type { get; set; }
        public ModuleCategoryType Category { get; set; }
        public string ModuleDescription { get; set; }
    }

    public class Category
    {
        public ModuleCategoryType CategoryType { get; set; }
        public string CategoryDescription { get; set; }
    }

    public class FunctionTree
    {
        public List<Category> Categories { get; set; }
        public List<Module> Modules { get; set; }

        public FunctionTree()
        {
            Categories = new List<Category>();
            Modules = new List<Module>();
        }
    }
}
