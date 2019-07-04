using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeviceSim.Device
{
    /// <summary>
    /// EB200协议中的数据类型Tag
    /// </summary>
    [Flags]
    internal enum TAGS
    {
        /// <summary>
        /// 扫描
        /// </summary>
        FSCAN = 101,
        MSCAN = 201,
        /// <summary>
        /// 音频数据
        /// </summary>
        AUDIO = 401,
        /// <summary>
        /// 频谱数据
        /// </summary>
        IFPAN = 501,
        /// <summary>
        /// CW数据
        /// </summary>
        CW = 801,
        /// <summary>
        /// IQ数据
        /// </summary>
        IF = 901,
        VIDEO = 1001,
        VDPAN = 1101,
        PSCAN = 1201,
        SELECall = 1301,
        DFPAN = 1401,
        PIFPAN = 1601,
        GPSCompass = 1801,
        AntLevel = 1901,
        DFPScan = 5301,
        SIGP = 5501,
        HRPAN = 5601,
        LAST_TAG
    };

    /// <summary>
    /// SelectorFlags
    /// </summary>
    [Flags]
    internal enum FLAGS : uint
    {
        /// <summary>
        /// 1/10 dBμV
        /// </summary>
        LEVEL = 0x1,
        /// <summary>
        /// Hz
        /// </summary>
        OFFSET = 0x2,
        /// <summary>
        /// 1/10 dBμV/m
        /// </summary>
        FSTRENGTH = 0x4,
        /// <summary>
        /// 1/10 %
        /// </summary>
        AM = 0x8,
        /// <summary>
        /// 1/10 %
        /// </summary>
        AM_POS = 0x10,
        /// <summary>
        /// 1/10 %
        /// </summary>
        AM_NEG = 0x20,
        /// <summary>
        /// Hz
        /// </summary>
        FM = 0x40,
        /// <summary>
        /// Hz
        /// </summary>
        FM_POS = 0x80,
        /// <summary>
        /// Hz
        /// </summary>
        FM_NEG = 0x100,
        /// <summary>
        /// 1/100 rad
        /// </summary>
        PM = 0x200,
        /// <summary>
        /// Hz
        /// </summary>
        BANDWIDTH = 0x400,
        /// <summary>
        /// 1/10 dBμV
        /// </summary>
        DF_LEVEL = 0x800,
        /// <summary>
        /// 1/10 °
        /// </summary>
        AZIMUTH = 0x1000,
        /// <summary>
        /// 1/10 %
        /// </summary>
        DF_QUALITY = 0x2000,
        /// <summary>
        /// 1/10 dBμV/m
        /// </summary>
        DF_FSTRENGTH = 0x4000,
        /// <summary>
        /// 1/10 dBμV
        /// </summary>
        DF_LEVEL_CONT = 0x8000,
        CHANNEL = 0x00010000,
        FREQLOW = 0x00020000,
        /// <summary>
        /// 1/10 °
        /// </summary>
        ELEVATION = 0x00040000,
        DF_CHANNEL_STATUS = 0x80000,
        /// <summary>
        /// 1/10 °
        /// </summary>
        DF_OMNIPHASE = 0x00100000,
        FREQHIGH = 0x00200000,
        BANDWIDTH_CENTER = 0x00400000,
        FREQ_OFFSET_REL = 0x00800000,
        PRIVATE = 0x10000000,
        SWAP = 0x20000000,              // swap ON means: do NOT swap (for little endian machines)
        SIGNAL_GREATER_SQUELCH = 0x40000000,
        OPTIONAL_HEADER = 0x80000000
    };

    /// <summary>
    /// 解调模式 默认FM
    /// </summary>
    public enum EDemMode
    {
        AM,
        FM,
        PULS,
        PM,
        A0,
        IQ,
        ISB,
        A1,
        CW,
        LSB,
        USB,
        TV
    }

    /// <summary>
    /// 检波方式 默认电压平均值
    /// </summary>
    public enum EDetector
    {
        /// <summary>
        /// 峰值测量
        /// </summary>
        POS,
        /// <summary>
        /// 电压平均值
        /// </summary>
        PAV,
        /// <summary>
        /// 快速检波
        /// </summary>
        FAST,
        /// <summary>
        /// 功率平均值
        /// </summary>
        RMS
    }

    /// <summary>
    /// 自动衰减是否打开
    /// </summary>
    enum EAttenuationMode
    {
        ON, OFF
    }

    /// <summary>
    /// 射频模式 默认常规模式
    /// </summary>
    public enum ERFMode
    {
        /// <summary>
        /// 低噪声
        /// </summary>
        LOWN,
        /// <summary>
        /// 低失真
        /// </summary>
        LOWD,
        /// <summary>
        /// 常规模式 默认
        /// </summary>
        NORM
    }

    /// <summary>
    /// 增益模式
    /// </summary>
    public enum EGainTimeMode
    {
        /// <summary>
        /// 慢增益模式
        /// </summary>
        SLOW,
        /// <summary>
        /// 默认
        /// </summary>
        DEF,
        /// <summary>
        /// 快增益模式
        /// </summary>
        FAST
    }

    /// <summary>
    /// FFT模式
    /// </summary>
    public enum EFFTMode
    {
        MIN,
        MAX,
        SCAL,
        OFF
    }

    /// <summary>
    /// 增益模式
    /// </summary>
    public enum EGainMode
    {
        AUTO,
        FIX
    }

    public enum EBandMeasureMode
    {
        XDB,
        BETA
    }

    /// <summary>
    /// 视频输出模式
    /// </summary>
    public enum EVideoRemoteMode
    {
        OFF,
        SHORT,
        LONG,
        TRGS,
        TRGC
    }

    /// <summary>
    /// 设置IQ字节长度 
    /// </summary>
    public enum EIfRemoteMode
    {
        /// <summary>
        /// 默认 IQ开关关闭
        /// </summary>
        OFF,
        /// <summary>
        /// 16位IQ
        /// </summary>
        SHOR,
        /// <summary>
        /// 32位IQ
        /// </summary>
        LONG,
        /// <summary>
        /// 激活IF数据流的单个触发器。在发送触发器数据和if_modede的自动模式设置为OFF之后
        /// </summary>
        TRGS,
        /// <summary>
        /// 激活IF数据流的连续触发器。触发器触发后将自动重新激活
        /// </summary>
        TRGC,
        /// <summary>
        /// Ammos的16位IQ
        /// </summary>
        ASH,
        /// <summary>
        /// Ammos的32位IQ
        /// </summary>
        ALON
    }

    /// <summary>
    /// 通过选择IF全景图中的FFT过滤器特性来设置IF全景图中的选择性。
    /// </summary>
    public enum EIfpanSel
    {
        /// <summary>
        /// 自动
        /// Sets automatic selection. With AUTO the selectivity is set automatically depending on the receiver mode DF or AF. 
        /// </summary>
        AUTO,
        /// <summary>
        /// Sets normal selection
        /// </summary>
        NORM,
        /// <summary>
        /// Sets narrow selection. 
        /// </summary>
        NARR,
        /// <summary>
        /// Sets sharp selection. 
        /// </summary>
        SHAR
    }
}
