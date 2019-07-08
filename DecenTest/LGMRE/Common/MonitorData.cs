using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

#pragma warning disable 1591
namespace Common
{
    /// <summary>
    /// 频段扫描、离散扫描的扫描数据 频段搜索和离散搜索见SDataDwellScan
    /// </summary>
    [Serializable]
    public class SDataScan
    {
        /// <summary>
        /// 多段扫描中的频段索引号 从0开始
        /// </summary>
        public int Segment;

        /// <summary>
        /// 起始频率 单位MHz，仅频段有意义
        /// </summary>
        public double StartFrequency;

        /// <summary>
        /// 结束频率 单位MHz，仅频段有意义
        /// </summary>
        public double StopFrequency;

        /// <summary>
        /// 频率步进 单位kHz，仅频段有意义
        /// </summary>
        public double StepFrequency;

        /// <summary>
        /// 扫描点数，频段扫描中此值始终为通过ScanCalculateFunctions.GetTotalCount计算出的点数，不可随意填写，否则会造成基类ReceiverBase中处理时出错
        /// </summary>
        public int Total;

        /// <summary>
        /// 该包数据在所有扫描点的位置
        /// </summary>
        public int Pdloc;

        /// <summary>
        /// 扫描数据，单位dBuV
        /// </summary>
        public float[] Datas;
    }

    /// <summary>
    /// 电平数据
    /// </summary>
    [Serializable]
    public class SDataLevel
    {
        /// <summary>
        /// 中心频率，单位MHz
        /// </summary>
        public double Frequency;
        /// <summary>
        /// 滤波带宽，单位kHz
        /// </summary>
        public double IFBandWidth;
        /// <summary>
        /// 电平，单位dBuV
        /// </summary>
        public float Data;
    }

    /// <summary>
    /// IQ数据
    /// </summary>
    [Serializable]
    public class SDataIQ
    {
        /// <summary>
        /// 中心频率，单位 MHz
        /// </summary>
        public double Frequency;

        /// <summary>
        /// 中频带宽，单位 kHz
        /// </summary>
        public double IFBandWidth;

        /// <summary>
        /// 采样率，单位 MHz
        /// </summary>
        public double SampleRate;

        /// <summary>
        /// 衰减，单位dBuV
        /// </summary>
        public float Attenuation;

        /// <summary>
        /// IQ数据，I分量和Q分量依次存储,适用于16位采样
        /// </summary>
        public short[] Datas;

        /// <summary>
        /// IQ数据，I分量和Q分量依次存储,适用于32位采样
        /// </summary>
        public int[] Datas32;
    }

    /// <summary>
    /// 调制识别数据结构
    /// </summary>
    [Serializable]
    public class SDataModRecognition
    {
        /// <summary>
        /// 中心频率，单位 MHz
        /// </summary>
        public double Frequency;

        /// <summary>
        /// 中频带宽，单位 kHz
        /// </summary>
        public double IFBandWidth;

        /// <summary>
        /// 采样率，单位 MHz
        /// </summary>
        public double SampleRate;

        /// <summary>
        /// 调制识别结果
        /// </summary>
        public RecognitionItem[] Datas;
    }

    [Serializable]
    public class RecognitionItem
    {
        /// <summary>
        /// 调制方式
        /// </summary>
        public string Name;

        /// <summary>
        /// 识别率，百分比
        /// </summary>
        public float Percentage;

        /// <summary>
        /// 调试方式描述
        /// </summary>
        public string Description;

        public RecognitionItem(string name, float percentage, string desc)
        {
            Name = name;
            Percentage = percentage;
            Description = desc;
        }
    }

    /// <summary>
    /// ITU建议的测量参数
    /// </summary>
    [Serializable]
    public class SDataITU
    {
        /// <summary>
        /// 中心频率 MHz 精确位数0.000001
        /// </summary>
        public double Frequency;

        /// <summary>
        /// β%测量法得出的带宽值 kHz，double.MinValue表示无效值 驱动认为设备返回小于2倍带宽才有效 精确位数0.1
        /// </summary>
        public double BetaBW;

