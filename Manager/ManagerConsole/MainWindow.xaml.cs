using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Manager.Common;
using Infragistics.Windows.DockManager;
using ManagerConsole.Model;
using ManagerConsole.Helper;
using ManagerConsole.UI;
using Infragistics.Controls.Menus;
using System.Collections.ObjectModel;
using System.Threading;
using ManagerConsole.ViewModel;
using System.Diagnostics;
using Infragistics.Controls.Interactions;

namespace ManagerConsole
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ConsoleClient _ConsoleClient = new ConsoleClient();
        public CommonDialogWin _CommonDialogWin;
        public ConfirmDialogWin _ConfirmDialogWin;
        public ConfirmOrderDialogWin _ConfirmOrderDialogWin;
        private QuotePriceWindow _QuotePriceWindow;
        private TaskSchedulerNotify _TaskSchedulerNotify;
        private ObservableCollection<string> _Layouts;
        private OrderHandle _OrderHandle;
        private MessageProcessor MessageProcessor;
        private SourceQuotationControl _SourceQuotationControl;
        private SourceRelationControl _SourceRelationControl;
        private LayoutManager _LayoutManager;

        public Dictionary<ModuleType, Module> AuthorizedModules = new Dictionary<ModuleType, Module>();
        public MainWindow()
        {
            InitializeComponent();
            this.InitDataManager = new InitDataManager();
            this._CommonDialogWin = new CommonDialogWin(this.MainFrame);
            this._ConfirmDialogWin = new ConfirmDialogWin(this.MainFrame);
            this._ConfirmOrderDialogWin = new ConfirmOrderDialogWin(this.MainFrame);
            this._OrderHandle = new OrderHandle();
            this._LayoutManager = new LayoutManager(this, this.OnAddContentPane);
        }

        public SourceQuotationControl SourceQuotationControl { get { return this._SourceQuotationControl; } }

        public SourceRelationControl SourceRelationControl { get { return this._SourceRelationControl; } }

        public InitDataManager InitDataManager
        {
            get;
            private set;
        }

        public OrderHandle OrderHandle
        {
            get { return this._OrderHandle; }
            set { this._OrderHandle = value; }
        }

        public void ShowAbnormalQuotation()
        {
            if(this.FloatPane.Visibility != Visibility.Visible)
            {
                this.FloatPane.Visibility = Visibility.Visible;
            }
            if (this.AbnormalQuotationProcessControl.DataContext == null)
            {
                this.AbnormalQuotationProcessControl.DataContext = VmQuotationManager.Instance.AbnormalQuotationManager;
            }
        }

        private void OnAddContentPane(ModuleType moduleType, UserControl userControl)
        {
            if (moduleType == ModuleType.SourceQuotation)
            {
                this._SourceQuotationControl = (SourceQuotationControl)userControl;
            }
            else if (moduleType == ModuleType.SourceRelation)
            {
                this._SourceRelationControl = (SourceRelationControl)userControl;
            }
        }

        private void treeViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem moduleNode = sender as TreeViewItem;
            ModuleType moduleType = (ModuleType)(int)moduleNode.Tag;

            if (moduleType == ModuleType.AbnormalQuotation)
            {
                this.ShowAbnormalQuotation();
            }
            else
            {
                if (this._LayoutManager.IsMultipleOpenModule(moduleType))
                {
                    this._LayoutManager.AddContentPane(moduleType).Activate();
                }
                else
                {
                    string paneName = this._LayoutManager.GetPaneName(moduleType);
                    ContentPane contentPane = this.DockManager.GetPanes(PaneNavigationOrder.ActivationOrder).Where(p => p.Name == paneName).SingleOrDefault();

                    if (contentPane == null)
                    {
                        this._LayoutManager.AddContentPane(moduleType).Activate();
                    }
                    else
                    {
                        contentPane.Visibility = Visibility.Visible;
                        contentPane.Activate();
                    }
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow(this.HandleSuccessLogin);
            this.MainFrame.Children.Add(loginWindow);
            loginWindow.IsModal = true;
            loginWindow.Show();
            loginWindow.BringToFront();
            App.MainWindow = this;  
        }

        private void AddQuotePriceFrm()
        {
            this._QuotePriceWindow = new QuotePriceWindow();
            this.MainFrame.Children.Add(this._QuotePriceWindow);
            this._QuotePriceWindow.WindowState = Infragistics.Controls.Interactions.WindowState.Hidden;
        }

        private void AddTaskSchedulerNotifyFrm()
        {
            this._TaskSchedulerNotify = new TaskSchedulerNotify();
            this.MainFrame.Children.Add(this._TaskSchedulerNotify);
            this._TaskSchedulerNotify.Visibility = System.Windows.Visibility.Collapsed;
            this._TaskSchedulerNotify.WindowState = Infragistics.Controls.Interactions.WindowState.Hidden;
            this._TaskSchedulerNotify.OnSettingTaskClickEvent += new SettingTaskClickHandler(HandleOnSettingTaskClickedEvent);
        }

        private void HandleOnSettingTaskClickedEvent(object sender,Manager.Common.Settings.TaskScheduler taskScheduler)
        {
            RunSettingTaskDetail control = new RunSettingTaskDetail(taskScheduler);
            XamDialogWindow window = new XamDialogWindow();
            window.FontSize = 12;
            window.StartupPosition = StartupPosition.Center;
            window.IsModal = false;
            window.Header = "任务详细";
            window.Width = 455;
            window.Height = 345;
            window.Content = control;
            window.Show();

            this.MainFrame.Children.Add(window);
            window.BringToFront();

            control.OnExited += new RoutedEventHandler(delegate(object sender2, RoutedEventArgs e2)
            {
                window.Close();
            });
        }

        private void HandleSuccessLogin(LoginResult result)
        {
            try
            {
                this.InitializeUI(result);
                ConsoleClient.Instance.LoadSettingsParameters(this.LoadSettingsParametersCallback);
                ConsoleClient.Instance.GetInitializeData(this.GetInitializeDataCallback);
                this.AddQuotePriceFrm();
                this.AddTaskSchedulerNotifyFrm();
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "HandleSuccessLogin.\r\n{0}", ex.ToString());
            }
        }

        private void LoadSettingsParametersCallback(List<string> parameters)
        {
            foreach (string xmlStr in parameters)
            {
                if (!string.IsNullOrEmpty(xmlStr))
                {
                    XElement parametersXml = XElement.Parse(xmlStr);
                    this.InitDataManager.SettingsManager.InitializeSettingParameter(parametersXml);
                }
            }
        }

        private void GetInitializeDataCallback(List<InitializeData> initalizeDatas)
        {
            this.InitDataManager.Initialize(initalizeDatas);
            this.MessageProcessor = new MessageProcessor(this._Media, this.InitDataManager);
        }

        private void InitializeUI(LoginResult result)
        {
            FunctionTree functionTree = ConsoleClient.Instance.GetFunctionTree();
            for (int i = 0; i < functionTree.Modules.Count; i++)
            {
                this.AuthorizedModules.Add(functionTree.Modules[i].Type, functionTree.Modules[i]);
            }

            // initialize layout
            try
            {
                this._Layouts = new ObservableCollection<string>(result.LayoutNames);
                foreach (string layoutName in result.LayoutNames)
                {
                    XamMenuItem item = new XamMenuItem();
                    item.Header = layoutName;
                    item.Click += Layout_Click;
                    this.layout.Items.Add(item);
                }
                if (!string.IsNullOrEmpty(result.DockLayout))
                {
                    this._LayoutManager.LoadLayout(result.DockLayout, result.ContentLayout);
                }
            }
            catch (Exception exception)
            {
                Logger.AddEvent(TraceEventType.Error, "MainWindow.InitializeUI\r\n{0}", exception);
            }

            // initialize function tree
            Dictionary<ModuleCategoryType, TreeViewItem> typeTreeViewItems = new Dictionary<ModuleCategoryType, TreeViewItem>();
            foreach (Module module in this.AuthorizedModules.Values)
            {
                TreeViewItem catalogNode;
                if (!typeTreeViewItems.TryGetValue(module.Category, out catalogNode))
                {
                    catalogNode = new TreeViewItem() { Header = functionTree.Categories.Single<Category>(c => c.CategoryType == module.Category).CategoryDescription };
                    this.FunctionTree.Items.Add(catalogNode);
                    typeTreeViewItems.Add(module.Category, catalogNode);
                }
                TreeViewItem functionNode = new TreeViewItem() { Header = module.ModuleDescription, Tag = module.Type };
                functionNode.MouseDoubleClick += treeViewItem_MouseDoubleClick;
                catalogNode.Items.Add(functionNode);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this._LayoutManager.SaveLayout(LayoutManager.DefaultLayoutName);
        }

        private void XamMenuItem_Click(object sender, EventArgs e)
        {
           
            this.Window_Loaded(null, null);
        }

        private void ChangePassword_Click(object sender, EventArgs e)
        {
            ChangePassword changePasswordWindow = new ChangePassword();
            this.MainFrame.Children.Add(changePasswordWindow);
            changePasswordWindow.IsModal = true;
            changePasswordWindow.Show();
            changePasswordWindow.BringToFront();
        }

        private void SaveLayout(string layoutName, Action CloseDialog)
        {
            try
            {
                if (!this._Layouts.Contains(layoutName))
                {
                    XamMenuItem item = new XamMenuItem();
                    item.Header = layoutName;
                    item.Click += Layout_Click;
                    this.layout.Items.Add(item);
                    this._Layouts.Add(layoutName);
                }
                this._LayoutManager.SaveLayout(layoutName);
                CloseDialog();
                MessageBox.Show("存储成功");
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "SaveLayout.\r\n{0}", ex.ToString());
            }
        }

        private void Layout_Click(object sender, EventArgs e)
        {
            try
            {
                XamMenuItem item = sender as XamMenuItem;
                string layoutName = item.Header.ToString();
                ConsoleClient.Instance.LoadLayout(layoutName, EndLoadLayout);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "Layout_Click.\r\n{0}", ex.ToString());
            }
        }

        private void EndLoadLayout(List<string> layouts)
        {
            try
            {
                string layout = layouts[0];
                string content = layouts[1];
                this.Dispatcher.BeginInvoke((Action<string, string>)delegate(string dockLayout, string contentLayout)
                {
                    this._LayoutManager.LoadLayout(dockLayout, contentLayout);
                }, layout, content);
                //this.LoadLayout(layout, content);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "EndLoadLayout.\r\n{0}", ex.ToString());
            }
        }

        private void SaveLayout_Click(object sender, EventArgs e)
        {
            SaveLayoutWindow saveLayout = new SaveLayoutWindow(this._Layouts,this.SaveLayout);
            this.MainFrame.Children.Add(saveLayout);
            saveLayout.IsModal = true;
            saveLayout.Show();
            saveLayout.BringToFront();
        }

        private void Reset_Click(object sender, EventArgs e)
        {
            foreach (var menuitem in this.layout.Items)
            {
                if (menuitem.GetType() == typeof(XamMenuItem))
                {
                    if (((XamMenuItem)menuitem).Icon != null)
                    {
                        ((CheckBox)((XamMenuItem)menuitem).Icon).IsChecked = false;
                    }
                }
            }
            ConsoleClient.Instance.LoadLayout("SystemDeafult", this.EndLoadLayout);
        }

        //private void LoadLayout(string dockLayout, string contentLayout)
        //{
        //    if (dockLayout != null)
        //    {
        //        this.Dispatcher.BeginInvoke((Action<string>)delegate(string layout)
        //        {
        //            XDocument xdocument = XDocument.Parse(dockLayout);
        //            var panes = xdocument.Element("xamDockManager").Element("contentPanes").Elements("contentPane").Where(p => p.Attribute("name").Value != "FunctionTreePane");
        //            foreach (XElement pane in panes)
        //            {
        //                int moduleType = MainWindowHelper.GetModuleType(pane.Attribute("name").Value);
        //                if (this.AuthorizedModules.ContainsKey(moduleType))
        //                {
        //                    bool isAdd = false;
        //                    string paneName = MainWindowHelper.GetPaneName(moduleType);
        //                    ContentPane contentPane = this.DockManager.GetPanes(PaneNavigationOrder.ActivationOrder).Where(p => p.Name == paneName).SingleOrDefault();
        //                    if (contentPane == null)
        //                    {
        //                        isAdd = true;
        //                    }
        //                    if (isAdd)
        //                    {
        //                        this.AddContentPane(moduleType);
        //                    }
        //                }
        //            }
        //            this.DockManager.LoadLayout(dockLayout);
        //        }, dockLayout);
        //    }
        //}

        private void XamMenuItem_Click_1(object sender, EventArgs e)
        {
            ConsoleClient.Instance.Updatetest();
        }

        private void XamMenuItem_Click_2(object sender, EventArgs e)
        {
            //this.FloatPane.Visibility = System.Windows.Visibility.Visible;
            foreach(ResourceDictionary dict in Application.Current.Resources.MergedDictionaries)
            {
                string source = dict.Source.OriginalString;
                this.Title = source;
            }
        }

        private void ChangeTheme_Click(object sender, EventArgs e)
        {
            XamMenuItem menuItem = (XamMenuItem)sender;
            ResourceDictionary dicts = Application.Current.Resources;
            for (int i = dicts.MergedDictionaries.Count - 1; i >= 0; i--)
            {
                var rd = dicts.MergedDictionaries[i];
                if (rd.Source != null)
                {
                    if (rd.Source.OriginalString.EndsWith("Black.xaml"))
                    {
                        dicts.MergedDictionaries.RemoveAt(i);
                        dicts.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("ManagerConsole;component/Asset/Theme_White.xaml", UriKind.Relative) });
                        break;
                    }
                    else if (rd.Source.OriginalString.EndsWith("White.xaml"))
                    {
                        dicts.MergedDictionaries.RemoveAt(i);
                        dicts.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("ManagerConsole;component/Asset/Theme_Black.xaml", UriKind.Relative) });
                        break;
                    }
                }
            }
        }
    }
}
