/*********************************************************************************************
 *	
 * 文件名称:    ..\Tracker800\Server\Source\Device\Receiver\MR3300A\MR3300A.cs
 *
 * 作    者:		陈鹏 
 *	
 * 创作日期:    2018/05/17
 * 
 * 修    改:    无
 * 
 * 备    注:		MR3000A系列接收机逻辑控制
 *                                            
*********************************************************************************************/

// #define WRITE_SCAN
#define WRITE_DEBUG_INFO

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Xml;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Common;
using System.Collections.Concurrent;
using System.Reflection;
using System.Net.NetworkInformation;

namespace MR3300A
{
    public partial class MR3300A : IDevice
    {
        #region 成员变量

        public Guid ID { get; private set; }

        /// <summary>
        /// 当前任务状态
        /// </summary>
        protected volatile TaskState _taskState = TaskState.Stop;

        /// <summary>
        /// 当前正在执行的功能
        /// </summary>
        protected SpecificAbility _curAbility = SpecificAbility.Unknown;

        private readonly int MAX_DDC_COUNT = 32;

        // 极小值，用于浮点数比较大小	
        private readonly double EPSILON = 1.0E-7d;
        // 数据/网络通道/数据处理线程标识符，分别标识音频、DDC、普通业务数据
        private readonly string[] DATA_IDENTIFIERS = new string[] { "audio", "ddc", "data", "device" };
        // 模块配置文件
        private readonly string CONFIG_PATH = @"Device\MR3300A.ini";

        //
        // 同步锁
        private object _ctrlChannelLock = new object(); // 控制通道锁
        private object _identifierLock = new object(); // 数据标识同步锁 
        private object _parameterLock = new object(); // 参数同步锁
        private object _invalidCountLock = new object(); // 无效扫描数据同步锁
        private object _levelSpectrumLock = new object(); // 电平频谱同步锁
        private object _gpsLock = new object(); // GPS数据同步锁

        //
        // 数据通道与队列
        private Socket _ctrlChannel; // 指令发送与查询通道（TCP）
                                     // 通过属性访问控制通道（同步锁）
        private Socket WritingChannel { get { lock (_ctrlChannelLock) { return _ctrlChannel; } } }

        private IDictionary<string, Socket> _channels; // 数据回传通道集合
        private IDictionary<string, Thread> _captures; // 数据接收线程集合
        private IDictionary<string, Thread> _dispatches; // 数据转发线程集合
        private IDictionary<string, ConcurrentQueue<byte[]>> _queues; // 数据队列集合

        //
        // DDC
        private IFMCHTemplate[] _prevDDCSettings; // 缓存前一次DDC参数设置

        //
        // 业务数据
        private DataType _subscribedData; // 当前订阅的主通道业务数据

        //
        // 音频相关
        private AudioConvert _audioConverter; // 音频转换器
        private long _audioSampleRate; // 当前输出的音频数据采样率，即当前_audioConverter中对应的采样率

        // 
        // 扫描配置
        private int _invalidCountConfig; // 无效次数
        private int _invalidScanCount;  // 无效扫描次数
        private int _scanDataLength; // 频段离散总点数

        // 修正值
        private float _levelCalibration; // 电平修正值
        private IDictionary<long, long> _frequencyOffsetDic; // 频率修正表
        private IDictionary<long, long> _reverseFrequencyOffsetDic; // 频率逆向修正表

        private SDataGPS _bufferedGPS; // 缓存GPS数据
        private DateTime _preGPSTimeStamp; // 缓存GPS时间戳

        public event DataArrivedDelegate DataArrivedEvent;
        public event DeviceStatusChangedDelegate DeviceStatusChangedEvent;

#if WRITE_SCAN
		private Stream _stream;
		private StreamWriter _writer;
#endif

        #endregion

        #region 构造函数

        public MR3300A()
        {
            ID = Guid.NewGuid();

            _ip = Utils.ReadIniFiles("Client", "IP", "127.0.0.1", "MR3300AConfig.ini");
            _port = Utils.ReadIniFiles("Client", "Port", 5025, "MR3300AConfig.ini");
            _enableGPS = Utils.ReadIniFiles("Client", "EnableGPS", false, "MR3300AConfig.ini");
            _enableCompass = Utils.ReadIniFiles("Client", "EnableCompass", false, "MR3300AConfig.ini");
            _compassInstallingAngle = Utils.ReadIniFiles("Client", "CompassInstallingAngle", 0, "MR3300AConfig.ini");
        }

        #endregion

        #region IDevice

        // 初始化
        public bool Initialize()
        {
            try
            {
                InitMiscs();
                InitNetworks();
                InitAntennas();
                InitChannels();
                InitThreads();

                SetHeartBeat(WritingChannel);

                return true;
            }
            catch
            {
                //检查非托管资源并释放
                ReleaseResource();

                throw;
            }
        }

        // 启动测量功能
        public bool Start()
        {
            InitChannels();
            ClearAll();
            PreSet();
            SetDataByAbility();
            PostSet();
            RequestTask(_subscribedData);

            return true;
        }

        // 停止测量功能
        public bool Stop()
        {
            PreReset();
            ResetDataByAbility();
            CancelTask();
            ClearAll();
            PostReset();

            return true;
        }

