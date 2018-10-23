using DeviceSimlib;
using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace EBD195Sim
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            if (!string.IsNullOrEmpty(Properties.Settings.Default.DeviceSetting))
                Device = (EBD195Device)XmlUtil.Deserialize(typeof(EBD195Device), Properties.Settings.Default.DeviceSetting);
            Device.Initialize(this.Dispatcher);
        }

        private static DependencyProperty _Device = DependencyProperty.Register("Device", typeof(EBD195Device), typeof(MainWindow), new PropertyMetadata(new EBD195Device()));
        public EBD195Device Device { get { return (EBD195Device)GetValue(_Device); } set { SetValue(_Device, value); } }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Command == Commands.Start)
            {
                e.CanExecute = !Device.DeviceInitialized && !string.IsNullOrEmpty(Device.ComPort);
            }
            else if (e.Command == Commands.Stop)
            {
                e.CanExecute = Device.DeviceInitialized;
            }
            else if (e.Command == Commands.Pause)
            {
                e.CanExecute = Device.DeviceInitialized;
            }
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == Commands.Start)
            {
                Device.Start();
                string str = XmlUtil.Serializer(typeof(EBD195Device), Device);
                Properties.Settings.Default.DeviceSetting = str;
                Properties.Settings.Default.Save();
            }
            else if (e.Command == Commands.Stop)
            {
                Device.Stop();
            }
            else if (e.Command == Commands.Pause)
            {
                CheckBox rdb = e.Parameter as CheckBox;
                bool isPause = (bool)rdb.IsChecked;
                Device.TaskPause(isPause);
            }
        }
    }

}
