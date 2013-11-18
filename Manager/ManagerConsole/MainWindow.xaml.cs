﻿using System;
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

namespace ManagerConsole
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Dictionary<int, Module> _Modules = new Dictionary<int, Module>();
        private ConsoleClient _ConsoleClient = new ConsoleClient();
        public CommonDialogWin CommonDialogWin;
        public ConfirmDialogWin ConfirmDialogWin;
        public ConfirmOrderDialogWin ConfirmOrderDialogWin;
        public OrderHandle OrderHandle;
        private OrderTaskControl _OrderTaskControl;
        private LMTProcess _LMTProcess;
        private ObservableCollection<string> _Layouts;

        public MainWindow()
        {
            InitializeComponent();

            this.InitDataManager = new InitDataManager();
            this.CommonDialogWin = new CommonDialogWin(this.MainFrame);
            this.ConfirmDialogWin = new ConfirmDialogWin(this.MainFrame);
            this.ConfirmOrderDialogWin = new ConfirmOrderDialogWin(this.MainFrame);
            this.OrderHandle = new OrderHandle();
        }

        public InitDataManager InitDataManager
        {
            get;
            private set;
        }

        private void treeViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem moduleNode = sender as TreeViewItem;
            int moduleId = (int)moduleNode.Tag;
            string paneName = MainWindowHelper.GetPaneName(moduleId);
            ContentPane contentPane = this.DockManager.GetPanes(PaneNavigationOrder.ActivationOrder).Where(p => p.Name == paneName).SingleOrDefault();
            if (contentPane == null)
            {
                this.AddContentPane(moduleId).Activate();
            }
            else
            {
                contentPane.Visibility = Visibility.Visible;
                contentPane.Activate();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow(this.HandleSuccessLogin);
            this.MainFrame.Children.Add(loginWindow);
            loginWindow.IsModal = true;
            loginWindow.Show();
            loginWindow.BringToFront();
        }

        private void HandleSuccessLogin(LoginResult result)
        {
            try
            {
                this.InitializeLayout(result);
                this.AttachEvent();
                this.InitDataManager.Initialize(result.InitializeDatas);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "HandleSuccessLogin.\r\n{0}", ex.ToString());
            }
        }

        private void InitializeLayout(LoginResult result)
        {
            // initialize layout
            this._Layouts = new ObservableCollection<string>(result.LayoutNames);
            foreach (string layoutName in result.LayoutNames)
            {
                XamMenuItem item = new XamMenuItem();
                item.Header = layoutName;
                item.Click += Layout_Click;
                this.layout.Items.Add(item);
            }
            FunctionTree functionTree = ConsoleClient.Instance.GetFunctionTree();
            for (int i = 0; i < functionTree.Modules.Count; i++)
            {
                this._Modules.Add((int)functionTree.Modules[i].Type, functionTree.Modules[i]);
            }
            if (result.DockLayout != null)
            {
                XDocument xdocument = XDocument.Parse(result.DockLayout);
                var panes = xdocument.Element("xamDockManager").Element("contentPanes").Elements("contentPane").Where(p => p.Attribute("name").Value != "FunctionTreePane");
                foreach (XElement pane in panes)
                {
                    int moduleType = MainWindowHelper.GetModuleType(pane.Attribute("name").Value);
                    if (this._Modules.ContainsKey(moduleType))
                    {
                        this.AddContentPane(moduleType);
                    }
                }
                this.DockManager.LoadLayout(result.DockLayout);
            }

            // initialize function tree
            Dictionary<ModuleCategoryType, TreeViewItem> typeTreeViewItems = new Dictionary<ModuleCategoryType, TreeViewItem>();
            foreach (Module module in this._Modules.Values)
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

        private ContentPane AddContentPane(int moduleType)
        {
            ContentPane contentPane = this.DockManager.AddDocument(this._Modules[moduleType].ModuleDescription, MainWindowHelper.GetControl((ModuleType)moduleType));
            contentPane.Name = MainWindowHelper.GetPaneName(moduleType);
            Thickness zeroThickness = new Thickness(0);
            contentPane.Padding = zeroThickness;
            contentPane.BorderThickness = zeroThickness;
            return contentPane;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                string layout = this.DockManager.SaveLayout();
                StringBuilder contentBld = new StringBuilder();
                contentBld.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                contentBld.Append("<Content>");
                var array = this.DockManager.GetPanes(PaneNavigationOrder.VisibleOrder);
                foreach (ContentPane item in array)
                {
                    if (item.Name.ToLower() != "leftedgedock" && item.Name.ToLower() != "rightedgedock")
                    {
                        contentBld.AppendFormat("<ContentPane Name=\"{0}\"/>", item.Name);
                    }
                }
                contentBld.Append("</Content>");
                string content = contentBld.ToString();
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Information, layout);
                ConsoleClient.Instance.SaveLayout(layout, content,"LastClosed");
                File.WriteAllText("Layout.xml", layout);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
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

        private void SaveLayout(string layoutName,Action Close)
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
                string layout = this.DockManager.SaveLayout();
                StringBuilder contentBld = new StringBuilder();
                contentBld.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                contentBld.Append("<Content>");
                var array = this.DockManager.GetPanes(PaneNavigationOrder.VisibleOrder);
                foreach (ContentPane item in array)
                {
                    if (item.Name.ToLower() != "leftedgedock" && item.Name.ToLower() != "rightedgedock")
                    {
                        contentBld.AppendFormat("<ContentPane Name=\"{0}\"/>", item.Name);
                    }
                }
                contentBld.Append("</Content>");
                string content = contentBld.ToString();
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Information, layout);
                ConsoleClient.Instance.SaveLayout(layout, content, layoutName);
                Close();
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
                this.LoadLayout(layout, content);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "EndLoadLayout.\r\n{0}", ex.ToString());
            }

        }

        #region Event
        private void AttachEvent()
        {
            ConsoleClient.Instance.MessageClient.QuoteOrderToDealerEvent += this.MessageClient_QuoteOrderReceived;
            ConsoleClient.Instance.MessageClient.HitPriceEvent += this.MessageClient_HitPriceReceived;
        }
        #endregion
        #region Notify Dealing

        void MessageClient_QuoteOrderReceived(PlaceMessage placeMessage)
        {
            this.Dispatcher.BeginInvoke((Action<PlaceMessage>)delegate(PlaceMessage message)
            {
                this.InitDataManager.AddPlaceMessage(message);

                ContentPane contentPane = this.DockManager.ActivePane;
                TreeViewItem moduleNode = (TreeViewItem)this.FunctionTree.SelectedItem;
                if (moduleNode == null || moduleNode.Tag == null) return;
                int moduleId = (int)moduleNode.Tag;

                if (moduleId == 13)
                {
 
                }

            }, placeMessage);
        }

        
        void MessageClient_HitPriceReceived(HitMessage hitMessage)
        {
            this.Dispatcher.BeginInvoke((Action<HitMessage>)delegate(HitMessage message)
            {
                this.InitDataManager.AddHitMessage(message);

                ContentPane contentPane = this.DockManager.ActivePane;
                TreeViewItem moduleNode = (TreeViewItem)this.FunctionTree.SelectedItem;
                if (moduleNode == null || moduleNode.Tag == null) return;
                int moduleId = (int)moduleNode.Tag;

                if (moduleId == 15)
                {
                    this._LMTProcess = (LMTProcess)MainWindowHelper.GetControl(ModuleType.LimitBatchProcess);
                    this._LMTProcess.RefreshUI();
                }
             
                //foreach (ContentPane panel in this.DockManager.Panes[3].Panes)
                //{
                //    if (panel.Name == "module13")
                //    {
                //        this._OrderTaskControl = (OrderTaskControl)(panel.Content);
                //        this._OrderTaskControl.RefreshUI();
                //    }
                //}

                //this._OrderTaskControl = (OrderTaskControl)MainWindowHelper.GetControl(ModuleType.OrderTask);
                //this._OrderTaskControl.RefreshUI();

            }, hitMessage);
        }
        
        #endregion

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
            ConsoleClient.Instance.LoadLayout("LastClosed", this.EndLoadLayout);
        }

        private void LoadLayout(string dockLayout, string contentLayout)
        {
            if (dockLayout != null)
            {
                this.Dispatcher.BeginInvoke((Action<string>)delegate(string layout)
                {
                    XDocument xdocument = XDocument.Parse(dockLayout);
                    var panes = xdocument.Element("xamDockManager").Element("contentPanes").Elements("contentPane").Where(p => p.Attribute("name").Value != "FunctionTreePane");
                    foreach (XElement pane in panes)
                    {
                        int moduleType = MainWindowHelper.GetModuleType(pane.Attribute("name").Value);
                        if (this._Modules.ContainsKey(moduleType))
                        {
                            bool isAdd = false;
                            string paneName = MainWindowHelper.GetPaneName(moduleType);
                            ContentPane contentPane = this.DockManager.GetPanes(PaneNavigationOrder.ActivationOrder).Where(p => p.Name == paneName).SingleOrDefault();
                            if (contentPane == null)
                            {
                                isAdd = true;
                            }
                            if (isAdd)
                            {
                                this.AddContentPane(moduleType);
                            }
                        }
                    }
                    this.DockManager.LoadLayout(dockLayout);
                }, dockLayout);
            }
        }

        private void XamMenuItem_Click_1(object sender, EventArgs e)
        {
            this._ConsoleClient.MessageClient.SendMessage(new Message());
        }
    }
}