        /// <summary>
        /// XdB测量法得出的带宽值 kHz，double.MinValue表示无效值 驱动认为设备返回小于2倍带宽才有效 精确位数0.1
        /// </summary>
        public double XdBBW;

        /// <summary>
        /// FM频偏 kHz，double.MinValue表示无效值 驱动认为设备返回小于2倍带宽才有效  精确位数0.1
        /// </summary>
        public double FMDev;

        /// <summary>
        /// FM正频偏 kHz，double.MinValue表示无效值 驱动认为设备返回小于2倍带宽才有效 精确位数0.1
        /// </summary>
        public double FMDevPos;

        /// <summary>
        /// FM负频偏 kHz，驱动取其绝对值本系统不出现负值 double.MinValue表示无效值 驱动认为设备返回小于2倍带宽才有效 精确位数0.1
        /// </summary>
        public double FMDevNeg;

        /// <summary>
        /// AM调幅度 %，double.MinValue表示无效值 范围[0, 100] 精确位数0.1
        /// </summary>
        public double AMDepth;

        /// <summary>
        /// PM调制度rad，double.MinValue表示无效值 范围(-2pi, 2pi])精确位数0.01
        /// </summary>
        public double PMDepth;

        /// <summary>
        /// 调制模式 DemoduMode.IQ为无效值
        /// </summary>
        public DemoduMode DemMode;

        /// <summary>
        /// 转换为字典表表示 
        /// </summary>
        /// <returns> key:显示名称 value:值</returns>
        public Dictionary<string, object> ToDictionary()
        {
            Dictionary<string, object> datas = new Dictionary<string, object>();

            datas.Add("中心频率(MHz)", Frequency);

            datas.Add("β带宽(kHz)", BetaBW);

            datas.Add("XdB带宽(kHz)", XdBBW);

            datas.Add("FM频偏(kHz)", FMDev);

            datas.Add("FM正频偏(kHz)", FMDevPos);

            datas.Add("FM负频偏(kHz)", FMDevNeg);

            datas.Add("AM调幅度(%)", AMDepth);

            datas.Add("PM调制度(rad)", PMDepth);

            datas.Add("调制模式", DemMode);

            return datas;
        }
    }

    /// <summary>
    /// 频谱数据
    /// </summary>
    [Serializable]
    public class SDataSpectrum
    {
        /// <summary>
        /// 中心频率 MHz
        /// </summary>
        public double Frequency;

        /// <summary>
        /// 频谱带宽，单位 kHz
        /// </summary>
        public double SpectrumSpan;

        /// <summary>
        /// 频谱数据
        /// </summary>
        public float[] Datas;
    }

    /// <summary>
    /// 音频数据
    /// </summary>
    [Serializable]
    public class SDataAudio
    {
        /// <summary>
        /// 中心频率,单位:MHz
        /// </summary>
        public double Frequency;
        /// <summary>
        /// 音频格式
        /// </summary>
        public AudioFormat Format;
        /// <summary>
        /// 音频数据
        /// </summary>
        public byte[] Datas;
    };

    /// <summary>
    /// GPS数据
    /// </summary>
    [Serializable]
    public class SDataGPS
    {
        /// <summary>
        /// GPS时间
        /// </summary>
        public DateTime Time;
        /// <summary>
        /// 经度，单位：度
        /// </summary>
        public double Longitude;
        /// <summary>
        /// 纬度，单位：度
        /// </summary>
        public double Latitude;
        /// <summary>
        /// 磁偏角，单位：度 0~360，该字段客户端暂未使用
        /// </summary>
        public float Declination;
        /// <summary>
        /// 海拔高度，单位：米 -9999.9~99999.9
        /// </summary>
        public float Altitude;
        /// <summary>
        /// GPS所接收的星数 ，单位：个 0~12
        /// </summary>
        public byte StarsNumber { get; set; }
        /// <summary>
        /// GPS运行速度 单位"km/小时"
        /// </summary>
        public float Speed { get; set; }
        /// <summary>
        /// GPS方向 单位：度 0~360
        /// </summary>
        public float Direction { get; set; }
    }

