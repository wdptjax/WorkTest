using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tracker800.Server.Contract;

namespace ODM20181102TJ01.DF9000B
{
    /// <summary>
    /// 数据转换 Tracker800=>0xE
    /// </summary>
    class OutDataConvert : IOutDataConvert
    {
        // 缓存扫描数据
        private float[] _scanDatas = null;
        // 缓存电平数据
        private float _level = 0f;
        //// 缓存罗盘数据
        //private SDataCompass _sDataCompass = null;
        //// 缓存GPS数据
        //private SDataGPS _sDataGPS = null;

        //当前任务打开的功能
        private EDataMode _dataMode = EDataMode.MODE_FFM;
        /// <summary>
        /// 当前任务打开的功能
        /// </summary>
        public EDataMode DataMode
        {
            get { return _dataMode; }
            set { _dataMode = value; }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="dataMode">任务类型</param>
        /// <param name="parameter">任务参数</param>
        public OutDataConvert(EDataMode dataMode, IParameter parameter)
        {
            _dataMode = dataMode;
            if (parameter is ScanParams)
            {
                ScanParams scanParams = (ScanParams)parameter;
                uint start = scanParams.StartFreq;
                uint stop = scanParams.StopFreq;
                uint step = scanParams.StepWidth;
                int count = (int)((stop - start) / step) + 1;
                _scanDatas = new float[count];
            }
        }

        /// <summary>
        /// 包装数据
        /// </summary>
        /// <param name="data"></param>
        /// <returns>可能会返回空集合，需要判断；不会返回null</returns>
        public List<byte[]> PackupData(List<object> data)
        {
            List<byte[]> list = new List<byte[]>();
            switch (_dataMode)
            {
                case EDataMode.MODE_FFM:
                    {
                        FfmData? ffmData = ParseFfmData(data);
                        if (ffmData != null)
                        {
                            EbdMsg ebdMsg = new EbdMsg();
                            ebdMsg.MessageType = EDataOut.SCAN_DATA;
                            ebdMsg.Datas = ((FfmData)ffmData).ToBytes();
                            byte[] sendData = ebdMsg.ToBytes();
                            list.Add(sendData);
                        }
                    }
                    {
                        IfSpectrumData? ifData = ParseIfSpectrumData(data);
                        if (ifData != null)
                        {
                            EbdMsg ebdMsg = new EbdMsg();
                            ebdMsg.MessageType = EDataOut.SCAN_DATA;
                            ebdMsg.Datas = ((IfSpectrumData)ifData).ToBytes();
                            byte[] sendData = ebdMsg.ToBytes();
                            list.Add(sendData);
                        }
                    }
                    break;
                case EDataMode.MODE_SCAN:
                    {
                        ScanData? scanData = ParseScanDfData(data);
                        if (scanData != null)
                        {
                            EbdMsg ebdMsg = new EbdMsg();
                            ebdMsg.MessageType = EDataOut.SCAN_DATA;
                            ebdMsg.Datas = ((ScanData)scanData).ToBytes();
                            byte[] sendData = ebdMsg.ToBytes();
                            list.Add(sendData);
                        }
                    }
                    break;
                case EDataMode.MODE_STANDBY:
                default:
                    break;
            }

            return list;
        }

        /// <summary>
        /// 包装GPS数据
        /// </summary>
        /// <param name="data"></param>
        /// <returns>可能为null，需要判断</returns>
        public byte[] PackupGpsData(List<object> data)
        {
            GpsData? gps = ParseGpsData(data);
            if (gps == null)
            {
                return null;
            }
            EbdMsg ebdMsg = new EbdMsg();
            ebdMsg.MessageType = EDataOut.GPS_DATA;
            ebdMsg.Datas = ((GpsData)gps).ToBytes();
            byte[] sendData = ebdMsg.ToBytes();
            return sendData;
        }

        /// <summary>
        /// 转换扫描测向数据
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private ScanData? ParseScanDfData(List<object> data)
        {
            ScanData scanDf = new ScanData();
            scanDf.BigTime = Utils.ParseTime2TwoWords(DateTime.Now);
            scanDf.JobId = 0;

            SDataScan scanData = (SDataScan)data.Find(i => i is SDataScan);
            SDataScanDF dfData = (SDataScanDF)data.Find(i => i is SDataScanDF);
            SDataCompass compassData = (SDataCompass)data.Find(i => i is SDataCompass);
            SDataGPS gpsData = (SDataGPS)data.Find(i => i is SDataGPS);

            if (scanData != null && _scanDatas != null && _scanDatas.Length > 0)
            {
                int start = scanData.Pdloc;
                int len = scanData.Datas.Length;
                //如果发来的数据多了，以本地的为准
                if (scanData.Datas.Length + start > _scanDatas.Length)
                {
                    len = _scanDatas.Length - start;
                }
                if (len > 0)
                {
                    Array.Copy(scanData.Datas, 0, _scanDatas, start, len);
                }
            }
            //if (compassData != null)
            //{
            //    _sDataCompass = compassData;
            //}
            //if (gpsData != null)
            //{
            //    _sDataGPS = gpsData;
            //}
            float elevation = 0;
            if (dfData == null)
            {
                return null;
            }
            if (compassData != null)
            {
                scanDf.AzimCompass = (uint)(compassData.Angle * 100);
            }
            if (gpsData != null)
            {
                elevation = gpsData.Declination;
            }
            if (dfData.Index != null && dfData.Index.Length > 0)
            {
                //TODO: 0xE中第一个通道号是1，而Tracker800是0，因此需要+1（这里需要测试，不确定是不是1）
                scanDf.FirstChannel = dfData.Index[0] + 1;

                int total = Utils.GetTotalCount(dfData.StartFrequency, dfData.StopFrequency, dfData.StepFrequency);
                scanDf.Count = total;
                scanDf.ScanStatus = new ScanStatus() { Valid = 1 };
                int count = dfData.Index.Length;
                scanDf.MeasChan = new ScanChannel[count];
                int start = dfData.Index[0];////////////TODO: 这里可能有问题
                for (int i = 0; i < count; i++)
                {
                    ScanChannel channel = new ScanChannel()
                    {
                        Azimuth = (ushort)(dfData.Azimuth[i] * 100),
                        Quality = (short)dfData.Quality[i],
                    };
                    if (_scanDatas != null && _scanDatas.Length >= count + start)
                    {
                        channel.Level = (short)(_scanDatas[i + start] * 100);
                    }
                    channel.Elevation = (short)(elevation * 100);
                    scanDf.MeasChan[i] = channel;
                }
            }
            return scanDf;
        }

        /// <summary>
        /// 转换定位数据
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private GpsData? ParseGpsData(List<object> data)
        {
            SDataGPS gpsData = (SDataGPS)data.Find(i => i is SDataGPS);
            if (gpsData == null)
            {
                return null;
            }
            GpsData gps = new GpsData();
            gps.BigTime = Utils.ParseTime2TwoWords(gpsData.Time);
            gps.HorDilution = 5000;
            gps.NoOfSatInView = gpsData.StarsNumber;
            gps.SetGpsTime(gpsData.Time);
            gps.LonDeg = (byte)gpsData.Longitude;
            gps.LonMin = (float)(gpsData.Longitude - (int)gpsData.Longitude) * 60;
            gps.LatDeg = (byte)gpsData.Latitude;
            gps.LatMin = (float)(gpsData.Latitude - (int)gpsData.Latitude) * 60;
            gps.GpsStatus = new GpsStatus() { Valid = 1 };
            return gps;
        }

        /// <summary>
        /// 转换测向数据
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private FfmData? ParseFfmData(List<object> data)
        {
            SDataDFind dfData = (SDataDFind)data.Find(i => i is SDataDFind);
            SDataGPS gpsData = (SDataGPS)data.Find(i => i is SDataGPS);
            SDataCompass compassData = (SDataCompass)data.Find(i => i is SDataCompass);
            SDataLevel levelData = (SDataLevel)data.Find(i => i is SDataLevel);

            if (levelData != null)
            {
                _level = levelData.Data;
            }
            if (dfData == null)
            {
                return null;
            }


            FfmData ffmData = new FfmData();
            ffmData.BigTime = Utils.ParseTime2TwoWords(DateTime.Now);
            ffmData.FfmStatus = new FfmStatus()
            {
                // TODO: 天线极化方式/////////////////////////////////////
                AntPol = (uint)EAntPol.ANTPOL_VERT,
                // TODO: 测向模式/////////////////////////////////////
                DfAlt = (uint)EDfAlt.DFALT_CORRELATION,
                Elevation = gpsData == null ? 0U : 1U,
                Overflow = 0,
                Valid = 1
            };
            ffmData.JobId = 0;
            ffmData.Channel = 0;
            ffmData.AzimCompass = compassData != null ? (uint)(compassData.Angle * 100) : 0;
            ffmData.Level = (short)(_level * 100);
            ffmData.FieldStrength = ffmData.Level;/////////////////////////////
            ffmData.Azimuth = (ushort)(dfData.Azimuth * 100);
            ffmData.Quality = (short)dfData.Quality;
            return ffmData;
        }

        private IfSpectrumData? ParseIfSpectrumData(List<object> data)
        {
            SDataSpectrum specData = (SDataSpectrum)data.Find(i => i is SDataSpectrum);
            if (specData == null || specData.Datas == null || specData.Datas.Length == 0)
            {
                return null;
            }
            IfSpectrumData ifSpecData = new IfSpectrumData();
            ifSpecData.BigTime = Utils.ParseTime2TwoWords(DateTime.Now);
            ifSpecData.IfStatus = new IfStatus()
            {
                Overflow = 0,
                Valid = 1,
                RealTimeBandwidth = (uint)(specData.SpectrumSpan * 1000),
            };
            ifSpecData.JobId = 0;
            ifSpecData.Count = 0;
            int len = (specData.Datas.Length % 2 == 0) ? specData.Datas.Length / 2 : (specData.Datas.Length + 1) / 2;
            ifSpecData.IfDatas = new IfData[len];
            for (int i = 0; i < ifSpecData.IfDatas.Length; i++)
            {
                IfData ifData = new IfData();
                if (i * 2 < specData.Datas.Length)
                {
                    ifData.D0 = (short)(specData.Datas[i * 2] * 100);
                }
                else
                {
                    //这个情况不会出现
                    ifData.D0 = 0;
                }
                if ((i * 2 + 1) < specData.Datas.Length)
                {
                    ifData.D1 = (short)(specData.Datas[i * 2 + 1] * 100);
                }
                else
                {
                    // 这个情况会出现，因为specData.Datas有可能有奇数个
                    ifData.D1 = 0;
                }
                ifSpecData.IfDatas[i] = ifData;
            }
            return ifSpecData;
        }
    }
}
