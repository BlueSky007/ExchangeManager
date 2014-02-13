using Manager.Common;
using System;
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

namespace ManagerConsole.UI
{
    /// <summary>
    /// Interaction logic for ConnectionStateControl.xaml
    /// </summary>
    public partial class ConnectionStateControl : UserControl
    {
        public static readonly DependencyProperty StateProperty =
            DependencyProperty.Register("State", typeof(ConnectionState), typeof(ConnectionStateControl), new PropertyMetadata(new PropertyChangedCallback(HandleConnectionStateChanged)));

        public ConnectionStateControl()
        {
            InitializeComponent();
            this.StateImage.Source = ImageSources.Instance.UnknownImage;
        }
        public ConnectionState State
        {
            get
            {
                return (ConnectionState)this.GetValue(ConnectionStateControl.StateProperty);
            }
            set
            {
                this.SetValue(ConnectionStateControl.StateProperty, value);
            }
        }
        private static void HandleConnectionStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ConnectionStateControl connectionStateControl = (ConnectionStateControl)d;
            ConnectionState connectionState = (ConnectionState)e.NewValue;
            switch (connectionState)
            {
                case ConnectionState.Unknown:
                    connectionStateControl.StateImage.Source = ImageSources.Instance.UnknownImage;
                    break;
                case ConnectionState.Disconnected:
                    connectionStateControl.StateImage.Source = ImageSources.Instance.DisconnectedImage;
                    break;
                case ConnectionState.Connecting:
                    connectionStateControl.StateImage.Source = ImageSources.Instance.ConnectingImage;
                    break;
                case ConnectionState.Connected:
                    connectionStateControl.StateImage.Source = ImageSources.Instance.ConnectedImage;
                    break;
                default:
                    break;
            }
        }
    }
}
