
/*********************************************************************************************
 *	
 * 文件名称:    PluginsManager.cs
 *
 * 作    者:    吴德鹏
 *	
 * 创作日期:    2018-10-22 16:17:08
 * 
 * 备    注:    描述类的用途
 *              
 *                                
 *               
*********************************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DeviceSimlib
{
    public class PluginsManager
    {
        public static List<Type> TypeCollection = new List<Type>();
        public static void Initializer()
        {
            string dir = AppDomain.CurrentDomain.BaseDirectory + "\\Device\\";
            foreach (var path in Directory.GetFiles(dir))
            {
                if (Path.GetExtension(path) == ".dll")
                {
                    Assembly ass = Assembly.LoadFile(path);
                    foreach (var type in ass.GetTypes())
                    {
                        if (type.BaseType == typeof(DeviceBase))
                        {
                            TypeCollection.Add(type);
                        }
                    }
                }
            }
        }

        public static Type GetTye(string name)
        {
            foreach(var type in TypeCollection)
            {
                if (type.FullName == name)
                    return type;
            }
            return null;
        }
    }
}
