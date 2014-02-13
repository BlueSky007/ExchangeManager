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
using ManagerConsole.ViewModel;
using Infragistics.Controls.Grids;
using Manager.Common.QuotationEntities;
using ManagerConsole.Model;
using Manager.Common;

namespace ManagerConsole.UI
{
    /// <summary>
    /// Interaction logic for SourceRelationControl.xaml
    /// </summary>
    public partial class SourceRelationControl : UserControl
    {
        public SourceRelationControl()
        {
            InitializeComponent();
            this.Loaded += SourceRelationControl_Loaded;
        }

        public void BindToInstrument(VmInstrument instrument)
        {
            if (instrument == null || instrument.IsDerivative)
            {
                this.DataContext = this.RelationGrid.ItemsSource = null;
                this.AddRelationButton.Visibility = Visibility.Hidden;
            }
            else
            {
                this.DataContext = instrument;
                this.RelationGrid.ItemsSource = instrument.SourceRelations;
                this.AddRelationButton.Visibility = Visibility.Visible;
            }
        }
        private bool CanModify
        {
            get
            {
                return Principal.Instance.HasPermission(ModuleCategoryType.Quotation, ModuleType.QuotationMonitor, OperationCode.Modify);
            }
        }
        private void SourceRelationControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.RelationGrid.Columns["Id"].Visibility = this.CanModify ? Visibility.Visible : Visibility.Collapsed;
            this.AddRelationButton.IsEnabled = this.CanModify;
            if (!this.CanModify)
            {
                (this.RelationGrid.Columns["AdjustPoints"] as TemplateColumn).ItemTemplate = this.Resources["AdjustPointsTemplate"] as DataTemplate;
            }
            foreach (var column in this.RelationGrid.Columns)
            {
                TextColumn textColumn = column as TextColumn;
                if (textColumn != null) textColumn.IsReadOnly = !this.CanModify;
            }
        }
        private void AdjustButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            VmInstrumentSourceRelation vmRelation = (VmInstrumentSourceRelation)button.Tag;
            if (button.Name == "IncButton")
            {
                vmRelation.AdjustPoints += vmRelation.AdjustIncrement;
            }
            else
            {
                vmRelation.AdjustPoints -= vmRelation.AdjustIncrement;
            }
        }
        private void RelationGrid_CellDoubleClicked(object sender, CellClickedEventArgs e)
        {
            if (!this.CanModify) return;
            if (e.Cell.Column.Key == "QuotationSource.Name" || e.Cell.Column.Key == FieldSR.SourceSymbol)
            {
                VmInstrumentSourceRelation vmRelation = e.Cell.Row.Data as VmInstrumentSourceRelation;
                if (vmRelation != null)
                {
                    InstrumentSourceRelationWindow window = new InstrumentSourceRelationWindow((VmInstrument)this.DataContext, EditMode.Modify, vmRelation);
                    App.MainFrameWindow.MainFrame.Children.Add(window);
                    window.IsModal = true;
                    window.Show();
                }
            }
        }

        private void RelationGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                Row selectedRow = this.RelationGrid.Rows.SingleOrDefault(r => r.IsSelected);
                if (selectedRow != null)
                {
                    VmInstrumentSourceRelation vmRelation = selectedRow.Data as VmInstrumentSourceRelation;
                    if (vmRelation != null)
                    {
                        if (MessageBox.Show(App.MainFrameWindow, string.Format("Confirm delete InstrumentSourceRelation for SourceSymbol:{0}?", vmRelation.SourceSymbol), "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning, MessageBoxResult.Cancel) == MessageBoxResult.OK)
                        {
                            ConsoleClient.Instance.DeleteMetadataObject(MetadataType.InstrumentSourceRelation, vmRelation.Id, delegate(bool success)
                            {
                                this.Dispatcher.BeginInvoke((Action<bool>)delegate(bool deleted)
                                {
                                    VmInstrument vmInstrument = (VmInstrument)this.DataContext;
                                    vmInstrument.SourceRelations.Remove(vmRelation);
                                }, success);
                            });
                        }
                    }
                }
            }
        }

        private void DeleteRelation_Click(object sender, RoutedEventArgs e)
        {
            VmInstrumentSourceRelation vmRelation = (VmInstrumentSourceRelation)((Button)sender).Tag;
            if (MessageBox.Show(App.MainFrameWindow, string.Format("Confirm delete relation from SourceSymbol: {0}?", vmRelation.SourceSymbol), "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning, MessageBoxResult.Cancel) == MessageBoxResult.OK)
            {
                ConsoleClient.Instance.DeleteMetadataObject(MetadataType.InstrumentSourceRelation, vmRelation.Id, delegate(bool succss)
                {
                    this.Dispatcher.BeginInvoke((Action<bool>)delegate(bool deleted)
                    {
                        if (deleted)
                        {
                            ((VmInstrument)this.DataContext).SourceRelations.Remove(vmRelation);
                        }
                    }, succss);
                });
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

        private void AddRelationButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext != null)
            {
                Button button = (Button)sender;
                InstrumentSourceRelationWindow window = new InstrumentSourceRelationWindow((VmInstrument)this.DataContext, EditMode.AddNew);
                App.MainFrameWindow.MainFrame.Children.Add(window);
                window.IsModal = true;
                window.Show();
            }
        }
    }
}
