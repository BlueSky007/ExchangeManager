using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Specialized;
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
using Manager.Common.Settings;
using System.Windows.Media.Animation;

namespace ManagerConsole
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public CommonDialogWin _CommonDialogWin;
        public ConfirmDialogWin _ConfirmDialogWin;
        public ConfirmOrderDialogWin _ConfirmOrderDialogWin;
        public QuotePriceWindow QuotePriceWindow;
        public TaskSchedulerNotify _TaskSchedulerNotify;
        private ObservableCollection<string> _Layouts;
        private OrderHandle _OrderHandle;
        private SourceQuotationControl _SourceQuotationControl;
        private SourceRelationControl _SourceRelationControl;
        private LayoutManager _LayoutManager;
        private Dictionary<string, TreeViewItem> _FunctionTreeItems = new Dictionary<string, TreeViewItem>();

        public Dictionary<ModuleType, Module> AuthorizedModules = new Dictionary<ModuleType, Module>();
        public MainWindow()
        {
            try
            {
                InitializeComponent();
                this.ExchangeDataManager = new ExchangeDataManager(this._Media);
                this._CommonDialogWin = new CommonDialogWin(this.MainFrame);
                this._ConfirmDialogWin = new ConfirmDialogWin(this.MainFrame);
                this._ConfirmOrderDialogWin = new ConfirmOrderDialogWin(this.MainFrame);
                this._OrderHandle = new OrderHandle();
                this._LayoutManager = new LayoutManager(this, this.OnAddContentPane);
            }
            catch(Exception exception)
            {
                Logger.TraceEvent(TraceEventType.Error, "MainWindow ctor: \r\n{0}", exception);
            }
        }

        public SourceQuotationControl SourceQuotationControl { get { return this._SourceQuotationControl; } }

        public SourceRelationControl SourceRelationControl { get { return this._SourceRelationControl; } }

        public ExchangeDataManager ExchangeDataManager
        {
            get;
            private set;
        }

        public OrderHandle OrderHandle
        {
            get { return this._OrderHandle; }
            set { this._OrderHandle = value; }
        }

        public void ShowAbnormalQuotationPane()
        {
            if(this.FloatPane.Visibility != Visibility.Visible)
            {
                this.FloatPane.Visibility = Visibility.Visible;
            }
            if (this.AbnormalQuotationProcessControl.DataContext == null)
            {
                this.AbnormalQuotationProcessControl.DataContext = VmQuotationManager.Instance.AbnormalQuotationManager;
                VmQuotationManager.Instance.AbnormalQuotationManager.AbnormalQuotations.CollectionChanged += AbnormalQuotations_CollectionChanged;
            }
        }

        private void AbnormalQuotations_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (VmQuotationManager.Instance.AbnormalQuotationManager.AbnormalQuotations.Count == 0)
            {
                this.HideAbnormalQuotationPane();
            }
        }

        private void ShowQuotePricePane()
        {
            this.QuotePriceWindow.WindowState = Infragistics.Controls.Interactions.WindowState.Normal;
            this.QuotePriceWindow.Height = 650;
            this.QuotePriceWindow.Width = 650;
        }

        public void HideAbnormalQuotationPane()
        {
            this.FloatPane.Visibility = Visibility.Collapsed;
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
                this.ShowAbnormalQuotationPane();
            }
            else if (moduleType == ModuleType.Quote)
            {
                this.ShowQuotePricePane();
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
            this.ShowLoginWindow();
            App.MainFrameWindow = this;
        }

        private void ShowLoginWindow()
        {
            LoginWindow loginWindow = new LoginWindow(this.HandleSuccessLogin);
            this.MainFrame.Children.Add(loginWindow);
            loginWindow.IsModal = true;
            loginWindow.Show();
            loginWindow.BringToFront();
        }

        private void AddQuotePriceFrm()
        {
            this.QuotePriceWindow = new QuotePriceWindow();
            this.MainFrame.Children.Add(this.QuotePriceWindow);
            this.QuotePriceWindow.WindowState = Infragistics.Controls.Interactions.WindowState.Hidden;
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
                VmQuotationManager.Instance.Initialize();
                this.StatusBar.HandleSuccessLogin(result);
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

        private void LoadSettingsParametersCallback(SettingsParameter settingsParameter)
        {
            this.ExchangeDataManager.InitializeSettingParameter(settingsParameter);
        }

        private void GetInitializeDataCallback(InitializeData initalizeData)
        {
            this.Dispatcher.BeginInvoke((Action)delegate()
            {
                this.ExchangeDataManager.Initialize(initalizeData);
                App.MainFrameWindow.StatusBar.ShowStatusText(string.Empty);
            });
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

                string iconName = module.Type.ToString();
                TreeViewItem functionNode;
                if (!this._FunctionTreeItems.TryGetValue(iconName, out functionNode))
                {
                    functionNode = new TreeViewItem() { Tag = module.Type };
                    if (this.Resources.Contains(iconName))
                    {
                        StackPanel headerPanel;
                        headerPanel = new StackPanel() { Orientation = Orientation.Horizontal };
                        headerPanel.Children.Add((UIElement)this.Resources[iconName]);
                        headerPanel.Children.Add(new TextBlock() { Text = module.ModuleDescription, Margin = new Thickness(3, 0, 0, 0) });
                        functionNode.Header = headerPanel;
                    }
                    else
                    {
                        functionNode.Header = module.ModuleDescription;
                    }
                    this._FunctionTreeItems.Add(iconName, functionNode);
                }
                functionNode.MouseDoubleClick += treeViewItem_MouseDoubleClick;
                catalogNode.Items.Add(functionNode);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.SaveLayoutAndLogout();
        }

        private void SaveLayoutAndLogout()
        {
            try
            {
                if (ConsoleClient.Instance.IsLoggedIn)
                {
                    this._LayoutManager.SaveLayout(LayoutManager.DefaultLayoutName);
                    ConsoleClient.Instance.Logout();
                }
            }
            catch (Exception exception)
            {
                Logger.TraceEvent(TraceEventType.Error, "MainWindow.SaveLayoutAndLogout\r\n{0}", exception);
                MessageBox.Show(exception.ToString(), "SaveLayoutAndLogout Error");
            }
        }

        public void Logout_Click(object sender, EventArgs e)
        {
            try
            {
                ConsoleClient.Instance.Logout();
                this.AuthorizedModules.Clear();
                foreach (TreeViewItem item in this.FunctionTree.Items) item.Items.Clear(); // When the tree reaches three-layer need to be replaced with the recursive algorithm
                this.FunctionTree.Items.Clear();
                this.ExchangeDataManager.Clear();
                VmQuotationManager.Instance.Reset();
                this._LayoutManager.Reset();
                this.ShowLoginWindow();
            }
            catch(Exception exception)
            {
                Logger.TraceEvent(TraceEventType.Error, "MainWindow.Logout_Click\r\n{0}", exception);
                MessageBox.Show(exception.ToString(), "Logout Error");
            }
        }

        public void KickOut()
        {
            this.Logout_Click(null, null);
            this.StatusBar.ShowStatusText("用户已被登出！");
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
            ConsoleClient.Instance.LoadLayout(SR.SystemDeafult, this.EndLoadLayout);
        }

        private void XamMenuItem_Click_1(object sender, EventArgs e)
        {
            ConsoleClient.Instance.Updatetest();
        }

        private void ChangeTheme_Click(object sender, EventArgs e)
        {
            this._LayoutManager.ThemeManager.SwitchTheme();
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            App.Current.Shutdown();
        }
    }
}
