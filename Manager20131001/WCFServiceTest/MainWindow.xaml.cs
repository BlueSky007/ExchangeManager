using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using iExchange.Common;
using iExchange.StateServer.Manager;

namespace WCFServiceTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            string server = this.ServerTextBox.Text.Trim();
            int port = int.Parse(this.PortTextBox.Text);
            string serverAddress = string.Format("net.tcp://{0}:{1}/Service", server, port);
            ManagerClient.Start(serverAddress, this.ExchangCodeTextBox.Text);
        }

        //询价Command
        private void SendQuoteCommandBtn_Click(object sender, RoutedEventArgs e)
        {
            string exchangeCode = this.QuoteCommand.Text;
            int commandSequence = 1;

            QuoteCommand quoteCommand = new QuoteCommand(commandSequence);
            quoteCommand.InstrumentID = Guid.NewGuid();
            quoteCommand.QuoteLot = 10;
            quoteCommand.CustomerID = Guid.NewGuid();
            quoteCommand.BSStatus = 1;
            ManagerClient.AddCommand(quoteCommand);
        }

        //批单Command
        private void SendQuoteOrderCommandBtn_Click(object sender, RoutedEventArgs e)
        {
            string exchangeCode = this.QuoteOrderCommandText.Text;
        }
    }
}
