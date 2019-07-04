
/*********************************************************************************************
 *	
 * 文件名称:    Define.cs
 *
 * 作    者:    吴德鹏
 *	
 * 创作日期:    2018-11-28 14:51:47
 * 
 * 备    注:    描述类的用途
 *              
 *                                
 *               
*********************************************************************************************/

using NotificationExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DeviceSim.Device
{
    class Define
    {
    }

    public class ClientInfo : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private IPEndPoint _address;
        private Socket _socket = null;

        private bool _isXml = false;
        private bool _isSendAudio = false;
        private bool _isSendIQ = false;
        private bool _isSendCW = false;
        private bool _isSendSpectrum = false;
        private bool _isSendDF = false;
        private bool _isSendITU = false;
        private bool _isSendPScan = false;

        public IPEndPoint Address
        {
            get { return _address; }
            set
            {
                _address = value;
                PropertyChanged.Notify(() => this.Address);
            }
        }

        public Socket ClientSocket
        {
            get { return _socket; }
            set { _socket = value; }
        }

        public bool IsXml
        {
            get { return _isXml; }
            set
            {
                _isXml = value;
                PropertyChanged.Notify(() => this.IsXml);
            }
        }
        public bool IsSendAudio
        {
            get { return _isSendAudio; }
            set
            {
                _isSendAudio = value;
                PropertyChanged.Notify(() => this.IsSendAudio);
            }
        }
        public bool IsSendIQ
        {
            get { return _isSendIQ; }
            set
            {
                _isSendIQ = value;
                PropertyChanged.Notify(() => this.IsSendIQ);
            }
        }
        public bool IsSendCW
        {
            get { return _isSendCW; }
            set
            {
                _isSendCW = value;
                PropertyChanged.Notify(() => this.IsSendCW);
            }
        }
        public bool IsSendITU
        {
            get { return _isSendITU; }
            set
            {
                _isSendITU = value;
                PropertyChanged.Notify(() => this.IsSendITU);
            }
        }
        public bool IsSendSpectrum
        {
            get { return _isSendSpectrum; }
            set
            {
                _isSendSpectrum = value;
                PropertyChanged.Notify(() => this.IsSendSpectrum);
            }
        }
        public bool IsSendDF
        {
            get { return _isSendDF; }
            set
            {
                _isSendDF = value;
                PropertyChanged.Notify(() => this.IsSendDF);
            }
        }

        public bool IsSendPScan
        {
            get { return _isSendPScan; }
            set
            {
                _isSendPScan = value;
                PropertyChanged.Notify(() => this.IsSendPScan);
            }
        }

        public void SendData(byte[] data)
        {
            if (_socket == null || !_socket.Connected)
            {
                return;
            }
            if (data != null)
            {
                _socket.Send(data);
            }
        }

        public void Stop()
        {
            if (_socket != null)
            {
                try
                {
                    _socket.Close();
                }
                catch
                {
                    _socket.Dispose();
                }
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }

    public class ScanRangeInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int _id = 0;
        private double _startFrequency = 88;
        private double _stopFrequency = 108;
        private double _step = 200;
        private double _span = 200;
        private int _numHops = 1;


        public int ID
        {
            get { return _id; }
            set
            {
                _id = value;
                PropertyChanged.Notify(() => this.ID);
            }
        }
        /// <summary>
        /// 起始频率MHz
        /// </summary>
        public double StartFrequency
        {
            get { return _startFrequency; }
            set
            {
                _startFrequency = value;
                PropertyChanged.Notify(() => this.StartFrequency);
            }
        }

        /// <summary>
        /// 结束频率MHz
        /// </summary>
        public double StopFrequency
        {
            get { return _stopFrequency; }
            set
            {
                _stopFrequency = value;
                PropertyChanged.Notify(() => this.StopFrequency);
            }
        }

        /// <summary>
        /// 步进KHz
        /// </summary>
        public double Step
        {
            get { return _step; }
            set
            {
                _step = value;
                PropertyChanged.Notify(() => this.Step);
            }
        }
        /// <summary>
        /// 频谱带宽KHz
        /// </summary>
        public double Span
        {
            get { return _span; }
            set
            {
                _span = value;
                PropertyChanged.Notify(() => this.Span);
            }
        }
        public int NumHops
        {
            get { return _numHops; }
            set
            {
                _numHops = value;
                PropertyChanged.Notify(() => this.NumHops);
            }
        }

    }
}
