using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace TestNew
{
    public interface IController
    {
        /// <summary>
        /// 设备ID
        /// </summary>
        Guid ID { get; set; }
        /// <summary>
        /// 父控制器
        /// </summary>
        IAntennaController Owner { get; set; }

        /// <summary>
        /// 获取/设置父控制器的端子ID
        /// </summary>
        Guid OwnerConnectorId { get; set; }
    }

    /// <summary>
    /// 天线控制器设备基础操作接口，所有的天线控制器设备模块必须实现该接口
    /// </summary>
    public interface IAntennaController : IController
    {
        /// <summary>
        /// 获取/设置当前打通的端子
        /// </summary>
        Guid ConnectorId { get; set; }

        List<ConnectorInfo> Connectors { get; set; }

        /// <summary>
        /// 发送控制码，打通/关闭接口
        /// </summary>
        /// <param name="code">天线码，以十六进制字节码的字符串形式给出</param>
        /// <returns>成功返回True，否则返回False</returns>
        bool SendControlCode(string code);

        /// <summary>
        /// 根据端子ID打通某个端子
        /// </summary>
        /// <param name="connectorId"></param>
        /// <returns></returns>
        bool SetConnector(Guid connectorId);
    }

    public interface IAntenna : IController
    {

    }

    public class AntennaControllerBase : IAntennaController
    {
        public Guid ConnectorId
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

        public List<ConnectorInfo> Connectors
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

        public Guid ID
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

        public IAntennaController Owner
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

        public Guid OwnerConnectorId
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

        public bool SendControlCode(string code)
        {
            throw new NotImplementedException();
        }

        public bool SetConnector(Guid connectorId)
        {
            if (ConnectorId.Equals(connectorId))
            {
                return true;
            }

            ConnectorInfo info = Connectors?.FirstOrDefault(c => c.Id == connectorId);
            if (info == null)
            {
                return false;
            }

            bool res= SendControlCode(info.ControlCode);
            if (!res)
            {
                return false;
            }

            if (this.Owner == null)
            {
                return true;
            }

            return Owner.SetConnector(connectorId);
        }
    }

    /// <summary>
    /// 功能包含的天线集合
    /// </summary>
    public class AntennaCollection
    {
        /// <summary>
        /// 获取/设置当前天线控制行为是“频率自动”、“极化手动”、“天线手动”
        /// 注：此参数在实际配置某个具体功能时，应该序列化为当前功能的“安装参数”	（“安装参数”先于“用户参数”设置，“用户参数”可能以“安装参数”不同的取值作为不同的分支条件）
        /// </summary>
        public AntennaSelectedType AntennaSelectedType { get; set; }

        /// <summary>
        /// 获取/设置天线频率，适用于“频率自动”和“极化手动”条件下选择天线，以及获取特定天线特定频率下的天线因子
        /// 注：此为用户交互参数，需要用户在选天线时显示设置（通过UI由用户操作设置或通过接口直接设置）
        /// </summary>
        public double Frequency { get; set; }

        /// <summary>
        /// 获取/设置天线极化方式，适用于“极化手动”条件下选择符合特定频率的天线
        /// 注：此为用户交互参数，当前功能如果将AntennaSelectedType配置为“极化手动”时，需要将此参数暴露到客户端；否则需要对客户端隐藏此参数
        /// </summary>
        public PolarityType PolarityType { get; set; }

        /// <summary>
        /// 天线集合
        /// </summary>
        public List<IAntenna> Antennas { get; set; }

        public bool SetAntennaID(Guid antennaId)
        {
            IAntenna ant = Antennas.FirstOrDefault(i => i.ID == antennaId);
            if (ant == null|| ant.Owner == null)
            {
                return false;
            }

            return ant.Owner.SetConnector(ant.OwnerConnectorId);
        }
    }

    /// <summary>
    /// 天线控制器的端子信息
    /// </summary>
    public class ConnectorInfo
    {
        public int Index { get; set; }

        public Guid Id { get; set; }

        public string ControlCode { get; set; }
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

}
