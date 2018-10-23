using DeviceSimlib;
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

namespace DeviceSimViewApp
{
    /// <summary>
    /// AddDevice.xaml 的交互逻辑
    /// </summary>
    public partial class AddDevice : Window
    {
        public AddDevice()
        {
            InitializeComponent();
        }

        private static DependencyProperty _selectedType = DependencyProperty.Register("SelectedType", typeof(Type), typeof(AddDevice), new PropertyMetadata(null));
        public Type SelectedType { get { return (Type)GetValue(_selectedType); } set { SetValue(_selectedType, value); } }

        private static DependencyProperty _newDevice = DependencyProperty.Register("NewDevice", typeof(DeviceBase), typeof(AddDevice), new PropertyMetadata(null));
        public DeviceBase NewDevice { get { return (DeviceBase)GetValue(_newDevice); } set { SetValue(_newDevice, value); } }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == _selectedType)
            {
                NewDevice = (DeviceBase)Activator.CreateInstance(SelectedType);
            }
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if(e.Command==Commands.OK)
            {
                if(NewDevice==null)
                {
                    MessageBox.Show(this, "请选择需要创建的设备");
                    return;
                }
                this.DialogResult = true;
                this.Close();
            }
            if(e.Command==Commands.Cancel)
            {
                this.DialogResult = false;
                this.Close();
            }
        }
    }
}
