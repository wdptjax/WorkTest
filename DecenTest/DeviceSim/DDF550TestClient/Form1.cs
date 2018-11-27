using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace DDF550TestClient
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            cboCmd.Items.Add("None");
            cboCmd.Items.Add("Options");
            txtIp.Text = "127.0.0.1";
        }

        private Socket _cmdSocket = null;

        public static readonly byte[] Xml_Start = new byte[] { 0xB1, 0xC2, 0xD3, 0xE4 };
        public static readonly byte[] Xml_End = new byte[] { 0xE4, 0xD3, 0xC2, 0xB1 };

        private void btnConnect_Click(object sender, EventArgs e)
        {
            string ip = txtIp.Text;
            int port = (int)numPort.Value;
            IPEndPoint ep1 = new IPEndPoint(IPAddress.Parse(ip), port);
            _cmdSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _cmdSocket.Connect(ep1);
            _cmdSocket.NoDelay = true;
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            string str = txtSendStr.Text;
            byte[] buffer = Encoding.ASCII.GetBytes(str);
            byte[] cmd = PackedXMLCommand(buffer);
            SendCmd(cmd);
        }

        private void cboCmd_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboCmd.Text == "None")
            {
                txtSendStr.Text = "";
                txtSendHex.Text = "";
            }
            if (cboCmd.Text == "Options")
            {
                Request request = new Request();
                request.Excute(OpreationMode.Get, "Options");
                byte[] buffer = XmlWrapper.SerializeObject(request);
                byte[] cmd = PackedXMLCommand(buffer);
                string str = Encoding.ASCII.GetString(buffer);
                string strHex = BitConverter.ToString(cmd).Replace("-", "");
                txtSendStr.Text = str;
                txtSendHex.Text = strHex;
            }
        }

        /// <summary>
        /// 发送命令
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns>返回应答信息</returns>
        private void SendCmd(byte[] cmd)
        {
            try
            {
                //int offset = 0;
                int recBytes = 0;
                string result = string.Empty;
                lock (this)
                {
                    byte[] sendBuffer = PackedXMLCommand(cmd);
                    _cmdSocket.Send(sendBuffer, 0, sendBuffer.Length, SocketFlags.None);
                    //Thread.Sleep(100);
                    List<byte> cacheCmd = new List<byte>();
                    try
                    {
                        while (true)
                        {
                            byte[] buffer = new byte[4096];
                            //recBytes = _cmdSocket.Receive(_cmdReply, offset, _cmdReply.Length - offset, SocketFlags.None);
                            recBytes = _cmdSocket.Receive(buffer, 0, buffer.Length, SocketFlags.None);
                            if (recBytes == 0)
                            {
                                break;
                            }

                            if (UnPackedXMLCommand(buffer.Take(recBytes).ToArray(), ref cacheCmd))
                            {
                                string str = BitConverter.ToString(cacheCmd.ToArray()).Replace("-", " ");
                                txtRecvHex.Invoke(new MethodInvoker(() => txtRecvHex.Text += str + "\r\n"));

                                if (!CheckXMLCommand(cacheCmd.ToArray()))
                                {
                                    MessageBox.Show(this, "!Error Recv");
                                    return;
                                }

                                //offset += recBytes;
                                int len = cacheCmd.Count;

                                byte[] data = cacheCmd.Skip(8).Take(len - 13).ToArray();
                                result = Encoding.ASCII.GetString(data);
                                txtRecvStr.Invoke(new MethodInvoker(() => txtRecvStr.Text += result + "\r\n"));
                                if (result.Substring(result.Length - 8, 8) == "</Reply>")
                                {
                                    MessageBox.Show(this, "!Error Recv2");
                                }
                                cacheCmd.Clear();
                                return;
                            }
                        }
                    }
                    catch
                    {
                        return;
                    }
                }
            }
            catch
            {
                return;
            }
        }

        //装包
        private byte[] PackedXMLCommand(byte[] cmd)
        {
            List<byte> arr = new List<byte>();
            arr.AddRange(Xml_Start);
            int len = cmd.Length + 1;
            byte[] lenArr = BitConverter.GetBytes(len);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(lenArr);
            arr.AddRange(lenArr);
            arr.AddRange(cmd);
            arr.Add(0x00);//添加终止符\0
            arr.AddRange(Xml_End);
            return arr.ToArray();
        }

        //解包
        private bool UnPackedXMLCommand(byte[] data, ref List<byte> cacheCmd)
        {
            cacheCmd.AddRange(data);
            byte[] buffer = cacheCmd.ToArray();
            cacheCmd.Clear();
            var find1 = buffer.IndexesOf(0, Xml_Start);
            var find2 = buffer.IndexesOf(0, Xml_End);
            int count1 = find1.Count();
            int count2 = find2.Count();
            if (count1 > 0 && count2 > 0)
            {
                //有头有尾,找第一个符合条件的
                int index1 = find1.First();
                int index2 = find2.FirstOrDefault(i => i > index1);//如果找不到符合条件的,则返回0
                if (index2 > 0)
                {
                    //尾在头后,正常
                    cacheCmd.AddRange(buffer.Skip(index1).Take(index2 + Xml_End.Length - index1));
                    return true;
                }
                else
                {
                    //尾在头前
                    cacheCmd.AddRange(data.Skip(index1));
                    return false;
                }
            }
            else if (count1 == 0 && count2 > 0)
            {
                //没头有尾
                return false;
            }
            else if (count1 > 0 && count2 == 0)
            {
                //有头没尾
                int index1 = find1.First();
                cacheCmd.AddRange(data.Skip(index1));
                return false;
            }
            else
            {
                //没头没尾
                return false;
            }
        }

        //校验
        private bool CheckXMLCommand(byte[] data)
        {
            byte[] lenData = new byte[4];
            Buffer.BlockCopy(data, 4, lenData, 0, 4);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(lenData);
            int len = BitConverter.ToInt32(lenData, 0);
            return len == data.Length - 12;
        }

    }

    internal static class Extend
    {
        /// <summary>
        /// 查找一个数组中与另外一个数组相匹配的位置集合
        /// </summary>
        /// <param name="source"></param>
        /// <param name="start"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static IEnumerable<int> IndexesOf(this byte[] source, int start, byte[] pattern)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (pattern == null)
            {
                throw new ArgumentNullException(nameof(pattern));
            }

            int valueLength = source.Length;
            int patternLength = pattern.Length;

            if ((valueLength == 0) || (patternLength == 0) || (patternLength > valueLength))
            {
                yield break;
            }

            var badCharacters = new int[256];

            for (var i = 0; i < 256; i++)
            {
                badCharacters[i] = patternLength;
            }

            var lastPatternByte = patternLength - 1;

            for (int i = 0; i < lastPatternByte; i++)
            {
                badCharacters[pattern[i]] = lastPatternByte - i;
            }

            int index = start;

            while (index <= valueLength - patternLength)
            {
                for (var i = lastPatternByte; source[index + i] == pattern[i]; i--)
                {
                    if (i == 0)
                    {
                        yield return index;
                        break;
                    }
                }

                index += badCharacters[source[index + lastPatternByte]];
            }
        }

        /// <summary>
        /// 查找一个数组中与另外一个数组匹配的第一个项的位置
        /// </summary>
        /// <param name="source"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static int IndexesOf(this byte[] source, byte[] pattern)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (pattern == null)
            {
                throw new ArgumentNullException(nameof(pattern));
            }

            int valueLength = source.Length;
            int patternLength = pattern.Length;

            if ((valueLength == 0) || (patternLength == 0) || (patternLength > valueLength))
            {
                return -1;
            }

            var badCharacters = new int[256];

            for (var i = 0; i < 256; i++)
            {
                badCharacters[i] = patternLength;
            }

            var lastPatternByte = patternLength - 1;

            for (int i = 0; i < lastPatternByte; i++)
            {
                badCharacters[pattern[i]] = lastPatternByte - i;
            }

            int index = 0;

            while (index <= valueLength - patternLength)
            {
                for (var i = lastPatternByte; source[index + i] == pattern[i]; i--)
                {
                    if (i == 0)
                    {
                        return index;
                    }
                }

                index += badCharacters[source[index + lastPatternByte]];
            }
            return -1;
        }
    }

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

        public Command()
        {
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

        public Request()
        {
            Type = "get";
            Id = "123";
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
            if (!string.IsNullOrEmpty(paramName))
            {
                Param param = new Param();
                param.Name = paramName;
                param.Value = paramValue;
                Command.Params.Add(param);
            }
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
