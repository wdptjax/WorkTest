using Microsoft.Win32;
using System;
using System.Collections.Generic;
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

namespace DellRemotingSwitch
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
            //if (!string.IsNullOrEmpty(Properties.Settings.Default.RacadmPath))
            //    DeviceManager.GetInstance().RacadmPath = Properties.Settings.Default.RacadmPath;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            Serializer();
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Command == Commands.Add || e.Command == Commands.Settings)
            {
                e.CanExecute = true;
                return;
            }
            if (e.Command == Commands.StartAll)
            {
                e.CanExecute = DeviceManager.GetInstance().Devices != null && DeviceManager.GetInstance().Devices.Count != 0;
                return;
            }
            if (DeviceManager.GetInstance().SelectDevice == null)
            {
                e.CanExecute = false;
                return;
            }
            if (e.Command == Commands.Delete)
            {
                e.CanExecute = DeviceManager.GetInstance().SelectDevice != null;
            }
            else if (e.Command == Commands.Start)
            {
                e.CanExecute = !DeviceManager.GetInstance().SelectDevice.IsRunning;
            }
            else if (e.Command == Commands.Stop)
            {
                e.CanExecute = DeviceManager.GetInstance().SelectDevice.IsRunning;
            }
            else if (e.Command == Commands.PowerUp)
            {
                e.CanExecute = DeviceManager.GetInstance().SelectDevice.IsRunning
                    & DeviceManager.GetInstance().SelectDevice.DeviceStatus != true & !DeviceManager.GetInstance().SelectDevice.IsSendingCmd;
            }
            else if (e.Command == Commands.PowerDown)
            {
                e.CanExecute = DeviceManager.GetInstance().SelectDevice.IsRunning
                    & DeviceManager.GetInstance().SelectDevice.DeviceStatus == true & !DeviceManager.GetInstance().SelectDevice.IsSendingCmd;
            }
            else if (e.Command == Commands.PowerCycle)
            {
                e.CanExecute = DeviceManager.GetInstance().SelectDevice.IsRunning
                    & DeviceManager.GetInstance().SelectDevice.DeviceStatus == true & !DeviceManager.GetInstance().SelectDevice.IsSendingCmd;
            }
            else if (e.Command == Commands.Query)
            {
                e.CanExecute = DeviceManager.GetInstance().SelectDevice.IsRunning;
            }
            else if (e.Command == Commands.PasswordChecking)
            {
                e.CanExecute = DeviceManager.GetInstance().SelectDevice.IsRunning
                    & !DeviceManager.GetInstance().SelectDevice.IsSendingCmd;
            }
            else if (e.Command == Commands.PathSelect)
            {
                e.CanExecute = !DeviceManager.GetInstance().SelectDevice.IsRunning;
            }
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {

            if (e.Command == Commands.Add)
            {
                DeviceManager.GetInstance().AddNewDevice(this.Dispatcher);
                return;
            }
            else if (e.Command == Commands.Settings)
            {
                DeviceSettings settings = DeviceManager.GetInstance().Settings.GetShowView();
                settings.Owner = this;
                settings.ShowDialog();
                return;
            }
            else if (e.Command == Commands.StartAll)
            {
                if (DeviceManager.GetInstance().Devices == null || DeviceManager.GetInstance().Devices.Count == 0)
                    return;
                foreach (var dev in DeviceManager.GetInstance().Devices)
                    dev.Start();
            }
            else if (e.Command == Commands.PathSelect)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.DefaultExt = ".exe";
                openFileDialog.Filter = "exe file|*.exe";
                openFileDialog.FileName = DeviceManager.GetInstance().RacadmPath;
                if (openFileDialog.ShowDialog() == true)
                {
                    string path = openFileDialog.FileName;
                    DeviceManager.GetInstance().RacadmPath = path;
                    //Properties.Settings.Default.RacadmPath = path;
                    //Properties.Settings.Default.Save();
                }
                return;
            }

            if (DeviceManager.GetInstance().SelectDevice == null)
                return;
            if (e.Command == Commands.Delete)
            {
                DeviceManager.GetInstance().DeleteDevice();
            }
            else if (e.Command == Commands.Start)
            {
                DeviceManager.GetInstance().SelectDevice.Start();
            }
            else if (e.Command == Commands.Stop)
            {
                DeviceManager.GetInstance().SelectDevice.Stop();
            }
            else if (e.Command == Commands.PowerUp)
            {
                DeviceManager.GetInstance().SelectDevice.PowerUp();
            }
            else if (e.Command == Commands.PowerDown)
            {
                DeviceManager.GetInstance().SelectDevice.PowerDown();
            }
            else if (e.Command == Commands.PowerCycle)
            {
                DeviceManager.GetInstance().SelectDevice.ReStart();
            }
            else if (e.Command == Commands.Query)
            {
                DeviceManager.GetInstance().SelectDevice.QueryStatus();
            }
            else if (e.Command == Commands.PasswordChecking)
            {
                DeviceManager.GetInstance().SelectDevice.PasswordCheckOn = !DeviceManager.GetInstance().SelectDevice.PasswordCheckOn;
                DeviceManager.GetInstance().SelectDevice.PasswodCheckSwitch();
            }
        }


        /// <summary>
        /// 序列化
        /// </summary>
        private void Serializer()
        {
            List<string> list = new List<string>();
            if (DeviceManager.GetInstance().Devices != null && DeviceManager.GetInstance().Devices.Count > 0)
            {
                foreach (var dev in DeviceManager.GetInstance().Devices)
                {
                    string str = XmlUtil.Serializer(dev.GetType(), dev);
                    list.Add(str);
                }
            }
            string settings = XmlUtil.Serializer(list.GetType(), list);
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
            var obj = XmlUtil.Deserialize(typeof(List<string>), settings);
            if (obj == null)
                return;
            List<string> list = (List<string>)obj;
            foreach (var str in list)
            {
                object devObj = XmlUtil.Deserialize(typeof(Device), str);
                if (devObj == null)
                    continue;
                Device dev = (Device)devObj;
                dev.UpdateDispatcher(this.Dispatcher);
                DeviceManager.GetInstance().Devices.Add(dev);
            }
        }
    }
}
