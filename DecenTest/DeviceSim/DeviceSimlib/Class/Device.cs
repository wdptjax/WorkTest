
/*********************************************************************************************
 *	
 * 文件名称:    Device.cs
 *
 * 作    者:    吴德鹏
 *	
 * 创作日期:    2018-8-31 14:31:50
 * 
 * 备    注:    描述类的用途
 *              
 *                                
 *               
*********************************************************************************************/

using NotificationExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace DeviceSimlib
{
    public class Device : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region 属性

        public string Name
        {
            get { return _name; }
            set { _name = value; PropertyChanged.Notify(() => Name); }
        }

        #endregion 属性

        #region 字段

        private string _name = "DemoReceiver";

        #endregion 字段
    }
}
