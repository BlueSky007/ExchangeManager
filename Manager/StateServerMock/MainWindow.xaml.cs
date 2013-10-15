﻿using System;
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
using System.Reflection;
using System.Xml;

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

        //Update Command
        private void SendUpdateCommandBtn_Click(object sender, RoutedEventArgs e)
        {
            string appDir = System.IO.Path.Combine(Assembly.GetEntryAssembly().Location.Substring(0, Assembly.GetEntryAssembly().Location.LastIndexOf(System.IO.Path.DirectorySeparatorChar)));
            string rootpath = GetDestDirName(appDir);
            string xmlPath = string.Empty;
            ComboBoxItem item = (ComboBoxItem)this.UpdateNameComboBox.SelectedItem;
            string updateName = item.Content.ToString();

            UpdateCommand updateCommand = new UpdateCommand();
            XmlDocument doc = new XmlDocument();
            switch (updateName)
            {
                case "PrivateDailyQuotation":
                    xmlPath = System.IO.Path.Combine(rootpath, "Commands\\AccountUpdate.xml");
                    break;
                case "SystemParameter":
                    break;
                case "Instruments":
                    break;
                case "Instrument":
                    break;
                case "Account":
                    xmlPath = System.IO.Path.Combine(rootpath, "Commands\\AccountUpdate.xml");
                    break;
                case "Customers":
                    break;
                case "Customer":
                    break;
                case "QuotePolicy":
                    break;
                case "QuotePolicyDetails":
                    break;
                case "QuotePolicyDetail":
                    break;
                case "TradePolicy":
                    break;
                case "TradePolicyDetail":
                    break;
                case "TradePolicyDetails":
                    break;
                case "DealingConsoleInstrument":
                    break;
            }
            doc.Load(xmlPath);
            updateCommand.Content = doc.DocumentElement;
            ManagerClient.AddCommand(updateCommand);
        }

        private string GetCommandXmlPath(string xmlFileName)
        {
            string appDir = System.IO.Path.Combine(Assembly.GetEntryAssembly().Location.Substring(0, Assembly.GetEntryAssembly().Location.LastIndexOf(System.IO.Path.DirectorySeparatorChar)));
            string rootpath = GetDestDirName(appDir);
            string xmlPath = string.Empty;
            xmlPath = System.IO.Path.Combine(rootpath, "Commands\\" + xmlFileName + ".xml");
            return xmlPath;
        }

        private static string GetDestDirName(string appDir)
        {
            string desDirName = string.Empty;
            desDirName = appDir.Substring(0, appDir.LastIndexOf(System.IO.Path.DirectorySeparatorChar));
            desDirName = desDirName.Substring(0, desDirName.LastIndexOf(System.IO.Path.DirectorySeparatorChar));
            return desDirName;
        }

        //Place Open Command /Order Task
        int DQIndex = 0;
        private void PlaceOpenCommandBtn_Click(object sender, RoutedEventArgs e)
        {
            if (DQIndex > 7) return;
            string xmlPath = this.GetCommandXmlPath("PlaceCommand_DQ_Buy");

            XmlDocument doc = new XmlDocument();
            doc.Load(xmlPath);
            XmlNode xmlTran = doc.ChildNodes[1].ChildNodes[DQIndex];

            PlaceCommand placeCommand;
            placeCommand = new PlaceCommand(DQIndex);
            placeCommand.InstrumentID = XmlConvert.ToGuid(xmlTran.Attributes["InstrumentID"].Value);
            placeCommand.AccountID = XmlConvert.ToGuid(xmlTran.Attributes["AccountID"].Value);

            XmlDocument xmlDoc = new XmlDocument();
            XmlNode content = xmlDoc.CreateElement("Place");
            xmlDoc.AppendChild(content);
            placeCommand.Content = content;

            content.AppendChild(xmlDoc.ImportNode(xmlTran, true));

            ManagerClient.AddCommand(placeCommand);
            DQIndex++;
        }
        //PlaceCommand /Order Task For MooMoc Order
        private void MooMocPlaceCommandBtn_Click(object sender, RoutedEventArgs e)
        {
            string xmlPath = string.Empty;
            ComboBoxItem item = (ComboBoxItem)this.MooMocComboBox.SelectedItem;
            string seletName = item.Content.ToString();
            switch (seletName)
            {
                case "MOOBuy":
                    xmlPath = this.GetCommandXmlPath("PlaceCommand_MOO_Buy");
                    break;
                case "MOOSell":
                    xmlPath = this.GetCommandXmlPath("PlaceCommand_MOO_Buy");
                    break;
                case "MOCBuy":
                    xmlPath = this.GetCommandXmlPath("PlaceCommand_MOO_Buy");
                    break;
                case "MOCSell":
                    xmlPath = this.GetCommandXmlPath("PlaceCommand_MOO_Buy");
                    break;
            }
            XmlDocument doc = new XmlDocument();
            doc.Load(xmlPath);
            XmlNode xmlTran = doc.ChildNodes[1].ChildNodes[0];

            PlaceCommand placeCommand;
            placeCommand = new PlaceCommand(1);
            placeCommand.InstrumentID = XmlConvert.ToGuid(xmlTran.Attributes["InstrumentID"].Value);
            placeCommand.AccountID = XmlConvert.ToGuid(xmlTran.Attributes["AccountID"].Value);

            XmlDocument xmlDoc = new XmlDocument();
            XmlNode content = xmlDoc.CreateElement("Place");
            xmlDoc.AppendChild(content);
            placeCommand.Content = content;

            content.AppendChild(xmlDoc.ImportNode(xmlTran, true));

            ManagerClient.AddCommand(placeCommand);
        }
        //PlaceCommand /Order Task For Lmt/Stop Order
        private void LMTCommandBtn_Click(object sender, RoutedEventArgs e)
        {
            string xmlPath = string.Empty;
            ComboBoxItem item = (ComboBoxItem)this.LmtComboBox.SelectedItem;
            string seletName = item.Content.ToString();
            switch (seletName)
            {
                case "LMTBuy":
                    xmlPath = this.GetCommandXmlPath("PlaceCommand_LMT_Sell");
                    break;
                case "LMTSell":
                    xmlPath = this.GetCommandXmlPath("PlaceCommand_LMT_Sell");
                    break;
                case "StopBuy":
                    xmlPath = this.GetCommandXmlPath("PlaceCommand_LMT_Sell");
                    break;
                case "StopSell":
                    xmlPath = this.GetCommandXmlPath("PlaceCommand_LMT_Sell");
                    break;
            }
            XmlDocument doc = new XmlDocument();
            doc.Load(xmlPath);
            XmlNode xmlTran = doc.ChildNodes[1].ChildNodes[0];

            PlaceCommand placeCommand;
            placeCommand = new PlaceCommand(1);
            placeCommand.InstrumentID = XmlConvert.ToGuid(xmlTran.Attributes["InstrumentID"].Value);
            placeCommand.AccountID = XmlConvert.ToGuid(xmlTran.Attributes["AccountID"].Value);

            XmlDocument xmlDoc = new XmlDocument();
            XmlNode content = xmlDoc.CreateElement("Place");
            xmlDoc.AppendChild(content);
            placeCommand.Content = content;

            content.AppendChild(xmlDoc.ImportNode(xmlTran, true));

            ManagerClient.AddCommand(placeCommand);
        }
    }
}
