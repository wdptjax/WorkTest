
/*********************************************************************************************
 *	
 * 文件名称:    MainViewModel.cs
 *
 * 作    者:    吴德鹏
 *	
 * 创作日期:    2018-12-11 9:57:49
 * 
 * 备    注:    描述类的用途
 *              
 *                                
 *               
*********************************************************************************************/

using NotificationExtensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using System.Xml.Serialization;

namespace DellRemotingSwitch
{
    public class Device : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region  变量/属性

        #region 前台显示

        [XmlIgnore]
        private bool? _deviceStatus = null;
        private string _user = "root";
        private string _password = "calvin";
        private string _remoteIP = "192.168.0.120";
        [XmlIgnore]
        private bool _isRunning = false;
        [XmlIgnore]
        private ObservableCollection<CmdInfo> _sendCmdList = new ObservableCollection<CmdInfo>();
        [XmlIgnore]
        private ObservableCollection<CmdInfo> _recvCmdList = new ObservableCollection<CmdInfo>();
        [XmlIgnore]
        private int _sendCount = 0;
        [XmlIgnore]
        private int _recvCount = 0;
        private bool _noSafetyWarning = true;
        [XmlIgnore]
        private bool _pwdCheckOn = false;

        [XmlIgnore]
        private CmdInfo _currentCmd = null;
        [XmlIgnore]
        private bool _isSendingCmd = false;
        [XmlIgnore]
        private bool _isWaitingForCmd = false;

        /// <summary>
        /// 是否在监听中
        /// </summary>
        [XmlIgnore]
        public bool IsRunning
        {
            get { return _isRunning; }
            set
            {
                _isRunning = value;
                PropertyChanged.Notify(() => this.IsRunning);
            }
        }

        /// <summary>
        /// 设备状态 开机/关机/不确定
        /// </summary>
        [XmlIgnore]
        public bool? DeviceStatus
        {
            get { return _deviceStatus; }
            set
            {
                _deviceStatus = value;
                PropertyChanged.Notify(() => this.DeviceStatus);
            }
        }

