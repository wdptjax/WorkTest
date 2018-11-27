using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
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
        public int IndexAudio = 0;
        public bool IsSendIQ = false;
        public int IndexIQ = 0;
        public bool IsSendITU = false;
        public int IndexITU = 0;
        public bool IsSendSpectrum = false;
        public int IndexSpectrum = 0;
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

            //TestDDF550();
            DDF550SendAsyn();
            Console.ReadLine();
        }


        #region DDF550发送模拟数据

        static List<byte[]> _audioData = new List<byte[]>();
        static List<byte[]> _iqData = new List<byte[]>();
        static List<byte[]> _spectrumData = new List<byte[]>();
        static List<byte[]> _ituData = new List<byte[]>();
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
            Socket socket = _tcpListener1.AcceptSocket();
            while (true)
            {
                try
                {
                    byte[] buffer = new byte[1024 * 1024];
                    int count = socket.Receive(buffer, SocketFlags.None);
                    if (count == 0)
                        continue;

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

                        Reply xml = (Reply)XmlWrapper.DeserializeObject<Reply>(xmlData);
                        if (xml.Command.Name == "TraceEnable")
                        {
                            List<Param> paras = xml.Command.Params;
                            bool isSendAudio = false;
                            string ip = "";
                            int port = 0;
                            paras.ForEach(p =>
                            {
                                if (p.Name == "eTraceTag" && p.Value == "TRACETAG_AUDIO")
                                {
                                    isSendAudio = true;
                                }
                                else if (p.Name == "zIP")
                                {
                                    ip = p.Value;
                                }
                                else if (p.Name == "iPort")
                                {
                                    int.TryParse(p.Value, out port);
                                }
                            });
                            lock (_lockClient)
                            {
                                ClientInfo info = _clientList.FirstOrDefault(i => i.Address.Address.ToString() == ip && i.Address.Port == port);
                                if (info != null)
                                {
                                    info.IsSendAudio = isSendAudio;
                                }
                            }
                        }

                        offset += 4 + 4 + datalen + 4;
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        static void ScanDataSendEB200()
        {
            Socket socket = _tcpListener2.AcceptSocket();
            IPEndPoint iPEndPoint = socket.RemoteEndPoint as IPEndPoint;
            ClientInfo info = new ClientInfo();
            info.Address = iPEndPoint;
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
                        socket.Send(data);
                    }
                    else
                        continue;
                }
                catch (Exception)
                {
                }
            }
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
            string strData = "00-0E-B2-00-00-64-00-02-03-56-00-00-00-00-0B-96-14-B5-00-00-00-00-0B-6E-00-00-0C-DC-00-00-0C-DC-00-01-00-00-00-00-00-00-00-00-00-C9-00-00-00-00-00-00-00-4C-80-0C-F8-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-C9-00-00-00-00-05-95-BF-A0-00-00-00-00-00-00-C3-50-00-00-00-01-00-98-96-80-46-DD-BA-7F-00-01-86-64-00-5F-FE-D4-00-00-FF-FF-02-20-20-11-00-00-00-00-00-10-38-D5-15-69-9F-BF-C7-4E-43-0E-00-AF-00-00-00-00-00-00-FF-5D-FF-5A-FF-5A-FF-5E-00-36-FF-5E-FF-5C-FF-60-FF-5C-FF-5D-FF-5A-FF-5F-FF-71-00-3E-01-26-00-41-FF-77-FF-5D-FF-67-FF-5D-FF-5D-FF-5F-FF-5F-FF-E2-01-83-FF-C9-FF-5C-FF-5F-FF-5B-FF-5A-FF-5D-FF-5D-FF-5E-FF-5E-FF-5D-FF-5E-FF-61-FF-64-FF-6A-FF-62-FF-63-FF-5F-FF-5F-FF-60-FF-5C-FF-5E-FF-61-00-31-01-64-00-2F-FF-64-FF-62-FF-6C-FF-66-FF-62-FF-60-FF-61-FF-5F-FF-5D-FF-62-00-11-FF-73-00-12-FF-74-FF-65-FF-E7-01-35-FF-F7-FF-8D-FF-5E-FF-5F-FF-62-FF-5D-FF-60-FF-5F-FF-60-FF-62-FF-61-FF-60-FF-60-FF-5F-FF-5E-FF-62-FF-60-FF-66-FF-91-FF-B8-FF-75-FF-66-FF-62-FF-61-FF-60-FF-63-FF-60-FF-5F-FF-71-FF-FF-FF-6B-FF-67-00-42-01-A7-00-69-FF-61-FF-6A-FF-62-FF-69-FF-6C-FF-63-FF-62-FF-91-FF-A8-00-20-01-9C-00-78-FF-A9-FF-78-FF-7B-00-B6-01-D0-00-94-FF-84-FF-71-00-06-FF-6F-FF-64-FF-67-FF-74-FF-68-FF-63-FF-63-FF-72-FF-5F-FF-62-FF-61-FF-63-FF-62-FF-61-FF-60-FF-73-00-62-01-24-00-3F-FF-6B-FF-64-FF-61-FF-5F-FF-5F-FF-5D-FF-60-FF-5E-FF-60-FF-6D-FF-E9-FF-6E-FF-61-FF-63-FF-63-FF-5D-FF-5D-FF-7D-FF-C1-FF-87-FF-64-FF-61-FF-62-FF-60-FF-62-FF-61-FF-63-FF-62-FF-60-FF-C4-01-64-FF-B4-FF-61-FF-64-FF-62-FF-5F-FF-64-FF-62-FF-63-FF-98-00-11-FF-8E-FF-66-FF-91-01-38-FF-BC-FF-62-FF-66-FF-76-FF-65-FF-63-FF-63-FF-62-FF-62-FF-61-00-2F-01-BD-00-0A-FF-63-02-69-01-F1-07-D2-0B-0D-0B-DB-07-B8-09-93-07-A8-04-50-07-31-09-15-02-4B-03-A0-04-A7-04-7B-04-8E-04-94-09-E4-09-07-03-BD-0D-FE-05-BE-0C-8B-02-32-02-35-02-47-04-00-01-3C-04-BE-09-6F-07-E0-0C-5C-07-48-02-07-02-2B-0B-7D-09-7B-0E-0C-0B-17-0B-4E-00-11-0C-E9-02-CC-03-F1-06-59-02-C7-00-64-02-4A-02-50-02-5B-02-59-07-C7-0A-1F-07-7B-07-E5-03-3A-06-79-04-28-09-71-05-86-01-15-0B-BA-0B-E7-0B-BA-01-FF-02-3F-02-92-02-1E-00-7A-01-E2-00-F1-09-74-05-9A-0A-63-07-04-05-AE-0B-9A-01-2C-0C-BB-06-1D-03-8D-03-21-02-A5-0B-BA-0D-71-0D-28-0C-67-04-80-0C-68-06-62-00-0A-0A-2C-06-06-05-48-0D-9A-04-DC-05-7F-03-49-0D-3B-00-0E-0D-F7-0D-D9-00-69-08-75-00-55-07-D9-0C-F3-0D-6D-0D-FF-0D-9C-0D-73-0D-3D-0B-E7-0C-1E-0C-EE-04-E0-04-DC-04-DB-05-05-04-C0-04-E6-04-CA-0B-96-01-2A-05-2D-06-37-0A-F0-0A-CB-04-A3-04-19-0A-28-03-63-05-5D-07-67-0A-CD-06-29-06-1B-02-30-01-DA-02-1D-02-2F-02-32-02-3C-05-2C-08-66-06-2A-06-F2-04-B6-05-E8-06-1C-04-19-09-BA-09-DE-09-A2-01-E4-06-CF-0D-64-01-96-01-BF-01-56-01-46-00-BF-07-A4-02-B1-08-13-00-0D-0D-85-0B-14-09-49-0B-23-0A-91-06-AE-07-01-07-0F-05-D1-04-60-07-73-07-BC-06-11-00-D3-09-92-0C-7C-0B-DF-0D-17-09-F9-0B-42-0C-45-0D-25-08-FC-01-09-0D-AE-05-2A-01-10-01-1C-05-E5-06-FA-05-62-02-1F-02-37-02-49-09-B4-00-2F-00-AD-00-59-01-78-03-E3-00-73-02-3F-01-16-00-CF-01-74-01-7B-01-5F-02-9D-03-12-03-35-03-14-02-FC-02-08-02-A6-01-57-00-E4-01-0B-01-94-03-CE-03-E5-03-CF-00-80-00-21-00-52-00-A7-02-38-01-7E-02-AF-01-7C-00-A6-01-0C-00-34-03-9D-02-04-03-01-01-45-01-8B-01-CC-01-F2-00-7B-01-DA-02-47-03-E0-03-E0-03-DC-03-C0-00-E8-03-93-00-2A-00-FA-01-48-01-B6-00-53-00-EF-01-01-03-E4-03-C8-03-D2-03-CE-03-04-03-E1-03-D8-03-E3-03-86-01-44-01-D2-01-D2-01-C2-00-B6-00-6E-01-29-01-0C-00-17-01-31-01-D1-00-7E-00-B1-00-B6-01-8C-03-72-03-E1-03-BA-02-E2-00-E9-01-33-01-90-01-9F-01-B9-01-1D-00-83-03-E2-02-46-03-B9-01-D8-03-DF-03-D8-03-C9-03-7D-02-8A-01-12-02-14-03-2A-01-E6-02-55-03-A5-03-D3-03-CF-02-75-03-E2-03-D5-02-90-03-D1-03-DE-03-D6-03-68-03-D5-02-9C-03-D0-02-24-02-69-02-CA-02-FD-00-24-02-D3-01-48-03-6E-01-B5-02-B3-00-79-01-56-00-47-01-70-00-CE-03-DE-03-E3-03-E1-03-D7-03-B9-01-1B-01-A0-00-AF-00-77-01-5D-00-91-00-FE-01-99-03-B5-03-BF-03-B6-01-55-00-FE-01-1F-01-B0-01-EA-03-B1-03-C5-03-C8-00-98-00-92-01-21-00-24-00-BE-00-64-02-6F-02-38-00-0B-03-B8-03-68-03-91-00-48-01-14-01-48-00-46-01-A3-01-11-00-64-03-BC-03-E4-03-C5-01-6E-03-B3-03-AB-03-BF-00-6F-03-2B-02-99-01-FC-01-9D-01-55-02-33-00-23-01-26-03-E1-03-E5-03-E4-01-A9-00-E7-00-E4-00-E4-00-E8-01-C0-00-E8-00-E6-00-E9-00-E5-00-E6-00-E3-00-E8-00-FA-01-C7-02-AF-01-C9-00-FF-00-E5-00-EF-00-E5-00-E5-00-E7-00-E7-01-6A-03-0A-01-50-00-E3-00-E6-00-E2-00-E1-00-E4-00-E4-00-E5-00-E4-00-E3-00-E4-00-E7-00-EA-00-F0-00-E8-00-E9-00-E5-00-E4-00-E5-00-E1-00-E3-00-E6-01-B6-02-E9-01-B4-00-E9-00-E6-00-F0-00-EA-00-E6-00-E4-00-E5-00-E3-00-E1-00-E6-01-94-00-F6-01-95-00-F7-00-E8-01-6A-02-B8-01-7A-01-10-00-E0-00-E1-00-E4-00-DF-00-E2-00-E1-00-E2-00-E4-00-E3-00-E1-00-E1-00-E0-00-DF-00-E3-00-E1-00-E7-01-12-01-39-00-F5-00-E6-00-E2-00-E1-00-E0-00-E3-00-E0-00-DF-00-F0-01-7E-00-EA-00-E6-01-C1-03-26-01-E8-00-E0-00-E9-00-E0-00-E7-00-EA-00-E1-00-E0-01-0F-01-26-01-9E-03-1A-01-F5-01-26-00-F5-00-F8-02-33-03-4D-02-11-01-01-00-EE-01-82-00-EB-00-E0-00-E3-00-F0-00-E4-00-DF-00-DF-00-EE-00-DB-00-DE-00-DD-00-DF-00-DE-00-DD-00-DC-00-EF-01-DD-02-9F-01-BA-00-E6-00-DF-00-DC-00-DA-00-DA-00-D8-00-DB-00-D9-00-DB-00-E8-01-64-00-E9-00-DC-00-DE-00-DE-00-D8-00-D8-00-F8-01-3C-01-02-00-DF-00-DC-00-DC-00-DA-00-DC-00-DB-00-DD-00-DC-00-DA-01-3E-02-DE-01-2E-00-DB-00-DE-00-DC-00-D9-00-DE-00-DC-00-DD-01-12-01-8B-01-08-00-E0-01-0B-02-B2-01-36-00-DC-00-DF-00-EF-00-DE-00-DC-00-DC-00-DB-00-DB-00-DA-01-A8-03-36-01-83-00-DC-FF-5D-FF-5A-FF-5A-FF-5E-00-36-FF-5E-FF-5C-FF-60-FF-5C-FF-5D-FF-5A-FF-5F-FF-71-00-3E-01-26-00-41-FF-77-FF-5D-FF-67-FF-5D-FF-5D-FF-5F-FF-5F-FF-E2-01-83-FF-C9-FF-5C-FF-5F-FF-5B-FF-5A-FF-5D-FF-5D-FF-5E-FF-5E-FF-5D-FF-5E-FF-61-FF-64-FF-6A-FF-62-FF-63-FF-5F-FF-5F-FF-60-FF-5C-FF-5E-FF-61-00-31-01-64-00-2F-FF-64-FF-62-FF-6C-FF-66-FF-62-FF-60-FF-61-FF-5F-FF-5D-FF-62-00-11-FF-73-00-12-FF-74-FF-65-FF-E7-01-35-FF-F7-FF-8D-FF-5E-FF-5F-FF-62-FF-5D-FF-60-FF-5F-FF-60-FF-62-FF-61-FF-60-FF-60-FF-5F-FF-5E-FF-62-FF-60-FF-66-FF-91-FF-B8-FF-75-FF-66-FF-62-FF-61-FF-60-FF-63-FF-60-FF-5F-FF-71-FF-FF-FF-6B-FF-67-00-42-01-A7-00-69-FF-61-FF-6A-FF-62-FF-69-FF-6C-FF-63-FF-62-FF-91-FF-A8-00-20-01-9C-00-78-FF-A9-FF-78-FF-7B-00-B6-01-D0-00-94-FF-84-FF-71-00-06-FF-6F-FF-64-FF-67-FF-74-FF-68-FF-63-FF-63-FF-72-FF-5F-FF-62-FF-61-FF-63-FF-62-FF-61-FF-60-FF-73-00-62-01-24-00-3F-FF-6B-FF-64-FF-61-FF-5F-FF-5F-FF-5D-FF-60-FF-5E-FF-60-FF-6D-FF-E9-FF-6E-FF-61-FF-63-FF-63-FF-5D-FF-5D-FF-7D-FF-C1-FF-87-FF-64-FF-61-FF-62-FF-60-FF-62-FF-61-FF-63-FF-62-FF-60-FF-C4-01-64-FF-B4-FF-61-FF-64-FF-62-FF-5F-FF-64-FF-62-FF-63-FF-98-00-11-FF-8E-FF-66-FF-91-01-38-FF-BC-FF-62-FF-66-FF-76-FF-65-FF-63-FF-63-FF-62-FF-62-FF-61-00-2F-01-BD-00-0A-FF-63-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-02-DF-00-00-00-00-00-00-00-00-01-F9-01-E0-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-01-0E-01-9A-00-00-00-00-00-F0-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-05-00-00-01-B8-00-00-00-00-00-00-03-16-00-00-00-00-01-59-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-01-8B-01-18-00-00-02-49-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-05-00-A5-01-F9-00-00-00-00-00-00-00-00-00-00-02-2B-00-00-00-D7-00-00-00-00-00-00-00-00-00-00-00-00-02-1C-01-72-00-00-02-80-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-05-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-D7-00-5A-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-01-B8-00-AA-00-3C-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-02-08-01-45-01-F4-00-00-00-00-00-00-00-00-02-3F-01-CC-00-82-00-00-00-00-02-C6-00-00-00-00-00-F5-00-64-00-00-00-00-01-9F-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E-9E-01-9E-01-01-9E-01-9E-01-9E-01-9E-01-9E-01-9E";
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

                    int offset = Marshal.SizeOf(typeof(EB200Header));
                    while (offset < data.Length)
                    {
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
                                break;
                            case (ushort)TAGS.IFPAN:
                                break;
                            case (ushort)TAGS.IF:
                                break;
                            case (ushort)TAGS.CW:
                                break;
                            case (ushort)TAGS.DFPScan:
                                obj = ToDFPScan(data, offset);
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

                if ((selectorFlags & (uint)FLAGS.DF_LEVEL) > 0)
                {
                    for (int i = 0; i < pLevel.Length; i++)
                    {
                        Array.Reverse(buffer, offset, 2);
                        pLevel[i] = BitConverter.ToInt16(buffer, offset) / 10f;
                        offset += 2;
                    }
                }
                if ((selectorFlags & (uint)FLAGS.AZIMUTH) > 0)
                {
                    for (int i = 0; i < pAzimuth.Length; i++)
                    {
                        Array.Reverse(buffer, offset, 2);
                        pAzimuth[i] = BitConverter.ToInt16(buffer, offset) / 10f;
                        if (pAzimuth[i] >= 3276.6)
                        {
                            pAzimuth[i] = 0f;
                        }
                        offset += 2;
                    }
                }
                if ((selectorFlags & (uint)FLAGS.DF_QUALITY) > 0)
                {
                    for (int i = 0; i < pQuality.Length; i++)
                    {
                        Array.Reverse(buffer, offset, 2);
                        pQuality[i] = BitConverter.ToInt16(buffer, offset) / 10f;
                        if (pQuality[i] >= 3276.6)
                        {
                            pQuality[i] = 0f;
                        }
                        offset += 2;
                    }
                }

                long freq = (long)opt.Frequency;
                int freqIndex = opt.LogChannel;
                double frequency = freq / 1000000.0;
                double bandwidth = opt.Bandwidth / 1000.0;


                return null;
            }
            catch (Exception ex)
            {
                return null;
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
