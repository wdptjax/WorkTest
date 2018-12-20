using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DellRemotingSwitch
{
    /// <summary>
    /// DeviceSettings.xaml 的交互逻辑
    /// </summary>
    public partial class DeviceSettings : Window
    {
        public DeviceSettings()
        {
            InitializeComponent();
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !DeviceManager.GetInstance().Settings.IsWaitingForCommand;
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == Commands.Exit)
                this.Close();
            else if (e.Command == Commands.ModfiySettings)
            {
                DeviceManager.GetInstance().Settings.ChangeConfig();
            }
            else if (e.Command == Commands.Query)
            {
                DeviceManager.GetInstance().Settings.SelectIpConfig();
            }
        }
    }
}
