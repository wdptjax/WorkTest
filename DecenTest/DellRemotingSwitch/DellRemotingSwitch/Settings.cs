
/*********************************************************************************************
 *	
 * 文件名称:    Settings.cs
 *
 * 作    者:    吴德鹏
 *	
 * 创作日期:    2018-12-14 15:49:32
 * 
 * 备    注:    描述类的用途
 *              
 *                                
 *               
*********************************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using NotificationExtensions;

namespace DellRemotingSwitch
{
    public class Settings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        const string CMD_SETIPCONFIG = "setniccfg -s";
        const string CMD_GETIPCONFIG = "get iDRAC.IPv4";
        const string CMD_CHANGEPASSWORD = "set iDRAC.Users.2.Password";

        private string _oldIp = "192.168.0.120";
        private string _oldNetmask = "";
        private string _oldGateway = "";
        private string _user = "root";
        private string _password = "calvin";

        private string _newIp = "";
        private string _newNetmask = "255.255.255.0";
        private string _newGateway = "192.168.0.1";
        private string _newPassword = "";

        private bool _isWaitingForCommand = false;

        private string _message = string.Empty;
        private ObservableCollection<CmdInfo> _showMsg = new ObservableCollection<CmdInfo>();

        /// <summary>
        /// 旧IP
        /// </summary>
        public string OldIp
        {
            get { return _oldIp; }
            set
            {
                _oldIp = value;
                PropertyChanged.Notify(() => this.OldIp);
            }
        }

        /// <summary>
        /// 旧子网掩码
        /// </summary>
        public string OldNetmask
        {
            get { return _oldNetmask; }
            set
            {
                _oldNetmask = value;
                PropertyChanged.Notify(() => this.OldNetmask);
            }
        }

        /// <summary>
        /// 旧网关
        /// </summary>
        public string OldGateway
        {
            get { return _oldGateway; }
            set
            {
                _oldGateway = value;
                PropertyChanged.Notify(() => this.OldGateway);
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
        /// 新IP
        /// </summary>
        public string NewIp
        {
            get { return _newIp; }
            set
            {
                _newIp = value;
                PropertyChanged.Notify(() => this.NewIp);
            }
        }

        /// <summary>
        /// 新子网掩码
        /// </summary>
        public string NewNetmask
        {
            get { return _newNetmask; }
            set
            {
                _newNetmask = value;
                PropertyChanged.Notify(() => this.NewNetmask);
            }
        }

        /// <summary>
        /// 新网关
        /// </summary>
        public string NewGateway
        {
            get { return _newGateway; }
            set
            {
                _newGateway = value;
                PropertyChanged.Notify(() => this.NewGateway);
            }
        }

        /// <summary>
        /// 新密码
        /// </summary>
        public string NewPassword
        {
            get { return _newPassword; }
            set
            {
                _newPassword = value;
                PropertyChanged.Notify(() => this.NewPassword);
            }
        }

        /// <summary>
        /// 等待数据返回
        /// </summary>
        public bool IsWaitingForCommand
        {
            get { return _isWaitingForCommand; }
            set
            {
                _isWaitingForCommand = value;
                PropertyChanged.Notify(() => this.IsWaitingForCommand);
            }
        }

        /// <summary>
        /// 显示信息
        /// </summary>
        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                PropertyChanged.Notify(() => this.Message);
            }
        }

        /// <summary>
        /// 日志显示
        /// </summary>
        public ObservableCollection<CmdInfo> ShowLog
        {
            get { return _showMsg; }
            set
            {
                _showMsg = value;
                PropertyChanged.Notify(() => this.ShowLog);
            }
        }


        Dispatcher _dispatcher = null;
        private DeviceSettings _deviceSettingsView = null;

        public Settings()
        {
        }

        public DeviceSettings GetShowView()
        {
            if (_deviceSettingsView == null)
                _deviceSettingsView = new DeviceSettings();
            _dispatcher = _deviceSettingsView.Dispatcher;
            return _deviceSettingsView;
        }

        public void SelectIpConfig()
        {
            IsWaitingForCommand = true;
            Message = "";
            Task.Factory.StartNew(() =>
            {
                try
                {
                    string cmd = string.Format("-r {0} -u {1} -p {2} --nocertwarn {3}", OldIp, User, Password, CMD_GETIPCONFIG);
                    var res = SendCommand(cmd);
                    bool isOk = false;
                    string[] resArr = res.Split('\r');
                    foreach (string str in resArr)
                    {
                        string line = str.Trim('\n');
                        if (line.Contains("Gateway"))
                        {
                            string gateway = line.Replace("Gateway=", "").Trim();
                            this.OldGateway = gateway;
                            this.NewGateway = OldGateway;
                            isOk = true;
                        }
                        if (line.Contains("Netmask"))
                        {
                            string gateway = line.Replace("Netmask=", "").Trim();
                            this.OldNetmask = gateway;
                            this.NewNetmask = OldNetmask;
                            isOk = true;
                        }
                    }
                    if (!isOk)
                    {
                        if (string.IsNullOrEmpty(res))
                            res = "IP地址所在主机无法访问,请检查IP地址或者用户名密码";
                        Message = res;
                    }
                }
                catch (Exception ex)
                {
                    Message = ex.Message;
                }
                finally
                {
                    IsWaitingForCommand = false;
                    _dispatcher.Invoke(new Action(() => CommandManager.InvalidateRequerySuggested()));
                }
            });
        }

        public void ChangeConfig()
        {
            bool signIp = false;
            bool signPwd = false;
            if ((!OldIp.Equals(NewIp) || !OldNetmask.Equals(NewNetmask) || !OldGateway.Equals(NewGateway)) && !string.IsNullOrEmpty(NewIp) && !string.IsNullOrEmpty(NewNetmask) && !string.IsNullOrEmpty(NewGateway))
                signIp = true;
            if (!Password.Equals(NewPassword) && !string.IsNullOrEmpty(NewPassword))
                signPwd = true;
            if (!signIp && !signPwd)
            {
                MessageBox.Show("Ip地址与密码均未修改");
                return;
            }
            IsWaitingForCommand = true;
            Message = "";
            Task.Factory.StartNew(() =>
            {
                try
                {
                    bool? isIpChanged = null;
                    bool? isPwdChanged = null;
                    if (signIp)
                    {
                        string cmd = string.Format("-r {0} -u {1} -p {2} --nocertwarn {3} {4} {5} {6}", OldIp, User, Password, CMD_SETIPCONFIG, NewIp, NewNetmask, NewGateway);
                        var res = SendCommand(cmd);
                        if (res.Contains("Static IP configuration enabled and modified successfully"))
                        {
                            isIpChanged = true;
                        }
                        else
                            isIpChanged = false;
                        Message = res;
                    }
                    if (signPwd)
                    {
                        string cmd = string.Format("-r {0} -u {1} -p {2} --nocertwarn {3} {4}", OldIp, User, Password, CMD_CHANGEPASSWORD, NewPassword);
                        var res = SendCommand(cmd);
                        if (res.Contains("Object value modified successfully"))
                        {
                            isPwdChanged = true;
                        }
                        else
                            isPwdChanged = false;
                        Message = res;
                    }
                    string text = "修改设置完毕!";
                    if (isIpChanged != null)
                        text += "\r\nIP地址设置" + (isIpChanged == true ? "成功" : "失败");
                    if (isPwdChanged != null)
                        text += "\r\n密码修改" + (isPwdChanged == true ? "成功" : "失败");
                    MessageBox.Show(text);
                }
                catch (Exception ex)
                {
                    Message = ex.Message;
                }
                finally
                {
                    IsWaitingForCommand = false;
                    _dispatcher.Invoke(new Action(() => CommandManager.InvalidateRequerySuggested()));
                }
            });
        }


        private string SendCommand(string cmd)
        {
            _dispatcher.Invoke(new Action(() => ShowLog.Add(new CmdInfo() { Message = cmd, Result = "Send" })));
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
            _dispatcher.Invoke(new Action(() => ShowLog.Add(new CmdInfo() { Message = str, Result = "Recv" })));
            return str;
        }
    }
}
