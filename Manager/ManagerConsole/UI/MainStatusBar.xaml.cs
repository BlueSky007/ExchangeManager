using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Manager.Common;
using ManagerConsole.Helper;
using ManagerConsole.ViewModel;


namespace ManagerConsole.UI
{
    /// <summary>
    /// Interaction logic for MainStatusBar.xaml
    /// </summary>
    public partial class MainStatusBar : StatusBar
    {
        private class ExchangeConnectionState : PropertyChangedNotifier
        {
            private ConnectionState _ConnectionState = ConnectionState.Unknown;
            public string ExchangeCode { get; set; }
            public ConnectionState ConnectionState
            {
                get { return this._ConnectionState; }
                set
                {
                    if (this._ConnectionState != value)
                    {
                        this._ConnectionState = value;
                        this.OnPropertyChanged("ConnectionState");
                    }
                }
            }
        }

        private ObservableCollection<ExchangeConnectionState> _ExchangeConnectionStates;

        public MainStatusBar()
        {
            InitializeComponent();
            this._ExchangeConnectionStates = new ObservableCollection<ExchangeConnectionState>();
        }

        public void ShowUserConnectionState(ConnectionState connectionState)
        {
            this.Dispatcher.BeginInvoke((Action<ConnectionState>)delegate(ConnectionState state)
            {
                switch (state)
                {
                    case ConnectionState.Unknown:
                        this.UserConnectStateImage.Source = ImageSources.Instance.UnknownImage;
                        break;
                    case ConnectionState.Disconnected:
                        this.UserConnectStateImage.Source = ImageSources.Instance.UserDisconnectedImage;
                        break;
                    case ConnectionState.Connecting:
                        this.UserConnectStateImage.Source = ImageSources.Instance.ConnectingImage;
                        break;
                    case ConnectionState.Connected:
                        this.UserConnectStateImage.Source = ImageSources.Instance.UserConnectedImage;
                        break;
                    default:
                        break;
                }
            }, connectionState);
        }

        public void ShowLoginUser(string userName)
        {
            this.Dispatcher.BeginInvoke((Action<string>)delegate(string user)
            {
                this.LoginUserNameTextBlock.Text = user;
            }, userName);
        }

        public void HandleSuccessLogin(LoginResult result)
        {
            this.SourceConnectionStateControl.ItemsSource = VmQuotationManager.Instance.QuotationSources; 
            foreach (string sourceName in result.SourceConnectionStates.Keys)
            {
                VmQuotationManager.Instance.QuotationSources.Single(s => s.Name == sourceName).ConnectionState = result.SourceConnectionStates[sourceName];
            }

            this._ExchangeConnectionStates.Clear();
            this.ExchangeConnectionStateControl.ItemsSource = this._ExchangeConnectionStates;
            foreach (string name in result.ExchangeConnectionStates.Keys)
            {
                this._ExchangeConnectionStates.Add(new ExchangeConnectionState() { ExchangeCode = name, ConnectionState = result.ExchangeConnectionStates[name] });
            }
        }

        public void Process(ExchangeConnectionStatusMessage message)
        {
            this.Dispatcher.BeginInvoke((Action<ExchangeConnectionStatusMessage>)delegate(ExchangeConnectionStatusMessage statusMessage)
            {
                this._ExchangeConnectionStates.Single(e => e.ExchangeCode == statusMessage.ExchangeCode).ConnectionState = statusMessage.ConnectionState;
            }, message);
        }

        private void UserConnectStateImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //VmQuotationManager.Instance.QuotationSources.First().ConnectionState = ConnectionState.Connected;
        }
    }
}
