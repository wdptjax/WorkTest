
/*********************************************************************************************
 *	
 * 文件名称:    SheetDataAppend.cs
 *
 * 作    者:    吴德鹏
 *	
 * 创作日期:    2018-7-25 9:50:43
 * 
 * 备    注:    向表格中添加数据
 *              
 *                                
 *               
*********************************************************************************************/

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReportExport.OpenXmlReport
{
    /// <summary>
    /// 表格数据处理
    /// </summary>
    public class SheetDataAppend
    {
        /// <summary>
        /// 组装单元格信息
        /// </summary>
        /// <param name="cellText">单元格显示信息</param>
        /// <param name="cellPosition">单元格位置（A1,B2,C3,D4……）</param>
        /// <param name="styleIndex">单元格样式序号</param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        public static Cell GetCell(string cellText, string cellPosition, UInt32 styleIndex, CellValues dataType = CellValues.String)
        {
            Cell cell = new Cell()
            {
                CellReference = cellPosition,
                StyleIndex = (UInt32Value)styleIndex,
                DataType = dataType
            };
            CellValue cellValue = new CellValue();
            cellValue.Text = cellText;
            cellValue.Space = SpaceProcessingModeValues.Preserve;

            cell.Append(cellValue);

            return cell;
        }

        /// <summary>
        /// 组装行信息
        /// </summary>
        /// <param name="rowIndex"></param>
        /// <param name="cellList"></param>
        /// <param name="customHeight"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static Row GetRow(UInt32 rowIndex, List<Cell> cellList, bool customHeight = false, double height = 0D)
        {
            Row row = new Row() { RowIndex = rowIndex };
            if (customHeight)
            {
                row.CustomHeight = true;
                row.Height = height;
            }

            cellList.ForEach(cell => row.Append(cell));
            return row;
        }

        /// <summary>
        /// 组装列信息
        /// </summary>
        /// <param name="start">起始列（从1开始）</param>
        /// <param name="stop">结束列（从1开始）</param>
        /// <param name="styleIndex">列样式序号</param>
        /// <param name="customWidth">是否自定义列宽</param>
        /// <param name="width">自定义列宽</param>
        /// <returns></returns>
        public static Column GetColumn(UInt32 start, UInt32 stop, UInt32 styleIndex, bool customWidth = false, double width = 0D)
        {
            Column column = new Column()
            {
                Min = (UInt32Value)start,
                Max = (UInt32Value)stop,
                Style = (UInt32Value)styleIndex,
                Collapsed = true
            };

            if (customWidth)
            {
                column.CustomWidth = customWidth;
                column.Width = width;
            }

            return column;
        }

        /// <summary>
        /// 生成单元格合并信息
        /// </summary>
        /// <param name="startCell">起始单元格</param>
        /// <param name="stopCell">结束单元格</param>
        /// <returns></returns>
        public static MergeCell GetMergeCell(string startCell, string stopCell)
        {
            //例如：Reference = "A1:D1"
            MergeCell mergeCell = new MergeCell() { Reference = string.Format("{0}:{1}", startCell, stopCell) };
            return mergeCell;
        }

        /// <summary>
        /// 填充列
        /// </summary>
        /// <param name="workSheetPart"></param>
        /// <param name="columnList"></param>
        public static void FillColumns(WorksheetPart workSheetPart, List<Column> columnList)
        {
            var worksheet = workSheetPart.Worksheet;
            var sheetData = worksheet.ChildElements.FirstOrDefault(i => i is SheetData);
            bool sign = sheetData != null;
            if (columnList != null && columnList.Count > 0)
            {
                Columns columns = new Columns();
                columnList.ForEach(col => columns.Append(col));
                if (sign)
                    worksheet.InsertBefore(columns, sheetData);
                else
                    worksheet.Append(columns);
            }
            worksheet.Save();
        }

        /// <summary>
        /// 向Excel中写入合并单元格信息
        /// </summary>
        /// <param name="columnList"></param>
        /// <param name="sheetData"></param>
        /// <param name="mergeCellList"></param>
        /// <returns></returns>
        public static void FillMerge(WorksheetPart workSheetPart, List<MergeCell> mergeCellList)
        {
            var worksheet = workSheetPart.Worksheet;
            if (mergeCellList != null && mergeCellList.Count > 0)
            {
                MergeCells mergeCells = new MergeCells();
                mergeCellList.ForEach(merge => mergeCells.Append(merge));
                worksheet.Append(mergeCells);
            }
            worksheet.Save();
        }

        /// <summary>
        /// 填充行数据
        /// </summary>
        /// <param name="workSheetPart"></param>
        /// <param name="rowList"></param>
        /// <param name="startIndex"></param>
        /// <param name="stopIndex"></param>
        public static void FillRows(WorksheetPart workSheetPart, List<Row> rowList, int startIndex, int stopIndex)
        {
            var sheetdata = FindSheetData(workSheetPart);
            var rows = sheetdata.ChildElements.Select(i => (Row)i);
            bool sign = false;
            uint min;
            uint max;
            Row minRow = null;
            if (rows.LongCount() == 0)
                sign = true;
            else
            {
                min = rows.Select(i => (UInt32)i.RowIndex).Min();
                max = rows.Select(i => (UInt32)i.RowIndex).Max();
                minRow = rows.First(i => i.RowIndex == min);
                if (stopIndex <= min)
                    sign = false;
                else if (startIndex >= max)
                    sign = true;
                else
                {
                    throw new Exception("不能在已经存在的数据行中插入新行，只能在最后添加行或者在最前面插入行");
                }
            }
            if (rowList != null && rowList.Count > 0)
            {
                rowList.ForEach(row =>
                {
                    if (sign)
                    {
                        sheetdata.Append(row);
                    }
                    else
                    {
                        sheetdata.InsertBefore(row, minRow);
                    }
                });
            }
            workSheetPart.Worksheet.Save();
        }

        /// <summary>
        /// 初始化表头
        /// </summary>
        /// <param name="workSheetPart"></param>
        public static void IniSheet(WorksheetPart workSheetPart)
        {
            Worksheet worksheet = new Worksheet();
            workSheetPart.Worksheet = worksheet;
            worksheet.AddNamespaceDeclaration("r", "http://schemas.openxmlformats.org/officeDocument/2006/relationships");
            SheetDimension sheetDimension = new SheetDimension() { Reference = "A1" };
            SheetViews sheetViews = new SheetViews();
            SheetView sheetView = new SheetView() { TabSelected = true, ShowRuler = true, ShowOutlineSymbols = true, DefaultGridColor = true, ColorId = (UInt32Value)64U, ZoomScale = (UInt32Value)100U, WorkbookViewId = (UInt32Value)0U };
            sheetViews.Append(sheetView);
            SheetFormatProperties sheetFormatProperties = new SheetFormatProperties() { DefaultRowHeight = 15D };
            worksheet.Append(sheetDimension);
            worksheet.Append(sheetViews);
            worksheet.Append(sheetFormatProperties);
            worksheet.Save();
        }

        /// <summary>
        /// 查找表格中已经存在的数据内容
        /// </summary>
        /// <param name="workSheetPart"></param>
        /// <returns></returns>
        public static SheetData FindSheetData(WorksheetPart workSheetPart)
        {
            Worksheet worksheet = workSheetPart.Worksheet;
            if (worksheet == null)
                return null;
            var data = worksheet.ChildElements.FirstOrDefault(i => i.LocalName == "sheetData");
            if (data == null)
            {
                SheetData sheetdata = new SheetData();
                worksheet.Append(sheetdata);
                return sheetdata;
            }

            return data as SheetData;
        }

        /// <summary>
        /// 使用OpenXmlWriter填充数据（大数据量）
        /// </summary>
        /// <param name="workSheetPart"></param>
        /// <param name="data">要填充的数据</param>
        /// <param name="startRow">起始的行号，从1开始</param>
        /// <param name="startColumn">起始的列号，从1开始</param>
        public static void FillData(WorksheetPart workSheetPart, object[,] data, int startRow, int startColumn)
        {
            OpenXmlWriter writer = OpenXmlWriter.Create(workSheetPart);
            //使用OpenXML麻烦的地方就是我们要用SDK去拼接XML内容
            writer.WriteStartElement(new Worksheet());
            writer.WriteStartElement(new SheetViews()); //sheetViews
            writer.WriteStartElement(new SheetView() //sheetView
            {
                TabSelected = true,
                WorkbookViewId = 0U  //这里的下标是从0开始的
            });
            writer.WriteEndElement(); //sheetView 关闭标签
            writer.WriteEndElement(); //sheetViews 关闭标签
            writer.WriteStartElement(new SheetData());
            for (int i = 0; i < data.GetLength(0); i++)
            {
                int row = startRow + i;
                //create a new list of attributes
                List<OpenXmlAttribute> attributes = new List<OpenXmlAttribute>();
                // add the row index attribute to the list
                attributes.Add(new OpenXmlAttribute("r", null, row.ToString()));
                // header start
                writer.WriteStartElement(new Row(), attributes);
                for (int j = 0; j < data.GetLength(1); j++)
                {
                    int col = startColumn + j;
                    attributes = new List<OpenXmlAttribute>();
                    attributes.Add(new OpenXmlAttribute("t", null, "str")); //string
                    attributes.Add(new OpenXmlAttribute("s", null, "1")); //string
                    attributes.Add(new OpenXmlAttribute("r", "", string.Format("{0}{1}", GetColumnName(col), row)));
                    writer.WriteStartElement(new Cell(), attributes);
                    writer.WriteElement(new CellValue(data[i, j].ToString()));
                    writer.WriteEndElement();

                }
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
            writer.Close();
        }

        public static void FillData1(WorksheetPart workSheetPart, object[,] data, int startRow, int startColumn)
        {
            var sheetData = FindSheetData(workSheetPart);

            for (int i = 0; i < data.GetLength(0); i++)
            {
                int rowIndex = startRow + i;
                List<Cell> list = new List<Cell>();
                for (int j = 0; j < data.GetLength(1); j++)
                {
                    int colIndex = startColumn + j;
                    Cell cell = GetCell(data[i, j].ToString(), string.Format("{0}{1}", GetColumnName(colIndex), rowIndex), 1);
                    list.Add(cell);
                }
                Row row = GetRow((uint)rowIndex, list);
                sheetData.Append(row);
            }
        }



        /// <summary>
        /// 根据序号返回Excel中的列名
        /// </summary>
        /// <param name="columnIndex">列序号，从1开始</param>
        /// <returns></returns>
        public static string GetColumnName(int columnIndex)
        {
            if (columnIndex <= 26)
            {
                int charIndex = 65 + columnIndex - 1;
                return ((char)charIndex).ToString();
            }

            int num1 = columnIndex / 26;
            string str = GetColumnName(num1);
            int num2 = columnIndex % 26;
            string str2 = GetColumnName(num2);
            return str + str2;
        }


        public static object[,] GetData()
        {
            Random rd = new Random();
            object[,] data = new object[1000000, 4];
            for (int i = 0; i < data.GetLength(0); i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    data[i, j] = rd.Next(10, 50);
                }
            }
            return data;
        }
    }
}
