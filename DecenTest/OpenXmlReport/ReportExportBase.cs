
/*********************************************************************************************
 *	
 * 文件名称:    ReportExportBase.cs
 *
 * 作    者:    wdp
 *	
 * 创作日期:    2018-8-14 16:57:23
 * 
 * 备    注:    报表导出基类
 *              
 *                                
 *               
*********************************************************************************************/

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;

namespace OpenXmlReport
{
    /// <summary>
    /// 进度条显示
    /// </summary>
    public abstract class ReportExportBase : INotifyPropertyChanged
    {
        #region WPF展示使用

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnNotifyPropertyChanged(string propertyName) { if (this.PropertyChanged != null) { this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); } }

        #endregion

        /// <summary>
        /// 当前进度
        /// </summary>
        public double Progress
        {
            get { return _progress; }
            set { if (_progress != value) { _progress = value; OnNotifyPropertyChanged("Progress"); } }
        }

        /// <summary>
        /// 显示信息
        /// </summary>
        public string Message
        {
            get { return _message; }
            set { if (_message != value) { _message = value; OnNotifyPropertyChanged("Message"); } }
        }

        /// <summary>
        /// 进度条最大值
        /// </summary>
        public double MaxValue
        {
            get { return _maxValue; }
            set { if (_maxValue != value) { _maxValue = value; OnNotifyPropertyChanged("MaxValue"); } }
        }

        /// <summary>
        /// 导出完毕
        /// </summary>
        public bool ExportCompleted
        {
            get { return _isExportCompleted; }
            set { if (_isExportCompleted != value) { _isExportCompleted = value; OnNotifyPropertyChanged("ExportCompleted"); } }
        }

        /// <summary>
        /// 取消导出
        /// </summary>
        public bool ExportCanceled
        {
            get { return _isExportCanceled; }
            set { if (_isExportCanceled != value) { _isExportCanceled = value; OnNotifyPropertyChanged("ExportCanceled"); } }
        }

        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName
        {
            get { return _fileName; }
            set { if (_fileName != value) { _fileName = value; OnNotifyPropertyChanged("FileName"); } }
        }

        #endregion

        // wpf进度条展示用
        protected double _maxValue = 0;
        protected double _progress = 0;

        private string _message = "";
        private bool _isExportCompleted = false;
        private bool _isExportCanceled = false;
        private string _fileName;

        #region 内部变量

        private Thread _thdScan = null;

        private SpreadsheetDocument _document = null;
        // Sheet页ID，从1开始
        private uint _sheetID = 1;
        private string _filePath;
        private string _fileDir;
        // 当前导出的频段信息(Sheet页的标题)
        private string _exportSegmentString = "";
        // 进度条展示窗体
        private ShowProgress _showExport = null;

        // 数据整合类
        private SheetDataAppend _dataExport;
        #endregion


        /// <summary>
        /// 构造函数
        /// </summary>
        public ReportExportBase()
        {
            _showExport = ShowProgress.GetInstance();
            _dataExport = new SheetDataAppend();
            _dataExport.ProgressChangedEvent += SheetDataAppend_ProgressChangedEvent;
            _dataExport.IsExportCanceled = false;
        }

        /// <summary>
        /// 开始导出过程
        /// </summary>
        /// <param name="FilePath"></param>
        public virtual void Start(string FilePath = "")
        {
            _fileDir = AppDomain.CurrentDomain.BaseDirectory + "Data\\Report\\";
            CreateDir(_fileDir);
            FileName = FilePath + ".xlsx";
            _filePath = _fileDir + _fileName;
            _thdScan = new Thread(Scan);
            _thdScan.IsBackground = true;
            _thdScan.Start();
        }

        /// <summary>
        /// 打开文件
        /// </summary>
        public virtual void OpenFile()
        {
            if (string.IsNullOrEmpty(_filePath))
                return;

            if (!File.Exists(_filePath))
            {
                MessageBox.Show(_showExport, "文件不存在");
                return;
            }

            Process.Start(_filePath);
        }

        /// <summary>
        /// 打开文件所在路径
        /// </summary>
        public virtual void OpenDir()
        {
            if (string.IsNullOrEmpty(_fileDir))
                return;
            if (!Directory.Exists(_fileDir))
            {
                MessageBox.Show(_showExport, "路径不存在");
                return;
            }

            Process.Start(_fileDir);
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
            _dataExport.IsExportCanceled = true;
            int progress = (int)_progress * 100 / (int)_maxValue;
            Message = string.Format("[进度:{0}%] -> 报表[{1}]取消导出……", progress, _exportSegmentString);
        }


        // 导出进度变化事件
        private void SheetDataAppend_ProgressChangedEvent(double value, double maxValue)
        {
            if (_isExportCanceled)
                return;

            Progress++;
            int progress = (int)_progress * 100 / (int)_maxValue;
            string show = string.Format("[进度:{0}%] -> 正在生成报表[{1}] [{2},{3}]", progress, _exportSegmentString, maxValue, value);
            Message = show;
        }

        // 异步方法导出
        private void Scan()
        {
            try
            {
                _progress = 0;
                string filename = _filePath;
                _maxValue = GetMaxValue();
                _sheetID = 0;
                _showExport.AddNewProgress(this);
                CreateDocument(filename);
                Message = "[进度:0%] -> 开始创建文件……";
                StartFillData();
                if (_isExportCanceled)
                {
                    if (File.Exists(_filePath))
                    {
                        File.Delete(_filePath);
                    }
                    return;
                }

                ExportCompleted = true;
                Message = "[进度:100%] -> 任务完成。";
            }
            catch (Exception ex)
            {
                _showExport.Dispatcher.Invoke(new Action(() => MessageBox.Show(_showExport, "报表导出失败/" + ex.Message)));
            }
            finally
            {
                ClearData();
            }

        }

        // 开始填充数据
        private void StartFillData()
        {
            // 添加一个工作区
            _document.AddWorkbookPart();
            WorkbookStylesPart wbsp = _document.WorkbookPart.AddNewPart<WorkbookStylesPart>();
            Stylesheet style = GetStylesheet();
            wbsp.Stylesheet = style;
            wbsp.Stylesheet.Save();
            FillData();
            _document.Close();
        }

        // 创建Excel文档
        private void CreateDocument(string fileName)
        {
            if (File.Exists(fileName))
                File.Delete(fileName);
            // 创建Excel文件
            _document = SpreadsheetDocument.Create(fileName, SpreadsheetDocumentType.Workbook);
        }

        // 创建路径
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

        /// <summary>
        /// 自定义样式
        /// </summary>
        /// <returns></returns>
        protected abstract Stylesheet GetStylesheet();

        /// <summary>
        /// 填充数据方法
        /// </summary>
        protected abstract void FillData();

        /// <summary>
        /// 导出取消或者导出完毕都需要清理
        /// </summary>
        protected virtual void ClearData()
        {
            return;
        }

        /// <summary>
        /// 获取进度最大值
        /// </summary>
        /// <returns></returns>
        protected abstract double GetMaxValue();
    }
}
