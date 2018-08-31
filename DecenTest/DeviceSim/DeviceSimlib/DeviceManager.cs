
/*********************************************************************************************
 *	
 * 文件名称:    DeviceManager.cs
 *
 * 作    者:    吴德鹏
 *	
 * 创作日期:    2018-8-31 15:32:05
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

namespace DeviceSimlib
{
    public class DeviceManager : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<Device> Devices
        {
            get { return _devices; }
            set { _devices = value; PropertyChanged.Notify(() => Devices); }
        }

        #region 局部变量

        private ObservableCollection<Device> _devices = new ObservableCollection<Device>();
        private static DeviceManager _default = null;
        private static object _lockInstance = new object();

        #endregion 局部变量

        private DeviceManager()
        {

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

        public void AddNew()
        {
            Device device = new Device();
            Devices.Add(device);
        }

    }
}