        // 修改测量功能参数
        public void SetParameter(string name, object value)
        {
            if (_taskState == TaskState.Start)
            {
                // 当前做法纯属胡扯
                RequestTask(DataType.None);
                Thread.Sleep(100);
                SetParameterValue(name, value);
                RequestTask(_subscribedData);
            }
            else
            {
                SetParameterValue(name, value);
            }

            if (_taskState == TaskState.Start)
            {
                // 单频测量/测向或类单频功能，在参数有变更的情况下都需要清理缓存，保证实时数据的实时响应
                if ((_curAbility & (SpecificAbility.SCAN | SpecificAbility.FastScan | SpecificAbility.MSCAN | SpecificAbility.FSCNE | SpecificAbility.MSCNE)) == 0)
                {
                    ClearAll();
                }
            }
        }

        // 销毁资源
        public void Close()
        {
            Dispose();
        }

        public void SetParameters(Dictionary<string, object> parameters)
        {
            foreach (var pairs in parameters)
            {
                SetParameter(pairs.Key, pairs.Value);
            }
        }

        public object GetParameter(string name)
        {
            PropertyInfo property = this.GetType().GetProperty(name);
            if (property == null)
            {
                string info = string.Format("获取参数值 {0} 错误:, 未找到名称为 {0} 的参数", name);
                throw new Exception(info);
            }

            try
            {
                object value = property.GetValue(this, null);
                return value;
            }
            catch (Exception ex)
            {
                string info = string.Format("获取参数值 {0} 错误: {1}", name, ex.Message);
                Exception ex1 = new Exception(info, ex);
                throw ex1;
            }
        }

        public void Dispose()
        {
            ReleaseResource();
        }

        #region 事件

        private void OnDeviceStateChanged(DeviceStatus status, string message)
        {
            if (DeviceStatusChangedEvent != null)
            {
                DeviceStatusChangedEvent(status, message);
            }
        }

        private void OnDataArrived(List<object> data)
        {
            if (DataArrivedEvent != null)
            {
                DataArrivedEvent(data);
            }
        }

        #endregion 事件

        #endregion IDevice

        #region 心跳检测

        protected void SetHeartBeat(Socket tcpSocket)
        {
            if (tcpSocket == null)
            {
                return;
            }

            //设置TCP-keepalive模式，若1秒钟之内没有收到探测包回复则再尝试以500ms为间隔发送10次，如果一直都没有回复则认为TCP连接已经断开（为了检测网线断连等异常情况）
            byte[] bytes = new byte[] { 0x01, 0x00, 0x00, 0x00, 0xE8, 0x03, 0x00, 0x00, 0xF4, 0x01, 0x00, 0x00 };
            tcpSocket.IOControl(IOControlCode.KeepAliveValues, bytes, null);
            tcpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

            //启动TCP连接检查线程
            Thread thHeartBeat = new Thread(KeepAlive);
            thHeartBeat.IsBackground = true;
            thHeartBeat.Name = "KeepAlive Thread";
            thHeartBeat.Start(tcpSocket);
        }

        /// <summary>
        /// 心跳检查线程函数
        /// 实现 tcp 连接的心跳检查；子类可重载此方法，实现其它连接方式的心跳检查
        /// </summary>
        /// <param name="connObject">tcp连接对象</param>
        protected virtual void KeepAlive(object connObject)
        {
            Socket socket = connObject as Socket;
            if (socket == null)
            {
                return;
            }

            while (true)
            {
                if (!IsSocketConnected(socket))
                {
                    break;
                }
                Thread.Sleep(1000);
            }

            OnDeviceStateChanged(DeviceStatus.Fault, "设备连接异常");
            //SendMessage(MessageDomain.Local, MessageType.DeviceRestart, "设备连接异常");
        }

        /// <summary>
        /// 检查当前TCP连接的状态
        /// </summary>
        /// <param name="socket">目标tcp连接</param>
        /// <param name="maxRetry">寻找目标连接的最大尝试次数</param>
        /// <returns></returns>
        protected bool IsSocketConnected(Socket socket, int maxRetry = 3)
        {
            if (socket == null || maxRetry == 0)
            {
                return false;
            }

            try
            {
                var localEndPoint = socket.LocalEndPoint.ToString();
                var remoteEndPoint = socket.RemoteEndPoint.ToString();
                var validConnection = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections().FirstOrDefault(item => item.LocalEndPoint.ToString().Equals(localEndPoint) && item.RemoteEndPoint.ToString().Equals(remoteEndPoint));
                if (validConnection != null)
                {
                    return validConnection.State == TcpState.Established;
                }
            }
            catch
            {
                // 理论上此处不会抛出任何异常
                // 异常:
                // T:System.Net.NetworkInformation.NetworkInformationException: Win32 函数 GetTcpTable 失败。
            }

            Thread.Sleep(1000);

            return IsSocketConnected(socket, --maxRetry);
        }

        #endregion

        #region 初始化

