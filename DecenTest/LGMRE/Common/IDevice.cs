using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{

    public interface IDevice : IDisposable
    {
        /// <summary>
        /// 初始化设备
        /// </summary>
        /// <returns></returns>
        bool Initialize();

        /// <summary>
        /// 销毁设备实例
        /// </summary>
        /// <returns></returns>
        void Close();

        /// <summary>
        /// 批量设置参数
        /// </summary>
        /// <param name="parameters"></param>
        void SetParameters(Dictionary<string, object> parameters);

        /// <summary>
        /// 设置单个参数
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        void SetParameter(string name, object value);

        /// <summary>
        /// 启动任务
        /// </summary>
        /// <returns></returns>
        bool Start();

        /// <summary>
        /// 停止任务
        /// </summary>
        /// <returns></returns>
        bool Stop();

        /// <summary>
        /// 获取参数的值
        /// </summary>
        /// <param name="name">参数名</param>
        /// <returns>参数值</returns>
        object GetParameter(string name);

        /// <summary>
        /// 数据到达事件
        /// </summary>
        event DataArrivedDelegate DataArrivedEvent;

        /// <summary>
        /// 设备状态变化事件
        /// </summary>
        event DeviceStatusChangedDelegate DeviceStatusChangedEvent;
    }
}
