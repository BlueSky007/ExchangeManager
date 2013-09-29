using Manager.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ManagerConsole.Model
{
    public class ConsoleClientMock
    {
        public static LoginResult Login(string server, int port, string userName, string password, string oldSessionId = null)
        {
            // test code to simulate login
            return new LoginResult
            {
                SessionId = "SessionId:aaaaaaaaaa",
                DockLayout = File.Exists("Layout.xml") ? File.ReadAllText("Layout.xml") : string.Empty,
                ContentLayout = File.Exists("ContentLayout.xml") ? File.ReadAllText("ContentLayout.xml") : string.Empty
            };
        }

        public static FunctionTree GetFunctionTree()
        {
            //Module[] _FunctionArray = new Module[] { 
            //    new Module { Type = 1, Category = ModuleCategory.UserManager, CategoryDescription = "用户管理", ModuleDescription="用户管理"},
            //    new Module { Type = 2, Category = ModuleCategory.UserManager, CategoryDescription = "用户管理", ModuleDescription="角色管理"},
            //    new Module { Type = 3, Category = ModuleCategory.Configuration, CategoryDescription = "配置管理", ModuleDescription="Instrument"},
            //    new Module { Type = 4, Category = ModuleCategory.Configuration, CategoryDescription = "配置管理", ModuleDescription="QuoationSource"},
            //    new Module { Type = 5, Category = ModuleCategory.Configuration, CategoryDescription = "配置管理", ModuleDescription="QuotePolicy"},
            //    new Module { Type = 6, Category = ModuleCategory.Quotation, CategoryDescription = "价格管理", ModuleDescription="异常价格处理"},
            //    new Module { Type = 7, Category = ModuleCategory.Dealing, CategoryDescription = "交易管理", ModuleDescription="询价"}
            //};

            return new FunctionTree
            {
                Categories = new List<Category>{
                    new Category{ CategoryType= ModuleCategoryType.UserManager, CategoryDescription="用户管理"},
                    new Category{ CategoryType= ModuleCategoryType.Configuration, CategoryDescription="配置管理"},
                    new Category{ CategoryType= ModuleCategoryType.Quotation, CategoryDescription="价格管理"},
                    new Category{ CategoryType= ModuleCategoryType.Dealing, CategoryDescription="交易管理"},
                },
                Modules = new List< Module>{
                    new Module{ Type= ModuleType.UserManager, Category= ModuleCategoryType.UserManager, ModuleDescription="用户管理" },
                    new Module{ Type= ModuleType.RoleManager, Category= ModuleCategoryType.UserManager, ModuleDescription="角色管理" },
                    new Module{ Type= ModuleType.InstrumentManager, Category= ModuleCategoryType.Configuration, ModuleDescription="Instrument" },
                    new Module{ Type= ModuleType.QuoationSource, Category= ModuleCategoryType.Configuration, ModuleDescription="QuoationSource" },
                    new Module{ Type= ModuleType.QuotePolicy, Category= ModuleCategoryType.Configuration, ModuleDescription="QuotePolicy" },
                    new Module{ Type= ModuleType.AbnormalQuotation, Category= ModuleCategoryType.Quotation, ModuleDescription="异常价格处理" },
                    new Module{ Type= ModuleType.Quote, Category= ModuleCategoryType.Dealing, ModuleDescription="询价" },
                }
            };
        }
    }
}
