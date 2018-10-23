
/*********************************************************************************************
 *	
 * 文件名称:    Commands.cs
 *
 * 作    者:    吴德鹏
 *	
 * 创作日期:    2018-10-16 10:50:55
 * 
 * 备    注:    描述类的用途
 *              
 *                                
 *               
*********************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace DeviceSimlib
{
    public static class Commands
    {
        public static RoutedUICommand Select { get { return _select; } }
        private static RoutedUICommand _select = new RoutedUICommand("查询", "查询", typeof(Commands));
        public static RoutedUICommand Edit { get { return _edit; } }
        private static RoutedUICommand _edit = new RoutedUICommand("编辑", "编辑", typeof(Commands));
        public static RoutedUICommand Check { get { return _check; } }
        private static RoutedUICommand _check = new RoutedUICommand("比较", "比较", typeof(Commands));

        public static RoutedUICommand Start { get { return _start; } }
        private static RoutedUICommand _start = new RoutedUICommand("启动", "启动", typeof(Commands));
        public static RoutedUICommand Stop { get { return _stop; } }
        private static RoutedUICommand _stop = new RoutedUICommand("停止", "停止", typeof(Commands));
        public static RoutedUICommand Pause { get { return _pause; } }
        private static RoutedUICommand _pause = new RoutedUICommand("暂停", "暂停", typeof(Commands));
        public static RoutedUICommand Add { get { return _add; } }
        private static RoutedUICommand _add = new RoutedUICommand("增加", "增加", typeof(Commands));
        public static RoutedUICommand Delete { get { return _delete; } }
        private static RoutedUICommand _delete = new RoutedUICommand("删除", "删除", typeof(Commands));

        public static RoutedUICommand OK { get { return _ok; } }
        private static RoutedUICommand _ok = new RoutedUICommand("确定", "确定", typeof(Commands));
        public static RoutedUICommand Cancel { get { return _cancel; } }
        private static RoutedUICommand _cancel = new RoutedUICommand("取消", "取消", typeof(Commands));
        public static RoutedUICommand Send { get { return _send; } }
        private static RoutedUICommand _send = new RoutedUICommand("发送", "发送", typeof(Commands));
        public static RoutedUICommand Login { get { return _login; } }
        private static RoutedUICommand _login = new RoutedUICommand("登录", "登录", typeof(Commands));
        public static RoutedUICommand Logout { get { return _logout; } }
        private static RoutedUICommand _logout = new RoutedUICommand("注销", "注销", typeof(Commands));
        public static RoutedUICommand Language { get { return _language; } }
        private static RoutedUICommand _language = new RoutedUICommand("切换语言", "切换语言", typeof(Commands));
        public static RoutedUICommand Export { get { return _export; } }
        private static RoutedUICommand _export = new RoutedUICommand("导出", "导出", typeof(Commands));
        public static RoutedUICommand Exit { get { return _exit; } }
        private static RoutedUICommand _exit = new RoutedUICommand("退出", "退出", typeof(Commands));
    }
}
