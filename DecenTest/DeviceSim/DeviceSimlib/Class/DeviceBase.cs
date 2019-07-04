
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
using System.Windows.Threading;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Xml.Serialization;
using System.IO;
using System.Windows.Controls;
using System.Net.Sockets;

namespace DeviceSimlib
{
    [Serializable]
    public abstract class DeviceBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(Expression<Func<object>> expression)
        {
            PropertyChanged.Notify(expression);
        }

        #region 变量/属性

        protected bool _isPause = false;
        protected Dispatcher _dispatcher = null;

        #region 界面展示

        [XmlIgnore]
        private ObservableCollection<MessageInfo> _recvStringList = new ObservableCollection<MessageInfo>();
        [XmlIgnore]
        private ObservableCollection<MessageInfo> _sendStringList = new ObservableCollection<MessageInfo>();
        [XmlIgnore]
        protected bool _deviceInitialized = false;
        [XmlIgnore]
        protected bool _isRunning = false;
        protected int _pauseSpan = 10;
        protected bool _isErr = false;
        private string _name;
        private bool _connected = false;


        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    PropertyChanged.Notify(() => this.Name);
                }
            }
        }

        /// <summary>
        /// 设备初始化状态
        /// </summary>
        [XmlIgnore]
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

        /// <summary>
        /// 任务运行状态
        /// </summary>
        [XmlIgnore]
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

        /// <summary>
        /// 与设备连接状态
        /// </summary>
        [XmlIgnore]
        public bool Connected
        {
            get { return _connected; }
            set
            {
                if (_connected != value)
                {
                    _connected = value;
                    PropertyChanged.Notify(() => this.Connected);
                }
            }
        }

        public int PauseSpan
        {
            get { return _pauseSpan; }
            set
            {
                if (_pauseSpan != value)
                {
                    _pauseSpan = value;
                    PropertyChanged.Notify(() => this.PauseSpan);
                }
            }
        }

        [XmlIgnore]
        public UserControl Control
        {
            get { return GetControl(); }
        }
        [XmlIgnore]
        public ObservableCollection<MessageInfo> RecvStringList
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

        [XmlIgnore]
        public ObservableCollection<MessageInfo> SendStringList
        {
            get { return _sendStringList; }
            set
            {
                if (_sendStringList != value)
                {
                    _sendStringList = value;
                    PropertyChanged.Notify(() => this.SendStringList);
                }
            }
        }

        #endregion 界面展示

        private Stream _deviceStream1
        {
            get { return GetStream1(); }
        }

        private Stream _deviceStream2
        {
            get { return GetStream2(); }
        }

        #endregion 变量/属性

        /// <summary>
        /// 构造函数
        /// </summary>
        public DeviceBase()
        {
            Initialize();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public abstract void Initialize();
        
        /// <summary>
        /// 启动监听
        /// </summary>
        public abstract void Start();

        /// <summary>
        /// 停止监听
        /// </summary>
        public abstract void Stop();

        public abstract void TaskPause(bool pause);

        /// <summary>
        /// 设备是否可以初始化
        /// </summary>
        public abstract bool CanDeviceIni
        { get; }

        /// <summary>
        /// 获取数据通道1
        /// </summary>
        /// <returns></returns>
        protected abstract Stream GetStream1();

        /// <summary>
        /// 获取数据通道2
        /// </summary>
        /// <returns></returns>
        protected abstract Stream GetStream2();

        protected abstract UserControl GetControl();

        /// <summary>
        /// 通过通道1写数据
        /// </summary>
        /// <param name="data"></param>
        protected virtual void WriteDataByStream1(byte[] data)
        {
            if (_deviceStream1 == null)
            {
                return;
            }
            if (data == null || data.Length == 0)
            {
                return;
            }
            _deviceStream1.Write(data, 0, data.Length);
            _deviceStream1.Flush();
        }
        /// <summary>
        /// 通过通道2写数据
        /// </summary>
        /// <param name="data"></param>
        protected virtual void WriteDataByStream2(byte[] data)
        {
            if (_deviceStream2 == null)
            {
                return;
            }
            if (data == null || data.Length == 0)
            {
                return;
            }
            _deviceStream2.Write(data, 0, data.Length);
            _deviceStream2.Flush();
        }
        /// <summary>
        /// 通过通道1读数据
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        protected virtual int ReadDataByStream1(byte[] buffer, int offset, int count)
        {
            if (_deviceStream1 == null)
            {
                return 0;
            }
            return _deviceStream1.Read(buffer, offset, count);
        }
        /// <summary>
        /// 通过通道2读数据
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        protected virtual int ReadDataByStream2(byte[] buffer, int offset, int count)
        {
            if (_deviceStream2 == null)
            {
                return 0;
            }
            return _deviceStream2.Read(buffer, offset, count);
        }

        /// <summary>
        /// 读取指定长度的数据到数组
        /// </summary>
        /// <param name="buffer">接收数据缓冲区</param>
        /// <param name="offset">缓冲区偏移</param>
        /// <param name="bytesToRead">要读取的字节数</param>
        /// <param name="socket">要接收数据的套接字</param>
        protected void ReceiveData(byte[] buffer, int offset, int bytesToRead, Socket socket)
        {
            // 当前已接收到的字节数
            int totalRecvLen = 0;

            // 循环接收数据，确保接收完指定字节数
            while (totalRecvLen < bytesToRead)
            {
                int recvLen = socket.Receive(buffer, offset + totalRecvLen, bytesToRead - totalRecvLen, SocketFlags.None);
                if (recvLen <= 0)
                {
                    // 远程主机使用close或shutdown关闭连接，并且所有数据已被接收的时候，此处不会抛异常而是立即返回0
                    // 为避免出现此情况将导致该函数列循环，此处直接抛SocketException异常
                    // 10054:远程主机强迫关闭了一个现在连接
                    throw new SocketException(10054);
                }

                totalRecvLen += recvLen;
            }
        }

        protected virtual void SendStrShow(string msg)
        {
            _dispatcher?.Invoke(new Action(() =>
            {
                SendStringList.Add(new MessageInfo(msg.TrimEnd(new char[] { '\r', '\n' })));
                if (SendStringList.Count > 1000)
                {
                    SendStringList.RemoveAt(0);
                }
            }));
        }

        protected virtual void RecvStrShow(string msg)
        {
            _dispatcher?.Invoke(new Action(() =>
            {
                RecvStringList.Add(new MessageInfo(msg.TrimEnd(new char[] { '\r', '\n' })));
                if (RecvStringList.Count > 1000)
                {
                    RecvStringList.RemoveAt(0);
                }
            }));
        }

    }

}
