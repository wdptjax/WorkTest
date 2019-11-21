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
    /// DemoSwivelView.xaml 的交互逻辑
    /// </summary>
    public partial class DemoSwivelView : UserControl
    {
        public DemoSwivelView(DemoSwivelDevice device)
        {
            InitializeComponent();
            Device = device;
        }

        private static DependencyProperty _Device = DependencyProperty.Register("Device", typeof(DemoSwivelDevice), typeof(DemoSwivelDevice), new PropertyMetadata(null));
        public DemoSwivelDevice Device { get { return (DemoSwivelDevice)GetValue(_Device); } set { SetValue(_Device, value); } }

    }
}
