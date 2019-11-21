using DeviceSimlib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace DeviceSim.Device
{
    [Serializable]
    public class ADS_B_USBDevice : DeviceBase
    {
        #region 变量/定义

        [XmlIgnore]
        private ADS_B_USBView _control = null;
        [XmlIgnore]
        private ObservableCollection<string> _comPortList = new ObservableCollection<string>();

        private string _comPort = "COM1";
        private SerialPort _serialPort;
        private Thread _thdSend = null;
        private Random _random = new Random();
        private byte _sequencyNo = 0;
        private TrafficReportMessage _message = new TrafficReportMessage();


        public string ComPort
        {
            get { return _comPort; }
            set
            {
                if (_comPort != value)
                {
                    _comPort = value;
                    OnPropertyChanged(() => ComPort);
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

        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        public ADS_B_USBDevice()
        {
            this.Name = "ADS_B_USB";
            _control = new ADS_B_USBView(this);
            _dispatcher = _control.Dispatcher;
        }

        #region DeviceBase

        public override bool CanDeviceIni
        {
            get
            {
                return !string.IsNullOrEmpty(_comPort);
            }
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
            IniMessage();
            DeviceInitialized = true;
            IsRunning = true;
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
            IsRunning = false;
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

        protected override UserControl GetControl()
        {
            return _control;
        }

        protected override Stream GetStream1()
        {
            return _serialPort == null || !_serialPort.IsOpen ? null : _serialPort.BaseStream;
        }

        protected override Stream GetStream2()
        {
            return null;
        }

        #endregion

        private void IniMessage()
        {
            _message.ICAO_address = 777888;
            _message.lat = 1040636110;
            _message.lon = 305477780;
            _message.altitude = 500000;
            _message.heading = 100;
            _message.hor_velocity = 8000;
            _message.hor_velocity = 8000;
            _message.validFlags = 0x00ff;
            _message.squawk = 0xffff;
            _message.altitude_type = 0;
            _message.callsign = "3U8986";
            _message.emitter_type = 0;
            _message.tslc = 0;
        }

        private void SerialPort_PinChanged(object sender, SerialPinChangedEventArgs e)
        {
        }
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {

        }
        private void ScanSend()
        {
            while (_deviceInitialized)
            {
                try
                {
                    if (DeviceInitialized)
                    {
                        byte[] buffer = GetSimData();
                        WriteDataByStream1(buffer);
                    }
                }
                catch (Exception)
                {
                }
                Thread.Sleep(500);
            }
        }

        private byte[] GetSimData()
        {
            PacketHeader header = new PacketHeader();
            header.MessageID = 246;
            header.PayloadLength = (byte)Marshal.SizeOf(typeof(TrafficReportMessage));
            header.PacketSequence = _sequencyNo;
            _sequencyNo++;
            if (_sequencyNo >= byte.MaxValue)
            {
                _sequencyNo = 0;
            }

            TrafficReportMessage message = _message;
            message.lat += _random.Next(-10000, 10000);
            message.lon += _random.Next(-10000, 10000);
            byte[] buffer = new byte[Marshal.SizeOf(header) + Marshal.SizeOf(message) + 2];
            byte[] data1 = header.ToBytes();
            byte[] data2 = message.ToBytes();
            Buffer.BlockCopy(data1, 0, buffer, 0, data1.Length);
            Buffer.BlockCopy(data2, 0, buffer, data1.Length, data2.Length);

            //string msg = string.Format("");

            return buffer;
        }
    }
}
