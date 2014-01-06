using ManagerConsole.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

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
}
