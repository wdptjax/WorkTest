using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace ODM20181102TJ01
{
    public class Utils
    {
        /// <summary>
        /// 时间格式转换
        /// </summary>
        /// <param name="time">要转换的时间</param>
        /// <returns></returns>
        public static TimeInTwoWords ParseTime2TwoWords(DateTime time)
        {
            //需要将时间转换为UTC时间，然后计算从1970年1月1日开始的时间间隔，转换为微秒
            ulong uTime = (ulong)(time.ToUniversalTime().Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds * 1000);
            uint low = (uint)(uTime & 0xFFFFFFFF);
            uint high = (uint)((uTime & 0xFFFFFFFF00000000) >> 32);
            TimeInTwoWords bigTime = new TimeInTwoWords
            {
                HiOrderBits = high,
                LoOrderBits = low
            };
            return bigTime;
        }

        /// <summary>
        /// 校验发来的参数数据长度是否符合定义的长度
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="dataLength"></param>
        /// <returns></returns>
        public static bool CheckParamLength(byte mode, uint dataLength)
        {
            int len = 0;
            EDataMode dataMode = (EDataMode)mode;
            switch (dataMode)
            {
                case EDataMode.MODE_FFM:
                    len = Marshal.SizeOf(typeof(FfmParams));
                    break;
                case EDataMode.MODE_SCAN:
                    len = Marshal.SizeOf(typeof(ScanParams));
                    break;
                case EDataMode.MODE_STANDBY:
                    return true;
                case EDataMode.MODE_SEARCH:
                case EDataMode.MODE_TDMA:
                case EDataMode.MODE_CALIB:
                case EDataMode.MODE_DIAG:
                default:
                    return false;
            }

            return len == dataLength;
        }

        /// <summary>
        /// Tracker800中获取频段点数
        /// </summary>
        /// <param name="startFrequency"></param>
        /// <param name="stopFrequency"></param>
        /// <param name="stepFrequency"></param>
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
