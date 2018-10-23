
/*********************************************************************************************
 *	
 * 文件名称:    MessageInfo.cs
 *
 * 作    者:    吴德鹏
 *	
 * 创作日期:    2018-10-18 11:32:52
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
    public class MessageInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private DateTime _time;
        private string _message;
        public DateTime Time
        {
            get { return _time; }
            set
            {
                if (_time != value)
                {
                    _time = value;
                    PropertyChanged.Notify(() => this.Time);
                }
            }
        }
        public string Message
        {
            get { return _message; }
            set
            {
                if (_message != value)
                {
                    _message = value;
                    PropertyChanged.Notify(() => this.Message);
                }
            }
        }

        public MessageInfo(string message)
        {
            this.Message = message;
            Time = DateTime.Now;
        }
    }
}
