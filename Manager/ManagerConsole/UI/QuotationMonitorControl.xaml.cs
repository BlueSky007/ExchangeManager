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
using Manager.Common.QuotationEntities;
using ManagerConsole.ViewModel;
using ManagerConsole.Model;

namespace ManagerConsole.UI
{
    /// <summary>
    /// Interaction logic for QuotationMonitorControl.xaml
    /// </summary>
    public partial class QuotationMonitorControl : UserControl
    {
        public QuotationMonitorControl()
        {
            InitializeComponent();
            this.Loaded += QuotationMonitorControl_Loaded;
        }

        void QuotationMonitorControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.PropertyDataGrid.DataSource = new[] { new VmWeightedPriceRule(new WeightedPriceRule(){
                 Id=1, AskAdjust=2, AskAskWeight=22, AskAvarageWeight=20, AskBidWeight=89, AskLastWeight=87,
                 BidAdjust=56, BidAskWeight=43, BidAvarageWeight=9, BidBidWeight=3, BidLastWeight=8,
                 LastAdjust=34, LastAskWeight=5, LastAvarageWeight=2, LastBidWeight=5, LastLastWeight=6, Multiplier=63
            }) };
            this.MonitorGrid.ItemsSource = VmQuotationManager.Instance.Instruments;
            this.MonitorGrid.SelectionSettings.CellClickAction = Infragistics.Controls.Grids.CellSelectionAction.SelectRow;
        }
        private void Add_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            Button deleteButton = (Button)sender;
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            Button deleteButton = (Button)sender;
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            Button sendButton = (Button)sender;
            TextBox askTextBox = (TextBox)LogicalTreeHelper.FindLogicalNode(LogicalTreeHelper.GetParent(sendButton), "SendAsk");
            TextBox bidTextBox = (TextBox)LogicalTreeHelper.FindLogicalNode(LogicalTreeHelper.GetParent(sendButton), "SendBid");

            double ask, bid;
            if (double.TryParse(askTextBox.Text, out ask))
            {
                if(double.TryParse(bidTextBox.Text, out bid))
                {
                    ConsoleClient.Instance.SendQuotation((int)sendButton.Tag, ask, bid);
                }
                else
                {
                    bidTextBox.BorderBrush = Brushes.Red;
                }
            }
            else
            {
                askTextBox.BorderBrush = Brushes.Red;
            }
        }

        private void SendTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            textBox.BorderBrush = Brushes.Gray;
        }

        private void SendTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            double value;
            if(!double.TryParse(textBox.Text, out value))
            {
                if (textBox.Tag != null)
                {
                    textBox.Text = textBox.Tag.ToString();
                }
            }
        }

        private void SendTextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox.Tag != null)
            {
                textBox.Text = textBox.Tag.ToString();
            }
        }
    }
}