        // 初始化成员（杂项）
        private void InitMiscs()
        {
            //
            // 数据回传通道与队列
            _channels = new Dictionary<string, Socket>();
            _captures = new Dictionary<string, Thread>();
            _dispatches = new Dictionary<string, Thread>();
            _queues = new Dictionary<string, ConcurrentQueue<byte[]>>();

            _preGPSTimeStamp = DateTime.MinValue;
            _bufferedGPS = new SDataGPS();

            //
            // 预定义的DDC通道
            _prevDDCSettings = new IFMCHTemplate[MAX_DDC_COUNT];
            for (var index = 0; index < _prevDDCSettings.Length; ++index)
            {
                _prevDDCSettings[index] = new IFMCHTemplate
                {
                    Frequency = 101.7d,
                    IFBandWidth = 120.0d,
                    DemMode = DemoduMode.FM,
                    IFSwitch = false
                };
            }

            //
            // 业务数据
            _subscribedData = DataType.None;
            _frequencyOffsetDic = new Dictionary<long, long>();
            _reverseFrequencyOffsetDic = new Dictionary<long, long>();

            //
            // 配置
            _invalidCountConfig = 1;    // 默认值
            var configFile = System.AppDomain.CurrentDomain.BaseDirectory + CONFIG_PATH;
            if (!File.Exists(configFile))
            {
                if (!File.Exists(configFile))
                {
                    File.WriteAllLines(configFile, new string[] {
                    "invalid=1",
                    "calib=0",
                    "df_calib=0",
                    "frequency_pair=0,0;1,1" });
                }
            }
            var configLines = File.ReadLines(configFile).ToArray();
            foreach (var line in configLines)
            {
                var config = line.ToLower().Split(new char[] { '=' });
                if (config == null || config.Length != 2)
                {
                    continue;
                }

                switch (config[0].Trim())
                {
                    case "invalid":
                        {
                            if (int.TryParse(config[1].Trim(), out _invalidCountConfig))
                            {
                                if (_invalidCountConfig < 0)
                                {
                                    _invalidCountConfig = 0;
                                }
                                else if (_invalidCountConfig > 20)
                                {
                                    _invalidCountConfig = 20;
                                }
                            }
                        }
                        break;
                    case "calib":
                        {
                            float.TryParse(config[1].Trim(), out _levelCalibration);
                        }
                        break;
                    case "frequency_pair":
                        {
                            var pairs = config[1].Trim().Split(new char[] { ';' });
                            if (pairs == null)
                            {
                                break;
                            }
                            foreach (var pair in pairs)
                            {
                                try
                                {
                                    var keyValue = pair.Trim().Split(new char[] { ',' });
                                    if (keyValue == null || keyValue.Length != 2)
                                    {
                                        continue;
                                    }
                                    var key = long.Parse(keyValue[0].Trim());
                                    var value = long.Parse(keyValue[1].Trim());

                                    _frequencyOffsetDic[key] = value;
                                    _reverseFrequencyOffsetDic[value] = key;
                                }
                                catch { }
                            }
                        }
                        break;
                    default:
                        break;
                }

            }

            _invalidScanCount = _invalidCountConfig;

        }

        // 初始化网络环境
        private void InitNetworks()
        {
            _ctrlChannel = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _ctrlChannel.NoDelay = true;
            _ctrlChannel.Connect(_ip, _port);

            var endPoint = _ctrlChannel.LocalEndPoint as IPEndPoint;
            if (endPoint == null)
            {
                throw new Exception("无可用网络地址");
            }

            foreach (var identifier in DATA_IDENTIFIERS)
            {
                _channels[identifier] = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                _channels[identifier].SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                _channels[identifier].Bind(new IPEndPoint(endPoint.Address, 0));
                _channels[identifier].Connect(_ip, 0);
            }

            // 注意，千万不要注释掉！！！
            // 关闭回显（没有收指令结果的需要）。PS：这个很奇葩，查询才显示结果啊，不是查询回显个蛋啊！SCPI协议也不是这么定义的嘛-_-!!额~，先这样吧！
            SendCommand("SYST:COMM:ECHO OFF");

            // 这个enable在V9没有，测试是否要删除
            if (_enableGPS)
            {
                SendCommand("HARD:GPS:ENAB ON");
            }
            else
            {
                SendCommand("HARD:GPS:ENAB OFF");
            }
        }

        // 初始化天线配置（添加监测和测向天线）
        private void InitAntennas()
        {
            //
            // 添加天线数量
            var antennaCount = 0;
            List<AntennaInfo> ants = Utils.GetAntennaInfos(ANTENNA_CONFIG_FILE);
            _monitorAntennas = Array.ConvertAll(ants.ToArray(), ant => AntennaInfoEx.Create(ant));
            if (_monitorAntennas != null)
            {
                antennaCount += _monitorAntennas.Length;
            }
            if (antennaCount > 0)
            {
                SendCommand(string.Format("ANT:COUN {0}", antennaCount));
            }

            var exception = string.Empty;
            byte antennaIndex = 0;
            //
            // 验证天线码合法性，并配置监测天线
            if (VerifyMonitorAntennas(out exception, ref antennaIndex))
            {
                foreach (var antenna in _monitorAntennas)
                {
                    SendCommand(string.Format("ANT:APP {0}", antenna.ToProtocolFormat()));
                }
            }
            else if (!string.IsNullOrEmpty(exception))
            {
                throw new Exception(string.Format("监测天线配置有误，{0}", exception));
            }
        }

