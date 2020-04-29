/*********************************************************************************************
 *	
 * 文件名称:    ..\Tracker800\Server\Source\Device\Receiver\DDF200M\DDF200MProtocol.cs
 *
 * 作    者:    苏 林 国 
 *	
 * 创作日期:    2020-03-04
 * 
 * 备    注:   DDF200M数据协议定义
                                       
*********************************************************************************************/
using System;
using System.Runtime.InteropServices;

namespace Tracker800.Server.Device
{
    [Flags]
    internal enum FLAGS : uint
    {
        LEVEL = 0x1,
        OFFSET = 0x2,
        FSTRENGTH = 0x4,
        AM = 0x8,
        AM_POS = 0x10,
        AM_NEG = 0x20,
        FM = 0x40,
        FM_POS = 0x80,
        FM_NEG = 0x100,
        PM = 0x200,
        BANDWIDTH = 0x400,
        DF_LEVEL = 0x800,
        AZIMUTH = 0x1000,
        DF_QUALITY = 0x2000,
        DF_FSTRENGTH = 0x4000,
        CHANNEL = 0x00010000,
        FREQLOW = 0x00020000,
        ELEVATION = 0x00040000,
        DF_OMNIPHASE = 0x00100000,
        FREQHIGH = 0x00200000,
        BANDWIDTH_CENTER = 0x00400000,
        FREQ_OFFSET_REL = 0x00800000,
        PRIVATE = 0x10000000,
        SWAP = 0x20000000,              // swap ON means: do NOT swap (for little endian machines)
        SIGNAL_GREATER_SQUELCH = 0x40000000,
        OPTIONAL_HEADER = 0x80000000
    };

