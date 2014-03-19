using Manager.Common;
using ManagerConsole.FramePages;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ManagerConsole.UI
{
    /// <summary>
    /// Interaction logic for LogAuditControl.xaml
    /// </summary>
    public partial class LogAuditControl : UserControl
    {
        private ObservableCollection<LogAudit> _LogAuditList = new ObservableCollection<LogAudit>();
        public LogAuditControl()
        {
            InitializeComponent();
            this.InitializLogData();
            this.LogGroupComboBox.ItemsSource = System.Enum.GetNames(typeof(LogGroup));
            this.LogGroupComboBox.SelectedIndex = 0;
            this.SetBindingByGroup(LogGroup.TradingLog);
        }

        public void InitializLogData()
        {
            this._LogAuditList.Add(new LogAudit(LogGroup.TradingLog, LogType.QuotePrice));
            this._LogAuditList.Add(new LogAudit(LogGroup.TradingLog, LogType.QuoteOrder));
            this._LogAuditList.Add(new LogAudit(LogGroup.TradingLog, LogType.SettingChange));
            this._LogAuditList.Add(new LogAudit(LogGroup.QuotationLog, LogType.AdjustPrice));
            this._LogAuditList.Add(new LogAudit(LogGroup.QuotationLog, LogType.SourceChange));
            this._LogAuditList.Add(new LogAudit(LogGroup.PermisstionLog, LogType.Permisstion));
        }

        private void SetBindingByGroup(LogGroup logGroup)
        {
            //this._LogRibbon.Theme = "IGTheme";
            this.LogTypeComboBox.ItemsSource = this._LogAuditList.Where(P => P.LogGroup == logGroup);
            this.LogTypeComboBox.DisplayMemberPath = "LogName";
            this.LogTypeComboBox.SelectedIndex = 0;
        }

        private void LogGroupComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LogGroup logGroup = (LogGroup)Enum.ToObject(typeof(LogGroup),this.LogGroupComboBox.SelectedIndex);
            this.SetBindingByGroup(logGroup);
            LogAudit logAudit = this.LogTypeComboBox.SelectedItem as LogAudit;
            if (logAudit == null) return;
            LogType type = logAudit.LogType;
            //this.ShowLogFrm(logGroup, type);
        }

        private void LogTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.LogTypeComboBox.SelectedItem == null) return;
            LogGroup logGroup = (LogGroup)Enum.ToObject(typeof(LogGroup), this.LogGroupComboBox.SelectedIndex);

            LogAudit logAudit = this.LogTypeComboBox.SelectedItem as LogAudit;
            LogType type = logAudit.LogType;
            this.ShowLogFrm(logGroup, type);
        }

        private void ShowLogFrm(LogGroup logGroup,LogType logType)
        {
            switch (logGroup)
            {
                case LogGroup.TradingLog:
                    this.LogContentFrame.Content = new DealingLogControl(logType);
                    break;
                case LogGroup.QuotationLog:
                    this.LogContentFrame.Content = new SourceManagerLogControl(logType);
                    break;
                default:
                    this.LogContentFrame.Content = new DealingLogControl(LogType.QuotePrice);
                    break;
            }
        }
    }

    public class LogAudit
    {
        public LogAudit(LogGroup logGroup,LogType logType)
        {
            this.LogGroup = logGroup;
            this.LogType = logType;
        }
        public LogGroup LogGroup { get; set; }
        public LogType LogType { get; set; }

        public string LogName
        {
            get { return System.Enum.GetName(typeof(LogType), this.LogType); }
        }
    }
}
