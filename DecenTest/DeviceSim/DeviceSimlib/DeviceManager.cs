
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
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace DeviceSimlib
{
    public class DeviceManager : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<DeviceBase> Devices
        {
            get { return _devices; }
            set { _devices = value; PropertyChanged.Notify(() => Devices); }
        }
        public DeviceBase SelectDevice
        {
            get { return _selectDevice; }
            set { _selectDevice = value; PropertyChanged.Notify(() => SelectDevice); }
        }

        #region 局部变量

        private ObservableCollection<DeviceBase> _devices = new ObservableCollection<DeviceBase>();
        private static DeviceManager _default = null;
        private static object _lockInstance = new object();
        private DeviceBase _selectDevice = null;

        #endregion 局部变量

        private DeviceManager()
        {
            //PluginsManager.TypeCollection.ForEach(type =>
            //{
            //    var device = (DeviceBase)Activator.CreateInstance(type);
            //    Devices.Add(device);
            //});
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

        public void AddNew(DeviceBase device)
        {
            Devices.Add(device);
        }

    }
}
