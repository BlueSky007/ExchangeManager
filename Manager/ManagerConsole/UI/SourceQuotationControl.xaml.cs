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
            if (instrument == null || instrument.IsDerivative)
            {
                this.DataContext = null;
            }
            else
            {
                if (!object.ReferenceEquals(this.DataContext, instrument))
                {
                    this.DataContext = instrument;
                }
            }
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
    }
}