        // 初始化数据回传通道
        private void InitChannels()
        {
            var address = (_ctrlChannel.LocalEndPoint as IPEndPoint).Address.ToString();
            var audioPort = (_channels["audio"].LocalEndPoint as IPEndPoint).Port;
            var ddcPort = (_channels["ddc"].LocalEndPoint as IPEndPoint).Port;
            var dataPort = (_channels["data"].LocalEndPoint as IPEndPoint).Port;
            var devicePort = (_channels["device"].LocalEndPoint as IPEndPoint).Port;

            SendCommand("TRAC OFF");
            // 常规测量通道
            SendCommand(string.Format("TRAC:UDP \"{0}\",{1},MEAS", address, dataPort));
            // 扫描通道
            SendCommand(string.Format("TRAC:UDP \"{0}\",{1},SCAN", address, dataPort));
            // 音频通道
            SendCommand(string.Format("TRAC:UDP \"{0}\",{1},AUDIO", address, audioPort));
            // 中频通道
            SendCommand(string.Format("TRAC:UDP \"{0}\",{1},IF", address, dataPort));
            // 测向通道
            SendCommand(string.Format("TRAC:UDP \"{0}\",{1},DFIN", address, dataPort));
            // DDC通道
            SendCommand(string.Format("TRAC:UDP \"{0}\",{1},DDC", address, ddcPort));
            // 硬件信息通道（GPS/电子罗盘）
            SendCommand(string.Format("TRAC:UDP \"{0}\",{1},HW", address, devicePort));
        }

        // 初始化收数据和转换数据线程
        private void InitThreads()
        {
            foreach (var identifier in DATA_IDENTIFIERS)
            {
                _captures[identifier] = new Thread(CapturePacket) { IsBackground = true, Name = string.Format("{0}_capture", identifier) };
                _captures[identifier].Start(identifier);

                _dispatches[identifier] = new Thread(DispatchPacket) { IsBackground = true, Name = string.Format("{0}_dispatch", identifier) };
                _dispatches[identifier].Start(identifier);
            }
        }

        #endregion

        #region 功能设置

        // 设置功能和业务数据
        private void SetDataByAbility()
        {
            if ((_curAbility & SpecificAbility.FixFQ) > 0)
            {
                SetFixFQ();
            }
            if ((_curAbility & SpecificAbility.FastScan) > 0)
            {
                SetFastScan();
            }
            if ((_curAbility & (SpecificAbility.SCAN | SpecificAbility.FSCNE)) > 0)
            {
                SetScan();
            }
            if ((_curAbility & (SpecificAbility.MSCAN | SpecificAbility.MSCNE)) > 0)
            {
                SetMScan();
            }
            if ((_curAbility & SpecificAbility.IFMultiChannel) > 0)
            {
                SetDDC();
            }
            if ((_curAbility & SpecificAbility.TDOA) > 0)
            {
                SetTDOA();
            }
        }

        // 重置功能和业务数据
        private void ResetDataByAbility()
        {
            if ((_curAbility & SpecificAbility.FixFQ) > 0)
            {
                ResetFixFQ();
            }
            if ((_curAbility & SpecificAbility.FastScan) > 0)
            {
                ResetFastScan();
            }
            if ((_curAbility & (SpecificAbility.SCAN | SpecificAbility.FSCNE)) > 0)
            {
                ResetScan();
            }
            if ((_curAbility & (SpecificAbility.MSCAN | SpecificAbility.MSCNE)) > 0)
            {
                ResetMScan();
            }
            if ((_curAbility & SpecificAbility.IFMultiChannel) > 0)
            {
                ResetDDC();
            }
            if ((_curAbility & SpecificAbility.TDOA) > 0)
            {
                ResetTDOA();
            }

            // 数据请求置空
            _subscribedData = DataType.None;
        }

        // 预先设置
        private void PreSet()
        {
            // 默认开启静噪开关
            SendCommand("MEAS:SQU ON");
        }

        // 后置设置
        private void PostSet()
        {
            if ((_curAbility & SpecificAbility.FixFQ) > 0)
            {
                SendCommand("FREQ:MODE FIX");
            }
            if ((_curAbility & (SpecificAbility.FixDF | SpecificAbility.WBDF)) > 0)
            {
                SendCommand("FREQ:MODE DFIN");
            }
            if ((_curAbility & SpecificAbility.IFMultiChannel) > 0)
            {
                SendCommand("DEM FM"); // 尽管主通道不进行音频解调，但是主通道依然受到该参数影响，这此也仅仅是容错处理，最终解决方案还是该依赖接收机
                SendCommand("FREQ:MODE DDC");
                Thread.Sleep(5000); // 按天津接收机团队的要求，从任何功能切换进DDC时，延时5秒
            }
        }

        // 预先复位
        private void PreReset()
        {
            // deliberately left blank
        }

        // 后置复位
        private void PostReset()
        {
            if (_curAbility == SpecificAbility.SCAN)
            {
                Thread.Sleep(20);
            }
            else if (_curAbility == SpecificAbility.IFMultiChannel) // 按天津接收机开发团队的要求，从DDC功能切换出来，需要延时5秒后才能进行其它功能的测量
            {
                Thread.Sleep(5000);
            }
        }

        // 设置单频测量
        private void SetFixFQ()
        {
            // 单频测量默认需要向接收机请求电平数据和短信数据
            _subscribedData |= (DataType.LEVEL | DataType.SMS);
        }

        // 重置单频测量
        private void ResetFixFQ()
        {
            // deliberately left blank
        }

