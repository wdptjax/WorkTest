
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

        #region 运行参数

        [XmlIgnore]
        private DDF550View _control = null;
        [XmlIgnore]
        private TcpListener _tcpListenerXml = null;
        [XmlIgnore]
        private TcpListener _tcpListenerData = null;
        [XmlIgnore]
        private Socket _socketXml = null;
        //[XmlIgnore]
        //private Socket _socketData = null;
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
        private ObservableCollection<ClientInfo> _clientList = new ObservableCollection<ClientInfo>();

        [XmlIgnore]
        public ObservableCollection<ClientInfo> ClientList
        {
            get { return _clientList; }
            set
            {
                _clientList = value;
                OnPropertyChanged(() => this.ClientList);
            }
        }
        [XmlIgnore]
        public override bool CanDeviceIni
        {
            get
            {
                return _port > 0;
            }
        }

        #endregion 运行参数

        #region 安装参数

        private int _port = 5555;

        public int Port
        {
            get { return _port; }
            set
            {
                _port = value;
                OnPropertyChanged(() => this.Port);
            }
        }

        #endregion 安装参数

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
        private double _spectrumSpan = 2000;
        private int _attenuation = -1;
        private bool _attAuto = false;
        private double _ifPanStep = 100;
        private EIFPan_Mode _fftMode = EIFPan_Mode.IFPAN_MODE_CLRWRITE;
        private bool _antPreAmp = false;

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

        public EIFPan_Mode FFTMode
        {
            get { return _fftMode; }
            set
            {
                _fftMode = value;
                OnPropertyChanged(() => this.FFTMode);
            }
        }

        public bool AntPreAmp
        {
            get { return _antPreAmp; }
            set
            {
                _antPreAmp = value;
                OnPropertyChanged(() => this.AntPreAmp);
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
        private bool _useAutoBandwidthLimits = false;
        private double _lowerBandwidthLimit = 0;
        private double _upperBandwidthLimit = 0;

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

        public bool UseAutoBandwidthLimits
        {
            get { return _useAutoBandwidthLimits; }
            set
            {
                _useAutoBandwidthLimits = value;
                OnPropertyChanged(() => this.UseAutoBandwidthLimits);
            }
        }

        public double LowerBandwidthLimit
        {
            get { return _lowerBandwidthLimit; }
            set
            {
                _lowerBandwidthLimit = value;
                OnPropertyChanged(() => this.LowerBandwidthLimit);
            }
        }

        public double UpperBandwidthLimit
        {
            get { return _upperBandwidthLimit; }
            set
            {
                _upperBandwidthLimit = value;
                OnPropertyChanged(() => this.UpperBandwidthLimit);
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
        private bool _gainAuto = false;
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
        /// 测量时间(s)
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

        private ERf_Mode _rfMode = ERf_Mode.RFMODE_NORMAL;

        /// <summary>
        /// 射频模式
        /// </summary>
        public ERf_Mode RfMode
        {
            get { return _rfMode; }
            set
            {
                _rfMode = value;
                OnPropertyChanged(() => this.RfMode);
            }
        }

        private EAudioMode _audioMode = EAudioMode.AUDIO_MODE_OFF;

        /// <summary>
        /// 音频格式
        /// </summary>
        public EAudioMode AudioMode
        {
            get { return _audioMode; }
            set
            {
                _audioMode = value;
                OnPropertyChanged(() => this.AudioMode);
            }
        }

        private bool _isCorrection = false;

        /// <summary>
        /// 相对示向度还是绝对示向度
        /// true:校正数据,绝对示向度
        /// false:未校正,相对示向度
        /// </summary>
        public bool IsCorrection
        {
            get { return _isCorrection; }
            set
            {
                _isCorrection = value;
                OnPropertyChanged(() => this.IsCorrection);
            }
        }

        #endregion 设备参数

        #region 数据开关

        private bool _cwSwitch = false;
        private bool _iqSwitch = false;
        private bool _spectrumSwitch = false;
        private bool _audioSwitch = false;
        private bool _dfSwitch = false;
        private bool _scanSwitch = false;
        private bool _ituSwitch = false;

        public bool CWSwitch
        {
            get { return _cwSwitch; }
            set { _cwSwitch = value; OnPropertyChanged(() => this.CWSwitch); }
        }
        public bool IQSwitch
        {
            get { return _iqSwitch; }
            set { _iqSwitch = value; OnPropertyChanged(() => this.IQSwitch); }
        }
        public bool SpectrumSwitch
        {
            get { return _spectrumSwitch; }
            set { _spectrumSwitch = value; OnPropertyChanged(() => this.SpectrumSwitch); }
        }
        public bool AudioSwitch
        {
            get { return _audioSwitch; }
            set { _audioSwitch = value; OnPropertyChanged(() => this.AudioSwitch); }
        }
        public bool DFSwitch
        {
            get { return _dfSwitch; }
            set { _dfSwitch = value; OnPropertyChanged(() => this.DFSwitch); }
        }
        public bool ScanSwitch
        {
            get { return _scanSwitch; }
            set { _scanSwitch = value; OnPropertyChanged(() => this.ScanSwitch); }
        }
        public bool ITUSwitch
        {
            get { return _ituSwitch; }
            set { _ituSwitch = value; OnPropertyChanged(() => this.ITUSwitch); }
        }

        #endregion 数据开关

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
                    {
                        _socketXml.Close();
                    }
                    //if (_socketData != null)
                    //    _socketData.Close();
                    Connected = false;
                    IsRunning = false;
                }
                _tcpListenerXml.Stop();
                _tcpListenerData.Stop();
                ClearClient();
            }
        }

        public override void TaskPause(bool pause)
        {
        }

        protected override UserControl GetControl()
        {
            return _control;
        }

        protected override Stream GetStream2()
        {
            return _socketXml == null ? null : new NetworkStream(_socketXml);
        }

        protected override Stream GetStream1()
        {
            return _socketXml == null ? null : new NetworkStream(_socketXml);
        }

        #region 私有方法

        private void XmlClientCallback(IAsyncResult asyncResult)
        {
            if (!DeviceInitialized)
            {
                return;
            }

            try
            {
                _socketXml = _tcpListenerXml.EndAcceptSocket(asyncResult);

                SetHeartBeat(_socketXml);
                Connected = true;
                IPEndPoint iPEndPoint = _socketXml.RemoteEndPoint as IPEndPoint;
                ClientInfo client = new ClientInfo();
                client.Address = iPEndPoint;
                client.IsXml = true;
                _dispatcher?.Invoke(new Action(() => ClientList.Add(client)));
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
            {
                return;
            }

            try
            {
                var socket = _tcpListenerData.EndAcceptSocket(asyncResult);
                //_socketXml.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.KeepAlive,);
                Connected = true;
                IPEndPoint iPEndPoint = socket.RemoteEndPoint as IPEndPoint;
                ClientInfo client = new ClientInfo();
                client.Address = iPEndPoint;
                client.IsXml = false;
                client.ClientSocket = socket;
                _dispatcher?.Invoke(new Action(() => ClientList.Add(client)));
                Thread thd = new Thread(DataSendSync);
                thd.IsBackground = true;
                thd.Start(ClientList);
            }
            catch (Exception)
            {
            }
            _tcpListenerData.BeginAcceptSocket(new AsyncCallback(DataClientCallback), _tcpListenerData);
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
            IPEndPoint iPEndPoint = socket.RemoteEndPoint as IPEndPoint;
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
                    CloseConnect(iPEndPoint);
                }
            }
        }

        /// <summary>
        /// 关闭链接
        /// </summary>
        /// <param name="socket"></param>

        private void CloseConnect(IPEndPoint iPEndPoint)
        {
            if (!DeviceInitialized)
            {
                return;
            }

            _socketXml?.Close();
            var list = _clientList.Where(i => i.Address.Address.ToString().Equals(iPEndPoint.Address.ToString())).ToList();
            if (list.Count() > 0)
            {
                list.ForEach(client =>
                 {
                     client.Stop();
                     _dispatcher?.Invoke(new Action(() => ClientList.Remove(client)));
                 });
            }
            if (_clientList.Count == 0)
            {
                Connected = false;
            }

            _tcpListenerXml.BeginAcceptSocket(new AsyncCallback(XmlClientCallback), _tcpListenerXml);
            _tcpListenerData.BeginAcceptSocket(new AsyncCallback(DataClientCallback), _tcpListenerData);
        }

        private void ClearClient()
        {
            foreach(ClientInfo client in ClientList)
            {
                client.Stop();
            }
            ClientList.Clear();
        }

        #endregion 心跳检测

    }
}
