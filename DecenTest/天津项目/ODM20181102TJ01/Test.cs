using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace ODM20181102TJ01
{
    public class Test
    {
        private TcpListener _tcpListener;
        private Random _random = new Random();

        private Socket _socket = null;


        private ParameterMessage _lastParameter;
        private EDataMode _nowSendMode = EDataMode.MODE_FFM;
        private bool _isSend = false;

        /// <summary>
        /// 开启测试
        /// </summary>
        public void Test_ODM20181102TJ01()
        {
            IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Any, 9999);
            _tcpListener = new TcpListener(iPEndPoint);
            _tcpListener.Start();
            Console.WriteLine(string.Format("{0:HH:mm:ss.fff} 服务开启:{1}", DateTime.Now, iPEndPoint));

            Thread thd = new Thread(RecvScan);
            thd.IsBackground = true;
            thd.Start();

            Thread thd1 = new Thread(SendScan);
            thd1.IsBackground = true;
            thd1.Start();

            //// 测试
            //Thread thd2 = new Thread(TestSendParam);
            //thd2.IsBackground = true;
            //thd2.Start();
        }

        /// <summary>
        /// 接受数据线程
        /// </summary>
        private void RecvScan()
        {
            while (true)
            {
                _socket = _tcpListener.AcceptSocket();
                IPEndPoint iPEndPoint = _socket.RemoteEndPoint as IPEndPoint;
                Console.WriteLine(string.Format("{0:HH:mm:ss.fff} 服务接收到连接:{1}", DateTime.Now, iPEndPoint));
                _tcpListener.Stop();

                byte[] buffer = new byte[1024 * 1024];
                while (true)
                {
                    try
                    {
                        // 收第一个字节，判断下发参数的功能类别
                        ReceiveData(buffer, 0, 1, _socket);
                        byte mode = buffer[0];
                        if (mode > (byte)EDataMode.MODE_STANDBY)// 类别为枚举EDataMode
                        {
                            continue;
                        }
                        // 收消息长度 4字节
                        ReceiveData(buffer, 1, 4, _socket);
                        uint len = BitConverter.ToUInt32(buffer, 1);
                        if (!Utils.CheckParamLength(mode, len))// 判断消息长度是否符合定义
                        {
                            continue;
                        }
                        // 接收数据主体
                        ReceiveData(buffer, 5, (int)len, _socket);
                        Console.WriteLine(string.Format("{0:HH:mm:ss.fff} 接收到数据，长度:{1}", DateTime.Now, len));
                        AnalysisRecvData(buffer, (int)len + 5);
                    }
                    catch (Exception ex)
                    {
                        if (ex is SocketException)
                        {
                            _isSend = false;
                            if (_socket.Connected)
                            {
                                _socket.Close();
                            }
                            Console.WriteLine(string.Format("{0:HH:mm:ss.fff} 连接断开:{1}\r\n{2}", DateTime.Now, iPEndPoint, ex.ToString()));
                            break;
                        }
                        else
                        {
                            Console.WriteLine(string.Format("{0:HH:mm:ss.fff} RecvScan解析错误:\r\n{1}", DateTime.Now, ex.ToString()));
                        }
                    }

                }
                try
                {
                    _socket.Close();
                }
                catch { }
                _socket = null;
                _tcpListener.Start();
            }
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        private void SendScan()
        {
            DateTime lastGpsTime = DateTime.Now;
            while (true)
            {
                if (_socket != null)
                {
                    IPEndPoint iPEndPoint = _socket.RemoteEndPoint as IPEndPoint;
                    try
                    {
                        if (_isSend)
                        {
                            byte[] buffer = null;
                            switch (_nowSendMode)
                            {
                                case EDataMode.MODE_FFM:
                                    buffer = GetFFMData();
                                    break;
                                case EDataMode.MODE_SCAN:
                                    buffer = GetScanData();
                                    break;
                                case EDataMode.MODE_SEARCH:
                                case EDataMode.MODE_TDMA:
                                case EDataMode.MODE_CALIB:
                                case EDataMode.MODE_DIAG:
                                case EDataMode.MODE_STANDBY:
                                default:
                                    break;
                            }
                            if (buffer != null)
                            {
                                _socket.Send(buffer);
                            }
                            if (_nowSendMode == EDataMode.MODE_FFM)
                            {
                                Thread.Sleep(50);
                                buffer = GetSpectrumData();
                                _socket.Send(buffer);
                            }
                            if (DateTime.Now.Subtract(lastGpsTime).TotalSeconds > 1)
                            {
                                //每秒发一次GPS信息
                                lastGpsTime = DateTime.Now;
                                buffer = GetGpsData();
                                _socket.Send(buffer);
                            }
                            Thread.Sleep(50);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex is SocketException)
                        {
                            Console.WriteLine(string.Format("{0:HH:mm:ss.fff} 连接断开:{1}\r\n{2}", DateTime.Now, iPEndPoint, ex.ToString()));
                        }
                        else
                        {
                            Console.WriteLine(string.Format("{0:HH:mm:ss.fff} SendScan解析错误:\r\n{1}", DateTime.Now, ex.ToString()));
                        }
                    }
                    Thread.Sleep(100);
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
        }

        /// <summary>
        /// 读取指定长度的数据到数组
        /// </summary>
        /// <param name="recvBuffer">接收数据缓冲区</param>
        /// <param name="offset">缓冲区的偏移</param>
        /// <param name="bytesToRead">要读取的字节数</param>
        /// <param name="socket">要接收数据的套接字</param>
        private void ReceiveData(byte[] recvBuffer, int offset, int bytesToRead, Socket socket)
        {
            //当前已接收到的字节数
            int totalRecvLen = 0;
            //循环接收数据，确保接收完指定字节数
            while (totalRecvLen < bytesToRead)
            {
                int recvLen = socket.Receive(recvBuffer, offset + totalRecvLen, bytesToRead - totalRecvLen, SocketFlags.None);
                if (recvLen <= 0)
                {
                    //远程主机使用close或shutdown关闭连接，并且所有数据已被接收的时候此处不会抛异常而是立即返回0，
                    //为避免出现此情况将导致该函数死循环，此处直接抛SocketException异常
                    //10054:远程主机强迫关闭了一个现有连接
                    throw new SocketException(10054);
                }
                totalRecvLen += recvLen;
            }
        }

        #region 生成数据

        // 模拟扫描数据
        private byte[] GetScanData()
        {
            ScanStatus scanStatus = new ScanStatus();
            scanStatus.SignalNone = 0;
            scanStatus.SignalAverage = 0;
            scanStatus.SignalEnd = 0;
            scanStatus.SignalStart = 0;
            scanStatus.DfAlt = (uint)EDfAlt.DFALT_CORRELATION;
            scanStatus.AntPol = (uint)EAntPol.ANTPOL_AUTO;
            scanStatus.Elevation = 0;
            scanStatus.Squelch = 0;
            scanStatus.Overflow = 0;
            scanStatus.Valid = 1;

            ScanData scanData = new ScanData();

            scanData.BigTime = Utils.ParseTime2TwoWords(DateTime.Now);
            scanData.ScanStatus = scanStatus;
            scanData.JobId = 0;
            scanData.AzimCompass = 1 * 100;
            scanData.FirstChannel = 1;
            scanData.Count = 500;
            //int dLen = 500;
            //if (scanData.Count < _position + 500)
            //{
            //    dLen = scanData.Count - _position;
            //}
            ScanChannel[] datas = new ScanChannel[500];
            for (int i = 0; i < 500; i++)
            {
                ScanChannel scanChannel = new ScanChannel();
                scanChannel.TimeOffset = 100;
                scanChannel.SignalDuration = 10;
                scanChannel.Azimuth = (ushort)(_random.Next(0, 359) * 100);
                scanChannel.Elevation = 1 * 100;
                scanChannel.Level = (short)(_random.Next(0, 20) * 100);
                scanChannel.Quality = (short)_random.Next(0, 50);
                if (i > 220 && i < 280)
                {
                    scanChannel.Level = (short)(_random.Next(40, 50) * 100);
                    scanChannel.Quality = (short)_random.Next(80, 99);
                    scanChannel.Azimuth = (ushort)(_random.Next(100, 110) * 100);
                }
                datas[i] = scanChannel;
            }
            scanData.MeasChan = datas;

            byte[] buffer = scanData.ToBytes();
            EbdMsg ebdMsg = new EbdMsg();
            ebdMsg.MessageType = EDataOut.SCAN_DATA;
            ebdMsg.Datas = buffer;
            byte[] sendData = ebdMsg.ToBytes();
            return sendData;
        }
        // 模拟FFM数据
        private byte[] GetFFMData()
        {
            FfmData ffmData = new FfmData();
            ffmData.BigTime = Utils.ParseTime2TwoWords(DateTime.Now);
            ffmData.FfmStatus = new FfmStatus()
            {
                SignalNone = 0,
                SignalAverage = 0,
                SignalEnd = 0,
                SignalStart = 0,
                DfAlt = 0,
                AntPol = 0,
                Elevation = 1,
                Squelch = 0,
                Overflow = 0,
                Valid = 1
            };
            ffmData.JobId = 0;
            ffmData.Channel = 0;
            ffmData.AzimCompass = (uint)_random.Next(1000, 1100);
            ffmData.Level = (short)_random.Next(4000, 5000);
            ffmData.FieldStrength = (short)_random.Next(0, 10000);
            ffmData.Azimuth = (ushort)_random.Next(0, 35999);
            ffmData.Elevation = 8000; //(ushort)_random.Next(0, 9000);
            ffmData.AzimuthVar = 500;
            ffmData.Quality = (short)_random.Next(50, 99);
            byte[] buffer = ffmData.ToBytes();
            EbdMsg ebdMsg = new EbdMsg();
            ebdMsg.MessageType = EDataOut.FFM_DATA;
            ebdMsg.Datas = buffer;
            return ebdMsg.ToBytes();
        }

        // 模拟频谱数据
        private byte[] GetSpectrumData()
        {
            IfSpectrumData specData = new IfSpectrumData();
            specData.BigTime = Utils.ParseTime2TwoWords(DateTime.Now);
            specData.IfStatus = new IfStatus()
            {
                RealTimeBandwidth = 120000,
                Overflow = 0,
                Valid = 1
            };
            specData.JobId = 0;
            specData.Count = 202;
            specData.IfDatas = new IfData[101];
            for (int i = 0; i < 202 / 2; i++)
            {
                IfData data = new IfData();
                data.D0 = (short)_random.Next(-1000, 2000);
                data.D1 = (short)_random.Next(-1000, 2000);
                if (i == 100)
                {
                    data.D0 = (short)_random.Next(4000, 4500);
                    data.D1 = (short)_random.Next(4000, 4500);
                }
                specData.IfDatas[i] = data;
            }
            byte[] buffer = specData.ToBytes();
            EbdMsg ebdMsg = new EbdMsg();
            ebdMsg.MessageType = EDataOut.IF_SPECTRUM_DATA;
            ebdMsg.Datas = buffer;
            return ebdMsg.ToBytes();
        }

        // 模拟GPS数据
        private byte[] GetGpsData()
        {
            GpsData gpsData = new GpsData();
            gpsData.BigTime = Utils.ParseTime2TwoWords(DateTime.Now);
            gpsData.GpsStatus = new GpsStatus()
            {
                Valid = 1
            };
            gpsData.NoOfSatInView = (ushort)_random.Next(0, 12);
            gpsData.HorDilution = 100;
            gpsData.LatMin = (float)(_random.NextDouble() * 60);
            gpsData.LonMin = (float)(_random.NextDouble() * 60);
            gpsData.LonRef = Encoding.ASCII.GetBytes("E").First();
            gpsData.LatRef = Encoding.ASCII.GetBytes("N").First();
            gpsData.LonDeg = (byte)_random.Next(0, 180);
            gpsData.LatDeg = (byte)_random.Next(0, 90);
            gpsData.SetGpsTime(DateTime.Now);
            byte[] buffer = gpsData.ToBytes();
            EbdMsg ebdMsg = new EbdMsg();
            ebdMsg.MessageType = EDataOut.GPS_DATA;
            ebdMsg.Datas = buffer;
            return ebdMsg.ToBytes();
        }

        #endregion 生成数据

        #region 解析数据

        private void AnalysisRecvData(byte[] buffer, int length)
        {
            try
            {
                byte[] data = new byte[length];
                Buffer.BlockCopy(buffer, 0, data, 0, length);
                ParameterMessage parameterMessage = new ParameterMessage(data, 0);

                Console.WriteLine(string.Format("{0:HH:mm:ss.fff} 解析成功:{1}", DateTime.Now, parameterMessage.MsgMode));
                if (parameterMessage.MsgMode == EDataMode.MODE_STANDBY)
                {
                    // 停止
                    _isSend = false;
                    return;
                }
                else
                {
                    _isSend = true;
                    _nowSendMode = parameterMessage.MsgMode;
                    if (_lastParameter == null ||
                        _lastParameter.MsgMode != parameterMessage.MsgMode ||
                        !_lastParameter.Parameter.Equals(parameterMessage.Parameter))
                    {
                        _lastParameter = parameterMessage;
                    }
                }
            }
            catch (Exception ex)
            {
                _isSend = false;
                Console.WriteLine(string.Format("{0:HH:mm:ss.fff} 解析失败:{1}", DateTime.Now, ex.ToString()));
            }
        }

        #endregion 解析数据

        #region 测试发送参数

        private void TestSendParam()
        {
            TcpClient client = new TcpClient();
            client.Connect("127.0.0.1", 9999);
            byte[] buffer = GetParamData();
            Socket socket = client.Client;
            socket.Send(buffer);

            Thread.Sleep(10000);
            ParameterMessage parameter = new ParameterMessage();
            parameter.MsgMode = EDataMode.MODE_STANDBY;
            buffer = parameter.ToBytes();
            socket.Send(buffer);

        }

        private byte[] GetParamData()
        {
            ParameterMessage parameter = new ParameterMessage();
            parameter.MsgMode = EDataMode.MODE_FFM;
            FfmParams ffmParams = new FfmParams();
            ffmParams.Frequency = (uint)(101.7 * 1000000);
            ffmParams.CommParams = new CommParams()
            {
                AfBandWidth = 120000,
                DfBandWdith = 100000,
                AfThreshold = 40,
                AverageMode = 2,
                AfDemod = 3,
                AntPol = 1,
                Bfo = 50,
                DfMethod = 12,
                ReadMode = 23,
                ReadTime = 100,
                SampleMode = 200,
                SpectrumTime = 110,
                Threshold = 50
            };
            parameter.Parameter = ffmParams;
            return parameter.ToBytes();
        }


        #endregion 测试发送参数

    }
}
