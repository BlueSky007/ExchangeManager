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
    public class PriceHelper
    {
        public static void GetSendPrice(string adjustPriceText, int decimalPlace, string ask, string bid, out decimal sendAsk, out decimal sendBid)
        {
            sendAsk = sendBid = 0;
            decimal spread = decimal.Parse(ask) - decimal.Parse(bid);
            bid = new string('0', adjustPriceText.Length) + bid;
            int offset = adjustPriceText.IndexOf('.');
            if (decimalPlace > 0)
            {
                if (bid.IndexOf('.') < 0) bid += '.';
                bid += new string('0', decimalPlace);
                bid = PriceHelper.Cut(bid, decimalPlace);

                if (offset < 0)
                {
                    if (adjustPriceText.Length > decimalPlace)
                    {
                        adjustPriceText = adjustPriceText.Substring(0, decimalPlace);
                    }
                    bid = bid.Substring(0, bid.Length - adjustPriceText.Length) + adjustPriceText;
                }
                else
                {
                    adjustPriceText = PriceHelper.Cut(adjustPriceText, decimalPlace);

                    int startIndex = bid.IndexOf('.') - offset;
                    bid = bid.Substring(0, startIndex) + adjustPriceText + bid.Substring(startIndex + adjustPriceText.Length);
                }
            }
            else
            {
                if (offset == 0) throw new ArgumentException();
                if (offset > 0) adjustPriceText = adjustPriceText.Substring(0, offset);
                if (bid.IndexOf('.') >= 0)
                {
                    bid = bid.Substring(0, bid.IndexOf('.'));
                }
                bid = bid.Substring(0, bid.Length - adjustPriceText.Length) + adjustPriceText;
            }
            sendBid = decimal.Parse(bid);
            sendAsk = sendBid + spread;
        }

        public static string Cut(string value, int decimalPlace)
        {
            int position = value.IndexOf('.') + 1;
            if (position > 0)
            {
                if(value.Length - position > decimalPlace)
                {
                    value = value.Substring(0, position + decimalPlace);
                }
            }
            return value;
        }
    }
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

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            Button sendButton = (Button)sender;
            TextBox priceTextBox = (TextBox)LogicalTreeHelper.FindLogicalNode(LogicalTreeHelper.GetParent(sendButton), "AdjustPrice");

            string adjustPriceText = priceTextBox.Text.Trim();
            double value;
            if (double.TryParse(adjustPriceText, out value))
            {
                VmInstrument instrument = (VmInstrument)priceTextBox.Tag;
                VmInstrumentSourceRelation relation = instrument.SourceRelations.SingleOrDefault(r => r.IsActive);
                if (relation != null)
                {
                    try
                    {
                        decimal sendAsk, sendBid;
                        if (string.IsNullOrEmpty(instrument.Ask) || string.IsNullOrEmpty(instrument.Bid))
                        {
                            ConsoleClient.Instance.SendQuotation(relation.Id, value, value);
                        }
                        else
                        {
                            PriceHelper.GetSendPrice(adjustPriceText, instrument.DecimalPlace, instrument.Ask, instrument.Bid, out sendAsk, out sendBid);
                            ConsoleClient.Instance.SendQuotation(relation.Id, (double)sendAsk, (double)sendBid);
                        }
                    }
                    catch
                    {
                        priceTextBox.BorderBrush = Brushes.Red;
                    }
                }
            }
            else
            {
                priceTextBox.BorderBrush = Brushes.Red;
            }
        }

        private void AdjustPrice_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            textBox.BorderBrush = Brushes.Gray;
        }

        private void AdjustPrice_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox.Tag != null)
            {
                VmInstrument vmInstrument = (VmInstrument)textBox.Tag;
                textBox.Text = vmInstrument.Bid;
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

        private void AdjustPrice_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox bidTextBox = (TextBox)sender;
                Button sendButton = (Button)LogicalTreeHelper.FindLogicalNode(LogicalTreeHelper.GetParent(bidTextBox), "SendButton");
                this.SendButton_Click(sendButton, null);
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
            InstrumentWindow window = new InstrumentWindow(EditMode.AddNew);
            App.MainWindow.MainFrame.Children.Add(window);
            window.IsModal = true;
            window.Show();
        }

        private void EditInstrument_Click(object sender, RoutedEventArgs e)
        {
            Row selectedRow = this.MonitorGrid.Rows.SingleOrDefault(r => r.IsSelected);
            if (selectedRow != null)
            {
                VmInstrument vmInstrument = (VmInstrument)selectedRow.Data;
                InstrumentWindow window = new InstrumentWindow(EditMode.Modify, vmInstrument);
                App.MainWindow.MainFrame.Children.Add(window);
                window.IsModal = true;
                window.Show();
            }
        }

        private void AdjustButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            VmInstrument vmInstrument = (VmInstrument)button.Tag;
            if(button.Name == "IncButton")
            {
                vmInstrument.AdjustPoints += vmInstrument.AdjustIncrement;
            }
            else
            {
                vmInstrument.AdjustPoints -= vmInstrument.AdjustIncrement;
            }
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox textBox = (TextBox)sender;
                BindingExpression be = textBox.GetBindingExpression(TextBox.TextProperty);
                be.UpdateSource();
            }
        }
    }
}
