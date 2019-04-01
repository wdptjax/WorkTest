using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tracker800.Server.Parameters;

namespace ODM20181102TJ01
{
    /// <summary>
    /// 参数转换 0xE=>Tracker800
    /// </summary>
    interface IParameterConverter
    {
        /// <summary>
        /// 当前任务开启的功能
        /// /// </summary>
        EDataMode DataMode { get; set; }

        /// <summary>
        /// 参数转换
        /// </summary>
        /// <param name="ffmParams"></param>
        /// <param name="parameterTable"></param>
        void ParamsConverter(IParameter parameters, ref ParameterTable parameterTable);

    }
}
