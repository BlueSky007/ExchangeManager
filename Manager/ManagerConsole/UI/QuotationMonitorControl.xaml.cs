using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using Manager.Common;
using Infragistics.Controls.Interactions;
using Infragistics;

namespace ManagerConsole.UI
{
    /// <summary>
    /// Interaction logic for QuotationMonitorControl.xaml
    /// </summary>
    public partial class QuotationMonitorControl : UserControl, IControlLayout
    {
        private Timer _Timer;
        private VmInstrument _CurrentVmInstrument;

        public QuotationMonitorControl()
        {
            InitializeComponent();
            this.MonitorGrid.ItemsSource = VmQuotationManager.Instance.Instruments;
            this._Timer = new Timer(this.ShowRelation);
            this.MonitorGrid.Filtering += MonitorGrid_Filtering;
            this.MonitorGrid.Filtered += MonitorGrid_Filtered;

            RowsFilter rowsFilter = new RowsFilter(typeof(string), this.MonitorGrid.Columns.DataColumns["Code"]);
            rowsFilter.Conditions.LogicalOperator = LogicalOperator.Or;
            rowsFilter.Conditions.Add(new ComparisonCondition() { FilterValue = "XAU", Operator= ComparisonOperator.Equals });
            rowsFilter.Conditions.Add(new ComparisonCondition() { FilterValue = "XAUD", Operator = ComparisonOperator.Equals });
            this.MonitorGrid.FilteringSettings.RowFiltersCollection.Add(rowsFilter);
        }

        void MonitorGrid_Filtering(object sender, CancellableFilteringEventArgs e)
        {
            e.Cancel = false;
        }

        void MonitorGrid_Filtered(object sender, FilteredEventArgs e)
        {
            RowFiltersCollection collection = e.RowFiltersCollection;
        }

        private void ShowRelation(object state)
        {
            this.Dispatcher.BeginInvoke((Action)delegate()
            {
                if (App.MainWindow.SourceQuotationControl != null) App.MainWindow.SourceQuotationControl.BindToInstrument(this._CurrentVmInstrument);
                if (App.MainWindow.SourceRelationControl != null) App.MainWindow.SourceRelationControl.BindToInstrument(this._CurrentVmInstrument);
            });
        }
        private void AdjustPrice_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            textBox.BorderBrush = Brushes.Gray;
            //this.MonitorGrid.
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

        private void AdjustPrice_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox priceTextBox = (TextBox)sender;
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
                                sendAsk -= (decimal)(Manager.Common.Helper.GetAdjustValue(instrument.AdjustPoints + relation.AdjustPoints, instrument.DecimalPlace));
                                sendBid -= (decimal)(Manager.Common.Helper.GetAdjustValue(instrument.AdjustPoints + relation.AdjustPoints, instrument.DecimalPlace));
                                ConsoleClient.Instance.SendQuotation(relation.Id, (double)sendAsk, (double)sendBid);
                            }
                        }
                        catch
                        {
                            priceTextBox.BorderBrush = Brushes.Red;
                        }
                    }
                    else
                    {
                        MessageBox.Show("No active source.");
                    }
                }
                else
                {
                    priceTextBox.BorderBrush = Brushes.Red;
                }

            }
        }

        private void MonitorGrid_SelectedRowsCollectionChanged(object sender, SelectionCollectionChangedEventArgs<SelectedRowsCollection> e)
        {
            if (e.NewSelectedItems.Count > 0)
            {
                this._CurrentVmInstrument = e.NewSelectedItems[0].Data as VmInstrument;
                if (this._CurrentVmInstrument != null)
                {
                    this.DataContext = this._CurrentVmInstrument;
                    this._Timer.Change(500, Timeout.Infinite);
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
                XamDialogWindow window;
                if(vmInstrument.IsDerivative)
                {
                    window = new DerivedInstrumentWindow(EditMode.Modify, vmInstrument);
                }
                else
                {
                    window = new InstrumentWindow(EditMode.Modify, vmInstrument);
                }
                App.MainWindow.MainFrame.Children.Add(window);
                window.IsModal = true;
                window.Show();
            }
        }

        private void DeleteInstrument_Click(object sender, RoutedEventArgs e)
        {
            Row selectedRow = this.MonitorGrid.Rows.SingleOrDefault(r => r.IsSelected);
            if (selectedRow != null)
            {
                VmInstrument vmInstrument = (VmInstrument)selectedRow.Data;
                if (MessageBox.Show(App.MainWindow, string.Format("Confirm delete Instrument:{0}?", vmInstrument.Code), "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning, MessageBoxResult.Cancel) == MessageBoxResult.OK)
                {
                    ConsoleClient.Instance.DeleteMetadataObject(MetadataType.Instrument, vmInstrument.Id, delegate(bool success)
                    {
                        this.Dispatcher.BeginInvoke((Action<bool>)delegate(bool deleted)
                        {
                            if (deleted)
                            {
                                VmQuotationManager.Instance.Delete(new DeleteMetadataObjectMessage() { MetadataType = MetadataType.Instrument, ObjectId = vmInstrument.Id });

                                if (this.MonitorGrid.Rows.Count > 0)
                                {
                                    //int newIndex = (selectedRow.Index == 0) ? 0 : selectedRow.Index - 1;
                                }
                                if (App.MainWindow.SourceQuotationControl != null) App.MainWindow.SourceQuotationControl.BindToInstrument(null);
                                if (App.MainWindow.SourceRelationControl != null) App.MainWindow.SourceRelationControl.BindToInstrument(null);
                                this.DataContext = null;
                            }
                        }, success);
                    });
                }
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

        private void AddDeriveInstrument_Click(object sender, RoutedEventArgs e)
        {
            DerivedInstrumentWindow window = new DerivedInstrumentWindow(EditMode.AddNew);
            App.MainWindow.MainFrame.Children.Add(window);
            window.IsModal = true;
            window.Show();
        }

        public string GetLayout()
        {
            throw new NotImplementedException();
        }

        public void SetLayout(string layout)
        {
            throw new NotImplementedException();
        }
    }
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
                if (value.Length - position > decimalPlace)
                {
                    value = value.Substring(0, position + decimalPlace);
                }
            }
            return value;
        }
    }
}
