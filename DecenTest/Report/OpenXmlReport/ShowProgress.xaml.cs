using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace OpenXmlReport
{
    /// <summary>
    /// ShowProgress.xaml 的交互逻辑
    /// </summary>
    public partial class ShowProgress : Window
    {
        public ShowProgress()
        {
            InitializeComponent();
            _helper = new WindowInteropHelper(this);
        }

        public ObservableCollection<ReportBase> ProgressCollection { get { return (ObservableCollection<ReportBase>)GetValue(ProgressCollectionProperty); } set { SetValue(ProgressCollectionProperty, value); } }
        private static DependencyProperty ProgressCollectionProperty = DependencyProperty.Register("ProgressCollection", typeof(ObservableCollection<ReportBase>), typeof(ShowProgress), new PropertyMetadata(new ObservableCollection<ReportBase>()));

        /// <summary>
        /// 正文前景色
        /// </summary>
        public Brush ForeColor { get { return (Brush)GetValue(ForeColorProperty); } set { SetValue(ForeColorProperty, value); } }
        private static DependencyProperty ForeColorProperty = DependencyProperty.Register("ForeColor", typeof(Brush), typeof(ShowProgress), new PropertyMetadata(Brushes.White));

        /// <summary>
        /// 正文背景色
        /// </summary>
        public Brush BackColor { get { return (Brush)GetValue(BackColorProperty); } set { SetValue(BackColorProperty, value); } }
        private static DependencyProperty BackColorProperty = DependencyProperty.Register("BackColor", typeof(Brush), typeof(ShowProgress), new PropertyMetadata(Brushes.Gray));

        /// <summary>
        /// 标题背景色
        /// </summary>
        public Brush TitleForeColor { get { return (Brush)GetValue(TitleForeColorProperty); } set { SetValue(TitleForeColorProperty, value); } }
        private static DependencyProperty TitleForeColorProperty = DependencyProperty.Register("TitleForeColor", typeof(Brush), typeof(ShowProgress), new PropertyMetadata(Brushes.White));
        /// <summary>
        /// 标题背景色
        /// </summary>
        public Brush TitleBackColor { get { return (Brush)GetValue(TitleBackColorProperty); } set { SetValue(TitleBackColorProperty, value); } }
        private static DependencyProperty TitleBackColorProperty = DependencyProperty.Register("TitleBackColor", typeof(Brush), typeof(ShowProgress), new PropertyMetadata(Brushes.Black));
      
        private static object _lockThis = new object();
        private static ShowProgress _this = null;

        // 设置父窗体句柄使用，WPF与Winform交互
        // WPF不能直接设置Winform为父窗体，需要用这个类进行设置
        /*
            即使设置了父窗体，也不能将子窗体的显示位置设置为CenterOwner
            因为最后显示的窗口不会位于Owner窗口的中心。
            事实上，WindowInteropHelper.Owner属性设置的是Window类的_ownerHandle成员,
            这个成员和Window.Owner设置的成员不是同一个,
            因此，文档中的说明和WPF的实际实现不相符的。这个问题基本已经确认是WPF的一个BUG。
         */
        private WindowInteropHelper _helper = null;

        /// <summary>
        /// 单实例模式
        /// </summary>
        /// <param name="owner">父窗体句柄</param>
        /// <returns></returns>
        public static ShowProgress GetInstance(IntPtr owner)
        {
            lock (_lockThis)
            {
                if (_this == null)
                {
                    _this = new ShowProgress();
                    _this.Show();
                }
                if (!_this.IsActive)
                {
                    _this.Show();
                }
                _this.SetOwner(owner);
            }
            return _this;
        }

        /// <summary>
        /// 添加新的导出过程
        /// </summary>
        /// <param name="export"></param>
        public void AddNewProgress(ReportBase export)
        {
            this.Dispatcher.Invoke(new Action(() => ProgressCollection.Add(export)));
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            e.Cancel = true;
            if(ProgressCollection.Where(i=>!i.ExportCanceled&&!i.ExportCompleted).Count()>0)
            {
                MessageBox.Show(this, "还有导出任务在执行中，请等待导出完毕或取消导出再关闭界面");
                return;
            }
            this.Hide();
        }

        // 设置父窗体句柄
        protected void SetOwner(IntPtr owner)
        {
            if (_helper != null && owner != IntPtr.Zero)
                _helper.Owner = owner;
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // 打开文件
            if (e.Command == ApplicationCommands.Open)
            {
                if (e.Parameter == null)
                    return;

                ReportBase export = e.Parameter as ReportBase;
                export.OpenFile();
            }
            // 取消导出
            if (e.Command == ApplicationCommands.Stop)
            {
                if (e.Parameter == null)
                    return;

                ReportBase export = e.Parameter as ReportBase;
                export.Cancel();
            }
            // 关闭界面（隐藏界面）
            if (e.Command == ApplicationCommands.Close)
            {
                this.Close();
            }
            // 打开文件所在文件夹
            if (e.Command == ApplicationCommands.Find)
            {
                if (e.Parameter == null)
                    return;

                ReportBase export = e.Parameter as ReportBase;
                export.OpenDir();
            }
            // 清理条目
            if (e.Command == ApplicationCommands.Delete)
            {
                if (e.Parameter == null)
                    return;

                ReportBase export = e.Parameter as ReportBase;
                ProgressCollection.Remove(export);
            }
        }

        // 窗体移动
        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }
    }
}
