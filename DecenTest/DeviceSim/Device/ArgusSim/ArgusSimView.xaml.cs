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
    /// ArgusSimView.xaml 的交互逻辑
    /// </summary>
    public partial class ArgusSimView : UserControl
    {
        public ArgusSimView(ArgusSim device)
        {
            InitializeComponent();
            Device = device;
        }

        private static DependencyProperty _Device = DependencyProperty.Register("Device", typeof(ArgusSim), typeof(ArgusSimView), new PropertyMetadata(null));
        public ArgusSim Device { get { return (ArgusSim)GetValue(_Device); } set { SetValue(_Device, value); } }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Command == Commands.OK)
            {
                e.CanExecute = Device.DeviceInitialized && !Device.IsRunning;
            }
            else if (e.Command == Commands.Cancel)
            {
                e.CanExecute = Device.DeviceInitialized && Device.IsRunning;
            }
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == Commands.OK)
            {
                Device.StartTask();
            }
            else if (e.Command == Commands.Cancel)
            {
                Device.StopTask();
            }
        }
    }
}
