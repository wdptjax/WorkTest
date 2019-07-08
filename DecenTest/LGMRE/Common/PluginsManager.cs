using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Common
{
    internal class PluginsManager
    {
        private static List<Type> _typeCollection = null;
        private static object _lockType = new object();
        public static List<Type> TypeCollection
        {
            get
            {
                if (_typeCollection == null)
                {
                    lock (_lockType)
                    {
                        Initializer();
                    }
                }
                return _typeCollection;
            }
        }

        private static void Initializer()
        {
            try
            {
                _typeCollection = new List<Type>();
                string dir = AppDomain.CurrentDomain.BaseDirectory + Define.DEVICE_PATH;
                foreach (var path in Directory.GetFiles(dir))
                {
                    if (Path.GetExtension(path) == ".dll")
                    {
                        Assembly ass = Assembly.LoadFile(path);
                        foreach (var type in ass.GetTypes())
                        {
                            if (type.GetInterfaces().Contains(typeof(IDevice)))
                            {
                                _typeCollection.Add(type);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }

        }

        public static Type GetTye(string name)
        {
            foreach (var type in TypeCollection)
            {
                if (type.FullName == name)
                {
                    return type;
                }
            }
            return null;
        }
    }
}