        // 设置快速扫描
        private void SetFastScan()
        {
            SendCommand("FREQ:MODE PSC");
            SendCommand("FREQ:PSC:MODE FAST");  // 快速扫描属于PSCAN的子模式

            // 按MR3300A接收机开发者的要求，扫描参数（起始频率、結束频率、扫描步进）需要后于频率模式设置
            SendCommand(string.Format("FREQ:START {0} MHz", _startFrequency));
            SendCommand(string.Format("FREQ:STOP {0} MHz", _stopFrequency));
            SendCommand(string.Format("FREQ:STEP {0} kHz", _stepFrequency));

            _scanMode = ScanMode.PSCAN;
            _subscribedData |= DataType.SCAN;
        }

        // 重置快速扫描
        private void ResetFastScan()
        {
            // delibrately left blank
        }

        // 设置频段扫描/频段搜索
        private void SetScan()
        {
            if ((_curAbility & SpecificAbility.SCAN) > 0) // 频段扫描
            {
                if (_scanMode == ScanMode.PSCAN)    // 此参数已暴露到功能
                {
                    SendCommand("FREQ:MODE PSC");
                    SendCommand("FREQ:PSC:MODE NORM"); // 常规的PSCAN 
                }
                else if (_scanMode == ScanMode.FSCAN)
                {
                    SendCommand("FREQ:MODE SWE");
                }
                // 
                // 当前功能不包含等待时间和驻留时间，因此需要隐式向接收机设置，全部置为零
                // SendCommand("MEAS:HOLD 0");
                SendCommand("MEAS:DWEL 0");
            }
            else
            {
                // 频段搜索采用FSCAN的模式
                SendCommand("FREQ:MODE SWE");
                // SendCommand("MEAS:HOLD 0"); // 按业务需求，将等待时间设置为零（内部依赖测量时间）
                _scanMode = ScanMode.FSCAN; // 此参数未暴露到用户能表，因此手动更新
            }

            // 按MR3300A接收机开发者的要求，扫描参数（起始频率、結束频率、扫描步进）需要后于频率模式设置
            SendCommand(string.Format("FREQ:START {0} MHz", _startFrequency));
            SendCommand(string.Format("FREQ:STOP {0} MHz", _stopFrequency));
            SendCommand(string.Format("FREQ:STEP {0} kHz", _stepFrequency));

            _scanDataLength = Utils.GetTotalCount(_startFrequency, _stopFrequency, _stepFrequency);
            _subscribedData |= DataType.SCAN;
        }

        // 重置频段扫描/频段搜索
        private void ResetScan()
        {
            // delibrately left blank
        }

        // 设置离散扫描/离散搜索
        private void SetMScan()
        {
            SendCommand("FREQ:MODE MSC");
            // SendCommand("MEAS:HOLD 0"); // 按业务需求，将等待时间设置为零（内部依赖测量时间）
            SendCommand(string.Format("MSC:COUN {0}", _frequencies.Length));
            for (var index = 0; index < _frequencies.Length; ++index)
            {
                SendCommand(string.Format("MEM:CONT {0},{1} MHz,{2} kHz,{3}",
                    index,
                    _frequencies[index]["Frequency"],
                    _frequencies[index]["IFBandWidth"],
                    _frequencies[index]["DemMode"]
                    ));
            }
            if ((_curAbility & SpecificAbility.MSCAN) > 0)  // 如果是离散扫描，等待时间和驻留时间需要隐式设置到接收机，取值为零
            {
                SendCommand("MEAS:DWEL 0");
            }
            _scanMode = ScanMode.MSCAN;

            _scanDataLength = _frequencies.Length;
            _subscribedData |= DataType.SCAN;
        }

        // 重置离散扫描/离散搜索
        private void ResetMScan()
        {
            // clear all mscan frequencies
        }

        // 设置DDC
        private void SetDDC()
        {
            for (var index = 0; index < _ifMultiChannels.Length; ++index)
            {
                var frequency = (double)_ifMultiChannels[index]["Frequency"];
                var ifBandwidth = (double)_ifMultiChannels[index]["IFBandWidth"];
                var demMode = (DemoduMode)_ifMultiChannels[index]["DemMode"];
                var ifSwitch = (bool)_ifMultiChannels[index]["IFSwitch"];
                var levelSwitch = (bool)_ifMultiChannels[index]["LevelSwitch"];
                var spectrumSwitch = (bool)_ifMultiChannels[index]["SpectrumSwitch"];
                var audioSwitch = (bool)_ifMultiChannels[index]["AudioSwitch"];

                // 所有数据都一样，则不需要进行任何变更
                if (Math.Abs(frequency - _prevDDCSettings[index].Frequency) <= EPSILON
                    && Math.Abs(ifBandwidth - _prevDDCSettings[index].IFBandWidth) <= EPSILON
                    && demMode == _prevDDCSettings[index].DemMode
                    && ifSwitch == _prevDDCSettings[index].IFSwitch
                    && levelSwitch == _prevDDCSettings[index].LevelSwitch
                    && spectrumSwitch == _prevDDCSettings[index].SpectrumSwitch
                    && audioSwitch == _prevDDCSettings[index].AudioSwitch
                    )
                {
                    continue;
                }

                var dataType = DataType.None;
                if (ifSwitch) // 相当于数据总开关，如果为False，数据类型默认为None
                {
                    dataType = DataType.LEVEL | DataType.SPECTRUM | DataType.AUDIO;
                }

                SendCommand(string.Format("DDC:CONT {0},{1} MHz,{2} kHz,{3},{4}",
                    index,
                    frequency,
                    ifBandwidth,
                    demMode,
                    dataType.ToString().Replace(", ", "|")
                    ));

                // 更新缓存的DDC通道参数
                _prevDDCSettings[index].Frequency = frequency;
                _prevDDCSettings[index].IFBandWidth = ifBandwidth;
                _prevDDCSettings[index].DemMode = demMode;
                _prevDDCSettings[index].IFSwitch = ifSwitch;
                _prevDDCSettings[index].LevelSwitch = levelSwitch;
                _prevDDCSettings[index].SpectrumSwitch = spectrumSwitch;
                _prevDDCSettings[index].AudioSwitch = audioSwitch;
            }

            _subscribedData |= DataType.SPECTRUM;
        }

