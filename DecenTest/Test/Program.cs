using ODM20181102TJ01;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tracker800.Server.Device;

namespace Test
{
    static class Extend
    {
        public static int IndexesOf(this byte[] source, byte[] pattern, int startIndex)
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

            int index = startIndex;

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

    class ClientInfo
    {
        public IPEndPoint Address;

        public bool IsSendAudio = false;
        public bool IsSendIQ = false;
        public bool IsSendITU = false;
        public bool IsSendSpectrum = false;
        public bool IsSendDF = false;

        public int IndexAudio = 0;
        public int IndexIQ = 0;
        public int IndexITU = 0;
        public int IndexSpectrum = 0;
        public int IndexDF = 0;
    }

    class Program
    {


        static void Main(string[] args)
        {
            int[] k = new int[10];
            //CreateDir("D:\\Testttttt\\");
            //Test2();
            //Test3();
            //TestUdp();
            //string str = "I6\r\n";
            //byte[] buffer = Encoding.ASCII.GetBytes(str);
            //string hex = BitConverter.ToString(buffer).Replace("-", " ");

            //IPEndPoint ep2 = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1720);
            //Socket dataReceiveSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //dataReceiveSocket.Connect(ep2);
            //dataReceiveSocket.NoDelay = true;
            //string _localIp = (dataReceiveSocket.LocalEndPoint as IPEndPoint).Address.ToString();
            //int _localPort = (dataReceiveSocket.LocalEndPoint as IPEndPoint).Port;

            //TestFindByteArray();

            //TestComport();

            //EBD195Sim();
            //TestIDRAC();
            //TestDDF550();
            //Task.Factory.StartNew(new Action(() => PingStatus()));
            //DDF550SendAsyn();
            //TestReverse();

            Test_ODM20181102TJ01();
            Console.ReadLine();

        }

        #region 测试天津项目

        static void Test_ODM20181102TJ01()
        {
            ODM20181102TJ01.Test test = new ODM20181102TJ01.Test();
            test.Test_ODM20181102TJ01();
        }


        #endregion 测试天津项目

        #region 测试ping
        static void PingStatus()
        {
            while (true)
            {
                Thread.Sleep(1000);
                using (Ping pingSender = new Ping())
                {
                    try
                    {
                        PingReply reply = pingSender.Send("192.168.0.120", 200);//第一个参数为ip地址，第二个参数为ping的时间 
                        Console.WriteLine(DateTime.Now.ToString() + " " + reply.Status.ToString());
                        if (reply.Status != IPStatus.Success)
                        {

                        }
                    }
                    catch
                    {
                    }
                }
            }
        }
        #endregion

        #region 测试戴尔

        static void TestIDRAC()
        {
            Process cmd = new Process();//创建进程对象 

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = @"E:\Program Files\Dell\SysMgt\rac5\racadm.exe";//设定需要执行的命令  
            startInfo.Arguments = "";//“/C”表示执行完命令后马上退出  
            startInfo.UseShellExecute = false;//不使用系统外壳程序启动  
            startInfo.RedirectStandardInput = true;//不重定向输入  
            startInfo.RedirectStandardOutput = true; //重定向输出  
            startInfo.CreateNoWindow = true;//不创建窗口  
            cmd.StartInfo = startInfo;
            cmd.Start();
            cmd.WaitForExit();
            string str = cmd.StandardOutput.ReadToEnd();
        }


        #endregion 测试戴尔

        #region 测试效率

        static void TestList()
        {
            int[] source = new int[100000];
            Random rd = new Random();
            for (int i = 0; i < 100000; i++)
            {
                source[i] = rd.Next();
            }

            Stopwatch sw = new Stopwatch();
            sw.Start();
            List<byte> list = new List<byte>();
            for (int i = 0; i < 100000; i++)
            {
                list.AddRange(BitConverter.GetBytes(source[i]));
            }
            byte[] arr = list.ToArray();
            sw.Stop();
            Console.WriteLine("List " + sw.ElapsedMilliseconds + " 毫秒");//11ms
            Stopwatch sw1 = new Stopwatch();
            sw1.Start();
            byte[] buffer = new byte[100000 * 4];
            for (int i = 0; i < 100000; i++)
            {
                byte[] data = BitConverter.GetBytes(source[i]);
                Buffer.BlockCopy(data, 0, buffer, i * 4, data.Length);
            }
            sw1.Stop();
            Console.WriteLine("Array " + sw1.ElapsedMilliseconds + " 毫秒");//2ms

            Console.ReadLine();
        }
        static void TestReverse()
        {
            while (true)
            {
                int[] source = new int[100000];
                Random rd = new Random();
                for (int i = 0; i < 100000; i++)
                {
                    source[i] = rd.Next();
                }

                Stopwatch sw = new Stopwatch();
                sw.Start();
                byte[] buffer1 = new byte[100000 * 4];
                for (int i = 0; i < 100000; i++)
                {
                    byte[] data = BitConverter.GetBytes(source[i]).Reverse().ToArray();
                    Buffer.BlockCopy(data, 0, buffer1, i * 4, data.Length);
                }
                sw.Stop();
                Console.WriteLine("ReverseToArray " + sw.ElapsedMilliseconds + " 毫秒");//效率低
                Stopwatch sw1 = new Stopwatch();
                sw1.Start();
                byte[] buffer2 = new byte[100000 * 4];
                for (int i = 0; i < 100000; i++)
                {
                    byte[] data = BitConverter.GetBytes(source[i]);
                    Array.Reverse(data);
                    Buffer.BlockCopy(data, 0, buffer2, i * 4, data.Length);
                }
                sw1.Stop();
                Console.WriteLine("ArrayReverse " + sw1.ElapsedMilliseconds + " 毫秒");//效率高

                string str = Console.ReadLine();
                if (!string.IsNullOrEmpty(str))
                {
                    break;
                }
            }
        }

        #endregion 测试效率

        #region DDF550发送模拟数据

        static List<byte[]> _audioData = new List<byte[]>();
        static List<byte[]> _iqData = new List<byte[]>();
        static List<byte[]> _spectrumData = new List<byte[]>();
        static List<byte[]> _ituData = new List<byte[]>();
        static List<byte[]> _dfData = new List<byte[]>();
        static List<ClientInfo> _clientList = new List<ClientInfo>();
        static object _lockClient = new object();
        static Thread thd1 = null;
        static Thread thd2 = null;
        static TcpListener _tcpListener1 = null;
        static TcpListener _tcpListener2 = null;
        static byte[] _xmlHeader = { 0xB1, 0xC2, 0xD3, 0xE4 };
        static byte[] _xmlEnd = { 0xE4, 0xD3, 0xC2, 0xB1 };

        static void DDF550SendAsyn()
        {
            string path = @"D:\文件\工作\DDF550\出差收集到的数据\抓包数据\EB200data\2018-11-22-data_Audio.txt";
            _audioData = GetData(path);
            path = @"D:\文件\工作\DDF550\出差收集到的数据\抓包数据\EB200data\2018-11-22-data_IF.txt";
            _iqData = GetData(path);
            path = @"D:\文件\工作\DDF550\出差收集到的数据\抓包数据\EB200data\2018-11-22-data_IFPan.txt";
            _spectrumData = GetData(path);
            path = @"D:\文件\工作\DDF550\出差收集到的数据\抓包数据\EB200data\2018-11-22-data_CW.txt";
            _ituData = GetData(path);
            path = @"D:\文件\工作\DDF550\出差收集到的数据\抓包数据\EB200data\2018-11-23-data_DFPScan.txt";
            _dfData = GetData(path);

            IPEndPoint iPEndPoint1 = new IPEndPoint(IPAddress.Any, 5563);
            IPEndPoint iPEndPoint2 = new IPEndPoint(IPAddress.Any, 5565);
            _tcpListener1 = new TcpListener(iPEndPoint1);
            _tcpListener1.Start();
            _tcpListener2 = new TcpListener(iPEndPoint2);
            _tcpListener2.Start();

            thd1 = new Thread(ScanXmlCmd);
            thd1.IsBackground = true;
            thd1.Start();
            thd2 = new Thread(ScanDataSendEB200);
            thd2.IsBackground = true;
            thd2.Start();
        }

        static void ScanXmlCmd()
        {
            while (true)
            {
                Socket socket = _tcpListener1.AcceptSocket();
                IPEndPoint iPEndPoint = socket.RemoteEndPoint as IPEndPoint;
                Console.WriteLine("XML服务接收到连接:" + iPEndPoint.ToString());
                while (true)
                {
                    try
                    {
                        byte[] buffer = new byte[1024 * 1024];
                        int count = socket.Receive(buffer, SocketFlags.None);
                        if (count == 0)
                        {
                            break;
                        }

                        int index = buffer.IndexesOf(_xmlHeader, 0);
                        int offset = index;
                        while (offset < count)
                        {
                            if (BitConverter.IsLittleEndian)
                            {
                                Array.Reverse(buffer, index + 4, 4);
                            }

                            int datalen = BitConverter.ToInt32(buffer, index + 4);

                            byte[] xmlData = new byte[datalen - 1];
                            Buffer.BlockCopy(buffer, index + 4 + 4, xmlData, 0, datalen - 1);
                            string strXml = Encoding.ASCII.GetString(xmlData).TrimEnd('0');

                            Request xml = (Request)XmlWrapper.DeserializeObject<Request>(xmlData);
                            if (xml.Command.Name == "TraceEnable" || xml.Command.Name == "TraceDisable")
                            {
                                lock (_lockClient)
                                {
                                    string ip = "";
                                    int port = 0;
                                    bool enabled = xml.Command.Name == "TraceEnable";
                                    List<Param> paras = xml.Command.Params;
                                    paras.ForEach(p =>
                                    {
                                        if (p.Name == "zIP")
                                        {
                                            ip = p.Value;
                                        }
                                        else if (p.Name == "iPort")
                                        {
                                            int.TryParse(p.Value, out port);
                                        }
                                    });
                                    ClientInfo info = _clientList.FirstOrDefault(i => i.Address.Address.ToString() == ip && i.Address.Port == port);
                                    if (info != null)
                                    {
                                        bool isSendAudio = info.IsSendAudio;
                                        bool isSendSpectrum = info.IsSendSpectrum;
                                        bool isSendIQ = info.IsSendIQ;
                                        bool isSendITU = info.IsSendITU;
                                        bool isSendDF = info.IsSendDF;

                                        paras.ForEach(p =>
                                        {
                                            if (p.Name == "eTraceTag")
                                            {
                                                switch (p.Value)
                                                {
                                                    case "TRACETAG_AUDIO":
                                                        isSendAudio = enabled;
                                                        Console.WriteLine("连接:" + info.Address.ToString() + ",音频开关状态" + enabled.ToString());
                                                        break;
                                                    case "TRACETAG_IFPAN":
                                                        isSendSpectrum = enabled;
                                                        Console.WriteLine("连接:" + info.Address.ToString() + ",频谱开关状态" + enabled.ToString());
                                                        break;
                                                    case "TRACETAG_IF":
                                                        isSendIQ = enabled;
                                                        Console.WriteLine("连接:" + info.Address.ToString() + ",IQ开关状态" + enabled.ToString());
                                                        break;
                                                    case "TRACETAG_CWAVE":
                                                        isSendITU = enabled;
                                                        Console.WriteLine("连接:" + info.Address.ToString() + ",ITU开关状态" + enabled.ToString());
                                                        break;
                                                    case "TRACETAG_DF":
                                                        isSendDF = enabled;
                                                        Console.WriteLine("连接:" + info.Address.ToString() + ",测向开关状态" + enabled.ToString());
                                                        break;
                                                }
                                            }
                                        });

                                        info.IsSendAudio = isSendAudio;
                                        info.IsSendIQ = isSendIQ;
                                        info.IsSendITU = isSendITU;
                                        info.IsSendSpectrum = isSendSpectrum;
                                        info.IsSendDF = isSendDF;
                                    }
                                }
                            }

                            Reply reply = new Reply();
                            reply.Command = xml.Command;
                            reply.Id = xml.Id;
                            reply.Type = xml.Type;
                            byte[] rep = XmlWrapper.SerializeObject(reply);
                            byte[] sendData = PackedXMLCommand(rep);
                            socket.Send(sendData);

                            offset += 4 + 4 + datalen + 4;
                        }
                    }
                    catch (Exception)
                    {
                        break;
                    }
                }
            }
        }

