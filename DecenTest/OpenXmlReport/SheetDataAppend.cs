
/*********************************************************************************************
 *	
 * 文件名称:    SheetDataAppend.cs
 *
 * 作    者:    wdp
 *	
 * 创作日期:    2018-7-26 13:47:58
 * 
 * 备    注:    表格数据处理类
 *              包含以下功能：
 *              1. 普通方式。可以对现有的Excel的Sheet页进行追加数据；但是效率低，速度慢
 *                  1. 组装单元格、行、列、单元格合并
 *                  2. 向表格内填充数据
 *              2. 使用Xml方式。效率高，速度快；但是只能新建Sheet页进行导出，不能对Excel已经存在的Sheet页进行追加数据
 *                  1. 组装列、单元格合并
 *                  2. 向表格内填充数据
 *               
*********************************************************************************************/

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenXmlReport
{
    /// <summary>
    /// 表格数据处理
    /// </summary>
    public class SheetDataAppend
    {
        public delegate void ExportProgressChangedDelegate(double value, double maxValue);
        /// <summary>
        /// 进度变化事件
        /// </summary>
        public event ExportProgressChangedDelegate ProgressChangedEvent;

        /// <summary>
        /// 取消导出
        /// </summary>
        public bool IsExportCanceled { get; set; }

        public SheetDataAppend()
        {

        }

        // 单个表格导出进度
        private void CallProgressChanged(double value, double maxValue)
        {
            if (ProgressChangedEvent != null)
                ProgressChangedEvent(value, maxValue);
        }

        // 这种方式效率不高，数据量大的话会出现内存溢出的情况
        // 优点是可以在现有Excel的Sheet页中追加数据
        /*
         * 使用这种方式进行导出时，需要以下步骤：
         * 1. 调用方法[IniSheet]初始化表格信息-如果向现有Sheet追加数据，则不能用此方法
         * 2. 调用[FillRows]或[FillData]方法填充表格：
         *      2.1 将需要填充的每个数据使用方法[GetCell]封装成Cell类型(如果使用方法[FillData]进行导出，这一步跳过)
         *      2.2 调用[GetRow]方法将多个Cell类型放到一行(如果使用方法[FillData]进行导出，这一步跳过)
         *      2.3 调用[FillRows]方法（或[FillData]方法）将行集合填充到表格中
         * 3. 根据需要是否有自定义列宽、是否有单元格合并执行方法：
         *      3.1 有自定义列宽
         *          3.1.1 调用方法[GetColumn]获取每列的自定义信息
         *          3.1.2 调用方法[FillColumns]填充到列样式
         *      3.2 有单元格合并
         *          3.2.1 调用方法[GetMergeCell]组装需要的单元格合并信息
         *          3.2.2 调用方法[FillMerge]将合并信息写入表格
         */
        #region OpenXml普通方式导出Excel

        /// <summary>
        /// 初始化表格
        /// </summary>
        /// <param name="workSheetPart"></param>
        public void IniSheet(WorksheetPart workSheetPart)
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
        public void FillColumns(WorksheetPart workSheetPart, List<Column> columnList)
        {
            var worksheet = workSheetPart.Worksheet;
            // 先查找表格中是否已经有数据，如果有数据需要将列信息插入到数据前面，否则虽然导出成功，但是打开excel的时候会报错
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
        /// 这个方法需要在数据导出完毕(FillRows方法执行完毕)以后调用
        /// 如果在数据导出之前调用，虽然导出成功，但是打开excel的时候会报错
        /// </summary>
        /// <param name="columnList"></param>
        /// <param name="sheetData"></param>
        /// <param name="mergeCellList"></param>
        /// <returns></returns>
        public void FillMerge(WorksheetPart workSheetPart, List<MergeCell> mergeCellList)
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
        public void FillRows(WorksheetPart workSheetPart, List<Row> rowList, int startIndex, int stopIndex)
        {
            double progress = 0;
            double maxNum = rowList.Count;
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
                    progress++;
                    CallProgressChanged(progress, maxNum);
                    if (IsExportCanceled)
                        return;
                });
            }
            workSheetPart.Worksheet.Save();
        }

        /// <summary>
        /// 查找表格中已经存在的数据内容
        /// </summary>
        /// <param name="workSheetPart"></param>
        /// <returns></returns>
        public SheetData FindSheetData(WorksheetPart workSheetPart)
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
        /// 填充数据
        /// </summary>
        /// <param name="workSheetPart"></param>
        /// <param name="data"></param>
        /// <param name="startRow"></param>
        /// <param name="startColumn"></param>
        public void FillData(WorksheetPart workSheetPart, object[,] data, uint[,] styles, int startRow, int startColumn)
        {
            var sheetData = FindSheetData(workSheetPart);

            for (int i = 0; i < data.GetLength(0); i++)
            {
                int rowIndex = startRow + i;
                List<Cell> list = new List<Cell>();
                for (int j = 0; j < data.GetLength(1); j++)
                {
                    int colIndex = startColumn + j;
                    Cell cell = GetCell(data[i, j].ToString(), string.Format("{0}{1}", GetColumnName(colIndex), rowIndex), styles[i, j]);
                    list.Add(cell);
                }
                Row row = GetRow((uint)rowIndex, list);
                sheetData.Append(row);
            }
        }

        #endregion OpenXml普通方式导出Excel

        // 这种方式效率很高，适合数据量大的导出
        // 但是这种方法只能新建Sheet，不能在现有的Sheet页中追加数据
        #region OpenXml通过OpenXmlWriter导出Excel

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
        public void FillData(WorksheetPart workSheetPart,
            object[,] data, uint[,] styles, double[] rowHeights, List<Column> columnList, List<MergeCell> mergeCellList,
            int startRow = 1, int startColumn = 1)
        {
            double progress = 0;
            double max = data.GetLength(0) * data.GetLength(1);
            OpenXmlWriter writer = OpenXmlWriter.Create(workSheetPart);
            try
            {
                // 拼接表格Xml文档的开头
                writer.WriteStartElement(new Worksheet());
                writer.WriteStartElement(new SheetViews()); //sheetViews
                writer.WriteStartElement(new SheetView() //sheetView
                {
                    TabSelected = true,
                    WorkbookViewId = 0U  //这里的下标是从0开始的
                });
                writer.WriteEndElement(); //sheetView 关闭标签
                writer.WriteEndElement(); //sheetViews 关闭标签

                // 拼接列信息
                if (columnList != null)
                    FillColumn(writer, columnList);

                // 以下开始拼接数据
                writer.WriteStartElement(new SheetData());
                for (int i = 0; i < data.GetLength(0); i++)
                {
                    int rowIndex = startRow + i;
                    //create a new list of attributes
                    List<OpenXmlAttribute> attributes = new List<OpenXmlAttribute>();
                    // add the row index attribute to the list
                    attributes.Add(new OpenXmlAttribute("r", null, rowIndex.ToString()));
                    // header start
                    Row row = new Row();
                    if (rowHeights[i] > 0)
                    {
                        attributes.Add(new OpenXmlAttribute("ht", null, rowHeights[i].ToString()));
                        attributes.Add(new OpenXmlAttribute("customHeight", null, "1"));
                    }
                    writer.WriteStartElement(new Row(), attributes);
                    for (int j = 0; j < data.GetLength(1); j++)
                    {
                        int colIndex = startColumn + j;
                        attributes = new List<OpenXmlAttribute>();
                        attributes.Add(new OpenXmlAttribute("t", null, "str")); //string
                        attributes.Add(new OpenXmlAttribute("s", null, styles[i, j].ToString())); //string
                        attributes.Add(new OpenXmlAttribute("r", "", string.Format("{0}{1}", GetColumnName(colIndex), rowIndex)));
                        writer.WriteStartElement(new Cell(), attributes);
                        if (data[i, j] == null)
                            data[i, j] = "";
                        CellValue cv = new CellValue(data[i, j].ToString());
                        cv.Space = SpaceProcessingModeValues.Preserve;
                        writer.WriteElement(cv);
                        writer.WriteEndElement();
                        progress++;
                        CallProgressChanged(progress, max);
                        if (IsExportCanceled)
                            return;
                    }
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();

                // 如果有单元格合并信息则进行拼接
                if (mergeCellList != null)
                    FillMerge(writer, mergeCellList);

                writer.Close();
                writer.Dispose();
            }
            catch (Exception ex)
            {
                /////////////////////
                throw ex;
            }
            finally
            {
                if (writer != null)
                {
                    writer.Dispose();
                }
            }
        }

        /// <summary>
        /// 使用OpenXmlWriter填充数据（大数据量）
        /// 这个方法只能新建，不能向已有的数据追加
        /// </summary>
        /// <param name="workSheetPart"></param>
        /// <param name="data">要填充的数据</param>
        /// <param name="styles">单元格样式</param>
        /// <param name="dataFormats">数据格式</param>
        /// <param name="rowHeights">行高数组（自动行高时为-1）</param>
        /// <param name="columnList">列样式数组</param>
        /// <param name="mergeCellList">合并单元格数组</param>
        /// <param name="startRow">起始的行号，从1开始</param>
        /// <param name="startColumn">起始的列号，从1开始</param>
        public void FillData(WorksheetPart workSheetPart,
            object[,] data, uint[,] styles, CellDataType[,] dataFormats, double[] rowHeights, List<Column> columnList, List<MergeCell> mergeCellList,
            int startRow = 1, int startColumn = 1)
        {
            double progress = 0;
            double max = data.GetLength(0) * data.GetLength(1);
            OpenXmlWriter writer = OpenXmlWriter.Create(workSheetPart);
            DateTime localTime = new DateTime(1900, 1, 1);// 初始时间
            try
            {
                // 拼接表格Xml文档的开头
                writer.WriteStartElement(new Worksheet());
                writer.WriteStartElement(new SheetViews()); //sheetViews
                writer.WriteStartElement(new SheetView() //sheetView
                {
                    TabSelected = true,
                    WorkbookViewId = 0U  //这里的下标是从0开始的
                });
                writer.WriteEndElement(); //sheetView 关闭标签
                writer.WriteEndElement(); //sheetViews 关闭标签
                // 拼接列信息
                if (columnList != null)
                    FillColumn(writer, columnList);

                writer.WriteStartElement(new SheetData());
                for (int i = 0; i < data.GetLength(0); i++)
                {
                    int rowIndex = startRow + i;
                    //create a new list of attributes
                    List<OpenXmlAttribute> attributes = new List<OpenXmlAttribute>();
                    // add the row index attribute to the list
                    attributes.Add(new OpenXmlAttribute("r", null, rowIndex.ToString()));
                    // header start
                    Row row = new Row();
                    if (rowHeights[i] > 0)
                    {
                        attributes.Add(new OpenXmlAttribute("ht", null, rowHeights[i].ToString()));
                        attributes.Add(new OpenXmlAttribute("customHeight", null, "1"));
                    }
                    writer.WriteStartElement(new Row(), attributes);
                    for (int j = 0; j < data.GetLength(1); j++)
                    {
                        int colIndex = startColumn + j;
                        attributes = new List<OpenXmlAttribute>();
                        if (dataFormats[i, j] == CellDataType.String)
                        {
                            attributes.Add(new OpenXmlAttribute("t", null, "str")); //数据格式
                        }
                        attributes.Add(new OpenXmlAttribute("s", null, styles[i, j].ToString())); //样式
                        attributes.Add(new OpenXmlAttribute("r", "", string.Format("{0}{1}", GetColumnName(colIndex), rowIndex)));
                        writer.WriteStartElement(new Cell(), attributes);
                        if (data[i, j] == null)
                            data[i, j] = "";

                        string val = data[i, j].ToString();
                        if (dataFormats[i, j] == CellDataType.DateTime)
                        {
                            DateTime dt = DateTime.Now;
                            if(DateTime.TryParse(val,out dt))
                            {
                                double span = dt.Subtract(localTime).TotalDays;
                                val = span.ToString();
                            }
                        }
                        CellValue cv = new CellValue(val);
                        cv.Space = SpaceProcessingModeValues.Preserve;
                        writer.WriteElement(cv);

                        writer.WriteEndElement();
                        progress++;
                        CallProgressChanged(progress, max);
                        if (IsExportCanceled)
                            return;
                    }
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();

                // 如果有单元格合并信息则进行拼接
                if (mergeCellList != null)
                    FillMerge(writer, mergeCellList);

                writer.Close();
                writer.Dispose();
            }
            catch (Exception ex)
            {
                /////////////////////
                throw ex;
            }
            finally
            {
                if (writer != null)
                {
                    writer.Dispose();
                }
            }
        }

        /// <summary>
        /// 使用xml模式添加列
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="columnList"></param>
        public void FillColumn(OpenXmlWriter writer, List<Column> columnList)
        {
            writer.WriteStartElement(new Columns());
            columnList.ForEach(col =>
            {
                List<OpenXmlAttribute> attributes = new List<OpenXmlAttribute>();
                attributes.Add(new OpenXmlAttribute("min", null, col.Min.ToString()));
                attributes.Add(new OpenXmlAttribute("max", null, col.Max.ToString()));
                if (col.CustomWidth)
                {
                    attributes.Add(new OpenXmlAttribute("width", null, col.Width.ToString()));
                    attributes.Add(new OpenXmlAttribute("customWidth", null, "1"));
                }
                attributes.Add(new OpenXmlAttribute("style", null, col.Style.ToString()));
                attributes.Add(new OpenXmlAttribute("collapsed", null, "1"));
                writer.WriteStartElement(new Column(), attributes);
                writer.WriteEndElement();
            });
            writer.WriteEndElement();
        }

        /// <summary>
        /// 使用xml模式添加合并单元格
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="mergeCellList"></param>
        public void FillMerge(OpenXmlWriter writer, List<MergeCell> mergeCellList)
        {
            writer.WriteStartElement(new MergeCells());
            mergeCellList.ForEach(merge =>
            {
                List<OpenXmlAttribute> attributes = new List<OpenXmlAttribute>();
                attributes.Add(new OpenXmlAttribute("ref", null, merge.Reference));
                writer.WriteStartElement(new MergeCell(), attributes);
                writer.WriteEndElement();
            });
            writer.WriteEndElement();
        }

        #endregion OpenXml通过OpenXmlWriter导出Excel

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

    }
}
