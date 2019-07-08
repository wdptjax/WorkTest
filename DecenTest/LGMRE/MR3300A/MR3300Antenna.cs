/*********************************************************************************************
 *	
 * 文件名称:    ..\Tracker800\Server\Source\Device\Receiver\MR3300A\MR3000Antenna.cs
 *
 * 作    者:		陈鹏 
 *	
 * 创作日期:    2018/05/17
 * 
 * 修    改:    2019-03-13 吴德鹏 配合天线控制重构进行了修改
 * 
 * 备    注:		MR3000A系列接收机天线控制与配置
 *                                            
*********************************************************************************************/
using Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace MR3300A
{
    public partial class MR3300A
    {
        #region 成员变量

        private const string ANTENNA_CONFIG_FILE = "MR3300AAntenna.ini";
        private Guid _antennaID = Guid.Empty;   // 天线编号
        private AntennaInfoEx[] _monitorAntennas;

        #endregion

        #region IAntennaController


        // 发送天线码，符合要求的天线码应该与接收机天线名称保持一致
        public bool OpenAntenna(AntennaInfo antennaInfo)
        {
            if (_antennaID == antennaInfo.ID)
            {
                return true;
            }
            try
            {
                _antennaID = antennaInfo.ID;
                var antenna = _monitorAntennas.FirstOrDefault(x => x.ControlCode.Equals(antennaInfo.ControlCode));  // 根据天线码反向查询天线索引，虽然有点搞笑，但为了适配，也是没法了^_*
                if (antenna != null)
                {
                    SendCommand(string.Format("ANT:SEL:IND {0}", antenna.Index)); // 根据天线索引选择天线
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                var errorInfo = string.Format("天线码值解析失败,天线名: {0}, 天线码: {1}", antennaInfo.Name, antennaInfo.ControlCode);
                //var item = new LogItem(errorInfo, ex);
                //LogManager.Add(item);
                return false;
            }
        }

        #endregion

        #region 安装属性（监测天线配置）

        // 监测天线信息扩展类，为天线信息添加索引
        private class AntennaInfoEx : AntennaInfo
        {
            /// <summary>
            /// 按要求转换为符合添加天线的协议格式
            /// </summary>
            /// <returns></returns>
            public string ToProtocolFormat()
            {
                try
                {
                    var code = Convert.ToByte(ControlCode.Trim(), 16).ToString();
                    return string.Format("{0},1,0,{1}", Index, code);
                }
                catch
                {
                    return string.Empty;
                }
            }

            /// <summary>
            /// 对象的字符串表示方式
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return string.Format("{0},1,0x00,{1}", Index, ControlCode.Trim());
            }

            /// <summary>
            /// 隐式类型转换，将对象转换为字符串
            /// </summary>
            public static implicit operator string(AntennaInfoEx antenna)
            {
                return antenna.ToString();
            }

            /// <summary>
            /// 将父类成员拷贝到子类，创建子类
            /// </summary>
            /// <param name="parent"></param>
            /// <returns></returns>
            public static AntennaInfoEx Create(AntennaInfo parent)
            {
                var child = new AntennaInfoEx();

                var parentType = typeof(AntennaInfo);
                var properties = parentType.GetProperties();
                foreach (var property in properties)
                {
                    if (property.CanRead && property.CanWrite)
                    {
                        property.SetValue(child, property.GetValue(parent, null), null);
                    }
                }

                return child;
            }
        }

        #endregion

    }
}