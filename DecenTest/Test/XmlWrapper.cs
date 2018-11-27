/*********************************************************************************************
 *	
 * 文件名称:    ..\Tracker800\Server\Source\Framework\Contract\PublicClass\XmlWrapper.cs
 *
 * 作    者:    吴 刚
 *	
 * 创作日期:    2018-11-01
 * 
 * 备    注:	在这个文件下，封装对Xml的序列化和反序列化的方法
 *               
*********************************************************************************************/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Tracker800.Server.Device
{
    public enum OpreationMode
    {
        Get,
        Set
    }

    public class Param
    {
        [XmlAttribute("name")]
        public string Name;
        [XmlText]
        public string Value;
    }

    public class Struct
    {
        [XmlElement("Param")]
        public List<Param> Params;
    }

    public class Command
    {
        [XmlAttribute("name")]
        public string Name;
        [XmlAttribute("returnCode")]
        public string RtnCode;
        [XmlAttribute("returnMessage")]
        public string RtnMessage;

        [XmlElement("Param")]
        public List<Param> Params;
        [XmlElement("Array")]
        public List<Struct> Structs;
        [XmlText]
        public string Value;

        public Command()
        {
            Value = string.Empty;
            Params = new List<Param>();
            Structs = new List<Struct>();
        }
    }

    [XmlRoot("Reply")]
    public class Reply
    {
        [XmlAttribute("type")]
        public string Type;
        [XmlAttribute("id")]
        public string Id;
        [XmlElement("Command")]
        public Command Command;

        public Reply()
        {
            Command = new Command();
        }
    }

    [XmlRoot("Request")]
    public class Request
    {
        [XmlAttribute("type")]
        public string Type;
        [XmlAttribute("id")]
        public string Id;
        [XmlElement("Command")]
        public Command Command;

        private static int _idSign=123;
        private static object _lockSign = new object();

        public Request()
        {
            Type = "get";
            lock (_lockSign)
            {
                _idSign++;
                if (_idSign >= 100000)
                    _idSign = 123;
            }
            Id = _idSign.ToString();
            Command = new Command();
        }

        /// <summary>
        /// 查询指令
        /// </summary>
        /// <param name="commandName"></param>
        public void Query(string commandName)
        {
            Type = "get";
            Command.Name = commandName;
        }

        /// <summary>
        /// 设置或查询
        /// </summary>
        /// <param name="opreationMode">操作方式（查询/设置）</param>
        /// <param name="commandName">命令名称</param>
        /// <param name="paramName">参数名称</param>
        /// <param name="paramValue">参数值</param>
        public void Excute(OpreationMode opreationMode, string commandName, string paramName = null, string paramValue = null)
        {
            if (opreationMode == OpreationMode.Set)
            {
                Type = "set";
            }
            else
            {
                Type = "get";
            }

            Command.Name = commandName;
            Param param = new Param();
            param.Name = paramName;
            param.Value = paramValue;
            Command.Params.Add(param);
        }
    }

    public static class XmlWrapper
    {
        /// <summary>
        /// 将对象序列化成xml
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>返回byte数组</returns>
        public static byte[] SerializeObject(object obj)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.OmitXmlDeclaration = true;
            settings.NewLineChars = "\r\n";
            settings.Encoding = Encoding.ASCII;

            using (MemoryStream ms = new MemoryStream())
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(ms, settings))
                {
                    XmlSerializerNamespaces xmlSerializerNamespaces = new XmlSerializerNamespaces();
                    xmlSerializerNamespaces.Add(string.Empty, string.Empty);
                    XmlSerializer xs = new XmlSerializer(obj.GetType());
                    xs.Serialize(xmlWriter, obj, xmlSerializerNamespaces);
                    string test = Encoding.ASCII.GetString(ms.ToArray());
                    return ms.ToArray();
                }
            }
        }

        /// <summary>
        /// 将byte数组反序列化成对象
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static object DeserializeObject<T>(byte[] data)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            settings.IgnoreWhitespace = true;
            using (MemoryStream ms = new MemoryStream(data))
            {
                using (XmlReader xmlReader = XmlReader.Create(ms, settings))
                {
                    XmlSerializer xs = new XmlSerializer(typeof(T));
                    return (T)xs.Deserialize(xmlReader);
                }
            }
        }
    }
}
