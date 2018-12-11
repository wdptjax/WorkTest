using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace DellRemotingSwitch
{
    public class DataConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && parameter != null && parameter.ToString() == "TakeBack" && value is bool && targetType == typeof(bool))
            {
                return !(bool)value;
            }

            if (value is bool? && targetType == typeof(Brush))
            {
                bool? val = (bool?)value;
                if (val == null)
                    return Brushes.Yellow;
                else if (val == true)
                    return Brushes.Lime;
                else return Brushes.Red;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
