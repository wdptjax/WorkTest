﻿
/*********************************************************************************************
 *	
 * 文件名称:    DDF550Device.cs
 *
 * 作    者:    吴德鹏
 *	
 * 创作日期:    2018-11-28 14:37:02
 * 
 * 备    注:    描述类的用途
 *              
 *                                
 *               
*********************************************************************************************/

using DeviceSimlib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace DeviceSim.Device
{
    public partial class DDF550Device : DeviceBase
    {
        #region 变量/属性

        #region 其他参数

        [XmlIgnore]
        private DDF550View _control = null;
        [XmlIgnore]
        private TcpListener _tcpListenerXml = null;
        [XmlIgnore]
        private TcpListener _tcpListenerData = null;
        [XmlIgnore]
        private Socket _socketXml = null;
        [XmlIgnore]
        private Socket _socketData = null;
        [XmlIgnore]
        private int _portXml
        {
            get { return _port + 8; }
        }
        [XmlIgnore]
        private int _portData
        {
            get { return _port + 10; }
        }

        [XmlIgnore]
        private ClientInfo _client = new ClientInfo();
        private int _port = 5555;

        [XmlIgnore]
        public ClientInfo Client
        {
            get { return _client; }
            set
            {
                _client = value;
                OnPropertyChanged(() => this.Client);
            }
        }

        public int Port
        {
            get { return _port; }
            set
            {
                _port = value;
                OnPropertyChanged(() => this.Port);
            }
        }



        public override bool CanDeviceIni
        {
            get
            {
                return _port > 0;
            }
        }

        #endregion 其他参数

        #region 设备参数

        private EDfMode _dfMode = EDfMode.DFMODE_RX;

        /// <summary>
        /// 设备运行模式
        /// </summary>
        public EDfMode DfMode
        {
            get { return _dfMode; }
            set
            {
                _dfMode = value;
                OnPropertyChanged(() => this.DfMode);
            }
        }

        #region MeasureSettingsFFM

        private double _frequency = 101.7;
        private EAverage_Mode _dFindMode = EAverage_Mode.DFSQU_NORM;
        private double _dfPanStep = 100;
        private int _integralTime = 100;
        private int _levelThreshold = -20;
        private EAnt_Pol _antPol = EAnt_Pol.POL_VERTICAL;
        private double _spectrumSpan = 100;
        private int _attenuation = -1;
        private double _ifPanStep = 100;

        /// <summary>
        /// 中心频率 MHz
        /// </summary>
        public double Frequency
        {
            get { return _frequency; }
            set
            {
                _frequency = value;
                OnPropertyChanged(() => this.Frequency);
            }
        }

        /// <summary>
        /// 测向模式
        /// </summary>
        public EAverage_Mode DFindMode
        {
            get { return _dFindMode; }
            set
            {
                _dFindMode = value;
                OnPropertyChanged(() => this.DFindMode);
            }
        }

        /// <summary>
        /// 测向带宽/信道带宽 KHz
        /// </summary>
        public double DfPanStep
        {
            get { return _dfPanStep; }
            set
            {
                _dfPanStep = value;
                OnPropertyChanged(() => this.DfPanStep);
            }
        }

        /// <summary>
        /// 中频带宽 KHz
        /// </summary>
        public double IfPanStep
        {
            get { return _ifPanStep; }
            set
            {
                _ifPanStep = value;
                OnPropertyChanged(() => this.IfPanStep);
            }
        }

        /// <summary>
        /// 测量时间 ms
        /// </summary>
        public int IntegrationTime
        {
            get { return _integralTime; }
            set
            {
                _integralTime = value;
                OnPropertyChanged(() => this.IntegrationTime);
            }
        }

        /// <summary>
        /// 电平门限
        /// </summary>
        public int LevelThreshold
        {
            get { return _levelThreshold; }
            set
            {
                _levelThreshold = value;
                OnPropertyChanged(() => this.LevelThreshold);
            }
        }

        /// <summary>
        /// 天线极化方式
        /// </summary>
        public EAnt_Pol AntPol
        {
            get { return _antPol; }
            set
            {
                _antPol = value;
                OnPropertyChanged(() => this.AntPol);
            }
        }

        /// <summary>
        /// 频谱带宽
        /// </summary>
        public double SpectrumSpan
        {
            get { return _spectrumSpan; }
            set
            {
                _spectrumSpan = value;
                OnPropertyChanged(() => this.SpectrumSpan);
            }
        }

        /// <summary>
        /// 衰减
        /// </summary>
        public int Attenuation
        {
            get { return _attenuation; }
            set
            {
                _attenuation = value;
                OnPropertyChanged(() => this.Attenuation);
            }
        }

        #endregion MeasureSettingsFFM

        #region MeasureSettingsPScan

        private double _startFrequency = 88;
        private double _stopFrequency = 108;
        private double _step = 200;

        /// <summary>
        /// 起始频率
        /// </summary>
        public double StartFrequency
        {
            get { return _startFrequency; }
            set
            {
                _startFrequency = value;
                OnPropertyChanged(() => this.StartFrequency);
            }
        }

        /// <summary>
        /// 结束频率
        /// </summary>
        public double StopFrequency
        {
            get { return _stopFrequency; }
            set
            {
                _stopFrequency = value;
                OnPropertyChanged(() => this.StopFrequency);
            }
        }

        /// <summary>
        /// 步进
        /// </summary>
        public double Step
        {
            get { return _step; }
            set
            {
                _step = value;
                OnPropertyChanged(() => this.Step);
            }
        }

        #endregion MeasureSettingsPScan

        #region ITU

        private EMeasureMode _ituMeasureMode = EMeasureMode.MEASUREMODE_XDB;
        private double _xdbBandWidth = 26;
        private double _betaBandWidth = 1;

        public EMeasureMode ItuMeasureMode
        {
            get { return _ituMeasureMode; }
            set
            {
                _ituMeasureMode = value;
                OnPropertyChanged(() => this.ItuMeasureMode);
            }
        }

        public double XdbBandWidth
        {
            get { return _xdbBandWidth; }
            set
            {
                _xdbBandWidth = value;
                OnPropertyChanged(() => this.XdbBandWidth);
            }
        }

        public double BetaBandWidth
        {
            get { return _betaBandWidth; }
            set
            {
                _betaBandWidth = value;
                OnPropertyChanged(() => this.BetaBandWidth);
            }
        }

        #endregion ITU

        #region DemodulationSettings

        private EDemodulation _demMode = EDemodulation.MOD_AM;
        private double _demFrequency = 101.7;
        private double _demBandWidth = 120;
        private int _squelchThreshold = 10;
        private bool _isUseSquelch = false;
        private ELevel_Indicatir _detector = ELevel_Indicatir.LEVEL_INDICATOR_FAST;
        private int _gain = -100;
        /// <summary>
        /// 解调模式
        /// </summary>
        public EDemodulation DemMode
        {
            get { return _demMode; }
            set
            {
                _demMode = value;
                OnPropertyChanged(() => this.DemMode);
            }
        }
        /// <summary>
        /// 解调频率
        /// </summary>
        public double DemFrequency
        {
            get { return _demFrequency; }
            set
            {
                _demFrequency = value;
                OnPropertyChanged(() => this.DemFrequency);
            }
        }
        /// <summary>
        /// 解调带宽
        /// </summary>
        public double DemBandWidth
        {
            get { return _demBandWidth; }
            set
            {
                _demBandWidth = value;
                OnPropertyChanged(() => this.DemBandWidth);
            }
        }
        /// <summary>
        /// 静噪门限
        /// </summary>
        public int SquelchThreshold
        {
            get { return _squelchThreshold; }
            set
            {
                _squelchThreshold = value;
                OnPropertyChanged(() => this.SquelchThreshold);
            }
        }
        /// <summary>
        /// 静噪开关
        /// </summary>
        public bool IsUseSquelch
        {
            get { return _isUseSquelch; }
            set
            {
                _isUseSquelch = value;
                OnPropertyChanged(() => this.IsUseSquelch);
            }
        }
        /// <summary>
        /// 检波方式
        /// </summary>
        public ELevel_Indicatir Detector
        {
            get { return _detector; }
            set
            {
                _detector = value;
                OnPropertyChanged(() => this.Detector);
            }
        }
        /// <summary>
        /// 增益
        /// </summary>
        public int Gain
        {
            get { return _gain; }
            set
            {
                _gain = value;
                OnPropertyChanged(() => this.Gain);
            }
        }
        #endregion DemodulationSettings

        #region Scan

        private ObservableCollection<ScanRangeInfo> _scanRangeList = new ObservableCollection<ScanRangeInfo>();
        private ScanRangeInfo _runningScanRange = null;
        /// <summary>
        /// 频段列表
        /// </summary>
        public ObservableCollection<ScanRangeInfo> ScanRangeList
        {
            get { return _scanRangeList; }
            set
            {
                _scanRangeList = value;
                OnPropertyChanged(() => this.ScanRangeList);
            }
        }
        /// <summary>
        /// 当前任务运行的频段
        /// </summary>
        public ScanRangeInfo RunningScanRange
        {
            get { return _runningScanRange; }
            set
            {
                _runningScanRange = value;
                OnPropertyChanged(() => this.RunningScanRange);
            }
        }

        #endregion Scan

        private double _measureTime = 100;

        /// <summary>
        /// 测量时间
        /// </summary>
        public double MeasureTime
        {
            get { return _measureTime; }
            set
            {
                _measureTime = value;
                OnPropertyChanged(() => this.MeasureTime);
            }
        }

        #endregion 设备参数

        #endregion 变量/属性

        public DDF550Device() : base()
        {
            this.Name = "DDF550";
            _control = new DDF550View(this);
            _dispatcher = _control.Dispatcher;
        }

        public override void Initialize()
        {
            DeviceInitialized = false;
        }

        public override void Start()
        {
            DeviceInitialized = true;
            IPEndPoint iPEndPointXml = new IPEndPoint(IPAddress.Any, _portXml);
            IPEndPoint iPEndPointData = new IPEndPoint(IPAddress.Any, _portData);
            _tcpListenerXml = new TcpListener(iPEndPointXml);
            _tcpListenerXml.Start();
            _tcpListenerXml.BeginAcceptSocket(new AsyncCallback(XmlClientCallback), _tcpListenerXml);

            _tcpListenerData = new TcpListener(iPEndPointData);
            _tcpListenerData.Start();
            _tcpListenerData.BeginAcceptSocket(new AsyncCallback(DataClientCallback), _tcpListenerXml);
        }

        public override void Stop()
        {
            if (DeviceInitialized)
            {
                DeviceInitialized = false;
                if (Connected)
                {
                    if (_socketXml != null)
                        _socketXml.Close();
                    if (_socketData != null)
                        _socketData.Close();
                    Connected = false;
                    IsRunning = false;
                }
                _tcpListenerXml.Stop();
                _tcpListenerData.Stop();
            }
        }

        public override void TaskPause(bool pause)
        {
        }

        protected override UserControl GetControl()
        {
            return _control;
        }

        protected override Stream GetStreamRecvData()
        {
            if (_socketXml == null)
                return null;
            return new NetworkStream(_socketXml);
        }

        protected override Stream GetStreamSendData()
        {
            if (_socketData == null)
                return null;
            return new NetworkStream(_socketData);
        }

        #region 私有方法

        private void XmlClientCallback(IAsyncResult asyncResult)
        {
            if (!DeviceInitialized)
                return;
            try
            {
                _socketXml = _tcpListenerXml.EndAcceptSocket(asyncResult);

                SetHeartBeat(_socketXml);
                Connected = true;
                IPEndPoint iPEndPoint = _socketXml.RemoteEndPoint as IPEndPoint;
                Client.AddressXml = iPEndPoint.Address.ToString();
                Client.PortXml = iPEndPoint.Port;
                Thread thd = new Thread(DataReceiveXml);
                thd.IsBackground = true;
                thd.Start();
            }
            catch (Exception)
            {
                _tcpListenerXml.BeginAcceptSocket(new AsyncCallback(XmlClientCallback), _tcpListenerXml);
            }

        }
        private void DataClientCallback(IAsyncResult asyncResult)
        {
            if (!DeviceInitialized)
                return;
            try
            {
                _socketData = _tcpListenerData.EndAcceptSocket(asyncResult);
                //_socketXml.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.KeepAlive,);
                IPEndPoint iPEndPoint = _socketData.RemoteEndPoint as IPEndPoint;
                Client.AddressData = iPEndPoint.Address.ToString();
                Client.PortData = iPEndPoint.Port;
                Thread thd = new Thread(DataSendSync);
                thd.IsBackground = true;
                thd.Start();
            }
            catch (Exception)
            {
                _tcpListenerData.BeginAcceptSocket(new AsyncCallback(DataClientCallback), _tcpListenerData);
            }
        }

        #endregion 私有方法

        #region 心跳检测
        private void SetHeartBeat(object connObject)
        {
            Thread thHeartBeat = new Thread(KeepAlive);
            thHeartBeat.IsBackground = true;
            thHeartBeat.Name = "DDF550 HeartBeatThread";
            thHeartBeat.Start(connObject);
        }

        protected virtual void KeepAlive(object connObject)
        {
            Socket socket = connObject as Socket;
            if (socket == null)
            {
                return;
            }

            try
            {
                byte[] bytes = new byte[] { 0x01, 0x00, 0x00, 0x00, 0x64, 0x00, 0x00, 0x00, 0xF4, 0x01, 0x00, 0x00 };

                //////byte[] bytes = new byte[4 * 3];
                //////BitConverter.GetBytes((uint)1).CopyTo(bytes, 0);
                //////BitConverter.GetBytes((uint)5000).CopyTo(bytes, 4);//一回送信間隔 
                //////BitConverter.GetBytes((uint)1000).CopyTo(bytes, 8);//二回送信間隔 

                socket.IOControl(IOControlCode.KeepAliveValues, bytes, null);
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                byte[] msg = Encoding.Default.GetBytes("");
                while (true)
                {
                    socket.Send(msg);
                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                //此处先记录详细日志

                if (ex is SocketException)
                {
                    CloseConnect();
                }
            }
        }

        private void CloseConnect()
        {
            if (!DeviceInitialized)
                return;
            _socketXml?.Close();
            _socketData?.Close();
            Connected = false;
            _tcpListenerXml.BeginAcceptSocket(new AsyncCallback(XmlClientCallback), _tcpListenerXml);
            _tcpListenerData.BeginAcceptSocket(new AsyncCallback(DataClientCallback), _tcpListenerData);

        }

        #endregion 心跳检测
    }
}