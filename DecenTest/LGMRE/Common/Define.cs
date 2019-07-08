using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace Common
{
    public delegate void DataArrivedDelegate(List<object> data);

    public delegate void DeviceStatusChangedDelegate(DeviceStatus status, string message);

    public class Define
    {
        public const string DEVICE_MR3300A = "MR3300A.MR3300A";

        public const string ANTENNA_CONFIGPATH = "\\Device\\";

        public const string DEVICE_PATH = "\\Device\\";

        #region 天线配置信息

        public const string ANTENNA_INDEX = "Index";
        public const string ANTENNA_CONTROLCODE = "ControlCode";
        public const string ANTENNA_ANTTYPE = "AntType";
        public const string ANTENNA_POLARITYTYPE = "PolarityType";
        public const string ANTENNA_STARTFREQUENCY = "StartFrequency";
        public const string ANTENNA_STOPFREQUENCY = "StopFrequency";
        public const string ANTENNA_FACTORFILE = "FactorFile";
        public const string ANTENNA_DESCRIPTION = "Description";

        #endregion 天线配置信息
    }

    /// <summary>
    /// 天线信息类
    /// </summary>
    [Serializable]
    [DataContract]
    public class AntennaInfo
    {
        #region 属性

        /// <summary>
        /// 天线ID
        /// </summary>
        public Guid ID { get; private set; }

        /// <summary>
        /// 天线索引
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 天线名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 天线打通码
        /// </summary>
        public string ControlCode { get; set; }

        /// <summary>
        /// 天线类型
        /// </summary>
        public AntennaType AntType { get; set; }

        /// <summary>
        /// 极化方式
        /// </summary>
        public PolarityType PolarityType { get; set; }

        /// <summary>
        /// 起始频率
        /// </summary>
        public double StartFrequency { get; set; }

        /// <summary>
        /// 停止频率
        /// </summary>
        public double StopFrequency { get; set; }

        /// <summary>
        /// 天线因子文件路径
        /// </summary>
        public string FactorFile { get; set; }

        /// <summary>
        /// 天线描述
        /// </summary>
        public string Description { get; set; }

        #endregion

        public AntennaInfo()
        {
            ID = Guid.NewGuid();
        }

        #region 公有方法

        /// <summary>
        /// 从因子文件中获取天线因子数据
        /// </summary>
        /// <returns>天线因子数据</returns>
        public SDataFactor GetFactor()
        {
            if (!File.Exists(FactorFile))
            {
                throw new FileNotFoundException("未找到因子文件", FactorFile);
            }

            var doc = new XmlDocument();
            doc.Load(FactorFile);
            List<FrequencyFactorPair> factors = FrequencyFactorPair.CreateFactors(doc);

            return ToSDataFactor(factors);
        }

        /// <summary>
        /// 运算符重载，装天线配置的字典类型转换成对应的天线模板对象
        /// </summary>
        /// <param name="dict">天线配置字典表</param>
        /// <returns>类型为天线码模板类的实例</returns>
        public static explicit operator AntennaInfo(Dictionary<string, object> dict)
        {
            if (dict == null)
            {
                return null;
            }

            var template = new AntennaInfo();
            var type = template.GetType();
            try
            {
                var properties = type.GetProperties();
                foreach (var property in properties)
                {
                    if (dict.ContainsKey(property.Name))
                    {
                        var value = dict[property.Name];
                        property.SetValue(template, value, null);
                    }
                }
            }
            catch { }

            return template;
        }

        /// <summary>
        /// 运算符重载，将天线转换为字符串描述
        /// </summary>
        /// <param name="antennaTemplate">天线模板对象</param>
        /// <returns></returns>
        public static implicit operator string(AntennaInfo antennaTemplate)
        {
            return antennaTemplate.Name;
        }

        #endregion

        #region 辅助方法

        private SDataFactor ToSDataFactor(List<FrequencyFactorPair> factors)
        {
            if (factors == null || factors.Count == 0)
            {
                throw new ArgumentNullException("空的因子数据");
            }

            var step = (long)(factors[0].Frequency * 1000000);
            foreach (var factor in factors)
            {
                step = CalculateGCD((long)(factor.Frequency * 1000000), step);
            }
            var start = (long)(factors[0].Frequency * 1000000);
            var stop = (long)(factors[factors.Count - 1].Frequency * 1000000);

            var index = 0;
            var previousValue = factors[0];
            var currentValue = factors[0];
            var datas = new float[(stop - start) / step + 1];
            for (long i = 0; i < datas.Length; ++i)
            {
                long frequency = start + i * step;
                if (frequency > (long)(currentValue.Frequency * 1000000))
                {
                    previousValue = currentValue;
                    if ((++index) < factors.Count)
                    {
                        currentValue = factors[index];
                    }
                }

                if (frequency <= (long)(previousValue.Frequency * 1000000))
                {
                    datas[i] = previousValue.Factor;
                }
                else
                {
                    datas[i] = previousValue.Factor + (currentValue.Factor - previousValue.Factor) * (frequency - (long)(previousValue.Frequency * 1000000)) / ((long)(currentValue.Frequency * 1000000) - (long)(previousValue.Frequency * 1000000));
                }
            }

            var result = new SDataFactor
            {
                // 起始频率/结束频率，单位：MHz
                // 频率步进，单位：kHz
                StartFrequency = factors[0].Frequency,
                StopFrequency = factors[factors.Count - 1].Frequency,
                StepFrequency = step / 1000.0d,
                Datas = datas,
                Total = datas.Length
            };

            return result;
        }

        // 计算两个数的最大公约数
        private long CalculateGCD(long x, long y)
        {
            if (y == 0)
            {
                throw new DivideByZeroException();
            }

            var mod = x % y;
            while (mod != 0)
            {
                x = y;
                y = mod;
                mod = x % y;
            }

            return y;
        }

        #endregion
    }

    /// <summary>
    /// 频率-天线因子数据对结构 可从天线因子文件(XML)中解析 频率-因子 数据
    /// </summary>
    public struct FrequencyFactorPair
    {
        /// <summary>
        /// 频率(MHz)
        /// </summary>
        public double Frequency { get; set; }

        /// <summary>
        /// 因子
        /// </summary>
        public float Factor { get; set; }

        /// <summary>
        /// 获取天线因子数据 
        /// </summary>
        /// <param name="doc">因子文档信息</param>
        /// <returns>因子数据列表</returns>
        public static List<FrequencyFactorPair> CreateFactors(XmlDocument doc)
        {
            var factors = new List<FrequencyFactorPair>();
            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            {
                FrequencyFactorPair ffPair = CreateFactor(node);
                factors.Add(ffPair);
            }

            return factors;
        }

        /// <summary>
        /// 获取单个天线因子数据
        /// </summary>
        /// <param name="node">结点</param>
        /// <returns>因子数据</returns>
        private static FrequencyFactorPair CreateFactor(XmlNode node)
        {
            var data = new Dictionary<string, string>();
            foreach (XmlNode childNode in node)
            {
                data[childNode.Name] = childNode.InnerText;
            }

            var factor = new FrequencyFactorPair
            {
                Frequency = double.Parse(data["Freq"]),
                Factor = float.Parse(data["Value"])
            };

            return factor;
        }
    }

    /// <summary>
    /// 天线类型
    /// </summary>
    public enum AntennaType
    {
        [EnumMember(Value = "监测天线")]
        Monitor,

        [EnumMember(Value = "测向天线")]
        DFind
    }

    /// <summary>
    /// 极化方式
    /// </summary>
    public enum PolarityType
    {
        [EnumMember(Value = "垂直极化")]
        Vertical,

        [EnumMember(Value = "水平极化")]
        Horizontal
    }

    /// <summary>
    /// 设备状态
    /// </summary>
    public enum DeviceStatus
    {
        Normal,
        Busy,
        Fault,
    }

    /// <summary>
    /// 任务状态 也表示任务请求状态
    /// </summary>

    public enum TaskState
    {
        /// <summary>
        /// 启动
        /// </summary>        
        Start = 0,
        /// <summary>
        /// 暂停
        /// </summary>        
        Pause = 1,
        /// <summary>
        /// 停止
        /// </summary>        
        Stop = 2
    }


    /// <summary>
    /// 具体功能类型定义 支持与、或位运算
    /// </summary>
    [Flags]
    public enum SpecificAbility : long
    {
        /// <summary>
        /// 未定义的类型
        /// </summary>
        [EnumMember(Value = "其它")]
        Unknown = 0,

        /// <summary>
        /// 可对应 RMTP 中的"固定频率测量FIXFQ、单频测量SIGME"
        /// </summary>
        [EnumMember(Value = "单频测量")]
        FixFQ = 1,

        /// <summary>
        /// 可对应 RMTP 中的"中频测量IFFQ"
        /// </summary>
        [EnumMember(Value = "中频测量")]
        IFFQ = FixFQ << 1,

        /// <summary>
        /// 可对应 RMTP 中的"单频测向FIXDF、中频测向IF_DF"
        /// </summary>
        [EnumMember(Value = "单频测向")]
        FixDF = IFFQ << 1,

#warning "频段扫描和频点扫描是否拆分成两个模块，需要再讨论"
        /// <summary>
        /// 可对应 RMTP 中的"频段扫描FSCAN"
        /// </summary>
        [EnumMember(Value = "频段扫描")]
        SCAN = FixDF << 1,

        /// <summary>
        /// 可对应 RMTP 中的"离散扫描RMTP"
        /// </summary>
        [EnumMember(Value = "离散扫描")]
        MSCAN = SCAN << 1,

        /// <summary>
        /// 也就是驻留频段扫描 可对应 RMTP 中的"频段搜索FSCNE"
        /// </summary>
        [EnumMember(Value = "频段搜索")]
        FSCNE = MSCAN << 1,

        /// <summary>
        /// 也就是驻留离散扫描 可对应 RMTP 中的"离散搜索MSCNE"
        /// </summary>
        [EnumMember(Value = "离散搜索")]
        MSCNE = FSCNE << 1,

        /// <summary>
        /// TDOA定位功能
        /// </summary>
        [EnumMember(Value = "TDOA定位")]
        TDOA = MSCNE << 1,

        /// <summary>
        /// 中频多路分析功能
        /// </summary>
        [EnumMember(Value = "中频多路")]
        IFMultiChannel = TDOA << 1,

        /// <summary>
        /// 频段管制
        /// </summary>
        [EnumMember(Value = "频段管制")]
        ScanControl = IFMultiChannel << 1,

        /// <summary>
        /// 离散信道管制
        /// </summary>
        [EnumMember(Value = "信道管制")]
        MScanControl = ScanControl << 1,

        /// <summary>
        /// 一键式公众移动通信管制
        /// </summary>
        [EnumMember(Value = "公众移动通信管制")]
        MobileControl = MScanControl << 1,

        /// <summary>
        /// 远程开关机控制
        /// </summary>
        [EnumMember(Value = "环境控制")]
        RemoteControl = MobileControl << 1,

        /// <summary>
        /// 中频输出
        /// </summary>
        [EnumMember(Value = "中频输出")]
        IFOUT = RemoteControl << 1,

        /// <summary>
        /// 宽带测向
        /// </summary>
        [EnumMember(Value = "宽带测向")]
        WBDF = IFOUT << 1,

        /// <summary>
        /// 比幅测向
        /// </summary>
        [EnumMember(Value = "比幅测向")]
        AMPDF = WBDF << 1,

        /// <summary>
        /// 扫描比幅测向
        /// </summary>
        [EnumMember(Value = "扫描比幅测向")]
        ScanAMPDF = AMPDF << 1,

        /// <summary>
        /// 基站解码
        /// </summary>
        [EnumMember(Value = "基站解码")]
        BSDecoding = ScanAMPDF << 1,

        /// <summary>
        /// 电视解码
        /// </summary>
        [EnumMember(Value = "电视解码")]
        TVDecoding = BSDecoding << 1,

        /// <summary>
        /// 数字对讲机解码
        /// </summary>
        [EnumMember(Value = "数字对讲机解码")]
        DigitalDecoding = TVDecoding << 1,

        /// <summary>
        /// 扫描测向
        /// </summary>
        [EnumMember(Value = "扫描测向")]
        ScanDF = DigitalDecoding << 1,

        /// <summary>
        /// 信号侦察（目前应用于XE，以实现类似于LG319软件的一个组合功能)
        /// </summary>
        [EnumMember(Value = "信号侦察")]
        SignalRecon = ScanDF << 1,

        /// <summary>
        /// 快速扫描(目前应用于天津新版接收机)
        /// </summary>
        [EnumMember(Value = "快速扫描")]
        FastScan = SignalRecon << 1
    }
}