    /// <summary>
    /// Compass数据
    /// </summary>
    [Serializable]
    public class SDataCompass
    {
        /// <summary>
        /// 指示方向角值是地理北还是地磁北，true地理北，false地磁北。
        /// </summary>
        public bool IsGeographic;
        /// <summary>
        /// 相对于磁北方向角或相对地理北方向角，单位：度,0~360
        /// </summary>
        public float Angle;
    }

    /// <summary>
    /// 单频测向示向度数据
    /// </summary>
    [Serializable]
    public class SDataDFind
    {
        /// <summary>
        /// 中心频率 MHz
        /// </summary>
        public double Frequency;

        /// <summary>
        /// 测向带宽，单位 kHz
        /// </summary>
        public double DFBandwidth;

        /// <summary>
        /// 示相度，单位:度°
        /// </summary>
        public float Azimuth;

        /// <summary>
        /// 测向质量，单位%
        /// </summary>
        public float Quality;
    }

    /// <summary>
    /// 宽带(FFT)测向数据
    /// </summary>
    [Serializable]
    public class SDataWBDF
    {
        /// <summary>
        /// 中心频率 MHz
        /// </summary>
        public double Frequency;

        /// <summary>
        /// 频谱带宽，单位 kHz
        /// </summary>
        public double SpectrumSpan;

        /// <summary>
        /// 信道带宽，单位 kHz
        /// </summary>
        public double ChannelBandwidth;

        /// <summary>
        /// 测向结果，数组长度不定根据设备实际返回的值来
        /// </summary>
        public ChannelDFind[] Datas;
    }

    [Serializable]
    public class ChannelDFind
    {
        /// <summary>
        /// 信道号(从0开始)，即为在 SpectrumSpan/ChannelBandwidth 个信道数组中的索引
        /// </summary>
        public int ChannelNo;
        /// <summary>
        /// 示相度，单位:度° float.MinValue 表示无效值
        /// </summary>
        public float Azimuth;
        /// <summary>
        /// 测向质量，单位%
        /// </summary>
        public float Quality;
        /// <summary>
        /// 电平，单位dbuv
        /// </summary>
        public float Level;
    }

    /// <summary>
    /// 扫描测向数据
    /// </summary>
    [Serializable]
    public class SDataScanDF
    {
        /// <summary>
        /// 起始频率 MHz
        /// </summary>
        public double StartFrequency;

        /// <summary>
        /// 结束频率，单位 MHz
        /// </summary>
        public double StopFrequency;

        /// <summary>
        /// 扫描步进，单位kHz，即为信道带宽
        /// </summary>
        public double StepFrequency;

        /// <summary>
        /// 测向结果，数组长度不定根据设备实际返回的值来确定，ChannelNo即为频率在start-stop-step中对应的索引位置
        /// </summary>
        public ChannelDFind[] Datas;
    }

    /// <summary>
    /// 因子数据
    /// </summary>
    [Serializable]
    public class SDataFactor
    {
        /// <summary>
        /// 段索引号
        /// </summary>
        public int Segment;
        /// <summary>
        /// 起始频率 单位MHz，仅频段有意义
        /// </summary>
        public double StartFrequency;
        /// <summary>
        /// 结束频率 单位MHz，仅频段有意义
        /// </summary>
        public double StopFrequency;
        /// <summary>
        /// 频率步进 单位kHz，仅频段有意义
        /// </summary>
        public double StepFrequency;
        /// <summary>
        /// 数据个数
        /// </summary>
        public int Total;
        /// <summary>
        /// 天线因子数据
        /// </summary>
        public float[] Datas;
    }

