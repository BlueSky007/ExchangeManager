using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Infragistics.Windows.DockManager;
using Manager.Common;
using ManagerConsole.Model;
using ManagerConsole.UI;
using System.Xml.Linq;
using System.Windows.Controls;
using System.Diagnostics;

namespace ManagerConsole
{
    public class LayoutManager
    {
        public const string DefaultLayoutName = "LastClosed";

        private MainWindow _MainWindow;

        // map for ModuleType->MaxSuffix
        private Dictionary<ModuleType, int> _MultipleOpenModuleMaxSuffixes = new Dictionary<ModuleType, int>();

        private HashSet<string> _IgnorePaneNames = new HashSet<string>();
        private Action<ModuleType, UserControl> _OnAddContentPane;
        private ThemeManager _ThemeManager;

        public LayoutManager(MainWindow mainWindow, Action<ModuleType, UserControl> onAddContentPane)
        {
            this._MainWindow = mainWindow;
            this._OnAddContentPane = onAddContentPane;
            this._IgnorePaneNames.Add("FloatPane");
            this._IgnorePaneNames.Add("FunctionTreePane");
            this._MultipleOpenModuleMaxSuffixes.Add(ModuleType.QuotationMonitor, 0);
            this._MultipleOpenModuleMaxSuffixes.Add(ModuleType.ExchangeQuotation, 0);
            this._MultipleOpenModuleMaxSuffixes.Add(ModuleType.DQOrderProcess, 0);
            this._MultipleOpenModuleMaxSuffixes.Add(ModuleType.LimitBatchProcess, 0);
            this._ThemeManager = new ThemeManager(false);
        }

        public ThemeManager ThemeManager { get { return this._ThemeManager; } }
        
        public bool IsMultipleOpenModule(ModuleType moduleType)
        {
            return this._MultipleOpenModuleMaxSuffixes.ContainsKey(moduleType);
        }
        public string GetPaneName(ModuleType moduleType)
        {
            return moduleType.ToString();
        }

        public bool SaveLayout(string layoutName)
        {
            try
            {
                string layout = this._MainWindow.DockManager.SaveLayout();
                StringBuilder contentStringBuilder = new StringBuilder();
                contentStringBuilder.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                contentStringBuilder.Append("<Content>");

                // Save theme
                contentStringBuilder.AppendFormat("<Theme IsWhite=\"{0}\"/>", this._ThemeManager.IsWhite);

                var contentPanes = this._MainWindow.DockManager.GetPanes(PaneNavigationOrder.VisibleOrder);
                foreach (ContentPane item in contentPanes)
                {
                    contentStringBuilder.AppendFormat("<ContentPane Name=\"{0}\">", item.Name);
                    IControlLayout controlLayout = item.Content as IControlLayout;
                    if(controlLayout != null)
                    {
                        contentStringBuilder.Append(controlLayout.GetLayout());
                    }
                    contentStringBuilder.Append("</ContentPane>");
                }
                contentStringBuilder.Append("</Content>");
                string content = contentStringBuilder.ToString();

                ConsoleClient.Instance.SaveLayout(layout, content, layoutName);
                return true;
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Information, ex.ToString());
            }
            return false;
        }

        public void Reset()
        {
            foreach(ContentPane pane in this._MainWindow.DockManager.GetPanes(PaneNavigationOrder.ActivationOrder))
            {
                if (this._IgnorePaneNames.Contains(pane.Name)) continue;
                pane.CloseAction = PaneCloseAction.RemovePane;
                pane.ExecuteCommand(ContentPaneCommands.Close);
            }
        }

        public void LoadLayout(string dockLayout, string contentLayout)
        {
            XDocument contentXDocument = XDocument.Parse(contentLayout);
            try
            {
                bool isWhite;
                if (bool.TryParse(contentXDocument.Element("Content").Element("Theme").Attribute("IsWhite").Value, out isWhite))
                {
                    if (this._ThemeManager.IsWhite != isWhite)
                    {
                        this._ThemeManager.SwitchTheme();
                    }
                }
            }
            catch { }

            XDocument xdocument = XDocument.Parse(dockLayout);
            var layoutPanes = xdocument.Element("xamDockManager").Element("contentPanes").Elements("contentPane");
            // close excess panes
            List<ContentPane> panesWillRemoved = new List<ContentPane>();
            foreach(ContentPane pane in this._MainWindow.DockManager.GetPanes(PaneNavigationOrder.ActivationOrder))
            {
                if (this._IgnorePaneNames.Contains(pane.Name)) continue;
                ModuleType moduleType = this.GetModuleType(pane.Name);
                if(this.IsMultipleOpenModule(moduleType))
                {
                    panesWillRemoved.Add(pane);
                }
                else
                {
                    if (!layoutPanes.Any(p => p.Attribute("name").Value == pane.Name)) panesWillRemoved.Add(pane);
                }
            }
            foreach (ContentPane pane in panesWillRemoved)
            {
                pane.CloseAction = PaneCloseAction.RemovePane;
                pane.ExecuteCommand(ContentPaneCommands.Close);
            }

            // add new panes and apply contentLayout
            var contentPaneLayouts = contentXDocument.Element("Content").Elements("ContentPane");
            foreach (XElement pane in layoutPanes)
            {
                string paneName = pane.Attribute("name").Value;
                if (this._IgnorePaneNames.Contains(paneName)) continue;
                ModuleType moduleType = this.GetModuleType(pane.Attribute("name").Value);
                if (this._MainWindow.AuthorizedModules.ContainsKey(moduleType))
                {
                    ContentPane contentPane = this._MainWindow.DockManager.GetPanes(PaneNavigationOrder.ActivationOrder).SingleOrDefault(p => p.Name == paneName);
                    if (contentPane == null)
                    {
                        contentPane = this.AddContentPane(moduleType, paneName);
                    }
                    IControlLayout controlLayout = contentPane.Content as IControlLayout;
                    if (controlLayout != null)
                    {
                        XElement layoutElement = contentPaneLayouts.SingleOrDefault(l => l.Attribute("Name").Value == paneName);
                        if (layoutElement != null)
                        {
                            controlLayout.SetLayout(layoutElement);
                        }
                    }
                }
            }
            this._MainWindow.DockManager.LoadLayout(dockLayout);
        }

