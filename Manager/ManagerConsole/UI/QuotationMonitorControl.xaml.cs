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
using Infragistics.Controls.Grids;
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
            this.MonitorGrid.ItemsSource = VmQuotationManager.Instance.Instruments;
            this.MonitorGrid.SelectionSettings.CellClickAction = Infragistics.Controls.Grids.CellSelectionAction.SelectRow;
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

        private void MonitorGrid_CellDoubleClicked(object sender, Infragistics.Controls.Grids.CellClickedEventArgs e)
        {
            if (e.Cell.Column.Key == FieldSR.Code)
            {
                VmInstrument vmInstrument = (VmInstrument)e.Cell.Row.Data;
                App.MainWindow.SourceQuotationControl.BindToInstrument(vmInstrument);
            }
        }

        private void SendBid_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                TextBox bidTextBox = (TextBox)sender;
                Button sendButton = (Button)LogicalTreeHelper.FindLogicalNode(LogicalTreeHelper.GetParent(bidTextBox), "SendButton");
                this.SendButton_Click(sendButton, null);
            }
        }

        private void SendAsk_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox askTextBox = (TextBox)sender;
                TextBox bidTextBox = (TextBox)LogicalTreeHelper.FindLogicalNode(LogicalTreeHelper.GetParent(askTextBox), "SendBid");
                bidTextBox.Focus();
            }
        }

        private void MonitorGrid_SelectedRowsCollectionChanged(object sender, SelectionCollectionChangedEventArgs<SelectedRowsCollection> e)
        {
            if (e.NewSelectedItems.Count > 0)
            {
                VmInstrument instrument = e.NewSelectedItems[0].Data as VmInstrument;
                if (instrument != null)
                {
                    this.InstrumentCodeTextBlock.Text = instrument.Code;
                    this.RangeCheckRuleControl.DataContext = instrument.VmPriceRangeCheckRule;
                    this.WeightedRuleControl.DataContext = instrument.VmWeightedPriceRule;
                }
            }
        }

        private void AddInstrument_Click(object sender, RoutedEventArgs e)
        {
            InstrumentWindow window = new InstrumentWindow();
            App.MainWindow.MainFrame.Children.Add(window);
            window.IsModal = true;
            window.Show();
        }

        private void AddRelation_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            InstrumentSourceRelationWindow window = new InstrumentSourceRelationWindow((VmInstrument)button.Tag);
            App.MainWindow.MainFrame.Children.Add(window);
            window.IsModal = true;
            window.Show();
        }
    }
}
