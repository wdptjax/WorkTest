
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
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace DeviceSim.Device
{
    public class DataConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is EDfMode)
            {
                return ((EDfMode)value).ToString().Substring(7);
            }
            if (value != null && value is ELevel_Indicatir)
            {
                return ((ELevel_Indicatir)value).ToString().Substring(16);
            }
            if (value != null && value is EDemodulation)
            {
                return ((EDemodulation)value).ToString().Substring(4);
            }
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
            if (targetType == typeof(Brush))
            {
                if (value is bool && parameter is null || parameter.ToString() == "")
                {
                    return (bool)value ? Brushes.Lime : Brushes.Red;
                }
            }
            if (parameter.ToString() == "ShowRx")
            {
                if (value is EDfMode)
                {
                    EDfMode mode = (EDfMode)value;
                    if (mode == EDfMode.DFMODE_FFM || mode == EDfMode.DFMODE_RX)
                        return Visibility.Visible;
                    else
                        return Visibility.Collapsed;
                }
            }
            if (parameter.ToString() == "ShowScan")
            {
                if (value is EDfMode)
                {
                    EDfMode mode = (EDfMode)value;
                    if (mode == EDfMode.DFMODE_RXPSCAN || mode == EDfMode.DFMODE_SCAN || mode == EDfMode.DFMODE_SEARCH)
                        return Visibility.Visible;
                    else
                        return Visibility.Collapsed;
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