        // 重置DDC
        private void ResetDDC()
        {
            for (var index = 0; index < _prevDDCSettings.Length && index < _ifMultiChannels.Length; ++index)
            {
                SendCommand(string.Format("DDC:CONT {0},{1} MHz,{2} kHz,{3},None",
                    index,
                    _prevDDCSettings[index].Frequency,
                    _prevDDCSettings[index].IFBandWidth,
                    _prevDDCSettings[index].DemMode));

                _prevDDCSettings[index].IFSwitch = false;
            }
        }

        // 设置TDOA测量
        private void SetTDOA()
        {
            _subscribedData |= DataType.TDOA;
        }

        // 重置TDOA测量
        private void ResetTDOA()
        {
            // deliberately left blank
        }

        #endregion

        #region 任务处理

        // 设置参数
        private void SetParameterValue(string name, object value)
        {
            PropertyInfo property = this.GetType().GetProperty(name);
            if (property == null)
            {
                string info = string.Format("设置参数 {0} = {1} 错误:, 未找到名称为 {2} 的参数", name, value.ToString(), name);
                throw new Exception(info);
            }

            try
            {
                property.SetValue(this, value, null);
            }
            catch (Exception ex)
            {
                string info = string.Format("设置参数 {0} 错误: {1}", name, value.ToString());
                Exception ex1 = new Exception(info, ex);
                throw ex1;
            }
        }

        // 接受业务数据请求
        private void AcceptDataRequest(DataType dataType)
        {
            _subscribedData |= dataType;
            if (_taskState == TaskState.Start)
            {
                RequestTask(_subscribedData);
            }
        }

        // 拒绝业务数据请求
        private void RejectDataRequest(DataType dataType)
        {
            _subscribedData &= ~dataType;
            if (_taskState == TaskState.Start)
            {
                RequestTask(_subscribedData);
            }
        }

        // 请求任务执行
        private void RequestTask(DataType dataType)
        {
            if (dataType == DataType.None && (_curAbility & SpecificAbility.IFMultiChannel) == 0)
            {
                SendCommand("TRAC OFF");
                return;
            }

            // 切换单频与信号识别
            if ((_curAbility & SpecificAbility.FixFQ) > 0)
            {
                if ((dataType & DataType.IQ) > 0)
                {
                    dataType = DataType.IQ;
                    SendCommand("FREQ:MODE SD");
                }
                else
                {
                    SendCommand("FREQ:MODE FIX");
                }
            }
            if ((_curAbility & SpecificAbility.TDOA) > 0)
            {
                dataType = DataType.TDOA;
            }

            lock (_invalidCountLock)
            {
                if ((dataType & DataType.SCAN) > 0)
                {
                    _invalidScanCount = _invalidCountConfig;
                }
            }

            if (dataType != DataType.None)
            {
                SendCommand(string.Format("TRAC:MED {0}", dataType.ToString().Replace(", ", ",")));
            }
            SendCommand("TRAC ON");
        }

        // 取消任务执行
        private void CancelTask()
        {
            SendCommand("TRAC OFF");
        }

        // 清空数据
        private void ClearAll()
        {
            foreach (var identifier in DATA_IDENTIFIERS)
            {
                if (_queues.ContainsKey(identifier) && _queues[identifier] != null)
                {
                    while (!_queues[identifier].IsEmpty)
                    {
                        byte[] datas;
                        _queues[identifier].TryDequeue(out datas);
                    }
                }
            }
        }

        #endregion

        #region 资源释放

        // 释放资源
        private void ReleaseResource()
        {
            ReleaseThreads();
            ReleaseNetworks();
            ReleaseQueues();
        }

        // 销毁线程资源
        private void ReleaseThreads()
        {
            foreach (var identifier in DATA_IDENTIFIERS)
            {
                if (_captures.ContainsKey(identifier) && _captures[identifier] != null && _captures[identifier].IsAlive)
                {
                    try { _captures[identifier].Abort(); }
                    catch { }
                    _captures[identifier] = null;
                }

                if (_dispatches.ContainsKey(identifier) && _dispatches[identifier] != null && _dispatches[identifier].IsAlive)
                {
                    try { _dispatches[identifier].Abort(); }
                    catch { }
                    _dispatches[identifier] = null;
                }
            }
        }

