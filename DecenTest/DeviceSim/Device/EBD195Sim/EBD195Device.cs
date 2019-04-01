
/*********************************************************************************************
 *	
 * 文件名称:    Device.cs
 *
 * 作    者:    吴德鹏
 *	
 * 创作日期:    2018-10-18 11:42:42
 * 
 * 备    注:    描述类的用途
 *              
 *                                
 *               
*********************************************************************************************/

using DeviceSimlib;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace DeviceSim.Device
{
    [Serializable]
    public class EBD195Device : DeviceBase
    {
        #region 变量/属性
        private Thread _thdSend = null;
        private Random _random = new Random();
        private DateTime _aliveTime = DateTime.Now;
        private DateTime _lastSendTime = DateTime.Now;

        static bool _isReadCompass = false;
        static int _compassPosition = 0;

        private SerialPort _serialPort;
        private string _comPort = "COM1";
        private int _normalDDF = 135;
        private int _normalLevel = 40;
        [XmlIgnore]
        private ObservableCollection<string> _comPortList = new ObservableCollection<string>();
        private bool _isHasCompass = true;
        [XmlIgnore]
        private EBD195View _control = null;
        private int _interTime = 1000;//ms

        public int IntegrationTime
        {
            get { return _interTime; }
            set
            {
                if (_interTime != value)
                {
                    _interTime = value;
                    OnPropertyChanged(() => this.IntegrationTime);
                }
            }
        }

        /// <summary>
        /// 设定示相角度
        /// </summary>
        public int SettingAngleDDF
        {
            get { return _normalDDF; }
            set
            {
                if (_normalDDF != value)
                {
                    _normalDDF = value;
                    OnPropertyChanged(() => SettingAngleDDF);
                }
            }
        }

        public int SettingLevel
        {
            get { return _normalLevel; }
            set
            {
                if (_normalLevel != value)
                {
                    _normalLevel = value;
                    OnPropertyChanged(() => this.SettingLevel);
                }
            }
        }

        public string ComPort
        {
            get { return _comPort; }
            set
            {
                if (_comPort != value)
                {
                    _comPort = value;
                    OnPropertyChanged(() => this.ComPort);
                }
            }
        }

        [XmlIgnore]
        public ObservableCollection<string> ComportList
        {
            get { return _comPortList; }
            set
            {
                if (_comPortList != value)
                {
                    _comPortList = value;
                    OnPropertyChanged(() => this.ComportList);
                }
            }
        }

        public DateTime AliveTime
        {
            get { return _aliveTime; }
            set
            {
                if (_aliveTime != value)
                {
                    _aliveTime = value;
                    OnPropertyChanged(() => this.AliveTime);
                }
            }
        }

        public bool IsHasCompass
        {
            get { return _isHasCompass; }
            set
            {
                if (_isHasCompass != value)
                {
                    _isHasCompass = value;
                    OnPropertyChanged(() => this.IsHasCompass);
                }
            }
        }

        #endregion 变量/属性

        public EBD195Device() : base()
        {
            this.Name = "EBD195";
            _control = new EBD195View(this);
            _dispatcher = _control.Dispatcher;
        }

        public override void Initialize()
        {
            DeviceInitialized = false;
            IsRunning = false;
            string tmpPort = _comPort;
            ComportList.Clear();
            foreach (var port in SerialPort.GetPortNames())
            {
                ComportList.Add(port);
            }
            ComPort = ComportList.Contains(tmpPort) ? tmpPort : "";
        }

        public override void Start()
        {
            DeviceInitialized = true;
            _serialPort = new SerialPort(_comPort, 9600, Parity.None, 8, StopBits.One);
            _serialPort.DataReceived += SerialPort_DataReceived;
            _serialPort.PinChanged += SerialPort_PinChanged;
            _serialPort.Open();

            _thdSend = new Thread(ScanSend);
            _thdSend.IsBackground = true;
            _thdSend.Start();
        }

        public override void Stop()
        {
            DeviceInitialized = false;
            if (_serialPort.IsOpen)
            {
                _serialPort.DataReceived -= SerialPort_DataReceived;
                _serialPort.PinChanged -= SerialPort_PinChanged;
                _serialPort.Close();
            }
        }

        public override void TaskPause(bool pause)
        {
            _isPause = pause;
        }

        public override bool CanDeviceIni
        {
            get
            {
                return !string.IsNullOrEmpty(this._comPort);
            }
        }

        protected override Stream GetStreamSendData()
        {
            return _serialPort == null || !_serialPort.IsOpen ? null : _serialPort.BaseStream;
        }

        protected override Stream GetStreamRecvData()
        {
            return _serialPort == null || !_serialPort.IsOpen ? null : _serialPort.BaseStream;
        }

        protected override UserControl GetControl()
        {
            return _control;
        }

        private void SerialPort_PinChanged(object sender, SerialPinChangedEventArgs e)
        {
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            byte[] buffer = new byte[_serialPort.ReadBufferSize];
            string data = "";
            int recvCount = ReadRecvData(buffer, 0, buffer.Length);
            data = System.Text.Encoding.ASCII.GetString(buffer, 0, recvCount);
            RecvStrShow(data);
            Console.WriteLine(string.Format("Time:{0:HH:mm:ss.fff} Data:{1}", DateTime.Now, data));
            if (data.Contains("D1"))
            {
                IsRunning = true;
                Console.WriteLine("Task Start……\r\n=============================");
            }
            if (data.Contains("D0"))
            {
                IsRunning = false;
                Console.WriteLine("Task Stop……\r\n=============================");
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
                        IntegrationTime = 100;
                        break;
                    case 1:
                        IntegrationTime = 200;
                        break;
                    case 2:
                        IntegrationTime = 500;
                        break;
                    case 3:
                        IntegrationTime = 1000;
                        break;
                    case 4:
                        IntegrationTime = 2000;
                        break;
                    case 5:
                        IntegrationTime = 5000;
                        break;
                    default:
                        IntegrationTime = 100;
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

        private void ScanSend()
        {
            while (_deviceInitialized)
            {
                Thread.Sleep(10);
                try
                {
                    if (_isPause)
                    {
                        int span = (int)DateTime.Now.Subtract(_aliveTime).TotalSeconds;
                        if (_pauseSpan == 0)
                        {
                            continue;
                        }
                        else if (span < _pauseSpan)
                        {
                            continue;
                        }
                    }

                    if (!_isRunning)
                    {
                        int span = (int)DateTime.Now.Subtract(_aliveTime).TotalMilliseconds;
                        if (span < 1000)
                        {
                            continue;
                        }

                        string sendStr = "A*,*,*,2\r\n";
                        SendStrShow(sendStr);
                        byte[] buffer = Encoding.ASCII.GetBytes(sendStr);
                        WriteSendData(buffer);
                        _aliveTime = DateTime.Now;
                    }
                    else
                    {
                        int span = (int)DateTime.Now.Subtract(_lastSendTime).TotalMilliseconds;
                        if (span < _interTime)
                        {
                            continue;
                        }

                        _lastSendTime = DateTime.Now;
                        _aliveTime = DateTime.Now;

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
                        int level = _normalLevel + _random.Next(-5, 5);
                        string sendData = string.Format("A{0},{1},{2},{3}\r\n", ddf, quality, time, level);
                        if (_isErr)
                        {
                            sendData = string.Format("A*,*,*,{3}\r\n", ddf, quality, time, level);
                        }

                        SendStrShow(sendData);
                        byte[] buffer = Encoding.ASCII.GetBytes(sendData);
                        WriteSendData(buffer);
                    }
                    if (_isReadCompass)
                    {
                        _isReadCompass = false;
                        _compassPosition += 5;
                        if (_compassPosition == 360)
                        {
                            _compassPosition = 0;
                        }

                        string sendData = _isHasCompass ? string.Format("C{0}\r\n", _compassPosition) : "C999\r\n";
                        Console.WriteLine("Send:" + sendData);
                        SendStrShow(sendData);
                        byte[] buffer = Encoding.ASCII.GetBytes(sendData);
                        WriteSendData(buffer);
                    }
                }
                catch
                {
                }
            }
        }
    }
}
