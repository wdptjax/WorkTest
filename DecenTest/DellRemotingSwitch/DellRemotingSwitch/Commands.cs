using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace DellRemotingSwitch
{
    public static class Commands
    {
        public static RoutedUICommand Add { get { return _add; } }
        private static RoutedUICommand _add = new RoutedUICommand("添加设备", "添加设备", typeof(Commands));

        public static RoutedUICommand Delete { get { return _delete; } }
        private static RoutedUICommand _delete = new RoutedUICommand("删除设备", "删除设备", typeof(Commands));

        public static RoutedUICommand Start { get { return _start; } }
        private static RoutedUICommand _start = new RoutedUICommand("开启监听", "开启监听", typeof(Commands));
        public static RoutedUICommand StartAll { get { return _startAll; } }
        private static RoutedUICommand _startAll = new RoutedUICommand("全部开启", "全部开启", typeof(Commands));
        public static RoutedUICommand Settings { get { return _settings; } }
        private static RoutedUICommand _settings = new RoutedUICommand("修改设置", "修改设置", typeof(Commands));
        public static RoutedUICommand Stop { get { return _stop; } }
        private static RoutedUICommand _stop = new RoutedUICommand("停止监听", "停止监听", typeof(Commands));
        public static RoutedUICommand PowerUp { get { return _powerUp; } }
        private static RoutedUICommand _powerUp = new RoutedUICommand("开机", "开机", typeof(Commands));
        public static RoutedUICommand PowerDown { get { return _powerDown; } }
        private static RoutedUICommand _powerDown = new RoutedUICommand("关机", "关机", typeof(Commands));
        public static RoutedUICommand PowerCycle { get { return _powerCycle; } }
        private static RoutedUICommand _powerCycle = new RoutedUICommand("重启", "重启", typeof(Commands));
        public static RoutedUICommand Query { get { return _query; } }
        private static RoutedUICommand _query = new RoutedUICommand("查询", "查询", typeof(Commands));
        public static RoutedUICommand IgnoreWarning{ get { return _ignoreWarning; } }
        private static RoutedUICommand _ignoreWarning = new RoutedUICommand("忽略警告", "忽略警告", typeof(Commands));
        public static RoutedUICommand PasswordChecking { get { return _passwordChecking; } }
        private static RoutedUICommand _passwordChecking = new RoutedUICommand("密码检查", "密码检查", typeof(Commands));
        public static RoutedUICommand PathSelect { get { return _pathSelect; } }
        private static RoutedUICommand _pathSelect = new RoutedUICommand("密码检查", "密码检查", typeof(Commands));

        public static RoutedUICommand ModfiySettings { get { return _modfiySettings; } }
        private static RoutedUICommand _modfiySettings = new RoutedUICommand("修改设置", "修改设置", typeof(Commands));

        public static RoutedUICommand Exit { get { return _exit; } }
        private static RoutedUICommand _exit = new RoutedUICommand("退出", "退出", typeof(Commands));

    }
}
