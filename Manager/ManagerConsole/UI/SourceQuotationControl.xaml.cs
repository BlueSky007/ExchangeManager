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
using ManagerConsole.Model;
using Manager.Common.QuotationEntities;

namespace ManagerConsole.UI
{
    /// <summary>
    /// Interaction logic for SourceQuotationControl.xaml
    /// </summary>
    public partial class SourceQuotationControl : UserControl
    {
        public SourceQuotationControl()
        {
            InitializeComponent();
        }

        public void BindToInstrument(VmInstrument instrument)
        {
            this.DataContext = instrument;
            this.RelationGrid.ItemsSource = instrument.SourceRelations;
        }

        private void XamGrid_SelectedRowsCollectionChanged(object sender, SelectionCollectionChangedEventArgs<SelectedRowsCollection> e)
        {
            if(e.NewSelectedItems.Count >0)
            {
                int selectedIndex = e.NewSelectedItems[0].Index;
                for (int i = 0; i < this.QuotationControl.Items.Count; i++)
                {
                    DependencyObject itemContainer = QuotationControl.ItemContainerGenerator.ContainerFromIndex(i);
                    this.SelectRow(itemContainer, selectedIndex);
                }
            }
        }

        private void SelectRow(DependencyObject uiObject, int selectedIndex)
        {
            int count = VisualTreeHelper.GetChildrenCount(uiObject);
            for (int i = 0; i < count; i++)
            {
                DependencyObject obj = VisualTreeHelper.GetChild(uiObject, i);
                XamGrid grid = obj as XamGrid;
                if (grid != null)
                {
                    grid.Rows[selectedIndex].IsSelected = true;
                }
                else
                {
                    this.SelectRow(obj, selectedIndex);
                }
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

        private void AddRelationButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext != null)
            {
                Button button = (Button)sender;
                InstrumentSourceRelationWindow window = new InstrumentSourceRelationWindow((VmInstrument)this.DataContext, EditMode.AddNew);
                App.MainWindow.MainFrame.Children.Add(window);
                window.IsModal = true;
                window.Show();
            }
        }

        private void RelationGrid_CellDoubleClicked(object sender, CellClickedEventArgs e)
        {
            if (e.Cell.Column.Key == "QuotationSource.Name" || e.Cell.Column.Key == FieldSR.SourceSymbol)
            {
                VmInstrumentSourceRelation vmRelation = e.Cell.Row.Data as VmInstrumentSourceRelation;
                if (vmRelation != null)
                {
                    InstrumentSourceRelationWindow window = new InstrumentSourceRelationWindow((VmInstrument)this.DataContext, EditMode.Modify, vmRelation);
                    App.MainWindow.MainFrame.Children.Add(window);
                    window.IsModal = true;
                    window.Show();
                }
            }
        }

        private void RelationGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Delete)
            {
                Row selectedRow = this.RelationGrid.Rows.SingleOrDefault(r => r.IsSelected);
                if (selectedRow != null)
                {
                    VmInstrumentSourceRelation vmRelation = selectedRow.Data as VmInstrumentSourceRelation;
                    if (vmRelation != null)
                    {
                        if (MessageBox.Show(App.MainWindow, string.Format("Confirm delete InstrumentSourceRelation for SourceSymbol:{0}?", vmRelation.SourceSymbol), "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning, MessageBoxResult.Cancel) == MessageBoxResult.OK)
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
    }
}
