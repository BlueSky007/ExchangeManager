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
using System.Reflection;
using Infragistics.Controls.Interactions;
using ManagerConsole.Model;
using ManagerConsole.ViewModel;
using Manager.Common.QuotationEntities;

namespace ManagerConsole.UI
{
    /// <summary>
    /// Interaction logic for InstrumentWindow.xaml
    /// </summary>
    public partial class InstrumentWindow : XamDialogWindow
    {
        private EditMode _EditMode;
        private HintMessage _HintMessage;
        private InstrumentData _InstrumentData;
        private VmInstrument _vmInstrument;

        public InstrumentWindow(EditMode editMode, VmInstrument vmInstrument =  null)
        {
            InitializeComponent();
            this._EditMode = editMode;
            this._vmInstrument = vmInstrument;
            this._InstrumentData = new InstrumentData();
            if (editMode == EditMode.AddNew)
            {
                this._InstrumentData.Instrument = new Instrument() { IsDerivative = false, IsSwitchUseAgio = false, UseWeightedPrice = false };
                this._InstrumentData.PriceRangeCheckRule = new PriceRangeCheckRule();
                this._InstrumentData.WeightedPriceRule = new WeightedPriceRule();
            }
            else
            {
                this._InstrumentData.Instrument = vmInstrument.Instrument.Clone();
                this._InstrumentData.PriceRangeCheckRule = vmInstrument.VmPriceRangeCheckRule.PriceRangeCheckRule.Clone();
                this._InstrumentData.WeightedPriceRule = vmInstrument.VmWeightedPriceRule.WeightedPriceRule.Clone();
            }
            this.DataContext = this._InstrumentData;
            this._HintMessage = new HintMessage(this.HintTextBlock);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private bool IsValid(DependencyObject obj)
        {
            return !Validation.GetHasError(obj) && LogicalTreeHelper.GetChildren(obj).OfType<DependencyObject>().All(child => this.IsValid(child));
        }

        private bool RequireCheck(TextBox textBox,string errorMessage)
        {
            if(textBox.Text.Trim() == string.Empty)
            {
                this._HintMessage.ShowError(errorMessage);
                textBox.Focus();
                return true;
            }
            return false;
        }
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.RequireCheck(this.CodeBox, "Instrument Code can not be empty.")) return;
            if (this.RequireCheck(this.DecimalPlaceBox, "DecimalPlace can not be empty.")) return;
            if (this.RequireCheck(this.InactiveTimeBox, "InactiveTime can not be empty.")) return;
            if (this._InstrumentData.Instrument.IsSwitchUseAgio.HasValue && this._InstrumentData.Instrument.IsSwitchUseAgio.Value)
            {
                if (this.RequireCheck(this.AgioSecondsBox, "Please provide AgioSeconds.")) return;
                if (this.RequireCheck(this.LeastTicksBox, "Please provide LeastTicks.")) return;
            }
            if (!this.IsValid(this))
            {
                this._HintMessage.ShowError("Please provide appropriate data.");
                return;
            }

