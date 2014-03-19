using Infragistics.Controls.Interactions;
using ManagerConsole.Model;
using ManagerConsole.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using CommonBlotterSelection = Manager.Common.ReportEntities.BlotterSelection;

namespace ManagerConsole.UI
{
    /// <summary>
    /// Interaction logic for BlotterSelectionControl.xaml
    /// </summary>
    public partial class BlotterSelectionControl : XamDialogWindow
    {
        private MainWindow _App;
        public delegate void ConfirmBlotterResultHandle(string[] blotterCodes);
        public event ConfirmBlotterResultHandle OnBlotterResultHandle;

        private List<string> _BlotterCodes;

        private ReportDataManager _ReportDataManager;
        private ObservableCollection<BlotterSelection> _BlotterSelectionList; 
        private string _ExchangeCode;
        public BlotterSelectionControl(string exchangeCode)
        {
            InitializeComponent();

            this._App = (MainWindow)Application.Current.MainWindow;
            this._ExchangeCode = exchangeCode;
            this._ReportDataManager = this._App.ExchangeDataManager.ReportDataManager;
            if (!this._ReportDataManager.ExchangeBlotterDict.ContainsKey(exchangeCode))
            {
                this._BlotterSelectionList = new ObservableCollection<BlotterSelection>();
                ConsoleClient.Instance.GetBlotterList(exchangeCode, GetBlotterListCallback);
            }
            else
            {
                this._BlotterSelectionList = this._ReportDataManager.GetBlotterCodeList(exchangeCode);
            }

            this.BlotterGrid.ItemsSource = this._BlotterSelectionList;
        }

        private void SelectAllBtn_Click(object sender, RoutedEventArgs e)
        {
            this.SellAllOrClearBlotter(true);
        }

        private void ClearAllBtn_Click(object sender, RoutedEventArgs e)
        {
            this.SellAllOrClearBlotter(false);
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this._BlotterCodes = this.GetSelectedBlotters();


            if (this.OnBlotterResultHandle != null)
            {
                this.OnBlotterResultHandle(this._BlotterCodes.ToArray());
            }

            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SellAllOrClearBlotter(bool isSelected)
        {
            foreach (BlotterSelection blotter in this._BlotterSelectionList)
            {
                blotter.IsSelected = isSelected;
            }
        }

        private List<string> GetSelectedBlotters()
        {
            List<string> blotterCodes = new List<string>();
            foreach (BlotterSelection blotter in this._BlotterSelectionList)
            {
                if (blotter.IsSelected)
                {
                    if (blotter.Code == "Null")
                    {
                        blotterCodes.Add(null);
                    }
                    else
                    {
                        blotterCodes.Add(blotter.Code);
                    }
                }
            }
            return blotterCodes;
        }

        private void GetBlotterListCallback(List<CommonBlotterSelection> blotterCodeList)
        {
            this.Dispatcher.BeginInvoke((Action<List<CommonBlotterSelection>>)delegate(List<CommonBlotterSelection> result) 
            {
                BlotterSelection nullBlotter = new BlotterSelection();
                nullBlotter.Id = Guid.Empty;
                nullBlotter.Code = "Null";
                this._BlotterSelectionList.Add(nullBlotter);
                foreach (CommonBlotterSelection entity in result)
                {
                    BlotterSelection blotterSelection = new BlotterSelection(entity);
                    this._BlotterSelectionList.Add(blotterSelection);
                }
                this._ReportDataManager.ExchangeBlotterDict.Add(this._ExchangeCode, this._BlotterSelectionList);
                this.BlotterGrid.ItemsSource = this._BlotterSelectionList;
            }, blotterCodeList);
        }
    }
}
