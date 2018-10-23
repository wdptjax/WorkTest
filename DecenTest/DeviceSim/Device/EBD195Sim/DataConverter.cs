
/*********************************************************************************************
 *	
 * 文件名称:    DataConverter.cs
 *
 * 作    者:    吴德鹏
 *	
 * 创作日期:    2018-10-22 15:54:16
 * 
 * 备    注:    描述类的用途
 *              
 *                                
 *               
*********************************************************************************************/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;

namespace DeviceSim.Device
{
    public class DataConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof(Brush))
            {
                if (value is bool)
                {
                    return (bool)value ? Brushes.Lime : Brushes.Red;
                }
            }
            if (targetType == typeof(bool))
            {
                if (value is bool)
                    return !(bool)value;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
