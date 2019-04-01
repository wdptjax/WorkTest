using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace ODM20181102TJ01
{
    /// <summary>
    /// 时间数据
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TimeInTwoWords
    {
        /// <summary>
        /// 时间Ticks低四位
        /// </summary>
        [MarshalAs(UnmanagedType.U4)]
        public uint LoOrderBits;

        /// <summary>
        /// 时间Ticks高四位
        /// </summary>
        [MarshalAs(UnmanagedType.U4)]
        public uint HiOrderBits;

        public byte[] ToBytes()
        {
            int len = Marshal.SizeOf(this);
            byte[] buffer = new byte[len];
            int index = 0;
            byte[] arr = BitConverter.GetBytes(LoOrderBits);
            Array.Reverse(arr);
            Buffer.BlockCopy(arr, 0, buffer, index, arr.Length);
            index += arr.Length;
            arr = BitConverter.GetBytes(HiOrderBits);
            Array.Reverse(arr);
            Buffer.BlockCopy(arr, 0, buffer, index, arr.Length);
            index += arr.Length;
            return buffer;
        }

    }

    //[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    //public struct BigTime
    //{
    //    public TimeInTwoWords TimeInTwoWords;

    //    /// <summary>
    //    /// 下面的数组只是一个“假数组”，永远不应该被插入。它的用途是强制编译器将上述结构的两个成员保存在相邻的内存单词中。
    //    /// </summary>
    //    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2, ArraySubType = UnmanagedType.U4)]
    //    public uint[] TimeAsArray;

    //    public byte[] ToBytes()
    //    {
    //        int len = Marshal.SizeOf(this);
    //        byte[] buffer = new byte[len];
    //        int index = 0;
    //        byte[] arr = TimeInTwoWords.ToBytes();
    //        Buffer.BlockCopy(arr, 0, buffer, index, arr.Length);
    //        index += arr.Length;
    //        for (int i = 0; i < 2; i++)
    //        {
    //            if (TimeAsArray.Length > i)
    //            {
    //                arr = BitConverter.GetBytes(TimeAsArray[i]);
    //                Array.Reverse(arr);
    //                Buffer.BlockCopy(arr, 0, buffer, index, arr.Length);
    //                index += arr.Length;
    //            }
    //        }
    //        return buffer;
    //    }
    //}

    #region FFMData

    /// <summary>
    /// FFM数据状态信息
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct FfmStatus
    {
        [MarshalAs(UnmanagedType.U4)]
        private uint _value;

        /// <summary>
        /// 预留 16位
        /// </summary>
        public uint ReservedH
        {
            get { return (_value & 0xFFFF0000) >> 16; }
            set { _value = (_value & 0x0000FFFF) | ((value & 0xFFFF) << 16); }
        }

        /// <summary>
        /// 标识平均信号是否小于阈值电平
        /// </summary>
        public uint SignalNone
        {
            get { return (_value & 0x8000) >> 15; }
            set { _value = (_value & 0xFFFF7FFF) | ((value & 0x01) << 15); }
        }

        /// <summary>
        /// 平均信号
        /// </summary>
        public uint SignalAverage
        {
            get { return (_value & 0x4000) >> 14; }
            set { _value = (_value & 0xFFFFBFFF) | ((value & 0x01) << 14); }
        }

        /// <summary>
        /// 标志信号结束（下降沿）
        /// </summary>
        public uint SignalEnd
        {
            get { return (_value & 0x2000) >> 13; }
            set { _value = (_value & 0xFFFBFFFF) | ((value & 0x01) << 13); }
        }

        /// <summary>
        /// 标识信号开始（上升沿）
        /// </summary>
        public uint SignalStart
        {
            get { return (_value & 0x1000) >> 12; }
            set { _value = (_value & 0xFFFFEFFF) | ((value & 0x01) << 12); }
        }

        /// <summary>
        /// 对应枚举DfAlt 测向体制
        /// </summary>
        public uint DfAlt
        {
            get { return (_value & 0xF00) >> 8; }
            set { _value = (_value & 0xFFFFF0FF) | ((value & 0x0F) << 8); }
        }

        /// <summary>
        /// 对应枚举AntPol 天线极化方式 默认为ANT_AUTO
        /// </summary>
        public uint AntPol
        {
            get { return (_value & 0xC0) >> 6; }
            set { _value = (_value & 0xFFFFFF3F) | ((value & 0x03) << 6); }
        }

        /// <summary>
        /// 预留
        /// </summary>
        public uint Reserved
        {
            get { return (_value & 0x30) >> 4; }
            set { _value = (_value & 0xFFFFFFCF) | ((value & 0x03) << 4); }
        }

        /// <summary>
        /// 
        /// </summary>
        public uint Elevation
        {
            get { return (_value & 0x08) >> 3; }
            set { _value = (_value & 0xFFFFFFF7) | ((value & 0x01) << 3); }
        }

        /// <summary>
        /// 信号电平过低
        /// </summary>
        public uint Squelch
        {
            get { return (_value & 0x04) >> 2; }
            set { _value = (_value & 0xFFFFFFFB) | ((value & 0x01) << 2); }
        }

        /// <summary>
        /// 数据块由于adc溢出而损坏
        /// </summary>
        public uint Overflow
        {
            get { return (_value & 0x02) >> 1; }
            set { _value = (_value & 0xFFFFFFFD) | ((value & 0x01) << 1); }
        }

        /// <summary>
        /// 标志此数据包有效
        /// </summary>
        public uint Valid
        {
            get { return (_value & 0x01); }
            set { _value = (_value & 0xFFFFFFFE) | (value & 0x01); }
        }

        /// <summary>
        /// 将本数据体转换为字节数组
        /// </summary>
        /// <returns></returns>
        public byte[] ToBytes()
        {
            int len = Marshal.SizeOf(this);
            byte[] buffer = new byte[len];
            byte[] arr = BitConverter.GetBytes(_value);
            int index = 0;
            Array.Reverse(arr);
            Buffer.BlockCopy(arr, 0, buffer, index, arr.Length);
            return buffer;
        }
    }

    /// <summary>
    /// FFM数据
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct FfmData
    {
        /// <summary>
        /// 起始时间
        /// </summary>
        public TimeInTwoWords BigTime;

        /// <summary>
        /// FFM数据的状态信息
        /// </summary>
        public FfmStatus FfmStatus;

        /// <summary>
        /// 任务的唯一标识符
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public int JobId;

        /// <summary>
        /// 测量的通道
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public int Channel;

        /// <summary>
        /// 罗盘方位 1/100 °
        /// </summary>
        [MarshalAs(UnmanagedType.U4)]
        public uint AzimCompass;

        /// <summary>
        /// 电平值 1/100 dBμV
        /// </summary>
        [MarshalAs(UnmanagedType.I2)]
        public short Level;

        /// <summary>
        /// 场强值 1/100 dBμV
        /// </summary>
        [MarshalAs(UnmanagedType.I2)]
        public short FieldStrength;

        /// <summary>
        /// 测向方位角 1/100 °
        /// </summary>
        [MarshalAs(UnmanagedType.U2)]
        public ushort Azimuth;

        /// <summary>
        /// 倾斜角 1/100 °
        /// </summary>
        [MarshalAs(UnmanagedType.U2)]
        public ushort Elevation;

        /// <summary>
        /// 方位角变化值 1/100
        /// </summary>
        [MarshalAs(UnmanagedType.I2)]
        public short AzimuthVar;

        /// <summary>
        /// 测向质量 1-100 %
        /// 0：预留，如测向结果无效
        /// </summary>
        [MarshalAs(UnmanagedType.I2)]
        public short Quality;

        public byte[] ToBytes()
        {
            int len = Marshal.SizeOf(this);
            byte[] buffer = new byte[len];
            int index = 0;
            byte[] arr = BigTime.ToBytes();
            Buffer.BlockCopy(arr, 0, buffer, index, arr.Length);
            index += arr.Length;
            arr = FfmStatus.ToBytes();
            Buffer.BlockCopy(arr, 0, buffer, index, arr.Length);
            index += arr.Length;
            arr = BitConverter.GetBytes(JobId);
            Array.Reverse(arr);
            Buffer.BlockCopy(arr, 0, buffer, index, arr.Length);
            index += arr.Length;
            arr = BitConverter.GetBytes(Channel);
            Array.Reverse(arr);
            Buffer.BlockCopy(arr, 0, buffer, index, arr.Length);
            index += arr.Length;
            arr = BitConverter.GetBytes(AzimCompass);
            Array.Reverse(arr);
            Buffer.BlockCopy(arr, 0, buffer, index, arr.Length);
            index += arr.Length;
            arr = BitConverter.GetBytes(Level);
            Array.Reverse(arr);
            Buffer.BlockCopy(arr, 0, buffer, index, arr.Length);
            index += arr.Length;
            arr = BitConverter.GetBytes(FieldStrength);
            Array.Reverse(arr);
            Buffer.BlockCopy(arr, 0, buffer, index, arr.Length);
            index += arr.Length;
            arr = BitConverter.GetBytes(Azimuth);
            Array.Reverse(arr);
            Buffer.BlockCopy(arr, 0, buffer, index, arr.Length);
            index += arr.Length;
            arr = BitConverter.GetBytes(Elevation);
            Array.Reverse(arr);
            Buffer.BlockCopy(arr, 0, buffer, index, arr.Length);
            index += arr.Length;
            arr = BitConverter.GetBytes(AzimuthVar);
            Array.Reverse(arr);
            Buffer.BlockCopy(arr, 0, buffer, index, arr.Length);
            index += arr.Length;
            arr = BitConverter.GetBytes(Quality);
            Array.Reverse(arr);
            Buffer.BlockCopy(arr, 0, buffer, index, arr.Length);
            index += arr.Length;
            return buffer;
        }
    }

    #endregion FFMData

    #region SpectrumData

    /// <summary>
    /// 频谱数据状态信息
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct IfStatus
    {
        [MarshalAs(UnmanagedType.U4)]
        private uint _value;

        /// <summary>
        /// 预留
        /// </summary>
        public uint Reserved
        {
            get { return (_value & 0xC0000000) >> 30; }
            set { _value = (_value & 0x3FFFFFFF) | ((value & 0x03) << 30); }
        }

        /// <summary>
        /// 实时带宽 Hz
        /// </summary>
        public uint RealTimeBandwidth
        {
            get { return (_value & 0x3FFFFFFC) >> 2; }
            set { _value = (_value & 0xC0000003) | ((value & 0x0FFFFFFF) << 2); }
        }

        /// <summary>
        /// 数据块由于adc溢出而损坏
        /// </summary>
        public uint Overflow
        {
            get { return (_value & 0x02) >> 1; }
            set { _value = (_value & 0xFFFFFFFD) | ((value & 0x01) << 1); }
        }

        /// <summary>
        /// 标识数据包有效
        /// </summary>
        public uint Valid
        {
            get { return (_value & 0x01); }
            set { _value = (_value & 0xFFFFFFFE) | (value & 0x01); }
        }

        public byte[] ToBytes()
        {
            int len = Marshal.SizeOf(this);
            byte[] buffer = new byte[len];
            byte[] arr = BitConverter.GetBytes(_value);
            //Array.Reverse(arr);
            int index = 0;
            Buffer.BlockCopy(arr, 0, buffer, index, arr.Length);
            return buffer;
        }

        public void SetValue(uint value)
        {
            _value = value;
        }
    }

    /// <summary>
    /// 频谱数据
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct IfData
    {
        /// <summary>
        /// 1/100 dbμV
        /// </summary>
        [MarshalAs(UnmanagedType.I2)]
        public short D1;
        /// <summary>
        /// 1/100 dbμV
        /// </summary>
        [MarshalAs(UnmanagedType.I2)]
        public short D0;

        public byte[] ToBytes()
        {
            int len = Marshal.SizeOf(this);
            byte[] buffer = new byte[len];
            int index = 0;
            byte[] arr = BitConverter.GetBytes(D1);
            Array.Reverse(arr);
            Buffer.BlockCopy(arr, 0, buffer, index, arr.Length);
            index += arr.Length;
            arr = BitConverter.GetBytes(D0);
            Array.Reverse(arr);
            Buffer.BlockCopy(arr, 0, buffer, index, arr.Length);
            index += arr.Length;
            return buffer;
        }
    }

    /// <summary>
    /// 频谱数据结构体
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct IfSpectrumData
    {
        private const int MAX_IFSPECTRUM_DATA = 201;

        /// <summary>
        /// 频谱数据产生的时间
        /// </summary>
        public TimeInTwoWords BigTime;

        /// <summary>
        /// 状态码
        /// </summary>
        public IfStatus IfStatus;

        /// <summary>
        /// 任务唯一ID
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public int JobId;

        /// <summary>
        /// 本次数据包在总的位置
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public int Count;

        /// <summary>
        /// 数据包
        /// </summary>
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = (MAX_IFSPECTRUM_DATA + 1) / 2, ArraySubType = UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(IfData))]
        public IfData[] IfDatas;

        public byte[] ToBytes()
        {
            int ifDataLen = 0;
            if (IfDatas != null)
            {
                ifDataLen = Marshal.SizeOf(typeof(IfData)) * IfDatas.Length;
            }
            int len = Marshal.SizeOf(BigTime) + Marshal.SizeOf(IfStatus) + 4 + 4 + ifDataLen;
            byte[] buffer = new byte[len];
            int index = 0;
            byte[] arr = BigTime.ToBytes();
            Buffer.BlockCopy(arr, 0, buffer, index, arr.Length);
            index += arr.Length;
            arr = IfStatus.ToBytes();
            Array.Reverse(arr);
            Buffer.BlockCopy(arr, 0, buffer, index, arr.Length);
            index += arr.Length;
            arr = BitConverter.GetBytes(JobId);
            Array.Reverse(arr);
            Buffer.BlockCopy(arr, 0, buffer, index, arr.Length);
            index += arr.Length;
            arr = BitConverter.GetBytes(Count);
            Array.Reverse(arr);
            Buffer.BlockCopy(arr, 0, buffer, index, arr.Length);
            index += arr.Length;
            if (IfDatas != null)
            {
                for (int i = 0; i < IfDatas.Length; i++)
                {
                    if (i >= (MAX_IFSPECTRUM_DATA + 1) / 2)
                    {
                        break;
                    }
                    arr = IfDatas[i].ToBytes();
                    Buffer.BlockCopy(arr, 0, buffer, index, arr.Length);
                    index += arr.Length;
                }
            }

            return buffer;
        }
    }

    #endregion SpectrumData

    #region ScanData

    /// <summary>
    /// 扫描数据状态信息
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct ScanStatus
    {
        [MarshalAs(UnmanagedType.U4)]
        private uint _value;

        /// <summary>
        /// 预留
        /// </summary>
        public uint ReservedH
        {
            get { return (_value & 0xFFFF0000) >> 16; }
            set { _value = (_value & 0x0000FFFF) | ((value & 0xFFFF) << 16); }
        }

        /// <summary>
        /// averaged signal < threshold level   
        /// </summary>
        public uint SignalNone
        {
            get { return (_value & 0x8000) >> 15; }
            set { _value = (_value & 0xFFFF7FFF) | ((value & 0x01) << 15); }
        }

        /// <summary>
        /// averaged signal     
        /// </summary>
        public uint SignalAverage
        {
            get { return (_value & 0x4000) >> 14; }
            set { _value = (_value & 0xFFFFBFFF) | ((value & 0x01) << 14); }
        }

        /// <summary>
        /// falling edge
        /// </summary>
        public uint SignalEnd
        {
            get { return (_value & 0x2000) >> 13; }
            set { _value = (_value & 0xFFFFDFFF) | ((value & 0x01) << 13); }
        }

        /// <summary>
        /// rising edge
        /// </summary>
        public uint SignalStart
        {
            get { return (_value & 0x1000) >> 12; }
            set { _value = (_value & 0xFFFFEFFF) | ((value & 0x01) << 12); }
        }

        /// <summary>
        /// see "enumDfAlt" 
        /// </summary>
        public uint DfAlt
        {
            get { return (_value & 0xF00) >> 8; }
            set { _value = (_value & 0xFFFFF0FF) | ((value & 0x0F) << 8); }
        }

        /// <summary>
        /// see "enumAntPol w/o ANT_AUTO"
        /// </summary>
        public uint AntPol
        {
            get { return (_value & 0xC0) >> 6; }
            set { _value = (_value & 0xFFFFFF3F) | ((value & 0x03) << 6); }
        }

        /// <summary>
        /// 预留
        /// </summary>
        public uint Reserved
        {
            get { return (_value & 0x30) >> 4; }
            set { _value = (_value & 0xFFFFFFCF) | ((value & 0x03) << 4); }
        }
        /// <summary>
        /// 倾斜角度是否有效
        /// </summary>
        public uint Elevation
        {
            get { return (_value & 0x08) >> 3; }
            set { _value = (_value & 0xFFFFFFF7) | ((value & 0x01) << 3); }
        }

        /// <summary>
        /// 信号电平过低
        /// </summary>
        public uint Squelch
        {
            get { return (_value & 0x04) >> 2; }
            set { _value = (_value & 0xFFFFFFFB) | ((value & 0x01) << 2); }
        }

        /// <summary>
        /// 数据溢出
        /// </summary>
        public uint Overflow
        {
            get { return (_value & 0x02) >> 1; }
            set { _value = (_value & 0xFFFFFFFD) | ((value & 0x01) << 1); }
        }

        /// <summary>
        /// 数据有效
        /// </summary>
        public uint Valid
        {
            get { return (_value & 0x01); }
            set { _value = (_value & 0xFFFFFFFE) | (value & 0x01); }
        }

        public byte[] ToBytes()
        {
            int len = Marshal.SizeOf(this);
            byte[] buffer = new byte[len];
            byte[] arr = BitConverter.GetBytes(_value);
            //Array.Reverse(arr);
            int index = 0;
            Buffer.BlockCopy(arr, 0, buffer, index, arr.Length);
            return buffer;
        }

        public void SetValue(uint value)
        {
            _value = value;
        }
    }

    /// <summary>
    /// 扫描通道信息
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct ScanChannel
    {
        /// <summary>
        /// 从通道扫描开始(跳数乘以跳的持续时间)到预期信号出现的时间
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public int TimeOffset;

        /// <summary>
        /// 通道的信号发射持续时间
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public int SignalDuration;

        /// <summary>
        /// 测向电平 1/100 dBμV
        /// </summary>
        [MarshalAs(UnmanagedType.I2)]
        public short Level;

        /// <summary>
        /// 示向度 1/100 °  0-35999
        /// </summary>
        [MarshalAs(UnmanagedType.U2)]
        public ushort Azimuth;

        /// <summary>
        /// 倾斜度 1/100 ° 0-9000
        /// </summary>
        [MarshalAs(UnmanagedType.I2)]
        public short Elevation;

        /// <summary>
        /// 测向质量 1-100 % 
        /// 0：预留，如测向结果无效
        /// </summary>
        [MarshalAs(UnmanagedType.I2)]
        public short Quality;

        public byte[] ToBytes()
        {
            int len = Marshal.SizeOf(this);
            byte[] buffer = new byte[len];
            int index = 0;
            byte[] arr = BitConverter.GetBytes(TimeOffset);
            Array.Reverse(arr);
            Buffer.BlockCopy(arr, 0, buffer, index, arr.Length);
            index += arr.Length;
            arr = BitConverter.GetBytes(SignalDuration);
            Array.Reverse(arr);
            Buffer.BlockCopy(arr, 0, buffer, index, arr.Length);
            index += arr.Length;
            arr = BitConverter.GetBytes(Level);
            Array.Reverse(arr);
            Buffer.BlockCopy(arr, 0, buffer, index, arr.Length);
            index += arr.Length;
            arr = BitConverter.GetBytes(Azimuth);
            Array.Reverse(arr);
            Buffer.BlockCopy(arr, 0, buffer, index, arr.Length);
            index += arr.Length;
            arr = BitConverter.GetBytes(Elevation);
            Array.Reverse(arr);
            Buffer.BlockCopy(arr, 0, buffer, index, arr.Length);
            index += arr.Length;
            arr = BitConverter.GetBytes(Quality);
            Array.Reverse(arr);
            Buffer.BlockCopy(arr, 0, buffer, index, arr.Length);
            index += arr.Length;
            return buffer;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct ScanData
    {
        private const int MAX_CHANNEL_PER_SUBHOP = 500;

        /// <summary>
        /// 从第一次计算FFT开始到本次数据的绝对时间
        /// </summary>
        public TimeInTwoWords BigTime;

        /// <summary>
        /// 测向状态数据
        /// </summary>
        public ScanStatus ScanStatus;
        [MarshalAs(UnmanagedType.I4)]
        public int JobId;

        /// <summary>
        /// 罗盘角度 1/100 °
        /// </summary>
        [MarshalAs(UnmanagedType.U4)]
        public uint AzimCompass;

        /// <summary>
        /// 本包数据的第一个通道在总扫描数据的位置
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public int FirstChannel;

        /// <summary>
        /// 扫描数据的总通道数
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public int Count;
        /// <summary>
        /// 数据集合 最大500条数据
        /// </summary>
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_CHANNEL_PER_SUBHOP, ArraySubType = UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ScanChannel))]
        public ScanChannel[] MeasChan;

        public byte[] ToBytes()
        {
            int scanDataLen = 0;
            if (MeasChan != null)
            {
                scanDataLen = Marshal.SizeOf(typeof(ScanChannel)) * MeasChan.Length;
            }
            int len = Marshal.SizeOf(BigTime) + Marshal.SizeOf(ScanStatus) + 4 * 4 + scanDataLen;
            byte[] buffer = new byte[len];
            int index = 0;
            byte[] arr = BigTime.ToBytes();
            Buffer.BlockCopy(arr, 0, buffer, index, arr.Length);
            index += arr.Length;
            arr = ScanStatus.ToBytes();
            Array.Reverse(arr);
            Buffer.BlockCopy(arr, 0, buffer, index, arr.Length);
            index += arr.Length;
            arr = BitConverter.GetBytes(JobId);
            Array.Reverse(arr);
            Buffer.BlockCopy(arr, 0, buffer, index, arr.Length);
            index += arr.Length;
            arr = BitConverter.GetBytes(AzimCompass);
            Array.Reverse(arr);
            Buffer.BlockCopy(arr, 0, buffer, index, arr.Length);
            index += arr.Length;
            arr = BitConverter.GetBytes(FirstChannel);
            Array.Reverse(arr);
            Buffer.BlockCopy(arr, 0, buffer, index, arr.Length);
            index += arr.Length;
            arr = BitConverter.GetBytes(Count);
            Array.Reverse(arr);
            Buffer.BlockCopy(arr, 0, buffer, index, arr.Length);
            index += arr.Length;
            if (MeasChan != null)
            {
                for (int i = 0; i < MeasChan.Length; i++)
                {
                    if (i >= MAX_CHANNEL_PER_SUBHOP)
                    {
                        break;
                    }
                    arr = MeasChan[i].ToBytes();
                    Buffer.BlockCopy(arr, 0, buffer, index, arr.Length);
                    index += arr.Length;
                }
            }

            return buffer;
        }
    }

    #endregion ScanData

    #region GpsData

    /// <summary>
    /// GPS数据状态信息
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct GpsStatus
    {

        [MarshalAs(UnmanagedType.U4)]
        private uint _value;

        /// <summary>
        /// 预留
        /// </summary>
        public uint Reserved
        {
            get { return ((_value & 0xFFFFFFFE) >> 1); }
            set { _value = (_value & 0x01) | ((value & 0x7FFFFFFF) << 1); }
        }

        /// <summary>
        /// 数据是否有效
        /// </summary>
        public uint Valid
        {
            get { return ((_value & 0x01)); }
            set { _value = (_value & 0xFFFFFFFE) | ((value & 0x01)); }
        }

        public byte[] ToBytes()
        {
            int len = Marshal.SizeOf(this);
            byte[] buffer = new byte[len];
            byte[] arr = BitConverter.GetBytes(_value);
            //Array.Reverse(arr);
            int index = 0;
            Buffer.BlockCopy(arr, 0, buffer, index, arr.Length);
            return buffer;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct GpsData
    {
        public TimeInTwoWords BigTime;
        public GpsStatus GpsStatus;
        /// <summary>
        /// 接收到卫星的数量0-12
        /// 仅当收到GGA msg时有效，否则0xFFFF (DDF_UNDEFINED)
        /// </summary>
        [MarshalAs(UnmanagedType.U2)]
        public ushort NoOfSatInView;
        /// <summary>
        /// 水平精度 * 100 (50(最好). 9999(最差));
        /// 仅当收到GGA msg时有效，否则0xFFFF(DDF_UNDEFINED)
        /// </summary>
        [MarshalAs(UnmanagedType.U2)]
        public ushort HorDilution;
        /// <summary>
        /// 地理纬度 分′
        /// </summary>
        [MarshalAs(UnmanagedType.R4)]
        public float LatMin;
        /// <summary>
        /// 地理经度 分′
        /// </summary>
        [MarshalAs(UnmanagedType.R4)]
        public float LonMin;
        /// <summary>
        /// 经度方向"E"或"W"
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public byte LonRef;
        /// <summary>
        /// 纬度方向"N"或"S"
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public byte LatRef;
        /// <summary>
        /// 经度度数 0-180°
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public byte LonDeg;
        /// <summary>
        /// 纬度度数 0-90°
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public byte LatDeg;
        [MarshalAs(UnmanagedType.U1)]
        public byte Hour;
        [MarshalAs(UnmanagedType.U1)]
        public byte Day;
        [MarshalAs(UnmanagedType.U1)]
        public byte Month;
        [MarshalAs(UnmanagedType.U1)]
        public byte Year;
        [MarshalAs(UnmanagedType.U2)]
        public ushort MSec;
        [MarshalAs(UnmanagedType.U1)]
        public byte Sec;
        [MarshalAs(UnmanagedType.U1)]
        public byte Min;

        /// <summary>
        /// 设置本包数据的GPS时间
        /// 这个时间可能与BigTime不同？
        /// </summary>
        /// <param name="gpsTime"></param>
        /// <returns></returns>
        public void SetGpsTime(DateTime gpsTime)
        {
            DateTime time = gpsTime.ToUniversalTime();
            Year = (byte)(time.Year % 100);
            Month = (byte)time.Month;
            Day = (byte)time.Day;
            Hour = (byte)time.Hour;
            Min = (byte)time.Minute;
            Sec = (byte)time.Second;
            MSec = (ushort)time.Millisecond;
        }

        public byte[] ToBytes()
        {
            int len = Marshal.SizeOf(this);
            byte[] buffer = new byte[len];
            int index = 0;
            byte[] arr = BigTime.ToBytes();
            Buffer.BlockCopy(arr, 0, buffer, index, arr.Length);
            index += arr.Length;
            arr = GpsStatus.ToBytes();
            Buffer.BlockCopy(arr, 0, buffer, index, arr.Length);
            index += arr.Length;
            arr = BitConverter.GetBytes(NoOfSatInView);
            Array.Reverse(arr);
            Buffer.BlockCopy(arr, 0, buffer, index, arr.Length);
            index += arr.Length;
            arr = BitConverter.GetBytes(HorDilution);
            Array.Reverse(arr);
            Buffer.BlockCopy(arr, 0, buffer, index, arr.Length);
            index += arr.Length;
            arr = BitConverter.GetBytes(LatMin);
            Array.Reverse(arr);
            Buffer.BlockCopy(arr, 0, buffer, index, arr.Length);
            index += arr.Length;
            arr = BitConverter.GetBytes(LonMin);
            Array.Reverse(arr);
            Buffer.BlockCopy(arr, 0, buffer, index, arr.Length);
            index += arr.Length;
            buffer[index++] = LonRef;
            buffer[index++] = LatRef;
            buffer[index++] = LonDeg;
            buffer[index++] = LatDeg;
            buffer[index++] = Hour;
            buffer[index++] = Day;
            buffer[index++] = Month;
            buffer[index++] = Year;

            arr = BitConverter.GetBytes(MSec);
            Array.Reverse(arr);
            Buffer.BlockCopy(arr, 0, buffer, index, arr.Length);
            index += arr.Length;
            buffer[index++] = Sec;
            buffer[index++] = Min;

            return buffer;
        }
    }

    #endregion GpsData

    #region EbdMsg

    /// <summary>
    /// 发送到C++端的数据包
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct EbdMsg
    {
        [MarshalAs(UnmanagedType.U4)]
        private uint _msgLength;

        /// <summary>
        /// 消息ID，对应枚举DataOut
        /// </summary>
        [MarshalAs(UnmanagedType.U4)]
        private uint _msgId;

        /// <summary>
        /// 已经序列化好的数据体
        /// 包括GpsData、ScanData、IfSpectrumData、FfmData等
        /// </summary>
        public byte[] Datas;

        /// <summary>
        /// 发送的消息类型
        /// </summary>
        public EDataOut MessageType
        {
            get
            {
                _msgId = _msgId < (uint)EDataOut.AUDIO_DATA ? (uint)EDataOut.AUDIO_DATA : _msgId;
                _msgId = _msgId > (uint)EDataOut.MAX_DATA_OUT ? (uint)EDataOut.MAX_DATA_OUT : _msgId;
                return (EDataOut)_msgId;
            }
            set { _msgId = (uint)value; }
        }

        /// <summary>
        /// 将数据序列化为字节数组
        /// </summary>
        /// <returns></returns>
        public byte[] ToBytes()
        {
            if (Datas == null)
            {
                return null;
            }
            _msgLength = 4 + (uint)Datas.Length;
            int len = 4 + (int)_msgLength;
            byte[] buffer = new byte[len];
            int index = 0;
            byte[] arr = BitConverter.GetBytes(_msgLength);
            Array.Reverse(arr);
            Buffer.BlockCopy(arr, 0, buffer, index, arr.Length);
            index += arr.Length;
            arr = BitConverter.GetBytes(_msgId);
            Array.Reverse(arr);
            Buffer.BlockCopy(arr, 0, buffer, index, arr.Length);
            index += arr.Length;
            Buffer.BlockCopy(Datas, 0, buffer, index, Datas.Length);
            return buffer;
        }
    }

    #endregion EbdMsg
}
