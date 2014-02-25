using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
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
using Infragistics;
using Infragistics.Controls.Grids;
using Infragistics.Controls.Interactions;
using Manager.Common;
using Manager.Common.QuotationEntities;
using ManagerConsole.Helper;
using ManagerConsole.Model;
using ManagerConsole.ViewModel;

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
            this.Loaded += QuotationMonitorControl_Loaded;
        }

        private bool CanModify
        {
            get
            {
                return Principal.Instance.HasPermission(ModuleCategoryType.Quotation, ModuleType.QuotationMonitor, OperationCode.Modify);
            }
        }

        private void QuotationMonitorControl_Loaded(object sender, RoutedEventArgs e)
        {
            if(this.MonitorGrid.Rows.Count > 0 && !this.MonitorGrid.Rows.Any(r=>r.IsSelected))
            {
                this.MonitorGrid.Rows[0].IsSelected = true;
                this.SelectRow(this.MonitorGrid.Rows[0].Data as VmInstrument);
            }

            this.ToolBarPanel.Visibility = this.MonitorGrid.Columns["Id"].Visibility = this.MonitorGrid.Columns["TempProperty"].Visibility = this.CanModify ? Visibility.Visible : Visibility.Hidden;
            this.CoverGrid.Visibility = this.CanModify ? Visibility.Hidden : Visibility.Visible;
            foreach(var column in this.MonitorGrid.Columns)
            {
                TextColumn textColumn = column as TextColumn;
                if (textColumn != null) textColumn.IsReadOnly = !this.CanModify;
            }
            if (!this.CanModify)
            {
                (this.MonitorGrid.Columns["AdjustPoints"] as TemplateColumn).ItemTemplate = this.Resources["AdjustPointsTemplate"] as DataTemplate;
            }
        }
        private void ShowRelation(object state)
        {
            this.Dispatcher.BeginInvoke((Action)delegate()
            {
                if (App.MainFrameWindow.SourceQuotationControl != null) App.MainFrameWindow.SourceQuotationControl.BindToInstrument(this._CurrentVmInstrument);
                if (App.MainFrameWindow.SourceRelationControl != null) App.MainFrameWindow.SourceRelationControl.BindToInstrument(this._CurrentVmInstrument);
            });
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
                this.SelectRow(e.NewSelectedItems[0].Data as VmInstrument);
            }
        }

        private void SelectRow(VmInstrument vmInstrument)
        {
            this._CurrentVmInstrument = vmInstrument;
            if (this._CurrentVmInstrument != null)
            {
                this.DataContext = this._CurrentVmInstrument;
                this._Timer.Change(500, Timeout.Infinite);
            }
        }

        private void AddInstrument_Click(object sender, RoutedEventArgs e)
        {
            InstrumentWindow window = new InstrumentWindow(EditMode.AddNew);
            App.MainFrameWindow.MainFrame.Children.Add(window);
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
                App.MainFrameWindow.MainFrame.Children.Add(window);
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
                if (MessageBox.Show(App.MainFrameWindow, string.Format("Confirm delete Instrument:{0}?", vmInstrument.Code), "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning, MessageBoxResult.Cancel) == MessageBoxResult.OK)
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
                                if (App.MainFrameWindow.SourceQuotationControl != null) App.MainFrameWindow.SourceQuotationControl.BindToInstrument(null);
                                if (App.MainFrameWindow.SourceRelationControl != null) App.MainFrameWindow.SourceRelationControl.BindToInstrument(null);
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
            App.MainFrameWindow.MainFrame.Children.Add(window);
            window.IsModal = true;
            window.Show();
        }

        /// <summary>
        /// Layout format:
        ///   <Fitler LogicalOperator="Or(And)" ><Condition op="=" value="XAU"/><Condition op="!=" value="XAUD"/></Fitler>
        ///   <Spliter width0=""/>
        /// </summary>
        /// <returns></returns>
        public string GetLayout()
        {
            StringBuilder layoutBuilder = new StringBuilder();
            if (this.MonitorGrid.FilteringSettings.RowFiltersCollection.Count > 0)
            {
                IRecordFilter rowsFilter = this.MonitorGrid.FilteringSettings.RowFiltersCollection[0];
                if (rowsFilter.FieldName == "Code")
                {
                    
                    layoutBuilder.AppendFormat("<Fitler LogicalOperator=\"{0}\">", (int)rowsFilter.Conditions.LogicalOperator);
                    foreach (IFilterCondition condition in rowsFilter.Conditions)
                    {
                        ComparisonCondition comparisonCondition = condition as ComparisonCondition;
                        if (comparisonCondition != null)
                        {
                            layoutBuilder.AppendFormat("<Condition op=\"{0}\" val=\"{1}\"/>", (int)comparisonCondition.Operator, comparisonCondition.FilterValue);
                        }
                    }
                    layoutBuilder.Append("</Fitler>");
                }
            }
            layoutBuilder.AppendFormat("<Spliter width0=\"{0}\"/>", this.MainGrid.ColumnDefinitions[0].ActualWidth);
            layoutBuilder.Append(ColumnWidthPersistence.GetPersistentColumnsWidthString(this.MonitorGrid));
            return layoutBuilder.ToString();
        }

        public void SetLayout(XElement layout)
        {
            try
            {
                if (layout.HasElements)
                {
                    XElement filterElement = layout.Element("Fitler");
                    if (filterElement != null)
                    {
                        RowsFilter rowsFilter = new RowsFilter(typeof(string), this.MonitorGrid.Columns.DataColumns["Code"]);
                        rowsFilter.Conditions.LogicalOperator = (LogicalOperator)int.Parse(filterElement.Attribute("LogicalOperator").Value);

                        foreach (XElement element in filterElement.Elements("Condition"))
                        {
                            rowsFilter.Conditions.Add(new ComparisonCondition() { FilterValue = element.Attribute("val").Value, Operator = (ComparisonOperator)int.Parse(element.Attribute("op").Value) });
                        }
                        this.MonitorGrid.FilteringSettings.RowFiltersCollection.Add(rowsFilter);
                    }
                    XElement spliterElement = layout.Element("Spliter");
                    if(spliterElement != null)
                    {
                        this.MainGrid.ColumnDefinitions[0].Width = new GridLength(double.Parse(spliterElement.Attribute("width0").Value));
                    }
                    ColumnWidthPersistence.LoadColumnsWidth(this.MonitorGrid, layout);
                }
            }
            catch (Exception ex)
            {
                Logger.AddEvent(System.Diagnostics.TraceEventType.Error, "QuotationMonitorControl.SetLayout\r\n{0}", ex.ToString());
            }
        }

        private void Grid_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            RowFiltersCollection collection = this.MonitorGrid.FilteringSettings.RowFiltersCollection;
        }

        private void SuspendResumeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button button = (Button)sender;
                VmInstrument vmInstrument = (VmInstrument)button.Tag;
                ConsoleClient.Instance.SuspendResume(new int[] { vmInstrument.Id }, button.Name == "ResumeButton");
            }
            catch (Exception exception)
            {
                Logger.AddEvent(System.Diagnostics.TraceEventType.Error, "QuotationMonitorControl.SuspendResumeButton_Click\r\n{0}", exception);
            }
        }

        private void SuspendResumeAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button Button = (Button)sender;
                List<int> instrumentIds = new List<int>();
                foreach (var row in this.MonitorGrid.Rows)
                {
                    VmInstrument vmInstrument = (VmInstrument)row.Data;
                    instrumentIds.Add(vmInstrument.Id);
                }
                ConsoleClient.Instance.SuspendResume(instrumentIds.ToArray(), Button.Name == "ResumeAll");
            }
            catch (Exception exception)
            {
                Logger.AddEvent(System.Diagnostics.TraceEventType.Error, "QuotationMonitorControl.SuspendResumeAll_Click\r\n{0}", exception);
            }
        }
    }
}
