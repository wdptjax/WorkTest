using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace DeviceSim.Device
{
    internal class DataDefine
    {
        #region Argus向UMS300下发TDOA参数
        /// <summary>
        /// 设置解调模式
        /// </summary>
        public const string SET_DEMODULATION_MODE = ":DEM ";

        /// <summary>
        /// 设置解调带宽
        /// </summary>
        public const string SET_DEMODULATION_BANDWIDTH = ":BAND ";

        /// <summary>
        /// 电平测量模式（检波方式）
        /// </summary>
        public const string SET_LEVEL_MEASUREMODE = ":DET ";

        /// <summary>
        /// 设置衰减模式 自动/手动
        /// </summary>
        public const string SET_ATTENUATION_AUTO = ":INP:ATT:AUTO ";

        /// <summary>
        /// 衰减值
        /// </summary>
        public const string SET_ATTENUATION_VALUE = ":INP:ATT ";

        /// <summary>
        /// 射频模式
        /// </summary>
        public const string SET_RFMODE = ":INP:ATT:MODE ";

        /// <summary>
        /// 保持时间 秒 （:INP:ATT:AUTO:HOLD:TIME 0或者:INP:ATT:AUTO:HOLD:TIME  0.1 s
        /// </summary>
        public const string SET_HOLDTIME = ":INP:ATT:AUTO:HOLD:TIME ";

        ///// <summary>
        ///// 设置增益模式
        ///// </summary>
        //public const string SET_GAINCONTROL_MODE = "SENSE:GCON:AUTO:TIME ";

        ///// <summary>
        ///// 设置载波频率 SENSE:DEM:BFO 1000 Hz
        ///// </summary>
        //public const string SET_BFOFREQUENCY = "SENSE:DEM:BFO ";

        ///// <summary>
        ///// 是否使用自动频率控制（默认OFF   :FREQ:AFC OFF）
        ///// </summary>
        //public const string SET_FREQ_AFC = ":FREQ:AFC ";

        ///// <summary>
        ///// 是否应根据IF全景图跨度自动选择IF全景图步长 ON/OFF
        ///// </summary>
        //public const string SET_IFPAN_STEP_AUTO = "CALC:IFPAN:STEP:AUTO ";

        /// <summary>
        /// 频谱带宽
        /// </summary>
        public const string SET_FREQ_SPAN = ":FREQ:SPAN ";

        ///// <summary>
        ///// 所测量带宽的限制是否耦合到当前IF全景范围 on/off 需要ToLower()
        ///// </summary>
        //public const string SET_MEAS_BAND_LIM_AUTO = "meas:band:lim:auto ";

        /// <summary>
        /// FFT模式
        /// </summary>
        public const string SET_FFTMODE = "CALC:IFPAN:AVER:TYPE ";

        ///// <summary>
        ///// 设置多色IF全景图的操作模式
        ///// </summary>
        //public const string SET_PIFP_MODE = "CALC:PIFP:MODE ";

        ///// <summary>
        ///// 设置多彩IF全景图的活动持续时间(CALC:PIFP:ACTT 0.0150)
        ///// </summary>
        //public const string SET_PIFP_ACTT= "CALC:PIFP:ACTT ";

        ///// <summary>
        ///// 设置多色IF全景图的观察时间/持久性CALC:PIFP:OBST 0.5000
        ///// </summary>
        //public const string SET_PIFP_OBST = "CALC:PIFP:OBST ";

        /// <summary>
        /// 设置中心频率（:FREQ 99.000000 MHZ）
        /// </summary>
        public const string SET_FREQUENCY = ":FREQ ";

        ///// <summary>
        ///// 设置压制阈值 dbμV :OUTP:SQU:THR -10
        ///// </summary>
        //public const string SET_SQU_THRESHOLD = ":OUTP:SQU:THR ";

        ///// <summary>
        ///// 设置是否打开压制 ON/OFF
        ///// </summary>
        //public const string SET_SQU = ":OUTP:SQU ";

        ///// <summary>
        ///// 增益控制模式 
        ///// </summary>
        //public const string SET_GCON_MODE = ":GCON:MODE ";

        ///// <summary>
        ///// 打开/关闭内置扬声器 ON/OFF
        ///// </summary>
        //public const string SET_SPEAKER_STAT = ":SYST:SPEAKER:STAT ";

        ///// <summary>
        ///// 设置音量 MIN/MAX/值
        ///// </summary>
        //public const string SET_AUDIO_VOL = ":SYSTEM:AUDIO:VOL ";

        ///// <summary>
        ///// 设置解调频率（FREQ:DEM 99000000Hz）
        ///// </summary>
        //public const string SET_DEM_FREQ = "FREQ:DEM ";

        ///// <summary>
        ///// 为所有测量功能设置时间跨度 DEF/MIN/MAX(:Meas:Time DEF //:Meas:Time 200 ms)
        ///// </summary>
        //public const string SET_MEAS_TIME = ":Meas:Time ";

        /// <summary>
        /// 设置带宽测量模式 XDB/BETA
        /// </summary>
        public const string SET_MEAS_BAND_MODE = ":Meas:Band:Mode ";

        /// <summary>
        /// beta值 :Meas:Band:Beta 1.0 ///%
        /// </summary>
        public const string SET_MEAS_BETA_VALUE = ":Meas:Band:Beta ";

        /// <summary>
        /// xdB值 :Meas:Band:XDB 26.0 dB
        /// </summary>
        public const string SET_MEAS_XDB_VALUE = ":Meas:Band:XDB ";

        ///// <summary>
        ///// 视频输出模式
        ///// </summary>
        //public const string SET_VIDEO_REMOTE_MODE = ":SYSTEM:VIDEO:REMOTE:MODE ";

        ///// <summary>
        ///// :DISP:MENU IFPAN
        ///// </summary>
        //public const string SET_DISP_MENU = ":DISP:MENU ";

        /// <summary>
        /// 设置IQ字节数 16位/32位
        /// </summary>
        public const string SET_IF_REMOTE_MODE = ":SYSTEM:IF:REMOTE:MODE ";

        ///// <summary>
        ///// 根据CCIR设置电视解调标准B|G|D|K|I|L|M|N
        ///// </summary>
        //public const string SET_VID_STAN = ":SENS:VID:STAN ";

        ///// <summary>
        ///// 通过选择IF全景图中的FFT过滤器特性来设置IF全景图中的选择性。AUTO | NORMal | NARRow | SHARp 
        ///// </summary>
        //public const string SET_IFPAN_SEL = "CALC:IFPAN:SEL ";

        ///// <summary>
        ///// 打开或关闭SelCall分析。
        ///// </summary>
        //public const string SET_DEC_SELC_STATE = ":SENSE:DEC:SELC:STATE ";

        ///// <summary>
        ///// 设置视频模式与视频模拟输出的中心频率【IF | DEModulator | PDEModulator | NDEModulator 】？（OUTP:VID:MODE IF;Freq 10700000 Hz）
        ///// </summary>
        //public const string SET_VIDEO_MODE = "OUTP:VID:MODE ";

        /// <summary>
        /// 关闭功能///这个可以不用，反正我们停止任务的时候会关闭所有功能的
        /// </summary>
        public const string SET_FUNC_OFF = ":FUNC:OFF ";

        /// <summary>
        /// 停止任务
        /// </summary>
        public const string ABORT = "ABORT";

        /// <summary>
        /// 启动任务
        /// </summary>
        public const string INIT = "Init";

        #endregion Argus向UMS300下发TDOA参数
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct EB200Header
    {
        [MarshalAs(UnmanagedType.U4)]
        public uint MagicNumber;
        [MarshalAs(UnmanagedType.U2)]
        public ushort VersionMinor;
        [MarshalAs(UnmanagedType.U2)]
        public ushort VersionMajor;
        [MarshalAs(UnmanagedType.U2)]
        public ushort SequenceNumber;
        [MarshalAs(UnmanagedType.I2)]
        public short Reserved0;
        [MarshalAs(UnmanagedType.U4)]
        public uint DataSize;

        public EB200Header(byte[] buffer, int offset)
        {
            Array.Reverse(buffer, offset, 4);
            MagicNumber = BitConverter.ToUInt32(buffer, offset);
            Array.Reverse(buffer, offset + 4, 2);
            VersionMinor = BitConverter.ToUInt16(buffer, offset + 4);
            Array.Reverse(buffer, offset + 6, 2);
            VersionMajor = BitConverter.ToUInt16(buffer, offset + 6);
            Array.Reverse(buffer, offset + 8, 2);
            SequenceNumber = BitConverter.ToUInt16(buffer, offset + 8);
            Array.Reverse(buffer, offset + 10, 2);
            Reserved0 = BitConverter.ToInt16(buffer, offset + 10);
            Array.Reverse(buffer, offset + 12, 4);
            DataSize = BitConverter.ToUInt32(buffer, offset + 12);
        }

        public byte[] ToBytes()
        {
            int len = Marshal.SizeOf(this);
            byte[] buffer = new byte[len];
            int offset = 0;
            byte[] data = BitConverter.GetBytes(MagicNumber);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(VersionMinor);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(VersionMajor);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(SequenceNumber);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(Reserved0);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(DataSize);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            return buffer;
        }
    };

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct GenericAttribute
    {
        [MarshalAs(UnmanagedType.U2)]
        public ushort Tag;
        [MarshalAs(UnmanagedType.U2)]
        public ushort Length;
        public GenericAttribute(byte[] buffer, int offset)
        {
            Array.Reverse(buffer, offset, 2);
            Tag = BitConverter.ToUInt16(buffer, offset);
            Array.Reverse(buffer, offset + 2, 2);
            Length = BitConverter.ToUInt16(buffer, offset + 2);
        }

        public byte[] ToBytes()
        {
            int len = Marshal.SizeOf(this);
            byte[] buffer = new byte[len];
            int offset = 0;
            byte[] data = BitConverter.GetBytes(Tag);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(Length);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;

            return buffer;
        }
    };

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct TraceAttribute
    {
        /// <summary>
        /// 值的个数
        /// </summary>
        [MarshalAs(UnmanagedType.I2)]
        public short NumberOfTraceItems;
        /// <summary>
        /// 接收机通道
        /// 0-Receiver
        /// 1-DDC1
        /// 2-DDC2
        /// 3-DDC3
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public byte ChannelNumber;
        [MarshalAs(UnmanagedType.U1)]
        public byte OptionalHeaderLength;
        [MarshalAs(UnmanagedType.U4)]
        public uint SelectorFlags;

        public TraceAttribute(byte[] buffer, int offset)
        {
            Array.Reverse(buffer, offset, 2);
            NumberOfTraceItems = BitConverter.ToInt16(buffer, offset);
            ChannelNumber = buffer[offset + 2];
            OptionalHeaderLength = buffer[offset + 3];
            Array.Reverse(buffer, offset + 4, 4);
            SelectorFlags = BitConverter.ToUInt32(buffer, offset + 4);
        }

        public byte[] ToBytes()
        {
            int len = Marshal.SizeOf(this);
            byte[] buffer = new byte[len];
            int offset = 0;
            byte[] data = BitConverter.GetBytes(NumberOfTraceItems);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            buffer[offset] = ChannelNumber;
            offset += 1;
            buffer[offset] = OptionalHeaderLength;
            offset += 1;
            data = BitConverter.GetBytes(SelectorFlags);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;

            return buffer;
        }
    }

    interface IOptionalHeader
    {
        byte[] ToBytes();
    }

    /// <summary>
    /// OptionalHeaderIFPan
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct OptionalHeaderIFPan : IOptionalHeader
    {
        [MarshalAs(UnmanagedType.U4)]
        public uint FrequencyLow;
        [MarshalAs(UnmanagedType.U4)]
        public uint SpanFrequency;
        [MarshalAs(UnmanagedType.I2)]
        public short AverageTime;//Not used and always set to 0
        [MarshalAs(UnmanagedType.I2)]
        public short AverageType;
        [MarshalAs(UnmanagedType.U4)]
        public uint MeasureTime;//us
        [MarshalAs(UnmanagedType.U4)]
        public uint FrequencyHigh;
        [MarshalAs(UnmanagedType.I4)]
        public int DemodFreqChannel;
        [MarshalAs(UnmanagedType.U4)]
        public uint DemodFreqLow;
        [MarshalAs(UnmanagedType.U4)]
        public uint DemodFreqHigh;
        [MarshalAs(UnmanagedType.U8)]
        public ulong OutputTimestamp;
        [MarshalAs(UnmanagedType.U4)]
        public uint StepFrequencyNumerator;
        [MarshalAs(UnmanagedType.U4)]
        public uint StepFrequencyDenominator;
        [MarshalAs(UnmanagedType.I2)]
        public short SignalSource;
        [MarshalAs(UnmanagedType.I2)]
        public short MeasureMode;
        [MarshalAs(UnmanagedType.U8)]
        public ulong MeasureTimestamp;
        [MarshalAs(UnmanagedType.I2)]
        public short Reserved;

        public OptionalHeaderIFPan(byte[] value, int startIndex)
        {
            Reserved = 0;
            if (BitConverter.IsLittleEndian)
            {
                byte[] data = new byte[4];
                Buffer.BlockCopy(value, startIndex, data, 0, 4);
                Array.Reverse(data);
                FrequencyLow = BitConverter.ToUInt32(data, 0);
                Buffer.BlockCopy(value, startIndex + 4, data, 0, 4);
                Array.Reverse(data);
                SpanFrequency = BitConverter.ToUInt32(data, 0);
                data = new byte[2];
                Buffer.BlockCopy(value, startIndex + 8, data, 0, 2);
                Array.Reverse(data);
                AverageTime = BitConverter.ToInt16(data, 0);
                Buffer.BlockCopy(value, startIndex + 10, data, 0, 2);
                Array.Reverse(data);
                AverageType = BitConverter.ToInt16(data, 0);
                data = new byte[4];
                Buffer.BlockCopy(value, startIndex + 12, data, 0, 4);
                Array.Reverse(data);
                MeasureTime = BitConverter.ToUInt32(data, 0);
                Buffer.BlockCopy(value, startIndex + 16, data, 0, 4);
                Array.Reverse(data);
                FrequencyHigh = BitConverter.ToUInt32(data, 0);
                Buffer.BlockCopy(value, startIndex + 20, data, 0, 4);
                Array.Reverse(data);
                DemodFreqChannel = BitConverter.ToInt32(data, 0);

                Buffer.BlockCopy(value, startIndex + 24, data, 0, 4);
                Array.Reverse(data);
                DemodFreqLow = BitConverter.ToUInt32(data, 0);
                Buffer.BlockCopy(value, startIndex + 28, data, 0, 4);
                Array.Reverse(data);
                DemodFreqHigh = BitConverter.ToUInt32(data, 0);

                data = new byte[8];
                Buffer.BlockCopy(value, startIndex + 32, data, 0, 8);
                Array.Reverse(data);
                OutputTimestamp = BitConverter.ToUInt64(data, 0);
                data = new byte[4];
                Buffer.BlockCopy(value, startIndex + 40, data, 0, 4);
                Array.Reverse(data);
                StepFrequencyNumerator = BitConverter.ToUInt32(data, 0);
                Buffer.BlockCopy(value, startIndex + 44, data, 0, 4);
                Array.Reverse(data);
                StepFrequencyDenominator = BitConverter.ToUInt32(data, 0);
                data = new byte[2];
                Buffer.BlockCopy(value, startIndex + 48, data, 0, 2);
                Array.Reverse(data);
                SignalSource = BitConverter.ToInt16(data, 0);
                Buffer.BlockCopy(value, startIndex + 50, data, 0, 2);
                Array.Reverse(data);
                MeasureMode = BitConverter.ToInt16(data, 0);
                data = new byte[8];
                Buffer.BlockCopy(value, startIndex + 52, data, 0, 8);
                Array.Reverse(data);
                MeasureTimestamp = BitConverter.ToUInt64(data, 0);
            }
            else
            {
                FrequencyLow = BitConverter.ToUInt32(value, startIndex);
                SpanFrequency = BitConverter.ToUInt32(value, startIndex + 4);
                AverageTime = BitConverter.ToInt16(value, startIndex + 8);
                AverageType = BitConverter.ToInt16(value, startIndex + 10);
                MeasureTime = BitConverter.ToUInt32(value, startIndex + 12);
                FrequencyHigh = BitConverter.ToUInt32(value, startIndex + 16);
                DemodFreqChannel = BitConverter.ToInt32(value, startIndex + 20);
                DemodFreqLow = BitConverter.ToUInt32(value, startIndex + 24);
                DemodFreqHigh = BitConverter.ToUInt32(value, startIndex + 28);
                OutputTimestamp = BitConverter.ToUInt64(value, startIndex + 32);
                StepFrequencyNumerator = BitConverter.ToUInt32(value, startIndex + 40);
                StepFrequencyDenominator = BitConverter.ToUInt32(value, startIndex + 44);
                SignalSource = BitConverter.ToInt16(value, startIndex + 48);
                MeasureMode = BitConverter.ToInt16(value, startIndex + 50);
                MeasureTimestamp = BitConverter.ToUInt64(value, startIndex + 52);
            }

        }
        public byte[] ToBytes()
        {
            int len = Marshal.SizeOf(this);
            byte[] buffer = new byte[len];
            int offset = 0;
            byte[] data = BitConverter.GetBytes(FrequencyLow);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(SpanFrequency);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(AverageTime);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(AverageType);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(MeasureTime);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(FrequencyHigh);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(DemodFreqChannel);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(DemodFreqLow);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(DemodFreqHigh);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(OutputTimestamp);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(StepFrequencyNumerator);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(StepFrequencyDenominator);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(SignalSource);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(MeasureMode);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(MeasureTimestamp);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(Reserved);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            return buffer;
        }
    }

    /// <summary>
    /// OptionalHeaderIF
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct OptionalHeaderIF : IOptionalHeader
    {
        //SYSTem:IF:REMote:MODE OFF|SHORT|LONG
        [MarshalAs(UnmanagedType.I2)]
        public short Mode;
        [MarshalAs(UnmanagedType.I2)]
        public short FrameLen;
        [MarshalAs(UnmanagedType.U4)]
        public uint Samplerate;
        [MarshalAs(UnmanagedType.U4)]
        public uint FrequencyLow;
        [MarshalAs(UnmanagedType.U4)]
        public uint Bandwidth;//IF bandwidth
        [MarshalAs(UnmanagedType.U2)]
        public ushort Demodulation;
        [MarshalAs(UnmanagedType.I2)]
        public short RxAtt;
        [MarshalAs(UnmanagedType.U2)]
        public ushort Flags;
        [MarshalAs(UnmanagedType.I2)]
        public short KFactor;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
        public string sDemodulation;
        [MarshalAs(UnmanagedType.U8)]
        public ulong SampleCount;
        [MarshalAs(UnmanagedType.U4)]
        public uint FrequencyHigh;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 4)]
        public byte[] reserved;
        [MarshalAs(UnmanagedType.U8)]
        public ulong StartTimestamp;
        [MarshalAs(UnmanagedType.I2)]
        public short SignalSource;

        public OptionalHeaderIF(byte[] value, int startIndex)
        {
            if (BitConverter.IsLittleEndian)
            {
                byte[] data = new byte[2];
                Buffer.BlockCopy(value, startIndex, data, 0, 2);
                Array.Reverse(data);
                Mode = BitConverter.ToInt16(data, 0);
                Buffer.BlockCopy(value, startIndex + 2, data, 0, 2);
                Array.Reverse(data);
                FrameLen = BitConverter.ToInt16(data, 0);
                data = new byte[4];
                Buffer.BlockCopy(value, startIndex + 4, data, 0, 4);
                Array.Reverse(data);
                Samplerate = BitConverter.ToUInt32(data, 0);
                Buffer.BlockCopy(value, startIndex + 8, data, 0, 4);
                Array.Reverse(data);
                FrequencyLow = BitConverter.ToUInt32(data, 0);
                Buffer.BlockCopy(value, startIndex + 12, data, 0, 4);
                Array.Reverse(data);
                Bandwidth = BitConverter.ToUInt32(data, 0);
                data = new byte[2];
                Buffer.BlockCopy(value, startIndex + 16, data, 0, 2);
                Array.Reverse(data);
                Demodulation = BitConverter.ToUInt16(data, 0);
                Buffer.BlockCopy(value, startIndex + 18, data, 0, 2);
                Array.Reverse(data);
                RxAtt = BitConverter.ToInt16(data, 0);
                Buffer.BlockCopy(value, startIndex + 20, data, 0, 2);
                Array.Reverse(data);
                Flags = BitConverter.ToUInt16(data, 0);
                Buffer.BlockCopy(value, startIndex + 22, data, 0, 2);
                Array.Reverse(data);
                KFactor = BitConverter.ToInt16(data, 0);
                sDemodulation = BitConverter.ToString(value, startIndex + 24, 8);
                data = new byte[8];
                Buffer.BlockCopy(value, startIndex + 32, data, 0, 8);
                Array.Reverse(data);
                SampleCount = BitConverter.ToUInt64(data, 0);
                data = new byte[4];
                Buffer.BlockCopy(value, startIndex + 40, data, 0, 4);
                Array.Reverse(data);
                FrequencyHigh = BitConverter.ToUInt32(data, 0);
                reserved = new byte[4];
                data = new byte[8];
                Buffer.BlockCopy(value, startIndex + 48, data, 0, 8);
                Array.Reverse(data);
                StartTimestamp = BitConverter.ToUInt64(data, 0);
                data = new byte[2];
                Buffer.BlockCopy(value, startIndex + 56, data, 0, 2);
                Array.Reverse(data);
                SignalSource = BitConverter.ToInt16(data, 0);
            }
            else
            {
                Mode = BitConverter.ToInt16(value, startIndex);
                FrameLen = BitConverter.ToInt16(value, startIndex + 2);
                Samplerate = BitConverter.ToUInt32(value, startIndex + 4);
                FrequencyLow = BitConverter.ToUInt32(value, startIndex + 8);
                Bandwidth = BitConverter.ToUInt32(value, startIndex + 12);
                Demodulation = BitConverter.ToUInt16(value, startIndex + 16);
                RxAtt = BitConverter.ToInt16(value, startIndex + 18);
                Flags = BitConverter.ToUInt16(value, startIndex + 20);
                KFactor = BitConverter.ToInt16(value, startIndex + 22);
                sDemodulation = BitConverter.ToString(value, startIndex + 24, 8);
                SampleCount = BitConverter.ToUInt64(value, startIndex + 32);
                FrequencyHigh = BitConverter.ToUInt32(value, startIndex + 40);
                reserved = new byte[4];
                StartTimestamp = BitConverter.ToUInt64(value, startIndex + 48);
                SignalSource = BitConverter.ToInt16(value, startIndex + 56);
            }
        }
        public byte[] ToBytes()
        {
            int len = Marshal.SizeOf(this);
            byte[] buffer = new byte[len];
            int offset = 0;
            byte[] data = BitConverter.GetBytes(Mode);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(FrameLen);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(Samplerate);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(FrequencyLow);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(Bandwidth);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(Demodulation);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(RxAtt);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(Flags);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(KFactor);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = Encoding.ASCII.GetBytes(sDemodulation);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += 8;
            data = BitConverter.GetBytes(SampleCount);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(FrequencyHigh);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            offset += 4;
            data = BitConverter.GetBytes(StartTimestamp);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(SignalSource);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;

            return buffer;
        }
    }

    /// <summary>
    /// OptionalHeaderCW
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct OptionalHeaderCW : IOptionalHeader
    {
        [MarshalAs(UnmanagedType.U4)]
        public uint Freq_Low;
        [MarshalAs(UnmanagedType.U4)]
        public uint Freq_High;
        [MarshalAs(UnmanagedType.U8)]
        public ulong OutputTimestamp;
        [MarshalAs(UnmanagedType.I2)]
        public short SignalSource;

        public OptionalHeaderCW(byte[] value, int startIndex)
        {
            if (BitConverter.IsLittleEndian)
            {
                byte[] data = new byte[4];
                Buffer.BlockCopy(value, startIndex, data, 0, 4);
                Array.Reverse(data);
                Freq_Low = BitConverter.ToUInt32(data, 0);
                Buffer.BlockCopy(value, startIndex + 4, data, 0, 4);
                Array.Reverse(data);
                Freq_High = BitConverter.ToUInt32(data, 0);
                data = new byte[8];
                Buffer.BlockCopy(value, startIndex + 8, data, 0, 8);
                Array.Reverse(data);
                OutputTimestamp = BitConverter.ToUInt64(data, 0);
                data = new byte[2];
                Buffer.BlockCopy(value, startIndex + 16, data, 0, 2);
                Array.Reverse(data);
                SignalSource = BitConverter.ToInt16(data, 0);
            }
            else
            {
                Freq_Low = BitConverter.ToUInt32(value, startIndex);
                Freq_High = BitConverter.ToUInt32(value, startIndex + 4);
                OutputTimestamp = BitConverter.ToUInt64(value, startIndex + 8);
                SignalSource = BitConverter.ToInt16(value, startIndex + 16);
            }
        }

        public byte[] ToBytes()
        {
            int len = Marshal.SizeOf(this);
            byte[] buffer = new byte[len];
            int offset = 0;
            byte[] data = BitConverter.GetBytes(Freq_Low);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(Freq_High);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(OutputTimestamp);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(SignalSource);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;

            return buffer;
        }
    }


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct OptionalHeaderGPSCompass : IOptionalHeader
    {
        [MarshalAs(UnmanagedType.U8)]
        public UInt64 OutputTimestamp;

        public OptionalHeaderGPSCompass(byte[] value, int startIndex)
        {
            Array.Reverse(value, startIndex, 8);
            OutputTimestamp = BitConverter.ToUInt64(value, startIndex);
        }

        public byte[] ToBytes()
        {
            byte[] buffer = BitConverter.GetBytes(OutputTimestamp);
            Array.Reverse(buffer);
            return buffer;
        }
    }

    /// <summary>
    /// CW数据结构
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct CWData
    {
        public int DataCnt;
        public short[] Level;
        public int[] FreqOffset;
        public short[] FStrength;
        public short[] AMDepth;
        public short[] AMDepthPos;
        public short[] AMDepthNeg;
        public int[] FMDev;
        public int[] FMDevPos;
        public int[] FMDevNeg;
        public short[] PMDepth;
        public int[] BandWidth;
        public uint[] Freq_Low;
        public uint[] Freq_High;

        public CWData(int dataCnt, byte[] buffer, int startIndex, uint selectorFlags)
        {
            DataCnt = dataCnt;
            Level = new short[dataCnt];
            FreqOffset = new int[dataCnt];
            FStrength = new short[dataCnt];
            AMDepth = new short[dataCnt];
            AMDepthPos = new short[dataCnt];
            AMDepthNeg = new short[dataCnt];
            FMDev = new int[dataCnt];
            FMDevPos = new int[dataCnt];
            FMDevNeg = new int[dataCnt];
            PMDepth = new short[dataCnt];
            BandWidth = new int[dataCnt];
            Freq_Low = new uint[dataCnt];
            Freq_High = new uint[dataCnt];
            int offset = startIndex;
            if ((selectorFlags & (uint)FLAGS.LEVEL) > 0)
            {
                for (int i = 0; i < dataCnt; i++)
                {
                    Array.Reverse(buffer, offset, 2);
                    Level[i] = BitConverter.ToInt16(buffer, offset);
                    if (Level[i] == 2000)
                    {
                        Level[i] = short.MinValue;
                    }
                    offset += 2;
                }
            }
            if ((selectorFlags & (uint)FLAGS.OFFSET) > 0)
            {
                for (int i = 0; i < dataCnt; i++)
                {
                    Array.Reverse(buffer, offset, 4);
                    FreqOffset[i] = BitConverter.ToInt32(buffer, offset);
                    if (FreqOffset[i] == 10000000)
                    {
                        FreqOffset[i] = int.MinValue;
                    }
                    offset += 4;
                }
            }
            if ((selectorFlags & (uint)FLAGS.FSTRENGTH) > 0)
            {
                for (int i = 0; i < dataCnt; i++)
                {
                    Array.Reverse(buffer, offset, 2);
                    FStrength[i] = BitConverter.ToInt16(buffer, offset);
                    if (FStrength[i] == 0x7FFF)
                    {
                        FStrength[i] = short.MinValue;
                    }
                    offset += 2;
                }
            }
            if ((selectorFlags & (uint)FLAGS.AM) > 0)
            {
                for (int i = 0; i < dataCnt; i++)
                {
                    Array.Reverse(buffer, offset, 2);
                    AMDepth[i] = BitConverter.ToInt16(buffer, offset);
                    if (AMDepth[i] == 0x7FFF)
                    {
                        AMDepth[i] = short.MinValue;
                    }
                    offset += 2;
                }
            }
            if ((selectorFlags & (uint)FLAGS.AM_POS) > 0)
            {
                for (int i = 0; i < dataCnt; i++)
                {
                    Array.Reverse(buffer, offset, 2);
                    AMDepthPos[i] = BitConverter.ToInt16(buffer, offset);
                    offset += 2;
                }
            }
            if ((selectorFlags & (uint)FLAGS.AM_NEG) > 0)
            {
                for (int i = 0; i < dataCnt; i++)
                {
                    Array.Reverse(buffer, offset, 2);
                    AMDepthNeg[i] = BitConverter.ToInt16(buffer, offset);
                    offset += 2;
                }
            }
            if ((selectorFlags & (uint)FLAGS.FM) > 0)
            {
                for (int i = 0; i < dataCnt; i++)
                {
                    Array.Reverse(buffer, offset, 4);
                    FMDev[i] = BitConverter.ToInt32(buffer, offset);
                    if (FMDev[i] == 0x7FFFFFFF)
                    {
                        FMDev[i] = int.MinValue;
                    }
                    offset += 4;
                }
            }
            if ((selectorFlags & (uint)FLAGS.FM_POS) > 0)
            {
                for (int i = 0; i < dataCnt; i++)
                {
                    Array.Reverse(buffer, offset, 4);
                    FMDevPos[i] = BitConverter.ToInt32(buffer, offset);
                    offset += 4;
                }
            }
            if ((selectorFlags & (uint)FLAGS.FM_NEG) > 0)
            {
                for (int i = 0; i < dataCnt; i++)
                {
                    Array.Reverse(buffer, offset, 4);
                    FMDevNeg[i] = BitConverter.ToInt32(buffer, offset);
                    offset += 4;
                }
            }
            if ((selectorFlags & (uint)FLAGS.PM) > 0)
            {
                for (int i = 0; i < dataCnt; i++)
                {
                    Array.Reverse(buffer, offset, 2);
                    PMDepth[i] = BitConverter.ToInt16(buffer, offset);
                    if (PMDepth[i] == 0x7FFF)
                    {
                        PMDepth[i] = short.MinValue;
                    }
                    offset += 2;
                }
            }
            if ((selectorFlags & (uint)FLAGS.BANDWIDTH) > 0)
            {
                for (int i = 0; i < dataCnt; i++)
                {
                    Array.Reverse(buffer, offset, 4);
                    BandWidth[i] = BitConverter.ToInt32(buffer, offset);
                    if (BandWidth[i] == 0x7FFFFFFF)
                    {
                        BandWidth[i] = int.MinValue;
                    }
                    offset += 4;
                }
            }
            if ((selectorFlags & (uint)FLAGS.FREQLOW) > 0)
            {
                for (int i = 0; i < dataCnt; i++)
                {
                    Array.Reverse(buffer, offset, 4);
                    Freq_Low[i] = BitConverter.ToUInt32(buffer, offset);
                    offset += 4;
                }
            }
            if ((selectorFlags & (uint)FLAGS.FREQHIGH) > 0)
            {
                for (int i = 0; i < dataCnt; i++)
                {
                    Array.Reverse(buffer, offset, 4);
                    Freq_High[i] = BitConverter.ToUInt32(buffer, offset);
                    offset += 4;
                }
            }
        }

        public CWData(int dataCnt)
        {
            DataCnt = dataCnt;
            Level = new short[dataCnt];
            FreqOffset = new int[dataCnt];
            FStrength = new short[dataCnt];
            AMDepth = new short[dataCnt];
            AMDepthPos = new short[dataCnt];
            AMDepthNeg = new short[dataCnt];
            FMDev = new int[dataCnt];
            FMDevPos = new int[dataCnt];
            FMDevNeg = new int[dataCnt];
            PMDepth = new short[dataCnt];
            BandWidth = new int[dataCnt];
            Freq_Low = new uint[dataCnt];
            Freq_High = new uint[dataCnt];
        }
        public byte[] ToBytes(uint selectorFlags)
        {
            int len = DataCnt * 40;
            byte[] buffer = new byte[len];
            int offset = 0;
            if ((selectorFlags & (uint)FLAGS.LEVEL) > 0)
            {
                for (int i = 0; i < DataCnt; i++)
                {
                    byte[] data = BitConverter.GetBytes(Level[i]);
                    Array.Reverse(data);
                    Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
                    offset += data.Length;
                }
            }
            if ((selectorFlags & (uint)FLAGS.OFFSET) > 0)
            {
                for (int i = 0; i < DataCnt; i++)
                {
                    byte[] data = BitConverter.GetBytes(FreqOffset[i]);
                    Array.Reverse(data);
                    Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
                    offset += data.Length;
                }
            }
            if ((selectorFlags & (uint)FLAGS.FSTRENGTH) > 0)
            {
                for (int i = 0; i < DataCnt; i++)
                {
                    byte[] data = BitConverter.GetBytes(FStrength[i]);
                    Array.Reverse(data);
                    Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
                    offset += data.Length;
                }
            }
            if ((selectorFlags & (uint)FLAGS.AM) > 0)
            {
                for (int i = 0; i < DataCnt; i++)
                {
                    byte[] data = BitConverter.GetBytes(AMDepth[i]);
                    Array.Reverse(data);
                    Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
                    offset += data.Length;
                }
            }
            if ((selectorFlags & (uint)FLAGS.AM_POS) > 0)
            {
                for (int i = 0; i < DataCnt; i++)
                {
                    byte[] data = BitConverter.GetBytes(AMDepthPos[i]);
                    Array.Reverse(data);
                    Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
                    offset += data.Length;
                }
            }
            if ((selectorFlags & (uint)FLAGS.AM_NEG) > 0)
            {
                for (int i = 0; i < DataCnt; i++)
                {
                    byte[] data = BitConverter.GetBytes(AMDepthNeg[i]);
                    Array.Reverse(data);
                    Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
                    offset += data.Length;
                }
            }
            if ((selectorFlags & (uint)FLAGS.FM) > 0)
            {
                for (int i = 0; i < DataCnt; i++)
                {
                    byte[] data = BitConverter.GetBytes(FMDev[i]);
                    Array.Reverse(data);
                    Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
                    offset += data.Length;
                }
            }
            if ((selectorFlags & (uint)FLAGS.FM_POS) > 0)
            {
                for (int i = 0; i < DataCnt; i++)
                {
                    byte[] data = BitConverter.GetBytes(FMDevPos[i]);
                    Array.Reverse(data);
                    Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
                    offset += data.Length;
                }
            }
            if ((selectorFlags & (uint)FLAGS.FM_NEG) > 0)
            {
                for (int i = 0; i < DataCnt; i++)
                {
                    byte[] data = BitConverter.GetBytes(FMDevNeg[i]);
                    Array.Reverse(data);
                    Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
                    offset += data.Length;
                }
            }
            if ((selectorFlags & (uint)FLAGS.PM) > 0)
            {
                for (int i = 0; i < DataCnt; i++)
                {
                    byte[] data = BitConverter.GetBytes(PMDepth[i]);
                    Array.Reverse(data);
                    Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
                    offset += data.Length;
                }
            }
            if ((selectorFlags & (uint)FLAGS.BANDWIDTH) > 0)
            {
                for (int i = 0; i < DataCnt; i++)
                {
                    byte[] data = BitConverter.GetBytes(BandWidth[i]);
                    Array.Reverse(data);
                    Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
                    offset += data.Length;
                }
            }
            if ((selectorFlags & (uint)FLAGS.FREQLOW) > 0)
            {
                for (int i = 0; i < DataCnt; i++)
                {
                    byte[] data = BitConverter.GetBytes(Freq_Low[i]);
                    Array.Reverse(data);
                    Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
                    offset += data.Length;
                }
            }
            if ((selectorFlags & (uint)FLAGS.FREQHIGH) > 0)
            {
                for (int i = 0; i < DataCnt; i++)
                {
                    byte[] data = BitConverter.GetBytes(Freq_High[i]);
                    Array.Reverse(data);
                    Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
                    offset += data.Length;
                }
            }

            byte[] arr = new byte[offset];
            if (offset < len)
            {
                Buffer.BlockCopy(buffer, 0, arr, 0, offset);
            }

            return arr;
        }
    }

    /// <summary>
    /// GPS数据
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct GPSCompassData
    {
        #region 属性

        [MarshalAs(UnmanagedType.U2)]
        public ushort CompassHeading;
        /// <summary>
        /// -1代表罗盘无效
        /// </summary>
        [MarshalAs(UnmanagedType.I2)]
        public short CompassHeadingType;
        /// <summary>
        /// GPS数据是否有效,1-有效,0-无效
        /// </summary>
        [MarshalAs(UnmanagedType.I2)]
        public short GPSValid;
        /// <summary>
        /// GPS所接收的星数
        /// </summary>
        [MarshalAs(UnmanagedType.I2)]
        public short NoOfSatInView;
        /// <summary>
        /// 地理纬度方向(N/S) ASCII
        /// </summary>
        [MarshalAs(UnmanagedType.I2)]
        public short LatRef;
        /// <summary>
        /// 纬度度数°
        /// </summary>
        [MarshalAs(UnmanagedType.I2)]
        public short LatDeg;
        /// <summary>
        /// 纬度分数′
        /// </summary>
        [MarshalAs(UnmanagedType.R4)]
        public float LatMin;
        /// <summary>
        /// 地理经度方向(E/W) ASCII
        /// </summary>
        [MarshalAs(UnmanagedType.I2)]
        public short LonRef;
        /// <summary>
        /// 经度度数°
        /// </summary>
        [MarshalAs(UnmanagedType.I2)]
        public short LonDeg;
        /// <summary>
        /// 经度分数′
        /// </summary>
        [MarshalAs(UnmanagedType.R4)]
        public float LonMin;
        /// <summary>
        /// 水平精度因子
        /// Horizontal Dilution Of Precision
        /// </summary>
        [MarshalAs(UnmanagedType.R4)]
        public float HDOP;
        /// <summary>
        /// 表示天线数据是否被视为有效
        /// Denotes whether Antenna data is to be considered valid:
        /// </summary>
        [MarshalAs(UnmanagedType.I2)]
        public short AntValid;
        /// <summary>
        /// 天线是否倾斜
        /// 0-不倾斜 1-倾斜
        /// Denotes whether the antenna is tilt over: 
        /// </summary>
        [MarshalAs(UnmanagedType.I2)]
        public short AntTiltOver;
        /// <summary>
        /// 长度轴上方天线标高[°]
        /// Antenna elevation seen over the length axis [°] 
        /// </summary>
        [MarshalAs(UnmanagedType.I2)]
        public short AntElevation;
        /// <summary>
        /// 天线横摇超过长度轴[°]
        /// Antenna roll seen over the length axis [°]
        /// </summary>
        [MarshalAs(UnmanagedType.I2)]
        public short AntRoll;
        /// <summary>
        /// 信号源 0-天线 1-播放历史文件 Replay of a recording 
        /// </summary>
        [MarshalAs(UnmanagedType.I2)]
        public short SignalSource;
        /// <summary>
        /// 表示角速率是否被认为是有效的
        /// Denotes whether the angular rates are to be considered valid
        /// </summary>
        [MarshalAs(UnmanagedType.I2)]
        public short AngularRatesValid;
        /// <summary>
        /// 航向角速率(1/100°/ s)
        /// Heading angular rate[1/100°/s]
        /// </summary>
        [MarshalAs(UnmanagedType.I2)]
        public short HeadingAngularRate;
        /// <summary>
        /// 仰角速率[1/100°/s]
        /// Elevation angular rate [1/100°/s]
        /// </summary>
        [MarshalAs(UnmanagedType.I2)]
        public short ElevationAngularRate;
        /// <summary>
        /// 滚转角速率[1/100°/s]
        /// Roll angular rate [1/100°/s]
        /// </summary>
        [MarshalAs(UnmanagedType.I2)]
        public short RollAngularRate;
        /// <summary>
        /// 表示是否认为大地水准面分离是有效的
        /// </summary>
        [MarshalAs(UnmanagedType.I2)]
        public short GeoidalSeparationValid;
        /// <summary>
        /// 大地水准面分离 /100
        ///  Geoidal separation [1/100 m]. Difference between height above WGS84 ellipsoid and height above mean sea level: heightWGS84 = heightMSL + geoidalSeparation
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public int GeoidalSeparation;
        /// <summary>
        /// 海拔高度(厘米)
        /// Height above mean sea level [1/100 m]
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public int Altitude;
        /// <summary>
        /// 对地速度(0.1m/s)
        /// </summary>
        [MarshalAs(UnmanagedType.I2)]
        public short SpeedOverGround;
        /// <summary>
        /// 轨道对地良好[1/10°]?
        /// Track made good over ground [1/10 °]
        /// </summary>
        [MarshalAs(UnmanagedType.I2)]
        public short TrackMadeGood;
        /// <summary>
        /// 水平精度 ?或无效时为-1.0
        /// Position Dilution Of Precision or -1.0 when invalid
        /// </summary>
        [MarshalAs(UnmanagedType.R4)]
        public float PDOP;
        /// <summary>
        /// 垂直精度?或无效时为-1.0
        /// Vertical Dilution Of Precision or -1.0 when invalid
        /// </summary>
        [MarshalAs(UnmanagedType.R4)]
        public float VDOP;
        /// <summary>
        /// GPS时间刻度数(ns)
        /// </summary>
        [MarshalAs(UnmanagedType.U8)]
        public ulong GPSTimestamp;
        [MarshalAs(UnmanagedType.I4)]
        public int Reserved;
        /// <summary>
        /// 罗盘时间戳ns
        /// </summary>
        [MarshalAs(UnmanagedType.U8)]
        public ulong CompassTimestamp;
        /// <summary>
        /// 磁偏角来源 1-手动 2-GPS
        /// </summary>
        [MarshalAs(UnmanagedType.I2)]
        public short MagneticDeclinationSource;
        /// <summary>
        /// 磁偏角 磁偏角[1/10°]或-1无效
        /// </summary>
        [MarshalAs(UnmanagedType.I2)]
        public short MagneticDeclination;

        #endregion 属性

        public GPSCompassData(byte[] buffer, int startIndex)
        {
            int offset = startIndex;
            Array.Reverse(buffer, offset, 2);
            CompassHeading = BitConverter.ToUInt16(buffer, offset);
            offset += 2;
            Array.Reverse(buffer, offset, 2);
            CompassHeadingType = BitConverter.ToInt16(buffer, offset);
            offset += 2;
            Array.Reverse(buffer, offset, 2);
            GPSValid = BitConverter.ToInt16(buffer, offset);
            offset += 2;
            Array.Reverse(buffer, offset, 2);
            NoOfSatInView = BitConverter.ToInt16(buffer, offset);
            offset += 2;
            Array.Reverse(buffer, offset, 2);
            LatRef = BitConverter.ToInt16(buffer, offset);
            offset += 2;
            Array.Reverse(buffer, offset, 2);
            LatDeg = BitConverter.ToInt16(buffer, offset);
            offset += 2;
            Array.Reverse(buffer, offset, 4);
            LatMin = BitConverter.ToSingle(buffer, offset);
            offset += 4;
            Array.Reverse(buffer, offset, 2);
            LonRef = BitConverter.ToInt16(buffer, offset);
            offset += 2;
            Array.Reverse(buffer, offset, 2);
            LonDeg = BitConverter.ToInt16(buffer, offset);
            offset += 2;
            Array.Reverse(buffer, offset, 4);
            LonMin = BitConverter.ToSingle(buffer, offset);
            offset += 4;
            Array.Reverse(buffer, offset, 4);
            HDOP = BitConverter.ToSingle(buffer, offset);
            offset += 4;
            Array.Reverse(buffer, offset, 2);
            AntValid = BitConverter.ToInt16(buffer, offset);
            offset += 2;
            Array.Reverse(buffer, offset, 2);
            AntTiltOver = BitConverter.ToInt16(buffer, offset);
            offset += 2;
            Array.Reverse(buffer, offset, 2);
            AntElevation = BitConverter.ToInt16(buffer, offset);
            offset += 2;
            Array.Reverse(buffer, offset, 2);
            AntRoll = BitConverter.ToInt16(buffer, offset);
            offset += 2;
            Array.Reverse(buffer, offset, 2);
            SignalSource = BitConverter.ToInt16(buffer, offset);
            offset += 2;
            Array.Reverse(buffer, offset, 2);
            AngularRatesValid = BitConverter.ToInt16(buffer, offset);
            offset += 2;
            Array.Reverse(buffer, offset, 2);
            HeadingAngularRate = BitConverter.ToInt16(buffer, offset);
            offset += 2;
            Array.Reverse(buffer, offset, 2);
            ElevationAngularRate = BitConverter.ToInt16(buffer, offset);
            offset += 2;
            Array.Reverse(buffer, offset, 2);
            RollAngularRate = BitConverter.ToInt16(buffer, offset);
            offset += 2;
            Array.Reverse(buffer, offset, 2);
            GeoidalSeparationValid = BitConverter.ToInt16(buffer, offset);
            offset += 2;
            Array.Reverse(buffer, offset, 4);
            GeoidalSeparation = BitConverter.ToInt32(buffer, offset);
            offset += 4;
            Array.Reverse(buffer, offset, 4);
            Altitude = BitConverter.ToInt32(buffer, offset);
            offset += 4;
            Array.Reverse(buffer, offset, 2);
            SpeedOverGround = BitConverter.ToInt16(buffer, offset);
            offset += 2;
            Array.Reverse(buffer, offset, 2);
            TrackMadeGood = BitConverter.ToInt16(buffer, offset);
            offset += 2;
            Array.Reverse(buffer, offset, 4);
            PDOP = BitConverter.ToSingle(buffer, offset);
            offset += 4;
            Array.Reverse(buffer, offset, 4);
            VDOP = BitConverter.ToSingle(buffer, offset);
            offset += 4;
            Array.Reverse(buffer, offset, 8);
            GPSTimestamp = BitConverter.ToUInt64(buffer, offset);
            offset += 8;
            Array.Reverse(buffer, offset, 4);
            Reserved = BitConverter.ToInt32(buffer, offset);
            offset += 4;
            Array.Reverse(buffer, offset, 8);
            CompassTimestamp = BitConverter.ToUInt64(buffer, offset);
            offset += 8;
            Array.Reverse(buffer, offset, 2);
            MagneticDeclinationSource = BitConverter.ToInt16(buffer, offset);
            offset += 2;
            Array.Reverse(buffer, offset, 2);
            MagneticDeclination = BitConverter.ToInt16(buffer, offset);
            offset += 2;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public byte[] ToBytes()
        {
            int len = Marshal.SizeOf(this);
            int offset = 0;
            byte[] buffer = new byte[len];
            byte[] data = BitConverter.GetBytes(CompassHeading);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(CompassHeadingType);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(GPSValid);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(NoOfSatInView);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(LatRef);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(LatDeg);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(LatMin);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(LonRef);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(LonDeg);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(LonMin);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(HDOP);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(AntValid);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(AntTiltOver);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(AntElevation);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(AntRoll);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(SignalSource);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(AngularRatesValid);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(HeadingAngularRate);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(ElevationAngularRate);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(RollAngularRate);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(GeoidalSeparationValid);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(GeoidalSeparation);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(Altitude);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(SpeedOverGround);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(TrackMadeGood);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(PDOP);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(VDOP);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(GPSTimestamp);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(Reserved);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(CompassTimestamp);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(MagneticDeclinationSource);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(MagneticDeclination);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            return buffer;
        }
    }

    /// <summary>
    /// EB200数据体
    /// </summary>
    class EB200Data
    {
        public EB200Header EB200Header;
        public GenericAttribute GenericAttribute;
        public TraceAttribute TraceAttribute;
        public IOptionalHeader OptionalHeader;
        public byte[] Data;

        public TAGS Tag;

        private static object _lockSequenceNumUDP = new object();
        private static ushort _sequenceNumberUDP = 0;
        private static object _lockSequenceNumTCP = new object();
        private static ushort _sequenceNumberTCP = 0;

        public EB200Data()
        {
        }

        public EB200Data(TAGS tag, TraceAttribute traceAttribute, IOptionalHeader optionalHeader, byte[] data)
        {
            GenericAttribute = new GenericAttribute();
            GenericAttribute.Length = (ushort)(Marshal.SizeOf(traceAttribute) + Marshal.SizeOf(optionalHeader) + data.Length);
            GenericAttribute.Tag = (ushort)tag;

            OptionalHeader = optionalHeader;
            Data = data;
            EB200Header = new EB200Header();
            uint dataSize = (uint)(Marshal.SizeOf(EB200Header) + Marshal.SizeOf(GenericAttribute) + GenericAttribute.Length);
            EB200Header.MagicNumber = 0x000EB200;
            EB200Header.VersionMinor = 0x0061;
            EB200Header.VersionMajor = 0x0002;
            EB200Header.DataSize = dataSize;
            if (tag == TAGS.IF)
            {
                EB200Header.SequenceNumber = GetSequenceNoTCP();
            }
            else
            {
                EB200Header.SequenceNumber = GetSequenceNoUDP();
            }
        }

        public EB200Data(byte[] buffer, int offset)
        {
            EB200Header = new EB200Header(buffer, offset);
            offset += Marshal.SizeOf(EB200Header);
            GenericAttribute = new GenericAttribute(buffer, offset);
            offset += Marshal.SizeOf(GenericAttribute);
            TraceAttribute = new TraceAttribute(buffer, offset);
            offset += Marshal.SizeOf(TraceAttribute);
            Tag = (TAGS)GenericAttribute.Tag;
            switch (Tag)
            {
                case TAGS.IF:
                    OptionalHeader = new OptionalHeaderIF(buffer, offset);
                    break;
                case TAGS.GPSCompass:
                    OptionalHeader = new OptionalHeaderGPSCompass(buffer, offset);
                    break;
                case TAGS.CW:
                    OptionalHeader = new OptionalHeaderCW(buffer, offset);
                    break;
                case TAGS.IFPAN:
                    OptionalHeader = new OptionalHeaderIFPan(buffer, offset);
                    break;
                default:
                    break;
            }
            offset += Marshal.SizeOf(OptionalHeader);
            int len = GenericAttribute.Length - (Marshal.SizeOf(TraceAttribute) + Marshal.SizeOf(OptionalHeader));
            Data = new byte[len];
            Buffer.BlockCopy(buffer, offset, Data, 0, len);
            offset += len;
        }

        public byte[] ToBytes()
        {
            try
            {
                byte[] buffer = new byte[EB200Header.DataSize];
                byte[] src = EB200Header.ToBytes();
                int offset = 0;
                Buffer.BlockCopy(src, 0, buffer, offset, src.Length);
                offset += src.Length;
                src = GenericAttribute.ToBytes();
                Buffer.BlockCopy(src, 0, buffer, offset, src.Length);
                offset += src.Length;
                src = TraceAttribute.ToBytes();
                Buffer.BlockCopy(src, 0, buffer, offset, src.Length);
                offset += src.Length;
                src = OptionalHeader.ToBytes();
                Buffer.BlockCopy(src, 0, buffer, offset, src.Length);
                offset += src.Length;
                Buffer.BlockCopy(Data, 0, buffer, offset, Data.Length);
                return buffer;
            }
            catch
            {
                return null;
            }
        }

        // 获取自增长的UDP Sequence Number
        private static ushort GetSequenceNoUDP()
        {
            lock (_lockSequenceNumUDP)
            {
                if (_sequenceNumberUDP >= ushort.MaxValue)
                {
                    _sequenceNumberUDP = 0;
                }
                _sequenceNumberUDP++;
            }
            return _sequenceNumberUDP;
        }

        // 获取自增长的TCP Sequence Number
        private static ushort GetSequenceNoTCP()
        {
            lock (_lockSequenceNumTCP)
            {
                if (_sequenceNumberTCP >= ushort.MaxValue)
                {
                    _sequenceNumberTCP = 0;
                }
                _sequenceNumberTCP++;
            }
            return _sequenceNumberTCP;
        }
    }
}
