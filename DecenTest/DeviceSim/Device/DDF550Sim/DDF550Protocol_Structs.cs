/*********************************************************************************************
 *	
 * 文件名称:    DDF550Protocol_Structs.cs
 *
 * 作    者:    吴 刚
 *	
 * 创作日期:    2018-11-14
 * 
 * 备    注:	   定义数据结构体
 *               
*********************************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace DeviceSim.Device
{
    /// <summary>
    /// Version indication (version number) of hardware module.
    /// </summary>
    public struct SVersion
    {
        /// <summary>
        /// Main version number.
        /// </summary>
        public int iMainVersion;

        /// <summary>
        /// Sub version number.
        /// </summary>
        public int iSubVersion;
    }

    /// <summary>
    /// Antenna properties for an individual antenna frequency range 
    /// (all items apply only to addressed frequency range). 
    /// </summary>
    public struct SAntRangeProp
    {
        /// <summary>
        /// Antenna has preamplifier (true: yes, false: no).
        /// </summary>
        public bool bAntPreAmp;

        /// <summary>
        /// Measuring elevation possible (ditto).
        /// </summary>
        public bool bAntElevation;

        /// <summary>
        /// Lower border frequency [Hz].
        /// </summary>
        public long iFreqRangeBegin;

        /// <summary>
        /// Upper border frequency [Hz].
        /// </summary>
        public long iFreqRangeEnd;

        /// <summary>
        /// Type of input connector relevant to addressed range (to be used with AntennaSetup).
        /// </summary>
        public EInput_Range eInput_Range;

        /// <summary>
        /// DF evaluation principle.
        /// </summary>
        public EDf_Alt eDF_Alt;

        /// <summary>
        /// Antenna polarization.
        /// </summary>
        public EAnt_Pol eAnt_Pol;
    }

    /// <summary>
    /// Frequency switch point: frequency that cannot be used inside a scan range if R&S DDFx is 
    /// operated in synchronous scan mode (option R&S DDFx-TS) (e.g. antenna switches over at this
    /// frequency).Frequency may only be used as start frequency of the scan range.
    /// </summary>
    public struct SFreqTimePair
    {
        /// <summary>
        /// Frequency of switch point [Hz].
        /// </summary>
        public long iFrequency;

        /// <summary>
        /// Switching time [ns], indicates latency until DDFx is able to scan frequency.
        /// </summary>
        public long iTime;
    }

    /// <summary>
    /// Information on hardware module (peripheral device).
    /// </summary>
    public struct SHWInfo
    {
        /// <summary>
        /// Type of hardware (antenna, compass, ...).
        /// </summary>
        public EHW_Type eHw_Type;

        /// <summary>
        /// Status of module (OK or disconnected).
        /// </summary>
        public EHW_Status eHw_Status;

        /// <summary>
        /// Hardware code.
        /// </summary>
        public int iCode;

        /// <summary>
        /// Hardware handle (internal).
        /// </summary>
        public int iHandle;

        /// <summary>
        /// Port module is connected to.
        /// </summary>
        public int iPort;

        /// <summary>
        /// Version indication.
        /// </summary>
        public SVersion sVersion;

        /// <summary>
        /// Name of module (24 chars max.).
        /// </summary>
        public string zName;
    }

    /// <summary>
    /// Data record of antenna factors (k-factors) for Rx antenna. 
    /// </summary>
    public struct SKFactor
    {
        /// <summary>
        /// Antenna number (used for backward compatibility).
        /// </summary>
        public int iAntNo;

        /// <summary>
        /// Name of data record (usually corresponds to antenna model in AntennaRxDefine, 24 chars max.).
        /// </summary>
        public string zName;

        /// <summary>
        /// Lower border frequency [Hz] of antenna factors.
        /// </summary>
        public long iFreqRangeBegin;

        /// <summary>
        /// Upper border frequency [Hz].
        /// </summary>
        public long iFreqRangeEnd;

        /// <summary>
        /// Validity flags (1: applies, 0: does not apply; see table above for details).
        /// </summary>
        public int iAntParams;
    }

    /// <summary>
    /// Information on hardware module.
    /// </summary>
    public struct SModuleInfo
    {
        /// <summary>
        /// Part number (24 chars max.).
        /// </summary>
        public string zPartNumber;

        /// <summary>
        /// Hardware code.
        /// </summary>
        public int iHwCode;

        /// <summary>
        /// Product index (24 chars max.).
        /// </summary>
        public string zProductIndex;

        /// <summary>
        /// Serial number (24 chars max.).
        /// </summary>
        public string zSerialNumber;

        /// <summary>
        /// Production date (24 chars max.).
        /// </summary>
        public string zProductDate;

        /// <summary>
        /// Name of hardware module to address (24 chars max.).
        /// </summary>
        public string zName;
    }

    /// <summary>
    /// Location of temperature sensor (name of module and sensor) and temperature value.
    /// </summary>
    public struct STempInfo
    {
        /// <summary>
        /// Name of hardware module to address (24 chars max.) or "ALL" (also default value).
        /// </summary>
        public string zModule;

        /// <summary>
        /// Name of temperature sensor (24 chars max.).
        /// </summary>
        public string zSensor;

        /// <summary>
        /// Temperature value [°C].
        /// </summary>
        public int iValue;
    }

    /// <summary>
    /// Test command.
    /// </summary>
    public struct STestInfo
    {
        /// <summary>
        /// Description (256 chars max.) of this test and the meaning of the parameters for this test.
        /// </summary>
        public string zTestDescription;

        /// <summary>
        /// Name (24 chars max.) of the test that should be executed.
        /// </summary>
        public string zTestName;

        /// <summary>
        /// Default value (32 chars max.) for string parameter.
        /// </summary>
        public string zTestString;

        /// <summary>
        /// Default value (32 chars max.) for parameter 1.
        /// </summary>
        public long iParam1Default;

        /// <summary>
        /// Default value (32 chars max.) for parameter 2.
        /// </summary>
        public long iParam2Default;

        /// <summary>
        /// Default value (32 chars max.) for parameter 3.
        /// </summary>
        public long iParam3Default;

        /// <summary>
        /// Default value (32 chars max.) for parameter 4.
        /// </summary>
        public long iParam4Default;
    }

    /// <summary>
    /// Location and state of test point: name of point, name of module, measured value, tolerance range, value location related to tolerance range,
    /// test point validity. If testpoint is invalid, it can be ignored.
    /// </summary>
    public struct STestPoint
    {
        /// <summary>
        /// Name of test point (24 chars max.).
        /// </summary>
        public string zName;

        /// <summary>
        /// Name of module (24 chars max.).
        /// </summary>
        public string zModule;

        /// <summary>
        /// Measured value.
        /// </summary>
        public int iValue;

        /// <summary>
        /// Upper limit of tolerance range.
        /// </summary>
        public int iUpperLimit;

        /// <summary>
        /// Lower limit.
        /// </summary>
        public int iLowerLimit;

        /// <summary>
        /// Location of measured value related to tolerance range (below, inside, above tolerance range).
        /// </summary>
        public EOut_Of_Range eOut_Of_Range;

        /// <summary>
        /// Test point validity (true: valid, false: invalid).
        /// </summary>
        public bool bValid;
    }

    /// <summary>
    /// Characteristics of a trace (massdata socket connection with a client).
    /// </summary>
    public struct STraceInfo
    {
        /// <summary>
        /// Slot number of socket connection (0: default client).
        /// </summary>
        public int iSlot;

        /// <summary>
        /// 	IP address of the client (24 chars max.).
        /// </summary>
        public string zIP;

        /// <summary>
        /// Local port in use on client host for mass data connection.
        /// </summary>
        public int iPort;

        /// <summary>
        /// Array with all enabled trace tags.
        /// </summary>
        public ETraceTag eTraceTag;

        /// <summary>
        /// Array with all enabled selector flags.
        /// </summary>
        public ESelectorFlag selectorFlag;
    }

    /// <summary>
    /// Version indication (version number) of software assigned to an individual hardware module.
    /// </summary>
    public struct SSWVersion
    {
        /// <summary>
        /// Name of hardware module (24 chars max.).
        /// </summary>
        public string zModuleName;

        /// <summary>
        /// Software version number of hardware module (80 chars max.).
        /// </summary>
        public string zSwVersion;
    }

    /// <summary>
    /// EB200Header
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct EB200Header
    {
        [MarshalAs(UnmanagedType.U4)]
        public uint MagicNumber;
        [MarshalAs(UnmanagedType.U2)]
        public ushort VersionMinor;
        [MarshalAs(UnmanagedType.U2)]
        public ushort VersionMajor;
        [MarshalAs(UnmanagedType.U2)]
        public ushort SequenceNumberLow;
        [MarshalAs(UnmanagedType.U2)]
        public ushort SequenceNumberHigh;
        [MarshalAs(UnmanagedType.U4)]
        public uint DataSize;

        public EB200Header(byte[] value, int startIndex)
        {
            MagicNumber = BitConverter.ToUInt32(value, startIndex);
            VersionMinor = BitConverter.ToUInt16(value, startIndex + 4);
            VersionMajor = BitConverter.ToUInt16(value, startIndex + 6);
            SequenceNumberLow = BitConverter.ToUInt16(value, startIndex + 8);
            SequenceNumberHigh = BitConverter.ToUInt16(value, startIndex + 10);
            DataSize = BitConverter.ToUInt32(value, startIndex + 12);
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
            data = BitConverter.GetBytes(SequenceNumberLow);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(SequenceNumberHigh);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(DataSize);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            return buffer;
        }
    }

    public interface IGenericAttribute
    {
        ushort TraceTag { get; }
        uint DataLength { get; }

        byte[] ToBytes();
    }
    /// <summary>
    /// GenericAttribute Tag<5000
    /// </summary>[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct GenericAttributeConventional : IGenericAttribute
    {
        [MarshalAs(UnmanagedType.U2)]
        public ushort Tag;
        [MarshalAs(UnmanagedType.U2)]
        public ushort Length;

        public GenericAttributeConventional(byte[] value, int startIndex)
        {
            Array.Reverse(value, startIndex, 2);
            Tag = BitConverter.ToUInt16(value, startIndex);
            Array.Reverse(value, startIndex, 2);
            Array.Reverse(value, startIndex + 2, 2);
            Length = BitConverter.ToUInt16(value, startIndex + 2);
            Array.Reverse(value, startIndex + 2, 2);
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
        public ushort TraceTag
        {
            get
            {
                return Tag;
            }
        }

        public uint DataLength
        {
            get
            {
                return Length;
            }
        }

    }

    public struct GenericAttributeAdvanced : IGenericAttribute
    {
        [MarshalAs(UnmanagedType.U2)]
        public ushort Tag;
        [MarshalAs(UnmanagedType.U2)]
        public ushort Reserved1;
        [MarshalAs(UnmanagedType.U4)]
        public uint Length;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public uint[] Reserved2;
        public GenericAttributeAdvanced(byte[] value, int startIndex)
        {
            Reserved2 = new uint[4];
            Array.Reverse(value, startIndex, 2);
            Tag = BitConverter.ToUInt16(value, startIndex);
            Array.Reverse(value, startIndex, 2);
            Array.Reverse(value, startIndex + 2, 2);
            Reserved1 = BitConverter.ToUInt16(value, startIndex + 2);
            Array.Reverse(value, startIndex + 2, 2);
            Array.Reverse(value, startIndex + 4, 4);
            Length = BitConverter.ToUInt32(value, startIndex + 4);
            Array.Reverse(value, startIndex + 4, 4);
            for (int i = 0; i < 4; i++)
            {
                Array.Reverse(value, startIndex + 8 + 4 * i, 4);
                Reserved2[i] = BitConverter.ToUInt32(value, startIndex + 8 + 4 * i);
                Array.Reverse(value, startIndex + 8 + 4 * i, 4);
            }
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
            data = BitConverter.GetBytes(Reserved1);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(Length);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            for (int i = 0; i < Reserved2.Length; i++)
            {
                data = BitConverter.GetBytes(Reserved2[i]);
                Array.Reverse(data);
                Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
                offset += data.Length;
            }
            return buffer;
        }
        public ushort TraceTag
        {
            get
            {
                return Tag;
            }
        }
        public uint DataLength
        {
            get
            {
                return Length;
            }
        }
    }

    interface ITraceAttribute
    {
        byte[] ToBytes();
    }

    /// <summary>
    /// TraceAttribute 一般版本
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TraceAttributeConventional : ITraceAttribute
    {
        [MarshalAs(UnmanagedType.I2)]
        public short NumberOfTraceItems;
        [MarshalAs(UnmanagedType.U1)]
        public byte ChannelNumber;
        [MarshalAs(UnmanagedType.U1)]
        public byte OptionalHeaderLength;
        [MarshalAs(UnmanagedType.U4)]
        public uint SelectorFlags;

        public TraceAttributeConventional(byte[] value, int startIndex)
        {
            Array.Reverse(value, startIndex, 2);
            NumberOfTraceItems = BitConverter.ToInt16(value, startIndex);
            ChannelNumber = value[startIndex + 2];
            OptionalHeaderLength = value[startIndex + 3];
            Array.Reverse(value, startIndex + 4, 4);
            SelectorFlags = BitConverter.ToUInt32(value, startIndex + 4);
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
            offset++;
            buffer[offset] = OptionalHeaderLength;
            offset++;
            data = BitConverter.GetBytes(SelectorFlags);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            return buffer;
        }
    }
    /// <summary>
    /// TraceAttribute 高级版本
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TraceAttributeAdvanced : ITraceAttribute
    {
        [MarshalAs(UnmanagedType.U4)]
        public UInt32 NumberOfTraceItems;
        [MarshalAs(UnmanagedType.U4)]
        public UInt32 Reserved1;
        [MarshalAs(UnmanagedType.U4)]
        public UInt32 OptionalHeaderLength;
        [MarshalAs(UnmanagedType.U4)]
        public UInt32 SelectorFlagsLow;
        [MarshalAs(UnmanagedType.U4)]
        public UInt32 SelectorFlagsHigh;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public UInt32[] Reserved2;

        public TraceAttributeAdvanced(byte[] value, int startIndex)
        {
            Array.Reverse(value, startIndex, 4);
            NumberOfTraceItems = BitConverter.ToUInt32(value, startIndex);
            Array.Reverse(value, startIndex + 4, 4);
            Reserved1 = BitConverter.ToUInt32(value, startIndex + 4);
            Array.Reverse(value, startIndex + 8, 4);
            OptionalHeaderLength = BitConverter.ToUInt32(value, startIndex + 8);
            Array.Reverse(value, startIndex + 12, 4);
            SelectorFlagsLow = BitConverter.ToUInt32(value, startIndex + 12);
            Array.Reverse(value, startIndex + 16, 4);
            SelectorFlagsHigh = BitConverter.ToUInt32(value, startIndex + 16);
            Reserved2 = new UInt32[4];
            for (int i = 0; i < 4; i++)
            {
                Array.Reverse(value, startIndex + 20 + 4 * i, 4);
                Reserved2[i] = BitConverter.ToUInt32(value, startIndex + 20 + 4 * i);
            }
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
            data = BitConverter.GetBytes(Reserved1);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(OptionalHeaderLength);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(SelectorFlagsLow);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(SelectorFlagsHigh);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            for (int i = 0; i < Reserved2.Length; i++)
            {
                data = BitConverter.GetBytes(Reserved2[i]);
                Array.Reverse(data);
                Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
                offset += data.Length;
            }
            return buffer;
        }
    }

    interface IOptionalHeader
    {
        byte[] ToBytes();
    }

    /// <summary>
    /// OptionalHeaderAudio
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct OptionalHeaderAudio : IOptionalHeader
    {
        [MarshalAs(UnmanagedType.I2)]
        public short AudioMode;
        [MarshalAs(UnmanagedType.I2)]
        public short FrameLen;
        [MarshalAs(UnmanagedType.U4)]
        public uint FrequencyLow;
        [MarshalAs(UnmanagedType.U4)]
        public uint Bandwidth;
        [MarshalAs(UnmanagedType.U2)]
        public ushort Demodulation;//FM:0, AM:1, PULS:2, PM:3, IQ:4, ISB:5, CW:6, USB:7, LSB:8, TV:9
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
        public string sDemodulation;
        [MarshalAs(UnmanagedType.U4)]
        public uint FrequencyHigh;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 6)]
        public byte[] reserved;
        [MarshalAs(UnmanagedType.U8)]
        public ulong OutputTimestamp;
        [MarshalAs(UnmanagedType.I2)]
        public short SignalSource;

        public OptionalHeaderAudio(byte[] value, int startIndex)
        {
            if (BitConverter.IsLittleEndian)
            {
                byte[] data = new byte[2];
                Buffer.BlockCopy(value, startIndex, data, 0, 2);
                Array.Reverse(data);
                AudioMode = BitConverter.ToInt16(data, 0);
                Buffer.BlockCopy(value, startIndex + 2, data, 0, 2);
                Array.Reverse(data);
                FrameLen = BitConverter.ToInt16(data, 0);
                data = new byte[4];
                Buffer.BlockCopy(value, startIndex + 4, data, 0, 4);
                Array.Reverse(data);
                FrequencyLow = BitConverter.ToUInt32(data, 0);
                Buffer.BlockCopy(value, startIndex + 8, data, 0, 4);
                Array.Reverse(data);
                Bandwidth = BitConverter.ToUInt32(data, 0);
                data = new byte[2];
                Buffer.BlockCopy(value, startIndex + 12, data, 0, 2);
                Array.Reverse(data);
                Demodulation = BitConverter.ToUInt16(data, 0);

                sDemodulation = BitConverter.ToString(value, startIndex + 14, 8);
                data = new byte[4];
                Buffer.BlockCopy(value, startIndex + 22, data, 0, 4);
                Array.Reverse(data);
                FrequencyHigh = BitConverter.ToUInt32(data, 0);
                reserved = new byte[6];
                data = new byte[8];
                Buffer.BlockCopy(value, startIndex + 32, data, 0, 8);
                Array.Reverse(data);
                OutputTimestamp = BitConverter.ToUInt64(data, 0);
                data = new byte[2];
                Buffer.BlockCopy(value, startIndex + 40, data, 0, 2);
                Array.Reverse(data);
                SignalSource = BitConverter.ToInt16(data, 0);
            }
            else
            {
                AudioMode = BitConverter.ToInt16(value, startIndex);
                FrameLen = BitConverter.ToInt16(value, startIndex + 2);
                FrequencyLow = BitConverter.ToUInt32(value, startIndex + 4);
                Bandwidth = BitConverter.ToUInt32(value, startIndex + 8);
                Demodulation = BitConverter.ToUInt16(value, startIndex + 12);
                sDemodulation = BitConverter.ToString(value, startIndex + 14, 8);
                FrequencyHigh = BitConverter.ToUInt32(value, startIndex + 22);
                reserved = new byte[6];
                OutputTimestamp = BitConverter.ToUInt64(value, startIndex + 32);
                SignalSource = BitConverter.ToInt16(value, startIndex + 40);
            }
        }
        public byte[] ToBytes()
        {
            int len = Marshal.SizeOf(this);
            byte[] buffer = new byte[len];
            int offset = 0;
            byte[] data = BitConverter.GetBytes(AudioMode);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(FrameLen);
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
            data = Encoding.ASCII.GetBytes(sDemodulation);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += 8;
            data = BitConverter.GetBytes(FrequencyHigh);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            offset += 6;
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

    /// <summary>
    /// OptionalHeaderIFPan
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct OptionalHeaderIFPan : IOptionalHeader
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
    public struct OptionalHeaderIF : IOptionalHeader
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

    //optional_header_length is thus either 0 or 48.
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct OptionalHeaderPScan : IOptionalHeader
    {
        [MarshalAs(UnmanagedType.U4)]
        public uint StartFrequencyLow;
        [MarshalAs(UnmanagedType.U4)]
        public uint StopFrequencyLow;
        [MarshalAs(UnmanagedType.U4)]
        public uint StepFrequency;
        [MarshalAs(UnmanagedType.U4)]
        public uint StartFrequencyHigh;
        [MarshalAs(UnmanagedType.U4)]
        public uint StopFrequencyHigh;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 4)]
        public byte[] reserved;
        [MarshalAs(UnmanagedType.U8)]
        public ulong OutputTimestamp;
        [MarshalAs(UnmanagedType.U4)]
        public uint StepFrequencyNumerator;
        [MarshalAs(UnmanagedType.U4)]
        public uint StepFrequencyDenominator;
        [MarshalAs(UnmanagedType.U8)]
        public ulong FreqOfFirstStep;

        public OptionalHeaderPScan(byte[] value, int startIndex)
        {
            Array.Reverse(value, startIndex, 4);
            StartFrequencyLow = BitConverter.ToUInt32(value, startIndex);

            Array.Reverse(value, startIndex + 4, 4);
            StopFrequencyLow = BitConverter.ToUInt32(value, startIndex + 4);

            Array.Reverse(value, startIndex + 8, 4);
            StepFrequency = BitConverter.ToUInt32(value, startIndex + 8);

            Array.Reverse(value, startIndex + 12, 4);
            StartFrequencyHigh = BitConverter.ToUInt32(value, startIndex + 12);

            Array.Reverse(value, startIndex + 16, 4);
            StopFrequencyHigh = BitConverter.ToUInt32(value, startIndex + 16);

            reserved = new byte[4];

            Array.Reverse(value, startIndex + 24, 4);
            OutputTimestamp = BitConverter.ToUInt64(value, startIndex + 24);

            Array.Reverse(value, startIndex + 28, 4);
            StepFrequencyNumerator = BitConverter.ToUInt32(value, startIndex + 28);

            Array.Reverse(value, startIndex + 32, 4);
            StepFrequencyDenominator = BitConverter.ToUInt32(value, startIndex + 32);

            Array.Reverse(value, startIndex + 40, 4);
            FreqOfFirstStep = BitConverter.ToUInt64(value, startIndex + 40);
        }
        public byte[] ToBytes()
        {
            int len = Marshal.SizeOf(this);
            byte[] buffer = new byte[len];
            int offset = 0;
            byte[] data = BitConverter.GetBytes(StartFrequencyLow);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(StopFrequencyLow);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(StepFrequency);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(StartFrequencyHigh);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(StopFrequencyHigh);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            offset += 4;
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
            data = BitConverter.GetBytes(FreqOfFirstStep);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;

            return buffer;
        }
    }

    //optional_header_length is thus either 0 or 40.
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct OptionalHeaderFScan : IOptionalHeader
    {
        [MarshalAs(UnmanagedType.I2)]
        public short CycleCount;
        [MarshalAs(UnmanagedType.I2)]
        public short HoldTime;
        [MarshalAs(UnmanagedType.I2)]
        public short DwellTime;
        [MarshalAs(UnmanagedType.I2)]
        public short DirectionUP;
        [MarshalAs(UnmanagedType.I2)]
        public short StopSignal;
        [MarshalAs(UnmanagedType.U4)]
        public uint StartFrequencyLow;
        [MarshalAs(UnmanagedType.U4)]
        public uint StopFrequencyLow;
        [MarshalAs(UnmanagedType.U4)]
        public uint StepFrequency;
        [MarshalAs(UnmanagedType.U4)]
        public uint StartFrequencyHigh;
        [MarshalAs(UnmanagedType.U4)]
        public uint StopFrequencyHigh;
        [MarshalAs(UnmanagedType.U2)]
        ushort reserved;
        [MarshalAs(UnmanagedType.U8)]
        ulong OutputTimestamp;

        public OptionalHeaderFScan(byte[] value, int startIndex)
        {
            CycleCount = BitConverter.ToInt16(value, startIndex);
            HoldTime = BitConverter.ToInt16(value, startIndex + 2);
            DwellTime = BitConverter.ToInt16(value, startIndex + 4);
            DirectionUP = BitConverter.ToInt16(value, startIndex + 6);
            StopSignal = BitConverter.ToInt16(value, startIndex + 8);
            StartFrequencyLow = BitConverter.ToUInt32(value, startIndex + 10);
            StopFrequencyLow = BitConverter.ToUInt32(value, startIndex + 14);
            StepFrequency = BitConverter.ToUInt32(value, startIndex + 18);
            StartFrequencyHigh = BitConverter.ToUInt32(value, startIndex + 22);
            StopFrequencyHigh = BitConverter.ToUInt32(value, startIndex + 26);
            reserved = BitConverter.ToUInt16(value, startIndex + 30);
            OutputTimestamp = BitConverter.ToUInt64(value, startIndex + 32);
        }
        public byte[] ToBytes()
        {
            int len = Marshal.SizeOf(this);
            byte[] buffer = new byte[len];
            int offset = 0;
            byte[] data = BitConverter.GetBytes(CycleCount);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(HoldTime);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(DwellTime);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(DirectionUP);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(StopSignal);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(StartFrequencyLow);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(StopFrequencyLow);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(StepFrequency);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(StartFrequencyHigh);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(StopFrequencyHigh);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(reserved);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(OutputTimestamp);
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
    internal struct OptionalHeaderCW : IOptionalHeader
    {
        //SYSTem:IF:REMote:MODE OFF|SHORT|LONG
        [MarshalAs(UnmanagedType.U4)]
        public UInt32 Freq_Low;
        [MarshalAs(UnmanagedType.U4)]
        public UInt32 Freq_High;
        [MarshalAs(UnmanagedType.U8)]
        public UInt64 OutputTimestamp;
        [MarshalAs(UnmanagedType.I2)]
        public Int16 SignalSource;

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

    /// <summary>
    /// OptionalHeaderDFPScan
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct OptionalHeaderDFPScan : IOptionalHeader
    {
        [MarshalAs(UnmanagedType.I4)]
        public Int32 ScanRangeID;
        [MarshalAs(UnmanagedType.I4)]
        public Int32 ChannelsInScanRange;
        [MarshalAs(UnmanagedType.U8)]
        public UInt64 Frequency;
        [MarshalAs(UnmanagedType.I4)]
        public Int32 LogChannel;
        [MarshalAs(UnmanagedType.I4)]
        public Int32 FrequencyStepNumerator;
        [MarshalAs(UnmanagedType.I4)]
        public Int32 FrequencyStepDenominator;
        [MarshalAs(UnmanagedType.I4)]
        public Int32 Span;
        [MarshalAs(UnmanagedType.R4)]
        public float Bandwidth;
        [MarshalAs(UnmanagedType.I4)]
        public Int32 MeasureTime;
        [MarshalAs(UnmanagedType.I2)]
        public Int16 MeasureCount;
        [MarshalAs(UnmanagedType.I2)]
        public Int16 Threshold;
        [MarshalAs(UnmanagedType.I2)]
        public Int16 CompassHeading;
        [MarshalAs(UnmanagedType.I2)]
        public Int16 CompassHeadingType;
        [MarshalAs(UnmanagedType.I4)]
        public Int32 DFStatus;
        [MarshalAs(UnmanagedType.U8)]
        public UInt64 SweepTime;
        [MarshalAs(UnmanagedType.U8)]
        public UInt64 MeasureTimestamp;
        [MarshalAs(UnmanagedType.U2)]
        public UInt16 JobID;
        [MarshalAs(UnmanagedType.I2)]
        public Int16 SRSelectorflags;
        [MarshalAs(UnmanagedType.U1)]
        public Byte SRWaveCount;
        [MarshalAs(UnmanagedType.U1)]
        public Byte NumberOfEigenvalues;
        [MarshalAs(UnmanagedType.I2)]
        public Int16 Reserved;

        public OptionalHeaderDFPScan(byte[] value, int startIndex)
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(value, startIndex, 4);
                ScanRangeID = BitConverter.ToInt32(value, startIndex);
                Array.Reverse(value, startIndex + 4, 4);
                ChannelsInScanRange = BitConverter.ToInt32(value, startIndex + 4);
                Array.Reverse(value, startIndex + 8, 8);
                Frequency = BitConverter.ToUInt64(value, startIndex + 8);
                Array.Reverse(value, startIndex + 16, 4);
                LogChannel = BitConverter.ToInt32(value, startIndex + 16);
                Array.Reverse(value, startIndex + 20, 4);
                FrequencyStepNumerator = BitConverter.ToInt32(value, startIndex + 20);
                Array.Reverse(value, startIndex + 24, 4);
                FrequencyStepDenominator = BitConverter.ToInt32(value, startIndex + 24);
                Array.Reverse(value, startIndex + 28, 4);
                Span = BitConverter.ToInt32(value, startIndex + 28);
                Array.Reverse(value, startIndex + 32, 4);
                Bandwidth = BitConverter.ToSingle(value, startIndex + 32);
                Array.Reverse(value, startIndex + 36, 4);
                MeasureTime = BitConverter.ToInt32(value, startIndex + 36);
                Array.Reverse(value, startIndex + 40, 2);
                MeasureCount = BitConverter.ToInt16(value, startIndex + 40);
                Array.Reverse(value, startIndex + 42, 2);
                Threshold = BitConverter.ToInt16(value, startIndex + 42);
                Array.Reverse(value, startIndex + 44, 2);
                CompassHeading = BitConverter.ToInt16(value, startIndex + 44);
                Array.Reverse(value, startIndex + 46, 2);
                CompassHeadingType = BitConverter.ToInt16(value, startIndex + 46);
                Array.Reverse(value, startIndex + 48, 4);
                DFStatus = BitConverter.ToInt32(value, startIndex + 48);
                Array.Reverse(value, startIndex + 52, 8);
                SweepTime = BitConverter.ToUInt64(value, startIndex + 52);
                Array.Reverse(value, startIndex + 60, 8);
                MeasureTimestamp = BitConverter.ToUInt64(value, startIndex + 60);
                Array.Reverse(value, startIndex + 68, 2);
                JobID = BitConverter.ToUInt16(value, startIndex + 68);
                Array.Reverse(value, startIndex + 70, 2);
                SRSelectorflags = BitConverter.ToInt16(value, startIndex + 70);
                SRWaveCount = value[startIndex + 72];
                NumberOfEigenvalues = value[startIndex + 73];
                Array.Reverse(value, startIndex + 74, 2);
                Reserved = BitConverter.ToInt16(value, startIndex + 74);
            }
            else
            {
                ScanRangeID = BitConverter.ToInt32(value, startIndex);
                ChannelsInScanRange = BitConverter.ToInt32(value, startIndex + 4);
                Frequency = BitConverter.ToUInt64(value, startIndex + 8);
                LogChannel = BitConverter.ToInt32(value, startIndex + 16);
                FrequencyStepNumerator = BitConverter.ToInt32(value, startIndex + 20);
                FrequencyStepDenominator = BitConverter.ToInt32(value, startIndex + 24);
                Span = BitConverter.ToInt32(value, startIndex + 28);
                Bandwidth = BitConverter.ToSingle(value, startIndex + 32);
                MeasureTime = BitConverter.ToInt32(value, startIndex + 36);
                MeasureCount = BitConverter.ToInt16(value, startIndex + 40);
                Threshold = BitConverter.ToInt16(value, startIndex + 42);
                CompassHeading = BitConverter.ToInt16(value, startIndex + 44);
                CompassHeadingType = BitConverter.ToInt16(value, startIndex + 46);
                DFStatus = BitConverter.ToInt32(value, startIndex + 48);
                SweepTime = BitConverter.ToUInt64(value, startIndex + 52);
                MeasureTimestamp = BitConverter.ToUInt64(value, startIndex + 60);
                JobID = BitConverter.ToUInt16(value, startIndex + 68);
                SRSelectorflags = BitConverter.ToInt16(value, startIndex + 70);
                SRWaveCount = value[startIndex + 72];
                NumberOfEigenvalues = value[startIndex + 73];
                Reserved = BitConverter.ToInt16(value, startIndex + 74);
            }
        }
        public byte[] ToBytes()
        {
            int len = Marshal.SizeOf(this);
            byte[] buffer = new byte[len];
            int offset = 0;
            byte[] data = BitConverter.GetBytes(ScanRangeID);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(ChannelsInScanRange);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(Frequency);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(LogChannel);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(FrequencyStepNumerator);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(FrequencyStepDenominator);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(Span);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(Bandwidth);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(MeasureTime);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(MeasureCount);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(Threshold);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(CompassHeading);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(CompassHeadingType);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(DFStatus);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(SweepTime);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(MeasureTimestamp);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(JobID);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            data = BitConverter.GetBytes(SRSelectorflags);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
            buffer[offset] = SRWaveCount;
            offset++;
            buffer[offset] = NumberOfEigenvalues;
            offset++;
            data = BitConverter.GetBytes(Reserved);
            Array.Reverse(data);
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;

            return buffer;
        }
    }

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
            int offset = startIndex;
            for (int i = 0; i < dataCnt; i++)
            {
                Array.Reverse(buffer, offset, 2);
                Level[i] = BitConverter.ToInt16(buffer, offset);
                if (Level[i] == 2000)
                    Level[i] = short.MinValue;
                offset += 2;
            }
            for (int i = 0; i < dataCnt; i++)
            {
                Array.Reverse(buffer, offset, 4);
                FreqOffset[i] = BitConverter.ToInt32(buffer, offset);
                if (FreqOffset[i] == 10000000)
                    FreqOffset[i] = int.MinValue;
                offset += 4;
            }

            if ((selectorFlags & (uint)FLAGS.FSTRENGTH) > 0)
            {
                for (int i = 0; i < dataCnt; i++)
                {
                    Array.Reverse(buffer, offset, 2);
                    FStrength[i] = BitConverter.ToInt16(buffer, offset);
                    if (FStrength[i] == 0x7FFF)
                        FStrength[i] = short.MinValue;
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
                        AMDepth[i] = short.MinValue;
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
                        FMDev[i] = int.MinValue;
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
                        PMDepth[i] = short.MinValue;
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
                        BandWidth[i] = int.MinValue;
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
        }
        public byte[] ToBytes(uint selectorFlags)
        {
            int len = DataCnt * 40;
            byte[] buffer = new byte[len];
            int offset = 0;
            for (int i = 0; i < DataCnt; i++)
            {
                byte[] data = BitConverter.GetBytes(Level[i]);
                Array.Reverse(data);
                Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
                offset += data.Length;
            }
            for (int i = 0; i < DataCnt; i++)
            {
                byte[] data = BitConverter.GetBytes(FreqOffset[i]);
                Array.Reverse(data);
                Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
                offset += data.Length;
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

            byte[] arr = new byte[offset];
            if (offset < len)
            {
                Buffer.BlockCopy(buffer, 0, arr, 0, offset);
            }

            return arr;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct DFPScanData
    {
        public uint DataCnt;
        public short[] DfLevel;
        public short[] Azimuth;
        public short[] DfQuality;
        public short[] DfFstrength;
        public short[] DfLevelCont;
        public short[] Elevation;
        public short[] DfChannelStatus;
        public short[] DfOmniphase;

        public DFPScanData(uint dataCnt, byte[] buffer, int startIndex, ulong selectorFlags)
        {
            DataCnt = dataCnt;
            DfLevel = new short[dataCnt];
            Azimuth = new short[dataCnt];
            DfQuality = new short[dataCnt];
            DfFstrength = new short[dataCnt];
            DfLevelCont = new short[dataCnt];
            Elevation = new short[dataCnt];
            DfChannelStatus = new short[dataCnt];
            DfOmniphase = new short[dataCnt];
            int offset = startIndex;
            if ((selectorFlags & (uint)FLAGS.DF_LEVEL) > 0)
            {
                for (int i = 0; i < dataCnt; i++)
                {
                    Array.Reverse(buffer, offset, 2);
                    DfLevel[i] = BitConverter.ToInt16(buffer, offset);
                    if (DfLevel[i] == 2000 || DfLevel[i] == 1999)
                        DfLevel[i] = short.MinValue;
                    offset += 2;
                }
            }
            if ((selectorFlags & (uint)FLAGS.AZIMUTH) > 0)
            {
                for (int i = 0; i < dataCnt; i++)
                {
                    Array.Reverse(buffer, offset, 2);
                    Azimuth[i] = BitConverter.ToInt16(buffer, offset);
                    if (Azimuth[i] == 0x7FFF || Azimuth[i] == 0x7FFE)
                        Azimuth[i] = short.MinValue;
                    offset += 2;
                }
            }

            if ((selectorFlags & (uint)FLAGS.DF_QUALITY) > 0)
            {
                for (int i = 0; i < dataCnt; i++)
                {
                    Array.Reverse(buffer, offset, 2);
                    DfQuality[i] = BitConverter.ToInt16(buffer, offset);
                    if (DfQuality[i] == 0x7FFF || DfQuality[i] == 0x7FFE)
                        DfQuality[i] = short.MinValue;
                    offset += 2;
                }
            }
            if ((selectorFlags & (uint)FLAGS.DF_FSTRENGTH) > 0)
            {
                for (int i = 0; i < dataCnt; i++)
                {
                    Array.Reverse(buffer, offset, 2);
                    DfFstrength[i] = BitConverter.ToInt16(buffer, offset);
                    if (DfFstrength[i] == 0x7FFF || DfFstrength[i] == 0x7FFE)
                        DfFstrength[i] = short.MinValue;
                    offset += 2;
                }
            }
            if ((selectorFlags & (uint)FLAGS.DF_LEVEL_CONT) > 0)
            {
                for (int i = 0; i < dataCnt; i++)
                {
                    Array.Reverse(buffer, offset, 2);
                    DfLevelCont[i] = BitConverter.ToInt16(buffer, offset);
                    offset += 2;
                }
            }
            if ((selectorFlags & (uint)FLAGS.ELEVATION) > 0)
            {
                for (int i = 0; i < dataCnt; i++)
                {
                    Array.Reverse(buffer, offset, 2);
                    Elevation[i] = BitConverter.ToInt16(buffer, offset);
                    if (Elevation[i] == 0x7FFF || Elevation[i] == 0x7FFE)
                        Elevation[i] = short.MinValue;
                    offset += 2;
                }
            }
            if ((selectorFlags & (uint)FLAGS.DF_CHANNEL_STATUS) > 0)
            {
                for (int i = 0; i < dataCnt; i++)
                {
                    Array.Reverse(buffer, offset, 2);
                    DfChannelStatus[i] = BitConverter.ToInt16(buffer, offset);
                    offset += 2;
                }
            }
            if ((selectorFlags & (uint)FLAGS.DF_OMNIPHASE) > 0)
            {
                for (int i = 0; i < dataCnt; i++)
                {
                    Array.Reverse(buffer, offset, 2);
                    DfOmniphase[i] = BitConverter.ToInt16(buffer, offset);
                    if (DfOmniphase[i] == 0x7FFF || DfOmniphase[i] == 0x7FFE)
                        DfOmniphase[i] = short.MinValue;
                    offset += 2;
                }
            }
        }
        public DFPScanData(uint dataCnt)
        {
            DataCnt = dataCnt;
            DfLevel = new short[dataCnt];
            Azimuth = new short[dataCnt];
            DfQuality = new short[dataCnt];
            DfFstrength = new short[dataCnt];
            DfLevelCont = new short[dataCnt];
            Elevation = new short[dataCnt];
            DfChannelStatus = new short[dataCnt];
            DfOmniphase = new short[dataCnt];
        }
        public byte[] ToBytes(ulong selectorFlags)
        {
            uint len = DataCnt * 16;
            byte[] buffer = new byte[len];
            int offset = 0;
            if ((selectorFlags & (uint)FLAGS.DF_LEVEL) > 0)
            {
                for (int i = 0; i < DataCnt; i++)
                {
                    byte[] data = BitConverter.GetBytes(DfLevel[i]);
                    Array.Reverse(data);
                    Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
                    offset += data.Length;
                }
            }
            if ((selectorFlags & (uint)FLAGS.AZIMUTH) > 0)
            {
                for (int i = 0; i < DataCnt; i++)
                {
                    byte[] data = BitConverter.GetBytes(Azimuth[i]);
                    Array.Reverse(data);
                    Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
                    offset += data.Length;
                }
            }
            if ((selectorFlags & (uint)FLAGS.DF_QUALITY) > 0)
            {
                for (int i = 0; i < DataCnt; i++)
                {
                    byte[] data = BitConverter.GetBytes(DfQuality[i]);
                    Array.Reverse(data);
                    Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
                    offset += data.Length;
                }
            }
            if ((selectorFlags & (uint)FLAGS.DF_FSTRENGTH) > 0)
            {
                for (int i = 0; i < DataCnt; i++)
                {
                    byte[] data = BitConverter.GetBytes(DfFstrength[i]);
                    Array.Reverse(data);
                    Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
                    offset += data.Length;
                }
            }
            if ((selectorFlags & (uint)FLAGS.DF_LEVEL_CONT) > 0)
            {
                for (int i = 0; i < DataCnt; i++)
                {
                    byte[] data = BitConverter.GetBytes(DfLevelCont[i]);
                    Array.Reverse(data);
                    Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
                    offset += data.Length;
                }
            }
            if ((selectorFlags & (uint)FLAGS.ELEVATION) > 0)
            {
                for (int i = 0; i < DataCnt; i++)
                {
                    byte[] data = BitConverter.GetBytes(Elevation[i]);
                    Array.Reverse(data);
                    Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
                    offset += data.Length;
                }
            }
            if ((selectorFlags & (uint)FLAGS.DF_CHANNEL_STATUS) > 0)
            {
                for (int i = 0; i < DataCnt; i++)
                {
                    byte[] data = BitConverter.GetBytes(DfChannelStatus[i]);
                    Array.Reverse(data);
                    Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
                    offset += data.Length;
                }
            }
            if ((selectorFlags & (uint)FLAGS.DF_OMNIPHASE) > 0)
            {
                for (int i = 0; i < DataCnt; i++)
                {
                    byte[] data = BitConverter.GetBytes(DfOmniphase[i]);
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
}