        /// <summary>
        /// 用户名
        /// </summary>
        public string User
        {
            get { return _user; }
            set
            {
                _user = value;
                PropertyChanged.Notify(() => this.User);
            }
        }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                PropertyChanged.Notify(() => this.Password);
            }
        }

        /// <summary>
        /// 远程主机IP地址
        /// </summary>
        public string RemoteIP
        {
            get { return _remoteIP; }
            set
            {
                _remoteIP = value;
                PropertyChanged.Notify(() => this.RemoteIP);
            }
        }

        /// <summary>
        /// 发送队列
        /// </summary>
        [XmlIgnore]
        public ObservableCollection<CmdInfo> SendCmdList
        {
            get { return _sendCmdList; }
            set
            {
                _sendCmdList = value;
                PropertyChanged.Notify(() => this.SendCmdList);
            }
        }

        /// <summary>
        /// 接收队列
        /// </summary>
        [XmlIgnore]
        public ObservableCollection<CmdInfo> RecvCmdList
        {
            get { return _recvCmdList; }
            set
            {
                _recvCmdList = value;
                PropertyChanged.Notify(() => this.RecvCmdList);
            }
        }

        /// <summary>
        /// 发送计数
        /// </summary>
        [XmlIgnore]
        public int SendCount
        {
            get { return _sendCount; }
            set
            {
                _sendCount = value;
                PropertyChanged.Notify(() => this.SendCount);
            }
        }

        /// <summary>
        /// 接收计数
        /// </summary>
        [XmlIgnore]
        public int RecvCount
        {
            get { return _recvCount; }
            set
            {
                _recvCount = value;
                PropertyChanged.Notify(() => this.RecvCount);
            }
        }
        /// <summary>
        /// 当前正在执行的命令
        /// </summary>
        [XmlIgnore]
        public CmdInfo CurrentCmd
        {
            get { return _currentCmd; }
            set
            {
                _currentCmd = value;
                PropertyChanged.Notify(() => this.CurrentCmd);
            }
        }
        [XmlIgnore]
        public bool IsWaitingForCmd
        {
            get { return _isWaitingForCmd; }
            set
            {
                _isWaitingForCmd = value;
                PropertyChanged.Notify(() => this.IsWaitingForCmd);
            }
        }

        /// <summary>
        /// 忽略安全警告
        /// </summary>
        public bool NoSafetyWarning
        {
            get { return _noSafetyWarning; }
            set
            {
                _noSafetyWarning = value;
                PropertyChanged.Notify(() => this.NoSafetyWarning);
            }
        }

        /// <summary>
        /// 启用密码检查
        /// </summary>
        [XmlIgnore]
        public bool PasswordCheckOn
        {
            get { return _pwdCheckOn; }
            set
            {
                _pwdCheckOn = value;
                PropertyChanged.Notify(() => this.PasswordCheckOn);
            }
        }

        /// <summary>
        /// 命令发送中(限制一次只能进行一个操作命令)
        /// </summary>
        [XmlIgnore]
        public bool IsSendingCmd
        {
            get { return _isSendingCmd; }
            set
            {
                _isSendingCmd = value;
                PropertyChanged.Notify(() => this.IsSendingCmd);
            }
        }

        #endregion 前台显示

        #region 命令参数

        /// <summary>
        /// 开机命令
        /// </summary>
        const string CMD_POWERUP = "serveraction powerup";
        /// <summary>
        /// 关机命令
        /// </summary>
        const string CMD_POWERDOWN = "serveraction powerdown";
        /// <summary>
        /// 重启命令
        /// </summary>
        const string CMD_RESTART = "serveraction powercycle";
        /// <summary>
        /// 查询设备状态
        /// </summary>
        const string CMD_STATUS = "serveraction powerstatus";
        /// <summary>
        /// 忽略密码警告
        /// </summary>
        const string CMD_WARNING_NOPWD = "set iDRAC.Tuning.DefaultCredentialWarning";
        /// <summary>
        /// 忽略安全警告
        /// </summary>
        const string SWITCH_WARNING_SAFETY = "--nocertwarn";

        #endregion 命令参数

        [XmlIgnore]
        Dispatcher _dispatcher = null;


        private object _lockSendList = new object();
        private object _lockRecvList = new object();
        private DateTime _lastStatusTime = DateTime.Now;

        #endregion 变量/属性

        public Device()
        {

        }

        public Device(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public void UpdateDispatcher(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        /// <summary>
        /// 启动监听
        /// </summary>
        public void Start()
        {
            if (!File.Exists(DeviceManager.GetInstance().RacadmPath))
            {
                MessageBox.Show("Racadm路径错误!", "Error");
                return;
            }
            if (!IsRunning)
            {
                IsRunning = true;
                _lastStatusTime = DateTime.Now.AddHours(-1);
                Thread thd = new Thread(Scan);
                thd.IsBackground = true;
                thd.Start();
            }
        }

        /// <summary>
        /// 停止监听
        /// </summary>
        public void Stop()
        {
            IsRunning = false;
            ClearSendCmd();
        }

        /// <summary>
        /// 开机
        /// </summary>
        public void PowerUp()
        {
            string safety = _noSafetyWarning ? SWITCH_WARNING_SAFETY : "";
            string cmd = string.Format("-r {0} -u {1} -p {2} {3} {4}", RemoteIP, User, Password, safety, CMD_POWERUP);
            AddSendCmd(cmd, ECommandType.PowerUp);
        }

        /// <summary>
        /// 关机
        /// </summary>
        public void PowerDown()
        {
            string safety = _noSafetyWarning ? SWITCH_WARNING_SAFETY : "";
            string cmd = string.Format("-r {0} -u {1} -p {2} {3} {4}", RemoteIP, User, Password, safety, CMD_POWERDOWN);
            AddSendCmd(cmd, ECommandType.PowerDown);
        }

        /// <summary>
        /// 重启设备
        /// </summary>
        public void ReStart()
        {
            string safety = _noSafetyWarning ? SWITCH_WARNING_SAFETY : "";
            string cmd = string.Format("-r {0} -u {1} -p {2} {3} {4}", RemoteIP, User, Password, safety, CMD_RESTART);
            AddSendCmd(cmd, ECommandType.Restart);
        }

        /// <summary>
        /// 查询设备状态
        /// </summary>
        public void QueryStatus()
        {
            string safety = _noSafetyWarning ? SWITCH_WARNING_SAFETY : "";
            string cmd = string.Format("-r {0} -u {1} -p {2} {3} {4}", RemoteIP, User, Password, safety, CMD_STATUS);
            AddSendCmd(cmd, ECommandType.Status);
        }

        /// <summary>
        /// 启用或禁用默认密码警告
        /// </summary>
        public void PasswodCheckSwitch()
        {
            string safety = _noSafetyWarning ? SWITCH_WARNING_SAFETY : "";
            string open = PasswordCheckOn ? ECommandSwitch.Enabled.ToString() : ECommandSwitch.Disabled.ToString();
            string cmd = string.Format("-r {0} -u {1} -p {2} {3} {4} {5}", RemoteIP, User, Password, safety, CMD_WARNING_NOPWD, open);
            AddSendCmd(cmd, ECommandType.PwdCheckSwitchOn);
        }

        private void Scan()
        {
            DateTime lastPowerTime = DateTime.Now;
            while (_isRunning)
            {
                Thread.Sleep(100);
                try
                {
                    //if (DateTime.Now.Subtract(_lastStatusTime).TotalSeconds > 10)
                    {
                        if (SendCmdList.Count == 0 || SendCmdList.Count(i => i.Type == ECommandType.Status) == 0)
                        {
                            QueryStatus();
                        }
                        _lastStatusTime = DateTime.Now;
                        if (DateTime.Now.Subtract(lastPowerTime).TotalMinutes > 10)
                        {
                            if (DeviceStatus == true)
                            {
                                PowerDown();
                            }
                            else
                            {
                                PowerUp();
                            }
                            lastPowerTime = DateTime.Now;
                        }
                    }
                    CmdInfo cmd = null;
                    lock (_lockSendList)
                    {
                        if (SendCmdList.Count == 0)
                        {
                            continue;
                        }
                        cmd = SendCmdList.First();
                        _dispatcher.BeginInvoke(new Action(() => SendCmdList.Remove(cmd)));
                    }

                    CurrentCmd = cmd;
                    ECommandType type = cmd.Type;
                    string msg = SendCommand(cmd.Message);
                    string result = "";
                    AnalysisCmd(msg, type, out result);
                    if (type != ECommandType.Status)
                    {
                        IsSendingCmd = false;
                    }
                    AddRecvCmd(msg, type, result);
                    _lastStatusTime = DateTime.Now;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("error" + DateTime.Now.ToString() + ex.Message);
                }
            }
        }

        private void AddSendCmd(string msg, ECommandType type)
        {
            lock (_lockSendList)
            {
                if (type != ECommandType.Status)
                {
                    IsSendingCmd = true;
                }
                _dispatcher.BeginInvoke(new Action(() => SendCmdList.Add(new CmdInfo(msg, type))));
            }
        }

        private void ClearSendCmd()
        {
            lock (_lockSendList)
            {
                _dispatcher.BeginInvoke(new Action(() => SendCmdList.Clear()));
            }
        }

        private void AddRecvCmd(string msg, ECommandType type, string result = "")
        {
            lock (_lockRecvList)
            {
                _dispatcher.BeginInvoke(new Action(() => RecvCmdList.Add(new CmdInfo(msg, type) { Result = result })));
            }
        }

        private string SendCommand(string cmd)
        {
            IsWaitingForCmd = true;
            Process process = new Process();//创建进程对象 

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = DeviceManager.GetInstance().RacadmPath;//设定需要执行的命令  
            startInfo.Arguments = cmd;//“/C”表示执行完命令后马上退出  
            startInfo.UseShellExecute = false;//不使用系统外壳程序启动  
            startInfo.RedirectStandardInput = true;//不重定向输入  
            startInfo.RedirectStandardOutput = true; //重定向输出  
            startInfo.CreateNoWindow = true;//不创建窗口  
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit(1000);

            string str = process.StandardOutput.ReadToEnd().Trim();
            IsWaitingForCmd = false;
            return str;
        }

        private void AnalysisCmd(string cmd, ECommandType type, out string result)
        {
            result = "";
            string str = cmd.Trim();
            if (string.IsNullOrEmpty(str))
            {
                DeviceStatus = null;
                return;
            }
            int index = str.IndexOf("Server power status:");
            if (index >= 0)
            {
                string status = str.Substring(index + "Server power status:".Length).Trim();
                if (status.ToUpper() == "ON")
                {
                    DeviceStatus = true;
                }
                else if (status.ToUpper() == "OFF")
                {
                    DeviceStatus = false;
                }
                else
                {
                    DeviceStatus = null;
                }
            }

            index = str.IndexOf("SEC0701: Warning: Default username and password are currently in use.");
            if (index >= 0)
            {
                PasswordCheckOn = true;
            }
            else
            {
                PasswordCheckOn = false;
            }


            int index1 = str.IndexOf("[Key=iDRAC.Embedded.1#DefaultCredentialMitigationConfigGroup.1]");
            int index2 = str.IndexOf("Object value modified successfully");
            if (index1 >= 0 && index2 >= 0)
            {
                if (index >= 0)
                {
                    PasswordCheckOn = false;
                }
                else
                {
                    PasswordCheckOn = true;
                }
            }

            if (type == ECommandType.PowerUp || type == ECommandType.PowerDown || type == ECommandType.Restart)
            {
                index = str.IndexOf("Server power operation successful");
                if (index >= 0)
                {
                    result = "操作成功";
                }
                else
                {
                    result = "操作失败";
                }
            }
        }
    }

    /// <summary>
    /// 命令开关
    /// </summary>
    public enum ECommandSwitch
    {
        Disabled, Enabled
    }

    public enum ECommandType
    {
        Status, PowerUp, PowerDown, Restart, PwdCheckSwitchOn
    }

    public class CmdInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private DateTime _time = DateTime.Now;
        private string _message = "";
        private string _result = "";
        private ECommandType _type;

        public DateTime Time
        {
            get { return _time; }
            set
            {
                _time = value;
                PropertyChanged.Notify(() => this.Time);
            }
        }

        public ECommandType Type
        {
            get { return _type; }
            set
            {
                _type = value;
                PropertyChanged.Notify(() => this.Type);
            }
        }

        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                PropertyChanged.Notify(() => this.Message);
            }
        }

        public string Result
        {
            get { return _result; }
            set
            {
                _result = value;
                PropertyChanged.Notify(() => this.Result);
            }
        }

        public CmdInfo()
        {
            Time = DateTime.Now;
        }

        public CmdInfo(string msg, ECommandType type)
        {
            _time = DateTime.Now;
            _message = msg;
            _type = type;
        }
    }
}
