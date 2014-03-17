using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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

namespace ManagerConsole.UI
{
    /// <summary>
    /// Interaction logic for DerivedInstrumentControl.xaml
    /// </summary>
    public partial class DerivedInstrumentControl : UserControl
    {
        public class IdCode
        {
            public int? Id { get; set; }
            public string Code { get; set; }
        }

        public DerivedInstrumentControl()
        {
            InitializeComponent();
            this.Loaded += DerivedInstrumentControl_Loaded;
            VmQuotationManager.Instance.Instruments.CollectionChanged += Instruments_CollectionChanged;
        }

        private void Instruments_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            List<IdCode> instruments = new List<IdCode>();
            instruments.Add(new IdCode { Id = null, Code = "(None)" });
            foreach (VmInstrument item in VmQuotationManager.Instance.Instruments)
            {
                instruments.Add(new IdCode { Id = item.Id, Code = item.Code });
            }
            Instrument2ComboBox.ItemsSource = instruments;
        }

        private void DerivedInstrumentControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Instruments_CollectionChanged(null, null);
            Instrument1ComboBox.ItemsSource = VmQuotationManager.Instance.Instruments;
        }

        private void Instrument2ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                IdCode idCode = e.AddedItems[0] as IdCode;
                if (idCode != null)
                {
                    AskOperator1TypeComboBox.IsEnabled = idCode.Id != null;
                    BidOperator1TypeComboBox.IsEnabled = AskOperator1TypeComboBox.IsEnabled;
                    LastOperator1TypeComboBox.IsEnabled = AskOperator1TypeComboBox.IsEnabled;
                    AskOperand2TypeComboBox.IsEnabled = AskOperator1TypeComboBox.IsEnabled;
                    BidOperand2TypeComboBox.IsEnabled = AskOperator1TypeComboBox.IsEnabled;
                    LastOperand2TypeComboBox.IsEnabled = AskOperator1TypeComboBox.IsEnabled;
                }
            }
        }
    }
}
