
/*********************************************************************************************
 *	
 * 文件名称:    DataSim.cs
 *
 * 作    者:    吴德鹏
 *	
 * 创作日期:    2018-11-29 15:30:36
 * 
 * 备    注:    描述类的用途
 *              
 *                                
 *               
*********************************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace DeviceSim.Device
{
    public partial class DDF550Device
    {
        int _noiseMax = -30;
        int _noiseMin = -50;
        int _levelMax = 50;
        int _levelMin = 40;
        int _ituOffsetMax = 250;
        int _ituOffsetMin = -250;
        ulong _sampleCount = 1000;
        int _azimuthMax = 70;
        int _azimuthMin = 60;
        ushort _jobId = 3649;
        float _angle = 0;

        DateTime _lastDfTime = DateTime.Now;

        private bool _canSendAudio = true;

        //ScanDF使用
        //每包数据的最大长度
        const uint SCANDF_MAXCNT = 199;
        //当前包第一个频点在本频段的序号
        uint _hopIndexScanDF = 0;
        //本频段的频点数
        uint _scanRangeCnt = 0;
        uint _lastSendCnt = 0;

        Random _random = new Random();
        private FileStream _fsAudio = new FileStream(@"..\bin\Device\audiodata.bin", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        private long _indexAudio = 0;

        private void DataSendSync()
        {
            while (Connected)
            {
                Thread.Sleep(30);
                if (!IsRunning)
                    continue;
                try
                {
                    if (Client.IsSendSpectrum)
                    {
                        byte[] buffer = GetSpectrumData();
                        WriteSendData(buffer);
                    }
                    if (Client.IsSendCW)
                    {
                        byte[] buffer = GetITUData();
                        WriteSendData(buffer);
                    }
                    if (Client.IsSendIQ)
                    {
                        byte[] buffer = GetIQData();
                        WriteSendData(buffer);
                    }
                    if (Client.IsSendAudio)
                    {
                        byte[] buffer = GetAudioData();
                        WriteSendData(buffer);
                    }
                    if (Client.IsSendDF)
                    {
                        byte[] buffer = GetDFData();
                        WriteSendData(buffer);
                    }
                    if (Client.IsSendPScan)
                    {
                        byte[] buffer = GetPScanData();
                        WriteSendData(buffer);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("DataSendSync报错:" + ex.ToString());
                }

            }
        }

        private byte[] GetAudioData()
        {
            if (!_canSendAudio)
                return null;
            int dataCnt = 960;
            byte[] data = new byte[dataCnt * 2];
            if (_indexAudio + dataCnt * 2 <= _fsAudio.Length)
            {
                int n = _fsAudio.Read(data, 0, dataCnt * 2);
                if (n == 0)
                {
                    // 音频文件读到结尾了，重头再开始
                    _fsAudio.Seek(0, SeekOrigin.Begin);
                    _fsAudio.Read(data, 0, dataCnt * 2);
                }
            }
            _indexAudio = (_indexAudio + dataCnt * 2 >= _fsAudio.Length) ? 0 : (_indexAudio + dataCnt * 2);
            ulong freq = (ulong)(_frequency * 1000000);

            OptionalHeaderAudio audioHeader = new OptionalHeaderAudio();
            audioHeader.AudioMode = (short)AudioMode;
            audioHeader.FrequencyLow = (uint)(freq & 0x00000000FFFFFFFF);
            audioHeader.Bandwidth = (uint)(_demBandWidth * 1000);
            audioHeader.Demodulation = (ushort)_demMode;
            audioHeader.sDemodulation = _demMode.ToString().Replace("MOD_", "");
            audioHeader.FrequencyHigh = (uint)((freq & 0xFFFFFFFF00000000) >> 32);
            audioHeader.OutputTimestamp = (ulong)DateTime.Now.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).Ticks * 100;
            audioHeader.SignalSource = 0;

            TraceAttributeConventional traceAttribute = new TraceAttributeConventional();
            traceAttribute.ChannelNumber = 0;
            traceAttribute.NumberOfTraceItems = (short)dataCnt;
            traceAttribute.OptionalHeaderLength = (byte)Marshal.SizeOf(audioHeader);
            traceAttribute.SelectorFlags = 0x80000000;

            GenericAttributeConventional genericAttribute = new GenericAttributeConventional();
            genericAttribute.Length = (ushort)(Marshal.SizeOf(traceAttribute) + Marshal.SizeOf(audioHeader) + data.Length);
            genericAttribute.Tag = (ushort)TAGS.AUDIO;

            byte[] buffer = GetData(genericAttribute, traceAttribute, audioHeader, data);
            return buffer;
        }

        private byte[] GetIQData()
        {
            int dataCnt = 1024;
            byte[] data = new byte[dataCnt * 4];
            int offset = 0;
            for (int i = 0; i < dataCnt; i++)
            {
                double level = ((double)_random.Next(_levelMin * 10, _levelMax * 10)) / 10;
                //double radian = (2 * Math.PI) * _random.NextDouble();
                double radian = Math.PI / 180 * _angle;
                _angle++;
                if (_angle >= 360)
                    _angle = 0;
                double iValue = Math.Sqrt(Math.Pow(10, level / 10)) * Math.Cos(radian);
                double qValue = Math.Sqrt(Math.Pow(10, level / 10)) * Math.Sin(radian);
                short iData = (short)iValue;
                short qData = (short)qValue;
                byte[] arr = BitConverter.GetBytes(iData);
                Array.Reverse(arr);
                Buffer.BlockCopy(arr, 0, data, offset, arr.Length);
                offset += 2;
                arr = BitConverter.GetBytes(qData);
                Array.Reverse(arr);
                Buffer.BlockCopy(arr, 0, data, offset, arr.Length);
                offset += 2;
            }

            ulong freq = (ulong)(_frequency * 1000000);

            OptionalHeaderIF ifHeader = new OptionalHeaderIF();
            ifHeader.Mode = 1;
            ifHeader.FrameLen = 4;
            ifHeader.Samplerate = 320000;
            ifHeader.FrequencyLow = (uint)(freq & 0x00000000FFFFFFFF);
            ifHeader.Bandwidth = (uint)(_demBandWidth * 1000);
            ifHeader.Demodulation = (ushort)_demMode;
            ifHeader.RxAtt = (short)Attenuation;
            ifHeader.Flags = 0x8001;
            ifHeader.KFactor = 383;
            ifHeader.sDemodulation = "FM";
            ifHeader.SampleCount = _sampleCount;
            _sampleCount++;
            ifHeader.FrequencyHigh = (uint)((freq & 0xFFFFFFFF00000000) >> 32);
            ifHeader.StartTimestamp = (ulong)DateTime.Now.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).Ticks * 100;
            ifHeader.SignalSource = 0;

            TraceAttributeConventional traceAttribute = new TraceAttributeConventional();
            traceAttribute.ChannelNumber = 0;
            traceAttribute.NumberOfTraceItems = (short)dataCnt;
            traceAttribute.OptionalHeaderLength = (byte)Marshal.SizeOf(ifHeader);
            traceAttribute.SelectorFlags = 0x80000000;

            GenericAttributeConventional genericAttribute = new GenericAttributeConventional();
            genericAttribute.Length = (ushort)(Marshal.SizeOf(traceAttribute) + Marshal.SizeOf(ifHeader) + data.Length);
            genericAttribute.Tag = (ushort)TAGS.IF;

            byte[] buffer = GetData(genericAttribute, traceAttribute, ifHeader, data);
            return buffer;
        }

        private byte[] GetITUData()
        {
            int dataCnt = 1;
            uint selectorFlags = 0x80000003;
            if (Client.IsSendITU)
                selectorFlags = 0x800007FF;
            ulong freq = (ulong)(_frequency * 1000000);
            CWData cwData = new CWData(dataCnt);
            short level = (short)_random.Next(_levelMin * 10, _levelMax * 10);
            if (!_isUseSquelch)
                _canSendAudio = true;
            else
            {
                if (level < _squelchThreshold)
                    _canSendAudio = false;
            }
            cwData.Level[0] = level;
            cwData.FreqOffset[0] = (int)_random.Next(_ituOffsetMin * 10, _ituOffsetMax * 10);
            if (Client.IsSendITU)
            {
                cwData.FStrength[0] = 794;
                cwData.AMDepth[0] = 796;
                cwData.AMDepthPos[0] = 290;
                cwData.AMDepthNeg[0] = 853;
                cwData.FMDev[0] = 79741;
                cwData.FMDevPos[0] = 82007;
                cwData.FMDevNeg[0] = 77475;
                cwData.PMDepth[0] = 32766;
                cwData.BandWidth[0] = 566875;
            }
            byte[] data = cwData.ToBytes(selectorFlags);

            OptionalHeaderCW cwHeader = new OptionalHeaderCW();
            cwHeader.Freq_High = (uint)((freq & 0xFFFFFFFF00000000) >> 32);
            cwHeader.Freq_Low = (uint)(freq & 0x00000000FFFFFFFF);
            cwHeader.SignalSource = 0;
            cwHeader.OutputTimestamp = (ulong)DateTime.Now.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).Ticks * 100;

            TraceAttributeConventional traceAttribute = new TraceAttributeConventional();
            traceAttribute.ChannelNumber = 0;
            traceAttribute.NumberOfTraceItems = (short)dataCnt;
            traceAttribute.OptionalHeaderLength = (byte)Marshal.SizeOf(cwHeader);
            traceAttribute.SelectorFlags = selectorFlags;

            GenericAttributeConventional genericAttribute = new GenericAttributeConventional();
            genericAttribute.Length = (ushort)(Marshal.SizeOf(traceAttribute) + Marshal.SizeOf(cwHeader) + data.Length);
            genericAttribute.Tag = (ushort)TAGS.CW;


            byte[] buffer = GetData(genericAttribute, traceAttribute, cwHeader, data);
            return buffer;
        }

        private byte[] GetSpectrumData()
        {
            uint span = (uint)(_spectrumSpan * 1000);
            ulong freq = (ulong)(_frequency * 1000000);
            string strStep = (_ifPanStep * 1000).ToString();
            int index = strStep.IndexOf(".");
            int denominator = 0;
            if (index == -1)
                denominator = 1;
            else
                denominator = 10 ^ (strStep.Length - index - 1);
            int numerator = int.Parse(strStep.Replace(".", ""));
            int step = numerator / denominator;
            short dataLen = (short)(span / step + 1);
            OptionalHeaderIFPan ifPanHeader = new OptionalHeaderIFPan();
            ifPanHeader.AverageTime = 0;
            ifPanHeader.AverageType = 3;
            ifPanHeader.MeasureMode = 1;
            ifPanHeader.MeasureTime = 0;
            ifPanHeader.SignalSource = 0;
            ifPanHeader.SpanFrequency = span;
            ifPanHeader.StepFrequencyDenominator = (uint)denominator;
            ifPanHeader.StepFrequencyNumerator = (uint)numerator;
            ifPanHeader.MeasureTimestamp = (ulong)DateTime.Now.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).Ticks * 100;
            ifPanHeader.OutputTimestamp = (ulong)DateTime.Now.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).Ticks * 100;
            ifPanHeader.DemodFreqChannel = dataLen / 2;
            ifPanHeader.DemodFreqHigh = (uint)((freq & 0xFFFFFFFF00000000) >> 32);
            ifPanHeader.DemodFreqLow = (uint)(freq & 0x00000000FFFFFFFF);
            ifPanHeader.FrequencyHigh = ifPanHeader.DemodFreqHigh;
            ifPanHeader.FrequencyLow = ifPanHeader.DemodFreqLow;
            byte[] level = new byte[dataLen * 2];
            for (int i = 0; i < dataLen; i++)
            {
                int min = _random.Next(_noiseMin * 10 - 20, _noiseMin * 10 + 20);
                int max = _random.Next(_noiseMax * 10 - 20, _noiseMax * 10 + 20);
                short data = (short)_random.Next(min, max);
                if (_rfMode == ERf_Mode.RFMODE_LOW_NOISE)
                    data -= 50;
                if (i == ifPanHeader.DemodFreqChannel)
                {
                    data = (short)_random.Next(_levelMin * 10, _levelMax * 10);
                    if (_rfMode == ERf_Mode.RFMODE_LOW_DISTORTION)
                        data -= 50;
                }
                if (!_gainAuto)
                    data += (short)(_gain * 10);
                if (!_attAuto)
                    data -= (short)(_attenuation * 10);
                byte[] arr = BitConverter.GetBytes(data);
                level[i * 2] = arr[1];
                level[i * 2 + 1] = arr[0];
            }

            TraceAttributeConventional traceAttribute = new TraceAttributeConventional();
            traceAttribute.ChannelNumber = 0;
            traceAttribute.NumberOfTraceItems = dataLen;
            traceAttribute.OptionalHeaderLength = (byte)Marshal.SizeOf(ifPanHeader);
            traceAttribute.SelectorFlags = 0x80000001;

            GenericAttributeConventional genericAttribute = new GenericAttributeConventional();
            genericAttribute.Length = (ushort)(Marshal.SizeOf(traceAttribute) + Marshal.SizeOf(ifPanHeader) + dataLen * 2);
            genericAttribute.Tag = (ushort)TAGS.IFPAN;

            byte[] buffer = GetData(genericAttribute, traceAttribute, ifPanHeader, level);

            return buffer;
        }

        private byte[] GetDFData()
        {
            ulong selectorFlags = 0x800CF800;

            if (_dfMode == EDfMode.DFMODE_SCAN)
            {
                if (RunningScanRange == null)
                    return null;
                string strStep = (RunningScanRange.Step * 1000).ToString();
                int index = strStep.IndexOf(".");
                int denominator = 0;
                if (index == -1)
                    denominator = 1;
                else
                    denominator = 10 ^ (strStep.Length - index - 1);
                int numerator = int.Parse(strStep.Replace(".", ""));
                int step = numerator / denominator;
                ulong startFreq = (ulong)(RunningScanRange.StartFrequency * 1000000);
                ulong stopFreq = (ulong)(RunningScanRange.StopFrequency * 1000000);
                int span = (int)(stopFreq - startFreq);
                uint dataLen = (uint)(span / step + 1);
                int freqIndex = (int)dataLen / 2;
                //freqIndex = -1;

                _scanRangeCnt = dataLen;
                _hopIndexScanDF += _lastSendCnt;
                uint sendLen = SCANDF_MAXCNT;
                bool isLast = false;
                if (_scanRangeCnt - _hopIndexScanDF < SCANDF_MAXCNT)
                {
                    sendLen = _scanRangeCnt - _hopIndexScanDF;
                    isLast = true;
                }
                freqIndex = freqIndex - (int)_hopIndexScanDF;
                byte[] data = GetDFData(dataLen, sendLen, _hopIndexScanDF, freqIndex, selectorFlags, startFreq, numerator, denominator, span, isLast);
                _lastSendCnt = sendLen;
                if (isLast)
                {
                    _hopIndexScanDF = 0;
                    _lastSendCnt = 0;
                }
                return data;
            }
            else
            {
                if (DateTime.Now.Subtract(_lastDfTime).TotalMilliseconds < _integralTime)
                    return null;

                _lastDfTime = DateTime.Now;
                int span = (int)(_spectrumSpan * 1000);
                ulong freq = (ulong)(_frequency * 1000000);
                string strStep = (_dfPanStep * 1000).ToString();
                int index = strStep.IndexOf(".");
                int denominator = 0;
                if (index == -1)
                    denominator = 1;
                else
                    denominator = 10 ^ (strStep.Length - index - 1);
                int numerator = int.Parse(strStep.Replace(".", ""));
                int step = numerator / denominator;
                uint dataLen = (uint)(span / step + 1);
                ulong startFreq = freq - (ulong)span / 2;
                int freqIndex = (int)dataLen / 2;
                return GetDFData(dataLen, dataLen, 0, freqIndex, selectorFlags, startFreq, numerator, denominator, span, true);
            }
        }

        private byte[] GetDFData(uint totalLen, uint dataLen, uint logChannel, int freqIndex, ulong selectorFlags, ulong startFreq, int numerator, int denominator, int span, bool isLastHop)
        {
            DFPScanData dFPScanData = new DFPScanData(dataLen);
            for (int i = 0; i < dataLen; i++)
            {
                short level = (short)_random.Next(_noiseMin * 10, _noiseMax * 10);
                short azimuth = (short)_random.Next(0, 3599);
                short quality = (short)_random.Next(0, 500);
                short fstrength = level;
                short dfLevelCont = level;
                short elevation = azimuth;
                short dfChannelStatus = 414;
                short dfOmniphase = 0;
                if (_rfMode == ERf_Mode.RFMODE_LOW_NOISE)
                    level -= 100;
                if (Math.Abs(i - freqIndex) < 10)
                {
                    level = (short)_random.Next(_levelMin * 10, _levelMax * 10);
                    azimuth = (short)_random.Next(_azimuthMin * 10, _azimuthMax * 10);
                    quality = (short)_random.Next(900, 999);
                    fstrength = level;
                    dfLevelCont = level;
                    elevation = azimuth;
                    if (!_isUseSquelch)
                        _canSendAudio = true;
                    else
                    {
                        if (level < _squelchThreshold)
                            _canSendAudio = false;
                    }
                    if (_rfMode == ERf_Mode.RFMODE_LOW_DISTORTION)
                        level -= 100;
                    if (level < _levelThreshold * 10)
                        return null;
                }

                dFPScanData.DfLevel[i] = level;
                dFPScanData.Azimuth[i] = azimuth;
                dFPScanData.DfQuality[i] = quality;
                dFPScanData.DfFstrength[i] = fstrength;
                dFPScanData.DfLevelCont[i] = dfLevelCont;
                dFPScanData.Elevation[i] = elevation;
                dFPScanData.DfChannelStatus[i] = dfChannelStatus;
                dFPScanData.DfOmniphase[i] = dfOmniphase;
            }
            byte[] data = dFPScanData.ToBytes(selectorFlags);

            OptionalHeaderDFPScan header = new OptionalHeaderDFPScan();
            header.ScanRangeID = 0;
            header.ChannelsInScanRange = (int)totalLen;
            header.Frequency = startFreq;
            header.LogChannel = (int)logChannel;
            header.FrequencyStepNumerator = numerator;
            header.FrequencyStepDenominator = denominator;
            header.Span = span;
            header.Bandwidth = 56800;////////////////////////////////
            header.MeasureTime = 200000;///////////////////////////////
            header.MeasureCount = 200;/////////////////////////
            header.Threshold = (short)_levelThreshold;
            header.CompassHeading = 0;
            header.CompassHeadingType = -1;
            header.DFStatus = GetDFStatus(isLastHop);
            header.SweepTime = 4129543557;
            header.MeasureTimestamp = (ulong)DateTime.Now.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).Ticks * 100;
            header.JobID = _jobId;
            header.SRSelectorflags = 0;
            header.SRWaveCount = 0;

            TraceAttributeAdvanced traceAttribute = new TraceAttributeAdvanced();
            traceAttribute.NumberOfTraceItems = dataLen;
            traceAttribute.OptionalHeaderLength = (byte)Marshal.SizeOf(header);
            traceAttribute.SelectorFlagsLow = (uint)(selectorFlags & 0x00000000FFFFFFFF);
            traceAttribute.SelectorFlagsHigh = (uint)((selectorFlags & 0xFFFFFFFF00000000) >> 32);
            traceAttribute.Reserved2 = new UInt32[4];

            GenericAttributeAdvanced genericAttribute = new GenericAttributeAdvanced();
            genericAttribute.Length = (uint)(Marshal.SizeOf(traceAttribute) + Marshal.SizeOf(header) + data.Length);
            genericAttribute.Tag = (ushort)TAGS.DFPScan;
            genericAttribute.Reserved1 = 0;
            genericAttribute.Reserved2 = new uint[4];

            byte[] buffer = GetData(genericAttribute, traceAttribute, header, data);
            return buffer;
        }
        private byte[] GetPScanData()
        {
            uint selectorFlags = 0x80000001;
            int span = (int)((_stopFrequency - _startFrequency) * 1000000);
            ulong startFreq = (ulong)(_startFrequency * 1000000);
            ulong stopFreq = (ulong)(_stopFrequency * 1000000);
            uint step = (uint)(_step * 1000);
            uint denominator = 1;
            uint numerator = step;
            short dataLen = (short)(span / step + 1);

            byte[] data = new byte[dataLen * 2];
            for (int i = 0; i < dataLen; i++)
            {
                short level = (short)_random.Next(_noiseMin * 10, _noiseMax * 10);
                if (i == dataLen / 2)
                    level = (short)_random.Next(_levelMin * 10, _levelMax * 10);
                byte[] arr = BitConverter.GetBytes(level);
                data[i * 2] = arr[1];
                data[i * 2 + 1] = arr[0];
            }

            OptionalHeaderPScan header = new OptionalHeaderPScan();
            header.StartFrequencyLow = (uint)(startFreq & 0x00000000FFFFFFFF);
            header.StopFrequencyLow = (uint)(stopFreq & 0x00000000FFFFFFFF);
            header.StepFrequency = step;
            header.StartFrequencyHigh = (uint)((startFreq & 0xFFFFFFFF00000000) >> 32);
            header.StopFrequencyHigh = (uint)((stopFreq & 0xFFFFFFFF00000000) >> 32);
            header.reserved = new byte[4];
            header.OutputTimestamp = (ulong)DateTime.Now.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).Ticks * 100;
            header.StepFrequencyNumerator = numerator;
            header.StepFrequencyDenominator = denominator;
            header.FreqOfFirstStep = startFreq;

            TraceAttributeConventional traceAttribute = new TraceAttributeConventional();
            traceAttribute.ChannelNumber = 0;
            traceAttribute.NumberOfTraceItems = dataLen;
            traceAttribute.OptionalHeaderLength = (byte)Marshal.SizeOf(header);
            traceAttribute.SelectorFlags = selectorFlags;

            GenericAttributeConventional genericAttribute = new GenericAttributeConventional();
            genericAttribute.Length = (ushort)(Marshal.SizeOf(traceAttribute) + Marshal.SizeOf(header) + data.Length);
            genericAttribute.Tag = (ushort)TAGS.PSCAN;

            byte[] buffer = GetData(genericAttribute, traceAttribute, header, data);

            return buffer;
        }



        private byte[] GetData(IGenericAttribute genericAttribute, ITraceAttribute traceAttribute, IOptionalHeader optionalHeader, byte[] data)
        {
            EB200Header header = new EB200Header();
            header.DataSize = (uint)(Marshal.SizeOf(header) + Marshal.SizeOf(genericAttribute) + genericAttribute.DataLength);
            header.MagicNumber = 0x000EB200;
            header.VersionMinor = 0x0064;
            header.VersionMajor = 0x0002;
            header.SequenceNumberLow = 0x0001;
            header.SequenceNumberHigh = 0x0000;

            byte[] buffer = new byte[header.DataSize];
            byte[] src = header.ToBytes();
            int offset = 0;
            Buffer.BlockCopy(src, 0, buffer, offset, src.Length);
            offset += src.Length;
            src = genericAttribute.ToBytes();
            Buffer.BlockCopy(src, 0, buffer, offset, src.Length);
            offset += src.Length;
            src = traceAttribute.ToBytes();
            Buffer.BlockCopy(src, 0, buffer, offset, src.Length);
            offset += src.Length;
            src = optionalHeader.ToBytes();
            Buffer.BlockCopy(src, 0, buffer, offset, src.Length);
            offset += src.Length;
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            return buffer;
        }

        private int GetDFStatus(bool isLast)
        {
            byte[] data = new byte[4];
            if (_dfMode == EDfMode.DFMODE_FFM)
                data[0] = 0x02;
            else if (_dfMode == EDfMode.DFMODE_SCAN)
                data[0] = 0x03;
            else if (_dfMode == EDfMode.DFMODE_SEARCH)
                data[0] = 0x04;
            else
                data[0] = 0;
            switch (_dFindMode)
            {
                case EAverage_Mode.DFSQU_OFF:
                    data[1] = 0x20;
                    break;
                case EAverage_Mode.DFSQU_GATE:
                    data[1] = 0x21;
                    break;
                case EAverage_Mode.DFSQU_NORM:
                    data[1] = 0x22;
                    break;
            }
            if (_isCorrection)
                data[1] |= 0x10;
            data[2] = 0x20;
            if (isLast)
                data[3] = 0x11;
            else
                data[3] = 0x01;
            Array.Reverse(data);
            return BitConverter.ToInt32(data, 0);
        }
    }
}