        // 销毁网络资源
        private void ReleaseNetworks()
        {
            if (_ctrlChannel != null)
            {
                try
                {
                    _ctrlChannel.Close();
                }
                catch { }
                finally
                {
                    _ctrlChannel = null;
                }
            }

            foreach (var identifier in DATA_IDENTIFIERS)
            {
                if (_channels.ContainsKey(identifier) && _channels[identifier] != null)
                {
                    try
                    {
                        _channels[identifier].Close();
                    }
                    catch { }
                    finally
                    {
                        _channels[identifier] = null;
                    }
                }
            }
        }

        // 清空缓存队列
        private void ReleaseQueues()
        {
            foreach (var identifier in DATA_IDENTIFIERS)
            {
                if (_queues.ContainsKey(identifier) && _queues[identifier] != null)
                {
                    byte[] datas;
                    while (!_queues[identifier].IsEmpty)
                    {
                        _queues[identifier].TryDequeue(out datas);
                    }
                    _queues[identifier] = null;
                }
            }
        }

        #endregion

        #region 数据接收与处理

        // 捕获数据
        private void CapturePacket(object obj)
        {
            var identifier = obj.ToString();
            ConcurrentQueue<byte[]> queue = null;
            Socket socket = null;

            lock (_identifierLock)
            {
                socket = _channels[identifier];
                if (!_queues.ContainsKey(identifier))
                {
                    _queues[identifier] = new ConcurrentQueue<byte[]>();
                }
                queue = _queues[identifier];
            }

            var buffer = new byte[1024 * 1024];
            socket.ReceiveBufferSize = buffer.Length;

            while (true)
            {
                try
                {
                    var receivedCount = socket.Receive(buffer);
                    if (receivedCount <= 0)
                    {
#if WRITE_DEBUG_INFO
                        Console.WriteLine("Received data size: {0}", receivedCount);
#endif
                        Thread.Sleep(1);
                        continue;
                    }

                    var receivedBuffer = new byte[receivedCount];
                    Buffer.BlockCopy(buffer, 0, receivedBuffer, 0, receivedCount);

                    if (_taskState == TaskState.Start || identifier.ToLower().Equals("device"))
                    {
                        queue.Enqueue(receivedBuffer);
                    }
                    else
                    {
#if WRITE_DEBUG_INFO
                        Console.WriteLine("Task has been aborted, received data size: {0}", receivedCount);
#endif
                    }
                }
                catch (Exception ex)
                {
                    if (ex is ThreadAbortException)
                    {
                        return;
                    }
                    else if (ex is SocketException)
                    {
                        Thread.Sleep(10);
                        continue;
                    }
                    else
                    {
                        //var item = new LogItem(ex);
                        //LogManager.Add(item);
                    }
                }
            }
        }

        // 派发数据
        private void DispatchPacket(object obj)
        {
            var identifier = obj.ToString();
            ConcurrentQueue<byte[]> queue = null;

            lock (_identifierLock)
            {
                if (!_queues.ContainsKey(identifier))
                {
                    _queues[identifier] = new ConcurrentQueue<byte[]>();
                }
                queue = _queues[identifier];
            }

            while (true)
            {
                try
                {
                    byte[] buffer;
                    queue.TryDequeue(out buffer);
                    if (buffer == null)
                    {
                        Thread.Sleep(1);
                        continue;
                    }

                    var packet = RawPacket.Parse(buffer, 0);
                    if (packet.DataCollection.Count <= 0)
                    {
                        Thread.Sleep(1);
                        continue;
                    }

                    ForwardPacket(packet);
                }
                catch (Exception ex)
                {
                    if (ex is ThreadAbortException)
                    {
                        return;
                    }
                    else
                    {
                        //                        var item = new LogItem(ex);
                        //                        LogManager.Add(item);
                        //#warning 现阶段先推送详细信息到客户端，等稳定以后再改为简略信息
                        //                        SendMessage(MessageDomain.Task, MessageType.Warning, "解析数据错误！" + ex.ToString());
                    }
                }
            }
        }

