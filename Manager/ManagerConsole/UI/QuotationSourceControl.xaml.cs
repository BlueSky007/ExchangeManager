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
using Manager.Common.QuotationEntities;
using ManagerConsole.Model;
using ManagerConsole.ViewModel;

namespace ManagerConsole.UI
{
    /// <summary>
    /// Interaction logic for QuotationSourceControl.xaml
    /// </summary>
    public partial class QuotationSourceControl : UserControl
    {
        private VmQuotationManager _QuotationConfigData;
        private VmQuotationSource _EditingOrigin;
        private VmQuotationSource _EditingSource;

        public QuotationSourceControl()
        {
            InitializeComponent();
            this.Loaded += QuotationSourceControl_Loaded;
        }

        private void QuotationSourceControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataGrid.ItemsSource = VmQuotationManager.Instance.QuotationSources;
            this._QuotationConfigData = VmQuotationManager.Instance;
            //Uri themeUri = new Uri("/Infragistics.Themes.Office2010BlueTheme;component/Office2010Blue.xamGrid.xaml", UriKind.Relative);
            //this.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = themeUri });
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            this.AddNewDialog.Visibility = Visibility.Visible;
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            Button deleteButton = (Button)sender;
            this._EditingOrigin = (VmQuotationSource)deleteButton.Tag;
            this._EditingSource = this._EditingOrigin.Clone();
            this.AddNewDialog.DataContext = this._EditingSource;
            this.AddNewDialog.Visibility = Visibility.Visible;
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            Button deleteButton = (Button)sender;
            VmQuotationSource quotationSource = (VmQuotationSource)deleteButton.Tag;
            if (MessageBox.Show(App.MainFrameWindow, string.Format("确认删除{0}吗？", quotationSource.Name), "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning, MessageBoxResult.Cancel) == MessageBoxResult.OK)
            {
                ConsoleClient.Instance.DeleteMetadataObject(MetadataType.QuotationSource, quotationSource.Id, delegate(bool deleted)
                {
                    this.Dispatcher.BeginInvoke((Action<bool>)delegate(bool deleted2)
                    {
                        if (deleted2)
                        {
                            MessageBox.Show(App.MainFrameWindow, "删除成功！");
                            this._QuotationConfigData.RemoveQuotationSource(quotationSource.Id);
                        }
                        else
                        {
                            MessageBox.Show(App.MainFrameWindow, "删除失败！");
                        }
                    }, deleted);
                });
            }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (this._EditingSource == null)
            {
                QuotationSource source = new QuotationSource();
                source.Name = this.NameTextBox.Text;
                source.AuthName = this.AuthNameTextBox.Text;
                source.Password = this.PasswordTextBox.Text;

                ConsoleClient.Instance.AddMetadataObject(source, delegate(int objectId)
                {
                    this.Dispatcher.BeginInvoke((Action)delegate()
                    {
                        if (objectId > 0)
                        {
                            source.Id = objectId;
                            this._QuotationConfigData.Add(source);
                            this.HintTextBlock.Foreground = Brushes.Green;
                            this.HintTextBlock.Text = "Add success.";
                            this.NameTextBox.Text = string.Empty;
                            this.AuthNameTextBox.Text = string.Empty;
                            this.PasswordTextBox.Text = string.Empty;
                        }
                        else
                        {
                            this.HintTextBlock.Foreground = Brushes.Red;
                            this.HintTextBlock.Text = "Add failed.";
                        }
                    });
                });
            }
            else
            {
                Dictionary<string, object> fieldAndValues = new Dictionary<string, object>();
                if (this._EditingOrigin.Name != this._EditingSource.Name)
                {
                    fieldAndValues.Add(FieldSR.Name, this._EditingSource.Name);
                }
                if (this._EditingOrigin.AuthName != this._EditingSource.AuthName)
                {
                    fieldAndValues.Add(FieldSR.AuthName, this._EditingSource.AuthName);
                }
                if (this._EditingOrigin.Password != this._EditingSource.Password)
                {
                    fieldAndValues.Add(FieldSR.Password, this._EditingSource.Password);
                }
                if (fieldAndValues.Count > 0)
                {
                    ConsoleClient.Instance.UpdateMetadataObject(MetadataType.QuotationSource, this._EditingSource.Id, fieldAndValues, delegate(bool success)
                    {
                        if (success)
                        {
                            this._EditingOrigin.ApplyChangeToUI(fieldAndValues);
                            this.ShowSuccessHint("Update Successful.");
                        }
                        else
                        {
                            this.ShowSuccessHint("Update failed.");
                        }
                    });
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.AddNewDialog.Visibility = Visibility.Collapsed;
            this.AddNewDialog.DataContext = null;
            this._EditingSource = null;
            this._EditingOrigin = null;
        }

        private void ShowSuccessHint(string message)
        {
            this.HintTextBlock.Foreground = Brushes.Green;
            this.HintTextBlock.Text = message;
        }

        private void ShowFailedHint(string message)
        {
            this.HintTextBlock.Foreground = Brushes.Red;
            this.HintTextBlock.Text = message;
        }

        private void EditText_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.HintTextBlock.Text = string.Empty;
        }
    }
}
