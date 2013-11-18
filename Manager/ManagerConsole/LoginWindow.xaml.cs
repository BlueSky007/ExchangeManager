﻿using System;
using System.IO;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Linq;
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
using Infragistics.Controls.Interactions;
using ManagerConsole.Model;
using System.Configuration;
using Manager.Common;
using System.Diagnostics;

namespace ManagerConsole
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : XamDialogWindow
    {
        private Action<LoginResult> _LoginSuccssAction;

        public LoginWindow(Action<LoginResult> loginSuccssAction)
        {
            InitializeComponent();
            this._LoginSuccssAction = loginSuccssAction;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string[] servers = ConfigurationManager.AppSettings["Servers"].Split(';');
            for (int i = 0; i < servers.Length; i++)
			{
                this.ServerComboBox.Items.Add(servers[i]);
			}
            if (servers.Length > 0) this.ServerComboBox.SelectedIndex = 0;

            this.UILanguage.ItemsSource = Enum.GetValues(typeof(Language)).Cast<Language>();
            this.UILanguage.SelectedIndex = 0;
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Call Update Service client here, Pass server address(IP,Port) to update service client
            //       and then wait until the update client process terminated.
            //       If there're no updates, then continue to login. otherwise the update client will terminate the Manager process.
            if (this.UILanguage.SelectedItem != null)
            {
                this.LoginButton.IsEnabled = false;
                string server;
                int? port;
                string updateServicePort = ConfigurationManager.AppSettings["UpdateServicePort"];
                string defaultServicePort = ConfigurationManager.AppSettings["DefaultServicePort"];
                if (this.TryGetServerAndPort(out server, out port))
                {
                    if (!port.HasValue) port = int.Parse(defaultServicePort);
                    ConsoleClient.Instance.Login(this.EndLogin, server, port.Value, this.UserNameTextBox.Text, this.PasswordTextBox.Password, (Language)this.UILanguage.SelectedItem);
                }
                else
                {
                    this.HintMessage.Text = "Invalid server.";
                }
            }
            else
            {
                this.HintMessage.Text = "Please Select Language";
            }
        }

        private void EndLogin(LoginResult loginResult, string errorMessage)
        {
            this.Dispatcher.BeginInvoke((Action<LoginResult>)delegate(LoginResult result)
            {
                if (result != null && result.Succeeded)
                {
                    this.SaveServerSettings();
                    this._LoginSuccssAction(result);
                    this.LoginButton.IsEnabled = true;
                    this.Close();
                }
                else
                {
                    this.HintMessage.Text = errorMessage;
                    this.LoginButton.IsEnabled = true;
                }
            }, loginResult
            );
        }

        private void SaveServerSettings()
        {
            string[] servers = ConfigurationManager.AppSettings["Servers"].Split(';');
            string newServer = this.ServerComboBox.Text.Trim();
            bool isNewServer = !servers.Any(s => s == newServer);
            string serverConfig = string.Empty;

            if (isNewServer)
            {
                serverConfig = newServer + ";";
                for (int i = 0; i < servers.Length; i++) serverConfig += servers[i] + ";";
            }
            else if (servers[0] != newServer)
            {
                serverConfig = newServer + ";";
                for (int i = 0; i < servers.Length; i++)
                {
                    if(servers[i] != newServer) serverConfig += servers[i] + ";";
                }
            }
            if (serverConfig != string.Empty)
            {
                XDocument xDocument = XDocument.Load(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
                XElement configElement = xDocument.Element("configuration").Element("appSettings").Elements("add").Single(e => e.Attribute("key").Value == "Servers");
                configElement.Attribute("value").Value = serverConfig;
                xDocument.Save(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
            }
        }

        private bool TryGetServerAndPort(out string server, out int? port)
        {
            server = string.Empty;
            port = null;
            try
            {
                string serverAndPort = this.ServerComboBox.Text;
                if (serverAndPort.IndexOf(':') >= 0)
                {
                    string[] stringArray = serverAndPort.Split(':');
                    server = stringArray[0];
                    port = int.Parse(stringArray[1]);
                }
                else
                {
                    server = serverAndPort;
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(TraceEventType.Error, "LoginWindow.GetServerAndPort error:\r\n{0}", ex.ToString());
            }
            return false;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            App.Current.Shutdown();
        }
    }
}
