
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
                return ((EDfMode)value).ToString().Replace("DFMODE_", "");
            }
            if (value != null && value is ELevel_Indicatir)
            {
                return ((ELevel_Indicatir)value).ToString().Replace("LEVEL_INDICATOR_", "");
            }
            if (value != null && value is EAverage_Mode)
            {
                return ((EAverage_Mode)value).ToString().Replace("DFSQU_", "");
            }
            if (value != null && value is EDemodulation)
            {
                return ((EDemodulation)value).ToString().Replace("MOD_", "");
            }
            if (value != null && value is ERf_Mode)
            {
                return ((ERf_Mode)value).ToString().Replace("RFMODE_", "");
            }
            if (value != null && value is EMeasureMode)
            {
                return ((EMeasureMode)value).ToString().Replace("MEASUREMODE_", "");
            }
            if (value != null && value is EAudioMode)
            {
                EAudioMode mode = (EAudioMode)value;

                return string.Format("{0}:{1}", (int)mode, mode.ToString().Replace("AUDIO_MODE_", ""));
            }

            if (value != null && parameter != null && parameter.ToString() == "Gain")
            {
                int val = (int)value;
                if (val <= -100)
                    return "自动";
                else
                    return val;
            }
            if (value != null && parameter != null && parameter.ToString() == "Attenuation")
            {
                int val = (int)value;
                if (val <= -1)
                    return "自动";
                else
                    return val;
            }
            if (value != null && parameter != null && parameter.ToString() == "MeasureTime")
            {
                double val = (double)value;
                if (val == 0)
                    return "自动";
                else
                    return val;
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
