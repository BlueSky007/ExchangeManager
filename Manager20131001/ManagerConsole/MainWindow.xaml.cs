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

namespace ManagerConsole
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Dictionary<int, Module> _Modules = new Dictionary<int, Module>();
        private ConsoleClient _ConsoleClient = new ConsoleClient();

        public MainWindow()
        {
            InitializeComponent();
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
            // initialize layout
            FunctionTree functionTree = ConsoleClient.Instance.GetFunctionTree();
            for (int i = 0; i < functionTree.Modules.Count; i++)
            {
                this._Modules.Add((int)functionTree.Modules[i].Type, functionTree.Modules[i]);
            }
            if (result.DockLayout !=null)
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
                    catalogNode = new TreeViewItem() { Header = functionTree.Categories.Single<Category>(c=>c.CategoryType == module.Category).CategoryDescription };
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
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Information, layout);
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
    }
}