        public ContentPane AddContentPane(ModuleType moduleType, string paneName = null)
        {
            UserControl userControl = this.GetControl(moduleType);
            ContentPane contentPane = this._MainWindow.DockManager.AddDocument(this._MainWindow.AuthorizedModules[moduleType].ModuleDescription, userControl);

            if (paneName == null)
            {
                contentPane.Name = this.GetPaneName(moduleType);
                if (this.IsMultipleOpenModule(moduleType))
                {
                    contentPane.CloseAction = PaneCloseAction.RemovePane;
                    this._MultipleOpenModuleMaxSuffixes[moduleType]++;
                    contentPane.Name += "_" + this._MultipleOpenModuleMaxSuffixes[moduleType].ToString();
                }
            }
            else
            {
                contentPane.Name = paneName;
                if(this.IsMultipleOpenModule(moduleType))
                {
                    contentPane.CloseAction = PaneCloseAction.RemovePane;

                    int suffix = this.GetSuffix(paneName);
                    int currentSuffix = this._MultipleOpenModuleMaxSuffixes[moduleType];
                    this._MultipleOpenModuleMaxSuffixes[moduleType] = Math.Max(currentSuffix, suffix);
                }
            }

            contentPane.Padding = contentPane.BorderThickness = new Thickness(0);
            this._OnAddContentPane(moduleType, userControl);
            return contentPane;
        }

        private ModuleType GetModuleType(string paneName)
        {
            string module = paneName;
            if (paneName.Contains('_'))
            {
                string[] sections = paneName.Split('_');
                module = sections[0];
            }
            return (ModuleType)Enum.Parse(typeof(ModuleType), module);
        }

        private int GetSuffix(string paneName)
        {
            string[] sections = paneName.Split('_');
            if(sections.Length < 2)
            {
                Logger.AddEvent(TraceEventType.Error, "LayoutManager.GetSuffix invalid paneName:" + paneName);
                throw new ArgumentException();
            }
            return int.Parse(sections[1]);
        }

        private UserControl GetControl(ModuleType moduleType)
        {
            switch (moduleType)
            {
                case ModuleType.UserManager:
                    return new UserManagerControl();
                case ModuleType.RoleManager:
                    return new RoleManagerControl();
                case ModuleType.SettingScheduler:
                    return new SettingSchedulerControl();
                case ModuleType.SettingParameter:
                    return new SettingParameterControl();
                case ModuleType.QuotationSource:
                    return new QuotationSourceControl();
                case ModuleType.SourceQuotation:
                    return new SourceQuotationControl();
                case ModuleType.QuotationMonitor:
                    return new QuotationMonitorControl();
                case ModuleType.AbnormalQuotation:
                    break;
                case ModuleType.ExchangeQuotation:
                    return new ExchangeQuotationControl();
                case ModuleType.AdjustSpreadSetting:
                    return new AdjustAndSpreadSettingControl();
                case ModuleType.SourceRelation:
                    return new SourceRelationControl();
                case ModuleType.Quote:
                //return new QutePriceControl();
                //case ModuleType.OrderProcess:
                //    return new OrderTaskControl();
                case ModuleType.LimitBatchProcess:
                    return new DealingLmtOrder();
                case ModuleType.MooMocProcess:
                    return new MooMocOrderTask();
                case ModuleType.DQOrderProcess:
                    return new DealingInstanceOrder();
                case ModuleType.LogAuditQuery:
                    return new LogAuditControl();
                case ModuleType.OrderSearch:
                    return new OrderSearchControl();
                case ModuleType.ExecutedOrder:
                    return new ExecutedOrders();
                case ModuleType.OpenInterest:
                    return new OpenInterestControl();
                case ModuleType.AccountStatus:
                    return new AccountStatusQuery();
                default:
                    break;
            }
            return null;
        }
    }
}
