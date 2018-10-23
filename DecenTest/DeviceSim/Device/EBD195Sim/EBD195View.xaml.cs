using DeviceSimlib;
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
    /// EBD195View.xaml 的交互逻辑
    /// </summary>
    public partial class EBD195View : UserControl
    {
        public EBD195View(EBD195Device device)
        {
            InitializeComponent();
            Device = device;
        }

        private static DependencyProperty _Device = DependencyProperty.Register("Device", typeof(EBD195Device), typeof(EBD195View), new PropertyMetadata(null));
        public EBD195Device Device { get { return (EBD195Device)GetValue(_Device); } set { SetValue(_Device, value); } }
    }
}
