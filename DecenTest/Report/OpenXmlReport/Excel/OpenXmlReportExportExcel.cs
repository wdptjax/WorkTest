
/*********************************************************************************************
 *	
 * 文件名称:    OpenXmlReportExport.cs
 *
 * 作    者:    wdp
 *	
 * 创作日期:    2018-7-24 16:27:00
 * 
 * 备    注:    报表导出的例子，使用OpenXml工具进行导出，在导出数据量特别大时效率高，而且不会出现内存溢出的情况
 *              在本实例中使用DataSet作为导出的数据源，根据需要可以修改为自定义的其他集合
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
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;

namespace OpenXmlReport.Excel
{
    /// <summary>
    /// 报表导出例子
    /// </summary>
    public class OpenXmlReportExportExcel : ExcelReportExportBase
    {
        /// <summary>
        /// 需要导出的数据
        /// </summary>
        public DataSet ExportData { get; set; }

        // 数值格式
        private string _numberFormatStr = "0.00_ ";
        // 时间格式
        private string _dateTimeFormatStr = "yyyy/MM/dd hh:mm:ss";
        // 数据格式的ID，ID从176开始（通过Open Xml SDK工具查看到的都是从176开始，可能176之前的是系统定义的格式）
        private uint _numberFormatIdStart = 176;

        /// <summary>
        /// 构造函数
        /// </summary>
        public OpenXmlReportExportExcel(IntPtr owner) : base(owner)
        {
        }

        /// <summary>
        /// 如果需要清理相关资源，比如导出以后需要清理数据源，则重写这个方法
        /// </summary>
        protected override void ClearData()
        {
        }

        #region 重写

        protected override void ExportSheets()
        {
            if (ExportData == null || ExportData.Tables.Count == 0)
                return;
            foreach (DataTable dt in ExportData.Tables)
            {
                if (ExportCanceled)
                    return;
                FillSheet(dt);
            }
        }

        /// <summary>
        /// 自定义样式
        /// 0 默认样式 宋体-11-普通-无下划线-颜色普通-无边框
        /// 1 Sheet1 标题样式 宋体-16-加粗-下划线-颜色普通-普通边框-居中对齐
        /// 2 所有 普通样式 宋体-12-普通-无下划线-颜色普通-普通边框
        /// 3 Sheet1 带斜线的单元格样式 宋体-11-普通-无下划线-颜色普通-斜线边框
        /// 4 Sheet1 备注样式 宋体-9-加粗-无下划线-颜色普通-普通边框
        /// 5 SheetN 标题样式 宋体-16-加粗-无下划线-前景背景色-普通边框-居中对齐
        /// 6 SheetN 带斜线的单元格样式 宋体-9-普通-无下划线-颜色普通-斜线边框
        /// 7 SheetN 普通样式 宋体-9-普通-无下划线-颜色普通-普通边框
        /// 8 SheetN 备注样式 宋体-12-加粗-无下划线-颜色普通-普通边框
        /// 9 SheetN 备注样式 宋体-12-加粗-无下划线-颜色普通-普通边框-居中对齐
        /// 10 所有 普通样式 宋体-12-普通-无下划线-颜色普通-普通边框-居中对齐
        /// 11 SheetN 普通样式 宋体-9-普通-无下划线-颜色普通-普通边框-左对齐
        /// 12 SheetN 普通样式 宋体-9-普通-无下划线-颜色普通-普通边框-左对齐-数值
        /// 13 SheetN 普通样式 宋体-9-普通-无下划线-颜色普通-普通边框-左对齐-时间
        /// </summary>
        /// <returns></returns>
        protected override Stylesheet GetStylesheet()
        {
            #region 数据格式
            // ID从176开始（通过Open Xml SDK工具查看到的都是从176开始，可能176之前的是系统定义的格式）
            // 如果是数值，需要的格式如下：[0.00_ ]后面必须跟下划线与空格，否则会报错：(
            NumberingFormat num1 = ExcelSheetStyles.GetNumberingFormat(_numberFormatIdStart, _numberFormatStr);
            NumberingFormat num2 = ExcelSheetStyles.GetNumberingFormat(++_numberFormatIdStart, _dateTimeFormatStr);
            List<NumberingFormat> numfList = new List<NumberingFormat>()
            {
                num1,num2
            };
            #endregion

            #region 字体样式

            Font font1 = ExcelSheetStyles.GetFont(11, "宋体");
            Font font2 = ExcelSheetStyles.GetFont(16, "宋体", "000000", UnderlineValues.Single, true);
            Font font3 = ExcelSheetStyles.GetFont(12, "宋体");
            Font font4 = ExcelSheetStyles.GetFont(11, "宋体");
            Font font5 = ExcelSheetStyles.GetFont(9, "宋体", "000000", UnderlineValues.None, true);
            Font font6 = ExcelSheetStyles.GetFont(16, "宋体", "008000", UnderlineValues.None, true);
            Font font7 = ExcelSheetStyles.GetFont(9, "宋体");
            Font font8 = ExcelSheetStyles.GetFont(12, "宋体", "000000", UnderlineValues.None, true);

            List<Font> fontlist = new List<Font>
            {
                font1,
                font2,
                font3,
                font4,
                font5,
                font6,
                font7,
                font8
            };

            #endregion

            #region 填充样式
            Fill fill1 = ExcelSheetStyles.GetFill(PatternValues.None);
            Fill fill2 = ExcelSheetStyles.GetFill(PatternValues.Gray125);
            Fill fill3 = ExcelSheetStyles.GetFill(PatternValues.Solid, "FFFFFF99");

            List<Fill> filllist = new List<Fill>
            {
                fill1,
                fill2,
                fill3
            };

            #endregion

            #region 边框样式

            //无边框
            LeftBorder left0 = (LeftBorder)ExcelSheetStyles.GetBorderLineStyle(2, BorderStyleValues.None);
            RightBorder right0 = (RightBorder)ExcelSheetStyles.GetBorderLineStyle(3, BorderStyleValues.None);
            TopBorder top0 = (TopBorder)ExcelSheetStyles.GetBorderLineStyle(0, BorderStyleValues.None);
            BottomBorder bottom0 = (BottomBorder)ExcelSheetStyles.GetBorderLineStyle(1, BorderStyleValues.None);
            DiagonalBorder diag0 = (DiagonalBorder)ExcelSheetStyles.GetBorderLineStyle(4, BorderStyleValues.None);
            //单边框
            LeftBorder left1 = (LeftBorder)ExcelSheetStyles.GetBorderLineStyle(2, BorderStyleValues.Thin, "000000");
            RightBorder right1 = (RightBorder)ExcelSheetStyles.GetBorderLineStyle(3, BorderStyleValues.Thin, "000000");
            TopBorder top1 = (TopBorder)ExcelSheetStyles.GetBorderLineStyle(0, BorderStyleValues.Thin, "000000");
            BottomBorder bottom1 = (BottomBorder)ExcelSheetStyles.GetBorderLineStyle(1, BorderStyleValues.Thin, "000000");
            //单边框以及斜线
            LeftBorder left2 = (LeftBorder)ExcelSheetStyles.GetBorderLineStyle(2, BorderStyleValues.Thin, "000000");
            RightBorder right2 = (RightBorder)ExcelSheetStyles.GetBorderLineStyle(3, BorderStyleValues.Thin, "000000");
            TopBorder top2 = (TopBorder)ExcelSheetStyles.GetBorderLineStyle(0, BorderStyleValues.Thin, "000000");
            BottomBorder bottom2 = (BottomBorder)ExcelSheetStyles.GetBorderLineStyle(1, BorderStyleValues.Thin, "000000");
            DiagonalBorder diag2 = (DiagonalBorder)ExcelSheetStyles.GetBorderLineStyle(4, BorderStyleValues.Thin, "000000");

            List<BorderPropertiesType> list = new List<BorderPropertiesType>
            {
                left0,
                right0,
                top0,
                bottom0
            };
            // 无边框
            Border border1 = ExcelSheetStyles.GetBorder(list);

            list.Clear();
            list.Add(left1);
            list.Add(right1);
            list.Add(top1);
            list.Add(bottom1);
            // 有边框
            Border border2 = ExcelSheetStyles.GetBorder(list);

            list.Clear();
            list.Add(left2);
            list.Add(right2);
            list.Add(top2);
            list.Add(bottom2);
            list.Add(diag2);
            // 中间有斜边框
            Border border3 = ExcelSheetStyles.GetBorder(list, false, true);

            List<Border> borderList = new List<Border>
            {
                border1,
                border2,
                border3
            };

            #endregion

            #region 单元格样式

            // CellFormat的fontId,fillId,borderId分别对应上面的fontlist、filllist、borderList从0开始的索引
            CellFormat cellFormat0 = ExcelSheetStyles.GetCellFormat(HorizontalAlignmentValues.Left,
                VerticalAlignmentValues.Center, 0, 0, 0, 0);
            CellFormat cellFormat1 = ExcelSheetStyles.GetCellFormat(HorizontalAlignmentValues.Center,
                 VerticalAlignmentValues.Center, 0, 1, 0, 1);
            CellFormat cellFormat2 = ExcelSheetStyles.GetCellFormat(HorizontalAlignmentValues.Left,
                VerticalAlignmentValues.Center, 0, 2, 0, 1);
            CellFormat cellFormat3 = ExcelSheetStyles.GetCellFormat(HorizontalAlignmentValues.Left,
                VerticalAlignmentValues.Center, 0, 3, 0, 2);
            CellFormat cellFormat4 = ExcelSheetStyles.GetCellFormat(HorizontalAlignmentValues.Left,
                VerticalAlignmentValues.Center, 0, 4, 0, 1);
            CellFormat cellFormat5 = ExcelSheetStyles.GetCellFormat(HorizontalAlignmentValues.Center,
                VerticalAlignmentValues.Center, 0, 5, 2, 1);
            CellFormat cellFormat6 = ExcelSheetStyles.GetCellFormat(HorizontalAlignmentValues.Left,
                VerticalAlignmentValues.Center, 0, 6, 0, 2);
            CellFormat cellFormat7 = ExcelSheetStyles.GetCellFormat(HorizontalAlignmentValues.Center,
                VerticalAlignmentValues.Center, 0, 6, 0, 1);
            CellFormat cellFormat8 = ExcelSheetStyles.GetCellFormat(HorizontalAlignmentValues.Left,
                VerticalAlignmentValues.Center, 0, 7, 0, 1);
            CellFormat cellFormat9 = ExcelSheetStyles.GetCellFormat(HorizontalAlignmentValues.Center,
                VerticalAlignmentValues.Center, 0, 7, 0, 1);
            CellFormat cellFormat10 = ExcelSheetStyles.GetCellFormat(HorizontalAlignmentValues.Center,
                VerticalAlignmentValues.Center, 0, 2, 0, 1);
            CellFormat cellFormat11 = ExcelSheetStyles.GetCellFormat(HorizontalAlignmentValues.Left,
                VerticalAlignmentValues.Center, 0, 6, 0, 1);
            CellFormat cellFormat12 = ExcelSheetStyles.GetCellFormat(HorizontalAlignmentValues.Left,
                VerticalAlignmentValues.Center, 176, 6, 0, 1);
            CellFormat cellFormat13 = ExcelSheetStyles.GetCellFormat(HorizontalAlignmentValues.Left,
                VerticalAlignmentValues.Center, 177, 6, 0, 1);
            List<CellFormat> cellList = new List<CellFormat>
            {
                cellFormat0,  //0 默认样式 宋体-11-普通-无下划线-颜色普通-无边框
                cellFormat1,  //1 Sheet1 标题样式 宋体-16-加粗-下划线-颜色普通-普通边框-居中对齐
                cellFormat2,  //2 所有 普通样式 宋体-12-普通-无下划线-颜色普通-普通边框
                cellFormat3,  //3 Sheet1 带斜线的单元格样式 宋体-11-普通-无下划线-颜色普通-斜线边框
                cellFormat4,  //4 Sheet1 备注样式 宋体-9-加粗-无下划线-颜色普通-普通边框
                cellFormat5,  //5 SheetN 标题样式 宋体-16-加粗-无下划线-前景背景色-普通边框-居中对齐
                cellFormat6,  //6 SheetN 带斜线的单元格样式 宋体-9-普通-无下划线-颜色普通-斜线边框
                cellFormat7,  //7 SheetN 普通样式 宋体-9-普通-无下划线-颜色普通-普通边框-居中对齐
                cellFormat8,  //8 SheetN 备注样式 宋体-12-加粗-无下划线-颜色普通-普通边框
                cellFormat9,  //9 SheetN 备注样式 宋体-12-加粗-无下划线-颜色普通-普通边框-居中对齐
                cellFormat10, //10 所有 普通样式 宋体-12-普通-无下划线-颜色普通-普通边框-居中对齐
                cellFormat11, //11 SheetN 普通样式 宋体-9-普通-无下划线-颜色普通-普通边框-左对齐
                cellFormat12,
                cellFormat13,
            };

            #endregion

            Stylesheet style = ExcelSheetStyles.GetStyleSheet(numfList, fontlist, filllist, borderList, cellList);
            return style;
        }

        /// <summary>
        /// 填充表格的数据
        /// </summary>
        /// <returns></returns>
        private bool FillSheet(DataTable dataTable)
        {
            WorksheetPart workSheetPart = AddNewSheetPart();

            int rowCount = dataTable.Rows.Count + 1 + 1;//数据行+一行总标题+一行列标题
            int colCount = dataTable.Columns.Count;

            string sheetName = dataTable.TableName;
            string msg = string.Format("正在生成报表[{0}] 开始组合数据……", sheetName);
            CallProgress(sheetName, msg);

            // 数据空间
            object[,] data = new object[rowCount, colCount];
            // 单元格样式
            uint[,] styles = new uint[rowCount, colCount];
            // 数值格式
            CellDataType[,] formats = new CellDataType[rowCount, colCount];
            // 公式
            object[,] formulas = new object[rowCount, colCount];
            // 行高
            double[] rowHeights = Enumerable.Repeat(-1D, rowCount).ToArray();//-1代表自动行高
            // 填充标题（第一行） 宋体-16-加粗 背景淡黄-前景绿色
            int startRow = 0;
            string titleString = sheetName;
            data[startRow, 0] = titleString;
            styles[startRow, 0] = 5;// 表格样式对应方法[GetStylesheet]中的样式ID
            rowHeights[0] = 44.5;
            formats[startRow, 0] = CellDataType.String;
            for (int i = 1; i < data.GetLength(1); i++)
            {
                // 标题行需要合并，其他单元格用空字符串填充
                data[startRow, i] = "";
                formats[startRow, i] = CellDataType.String;
            }

            #region 填充列标题

            startRow++;
            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                data[startRow, i] = dataTable.Columns[i].ColumnName;
                styles[startRow, i] = 9;
                formats[startRow, i] = CellDataType.String;
            }

            #endregion 填充列标题

            #region 填充数据

            startRow++;

            // 设置单元格样式
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                for (int j = 0; j < dataTable.Columns.Count; j++)
                {
                    data[startRow + i, j] = dataTable.Rows[i][j];
                    styles[startRow + i, j] = 11;
                    formats[startRow + i, j] = CellDataType.String;
                    if (dataTable.Columns[j].DataType == typeof(double))
                    {
                        styles[startRow + i, j] = 12;
                        formats[startRow + i, j] = CellDataType.Number;
                    }
                    else if (dataTable.Columns[j].DataType == typeof(DateTime))
                    {
                        styles[startRow + i, j] = 13;
                        formats[startRow + i, j] = CellDataType.DateTime;
                    }
                    if (j == dataTable.Columns.Count - 1)
                    {
                        string c1 = ExcelSheetDataAppend.GetColumnName(3);
                        string c2 = ExcelSheetDataAppend.GetColumnName(5);
                        formulas[startRow + i, j] = string.Format("{1}{0}+{2}{0}", startRow + i + 1, c1, c2);
                    }
                }
            }

            #endregion 填充数据

            #region 设置列格式以及合并单元格
            // 向表格插入列格式设置（列宽）
            List<Column> colList = new List<Column>();
            Column col1 = ExcelSheetDataAppend.GetColumn(1, (uint)colCount, 0, true, 18);
            colList.Add(col1);

            // 向表格中插入合并单元格信息
            // 这里只是将第一行的总标题合并
            List<MergeCell> mergeList = new List<MergeCell>();
            string startCell = "A1";
            string stopCell = string.Format("{0}1", ExcelSheetDataAppend.GetColumnName(colCount));
            MergeCell merge = ExcelSheetDataAppend.GetMergeCell(startCell, stopCell);
            mergeList.Add(merge);

            #endregion

            msg = string.Format("正在生成报表[{0}] 开始导出数据……", sheetName);
            CallProgress(sheetName, msg);

            FillData(workSheetPart, data, styles, formats, formulas, rowHeights, colList, mergeList);

            AddSheet(workSheetPart);

            return true;
        }

        protected override double GetMaxValue()
        {
            double max = 0;

            if (ExportData == null || ExportData.Tables.Count == 0)
                return 0;
            foreach (DataTable dt in ExportData.Tables)
            {
                if (dt == null || dt.Rows.Count == 0)
                    continue;
                if (dt.Columns.Count == 0)
                    continue;
                max += dt.Rows.Count * dt.Columns.Count;
            }

            return max;
        }

        #endregion 重写
    }
}
