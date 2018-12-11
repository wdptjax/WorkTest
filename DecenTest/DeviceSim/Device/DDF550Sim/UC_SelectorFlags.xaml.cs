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
    /// UC_TraceFlags.xaml 的交互逻辑
    /// </summary>
    public partial class UC_SelectorFlags : UserControl
    {
        public UC_SelectorFlags()
        {
            InitializeComponent();
        }

        public uint SelectorFlags { get { return (uint)GetValue(SelectorFlagsProperty); } set { SetValue(SelectorFlagsProperty, value); } }

        static DependencyProperty SelectorFlagsProperty = DependencyProperty.Register("SelectorFlags", typeof(uint), typeof(UC_SelectorFlags), new PropertyMetadata(0d));

    }
}
