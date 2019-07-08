/*********************************************************************************************
 *	
 * 文件名称:    ..\Tracker800\Server\Source\Device\Receiver\MR3300A\MR3000AParams.cs
 *
 * 作    者:		陈鹏 
 *	
 * 创作日期:    2018/05/17
 * 
 * 修    改:    无
 * 
 * 备    注:		MR3000A系列接收机参数
 *                                            
*********************************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Globalization;
using Common;

namespace MR3300A
{

    public partial class MR3300A
    {
        #region 常规参数

        private double _frequency;
        public double Frequency
        {
            get { lock (_parameterLock) { return _frequency; } }
            set
            {
                lock (_parameterLock)
                {
                    var temp = (long)(value * 1000000);
                    if (_frequencyOffsetDic.ContainsKey(temp))
                    {
                        _frequency = _frequencyOffsetDic[temp] / 1000000.0d;
                    }
                    else
                    {
                        _frequency = value;
                    }
                    SendCommand(string.Format("FREQ {0} MHz", _frequency));
                }
            }
        }

        private double _spectrumSpan;
        public double SpectrumSpan
        {
            get { return _spectrumSpan; }
            set
            {
                _spectrumSpan = value;
                SendCommand(string.Format("FREQ:SPAN {0} kHz", value));
            }
        }

        private double _ifBandWidth;
        public double IFBandWidth
        {
            get { return _ifBandWidth; }
            set
            {
                _ifBandWidth = value;
                SendCommand(string.Format("BAND {0} kHz", value));
            }
        }

        private RFMode _rfMode;
        public RFMode RFMode
        {
            get { return _rfMode; }
            set
            {
                _rfMode = value;
                SendCommand(string.Format("ATT:RF:MODE {0}", value));
            }
        }

        private string _autoAttenuation;
        public string AutoAttenuation
        {
            get { return _autoAttenuation; }
            set
            {
                _autoAttenuation = value;
                if (_autoAttenuation.ToLower().Equals("agc"))
                {
                    SendCommand("ATT:AUT ON"); // 上面两行写得好无辜，这特么用上自动档了（AUTO），还非要说你开车不用哈手动，C1岂不是白考了！
                }
                else
                {
                    SendCommand("ATT:AUT OFF");
                    SendCommand(string.Format("ATT:RF {0} dB", _rfAttenuation));
                    // SendCommand(string.Format("ATT:IF {0} dB", _ifAttenuation));
                }
            }
        }

        private int _rfAttenuation;
        public int RFAttenuation
        {
            get { return _rfAttenuation; }
            set
            {
                _rfAttenuation = value;
                if (value % 2 != 0) // 射频衰减步进为2
                {
                    _rfAttenuation -= 1;
                }
                if (_autoAttenuation.ToLower().Equals("mgc"))
                {
                    SendCommand(string.Format("ATT:RF {0} dB", _rfAttenuation));
                }
            }
        }

        private int _ifAttenuation;
        public int IFAttenuation
        {
            get { return _ifAttenuation; }
            set
            {
                _ifAttenuation = value;
                // if (_autoAttenuation.ToLower().Equals("mgc"))
                // {
                // 	SendCommand(string.Format("ATT:IF {0} dB", value));
                // }
            }
        }

        private double _startFrequency;
        public double StartFrequency
        {
            get { return _startFrequency; }
            set
            {
                _startFrequency = value;
                if (_taskState == TaskState.Start)
                {
                    SendCommand(string.Format("FREQ:START {0} MHz", value));
                }
            }
        }

        private double _stopFrequency;
        public double StopFrequency
        {
            get { return _stopFrequency; }
            set
            {
                _stopFrequency = value;
                if (_taskState == TaskState.Start)
                {
                    SendCommand(string.Format("FREQ:STOP {0} MHz", value));
                }
            }
        }

        private double _stepFrequency;
        public double StepFrequency
        {
            get { return _stepFrequency; }
            set
            {
                _stepFrequency = value;
                if (_taskState == TaskState.Start)
                {
                    SendCommand(string.Format("FREQ:STEP {0} kHz", value));
                }
            }
        }

        private ScanMode _scanMode;
        public ScanMode ScanMode
        {
            get { return _scanMode; }
            set
            {
                _scanMode = value;
                if (_taskState == TaskState.Start)
                {
                    if (value == ScanMode.PSCAN)
                    {
                        SendCommand("FREQ:MODE PSC");
                        SendCommand("FREQ:PSC:MODE NORM");
                    }
                    else if (value == ScanMode.FSCAN)
                    {
                        SendCommand("FREQ:MODE SWE");
                    }
                }
            }
        }

        private DemoduMode _demMode;
        public DemoduMode DemMode
        {
            get { return _demMode; }
            set
            {
                _demMode = value;
                SendCommand(string.Format("DEM {0}", value));
            }
        }

        private int _squelchThreshold;
        public int SquelchThreshold
        {
            get { return _squelchThreshold; }
            set
            {
                _squelchThreshold = value;
                SendCommand(string.Format("MEAS:THR {0}", value));
            }
        }

        #endregion

        #region 高级参数

        private float _holdTime;
        public float HoldTime
        {
            get { return _holdTime; }
            set
            {
                _holdTime = value;
                SendCommand(string.Format("MEAS:HOLD {0} s", value));
            }
        }

        private float _dwellTime;
        public float DwellTime
        {
            get { return _dwellTime; }
            set
            {
                _dwellTime = value;
                SendCommand(string.Format("MEAS:DWEL {0} s", value));
            }
        }

        private DetectMode _detector;
        public DetectMode Detector
        {
            get { return _detector; }
            set
            {
                _detector = value;
                var cmd = string.Empty;
                var modeON = string.Empty;
                switch (_detector)
                {
                    case DetectMode.FAST:
                        cmd = "FAST";
                        modeON = "ON";
                        break;
                    case DetectMode.PEAK:
                        cmd = "POS";
                        modeON = "OFF";
                        break;
                    case DetectMode.AVE:
                        cmd = "AVG";
                        modeON = "OFF";
                        break;
                    case DetectMode.RMS:
                        cmd = "RMS";
                        modeON = "OFF";
                        break;
                    default:
                        break;
                }
                if (!string.IsNullOrEmpty(cmd))
                {
                    SendCommand(string.Format("MEAS:DET {0}", cmd));
                    SendCommand(string.Format("MEAS:TIME:AUT {0}", modeON));
                }
            }
        }

        private float _measureTime;
        public float MeasureTime
        {
            get { return _measureTime; }
            set
            {
                _measureTime = value;
                SendCommand(string.Format("MEAS:TIME {0} ms", value / 1000.0f));
                SendCommand(string.Format("MEAS:HOLD {0} ms", value / 1000.0f));
            }
        }

        private float _xdB;
        public float XdB
        {
            get { return _xdB; }
            set
            {
                _xdB = value;
                SendCommand(string.Format("MEAS:BAND:XDB {0}", value));
            }
        }

        private float _beta;
        public float Beta
        {
            get { return _beta; }
            set
            {
                _beta = value;
                SendCommand(string.Format("MEAS:BAND:BETA {0}", value / 100));
            }
        }

        #endregion

        #region 数据开关

        private bool _levelSwitch;
        public bool LevelSwitch
        {
            get { return _levelSwitch; }
            set
            {
                _levelSwitch = value;
                if (value)
                {
                    AcceptDataRequest(DataType.LEVEL);
                }
                else
                {
                    RejectDataRequest(DataType.LEVEL);
                }
            }
        }

        private bool _spectrumSwitch;
        public bool SpectrumSwitch
        {
            get { lock (_parameterLock) { return _spectrumSwitch; } }
            set
            {
                lock (_parameterLock)
                {
                    _spectrumSwitch = value;
                    if (value)
                    {
                        AcceptDataRequest(DataType.SPECTRUM);
                    }
                    else
                    {
                        RejectDataRequest(DataType.SPECTRUM);
                    }
                }
            }
        }

        private bool _audioSwitch;
        public bool AudioSwitch
        {
            get
            {
                return _audioSwitch;
            }
            set
            {
                _audioSwitch = value;
                if (value)
                {
                    AcceptDataRequest(DataType.AUDIO);
                }
                else
                {
                    RejectDataRequest(DataType.AUDIO);
                }
            }
        }

        private bool _ituSwitch;
        public bool ITUSwitch
        {
            get
            {
                return _ituSwitch;
            }
            set
            {
                _ituSwitch = value;
                if (value)
                {
                    AcceptDataRequest(DataType.ITU);
                }
                else
                {
                    RejectDataRequest(DataType.ITU);
                }
            }
        }

        private bool _iqSwitch;
        public bool IQSwitch
        {
            get { return _iqSwitch; }
            set
            {
                _iqSwitch = value;
                if (value)
                {
                    AcceptDataRequest(DataType.IQ);
                }
                else
                {
                    RejectDataRequest(DataType.IQ);
                }
            }
        }

        #endregion

        #region 离散扫描/离散搜索

        private Dictionary<string, object>[] _frequencies;
        public Dictionary<string, object>[] Frequencys
        {
            get { return _frequencies; }
            set { _frequencies = value; }
        }

        private class MScanTemplate
        {
            private double _frequency;
            public double Frequency
            {
                get { return _frequency; }
                set { _frequency = value; }
            }

            private double _ifBandWidth;
            public double IFBandWidth
            {
                get { return _ifBandWidth; }
                set { _ifBandWidth = value; }
            }

            private DemoduMode _demMode;
            public DemoduMode DemMode
            {
                get { return _demMode; }
                set { _demMode = value; }
            }
        }

        #endregion

        #region DDC

        public int MaxChanCount
        {
            get;
            set;
        }

        private Dictionary<string, object>[] _ifMultiChannels;
        public Dictionary<string, object>[] IFMultiChannels
        {
            get { return _ifMultiChannels; }
            set
            {
                _ifMultiChannels = value;
                // 通道改变，需要重新设置参数
                SetDDC();
            }
        }

        /// <summary>
        /// 中频多路分析模板
        /// </summary>
        private class IFMCHTemplate
        {
            private double _frequency;
            public double Frequency
            {
                get { return _frequency; }
                set { _frequency = value; }
            }

            private double _ifBandWidth;
            public double IFBandWidth
            {
                get { return _ifBandWidth; }
                set { _ifBandWidth = value; }
            }

            private DemoduMode _demMode;
            public DemoduMode DemMode
            {
                get { return _demMode; }
                set { _demMode = value; }
            }

            private bool _ifSwitch;
            public bool IFSwitch
            {
                get { return _ifSwitch; }
                set { _ifSwitch = value; }
            }

            private bool _levelSwitch;
            public bool LevelSwitch
            {
                get { return _levelSwitch; }
                set { _levelSwitch = value; }
            }

            private bool _spectrumSwitch;
            public bool SpectrumSwitch
            {
                get { return _spectrumSwitch; }
                set { _spectrumSwitch = value; }
            }

            private bool _audioSwitch;
            public bool AudioSwitch
            {
                get { return _audioSwitch; }
                set { _audioSwitch = value; }
            }

            private bool _iqSwitch; // 不适用于DDC
            public bool IQSwitch
            {
                get { return _iqSwitch; }
                set { _iqSwitch = value; }
            }
        }

        #endregion

        #region 安装属性（网络地址）

        private string _ip;
        public string IP
        {
            get { return _ip; }
            set { _ip = value; }
        }

        private int _port;
        public int Port
        {
            get { return _port; }
            set { _port = value; }
        }

        private bool _enableGPS;
        public bool EnableGPS
        {
            get { return _enableGPS; }
            set { _enableGPS = value; }
        }

        private bool _enableCompass;
        public bool EnableCompass
        {
            get { return _enableCompass; }
            set { _enableCompass = value; }
        }

        private float _compassInstallingAngle;
        public float CompassInstallingAngle
        {
            get { return _compassInstallingAngle; }
            set { _compassInstallingAngle = value; }
        }

        #endregion
    }
}