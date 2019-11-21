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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DeviceSim.Device
{
    /// <summary>
    /// UserControl1.xaml 的交互逻辑
    /// </summary>
    public partial class ADS_B_USBView : UserControl
    {
        public ADS_B_USBView(ADS_B_USBDevice device)
        {
            InitializeComponent();
            Device = device;
        }

        private static DependencyProperty _Device = DependencyProperty.Register("Device", typeof(ADS_B_USBDevice), typeof(ADS_B_USBView), new PropertyMetadata(null));
        public ADS_B_USBDevice Device { get { return (ADS_B_USBDevice)GetValue(_Device); } set { SetValue(_Device, value); } }

    }
}
