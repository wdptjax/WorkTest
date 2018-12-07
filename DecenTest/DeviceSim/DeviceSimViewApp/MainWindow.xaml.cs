using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
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
using System.Xml.Linq;

using DeviceSimlib;

namespace DeviceSimViewApp
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Deserialize();
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Command == Commands.Start)
            {
                e.CanExecute = DeviceManager.GetInstance().SelectDevice != null && !DeviceManager.GetInstance().SelectDevice.DeviceInitialized && DeviceManager.GetInstance().SelectDevice.CanDeviceIni;
            }
            else if (e.Command == Commands.Stop)
            {
                e.CanExecute = DeviceManager.GetInstance().SelectDevice != null && DeviceManager.GetInstance().SelectDevice.DeviceInitialized;
            }
            else if (e.Command == Commands.Pause)
            {
                e.CanExecute = DeviceManager.GetInstance().SelectDevice != null && DeviceManager.GetInstance().SelectDevice.DeviceInitialized;
            }
            else if (e.Command == Commands.Delete)
            {
                e.CanExecute = DeviceManager.GetInstance().SelectDevice != null;
            }
            else if (e.Command == Commands.Add)
            {
                e.CanExecute = true;
            }
            else
                e.CanExecute = false;
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == Commands.Start)
            {
                DeviceManager.GetInstance().SelectDevice.Start();
            }
            else if (e.Command == Commands.Stop)
            {
                DeviceManager.GetInstance().SelectDevice.Stop();
            }
            else if (e.Command == Commands.Pause)
            {
                CheckBox rdb = e.Parameter as CheckBox;
                bool isPause = (bool)rdb.IsChecked;
                DeviceManager.GetInstance().SelectDevice.TaskPause(isPause);
            }
            else if (e.Command == Commands.Add)
            {
                AddDevice add = new AddDevice();
                add.Owner = this;
                if (add.ShowDialog() == true)
                {
                    var device = add.NewDevice;
                    if (device == null)
                    {
                        MessageBox.Show(this, "创建失败!");
                        return;
                    }
                    DeviceManager.GetInstance().Devices.Add(device);
                }
            }
            else if (e.Command == Commands.Delete)
            {
                DeviceManager.GetInstance().Devices.Remove(DeviceManager.GetInstance().SelectDevice);
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            Serializer();
        }

        /// <summary>
        /// 序列化
        /// </summary>
        private void Serializer()
        {
            SerializableDictionary<string, string> dic = new SerializableDictionary<string, string>();
            if (DeviceManager.GetInstance().Devices != null && DeviceManager.GetInstance().Devices.Count > 0)
            {
                foreach (var dev in DeviceManager.GetInstance().Devices)
                {
                    string type = dev.GetType().FullName;
                    string str = XmlUtil.Serializer(dev.GetType(), dev);
                    dic.Add(str, type);
                }
            }
            string settings = XmlUtil.Serializer(dic.GetType(), dic);
            Properties.Settings.Default.DeviceSettings = settings;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        private void Deserialize()
        {
            var settings = Properties.Settings.Default.DeviceSettings;
            if (string.IsNullOrEmpty(settings))
                return;
            var obj = XmlUtil.Deserialize(typeof(SerializableDictionary<string, string>), settings);
            if (obj == null)
                return;
            SerializableDictionary<string, string> dic = (SerializableDictionary<string, string>)obj;
            foreach(var pair in dic)
            {
                Type type = PluginsManager.GetTye(pair.Value);
                object devObj = XmlUtil.Deserialize(type, pair.Key);
                if (devObj == null)
                    continue;
                DeviceBase dev = (DeviceBase)devObj;
                DeviceManager.GetInstance().Devices.Add(dev);
            }
        }
    }
}
