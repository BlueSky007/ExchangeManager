﻿using ManagerConsole.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Resources;

namespace ManagerConsole.Helper
{
    public class DeciamlTypeNullValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            decimal lot = (decimal)value;
            if (lot == 0)
            {
                return "-";
            }
            return lot;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class InstrumentToBuySellColorConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            InstrumentClient instrument = (InstrumentClient)value;
            string askBid = (string)parameter;
            if (instrument.IsNormal)
            {
                return askBid == "Bid" ? SolidColorBrushes.Sell : SolidColorBrushes.Buy;
            }
            else
            {
                return askBid == "Bid" ? SolidColorBrushes.Buy : SolidColorBrushes.Sell;
            }
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }

    public class TaskSchedulerDateConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return "Never";
            DateTime lastRunTime = (DateTime)value;
            if (lastRunTime == DateTime.MaxValue)
            {
                return "Never";
            }
            return lastRunTime;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }

    public class PriceTrendConverter : IValueConverter
    {
        public static BitmapImage _Up = null;
        public static BitmapImage _Down = null;

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return value;

            if (_Up == null)
            {
                _Up = new BitmapImage(new Uri("../../Asset/Images/Up.png", UriKind.Relative));
            }
            if (_Down == null)
            {
                _Down = new BitmapImage(new Uri("../../Asset/Images/Down.png", UriKind.Relative));
            }

            PriceTrend priceTrend = (PriceTrend)value;
            if (priceTrend == PriceTrend.NoChange)
            {
                return null;
            }
            else if (priceTrend == PriceTrend.Up)
            {
                return _Up;
            }
            else if (priceTrend == PriceTrend.Down)
            {
                return _Down;
            }
            else
            {
                throw new NotSupportedException(priceTrend.ToString());
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }

    public class PriceTrendForegroundConvert : IValueConverter
    {
        public static readonly SolidColorBrush _PriceDownBrush = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
        public static readonly SolidColorBrush _PriceUpBrush = new SolidColorBrush(Color.FromArgb(255, 1, 137, 10));

        object IValueConverter.Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            PriceTrend trend = (PriceTrend)value;
            if (trend == PriceTrend.Up)
            {
                return PriceTrendForegroundConvert._PriceUpBrush;
            }
            else if (trend == PriceTrend.Down)
            {
                return PriceTrendForegroundConvert._PriceDownBrush;
            }
            else
            {
                return SolidColorBrushes.LightBlue;
            }
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }

    public class LotForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            decimal lot = (decimal)value;
            if (lot >= 0)
            {
                return SolidColorBrushes.LightBlue;
            }
            else
            {
                return SolidColorBrushes.Red;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