        static void ScanDataSendEB200()
        {
            while (true)
            {
                Socket socket = _tcpListener2.AcceptSocket();
                IPEndPoint iPEndPoint = socket.RemoteEndPoint as IPEndPoint;
                ClientInfo info = new ClientInfo();
                info.Address = iPEndPoint;
                Console.WriteLine("数据服务接收到连接:" + iPEndPoint.ToString());
                lock (_lockClient)
                {
                    _clientList.Add(info);
                }
                while (true)
                {
                    try
                    {
                        Thread.Sleep(30);
                        if (info.IsSendAudio)
                        {
                            byte[] data = _audioData[info.IndexAudio];
                            info.IndexAudio++;
                            if (info.IndexAudio >= _audioData.Count)
                            {
                                info.IndexAudio = 0;
                            }

                            socket.Send(data);
                        }
                        if (info.IsSendIQ)
                        {
                            byte[] data = _iqData[info.IndexIQ];
                            info.IndexIQ++;
                            if (info.IndexIQ >= _iqData.Count)
                            {
                                info.IndexIQ = 0;
                            }

                            socket.Send(data);
                        }
                        if (info.IsSendITU)
                        {
                            byte[] data = _ituData[info.IndexITU];
                            info.IndexITU++;
                            if (info.IndexITU >= _ituData.Count)
                            {
                                info.IndexITU = 0;
                            }

                            socket.Send(data);
                        }
                        if (info.IsSendSpectrum)
                        {
                            byte[] data = _spectrumData[info.IndexSpectrum];
                            info.IndexSpectrum++;
                            if (info.IndexSpectrum >= _spectrumData.Count)
                            {
                                info.IndexSpectrum = 0;
                            }

                            socket.Send(data);
                        }
                        if (info.IsSendDF)
                        {
                            byte[] data = _dfData[info.IndexDF];
                            info.IndexDF++;
                            if (info.IndexDF >= _dfData.Count)
                            {
                                info.IndexDF = 0;
                            }

                            socket.Send(data);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        //装包
        static byte[] PackedXMLCommand(byte[] cmd)
        {
            List<byte> arr = new List<byte>();
            arr.AddRange(_xmlHeader);
            int len = cmd.Length + 1;
            byte[] lenArr = BitConverter.GetBytes(len);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(lenArr);
            }

            arr.AddRange(lenArr);
            arr.AddRange(cmd);
            arr.Add(0x00);//添加终止符\0
            arr.AddRange(_xmlEnd);
            return arr.ToArray();
        }

        static List<byte[]> GetData(string filename)
        {
            List<byte[]> recvData = new List<byte[]>();
            FileStream fs = null;
            StreamReader sr = null;
            try
            {
                fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
                sr = new StreamReader(fs);
                string str = sr.ReadLine();
                while (!sr.EndOfStream)
                {
                    int index = str.IndexOf("拼接数据:");
                    string strData = str.Substring(index + 5);
                    string[] arr = strData.Split('-');
                    byte[] data = arr.Select(i => Convert.ToByte(i, 16)).ToArray();
                    recvData.Add(data);
                    str = sr.ReadLine();
                }

                sr.Close();
                fs.Close();
                return recvData;
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                if (sr != null)
                {
                    sr.Dispose();
                }

                if (fs != null)
                {
                    fs.Dispose();
                }
            }
        }

        #endregion DDF550发送模拟数据

        #region 测试DDF550

        static void TestDDF550()
        {
            string strData = "00:0e:b2:00:00:64:00:02:02:ef:00:00:00:00:0b:7a:14:b5:00:00:00:00:0b:52:3c:3f:78:6d:00:a1:2f:90:00:00:00:00:05:0f:ab:20:00:00:00:c7:00:00:00:00:00:00:00:4c:80:0c:f8:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:00:01:00:00:01:91:00:00:00:00:05:3e:c6:00:00:00:00:00:00:00:c3:50:00:00:00:01:00:98:96:80:46:dd:ba:7f:00:01:86:64:00:5f:fe:d4:00:00:ff:ff:03:20:20:01:ff:ff:ff:ff:ff:ff:ff:ff:15:69:4d:9f:61:a0:6f:b0:0f:11:00:00:00:00:00:00:ff:67:ff:61:ff:5f:ff:5e:ff:b9:00:99:01:54:00:68:ff:c1:ff:64:ff:5c:ff:5f:ff:5e:ff:5d:ff:5d:ff:5b:ff:5f:ff:5c:ff:61:ff:5f:ff:5d:ff:58:ff:64:ff:72:00:06:00:ba:01:55:00:ad:00:02:ff:6f:ff:60:ff:5f:ff:5c:ff:5f:ff:5f:ff:5f:ff:60:ff:65:ff:69:ff:60:ff:80:00:4d:01:0c:00:45:ff:83:ff:65:ff:68:ff:6b:ff:6a:ff:76:ff:e7:00:fb:01:f4:01:16:ff:ce:ff:72:ff:6a:ff:67:ff:65:ff:6b:ff:6a:ff:66:ff:66:ff:6a:ff:c9:00:60:01:41:00:93:ff:b5:ff:69:ff:64:ff:66:ff:5e:ff:61:ff:63:ff:61:ff:64:ff:60:ff:5e:ff:65:ff:e1:ff:65:ff:63:ff:69:ff:66:ff:63:ff:63:ff:61:ff:63:ff:62:ff:a2:00:a5:01:69:00:94:ff:9f:ff:64:ff:61:ff:5f:ff:a1:ff:a9:00:d9:ff:a7:ff:64:ff:5f:ff:64:ff:64:ff:d1:ff:62:ff:64:00:15:01:63:00:41:ff:63:ff:61:ff:68:ff:67:ff:63:ff:62:00:3d:ff:64:ff:62:ff:61:ff:62:ff:63:ff:61:ff:5f:ff:67:00:0d:01:3a:00:24:ff:63:ff:63:ff:6f:ff:61:ff:61:ff:5f:ff:60:ff:ef:01:71:ff:d4:ff:62:ff:61:ff:63:ff:62:ff:60:ff:65:ff:64:ff:60:ff:5f:ff:60:ff:62:ff:65:ff:7b:ff:65:ff:65:ff:5f:ff:63:ff:65:ff:64:ff:61:ff:5f:00:27:01:9a:00:1c:ff:65:ff:66:ff:75:ff:66:ff:66:ff:61:ff:64:ff:62:ff:62:ff:6d:ff:ff:ff:8b:ff:e6:ff:82:ff:68:00:13:01:27:00:04:ff:8e:ff:62:ff:61:ff:61:ff:65:ff:61:ff:5e:ff:61:ff:5f:ff:62:ff:5d:ff:60:ff:5e:ff:60:ff:60:ff:63:ff:61:0e:0d:0d:41:06:cd:00:31:0d:87:0d:ab:0d:55:00:62:0c:bb:0e:0a:0d:4d:0c:0e:06:a3:03:14:08:93:04:de:06:02:03:cf:05:93:07:1c:0a:84:04:8b:0a:f3:0b:2a:0b:a1:0b:af:0a:fe:0b:14:0b:6c:0a:c2:02:1d:02:81:0a:44:04:41:0c:53:0d:30:08:5e:05:a9:02:c8:05:53:0b:33:0b:0a:0b:2d:0b:30:0b:22:04:87:05:27:05:26:04:ca:04:d0:05:19:04:ef:04:f2:05:0d:04:f4:04:d1:04:ee:05:84:04:ed:04:8e:01:ad:05:55:05:51:06:1a:0c:a4:0c:21:00:13:09:64:0c:aa:0d:a6:09:66:07:38:07:75:07:4a:05:39:03:93:0c:b0:06:f5:0c:33:02:b3:00:30:07:97:08:82:0b:45:00:dd:04:43:02:71:0d:5d:02:4a:02:99:02:97:02:04:02:14:02:28:02:1a:02:d6:0d:3f:03:53:04:e2:05:ad:05:12:06:15:05:3f:0d:49:06:78:09:c4:00:f0:09:d0:03:6a:02:2c:02:08:01:ec:05:c7:0d:1b:02:fd:05:58:08:a2:0d:c3:0b:e0:0b:1c:03:54:0d:ec:0b:46:01:ab:07:5a:0a:76:03:c3:04:a2:04:75:04:89:08:4f:01:8b:09:09:09:30:00:d2:06:2b:01:82:02:22:02:18:02:2e:00:ee:05:30:0a:78:05:b7:0b:87:06:0b:0d:5e:0c:7a:00:a3:0e:04:03:ec:0c:13:0b:18:06:f0:04:f7:04:93:01:57:08:4d:04:ad:06:f9:00:78:02:48:02:59:02:65:06:bf:0a:da:0a:05:0b:95:0c:00:08:a8:07:15:0c:ca:09:06:00:db:01:04:0b:a7:0b:e4:0b:bc:06:89:02:35:02:7f:02:47:00:7d:07:0e:07:0a:0c:fc:02:95:03:b7:0d:8a:04:89:0d:44:03:f0:0c:ff:07:b7:03:00:00:b7:0a:c0:0c:da:0d:ca:02:ff:02:10:02:1e:01:72:03:e4:03:dc:03:e5:03:db:03:de:03:09:02:22:01:57:00:43:01:98:00:04:00:f3:01:dd:00:bd:01:de:01:2b:00:6c:00:9e:00:e4:03:3e:03:46:02:97:02:d1:03:1f:03:43:02:8e:00:ba:00:c5:01:3d:01:49:01:5d:01:af:02:36:02:03:01:1e:03:1a:03:2e:03:60:03:53:03:78:03:47:03:8e:03:b9:03:66:03:a3:03:c3:03:da:03:dd:03:e3:03:e3:03:bb:03:d7:03:5b:03:2b:03:72:03:ab:01:f8:02:d2:03:64:02:67:03:ce:03:4e:03:d1:03:98:02:70:00:c8:00:40:00:e3:01:67:00:bc:00:d7:01:96:01:a9:01:a3:01:8f:01:82:03:e4:01:c0:01:0d:01:98:02:d5:02:78:01:7e:01:4b:01:5b:01:8f:03:d3:03:e4:03:e4:03:e2:03:db:01:73:01:12:01:27:02:79:03:cc:03:e2:03:c4:02:99:01:d8:01:b6:00:c8:03:d4:01:af:02:7e:03:ac:03:cb:03:e4:02:ce:00:48:01:e9:01:28:00:16:00:06:03:e3:01:e4:01:15:01:bd:00:c7:00:04:00:99:01:86:02:3e:03:32:03:38:03:06:01:e7:00:58:02:cb:00:47:01:16:00:dd:01:4a:03:da:03:e6:03:cb:00:af:01:b4:02:70:00:c3:00:0f:00:9e:01:5b:01:0c:00:d3:01:cc:00:11:02:a9:03:2d:02:b2:01:c0:00:c5:02:3d:00:60:01:52:00:57:01:00:03:db:03:e0:03:dc:01:e6:00:b0:03:c8:01:04:01:af:00:14:01:db:00:19:01:be:03:7c:03:e2:03:ce:03:cb:03:d1:02:fd:03:e5:03:d8:03:e4:03:84:00:74:00:95:01:57:02:79:00:99:01:75:01:14:00:df:00:ec:00:a3:01:41:01:90:02:10:01:29:01:b4:01:8e:00:fe:00:f8:00:f6:00:f5:01:50:02:2f:02:ea:01:fe:01:57:00:fa:00:f2:00:f5:00:f4:00:f3:00:f2:00:f0:00:f4:00:f1:00:f6:00:f4:00:f2:00:ed:00:f9:01:06:01:9a:02:4e:02:e9:02:41:01:96:01:03:00:f4:00:f3:00:ef:00:f2:00:f2:00:f2:00:f3:00:f8:00:fc:00:f3:01:13:01:df:02:9e:01:d7:01:15:00:f7:00:fa:00:fd:00:fc:01:07:01:78:02:8c:03:85:02:a7:01:5f:01:03:00:fb";
            string[] strArr = strData.Split(':', '-');
            byte[] data = new byte[strArr.Length];
            for (int i = 0; i < strArr.Length; i++)
            {
                data[i] = Convert.ToByte(strArr[i], 16);
            }
            DataProc(data);
        }

        static void DataProc(byte[] data)
        {
            {
                try
                {
                    List<object> sendDatas = new List<object>();

                    int offset = 0;
                    while (offset < data.Length)
                    {
                        offset += Marshal.SizeOf(typeof(EB200Header));
                        IGenericAttribute ga = new GenericAttributeConventional(data, offset);
                        if (ga.TraceTag > 5000)
                        {
                            //大于5000的都用高级的数据包
                            ga = new GenericAttributeAdvanced(data, offset);
                            offset += Marshal.SizeOf(typeof(GenericAttributeAdvanced));
                        }
                        else
                        {
                            offset += Marshal.SizeOf(typeof(GenericAttributeConventional));
                        }

                        object obj = null;
                        switch (ga.TraceTag)
                        {
                            case (ushort)TAGS.AUDIO:
                                obj = ToAudio(data, offset);
                                break;
                            case (ushort)TAGS.IFPAN:
                                obj = ToSpectrum(data, offset);
                                break;
                            case (ushort)TAGS.IF:
                                obj = ToIQ(data, offset);
                                break;
                            case (ushort)TAGS.CW:
                                obj = ToCW(data, offset);
                                break;
                            case (ushort)TAGS.DFPScan:
                                obj = ToDFPScan(data, offset);
                                break;
                            case (ushort)TAGS.GPSCompass:
                                obj = ToGPSCompass(data, offset);
                                break;
                            case (ushort)TAGS.FSCAN:
                            //    obj = ToFScan(data, offset);
                            //    break;
                            //case (ushort)TAGS.MSCAN:
                            //    obj = ToMScan(data, offset);
                            //    break;
                            //case (ushort)TAGS.PSCAN:
                            //    obj = ToPScan(data, offset);
                            //    break;
                            default:
                                break;
                        }

                        if (obj != null)
                        {
                            if ((obj as List<object>) != null)
                            {
                                sendDatas.AddRange(obj as List<object>);
                            }
                            else
                            {
                                sendDatas.Add(obj);
                            }
                        }
                        offset += (int)ga.DataLength;
                    }
                }
                catch (Exception)
                {

                }
            }
        }
        static object ToGPSCompass(byte[] buffer, int offset)
        {
            try
            {
                TraceAttributeConventional trace = new TraceAttributeConventional(buffer, offset);
                offset += Marshal.SizeOf(typeof(TraceAttributeConventional));
                OptionalHeaderGPSCompass header = new OptionalHeaderGPSCompass(buffer, offset);
                offset += trace.OptionalHeaderLength;

                GPSCompassData gpsData = new GPSCompassData(buffer, offset);
                ulong tick = (ulong)DateTime.Now.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).Ticks * 100;
                DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddTicks((long)tick / 100);

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
        static object ToAudio(byte[] buffer, int offset)
        {
            try
            {
                {
                    TraceAttributeConventional trace = new TraceAttributeConventional(buffer, offset);
                    offset += Marshal.SizeOf(typeof(TraceAttributeConventional));
                    OptionalHeaderAudio header = new OptionalHeaderAudio(buffer, offset);
                    offset += trace.OptionalHeaderLength;

                    //校验数据有效性
                    if (header.FrequencyHigh != 0)
                    {
                        return null;
                    }

                    byte[] audio = new byte[trace.NumberOfTraceItems * 2];
                    Buffer.BlockCopy(buffer, offset, audio, 0, audio.Length);
                    //960
                    return null;
                }

            }
            catch (Exception)
            {
                return null;
            }
        }
        static object ToIQ(byte[] buffer, int offset)
        {
            try
            {
                List<object> datas = new List<object>();
                TraceAttributeConventional trace = new TraceAttributeConventional(buffer, offset);
                offset += Marshal.SizeOf(typeof(TraceAttributeConventional));
                OptionalHeaderIF header = new OptionalHeaderIF(buffer, offset);
                offset += trace.OptionalHeaderLength;

                //校验数据有效性
                if (header.FrequencyHigh != 0)
                {
                    return null;
                }

                short[] iq = new short[trace.NumberOfTraceItems * 2];
                //Buffer.BlockCopy(buffer, offset, iq, 0, trace.NumberOfTraceItems * 4);
                for (int i = 0; i < trace.NumberOfTraceItems; i++)
                {
                    byte[] data = new byte[2];
                    Buffer.BlockCopy(buffer, offset, data, 0, 2);
                    Array.Reverse(data);
                    iq[i * 2] = BitConverter.ToInt16(data, 0);
                    offset += 2;
                    Buffer.BlockCopy(buffer, offset, data, 0, 2);
                    Array.Reverse(data);
                    iq[i * 2 + 1] = BitConverter.ToInt16(data, 0);
                    offset += 2;
                }

                //if (_iqSwitch)
                //{
                //    SDataIQ dataIQ = new SDataIQ();
                //    dataIQ.Frequency = ((header.FrequencyHigh == 0 ? 0 : (((Int64)header.FrequencyHigh) << 32)) + header.FrequencyLow) / 1000000d;
                //    //dataIQ.Frequency = header.FrequencyLow / 1000000d;
                //    dataIQ.IFBandWidth = header.Bandwidth / 1000d;
                //    dataIQ.SampleRate = header.Samplerate / 1000000d;
                //    dataIQ.Attenuation = header.RxAtt;
                //    dataIQ.Datas = iq;
                //    datas.Add(dataIQ);
                //}

                return datas;
            }
            catch (Exception)
            {
                return null;
            }
        }
        static object ToCW(byte[] buffer, int offset)
        {
            try
            {
                List<object> datas = new List<object>();
                TraceAttributeConventional trace = new TraceAttributeConventional(buffer, offset);
                if (trace.NumberOfTraceItems <= 0)
                {
                    return null;
                }

                offset += Marshal.SizeOf(typeof(TraceAttributeConventional));
                OptionalHeaderCW optionalHeaderCW = new OptionalHeaderCW(buffer, offset);
                offset += trace.OptionalHeaderLength;
                CWData cWData = new CWData(trace.NumberOfTraceItems, buffer, offset, trace.SelectorFlags);

                //if (ITUSwitch)
                //{
                //    SDataITU sDataITU = new SDataITU();
                //    sDataITU.AMDepth = cWData.AMDepth[0] < 0 || cWData.AMDepth[0] > 100 ? double.MinValue : cWData.AMDepth[0] / 10f;
                //    sDataITU.DemMode = DemoduMode.CW;
                //    sDataITU.FMDev = cWData.FMDev[0] < -1000000000f ? double.MinValue : cWData.FMDev[0] / 1000f;
                //    sDataITU.FMDevPos = cWData.FMDevPos[0] < -1000000000f ? double.MinValue : cWData.FMDevPos[0] / 1000f;
                //    sDataITU.FMDevNeg = cWData.FMDevNeg[0] < -1000000000f ? double.MinValue : cWData.FMDevNeg[0] / 1000f;
                //    if (cWData.FreqOffset[0] < -1000000000f)
                //        sDataITU.Frequency = _frequency;
                //    else
                //        sDataITU.Frequency = _frequency + cWData.FMDev[0] / 1000000f;
                //    sDataITU.PMDepth = cWData.PMDepth[0] < -1000000000f ? double.MinValue : cWData.PMDepth[0] / 100f;

                //    if (_bandMeasureMode == "XDB")
                //    {
                //        sDataITU.XdBBW = cWData.BandWidth[0] < -1000000000f ? double.MinValue : cWData.BandWidth[0] / 1000f;
                //        sDataITU.BetaBW = double.MinValue;
                //    }
                //    else
                //    {
                //        sDataITU.BetaBW = cWData.BandWidth[0] < -1000000000f ? double.MinValue : cWData.BandWidth[0] / 1000f;
                //        sDataITU.XdBBW = double.MinValue;
                //    }

                //    datas.Add(sDataITU);
                //}

                // 计算电平值
                //SDataLevel sDataLevel = new SDataLevel();
                //sDataLevel.Frequency = _frequency;
                //sDataLevel.IFBandWidth = _ifBandWidth;
                //sDataLevel.Data = cWData.Level[0] / 10;
                //datas.Add(sDataLevel);
                return datas;
            }
            catch (Exception)
            {
                return null;
            }
        }
        static object ToSpectrum(byte[] buffer, int offset)
        {
            try
            {
                TraceAttributeConventional trace = new TraceAttributeConventional(buffer, offset);
                offset += Marshal.SizeOf(typeof(TraceAttributeConventional));
                OptionalHeaderIFPan header = new OptionalHeaderIFPan(buffer, offset);
                offset += trace.OptionalHeaderLength;
                DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                DateTime dt1 = dt.AddMilliseconds((double)header.OutputTimestamp / 1000 / 1000);
                //校验数据有效性
                if (header.FrequencyHigh != 0)
                {
                    return null;
                }

                float[] spectrum = new float[trace.NumberOfTraceItems];
                for (int i = 0; i < spectrum.Length; ++i)
                {
                    Array.Reverse(buffer, offset, 2);
                    spectrum[i] = BitConverter.ToInt16(buffer, offset) / 10f;
                    offset += 2;
                }

                List<object> datas = new List<object>();
                return datas;
            }
            catch (Exception)
            {
                return null;
            }
        }
        static object ToDFPScan(byte[] buffer, int offset)
        {
            try
            {
                TraceAttributeAdvanced pCommon = new TraceAttributeAdvanced(buffer, offset);
                offset += Marshal.SizeOf(typeof(TraceAttributeAdvanced));
                OptionalHeaderDFPScan opt = new OptionalHeaderDFPScan(buffer, offset);
                offset += (int)pCommon.OptionalHeaderLength;
                ulong selectorFlags = pCommon.SelectorFlagsLow + pCommon.SelectorFlagsHigh << 32;
                float[] pLevel = new float[pCommon.NumberOfTraceItems];
                float[] pAzimuth = new float[pCommon.NumberOfTraceItems];
                float[] pQuality = new float[pCommon.NumberOfTraceItems];
                DFPScanData dFPScanData = new DFPScanData(pCommon.NumberOfTraceItems, buffer, offset, selectorFlags);

                long freq = (long)opt.Frequency;
                int freqIndex = (int)((98.7 * 1000000 - freq) / (opt.FrequencyStepNumerator / opt.FrequencyStepDenominator));
                double bandwidth = opt.Bandwidth / 1000.0;
                if (freqIndex > pCommon.NumberOfTraceItems)
                {
                    freqIndex = 0;
                }

                float level = pLevel[freqIndex];
                float azimuth = pAzimuth[freqIndex];
                float quality = pQuality[freqIndex];

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct OptionalHeaderGPSCompass
        {
            [MarshalAs(UnmanagedType.U8)]
            public UInt64 OutputTimestamp;

            public OptionalHeaderGPSCompass(byte[] value, int startIndex)
            {
                Array.Reverse(value, startIndex, 8);
                OutputTimestamp = BitConverter.ToUInt64(value, startIndex);
            }
        }
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        internal struct GPSCompassData
        {
            public ushort CompassHeading;
            public short CompassHeadingType;
            public short GPSValid;
            public short NoOfSatInView;
            public string LatRef;
            public short LatDeg;
            public float LatMin;
            public string LonRef;
            public short LonDeg;
            public float LonMin;
            public float Dilution;
            public short AntValid;
            public short AntTiltOver;
            public short AntElevation;
            public short AntRoll;
            public short SignalSource;
            public short AngularRatesValid;
            public short HeadingAngularRate;
            public short ElevationAngularRate;
            public short RollAngularRate;
            public short GeoidalSeparationValid;
            public int GeoidalSeparation;
            public int Altitude;
            public short SpeedOverGround;
            public short TrackMadeGood;
            public float PDOP;
            public float VDOP;
            public ulong GPSTimestamp;
            public int Reserved;
            public ulong CompassTimestamp;
            public short MagneticDeclinationSource;
            public short MagneticDeclination;
            public short AntRollExact;
            public short AntElevationExact;


            public GPSCompassData(byte[] buffer, int startIndex)
            {
                int offset = startIndex;
                Array.Reverse(buffer, offset, 2);
                CompassHeading = BitConverter.ToUInt16(buffer, offset);
                offset += 2;
                Array.Reverse(buffer, offset, 2);
                CompassHeadingType = BitConverter.ToInt16(buffer, offset);
                offset += 2;
                Array.Reverse(buffer, offset, 2);
                GPSValid = BitConverter.ToInt16(buffer, offset);
                offset += 2;
                Array.Reverse(buffer, offset, 2);
                NoOfSatInView = BitConverter.ToInt16(buffer, offset);
                offset += 2;
                //Array.Reverse(buffer, offset, 2);
                LatRef = Encoding.ASCII.GetString(buffer, offset, 2).Trim('\0');//BitConverter.ToInt16(buffer, offset);
                offset += 2;
                Array.Reverse(buffer, offset, 2);
                LatDeg = BitConverter.ToInt16(buffer, offset);
                offset += 2;
                Array.Reverse(buffer, offset, 4);
                LatMin = BitConverter.ToSingle(buffer, offset);
                offset += 4;
                //Array.Reverse(buffer, offset, 2);
                LonRef = Encoding.ASCII.GetString(buffer, offset, 2).Trim('\0'); //BitConverter.ToInt16(buffer, offset);
                offset += 2;
                Array.Reverse(buffer, offset, 2);
                LonDeg = BitConverter.ToInt16(buffer, offset);
                offset += 2;
                Array.Reverse(buffer, offset, 4);
                LonMin = BitConverter.ToSingle(buffer, offset);
                offset += 4;
                Array.Reverse(buffer, offset, 4);
                Dilution = BitConverter.ToSingle(buffer, offset);
                offset += 4;
                Array.Reverse(buffer, offset, 2);
                AntValid = BitConverter.ToInt16(buffer, offset);
                offset += 2;
                Array.Reverse(buffer, offset, 2);
                AntTiltOver = BitConverter.ToInt16(buffer, offset);
                offset += 2;
                Array.Reverse(buffer, offset, 2);
                AntElevation = BitConverter.ToInt16(buffer, offset);
                offset += 2;
                Array.Reverse(buffer, offset, 2);
                AntRoll = BitConverter.ToInt16(buffer, offset);
                offset += 2;
                Array.Reverse(buffer, offset, 2);
                SignalSource = BitConverter.ToInt16(buffer, offset);
                offset += 2;
                Array.Reverse(buffer, offset, 2);
                AngularRatesValid = BitConverter.ToInt16(buffer, offset);
                offset += 2;
                Array.Reverse(buffer, offset, 2);
                HeadingAngularRate = BitConverter.ToInt16(buffer, offset);
                offset += 2;
                Array.Reverse(buffer, offset, 2);
                ElevationAngularRate = BitConverter.ToInt16(buffer, offset);
                offset += 2;
                Array.Reverse(buffer, offset, 2);
                RollAngularRate = BitConverter.ToInt16(buffer, offset);
                offset += 2;
                Array.Reverse(buffer, offset, 2);
                GeoidalSeparationValid = BitConverter.ToInt16(buffer, offset);
                offset += 2;
                Array.Reverse(buffer, offset, 4);
                GeoidalSeparation = BitConverter.ToInt32(buffer, offset);
                offset += 4;
                Array.Reverse(buffer, offset, 4);
                Altitude = BitConverter.ToInt32(buffer, offset);
                offset += 4;
                Array.Reverse(buffer, offset, 2);
                SpeedOverGround = BitConverter.ToInt16(buffer, offset);
                offset += 2;
                Array.Reverse(buffer, offset, 2);
                TrackMadeGood = BitConverter.ToInt16(buffer, offset);
                offset += 2;
                Array.Reverse(buffer, offset, 4);
                PDOP = BitConverter.ToSingle(buffer, offset);
                offset += 4;
                Array.Reverse(buffer, offset, 4);
                VDOP = BitConverter.ToSingle(buffer, offset);
                offset += 4;
                Array.Reverse(buffer, offset, 8);
                GPSTimestamp = BitConverter.ToUInt64(buffer, offset);
                offset += 8;
                Array.Reverse(buffer, offset, 4);
                Reserved = BitConverter.ToInt32(buffer, offset);
                offset += 4;
                Array.Reverse(buffer, offset, 8);
                CompassTimestamp = BitConverter.ToUInt64(buffer, offset);
                offset += 8;
                Array.Reverse(buffer, offset, 2);
                MagneticDeclinationSource = BitConverter.ToInt16(buffer, offset);
                offset += 2;
                Array.Reverse(buffer, offset, 2);
                MagneticDeclination = BitConverter.ToInt16(buffer, offset);
                offset += 2;
                Array.Reverse(buffer, offset, 2);
                AntRollExact = BitConverter.ToInt16(buffer, offset);
                offset += 2;
                Array.Reverse(buffer, offset, 2);
                AntElevationExact = BitConverter.ToInt16(buffer, offset);
                offset += 2;
            }
        }
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct OptionalHeaderAudio
        {
            [MarshalAs(UnmanagedType.I2)]
            public short AudioMode;
            [MarshalAs(UnmanagedType.I2)]
            public short FrameLen;
            [MarshalAs(UnmanagedType.U4)]
            public uint FrequencyLow;
            [MarshalAs(UnmanagedType.U4)]
            public uint Bandwidth;
            [MarshalAs(UnmanagedType.U2)]
            public ushort Demodulation;//FM:0, AM:1, PULS:2, PM:3, IQ:4, ISB:5, CW:6, USB:7, LSB:8, TV:9
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
            public string sDemodulation;
            [MarshalAs(UnmanagedType.U4)]
            public uint FrequencyHigh;
            [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 6)]
            public byte[] reserved;
            [MarshalAs(UnmanagedType.U8)]
            public ulong OutputTimestamp;
            [MarshalAs(UnmanagedType.I2)]
            public short SignalSource;

            public OptionalHeaderAudio(byte[] value, int startIndex)
            {
                if (BitConverter.IsLittleEndian)
                {
                    byte[] data = new byte[2];
                    Buffer.BlockCopy(value, startIndex, data, 0, 2);
                    Array.Reverse(data);
                    AudioMode = BitConverter.ToInt16(data, 0);
                    Buffer.BlockCopy(value, startIndex + 2, data, 0, 2);
                    Array.Reverse(data);
                    FrameLen = BitConverter.ToInt16(data, 0);
                    data = new byte[4];
                    Buffer.BlockCopy(value, startIndex + 4, data, 0, 4);
                    Array.Reverse(data);
                    FrequencyLow = BitConverter.ToUInt32(data, 0);
                    Buffer.BlockCopy(value, startIndex + 8, data, 0, 4);
                    Array.Reverse(data);
                    Bandwidth = BitConverter.ToUInt32(data, 0);
                    data = new byte[2];
                    Buffer.BlockCopy(value, startIndex + 12, data, 0, 2);
                    Array.Reverse(data);
                    Demodulation = BitConverter.ToUInt16(data, 0);

                    sDemodulation = BitConverter.ToString(value, startIndex + 14, 8);
                    data = new byte[4];
                    Buffer.BlockCopy(value, startIndex + 22, data, 0, 4);
                    Array.Reverse(data);
                    FrequencyHigh = BitConverter.ToUInt32(data, 0);
                    reserved = new byte[6];
                    data = new byte[8];
                    Buffer.BlockCopy(value, startIndex + 32, data, 0, 8);
                    Array.Reverse(data);
                    OutputTimestamp = BitConverter.ToUInt64(data, 0);
                    data = new byte[2];
                    Buffer.BlockCopy(value, startIndex + 40, data, 0, 2);
                    Array.Reverse(data);
                    SignalSource = BitConverter.ToInt16(data, 0);
                }
                else
                {
                    AudioMode = BitConverter.ToInt16(value, startIndex);
                    FrameLen = BitConverter.ToInt16(value, startIndex + 2);
                    FrequencyLow = BitConverter.ToUInt32(value, startIndex + 4);
                    Bandwidth = BitConverter.ToUInt32(value, startIndex + 8);
                    Demodulation = BitConverter.ToUInt16(value, startIndex + 12);
                    sDemodulation = BitConverter.ToString(value, startIndex + 14, 8);
                    FrequencyHigh = BitConverter.ToUInt32(value, startIndex + 22);
                    reserved = new byte[6];
                    OutputTimestamp = BitConverter.ToUInt64(value, startIndex + 32);
                    SignalSource = BitConverter.ToInt16(value, startIndex + 40);
                }
            }
        }
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct OptionalHeaderIF
        {
            //SYSTem:IF:REMote:MODE OFF|SHORT|LONG
            [MarshalAs(UnmanagedType.I2)]
            public short Mode;
            [MarshalAs(UnmanagedType.I2)]
            public short FrameLen;
            [MarshalAs(UnmanagedType.U4)]
            public uint Samplerate;
            [MarshalAs(UnmanagedType.U4)]
            public uint FrequencyLow;
            [MarshalAs(UnmanagedType.U4)]
            public uint Bandwidth;//IF bandwidth
            [MarshalAs(UnmanagedType.U2)]
            public ushort Demodulation;
            [MarshalAs(UnmanagedType.I2)]
            public short RxAtt;
            [MarshalAs(UnmanagedType.U2)]
            public ushort Flags;
            [MarshalAs(UnmanagedType.I2)]
            public short KFactor;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
            public string sDemodulation;
            [MarshalAs(UnmanagedType.U8)]
            public ulong SampleCount;
            [MarshalAs(UnmanagedType.U4)]
            public uint FrequencyHigh;
            [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 4)]
            public byte[] reserved;
            [MarshalAs(UnmanagedType.U8)]
            public ulong StartTimestamp;
            [MarshalAs(UnmanagedType.I2)]
            public short SignalSource;

            public OptionalHeaderIF(byte[] value, int startIndex)
            {
                if (BitConverter.IsLittleEndian)
                {
                    byte[] data = new byte[2];
                    Buffer.BlockCopy(value, startIndex, data, 0, 2);
                    Array.Reverse(data);
                    Mode = BitConverter.ToInt16(data, 0);
                    Buffer.BlockCopy(value, startIndex + 2, data, 0, 2);
                    Array.Reverse(data);
                    FrameLen = BitConverter.ToInt16(data, 0);
                    data = new byte[4];
                    Buffer.BlockCopy(value, startIndex + 4, data, 0, 4);
                    Array.Reverse(data);
                    Samplerate = BitConverter.ToUInt32(data, 0);
                    Buffer.BlockCopy(value, startIndex + 8, data, 0, 4);
                    Array.Reverse(data);
                    FrequencyLow = BitConverter.ToUInt32(data, 0);
                    Buffer.BlockCopy(value, startIndex + 12, data, 0, 4);
                    Array.Reverse(data);
                    Bandwidth = BitConverter.ToUInt32(data, 0);
                    data = new byte[2];
                    Buffer.BlockCopy(value, startIndex + 16, data, 0, 2);
                    Array.Reverse(data);
                    Demodulation = BitConverter.ToUInt16(data, 0);
                    Buffer.BlockCopy(value, startIndex + 18, data, 0, 2);
                    Array.Reverse(data);
                    RxAtt = BitConverter.ToInt16(data, 0);
                    Buffer.BlockCopy(value, startIndex + 20, data, 0, 2);
                    Array.Reverse(data);
                    Flags = BitConverter.ToUInt16(data, 0);
                    Buffer.BlockCopy(value, startIndex + 22, data, 0, 2);
                    Array.Reverse(data);
                    KFactor = BitConverter.ToInt16(data, 0);
                    sDemodulation = BitConverter.ToString(value, startIndex + 24, 8);
                    data = new byte[8];
                    Buffer.BlockCopy(value, startIndex + 32, data, 0, 8);
                    Array.Reverse(data);
                    SampleCount = BitConverter.ToUInt64(data, 0);
                    data = new byte[4];
                    Buffer.BlockCopy(value, startIndex + 40, data, 0, 4);
                    Array.Reverse(data);
                    FrequencyHigh = BitConverter.ToUInt32(data, 0);
                    reserved = new byte[4];
                    data = new byte[8];
                    Buffer.BlockCopy(value, startIndex + 48, data, 0, 8);
                    Array.Reverse(data);
                    StartTimestamp = BitConverter.ToUInt64(data, 0);
                    data = new byte[2];
                    Buffer.BlockCopy(value, startIndex + 56, data, 0, 2);
                    Array.Reverse(data);
                    SignalSource = BitConverter.ToInt16(data, 0);
                }
                else
                {
                    Mode = BitConverter.ToInt16(value, startIndex);
                    FrameLen = BitConverter.ToInt16(value, startIndex + 2);
                    Samplerate = BitConverter.ToUInt32(value, startIndex + 4);
                    FrequencyLow = BitConverter.ToUInt32(value, startIndex + 8);
                    Bandwidth = BitConverter.ToUInt32(value, startIndex + 12);
                    Demodulation = BitConverter.ToUInt16(value, startIndex + 16);
                    RxAtt = BitConverter.ToInt16(value, startIndex + 18);
                    Flags = BitConverter.ToUInt16(value, startIndex + 20);
                    KFactor = BitConverter.ToInt16(value, startIndex + 22);
                    sDemodulation = BitConverter.ToString(value, startIndex + 24, 8);
                    SampleCount = BitConverter.ToUInt64(value, startIndex + 32);
                    FrequencyHigh = BitConverter.ToUInt32(value, startIndex + 40);
                    reserved = new byte[4];
                    StartTimestamp = BitConverter.ToUInt64(value, startIndex + 48);
                    SignalSource = BitConverter.ToInt16(value, startIndex + 56);
                }
            }
        }
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        internal struct OptionalHeaderCW
        {
            //SYSTem:IF:REMote:MODE OFF|SHORT|LONG
            [MarshalAs(UnmanagedType.U4)]
            public UInt32 Freq_Low;
            [MarshalAs(UnmanagedType.U4)]
            public UInt32 Freq_High;
            [MarshalAs(UnmanagedType.U8)]
            public UInt64 OutputTimestamp;
            [MarshalAs(UnmanagedType.I2)]
            public Int16 SignalSource;

            public OptionalHeaderCW(byte[] value, int startIndex)
            {
                if (BitConverter.IsLittleEndian)
                {
                    byte[] data = new byte[4];
                    Buffer.BlockCopy(value, startIndex, data, 0, 4);
                    Array.Reverse(data);
                    Freq_Low = BitConverter.ToUInt32(data, 0);
                    Buffer.BlockCopy(value, startIndex + 4, data, 0, 4);
                    Array.Reverse(data);
                    Freq_High = BitConverter.ToUInt32(data, 0);
                    data = new byte[8];
                    Buffer.BlockCopy(value, startIndex + 8, data, 0, 8);
                    Array.Reverse(data);
                    OutputTimestamp = BitConverter.ToUInt64(data, 0);
                    data = new byte[2];
                    Buffer.BlockCopy(value, startIndex + 16, data, 0, 2);
                    Array.Reverse(data);
                    SignalSource = BitConverter.ToInt16(data, 0);
                }
                else
                {
                    Freq_Low = BitConverter.ToUInt32(value, startIndex);
                    Freq_High = BitConverter.ToUInt32(value, startIndex + 4);
                    OutputTimestamp = BitConverter.ToUInt64(value, startIndex + 8);
                    SignalSource = BitConverter.ToInt16(value, startIndex + 16);
                }
            }
        }
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        internal struct CWData
        {
            public short[] Level;
            public int[] FreqOffset;
            public short[] FStrength;
            public short[] AMDepth;
            public short[] AMDepthPos;
            public short[] AMDepthNeg;
            public int[] FMDev;
            public int[] FMDevPos;
            public int[] FMDevNeg;
            public short[] PMDepth;
            public int[] BandWidth;

            public CWData(int dataCnt, byte[] buffer, int startIndex, uint selectorFlags)
            {
                Level = new short[dataCnt];
                FreqOffset = new int[dataCnt];
                FStrength = new short[dataCnt];
                AMDepth = new short[dataCnt];
                AMDepthPos = new short[dataCnt];
                AMDepthNeg = new short[dataCnt];
                FMDev = new int[dataCnt];
                FMDevPos = new int[dataCnt];
                FMDevNeg = new int[dataCnt];
                PMDepth = new short[dataCnt];
                BandWidth = new int[dataCnt];
                int offset = startIndex;
                for (int i = 0; i < dataCnt; i++)
                {
                    Array.Reverse(buffer, offset, 2);
                    Level[i] = BitConverter.ToInt16(buffer, offset);
                    if (Level[i] == 2000)
                    {
                        Level[i] = short.MinValue;
                    }

                    offset += 2;
                }
                for (int i = 0; i < dataCnt; i++)
                {
                    Array.Reverse(buffer, offset, 4);
                    FreqOffset[i] = BitConverter.ToInt32(buffer, offset);
                    if (FreqOffset[i] == 10000000)
                    {
                        FreqOffset[i] = int.MinValue;
                    }

                    offset += 4;
                }

                if ((selectorFlags & (uint)FLAGS.FSTRENGTH) > 0)
                {
                    for (int i = 0; i < dataCnt; i++)
                    {
                        Array.Reverse(buffer, offset, 2);
                        FStrength[i] = BitConverter.ToInt16(buffer, offset);
                        if (FStrength[i] == 0x7FFF)
                        {
                            FStrength[i] = short.MinValue;
                        }

                        offset += 2;
                    }
                }
                if ((selectorFlags & (uint)FLAGS.AM) > 0)
                {
                    for (int i = 0; i < dataCnt; i++)
                    {
                        Array.Reverse(buffer, offset, 2);
                        AMDepth[i] = BitConverter.ToInt16(buffer, offset);
                        if (AMDepth[i] == 0x7FFF)
                        {
                            AMDepth[i] = short.MinValue;
                        }

                        offset += 2;
                    }
                }
                if ((selectorFlags & (uint)FLAGS.AM_POS) > 0)
                {
                    for (int i = 0; i < dataCnt; i++)
                    {
                        Array.Reverse(buffer, offset, 2);
                        AMDepthPos[i] = BitConverter.ToInt16(buffer, offset);
                        offset += 2;
                    }
                }
                if ((selectorFlags & (uint)FLAGS.AM_NEG) > 0)
                {
                    for (int i = 0; i < dataCnt; i++)
                    {
                        Array.Reverse(buffer, offset, 2);
                        AMDepthNeg[i] = BitConverter.ToInt16(buffer, offset);
                        offset += 2;
                    }
                }
                if ((selectorFlags & (uint)FLAGS.FM) > 0)
                {
                    for (int i = 0; i < dataCnt; i++)
                    {
                        Array.Reverse(buffer, offset, 4);
                        FMDev[i] = BitConverter.ToInt32(buffer, offset);
                        if (FMDev[i] == 0x7FFFFFFF)
                        {
                            FMDev[i] = int.MinValue;
                        }

                        offset += 4;
                    }
                }
                if ((selectorFlags & (uint)FLAGS.FM_POS) > 0)
                {
                    for (int i = 0; i < dataCnt; i++)
                    {
                        Array.Reverse(buffer, offset, 4);
                        FMDevPos[i] = BitConverter.ToInt32(buffer, offset);
                        offset += 4;
                    }
                }
                if ((selectorFlags & (uint)FLAGS.FM_NEG) > 0)
                {
                    for (int i = 0; i < dataCnt; i++)
                    {
                        Array.Reverse(buffer, offset, 4);
                        FMDevNeg[i] = BitConverter.ToInt32(buffer, offset);
                        offset += 4;
                    }
                }
                if ((selectorFlags & (uint)FLAGS.PM) > 0)
                {
                    for (int i = 0; i < dataCnt; i++)
                    {
                        Array.Reverse(buffer, offset, 2);
                        PMDepth[i] = BitConverter.ToInt16(buffer, offset);
                        if (PMDepth[i] == 0x7FFF)
                        {
                            PMDepth[i] = short.MinValue;
                        }

                        offset += 2;
                    }
                }
                if ((selectorFlags & (uint)FLAGS.BANDWIDTH) > 0)
                {
                    for (int i = 0; i < dataCnt; i++)
                    {
                        Array.Reverse(buffer, offset, 4);
                        BandWidth[i] = BitConverter.ToInt32(buffer, offset);
                        if (BandWidth[i] == 0x7FFFFFFF)
                        {
                            BandWidth[i] = int.MinValue;
                        }

                        offset += 4;
                    }
                }
            }
        }
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        internal struct DFPScanData
        {
            public uint DataCnt;
            public short[] DfLevel;
            public short[] Azimuth;
            public short[] DfQuality;
            public short[] DfFstrength;
            public short[] DfLevelCont;
            public short[] Elevation;
            public short[] DfChannelStatus;
            public short[] DfOmniphase;

            public float[] LevelF;
            public float[] AzimuthF;
            public float[] QualityF;

            public DFPScanData(uint dataCnt, byte[] buffer, int startIndex, ulong selectorFlags)
            {
                DataCnt = dataCnt;
                DfLevel = new short[dataCnt];
                Azimuth = new short[dataCnt];
                DfQuality = new short[dataCnt];
                DfFstrength = new short[dataCnt];
                DfLevelCont = new short[dataCnt];
                Elevation = new short[dataCnt];
                DfChannelStatus = new short[dataCnt];
                DfOmniphase = new short[dataCnt];

                LevelF = new float[dataCnt];
                AzimuthF = new float[dataCnt];
                QualityF = new float[dataCnt];
                int offset = startIndex;
                if ((selectorFlags & (uint)FLAGS.DF_LEVEL) > 0)
                {
                    for (int i = 0; i < dataCnt; i++)
                    {
                        Array.Reverse(buffer, offset, 2);
                        DfLevel[i] = BitConverter.ToInt16(buffer, offset);
                        LevelF[i] = ((float)DfLevel[i]) / 10;
                        if (DfLevel[i] == 2000 || DfLevel[i] == 1999)
                        {
                            DfLevel[i] = short.MinValue;
                            LevelF[i] = float.MinValue;
                        }
                        offset += 2;
                    }
                }
                if ((selectorFlags & (uint)FLAGS.AZIMUTH) > 0)
                {
                    for (int i = 0; i < dataCnt; i++)
                    {
                        Array.Reverse(buffer, offset, 2);
                        Azimuth[i] = BitConverter.ToInt16(buffer, offset);
                        AzimuthF[i] = ((float)Azimuth[i]) / 10;
                        if (Azimuth[i] == 0x7FFF || Azimuth[i] == 0x7FFE)
                        {
                            Azimuth[i] = short.MinValue;
                            AzimuthF[i] = 0;
                        }
                        offset += 2;
                    }
                }

                if ((selectorFlags & (uint)FLAGS.DF_QUALITY) > 0)
                {
                    for (int i = 0; i < dataCnt; i++)
                    {
                        Array.Reverse(buffer, offset, 2);
                        DfQuality[i] = BitConverter.ToInt16(buffer, offset);
                        QualityF[i] = ((float)DfQuality[i]) / 10;
                        if (DfQuality[i] == 0x7FFF || DfQuality[i] == 0x7FFE)
                        {
                            DfQuality[i] = short.MinValue;
                            QualityF[i] = 0;
                        }
                        offset += 2;
                    }
                }
                if ((selectorFlags & (uint)FLAGS.DF_FSTRENGTH) > 0)
                {
                    for (int i = 0; i < dataCnt; i++)
                    {
                        Array.Reverse(buffer, offset, 2);
                        DfFstrength[i] = BitConverter.ToInt16(buffer, offset);
                        if (DfFstrength[i] == 0x7FFF || DfFstrength[i] == 0x7FFE)
                        {
                            DfFstrength[i] = short.MinValue;
                        }

                        offset += 2;
                    }
                }
                if ((selectorFlags & (uint)FLAGS.DF_LEVEL_CONT) > 0)
                {
                    for (int i = 0; i < dataCnt; i++)
                    {
                        Array.Reverse(buffer, offset, 2);
                        DfLevelCont[i] = BitConverter.ToInt16(buffer, offset);
                        offset += 2;
                    }
                }
                if ((selectorFlags & (uint)FLAGS.ELEVATION) > 0)
                {
                    for (int i = 0; i < dataCnt; i++)
                    {
                        Array.Reverse(buffer, offset, 2);
                        Elevation[i] = BitConverter.ToInt16(buffer, offset);
                        if (Elevation[i] == 0x7FFF || Elevation[i] == 0x7FFE)
                        {
                            Elevation[i] = short.MinValue;
                        }

                        offset += 2;
                    }
                }
                if ((selectorFlags & (uint)FLAGS.DF_CHANNEL_STATUS) > 0)
                {
                    for (int i = 0; i < dataCnt; i++)
                    {
                        Array.Reverse(buffer, offset, 2);
                        DfChannelStatus[i] = BitConverter.ToInt16(buffer, offset);
                        offset += 2;
                    }
                }
                if ((selectorFlags & (uint)FLAGS.DF_OMNIPHASE) > 0)
                {
                    for (int i = 0; i < dataCnt; i++)
                    {
                        Array.Reverse(buffer, offset, 2);
                        DfOmniphase[i] = BitConverter.ToInt16(buffer, offset);
                        if (DfOmniphase[i] == 0x7FFF || DfOmniphase[i] == 0x7FFE)
                        {
                            DfOmniphase[i] = short.MinValue;
                        }

                        offset += 2;
                    }
                }
            }
        }
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct OptionalHeaderDFPScan
        {
            [MarshalAs(UnmanagedType.I4)]
            public Int32 ScanRangeID;
            [MarshalAs(UnmanagedType.I4)]
            public Int32 ChannelsInScanRange;
            [MarshalAs(UnmanagedType.U8)]
            public UInt64 Frequency;
            [MarshalAs(UnmanagedType.I4)]
            public Int32 LogChannel;
            [MarshalAs(UnmanagedType.I4)]
            public Int32 FrequencyStepNumerator;
            [MarshalAs(UnmanagedType.I4)]
            public Int32 FrequencyStepDenominator;
            [MarshalAs(UnmanagedType.I4)]
            public Int32 Span;
            [MarshalAs(UnmanagedType.R4)]
            public float Bandwidth;
            [MarshalAs(UnmanagedType.I4)]
            public Int32 MeasureTime;
            [MarshalAs(UnmanagedType.I2)]
            public Int16 MeasureCount;
            [MarshalAs(UnmanagedType.I2)]
            public Int16 Threshold;
            [MarshalAs(UnmanagedType.I2)]
            public Int16 CompassHeading;
            [MarshalAs(UnmanagedType.I2)]
            public Int16 CompassHeadingType;
            [MarshalAs(UnmanagedType.I4)]
            public Int32 DFStatus;
            [MarshalAs(UnmanagedType.U8)]
            public UInt64 SweepTime;
            [MarshalAs(UnmanagedType.U8)]
            public UInt64 MeasureTimestamp;
            [MarshalAs(UnmanagedType.U2)]
            public UInt16 JobID;
            [MarshalAs(UnmanagedType.I2)]
            public Int16 SRSelectorflags;
            [MarshalAs(UnmanagedType.U1)]
            public Byte SRWaveCount;
            [MarshalAs(UnmanagedType.U1)]
            public Byte NumberOfEigenvalues;
            [MarshalAs(UnmanagedType.I2)]
            public Int16 Reserved;

            public OptionalHeaderDFPScan(byte[] value, int startIndex)
            {
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(value, startIndex, 4);
                    ScanRangeID = BitConverter.ToInt32(value, startIndex);
                    Array.Reverse(value, startIndex + 4, 4);
                    ChannelsInScanRange = BitConverter.ToInt32(value, startIndex + 4);
                    Array.Reverse(value, startIndex + 8, 8);
                    Frequency = BitConverter.ToUInt64(value, startIndex + 8);
                    Array.Reverse(value, startIndex + 16, 4);
                    LogChannel = BitConverter.ToInt32(value, startIndex + 16);
                    Array.Reverse(value, startIndex + 20, 4);
                    FrequencyStepNumerator = BitConverter.ToInt32(value, startIndex + 20);
                    Array.Reverse(value, startIndex + 24, 4);
                    FrequencyStepDenominator = BitConverter.ToInt32(value, startIndex + 24);
                    Array.Reverse(value, startIndex + 28, 4);
                    Span = BitConverter.ToInt32(value, startIndex + 28);
                    Array.Reverse(value, startIndex + 32, 4);
                    Bandwidth = BitConverter.ToSingle(value, startIndex + 32);
                    Array.Reverse(value, startIndex + 36, 4);
                    MeasureTime = BitConverter.ToInt32(value, startIndex + 36);
                    Array.Reverse(value, startIndex + 40, 2);
                    MeasureCount = BitConverter.ToInt16(value, startIndex + 40);
                    Array.Reverse(value, startIndex + 42, 2);
                    Threshold = BitConverter.ToInt16(value, startIndex + 42);
                    Array.Reverse(value, startIndex + 44, 2);
                    CompassHeading = BitConverter.ToInt16(value, startIndex + 44);
                    Array.Reverse(value, startIndex + 46, 2);
                    CompassHeadingType = BitConverter.ToInt16(value, startIndex + 46);
                    Array.Reverse(value, startIndex + 48, 4);
                    DFStatus = BitConverter.ToInt32(value, startIndex + 48);
                    Array.Reverse(value, startIndex + 52, 8);
                    SweepTime = BitConverter.ToUInt64(value, startIndex + 52);
                    Array.Reverse(value, startIndex + 60, 8);
                    MeasureTimestamp = BitConverter.ToUInt64(value, startIndex + 60);
                    Array.Reverse(value, startIndex + 68, 2);
                    JobID = BitConverter.ToUInt16(value, startIndex + 68);
                    Array.Reverse(value, startIndex + 70, 2);
                    SRSelectorflags = BitConverter.ToInt16(value, startIndex + 70);
                    SRWaveCount = value[startIndex + 72];
                    NumberOfEigenvalues = value[startIndex + 73];
                    Array.Reverse(value, startIndex + 74, 2);
                    Reserved = BitConverter.ToInt16(value, startIndex + 74);
                }
                else
                {
                    ScanRangeID = BitConverter.ToInt32(value, startIndex);
                    ChannelsInScanRange = BitConverter.ToInt32(value, startIndex + 4);
                    Frequency = BitConverter.ToUInt64(value, startIndex + 8);
                    LogChannel = BitConverter.ToInt32(value, startIndex + 16);
                    FrequencyStepNumerator = BitConverter.ToInt32(value, startIndex + 20);
                    FrequencyStepDenominator = BitConverter.ToInt32(value, startIndex + 24);
                    Span = BitConverter.ToInt32(value, startIndex + 28);
                    Bandwidth = BitConverter.ToSingle(value, startIndex + 32);
                    MeasureTime = BitConverter.ToInt32(value, startIndex + 36);
                    MeasureCount = BitConverter.ToInt16(value, startIndex + 40);
                    Threshold = BitConverter.ToInt16(value, startIndex + 42);
                    CompassHeading = BitConverter.ToInt16(value, startIndex + 44);
                    CompassHeadingType = BitConverter.ToInt16(value, startIndex + 46);
                    DFStatus = BitConverter.ToInt32(value, startIndex + 48);
                    SweepTime = BitConverter.ToUInt64(value, startIndex + 52);
                    MeasureTimestamp = BitConverter.ToUInt64(value, startIndex + 60);
                    JobID = BitConverter.ToUInt16(value, startIndex + 68);
                    SRSelectorflags = BitConverter.ToInt16(value, startIndex + 70);
                    SRWaveCount = value[startIndex + 72];
                    NumberOfEigenvalues = value[startIndex + 73];
                    Reserved = BitConverter.ToInt16(value, startIndex + 74);
                }
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TraceAttributeConventional
        {
            [MarshalAs(UnmanagedType.I2)]
            public short NumberOfTraceItems;
            [MarshalAs(UnmanagedType.U1)]
            public byte ChannelNumber;
            [MarshalAs(UnmanagedType.U1)]
            public byte OptionalHeaderLength;
            [MarshalAs(UnmanagedType.U4)]
            public uint SelectorFlags;

            public TraceAttributeConventional(byte[] value, int startIndex)
            {
                Array.Reverse(value, startIndex, 2);
                NumberOfTraceItems = BitConverter.ToInt16(value, startIndex);
                ChannelNumber = value[startIndex + 2];
                OptionalHeaderLength = value[startIndex + 3];
                Array.Reverse(value, startIndex + 4, 4);
                SelectorFlags = BitConverter.ToUInt32(value, startIndex + 4);
            }
        }
        /// <summary>
        /// TraceAttribute 高级版本
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TraceAttributeAdvanced
        {
            [MarshalAs(UnmanagedType.U4)]
            public UInt32 NumberOfTraceItems;
            [MarshalAs(UnmanagedType.U4)]
            public UInt32 Reserved1;
            [MarshalAs(UnmanagedType.U4)]
            public UInt32 OptionalHeaderLength;
            [MarshalAs(UnmanagedType.U4)]
            public UInt32 SelectorFlagsLow;
            [MarshalAs(UnmanagedType.U4)]
            public UInt32 SelectorFlagsHigh;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public UInt32[] Reserved2;

            public TraceAttributeAdvanced(byte[] value, int startIndex)
            {
                Array.Reverse(value, startIndex, 4);
                NumberOfTraceItems = BitConverter.ToUInt32(value, startIndex);
                Array.Reverse(value, startIndex + 4, 4);
                Reserved1 = BitConverter.ToUInt32(value, startIndex + 4);
                Array.Reverse(value, startIndex + 8, 4);
                OptionalHeaderLength = BitConverter.ToUInt32(value, startIndex + 8);
                Array.Reverse(value, startIndex + 12, 4);
                SelectorFlagsLow = BitConverter.ToUInt32(value, startIndex + 12);
                Array.Reverse(value, startIndex + 16, 4);
                SelectorFlagsHigh = BitConverter.ToUInt32(value, startIndex + 16);
                Reserved2 = new UInt32[4];
                for (int i = 0; i < 4; i++)
                {
                    Array.Reverse(value, startIndex + 20 + 4 * i, 4);
                    Reserved2[i] = BitConverter.ToUInt32(value, startIndex + 20 + 4 * i);
                }
            }
        }
        [Flags]
        public enum FLAGS : uint
        {
            /// <summary>
            /// 1/10 dBμV
            /// </summary>
            LEVEL = 0x1,
            /// <summary>
            /// Hz
            /// </summary>
            OFFSET = 0x2,
            /// <summary>
            /// 1/10 dBμV/m
            /// </summary>
            FSTRENGTH = 0x4,
            /// <summary>
            /// 1/10 %
            /// </summary>
            AM = 0x8,
            /// <summary>
            /// 1/10 %
            /// </summary>
            AM_POS = 0x10,
            /// <summary>
            /// 1/10 %
            /// </summary>
            AM_NEG = 0x20,
            /// <summary>
            /// Hz
            /// </summary>
            FM = 0x40,
            /// <summary>
            /// Hz
            /// </summary>
            FM_POS = 0x80,
            /// <summary>
            /// Hz
            /// </summary>
            FM_NEG = 0x100,
            /// <summary>
            /// 1/100 rad
            /// </summary>
            PM = 0x200,
            /// <summary>
            /// Hz
            /// </summary>
            BANDWIDTH = 0x400,
            /// <summary>
            /// 1/10 dBμV
            /// </summary>
            DF_LEVEL = 0x800,
            /// <summary>
            /// 1/10 °
            /// </summary>
            AZIMUTH = 0x1000,
            /// <summary>
            /// 1/10 %
            /// </summary>
            DF_QUALITY = 0x2000,
            /// <summary>
            /// 1/10 dBμV/m
            /// </summary>
            DF_FSTRENGTH = 0x4000,
            /// <summary>
            /// 1/10 dBμV
            /// </summary>
            DF_LEVEL_CONT = 0x8000,
            CHANNEL = 0x00010000,
            FREQLOW = 0x00020000,
            /// <summary>
            /// 1/10 °
            /// </summary>
            ELEVATION = 0x00040000,
            DF_CHANNEL_STATUS = 0x80000,
            /// <summary>
            /// 1/10 °
            /// </summary>
            DF_OMNIPHASE = 0x00100000,
            FREQHIGH = 0x00200000,
            BANDWIDTH_CENTER = 0x00400000,
            FREQ_OFFSET_REL = 0x00800000,
            PRIVATE = 0x10000000,
            SWAP = 0x20000000,              // swap ON means: do NOT swap (for little endian machines)
            SIGNAL_GREATER_SQUELCH = 0x40000000,
            OPTIONAL_HEADER = 0x80000000
        };
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct EB200Header
        {
            [MarshalAs(UnmanagedType.U4)]
            public uint MagicNumber;
            [MarshalAs(UnmanagedType.U2)]
            public ushort VersionMinor;
            [MarshalAs(UnmanagedType.U2)]
            public ushort VersionMajor;
            [MarshalAs(UnmanagedType.U2)]
            public ushort SequenceNumberLow;
            [MarshalAs(UnmanagedType.U2)]
            public ushort SequenceNumberHigh;
            [MarshalAs(UnmanagedType.U4)]
            public uint DataSize;

            public EB200Header(byte[] value, int startIndex)
            {
                MagicNumber = BitConverter.ToUInt32(value, startIndex);
                VersionMinor = BitConverter.ToUInt16(value, startIndex + 4);
                VersionMajor = BitConverter.ToUInt16(value, startIndex + 6);
                SequenceNumberLow = BitConverter.ToUInt16(value, startIndex + 8);
                SequenceNumberHigh = BitConverter.ToUInt16(value, startIndex + 10);
                DataSize = BitConverter.ToUInt32(value, startIndex + 12);
            }
        }
        public interface IGenericAttribute
        {
            ushort TraceTag { get; }
            uint DataLength { get; }
        }
        /// <summary>
        /// GenericAttribute Tag<5000
        /// </summary>[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct GenericAttributeConventional : IGenericAttribute
        {
            [MarshalAs(UnmanagedType.U2)]
            public ushort Tag;
            [MarshalAs(UnmanagedType.U2)]
            public ushort Length;

            public GenericAttributeConventional(byte[] value, int startIndex)
            {
                Array.Reverse(value, startIndex, 2);
                Tag = BitConverter.ToUInt16(value, startIndex);
                Array.Reverse(value, startIndex, 2);
                Array.Reverse(value, startIndex + 2, 2);
                Length = BitConverter.ToUInt16(value, startIndex + 2);
                Array.Reverse(value, startIndex + 2, 2);
            }

            public ushort TraceTag
            {
                get
                {
                    return Tag;
                }
            }

            public uint DataLength
            {
                get
                {
                    return Length;
                }
            }
        }

        public struct GenericAttributeAdvanced : IGenericAttribute
        {
            [MarshalAs(UnmanagedType.U2)]
            public ushort Tag;
            [MarshalAs(UnmanagedType.U2)]
            public ushort Reserved1;
            [MarshalAs(UnmanagedType.U4)]
            public uint Length;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public uint[] Reserved2;
            public GenericAttributeAdvanced(byte[] value, int startIndex)
            {
                Reserved2 = new uint[4];
                Array.Reverse(value, startIndex, 2);
                Tag = BitConverter.ToUInt16(value, startIndex);
                Array.Reverse(value, startIndex, 2);
                Array.Reverse(value, startIndex + 2, 2);
                Reserved1 = BitConverter.ToUInt16(value, startIndex + 2);
                Array.Reverse(value, startIndex + 2, 2);
                Array.Reverse(value, startIndex + 4, 4);
                Length = BitConverter.ToUInt32(value, startIndex + 4);
                Array.Reverse(value, startIndex + 4, 4);
                for (int i = 0; i < 4; i++)
                {
                    Array.Reverse(value, startIndex + 8 + 4 * i, 4);
                    Reserved2[i] = BitConverter.ToUInt32(value, startIndex + 8 + 4 * i);
                    Array.Reverse(value, startIndex + 8 + 4 * i, 4);
                }
            }
            public ushort TraceTag
            {
                get
                {
                    return Tag;
                }
            }
            public uint DataLength
            {
                get
                {
                    return Length;
                }
            }
        }
        [Flags]
        public enum TAGS
        {
            FSCAN = 101,
            MSCAN = 201,
            AUDIO = 401,
            IFPAN = 501,
            CW = 801,
            IF = 901,
            VIDEO = 1001,
            VDPAN = 1101,
            PSCAN = 1201,
            SELECall = 1301,
            DFPAN = 1401,
            PIFPAN = 1601,
            GPSCompass = 1801,
            AntLevel = 1901,
            DFPScan = 5301,
            SIGP = 5501,
            HRPAN = 5601,
            LAST_TAG
        };

        /// <summary>
        /// OptionalHeaderIFPan
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct OptionalHeaderIFPan
        {
            [MarshalAs(UnmanagedType.U4)]
            public uint FrequencyLow;
            [MarshalAs(UnmanagedType.U4)]
            public uint SpanFrequency;
            [MarshalAs(UnmanagedType.I2)]
            public short AverageTime;//Not used and always set to 0
            [MarshalAs(UnmanagedType.I2)]
            public short AverageType;
            [MarshalAs(UnmanagedType.U4)]
            public uint MeasureTime;//us
            [MarshalAs(UnmanagedType.U4)]
            public uint FrequencyHigh;
            [MarshalAs(UnmanagedType.I4)]
            public int DemodFreqChannel;
            //[MarshalAs(UnmanagedType.U4)]
            //public uint DemodFreqLow;
            //[MarshalAs(UnmanagedType.U4)]
            //public uint DemodFreqHigh;
            [MarshalAs(UnmanagedType.U8)]
            public ulong DemodFreq;
            [MarshalAs(UnmanagedType.U8)]
            public ulong OutputTimestamp;
            [MarshalAs(UnmanagedType.U4)]
            public uint StepFrequencyNumerator;
            [MarshalAs(UnmanagedType.U4)]
            public uint StepFrequencyDenominator;
            [MarshalAs(UnmanagedType.I2)]
            public short SignalSource;
            [MarshalAs(UnmanagedType.I2)]
            public short MeasureMode;
            [MarshalAs(UnmanagedType.U8)]
            public ulong MeasureTimestamp;

            public OptionalHeaderIFPan(byte[] value, int startIndex)
            {
                if (BitConverter.IsLittleEndian)
                {
                    byte[] data = new byte[4];
                    Buffer.BlockCopy(value, startIndex, data, 0, 4);
                    Array.Reverse(data);
                    FrequencyLow = BitConverter.ToUInt32(data, 0);
                    Buffer.BlockCopy(value, startIndex + 4, data, 0, 4);
                    Array.Reverse(data);
                    SpanFrequency = BitConverter.ToUInt32(data, 0);
                    data = new byte[2];
                    Buffer.BlockCopy(value, startIndex + 8, data, 0, 2);
                    Array.Reverse(data);
                    AverageTime = BitConverter.ToInt16(data, 0);
                    Buffer.BlockCopy(value, startIndex + 10, data, 0, 2);
                    Array.Reverse(data);
                    AverageType = BitConverter.ToInt16(data, 0);
                    data = new byte[4];
                    Buffer.BlockCopy(value, startIndex + 12, data, 0, 4);
                    Array.Reverse(data);
                    MeasureTime = BitConverter.ToUInt32(data, 0);
                    Buffer.BlockCopy(value, startIndex + 16, data, 0, 4);
                    Array.Reverse(data);
                    FrequencyHigh = BitConverter.ToUInt32(data, 0);
                    Buffer.BlockCopy(value, startIndex + 20, data, 0, 4);
                    Array.Reverse(data);
                    DemodFreqChannel = BitConverter.ToInt32(data, 0);

                    Buffer.BlockCopy(value, startIndex + 24, data, 0, 4);
                    Array.Reverse(data);
                    uint low = BitConverter.ToUInt32(data, 0);
                    Buffer.BlockCopy(value, startIndex + 28, data, 0, 4);
                    Array.Reverse(data);
                    uint high = BitConverter.ToUInt32(data, 0);
                    DemodFreq = (high << 32) + low;

                    data = new byte[8];
                    Buffer.BlockCopy(value, startIndex + 32, data, 0, 8);
                    Array.Reverse(data);
                    OutputTimestamp = BitConverter.ToUInt64(data, 0);
                    data = new byte[4];
                    Buffer.BlockCopy(value, startIndex + 40, data, 0, 4);
                    Array.Reverse(data);
                    StepFrequencyNumerator = BitConverter.ToUInt32(data, 0);
                    Buffer.BlockCopy(value, startIndex + 44, data, 0, 4);
                    Array.Reverse(data);
                    StepFrequencyDenominator = BitConverter.ToUInt32(data, 0);
                    data = new byte[2];
                    Buffer.BlockCopy(value, startIndex + 48, data, 0, 2);
                    Array.Reverse(data);
                    SignalSource = BitConverter.ToInt16(data, 0);
                    Buffer.BlockCopy(value, startIndex + 50, data, 0, 2);
                    Array.Reverse(data);
                    MeasureMode = BitConverter.ToInt16(data, 0);
                    data = new byte[8];
                    Buffer.BlockCopy(value, startIndex + 52, data, 0, 8);
                    Array.Reverse(data);
                    MeasureTimestamp = BitConverter.ToUInt64(data, 0);
                }
                else
                {
                    FrequencyLow = BitConverter.ToUInt32(value, startIndex);
                    SpanFrequency = BitConverter.ToUInt32(value, startIndex + 4);
                    AverageTime = BitConverter.ToInt16(value, startIndex + 8);
                    AverageType = BitConverter.ToInt16(value, startIndex + 10);
                    MeasureTime = BitConverter.ToUInt32(value, startIndex + 12);
                    FrequencyHigh = BitConverter.ToUInt32(value, startIndex + 16);
                    DemodFreqChannel = BitConverter.ToInt32(value, startIndex + 20);
                    DemodFreq = BitConverter.ToUInt64(value, startIndex + 24);
                    OutputTimestamp = BitConverter.ToUInt64(value, startIndex + 32);
                    StepFrequencyNumerator = BitConverter.ToUInt32(value, startIndex + 40);
                    StepFrequencyDenominator = BitConverter.ToUInt32(value, startIndex + 44);
                    SignalSource = BitConverter.ToInt16(value, startIndex + 48);
                    MeasureMode = BitConverter.ToInt16(value, startIndex + 50);
                    MeasureTimestamp = BitConverter.ToUInt64(value, startIndex + 52);
                }

            }
        }
        #endregion 测试DDF550

        #region 测试查找字节数组

        public static IEnumerable<long> IndexesOf(byte[] source, int start, byte[] pattern)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (pattern == null)
            {
                throw new ArgumentNullException(nameof(pattern));
            }

            long valueLength = source.LongLength;
            long patternLength = pattern.LongLength;

            if ((valueLength == 0) || (patternLength == 0) || (patternLength > valueLength))
            {
                yield break;
            }

            var badCharacters = new long[256];

            for (var i = 0; i < 256; i++)
            {
                badCharacters[i] = patternLength;
            }

            var lastPatternByte = patternLength - 1;

            for (long i = 0; i < lastPatternByte; i++)
            {
                badCharacters[pattern[i]] = lastPatternByte - i;
            }

            long index = start;

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

        public static long IndexesOf(byte[] source, byte[] pattern)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (pattern == null)
            {
                throw new ArgumentNullException(nameof(pattern));
            }

            long valueLength = source.LongLength;
            long patternLength = pattern.LongLength;

            if ((valueLength == 0) || (patternLength == 0) || (patternLength > valueLength))
            {
                return -1;
            }

            var badCharacters = new long[256];

            for (var i = 0; i < 256; i++)
            {
                badCharacters[i] = patternLength;
            }

            var lastPatternByte = patternLength - 1;

            for (long i = 0; i < lastPatternByte; i++)
            {
                badCharacters[pattern[i]] = lastPatternByte - i;
            }

            long index = 0;

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

        static void TestFindByteArray()
        {
            byte[] arr1 = new byte[60];
            byte[] arr2 = new byte[6];
            for (int i = 0; i < 10; i++)
            {
                arr1[i] = (byte)i;
                arr1[i + 10] = (byte)i;
                arr1[i + 20] = (byte)i;
                arr1[i + 30] = (byte)i;
                arr1[i + 40] = (byte)i;
                arr1[i + 50] = (byte)i;
            }
            for (int i = 3; i < 9; i++)
            {
                arr2[i - 3] = (byte)(i);
            }
            var find = IndexesOf(arr1, 0, arr2);
            if (find == null)
            {
            }
            else if (find.Count() == 0)
            {

            }
            long index1 = find.Last(i => i < 2);
            long index = IndexesOf(arr1, arr2);
        }

        #endregion 测试查找字节数组

        #region 测试先关闭线程再关闭串口的报错(System.ObjectDisposedException 已关闭 Safe handle)

        static SerialPort com = new SerialPort("COM4", 19200, Parity.Space, 8, StopBits.One);
        static void TestComport()
        {
            com.Open();
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    object _lock = new object();
                    bool iserr = false;
                    Thread.Sleep(1);
                    try
                    {
                        Thread thd = new Thread(() =>
                          {
                              try
                              {
                                  Console.WriteLine("write aaaa=====");
                                  while (com.IsOpen)
                                  {
                                      lock (_lock)
                                      {
                                          com.Write("aaaaaaaaaaaaaaaaaaaaaa");
                                      }
                                  }
                                  Console.WriteLine("write complete-----");
                              }
                              catch (Exception ex)
                              {
                                  if (ex is ObjectDisposedException)
                                  {
                                      Console.WriteLine(ex.Message + "!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                                      iserr = true;
                                  }
                              }
                          });
                        thd.IsBackground = true;
                        thd.Start();
                        Thread.Sleep(10);
                        Console.WriteLine("Close");
                        lock (_lock)
                        {
                            com.Close();
                        }

                        thd.Abort();
                        Thread.Sleep(1);
                        Console.WriteLine("Open");
                        com.Open();
                        if (iserr)
                        {
                            Console.ReadLine();
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex is ObjectDisposedException)
                        {
                            Console.WriteLine(ex.Message + "!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                            Console.Read();
                        }
                    }
                }
            });
            while (!Console.ReadLine().Contains("Exit"))
            {

            }
        }

        #endregion 测试先关闭线程再关闭串口的报错(System.ObjectDisposedException 已关闭 Safe handle)

        #region EBD195模拟

        static Random _random = new Random();
        static SerialPort _port;
        static bool _isRunning = false;
        static int _normalDDF = 135;
        static int _interTime = 1000;//ms
        static DateTime _lastSendTime = DateTime.Now;
        static bool _isPause = false;
        static int _pauseSpan = 0;
        static bool _isErr = false;
        static bool _isReadCompass = false;
        static int _compassPosition = 0;

        private static void EBD195Sim()
        {
            Console.WriteLine("EBD195 Sim StartUp……");
            TestEBD195();
            string str = Console.ReadLine();
            while (!string.IsNullOrEmpty(str))
            {
                if (str.StartsWith("E"))
                {
                    _isErr = true;
                }
                else if (str.StartsWith("S"))
                {
                    _isErr = false;
                }
                else
                {
                    int pauseSpan = 0;
                    if (int.TryParse(str, out pauseSpan))
                    {
                        _pauseSpan = pauseSpan;
                        _isPause = true;
                        Console.WriteLine("Pause:" + _pauseSpan + "ms");
                    }
                }
                str = Console.ReadLine();
            }
            _port.Close();
            _port.Dispose();
        }

        private static void TestEBD195()
        {
            _port = new SerialPort("COM3", 9600, Parity.None, 8, StopBits.One);
            _port.Open();
            _port.DataReceived += _port_DataReceived;
            _port.PinChanged += _port_PinChanged;
            //_serialPort.ReceivedBytesThreshold = 1;

            Task.Factory.StartNew((Action)(() =>
            {
                DateTime aliveTime = DateTime.Now;
                while (true)
                {
                    Thread.Sleep(10);
                    if (!_isRunning)
                    {
                        int span = (int)DateTime.Now.Subtract(aliveTime).TotalMilliseconds;
                        if (span < 1000)
                        {
                            continue;
                        }

                        if (_isPause && span < _pauseSpan)
                        {
                            continue;
                        }

                        string sendStr = "A*,*,*,2\r\n";
                        byte[] buffer = Encoding.ASCII.GetBytes(sendStr);
                        _port.Write(buffer, 0, buffer.Length);
                        aliveTime = DateTime.Now;
                    }
                    else
                    {
                        int span = (int)DateTime.Now.Subtract(_lastSendTime).TotalMilliseconds;
                        if (span < _interTime)
                        {
                            continue;
                        }

                        _lastSendTime = DateTime.Now;
                        aliveTime = DateTime.Now;

                        int rd = Program._random.Next(0, 100);
                        int ddfMin = 0;
                        int ddfMax = 360;
                        int quMin = 0;
                        int quMax = 100;
                        if (rd > 5 && rd < 95)
                        {
                            ddfMin = _normalDDF - 10;
                            ddfMax = _normalDDF + 10;
                            quMin = 30;
                            quMax = 100;
                        }
                        int ddf = Program._random.Next(ddfMin, ddfMax);
                        int quality = Program._random.Next(quMin, quMax);
                        int time = _interTime;
                        int level = Program._random.Next(40, 50);
                        string sendData = string.Format("A{0},{1},{2},{3}\r\n", ddf, quality, time, level);
                        if (_isErr)
                        {
                            sendData = string.Format("A*,*,*,{3}\r\n", ddf, quality, time, level);
                        }

                        byte[] buffer = Encoding.ASCII.GetBytes(sendData);
                        _port.Write(buffer, 0, buffer.Length);
                    }
                    if (_isReadCompass)
                    {
                        _isReadCompass = false;
                        _compassPosition += 5;
                        if (_compassPosition == 360)
                        {
                            _compassPosition = 0;
                        }

                        string sendData = string.Format("C{0}\r\n", _compassPosition);
                        Console.WriteLine("Send:" + sendData);
                        byte[] buffer = Encoding.ASCII.GetBytes(sendData);
                        _port.Write(buffer, 0, buffer.Length);
                    }
                }
            }));
        }

        private static void _port_PinChanged(object sender, SerialPinChangedEventArgs e)
        {
            bool res = false;
            switch (e.EventType)
            {
                case SerialPinChange.CDChanged:
                    res = _port.CDHolding;
                    break;
                case SerialPinChange.DsrChanged:
                    res = _port.DsrHolding;
                    break;
                default:
                    break;
            }
        }

        private static void _port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            byte[] buffer = new byte[_port.ReadBufferSize];
            string data = "";
            int recvCount = _port.Read(buffer, 0, buffer.Length);
            data = System.Text.Encoding.ASCII.GetString(buffer, 0, recvCount);
            Console.WriteLine(string.Format("Time:{0:HH:mm:ss.fff} Data:{1}", DateTime.Now, data));
            if (data.Contains("D1"))
            {
                _isRunning = true;
                Console.WriteLine("Start……\r\n=============================");
            }
            if (data.Contains("D0"))
            {
                _isRunning = false;
                Console.WriteLine("Stop……\r\n=============================");
            }
            if (data.Contains("I"))
            {
                //积分时间
                int index = data.IndexOf("I");
                int num = int.Parse(data.Substring(index + 1, 1));
                //x=0 100ms;x=1 200ms;x=2 500ms;x=3 1s;x=4 2s;x=5 5s;
                switch (num)
                {
                    case 0:
                        _interTime = 100;
                        break;
                    case 1:
                        _interTime = 200;
                        break;
                    case 2:
                        _interTime = 500;
                        break;
                    case 3:
                        _interTime = 1000;
                        break;
                    case 4:
                        _interTime = 2000;
                        break;
                    case 5:
                        _interTime = 5000;
                        break;
                    default:
                        _interTime = 100;
                        break;
                }
            }
            if (data.Contains("C?"))
            {
                // 查询电子罗盘
                _isReadCompass = true;
                Console.WriteLine("Read Compass......");
            }
        }

        #endregion EBD195模拟

        private static void TestUdp()
        {
            Task.Factory.StartNew(Server);
            Client();
        }

        private static void Server()
        {
            Socket srv = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            srv.Bind(new IPEndPoint(IPAddress.Any, 50000));
            while (true)
            {
                EndPoint point = new IPEndPoint(IPAddress.Any, 0);//用来保存发送方的ip和端口号
                byte[] buffer = new byte[1024];
                int len = srv.ReceiveFrom(buffer, ref point);
                string message = Encoding.ASCII.GetString(buffer, 0, len);
                Console.WriteLine(point.ToString() + ":" + message);
            }
        }

        private static void Client()
        {
            UdpClient client = new UdpClient("127.0.0.1", 50000);
            string str = Console.ReadLine();
            while (!string.IsNullOrEmpty(str))
            {
                byte[] data = Encoding.ASCII.GetBytes(str);
                client.Send(data, data.Length);
                str = Console.ReadLine();
            }
        }

        private static void Test3()
        {
            int[] arr1 = new int[] { 1, 2, 3, 4 };
            Console.WriteLine("before------------");
            Show(arr1);
            Test4(ref arr1);
            Console.WriteLine("after------------");
            Show(arr1);
        }

        private static void Show(int[] arr)
        {
            string str = "";
            for (int i = 0; i < arr.Length; i++)
            {
                str += arr[i].ToString() + ",";
            }
            Console.WriteLine(str);
        }

        private static void Test4(ref int[] arr)
        {
            arr = new int[5];
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = i + 10;
            }
        }

        private static void Test2()
        {
            Dictionary<string, Dictionary<string, int>> dic = new Dictionary<string, Dictionary<string, int>>();
            string[] strArr = new string[100000];
            for (int i = 0; i < 100000; i++)
            {
                string str = i.ToString();
                Dictionary<string, int> d = new Dictionary<string, int>();
                for (int j = 0; j < 100; j++)
                {
                    string s = j.ToString();
                    d.Add(s, j);
                }
                dic.Add(str, d);
                strArr[i] = str;
            }

            Console.WriteLine("1--------------");

            double avg = 0;
            for (int i = 0; i < 10; i++)
            {
                DateTime dt = DateTime.Now;
                for (int k = 0; k < 100; k++)
                {
                    for (int j = 0; j < dic.Count; j++)
                    {
                    }
                }
                double d = DateTime.Now.Subtract(dt).TotalMilliseconds;
                avg += d;
            }
            avg = avg / 10;

            Console.WriteLine("平均用时：" + avg);

            Console.WriteLine("2--------------");

            avg = 0;
            int count = dic.Count;
            for (int i = 0; i < 10; i++)
            {
                DateTime dt = DateTime.Now;
                for (int k = 0; k < 100; k++)
                {
                    for (int j = 0; j < count; j++)
                    {
                    }
                }
                double d = DateTime.Now.Subtract(dt).TotalMilliseconds;
                avg += d;
            }
            avg = avg / 10;

            Console.WriteLine("平均用时：" + avg);

            Console.WriteLine("3--------------");

            avg = 0;
            for (int i = 0; i < 10; i++)
            {
                DateTime dt = DateTime.Now;
                for (int k = 0; k < 100; k++)
                {
                    foreach (var pair in dic)
                    {

                    }
                }
                double d = DateTime.Now.Subtract(dt).TotalMilliseconds;
                avg += d;
            }
            avg = avg / 10;

            Console.WriteLine("平均用时：" + avg);

            Console.ReadLine();
            /*
             * 结论 第二种方法效率最高
             * 1--------------
             * 平均用时：24.3
             * 2--------------
             * 平均用时：15.2
             * 3--------------
             * 平均用时：148.5
             */
        }

        private static void CreateDir(string dir)
        {
            if (Directory.Exists(dir))
            {
                return;
            }

            string dir1 = dir.TrimEnd('\\');
            int index = dir1.LastIndexOf('\\');
            string dir2 = dir1;
            if (index > 0)
            {
                dir2 = dir1.Substring(0, index).TrimEnd('\\');
            }

            CreateDir(dir2);
            Directory.CreateDirectory(dir1);
        }

        private static void Test1()
        {
            ConcurrentDictionary<int, string> dic = new ConcurrentDictionary<int, string>();

            for (int i = 0; i < 10; i++)
            {
                dic.TryAdd(i, i.ToString());
                Console.WriteLine(i);
            }
            Console.WriteLine("------------");
            foreach (var pairs in dic)
            {
                Console.WriteLine(pairs.Key);
                if (pairs.Key == 5)
                {
                    string str;
                    dic.TryRemove(pairs.Key, out str);
                    Console.WriteLine("del " + pairs.Key);
                }
            }
            Console.WriteLine("new------------");
            foreach (var pairs in dic)
            {
                Console.WriteLine(pairs.Key);

            }
            Console.ReadLine();
        }

        private static void Test()
        {
            int[] fl = new int[10000];
            Random rd = new Random();
            for (int i = 0; i < fl.Length; i++)
            {
                fl[i] = rd.Next();
            }
            List<int> list = fl.ToList();
            while (string.IsNullOrEmpty(Console.ReadLine()))
            {
                /*
                    List,lambda 760.000     耗时最久
                    List,For 520.000
                    Array,lambda 704.000
                    Array,For 421.000       耗时最少
                 */
                DateTime dt = DateTime.Now;
                for (int i = 0; i < 10000; i++)
                {
                    int max = list.Max();
                }
                Console.WriteLine("List,lambda " + DateTime.Now.Subtract(dt).TotalMilliseconds.ToString("0.000"));
                dt = DateTime.Now;
                for (int i = 0; i < 10000; i++)
                {
                    int max = list[0];
                    for (int j = 0; j < list.Count; j++)
                    {
                        if (max < list[j])
                        {
                            max = list[j];
                        }
                    }
                }
                Console.WriteLine("List,For " + DateTime.Now.Subtract(dt).TotalMilliseconds.ToString("0.000"));
                dt = DateTime.Now;
                for (int i = 0; i < 10000; i++)
                {
                    int[] arr = list.ToArray();
                    int max = arr.Max();
                }
                Console.WriteLine("Array,lambda " + DateTime.Now.Subtract(dt).TotalMilliseconds.ToString("0.000"));
                dt = DateTime.Now;
                for (int i = 0; i < 10000; i++)
                {
                    int[] arr = list.ToArray();
                    int max = arr[0];
                    for (int j = 0; j < list.Count; j++)
                    {
                        if (max < arr[j])
                        {
                            max = arr[j];
                        }
                    }
                }
                Console.WriteLine("Array,For " + DateTime.Now.Subtract(dt).TotalMilliseconds.ToString("0.000"));
            }
        }
    }
}
