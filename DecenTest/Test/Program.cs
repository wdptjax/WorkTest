using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
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
            TestDDF550();
            //DDF550SendAsyn();
            //TestReverse();
            Console.ReadLine();

        }

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
                source[i] = rd.Next();

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
                    source[i] = rd.Next();

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
                    break;
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
                            break;

                        int index = buffer.IndexesOf(_xmlHeader, 0);
                        int offset = index;
                        while (offset < count)
                        {
                            if (BitConverter.IsLittleEndian)
                                Array.Reverse(buffer, index + 4, 4);
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
                                info.IndexAudio = 0;
                            socket.Send(data);
                        }
                        if (info.IsSendIQ)
                        {
                            byte[] data = _iqData[info.IndexIQ];
                            info.IndexIQ++;
                            if (info.IndexIQ >= _iqData.Count)
                                info.IndexIQ = 0;
                            socket.Send(data);
                        }
                        if (info.IsSendITU)
                        {
                            byte[] data = _ituData[info.IndexITU];
                            info.IndexITU++;
                            if (info.IndexITU >= _ituData.Count)
                                info.IndexITU = 0;
                            socket.Send(data);
                        }
                        if (info.IsSendSpectrum)
                        {
                            byte[] data = _spectrumData[info.IndexSpectrum];
                            info.IndexSpectrum++;
                            if (info.IndexSpectrum >= _spectrumData.Count)
                                info.IndexSpectrum = 0;
                            socket.Send(data);
                        }
                        if (info.IsSendDF)
                        {
                            byte[] data = _dfData[info.IndexDF];
                            info.IndexDF++;
                            if (info.IndexDF >= _dfData.Count)
                                info.IndexDF = 0;
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
                Array.Reverse(lenArr);
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
                    sr.Dispose();
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
            string strData = "00-0E-B2-00-00-64-00-02-02-F9-00-00-00-00-0B-96-14-B5-00-00-00-00-0B-6E-3C-3F-78-6D-6C-20-76-65-72-73-69-6F-6E-3D-22-31-00-00-00-C9-00-00-00-00-00-00-00-4C-80-0C-F8-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-C9-00-00-00-00-05-95-BF-A0-00-00-00-00-00-00-C3-50-00-00-00-01-00-98-96-80-46-DD-BA-7F-00-01-86-64-00-5F-FE-D4-00-00-FF-FF-02-20-20-11-00-00-00-00-00-10-38-D5-15-69-9F-BE-FC-30-AE-17-00-91-00-00-00-00-00-00-FF-5D-FF-5C-FF-5C-FF-5E-00-2D-FF-5E-FF-5A-FF-5B-FF-5C-FF-5D-FF-5F-FF-5A-FF-95-00-A9-01-33-00-B3-FF-A8-FF-5E-FF-6A-FF-59-FF-5F-FF-5A-FF-5A-FF-F0-01-6F-FF-E5-FF-5C-FF-5C-FF-5E-FF-5F-FF-59-FF-5D-FF-5B-FF-5C-FF-59-FF-5C-FF-5A-FF-63-FF-68-FF-63-FF-61-FF-5D-FF-5D-FF-60-FF-5D-FF-5D-FF-5D-00-1F-01-84-00-22-FF-5F-FF-65-FF-68-FF-65-FF-64-FF-62-FF-5D-FF-5D-FF-5F-FF-79-FF-D0-FF-87-00-06-FF-79-FF-60-FF-E7-01-30-FF-F5-FF-8E-FF-5F-FF-61-FF-60-FF-63-FF-61-FF-5F-FF-61-FF-5E-FF-5C-FF-60-FF-62-FF-60-FF-61-FF-60-FF-5F-FF-63-FF-92-FF-A9-FF-7F-FF-63-FF-62-FF-60-FF-60-FF-63-FF-60-FF-60-FF-66-00-12-FF-6A-FF-7F-00-8D-01-96-00-9F-FF-83-FF-61-FF-65-FF-65-FF-6D-FF-65-FF-68-FF-95-FF-A6-00-26-01-96-00-79-FF-A7-FF-79-FF-92-00-D5-01-BC-00-AC-FF-A9-FF-7E-FF-EC-FF-79-FF-6A-FF-68-FF-74-FF-64-FF-64-FF-68-FF-74-FF-65-FF-62-FF-61-FF-63-FF-63-FF-61-FF-63-FF-72-00-55-01-16-00-4E-FF-64-FF-61-FF-62-FF-63-FF-5F-FF-62-FF-65-FF-61-FF-61-FF-73-FF-D2-FF-75-FF-5E-FF-61-FF-61-FF-62-FF-62-FF-7B-FF-C1-FF-84-FF-61-FF-64-FF-65-FF-65-FF-61-FF-61-FF-63-FF-61-FF-61-FF-CF-01-5A-FF-BC-FF-5F-FF-61-FF-61-FF-64-FF-61-FF-60-FF-6C-FF-A1-FF-F1-FF-A6-FF-6B-FF-CE-00-CF-FF-E7-FF-65-FF-67-FF-79-FF-63-FF-61-FF-61-FF-5F-FF-64-FF-67-00-56-01-6A-00-37-FF-67-06-5F-07-A2-05-41-0A-A2-0B-DF-09-55-03-FA-08-68-0B-79-01-14-0D-A6-01-63-03-58-04-AC-04-7E-04-95-04-96-09-AF-09-5B-02-8F-07-45-07-15-01-5B-02-39-02-1F-02-5B-06-8B-07-0B-01-31-00-13-07-16-07-7F-04-FC-0A-B7-0A-C9-03-E7-04-2B-0B-D9-0B-C5-05-E0-04-5D-0A-86-08-0A-02-68-0A-E7-08-5A-08-34-02-52-02-51-02-65-04-14-05-1F-0A-5E-09-8A-0E-07-02-E3-06-CB-00-74-0D-3E-01-10-01-24-0C-98-0B-D8-0B-AB-02-4A-02-4B-02-92-02-24-00-58-0C-F1-03-26-04-5F-00-0A-05-36-00-BF-08-D6-0B-80-02-0B-08-B9-01-D0-05-6B-0B-C0-02-F5-04-9C-09-DD-0D-4D-0C-87-0B-92-04-2F-03-99-05-67-0A-BF-06-56-06-81-0B-E5-04-94-05-AD-01-CA-0D-D2-00-16-0D-F8-0E-05-00-4B-07-ED-0C-6F-0C-4C-0D-9A-02-C7-04-5F-0D-A8-0D-5B-0D-4E-0B-E4-0C-2F-0C-F7-03-79-04-74-05-3C-05-0F-04-AC-04-FE-0B-7B-0B-93-0B-B4-06-20-04-0D-0A-B5-05-CA-09-3F-05-12-0A-27-03-5C-0C-20-06-B9-09-EE-0D-88-05-44-06-E2-01-DB-02-12-02-32-02-41-01-50-0A-58-01-A6-0A-13-0A-E1-00-CD-0C-7C-01-C9-00-EF-09-9C-09-E9-09-A5-00-52-0D-A0-0A-0B-03-37-0B-CA-01-55-01-0E-00-C2-08-88-05-D2-0C-CE-07-18-04-35-05-6A-09-D1-03-16-0B-D4-06-AA-06-F2-06-FC-00-7B-06-7A-0A-B6-06-AD-08-2E-0B-7B-0C-4F-0C-DD-0B-F7-0D-5C-01-15-0A-E7-0C-26-0D-4D-0B-8C-02-AF-03-0F-05-CF-09-20-06-4D-00-B4-04-3F-02-0B-02-1D-02-2B-02-1A-02-5D-01-22-01-E1-01-46-03-9A-03-E3-00-95-01-49-02-2C-01-CA-00-80-02-47-01-03-02-8C-03-1A-03-2A-03-0C-02-ED-01-67-02-71-01-52-00-16-02-12-00-11-03-CA-03-E6-03-D7-00-09-00-7F-00-F9-00-4B-01-26-01-06-00-6A-02-92-01-20-00-15-02-2F-03-05-02-B0-02-1E-01-1B-01-A8-00-A3-01-0D-00-3F-00-6C-00-87-03-E2-03-E1-03-DE-00-46-01-D4-03-5D-02-31-00-D2-00-E4-01-01-00-95-00-E9-03-E2-03-DF-03-C8-03-D1-03-D1-00-92-03-E3-03-D9-03-E3-03-97-00-66-00-61-01-5E-02-23-01-BA-01-88-00-AA-01-D4-02-4D-00-51-00-A1-01-81-00-8A-01-42-00-07-01-5F-03-E2-03-C1-03-01-01-7A-02-5E-00-6A-00-37-00-20-00-71-00-6B-03-AE-02-87-03-D5-03-B1-03-E1-03-DA-03-C0-03-CE-00-B7-01-E8-01-34-03-24-01-68-01-A9-03-A0-03-C2-03-CA-02-7E-03-DD-03-B8-02-6F-03-BB-03-DD-03-D8-03-92-03-C9-03-86-03-CC-03-B2-02-41-02-A2-03-1E-01-17-02-12-00-E2-03-A0-01-00-01-B8-00-7D-00-F5-00-0E-02-29-02-0A-03-D9-03-E3-03-E1-03-D7-00-FB-01-70-01-76-00-BA-01-B8-01-A9-00-D6-02-7F-01-04-03-3F-03-B8-03-9A-01-AD-01-0E-01-B4-01-B4-00-D8-03-B7-03-C4-03-D3-01-29-00-0B-01-6A-00-BB-01-CB-01-DB-00-90-01-54-01-BB-03-A3-03-58-03-B4-02-17-01-83-00-26-01-2B-01-68-00-CD-02-64-03-D5-03-E5-03-D3-01-CB-03-C8-03-AF-03-83-02-8B-00-DA-02-B8-01-26-01-76-01-AC-01-09-01-08-03-9C-03-DD-03-E6-03-DF-03-B1-00-E7-00-E6-00-E6-00-E8-01-B7-00-E8-00-E4-00-E4-00-E5-00-E6-00-E8-00-E3-01-1E-02-32-02-BC-02-3B-01-30-00-E6-00-F2-00-E1-00-E7-00-E2-00-E2-01-78-02-F6-01-6C-00-E3-00-E3-00-E5-00-E6-00-E0-00-E4-00-E2-00-E2-00-DF-00-E2-00-E0-00-E9-00-EE-00-E9-00-E7-00-E3-00-E2-00-E5-00-E2-00-E2-00-E2-01-A4-03-09-01-A7-00-E4-00-E9-00-EC-00-E9-00-E8-00-E6-00-E1-00-E1-00-E3-00-FD-01-53-01-0A-01-89-00-FC-00-E3-01-6A-02-B3-01-78-01-11-00-E1-00-E3-00-E2-00-E5-00-E3-00-E1-00-E3-00-E0-00-DE-00-E1-00-E3-00-E1-00-E2-00-E1-00-E0-00-E4-01-13-01-2A-00-FF-00-E3-00-E2-00-E0-00-E0-00-E3-00-E0-00-E0-00-E5-01-91-00-E9-00-FE-02-0C-03-15-02-1E-01-02-00-E0-00-E3-00-E3-00-EB-00-E3-00-E6-01-13-01-24-01-A4-03-14-01-F6-01-24-00-F6-01-0F-02-52-03-39-02-29-01-26-00-FB-01-68-00-F5-00-E6-00-E4-00-F0-00-E0-00-E0-00-E4-00-F0-00-E1-00-DE-00-DD-00-DF-00-DF-00-DD-00-DF-00-EE-01-D0-02-91-01-C9-00-DF-00-DC-00-DD-00-DE-00-DA-00-DD-00-E0-00-DC-00-DC-00-EE-01-4D-00-F0-00-D9-00-DC-00-DC-00-DD-00-DD-00-F6-01-3C-00-FF-00-DC-00-DF-00-DF-00-DF-00-DB-00-DB-00-DD-00-DB-00-DB-01-49-02-D4-01-36-00-D9-00-DB-00-DB-00-DE-00-DB-00-DA-00-E6-01-1B-01-6B-01-20-00-E5-01-48-02-49-01-61-00-DF-00-E0-00-F2-00-DC-00-DA-00-DA-00-D8-00-DD-00-E0-01-CF-02-E3-01-B0-00-E0-FF-5D-FF-5C-FF-5C-FF-5E-00-2D-FF-5E-FF-5A-FF-5B-FF-5C-FF-5D-FF-5F-FF-5A-FF-95-00-A9-01-33-00-B3-FF-A8-FF-5E-FF-6A-FF-59-FF-5F-FF-5A-FF-5A-FF-F0-01-6F-FF-E5-FF-5C-FF-5C-FF-5E-FF-5F-FF-59-FF-5D-FF-5B-FF-5C-FF-59-FF-5C-FF-5A-FF-63-FF-68-FF-63-FF-61-FF-5D-FF-5D-FF-60-FF-5D-FF-5D-FF-5D-00-1F-01-84-00-22-FF-5F-FF-65-FF-68-FF-65-FF-64-FF-62-FF-5D-FF-5D-FF-5F-FF-79-FF-D0-FF-87-00-06-FF-79-FF-60-FF-E7-01-30-FF-F5-FF-8E-FF-5F-FF-61-FF-60-FF-63-FF-61-FF-5F-FF-61-FF-5E-FF-5C-FF-60-FF-62-FF-60-FF-61-FF-60-FF-5F-FF-63-FF-92-FF-A9-FF-7F-FF-63-FF-62-FF-60-FF-60-FF-63-FF-60-FF-60-FF-66-00-12-FF-6A-FF-7F-00-8D-01-96-00-9F-FF-83-FF-61-FF-65-FF-65-FF-6D-FF-65-FF-68-FF-95-FF-A6-00-26-01-96-00-79-FF-A7-FF-79-FF-92-00-D5-01-BC-00-AC-FF-A9-FF-7E-FF-EC-FF-79-FF-6A-FF-68-FF-74-FF-64-FF-64-FF-68-FF-74-FF-65-FF-62-FF-61-FF-63-FF-63-FF-61-FF-63-FF-72-00-55-01-16-00-4E-FF-64-FF-61-FF-62-FF-63-FF-5F-FF-62-FF-65-FF-61-FF-61-FF-73-FF-D2-FF-75-FF-5E-FF-61-FF-61-FF-62-FF-62-FF-7B-FF-C1-FF-84-FF-61-FF-64-FF-65-FF-65-FF-61-FF-61-FF-63-FF-61-FF-61-FF-CF-01-5A-FF-BC-FF-5F-FF-61-FF-61-FF-64-FF-61-FF-60-FF-6C-FF-A1-FF-F1-FF-A6-FF-6B-FF-CE-00-CF-FF-E7-FF-65-FF-67-FF-79-FF-63-FF-61-FF-61-FF-5F-FF-64-FF-67-00-56-01-6A-00-37-FF-67-00-00-00-00-00-00-02-5D-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-02-FD-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-E6-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-02-49-00-05-00-05-00-00-00-00-00-00-01-EF-00-00-00-05-01-31-00-CD-00-00-00-00-00-00-00-00-00-00-01-4A-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-05-00-00-01-D1-00-00-00-00-01-BD-00-00-00-00-00-00-00-00-01-18-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-01-C7-01-31-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-01-F9-01-6D-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-01-7C-00-05-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-01-13-00-9B-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-01-4A-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-01-C2-00-E6-01-1D-00-00-00-00-00-00-00-0A-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-1E-00-00-01-F9-01-36-00-00-00-00-00-00-00-00-00-00-00-00-03-2A-00-00-02-3F-00-00-00-00-00-00-00-00-00-C3-00-AA-00-00-00-00-01-9F-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-9E-01-9E-01-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E";
            string[] strArr = strData.Split('-');
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
                            offset += Marshal.SizeOf(typeof(GenericAttributeConventional));
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
                catch (Exception ex)
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

                return null;
            }
            catch (Exception ex)
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
            catch (Exception ex)
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
            catch (Exception ex)
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
            catch (Exception ex)
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
            catch (Exception ex)
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
                if (freqIndex > pCommon.NumberOfTraceItems) freqIndex = 0;
                float level = pLevel[freqIndex];
                float azimuth = pAzimuth[freqIndex];
                float quality = pQuality[freqIndex];

                return null;
            }
            catch (Exception ex)
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
                        Level[i] = short.MinValue;
                    offset += 2;
                }
                for (int i = 0; i < dataCnt; i++)
                {
                    Array.Reverse(buffer, offset, 4);
                    FreqOffset[i] = BitConverter.ToInt32(buffer, offset);
                    if (FreqOffset[i] == 10000000)
                        FreqOffset[i] = int.MinValue;
                    offset += 4;
                }

                if ((selectorFlags & (uint)FLAGS.FSTRENGTH) > 0)
                {
                    for (int i = 0; i < dataCnt; i++)
                    {
                        Array.Reverse(buffer, offset, 2);
                        FStrength[i] = BitConverter.ToInt16(buffer, offset);
                        if (FStrength[i] == 0x7FFF)
                            FStrength[i] = short.MinValue;
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
                            AMDepth[i] = short.MinValue;
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
                            FMDev[i] = int.MinValue;
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
                            PMDepth[i] = short.MinValue;
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
                            BandWidth[i] = int.MinValue;
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
                            DfFstrength[i] = short.MinValue;
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
                            Elevation[i] = short.MinValue;
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
                            DfOmniphase[i] = short.MinValue;
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
            int count = 0;
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
                                          com.Write("aaaaaaaaaaaaaaaaaaaaaa");
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
                            com.Close();
                        thd.Abort();
                        Thread.Sleep(1);
                        Console.WriteLine("Open");
                        com.Open();
                        if (iserr)
                            Console.ReadLine();
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

            Task.Factory.StartNew(() =>
            {
                DateTime aliveTime = DateTime.Now;
                while (true)
                {
                    Thread.Sleep(10);
                    if (!_isRunning)
                    {
                        int span = (int)DateTime.Now.Subtract(aliveTime).TotalMilliseconds;
                        if (span < 1000)
                            continue;
                        if (_isPause && span < _pauseSpan)
                            continue;
                        string sendStr = "A*,*,*,2\r\n";
                        byte[] buffer = Encoding.ASCII.GetBytes(sendStr);
                        _port.Write(buffer, 0, buffer.Length);
                        aliveTime = DateTime.Now;
                    }
                    else
                    {
                        int span = (int)DateTime.Now.Subtract(_lastSendTime).TotalMilliseconds;
                        if (span < _interTime)
                            continue;
                        _lastSendTime = DateTime.Now;
                        aliveTime = DateTime.Now;

                        int rd = _random.Next(0, 100);
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
                        int ddf = _random.Next(ddfMin, ddfMax);
                        int quality = _random.Next(quMin, quMax);
                        int time = _interTime;
                        int level = _random.Next(40, 50);
                        string sendData = string.Format("A{0},{1},{2},{3}\r\n", ddf, quality, time, level);
                        if (_isErr)
                            sendData = string.Format("A*,*,*,{3}\r\n", ddf, quality, time, level);
                        byte[] buffer = Encoding.ASCII.GetBytes(sendData);
                        _port.Write(buffer, 0, buffer.Length);
                    }
                    if (_isReadCompass)
                    {
                        _isReadCompass = false;
                        _compassPosition += 5;
                        if (_compassPosition == 360)
                            _compassPosition = 0;
                        string sendData = string.Format("C{0}\r\n", _compassPosition);
                        Console.WriteLine("Send:" + sendData);
                        byte[] buffer = Encoding.ASCII.GetBytes(sendData);
                        _port.Write(buffer, 0, buffer.Length);
                    }
                }
            });
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
                return;

            string dir1 = dir.TrimEnd('\\');
            int index = dir1.LastIndexOf('\\');
            string dir2 = dir1;
            if (index > 0)
                dir2 = dir1.Substring(0, index).TrimEnd('\\');
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
                            max = list[j];
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
                            max = arr[j];
                    }
                }
                Console.WriteLine("Array,For " + DateTime.Now.Subtract(dt).TotalMilliseconds.ToString("0.000"));
            }
        }
    }
}
