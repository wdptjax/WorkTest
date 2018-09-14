
/*********************************************************************************************
 *	
 * 文件名称:    ReportBase.cs
 *
 * 作    者:    吴德鹏
 *	
 * 创作日期:    2018-9-10 11:16:21
 * 
 * 备    注:    报表导出基类
 *              
 *                                
 *               
*********************************************************************************************/

using NotificationExtensions;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;

namespace OpenXmlReport
{
    /// <summary>
    /// 报表导出基类
    /// </summary>
    public abstract class ReportBase : INotifyPropertyChanged
    {
        #region WPF展示使用

        public event PropertyChangedEventHandler PropertyChanged;

        // wpf进度条展示用
        private double _maxValue = 0;
        private double _progress = 0;

        private string _message = "";
        private bool _isExportCompleted = false;
        private bool _isExportCanceled = false;
        private string _fileName;

        /// <summary>
        /// 当前进度
        /// </summary>
        public double Progress
        {
            get { return _progress; }
            set { if (_progress != value) { _progress = value; PropertyChanged.Notify(() => this.Progress); } }
        }

        /// <summary>
        /// 显示信息
        /// </summary>
        public string Message
        {
            get { return _message; }
            set { if (_message != value) { _message = value; PropertyChanged.Notify(() => this.Message); } }
        }

        /// <summary>
        /// 进度条最大值
        /// </summary>
        public double MaxValue
        {
            get { return _maxValue; }
            set { if (_maxValue != value) { _maxValue = value; PropertyChanged.Notify(() => this.MaxValue); } }
        }

        /// <summary>
        /// 导出完毕
        /// </summary>
        public bool ExportCompleted
        {
            get { return _isExportCompleted; }
            set { if (_isExportCompleted != value) { _isExportCompleted = value; PropertyChanged.Notify(() => this.ExportCompleted); } }
        }

        /// <summary>
        /// 取消导出
        /// </summary>
        public bool ExportCanceled
        {
            get { return _isExportCanceled; }
            set { if (_isExportCanceled != value) { _isExportCanceled = value; PropertyChanged.Notify(() => this.ExportCanceled); } }
        }

        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName
        {
            get { return _fileName; }
            set { if (_fileName != value) { _fileName = value; PropertyChanged.Notify(() => this.FileName); } }
        }

        #endregion

        // 进度条展示窗体
        private ShowProgress _showExportView = null;
        private Thread _thdScan = null;
        private string _fileDir;
        private string _filePath;

        #region 构造函数

        public ReportBase(IntPtr owner)
        {
            _showExportView = ShowProgress.GetInstance(owner);
        }

        #endregion 构造函数

        #region 公共方法

        /// <summary>
        /// 打开文件
        /// </summary>
        public void OpenFile()
        {
            if (string.IsNullOrEmpty(_filePath))
                return;

            if (!File.Exists(_filePath))
            {
                MessageBox.Show(_showExportView, "文件不存在");
                return;
            }

            Process.Start(_filePath);
        }

        /// <summary>
        /// 打开文件所在路径
        /// </summary>
        public void OpenDir()
        {
            if (string.IsNullOrEmpty(_fileDir))
                return;
            if (!Directory.Exists(_fileDir))
            {
                MessageBox.Show(_showExportView, "路径不存在");
                return;
            }

            Process.Start(_fileDir);
        }

        #endregion 公共方法

        #region 内部方法

        /// <summary>
        /// 弹出错误提示框
        /// </summary>
        /// <param name="message"></param>
        protected void ShowError(string message)
        {
            _showExportView.Dispatcher.Invoke(new Action(() => MessageBox.Show(_showExportView, "报表导出失败/" + message)));
        }

        /// <summary>
        /// 创建路径
        /// </summary>
        /// <param name="dir"></param>
        private void CreateDir(string dir)
        {
            if (Directory.Exists(dir))
                return;

            string dir1 = dir.TrimEnd('\\');
            int index = dir1.LastIndexOf('\\');
            string dir2 = dir1;
            if (index > 0)
                dir2 = dir1.Substring(0, index).TrimEnd('\\');
            CreateDir(dir2);
            Directory.CreateDirectory(dir1);
        }

        #endregion 内部方法

        #region 虚方法

        /// <summary>
        /// 开始导出过程
        /// </summary>
        /// <param name="FilePath"></param>
        public virtual void Start(string filePath = "")
        {
            _fileDir = AppDomain.CurrentDomain.BaseDirectory + "Data\\Report\\";
            CreateDir(_fileDir);
            if (string.IsNullOrEmpty(filePath))
                filePath = Guid.NewGuid().ToString();
            FileName = filePath + ".xlsx";
            _filePath = _fileDir + _fileName;
            _thdScan = new Thread(Scan);
            _thdScan.IsBackground = true;
            _thdScan.Start();
        }

        /// <summary>
        /// 取消导出报表
        /// </summary>
        public virtual void Cancel()
        {
            if (_isExportCompleted)
                return;
            ExportCanceled = true;
            ExportCompleted = false;
        }

        /// <summary>
        /// 取消导出以后删除文件
        /// 取消导出并且文件删除返回true；未取消返回false
        /// </summary>
        /// <returns>取消导出并且文件删除返回true；未取消返回false</returns>
        protected virtual bool CancelDelFile()
        {
            if (_isExportCanceled)
            {
                if (File.Exists(_filePath))
                {
                    File.Delete(_filePath);
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 开始异步方法导出
        /// </summary>
        protected virtual void Scan()
        {
            _progress = 0;
            string filename = _filePath;
            CreateDocument(filename);
            _showExportView.AddNewProgress(this);
        }

        /// <summary>
        /// 发送到前台显示进度
        /// </summary>
        /// <param name="exportName">当前的进度内容</param>
        /// <param name="message"></param>
        /// <param name="progress">默认-1，自动显示进度</param>
        protected virtual void CallProgress(string message, int progress = -1)
        {
            if (progress >= 0)
                Progress = progress;
            int val = (int)Progress * 100 / (int)_maxValue;
            Message = string.Format("[进度:{0}%] -> {1}", val, message);
        }

        #endregion 虚方法

        #region 抽象方法

        /// <summary>
        /// 获取进度最大值
        /// </summary>
        /// <returns></returns>
        protected abstract double GetMaxValue();

        /// <summary>
        /// 创建报表文档
        /// </summary>
        /// <param name="fileName"></param>
        protected abstract void CreateDocument(string fileName);

        /// <summary>
        /// 导出取消或者导出完毕都需要清理
        /// </summary>
        protected abstract void ClearData();

        #endregion 抽象方法
    }
}
