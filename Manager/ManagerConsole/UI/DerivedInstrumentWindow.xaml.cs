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
using Manager.Common.QuotationEntities;
using ManagerConsole.ViewModel;
using ManagerConsole.Model;

namespace ManagerConsole.UI
{
    /// <summary>
    /// Interaction logic for DerivedInstrumentWindow.xaml
    /// </summary>
    public partial class DerivedInstrumentWindow : XamDialogWindow
    {
        private EditMode _EditMode;
        private HintMessage _HintMessage;
        private InstrumentData _InstrumentData;
        private VmInstrument _vmInstrument;

        public DerivedInstrumentWindow(EditMode editMode, VmInstrument vmInstrument = null)
        {
            InitializeComponent();
            this._EditMode = editMode;
            this._vmInstrument = vmInstrument;
            this._InstrumentData = new InstrumentData();
            if (editMode == EditMode.AddNew)
            {
                this._InstrumentData.Instrument = new Instrument() { IsDerivative = true };
                this._InstrumentData.DerivativeRelation = new DerivativeRelation()
                {
                    AskOperand1Type = OperandType.Ask,
                    AskOperand2Type = OperandType.Ask,
                    AskOperator1Type = OperatorType.Multiply,
                    AskOperator2Type = OperatorType.Multiply,
                    AskOperand3 = 1,
                    BidOperand1Type = OperandType.Bid,
                    BidOperand2Type = OperandType.Bid,
                    BidOperator1Type = OperatorType.Multiply,
                    BidOperator2Type = OperatorType.Multiply,
                    BidOperand3 = 1,
                    LastOperand1Type = OperandType.Last,
                    LastOperand2Type = OperandType.Last,
                    LastOperator1Type = OperatorType.Multiply,
                    LastOperator2Type = OperatorType.Multiply,
                    LastOperand3 = 1,
                    UnderlyingInstrument1IdInverted = false,
                    UnderlyingInstrument2Id = null
                };
                if(VmQuotationManager.Instance.Instruments.Count>0)
                {
                    this._InstrumentData.DerivativeRelation.UnderlyingInstrument1Id = VmQuotationManager.Instance.Instruments[0].Id;
                }
            }
            else
            {
                this._InstrumentData.Instrument = vmInstrument.Instrument.Clone();
                this._InstrumentData.DerivativeRelation = vmInstrument.VmDerivativeRelation.DerivativeRelation.Clone();
            }

            this.DataContext = this._InstrumentData;
            this._HintMessage = new HintMessage(this.HintTextBlock);
        }
        private bool RequireCheck(TextBox textBox, string errorMessage)
        {
            if (textBox.Text.Trim() == string.Empty)
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
            if (!VmBase.IsValid(this))
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
                            VmQuotationManager.Instance.Add(this._InstrumentData.Instrument.Clone());

                            this._InstrumentData.DerivativeRelation.Id = id;
                            VmQuotationManager.Instance.Add(this._InstrumentData.DerivativeRelation.Clone());

                            this.CancelButton.Content = "Close";
                            this._HintMessage.ShowMessage("Add Instrument successfully.");
                        }
                        else
                        {
                            this._HintMessage.ShowMessage("Add Instrument failed.");
                        }
                    }, instrumentId);
                });
            }
            else
            {
                Dictionary<string, object> instrumentUpdates = new Dictionary<string, object>();
                VmBase.GetUpdates(typeof(Instrument), this._vmInstrument.Instrument, this._InstrumentData.Instrument, instrumentUpdates);

                Dictionary<string, object> derivativeRelationUpdates = new Dictionary<string, object>();
                VmBase.GetUpdates(typeof(DerivativeRelation), this._vmInstrument.VmDerivativeRelation.DerivativeRelation, this._InstrumentData.DerivativeRelation, derivativeRelationUpdates);

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
                if (derivativeRelationUpdates.Count > 0)
                {
                    updateDatas.Add(new UpdateData
                    {
                        MetadataType = MetadataType.DerivativeRelation,
                        ObjectId = this._InstrumentData.DerivativeRelation.Id,
                        FieldsAndValues = derivativeRelationUpdates
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
                                if (instrumentUpdates.Count > 0) this._vmInstrument.ApplyChangeToUI(instrumentUpdates);
                                if (derivativeRelationUpdates.Count > 0) this._vmInstrument.VmDerivativeRelation.ApplyChangeToUI(derivativeRelationUpdates);
                                this.CancelButton.Content = "Close";
                                this._HintMessage.ShowMessage("Update Instrument successfully.");
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

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