    [Flags]
    internal enum TAGS
    {
        FSCAN = 101,
        MSCAN = 201,
        DSCAN = 301,
        AUDIO = 401,
        IFPAN = 501,
        FASTL = 601,
        LISTF = 701,
        CW = 801,
        IF = 901,
        VIDEO = 1001,
        VDPAN = 1101,
        PSCAN = 1201,
        SELCALL = 1301,
        DFPan = 1401,
        LAST_TAG
    };

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct EB200DatagramFormat
    {
        [MarshalAs(UnmanagedType.U4)]
        public UInt32 Magic;
        [MarshalAs(UnmanagedType.U2)]
        public UInt16 VersionMinor;
        [MarshalAs(UnmanagedType.U2)]
        public UInt16 VersionMajor;
        [MarshalAs(UnmanagedType.U2)]
        public UInt16 Sequence;
        [MarshalAs(UnmanagedType.U2)]
        public UInt16 reserved;
        [MarshalAs(UnmanagedType.U4)]
        public UInt32 DataSize;

        public EB200DatagramFormat(byte[] buffer, int index)
        {
            Magic = BitConverter.ToUInt32(buffer, index);
            VersionMinor = BitConverter.ToUInt16(buffer, index + 4);
            VersionMajor = BitConverter.ToUInt16(buffer, index + 6);
            Sequence = BitConverter.ToUInt16(buffer, index + 8);
            reserved = BitConverter.ToUInt16(buffer, index + 10);
            DataSize = BitConverter.ToUInt32(buffer, 14);
        }
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct GenericAttribute
    {
        [MarshalAs(UnmanagedType.U2)]
        public short tag;
        [MarshalAs(UnmanagedType.U2)]
        public ushort length;
        public GenericAttribute(byte[] buffer, int index)
        {
            Array.Reverse(buffer, index, 2);
            tag = BitConverter.ToInt16(buffer, index);
            Array.Reverse(buffer, index + 2, 2);
            length = BitConverter.ToUInt16(buffer, index + 2);
        }
    };

    //advanced attribute type. Tag > 5000
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct GenericAttributeA
    {
        [MarshalAs(UnmanagedType.U2)]
        public ushort tag;
        [MarshalAs(UnmanagedType.U2)]
        public ushort reserved;
        [MarshalAs(UnmanagedType.U4)]
        public UInt32 length;
        [MarshalAs(UnmanagedType.U4)]
        public uint reserved0;
        [MarshalAs(UnmanagedType.U4)]
        public uint reserved1;
        [MarshalAs(UnmanagedType.U4)]
        public uint reserved2;
        [MarshalAs(UnmanagedType.U4)]
        public uint reserved3;

        public GenericAttributeA(byte[] value, int startIndex)
        {
            //TODO: lx确定此处是否需要转换
            Array.Reverse(value, startIndex, 2);
            tag = BitConverter.ToUInt16(value, startIndex);
            Array.Reverse(value, startIndex + 2, 2);
            reserved = BitConverter.ToUInt16(value, startIndex + 2);
            Array.Reverse(value, startIndex + 4, 4);
            length = BitConverter.ToUInt32(value, startIndex + 4);
            Array.Reverse(value, startIndex + 8, 4);
            reserved0 = BitConverter.ToUInt32(value, startIndex + 8);
            Array.Reverse(value, startIndex + 12, 4);
            reserved1 = BitConverter.ToUInt32(value, startIndex + 12);
            Array.Reverse(value, startIndex + 16, 4);
            reserved2 = BitConverter.ToUInt32(value, startIndex + 16);
            Array.Reverse(value, startIndex + 20, 4);
            reserved3 = BitConverter.ToUInt32(value, startIndex + 20);
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct TraceAttribute
    {
        [MarshalAs(UnmanagedType.I2)]
        public Int16 number_of_trace_items;
        [MarshalAs(UnmanagedType.U1)]
        public byte ChannelNumber;
        [MarshalAs(UnmanagedType.U1)]
        public byte optional_header_length;
        [MarshalAs(UnmanagedType.U4)]
        public int selectorFlags;
        public TraceAttribute(byte[] buffer, int offset)
        {
            Array.Reverse(buffer, offset, 2);
            number_of_trace_items = BitConverter.ToInt16(buffer, offset);
            ChannelNumber = buffer[offset + 2];
            optional_header_length = buffer[offset + 3];
            Array.Reverse(buffer, offset + 4, 4);
            selectorFlags = BitConverter.ToInt32(buffer, offset + 4);
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct OptionalHeaderIF
    {
        //SYSTem:IF:REMote:MODE OFF|SHORT|LONG
        [MarshalAs(UnmanagedType.I2)]
        public Int16 IFMode;
        [MarshalAs(UnmanagedType.I2)]
        public Int16 FrameLen;
        [MarshalAs(UnmanagedType.U4)]
        public UInt32 Samplerate;
        [MarshalAs(UnmanagedType.U4)]
        public UInt32 FrequencyLow;
        [MarshalAs(UnmanagedType.U4)]
        public UInt32 Bandwidth;//IF bandwidth
        [MarshalAs(UnmanagedType.U2)]
        public UInt16 Demodulation;
        [MarshalAs(UnmanagedType.I2)]
        public Int16 RxAttenuation;
        [MarshalAs(UnmanagedType.U2)]
        public UInt16 Flags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
        public string sDemodulation;
        [MarshalAs(UnmanagedType.U8)]
        public UInt64 SampleCount;
        [MarshalAs(UnmanagedType.U4)]
        public UInt32 FrequencyHigh;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 4)]
        public byte[] reserved;
        [MarshalAs(UnmanagedType.U8)]
        public UInt64 StartTimestamp;
        [MarshalAs(UnmanagedType.I2)]
        public Int16 SignalSource;

        public OptionalHeaderIF(byte[] value, int startIndex)
        {
            IFMode = BitConverter.ToInt16(value, startIndex);
            FrameLen = BitConverter.ToInt16(value, startIndex + 2);
            Samplerate = BitConverter.ToUInt32(value, startIndex + 4);
            FrequencyLow = BitConverter.ToUInt32(value, startIndex + 8);
            Bandwidth = BitConverter.ToUInt32(value, startIndex + 12);
            Demodulation = BitConverter.ToUInt16(value, startIndex + 16);
            RxAttenuation = BitConverter.ToInt16(value, startIndex + 18);
            Flags = BitConverter.ToUInt16(value, startIndex + 20);
            sDemodulation = BitConverter.ToString(value, startIndex + 22, 8);
            SampleCount = BitConverter.ToUInt64(value, startIndex + 30);
            FrequencyHigh = BitConverter.ToUInt32(value, startIndex + 38);
            reserved = new byte[4];
            StartTimestamp = BitConverter.ToUInt64(value, startIndex + 46);
            SignalSource = BitConverter.ToInt16(value, startIndex + 54);
        }
    }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct OptionalHeaderFScan
    {
        [MarshalAs(UnmanagedType.I2)]
        public short cycleCount;
        [MarshalAs(UnmanagedType.I2)]
        public short holdTime;
        [MarshalAs(UnmanagedType.I2)]
        public short dwellTime;
        [MarshalAs(UnmanagedType.I2)]
        public short directionUp;
        [MarshalAs(UnmanagedType.I2)]
        public short stopSignal;
        [MarshalAs(UnmanagedType.U4)]
        public uint startFrequency;
        [MarshalAs(UnmanagedType.U4)]
        public uint stopFrequency;
        [MarshalAs(UnmanagedType.U4)]
        public uint stepFrequency;
        public OptionalHeaderFScan(byte[] buffer, int offset)
        {
            cycleCount = BitConverter.ToInt16(buffer, offset);
            holdTime = BitConverter.ToInt16(buffer, offset + 2);
            dwellTime = BitConverter.ToInt16(buffer, offset + 4);
            directionUp = BitConverter.ToInt16(buffer, offset + 6);
            stopSignal = BitConverter.ToInt16(buffer, offset + 8);
            startFrequency = BitConverter.ToUInt32(buffer, offset + 10);
            stopFrequency = BitConverter.ToUInt32(buffer, offset + 14);
            stepFrequency = BitConverter.ToUInt32(buffer, offset + 18);
        }
    }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct OptionalHeaderMScan
    {
        [MarshalAs(UnmanagedType.I2)]
        public Int16 cycleCount;
        [MarshalAs(UnmanagedType.I2)]
        public Int16 holdTime;
        [MarshalAs(UnmanagedType.I2)]
        public Int16 dwellTime;
        [MarshalAs(UnmanagedType.I2)]
        public Int16 directionUp;
        [MarshalAs(UnmanagedType.I2)]
        public Int16 stopSignal;
        [MarshalAs(UnmanagedType.U4)]
        public UInt32 reserved1;
        [MarshalAs(UnmanagedType.U2)]
        public UInt16 reserved2;
        [MarshalAs(UnmanagedType.U8)]
        public UInt64 outputTimestamp;
        //[MarshalAs(UnmanagedType.U8)]
        //public UInt64 startFreq;
        //[MarshalAs(UnmanagedType.U8)]
        //public UInt64 stopFreq;   
        public OptionalHeaderMScan(byte[] buffer, int offset)
        {
            cycleCount = BitConverter.ToInt16(buffer, offset);
            holdTime = BitConverter.ToInt16(buffer, offset + 2);
            dwellTime = BitConverter.ToInt16(buffer, offset + 4);
            directionUp = BitConverter.ToInt16(buffer, offset + 6);
            stopSignal = BitConverter.ToInt16(buffer, offset + 8);
            reserved1 = BitConverter.ToUInt32(buffer, offset + 12);
            reserved2 = BitConverter.ToUInt16(buffer, offset + 14);
            outputTimestamp = BitConverter.ToUInt64(buffer, offset + 22);
            //startFreq = BitConverter.ToUInt64(buffer, offset + 30);
            //stopFreq = BitConverter.ToUInt64(buffer, offset + 38);
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct OptionalHeaderPScan
    {
        [MarshalAs(UnmanagedType.U4)]
        public uint startFrequency;
        [MarshalAs(UnmanagedType.U4)]
        public uint stopFrequency;
        [MarshalAs(UnmanagedType.U4)]
        public uint stepFrequency;
        [MarshalAs(UnmanagedType.U4)]
        public uint startFrequencyHigh;
        [MarshalAs(UnmanagedType.U4)]
        public uint stopFrequencyHigh;
        [MarshalAs(UnmanagedType.U4)]
        public uint reserved;
        [MarshalAs(UnmanagedType.U8)]
        public UInt64 outputTimeStamp;
        [MarshalAs(UnmanagedType.U4)]
        public uint stepFreqNumerator;
        [MarshalAs(UnmanagedType.U4)]
        public uint stepFreqDenominator;
        [MarshalAs(UnmanagedType.U8)]
        public UInt64 firstFreq;
        [MarshalAs(UnmanagedType.U4)]
        public uint unknown;
        public OptionalHeaderPScan(byte[] buffer, int offset)
        {
            startFrequency = BitConverter.ToUInt32(buffer, offset);
            stopFrequency = BitConverter.ToUInt32(buffer, offset + 4);
            stepFrequency = BitConverter.ToUInt32(buffer, offset + 8);
            startFrequencyHigh = BitConverter.ToUInt32(buffer, offset + 12);
            stopFrequencyHigh = BitConverter.ToUInt32(buffer, offset + 16);
            reserved = BitConverter.ToUInt32(buffer, offset + 20);
            outputTimeStamp = BitConverter.ToUInt64(buffer, offset + 24);
            stepFreqNumerator = BitConverter.ToUInt32(buffer, offset + 32);
            stepFreqDenominator = BitConverter.ToUInt32(buffer, offset + 36);
            firstFreq = BitConverter.ToUInt64(buffer, offset + 40);
            unknown = BitConverter.ToUInt32(buffer, offset + 48);
        }
    }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct OptionalHeaderIFPan
    {
        [MarshalAs(UnmanagedType.U4)]
        public uint freqlow;
        [MarshalAs(UnmanagedType.U4)]
        public uint spanFrequency;
        [MarshalAs(UnmanagedType.I2)]
        public short avgtime;
        [MarshalAs(UnmanagedType.I2)]
        public short avgType;
        [MarshalAs(UnmanagedType.I4)]
        public int measureTime;
        [MarshalAs(UnmanagedType.U4)]
        public uint freqhigh;
        [MarshalAs(UnmanagedType.I4)]
        public int demFreqChan;
        [MarshalAs(UnmanagedType.U4)]
        public uint demFreqlow;
        [MarshalAs(UnmanagedType.U4)]
        public uint demFreqhigh;
        [MarshalAs(UnmanagedType.U8)]
        public ulong outputTimestamp;
        [MarshalAs(UnmanagedType.U4)]
        public uint stepFreqNumerator;
        [MarshalAs(UnmanagedType.U4)]
        public uint stepFreqDenominator;
        [MarshalAs(UnmanagedType.I2)]
        public short SignalSource;
        [MarshalAs(UnmanagedType.I2)]
        public short MeasureMode;
        [MarshalAs(UnmanagedType.U8)]
        public UInt64 MeasureTimestamp;
        [MarshalAs(UnmanagedType.I2)]
        public Int16 Selectivity;

        public OptionalHeaderIFPan(byte[] buffer, int offset)
        {
            freqlow = BitConverter.ToUInt32(buffer, offset);
            spanFrequency = BitConverter.ToUInt32(buffer, offset + 4);
            avgtime = BitConverter.ToInt16(buffer, offset + 8);
            avgType = BitConverter.ToInt16(buffer, offset + 10);
            measureTime = BitConverter.ToInt32(buffer, offset + 12);
            freqhigh = BitConverter.ToUInt32(buffer, offset + 16);
            demFreqChan = BitConverter.ToInt32(buffer, offset + 20);
            demFreqlow = BitConverter.ToUInt32(buffer, offset + 24);
            demFreqhigh = BitConverter.ToUInt32(buffer, offset + 28);
            outputTimestamp = BitConverter.ToUInt64(buffer, offset + 32);
            stepFreqNumerator = BitConverter.ToUInt32(buffer, offset + 36);
            stepFreqDenominator = BitConverter.ToUInt32(buffer, offset + 40);
            SignalSource = BitConverter.ToInt16(buffer, offset + 42);
            MeasureMode = BitConverter.ToInt16(buffer, offset + 44);
            MeasureTimestamp = BitConverter.ToUInt64(buffer, offset + 52);
            Selectivity = BitConverter.ToInt16(buffer, offset + 60);
        }
    }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct OptionalHeaderAudio
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

    /// <summary>
    /// GPS数据结构
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct GpsHeader
    {
        /* denotes whether GPS data are to be considered valid */
        [MarshalAs(UnmanagedType.I2)]
        public short bValid;
        /* number of satellites in view 0-12; only 
         * valid, if GGA msg is received, else -1 (GPS_UNDEFINDED) */
        [MarshalAs(UnmanagedType.I2)]
        public Int16 iNoOfSatInView;
        [MarshalAs(UnmanagedType.I2)]
        public Int16 iLatRef;         /* latitude direction ('N' or 'S') */
        [MarshalAs(UnmanagedType.I2)]
        public Int16 iLatDeg;         /* latitude degrees */
        [MarshalAs(UnmanagedType.R4)]
        public float fLatMin;         /* geographical latitude: minutes */
        [MarshalAs(UnmanagedType.I2)]
        public Int16 iLonRef;         /* longitude direction ('E' or 'W') */
        [MarshalAs(UnmanagedType.I2)]
        public Int16 iLonDeg;         /* longitude degrees */
        [MarshalAs(UnmanagedType.R4)]
        public float fLonMin;         /* geographical longitude: minutes */
        [MarshalAs(UnmanagedType.R4)]
        public float HDOP;
        public GpsHeader(byte[] buffer, int offset)
        {
            bValid = BitConverter.ToInt16(buffer, offset);
            iNoOfSatInView = BitConverter.ToInt16(buffer, offset + 2);
            iLatRef = BitConverter.ToInt16(buffer, offset + 4);
            iLatDeg = BitConverter.ToInt16(buffer, offset + 6);
            fLatMin = BitConverter.ToInt16(buffer, offset + 8);
            iLonRef = BitConverter.ToInt16(buffer, offset + 12);
            iLonDeg = BitConverter.ToInt16(buffer, offset + 14);
            fLonMin = BitConverter.ToInt16(buffer, offset + 16);
            HDOP = BitConverter.ToUInt16(buffer, offset + 20);
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct OptionalHeaderDFPan
    {
        [MarshalAs(UnmanagedType.U4)]
        public UInt32 Freq_low;
        [MarshalAs(UnmanagedType.U4)]
        public UInt32 Freq_high;
        [MarshalAs(UnmanagedType.U4)]
        public UInt32 FreqSpan;
        [MarshalAs(UnmanagedType.I4)]
        public Int32 DFThresholdMode;
        [MarshalAs(UnmanagedType.I4)]
        public Int32 DFThresholdValue;
        [MarshalAs(UnmanagedType.U4)]
        public UInt32 DFBandWidth;
        [MarshalAs(UnmanagedType.U4)]
        public UInt32 Stepwidth;
        [MarshalAs(UnmanagedType.I4)]
        public Int32 DFMeasureTime;
        [MarshalAs(UnmanagedType.I4)]
        public Int32 DFOption;
        [MarshalAs(UnmanagedType.U2)]
        public UInt16 CompassHeading;
        [MarshalAs(UnmanagedType.I2)]
        public Int16 CompassHeadingType;
        [MarshalAs(UnmanagedType.U4)]
        public UInt32 Reserved;
        [MarshalAs(UnmanagedType.I4)]
        public Int32 DemodFreqChannel;
        [MarshalAs(UnmanagedType.U4)]
        public UInt32 DemodFreq_low;
        [MarshalAs(UnmanagedType.U4)]
        public UInt32 DemodFreq_high;
        [MarshalAs(UnmanagedType.U8)]
        public UInt64 OutputTimeStamp;          // reserved for future use  
        [MarshalAs(UnmanagedType.Struct)]
        public GpsHeader GPSHeader;
        [MarshalAs(UnmanagedType.U4)]
        public UInt32 StepFreqNumerator;
        [MarshalAs(UnmanagedType.U4)]
        public UInt32 StepFreqDenominator;
        [MarshalAs(UnmanagedType.U8)]
        public UInt64 DFBandwidthHighRes;
        [MarshalAs(UnmanagedType.I2)]
        public Int16 Level;
        [MarshalAs(UnmanagedType.I2)]
        public Int16 Azimuth;
        [MarshalAs(UnmanagedType.I2)]
        public Int16 Quality;
        [MarshalAs(UnmanagedType.I2)]
        public Int16 Elevation;
        [MarshalAs(UnmanagedType.I2)]
        public Int16 Omniphase;
        public OptionalHeaderDFPan(byte[] buffer, int offset)
        {
            Freq_low = BitConverter.ToUInt32(buffer, offset);
            Freq_high = BitConverter.ToUInt32(buffer, offset + 4);
            FreqSpan = BitConverter.ToUInt32(buffer, offset + 8);
            DFThresholdMode = BitConverter.ToInt32(buffer, offset + 12);
            DFThresholdValue = BitConverter.ToInt32(buffer, offset + 16);
            DFBandWidth = BitConverter.ToUInt32(buffer, offset + 20);
            Stepwidth = BitConverter.ToUInt32(buffer, offset + 24);
            DFMeasureTime = BitConverter.ToInt32(buffer, offset + 28);
            DFOption = BitConverter.ToInt32(buffer, offset + 32);
            CompassHeading = BitConverter.ToUInt16(buffer, offset + 36);
            CompassHeadingType = BitConverter.ToInt16(buffer, offset + 38);
            Reserved = BitConverter.ToUInt32(buffer, offset + 40);
            DemodFreqChannel = BitConverter.ToInt32(buffer, offset + 44);
            DemodFreq_low = BitConverter.ToUInt32(buffer, offset + 48);
            DemodFreq_high = BitConverter.ToUInt32(buffer, offset + 52);
            OutputTimeStamp = BitConverter.ToUInt64(buffer, offset + 56);
            GPSHeader = new GpsHeader(buffer, offset + 64);
            StepFreqNumerator = BitConverter.ToUInt32(buffer, offset + 88);
            StepFreqDenominator = BitConverter.ToUInt32(buffer, offset + 92);
            DFBandwidthHighRes = BitConverter.ToUInt64(buffer, offset + 96);
            Level = BitConverter.ToInt16(buffer, offset + 104);
            Azimuth = BitConverter.ToInt16(buffer, offset + 106);
            Quality = BitConverter.ToInt16(buffer, offset + 108);
            Elevation = BitConverter.ToInt16(buffer, offset + 110);
            Omniphase = BitConverter.ToInt16(buffer, offset + 112);
        }
    }
}
