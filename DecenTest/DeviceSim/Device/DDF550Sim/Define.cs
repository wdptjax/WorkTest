
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
using System.Text;

namespace DeviceSim.Device
{
    class Define
    {
    }

    public class ClientInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private string _addressXml;
        private string _addressData;
        private int _portXml;
        private int _portData;

        private bool _isSendAudio = false;
        private bool _isSendIQ = false;
        private bool _isSendCW = false;
        private bool _isSendSpectrum = false;
        private bool _isSendDF = false;
        private bool _isSendITU = false;
        private bool _isSendPScan = false;

        public string AddressXml
        {
            get { return _addressXml; }
            set
            {
                _addressXml = value;
                PropertyChanged.Notify(() => this.AddressXml);
            }
        }
        public int PortXml
        {
            get { return _portXml; }
            set
            {
                _portXml = value;
                PropertyChanged.Notify(() => this.PortXml);
            }
        }
        public string AddressData
        {
            get { return _addressData; }
            set
            {
                _addressData = value;
                PropertyChanged.Notify(() => this.AddressData);
            }
        }
        public int PortData
        {
            get { return _portData; }
            set
            {
                _portData = value;
                PropertyChanged.Notify(() => this.PortData);
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
