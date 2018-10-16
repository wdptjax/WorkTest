﻿
/*********************************************************************************************
 *	
 * 文件名称:    Device.cs
 *
 * 作    者:    吴德鹏
 *	
 * 创作日期:    2018-9-26 15:48:01
 * 
 * 备    注:    描述类的用途
 *              
 *                                
 *               
*********************************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using NotificationExtensions;
using System.IO.Ports;
using System.Collections.ObjectModel;
using System.Threading;

namespace EBD195Sim
{
    public class Device : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region 变量/属性

        private SerialPort _serialPort;
        private Thread _thdSend = null;
        private Random _random = new Random();
        private DateTime _aliveTime = DateTime.Now;
        private bool _isPause = false;
        private int _pauseSpan = 0;
        private DateTime _lastSendTime = DateTime.Now;

        #region 界面展示

        private ObservableCollection<string> _recvStringList = new ObservableCollection<string>();
        private ObservableCollection<string> _comPortList = new ObservableCollection<string>();
        private string _comPort = "COM1";
        private bool _deviceInitialized = false;
        private bool _isRunning = false;
        private int _interTime = 1000;//ms
        private int _normalDDF = 135;

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
                    PropertyChanged.Notify(() => this.SettingAngleDDF);
                }
            }
        }

        public int IntegrationTime
        {
            get { return _interTime; }
            set
            {
                if (_interTime != value)
                {
                    _interTime = value;
                    PropertyChanged.Notify(() => this.IntegrationTime);
                }
            }
        }

        public bool DeviceInitialized
        {
            get { return _deviceInitialized; }
            set
            {
                if (_deviceInitialized != value)
                {
                    _deviceInitialized = value;
                    PropertyChanged.Notify(() => this.DeviceInitialized);
                }
            }
        }

        public bool IsRunning
        {
            get { return _isRunning; }
            set
            {
                if (_isRunning != value)
                {
                    _isRunning = value;
                    PropertyChanged.Notify(() => this.IsRunning);
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
                    PropertyChanged.Notify(() => this.ComPort);
                }
            }
        }
        public ObservableCollection<string> RecvStringList
        {
            get { return _recvStringList; }
            set
            {
                if (_recvStringList != value)
                {
                    _recvStringList = value;
                    PropertyChanged.Notify(() => this.RecvStringList);
                }
            }
        }
        public ObservableCollection<string> ComportList
        {
            get { return _comPortList; }
            set
            {
                if (_comPortList != value)
                {
                    _comPortList = value;
                    PropertyChanged.Notify(() => this.ComportList);
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
                    PropertyChanged.Notify(() => this.AliveTime);
                }
            }
        }

        #endregion 界面展示

        #endregion 变量/属性

        public Device()
        {
            Initialize();
        }

        public void Initialize()
        {
            foreach (var port in SerialPort.GetPortNames())
            {
                ComportList.Add(port);
            }
        }

        public void Start()
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

        public void Stop()
        {
            DeviceInitialized = false;
            if (_serialPort.IsOpen)
            {
                _serialPort.DataReceived -= SerialPort_DataReceived;
                _serialPort.PinChanged -= SerialPort_PinChanged;
                _serialPort.Close();
            }
        }

        private void SerialPort_PinChanged(object sender, SerialPinChangedEventArgs e)
        {
        }
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            byte[] buffer = new byte[_serialPort.ReadBufferSize];
            string data = "";
            int recvCount = _serialPort.Read(buffer, 0, buffer.Length);
            data = System.Text.Encoding.ASCII.GetString(buffer, 0, recvCount);
            RecvStringList.Add(data);
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
        }

        private void ScanSend()
        {
            while (_deviceInitialized)
            {
                Thread.Sleep(10);

                if (_isPause)
                {
                    int span = (int)DateTime.Now.Subtract(_aliveTime).TotalMilliseconds;
                    if (_pauseSpan == 0)
                        continue;
                    else if (span < _pauseSpan)
                        continue;
                }

                if (!_isRunning)
                {
                    int span = (int)DateTime.Now.Subtract(_aliveTime).TotalMilliseconds;
                    if (span < 1000)
                        continue;

                    string sendStr = "A*,*,*,2\r\n";
                    byte[] buffer = Encoding.ASCII.GetBytes(sendStr);
                    _serialPort.Write(buffer, 0, buffer.Length);
                    _aliveTime = DateTime.Now;
                    continue;
                }
                {
                    int span = (int)DateTime.Now.Subtract(_lastSendTime).TotalMilliseconds;
                    if (span < _interTime)
                        continue;
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
                    int level = _random.Next(40, 50);
                    string sendData = string.Format("A{0},{1},{2},{3}\r\n", ddf, quality, time, level);
                    byte[] buffer = Encoding.ASCII.GetBytes(sendData);
                    _serialPort.Write(buffer, 0, buffer.Length);
                }
            }
        }

    }
}
