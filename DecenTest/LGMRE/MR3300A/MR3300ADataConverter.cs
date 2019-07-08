/*********************************************************************************************
 *	
 * 文件名称:    ..\Tracker800\Server\Source\Device\Receiver\MR3300A\MR3300ADataConverter.cs
 *
 * 作    者:		陈鹏 
 *	
 * 创作日期:    2018/05/17
 * 
 * 修    改:    吴德鹏 2019/06/12 
 *              将数据转换的代码移到MR3300ADataConverter.cs中
 * 
 * 备    注:		MR3000A系列接收机接收到的数据转换为Tracker800的数据结构
 *                                            
*********************************************************************************************/

using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MR3300A
{
    public partial class MR3300A
    {
        #region 业务数据转换

        // 转换IQ数据
        private object ToIQ(object data)
        {
            var raw = data as RawIQ;
            if (raw == null || (_curAbility & (SpecificAbility.MSCAN | SpecificAbility.FSCNE)) == 0 && Math.Abs(raw.Frequency - (long)(_frequency * 1000000.0d)) > EPSILON)
            {
                return null;
            }
            raw.Bandwidth = (long)(_spectrumSpan * 1000d);

            var iq = new SDataIQ
            {
                Frequency = raw.Frequency / 1000000.0d,
                IFBandWidth = raw.Bandwidth / 1000.0d,
                Attenuation = raw.Attenuation,
                SampleRate = raw.SampleRate / 1000000.0d,
                Datas32 = raw.DataCollection
            };
            if (_reverseFrequencyOffsetDic.ContainsKey(raw.Frequency))
            {
                iq.Frequency = _reverseFrequencyOffsetDic[raw.Frequency] / 1000000.0d;
            }

            return iq;
        }

        // 将IQ转换为电平
        private object ToLevelByIQ(object data)
        {
            var raw = data as RawIQ;
            if (raw == null || (_curAbility & (SpecificAbility.MSCAN | SpecificAbility.FSCNE)) == 0 && Math.Abs(raw.Frequency / 1000000.0d - _frequency) > EPSILON)
            {
                return null;
            }
            raw.Bandwidth = (long)(_spectrumSpan * 1000d);

            var iq = Array.ConvertAll<int, float>(raw.DataCollection, item => (float)item);
            var level = Utilities.GetLevel(iq);
            level += (raw.Attenuation / 10.0f + _levelCalibration);

            var result = new SDataLevel
            {
                Frequency = raw.Frequency / 1000000.0d,
                IFBandWidth = raw.Bandwidth / 1000.0d,
                Data = level
            };

            if (_reverseFrequencyOffsetDic.ContainsKey(raw.Frequency))
            {
                result.Frequency = _reverseFrequencyOffsetDic[raw.Frequency] / 1000000.0d;
            }

            return result;
        }

        // 将IQ转换为频谱
        private object ToSpectrumByIQ(object data)
        {
            var raw = data as RawIQ;
            if (raw == null || (_curAbility & (SpecificAbility.MSCAN | SpecificAbility.FSCNE)) == 0 && Math.Abs(raw.Frequency - (long)(_frequency * 1000000.0d)) > EPSILON)
            {
                return null;
            }
            raw.Bandwidth = (long)(_spectrumSpan * 1000d);  // 临时加的，没有任何意义，LZ已经生无可恋了

            var iq = Array.ConvertAll<int, float>(raw.DataCollection, item => (float)item);

            var exp = Utilities.Log2n(iq.Length / 2);
            var length = 1 << exp;
            var windowValue = new float[length];
            var coe = Utilities.Window(ref windowValue, WindowType.Hanning);

            var spectrum = Utilities.GetWindowData(iq, windowValue, length);
            Utilities.FFT(ref spectrum);

            var efficientLength = (int)(length * 1.0 * raw.Bandwidth / raw.SampleRate + 0.5);
            var efficientIndex = length - efficientLength / 2;
            var ifAttenuation = (raw.Attenuation & 0xffff);
            var rfAttenuation = ((raw.Attenuation >> 16) & 0xffff);
            coe += (float)(-20 * Math.Log10(length) + (raw.Attenuation / 10.0f) + _levelCalibration);

            var spectrumEx = new float[length];
            for (var index = 0; index < length; ++index)
            {
                spectrumEx[index] = (float)(20 * Math.Log10(spectrum[index].Magnitude));
            }

            var validSpectrum = new float[efficientLength];
            for (var index = 0; index < validSpectrum.Length; ++index)
            {
                validSpectrum[index] = spectrumEx[(efficientIndex + index) % length] + coe;
            }

            var result = new SDataSpectrum
            {
                Frequency = raw.Frequency / 1000000.0d,
                SpectrumSpan = raw.Bandwidth / 1000.0d,
                Datas = validSpectrum
            };

            if (_reverseFrequencyOffsetDic.ContainsKey(raw.Frequency))
            {
                result.Frequency = _reverseFrequencyOffsetDic[raw.Frequency] / 1000000.0d;
            }

            return result;
        }

        // 转换为TDOA数据
        private object ToTDOA(object data)
        {
            var raw = data as RawIQ;
            if (raw == null || (_curAbility & (SpecificAbility.MSCAN | SpecificAbility.FSCNE)) == 0 && Math.Abs(raw.Frequency / 1000000.0d - _frequency) > EPSILON)
            {
                return null;
            }

            raw.Bandwidth = (long)(_spectrumSpan * 1000d);
            var tdoa = new SDataTDOA
            {
                TimeStamp = (long)raw.TimeStampSecond * 1000000000L + raw.TimeStampNano,
                Frequency = raw.Frequency / 1000000.0d,
                IFBandWidth = raw.Bandwidth / 1000.0d,
                Attenuation = raw.Attenuation,
                SampleRate = raw.SampleRate / 1000000.0d,
                Datas32 = raw.DataCollection
            };

            if (_reverseFrequencyOffsetDic.ContainsKey(raw.Frequency))
            {
                tdoa.Frequency = _reverseFrequencyOffsetDic[raw.Frequency] / 1000000.0d;
            }

            return tdoa;
        }

        // 转换音频数据
        private object ToAudio(object data)
        {
            var raw = data as RawAudio;
            if (raw == null)
            {
                return null;
            }

            if (_audioConverter == null || _audioSampleRate != raw.SampleRate)
            {
                if (_audioConverter != null)
                {
                    _audioConverter.Dispose();
                    _audioConverter = null;
                }

                _audioSampleRate = raw.SampleRate;

                var pcm = WaveFormat.PCM_MONO;
                // 无论音频数据采用何种采样率，无非是根据实际采样率创建符合要求的音频转换器，只要保证其它参数符合PCM格式即可
                pcm.nSamplesPerSec = (uint)(raw.SampleRate);
                pcm.nAvgBytesPerSec = pcm.nSamplesPerSec * pcm.nBlockAlign;

                _audioConverter = AudioConvert.Create(pcm, WaveFormat.PCM_MONO);
            }

            var audio = new SDataAudio
            {
                Frequency = raw.Frequency / 1000000.0d,
                Format = AudioFormat.PCM_MONO,
                Datas = _audioConverter.Convert(raw.DataCollection, false)
            };

            if (_reverseFrequencyOffsetDic.ContainsKey(raw.Frequency))
            {
                audio.Frequency = _reverseFrequencyOffsetDic[raw.Frequency] / 1000000.0d;
            }

            return audio;
        }

        // 转换电平数据
        private object ToLevel(object data)
        {
            var raw = data as RawLevel;
            if (raw == null || (_curAbility & (SpecificAbility.MSCNE | SpecificAbility.FSCNE)) == 0 && Math.Abs(raw.Frequency / 1000000.0d - _frequency) > EPSILON)
            {
                return null;
            }

            var level = new SDataLevel
            {
                Frequency = raw.Frequency / 1000000.0d,
                IFBandWidth = raw.Bandwidth / 1000.0d,
                Data = raw.Level
            };

            if (_reverseFrequencyOffsetDic.ContainsKey(raw.Frequency))
            {
                level.Frequency = _reverseFrequencyOffsetDic[raw.Frequency] / 1000000.0d;
            }

            return level;
        }

        // 转换ITU数据
        private object ToITU(object data)
        {
            var raw = data as RawITU;
            if (raw == null || raw.Modulation == -1 || (_curAbility & (SpecificAbility.MSCNE | SpecificAbility.FSCNE)) == 0 && Math.Abs(raw.Frequency / 1000000.0d - _frequency) > EPSILON)
            {
                return null;
            }

            var itu = new SDataITU
            {
                Frequency = raw.Frequency / 1000000.0d,
                BetaBW = raw.Beta >= _ifBandWidth * 1000 * 2 ? double.MinValue : raw.Beta / 1000.0d,
                XdBBW = raw.XdB >= _ifBandWidth * 1000 * 2 ? double.MinValue : raw.XdB / 1000.0d,
                AMDepth = raw.AM < 0 || raw.AM > 100 ? double.MinValue : raw.AM,
                FMDev = raw.FM > _ifBandWidth * 1000 * 2 ? double.MinValue : raw.FM / 1000.0d,
                FMDevPos = raw.FMPos > _ifBandWidth * 1000 * 2 ? double.MinValue : raw.FMPos / 1000.0d,
                FMDevNeg = raw.FMNeg > _ifBandWidth * 1000 * 2 ? double.MinValue : raw.FMNeg / 1000.0d,
                PMDepth = raw.PM <= -2 * Math.PI || raw.PM >= 2 * Math.PI ? double.MinValue : raw.PM
            };

            if (_reverseFrequencyOffsetDic.ContainsKey(raw.Frequency))
            {
                itu.Frequency = _reverseFrequencyOffsetDic[raw.Frequency] / 1000000.0d;
            }

            return itu;
        }

        // 转换频谱数据
        private object ToSpectrum(object data)
        {
            var raw = data as RawSpectrum;
            if (raw == null || (_curAbility & (SpecificAbility.FSCNE | SpecificAbility.MSCNE)) == 0 && Math.Abs(raw.Frequency / 1000000.0d - _frequency) > EPSILON)
            {
                return null;
            }

            var maxValuePos = -999;
            var maxValue = raw.DataCollection[0];

            var spectrum = new SDataSpectrum
            {
                Frequency = raw.Frequency / 1000000d,
                SpectrumSpan = raw.Span / 1000.0d,
                Datas = new float[raw.DataCollection.Length]
            };
            // 电平修正，中心频率 -> 偏移量
            for (var index = 0; index < spectrum.Datas.Length; ++index)
            {
                // 找最大值以及其索引
                var tmp = raw.DataCollection[index];
                if (tmp > maxValue)
                {
                    maxValuePos = index;
                    maxValue = tmp;
                }
                // 得到频谱数据
                spectrum.Datas[index] = raw.DataCollection[index] / 10.0f;
            }

            if (_reverseFrequencyOffsetDic.ContainsKey(raw.Frequency))
            {
                spectrum.Frequency = _reverseFrequencyOffsetDic[raw.Frequency] / 1000000.0d;
            }

            return spectrum;
        }

        // 转换扫描数据，包含PSCAN, FSCAN, MSCAN等
        private object ToScan(object data)
        {
            if (data is RawScan)    // 适用于PSCAN/FSCAN/MSCAN
            {
                return ToGeneralScan(data);
            }
            else if (data is RawFastScan)
            {
                return ToFastScan(data);
            }

            return null;
        }

        // 转换为普通扫描数据
        private object ToGeneralScan(object data)
        {
            var raw = data as RawScan;
            if (!IsScanValid(data))
            {
                return null;
            }

            //判断是否是最后一包，如果是再判断是否与框架计算点数有出入
            var delta = _scanDataLength - (int)raw.Total;
            if (delta != 0)
            {
                if (raw.Offset + raw.DataCollection.Length == raw.Total)
                {
                    Array.Resize(ref raw.DataCollection, raw.DataCollection.Length + delta);
                }
                raw.Total += delta;
            }

            var scan = new SDataScan
            {
                StartFrequency = raw.StartFrequency / 1000000.0d,
                StopFrequency = raw.StopFrequency / 1000000.0d,
                StepFrequency = raw.StepFrequency / 1000.0d,
                Total = (int)raw.Total,
                Pdloc = (int)raw.Offset,
                Datas = new float[raw.DataCollection.Length]
            };
            for (int i = 0; i < raw.DataCollection.Length; ++i)
            {
                scan.Datas[i] = raw.DataCollection[i] / 10f;
            }

            lock (_invalidCountLock)
            {
                if (_curAbility == SpecificAbility.SCAN && _invalidScanCount > 0)
                {
                    if (scan.Pdloc + scan.Datas.Length == scan.Total)
                    {
                        --_invalidScanCount;
                    }

                    return null;
                }
            }

            return scan;
        }

        // 转换为快速扫描数据
        private object ToFastScan(object data)
        {
            var raw = data as RawFastScan;
            if (raw.StartFrequency != (long)(_startFrequency * 1000000.0d)
                || raw.StopFrequency != (long)(_stopFrequency * 1000000.0d)
                || raw.StepFrequency != (long)(_stepFrequency * 1000.0d))
            {
                return null;
            }

            var scan = new SDataFastScan
            {
                StartFrequency = raw.StartFrequency / 1000000.0d,
                StopFrequency = raw.StopFrequency / 1000000.0d,
                StepFrequency = raw.StepFrequency / 1000.0d,
                Signals = new float[raw.Count],
                Noises = new float[raw.Count],
                Indices = new int[raw.Count]
            };
            for (var index = 0; index < raw.Count; ++index)
            {
                scan.Signals[index] = raw.SignalCollection[index] / 10.0f;
                scan.Noises[index] = raw.NoiseCollection[index] / 10.0f;
                scan.Indices[index] = raw.SignalIndexCollection[index];
            }

            return scan;
        }

        // 转换短信数据
        private object ToSMS(object data)
        {
            var raw = data as RawSMS;
            if (raw == null)
            {
                return null;
            }

            var sms = new SDataSMS
            {
                Frequency = raw.Frequency / 1000000.0d,
                CallingNumber = raw.CallingNumber.ToString(),
                CalledNumber = raw.CalledNumber.ToString(),
                Text = raw.Text
            };

            if (_reverseFrequencyOffsetDic.ContainsKey(raw.Frequency))
            {
                sms.Frequency = _reverseFrequencyOffsetDic[raw.Frequency] / 1000000.0d;
            }

            return sms;
        }

        #endregion

        // 处理GPS字符串
        private void ProcessGPS(object data)
        {
            var gps = data as RawGPS;
            if (gps == null)
            {
                return;
            }

            var dataCollection = gps.Text.Split(new char[] { '\r', '\n' }).ToList().Where(item => item.StartsWith("$"));
            // 异步解析GPS，避免阻塞
            var action = new Action<IEnumerable<string>>(
                args =>
                {
                    foreach (var item in args)
                    {
                        ParseGPS(item.Trim());
                    }
                });

            var asyncResult = action.BeginInvoke(dataCollection, null, null);
            asyncResult.AsyncWaitHandle.WaitOne(2000, false); // 短时间大量异步调用没有及时完成，可能造成的堆栈溢出；设置超时时间，降低出现此类情况的概率
        }

        // 处理罗盘数据
        private void ProcessCompass(object data)
        {
            RawCompass raw = data as RawCompass;
            if (raw == null)
            {
                return;
            }

            var compass = new SDataCompass
            {
                Angle = (((raw.Heading / 10.0f) + _compassInstallingAngle) % 360 + 360) % 360
            };

        }

        // 处理DDC监测数据
        private void ProcessDDC(object data)
        {
            var raw = data as RawDDC;
            if (raw == null)
            {
                return;
            }

            //
            // 校验数据长度
            var count = raw.DDCCollection.Count;
            var validChannelCount = _ifMultiChannels.Count(item => (bool)item["IFSwitch"]);
            if (count != validChannelCount)
            {
                return;
            }

            //
            // 填充子路
            var ddcChannels = new int[count];
            for (int ddcIndex = 0, index = 0; index < 32 && ddcIndex < count; ++index)
            {
                if (((raw.EnabledChannels >> index) & 0x1) == 0x1)
                {
                    ddcChannels[ddcIndex++] = index;
                }
            }

            //
            // 打包数据
            var result = new List<object>();
            for (var index = 0; index < count; ++index)
            {
                var ddc = new SDataMCHChannel
                {
                    ChannelNo = ddcChannels[index],
                    Frequency = (double)_ifMultiChannels[index]["Frequency"],
                    IFBandWidth = (double)_ifMultiChannels[index]["IFBandWidth"],
                    Datas = new List<object>()
                };

                if ((bool)_ifMultiChannels[index]["LevelSwitch"])
                {
                    var level = new SDataLevel
                    {
                        Frequency = ddc.Frequency,
                        IFBandWidth = ddc.IFBandWidth,
                        Data = raw.DDCCollection[index].Level / 10.0f
                    };
                    ddc.Datas.Add(level);
                }

                if ((bool)_ifMultiChannels[index]["SpectrumSwitch"])
                {
                    var length = raw.DDCCollection[index].Spectrum.Length;
                    var spectrum = new SDataSpectrum
                    {
                        Frequency = ddc.Frequency,
                        SpectrumSpan = ddc.IFBandWidth,
                    };
                    spectrum.Datas = new float[length];
                    for (var subIndex = 0; subIndex < length; ++subIndex)
                    {
                        spectrum.Datas[subIndex] = raw.DDCCollection[index].Spectrum[subIndex] / 10.0f;
                    }
                    ddc.Datas.Add(spectrum);
                }

                if (ddc.Datas.Count > 0)
                {
                    result.Add(ddc);
                }
            }
            if (result.Count > 0)
            {
                OnDataArrived(result);
            }
        }

        // 处理DDC音频数据
        private void ProcessDDCAudio(object data)
        {
            var raw = data as RawDDCAudio;
            if (raw == null)
            {
                return;
            }

            var count = raw.DataCollection.Length / raw.Count / 2;
            var validChannelCount = _ifMultiChannels.Count(item => (bool)item["IFSwitch"]);
            if (count != validChannelCount)
            {
                return;
            }

            var ddcChannels = new int[count];
            for (int ddcIndex = 0, index = 0; index < 32 && ddcIndex < count; ++index)
            {
                if (((raw.EnabledChannels >> index) & 0x1) == 0x1)
                {
                    ddcChannels[ddcIndex++] = index;
                }
            }

            if (_audioConverter == null || _audioSampleRate != raw.SampleRate)
            {
                if (_audioConverter != null)
                {
                    _audioConverter.Dispose();
                    _audioConverter = null;
                }
                _audioSampleRate = raw.SampleRate;
                var pcm = WaveFormat.PCM_MONO;
                pcm.nSamplesPerSec = (uint)(raw.SampleRate);
                pcm.nAvgBytesPerSec = pcm.nSamplesPerSec * pcm.nBlockAlign;

                _audioConverter = AudioConvert.Create(pcm, WaveFormat.PCM_MONO);
            }

            var result = new List<object>();
            for (var index = 0; index < count; ++index)
            {
                var ddc = new SDataMCHChannel
                {
                    ChannelNo = ddcChannels[index],
                    Frequency = (double)_ifMultiChannels[index]["Frequency"],
                    IFBandWidth = (double)_ifMultiChannels[index]["IFBandWidth"],
                    Datas = new List<object>()
                };

                if ((bool)_ifMultiChannels[index]["AudioSwitch"])
                {
                    var buffer = new byte[raw.Count * 2];
                    var audio = new SDataAudio
                    {
                        Frequency = ddc.Frequency,
                        Format = AudioFormat.PCM_MONO
                    };
                    Buffer.BlockCopy(raw.DataCollection, index * buffer.Length, buffer, 0, buffer.Length);
                    audio.Datas = _audioConverter.Convert(buffer, false);
                    ddc.Datas.Add(audio);
                }

                if (ddc.Datas.Count > 0)
                {
                    result.Add(ddc);
                }
            }
            if (result.Count > 0)
            {
                OnDataArrived(result);
            }
        }

        #region GPS Parsing Helper

        // 解析收到的数据
        private void ParseGPS(string data)
        {
            var previousTimeStamp = _preGPSTimeStamp;
            var bufferedGPS = new SDataGPS
            {
                Latitude = _bufferedGPS.Latitude,
                Longitude = _bufferedGPS.Longitude
            };

            var rmc = ParseRMC(data) as SDataGPS; // RMC
            var gga = ParseGGA(data) as SDataGPS; // GGA
            var gll = ParseGLL(data) as SDataGPS; // GLL
            var dataCollection = from x in new object[] { rmc, gga, gll } where x is SDataGPS select x;
            var gps = dataCollection.Cast<SDataGPS>().ToArray();
            if (gps != null && gps.Count() > 0)
            {
                var distance = GetDistanceByPosition(bufferedGPS.Latitude, bufferedGPS.Longitude, gps[0].Latitude, gps[0].Longitude);
                var currentTimeStamp = DateTime.Now;
                var timespan = currentTimeStamp - previousTimeStamp;
                if (distance > 2 || timespan.TotalMilliseconds > 10000)
                {
                    //SendMessage(MessageDomain.Network, MessageType.MonNodeGPSChange, gps[0]);
                    previousTimeStamp = currentTimeStamp;
                    bufferedGPS = gps[0];
                }

                lock (_gpsLock)
                {
                    _preGPSTimeStamp = previousTimeStamp;
                    _bufferedGPS.Latitude = bufferedGPS.Latitude;
                    _bufferedGPS.Longitude = bufferedGPS.Longitude;
                }
            }
        }

        // 解析GGA
        private object ParseGGA(string data)
        {
            // 筛选GPGGA/BDGGA/GNGGA，分别表示GPS/北斗/GPS+北斗
            Regex reg = new Regex(@"^(\$\w{2}GGA)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            if (!reg.IsMatch(data))
            {
                return null;
            }

            var dataArray = data.Split(new char[] { ',' });
            if (dataArray == null || dataArray.Length < 8)
            {
                return null;
            }
            // “1”为单点定位，“2”为伪距差分定位，其它为无效定位或未定位
            if (!dataArray[6].Equals("1") && !dataArray[6].Equals("2"))
            {
                return null;
            }

            double? lat = ToDegree(dataArray[2]);
            double? lon = ToDegree(dataArray[4]);
            if (lat == null || lon == null)
            {
                return null;
            }

            if (dataArray[3].ToLower().Equals("s"))
            {
                lat *= -1;
            }
            if (dataArray[5].ToLower().Equals("w"))
            {
                lon *= -1;
            }

            var gps = new SDataGPS();
            gps.Latitude = lat.Value;
            gps.Longitude = lon.Value;
            gps.Declination = 0;

            return gps;
        }

        // 解析GGL
        private object ParseGLL(string data)
        {
            // 筛选GPGLL/BDGLL/GNGLL，分别表示GPS/北斗/GPS+北斗
            Regex reg = new Regex(@"^(\$\w{2}GLL)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            if (!reg.IsMatch(data))
            {
                return null;
            }
            string[] dataArray = data.Split(new char[] { ',' });
            if (dataArray == null || dataArray.Length < 7)
            {
                return null;
            }
            // 未定位
            if (!dataArray[6].ToLower().Equals("a"))
            {
                return null;
            }

            double? lat = ToDegree(dataArray[1]);
            double? lon = ToDegree(dataArray[3]);
            if (lat == null || lon == null)
            {
                return null;
            }

            if (dataArray[2].ToLower().Equals("s"))
            {
                lat *= -1;
            }
            if (dataArray[4].ToLower().Equals("w"))
            {
                lon *= -1;
            }

            var gps = new SDataGPS();
            gps.Latitude = lat.Value;
            gps.Longitude = lon.Value;
            gps.Declination = 0;

            return gps;
        }

        // 解析GSA
        private object ParseGSA(string data)
        {
            //TODO: 按实际需求编写
            return null;
        }

        // 解析GSV
        private object ParseGSV(string data)
        {
            //TODO: 按实际需求编写
            return null;
        }

        // 解析RMC
        private object ParseRMC(string data)
        {
            // 筛选GPRMC/BDRMC/GNRMC，分别表示GPS/北斗/GPS+北斗
            Regex reg = new Regex(@"^(\$\w{2}RMC)", RegexOptions.IgnoreCase);
            if (!reg.IsMatch(data))
            {
                return null;
            }
            var dataArray = data.Split(new char[] { ',' });
            if (dataArray == null || dataArray.Length < 12)
            {
                return null;
            }
            // 未定位
            if (!dataArray[2].ToLower().Equals("a"))
            {
                return null;
            }

            double? lat = ToDegree(dataArray[3]);
            double? lon = ToDegree(dataArray[5]);
            float? dec = ToDeclination(dataArray[10]);
            if (lat == null || lon == null)
            {
                return null;
            }
            if (dec == null)
            {
                dec = 0;
            }

            if (dataArray[4].ToLower().Equals("s"))
            {
                lat *= -1;
            }
            if (dataArray[6].ToLower().Equals("w"))
            {
                lon *= -1;
            }
            if (dataArray[11].ToLower().Equals("w"))
            {
                dec *= -1;
            }

            var gps = new SDataGPS();
            gps.Latitude = lat.Value;
            gps.Longitude = lon.Value;
            gps.Declination = dec.Value;

            return gps;
        }

        // 解析VTG
        private object ParseVTG(string data)
        {
            //TODO: 按实际需求编写
            return null;
        }

        // 解析ZDA
        private object ParseZDA(string data)
        {
            //TODO: 按实际需求编写
            return null;
        }

        // 解析GST
        private object ParseGST(string data)
        {
            //TODO: 按实际需求编写
            return null;
        }

        // 将字符串转换为浮点数度，原始格式为：ddmm.mmmmmm
        private double? ToDegree(string value)
        {
            try
            {
                decimal raw = decimal.Parse(value);
                decimal deg, min;
                double result;

                raw = raw / 100;
                deg = (int)raw;
                min = (raw - deg) * 100;
                result = (double)(deg + min / 60);

                return result;
            }
            catch
            {
                return null;
            }
        }

        // 转换为磁偏角
        private float? ToDeclination(string value)
        {
            try
            {
                float raw = float.Parse(value);
                raw = (short)(raw * 10);

                return raw;
            }
            catch
            {
                return null;
            }
        }

        //获取两个点的距离，单位米
        private double GetDistanceByPosition(double lantitude1, double longitude1, double lantitude2, double longitude2)
        {
            var dLat1InRad = lantitude1 * (Math.PI / 180);
            var dLong1InRad = longitude1 * (Math.PI / 180);
            var dLat2InRad = lantitude2 * (Math.PI / 180);
            var dLong2InRad = longitude2 * (Math.PI / 180);
            var dLongitude = dLong2InRad - dLong1InRad;
            var dLatitude = dLat2InRad - dLat1InRad;
            var a = Math.Pow(Math.Sin(dLatitude / 2), 2) + Math.Cos(dLat1InRad) * Math.Cos(dLat2InRad) * Math.Pow(Math.Sin(dLongitude / 2), 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var dDistance = 6378.137 * c * 1000;

            return dDistance;
        }

        #endregion

    }
}