    /// <summary>
    /// TDOA数据
    /// </summary>
    [Serializable]
    public class SDataTDOA
    {
        /// <summary>
        /// 数据时间戳
        /// </summary>
        public long TimeStamp;
        /// <summary>
        /// 中心频率  类型double 单位MHz
        /// </summary>
        public double Frequency;
        /// <summary>
        /// 滤波带宽 类型double 单位kHz
        /// </summary>
        public double IFBandWidth;
        /// <summary>
        /// 采样频率 类型double 单位MHz
        /// </summary>
        public double SampleRate;
        /// <summary>
        /// 衰减 类型float 单位dB
        /// </summary>
        public float Attenuation;
        /// <summary>
        /// IQ数据，I分量和Q分量依次存储,适用于16位采样
        /// </summary>
        public short[] Datas;
        /// <summary>
        /// IQ数据，I分量和Q分量依次存储,适用于32位采样
        /// </summary>
        public int[] Datas32;
    }

    /// <summary>
    /// 多路子通道数据 MCHChannel = Multi Child Channel
    /// </summary>
    [Serializable]
    public class SDataMCHChannel
    {
        /// <summary>
        /// 通道编号
        /// </summary>
        public int ChannelNo;
        /// <summary>
        /// 中心频率,单位:MHz
        /// </summary>
        public double Frequency;
        /// <summary>
        /// 滤波带宽,单位:kHz
        /// </summary>
        public double IFBandWidth;
        /// <summary>
        /// 通道数据 可能包含SDataLevel\SDataSpectrum\SDataAudio\SDataIQ\SDataITU等
        /// </summary>
        public List<object> Datas;
    }

    #region 遥控遥测数据

    /// <summary>
    /// 环境数据
    /// </summary>
    [Serializable]
    public class SDataEnvironment
    {
        /// <summary>
        /// 被控站点ID
        /// </summary>
        public Guid ControlledStationID;
        /// <summary>
        /// 开关状态信息
        /// 此属性可以表示多路开关，开关命名约束参考SwitchStatusInfo类，
        /// 通常至少包含system（系统开关）
        /// </summary>
        public SwitchStatusInfo[] Switches;
        /// <summary>
        /// 环境温度（单位：度）, float.MinValue为无效值
        /// </summary>
        public float Temperature;
        /// <summary>
        /// 环境湿度（单位：百分比）, float.MinValue为无效值
        /// </summary>
        public float Humidity;
        /// <summary>
        /// 安防报警信息
        /// </summary>
        public SecurityAlert SecurityAlert;

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public SDataEnvironment()
        {
            ControlledStationID = Guid.Empty;
            Switches = null;
            Temperature = float.MaxValue;
            Humidity = float.MaxValue;
            SecurityAlert = SecurityAlert.None;
        }
    }

