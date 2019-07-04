/*********************************************************************************************
 *	
 * 文件名称:    ..\Tracker800\Server\Source\Device\Receiver\MR3300A\MR3000AProtocol.cs
 *
 * 作    者:		陈鹏 
 *	
 * 创作日期:    2018/05/17
 * 
 * 修    改:    无
 * 
 * 备    注:		MR3000A系列接收机协议
 *                                            
*********************************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MR3300A
{
    #region 枚举/类定义

    /// <summary>
    /// 业务数据类型
    /// </summary>
    [Flags]
    internal enum DataType
    {
        /// <summary>
        /// 未知数据
        /// </summary>
        None = 0,
        /// <summary>
        /// IQ数据
        /// </summary>
        IQ = 1,
        /// <summary>
        /// 电平数据
        /// </summary>
        LEVEL = IQ << 1,
        /// <summary>
        /// 频谱数据
        /// </summary>
        SPECTRUM = LEVEL << 1,
        /// <summary>
        /// 测向数据
        /// </summary>
        DFIND = SPECTRUM << 1,
        /// <summary>
        /// 宽带测向数据
        /// </summary>
        DFPAN = DFIND << 1,
        /// <summary>
        /// 音频数据
        /// </summary>
        AUDIO = DFPAN << 1,
        /// <summary>
        /// ITU数据
        /// </summary>
        ITU = AUDIO << 1,
        /// <summary>
        /// 扫描数据
        /// </summary>
        SCAN = ITU << 1,
        /// <summary>
        /// TDOA数据
        /// </summary>
        TDOA = SCAN << 1,
        /// <summary>
        /// 测向原始数据
        /// </summary>
        DFIQ = TDOA << 1,
        /// <summary>
        /// 短信数据（DMR/dPMR解调）
        /// </summary>
        SMS = DFIQ << 1,
        /// <summary>
        /// DDC数据，包含电平，频谱等数据
        /// </summary>
        DDC = SMS << 1,
        /// <summary>
        /// GPS数据
        /// </summary>
        GPS = DDC << 1,
        /// <summary>
        /// 测向特征数据
        /// </summary>
        DFC = GPS << 1,
        /// <summary>
        /// 罗盘数据
        /// </summary>
        Compass = DFC << 1
    }

    #endregion

    #region 数据协议

    /// <summary>
    /// 协议头数据
    /// </summary>
    [Serializable]
    internal class RawPacket
    {
        // 协议版本号
        public int Version { get; private set; }
        // RawData总数
        public int Count { get; private set; }
        // 本报数据中数据包的个数
        public List<RawData> DataCollection { get; private set; }
        // 数据解析
        public static RawPacket Parse(byte[] value, int offset)
        {
            var packet = new RawPacket();
            packet.Version = BitConverter.ToInt32(value, offset);
            offset += 4;
            packet.Count = BitConverter.ToInt32(value, offset);
            offset += 4;
            packet.DataCollection = new List<RawData>();
            for (var index = 0; index < packet.Count && offset < value.Length; index++)
            {
                var data = RawData.Parse(value, offset, packet.Version);
                if (data != null)
                {
                    packet.DataCollection.Add(data);
                    offset += (int)data.Size;
                }
            }

            return packet;
        }
    }

    /// <summary>
    /// 数据基类
    /// </summary>
    [Serializable]
    internal class RawData
    {
        private int _packetVersion;// 包版本号

        /// <summary>
        /// 数据类型，参见DataType中的定义
        /// </summary>
        public int Tag { get; private set; }
        /// <summary>
        /// 数据结构后期可能作更改，留作备用
        /// </summary>
        public int Version { get; private set; }
        /// <summary>
        /// 通道数量，主要用于多路
        /// </summary>
        public byte DDCCount { get; private set; }
        /// <summary>
        /// 通道编号，主要用于多路
        /// </summary>
        public byte DDCIndex { get; private set; }
        /// <summary>
        /// 通道状态，暂未使用
        /// </summary>
        public short State { get; private set; }

        /// <summary>
        /// 精确到秒的时间戳
        /// </summary>
        public int TimestampSecond { get; private set; }

        /// <summary>
        /// 精确到纳秒的时间戳
        /// </summary>
        public int TimestampNanoSecond { get; private set; }

        /// <summary>
        /// 数据长度（包含RawData中的所有字段）
        /// </summary>
        public int Size { get; private set; }

        protected RawData(int packetVersion)
        {
            _packetVersion = packetVersion;
        }

        public static RawData Parse(byte[] value, int offset, int packetVersion = 0)
        {
            RawData raw = null;
            var tag = (DataType)BitConverter.ToUInt32(value, offset);
            switch (tag)
            {
                case DataType.IQ:
                case DataType.TDOA:
                    raw = new RawIQ(packetVersion);
                    break;
                case DataType.LEVEL:
                    raw = new RawLevel(packetVersion);
                    break;
                case DataType.SPECTRUM:
                    raw = new RawSpectrum(packetVersion);
                    break;
                case DataType.AUDIO:
                    {
                        var version = BitConverter.ToUInt32(value, offset + 4);
                        if (version == 1)
                        {
                            raw = new RawDDCAudio(packetVersion);
                        }
                        else
                        {
                            raw = new RawAudio(packetVersion);
                        }
                    }
                    break;
                case DataType.ITU:
                    raw = new RawITU(packetVersion);
                    break;
                case DataType.SCAN: // PSCAN
                    {
                        var version = BitConverter.ToUInt32(value, offset + 4);
                        if (version == 40)
                        {
                            raw = new RawScan(packetVersion);
                        }
                        else
                        {
                            raw = new RawFastScan(packetVersion);
                        }
                    }
                    break;
                case DataType.SCAN + 2: // FSCAN
                case DataType.SCAN + 4: // MSCAN
                    raw = new RawScan(packetVersion);
                    break;
                case DataType.SMS:
                    raw = new RawSMS(packetVersion);
                    break;
                case DataType.DDC:
                    raw = new RawDDC(packetVersion);
                    break;
                case DataType.GPS:
                    raw = new RawGPS(packetVersion);
                    break;
                case DataType.Compass:
                    raw = new RawCompass(packetVersion);
                    break;
                case DataType.DFIND:
                case DataType.DFPAN:
                case DataType.DFIQ:
                case DataType.DFC:  // 单频测向特征值
                case DataType.DFC + 4: // 宽带测向特征值
                default:
                    break;
            }

            if (raw == null)
            {
                return null;
            }

            //具体类型数据转换
            raw.Convert(value, offset);

            return raw;
        }

        //子类中需要重写该函数，并且需要先调用此函数
        public virtual int Convert(byte[] value, int offset)
        {
            Tag = BitConverter.ToInt32(value, offset);
            offset += 4;
            Version = BitConverter.ToInt32(value, offset);
            offset += 4;
            if (_packetVersion < 2)
            {
                DDCCount = value[offset];
                offset += 1;
                DDCIndex = value[offset];
                offset += 1;
                State = BitConverter.ToInt16(value, offset);
                offset += 2;
            }
            else
            {
                TimestampSecond = BitConverter.ToInt32(value, offset);
                offset += 4;
                TimestampNanoSecond = BitConverter.ToInt32(value, offset);
                offset += 4;
            }
            Size = BitConverter.ToInt32(value, offset);
            offset += 4;

            return offset;
        }

        public override string ToString()
        {
            //return string.Format("tag={0}, ver={1}, ddc_cnt={2}, ddc_idx={3}", Tag, Version, DDCCount, DDCIndex);
            return string.Format("tag=\"{0}\", ver={1}", ((DataType)Tag).ToString().Replace(", ", "|"), Version);
        }
    }

    /// <summary>
    /// I/Q数据
    /// </summary>
    [Serializable]
    internal class RawIQ : RawData
    {
        // 频率，单位Hz
        public long Frequency { get; private set; }

        // 带宽，单位Hz
        public long Bandwidth { get; set; }

        // 采样率，单位Hz
        public long SampleRate { get; private set; }

        // 时间戳，精确到秒
        public int TimeStampSecond { get; private set; }

        // 时间戳，精确到纳秒
        public int TimeStampNano { get; private set; }

        // 衰减
        public int Attenuation { get; private set; }

        // IQ对个数
        public int Count { get; private set; }

        // IQ数组，长度为2*Pt，索引依次为I0,Q0,I1,Q1...，In-1,Qn-1
        public int[] DataCollection { get; private set; }

        public RawIQ(int packetVersion = 0) : base(packetVersion) { }

        public override int Convert(byte[] value, int offset)
        {
            offset = base.Convert(value, offset);

            Frequency = BitConverter.ToInt64(value, offset);
            offset += 8;
            Bandwidth = BitConverter.ToInt64(value, offset);
            offset += 8;
            SampleRate = BitConverter.ToInt64(value, offset);
            offset += 8;
            TimeStampSecond = BitConverter.ToInt32(value, offset);
            offset += 4;
            TimeStampNano = BitConverter.ToInt32(value, offset);
            offset += 4;
            Attenuation = BitConverter.ToInt32(value, offset);
            offset += 4;
            Count = BitConverter.ToInt32(value, offset);
            offset += 4;
            DataCollection = new int[Count * 2];
            Buffer.BlockCopy(value, offset, DataCollection, 0, Count * 8);
            offset += Count * 8;

            return offset;
        }

        public override string ToString()
        {
            return string.Format("{0}, freq={1}, bw={2}, sr={3}, att={4}, cnt={5}, size={6}", base.ToString(), Frequency, Bandwidth, SampleRate, Attenuation, Count, DataCollection.Length * 4);
        }
    }

    /// <summary>
    /// 电平数据（与设备相关的）
    /// </summary>
    [Serializable]
    internal class RawLevel : RawData
    {
        // 频率，单位Hz
        public long Frequency { get; private set; }

        // 带宽，单位Hz
        public long Bandwidth { get; private set; }

        // 衰减
        public int Attenuation { get; private set; }

        // 电平，单位dBuV
        public float Level { get; private set; }

        // 场强，单位dBuV/m，暂未使用，在监测软件中统一处理
        public float FieldStrength { get; private set; }

        public RawLevel(int packetVersion = 0) : base(packetVersion) { }

        public override int Convert(byte[] value, int offset)
        {
            offset = base.Convert(value, offset);

            //解析子类中数据
            //Array.Reverse(value, offset,8);
            Frequency = BitConverter.ToInt64(value, offset);
            offset += 8;
            Bandwidth = BitConverter.ToInt64(value, offset);
            offset += 8;
            Attenuation = BitConverter.ToInt32(value, offset);
            offset += 4;
            Level = BitConverter.ToSingle(value, offset);
            offset += 4;
            FieldStrength = BitConverter.ToSingle(value, offset);
            offset += 4;

            return offset;
        }

        public override string ToString()
        {
            return string.Format("{0}, freq={1}, bw={2}, att={3}, lev={4}, fs={5}", base.ToString(), Frequency, Bandwidth, Attenuation, Level, FieldStrength);
        }
    }

    /// <summary>
    /// 频谱数据
    /// </summary>
    [Serializable]
    internal class RawSpectrum : RawData
    {
        // 频率，单位Hz
        public long Frequency { get; private set; }
        // 跨距，单位Hz
        public long Span { get; private set; }
        // 衰减
        public int Attenuation { get; private set; }
        // 频谱点数
        public int Count { get; private set; }
        // 频谱数据，单位 0.1dBuV
        public short[] DataCollection { get; private set; }

        public RawSpectrum(int packetVersion = 0) : base(packetVersion) { }

        public override int Convert(byte[] value, int offset)
        {
            offset = base.Convert(value, offset);

            Frequency = BitConverter.ToInt64(value, offset);
            offset += 8;
            Span = BitConverter.ToInt64(value, offset);
            offset += 8;
            Attenuation = BitConverter.ToInt32(value, offset);
            offset += 4;
            Count = BitConverter.ToInt32(value, offset);
            offset += 4;
            DataCollection = new short[Count];
            Buffer.BlockCopy(value, offset, DataCollection, 0, Count * 2);
            offset += Count * 2;

            return offset;
        }

        public override string ToString()
        {
            return string.Format("{0}, freq={1}, span={2}, att={3}, cnt={4}, len={5}", base.ToString(), Frequency, Span, Attenuation, Count, DataCollection.Length);
        }
    }

    /// <summary>
    /// 音频数据
    /// </summary>
    [Serializable]
    internal class RawAudio : RawData
    {
        // 频率，单位Hz
        public long Frequency { get; private set; }
        // 带宽，单位Hz
        public long Bandwidth { get; private set; }
        // 采样率，单位Hz
        public long SampleRate { get; private set; }
        // 保留值
        public int Reserved { get; private set; }
        // 音频长度
        public int Count { get; private set; }
        // 音频数据
        public byte[] DataCollection { get; private set; }

        public RawAudio(int packetVersion = 0) : base(packetVersion) { }

        public override int Convert(byte[] value, int offset)
        {
            offset = base.Convert(value, offset);

            Frequency = BitConverter.ToInt64(value, offset);
            offset += 8;
            Bandwidth = BitConverter.ToInt64(value, offset);
            offset += 8;
            SampleRate = BitConverter.ToInt64(value, offset);
            offset += 8;
            Reserved = BitConverter.ToInt32(value, offset);
            offset += 4;
            Count = BitConverter.ToInt32(value, offset);
            offset += 4;
            DataCollection = new byte[Count * 2]; // 从接收机到达的数据是16位的字节序列
            Buffer.BlockCopy(value, offset, DataCollection, 0, Count * 2);
            offset += Count * 2;

            return offset;
        }

        public override string ToString()
        {
            return string.Format("{0}, freq={1}, bw={2}, sr={3}, cnt={4}, bytes={5}", base.ToString(), Frequency, Bandwidth, SampleRate, Count, DataCollection.Length);
        }
    }

    /// <summary>
    /// DDC音频数据
    /// </summary>
    [Serializable]
    internal class RawDDCAudio : RawData
    {
        // 采样率
        public long SampleRate { get; private set; }
        // 使能的通道位，Flags，最大表示32位
        public uint EnabledChannels { get; private set; }
        // 音频数据个数（SHORT类型）
        public int Count { get; private set; }
        // 音频数据，包含最大32位音频
        public byte[] DataCollection { get; private set; }

        public RawDDCAudio(int packetVersion = 0) : base(packetVersion) { }

        public override int Convert(byte[] value, int offset)
        {
            offset = base.Convert(value, offset);

            SampleRate = BitConverter.ToInt64(value, offset);
            offset += 8;
            EnabledChannels = BitConverter.ToUInt32(value, offset);
            offset += 4;
            var ddcCount = 0;
            for (var index = 0; index < 32; ++index)
            {
                if (((EnabledChannels >> index) & 0x1) == 0x1)
                {
                    ddcCount++;
                }
            }
            Count = BitConverter.ToInt32(value, offset);
            offset += 4;
            DataCollection = new byte[ddcCount * Count * 2];
            Buffer.BlockCopy(value, offset, DataCollection, 0, DataCollection.Length);
            offset += DataCollection.Length;

            return offset;
        }

        public override string ToString()
        {
            return string.Format("{0}, sr={1}, enabled_ch_cnt={2}, ch_cnt={3}, cnt={4}, bytes={5}", base.ToString(), SampleRate, EnabledChannels, Count, DataCollection.Length);
        }
    }

    /// <summary>
    /// ITU数据
    /// </summary>
    [Serializable]
    internal class RawITU : RawData
    {
        // 频率，单位Hz
        public long Frequency { get; private set; }
        // Beta带宽，单位Hz
        public long Beta { get; private set; }
        // XdB带宽，单位Hz
        public long XdB { get; private set; }
        // AM调制度
        public float AM { get; private set; }
        // AM正调制度
        public float AMPos { get; private set; }
        // AM负调制度
        public float AMNeg { get; private set; }
        // FM调频
        public float FM { get; private set; }
        // FM正调频
        public float FMPos { get; private set; }
        // FM负调频
        public float FMNeg { get; private set; }
        // PM调相
        public float PM { get; private set; }
        // PM正调相
        public float PMPos { get; private set; }
        // PM负调相
        public float PMNeg { get; private set; }
        // 调制识别结果，值从-1开始依次对应Error, AM, FM, 2FSK, 4FSK, 2PSK, 4PSK, 2ASK, 4ASK, DSB, CW
        public int Modulation { get; private set; }

        public RawITU(int packetVersion = 0) : base(packetVersion) { }

        public override int Convert(byte[] value, int offset)
        {
            offset = base.Convert(value, offset);

            Frequency = BitConverter.ToInt64(value, offset);
            offset += 8;
            Beta = BitConverter.ToInt64(value, offset);
            offset += 8;
            XdB = BitConverter.ToInt64(value, offset);
            offset += 8;
            AM = BitConverter.ToSingle(value, offset);
            offset += 4;
            AMPos = BitConverter.ToSingle(value, offset);
            offset += 4;
            AMNeg = BitConverter.ToSingle(value, offset);
            offset += 4;
            FM = BitConverter.ToSingle(value, offset);
            offset += 4;
            FMPos = BitConverter.ToSingle(value, offset);
            offset += 4;
            FMNeg = BitConverter.ToSingle(value, offset);
            offset += 4;
            PM = BitConverter.ToSingle(value, offset);
            offset += 4;
            PMPos = BitConverter.ToSingle(value, offset);
            offset += 4;
            PMNeg = BitConverter.ToSingle(value, offset);
            offset += 4;
            Modulation = BitConverter.ToInt32(value, offset);
            offset += 4;

            return offset;
        }

        public override string ToString()
        {
            return string.Format("{0}, freq={1}, beta={2}, xdb={3}", base.ToString(), Frequency, Beta, XdB);
        }
    }

    /// <summary>
    /// 扫描数据
    /// </summary>
    [Serializable]
    internal class RawScan : RawData
    {
        // 起始频率，单位Hz
        public long StartFrequency { get; private set; }
        // 起始频率，单位Hz
        public long StopFrequency { get; private set; }
        // 扫描步进，单位Hz
        public long StepFrequency { get; private set; }
        // 原计划用于多段扫描中的频段编号，暂未使用
        public int SegmentIndex { get; private set; }
        // 当前频段总点数
        public int Total { get; set; }
        // 当前起始频点处所整段数据的偏移值
        public int Offset { get; private set; }
        // 当前扫描数据长度
        public int Count { get; private set; }
        // 扫描数据
        public short[] DataCollection;

        public RawScan(int packetVersion = 0) : base(packetVersion) { }

        public override int Convert(byte[] value, int offset)
        {
            offset = base.Convert(value, offset);

            StartFrequency = BitConverter.ToInt64(value, offset);
            offset += 8;
            StopFrequency = BitConverter.ToInt64(value, offset);
            offset += 8;
            StepFrequency = BitConverter.ToInt64(value, offset);
            offset += 8;
            SegmentIndex = BitConverter.ToInt32(value, offset);
            offset += 4;
            Total = BitConverter.ToInt32(value, offset);
            offset += 4;
            Offset = BitConverter.ToInt32(value, offset);
            offset += 4;
            Count = BitConverter.ToInt32(value, offset);
            offset += 4;
            DataCollection = new short[Count];
            Buffer.BlockCopy(value, offset, DataCollection, 0, 2 * Count);
            offset += 2 * Count;

            //var delta = Total - (Count + Offset);
            //// 容错处理, 接收机实际值可能相差1个点
            //if (Math.Abs(delta) == 1)
            //{
            //    Array.Resize(ref DataCollection, Count + delta);
            //    if (delta == 1)
            //    {
            //        DataCollection[Count] = DataCollection[Count - 1];
            //    }
            //}

            return offset;
        }

        public override string ToString()
        {
            return string.Format("{0}, start={1}, stop={2}, step={3}, total={4}, offset={5}, cnt={6}, len={7}", base.ToString(), StartFrequency, StopFrequency, StepFrequency, Total, Offset, Count, DataCollection.Length);
        }
    }

    /// <summary>
    /// 快速扫描数据
    /// </summary>
    [Serializable]
    internal class RawFastScan : RawData
    {
        // 起始频率，单位Hz
        public long StartFrequency { get; private set; }
        // 结束频率，单位Hz
        public long StopFrequency { get; private set; }
        // 扫描步进，单位Hz
        public long StepFrequency { get; private set; }
        // 频段索引
        public int SegmentIndex { get; private set; }
        // 当前扫描数据长度
        public int Count { get; private set; }
        // 当前扫描数据信号幅度，单位0.1dBuV
        public short[] SignalCollection { get; private set; }
        // 当前扫描数据底噪，单位0.1dBuV
        public short[] NoiseCollection { get; private set; }
        // 信号索引
        public int[] SignalIndexCollection { get; private set; }

        public RawFastScan(int packetVersion = 0) : base(packetVersion) { }

        public override int Convert(byte[] value, int offset)
        {
            offset = base.Convert(value, offset);

            StartFrequency = BitConverter.ToInt64(value, offset);
            offset += 8;
            StopFrequency = BitConverter.ToInt64(value, offset);
            offset += 8;
            StepFrequency = BitConverter.ToInt64(value, offset);
            offset += 8;
            SegmentIndex = BitConverter.ToInt32(value, offset);
            offset += 4;
            Count = BitConverter.ToInt32(value, offset);
            offset += 4;
            SignalCollection = new short[Count];
            Buffer.BlockCopy(value, offset, SignalCollection, 0, 2 * Count);
            offset += 2 * Count;
            NoiseCollection = new short[Count];
            Buffer.BlockCopy(value, offset, NoiseCollection, 0, 2 * Count);
            offset += 2 * Count;
            SignalIndexCollection = new int[Count];
            Buffer.BlockCopy(value, offset, SignalIndexCollection, 0, 4 * Count);
            offset += 4 * Count;

            return offset;
        }

        public override string ToString()
        {
            return string.Format("{0}, start={1}, stop={2}, step={3}, cnt={4}, s_len={5}, n_len={6}, idx_len={7}", base.ToString(), StartFrequency, StopFrequency, StepFrequency, Count, SignalCollection.Length, NoiseCollection.Length, SignalIndexCollection.Length);
        }
    }

    /// <summary>
    /// 短信数据
    /// </summary>
    [Serializable]
    internal class RawSMS : RawData
    {
        // 中心频率
        public long Frequency { get; private set; }

        // 解调带宽
        public long Bandwidth { get; private set; }

        // 色码
        public int ColorCode { get; private set; }

        // 被叫号码 
        public int CalledNumber { get; private set; }

        // 主叫号码
        public int CallingNumber { get; private set; }

        // 文本标识，用于标记文本编码格式
        public int IsASCII { get; private set; }

        // 文本字节数
        public int Count { get; private set; }

        // 文本信息
        public string Text { get; private set; }

        public RawSMS(int packetVersion = 0) : base(packetVersion) { }

        public override int Convert(byte[] value, int offset)
        {
            offset = base.Convert(value, offset);

            Frequency = BitConverter.ToInt64(value, offset);
            offset += 8;
            // Bandwidth = BitConverter.ToInt64(value, offset);
            // offset += 8;
            ColorCode = BitConverter.ToInt32(value, offset);
            offset += 4;
            CalledNumber = BitConverter.ToInt32(value, offset);
            offset += 4;
            CallingNumber = BitConverter.ToInt32(value, offset);
            offset += 4;
            IsASCII = BitConverter.ToInt32(value, offset);
            offset += 4;
            Count = BitConverter.ToInt32(value, offset);
            offset += 4;
            if (IsASCII == 1)
            {
                Text = System.Text.Encoding.ASCII.GetString(value, offset, Count);
            }
            else
            {
                Text = System.Text.Encoding.Unicode.GetString(value, offset, Count);
            }
            offset += Count;

            return offset;
        }

        public override string ToString()
        {
            return string.Format("{0}, freq={1}, bw={2}, cc={3}, calling={4}, called={5}", base.ToString(), Frequency, Bandwidth, ColorCode, CallingNumber, CalledNumber);
        }
    }

    /// <summary>
    /// DDC数据格式
    /// </summary>
    [Serializable]
    internal class RawDDC : RawData
    {
        public uint EnabledChannels { get; private set; }
        public int DDCSize { get; private set; }
        public List<RawSubDDC> DDCCollection { get; private set; }

		public RawDDC(int packetVersion = 0) : base(packetVersion) { }

        public override int Convert(byte[] value, int offset)
        {
            offset = base.Convert(value, offset);

            EnabledChannels = BitConverter.ToUInt32(value, offset);
            offset += 4;
            DDCSize = BitConverter.ToInt32(value, offset);
            offset += 4;
            DDCCollection = new List<RawSubDDC>();
            while (offset < value.Length)
            {
                var subDDC = RawSubDDC.Parse(value, ref offset);
                if (subDDC == null)
                {
                    break;
                }
                DDCCollection.Add(subDDC);
            }

            return offset;
        }
    }

    /// <summary>
    /// DDC子通道数据
    /// </summary>
    [Serializable]
    internal class RawSubDDC
    {
        // 电平
        public short Level { get; private set; }
        // 频谱点数，当前固定为128个点
        public short Count { get; private set; }
        // Beta带宽
        public int Beta { get; private set; }
        // XdB带宽
        public int XdB { get; private set; }
        // 频偏
        public short FrequencyDeviation { get; private set; }
        // 解调结果
        public short Demodulation { get; private set; }
        // 频谱
        public short[] Spectrum { get; private set; }

        public static RawSubDDC Parse(byte[] value, ref int offset)
        {
            try
            {
                var subDDC = new RawSubDDC();

                subDDC.Level = BitConverter.ToInt16(value, offset);
                offset += 2;
                subDDC.Count = BitConverter.ToInt16(value, offset);
                offset += 2;
                subDDC.Beta = BitConverter.ToInt32(value, offset);
                offset += 4;
                subDDC.XdB = BitConverter.ToInt32(value, offset);
                offset += 4;
                subDDC.FrequencyDeviation = BitConverter.ToInt16(value, offset);
                offset += 2;
                subDDC.Demodulation = BitConverter.ToInt16(value, offset);
                offset += 2;
                subDDC.Spectrum = new short[subDDC.Count];
                Buffer.BlockCopy(value, offset, subDDC.Spectrum, 0, sizeof(short) * subDDC.Count);
                offset += sizeof(short) * subDDC.Count;

                return subDDC;
            }
            catch
            {
                return null;
            }
        }
    }

    /// <summary>
    /// 原始GPS数据
    /// </summary>
    [Serializable]
    internal class RawGPS : RawData
    {
        // GPS字符串文本包含的字节总数
        public int Count { get; private set; }
        // GPS原始NMEA0183协议字符串
        public string Text { get; private set; }

		public RawGPS(int packetVersion = 0) : base(packetVersion) { }

        public override int Convert(byte[] value, int offset)
        {
            offset = base.Convert(value, offset);

            Count = BitConverter.ToInt32(value, offset);
            offset += 4;
            Text = System.Text.Encoding.ASCII.GetString(value, offset, Count);
            offset += Count;

            return offset;
        }

        public override string ToString()
        {
            return string.Format("{0}, text=\"{1}\"", base.ToString(), Text);
        }
    }

    /// <summary>
    /// 罗盘数据
    /// </summary>
    internal class RawCompass : RawData
    {
        // 是否为校准数据
        public int IsCalibration { get; private set; }

        // 方位角
        public int Heading { get; private set; }

        // 俯仰角
        public int Pitch { get; private set; }

        // 横滚角
        public int Rolling { get; private set; }

        public RawCompass(int packetVersion = 0) : base(packetVersion) { }

        public override int Convert(byte[] value, int offset)
        {
            offset = base.Convert(value, offset);

            IsCalibration = BitConverter.ToInt32(value, offset);
            offset += 4;
            Heading = BitConverter.ToInt32(value, offset);
            offset += 4;
            Pitch = BitConverter.ToInt32(value, offset);
            offset += 4;
            Rolling = BitConverter.ToInt32(value, offset);
            offset += 4;

            return offset;
        }

        public override string ToString()
        {
            return string.Format("{0}, heading=\"{1}\"", base.ToString(), Heading);
        }
    }

    #endregion
}