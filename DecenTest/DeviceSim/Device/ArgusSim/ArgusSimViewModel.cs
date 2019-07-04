using DeviceSimlib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace DeviceSim.Device
{
    public class ArgusSim : DeviceBase
    {
        private ArgusSimView _control = null;

        private Socket _socketTcp1 = null;
        private Socket _socketTcp2 = null;
        private Socket _socketUdp = null;

        private string _localIpAddress = string.Empty;
        private int _localTcpPort = 0;

        private Thread _socket1Thread = null;
        private Thread _socket2Thread = null;
        private Thread _socketUdpThread = null;
        private Thread _sendKeepAliveThread = null;
        // 解析IQ数据线程
        private Thread _tcpDataProThread = null;
        // 解析UDP数据线程
        private Thread _udpDataProThread = null;

        // 缓存设备初始化参数
        private Dictionary<string, string> _iniParamList = new Dictionary<string, string>();

        private ConcurrentQueue<byte[]> _tcpDataCache = new ConcurrentQueue<byte[]>();
        private ConcurrentQueue<byte[]> _udpDataCache = new ConcurrentQueue<byte[]>();

        private int _iqCount = 0;
        private int _udpCount = 0;

        #region 安装参数

        private string _ipAddress = "";
        /// <summary>
        /// 连接的IP地址
        /// </summary>
        public string IpAddress
        {
            get { return _ipAddress; }
            set
            {
                _ipAddress = value;
                OnPropertyChanged(() => this.IpAddress);
            }
        }

        private int _tcpPort = 5555;
        /// <summary>
        /// 连接的端口号
        /// </summary>
        public int TcpPort
        {
            get { return _tcpPort; }
            set
            {
                _tcpPort = value;
                OnPropertyChanged(() => this.TcpPort);
            }
        }
        private int _udpPort = 4060;
        /// <summary>
        /// 连接的UDP端口号
        /// </summary>
        public int UdpPort
        {
            get { return _udpPort; }
            set
            {
                _udpPort = value;
                OnPropertyChanged(() => this.UdpPort);
            }
        }

        #endregion 安装参数

        #region 运行参数

        private float _level = 0f;

        /// <summary>
        /// 显示运行时的电平值
        /// </summary>
        [XmlIgnore]
        public float Level
        {
            get { return _level; }
            set
            {
                if (_level != value)
                {
                    _level = value;
                    OnPropertyChanged(() => this.Level);
                }
            }
        }

        private string _gpsInfo = string.Empty;

        /// <summary>
        /// 显示运行时的GPS信息
        /// </summary>
        [XmlIgnore]
        public string GpsInfo
        {
            get { return _gpsInfo; }
            set
            {
                if (_gpsInfo != value)
                {
                    _gpsInfo = value;
                    OnPropertyChanged(() => this.GpsInfo);
                }
            }
        }

        private string _iqInfo = string.Empty;

        /// <summary>
        /// 显示运行时的IQ信息
        /// </summary>
        [XmlIgnore]
        public string IQInfo
        {
            get { return _iqInfo; }
            set
            {
                if (_iqInfo != value)
                {
                    _iqInfo = value;
                    OnPropertyChanged(() => this.IQInfo);
                }
            }
        }

        #endregion 运行参数

        #region 设置参数

        private EDemMode _demMode = EDemMode.FM;
        /// <summary>
        /// 解调模式
        /// </summary>
        public EDemMode DemMode
        {
            get { return _demMode; }
            set
            {
                if (_demMode != value)
                {
                    _demMode = value;
                    OnPropertyChanged(() => this.DemMode);
                }
            }
        }

        private double _bandWidth = 150;
        /// <summary>
        /// 解调带宽/中频带宽 kHz
        /// </summary>
        public double BandWidth
        {
            get { return _bandWidth; }
            set
            {
                if (_bandWidth != value)
                {
                    _bandWidth = value;
                    OnPropertyChanged(() => this.BandWidth);
                }
            }
        }

        private EDetector _detector = EDetector.PAV;
        /// <summary>
        /// 检波方式
        /// </summary>
        public EDetector Detector
        {
            get { return _detector; }
            set
            {
                if (_detector != value)
                {
                    _detector = value;
                    OnPropertyChanged(() => this.Detector);
                }
            }
        }

        private bool _attMode = true;
        /// <summary>
        /// 衰减模式
        /// </summary>
        public bool AttenuationMode
        {
            get { return _attMode; }
            set
            {
                if (_attMode != value)
                {
                    _attMode = value;
                    OnPropertyChanged(() => this.AttenuationMode);
                }
            }
        }

        private int _attenuation = 0;
        /// <summary>
        /// 衰减值
        /// </summary>
        public int Attenuation
        {
            get { return _attenuation; }
            set
            {
                if (_attenuation != value)
                {
                    _attenuation = value;
                    OnPropertyChanged(() => this.Attenuation);
                }
            }
        }

        private ERFMode _rfMode = ERFMode.NORM;
        /// <summary>
        /// 射频模式
        /// </summary>
        public ERFMode RfMode
        {
            get { return _rfMode; }
            set
            {
                if (_rfMode != value)
                {
                    _rfMode = value;
                    OnPropertyChanged(() => this.RfMode);
                }
            }
        }

        private float _holdTime = 0f;
        /// <summary>
        /// 保持时间
        /// </summary>
        public float HoldTime
        {
            get { return _holdTime; }
            set
            {
                if (_holdTime != value)
                {
                    _holdTime = value;
                    OnPropertyChanged(() => this.HoldTime);
                }
            }
        }

        private double _spectrumSpan = 100;
        /// <summary>
        /// 频谱带宽 kHz
        /// </summary>
        public double SpectrumSpan
        {
            get { return _spectrumSpan; }
            set
            {
                if (_spectrumSpan != value)
                {
                    _spectrumSpan = value;
                    OnPropertyChanged(() => this.SpectrumSpan);
                }
            }
        }

        private EFFTMode _fftMode = EFFTMode.OFF;
        /// <summary>
        /// FFT模式
        /// </summary>
        public EFFTMode FFTMode
        {
            get { return _fftMode; }
            set
            {
                if (_fftMode != value)
                {
                    _fftMode = value;
                    OnPropertyChanged(() => this.FFTMode);
                }
            }
        }

        private double _frequency = 99.0;
        /// <summary>
        /// 中心频率 MHz
        /// </summary>
        public double Frequency
        {
            get { return _frequency; }
            set
            {
                if (_frequency != value)
                {
                    _frequency = value;
                    OnPropertyChanged(() => this.Frequency);
                }
            }
        }

        private EBandMeasureMode _measMode = EBandMeasureMode.BETA;
        /// <summary>
        /// 带宽测量模式 XDB/BETA
        /// </summary>
        public EBandMeasureMode MeasMode
        {
            get { return _measMode; }
            set
            {
                if (_measMode != value)
                {
                    _measMode = value;
                    OnPropertyChanged(() => this.MeasMode);
                }
            }
        }

        private double _xdBValue = 26.0;
        /// <summary>
        /// xdB值 :Meas:Band:XDB 26.0 dB
        /// </summary>
        public double XdBValue
        {
            get { return _xdBValue; }
            set
            {
                if (_xdBValue != value)
                {
                    _xdBValue = value;
                    OnPropertyChanged(() => this.XdBValue);
                }
            }
        }

        private double _betaValue = 1.0;
        /// <summary>
        /// beta值 :Meas:Band:Beta 1.0 ///%
        /// </summary>
        public double BetaValue
        {
            get { return _betaValue; }
            set
            {
                if (_betaValue != value)
                {
                    _betaValue = value;
                    OnPropertyChanged(() => this.BetaValue);
                }
            }
        }

        private EIfRemoteMode _iqMode = EIfRemoteMode.LONG;
        /// <summary>
        /// IQ字节数 16位/32位
        /// </summary>
        public EIfRemoteMode IqMode
        {
            get { return _iqMode; }
            set
            {
                if (_iqMode != value)
                {
                    _iqMode = value;
                    OnPropertyChanged(() => this.IqMode);
                }
            }
        }

        #endregion 设置参数

        /// <summary>
        /// 构造函数
        /// </summary>
        public ArgusSim() : base()
        {
            this.Name = "Argus";
            _control = new ArgusSimView(this);
            _dispatcher = _control.Dispatcher;
            InitKeyValuePairs();
        }

        #region DeviceBase

        public override bool CanDeviceIni
        {
            get
            {
                return !string.IsNullOrEmpty(_ipAddress) && (_tcpPort > 0) && (_udpPort > 0);
            }
        }

        public override void Initialize()
        {
            DeviceInitialized = false;
        }

        public override void Start()
        {
            try
            {
                DeviceInitialized = true;
                IniSocket();
                SendIniCommand();
                IniThread();
            }
            catch (Exception ex)
            {
                MessageBox.Show("启动失败\r\n" + ex.ToString());
                DeviceInitialized = false;
            }
        }

        public override void Stop()
        {
            DeviceInitialized = false;
            DisposeSocket();
            DisposeThread();
        }

        public override void TaskPause(bool pause)
        {
            //NoUse
        }

        protected override UserControl GetControl()
        {
            return _control;
        }

        protected override Stream GetStream2()
        {
            return _socketTcp2 == null ? null : new NetworkStream(_socketTcp2);
        }

        protected override Stream GetStream1()
        {
            return _socketTcp1 == null ? null : new NetworkStream(_socketTcp1);
        }

        #endregion DeviceBase

        #region 公共方法

        /// <summary>
        /// 启动任务
        /// </summary>
        public void StartTask()
        {
            IPEndPoint ep2 = new IPEndPoint(IPAddress.Parse(_ipAddress), _tcpPort + 10);
            _socketTcp2 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socketTcp2.Connect(ep2);

            SendTaskStartCommand();
            IsRunning = true;
        }

        /// <summary>
        /// 停止任务
        /// </summary>
        public void StopTask()
        {
            _iqCount = 0;
            _udpCount = 0;
            SendTaskStopCommand();
            IsRunning = false;
            try
            {
                if (_socketTcp2 != null)
                {
                    _socketTcp2.Close();
                    _socketTcp2.Dispose();
                }
            }
            catch { }
        }

        #endregion 公共方法


        private void InitKeyValuePairs()
        {
            _iniParamList.Add("*IDN?", "ROHDE&SCHWARZ,EB500,100.409/012,V04.50-4072.8710.00");
            _iniParamList.Add("SYST:COMM:CLI?", string.Format("\"{0}\", \"0.0.0.0\"", ""));//////////////
            _iniParamList.Add("TRACE:UDP? 0", "DEF");
            _iniParamList.Add("TRACE:UDP? 1", "001");
            _iniParamList.Add("TRACE:UDP? 2", "002");
            _iniParamList.Add("TRACE:UDP? 3", "003");
            _iniParamList.Add("TRACE:UDP? 4", "004");
            _iniParamList.Add("TRACE:DDC1:UDP? 0", "DEF");
            _iniParamList.Add("TRACE:DDC1:UDP? 1", "001");
            _iniParamList.Add("TRACE:DDC1:UDP? 2", "002");
            _iniParamList.Add("TRACE:DDC1:UDP? 3", "003");
            _iniParamList.Add("TRACE:DDC1:UDP? 4", "004");
            _iniParamList.Add("TRACE:DDC2:UDP? 0", "DEF");
            _iniParamList.Add("TRACE:DDC2:UDP? 1", "001");
            _iniParamList.Add("TRACE:DDC2:UDP? 2", "002");
            _iniParamList.Add("TRACE:DDC2:UDP? 3", "003");
            _iniParamList.Add("TRACE:DDC2:UDP? 4", "004");
            _iniParamList.Add("TRACE:DDC3:UDP? 0", "DEF");
            _iniParamList.Add("TRACE:DDC3:UDP? 1", "001");
            _iniParamList.Add("TRACE:DDC3:UDP? 2", "002");
            _iniParamList.Add("TRACE:DDC3:UDP? 3", "003");
            _iniParamList.Add("TRACE:DDC3:UDP? 4", "004");
            _iniParamList.Add("TRACE:TCP? 0", "DEF");
            _iniParamList.Add("TRACE:TCP? 1", "001");
            _iniParamList.Add("TRACE:TCP? 2", "002");
            _iniParamList.Add("TRACE:TCP? 3", "003");
            _iniParamList.Add("TRACE:TCP? 4", "004");
            _iniParamList.Add("TRACE:DDC1:TCP? 0", "DEF");
            _iniParamList.Add("TRACE:DDC1:TCP? 1", "001");
            _iniParamList.Add("TRACE:DDC1:TCP? 2", "002");
            _iniParamList.Add("TRACE:DDC1:TCP? 3", "003");
            _iniParamList.Add("TRACE:DDC1:TCP? 4", "004");
            _iniParamList.Add("TRACE:DDC2:TCP? 0", "DEF");
            _iniParamList.Add("TRACE:DDC2:TCP? 1", "001");
            _iniParamList.Add("TRACE:DDC2:TCP? 2", "002");
            _iniParamList.Add("TRACE:DDC2:TCP? 3", "003");
            _iniParamList.Add("TRACE:DDC2:TCP? 4", "004");
            _iniParamList.Add("TRACE:DDC3:TCP? 0", "DEF");
            _iniParamList.Add("TRACE:DDC3:TCP? 1", "001");
            _iniParamList.Add("TRACE:DDC3:TCP? 2", "002");
            _iniParamList.Add("TRACE:DDC3:TCP? 3", "003");
            _iniParamList.Add("TRACE:DDC3:TCP? 4", "004");
            _iniParamList.Add("SYST:COMM:LAN:PING OFF", "");
            _iniParamList.Add("SYST:CLOCK:StART Ext", "");
            _iniParamList.Add("SYST:ERR?", "0,\"No error\"");
            _iniParamList.Add("*OPT?", "PS,IM,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,GPS");
            _iniParamList.Add("DIAG:INFO:PER:STAT?", "\"RX_ANT1\",OK,\"RX_ANT2\",OK,\"COMPASS_SW\",OK,\"COMPASS_GPS\",OK,\"u - Blox LEA - 6T\",OK");
            _iniParamList.Add("DIAG:INFO:PER?", "8,0,0,-1,-1.-1,\"RX_ANT1\",8,0,1,-1,-1.-1,\"RX_ANT2\",3,3,0,13,-1.-1,\"COMPASS_SW\",3,4,1,12,-1.-1,\"COMPASS_GPS\",4,6,0,12,07.03,\"u - Blox LEA - 6T\"");
            _iniParamList.Add("SYST:GPS:DATA:AUTO ON", "");
            _iniParamList.Add("SYST:ANT:RX:CLE1", "");
            _iniParamList.Add("SYST:ANT:RX:CLE2", "");
            _iniParamList.Add("SYST:ANT:RX:CLE3", "");
            _iniParamList.Add("SYST:ANT:RX:CLE4", "");
            _iniParamList.Add("SYST:ANT:RX:CLE5", "");
            _iniParamList.Add("SYST:ANT:RX:CLE6", "");
            _iniParamList.Add("SYST:ANT:RX:CLE7", "");
            _iniParamList.Add("SYST:ANT:RX:CLE8", "");
            _iniParamList.Add("SYST:ANT:RX:CLE9", "");
            _iniParamList.Add("SYST:ANT:RX:CLE10", "");
            _iniParamList.Add("SYST:ANT:RX:CLE11", "");
            _iniParamList.Add("SYST:ANT:RX:CLE12", "");
            _iniParamList.Add("SYST:ANT:RX:NAME1 \"RX_ANT1\"", "");
            _iniParamList.Add("SYST:ANT:RX:FREQ:STARt1 20000000", "");
            _iniParamList.Add("SYST:ANT:RX:FREQ:Stop1 400000000", "");
            _iniParamList.Add("SYST:ANT:RX:OUTP:BYTA1 #H00", "");
            _iniParamList.Add("SYST:ANT:FREQ:STAR \"RX_ANT1\", 20000000", "");
            _iniParamList.Add("SYST:ANT:FREQ:Stop \"RX_ANT1\", 400000000", "");
            //_keyValuePairs.Add("SYST:ANT:RX:OUTP:BYTA1 #H00", "");
            _iniParamList.Add("SYST:ANT:RX:NAME2 \"RX_ANT2\"", "");
            _iniParamList.Add("SYST:ANT:RX:FREQ:STARt2 400000000", "");
            _iniParamList.Add("SYST:ANT:RX:FREQ:Stop2 3600000000", "");
            _iniParamList.Add("SYST:ANT:RX:OUTP:BYTA2 #H01", "");
            _iniParamList.Add("SYST:ANT:FREQ:STAR \"RX_ANT2\", 400000000", "");
            _iniParamList.Add("SYST:ANT:FREQ:Stop \"RX_ANT2\", 3600000000", "");
            _iniParamList.Add("OUTP:AUXMode AUTO", "");
            _iniParamList.Add("SYST:GPS:Aver:MOT 600", "");
            _iniParamList.Add("SYST:GPS:AVER:ACC 0.1", "");
            _iniParamList.Add("SYST:GPS:MODE FRUN", "");
            ////////////////////////////////////////////////////////////////
            //_iniParamList.Add("SYST:GPS:DATA?", "GPS,1,#TIME,83,#STARNO,#SNO,#SNR,#SNM,#SNS,#EWO,#EWR,#EWM,#EWS,#YEAR,#MON,#DAY,#HOUR,#MIN,#SEC,0.01,0.00,#DEC,#Height,1,-5.59");//////////////////////
            //_iniParamList.Add("ROSC:SYNC?", "PPSL");
            //_iniParamList.Add("SYST:GPS:AVER:MSD?", "0.00");
            //_iniParamList.Add("SYST:GPS:MODE?", "FRUN");
        }

        // 初始化Socket连接
        private void IniSocket()
        {
            IPEndPoint ep1 = new IPEndPoint(IPAddress.Parse(_ipAddress), _tcpPort);
            _socketTcp1 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socketTcp1.Connect(ep1);

            IPEndPoint ep3 = _socketTcp1.RemoteEndPoint as IPEndPoint;
            _localIpAddress = ep3.Address.ToString();
            _localTcpPort = ep3.Port;

            IPEndPoint ep4 = new IPEndPoint(IPAddress.Parse(_ipAddress), _udpPort);
            _socketUdp = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _socketUdp.Bind(ep4);
        }

        // 初始化线程
        private void IniThread()
        {
            _socket1Thread = new Thread(Recv1Async);
            _socket1Thread.IsBackground = true;
            _socket1Thread.Name = "TcpSocket1 Receive Thread";
            _socket1Thread.Start();

            _socket2Thread = new Thread(Recv2Async);
            _socket2Thread.IsBackground = true;
            _socket2Thread.Name = "TcpSocket2 Receive Thread";
            _socket2Thread.Start();

            _socketUdpThread = new Thread(RecvUdpAsync);
            _socketUdpThread.IsBackground = true;
            _socketUdpThread.Name = "UdpSocket Receive Thread";
            _socketUdpThread.Start();

            _sendKeepAliveThread = new Thread(KeepAlive);
            _sendKeepAliveThread.IsBackground = true;
            _sendKeepAliveThread.Name = "KeepAlive Thread";
            _sendKeepAliveThread.Start();

            _tcpDataProThread = new Thread(ProcessTcpDataAsync);
            _tcpDataProThread.IsBackground = true;
            _tcpDataProThread.Name = "Process IQ Thread";
            _tcpDataProThread.Start();

            _udpDataProThread = new Thread(ProcessUdpDataAsync);
            _udpDataProThread.IsBackground = true;
            _udpDataProThread.Name = "Process UDP Thread";
            _udpDataProThread.Start();
        }

        // 释放Socket
        private void DisposeSocket()
        {
            try
            {
                if (_socketTcp1 != null)
                {
                    _socketTcp1.Close();
                    _socketTcp1.Dispose();
                }
            }
            catch { }
            try
            {
                if (_socketTcp2 != null)
                {
                    _socketTcp2.Close();
                    _socketTcp2.Dispose();
                }
            }
            catch { }
            try
            {
                if (_socketUdp != null)
                {
                    _socketUdp.Close();
                    _socketUdp.Dispose();
                }
            }
            catch { }
        }

        // 释放线程
        private void DisposeThread()
        {
            //try
            //{
            //    if (_socket1Thread != null)
            //    {
            //        _socket1Thread.Abort();
            //    }
            //}
            //catch { }
            //try
            //{
            //    if (_socket2Thread != null)
            //    {
            //        _socket2Thread.Abort();
            //    }
            //}
            //catch { }
            //try
            //{
            //    if (_socketUdpThread != null)
            //    {
            //        _socketUdpThread.Abort();
            //    }
            //}
            //catch { }
        }

        // 通过通道1发送命令
        private void SendCommand1(string cmd)
        {
            try
            {
                string str = cmd + "\n";
                byte[] buffer = Encoding.ASCII.GetBytes(str);
                _socketTcp1.Send(buffer);
                SendStrShow(str);
            }
            catch (Exception)
            {
            }
        }

        // 发送初始化命令
        private void SendIniCommand()
        {
            if (_socketTcp1 == null || !_socketTcp1.Connected)
            {
                return;
            }
            foreach (var pairs in _iniParamList)
            {
                SendCommand1(pairs.Key);
            }
        }

        private void SendTaskStartCommand()
        {
            SendCommand1(DataDefine.ABORT);
            SendCommand1("*SRE 0;*ESE 0;*CLS");
            SendCommand1(":VIDEO:PIC SINGLE");
            SendCommand1(":FORM:DATA ASCII;:FORMAT:BORD NORM");
            SendCommand1(":FUNC:OFF \"VOLT:AC\"");
            SendCommand1(":TRAC:FEED:CONT MTRACE, NEV;CONT ITRACE, NEV;CONT IFPAN, NEV");
            SendCommand1(":STAT:TRAC:ENAB 0;*CLS");
            SendCommand1("ROUT:HF (@0)");
            SendCommand1("ROUT:VUHF (@0)");
            SendCommand1(":FREQ:MODE CW");
            string cmd = string.Format("{0}{1};{2}{3}", DataDefine.SET_DEMODULATION_MODE, _demMode, DataDefine.SET_DEMODULATION_BANDWIDTH, (int)(_bandWidth * 1000));
            SendCommand1(cmd);
            cmd = string.Format("{0}{1}", DataDefine.SET_LEVEL_MEASUREMODE, _detector);
            SendCommand1(cmd);
            cmd = string.Format("{0}{1}", DataDefine.SET_ATTENUATION_AUTO, _attMode ? "ON" : "OFF");
            SendCommand1(cmd);
            if (!_attMode)
            {
                cmd = string.Format("{0}{1}", DataDefine.SET_ATTENUATION_VALUE, _attenuation);
                SendCommand1(cmd);
            }
            cmd = string.Format("{0}{1}", DataDefine.SET_RFMODE, _rfMode);
            SendCommand1(cmd);
            SendCommand1(":OUTP:FILT:MODE OFF");
            SendCommand1("SENSE:GCON:AUTO:TIME DEF");
            cmd = string.Format("{0}{1} s", DataDefine.SET_HOLDTIME, _holdTime);
            SendCommand1(cmd);
            SendCommand1("SENSE:DEM:BFO 1000 Hz");
            SendCommand1(":FREQ:AFC OFF");
            cmd = string.Format("CALC:IFPAN:STEP:AUTO ON;{0}{1}Hz;:meas:band:lim:auto ON;{2}{3}", DataDefine.SET_FREQ_SPAN, (long)(_spectrumSpan * 1000), DataDefine.SET_FFTMODE, _fftMode);
            SendCommand1(cmd);
            SendCommand1("meas:band:lim:auto on");
            SendCommand1("CALC:IFPAN:STEP:AUTO ON");
            SendCommand1("CALC:PIFP:MODE OFF");
            SendCommand1("CALC:PIFP:ACTT 0.0150");
            SendCommand1("CALC:PIFP:OBST 0.5000");
            cmd = string.Format("{0}{1} MHz", DataDefine.SET_FREQUENCY, _frequency.ToString("0.000000"));
            SendCommand1(cmd);
            SendCommand1(":OUTP:SQU:THR -10;:OUTP:SQU OFF");
            SendCommand1(":GCON:MODE AUTO");
            SendCommand1(":SYST:SPEAKER:STAT OFF;:SYSTEM:AUDIO:VOL MIN");
            cmd = string.Format("FREQ:DEM {0}Hz", (long)(_frequency * 1000000));
            SendCommand1(cmd);
            SendCommand1(":Meas:Time DEF");
            cmd = string.Format("{0}{1}", DataDefine.SET_MEAS_BAND_MODE, _measMode);
            SendCommand1(cmd);
            cmd = string.Format("{0}{1:0.0}", DataDefine.SET_MEAS_BETA_VALUE, _betaValue);
            SendCommand1(cmd);
            cmd = string.Format("{0}{1:0.0} dB", DataDefine.SET_MEAS_XDB_VALUE, _xdBValue);
            SendCommand1(cmd);
            SendCommand1(":SYSTEM:VIDEO:REMOTE:MODE OFF;:DISP:MENU IFPAN");
            cmd = string.Format("{0}{1}", DataDefine.SET_IF_REMOTE_MODE, _iqMode);
            SendCommand1(cmd);
            SendCommand1(":SENS:VID:STAN B");
            SendCommand1("CALC:IFPAN:SEL AUTO");
            SendCommand1(":SENSE:DEC:SELC:STATE OFF");
            SendCommand1("OUTP:VID:MODE IF;Freq 10700000 Hz");
            SendCommand1(":ROSC:SOUR INT");
            SendCommand1(":FUNC: OFF \"VOLT:AC\"");
            SendCommand1(":FUNC: OFF \"FREQ:OFFS\"");
            SendCommand1(":FUNC: OFF \"AM\"");
            SendCommand1(":FUNC: OFF \"AM:POS\"");
            SendCommand1(":FUNC: OFF \"AM:NEG\"");
            SendCommand1(":FUNC: OFF \"FM\"");
            SendCommand1(":FUNC: OFF \"FM:POS\"");
            SendCommand1(":FUNC: OFF \"FM:NEG\"");
            SendCommand1(":FUNC: OFF \"PM\"");
            SendCommand1(":FUNC: OFF \"BAND\"");
            SendCommand1(":FUNC: OFF \"DFL\"");
            SendCommand1(":FUNC: OFF \"AZIM\"");
            SendCommand1(":FUNC: OFF \"DFQ\"");
            SendCommand1(":FUNC: OFF \"VOLT:AC\"");
            SendCommand1(":FUNC: OFF \"FREQ:OFFS\"");
            SendCommand1(":FUNC: OFF \"AM\"");
            SendCommand1(":FUNC: OFF \"AM:POS\"");
            SendCommand1(":FUNC: OFF \"AM:NEG\"");
            SendCommand1(":FUNC: OFF \"FM\"");
            SendCommand1(":FUNC: OFF \"FM:POS\"");
            SendCommand1(":FUNC: OFF \"FM:NEG\"");
            SendCommand1(":FUNC: OFF \"PM\"");
            SendCommand1(":FUNC: OFF \"BAND\"");
            SendCommand1(":FUNC: OFF \"DFL\"");
            SendCommand1(":FUNC: OFF \"AZIM\"");
            SendCommand1(":FUNC: OFF \"DFQ\"");
            SendCommand1("Meas:Mode PER");
            cmd = string.Format("TRAC:TCP:TAG \"{0}\",{1},IF", _localIpAddress, _localTcpPort);
            SendCommand1(cmd);
            cmd = string.Format("TRAC:TCP:FLAG \"{0}\",{1},\"VOLT:AC\",\"OPT\",\"SWAP\"", _localIpAddress, _localTcpPort);
            SendCommand1(cmd);
            cmd = string.Format("TRAC:UDP:TAG \"{0}\",{1},GPSC", _localIpAddress, _udpPort);
            SendCommand1(cmd);
            cmd = string.Format("TRAC:UDP:FLAG \"{0}\",{1},\"OPT\"", _localIpAddress, _udpPort);
            SendCommand1(cmd);
            cmd = string.Format("TRAC:UDP:TAG \"{0}\",{1},CW", _localIpAddress, _udpPort);
            SendCommand1(cmd);
            cmd = string.Format("TRAC:UDP:FLAG \"{0}\",{1},\"VOLT:AC\",\"OPT\",\"FREQ:RX\",\"FREQ:HIGH:RX\"", _localIpAddress, _udpPort);
            SendCommand1(cmd);
            SendCommand1(DataDefine.INIT);
        }

        private void SendTaskStopCommand()
        {
            SendCommand1(DataDefine.ABORT);
            SendCommand1("STAT:TRAC:ENAB 0;*SRE 0;*ESE 0;*CLS");
            SendCommand1("TRAC:FEED:CONT IFPAN, NEV");
            /*
                TRAC:TCP:TAG:OFF "192.168.29.1",56725,IF
                TRAC:TCP:FLAG:OFF "192.168.29.1",56725,"VOLT:AC","OPT"
                TRAC:TCP:DEL "192.168.29.1",56725
                TRAC:UDP:TAG:OFF "192.168.29.1",4060,CW
                TRAC:UDP:FLAG:OFF "192.168.29.1",4060,"VOLT:AC","OPT","FREQ:RX","FREQ:HIGH:RX"
                TRAC:UDP:DEL "192.168.29.1",4060

             */
            string cmd = string.Format("TRAC:TCP:TAG:OFF \"{0}\",{1},IF", _localIpAddress, _localTcpPort);
            SendCommand1(cmd);
            cmd = string.Format("TRAC:TCP:FLAG:OFF \"{0}\",{1},\"VOLT: AC\",\"OPT\"", _localIpAddress, _localTcpPort);
            SendCommand1(cmd);
            cmd = string.Format("TRAC:TCP:DEL \"{0}\",{1}", _localIpAddress, _localTcpPort);
            SendCommand1(cmd);
            cmd = string.Format("TRAC:UDP:TAG:OFF \"{0}\",{1},GPSC", _localIpAddress, _udpPort);
            SendCommand1(cmd);
            cmd = string.Format("TRAC:UDP:FLAG:OFF \"{0}\",{1},\"OPT\"", _localIpAddress, _udpPort);
            SendCommand1(cmd);
            cmd = string.Format("TRAC:UDP:TAG:OFF \"{0}\",{1},CW", _localIpAddress, _udpPort);
            SendCommand1(cmd);
            cmd = string.Format("TRAC:UDP:FLAG:OFF \"{0}\",{1},\"VOLT: AC\",\"OPT\",\"FREQ: RX\",\"FREQ: HIGH:RX\"", _localIpAddress, _udpPort);
            SendCommand1(cmd);
            cmd = string.Format("TRAC:UDP:DEL \"{0}\",{1}", _localIpAddress, _udpPort);
            SendCommand1(cmd);
        }

        // 每秒发送查询GPS的消息
        private void KeepAlive()
        {
            DateTime lastTime = DateTime.Now;
            while (_deviceInitialized)
            {
                Thread.Sleep(1000);
                if (_socketTcp1 == null || !_socketTcp1.Connected)
                {
                    continue;
                }

                if (_isRunning)
                {
                    if (DateTime.Now.Subtract(lastTime).TotalSeconds > 5)
                    {
                        string cmd = string.Format("CALC:IFPAN:STEP:AUTO ON;{0}{1}Hz:meas:band:lim:auto ON;{2}{3}", DataDefine.SET_FREQ_SPAN, (long)(_spectrumSpan * 1000), DataDefine.SET_FFTMODE, _fftMode);
                        SendCommand1(cmd);
                        SendCommand1("meas:band:lim:auto on");
                        SendCommand1("CALC:IFPAN:STEP:AUTO ON");
                        cmd = string.Format("FREQ:DEM {0}Hz", (long)(_frequency * 1000000));
                        SendCommand1(cmd);
                        SendCommand1(":Meas:Time DEF");
                        SendCommand1("Meas:Mode PER");
                        SendCommand1(":VIDEO:PIC SINGLE");
                        lastTime = DateTime.Now;
                    }
                    continue;
                }
                else
                {
                    string cmd = "SYST:GPS:DATA?";
                    SendCommand1(cmd);
                    cmd = "ROSC:SYNC?";
                    SendCommand1(cmd);
                    cmd = "SYST:GPS:AVER:MSD?";
                    SendCommand1(cmd);
                    cmd = "SYST:GPS:MODE?";
                    SendCommand1(cmd);
                }
            }
        }

        // 接收Socket1的数据
        private void Recv1Async()
        {
            byte[] buffer = new byte[65536];
            while (_deviceInitialized)
            {
                if (_socketTcp1 == null || !_socketTcp1.Connected)
                {
                    Thread.Sleep(1);
                    continue;
                }
                try
                {
                    int len = _socketTcp1.Receive(buffer, SocketFlags.None);
                    if (len <= 0)
                    {
                        Stop();
                        break;
                    }

                    string strRecv = Encoding.ASCII.GetString(buffer, 0, len);
                    RecvStrShow(strRecv);
                    //GPS,1,1557477031,98,10,N,39,8,1.15,E,117,11,12.47,2019,5,10,8,30,31,0.00,0.00,353.00,28.30,1,-5.59
                    if (strRecv.StartsWith("GPS,"))
                    {
                        string[] arr = strRecv.Split(',');
                        string ns = arr[5];
                        string ew = arr[9];
                        string time = string.Format("{0}-{1}-{2} {3}:{4}:{5}", arr[13], arr[14], arr[15], arr[16], arr[17], arr[18]);
                        string str = string.Format("纬度{0}{1}°{2}′{3}″，经度{4}{5}°{6}′{7}″，卫星个数{8}，磁偏角{9}，海拔{10}，时间：{11}",
                            ns, arr[6], arr[7], arr[8], ew, arr[10], arr[11], arr[12], arr[4], arr[21], arr[22], time);
                        GpsInfo = str;
                    }
                }
                catch (SocketException se)
                {
                    Stop();
                    break;
                }
                catch (Exception)
                {
                }

            }
        }

        // 接收Socket2的数据
        private void Recv2Async()
        {
            byte[] buffer = new byte[65536];
            int headLen = Marshal.SizeOf(typeof(EB200Header));
            while (_deviceInitialized)
            {
                if (_socketTcp2 == null || !_socketTcp2.Connected)
                {
                    Thread.Sleep(1);
                    continue;
                }
                try
                {
                    ReceiveData(buffer, 0, 4, _socketTcp2);
                    // 判断包头是否是000EB200
                    if (buffer[0] == 0x00 && buffer[1] == 0x0E && buffer[2] == 0xB2 && buffer[3] == 0x00)
                    {
                        // 读取头结构
                        ReceiveData(buffer, 4, headLen - 4, _socketTcp2);
                        // 解析数据长度
                        byte[] tmp = new byte[4];
                        Buffer.BlockCopy(buffer, headLen - 4, tmp, 0, 4);
                        Array.Reverse(tmp);
                        int totalLen = BitConverter.ToInt32(tmp, 0);
                        // 读取剩下的数据
                        ReceiveData(buffer, headLen, totalLen - headLen, _socketTcp2);

                        if (_isRunning)
                        {
                            byte[] data = new byte[totalLen];
                            Buffer.BlockCopy(buffer, 0, data, 0, totalLen);
                            _tcpDataCache.Enqueue(data);
                        }
                    }
                }
                catch (SocketException sc)
                {
                    //Stop();
                    //break;
                }
                catch (Exception ex)
                {
                }

            }
        }

        // 接收UDP数据
        private void RecvUdpAsync()
        {
            byte[] buffer = new byte[65536];
            while (_deviceInitialized)
            {
                if (_socketUdp == null)
                {
                    Thread.Sleep(1);
                    continue;
                }
                try
                {
                    int len = _socketUdp.Receive(buffer, SocketFlags.None);
                    if (len <= 0)
                    {
                        Stop();
                        break;
                    }
                    if (_isRunning)
                    {
                        byte[] data = new byte[len];
                        Buffer.BlockCopy(buffer, 0, data, 0, len);
                        _udpDataCache.Enqueue(data);
                    }
                }
                catch (SocketException se)
                {
                    Stop();
                    break;
                }
                catch (Exception)
                {
                }

            }
        }

        private void ProcessTcpDataAsync()
        {
            while (_deviceInitialized)
            {
                if (_tcpDataCache.IsEmpty)
                {
                    Thread.Sleep(1);
                    continue;
                }

                byte[] buffer;
                if (_tcpDataCache.TryDequeue(out buffer))
                {
                    //LogDataToFile(AppDomain.CurrentDomain.BaseDirectory + "IQ.txt", buffer, 0, buffer.Length);
                    EB200Data eB200 = new EB200Data(buffer, 0);
                    if (eB200.Tag == TAGS.IF)
                    {
                        _iqCount++;
                        OptionalHeaderIF header = (OptionalHeaderIF)eB200.OptionalHeader;
                        string str = string.Format("Count:{0},SampleCount:0x{1:X8}", _iqCount, header.SampleCount);
                        IQInfo = str;
                    }
                }
            }
        }

        private void ProcessUdpDataAsync()
        {
            while (_deviceInitialized)
            {
                if (_udpDataCache.IsEmpty)
                {
                    Thread.Sleep(1);
                    continue;
                }
                byte[] buffer;
                if (_udpDataCache.TryDequeue(out buffer))
                {
                    //LogDataToFile(AppDomain.CurrentDomain.BaseDirectory + "UDP.txt", buffer, 0, buffer.Length);
                    EB200Data eB200 = new EB200Data(buffer, 0);
                    _udpCount++;
                    switch (eB200.Tag)
                    {
                        case TAGS.CW:
                            {
                                CWData data = new CWData(eB200.TraceAttribute.NumberOfTraceItems, eB200.Data, 0, eB200.TraceAttribute.SelectorFlags);
                                float average = 0f;
                                if (data.Level.Length > 0)
                                {
                                    average = data.Level.Average(l => (float)l / 10);
                                }
                                string str = string.Format("Count:{0},CWCount:{1},Level:{2:0.00}dBμV", _iqCount, eB200.TraceAttribute.NumberOfTraceItems, average);
                                Level = average;
                            }
                            break;
                        case TAGS.GPSCompass:
                            {
                                GPSCompassData data = new GPSCompassData(eB200.Data, 0);
                                string ns = Encoding.ASCII.GetString(BitConverter.GetBytes(data.LatRef)).Trim('\0');
                                string ew = Encoding.ASCII.GetString(BitConverter.GetBytes(data.LonRef)).Trim('\0');
                                string str = string.Format("纬度{0}{1}°{2}′，经度{3}{4}°{5}′，卫星个数{6}，磁偏角{7}，海拔{8}",
                                    ns, data.LatDeg, data.LatMin, ew, data.LonDeg, data.LonMin, data.NoOfSatInView,
                                    data.MagneticDeclination == -1 ? 0 : (float)data.MagneticDeclination / 10, (float)data.Altitude / 100);
                                GpsInfo = str;
                            }
                            break;
                        default:
                            continue;
                    }
                }
            }
        }

        private void LogDataToFile(string path, byte[] data, int offset, int length)
        {
            string str = BitConverter.ToString(data, offset, length);
            using (FileStream fs = new FileStream(path, FileMode.Append, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.WriteLine(str);
                    sw.Flush();
                }
            }
        }
    }
}
