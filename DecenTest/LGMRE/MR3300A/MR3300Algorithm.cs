/*********************************************************************************************
 *	
 * 文件名称:    ..\Tracker800\Server\Source\Device\Receiver\MR3300A\MR3000Algorithm.cs
 *
 * 作    者:		陈鹏 
 *	
 * 创作日期:    2018/05/17
 * 
 * 修    改:    无
 * 
 * 备    注:		MR3000A系列接收机算法相关
 *                                            
*********************************************************************************************/

// #define DATA_VERIFICATION
// #define WRITE_DATA
// #define OUTPUT_LEVEL
// #define OUTPUT_PHASE
//#define ENHANCED_DFIND

using System;
using System.Numerics;

namespace MR3300A
{
    /// <summary>
   	/// <summary>
	/// 窗类型
	/// </summary>
	internal enum WindowType
    {
        /// <summary>
        /// 矩形窗
        /// </summary>
        Rectangle,

        /// <summary>
        /// 汉宁窗
        /// </summary>
        Hanning,

        /// <summary>
        /// 布莱克曼窗
        /// </summary>
        Blackman
    }

    /// 实用类
    /// </summary>
    internal static class Utilities
    {
        /// <summary>
        /// 取以2为底的对数
        /// </summary>
        public static int Log2n(int value)
        {
            var n = 0;
            while ((value >>= 1) > 0)
            {
                n++;
            }

            return n;
        }

        /// <summary>
        /// 取窗系数
        /// </summary>
        public static float Window(ref float[] data, WindowType windowType)
        {
            var PI = (float)Math.PI;
            var coe = 0.0f;
            var length = data.Length;

            switch (windowType)
            {
                case WindowType.Rectangle:
                    for (int i = 0; i < length; ++i)
                    {
                        data[i] = 1;
                    }
                    coe = -1;
                    break;
                case WindowType.Hanning:
                    {
                        float pi2l = PI * 2.0f / length;
                        for (var i = 0; i < length; ++i)
                        {
                            data[i] = (float)(1 - Math.Cos(pi2l * i)) / 2;
                        }
                    }
                    coe = 5.08f;
                    break;
                case WindowType.Blackman:
                    {
                        float pi2l = PI * 2.0f / length, pi4l = PI * 4.0f / length;
                        for (var i = 0; i < length; ++i)
                        {
                            data[i] = (float)(0.42f - 0.5f * Math.Cos(pi2l * i) + 0.08f * Math.Cos(pi4l * i));
                        }
                    }
                    coe = 6.60f;
                    break;
                default:
                    break;
            }

            return coe;
        }

        /// <summary>
        /// 取加窗后的数据（复数形式表式）
        /// </summary>
        public static Complex[] GetWindowData(float[] data, float[] windowValue, int length)
        {
            if (data == null || data.Length != length * 2
                || windowValue == null || windowValue.Length != length)
            {
                return null;
            }

            var outputData = new Complex[length];
            for (var index = 0; index < length; ++index)
            {
                outputData[index] = new Complex(data[2 * index], data[2 * index + 1]);
                outputData[index] *= windowValue[index];
            }

            return outputData;
        }

        /// <summary>
        /// FFT变换
        /// </summary>
        public static void FFT(ref Complex[] x)
        {
            int log2n = Log2n(x.Length);
            int i, j, k, n, nv2, nm1, l, le, le1, ip;
            float pi, ain;
            Complex t, u, w;
            n = 1 << log2n;
            nv2 = n >> 1;
            nm1 = n - 1;
            j = 0;
            for (i = 0; i < nm1; i++)
            {
                if (i < j)
                {
                    t = x[j];
                    x[j] = x[i];
                    x[i] = t;
                }
                k = nv2;
                while (k <= j)
                {
                    j -= k;
                    k >>= 1;
                }
                j += k;
            }
            pi = (float)Math.PI;
            for (l = 1; l <= log2n; l++)
            {
                le = 1 << l;
                le1 = le >> 1;
                ain = pi / le1;
                u = new Complex(1, 0);
                w = new Complex(Math.Cos(ain), -Math.Sin(ain));
                for (j = 0; j < le1; j++)
                {
                    for (i = j; i < n; i += le)
                    {
                        ip = i + le1;
                        t = x[ip] * u;
                        x[ip] = x[i] - t;
                        x[i] += t;
                    }
                    u *= w;
                }
            }
        }

        /// <summary>
        /// 获取电平
        /// </summary>
        public static float GetLevel(float[] data)
        {
            if (data == null || (data.Length % 2) != 0)
            {
                return 0.0f;
            }

            var EPSILON = 1.0E-7d;
            var length = data.Length / 2;
            var sum = 0.0d;
            var increment = 0;
            for (var index = 0; index < length; ++index)
            {
                var real = data[2 * index];
                var image = data[2 * index + 1];
                if (Math.Abs(real - 0.0f) > EPSILON || Math.Abs(image - 0.0f) > EPSILON)
                {
                    sum += Math.Log10(real * real + image * image);
                    increment++;
                }
            }
            if (increment == 0)
            {
                return 0.0f;
            }

            return (float)(10 * sum / increment);
        }
    }
}