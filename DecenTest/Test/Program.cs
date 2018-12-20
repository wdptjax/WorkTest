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
            //TestDDF550();
            Task.Factory.StartNew(new Action(() => PingStatus()));
            //DDF550SendAsyn();
            //TestReverse();
            Console.ReadLine();

        }

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
            string strData = "00-0E-B2-00-00-64-00-02-00-01-00-00-00-00-40-56-03-85-40-42-10-00-00-3A-80-00-00-00-00-01-00-04-00-04-E2-00-05-E2-0A-E0-00-01-D4-C0-00-00-00-62-80-01-01-7F-46-4D-00-00-00-00-00-00-00-00-00-00-2E-B1-30-15-00-00-00-00-00-00-00-00-15-69-5E-B3-B8-9F-AC-31-00-00-00-53-00-38-00-43-00-4E-00-35-00-53-00-2E-00-46-00-2D-00-2E-00-2E-00-1A-00-2C-00-17-00-21-00-21-00-14-00-2F-00-0F-00-36-00-17-00-35-00-27-00-32-00-33-00-32-00-32-00-38-00-1F-00-41-00-02-00-46-FF-E5-00-43-FF-D2-00-39-FF-C8-00-2E-FF-C2-00-23-FF-B9-00-14-FF-B2-FF-FC-FF-B6-FF-D9-FF-CD-FF-B5-FF-F6-FF-A0-00-27-FF-A2-00-4F-FF-BB-00-65-FF-E2-00-68-00-0A-00-5B-00-2C-00-46-00-46-00-2B-00-55-00-0F-00-5C-FF-F7-00-58-FF-E9-00-4D-FF-E9-00-3F-FF-F2-00-32-FF-FD-00-2C-FF-FF-00-2C-FF-F3-00-2D-FF-DF-00-29-FF-C9-00-1B-FF-BA-00-06-FF-B7-FF-EE-FF-BE-FF-D9-FF-CC-FF-CA-FF-DA-FF-C2-FF-E5-FF-C3-FF-E9-FF-CB-FF-E3-FF-D7-FF-D8-FF-E4-FF-CB-FF-ED-FF-C0-FF-F2-FF-B8-FF-F3-FF-B3-FF-EF-FF-B2-FF-E5-FF-BB-FF-D4-FF-D0-FF-BF-FF-F1-FF-B0-00-18-FF-AF-00-3B-FF-BE-00-54-FF-DA-00-5F-00-00-00-58-00-2A-00-3D-00-52-00-10-00-6C-FF-DB-00-6D-FF-AC-00-54-FF-91-00-27-FF-8D-FF-F4-FF-9E-FF-C4-FF-C2-FF-9E-FF-F2-FF-89-00-28-FF-8A-00-58-FF-A4-00-77-FF-D0-00-7E-00-04-00-6F-00-35-00-51-00-59-00-2C-00-6B-00-09-00-6C-FF-EE-00-63-FF-D9-00-53-FF-CB-00-41-FF-C4-00-2E-FF-C5-00-20-FF-C9-00-1A-FF-C8-00-16-FF-C0-00-08-FF-BB-FF-E7-FF-CE-FF-B9-00-03-FF-99-00-49-FF-A3-00-7B-FF-DE-00-7D-00-2F-00-4B-00-70-FF-FD-00-86-FF-B3-00-6B-FF-86-00-2F-FF-80-FF-E9-FF-9C-FF-AE-FF-CD-FF-8A-00-06-FF-81-00-39-FF-90-00-60-FF-B2-00-76-FF-E1-00-76-00-1B-00-59-00-53-00-1F-00-7A-FF-D5-00-7C-FF-94-00-53-FF-74-00-0A-FF-84-FF-BB-FF-BF-FF-82-00-13-FF-76-00-5F-FF-9D-00-87-FF-EC-00-76-00-43-00-34-00-7D-FF-DB-00-80-FF-96-00-48-FF-85-FF-F1-FF-B2-FF-A5-00-05-FF-8A-00-53-FF-AF-00-73-FF-FC-00-57-00-48-00-11-00-6D-FF-C8-00-5E-FF-9A-00-29-FF-92-FF-EA-FF-A7-FF-B5-FF-CC-FF-94-FF-F6-FF-89-00-1D-FF-8F-00-39-FF-A3-00-47-FF-BD-00-49-FF-D4-00-4A-FF-E6-00-50-FF-FE-00-50-00-27-00-34-00-5A-FF-F0-00-78-FF-9E-00-5B-FF-73-00-03-FF-99-FF-A1-FF-FE-FF-7D-00-58-FF-B3-00-63-00-16-00-1D-00-54-FF-C9-00-3D-FF-B2-FF-ED-FF-E9-FF-B1-00-39-FF-BE-00-5D-00-08-00-35-00-51-FF-E4-00-5D-FF-A9-00-26-FF-AD-FF-D7-FF-EA-FF-A6-00-33-FF-B0-00-60-FF-E7-00-5F-00-29-00-3B-00-5A-00-08-00-6F-FF-D5-00-67-FF-AF-00-4C-FF-9B-00-26-FF-9A-00-02-FF-A5-FF-E9-FF-B2-FF-DC-FF-BB-FF-CF-FF-C9-FF-B6-FF-F3-FF-99-00-3B-FF-98-00-7D-FF-D3-00-82-00-38-00-33-00-83-FF-C4-00-73-FF-92-00-14-FF-C4-FF-BF-00-1D-FF-C2-00-3B-00-0E-00-01-00-44-FF-B7-00-20-FF-B7-FF-C8-00-09-FF-9C-00-5B-FF-CC-00-60-00-2C-00-16-00-64-FF-C0-00-4B-FF-9F-FF-FE-FF-C0-FF-B6-00-05-FF-9D-00-45-FF-B3-00-68-FF-E6-00-6C-00-1E-00-57-00-4C-00-34-00-67-00-10-00-6E-FF-F5-00-65-FF-E9-00-54-FF-EB-00-44-FF-F3-00-39-FF-FB-00-33-FF-FF-00-30-FF-FD-00-2F-FF-F4-00-30-FF-E1-00-2D-FF-C6-00-1C-FF-B3-FF-F5-FF-BD-FF-BD-FF-F3-FF-90-00-46-FF-97-00-83-FF-E0-00-74-00-45-00-1A-00-7D-FF-B8-00-58-FF-A0-FF-F7-FF-E0-FF-B8-00-2B-FF-D4-00-2D-00-1E-FF-E7-00-39-FF-B3-FF-FF-FF-D6-FF-B5-00-2D-FF-B3-00-5A-00-02-00-2C-00-50-FF-D3-00-52-FF-A1-00-0D-FF-B5-FF-C1-FF-ED-FF-9E-00-1B-FF-9E-00-32-FF-A7-00-39-FF-AC-00-34-FF-B4-00-1D-FF-C1-FF-F5-FF-CF-FF-CA-FF-DA-FF-AC-FF-E5-FF-A0-FF-F0-FF-A2-FF-F6-FF-AB-FF-EE-FF-B9-FF-D8-FF-CF-FF-BE-FF-EB-FF-A9-00-0D-FF-A1-00-34-FF-AC-00-56-FF-CF-00-65-00-08-00-53-00-48-00-1F-00-77-FF-D6-00-7E-FF-96-00-57-FF-7B-00-0C-FF-95-FF-BB-FF-DE-FF-88-00-38-FF-8D-00-77-FF-CB-00-7B-00-22-00-45-00-65-FF-F3-00-73-FF-AF-00-4E-FF-92-00-0C-FF-9D-FF-CC-FF-C1-FF-9F-FF-EC-FF-8B-00-13-FF-8B-00-30-FF-99-00-3E-FF-AE-00-3E-FF-C4-00-32-FF-D5-00-22-FF-DD-00-17-FF-DD-00-1B-FF-DC-00-2E-FF-E7-00-3F-00-0A-00-35-00-3E-00-02-00-65-FF-B6-00-5A-FF-82-00-14-FF-93-FF-B7-FF-E8-FF-84-00-46-FF-A6-00-62-00-03-00-29-00-4B-FF-D3-00-42-FF-B3-FF-F5-FF-E6-FF-B3-00-3B-FF-BD-00-60-00-0B-00-32-00-56-FF-DB-00-5A-FF-A6-00-15-FF-BE-FF-C4-00-0B-FF-A7-00-4E-FF-D0-00-59-00-1B-00-2D-00-54-FF-ED-00-61-FF-B8-00-4A-FF-9B-00-26-FF-92-00-01-FF-99-FF-DE-FF-B1-FF-BC-FF-D8-FF-A2-00-08-FF-98-00-35-FF-A2-00-58-FF-BC-00-6D-FF-E1-00-73-00-0F-00-62-00-41-00-39-00-6C-FF-FD-00-7F-FF-C1-00-71-FF-98-00-4B-FF-89-00-1B-FF-8F-FF-F0-FF-A1-FF-CF-FF-B7-FF-BA-FF-CB-FF-B1-FF-DB-FF-B5-FF-E4-FF-C3-FF-E6-FF-D4-FF-E2-FF-E3-FF-DA-FF-EB-FF-D0-FF-ED-FF-C5-FF-EE-FF-BC-FF-F0-FF-B8-FF-F0-FF-BA-FF-ED-FF-BE-FF-E7-FF-BC-FF-E3-FF-B5-FF-EB-FF-B0-00-05-FF-B7-00-2A-FF-CE-00-4C-FF-EA-00-5E-00-00-00-5F-00-07-00-56-00-01-00-4D-FF-F6-00-47-FF-E9-00-43-FF-D7-00-3C-FF-C3-00-29-FF-B3-00-0A-FF-B3-FF-E2-FF-C7-FF-BC-FF-ED-FF-A2-00-1C-FF-9D-00-47-FF-AD-00-66-FF-CA-00-73-FF-EC-00-71-00-0C-00-61-00-27-00-49-00-3E-00-28-00-4F-00-03-00-58-FF-DD-00-56-FF-BA-00-47-FF-9F-00-29-FF-90-FF-FC-FF-98-FF-C4-FF-C1-FF-90-00-06-FF-7A-00-4F-FF-92-00-7F-FF-D1-00-84-00-1E-00-61-00-5E-00-28-00-80-FF-EB-00-84-FF-B6-00-6E-FF-91-00-45-FF-82-00-12-FF-8B-FF-E0-FF-A4-FF-BA-FF-C4-FF-A6-FF-E0-FF-A2-FF-F4-FF-A9-00-01-FF-B5-00-08-FF-C1-00-0B-FF-C9-00-07-FF-CE-FF-FA-FF-CF-FF-E2-FF-D1-FF-C5-FF-DB-FF-AB-FF-F0-FF-9F-00-10-FF-A6-00-32-FF-BD-00-4E-FF-E0-00-5C-00-05-00-59-00-24-00-4B-00-36-00-3A-00-3D-00-2E-00-3B-00-2A-00-36-00-2C-00-2F-00-33-00-26-00-3B-00-1B-00-44-00-0B-00-4E-FF-F0-00-53-FF-CA-00-48-FF-A7-00-1D-FF-A4-FF-D4-FF-DA-FF-8F-00-37-FF-84-00-7C-FF-C7-00-6F-00-2A-00-19-00-5A-FF-C7-00-34-FF-BF-FF-E4-FF-FF-FF-BC-00-3E-FF-E0-00-43-00-27-00-13-00-56-FF-DA-00-58-FF-B9-00-43-FF-AD-00-32-FF-AA-00-2C-FF-B1-00-2C-FF-CD-00-31-FF-FA-00-39-00-2A-00-3D-00-4C-00-35-00-5D-00-24-00-60-00-17-00-58-00-1E-00-43-00-39-00-1B-00-58-FF-E3-00-64-FF-A9-00-49-FF-8A-00-08-FF-A0-FF-B9-FF-E9-FF-87-00-42-FF-91-00-7C-FF-D4-00-7A-00-27-00-45-00-61-00-00-00-70-FF-C6-00-5E-FF-A1-00-40-FF-8D-00-1F-FF-89-00-00-FF-94-FF-E4-FF-A8-FF-D6-FF-BC-FF-DC-FF-CA-FF-F8-FF-D9-00-21-FF-F1-00-4B-00-13-00-66-00-38-00-67-00-53-00-4A-00-5B-00-19-00-50-FF-E3-00-37-FF-B9-00-19-FF-A5-FF-FD-FF-A5-FF-EC-FF-B1-FF-E9-FF-B9-FF-FF-FF-B8-00-2C-FF-BD-00-5D-FF-E0-00-6B-00-23-00-3B-00-63-FF-DE-00-68-FF-97-00-20-FF-9F-FF-BC-FF-EE-FF-8B-00-3F-FF-B2-00-4C-00-0B-00-12-00-4A-FF-C6-00-43-FF-A1-00-0A-FF-B3-FF-CC-FF-E4-FF-A8-00-1A-FF-A3-00-45-FF-B7-00-60-FF-DB-00-6A-00-05-00-66-00-22-00-59-00-20-00-41-FF-FD-00-18-FF-CF-FF-DC-FF-B4-FF-9F-FF-BF-FF-7F-FF-EE-FF-92-00-28-FF-D2-00-50-00-21-00-55-00-5B-00-3A-00-70-00-10-00-6A-FF-EF-00-5E-FF-EA-00-51-00-08-00-32-00-3D-FF-F3-00-5F-FF-A9-00-4A-FF-89-FF-FE-FF-B6-FF-AF-00-14-FF-9D-00-5F-FF-D8-00-60-00-2E-00-21-00-5D-FF-D8-00-4B-FF-AE-00-13-FF-A4-FF-DF-FF-A7-FF-C5-FF-A8-FF-C3-FF-AD-FF-D0-FF-B7-FF-E9-FF-C4-00-0C-FF-D2-00-36-FF-E9-00-5A-00-13-00-6A-00-43-00-57-00-61-00-23-00-55-FF-E0-00-23-FF-AA-FF-E8-FF-95-FF-BF-FF-A0-FF-B4-FF-BD-FF-B9-FF-D9-FF-BF-FF-E6-FF-C9-FF-D9-FF-EA-FF-BC-00-27-FF-AE-00-65-FF-D2-00-71-00-1F-00-34-00-5F-FF-D2-00-59-FF-97-00-0D-FF-AC-FF-B8-FF-F7-FF-98-00-3E-FF-BA-00-5B-FF-F8-00-57-00-2B-00-4B-00-46-00-46-00-4E-00-48-00-43-00-46-00-1C-00-38-FF-E0-00-16-FF-A9-FF-E4-FF-92-FF-B1-FF-A7-FF-96-FF-DB-FF-A0-00-19-FF-CB-00-4D-00-08-00-66-00-42-00-5E-00-65-00-37-00-6B-00-00-00-58-FF-CD-00-3C-FF-AD-00-24-FF-A6-00-15-FF-AE-00-0D-FF-BA-00-10-FF-C3-00-25-FF-CE-00-46-FF-EB-00-59-00-24-00-3E-00-62-FF-F3-00-76-FF-A5-00-42-FF-93-FF-DD-FF-D0-FF-91-00-2A-FF-97-00-57-FF-E4-00-3A-00-38-FF-F2-00-58-FF-B7-00-3D-FF-A5-00-08-FF-AF-FF-D8-FF-BD-FF-BA-FF-C3-FF-B0-FF-C2-FF-BE-FF-C1-FF-E2-FF-C5-00-11-FF-CB-00-39-FF-D4-00-53-FF-E0-00-62-FF-F6-00-6B-00-17-00-66-00-3A-00-48-00-51-00-13-00-54-FF-DB-00-4A-FF-B8-00-42-FF-B7-00-42-FF-D0-00-47-FF-EF-00-4B-00-06-00-4D-00-16-00-4A-00-28-00-3B-00-40-00-18-00-55-FF-E1-00-56-FF-AC-00-34-FF-90-FF-F5-FF-9D-FF-AF-FF-CF-FF-7F-00-11-FF-77-00-4A-FF-92-00-6A-FF-BA-00-6B-FF-D8-00-50-FF-DE-00-1F-FF-D4-FF-DF-FF-CE-FF-A3-FF-DE-FF-80-00-07-FF-89-00-39-FF-BB-00-5B-FF-FC-00-61-00-2D-00-50-00-3F-00-3E-00-3B-00-38-00-33-00-39-00-34-00-32-00-3C-00-19-00-48-FF-FB-00-4E-FF-F2-00-47-00-08-00-2F-00-32-00-07-00-53-FF-DA-00-5A-FF-B5-00-46-FF-9D-00-1B-FF-9E-FF-DD-FF-C6-FF-9E-00-14-FF-80-00-63-FF-A6-00-7D-00-01-00-4A-00-56-FF-EB-00-67-FF-A6-00-2F-FF-A3-FF-DD-FF-D5-FF-A9-00-12-FF-A3-00-3E-FF-BB-00-55-FF-D8-00-5F-FF-EF-00-5F-FF-FF-00-55-00-05-00-42-FF-FB-00-29-FF-E5-00-0E-FF-CA-FF-F0-FF-B3-FF-CF-FF-A7-FF-AD-FF-B0-FF-97-FF-D4-FF-A0-00-11-FF-D2-00-4B-00-1E-00-60-00-60-00-3D-00-71-FF-F2-00-49-FF-AB-00-00-FF-8C-FF-BA-FF-A1-FF-93-FF-DB-FF-93-00-1F-FF-B7-00-53-FF-EA-00-6D-00-12-00-70-00-17-00-67-FF-F3-00-53-FF-C0-00-2B-FF-A6-FF-EB-FF-BE-FF-A6-00-03-FF-83-00-50-FF-9E-00-75-FF-F0-00-5A-00-4A-00-0E-00-7A-FF-BF-00-67-FF-98-00-23-FF-A8-FF-D4-FF-DE-FF-9E-00-1D-FF-90-00-4E-FF-A4-00-6A-FF-CD-00-70-FF-FC-00-66-00-23-00-53-00-3C-00-41-00-43-00-37-00-36-00-33-00-13-00-2C-FF-E0-00-17-FF-A9-FF-EF-FF-85-FF-BD-FF-8B-FF-9A-FF-C2-FF-A2-00-16-FF-DB-00-5B-00-2C-00-66-00-65-00-33-00-67-FF-E2-00-3E-FF-A4-00-0E-FF-8F-FF-F7-FF-9B-FF-FE-FF-B1-00-14-FF-C1-00-2C-FF-CD-00-45-FF-E4-00-55-00-10-00-4C-00-49-00-19-00-76-FF-CB-00-73-FF-8D-00-34-FF-8A-FF-D2-FF-C8-FF-86-00-23-FF-7D-00-65-FF-B9-00-6B-00-15-00-38-00-5B-FF-ED-00-6C-FF-B1-00-49-FF-99-00-0C-FF-A3-FF-D2-FF-BC-FF-AD-FF-D1-FF-9E-FF-D3-FF-A4-FF-C6-FF-C6-FF-BE-00-06-FF-D4-00-52-00-11-00-7E-00-53-00-66-00-68-00-14-00-36-FF-C0-FF-D9-FF-A4-FF-92-FF-CE-FF-8C-00-1A-FF-BD-00-54-FF-FC-00-68-00-24-00-60-00-2D-00-53-00-21-00-4E-00-09-00-4C-FF-E6-00-45-FF-BF-00-2D-FF-A5-FF-FF-FF-AA-FF-C2-FF-DA-FF-8F-00-25-FF-86-00-6A-FF-B7-00-81-00-11-00-5B-00-66-00-0B-00-88-FF-BC-00-67-FF-95-00-17-FF-A1-FF-C3-FF-CE-FF-8D-00-02-FF-7F-00-2B-FF-8F-00-41-FF-A6-00-43-FF-B6-00-2A-FF-BC-FF-F5-FF-C5-FF-B6-FF-E6-FF-91-00-21-FF-A6-00-5E-FF-F0-00-70-00-3F-00-3E-00-5C-FF-E1-00-32-FF-94-FF-E3-FF-88-FF-A3-FF-C1-FF-95-00-15-FF-B4-00-56-FF-E5-00-6F-00-0F-00-69-00-2A-00-58-00-38-00-47-00-3F-00-3C-00-40-00-33-00-3F-00-2C-00-3D-00-29-00-35-00-33-00-1B-00-48-FF-E6-00-54-FF-A8-00-3C-FF-8C-FF-F8-FF-B6-FF-AB-00-17-FF-90-00-6C-FF-C4-00-72-00-1D-00-28-00-50-FF-D1-00-35-FF-B1-FF-E9-FF-D7-FF-AF-00-18-FF-AB-00-48-FF-CF-00-59-FF-FA-00-59-00-16-00-56-00-21-00-51-00-1A-00-43-FF-FD-00-28-FF-D1-FF-FE-FF-A9-FF-C8-FF-9E-FF-9A-FF-BC-FF-8A-FF-F8-FF-A6-00-36-FF-E3-00-5B-00-26-00-5E-00-57-00-46-00-6D-00-23-00-69-FF-FF-00-55-FF-E1-00-3D-FF-CA-00-2E-FF-BE-00-33-FF-C7-00-44-FF-ED-00-48-00-2B-00-24-00-67-FF-DD-00-77-FF-9D-00-45-FF-95-FF-E8-FF-D4-FF-9C-00-30-FF-94-00-69-FF-CF-00-5F-00-1F-00-24-00-52-FF-E3-00-5A-FF-B8-00-4D-FF-A8-00-41-FF-AB-00-3F-FF-C0-00-40-FF-E8-00-41-00-1D-00-3B-00-50-00-27-00-6D-00-02-00-67-FF-D2-00-40-FF-AC-00-03-FF-A2-FF-C7-FF-B9-FF-A1-FF-E2-FF-95-00-0A-FF-9F-00-25-FF-B2-00-31-FF-C5-00-32-FF-D3-00-2F-FF-D9-00-2A-FF-D0-00-23-FF-BC-00-0E-FF-AC-FF-E3-FF-BD-FF-AB-FF-FB-FF-8C-00-49-FF-AB-00-6D-00-06-00-43-00-60-FF-E8-00-75-FF-A8-00-37-FF-B6-FF-DF-00-01-FF-B6-00-46-FF-D3-00-58-00-12-00-3E-00-43-00-1C-00-57-00-0C-00-5B-00-11-00-5A-00-23-00-4C-00-3C-00-25-00-4E-FF-E9-00-45-FF-AC-00-1B-FF-88-FF-DF-FF-8D-FF-B0-FF-B8-FF-A1-FF-F6-FF-B2-00-30-FF-CF-00-56-FF-E6-00-62-FF-F2-00-5C-FF-F1-00-4F-FF-E5-00-40-FF-CF-00-2E-FF-B3-00-10-FF-A4-FF-E2-FF-B6-FF-AC-FF-F3-FF-8D-00-41-FF-A6-00-6F-FF-F7-00-5A-00-54-00-0C-00-83-FF-BA-00-66-FF-99-00-14-FF-B4-FF-C2-FF-F1-FF-97-00-2A-FF-99-00-50-FF-B5-00-63-FF-D0-00-66-FF-DA-00-4E-FF-D1-00-16-FF-C2-FF-CC-FF-C6-FF-93-FF-EE-FF-8C-00-2C-FF-BE-00-5B-00-0D-00-5A-00-4F-00-2C-00-6D-FF-F0-00-68-FF-C9-00-55-FF-C3-00-46-FF-D3-00-41-FF-E9-00-44-00-05-00-41-00-2B-00-25-00-56-FF-EA-00-6A-FF-A8-00-4D-FF-8A-00-00-FF-B0-FF-AB-00-09-FF-87-00-5F-FF-AA-00-7C-FF-FB-00-54-00-45-00-07-00-63-FF-BF-00-55-FF-95-00-32-FF-89-00-0E-FF-92-FF-F0-FF-A1-FF-DA-FF-B0-FF-D7-FF-BD-FF-F0-FF-D4-00-20-FF-FE-00-51-00-38-00-62-00-67-00-42-00-6F-00-01-00-49-FF-C3-00-09-FF-A5-FF-CC-FF-AA-FF-AB-FF-BB-FF-B0-FF-C0-FF-DB-FF-B7-00-1E-FF-B9-00-5B-FF-DD-00-6F-00-1F-00-4C-00-5F-00-02-00-78-FF-B7-00-59-FF-8D-00-13-FF-93-FF-C5-FF-BE-FF-8E-FF-F8-FF-7A-00-29-FF-84-00-40-FF-9B-00-37-FF-B0-00-14-FF-C4-FF-E1-FF-DD-FF-B2-00-03-FF-9B-00-33-FF-AB-00-5C-FF-DC-00-6D-00-1A-00-5C-00-4B-00-32-00-62-00-00-00-5F-FF-D4-00-4C-FF-B7-00-2C-FF-AA-00-07-FF-AA-FF-E4-FF-B2-FF-D4-FF-B9-FF-E3-FF-B8-00-12-FF-BB-00-4C-FF-D4-00-70-00-0D-00-5D-00-52-00-15-00-75-FF-BF-00-56-FF-93-00-01-FF-AC-FF-AB-FF-F6-FF-88-00-3E-FF-A7-00-60-FF-EC-00-57-00-31-00-33-00-5C-00-06-00-6C-FF-DD-00-6B-FF-C8-00-62-FF-D0-00-55-FF-F5-00-43-00-27-00-28-00-51-00-02-00-65-FF-D6-00-5A-FF-AF-00-34-FF-99-FF-F8-FF-A1-FF-BB-FF-CD-FF-9A-00-15-FF-AF-00-5C-FF-F3-00-7A-00-42-00-58-00-6A-00-06-00-52-FF-B3-00-0A-FF-8E-FF-C0-FF-9E-FF-9A-FF-C8-FF-99-FF-E8-FF-A8-FF-ED-FF-B7-FF-DD-FF-CA-FF-C2-FF-F1-FF-AE-00-2C-FF-B4-00-61-FF-E1-00-6D-00-2B-00-3E-00-6A-FF-EA-00-77-FF-9E-00-49-FF-7E-FF-FD-FF-91-FF-BA-FF-C4-FF-97-FF-FC-FF-8F-00-20-FF-95-00-24-FF-A2-00-07-FF-BA-FF-DB-FF-E5-FF-B8-00-20-FF-B7-00-5B-FF-DE-00-7E-00-1C-00-73-00-50-00-39-00-5D-FF-E7-00-3B-FF-A1-FF-FD-FF-87-FF-C1-FF-A0-FF-9E-FF-D6-FF-9A-00-0E-FF-A8-00-31-FF-B9-00-3D-FF-C2-00-38-FF-BE-00-24-FF-B8-00-00-FF-C3-FF-CD-FF-EF-FF-A1-00-32-FF-99-00-6B-FF-C6-00-73-00-15-00-42-00-5F-FF-F5-00-81-FF-B4-00-75-FF-93-00-4F-FF-8C-00-26-FF-92-00-0C-FF-A4-00-0D-FF-D1-00-24-00-1A-00-38-00-65-00-27-00-82-FF-EC-00-56-FF-AF-FF-F9-FF-A7-FF-AC-FF-E4-FF-A3-00-3D-FF-DB-00-76-00-2A-00-6C-00-5F-00-31-00-6A-FF-EC-00-56-FF-BB-00-38-FF-A5-00-20-FF-A4-00-13-FF-AD-00-14-FF-B7-00-24-FF-C2-00-3D-FF-D6-00-52-FF-FB-00-4E-00-34-00-21-00-66-FF-D7-00-6C-FF-98-00-35-FF-91-FF-D8-FF-CA-FF-8F-00-1E-FF-85-00-5B-FF-BA-00-68-00-06-00-51-00-42-00-30-00-63-00-1A-00-70-00-17-00-70-00-28-00-58-00-3E-00-1C-00-3D-FF-CB-00-10-FF-8F-FF-CB-FF-91-FF-9E-FF-D6-FF-B1-00-31-FF-FB-00-68-00-4B-00-62-00-75-00-2F-00-71-FF-F6-00-56-FF-CF-00-3D-FF-BC-00-31-FF-B8-00-32-FF-BF-00-3B-FF-D4-00-45-FF-F7-00-44-00-23-00-2F-00-4D-00-03-00-67-FF-CC-00-64-FF-9E-00-3F-FF-8C-FF-FE-FF-9F-FF-B7-FF-D1-FF-83-00-11-FF-74-00-49-FF-8B-00-6A-FF-BA-00-73-FF-EA-00-6A-00-07-00-53-00-09-00-30-FF-F4-00-03-FF-D5-FF-CE-FF-BE-FF-9C-FF-C0-FF-81-FF-E2-FF-8D-00-1A-FF-C2-00-4B-00-0C-00-5D-00-4B-00-49-00-6A-00-25-00-68-00-0F-00-51-00-14-00-32-00-2D-00-0E-00-49-FF-E2-00-55-FF-B4-00-48-FF-92-00-1C-FF-95-FF-DC-FF-C9-FF-A2-00-1E-FF-92-00-6B-FF-BB-00-8A-00-09-00-6E-00-53-00-2A-00-79-FF-DE-00-72-FF-A3-00-4B-FF-85-00-13-FF-86-FF-DC-FF-9E-FF-AF-FF-C5-FF-96-FF-EA-FF-92-00-04-FF-9B-00-0E-FF-AA-00-08-FF-BC-FF-F3-FF-CF-FF-D3-FF-E5-FF-B4-00-00-FF-9F-00-1B-FF-9D-00-31-FF-A9-00-3D-FF-BC-00-3F-FF-CB-00-3C-FF-CE-00-36-FF-C4-00-29-FF-B5-00-0C-FF-B3-FF-DF-FF-CD-FF-B2-00-02-FF-9B-00-40-FF-A8-00-6E-FF-D7-00-7B-00-18-00-64-00-56-00-2F-00-7C-FF-ED-00-80-FF-B3-00-61-FF-91-00-31-FF-8A-00-08-FF-99-FF-F4-FF-B0-FF-F6-FF-C8-00-02-FF-DA-00-0F-FF-E5-00-1A-FF-E7-00-26-FF-E6-00-34-FF-E5-00-41-FF-EA-00-4C-FF-F6-00-53-00-0B-00-55-00-29-00-4C-00-4A-00-31-00-5D-00-05-00-55-FF-D1-00-2E-FF-A9-FF-F5-FF-9E-FF-BF-FF-B2-FF-A0-FF-DD-FF-9C-00-0C-FF-AF-00-32-FF-CA-00-48-FF-E3-00-4E-FF-F4-00-4B-FF-FC-00-45-00-01-00-3F-00-05-00-3C-00-04-00-3C-FF-F8-00-3D-FF-E0-00-3A-FF-C1-00-2F-FF-A4-00-15-FF-99-FF-E9-FF-B3-FF-B2-FF-F9-FF-8F-00-51-FF-A3-00-83-FF-F1-00-65-00-4A-00-0B-00-68-FF-C0-00-33-FF-C0-FF-E1-00-01-FF-BA-00-42-FF-D9-00-52-00-19-00-37-00-46-00-11-00-53-FF-F5-00-51-FF-E6-00-52-FF-E5-00-51-FF-FC-00-45-00-27-00-27-00-50-FF-FA-00-5B-FF-C6-00-41-FF-9C-00-0F-FF-8C-FF-DD-FF-9E-FF-BB-FF-CA-FF-AC-FF-F9-FF-AB-00-1C-FF-B0-00-2C-FF-B6-00-2E-FF-BB-00-28-FF-BE-00-1F-FF-BE-00-12-FF-BD-00-00-FF-BA-FF-EE-FF-B7-FF-E2-FF-B4-FF-E2-FF-B3-FF-F1-FF-B4-00-09-FF-B7-00-1D-FF-BB-00-1F-FF-BD-00-12-FF-BC-00-02-FF-B9-00-04-FF-BF-00-21-FF-DD-00-49-00-13-00-61-00-4E-00-51-00-70-00-1B-00-65-FF-D9-00-31-FF-A9-FF-EF-FF-9D-FF-BA-FF-B2-FF-A3-FF-D7-FF-A4-FF-F8-FF-AD-00-05-FF-B6-FF-F5-FF-C9-FF-CD-FF-F6-FF-A4-00-3B-FF-A1-00-72-FF-D6-00-6F-00-2C-00-27-00-68-FF-C6-00-5C-FF-8D-00-12-FF-9E-FF-BF-FF-E2-FF-99-00-2B-FF-AE-00-57-FF-E3-00-64-00-16-00-60-00-37-00-57-00-46-00-48-00-48-00-36-00-42-00-27-00-38-00-20-00-2F-00-27-00-24-00-37-00-0F-00-47-FF-EC-00-48-FF-C0-00-31-FF-9B-00-04-FF-90-FF-CF-FF-A8-FF-A8-FF-DD-FF-A0-00-1D-FF-BE-00-50-FF-F6-00-65-00-31-00-58-00-5A-00-33-00-67-00-0C-00-5D-FF-F1-00-4C-FF-E7-00-41-FF-ED-00-3D-00-02-00-35-00-25-00-1C-00-4C-FF-EE-00-65-FF-B7-00-5B-FF-90-00-2A-FF-91-FF-E0-FF-C2-FF-9D-00-11-FF-81-00-5D-FF-99-00-86-FF-D8-00-7F-00-26-00-4D-00-65-00-04-00-80-FF-BC-00-71-FF-8C-00-3E-FF-7F-FF-FB-FF-91-FF-BE-FF-B3-FF-99-FF-D2-FF-90-FF-E3-FF-9D-FF-E7-FF-B7-FF-E0-FF-D5-FF-D2-FF-F3-FF-C4-00-0B-FF-B9-00-1B-FF-B4-00-25-FF-AC-00-22-FF-A3-00-09-FF-AB-FF-D5-FF-DA-FF-A0-00-2B-FF-99-00-6B-FF-D9-00-5E-00-39-00-0A-00-65-FF-BE-00-39-FF-C4-FF-EB-00-0A-FF-D4-00-34-00-0B-00-0E-00-45-FF-C3-00-36-FF-A8-FF-E5-FF-D8-FF-9E-00-26-FF-92-00-59-FF-B3-00-67-FF-D7-00-66-FF-EA-00-65-FF-F9-00-5C-00-08-00-4A-00-0E-00-38-00-00-00-2C-FF-E3-00-21-FF-C2-00-0A-FF-AA-FF-E5-FF-A1-FF-BE-FF-AB-FF-A4-FF-C8-FF-9D-FF-F3-FF-AA-00-1F-FF-C1-00-3E-FF-DC-00-4D-FF-F0-00-50-FF-F3-00-4E-FF-E0-00-47-FF-BC-00-2E-FF-A2-FF-F9-FF-B4-FF-B8-FF-F9-FF-97-00-4A-FF-BA-00-67-00-14-00-36-00-63-FF-E0-00-6B-FF-AC-00-2A-FF-BE-FF-D9-FF-FF-FF-B0-00-3E-FF-B9-00-5F-FF-DC-00-67-FF-FF-00-63-00-19-00-5B-00-2C-00-4F-00-31-00-43-00-24-00-3A-00-08-00-36-FF-E7-00-31-FF-C8-00-25-FF-AE-00-12-FF-9C-FF-FE-FF-94-FF-EE-FF-99-FF-E5-FF-A8-FF-E5-FF-B9-FF-EB-FF-C2-FF-F9-FF-C0-00-16-FF-B9-00-40-FF-C3-00-64-FF-F0-00-5C-00-38-00-18-00-70-FF-B9-00-63-FF-84-00-12-FF-A5-FF-BA-FF-FE-FF-A3-00-44-FF-DA-00-45-00-2D-00-0F-00-5D-FF-D3-00-58-FF-B1-00-38-FF-A6-00-1B-FF-A4-00-0C-FF-A8-00-0A-FF-B8-00-14-FF-D5-00-29-FF-F8-00-44-00-1B-00-55-00-3A-00-55-00-52-00-48-00-5D-00-33-00-5C-00-1F-00-52-00-0E-00-46-00-00-00-40-FF-F8-00-41-FF-F5-00-45-FF-F8-00-4A-00-00-00-49-00-0B-00-45-00-16-00-41-00-20-00-39-00-31-00-21-00-4A-FF-EC-00-55-FF-AA-00-34-FF-8C-FF-E3-FF-B9-FF-93-00-17-FF-88-00-56-FF-D0-00-3E-00-27-FF-F0-00-3A-FF-C6-00-00-FF-EC-FF-C5-00-35-FF-CE-00-54-00-14-00-32-00-58-FF-F6-00-6E-FF-CD-00-61-FF-BD-00-4F-FF-B6-00-43-FF-B2-00-38-FF-BB-00-2F-FF-D8-00-2F-00-01-00-38-00-29-00-3F-00-48-00-38-00-5B-00-27-00-61-00-1C-00-5A-00-1F-00-4A-00-2B-00-38-00-39-00-28-00-44-00-15-00-51-FF-F3-00-58-FF-C3-00-49-FF-9B-00-13-FF-A1-FF-C2-FF-E3-FF-86-00-40-FF-90-00-74-FF-DE-00-54-00-36-FF-FD-00-52-FF-B8-00-21-FF-B8-FF-D5-FF-F0-FF-AB-00-2E-FF-B9-00-50-FF-E6-00-57-00-11-00-56-00-25-00-55-00-21-00-4A-00-08-00-2F-FF-E1-00-0A-FF-BB-FF-E7-FF-A4-FF-CB-FF-A2-FF-B7-FF-B0-FF-AA-FF-C9-FF-A8-FF-E8-FF-B1-00-07-FF-BE-00-1F-FF-C6-00-24-FF-C5-00-14-FF-C1-FF-F0-FF-C9-FF-C4-FF-EA-FF-9F-00-24-FF-98-00-60-FF-C0-00-77-00-14-00-4C-00-6B-FF-EE-00-8B-FF-9A-00-56-FF-93-FF-F3-FF-DB-FF-B2-00-35-FF-C4-00-53-00-10-00-24-00-4D-FF-D8-00-47-FF-AE-00-09-FF-BC-FF-C5-FF-EF-FF-A2-00-23-FF-A2-00-44-FF-AD-00-4A-FF-B3-00-32-FF-B3-00-02-FF-BB-FF-C9-FF-D5-FF-A0-00-03-FF-9A-00-39-FF-BC-00-63-FF-F6-00-6E-00-31-00-53-00-56-00-1B-00-5C-FF-DC-00-4A-FF-AF-00-30-FF-9E-00-1D-FF-A3-00-17-FF-B3-00-1A-FF-C3-00-21-FF-CE-00-2B-FF-D6-00-37-FF-DD-00-48-FF-EA-00-56-00-05-00-54-00-2E-00-33-00-56-FF-F2-00-64-FF-AA-00-43-FF-83-FF-FA-FF-96-FF-AA-FF-DE-FF-7F-00-33-FF-90-00-6C-FF-D1-00-74-00-1E-00-55-00-59-00-26-00-75-FF-F8-00-78-FF-D7-00-6E-FF-C7-00-5E-FF-CE-00-4E-FF-E9-00-3F-00-12-00-30-00-3A-00-21-00-57-00-13-00-64-00-09-00-63-00-07-00-5C-00-0B-00-53-00-18-00-46-00-2E-00-2A-00-4A-FF-F9-00-5B-FF-BC-00-4B-FF-91-00-10-FF-9A-FF-BC-FF-DF-FF-7F-00-3F-FF-85-00-7B-FF-D3-00-66-00-34-00-0F-00-60-FF-BB-00-38-FF-AD-FF-E5-FF-E7-FF-B2-00-30-FF-C8-00-47-00-0F-00-20-00-4B-FF-DF-00-56-FF-AE-00-3A-FF-9A-00-15-FF-99-FF-FA-FF-9E-FF-EC-FF-A4-FF-EC-FF-B2-00-00-FF-CC-00-25-FF-F5-00-4B-00-2B-00-5B-00-5E-00-46-00-78-00-14-00-6A-FF-DB-00-36-FF-B3-FF-F4-FF-A7-FF-BC-FF-B6-FF-9E-FF-D3-FF-9A-FF-EF-FF-A6-FF-FF-FF-B5-FF-FF-FF-C1-FF-F3-FF-C9-FF-E1-FF-D0-FF-D1-FF-D7-FF-C4-FF-E1-FF-B8-FF-F3-FF-AC-00-13-FF-A8-00-3B-FF-BA-00-5B-FF-EC-00-59-00-34-00-2A-00-73-FF-DB-00-83-FF-96-00-56-FF-7F-00-00-FF-A4-FF-AE-FF-EE-FF-86-00-38-FF-92-00-68-FF-C1-00-7A-FF-F8-00-77-00-21-00-6A-00-33-00-5A-00-2F-00-48-00-19-00-37-FF-F8-00-2A-FF-D8-00-23-FF-C0-00-20-FF-B3-00-21-FF-B0-00-27-FF-B3-00-35-FF-BD-00-4A-FF-D4-00-5A-00-00-00-50-00-3A-00-1B-00-69-FF-C9-00-68-FF-86-00-28-FF-85-FF-C5-FF-CF-FF-80-00-33-FF-8D-00-67-FF-E4-00-44-00-3D-FF-EF-00-53-FF-B6-00-1C-FF-C8-FF-D3-00-0F-FF-BB-00-4C-FF-E5-00-51-00-2B-00-25-00-5B-FF-EB-00-63-FF-BE-00-4F-FF-A4-00-32-FF-9C-00-19-FF-A0-00-0A-FF-AE-00-09-FF-C4-00-18-FF-E0-00-30-00-03-00-46-00-2C-00-4C-00-52-00-40-00-69-00-28-00-6B-00-12-00-5D-00-09-00-49-00-10-00-38-00-22-00-28-00-3B-00-0F-00-53-FF-E5-00-5E-FF-B0-00-48-FF-8F-00-06-FF-A5-FF-B0-FF-F7-FF-7B-00-58-FF-94-00-82-FF-F0-00-52-00-49-FF-F0-00-58-FF-AE-00-17-FF-BF-FF-C7-00-0B-FF-B1-00-47-FF-E5-00-40-00-34-00-02-00-5C-FF-BE-00-49-FF-9B-00-10-FF-9F-FF-D6-FF-B7-FF-B0-FF-D3-FF-A0-FF-E5-FF-9F-FF-E7-FF-AB-FF-D7-FF-C7-FF-C1-FF-F5-FF-BA-00-2E-FF-D4-00-61-00-0A-00-76-00-42-00-62-00-5F-00-2E-00-5B-FF-F5-00-47-FF-CD-00-36-FF-BC-00-2F-FF-BB-00-2B-FF-BF-00-25-FF-C4-00-1F-FF-C7-00-24-FF-CD-00-37-FF-DB-00-50-FF-FA-00-58-00-2B-00-3A-00-5C-FF-F5-00-6F-FF-A8-00-47-FF-84-FF-EE-FF-AB-FF-95-00-06-FF-7C-00-54-FF-B7-00-58-00-19-00-16-00-56-FF-C9-00-45-FF-AB-00-00-FF-C7-FF-C0-FF-FB-FF-A6-00-26-FF-AD-00-3D-FF-BE-00-48-FF-CA-00-47-FF-CD-00-31-FF-C9-00-01-FF-C7-FF-C7-FF-D3-FF-9B-FF-F6-FF-91-00-28-FF-A8-00-50-FF-CF-00-60-FF-F7-00-58-00-18-00-47-00-34-00-36-00-4C-00-23-00-5A-00-0B-00-5B-FF-F3-00-53-FF-E7-00-47-FF-EC-00-40-00-00-00-3F-00-1B-00-3E-00-3A-00-30-00-5C-00-06-00-72-FF-C6-00-60-FF-98-00-18-FF-AA-FF-B7-FF-FE-FF-85-00-52-FF-AA-00-5F-00-06-00-1B-00-41-FF-CB-00-2B-FF-BA-FF-E1-FF-EF-FF-AF-00-35-FF-B8-00-59-FF-E7-00-59-00-14-00-4E-00-2D-00-48-00-36-00-47-00-30-00-3E-00-14-00-2C-FF-E6-00-11-FF-B8-FF-F2-FF-9D-FF-D3-FF-9B-FF-B9-FF-AB-FF-A8-FF-C8-FF-A7-FF-F1-FF-B9-00-22-FF-DF-00-4D-00-07-00-65-00-1C-00-68-00-11-00-5A-FF-EE-00-43-FF-C9-00-2C-FF-AF-00-19-FF-9D-00-06-FF-97-FF-E5-FF-AF-FF-B8-FF-F4-FF-9B-00-4B-FF-B6-00-79-00-0A-00-55-00-61-FF-FA-00-78-FF-B0-00-3E-FF-AA-FF-E3-FF-E0-FF-A9-00-22-FF-A4-00-4E-FF-C3-00-60-FF-E8-00-65-00-07-00-64-00-1D-00-5B-00-25-00-4D-00-16-00-3A-FF-F4-00-24-FF-CC-00-0D-FF-AE-FF-F7-FF-9D-FF-E5-FF-96-FF-D2-FF-9A-FF-BC-FF-B3-FF-AE-FF-E8-FF-B9-00-2D-FF-E7-00-64-00-2A-00-6C-00-60-00-3D-00-6B-FF-F0-00-46-FF-AB-00-04-FF-8F-FF-C1-FF-A2-FF-95-FF-D6-FF-8B-00-0E-FF-9B-00-34-FF-B3-00-43-FF-C2-00-3F-FF-C1-00-2B-FF-BA-00-05-FF-C4-FF-D1-FF-EF-FF-A2-00-33-FF-97-00-6F-FF-C2-00-7A-00-15-00-47-00-62-FF-EF-00-7C-FF-A3-00-59-FF-87-00-12-FF-98-FF-D0-FF-BC-FF-A8-FF-D8-FF-97-FF-EA-FF-91-00-00-FF-94-00-20-FF-A4-00-42-FF-BD-00-52-FF-D2-00-42-FF-D7-00-11-FF-D2-FF-D0-FF-D8-FF-9A-FF-F6-FF-88-00-24-FF-A1-00-4E-FF-DA-00-5F-00-1A-00-53-00-4B-00-32-00-62-00-0A-00-60-FF-E9-00-53-FF-D8-00-47-FF-D8-00-43-FF-E5-00-45-FF-F9-00-48-00-13-00-40-00-32-00-2A-00-4F-00-09-00-61-FF-E7-00-5E-FF-CD-00-4A-FF-C2-00-32-FF-C7-00-26-FF-DE-00-2A-00-0B-00-32-00-46-00-27-00-74-FF-FB-00-6E-FF-BB-00-2A-FF-93-FF-CE-FF-AD-FF-9D-00-02-FF-BD-00-59-00-15-00-6D-00-5B-00-2F-00-55-FF-D0-00-0C-FF-98-FF-B8-FF-A9-FF-90-FF-ED-FF-A0-00-35-FF-D2-00-5F-00-07-00-64-00-2C-00-53-00-3D-00-3D-00-3E-00-2F-00-34-00-2F-00-22-00-3B-00-08-00-49-FF-E8-00-4D-FF-C5-00-3E-FF-A9-00-1C-FF-9E-FF-EF-FF-A9-FF-C5-FF-C6-FF-AC-FF-ED-FF-A6-00-0E-FF-AC-00-1B-FF-B3-00-08-FF-BC-FF-DB-FF-D8-FF-B2-00-11-FF-B5-00-59-FF-F0-00-7F-00-3B-00-5C-00-57-FF-FD-00-23-FF-A7-FF-C7-FF-9E-FF-91-FF-E7-FF-B2-00-40-00-0C-00-60-00-5A-00-31-00-66-FF-DD-00-34-FF-A0-FF-ED-FF-96-FF-B8-FF-B5-FF-A2-FF-E3-FF-A6-00-0B-FF-B2-00-20-FF-BB-00-1F-FF-BD-00-07-FF-C2-FF-E0-FF-D5-FF-B9-FF-FC-FF-A2-00-30-FF-A9-00-5D-FF-CD-00-72-00-02-00-67-00-34-00-41-00-57-00-10-00-68-FF-EE-00-6F-FF-F0-00-6A-00-17-00-4A-00-41-00-05-00-41-FF-AE-00-09-FF-7B-FF-C1-FF-97-FF-A8-FF-F3-FF-DD-00-4B-00-39-00-5B-00-74-00-1E-00-64-FF-C9-00-1D-FF-99-FF-D3-FF-9F-FF-AA-FF-C1-FF-A4-FF-DB-FF-B0-FF-E0-FF-C2-FF-D5-FF-D6-FF-C5-FF-EB-FF-B8-00-03-FF-B1-00-21-FF-B5-00-44-FF-CC-00-5D-FF-FB-00-59-00-34-00-2F-00-62-FF-EE-00-71-FF-B2-00-63-FF-93-00-49-FF-94-00-34-FF-A7-00-25-FF-BB-00-1A-FF-CB-00-19-FF-E2-00-2B-00-10-00-44-00-4F-00-43-00-78-00-12-00-63-FF-CB-00-10-FF-A9-FF-BB-FF-D4-FF-A7-00-2D-FF-E1-00-6D-00-38-00-64-00-6D-00-21-00-68-FF-D5-00-41-FF-A8-00-18-FF-9D-00-03-FF-A1-00-05-FF-A9-00-18-FF-B4-00-33-FF-CA-00-4A-FF-EA-00-51-00-0F-00-46-00-34-00-2C-00-53-00-06-00-65-FF-D7-00-60-FF-A9-00-3D-FF-8D-FF-FE-FF-94-FF-B7-FF-BD-FF-84-FF-F9-FF-77-00-33-FF-8E-00-60-FF-BA-00-78-FF-EB-00-7A-00-17-00-69-00-38-00-4F-00-48-00-3C-00-40-00-33-00-1B-00-27-FF-E0-00-01-FF-A6-FF-C2-FF-94-FF-8F-FF-C1-FF-97-00-17-FF-E3-00-58-00-44-00-4F-00-73-00-01-00-4B-FF-AF-FF-EF-FF-9A-FF-A5-FF-D0-FF-9B-00-23-FF-CA-00-5D-00-06-00-6B-00-2F-00-5B-00-41-00-45-00-45-00-33-00-47-00-23-00-49-00-16-00-49-00-14-00-42-00-22-00-2D-00-3A-00-06-00-4D-FF-D4-00-4C-FF-A8-00-31-FF-94-00-06-FF-9F-FF-D6-FF-C3-FF-AE-FF-F2-FF-96-00-1F-FF-91-00-3F-FF-9C-00-49-FF-AD-00-37-FF-B9-00-09-FF-C3-FF-C8-FF-D9-FF-95-00-09-FF-91-00-4A-FF-C8-00-73-00-1B-00-5F-00-52-00-10-00-46-FF-B5-00-03-FF-87-FF-B8-FF-9B-FF-91-FF-D9-FF-96-00-1C-FF-B4-00-48-FF-D6-00-5C-FF-EF-00-5F-00-00-00-5C-00-0E-00-57-00-21-00-50-00-3A-00-40-00-52-00-25-00-61-00-07-00-60-FF-F4-00-53-FF-F8-00-44-00-0E-00-3E-00-23-00-40-00-26-00-46-00-10-00-47-FF-EB-00-41-FF-C8-00-39-FF-B3-00-35-FF-AF-00-37-FF-B6-00-39-FF-BE-00-34-FF-C0-00-25-FF-BB-00-0F-FF-B4-FF-F6-FF-B0-FF-E3-FF-B0-FF-DA-FF-B1-FF-DD-FF-B1-FF-E9-FF-B2-FF-FA-FF-B2-00-09-FF-B4-00-11-FF-B6-00-11-FF-BA-00-0E-FF-C4-00-0E-FF-CF-00-13-FF-D2-00-0E-FF-C5-FF-ED-FF-B6-FF-B7-FF-C4-FF-91-FF-FF-FF-A9-00-4C-FF-FC-00-6C-00-4E-00-3C-00-5A-FF-DC-00-14-FF-9C-FF-B9-FF-B2-FF-98-00-0A-FF-C8-00-5A-00-1E-00-69-00-5F-00-38-00-71-FF-F4-00-64-FF-CF-00-56-FF-D8-00-4D-00-00-00-44-00-2C-00-33-00-4C-00-1D-00-5B-00-01-00-5D-FF-E0-00-54-FF-BA-00-3B-FF-9E-00-11-FF-9B-FF-DD-FF-B4-FF-AF-FF-DB-FF-94-FF-FD-FF-90-00-0C-FF-9D-00-08-FF-B0-FF-F3-FF-C5-FF-CF-FF-E2-FF-AA-00-0E-FF-A1-00-4A-FF-C8-00-78-00-14-00-72-00-4F-00-2C-00-47-FF-CD-FF-FE-FF-98-FF-AC-FF-B3-FF-8C-00-04-FF-AA-00-50-FF-E3-00-70-00-11-00-66-00-28-00-52-00-2C-00-4A-00-1F-00-4F-FF-FB-00-4E-FF-CA-00-32-FF-AD-FF-F7-FF-BC-FF-B5-FF-F6-FF-8E-00-3A-FF-94-00-6B-FF-BF-00-7C-FF-F9-00-74-00-30-00-57-00-5D-00-2A-00-77-FF-F3-00-77-FF-C5-00-61-FF-AE-00-41-FF-B3-00-29-FF-CB-00-20-FF-EE-00-23-00-17-00-2B-00-47-00-2B-00-73-00-14-00-7C-FF-E0-00-4B-FF-AA-FF-EF-FF-A1-FF-A5-FF-DC-FF-A7-00-35-FF-F4-00-64-00-4B-00-41-00-65-FF-EA-00-36-FF-A2-FF-E9-FF-93-FF-B1-FF-B6-FF-9F-FF-E5-FF-A2-00-06-FF-A5-00-09-FF-AA-FF-F0-FF-C2-FF-C7-FF-F3-FF-A6-00-33-FF-A9-00-64-FF-D9-00-70-00-20-00-51-00-5D-00-16-00-7B-FF-D3-00-74-FF-9C-00-51-FF-7E-00-21-FF-7B-FF-F2-FF-8E-FF-D2-FF-AA-FF-CB-FF-C3-FF-E0-FF-D9-00-0B-FF-F9-00-3F-00-29-00-64-00-5F-00-60-00-7C-00-2B-00-62-FF-DE-00-13-FF-AA-FF-BB-FF-B5-FF-8F-FF-F9-FF-AB-00-46-FF-F5-00-6C-00-3C-00-5F-00-61-00-36-00-65-00-13-00-5B-00-0C-00-48-00-20-00-24-00-40-FF-EB-00-4F-FF-AE-00-36-FF-8F-FF-F5-FF-A6-FF-AC-FF-E9-FF-85-00-37-FF-92-00-6E-FF-C4-00-83-FF-FB-00-7E-00-20-00-6F-00-2C-00-5B-00-23-00-42-00-07-00-22-FF-DE-FF-FC-FF-B6-FF-D5-FF-A0-FF-B3-FF-A4-FF-9E-FF-BF-FF-9B-FF-E9-FF-AB-00-17-FF-CB-00-3F-FF-F4-00-59-00-1D-00-5D-00-3C-00-4E-00-4C-00-36-00-4C-00-22-00-43-00-17-00-3B-00-17-00-36-00-23-00-2B-00-3E-00-0D-00-5C-FF-D7-00-65-FF-A0-00-3E-FF-94-FF-ED-FF-CC-FF-A2-00-2B-FF-94-00-71-FF-D0-00-6B-00-28-00-22-00-59-FF-D0-00-47-FF-A6-00-09-FF-AF-FF-CC-FF-D4-FF-A8-FF-FD-FF-9D-00-1B-FF-9C-00-2A-FF-A1-00-24-FF-AA-00-07-FF-B9-FF-DF-FF-CC-FF-BB-FF-E2-FF-A4-FF-F7-FF-99-00-0A-FF-98-00-19-FF-A0-00-27-FF-B6-00-37-FF-D9-00-45-00-02-00-4C-00-24-00-4D-00-34-00-4B-00-32-00-47-00-26-00-43-00-1B-00-41-00-13-00-46-00-03-00-53-FF-DD-00-58-FF-AE-00-3A-FF-9C-FF-F2-FF-CA-FF-A4-00-2A-FF-90-00-77-FF-CE-00-70-00-2E-00-1C-00-5D-FF-C8-00-33-FF-BD-FF-DD-FF-FB-FF-AC-00-41-FF-C6-00-54-00-0C-00-30-00-45-FF-F9-00-59-FF-D1-00-54-FF-C4-00-4D-FF-C7-00-48-FF-D4-00-42-FF-E4-00-39-FF-F5-00-34-00-06-00-36-00-1A-00-39-00-33-00-35-00-4E-00-24-00-61-00-0A-00-65-FF-EE-00-57-FF-D5-00-3C-FF-C3-00-1B-FF-BA-FF-FE-FF-B4-FF-F5-FF-AF-00-0B-FF-AF-00-3A-FF-C4-00-61-FF-FA-00-58-00-41-00-14-00-6D-FF-BC-00-55-FF-90-00-01-FF-B3-FF-AB-00-05-FF-92-00-47-FF-C2-00-4F-00-11-00-27-00-4C-FF-F3-00-5D-FF-CB-00-55-FF-B3-00-47-FF-A8-00-41-FF-B5-00-43-FF-E3-00-45-00-25-00-3A-00-5E-00-18-00-73-FF-E5-00-5E-FF-B6-00-2F-FF-9F-FF-F9-FF-A8-FF-CE-FF-C3-FF-B4-FF-E1-FF-A8-FF-F6-FF-A8-FF-FE-FF-AF-FF-FB-FF-B9-FF-F1-FF-C4-FF-E1-FF-D0-FF-D0-FF-DE-FF-BD-FF-F3-FF-AC-00-12-FF-A3-00-3A-FF-B0-00-5A-FF-DC-00-5E-00-22-00-36-00-65-FF-EE-00-83-FF-A6-00-69-FF-7F-00-26-FF-86-FF-D9-FF-AC-FF-A0-FF-DA-FF-88-FF-FF-FF-89-00-16-FF-96-00-17-FF-A9-FF-FF-FF-C3-FF-D7-FF-EB-FF-B7-00-25-FF-BC-00-61-FF-F0-00-80-00-35-00-66-00-60-00-18-00-50-FF-BD-00-0C-FF-89-FF-BD-FF-99-FF-91-FF-E0-FF-9C-00-33-FF-D3-00-69-00-17-00-70-00-4D-00-4E-00-66-00-1B-00-67-FF-EE-00-5B-FF-D2-00-4C-FF-C7-00-41-FF-C6-00-3E-FF-CB-00-42-FF-D8-00-48-FF-F2-00-47-00-1C-00-32-00-49-00-08-00-67-FF-D4-00-64-FF-AA-00-3F-FF-97-00-08-FF-9B-FF-D4-FF-AC-FF-B3-FF-BC-FF-A8-FF-C3-FF-B7-FF-C2-FF-E1-FF-CB-00-20-FF-EC-00-5F-00-27-00-7B-00-5F-00-5B-00-6D-00-0C-00-3C-FF-BC-FF-E4-FF-9F-FF-98-FF-C4-FF-82-00-10-FF-A9-00-54-FF-EF-00-71-00-32-00-64-00-5B-00-3C-00-69-00-0E-00-63-FF-E9-00-56-FF-D1-00-4B-FF-C8-00-43-FF-C9-00-40-FF-CE-00-3F-FF-D5-00-3F-FF-DF-00-40-FF-ED-00-44-FF-FE-00-48-00-0E-00-49-00-16-00-45-00-18-00-40-00-13-00-43-00-07-00-49-FF-E8-00-3E-FF-B7-00-0E-FF-8D-FF-C2-FF-94-FF-92-FF-DC-FF-AE-00-38-00-09-00-5A-00-52-00-23-00-44-FF-CC-FF-F0-FF-AF-FF-AB-FF-E9-FF-B6-00-41-00-03-00-6A-00-4F-00-4A-00-6A-00-09-00-59-FF-D7-00-3D-FF-C4-00-2D-FF-C4-00-29-FF-C9-00-2C-FF-D0-00-34-FF-DB-00-41-FF-EA-00-4B-FF-F9-00-4E-00-08-00-49-00-16-00-41-00-21-00-39-00-29-00-33-00-2E-00-2F-00-31-00-29-00-31-00-21-00-2D-00-17-00-2A-00-14-00-2B-00-20-00-2C-00-3F-00-1F-00-5C-FF-F5-00-55-FF-B6-00-17-FF-8C-FF-C4-FF-A3-FF-9C-FF-FA-FF-C9-00-50-00-25-00-5A-00-59-00-12-00-34-FF-C0-FF-DA-FF-B5-FF-A2-FF-FA-FF-BB-00-4E-00-0C-00-6A-00-52-00-45-00-69-00-07-00-5B-FF-DB-00-45-FF-CC-00-39-FF-D0-00-38-FF-DC-00-3D-FF-ED-00-44-00-02-00-47-00-19-00-42-00-2D-00-34-00-3C-00-23-00-44-00-17-00-48-00-13-00-46-00-11-00-3F-00-0D-00-36-00-09-00-32-00-12-00-33-00-30-00-2D-00-56-00-0B-00-5E-FF-CE-00-2F-FF-98-FF-DB-FF-9C-FF-A3-FF-E7-FF-BB-00-43-00-10-00-63-00-58-00-2C-00-50-FF-D1-00-03-FF-A2-FF-B0-FF-BE-FF-90-00-04-FF-A4-00-40-FF-C9-00-57-FF-DE-00-54-FF-DB-00-47-FF-CA-00-35-FF-B6-00-19-FF-AA-FF-F4-FF-AF-FF-CF-FF-C9-FF-B2-FF-F1-FF-A4-00-1B-FF-A4-00-3A-FF-B1-00-46-FF-C3-00-40-FF-D4-00-2E-FF-DF-00-15-FF-E1-FF-FB-FF-DA-FF-DF-FF-CE-FF-BC-FF-C8-FF-98-FF-DA-FF-8C-00-0C-FF-B2-00-48-00-06-00-61-00-59-00-35-00-6A-FF-DA-00-28-FF-99-FF-C9-FF-AB-FF-A1-00-03-FF-D1-00-52-00-2A-00-57-00-62-00-13-00-53-FF-C1-00-17-FF-9A-FF-DF-FF-A2-FF-C6-FF-BB-FF-CD-FF-C7-FF-EA-FF-C1-00-14-FF-BA-00-42-FF-C5-00-63-FF-EC-00-62-00-26-00-3C-00-5A-FF-FF-00-73-FF-C4-00-68-FF-9C-00-43-FF-8C-00-16-FF-8F-FF-F2-FF-9B-FF-DE-FF-AB-FF-DD-FF-BC-FF-F0-FF-CF-00-16-FF-EE-00-44-00-1D-00-66-00-54-00-63-00-79-00-31-00-6D-FF-E4-00-29-FF-A9-FF-CD-FF-A8-FF-91-FF-E6-FF-9B-00-3B-FF-E2-00-6F-00-38-00-64-00-6D-00-28-00-6F-FF-E3-00-4C-FF-B2-00-20-FF-9D-00-04-FF-99-00-05-FF-9E-00-22-FF-AF-00-47-FF-D7-00-58-00-15-00-3C-00-53-FF-FB-00-71-FF-B4-00-5B-FF-8D-00-1B-FF-94-FF-D1-FF-B6-FF-99-FF-D7-FF-82-FF-E5-FF-88-FF-E4-FF-9F-FF-D9-FF-C0-FF-C8-FF-E9-FF-B8-00-1B-FF-B9-00-52-FF-DD-00-7B-00-1D-00-7A-00-57-00-41-00-5E-FF-E7-00-28-FF-9E-FF-D5-FF-92-FF-98-FF-C8-FF-93-00-1B-FF-BE-00-5D-FF-FB-00-76-00-28-00-6D-00-3D-00-54-00-41-00-3E-00-3F-00-34-00-36-00-36-00-1E-00-40-FF-F3-00-43-FF-C0-00-2F-FF-9D-00-01-FF-A0-FF-C6-FF-CE-FF-98-00-13-FF-8D-00-52-FF-A9-00-77-FF-DF-00-83-00-14-00-7F-00-2B-00-6B-00-17-00-3D-FF-E4-FF-F1-FF-B8-FF-9E-FF-B8-FF-6F-FF-EC-FF-84-00-33-FF-D4-00-5C-00-31-00-4D-00-6B-00-10-00-6E-FF-CC-00-4A-FF-A1-00-1F-FF-97-00-05-FF-A2-00-02-FF-B0-00-13-FF-BD-00-2E-FF-D0-00-47-FF-F1-00-52-00-1E-00-47-00-4D-00-23-00-6E-FF-F1-00-74-FF-BF-00-5B-FF-9C-00-2B-FF-8E-FF-F3-FF-94-FF-C6-FF-A7-FF-AE-FF-BB-FF-AF-FF-CB-FF-C1-FF-D2-FF-E0-FF-D3-00-0B-FF-D9-00-3D-FF-F1-00-69-00-1D-00-7A-00-4D-00-5D-00-63-00-19-00-4C-FF-CD-00-0F-FF-9D-FF-CA-FF-9F-FF-9C-FF-CA-FF-91-00-01-FF-9F-00-2B-FF-B4-00-3C-FF-C4-00-3A-FF-CB-00-2F-FF-C9-00-1F-FF-C3-00-09-FF-C0-FF-EC-FF-C8-FF-CC-FF-D9-FF-B2-FF-EA-FF-A6-FF-ED-FF-AB-FF-DE-FF-C3-FF-C6-FF-EA-FF-B4-00-18-FF-B3-00-44-FF-C5-00-63-FF-E3-00-6E-00-05-00-65-00-23-00-50-00-39-00-38-00-43-00-24-00-46-00-1A-00-44-00-19-00-42-00-1F-00-3E-00-28-00-36-00-33-00-2D-00-3C-00-29-00-41-00-2E-00-3C-00-3B-00-2D-00-48-00-16-00-50-00-02-00-55-FF-F6-00-5A-FF-EB-00-5A-FF-D3-00-41-FF-AF-00-05-FF-98-FF-BA-FF-AE-FF-90-FF-F9-FF-AE-00-4F-00-08-00-6F-00-5D-00-3B-00-69-FF-D7-00-21-FF-94-FF-C1-FF-A7-FF-99-FF-FE-FF-C4-00-4F-00-1A-00-5C-00-58-00-26-00-5E-FF-DE-00-3E-FF-B3-00-1E-FF-AD-00-15-FF-B9-00-23-FF-C7-00-3C-FF-DA-00-52-FF-FC-00-52-00-2D-00-30-00-58-FF-F3-00-68-FF-B4-00-54-FF-8D-00-2A-FF-88-00-02-FF-9B-FF-ED-FF-B4-FF-EE-FF-CB-FF-FE-FF-E1-00-1B-FF-FF-00-3E-00-2B-00-58-00-5A-00-4E-00-71-00-19-00-54-FF-D0-00-06-FF-A6-FF-B0-FF-BE-FF-89-00-0A-FF-AD-00-57-00-01-00-73-00-51-00-4F-00-72-00-06-00-5D-FF-C0-00-27-FF-99-FF-EC-FF-97-FF-C2-FF-AC-FF-AE-FF-C6-FF-AD-FF-D8-FF-B4-FF-DC-FF-BE-FF-D7-FF-C8-FF-D1-FF-CF-FF-CC-FF-D7-FF-CA-FF-DF-FF-C7-FF-EB-FF-C1-FF-FA-FF-BA-00-08-FF-B6-00-0C-FF-B6-00-00-FF-BA-FF-E7-FF-C4-FF-CA-FF-D3-FF-B4-FF-E4-FF-AB-FF-F3-FF-AC-FF-F9-FF-B3-FF-F5-FF-B9-FF-E7-FF-C3-FF-D0-FF-DA-FF-B7-00-08-FF-A8-00-41-FF-B7-00-6C-FF-EA-00-6D-00-31-00-3D-00-6C-FF-F2-00-7D-FF-AE-00-63-FF-89-00-31-FF-84-00-03-FF-93-FF-E8-FF-A8-FF-E4-FF-BE-FF-F6-FF-D9-00-19-00-00-00-43-00-35-00-5C-00-69-00-4A-00-7C-00-0E-00-57-FF-C5-00-03-FF-A2-FF-AF-FF-BD-FF-8B-00-06-FF-AA-00-4F-FF-F4-00-6E-00-3E-00-5A-00-69-00-29-00-70-FF-F5-00-5E-FF-D2-00-46-FF-C1-00-35-FF-BD-00-34-FF-C2-00-42-FF-D8-00-4C-00-03-00-3B-00-3C-00-04-00-64-FF-BB-00-5C-FF-8D-00-1B-FF-9E-FF-C5-FF-E7-FF-8C-00-3D-FF-92-00-70-FF-CA-00-6F-00-10-00-4E-00-46-00-27-00-64-00-0D-00-6F-00-04-00-6C-00-0C-00-59-00-22-00-36-00-3C-00-04-00-48-FF-CC-00-36-FF-9B-00-06-FF-86-FF-CA-FF-9D-FF-A2-FF-DE-FF-A7-00-31-FF-D9-00-6E-00-22-00-75-00-5C-00-45-00-6D-FF-F7-00-51-FF-B2-00-1C-FF-8F-FF-E9-FF-8F-FF-C8-FF-A7-FF-B7-FF-C4-FF-B1-FF-D6-FF-B5-FF-D5-FF-CE-FF-C4-00-01-FF-B6-00-41-FF-C8-00-6A-00-02-00-5A-00-4A-00-13-00-71-FF-BD-00-59-FF-8A-00-10-FF-90-FF-C2-FF-BF-FF-94-FF-FA-FF-8E-00-2C-FF-A1-00-4E-FF-B8-00-5A-FF-C8-00-4A-FF-CC-00-1E-FF-CB-FF-DF-FF-D2-FF-A5-FF-EE-FF-8B-00-1F-FF-9E-00-52-FF-DB-00-6D-00-29-00-5B-00-62-00-1E-00-6B-FF-D0-00-3F-FF-98-FF-F6-FF-8E-FF-B4-FF-B4-FF-93-FF-F4-FF-9A-00-30-FF-BA-00-55-FF-E0-00-62-FF-FD-00-60-00-0B-00-58-00-05-00-51-FF-EE-00-47-FF-CB-00-2F-FF-AF-00-02-FF-AE-FF-C7-FF-D6-FF-98-00-18-FF-90-00-57-FF-BC-00-74-00-08-00-65-00-52-00-3A-00-7E-00-0A-00-85-FF-E5-00-74-FF-CF-00-5B-FF-CD-00-46-FF-E3-00-36-00-0F-00-25-00-43-00-08-00-67-FF-DC-00-61-FF-A9-00-2E-FF-8B-FF-E3-FF-9A-FF-A8-FF-DC-FF-A3-00-33-FF-D9-00-6F-00-2A-00-6D-00-66-00-2D-00-68-FF-D7-00-34-FF-9A-FF-EA-FF-91-FF-B0-FF-B3-FF-97-FF-E0-FF-98-FF-FA-FF-A5-FF-F7-FF-B6-FF-DF-FF-CD-FF-C1-FF-F3-FF-AC-00-25-FF-AE-00-54-FF-CC-00-6E-00-00-00-67-00-39-00-43-00-60-00-12-00-6E-FF-E9-00-6A-FF-D8-00-61-FF-EA-00-54-00-17-00-35-00-46-FF-FB-00-53-FF-B2-00-2B-FF-7F-FF-E2-FF-88-FF-AD-FF-D1-FF-B5-00-31-FF-F8-00-6D-00-49-00-60-00-74-00-1B-00-65-FF-CC-00-32-FF-9C-FF-FD-FF-94-FF-DA-FF-A3-FF-C7-FF-B7-FF-BE-FF-C6-FF-BB-FF-D0-FF-BE-FF-D2-FF-C7-FF-CF-FF-D4-FF-C7-FF-E7-FF-BD-00-05-FF-B6-00-2C-FF-BB-00-53-FF-D4-00-65-00-04-00-51-00-40-00-16-00-6D-FF-C9-00-74-FF-8C-00-4F-FF-77-00-0E-FF-8F-FF-CA-FF-BE-FF-9A-FF-E8-FF-87-FF-FB-FF-8E-FF-F8-FF-A9-FF-EA-FF-CD-FF-D9-FF-F2-FF-C9-00-11-FF-BB-00-2A-FF-B8-00-41-FF-C6-00-56-FF-E8-00-63-00-17-00-5D-00-42-00-42-00-5D-00-19-00-62-FF-ED-00-51-FF-C8-00-2E-FF-AF-00-03-FF-A6-FF-DA-FF-AB-FF-BB-FF-BE-FF-AB-FF-DA-FF-A9-FF-FA-FF-B3-00-1A-FF-C5-00-33-FF-D8-00-42-FF-E7-00-46-FF-EF-00-44-FF-F1-00-40-FF-F1-00-3F-FF-EF-00-40-FF-EB-00-40-FF-E3-00-3F-FF-DB-00-3F-FF-D9-00-41-FF-E4-00-47-FF-FD-00-4D-00-1F-00-4C-00-3E-00-41-00-53-00-31-00-5B-00-22-00-59-00-1A-00-53-00-17-00-4F-00-14-00-4D-00-0E-00-4D-00-03-00-4C-FF-F7-00-4A-FF-EC-00-48-FF-E3-00-46-FF-DA-00-43-FF-CC-00-36-FF-B7-00-18-FF-A2-FF-E8-FF-9C-FF-B4-FF-B4-FF-96-FF-E9-FF-9D-00-27-FF-C6-00-54-FF-FC-00-62-00-2B-00-54-00-47-00-38-00-51-00-1C-00-4F-00-09-00-47-00-05-00-3D-00-0C-00-35-00-1A-00-30-00-29-00-2D-00-34-00-2D-00-3A-00-2F-00-3C-00-33-00-3A-00-3C-00-31-00-47-00-1F-00-51-00-04-00-56-FF-E4-00-50-FF-C0-00-36-FF-A0-00-06-FF-92-FF-CE-FF-A6-FF-A6-FF-DC-FF-A5-00-22-FF-CD-00-5A-00-0A-00-6E-00-43-00-5B-00-65-00-29-00-63-FF-E6-00-37-FF-AA-FF-ED-FF-93-FF-A6-FF-B3-FF-84-FF-FB-FF-96-00-43-FF-C9-00-6B-00-00-00-6E-00-28-00-5F-00-37-00-53-00-2D-00-4F-00-0D-00-4B-FF-E1-00-3D-FF-BE-00-29-FF-AD-00-19-FF-AE-00-15-FF-B5-00-18-FF-BB-00-19-FF-C1-00-10-FF-C7-00-02-FF-CE-FF-F9-FF-D2-FF-FB-FF-D2-00-05-FF-CF-00-0E-FF-C8-00-14-FF-C0-00-1C-FF-BF-00-2E-FF-CF-00-45-FF-EE-00-57-00-12-00-5B-00-2B-00-53-00-32-00-49-00-2D-00-42-00-29-00-3B-00-2E-00-31-00-38-00-21-00-42-00-13-00-4A-00-09-00-50-00-01-00-52-FF-F3-00-4B-FF-DC-00-3D-FF-C1-00-31-FF-AF-00-30-FF-B0-00-3B-FF-C4-00-4A-FF-E3-00-53-FF-FC-00-50-00-01-00-46-FF-EF-00-31-FF-CB-00-0B-FF-A6-FF-D2-FF-96-FF-97-FF-AF-FF-7F-FF-F3-FF-A5-00-40-FF-FF-00-5E-00-4F-00-32-00-5A-FF-DB-00-16-FF-A2-FF-BC-FF-B7-FF-93-00-09-FF-B3-00-5A-FF-FE-00-77-00-41-00-5B-00-60-00-24-00-5D-FF-EE-00-45-FF-C7-00-29-FF-B2-00-15-FF-AE-00-11-FF-B5-00-1B-FF-C1-00-2A-FF-CB-00-39-FF-D2-00-48-FF-DE-00-57-FF-FC-00-55-00-30-00-2D-00-63-FF-E4-00-6C-FF-A0-00-34-FF-93-FF-D2-FF-CB-FF-87-00-21-FF-83-00-5A-FF-C6-00-5A-00-1E-00-2F-00-5B-FF-FB-00-6F-FF-D4-00-66-FF-BC-00-56-FF-B3-00-48-FF-BE-00-41-FF-E3-00-3F-00-1B-00-39-00-52-00-21-00-71-FF-F4-00-67-FF-C0-00-34-FF-9E-FF-EE-FF-A3-FF-B1-FF-D0-FF-96-00-11-FF-A8-00-4C-FF-DB-00-6A-00-16-00-67-00-40-00-51-00-4E-00-3A-00-48-00-2E-00-37-00-2E-00-22-00-37-00-07-00-41-FF-E1-00-44-FF-B4-00-33-FF-94-00-07-FF-99-FF-CB-FF-C9-FF-99-00-16-FF-8C-00-5F-FF-AB-00-88-FF-E8-00-87-00-26-00-68-00-52-00-41-00-65-00-28-00-5F-00-25-00-43-00-2D-00-10-00-2A-FF-CE-00-0A-FF-96-FF-D6-FF-87-FF-A9-FF-AE-FF-9F-FF-F6-FF-B6-00-39-FF-D4-00-58-FF-DF-00-51-FF-D4-00-39-FF-C5-00-23-FF-BC-00-19-FF-B8-00-12-FF-B6-00-04-FF-B8-FF-E8-FF-C9-FF-C4-FF-F1-FF-A9-00-25-FF-A7-00-56-FF-C8-00-6F-00-04-00-65-00-46-00-39-00-78-FF-F9-00-87-FF-B9-00-70-FF-8C-00-3C-FF-7E-FF-FD-FF-8E-FF-C3-FF-B6-FF-99-FF-EA-FF-85-00-1C-FF-87-00-40-FF-97-00-52-FF-AF-00-57-FF-CA-00-57-FF-E9-00-54-00-04-00-4C-00-0C-00-37-FF-FA-00-15-FF-D7-FF-EC-FF-B6-FF-CC-FF-A7-FF-C1-FF-A8-FF-CD-FF-AE-FF-E1-FF-B0-FF-E7-FF-B2-FF-D5-FF-BE-FF-B8-FF-E1-FF-A7-00-19-FF-BD-00-54-FF-FB-00-6F-00-40-00-53-00-60-00-08-00-45-FF-BB-00-01-FF-96-FF-C0-FF-A1-FF-A4-FF-C3-FF-A8-FF-DD-FF-B7-FF-E9-FF-BD-FF-F1-FF-BC-FF-FB-FF-BB-FF-FF-FF-C4-FF-F2-FF-D5-FF-D9-FF-EC-FF-C0-00-00-FF-B0-00-0E-FF-AD-00-13-FF-B1-00-11-FF-B7-00-0A-FF-BD-FF-FB-FF-C4-FF-E2-FF-CD-FF-C4-FF-DC-FF-AB-FF-F1-FF-9F-00-05-FF-A2-00-12-FF-AC-00-16-FF-B4-00-13-FF-B5-00-09-FF-B4-FF-F3-FF-C1-FF-CE-FF-E9-FF-AA-00-29-FF-A1-00-66-FF-C9-00-7B-00-17-00-53-00-66-00-00-00-86-FF-AD-00-63-FF-8A-00-10-FF-AA-FF-BD-FF-F7-FF-96-00-45-FF-AA-00-6E-FF-E8-00-68-00-2D-00-3F-00-5C-00-0A-00-6D-FF-DE-00-68-FF-C6-00-59-FF-C4-00-4A-FF-D9-00-40-00-04-00-35-00-37-00-20-00-62-FF-FA-00-6E-FF-C9-00-52-FF-A1-00-17-FF-97-FF-D7-FF-AF-FF-A9-FF-DC-FF-97-00-09-FF-9D-00-26-FF-AE-00-31-FF-BD-00-30-FF-C3-00-25-FF-C2-00-12-FF-BE-FF-F5-FF-C0-FF-D3-FF-CF-FF-B4-FF-EB-FF-A1-00-0D-FF-A0-00-2F-FF-B3-00-4D-FF-D6-00-5F-00-09-00-5C-00-47-00-35-00-7F-FF-EA-00-90-FF-9C-00-60-FF-80-FF-FE-FF-B3-FF-A4-00-12-FF-93-00-53-FF-D4-00-45-00-2B-FF-FE-00-4D-FF-BF-00-27-FF-B9-FF-E1-FF-E2-FF-B0-00-18-FF-A5-00-3F-FF-B1-00-55-FF-C4-00-62-FF-D8-00-64-FF-F2-00-5B-00-13-00-4B-00-31-00-3D-00-3D-00-36-00-2F-00-30-00-0A-00-26-FF-DC-00-1B-FF-B6-00-18-FF-A5-00-23-FF-A9-00-35-FF-B8-00-43-FF-C7-00-48-FF-D0-00-45-FF-D8-00-42-FF-E3-00-41-FF-F3-00-40-00-01-00-41-00-09-00-44-00-11-00-41-00-24-00-27-00-41-FF-F0-00-53-FF-AE-00-40-FF-88-00-03-FF-99-FF-B8-FF-DA-FF-89-00-29-FF-8C-00-62-FF-BB-00-79-FF-FA-00-76-00-2D-00-68-00-44-00-57-00-3D-00-44-00-1F-00-31-FF-FD-00-26-FF-E4-00-25-FF-D9-00-2D-FF-D6-00-35-FF-D2-00-36-FF-C9-00-33-FF-C1-00-32-FF-C0-00-38-FF-CC-00-46-FF-E5-00-4F-00-0B-00-45-00-38-00-1B-00-5D-FF-D8-00-60-FF-99-00-31-FF-88-FF-DF-FF-B7-FF-94-00-10-FF-82-00-5D-FF-B7-00-6D-00-12-00-3A-00-59-FF-E8-00-67-FF-A8-00-3B-FF-98-FF-F7-FF-B4-FF-BB-FF-E7-FF-9C-00-1F-FF-9E-00-4D-FF-BA-00-6B-FF-E0-00-73-FF-FA-00-62-FF-FA-00-38-FF-E4-FF-FD-FF-CE-FF-C0-FF-C9-FF-95-FF-D5-FF-86-FF-E3-FF-92-FF-EA-FF-A8-FF-EB-FF-BB-FF-EC-FF-C2-FF-E7-FF-C9-FF-D4-FF-E3-FF-B5-00-19-FF-A4-00-56-FF-C2-00-6F-00-0F-00-45-00-5F-FF-E9-00-79-FF-9A-00-46-FF-90-FF-EC-FF-CE-FF-AC-00-25-FF-AE-00-5C-FF-E7-00-5D-00-2D-00-33-00-5A-FF-FA-00-64-FF-CA-00-58-FF-AD-00-44-FF-A7-00-32-FF-B3-00-27-FF-C9-00-24-FF-E2-00-2A-FF-FF-00-37-00-27-00-40-00-54-00-37-00-73-00-13-00-6E-FF-DF-00-3F-FF-B2-FF-FA-FF-A1-FF-BE-FF-B0-FF-9F-FF-D4-FF-9A-FF-F8-FF-A3-00-09-FF-B2-FF-FC-FF-CC-FF-D8-FF-F7-FF-B3-00-2F-FF-A7-00-60-FF-C1-00-74-FF-F9-00-61-00-36-00-2E-00-60-FF-F0-00-6F-FF-BD-00-67-FF-A6-00-54-FF-AA-00-43-FF-BF-00-38-FF-D2-00-2F-FF-DB-00-25-FF-D9-00-1B-FF-D6-00-1A-FF-DE-00-23-FF-F0-00-36-00-02-00-4B-00-01-00-57-FF-EA-00-4E-FF-C9-00-2B-FF-B7-FF-F4-FF-C6-FF-BB-FF-F7-FF-97-00-36-FF-99-00-6C-FF-C5-00-7D-00-10-00-59-00-5A-00-0A-00-7E-FF-B4-00-64-FF-85-00-19-FF-92-FF-C4-FF-D0-FF-8F-00-1A-FF-86-00-51-FF-9E-00-6C-FF-C5-00-71-FF-F1-00-6B-00-16-00-61-00-25-00-4F-00-10-00-29-FF-DF-FF-E8-FF-B3-FF-A2-FF-B4-FF-7F-FF-E9-FF-9B-00-2F-FF-EA-00-58-00-41-00-4D-00-75-00-1D-00-78-FF-E8-00-58-FF-C3-00-2F-FF-B0-00-14-FF-A9-00-17-FF-B0-00-30-FF-CD-00-47-00-05-00-40-00-45-00-10-00-71-FF-CD-00-71-FF-98-00-42-FF-8B-FF-F8-FF-AC-FF-B1-FF-EA-FF-86-00-2F-FF-85-00-63-FF-AA-00-79-FF-E5-00-72-00-21-00-5A-00-4E-00-3E-00-64-00-28-00-65-00-1E-00-54-00-24-00-35-00-33-00-05-00-3A-FF-C9-00-22-FF-94-FF-EC-FF-84-FF-B5-FF-AE-FF-A5-00-00-FF-CA-00-4D-00-0B-00-72-00-3E-00-69-00-4E-00-4C-00-44-00-34-00-38-00-2A-00-32-00-30-00-20-00-41-FF-F3-00-4D-FF-B7-00-39-FF-95-FF-FD-FF-AD-FF-B6-FF-FA-FF-94-00-4C-FF-B1-00-78-FF-F8-00-71-00-3E-00-4B-00-66-00-1D-00-72-FF-F0-00-6C-FF-C9-00-5B-FF-AE-00-44-FF-A9-00-31-FF-BD-00-2A-FF-E2-00-2D-00-0E-00-31-00-3A-00-2F-00-5F-00-21-00-76-00-04-00-71-FF-D8-00-42-FF-AE-FF-F3-FF-A1-FF-A9-FF-C8-FF-93-00-15-FF-C2-00-59-00-17-00-69-00-5E-00-3F-00-75-FF-FB-00-62-FF-C5-00-3F-FF-AA-00-1D-FF-A3-00-01-FF-A4-FF-F0-FF-A5-FF-F4-FF-A8-00-12-FF-B8-00-39-FF-DD-00-4F-00-16-00-3E-00-51-00-0C-00-75-FF-CE-00-6F-FF-9C-00-3F-FF-88-FF-F6-FF-9C-FF-AB-FF-D7-FF-7D-00-25-FF-81-00-64-FF-B9-00-7A-00-0D-00-61-00-59-00-29-00-83-FF-EA-00-85-FF-B9-00-6B-FF-9C-00-47-FF-95-00-29-FF-A2-00-1D-FF-C3-00-25-FF-FA-00-34-00-3C-00-37-00-71-00-1C-00-7E-FF-EA-00-5A-FF-BA-00-17-FF-A4-FF-D3-FF-AF-FF-A8-FF-CC-FF-9B-FF-ED-FF-A2-00-0A-FF-B1-00-22-FF-BD-00-2F-FF-C0-00-27-FF-BF-00-06-FF-C8-FF-D5-FF-E1-FF-AB-00-07-FF-98-00-2A-FF-9E-00-3F-FF-B2-00-45-FF-C9-00-43-FF-DE-00-3C-FF-EB-00-30-FF-EE-00-1F-FF-E4-00-08-FF-D2-FF-EB-FF-BE-FF-C6-FF-B7-FF-A3-FF-C9-FF-91-FF-F7-FF-A6-00-35-FF-E8-00-60-00-3C-00-57-00-72-00-17-00-66-FF-C4-00-1E-FF-92-FF-C7-FF-A3-FF-96-FF-E8-FF-9F-00-36-FF-CE-00-66-FF-FE-00-70-00-18-00-64-00-18-00-58-00-07-00-53-FF-EA-00-4D-FF-C8-00-39-FF-AE-00-0F-FF-AB-FF-D7-FF-C8-FF-A5-FF-FC-FF-8C-00-33-FF-96-00-59-FF-B9-00-69-FF-DD-00-64-FF-F1-00-4F-FF-F0-00-2C-FF-DD-FF-F9-FF-C6-FF-BC-FF-BF-FF-8A-FF-D6-FF-7E-00-0C-FF-A8-00-46-FF-FB-00-5C-00-4D-00-39-00-71-FF-ED-00-54-FF-A3-00-0A-FF-86-FF-BD-FF-A0-FF-92-FF-E0-FF-94-00-24-FF-BA-00-56-FF-EF-00-6A-00-20-00-66-00-40-00-54-00-4F-00-3E-00-52-00-2D-00-4F-00-23-00-47-00-26-00-35-00-33-00-15-00-43-FF-EB-00-4C-FF-C2-00-45-FF-A8-00-30-FF-A2-00-16-FF-AC-00-00-FF-BD-FF-F0-FF-CC-FF-E7-FF-D4-FF-EA-FF-D7-FF-FA-FF-DA-00-19-FF-E5-00-3F-FF-FF-00-5F-00-29-00-68-00-54-00-4B-00-67-00-0B-00-49-FF-C1-FF-FE-FF-97-FF-AC-FF-AF-FF-87-FF-FD-FF-AA-00-51-00-01-00-72-00-53-00-49-00-6D-FF-F3-00-41-FF-A7-FF-EF-FF-90-FF-A9-FF-B7-FF-94-00-03-FF-B7-00-48-FF-FC-00-64-00-3E-00-4C-00-64-00-15-00-6B-FF-E7-00-5F-FF-DF-00-4B-FF-FF-00-37-00-2E-00-23-00-4F-00-15-00-55-00-11-00-49-00-15-00-3A-00-1B-00-30-00-23-00-2C-00-31-00-28-00-46-00-1C-00-59-00-04-00-61-FF-E3-00-57-FF-C2-00-3D-FF-AA-00-1C-FF-9F-FF-F7-FF-A3-FF-D5-FF-B4-FF-BA-FF-CD-FF-AB-FF-EA-FF-A9-00-03-FF-AF-00-16-FF-B8-00-26-FF-C5-00-39-FF-E0-00-50-00-0F-00-5C-00-48-00-46-00-69-00-06-00-4F-FF-BA-FF-FE-FF-99-FF-AD-FF-C5-FF-9F-00-20-FF-E2-00-60-00-3F-00-4E-00-6B-FF-FA-00-45-FF-A7-FF-F1-FF-94-FF-AC-FF-C7-FF-A4-00-1B-FF-D6-00-59-00-23-00-62-00-5E-00-3A-00-72-00-02-00-65-FF-D8-00-4C-FF-C5-00-36-FF-C0-00-29-FF-BE-00-23-FF-BD-00-23-FF-BE-00-28-FF-C2-00-2B-FF-C5-00-25-FF-C2-00-15-FF-BF-00-05-FF-BE-FF-FA-FF-BE-FF-F4-FF-BA-FF-E5-FF-B4-FF-C7-FF-B9-FF-A5-FF-DC-FF-9D-00-1C-FF-C4-00-5A-00-10-00-6E-00-55-00-41-00-63-FF-EB-00-30-FF-A5-FF-DC-FF-9B-FF-9A-FF-CA-FF-84-00-06-FF-91-00-22-FF-A9-00-16-FF-BB-FF-F6-FF-C6-FF-E1-FF-CA-FF-E1-FF-CB-FF-ED-FF-CA-FF-F7-FF-CC-FF-F3-FF-D3-FF-E3-FF-DD-FF-CF-FF-E1-FF-C1-FF-D9-FF-C2-FF-C8-FF-D6-FF-B7-FF-F7-FF-B0-00-20-FF-B9-00-48-FF-D7-00-6A-00-0D-00-77-00-4C-00-58-00-6E-00-0A-00-4E-FF-B3-FF-F5-FF-94-FF-AA-FF-CC-FF-B4-00-2A-00-0A-00-53-00-54-00-1E-00-45-FF-C8-FF-F0-FF-B0-FF-B3-FF-F1-FF-CF-00-45-00-27-00-59-00-6B-00-1D-00-6D-FF-CA-00-47-FF-9C-00-2A-FF-A0-00-2D-FF-BF-00-40-FF-E7-00-4B-00-11-00-42-00-3E-00-23-00-61-FF-F5-00-6A-FF-C2-00-54-FF-9A-00-27-FF-8A-FF-F5-FF-92-FF-CE-FF-A7-FF-BD-FF-BC-FF-C4-FF-CB-FF-DE-FF-D4-00-05-FF-DF-00-34-FF-F6-00-60-00-1D-00-78-00-4C-00-66-00-68-00-27-00-54-FF-D6-00-0E-FF-9F-FF-BA-FF-A7-FF-8A-FF-E9-FF-9A-00-3B-FF-DF-00-6E-00-2D-00-6D-00-60-00-45-00-6D-00-14-00-64-FF-F3-00-5A-FF-EC-00-53-00-03-00-40-00-2C-00-10-00-4D-FF-CC-00-4A-FF-96-00-19-FF-91-FF-CF-FF-C5-FF-98-00-17-FF-8D-00-5E-FF-AD-00-82-FF-E0-00-80-00-12-00-66-00-3A-00-49-00-54-00-36-00-55-00-2F-00-30-00-24-FF-EE-00-02-FF-AF-FF-CC-FF-93-FF-9C-FF-AB-FF-8C-FF-E9-FF-A9-00-2C-FF-E6-00-5A-00-28-00-61-00-5A-00-44-00-6C-00-11-00-61-FF-DC-00-44-FF-B6-00-29-FF-A9-00-1C-FF-B0-00-1E-FF-C0-00-2B-FF-D1-00-3E-FF-E7-00-50-00-0A-00-4E-00-3C-00-26-00-6A-FF-E0-00-70-FF-A0-00-3A-FF-92-FF-DE-FF-C2-FF-90-00-12-FF-7B-00-54-FF-9E-00-73-FF-D9-00-73-00-0C-00-65-00-30-00-56-00-46-00-4A-00-49-00-42-00-2C-00-33-FF-F2-00-0F-FF-B4-FF-D6-FF-97-FF-A0-FF-AE-FF-8C-FF-EC-FF-A5-00-2F-FF-D9-00-59-00-0D-00-65-00-2F-00-5D-00-40-00-4B-00-49-00-35-00-4D-00-1E-00-4B-00-10-00-43-00-10-00-35-00-22-00-20-00-3D-FF-FF-00-56-FF-D1-00-5E-FF-A1-00-44-FF-8C-00-03-FF-AD-FF-B6-00-02-FF-8B-00-5C-FF-A8-00-7B-FF-FC-00-49-00-48-FF-EE-00-54-FF-AF-00-1F-FF-AD-FF-D7-FF-D6-FF-AB-00-06-FF-A3-00-27-FF-AC-00-3B-FF-B4-00-42-FF-B8-00-31-FF-B8-00-03-FF-BD-FF-C7-FF-D0-FF-98-FF-F8-FF-8F-00-2D-FF-AC-00-59-FF-DF-00-69-00-13-00-5B-00-37-00-3F-00-4A-00-27-00-4F-00-19-00-4C-00-17-00-44-00-1F-00-36-00-30-00-1B-00-48-FF-F0-00-58-FF-BC-00-4C-FF-96-00-1B-FF-9C-FF-D2-FF-D9-FF-96-00-36-FF-8F-00-7D-FF-CA-00-80-00-24-00-3C-00-62-FF-DE-00-5E-FF-A3-00-1F-FF-A4-FF-D3-FF-D4-FF-A3-00-0E-FF-9B-00-3C-FF-AD-00-57-FF-C3-00-5D-FF-CE-00-4B-FF-CC-00-1E-FF-C3-FF-DE-FF-C3-FF-A0-FF-DB-FF-82-00-0C-FF-92-00-43-FF-CC-00-62-00-15-00-5A-00-50-00-30-00-6D-FF-FC-00-6A-FF-D1-00-51-FF-B9-00-30-FF-B0-00-10-FF-AE-FF-FA-FF-B0-FF-F3-FF-B5-FF-FB-FF-BD-00-0F-FF-C6-00-28-FF-D0-00-45-FF-DE-00-60-FF-F9-00-68-00-28-00-45-00-5B-FF-F5-00-6E-FF-9F-00-42-FF-80-FF-E5-FF-B4-FF-96-00-13-FF-93-00-52-FF-DC-00-43-00-32-FF-FE-00-53-FF-C0-00-31-FF-AE-FF-F4-FF-BE-FF-C4-FF-D4-FF-AE-FF-E3-FF-A8-FF-EB-FF-A9-FF-EF-FF-B2-FF-E8-FF-C5-FF-D5-FF-E0-FF-BD-00-02-FF-B1-00-2A-FF-BD-00-53-FF-E1-00-6F-00-12-00-6E-00-40-00-48-00-5A-00-07-00-54-FF-C4-00-2D-FF-96-FF-F4-FF-90-FF-BD-FF-B0-FF-9B-FF-E6-FF-97-00-19-FF-AB-00-3A-FF-C8-00-47-FF-D9-00-47-FF-D4-00-3A-FF-C0-00-15-FF-BA-FF-D7-FF-DF-FF-9A-00-2A-FF-8A-00-6A-FF-C2-00-6B-00-22-00-23-00-69-FF-C5-00-65-FF-93-00-22-FF-A6-FF-D4-FF-E4-FF-A8-00-21-FF-A3-00-49-FF-B1-00-5B-FF-C2-00-60-FF-D2-00-58-FF-DC-00-41-FF-DD-00-1D-FF-D1-FF-F1-FF-C3-FF-C4-FF-C1-FF-A0-FF-D3-FF-8F-FF-F6-FF-9A-00-1D-FF-BF-00-3C-FF-F2-00-4E-00-23-00-4F-00-44-00-45-00-4F-00-36-00-4A-00-27-00-3F-00-1F-00-36-00-21-00-2D-00-33-00-15-00-4E-FF-E5-00-5E-FF-AC-00-46-FF-91-00-01-FF-B4-FF-B1-00-0C-FF-8C-00-64-FF-B0-00-81-00-06-00-54-00-51-00-00-00-64-FF-B9-00-44-FF-95-00-19-FF-90-00-04-FF-9A-00-08-FF-AE-00-18-FF-CD-00-2A-FF-F6-00-3D-00-24-00-4A-00-4F-00-41-00-69-00-19-00-61-FF-DD-00-2F-FF-AB-FF-E3-FF-9F-FF-A0-FF-C1-FF-86-FF-FE-FF-9B-00-39-FF-D1-00-5E-00-0D-00-67-00-38-00-5D-00-4C-00-4B-00-4A-00-3B-00-3E-00-2E-00-34-00-28-00-2A-00-2B-00-18-00-39-FF-F2-00-47-FF-BE-00-40-FF-95-00-15-FF-97-FF-D1-FF-D2-FF-97-00-2C-FF-90-00-75-FF-C3-00-85-00-13-00-59-00-55-00-13-00-73-FF-D9-00-72-FF-C0-00-65-FF-C4-00-58-FF-D7-00-4A-FF-EF-00-3B-00-0D-00-2F-00-2F-00-25-00-52-00-14-00-66-FF-F0-00-59-FF-BF-00-25-FF-9C-FF-DA-FF-A1-FF-9C-FF-D5-FF-8F-00-23-FF-BE-00-62-00-11-00-70-00-5D-00-42-00-77-FF-F1-00-52-FF-AA-00-06-FF-8E-FF-BB-FF-A7-FF-92-FF-DE-FF-91-00-17-FF-A9-00-3D-FF-C1-00-4B-FF-C7-00-46-FF-BC-00-2F-FF-B0-00-05-FF-B8-FF-D0-FF-E1-FF-A3-00-21-FF-95-00-5E-FF-B5-00-77-FF-F9-00-5F-00-45-00-24-00-79-FF-E1-00-88-FF-AC-00-73-FF-8F-00-47-FF-8A-00-0F-FF-98-FF-D8-FF-B0-FF-B0-FF-C8-FF-A1-FF-D7-FF-AD-FF-DA-FF-CE-FF-D6-FF-FA-FF-D3-00-2A-FF-DA-00-58-FF-F7-00-78-00-28-00-75-00-56-00-42-00-5F-FF-ED-00-31-FF-A2-FF-E0-FF-8F-FF-9D-FF-C2-FF-93-00-1A-FF-C5-00-60-00-10-00-70-00-4B-00-51-00-65-00-29-00-60-00-18-00-47-00-27-00-1F-00-41-FF-E9-00-4C-FF-B4-00-35-FF-95-FF-FF-FF-9F-FF-BE-FF-D2-FF-90-00-1B-FF-8B-00-5C-FF-B1-00-80-FF-F0-00-80-00-2D-00-65-00-58-00-38-00-6D-00-06-00-6E-FF-DE-00-64-FF-CE-00-53-FF-DE-00-40-00-07-00-2A-00-37-00-0B-00-5B-FF-E3-00-63-FF-B6-00-45-FF-94-00-06-FF-90-FF-C1-FF-BA-FF-9B-00-06-FF-AD-00-53-FF-F0-00-77-00-3D-00-60-00-6B-00-1D-00-67-FF-D5-00-3F-FF-A6-00-12-FF-96-FF-FD-FF-9B-00-0A-FF-AB-00-2C-FF-C8-00-4B-FF-F9-00-4B-00-36-00-23-00-69-FF-E0-00-75-FF-A4-00-4F-FF-8A-00-07-FF-9B-FF-BE-FF-C8-FF-8C-FF-FA-FF-7A-00-24-FF-81-00-43-FF-9D-00-59-FF-C7-00-64-FF-F3-00-62-00-10-00-50-00-0E-00-2E-FF-F2-00-00-FF-D0-FF-CF-FF-BB-FF-A5-FF-B9-FF-8C-FF-CD-FF-89-FF-F4-FF-A3-00-27-FF-DD-00-53-00-27-00-5D-00-63-00-36-00-71-FF-EF-00-4C-FF-AE-00-0D-FF-91-FF-D3-FF-9D-FF-B3-FF-BC-FF-AA-FF-D7-FF-B2-FF-E1-FF-C4-FF-D9-FF-E4-FF-CA-00-0E-FF-BF-00-3C-FF-C4-00-5F-FF-DD-00-6D-00-05-00-61-00-2F-00-3F-00-4F-00-15-00-5F-FF-F1-00-60-FF-DD-00-59-FF-D7-00-4D-FF-D8-00-41-FF-DB-00-36";
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
                ulong tick = (ulong)DateTime.Now.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).Ticks * 100;
                DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddTicks((long)tick / 100);

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
