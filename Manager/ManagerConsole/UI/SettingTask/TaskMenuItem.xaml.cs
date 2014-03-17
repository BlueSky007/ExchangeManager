using ManagerConsole.Model;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ManagerConsole.UI.SettingTask
{
    /// <summary>
    /// Interaction logic for TaskMenuItem.xaml
    /// </summary>
    public partial class TaskMenuItem : UserControl
    {
        private bool _IsHidden = false;
        private Storyboard _Storyboard;
        private double _OriginHeight;
        public event RoutedEventHandler ExpandEvent;
        public TaskMenuItem()
        {
            InitializeComponent();
            this._Storyboard = new Storyboard();
            this._OriginHeight = this.MyBorder.Height;
        }

        public void SetEventItemBinding(TaskMenuItemEntity taskMenuItem)
        {
            this.DataContext = taskMenuItem;
            this.TaskMenuContentCanva.ItemsSource = taskMenuItem.TaskMenuEventItems;
        }

        protected void ExpandMenuButton_Click(object sender, RoutedEventArgs e)
        {
            if (!this._IsHidden)
            {
                this.Hidden();
            }
            else
            {
                this.Expand();
            }
        }

        private void Expand()
        {
            this.Name = "MyBorder";
            this.MyBorder.Name = "MyBorder";

            NameScope.SetNameScope(this, new NameScope());
            this.RegisterName(this.MyBorder.Name, this.MyBorder);

            DoubleAnimation myDoubleAnimation = new DoubleAnimation();
            myDoubleAnimation.From = 0;
            myDoubleAnimation.To = this._OriginHeight;
            myDoubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.5));

            Storyboard.SetTargetName(myDoubleAnimation, this.MyBorder.Name);
            Storyboard.SetTargetProperty(myDoubleAnimation, new PropertyPath(Border.HeightProperty));

            this._Storyboard.Children.Add(myDoubleAnimation);
            this._Storyboard.Begin(this);
            this._IsHidden = false;

            //this.ExpandEvent(true, new RoutedEventArgs());
        }

        private void Hidden()
        {
            this.Name = "MyBorder";
            this.MyBorder.Name = "MyBorder";

            NameScope.SetNameScope(this, new NameScope());
            this.RegisterName(this.MyBorder.Name, this.MyBorder);

            DoubleAnimation myDoubleAnimation = new DoubleAnimation();
            myDoubleAnimation.From = this._OriginHeight;
            myDoubleAnimation.To = 0;
            myDoubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.5));

            Storyboard.SetTargetName(myDoubleAnimation, this.MyBorder.Name);
            Storyboard.SetTargetProperty(myDoubleAnimation, new PropertyPath(Border.HeightProperty));

            this._Storyboard.Children.Add(myDoubleAnimation);
            this._Storyboard.Begin(this);
            this._IsHidden = true;

            //this.ExpandEvent(false, new RoutedEventArgs());
        }
    }
}
