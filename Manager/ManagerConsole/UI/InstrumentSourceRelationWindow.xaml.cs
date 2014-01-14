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
        private EditMode _EditMode;
        private HintMessage _HintMessage;
        private VmInstrument _vmInstrument;
        private InstrumentSourceRelation _Relation;
        private VmInstrumentSourceRelation _originRelation;
        public InstrumentSourceRelationWindow(VmInstrument vmInstrument, EditMode editMode, VmInstrumentSourceRelation vmRelation = null)
        {
            InitializeComponent();
            this._EditMode = editMode;
            this._vmInstrument = vmInstrument;
            this._originRelation = vmRelation;

            if (editMode == EditMode.AddNew)
            {
                this._Relation = new InstrumentSourceRelation() { InstrumentId = this._vmInstrument.Id, SwitchTimeout = 5 };
                this._Relation.IsActive = vmInstrument.SourceRelations.Count == 0;  // Only for UI display(the backend can process normally)
            }
            else
            {
                this._Relation = vmRelation.InstrumentSourceRelation.Clone();
                this.SourcesComboBox.IsEnabled = false;
            }
            InstrumentCodeTextBlock.Text = vmInstrument.Code;
            //if (vmInstrument.HasDefaultSourceRelation)
            //{
            //    this._Relation.IsDefault = false;
            //    this.IsDefaultCheckBox.IsEnabled = false;
            //}
            this.BindSourcesComboBox();
            this.DataContext = this._Relation;

            this._HintMessage = new HintMessage(this.HintTextBlock);
            this.Loaded += InstrumentSourceRelationWindow_Loaded;
        }

        private void BindSourcesComboBox()
        {
            IEnumerable<int> sourceIds = this._vmInstrument.SourceRelations.Select(r => r.SourceId);
            IEnumerable<VmQuotationSource> quotationSource = from s in VmQuotationManager.Instance.QuotationSources where !sourceIds.Contains(s.Id) select s;
            this.SourcesComboBox.ItemsSource = quotationSource;
            if (quotationSource.Count() > 0)
            {
                this.SourcesComboBox.SelectedIndex = 0;
            }
            else
            {
                this.Close();
            }
        }

        private void InstrumentSourceRelationWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.SourcesComboBox.SelectedIndex = 0;
        }

        private bool IsValid(DependencyObject obj)
        {
            return !Validation.GetHasError(obj) && LogicalTreeHelper.GetChildren(obj).OfType<DependencyObject>().All(child => this.IsValid(child));
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrEmpty(this._Relation.SourceSymbol))
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
            if (this._Relation.SwitchTimeout < 5 || this._Relation.SwitchTimeout > 600)
            {
                this._HintMessage.ShowError("SwitchTimeout must between 5 and 600.");
                this.SwitchTimeoutBox.Focus();
                return;
            }

            if(!this.IsValid(this))
            {
                this._HintMessage.ShowError("Please provide appropriate data.");
                return;
            }

            if (this._EditMode == EditMode.AddNew)
            {
                ConsoleClient.Instance.AddMetadataObject(this._Relation, delegate(int relationId)
                {
                    this.Dispatcher.BeginInvoke((Action<int>)delegate(int id)
                    {
                        if (id != 0)
                        {
                            this._Relation.Id = id;
                            VmQuotationManager.Instance.Add(this._Relation.Clone());
                            this.BindSourcesComboBox();
                            this.CancelButton.Content = "Close";
                            this._HintMessage.ShowSucess("Add Instrument Source Relation successfully.");
                        }
                        else
                        {
                            this._HintMessage.ShowError("Add Instrument Source Relation failed.");
                        }
                    }, relationId);
                });
            }
            else
            {
                Dictionary<string, object> fieldAndValues = new Dictionary<string, object>();
                if (this._originRelation.SourceSymbol != this._Relation.SourceSymbol) fieldAndValues.Add(FieldSR.SourceSymbol, this._Relation.SourceSymbol);
                if (this._originRelation.Inverted != this._Relation.Inverted) fieldAndValues.Add(FieldSR.Inverted, this._Relation.Inverted);
                if (this._originRelation.IsDefault != this._Relation.IsDefault) fieldAndValues.Add(FieldSR.IsDefault, this._Relation.IsDefault);
                if (this._originRelation.Priority != this._Relation.Priority) fieldAndValues.Add(FieldSR.Priority, this._Relation.Priority);
                if (this._originRelation.SwitchTimeout != this._Relation.SwitchTimeout) fieldAndValues.Add(FieldSR.SwitchTimeout, this._Relation.SwitchTimeout);
                if (this._originRelation.AdjustPoints != this._Relation.AdjustPoints) fieldAndValues.Add(FieldSR.AdjustPoints, this._Relation.AdjustPoints);
                if (this._originRelation.AdjustIncrement != this._Relation.AdjustIncrement) fieldAndValues.Add(FieldSR.AdjustIncrement, this._Relation.AdjustIncrement);
                if (fieldAndValues.Count > 0)
                {
                    VmInstrumentSourceRelation currentDefaultRelation = this._originRelation.vmInstrument.SourceRelations.SingleOrDefault(r => r.IsDefault == true);
                    if (fieldAndValues.ContainsKey(FieldSR.IsDefault) && this._Relation.IsDefault && currentDefaultRelation != null)
                    {
                        // 将修改的Relation的IsDefault设置为true,且Instrument下有一个其它的Relation的IsDefault为true时:
                        Dictionary<string, object> isDefaultfieldAndValues = new Dictionary<string, object>();
                        isDefaultfieldAndValues.Add(FieldSR.IsDefault, false);
                        UpdateData isDefaultUpdateData = new UpdateData
                        {
                            MetadataType = MetadataType.InstrumentSourceRelation,
                            ObjectId = currentDefaultRelation.Id,
                            FieldsAndValues = isDefaultfieldAndValues
                        };

                        UpdateData relationUpdateData = new UpdateData
                        {
                            MetadataType = MetadataType.InstrumentSourceRelation,
                            ObjectId = this._Relation.Id,
                            FieldsAndValues = fieldAndValues
                        };

                        ConsoleClient.Instance.UpdateMetadataObjects(new UpdateData[] { relationUpdateData, isDefaultUpdateData }, delegate(bool success)
                        {
                            this.Dispatcher.BeginInvoke((Action<bool>)delegate(bool updated) {
                                if(updated)
                                {
                                    this._originRelation.ApplyChangeToUI(fieldAndValues);
                                    currentDefaultRelation.ApplyChangeToUI(isDefaultfieldAndValues);
                                    this.CancelButton.Content = "Close";
                                    this._HintMessage.ShowSucess("Update Instrument Source Relation successfully.");
                                }
                                else
                                {
                                    this._HintMessage.ShowError("Update Instrument Source Relation failed.");
                                }
                            }, success);
                        });

                    }
                    else
                    {
                        ConsoleClient.Instance.UpdateMetadataObject(MetadataType.InstrumentSourceRelation, this._Relation.Id, fieldAndValues, delegate(bool success)
                        {
                            this.Dispatcher.BeginInvoke((Action<bool>)delegate(bool updated)
                            {
                                if (updated)
                                {
                                    this._originRelation.ApplyChangeToUI(fieldAndValues);
                                    this.CancelButton.Content = "Close";
                                    this._HintMessage.ShowSucess("Update Instrument Source Relation successfully.");
                                }
                                else
                                {
                                    this._HintMessage.ShowError("Update Instrument Source Relation failed.");
                                }
                            }, success);
                        });
                    }
                }
                else
                {
                    this._HintMessage.ShowError("Nothing changed.");
                }
            }
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
