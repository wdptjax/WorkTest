using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ODM20181102TJ01
{
    /// <summary>
    /// 数据转换 Tracker800=>0xE
    /// </summary>
    interface IOutDataConvert
    {
        /// <summary>
        /// 当前任务开启的功能
        /// </summary>
        EDataMode DataMode { get; set; }
        /// <summary>
        /// 包装任务数据
        /// </summary>
        /// <param name="data"></param>
        /// <returns>可能返回空集合，需要判断；不会返回null</returns>
        List<byte[]> PackupData(List<object> data);

        /// <summary>
        /// 包装GPS数据
        /// </summary>
        /// <param name="data"></param>
        /// <returns>可能返回null，需要判断</returns>
        byte[] PackupGpsData(List<object> data);
    }
}
