using Infragistics.Controls.Grids;
using Manager.Common;
using ManagerConsole.ViewModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace ManagerConsole.UI
{
   [TemplatePart(Name = "TradingSummaryGrid", Type = typeof(XamGrid))]
    public class TradingSummaryControl : Control
    {
       public delegate void GridExpandChangeHandler(bool isExpand, int rowIndex, int childCount);
        public event GridExpandChangeHandler OnGridExpandChangeEvent;
        private XamGrid _TradingSummaryGrid;
        private bool _IsLoaded;

        public TradingSummaryControl()
        {
            this.DefaultStyleKey = typeof(TradingSummaryControl);
            this.Loaded += new RoutedEventHandler(this.TradingSummaryControl_Loaded);
        }

        public XamGrid TradingSummaryGrid
        {
            get { return this._TradingSummaryGrid; }
            set { this._TradingSummaryGrid = value; }
        }

        private IEnumerable _ItemsSource;
        public IEnumerable ItemsSource
        {
            get { return this._ItemsSource; }
            set
            {
                this._ItemsSource = value;
                if (this._TradingSummaryGrid != null)
                {
                    this._TradingSummaryGrid.ItemsSource = value;
                    if (this._TradingSummaryGrid.Rows.Count > 0)
                    {
                        
                        this._TradingSummaryGrid.Rows[0].IsExpanded = true;
                        this._TradingSummaryGrid.Rows[1].IsExpanded = true;
                        this._TradingSummaryGrid.Rows[2].IsExpanded = true;

                        AccountStatusItem item = this._TradingSummaryGrid.Rows[2].Data as AccountStatusItem;
                        if (item != null && item.SubItems.Count == 0)
                        {
                            this._TradingSummaryGrid.ExpansionIndicatorSettings.Visibility = Visibility.Collapsed;
                            this._TradingSummaryGrid.Columns.DataColumns[0].Width = new ColumnWidth(150, false);
                        }
                        else
                        {
                            this._TradingSummaryGrid.ExpansionIndicatorSettings.Visibility = Visibility.Visible;
                            this._TradingSummaryGrid.Columns.DataColumns[0].Width = new ColumnWidth(120, false);
                        }
                    }
                }
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this._TradingSummaryGrid = this.GetTemplateChild("TradingSummaryGrid") as XamGrid;

            this._TradingSummaryGrid.Loaded += new RoutedEventHandler(delegate(object sender, RoutedEventArgs e)
            {
                if (!this._IsLoaded)
                {
                    this._IsLoaded = true;
                }
            });

            if (this.ItemsSource != null)
            {
                this._TradingSummaryGrid.ItemsSource = this.ItemsSource;
            }
            this._TradingSummaryGrid.RowExpansionChanged += new EventHandler<RowExpansionChangedEventArgs>(this._TradingSummaryGrid_RowExpansionChanging);
        }

        public void AdjustRowHeight(int rowIndex)
        {
            this._TradingSummaryGrid.Rows[rowIndex].Height = new RowHeight(100);
        }

        public void ExpandSubItemRow(bool isExpand, int rowIndex,int childCount)
        {
            if (this._TradingSummaryGrid.Rows.Count == 0) return;
            AccountStatusItem item = this._TradingSummaryGrid.Rows[rowIndex].Data as AccountStatusItem;
            int subItemsCount = item.SubItems.Count;
            this._TradingSummaryGrid.Rows[rowIndex].IsExpanded = isExpand;
            if (subItemsCount == 0)
            {
                if (isExpand)
                {
                    this._TradingSummaryGrid.Rows[rowIndex].Height = new RowHeight(25 * (childCount + 1) + 3);
                }
                else
                {
                    this._TradingSummaryGrid.Rows[rowIndex].Height = new RowHeight(25);
                }
            }
        }

        private void TradingSummaryControl_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void _TradingSummaryGrid_RowExpansionChanging(object sender, RowExpansionChangedEventArgs e)
        {
            try
            {
                if (!this._IsLoaded) return;
                AccountStatusItem item = e.Row.Data as AccountStatusItem;

                if (item.SubItems.Count == 0) return;
                bool isExpand = e.Row.IsExpanded;
                int rowIndex = e.Row.Index;

                if (this.OnGridExpandChangeEvent != null)
                {
                    this.OnGridExpandChangeEvent(isExpand, rowIndex, item.SubItems.Count);
                }
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "TradingSummaryControl._TradingSummaryGrid_RowExpansionChanging Error\r\n{0}", ex.ToString());
            }
        }
    }
}