        // 转发数据
        private void ForwardPacket(RawPacket packet)
        {
            if (packet == null)
            {
                return;
            }

#if WRITE_DEBUG_INFO && DATA_INFO
			packet.DataCollection.ForEach(item =>
			{
				if ((DataType)item.Tag != DataType.GPS)
				{
					Console.WriteLine(item.ToString());
				}
			});
#endif

            var result = new List<object>();
            foreach (var data in packet.DataCollection)
            {
                switch ((DataType)data.Tag)
                {
                    case DataType.IQ:
                        {
                            result.Add(ToIQ(data));
                            lock (_parameterLock)
                            {
                                if ((_subscribedData & DataType.LEVEL) > 0)
                                {
                                    result.Add(ToLevelByIQ(data));
                                }
                                if ((_subscribedData & DataType.SPECTRUM) > 0)
                                {
                                    result.Add(ToSpectrumByIQ(data));
                                }
                            }
                        }
                        break;
                    case DataType.LEVEL:
                        result.Add(ToLevel(data));
                        break;
                    case DataType.SPECTRUM:
                        result.Add(ToSpectrum(data));
                        break;
                    case DataType.AUDIO:
                        if (data.Version == 1)
                        {
                            ProcessDDCAudio(data);
                        }
                        else
                        {
                            result.Add(ToAudio(data));
                        }
                        break;
                    case DataType.ITU:
                        result.Add(ToITU(data));
                        break;
                    case DataType.SCAN: // PSCAN
                    case DataType.SCAN + 2: // FSCAN
                    case DataType.SCAN + 4: // MSCAN
                        result.Add(ToScan(data));
                        break;
                    case DataType.TDOA:
                        result.Add(ToTDOA(data));
                        lock (_parameterLock)
                        {
                            if ((_subscribedData & DataType.LEVEL) > 0)
                            {
                                result.Add(ToLevelByIQ(data));
                            }
                            if ((_subscribedData & DataType.SPECTRUM) > 0)
                            {
                                result.Add(ToSpectrumByIQ(data));
                            }
                        }
                        break;
                    case DataType.SMS:
                        result.Add(ToSMS(data));
                        break;
                    case DataType.DDC:
                        ProcessDDC(data);
                        break;
                    case DataType.GPS:
                        if (_enableGPS)
                        {
                            ProcessGPS(data);
                        }
                        break;
                    case DataType.Compass:
                        if (_enableCompass)
                        {
                            ProcessCompass(data);
                        }
                        break;
                    case DataType.DFIND:
                    case DataType.DFPAN:
                    case DataType.DFIQ:
                    case DataType.DFC:
                    case DataType.DFC + 4:
                    default:
                        break;
                }
            }

            result = result.Where(item => item != null).ToList();
            if (result.Count > 0 && _taskState == TaskState.Start)
            {
#if WRITE_SCAN
					if (_curAbility == SpecificAbility.SCAN && _scanMode == ScanMode.PSCAN
						&& _stream != null && _writer != null)
					{
						var scan = result.Find(item => item is SDataScan) as SDataScan;

						var tick = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff");
						var gpsFormat = string.Format("{0} {1}", _bufferedGPS.Longitude, _bufferedGPS.Latitude);
						var start = scan.StartFrequency + scan.Pdloc * StepFrequency / 1000.0d;
						var stop = scan.StartFrequency + scan.StepFrequency / 1000.0d * (scan.Datas.Length - 1);
						var step = scan.StepFrequency;
						var value = string.Join(" ", scan.Datas);
						_writer.WriteLine("{0} {1} {2} {3} {4} {5}", tick, gpsFormat, start, stop, step, value);
						_writer.Flush();
					}
#endif
                OnDataArrived(result);
            }
        }

        #endregion

        #region Helper

        // 验证监测天线配置是否有误
        private bool VerifyMonitorAntennas(out string exception, ref byte antennaIndex)
        {
            exception = string.Empty;
            if (_monitorAntennas == null || _monitorAntennas.Length == 0)
            {
                return false;
            }

            foreach (var antenna in _monitorAntennas)
            {
                antenna.Index = (int)(antennaIndex++);

                var pattern = "^((00)?[0-9]|0?[0-9]{2}|1[0-9][0-9]|2[0-4][0-9]|25[0-5]),1,(0[xX])?[0-9a-fA-F]{1,2},(0[xX])?[0-9a-fA-F]{1,2}$";
                var reg = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.CultureInvariant);
                if (!reg.IsMatch(antenna))
                {
                    exception = string.Format("天线名称：{0}，天线摘要：{1}", antenna.Name, antenna.ToString());
                    return false;
                }
                if (antenna.StartFrequency > antenna.StopFrequency)
                {
                    exception = "天线频率上限不得小于下限";
                    return false;
                }
            }

            return true;
        }

        // 验证是否为有效的扫描数据
        private bool IsScanValid(object data)
        {
            var raw = data as RawScan;
            // 验证扫描模式是否匹配
            if (raw == null
                || _scanMode == ScanMode.PSCAN && raw.Tag != (int)DataType.SCAN
                || _scanMode == ScanMode.FSCAN && raw.Tag != (int)DataType.SCAN + 2
                || _scanMode == ScanMode.MSCAN && raw.Tag != (int)DataType.SCAN + 4
                || (_scanMode != ScanMode.MSCAN
                    && (Math.Abs(raw.StartFrequency / 1000000.0d - _startFrequency) > EPSILON
                        || Math.Abs(raw.StopFrequency / 1000000.0d - _stopFrequency) > EPSILON
                        || Math.Abs(raw.StepFrequency / 1000.0d - _stepFrequency) > EPSILON)
                    )
                )
            {
                return false;
            }

            return true;
        }

        // 发送指令，该操作应该是原子化的
        private void SendCommand(string cmd)
        {
            var sendBuffer = System.Text.Encoding.Default.GetBytes(cmd + "\r");
            var bytesToSend = sendBuffer.Length;
            var total = 0;

            try
            {
                // 流式套接字，循环发送，直到所有数据全部发送完毕
                while (total < bytesToSend)
                {
                    var sentBytes = _ctrlChannel.Send(sendBuffer, total, bytesToSend - total, SocketFlags.None);
                    total += sentBytes;
                }

#if WRITE_DEBUG_INFO
                Console.WriteLine(string.Format("{0} <-- {1}", DateTime.Now.ToString("HH:mm:ss"), cmd.ToLower()));
#endif
            }
            catch { } // 此处的出现的异常（套机字异常），不在向上抛，会通过别的地方（比如心跳线程）对该异常进行处理
        }

        #endregion
    }
}