using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace ODM20181102TJ01
{
    /// <summary>
    /// 增益参数
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct GainParams
    {
        /// <summary>
        /// 增益控制
        /// </summary>
        [MarshalAs(UnmanagedType.U4)]
        public uint GainControl;

        [MarshalAs(UnmanagedType.U4)]
        public uint AgcDecay;

        [MarshalAs(UnmanagedType.U4)]
        public uint MgcGain;

        public GainParams(byte[] buffer, int offset)
        {
            GainControl = BitConverter.ToUInt32(buffer, offset);
            offset += 4;
            AgcDecay = BitConverter.ToUInt32(buffer, offset);
            offset += 4;
            MgcGain = BitConverter.ToUInt32(buffer, offset);
        }

        public byte[] ToBytes()
        {
            int len = Marshal.SizeOf(this);
            byte[] buffer = new byte[len];
            int offset = 0;

            byte[] data = BitConverter.GetBytes(GainControl);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(AgcDecay);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(MgcGain);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;

            return buffer;
        }

        private bool Equals(GainParams other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (GainControl != other.GainControl)
            {
                Console.WriteLine(string.Format("GainControl 变化{0}=>{1}", GainControl, other.GainControl));
                return false;
            }

            if (AgcDecay != other.AgcDecay)
            {
                Console.WriteLine(string.Format("AgcDecay 变化{0}=>{1}", AgcDecay, other.AgcDecay));
                return false;
            }

            if (MgcGain != other.MgcGain)
            {
                Console.WriteLine(string.Format("MgcGain 变化{0}=>{1}", MgcGain, other.MgcGain));
                return false;
            }

            return true;
        }

        #region 重写基类

        // 重写Equals
        public override bool Equals(object obj)
        {
            // this非空，obj如果为空，则返回false
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            // 如果为同一对象，必然相等
            if (ReferenceEquals(this, obj))
            { 
                return true;
            }

            // 如果类型不同，则必然不相等
            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            // 调用强类型对比
            return Equals((GainParams)obj);
        }

        // 重写Equals必须重写GetHashCode
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        // 重写==操作符
        public static bool operator ==(GainParams left, GainParams right)
        {
            return left.Equals(right);
        }

        // 重写!=操作符
        public static bool operator !=(GainParams left, GainParams right)
        {
            return !left.Equals(right);
        }

        #endregion 重写基类
    }

    /// <summary>
    /// 一般参数
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct CommParams
    {
        /// <summary>
        /// 测向带宽 Hz
        /// </summary>
        [MarshalAs(UnmanagedType.U4)]
        public uint DfBandWdith;
        /// <summary>
        /// 测向模式 对应枚举EAverageMode
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public byte AverageMode;
        /// <summary>
        /// 未知 对应枚举 EReadMode
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public byte ReadMode;
        /// <summary>
        /// 未知
        /// </summary>
        [MarshalAs(UnmanagedType.U2)]
        public ushort ReadTime;
        /// <summary>
        /// 电平门限
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public byte Threshold;
        /// <summary>
        /// 对应枚举ESampleMode
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public byte SampleMode;
        /// <summary>
        /// 天线极化方式 对应枚举EAntPol
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public byte AntPol;
        /// <summary>
        /// 测向体制 对应枚举EDfAlt
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public byte DfMethod;
        /// <summary>
        /// 解调模式 对应枚举EAfDemod
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public byte AfDemod;
        /// <summary>
        /// 静噪门限
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public byte AfThreshold;
        /// <summary>
        /// 未知
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public byte Bfo;
        /// <summary>
        /// 未知
        /// </summary>
        [MarshalAs(UnmanagedType.U4)]
        public uint SpectrumTime;
        /// <summary>
        /// 解调带宽 Hz
        /// </summary>
        [MarshalAs(UnmanagedType.U4)]
        public uint AfBandWidth;
        [MarshalAs(UnmanagedType.U4)]
        public uint UpperLevel;
        /// <summary>
        /// 测向质量门限 0-99
        /// </summary>
        [MarshalAs(UnmanagedType.U4)]
        public uint QualityThreshold;

        /// <summary>
        /// 增益控制参数
        /// </summary>
        public GainParams GainParams;

        public CommParams(byte[] buffer, int offset)
        {
            DfBandWdith = BitConverter.ToUInt32(buffer, offset);
            offset += 4;
            AverageMode = buffer[offset++];
            ReadMode = buffer[offset++];
            ReadTime = BitConverter.ToUInt16(buffer, offset);
            offset += 2;
            Threshold = buffer[offset++];
            SampleMode = buffer[offset++];
            AntPol = buffer[offset++];
            DfMethod = buffer[offset++];
            AfDemod = buffer[offset++];
            AfThreshold = buffer[offset++];
            Bfo = buffer[offset++];
            SpectrumTime = BitConverter.ToUInt32(buffer, offset);
            offset += 4;
            AfBandWidth = BitConverter.ToUInt32(buffer, offset);
            offset += 4;
            UpperLevel = BitConverter.ToUInt32(buffer, offset);
            offset += 4;
            QualityThreshold = BitConverter.ToUInt32(buffer, offset);
            offset += 4;

            GainParams = new GainParams(buffer, offset);
            offset += Marshal.SizeOf(GainParams);
        }

        public byte[] ToBytes()
        {
            int len = Marshal.SizeOf(this);
            byte[] buffer = new byte[len];
            int offset = 0;
            byte[] data = BitConverter.GetBytes(DfBandWdith);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            buffer[offset++] = AverageMode;
            buffer[offset++] = ReadMode;
            data = BitConverter.GetBytes(ReadTime);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            buffer[offset++] = Threshold;
            buffer[offset++] = SampleMode;
            buffer[offset++] = AntPol;
            buffer[offset++] = DfMethod;
            buffer[offset++] = AfDemod;
            buffer[offset++] = AfThreshold;
            buffer[offset++] = Bfo;
            data = BitConverter.GetBytes(SpectrumTime);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(AfBandWidth);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(UpperLevel);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(QualityThreshold);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            byte[] tmpArr = GainParams.ToBytes();
            Buffer.BlockCopy(tmpArr, 0, buffer, offset, tmpArr.Length);
            offset += tmpArr.Length;

            return buffer;
        }

        /// <summary>
        /// 判断两个CommParams是否相等
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        private bool Equals(CommParams other)
        {
            //this非空，obj如果为空，则返回false
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            //如果为同一对象，必然相等
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            #region 判断值变化

            //对比各个字段值

            //这段代码暂时不用，使用后面的代码输出那些值发生了变化
            //if (DfBandWdith != other.DfBandWdith || AverageMode != other.AverageMode
            //    || ReadMode != other.ReadMode || ReadTime != other.ReadTime
            //    || Threshold != other.Threshold || SampleMode != other.SampleMode
            //    || AntPol != other.AntPol || DfMethod != other.DfMethod
            //    || AfDemod != other.AfDemod || AfThreshold != other.AfThreshold
            //    || Bfo != other.Bfo || SpectrumTime != other.SpectrumTime || AfBandWidth != other.AfBandWidth
            //    || UpperLevel != other.UpperLevel || QualityThreshold != other.QualityThreshold)
            //{
            //    return false;
            //}

            if (DfBandWdith != other.DfBandWdith)
            {
                Console.WriteLine(string.Format("DfBandWdith 变化{0}=>{1}", DfBandWdith, other.DfBandWdith));
                return false;
            }
            if (AverageMode != other.AverageMode)
            {
                Console.WriteLine(string.Format("AverageMode 变化{0}=>{1}", AverageMode, other.AverageMode));
                return false;
            }
            if (ReadMode != other.ReadMode)
            {
                Console.WriteLine(string.Format("ReadMode 变化{0}=>{1}", ReadMode, other.ReadMode));
                return false;
            }
            if (ReadTime != other.ReadTime)
            {
                Console.WriteLine(string.Format("ReadTime 变化{0}=>{1}", ReadTime, other.ReadTime));
                return false;
            }
            if (Threshold != other.Threshold)
            {
                Console.WriteLine(string.Format("Threshold 变化{0}=>{1}", Threshold, other.Threshold));
                return false;
            }
            if (SampleMode != other.SampleMode)
            {
                Console.WriteLine(string.Format("SampleMode 变化{0}=>{1}", SampleMode, other.SampleMode));
                return false;
            }
            if (AntPol != other.AntPol)
            {
                Console.WriteLine(string.Format("AntPol 变化{0}=>{1}", AntPol, other.AntPol));
                return false;
            }
            if (DfMethod != other.DfMethod)
            {
                Console.WriteLine(string.Format("DfMethod 变化{0}=>{1}", DfMethod, other.DfMethod));
                return false;
            }
            if (AfDemod != other.AfDemod)
            {
                Console.WriteLine(string.Format("AfDemod 变化{0}=>{1}", AfDemod, other.AfDemod));
                return false;
            }
            if (AfThreshold != other.AfThreshold)
            {
                Console.WriteLine(string.Format("AfThreshold 变化{0}=>{1}", AfThreshold, other.AfThreshold));
                return false;
            }
            if (Bfo != other.Bfo)
            {
                Console.WriteLine(string.Format("Bfo 变化{0}=>{1}", Bfo, other.Bfo));
                return false;
            }
            if (SpectrumTime != other.SpectrumTime)
            {
                Console.WriteLine(string.Format("SpectrumTime 变化{0}=>{1}", SpectrumTime, other.SpectrumTime));
                return false;
            }
            if (AfBandWidth != other.AfBandWidth)
            {
                Console.WriteLine(string.Format("AfBandWidth 变化{0}=>{1}", AfBandWidth, other.AfBandWidth));
                return false;
            }
            if (UpperLevel != other.UpperLevel)
            {
                Console.WriteLine(string.Format("UpperLevel 变化{0}=>{1}", UpperLevel, other.UpperLevel));
                return false;
            }
            if (QualityThreshold != other.QualityThreshold)
            {
                Console.WriteLine(string.Format("QualityThreshold 变化{0}=>{1}", QualityThreshold, other.QualityThreshold));
                return false;
            }

            if (!GainParams.Equals(other.GainParams))
            {
                return false;
            }

            #endregion 判断值变化

            //如果基类不是从Object继承，需要调用base.Equals(other)
            //如果从Object继承，直接返回true
            return true;
        }

        #region 重写基类

        // 重写Equals
        public override bool Equals(object obj)
        {
            // this非空，obj如果为空，则返回false
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            // 如果为同一对象，必然相等
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            // 如果类型不同，则必然不相等
            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            // 调用强类型对比
            return Equals((CommParams)obj);
        }

        // 重写Equals必须重写GetHashCode
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        // 重写==操作符
        public static bool operator ==(CommParams left, CommParams right)
        {
            return left.Equals(right);
        }

        // 重写!=操作符
        public static bool operator !=(CommParams left, CommParams right)
        {
            return !left.Equals(right);
        }

        #endregion 重写基类
    }

    /// <summary>
    /// FFM参数
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct FfmParams : IParameter
    {
        /// <summary>
        /// 中心频率 Hz
        /// </summary>
        [MarshalAs(UnmanagedType.U4)]
        public uint Frequency;
        public CommParams CommParams;

        public FfmParams(byte[] buffer, int offset)
        {
            Frequency = BitConverter.ToUInt32(buffer, offset);
            offset += 4;
            CommParams = new CommParams(buffer, offset);
            offset += Marshal.SizeOf(CommParams);
        }

        public byte[] ToBytes()
        {
            int len = Marshal.SizeOf(this);
            byte[] buffer = new byte[len];
            int offset = 0;
            byte[] data = BitConverter.GetBytes(Frequency);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = CommParams.ToBytes();
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            return buffer;
        }

        /// <summary>
        /// 判断两个FfmParams是否相等
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        private bool Equals(FfmParams other)
        {
            //this非空，obj如果为空，则返回false
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            //如果为同一对象，必然相等
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (Frequency != other.Frequency)
            {
                Console.WriteLine(string.Format("Frequency 变化{0}=>{1}", Frequency, other.Frequency));
                return false;
            }

            //对比各个字段值
            if (!CommParams.Equals(other.CommParams))
            {
                return false;
            }

            //如果基类不是从Object继承，需要调用base.Equals(other)
            //如果从Object继承，直接返回true
            return true;
        }


        #region 重写基类

        // 重写Equals
        public override bool Equals(object obj)
        {
            // this非空，obj如果为空，则返回false
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            // 如果为同一对象，必然相等
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            // 如果类型不同，则必然不相等
            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            // 调用强类型对比
            return Equals((FfmParams)obj);
        }

        // 重写Equals必须重写GetHashCode
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        // 重写==操作符
        public static bool operator ==(FfmParams left, FfmParams right)
        {
            return left.Equals(right);
        }

        // 重写!=操作符
        public static bool operator !=(FfmParams left, FfmParams right)
        {
            return !left.Equals(right);
        }

        #endregion 重写基类
    }

    /// <summary>
    /// 扫描参数
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct ScanParams : IParameter
    {
        /// <summary>
        /// 起始频率 Hz
        /// </summary>
        [MarshalAs(UnmanagedType.U4)]
        public uint StartFreq;
        /// <summary>
        /// 结束频率 Hz
        /// </summary>
        [MarshalAs(UnmanagedType.U4)]
        public uint StopFreq;
        /// <summary>
        /// 步进 Hz
        /// </summary>
        [MarshalAs(UnmanagedType.U4)]
        public uint StepWidth;
        /// <summary>
        /// 起始通道号 从1开始
        /// </summary>
        [MarshalAs(UnmanagedType.U4)]
        public uint StartChannel;
        /// <summary>
        /// 结束通道号
        /// </summary>
        [MarshalAs(UnmanagedType.U4)]
        public uint StopChannel;
        /// <summary>
        /// 其他参数
        /// </summary>
        public CommParams CommParams;

        public ScanParams(byte[] buffer, int offset)
        {
            StartFreq = BitConverter.ToUInt32(buffer, offset);
            offset += 4;
            StopFreq = BitConverter.ToUInt32(buffer, offset);
            offset += 4;
            StepWidth = BitConverter.ToUInt32(buffer, offset);
            offset += 4;
            StartChannel = BitConverter.ToUInt32(buffer, offset);
            offset += 4;
            StopChannel = BitConverter.ToUInt32(buffer, offset);
            offset += 4;
            CommParams = new CommParams(buffer, offset);
            offset += Marshal.SizeOf(CommParams);
        }
        public byte[] ToBytes()
        {
            int len = Marshal.SizeOf(this);
            byte[] buffer = new byte[len];
            int offset = 0;
            byte[] data = BitConverter.GetBytes(StartFreq);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(StopFreq);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(StepWidth);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(StartChannel);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(StopChannel);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;

            data = CommParams.ToBytes();
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            return buffer;
        }

        /// <summary>
        /// 判断两个ScanParams是否相等
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        private bool Equals(ScanParams other)
        {
            //this非空，obj如果为空，则返回false
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            //如果为同一对象，必然相等
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            //对比各个字段值
            if (!CommParams.Equals(other.CommParams))
            {
                return false;
            }

            //这段代码暂时不用，使用后面的代码输出那些值发生了变化
            //if (StartFreq != other.StartFreq || StopFreq != other.StopFreq
            //    || StepWidth != other.StepWidth || StartChannel != other.StartChannel
            //    || StopChannel != other.StopChannel)
            //{
            //    return false;
            //}

            if (StartFreq != other.StartFreq)
            {
                Console.WriteLine(string.Format("StartFreq 变化{0}=>{1}", StartFreq, other.StartFreq));
                return false;
            }
            if (StopFreq != other.StopFreq)
            {
                Console.WriteLine(string.Format("StopFreq 变化{0}=>{1}", StopFreq, other.StopFreq));
                return false;
            }
            if (StepWidth != other.StepWidth)
            {
                Console.WriteLine(string.Format("StepWidth 变化{0}=>{1}", StepWidth, other.StepWidth));
                return false;
            }
            if (StartChannel != other.StartChannel)
            {
                Console.WriteLine(string.Format("StartChannel 变化{0}=>{1}", StartChannel, other.StartChannel));
                return false;
            }
            if (StopChannel != other.StopChannel)
            {
                Console.WriteLine(string.Format("StopChannel 变化{0}=>{1}", StopChannel, other.StopChannel));
                return false;
            }


            //如果基类不是从Object继承，需要调用base.Equals(other)
            //如果从Object继承，直接返回true
            return true;
        }


        #region 重写基类

        // 重写Equals
        public override bool Equals(object obj)
        {
            // this非空，obj如果为空，则返回false
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            // 如果为同一对象，必然相等
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            // 如果类型不同，则必然不相等
            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            // 调用强类型对比
            return Equals((ScanParams)obj);
        }

        // 重写Equals必须重写GetHashCode
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        // 重写==操作符
        public static bool operator ==(ScanParams left, ScanParams right)
        {
            return left.Equals(right);
        }

        // 重写!=操作符
        public static bool operator !=(ScanParams left, ScanParams right)
        {
            return !left.Equals(right);
        }

        #endregion 重写基类
    }

    /// <summary>
    /// 参数消息
    /// </summary>
    internal class ParameterMessage
    {
        [MarshalAs(UnmanagedType.U1)]
        public byte Mode;
        [MarshalAs(UnmanagedType.U4)]
        public uint ParamLen;

        public EDataMode MsgMode
        {
            get { return (EDataMode)Mode; }
            set
            {
                byte va = (byte)value;
                if (va != Mode)
                {
                    Mode = va;
                }
            }
        }
        public IParameter Parameter { get; set; }

        public ParameterMessage()
        {
        }

        public ParameterMessage(byte[] buffer, int offset)
        {
            Mode = buffer[offset++];
            ParamLen = BitConverter.ToUInt32(buffer, offset);
            offset += 4;
            //if (!Utils.CheckParamLength(Mode, ParamLen))
            //{
            //    throw new Exception("接受到的数据格式错误");
            //}

            EDataMode mode = (EDataMode)Mode;
            Parameter = null;
            switch (mode)
            {
                case EDataMode.MODE_FFM:
                    Parameter = new FfmParams(buffer, offset);
                    break;
                case EDataMode.MODE_SCAN:
                    Parameter = new ScanParams(buffer, offset);
                    break;
                case EDataMode.MODE_STANDBY:
                    Parameter = null;
                    return;
                case EDataMode.MODE_SEARCH:
                case EDataMode.MODE_TDMA:
                case EDataMode.MODE_CALIB:
                case EDataMode.MODE_DIAG:
                default:
                    break;
            }
        }

        public byte[] ToBytes()
        {
            byte[] data = Parameter == null ? new byte[0] : Parameter.ToBytes();
            ParamLen = (uint)data.Length;
            int len = data.Length + 4 + 1;
            byte[] buffer = new byte[len];
            int offset = 0;
            buffer[offset++] = Mode;
            byte[] data1 = BitConverter.GetBytes(ParamLen);
            Buffer.BlockCopy(data1, 0, buffer, offset, data1.Length);
            offset += data1.Length;
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            return buffer;
        }
    }
}
