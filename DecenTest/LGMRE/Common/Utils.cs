using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    public static class Utils
    {
        /// <summary>
        /// 计算扫描的总点数
        /// </summary>
        /// <param name="startFrequency">起始频率 MHz</param>
        /// <param name="stopFrequency">结束频率 MHz</param>
        /// <param name="stepFrequency">步进 kHz</param>
        /// <returns></returns>
        public static int GetTotalCount(double startFrequency, double stopFrequency, double stepFrequency)
        {
            decimal start = new decimal(startFrequency);
            decimal stop = new decimal(stopFrequency);
            decimal step = new decimal(stepFrequency / 1000.0d);
            int total = decimal.ToInt32((stop - start) / step) + 1;
            return total;
        }
    }
}
