
/*********************************************************************************************
 *	
 * 文件名称:    Manager.cs
 *
 * 作    者:    吴德鹏
 *	
 * 创作日期:    2018-12-11 15:50:21
 * 
 * 备    注:    描述类的用途
 *              
 *                                
 *               
*********************************************************************************************/

using NotificationExtensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Threading;

namespace DellRemotingSwitch
{
    public class DeviceManager : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private static DeviceManager _default = null;
        private static object _lockInstance = new object();
        private ObservableCollection<Device> _devices = new ObservableCollection<Device>();
        private Device _selectDevice = null;
        private Settings _settings = new Settings();
        private string _cmdPath = "";
        private const string RACADM_PATH = @"Racadm\racadm.exe";

        /// <summary>
        /// Racadm可执行程序路径
        /// </summary>
        public string RacadmPath
        {
            get { return _cmdPath; }
            set
            {
                _cmdPath = value;
                PropertyChanged.Notify(() => this.RacadmPath);
            }
        }
        public Device SelectDevice
        {
            get { return _selectDevice; }
            set { _selectDevice = value; PropertyChanged.Notify(() => SelectDevice); }
        }

        public Settings Settings
        {
            get { return _settings; }
            set { _settings = value; PropertyChanged.Notify(() => Settings); }
        }
        public ObservableCollection<Device> Devices
        {
            get { return _devices; }
            set
            {
                _devices = value;
                PropertyChanged.Notify(() => this.Devices);
            }
        }

        public static DeviceManager GetInstance()
        {
            if (_default == null)
            {
                lock (_lockInstance)
                {
                    if (_default == null)
                        _default = new DeviceManager();
                }
            }
            return _default;
        }

        public DeviceManager()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;
            RacadmPath = path + RACADM_PATH;
        }

        public void AddNewDevice(Dispatcher dispatcher)
        {
            this.Devices.Add(new Device(dispatcher));
        }

        public void DeleteDevice()
        {
            SelectDevice.Stop();
            this.Devices.Remove(this.SelectDevice);
        }
    }
}
