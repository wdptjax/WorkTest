
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

        private bool _isShown = false;

        #region 界面展示

        [XmlIgnore]
        private ObservableCollection<MessageInfo> _recvStringList = new ObservableCollection<MessageInfo>();
        [XmlIgnore]
        private ObservableCollection<MessageInfo> _sendStringList = new ObservableCollection<MessageInfo>();
        protected bool _deviceInitialized = false;
        protected bool _isRunning = false;
        protected int _interTime = 1000;//ms
        protected int _pauseSpan = 10;
        protected bool _isErr = false;
        private string _name;

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

        public bool IsShown
        {
            get { return _isShown; }
            set { _isShown = value; }
        }

        private Stream _deviceStream
        {
            get { return GetStream(); }
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

        public abstract bool CanDeviceIni
        { get; }

        protected abstract Stream GetStream();

        protected abstract UserControl GetControl();

        protected virtual void WriteData(byte[] data)
        {
            if (_deviceStream == null)
                return;
            _deviceStream.Write(data, 0, data.Length);
            _deviceStream.Flush();
        }

        protected virtual int ReadData(byte[] buffer, int offset, int count)
        {
            if (_deviceStream == null)
                return 0;
            return _deviceStream.Read(buffer, offset, count);
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
