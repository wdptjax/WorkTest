
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
    class SheetDataAppend
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
        /// 填充数据（填充表头等小数据量）
        /// </summary>
        /// <param name="rowList"></param>
        /// <returns></returns>
        public static SheetData GetSheetData(List<Row> rowList)
        {
            SheetData sheetData = new SheetData();
            rowList.ForEach(row => sheetData.Append(row));
            return sheetData;
        }

        /// <summary>
        /// 合并单元格
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
        /// 生成表格sheet页
        /// </summary>
        /// <param name="columnList"></param>
        /// <param name="sheetData"></param>
        /// <param name="mergeCellList"></param>
        /// <returns></returns>
        public static Worksheet GetWorkSheet(List<Column> columnList, SheetData sheetData = null, List<MergeCell> mergeCellList = null)
        {
            Worksheet worksheet = new Worksheet();
            worksheet.AddNamespaceDeclaration("r", "http://schemas.openxmlformats.org/officeDocument/2006/relationships");
            SheetDimension sheetDimension = new SheetDimension() { Reference = "A1" };
            SheetViews sheetViews = new SheetViews();
            SheetView sheetView = new SheetView() { TabSelected = true, ShowRuler = true, ShowOutlineSymbols = true, DefaultGridColor = true, ColorId = (UInt32Value)64U, ZoomScale = (UInt32Value)100U, WorkbookViewId = (UInt32Value)0U };
            sheetViews.Append(sheetView);
            SheetFormatProperties sheetFormatProperties = new SheetFormatProperties() { DefaultRowHeight = 15D };
            worksheet.Append(sheetDimension);
            worksheet.Append(sheetViews);
            worksheet.Append(sheetFormatProperties);

            if (columnList != null)
            {
                Columns columns = new Columns();
                columnList.ForEach(col => columns.Append(col));
                worksheet.Append(columns);
            }

            if (sheetData != null)
                worksheet.Append(sheetData);
            if (mergeCellList != null && mergeCellList.Count > 0)
            {
                MergeCells mergeCells = new MergeCells();
                mergeCellList.ForEach(merge => mergeCells.Append(merge));
                worksheet.Append(mergeCells);
            }

            return worksheet;
        }

        /// <summary>
        /// 填充数据（大数据量）
        /// </summary>
        /// <param name="data"></param>
        /// <param name="workSheetPart"></param>
        /// <param name="startRow">起始的行号，从1开始</param>
        /// <param name="startColumn">起始的列号，从1开始</param>
        public static void FillData(object[,] data, WorksheetPart workSheetPart, int startRow, int startColumn)
        {
            OpenXmlWriter writer = OpenXmlWriter.Create(workSheetPart);
            writer.WriteStartElement(new Worksheet());
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
            writer.WriteEndElement();
            writer.Close();
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
            object[,] data = new object[100, 4];
            for (int i = 0; i < 100; i++)
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
