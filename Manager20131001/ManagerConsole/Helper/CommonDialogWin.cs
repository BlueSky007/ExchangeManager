using Infragistics.Controls.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ManagerConsole.Helper
{
    public class CommonDialogWin
    {
        private Grid _LayoutContainer;
        //private XamDialogWindow _CommonDialogWin = null;
        private List<XamDialogWindow> _CommonDialogWins = new List<XamDialogWindow>();

        public CommonDialogWin(Grid layoutContainer)
        {
            this._LayoutContainer = layoutContainer;
        }
        public CommonDialogWin(Grid layoutContainer, double left, double top)
            : this(layoutContainer)
        {
            //this._CommonDialogWin = new XamDialogWindow
            //{
            //    StartupPosition = Infragistics.Controls.Interactions.StartupPosition.Manual,
            //    IsModal = false,
            //    CloseButtonVisibility = Visibility.Collapsed,
            //    MinimizeButtonVisibility=Visibility.Collapsed,
            //    MaximizeButtonVisibility=Visibility.Collapsed,
            //    Left=left,
            //    Top=top,
            //    Name = "_CommonDialogWin"
            //};			
        }

        public void ShowDialogWin(string message, string caption, double width, double height, double left, double top, bool isManualStartupPosition)
        {
            XamDialogWindow commonDialogWin = new XamDialogWindow()
            {
                Width = width,
                Height = height,
                Header = caption,
                StartupPosition = isManualStartupPosition ? Infragistics.Controls.Interactions.StartupPosition.Manual : Infragistics.Controls.Interactions.StartupPosition.Center,
                Left = left,
                Top = top,
                IsModal = true,
                CloseButtonVisibility = Visibility.Collapsed,
                MinimizeButtonVisibility = Visibility.Collapsed,
                MaximizeButtonVisibility = Visibility.Collapsed,
                Name = "_CommonDialogWin"
            };

            TextBlock msgLabel = new TextBlock() { Foreground = new SolidColorBrush(Colors.Blue), Text = message, Margin = new Thickness(10), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, TextWrapping = TextWrapping.Wrap };
            Button okBtn = new Button() { Content = "OK", Width = 100, Height = 25, VerticalAlignment = VerticalAlignment.Bottom, Margin = new Thickness(5), HorizontalAlignment = HorizontalAlignment.Center, Tag = commonDialogWin, TabIndex = 1 };
            okBtn.Click += new RoutedEventHandler(okBtn_Click);

            StackPanel msgPanel = new StackPanel();
            msgPanel.Margin = new Thickness(5);
            msgPanel.HorizontalAlignment = HorizontalAlignment.Center;
            msgPanel.VerticalAlignment = VerticalAlignment.Center;
            msgPanel.Children.Add(msgLabel);
            ScrollViewer scrollViewer = new ScrollViewer();
            scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            scrollViewer.VerticalContentAlignment = VerticalAlignment.Stretch;
            scrollViewer.Content = msgPanel;
            Grid.SetRow(scrollViewer, 0);

            StackPanel btnPanel = new StackPanel();
            btnPanel.Margin = new Thickness(5);
            btnPanel.HorizontalAlignment = HorizontalAlignment.Center;
            btnPanel.VerticalAlignment = VerticalAlignment.Bottom;
            btnPanel.Children.Add(okBtn);
            Grid.SetRow(btnPanel, 1);

            Grid layoutGrid = new Grid();
            layoutGrid.RowDefinitions.Add(new RowDefinition());
            layoutGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            layoutGrid.Children.Add(scrollViewer);
            layoutGrid.Children.Add(btnPanel);

            commonDialogWin.Content = layoutGrid;
            int columnSpan = this._LayoutContainer.ColumnDefinitions.Count;
            int rowSpan = this._LayoutContainer.RowDefinitions.Count;
            if (columnSpan > 0) Grid.SetColumnSpan(commonDialogWin, columnSpan);
            if (rowSpan > 0) Grid.SetRowSpan(commonDialogWin, rowSpan);

            if (!this._LayoutContainer.Children.Contains(commonDialogWin))
            {
                this._LayoutContainer.Children.Add(commonDialogWin);
            }
            this._CommonDialogWins.Add(commonDialogWin);
            commonDialogWin.Show();
        }

        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            Button clickBtn = (Button)sender;
            XamDialogWindow commonDialogWin = clickBtn.Tag as XamDialogWindow;
            this.Close(commonDialogWin);
        }
        public void ShowDialogWin(string message, string caption)
        {
            this.ShowDialogWin(message, caption, 350, 150, 0, 0, false);
        }
        public void ShowDialogWin(string message, string caption, double width, double height)
        {
            this.ShowDialogWin(message, caption, width, height, 0, 0, false);
        }
        private void Close(XamDialogWindow commonDialogWin)
        {
            if (commonDialogWin != null)
            {
                commonDialogWin.WindowState = Infragistics.Controls.Interactions.WindowState.Hidden;
                commonDialogWin.Close();
                this._LayoutContainer.Children.Remove(commonDialogWin);
                commonDialogWin = null;
            }
        }
        public void Close()
        {
            foreach (XamDialogWindow win in this._CommonDialogWins)
            {
                this.Close(win);
            }
        }
    }

    public class ConfirmDialogWin
    {
        public delegate void ConfirmDialogResultHandle(bool yesOrNo, UIElement uIElement);
        public event ConfirmDialogResultHandle OnConfirmDialogResult;

        private Grid _LayoutContainer;
        private XamDialogWindow _ConfirmDialogWin = null;
        private UIElement _ConfirmOptionElement = null;

        public ConfirmDialogWin(Grid layoutContainer)
        {
            this._LayoutContainer = layoutContainer;
        }
        public void ShowDialogWin(string message, string caption, double width, double height, UIElement uIElement)
        {
            this.ShowDialogWin(message, caption, width, height);

            var msgPanel = this._ConfirmDialogWin.FindName("msgPanel") as StackPanel;
            if (msgPanel != null)
            {
                msgPanel.Children.Add(uIElement);

                this._ConfirmOptionElement = uIElement;
            }
        }
        public void ShowDialogWin(string message, string caption, double width, double height)
        {
            if (this._ConfirmDialogWin == null)
            {
                this._ConfirmDialogWin = new XamDialogWindow()
                {
                    Width = width,
                    Height = height,
                    Header = caption,
                    StartupPosition = Infragistics.Controls.Interactions.StartupPosition.Center,
                    IsModal = true,
                    CloseButtonVisibility = Visibility.Collapsed,
                    MinimizeButtonVisibility = Visibility.Collapsed,
                    MaximizeButtonVisibility = Visibility.Collapsed,
                };
            }
            else
            {
                this._ConfirmDialogWin.Width = width;
                this._ConfirmDialogWin.Height = height;
                this._ConfirmDialogWin.Header = caption;
                this._ConfirmDialogWin.Content = null;
            }
            TextBlock msgLabel = new TextBlock() { Name = "msgLabel", Foreground = new SolidColorBrush(Colors.Blue), Text = message, Margin = new Thickness(10), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, TextWrapping = TextWrapping.Wrap };

            Button ok = new Button() { Name = "okBtn", Content = "Yes", Width = 80, Height = 25, TabIndex = 1, Margin = new Thickness(8) };
            Button cancel = new Button() { Name = "cancelBtn", Content = "No", Width = 80, Height = 25 };
            ok.Click += new RoutedEventHandler(ok_Click);
            cancel.Click += new RoutedEventHandler(cancel_Click);

            StackPanel msgPanel = new StackPanel() { Name = "msgPanel" };
            msgPanel.Margin = new Thickness(5);
            msgPanel.HorizontalAlignment = HorizontalAlignment.Center;
            msgPanel.VerticalAlignment = VerticalAlignment.Center;
            msgPanel.Children.Add(msgLabel);
            ScrollViewer scrollViewer = new ScrollViewer();
            scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            scrollViewer.VerticalContentAlignment = VerticalAlignment.Stretch;
            scrollViewer.Content = msgPanel;
            Grid.SetRow(scrollViewer, 0);

            StackPanel panel = new StackPanel();
            panel.Orientation = Orientation.Horizontal;
            panel.Children.Add(ok);
            panel.Children.Add(cancel);
            panel.VerticalAlignment = VerticalAlignment.Bottom;
            panel.HorizontalAlignment = HorizontalAlignment.Center;
            panel.Margin = new Thickness(5);

            Grid layoutGrid = new Grid();
            layoutGrid.RowDefinitions.Add(new RowDefinition());
            layoutGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            layoutGrid.Children.Add(scrollViewer);
            layoutGrid.Children.Add(panel);

            this._ConfirmDialogWin.Content = layoutGrid;
            int columnSpan = this._LayoutContainer.ColumnDefinitions.Count;
            int rowSpan = this._LayoutContainer.RowDefinitions.Count;
            if (columnSpan > 0) Grid.SetColumnSpan(this._ConfirmDialogWin, columnSpan);
            if (rowSpan > 0) Grid.SetRowSpan(this._ConfirmDialogWin, rowSpan);

            if (!this._LayoutContainer.Children.Contains(this._ConfirmDialogWin))
            {
                this._LayoutContainer.Children.Add(this._ConfirmDialogWin);
            }

            this._ConfirmDialogWin.Show();
        }

        void cancel_Click(object sender, RoutedEventArgs e)
        {
            Button cancelBtn = sender as Button;
            Button okBtn = this._ConfirmDialogWin.FindName("okBtn") as Button;
            if (okBtn != null) okBtn.IsEnabled = false;
            if (cancelBtn != null) cancelBtn.IsEnabled = false;
            if (this.OnConfirmDialogResult != null)
            {
                this.OnConfirmDialogResult(false, this._ConfirmOptionElement);
            }
            this.Close();
        }

        void ok_Click(object sender, RoutedEventArgs e)
        {
            Button okBtn = sender as Button;
            Button cancelBtn = this._ConfirmDialogWin.FindName("cancelBtn") as Button;
            if (okBtn != null) okBtn.IsEnabled = false;
            if (cancelBtn != null) cancelBtn.IsEnabled = false;
            TextBlock msgLabel = this._ConfirmDialogWin.FindName("msgLabel") as TextBlock;
            if (msgLabel != null)
            {
                msgLabel.Text = "Processing......";
            }

            if (this.OnConfirmDialogResult != null)
            {
                this.OnConfirmDialogResult(true, this._ConfirmOptionElement);
            }

        }
        public void Close()
        {
            if (this._ConfirmDialogWin != null)
            {
                this._LayoutContainer.Children.Remove(this._ConfirmDialogWin);
                this._ConfirmDialogWin.Close();
                this._ConfirmDialogWin = null;
            }
        }

    }
}