            if (this._EditMode == EditMode.AddNew)
            {
                ConsoleClient.Instance.AddInstrument(this._InstrumentData, delegate(int instrumentId)
                {
                    this.Dispatcher.BeginInvoke((Action<int>)delegate(int id)
                    {
                        if (id != 0)
                        {
                            this._InstrumentData.Instrument.Id = id;
                            VmQuotationManager.Instance.Add(this._InstrumentData.Instrument);

                            this._InstrumentData.PriceRangeCheckRule.Id = id;
                            VmQuotationManager.Instance.Add(this._InstrumentData.PriceRangeCheckRule);

                            this._InstrumentData.WeightedPriceRule.Id = id;
                            VmQuotationManager.Instance.Add(this._InstrumentData.WeightedPriceRule);

                            this._HintMessage.ShowSucess("Add Instrument successfully.");
                        }
                        else
                        {
                            this._HintMessage.ShowSucess("Add Instrument failed.");
                        }
                    }, instrumentId);
                });
            }
            else
            {
                Dictionary<string, object> instrumentUpdates = new Dictionary<string,object>();
                this.GetUpdates(typeof(Instrument), this._vmInstrument.Instrument, this._InstrumentData.Instrument, instrumentUpdates);

                Dictionary<string, object> rangeRuleUpdates = new Dictionary<string,object>();
                this.GetUpdates(typeof(PriceRangeCheckRule), this._vmInstrument.VmPriceRangeCheckRule.PriceRangeCheckRule, this._InstrumentData.PriceRangeCheckRule, rangeRuleUpdates);

                Dictionary<string, object> weightRuleUpdates = new Dictionary<string,object>();
                if (this._InstrumentData.Instrument.UseWeightedPrice.HasValue && this._InstrumentData.Instrument.UseWeightedPrice.Value)
                {
                    this.GetUpdates(typeof(WeightedPriceRule), this._vmInstrument.VmWeightedPriceRule.WeightedPriceRule, this._InstrumentData.WeightedPriceRule, weightRuleUpdates);
                }

                List<UpdateData> updateDatas = new List<UpdateData>();
                if (instrumentUpdates.Count > 0)
                {
                    updateDatas.Add(new UpdateData
                    {
                        MetadataType = MetadataType.Instrument,
                        ObjectId = this._InstrumentData.Instrument.Id,
                        FieldsAndValues = instrumentUpdates
                    });
                }
                if (rangeRuleUpdates.Count > 0)
                {
                    updateDatas.Add(new UpdateData
                    {
                        MetadataType = MetadataType.PriceRangeCheckRule,
                        ObjectId = this._InstrumentData.PriceRangeCheckRule.Id,
                        FieldsAndValues = rangeRuleUpdates
                    });
                }
                if (weightRuleUpdates.Count > 0)
                {
                    updateDatas.Add(new UpdateData
                    {
                        MetadataType = MetadataType.WeightedPriceRule,
                        ObjectId = this._InstrumentData.WeightedPriceRule.Id,
                        FieldsAndValues = weightRuleUpdates
                    });
                }

                if (updateDatas.Count > 0)
                {
                    ConsoleClient.Instance.UpdateMetadataObjects(updateDatas.ToArray(), delegate(bool success)
                    {
                        this.Dispatcher.BeginInvoke((Action<bool>)delegate(bool updated)
                        {
                            if (updated)
                            {
                                if (instrumentUpdates.Count > 0) this._vmInstrument.ApplyModification(instrumentUpdates);
                                if (rangeRuleUpdates.Count > 0) this._vmInstrument.VmPriceRangeCheckRule.ApplyModification(rangeRuleUpdates);
                                if (weightRuleUpdates.Count > 0) this._vmInstrument.VmWeightedPriceRule.ApplyModification(weightRuleUpdates);
                                this._HintMessage.ShowSucess("Update Instrument successfully.");
                            }
                            else
                            {
                                this._HintMessage.ShowError("Update Instrument failed.");
                            }
                        }, success);
                    });
                }
                else
                {
                    this._HintMessage.ShowError("Nothing changed.");
                }
            }
        }

        private void GetUpdates(Type type, object oldObject, object newObject, Dictionary<string, object> updates)
        {
            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo propertyInfo in properties)
            {
                if (propertyInfo.Name != FieldSR.Id)
                {
                    object oldValue = type.GetProperty(propertyInfo.Name).GetValue(oldObject, null);
                    object newValue = type.GetProperty(propertyInfo.Name).GetValue(newObject, null);
                    if (newValue != null && !newValue.Equals(oldValue))
                    {
                        updates.Add(propertyInfo.Name, newValue);
                    }
                }
            }
        }

        private void Grid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            this._HintMessage.Clear();
        }
    }
}
