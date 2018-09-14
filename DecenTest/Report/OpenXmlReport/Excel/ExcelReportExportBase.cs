
/*********************************************************************************************
 *	
 * 文件名称:    ExcelReportExportBase.cs
 *
 * 作    者:    wdp
 *	
 * 创作日期:    2018-8-14 16:57:23
 * 
 * 备    注:    Excel报表导出基类
 *              
 *                                
 *               
*********************************************************************************************/

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;

namespace OpenXmlReport.Excel
{
    /// <summary>
    /// Excel报表导出基类
    /// </summary>
    public abstract class ExcelReportExportBase : ReportBase
    {
        #region 内部变量

        private SpreadsheetDocument _document = null;
        // Sheet页ID，从1开始
        private uint _sheetID = 1;
        // 当前导出的Sheet页的标题
        private string _sheetName = "";

        // 数据整合类
        private ExcelSheetDataAppend _dataExport;

        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="owner">父窗体句柄</param>
        public ExcelReportExportBase(IntPtr owner) : base(owner)
        {
            _dataExport = new ExcelSheetDataAppend();
            _dataExport.ProgressChangedEvent += SheetDataAppend_ProgressChangedEvent;
            _dataExport.IsExportCanceled = false;
        }

        public override void Cancel()
        {
            base.Cancel();

            _dataExport.IsExportCanceled = true;
            string show = string.Format("报表[{0}]取消导出……", _sheetName);
            CallProgress(show);
        }

        protected void CallProgress(string exportName, string message, int progress = -1)
        {
            _sheetName = exportName;
            base.CallProgress(message, progress);
        }

        /// <summary>
        /// 创建一个新的sheet页
        /// </summary>
        /// <returns></returns>
        protected WorksheetPart AddNewSheetPart()
        {
            return _document.WorkbookPart.AddNewPart<WorksheetPart>();
        }

        /// <summary>
        /// 使用OpenXmlWriter填充数据（大数据量）
        /// 这个方法只能新建，不能向已有的数据追加
        /// </summary>
        /// <param name="workSheetPart"></param>
        /// <param name="data">要填充的数据</param>
        /// <param name="styles">单元格样式</param>
        /// <param name="rowHeights">行高数组（自动行高时为-1）</param>
        /// <param name="columnList">列样式数组</param>
        /// <param name="mergeCellList">合并单元格数组</param>
        /// <param name="startRow">起始的行号，从1开始</param>
        /// <param name="startColumn">起始的列号，从1开始</param>
        protected void FillData(WorksheetPart workSheetPart,
            object[,] data, uint[,] styles, double[] rowHeights, List<Column> columnList, List<MergeCell> mergeCellList,
            int startRow = 1, int startColumn = 1)
        {
            _dataExport.FillData(workSheetPart, data, styles, rowHeights, columnList, mergeCellList, startRow, startColumn);
        }

        /// <summary>
        /// 使用OpenXmlWriter填充数据（大数据量）
        /// 这个方法只能新建，不能向已有的数据追加
        /// </summary>
        /// <param name="workSheetPart"></param>
        /// <param name="data">要填充的数据</param>
        /// <param name="styles">单元格样式</param>
        /// <param name="dataFormats">数据格式</param>
        /// <param name="formulas">公式</param>
        /// <param name="rowHeights">行高数组（自动行高时为-1）</param>
        /// <param name="columnList">列样式数组</param>
        /// <param name="mergeCellList">合并单元格数组</param>
        /// <param name="startRow">起始的行号，从1开始</param>
        /// <param name="startColumn">起始的列号，从1开始</param>
        public void FillData(WorksheetPart workSheetPart,
            object[,] data, uint[,] styles, CellDataType[,] dataFormats, object[,] formulas,
            double[] rowHeights, List<Column> columnList, List<MergeCell> mergeCellList,
            int startRow = 1, int startColumn = 1)
        {
            _dataExport.FillData(workSheetPart, data, styles, dataFormats, formulas, rowHeights, columnList, mergeCellList, startRow, startColumn);
        }

        /// <summary>
        /// 添加Sheet（数据填充完毕以后添加）
        /// </summary>
        /// <param name="workSheetPart"></param>
        protected void AddSheet(WorksheetPart workSheetPart)
        {
            if (_document.WorkbookPart.Workbook == null)
            {
                _document.WorkbookPart.Workbook = new Workbook();
                _document.WorkbookPart.Workbook.Append(new Sheets());
            }

            //数据写入完成后，注册一个sheet引用到workbook.xml, 也就是在excel最下面的sheet name
            _sheetID++;
            var sheet = new Sheet()
            {
                Name = _sheetName,
                SheetId = (UInt32Value)_sheetID,
                Id = _document.WorkbookPart.GetIdOfPart(workSheetPart)
            };
            _document.WorkbookPart.Workbook.Sheets.Append(sheet);
        }

        protected override void Scan()
        {
            try
            {
                base.Scan();
                MaxValue = GetMaxValue();
                _sheetID = 0;
                Message = "[进度:0%] -> 开始创建文件……";
                StartExport();

                if (CancelDelFile())
                    return;

                ExportCompleted = true;
                Message = "[进度:100%] -> 任务完成。";
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
            finally
            {
                ClearData();
            }
        }

        #region 私有函数

        // 导出进度变化事件
        private void SheetDataAppend_ProgressChangedEvent(double value, double maxValue)
        {
            if (ExportCanceled)
                return;

            Progress++;
            string show = string.Format("正在生成报表[{0}] [{1},{2}]", _sheetName, maxValue, value);
            CallProgress(show);
        }

        // 开始填充数据
        private void StartExport()
        {
            // 添加一个工作区
            _document.AddWorkbookPart();
            WorkbookStylesPart wbsp = _document.WorkbookPart.AddNewPart<WorkbookStylesPart>();
            Stylesheet style = GetStylesheet();
            wbsp.Stylesheet = style;
            wbsp.Stylesheet.Save();
            ExportSheets();
            _document.Close();
        }

        // 创建Excel文档
        protected override void CreateDocument(string fileName)
        {
            if (File.Exists(fileName))
                File.Delete(fileName);
            // 创建Excel文件
            _document = SpreadsheetDocument.Create(fileName, SpreadsheetDocumentType.Workbook);
        }

        #endregion 私有函数

        protected override void ClearData()
        {
            return;
        }

        /// <summary>
        /// 自定义样式
        /// </summary>
        /// <returns></returns>
        protected abstract Stylesheet GetStylesheet();

        /// <summary>
        /// 填充数据方法
        /// </summary>
        protected abstract void ExportSheets();
    }
}
