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
    /// DDF550View.xaml 的交互逻辑
    /// </summary>
    public partial class DDF550View : UserControl
    {
        public DDF550View(DDF550Device device)
        {
            InitializeComponent();
            Device = device;
        }

        private static DependencyProperty _Device = DependencyProperty.Register("Device", typeof(DDF550Device), typeof(DDF550View), new PropertyMetadata(null));
        public DDF550Device Device { get { return (DDF550Device)GetValue(_Device); } set { SetValue(_Device, value); } }

    }
}
