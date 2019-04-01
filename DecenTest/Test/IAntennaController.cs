using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Test
{
    /// <summary>
    /// 天线控制器设备基础操作接口，所有的天线控制器设备模块必须实现该接口
    /// </summary>
    public interface IAntennaController
    {
        /// <summary>
        /// 获取/设置当前天线控制行为是“频率自动”、“极化手动”、“天线手动”
        /// 注：此参数在实际配置某个具体功能时，应该序列化为当前功能的“安装参数”	（“安装参数”先于“用户参数”设置，“用户参数”可能以“安装参数”不同的取值作为不同的分支条件）
        /// </summary>
        AntennaSelectedType AntennaSelectedType { get; set; }

        /// <summary>
        /// 获取/设置天线频率，适用于“频率自动”和“极化手动”条件下选择天线，以及获取特定天线特定频率下的天线因子
        /// 注：此为用户交互参数，需要用户在选天线时显示设置（通过UI由用户操作设置或通过接口直接设置）
        /// </summary>
        double Frequency { get; set; }

        /// <summary>
        /// 获取/设置天线极化方式，适用于“极化手动”条件下选择符合特定频率的天线
        /// 注：此为用户交互参数，当前功能如果将AntennaSelectedType配置为“极化手动”时，需要将此参数暴露到客户端；否则需要对客户端隐藏此参数
        /// </summary>
        PolarityType PolarityType { get; set; }

        /// <summary>
        /// 获取/设置选中的天线，适用于“天线手动”条件
        /// 注：此为用户交互参数，当前功能如果将AntennaSelectedType配置为“天线手动”时，需要将此参数暴露到客户端；否则需要对客户端隐藏此参数
        /// </summary>
        Guid AntennaID { get; set; }

        /// <summary>
        /// 获取/设置当前可用的天线集合
        /// 注：此天线集合不一定为天线控制器配置的所有天线，特定的功能可能只使用了所有天线集合的一个子集
        ///		此参数在实际配置某个具体功能时，应该序列化为当前功能的“安装参数”	（“安装参数”先于“用户参数”设置，“用户参数”可能以“安装参数”不同的取值作为不同的分支条件）
        /// </summary>
        List<AntennaInfo> Antennas { get; set; }
        
        /// <summary>
        /// 获取/设置当前天线控制器的子控制器
        /// 天线控制器可以配置为级联,每个天线控制器可以配置多个子控制器
        /// 需要有控制码来打通到子控制器的通道
        /// </summary>
        List<ChildController> ChildControllers { get; set; }

        /// <summary>
        /// 发送天线控制码，打通/关闭天线
        /// </summary>
        /// <param name="code">天线码，以十六进制字节码的字符串形式给出</param>
        /// <returns>成功返回True，否则返回False</returns>
        bool SendControlCode(string code);

        /// <summary>
        /// 获取本天线控制器下属的所有天线,包括子天线控制器的天线
        /// </summary>
        /// <returns></returns>
        List<AntennaInfo> GetAllAntennas();

        /// <summary>
        /// 任务开启的时候将功能下属的天线设置到天线控制器中
        /// </summary>
        /// <param name="antennas"></param>
        void SetAntennas(List<AntennaInfo> antennas);

        /// <summary>
        /// 根据天线ID打通天线
        /// </summary>
        /// <param name="antennaId"></param>
        /// <returns></returns>
        bool SetAntennaId(Guid antennaId);
        
        /// <summary>
        /// 根据极化方式打通天线
        /// </summary>
        /// <param name="frequency"></param>
        /// <param name="polarityType"></param>
        /// <returns></returns>
        bool SetPolarityType(double frequency, PolarityType polarityType);

        /// <summary>
        /// 根据频率打通天线
        /// </summary>
        /// <param name="frequency"></param>
        /// <returns></returns>
        bool SetFrequency(double frequency);
    }

    /// <summary>
    /// IAntennaController接口扩展类（方法）
    /// 封装所有继承自该接口的公共处理逻辑（重复代码），模拟实现类的多重继承
    /// </summary>
    public static class AntennaControllerExtension
    {


        /// <summary>
        /// 根据频率和极化方式打通天线
        /// </summary>
        /// <param name="antennaController">天线控制器</param>
        /// <param name="frequency">频率</param>
        /// <param name="polarityType">极化方式</param>
        /// <param name="antennaID">天线编号</param>
        /// <remarks>函数返回后，通过antennaID返回打通的天线，如果天线打通失败，则保持之前已经选通的天线</remarks>
        public static bool OpenAntenna(this IAntennaController antennaController, double frequency, PolarityType polarityType, ref Guid antennaID)
        {
            var keyValuePair = GetControlCode(antennaController, frequency, polarityType);
            if (keyValuePair != null)
            {
                if (antennaID != keyValuePair.Value.Key)
                {
                    //保证通过天线手动和极化手动AntennaID一致即不管SendControlCode成不成功AntennaID都为新的天线ID,以保证子类在重载的SendControlCode函数中能通过AntennaID准确的提示出出错信息
                    //此处必须明确设置当前选中的天线编号，以便通过IAntennaController.GetFactor时，能准确获取已打通天线的因子数据
                    antennaID = keyValuePair.Value.Key;
                    antennaController.SendControlCode(keyValuePair.Value.Value);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 根据天线编号打通天线
        /// </summary>
        /// <param name="antennaController">天线控制器</param>
        /// <param name="antennaID">当前天线编号</param>
        /// <param name="dstAntennaID">目标天线编号</param>
		public static void OpenAntenna(this IAntennaController antennaController, ref Guid antennaID, Guid dstAntennaID)
        {
            var keyValuePair = GetControlCode(antennaController, dstAntennaID);
            if (keyValuePair != null)
            {
                if (antennaID != dstAntennaID)
                {
                    antennaID = dstAntennaID;
                    antennaController.SendControlCode(keyValuePair.Value.Value);
                }
            }
        }

        /// <summary>
        /// 频率自动或极化手动获取天线控制码
        /// </summary>
        /// <param name="antennaController">天线控制器接口</param>
        /// <param name="frequency">天线频率</param>
        /// <param name="polarityType">极化方式</param>
        /// <returns>成功返回具体的天线编码与码值对，否则返回空字</returns>
        public static KeyValuePair<Guid, string>? GetControlCode(this IAntennaController antennaController, double frequency, PolarityType polarityType)
        {
            // 可供选择的天线集合为空，则天线控制码信息为空
            if (antennaController.Antennas == null || antennaController.Antennas.Count == 0)
            {
                return null;
            }

            AntennaInfo antenna = null;
            KeyValuePair<Guid, string>? keyValuePair = null;
            if (antennaController.AntennaSelectedType == AntennaSelectedType.Auto) // 频率自动，和极化方式无关，在当前天线列表里面找到第一个适用于当前频率的天线
            {
                antenna = antennaController.Antennas.FirstOrDefault<AntennaInfo>(item => frequency >= item.StartFrequency && frequency <= item.StopFrequency);
            }
            else if (antennaController.AntennaSelectedType == AntennaSelectedType.PolarityManual) // 极化方式手动，需要参考当前设置的频率，并在天线列表里面找到第一个同时满足频率和极化方式的天线
            {
                antenna = antennaController.Antennas.FirstOrDefault<AntennaInfo>(item => item.PolarityType.Equals(polarityType) && frequency >= item.StartFrequency && frequency <= item.StopFrequency);
                // 如果没有找到同时符合极化方式与频率范围内的天线，则只使用第一根满足极化方式要求的天线
                if (antenna == null)
                {
                    antenna = antennaController.Antennas.FirstOrDefault<AntennaInfo>(item => item.PolarityType.Equals(polarityType));
                }
            }

            if (antenna != null)
            {
                keyValuePair = new KeyValuePair<Guid, string>(antenna.ID, antenna.ControlCode);
            }

            return keyValuePair;
        }

        /// <summary>
        /// 根据天线编号获取天线控制码
        /// </summary>
        /// <param name="antennaController">天线控制器接口</param>
        /// <param name="antennaID">天线编号</param>
        /// <returns>成功返回具体的天线编号与码值对，否则返回空</returns>
        public static KeyValuePair<Guid, string>? GetControlCode(this IAntennaController antennaController, Guid antennaID)
        {
            if (antennaController.Antennas == null || antennaController.Antennas.Count == 0 // 可供选择的天线集合为空，则天线控制码信息为空
                || antennaID.Equals(Guid.Empty))    // 天线编码为空，则天线控制码信息为空
            {
                return null;
            }

            KeyValuePair<Guid, string>? keyValuePair = null;
            var antenna = antennaController.Antennas.FirstOrDefault<AntennaInfo>(item => item.ID.Equals(antennaID));
            if (antenna != null)
            {
                keyValuePair = new KeyValuePair<Guid, string>(antenna.ID, antenna.ControlCode);
            }

            return keyValuePair;
        }
    }
    public class AntennaControllerBase : IAntennaController
    {
        private double _frequency; // 被测信号的天线频率（单频测量/测向为信号的中心频率，频段扫描为频段起始频率）
        private Guid _antennaID; // 选中天线对应的ID
        private PolarityType _polarityType; // 选择的极化方式
        private AntennaInfo[] _antennaTemplates; // 天线信息模版	   
        /// <summary>
        /// 获取或设置天线选择类型：频率自动、极化手动、天线手动
        /// </summary>
        public AntennaSelectedType AntennaSelectedType { get; set; }

        /// <summary>
        /// 获取或设置天线频率
        /// </summary>
        public double Frequency
        {
            get { return _frequency; }
            set
            {
                _frequency = value;
                SetFrequency(value);
            }
        }

        /// <summary>
        /// 获取或设置极化方式
        /// </summary>
        public PolarityType PolarityType
        {
            get { return _polarityType; }
            set
            {
                _polarityType = value;
                SetPolarityType(_frequency, value);
            }
        }

        public Guid AntennaID
        {
            get { return _antennaID; }
            set
            {
                SetAntennaId(value);
            }
        }
        /// <summary>
        /// 天线集合，集合中每一个元素都是一个字典类型，保存了天线信息的键值对
        /// </summary>
        [DisplayName("天线集合")]
        [Description("配置当前设备可用的所有天线。其中天线码的配置以\"|\"方式分隔十六进制节符的方式表示，如：0x01|0x02|0x03|0xFF，或01|02|03|FF")]
        public Dictionary<string, object>[] AntennaTemplates
        {
            get { return null; }
            set { _antennaTemplates = Array.ConvertAll(value, item => (AntennaInfo)item); }
        }

        public List<AntennaInfo> Antennas { get; set; }

        [DisplayName("子天线控制器集合")]
        [Description("配置当前设备下属连接的天线控制器")]
        public List<ChildController> ChildControllers
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public List<AntennaInfo> GetAllAntennas()
        {
            List<AntennaInfo> list = new List<AntennaInfo>();
            Antennas.ForEach(ant =>
            {
                list.Add(ant);
            });
            ChildControllers.ForEach(ctrl =>
            {
                list.AddRange(ctrl.ChildAntennaController.GetAllAntennas());
            });
            return list;
        }

        public void SetAntennas(List<AntennaInfo> antennas)
        {
            Antennas.ForEach(ant => ant.IsSelected = false);
            List<AntennaInfo> remainAnts = new List<AntennaInfo>();
            foreach (var ant in antennas)
            {
                AntennaInfo antenna = Antennas.FirstOrDefault(i => i.ID == ant.ID);
                if (antenna != null)
                {
                    antenna.IsSelected = true;
                    continue;
                }
                remainAnts.Add(ant);
            }
            if (remainAnts.Count == 0)
            {
                return;
            }
            ChildControllers.ForEach(ctrl =>
            {
                if (ctrl.ChildAntennaController != null)
                {
                    ctrl.ChildAntennaController.SetAntennas(remainAnts);
                }
            });
        }

        public bool SendControlCode(string code)
        {
            throw new NotImplementedException();
        }

        public bool SetAntennaId(Guid antennaId)
        {
            var ant = Antennas.FirstOrDefault(i => i.ID == antennaId);
            if (ant != null)
            {
                this.OpenAntenna(ref _antennaID, antennaId);
                return true;
            }
            else
            {
                bool isFindAnt = false;
                if (this.ChildControllers != null)
                {
                    foreach (var ctrl in ChildControllers)
                    {
                        if (ctrl.ChildAntennaController == null)
                        {
                            continue;
                        }
                        if (ctrl.ChildAntennaController.SetAntennaId(antennaId))
                        {
                            this.SendControlCode(ctrl.ControlCode);
                            isFindAnt = true;
                            break;
                        }
                    }
                }
                return isFindAnt;
            }
        }

        public bool SetFrequency(double frequency)
        {
            if (this.OpenAntenna(frequency, _polarityType, ref _antennaID))
            {
                return true;
            }
            bool isFind = false;
            if (this.ChildControllers != null)
            {
                foreach (var ctrl in ChildControllers)
                {
                    if (ctrl.ChildAntennaController == null)
                    {
                        continue;
                    }
                    if (ctrl.ChildAntennaController.SetFrequency(frequency))
                    {
                        this.SendControlCode(ctrl.ControlCode);
                        isFind = true;
                        break;
                    }
                }
            }
            return isFind;
        }

        public bool SetPolarityType(double frequency, PolarityType polarityType)
        {
            if (this.OpenAntenna(frequency, polarityType, ref _antennaID))
            {
                return true;
            }
            bool isFind = false;
            if (this.ChildControllers != null)
            {
                foreach (var ctrl in this.ChildControllers)
                {
                    if (ctrl.ChildAntennaController == null)
                    {
                        continue;
                    }
                    var res = ctrl.ChildAntennaController.SetPolarityType(frequency, polarityType);
                    if (res)
                    {
                        this.SendControlCode(ctrl.ControlCode);
                        isFind = true;
                        break;
                    }
                }
            }
            return isFind;
        }
    }

    /// <summary>
    /// 天线选择类型 由配置时由配置人员确定
    /// </summary>
    [Serializable]
    public enum AntennaSelectedType
    {
        /// <summary>
        /// .配置为自动选择天线时，不需向客户端暴露任何天线选择方式，由服务端内部依据频率选择性自动选择一根符合的天线
        /// </summary>
        [EnumMember(Value = "频率自动")]
        Auto,

        /// <summary>
        /// 配置为极化方式选择天线时，把当前天线管理器极化方式的所有天线列为枚举格式暴露给客户端，由客户端运行功能时自行选择
        /// </summary>
        [EnumMember(Value = "极化手动")]
        PolarityManual,

        /// <summary>
        /// 配置为手动选择天线时，把当前天线管理器所有天线列为枚举格式暴露给客户端，由客户端运行功能时自行选择
        /// </summary>
        [EnumMember(Value = "天线手动")]
        AntennaManual
    }

    /// <summary>
    /// 极化方式
    /// </summary>
    [Serializable]
    public enum PolarityType
    {
        [EnumMember(Value = "垂直极化")]
        Vertical,

        [EnumMember(Value = "水平极化")]
        Horizontal
    }

    /// <summary>
    /// 天线类型
    /// </summary>
    [Serializable]
    public enum AntennaType
    {
        [EnumMember(Value = "监测天线")]
        Monitor,

        [EnumMember(Value = "测向天线")]
        DFind
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
        /// 天线是否被公开到功能的天线列表
        /// </summary>
        public bool IsSelected { get; set; }

        [DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string ControlCode { get; set; }

        [DataMember]
        public AntennaType AntType { get; set; }

        [DataMember]
        public PolarityType PolarityType { get; set; }

        [DataMember]
        public double StartFrequency { get; set; }

        [DataMember]
        public double StopFrequency { get; set; }

        [DataMember]
        public string FactorFile { get; set; }

        [DataMember]
        public string Description { get; set; }

        #endregion

        #region 类方法

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
                        property.SetValue(template, dict[property.Name], null);
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

    public class ChildController
    {

        /// <summary>
        /// 子天线控制器ID
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// 打通子天线控制器的控制码
        /// </summary>
        public string ControlCode { get; set; }

        /// <summary>
        /// 子天线控制器
        /// </summary>
        public IAntennaController ChildAntennaController { get; set; }

    }

}
