using DeviceSimlib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace DeviceSim.Device
{
    public partial class DemoSwivelDevice : DeviceBase
    {
        #region 变量/定义

        [XmlIgnore]
        private DemoSwivelView _control = null;

        [XmlIgnore]
        private Socket _socketSwivel = null;
        [XmlIgnore]
        private Socket _socketReceiver = null;
        [XmlIgnore]
        private Socket _socketGenerator = null;

        private int _portSwivel = 1721;
        private int _portReceiver = 1722;
        private int _portGenerator = 1723;

        public int PortSwivel
        {
            get { return _portSwivel; }
            set { if (_portSwivel != value) { _portSwivel = value; OnPropertyChanged(() => this.PortSwivel); } }
        }
        public int PortReceiver
        {
            get { return _portReceiver; }
            set { if (_portReceiver != value) { _portReceiver = value; OnPropertyChanged(() => this.PortReceiver); } }
        }
        public int PortGenerator
        {
            get { return _portGenerator; }
            set { if (_portGenerator != value) { _portGenerator = value; OnPropertyChanged(() => this.PortGenerator); } }
        }

        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        public DemoSwivelDevice()
        {
            this.Name = "DemoSwivel";
            _control = new DemoSwivelView(this);
            _dispatcher = _control.Dispatcher;
        }

        #region DeviceBase

        public override bool CanDeviceIni
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override void Initialize()
        {
            throw new NotImplementedException();
        }

        public override void Start()
        {
            throw new NotImplementedException();
        }

        public override void Stop()
        {
            throw new NotImplementedException();
        }

        public override void TaskPause(bool pause)
        {
            throw new NotImplementedException();
        }

        protected override UserControl GetControl()
        {
            return _control;
        }

        protected override Stream GetStream1()
        {
            throw new NotImplementedException();
        }

        protected override Stream GetStream2()
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
