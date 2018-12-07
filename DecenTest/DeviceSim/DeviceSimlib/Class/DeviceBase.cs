
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

        private Stream _deviceStreamSend
        {
            get { return GetStreamSendData(); }
        }

        private Stream _deviceStreamRecv
        {
            get { return GetStreamRecvData(); }
        }

        #endregion 变量/属性

        public DeviceBase()
        {
            Initialize();
        }

        public abstract void Initialize();

        public abstract void Start();
        public abstract void Stop();

        public abstract void TaskPause(bool pause);

        /// <summary>
        /// 设备是否可以初始化
        /// </summary>
        public abstract bool CanDeviceIni
        { get; }

        /// <summary>
        /// 获取下发参数的流
        /// </summary>
        /// <returns></returns>
        protected abstract Stream GetStreamSendData();

        /// <summary>
        /// 获取接收数据的流
        /// </summary>
        /// <returns></returns>
        protected abstract Stream GetStreamRecvData();

        protected abstract UserControl GetControl();

        /// <summary>
        /// 通过发送数据通道写数据
        /// </summary>
        /// <param name="data"></param>
        protected virtual void WriteSendData(byte[] data)
        {
            if (_deviceStreamSend == null)
                return;
            if (data == null || data.Length == 0)
                return;
            _deviceStreamSend.Write(data, 0, data.Length);
            _deviceStreamSend.Flush();
        }
        /// <summary>
        /// 通过接收数据通道写数据
        /// </summary>
        /// <param name="data"></param>
        protected virtual void WriteRecvData(byte[] data)
        {
            if (_deviceStreamRecv == null)
                return;
            if (data == null || data.Length == 0)
                return;
            _deviceStreamRecv.Write(data, 0, data.Length);
            _deviceStreamRecv.Flush();
        }
        /// <summary>
        /// 通过发送数据通道读数据
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        protected virtual int ReadSendData(byte[] buffer, int offset, int count)
        {
            if (_deviceStreamSend == null)
                return 0;
            return _deviceStreamSend.Read(buffer, offset, count);
        }
        /// <summary>
        /// 通过接收数据通道读数据
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        protected virtual int ReadRecvData(byte[] buffer, int offset, int count)
        {
            if (_deviceStreamRecv == null)
                return 0;
            return _deviceStreamRecv.Read(buffer, offset, count);
        }

        protected virtual void SendStrShow(string msg)
        {
            _dispatcher?.Invoke(new Action(() =>
            {
                SendStringList.Add(new MessageInfo(msg.TrimEnd(new char[] { '\r', '\n' })));
                if (SendStringList.Count > 1000)
                    SendStringList.RemoveAt(0);
            }));
        }

        protected virtual void RecvStrShow(string msg)
        {
            _dispatcher?.Invoke(new Action(() =>
            {
                RecvStringList.Add(new MessageInfo(msg.TrimEnd(new char[] { '\r', '\n' })));
                if (RecvStringList.Count > 1000)
                    RecvStringList.RemoveAt(0);
            }));
        }

    }

}
