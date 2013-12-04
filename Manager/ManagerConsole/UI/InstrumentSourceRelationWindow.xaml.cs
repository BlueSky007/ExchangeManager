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
using Infragistics.Controls.Interactions;
using ManagerConsole.Model;
using ManagerConsole.ViewModel;
using Manager.Common.QuotationEntities;

namespace ManagerConsole.UI
{
    /// <summary>
    /// Interaction logic for InstrumentSourceRelationWindow.xaml
    /// </summary>
    public partial class InstrumentSourceRelationWindow : XamDialogWindow
    {
        private HintMessage _HintMessage;
        private VmInstrument _vmInstrument;
        private VmInstrumentSourceRelation _vmRelation;
        public InstrumentSourceRelationWindow(VmInstrument instrument)
        {
            InitializeComponent();
            this._vmInstrument = instrument;

            IEnumerable<int> sourceIds = instrument.SourceRelations.Select(r=>r.SourceId);
            this.SourcesComboBox.ItemsSource = from s in VmQuotationManager.Instance.QuotationSources where !sourceIds.Contains(s.Id) select s;
            this._vmRelation = new VmInstrumentSourceRelation(new InstrumentSourceRelation() { InstrumentId = this._vmInstrument.Id, IsActive = false, SwitchTimeout = 3 }, this._vmInstrument, null);
            this.DataContext = this._vmRelation;

            this._HintMessage = new HintMessage(this.HintTextBlock);
            this.Loaded += InstrumentSourceRelationWindow_Loaded;
        }

        private void InstrumentSourceRelationWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.SourcesComboBox.SelectedIndex = 0;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrEmpty(this._vmRelation.SourceSymbol))
            {
                this._HintMessage.ShowError("Source Symbol can not be empty.");
                this.SourceSymbolBox.Focus();
                return;
            }
            if(Validation.GetHasError(this.SwitchTimeoutBox))
            {
                this._HintMessage.ShowError("Please provide a number for SwitchTimeout.");
                this.SwitchTimeoutBox.Focus();
                return;
            }
            if (this._vmRelation.SwitchTimeout < 3 || this._vmRelation.SwitchTimeout > 600)
            {
                this._HintMessage.ShowError("SwitchTimeout must between 3 and 600.");
                this.SwitchTimeoutBox.Focus();
                return;
            }

            ConsoleClient.Instance.AddMetadataObject(this._vmRelation.InstrumentSourceRelation, delegate(int relationId)
            {
                if(relationId != 0)
                {
                    this.Dispatcher.BeginInvoke((Action<int>)delegate(int id)
                    {
                        this._vmRelation.InstrumentSourceRelation.Id = id;
                        VmQuotationManager.Instance.Add(this._vmRelation.InstrumentSourceRelation);
                        this._HintMessage.ShowSucess("Add Instrument Source Relation successfully");
                    }, relationId);
                }
            });
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Grid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            this._HintMessage.Clear();
        }
    }
}
