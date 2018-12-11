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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DellRemotingSwitch
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainVM = new MainViewModel(this.Dispatcher);
        }

        private static DependencyProperty _mainVMProperty = DependencyProperty.Register("MainVM", typeof(MainViewModel), typeof(MainWindow), new PropertyMetadata(null));
        public MainViewModel MainVM { get { return (MainViewModel)GetValue(_mainVMProperty); } set { SetValue(_mainVMProperty, value); } }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.New)
            {
                e.CanExecute = !MainVM.IsRunning;
            }
            else if (e.Command == ApplicationCommands.Stop)
            {
                e.CanExecute = MainVM.IsRunning;
            }
            else if (e.Command == ApplicationCommands.Open)
            {
                e.CanExecute = MainVM.IsRunning & MainVM.DeviceStatus != true & !MainVM.IsSendingCmd;
            }
            else if (e.Command == ApplicationCommands.Close)
            {
                e.CanExecute = MainVM.IsRunning & MainVM.DeviceStatus == true & !MainVM.IsSendingCmd;
            }
            else if (e.Command == ApplicationCommands.Redo)
            {
                e.CanExecute = MainVM.IsRunning & MainVM.DeviceStatus == true & !MainVM.IsSendingCmd;
            }
            else if (e.Command == ApplicationCommands.SelectAll)
            {
                e.CanExecute = MainVM.IsRunning;
            }
            else if (e.Command == ApplicationCommands.Undo)
            {
                e.CanExecute = MainVM.IsRunning & !MainVM.IsSendingCmd;
            }
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.New)
            {
                MainVM.Start();
            }
            else if (e.Command == ApplicationCommands.Stop)
            {
                MainVM.Stop();
            }
            else if (e.Command == ApplicationCommands.Open)
            {
                MainVM.PowerUp();
            }
            else if (e.Command == ApplicationCommands.Close)
            {
                MainVM.PowerDown();
            }
            else if (e.Command == ApplicationCommands.Redo)
            {
                MainVM.ReStart();
            }
            else if (e.Command == ApplicationCommands.SelectAll)
            {
                MainVM.QueryStatus();
            }
            else if (e.Command == ApplicationCommands.Undo)
            {
                MainVM.PasswordCheckOn = !MainVM.PasswordCheckOn;
                MainVM.PasswodCheckSwitch();
            }
        }
    }
}