    /// <summary>
    /// 遥控遥测开关状态信息
    /// 开关状态，开关在此处有两种状态
    /// 	1. 开状态，值为true,
    /// 	2. 关状态，值为false,
    /// Name为开关参数名，DisplayName为显示名称，常见开关做以下约定：
    /// 	1. SystemSwitch - 系统开关
    /// 	2. WIFISwitch - WiFi开关
    /// 	3. AirConditionSwitch - 空调开关
    /// 	4. FanSwitch - 风扇开关
    /// 	5. LampSwitch - 电灯开关
    /// 	6. 除此之外的其它任意字符串 - 其它开关
    /// 	7. AC*Switch - 交流*路设备开关（“*”为1,2,3...，显示名字可配置，比如*路上接的是ESMD设备则此处的显示名可写成“ESMD开关”）
    /// 	8. DC*Switch - 直流*路设备开关（同7，显示名可根据具体接的设备配置）
    /// Voltage、Voltage为电流电压信息，目前只有DC1000PMS可提供多路交直流信息，如果没有电流电压信息，该字段为默认无效值float.MinValue即可
    /// </summary>
    [Serializable]
    public class SwitchStatusInfo
    {
        /// <summary>
        /// 开关参数名称
        /// </summary>
        public string Name;
        /// <summary>
        /// 开关显示名称
        /// </summary>
        public string DisplayName;
        /// <summary>
        /// 状态是否为已开启，true为开启，false为关闭
        /// </summary>
        public bool On;
        /// <summary>
        /// 电压（单位：伏特），float.MinValue为无效值
        /// </summary>
        public float Voltage;
        /// <summary>
        /// 电流（单位：安培），float.MinValue为无效值
        /// </summary>
        public float Current;

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public SwitchStatusInfo()
        {
            Name = "Unknown";
            DisplayName = "未知";
            On = false;
            Voltage = float.MinValue;
            Current = float.MinValue;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name"></param>
        /// <param name="displayName"></param>
        /// <param name="on"></param>
        public SwitchStatusInfo(string name, string displayName, bool on)
        {
            Name = name;
            DisplayName = displayName;
            On = on;
            Voltage = float.MinValue;
            Current = float.MinValue;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name">开关参数名称</param>
        /// <param name="displayName">开关显示名称</param>
        /// <param name="on">开关状态</param>
        /// <param name="voltage">电压</param>
        /// <param name="current">电流</param>
        public SwitchStatusInfo(string name, string displayName, bool on, float voltage, float current)
        {
            Name = name;
            DisplayName = displayName;
            On = on;
            Voltage = voltage;
            Current = current;
        }
    }

    #endregion

    #region 天津接收机相关

    /// <summary>
    /// 快速扫描数据
    /// </summary>
    /// <remarks>返回的数据长度通常不一定等于整个频段的点数，而是尽可能多的反映存在的信号数量</remarks>
    [Serializable]
    public class SDataFastScan
    {
        /// <summary>
        /// 起始频率
        /// </summary>
        public double StartFrequency;
        /// <summary>
        /// 结束频率
        /// </summary>
        public double StopFrequency;
        /// <summary>
        /// 扫描步进
        /// </summary>
        public double StepFrequency;
        /// <summary>
        /// 信号幅度
        /// </summary>
        public float[] Signals;
        /// <summary>
        /// 背景噪声
        /// </summary>
        public float[] Noises;
        /// <summary>
        /// 信号/噪声对应的索引值
        /// </summary>
        public int[] Indices;
    }

    /// <summary>
    /// 短信数据
    /// </summary>
    public class SDataSMS
    {
        /// <summary>
        /// 中心频率
        /// </summary>
        public double Frequency;
        /// <summary>
        /// 解调带宽
        /// </summary>
        public double IFBandwidth;
        /// <summary>
        /// 采样率
        /// </summary>
        public double SampleRate;
        /// <summary>
        /// 色码
        /// </summary>
        public int ColourCode;
        /// <summary>
        /// 主叫号码
        /// </summary>
        public string CallingNumber;
        /// <summary>
        /// 被叫号码
        /// </summary>
        public string CalledNumber;
        /// <summary>
        /// 短信正文
        /// </summary>
        public string Text;
    }

    #endregion


    #region 枚举参数类型

    /// <summary>
    /// 音频格式
    /// </summary>
    [Serializable]
    public enum AudioFormat
    {
        /// <summary>
        /// PCM格式、16位、22.05K采样率、单音、运算快、音质好、41K/S
        /// </summary>
        [EnumMember(Value = "PCM")]
        PCM_MONO = 0,
        /// <summary>
        /// GSM610格式、16位、22.05K采样率、单音、运算快、音质差、4K/S
        /// </summary>
        [EnumMember(Value = "GSM610")]
        GSM610_MONO,
        /// <summary>
        /// MP3格式、56K平均比特率、22.05K采样率、单音、运算慢、音质普通、7K/S
        /// </summary>
        [EnumMember(Value = "MP3")]
        MP3_MONO,
        /// <summary>
        /// RMTP协议格式、16位、11.025K采样率、单音、运算快、音质差、4K/S
        /// </summary>
        [EnumMember(Value = "GSM610_RMTP")]
        GSM610_RMTP,
    }

    /// <summary>
    /// 射频模式
    /// </summary>
    public enum RFMode
    {
        [EnumMember(Value = "常规")]
        Normal,

        [EnumMember(Value = "低噪声")]
        LowNoise,

        [EnumMember(Value = "低失真")]
        LowDistort
    }

    /// <summary>
    /// 检波方式
    /// </summary>
    public enum DetectMode
    {
        [EnumMember(Value = "FAST")]
        FAST,

        [EnumMember(Value = "PEAK")]
        PEAK,

        [EnumMember(Value = "AVE")]
        AVE,

        [EnumMember(Value = "RMS")]
        RMS
    }

    /// <summary>
    /// 扫描模式
    /// </summary>
    public enum ScanMode
    {
        [EnumMember(Value = "全景扫描")]
        PSCAN,

        [EnumMember(Value = "频点扫描")]
        FSCAN,

        [EnumMember(Value = "离散扫描")]
        MSCAN
    }

    /// <summary>
    /// 测向模式
    /// </summary>
    public enum DFindMode
    {
        [EnumMember(Value = "常规")]
        Normal,

        [EnumMember(Value = "弱小信号")]
        Feebleness,

        [EnumMember(Value = "突出信号")]
        Gate
    }

    /// <summary>
    /// 安防报警
    /// </summary>
    [Flags]
    [Serializable]
    public enum SecurityAlert : int
    {
        /// <summary>
        /// 无报警
        /// </summary>
        [EnumMember(Value = "无报警")]
        None = 0x0,
        /// <summary>
        /// 烟雾报警
        /// </summary>
        [EnumMember(Value = "烟雾报警")]
        Smoke = 0x1,
        /// <summary>
        /// 火灾报警
        /// </summary>
        [EnumMember(Value = "火灾报警")]
        Fire = 0x2,
        /// <summary>
        /// 浸水报警
        /// </summary>
        [EnumMember(Value = "浸水报警")]
        Flooding = 0x4,
        /// <summary>
        /// 红外（活动物）报警
        /// </summary>
        [EnumMember(Value = "红外报警")]
        Infrared = 0x8,
        /// <summary>
        /// 门磁报警
        /// </summary>
        [EnumMember(Value = "门磁报警")]
        GateAccess = 0x10,
        /// <summary>
        /// 高温报警
        /// </summary>
        [EnumMember(Value = "高温报警")]
        Overtemperature = 0x20,
        /// <summary>
        /// 电流过载
        /// </summary>
        [EnumMember(Value = "电流过载")]
        CurrentOverload = 0x40,
        /// <summary>
        /// 电压过载
        /// </summary>
        [EnumMember(Value = "电压过载")]
        VoltageOverload = 0x80
    }

    /// <summary>
    /// 解调/调制 模式
    /// </summary>
    public enum DemoduMode
    {
        /// <summary>
        /// 未识别的
        /// </summary>
        [EnumMember(Value = "未知")]
        None,

        [EnumMember(Value = "调频")]
        FM,

        [EnumMember(Value = "调幅")]
        AM,

        [EnumMember(Value = "调相")]
        PM,

        [EnumMember(Value = "正交")]
        IQ,

        [EnumMember(Value = "连续波")]
        CW,

        [EnumMember(Value = "下边带")]
        LSB,

        [EnumMember(Value = "上边带")]
        USB,

        [EnumMember(Value = "双边带")]
        DSB,

        [EnumMember(Value = "单边带")]
        ISB,

        [EnumMember(Value = "残留边带")]
        VSB,

        [EnumMember(Value = "脉冲")]
        PULSE,

        [EnumMember(Value = "幅移键控")]
        ASK,

        [EnumMember(Value = "2幅移键控")]
        ASK2,

        [EnumMember(Value = "频移键控")]
        FSK,

        [EnumMember(Value = "最小频移键控")]
        MSK,

        [EnumMember(Value = "高斯最小频移键控")]
        GMSK,

        [EnumMember(Value = "2频移键控")]
        FSK2,

        [EnumMember(Value = "4频移键控")]
        FSK4,

        [EnumMember(Value = "相移键控")]
        PSK,

        [EnumMember(Value = "二相相移键控")]
        BPSK,

        [EnumMember(Value = "正交相移键控")]
        QPSK,

        [EnumMember(Value = "差分相移键控")]
        DPSK,

        [EnumMember(Value = "正交幅度")]
        QAM,

        [EnumMember(Value = "正交频分复用")]
        OFDM,

        [EnumMember(Value = "DMR数字解调")]
        DMR,

        [EnumMember(Value = "dPMR数字解调")]
        dPMR
    }

    #endregion
}
