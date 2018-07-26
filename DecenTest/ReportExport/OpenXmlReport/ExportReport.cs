
/*********************************************************************************************
 *	
 * 文件名称:    ExportReport.cs
 *
 * 作    者:    吴德鹏
 *	
 * 创作日期:    2018-7-24 16:32:25
 * 
 * 备    注:    描述类的用途
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
using System.Linq;
using System.Text;

namespace ReportExport.OpenXmlReport
{
    public class ExportReport
    {
        private SpreadsheetDocument _document = null;
        private void CreateDocument(string fileName)
        {
            if (File.Exists(fileName))
                File.Delete(fileName);
            // 创建Excel文件
            _document = SpreadsheetDocument.Create(fileName, SpreadsheetDocumentType.Workbook);
        }

        public void Export()
        {
            string filename = @"E:\Project\Tracker800_V9\trunk\Client\Bin\Data\Report\Test.xlsx";
            CreateDocument(filename);
            // 添加一个工作区
            _document.AddWorkbookPart();

            WorksheetPart workSheetPart = _document.WorkbookPart.AddNewPart<WorksheetPart>();

            WorkbookStylesPart wbsp = _document.WorkbookPart.AddNewPart<WorkbookStylesPart>();

            var style = GetStylesheet();

            wbsp.Stylesheet = style;
            wbsp.Stylesheet.Save();

            SheetDataAppend.FillData(SheetDataAppend.GetData(), workSheetPart, 4, 1);

            var workSheet = GetWorksheet();
            workSheetPart.Worksheet = workSheet;
            workSheetPart.Worksheet.Save();


            if (_document.WorkbookPart.Workbook == null)
            {
                _document.WorkbookPart.Workbook = new Workbook();
                _document.WorkbookPart.Workbook.Append(new Sheets());
            }
            //数据写入完成后，注册一个sheet引用到workbook.xml, 也就是在excel最下面的sheet name
            var sheet = new Sheet()
            {
                Name = "7月-1",
                SheetId = (UInt32Value)1U,
                Id = _document.WorkbookPart.GetIdOfPart(workSheetPart)
            };
            _document.WorkbookPart.Workbook.Sheets.Append(sheet);
            _document.Close();
            using (SpreadsheetDocument myDoc = SpreadsheetDocument.Open(filename, true))
            {
                WorksheetPart part = myDoc.WorkbookPart.AddNewPart<WorksheetPart>();

                SheetDataAppend.FillData(SheetDataAppend.GetData(), part, 4, 1);
                if (myDoc.WorkbookPart.Workbook == null)
                {
                    myDoc.WorkbookPart.Workbook = new Workbook();
                    myDoc.WorkbookPart.Workbook.Append(new Sheets());
                }
                //数据写入完成后，注册一个sheet引用到workbook.xml, 也就是在excel最下面的sheet name
                var sheet1 = new Sheet()
                {
                    Name = "test",
                    SheetId = UInt32Value.FromUInt32(1),
                    Id = myDoc.WorkbookPart.GetIdOfPart(part)
                };
                myDoc.WorkbookPart.Workbook.Sheets.Append(sheet1);
            }
        }

        private Stylesheet GetStylesheet()
        {
            // 字体样式
            Font font1 = SheetStyles.GetFont(11, "宋体");
            Font font2 = SheetStyles.GetFont(20, "微软雅黑", "0000FF", UnderlineValues.Double, true);
            Font font3 = SheetStyles.GetFont(12, "宋体", "00FF00");
            Font font4 = SheetStyles.GetFont(9, "宋体", "FF0000", UnderlineValues.None, true);

            List<Font> fontlist = new List<Font>();
            fontlist.Add(font1);
            fontlist.Add(font2);
            fontlist.Add(font3);
            fontlist.Add(font4);

            // 填充样式
            Fill fill1 = SheetStyles.GetFill(PatternValues.None);
            Fill fill2 = SheetStyles.GetFill(PatternValues.Gray125);
            Fill fill3 = SheetStyles.GetFill(PatternValues.Solid, "FFFFFF00");
            Fill fill4 = SheetStyles.GetFill(PatternValues.Solid, "FFFF00FF");

            List<Fill> filllist = new List<Fill>();
            filllist.Add(fill1);
            filllist.Add(fill2);
            filllist.Add(fill3);
            filllist.Add(fill4);

            // 边框样式
            //无边框
            LeftBorder left0 = (LeftBorder)SheetStyles.GetBorderLineStyle(2, BorderStyleValues.None);
            RightBorder right0 = (RightBorder)SheetStyles.GetBorderLineStyle(3, BorderStyleValues.None);
            TopBorder top0 = (TopBorder)SheetStyles.GetBorderLineStyle(0, BorderStyleValues.None);
            BottomBorder bottom0 = (BottomBorder)SheetStyles.GetBorderLineStyle(1, BorderStyleValues.None);
            DiagonalBorder diag0 = (DiagonalBorder)SheetStyles.GetBorderLineStyle(4, BorderStyleValues.None);
            //单边框
            LeftBorder left1 = (LeftBorder)SheetStyles.GetBorderLineStyle(2, BorderStyleValues.Thin, "000000");
            RightBorder right1 = (RightBorder)SheetStyles.GetBorderLineStyle(3, BorderStyleValues.Thin, "000000");
            TopBorder top1 = (TopBorder)SheetStyles.GetBorderLineStyle(0, BorderStyleValues.Thin, "000000");
            BottomBorder bottom1 = (BottomBorder)SheetStyles.GetBorderLineStyle(1, BorderStyleValues.Thin, "000000");
            //单边框
            LeftBorder left2 = (LeftBorder)SheetStyles.GetBorderLineStyle(2, BorderStyleValues.Thin, "000000");
            RightBorder right2 = (RightBorder)SheetStyles.GetBorderLineStyle(3, BorderStyleValues.Thin, "000000");
            TopBorder top2 = (TopBorder)SheetStyles.GetBorderLineStyle(0, BorderStyleValues.Thin, "000000");
            BottomBorder bottom2 = (BottomBorder)SheetStyles.GetBorderLineStyle(1, BorderStyleValues.Thin, "000000");
            DiagonalBorder diag2 = (DiagonalBorder)SheetStyles.GetBorderLineStyle(4, BorderStyleValues.Thin, "000000");
            //粗轧框线
            LeftBorder left3 = (LeftBorder)SheetStyles.GetBorderLineStyle(2, BorderStyleValues.Medium, "000000");
            RightBorder right3 = (RightBorder)SheetStyles.GetBorderLineStyle(3, BorderStyleValues.Medium, "000000");
            TopBorder top3 = (TopBorder)SheetStyles.GetBorderLineStyle(0, BorderStyleValues.Medium, "000000");
            BottomBorder bottom3 = (BottomBorder)SheetStyles.GetBorderLineStyle(1, BorderStyleValues.Medium, "000000");
            DiagonalBorder diag3 = (DiagonalBorder)SheetStyles.GetBorderLineStyle(4, BorderStyleValues.Medium, "000000");

            List<BorderPropertiesType> list = new List<BorderPropertiesType>();
            list.Add(left0);
            list.Add(right0);
            list.Add(top0);
            list.Add(bottom0);
            // 无边框
            Border border1 = SheetStyles.GetBorder(list);

            list.Clear();
            list.Add(left1);
            list.Add(right1);
            list.Add(top1);
            list.Add(bottom1);
            // 有边框
            Border border2 = SheetStyles.GetBorder(list);

            list.Clear();
            list.Add(left2);
            list.Add(right2);
            list.Add(top2);
            list.Add(bottom2);
            list.Add(diag2);
            // 中间有斜边框
            Border border3 = SheetStyles.GetBorder(list, false, true);

            list.Clear();
            list.Add(left3);
            list.Add(right3);
            list.Add(top3);
            list.Add(bottom3);
            // 粗边框
            Border border4 = SheetStyles.GetBorder(list);

            List<Border> borderList = new List<Border>();
            borderList.Add(border1);
            borderList.Add(border2);
            borderList.Add(border3);
            borderList.Add(border4);

            // 单元格样式
            CellFormat cellFormat0 = SheetStyles.GetCellFormat(HorizontalAlignmentValues.Left,
                VerticalAlignmentValues.Center, 0, 0, 1);
            CellFormat cellFormat1 = SheetStyles.GetCellFormat(HorizontalAlignmentValues.Center,
                 VerticalAlignmentValues.Center, 1, 2, 3);
            CellFormat cellFormat2 = SheetStyles.GetCellFormat(HorizontalAlignmentValues.Left,
                VerticalAlignmentValues.Center, 2, 3, 1);
            CellFormat cellFormat3 = SheetStyles.GetCellFormat(HorizontalAlignmentValues.Left,
                VerticalAlignmentValues.Center, 3, 2, 1);
            CellFormat cellFormat4 = SheetStyles.GetCellFormat(HorizontalAlignmentValues.Left,
                VerticalAlignmentValues.Center, 2, 2, 2);
            List<CellFormat> cellList = new List<CellFormat>();
            cellList.Add(cellFormat0);//默认样式-0
            cellList.Add(cellFormat1);//1
            cellList.Add(cellFormat2);//2
            cellList.Add(cellFormat3);//3
            cellList.Add(cellFormat4);//4

            Stylesheet style = SheetStyles.GetStyleSheet(fontlist, filllist, borderList, cellList);
            return style;
        }

        private Worksheet GetWorksheet()
        {
            Column col1 = SheetDataAppend.GetColumn(1, 4, 0, true, 20);
            List<Column> colList = new List<Column>();
            colList.Add(col1);

            Cell cell1 = SheetDataAppend.GetCell("     省（区、市） 2018年7 月份重点频段占用度统计表", "A1", 1);
            Cell cell2 = SheetDataAppend.GetCell("", "B1", 1);
            Cell cell3 = SheetDataAppend.GetCell("", "C1", 1);
            Cell cell4 = SheetDataAppend.GetCell("", "D1", 1);

            Cell cell5 = SheetDataAppend.GetCell("序号", "A2", 2);
            Cell cell6 = SheetDataAppend.GetCell("          频段(MHz)\n地区", "B2", 4);
            Cell cell7 = SheetDataAppend.GetCell("137-200", "C2", 2);
            Cell cell8 = SheetDataAppend.GetCell("88-108", "D2", 2);

            Cell cell9 = SheetDataAppend.GetCell("注：直辖市填报所有固定站名称和数据，其它省（区）填报省级站及地市中心站名称和数据。", "A3", 3);
            Cell cell10 = SheetDataAppend.GetCell("", "B3", 3);
            Cell cell11 = SheetDataAppend.GetCell("", "C3", 3);
            Cell cell12 = SheetDataAppend.GetCell("", "D3", 3);

            List<Cell> celllist = new List<Cell>();
            celllist.Add(cell1);
            celllist.Add(cell2);
            celllist.Add(cell3);
            celllist.Add(cell4);
            Row row1 = SheetDataAppend.GetRow(1, celllist, true, 45);

            celllist.Clear();
            celllist.Add(cell5);
            celllist.Add(cell6);
            celllist.Add(cell7);
            celllist.Add(cell8);
            Row row2 = SheetDataAppend.GetRow(2, celllist);

            celllist.Clear();
            celllist.Add(cell9);
            celllist.Add(cell10);
            celllist.Add(cell11);
            celllist.Add(cell12);
            Row row3 = SheetDataAppend.GetRow(3, celllist);

            List<Row> rowList = new List<Row>();
            rowList.Add(row1);
            rowList.Add(row2);
            rowList.Add(row3);

            var data = SheetDataAppend.GetSheetData(rowList);
            MergeCell merge1 = SheetDataAppend.GetMergeCell("A1", "D1");
            MergeCell merge2 = SheetDataAppend.GetMergeCell("A3", "D3");
            List<MergeCell> mergeList = new List<MergeCell>();
            mergeList.Add(merge1);
            mergeList.Add(merge2);

            Worksheet worksheet = SheetDataAppend.GetWorkSheet(colList, data, mergeList);
            return worksheet;
        }

    }
}
