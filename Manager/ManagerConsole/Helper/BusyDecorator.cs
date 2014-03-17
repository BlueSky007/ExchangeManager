using Manager.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ManagerConsole.Helper
{
    public class BusyDecorator
    {
        private int _elementCount;
        private double _radious = 15;
        private DispatcherTimer _animationTimer;
        private int _currentElementIndex = 0;
        private int _opacityCount;
        private double _opacityInterval;
        private double _opacity;
        private double _minOpacity;
        private object[] _elements;
        private Grid _Grid;

        public BusyDecorator(Grid grid)
        {
            this._Grid = grid;
            this._animationTimer = new DispatcherTimer();
            this._animationTimer.Interval = TimeSpan.FromMilliseconds(100);
            this._animationTimer.Tick += new EventHandler(_animationTimer_Tick);
           

            this.CreateElements(grid, grid.Width / 2, grid.Height / 2);
        }

        private Rectangle CreateRectangle()
        {
            return new Rectangle() { Height = 12, Width = 4,Fill = SolidColorBrushes.LightBlue,RadiusX = 10,RadiusY = 10};
        }

        private void CreateElements(Grid grid, double left, double top)
        {
            this._opacity = 1;
            this._minOpacity = 0.4;
            this._elementCount = 12;
            double differOpacity = this._opacity - this._minOpacity;
            this._opacityCount = (int)(this._elementCount * 0.5);
            this._opacityInterval = differOpacity / this._opacityCount; //0.6/6

            this._elements = new object[_elementCount];
            for (int i = 0; i < this._elementCount; i++)
            {
                Rectangle rect = this.CreateRectangle();
                if (i < this._opacityCount) //设置为6
                {
                    rect.Opacity = this._opacity - i * this._opacityInterval; //前六个小圆透明度递减
                }
                else
                {
                    rect.Opacity = this._minOpacity;  
                }
                //2*PI*
                rect.SetValue(Canvas.LeftProperty, left + this._radious * Math.Cos(360 * i / this._elementCount * Math.PI / 180));
                rect.SetValue(Canvas.TopProperty, top - 2.5 - _radious * Math.Sin(i / this._elementCount * 2 * Math.PI));

                rect.RenderTransformOrigin = new Point(0.5, 0.5);
                rect.RenderTransform = new RotateTransform(360 - 360 / _elementCount * i, 0, 15);
                grid.Children.Add(rect);

                this._elements[i] = rect;
            }
            this._currentElementIndex = 0;
        }

        private void _animationTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                this._currentElementIndex--;
                this._currentElementIndex = this._currentElementIndex < 0 ? this._elements.Length - 1 : this._currentElementIndex;
                int showCount = 0;
                for (int i = this._currentElementIndex; i < this._currentElementIndex + this._elementCount; i++)
                {
                    int j = i > this._elements.Length - 1 ? i - this._elements.Length : i;
                    ((Rectangle)_elements[j]).Fill = SolidColorBrushes.Gold;
                    if (showCount < _opacityCount)
                    {
                        ((Rectangle)_elements[j]).Opacity = _opacity - showCount * _opacityInterval;
                        showCount++;
                    }
                    else
                    {
                        ((Rectangle)_elements[j]).Opacity = _minOpacity;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.AddEvent(System.Diagnostics.TraceEventType.Error, "BusyDecorator._animationTimer_Tick\r\n{0}", ex.ToString());
            }
        }

        public void Start()
        {
            this._Grid.Visibility = Visibility.Visible;
            this._animationTimer.Start();
        }

        public void Stop()
        {
            this._Grid.Visibility = Visibility.Hidden;
            this._animationTimer.Stop();
        }
    }
}
