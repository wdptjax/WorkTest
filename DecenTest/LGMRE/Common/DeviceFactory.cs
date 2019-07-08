using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Common
{
    public static class DeviceFactory
    {
        private static object _lockDeviceList = new object();
        private static List<IDevice> _deviceList = new List<IDevice>();

        public static IDevice CreateDeviceInstance(string deviceType)
        {
            Type type = PluginsManager.GetTye(deviceType);
            if (type != null)
            {
                lock (_lockDeviceList)
                {
                    if (_deviceList.Exists(d => d.GetType() == type))
                    {
#warning 这里暂时只实例化一个实例，如果已经存在实例化的则返回之前实例化的设备，后面根据实际情况修改
                        return _deviceList.Find(d => d.GetType() == type);
                    }
                    var instance = Activator.CreateInstance(type);
                    if (instance != null && instance is IDevice)
                    {
                        IDevice dev = (IDevice)instance;
                        _deviceList.Add(dev);
                        return dev;
                    }
                }
                return null;
            }

            return null;
        }

        public static void DisposeDevice(IDevice device)
        {
            device.Close();
            lock(_lockDeviceList)
            {
                if(_deviceList.Contains(device))
                {
                    _deviceList.Remove(device);
                }
            }
        }

    }
}
