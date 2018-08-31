
/*********************************************************************************************
 *	
 * 文件名称:    Export.cs
 *
 * 作    者:    吴德鹏
 *	
 * 创作日期:    2018-7-24 14:50:14
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

namespace ReportExport
{
    public class Export
    {
        private SpreadsheetDocument _document = null;
        private void OpenWorkDocument(string fileName)
        {
            if (File.Exists(fileName))
                File.Delete(fileName);
            _document = SpreadsheetDocument.Create(fileName, SpreadsheetDocumentType.Workbook);
            _document.AddWorkbookPart();
        }

        public void ExportReport()
        {
            OpenWorkDocument(@"E:\Project\Tracker800_V9\trunk\Client\Bin\Data\Report\Test.xlsx");

            WorksheetPart workSheetPart = _document.WorkbookPart.AddNewPart<WorksheetPart>();

            WorkbookStylesPart wbsp = _document.WorkbookPart.AddNewPart<WorkbookStylesPart>();
            SheetStyles.GetSheetStyles(wbsp);

            GeneratePartContent(workSheetPart);

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
        }

        // Generates content of part.
        private void GeneratePartContent(WorksheetPart part)
        {
            Worksheet worksheet1 = new Worksheet();
            worksheet1.AddNamespaceDeclaration("r", "http://schemas.openxmlformats.org/officeDocument/2006/relationships");
            SheetDimension sheetDimension1 = new SheetDimension() { Reference = "A1" };

            SheetViews sheetViews1 = new SheetViews();
            SheetView sheetView1 = new SheetView() { TabSelected = true, ShowRuler = true, ShowOutlineSymbols = true, DefaultGridColor = true, ColorId = (UInt32Value)64U, ZoomScale = (UInt32Value)100U, WorkbookViewId = (UInt32Value)0U };

            sheetViews1.Append(sheetView1);
            SheetFormatProperties sheetFormatProperties1 = new SheetFormatProperties() { DefaultRowHeight = 15D };

            Columns columns1 = new Columns();
            Column column1 = new Column() { Min = (UInt32Value)1U, Max = (UInt32Value)1U, Width = 12.421875D, Style = (UInt32Value)0U, CustomWidth = true, Collapsed = true };
            Column column2 = new Column() { Min = (UInt32Value)2U, Max = (UInt32Value)2U, Width = 20.37109375D, Style = (UInt32Value)0U, CustomWidth = true, Collapsed = true };
            Column column3 = new Column() { Min = (UInt32Value)3U, Max = (UInt32Value)3U, Width = 24.47265625D, Style = (UInt32Value)0U, CustomWidth = true, Collapsed = true };
            Column column4 = new Column() { Min = (UInt32Value)4U, Max = (UInt32Value)4U, Width = 24.47265625D, Style = (UInt32Value)0U, CustomWidth = true, Collapsed = true };

            columns1.Append(column1);
            columns1.Append(column2);
            columns1.Append(column3);
            columns1.Append(column4);

            SheetData sheetData1 = new SheetData();

            Row row1 = new Row() { RowIndex = (UInt32Value)1U, Height = 44.35D, CustomHeight = true };

            Cell cell1 = new Cell() { CellReference = "A1", StyleIndex = (UInt32Value)1U, DataType = CellValues.SharedString };
            CellValue cellValue1 = new CellValue();
            cellValue1.Text = "     省（区、市） 2018年7 月份重点频段占用度统计表";

            cell1.Append(cellValue1);
            Cell cell2 = new Cell() { CellReference = "B1", StyleIndex = (UInt32Value)1U };
            Cell cell3 = new Cell() { CellReference = "C1", StyleIndex = (UInt32Value)1U };
            Cell cell4 = new Cell() { CellReference = "D1", StyleIndex = (UInt32Value)1U };

            row1.Append(cell1);
            row1.Append(cell2);
            row1.Append(cell3);
            row1.Append(cell4);

            Row row2 = new Row() { RowIndex = (UInt32Value)2U };

            Cell cell5 = new Cell() { CellReference = "A2", StyleIndex = (UInt32Value)3U, DataType = CellValues.SharedString };
            CellValue cellValue2 = new CellValue();
            cellValue2.Text = "序号";

            cell5.Append(cellValue2);

            Cell cell6 = new Cell() { CellReference = "B2", StyleIndex = (UInt32Value)4U, DataType = CellValues.SharedString };
            CellValue cellValue3 = new CellValue();
            cellValue3.Text = "           频段(MHz)\n地区";

            cell6.Append(cellValue3);

            Cell cell7 = new Cell() { CellReference = "C2", StyleIndex = (UInt32Value)3U, DataType = CellValues.SharedString };
            CellValue cellValue4 = new CellValue();
            cellValue4.Text = "137-200";

            cell7.Append(cellValue4);

            Cell cell8 = new Cell() { CellReference = "D2", StyleIndex = (UInt32Value)3U, DataType = CellValues.SharedString };
            CellValue cellValue5 = new CellValue();
            cellValue5.Text = "88-108";

            cell8.Append(cellValue5);

            row2.Append(cell5);
            row2.Append(cell6);
            row2.Append(cell7);
            row2.Append(cell8);

            Row row3 = new Row() { RowIndex = (UInt32Value)3U };

            Cell cell9 = new Cell() { CellReference = "A3", StyleIndex = (UInt32Value)3U, DataType = CellValues.SharedString };
            CellValue cellValue6 = new CellValue();
            cellValue6.Text = "1";

            cell9.Append(cellValue6);

            Cell cell10 = new Cell() { CellReference = "B3", StyleIndex = (UInt32Value)3U, DataType = CellValues.SharedString };
            CellValue cellValue7 = new CellValue();
            cellValue7.Text = "192.168.120.77";

            cell10.Append(cellValue7);

            Cell cell11 = new Cell() { CellReference = "C3", StyleIndex = (UInt32Value)3U, DataType = CellValues.SharedString };
            CellValue cellValue8 = new CellValue();
            cellValue8.Text = "0.40%";

            cell11.Append(cellValue8);

            Cell cell12 = new Cell() { CellReference = "D3", StyleIndex = (UInt32Value)3U, DataType = CellValues.SharedString };
            CellValue cellValue9 = new CellValue();
            cellValue9.Text = "65.54%";

            cell12.Append(cellValue9);

            row3.Append(cell9);
            row3.Append(cell10);
            row3.Append(cell11);
            row3.Append(cell12);

            Row row4 = new Row() { RowIndex = (UInt32Value)4U, Height = 29.55D, CustomHeight = true };

            Cell cell13 = new Cell() { CellReference = "A4", StyleIndex = (UInt32Value)5U, DataType = CellValues.SharedString };
            CellValue cellValue10 = new CellValue();
            cellValue10.Text = "注：直辖市填报所有固定站名称和数据，其它省（区）填报省级站及地市中心站名称和数据。";

            cell13.Append(cellValue10);
            Cell cell14 = new Cell() { CellReference = "B4", StyleIndex = (UInt32Value)5U };
            Cell cell15 = new Cell() { CellReference = "C4", StyleIndex = (UInt32Value)5U };
            Cell cell16 = new Cell() { CellReference = "D4", StyleIndex = (UInt32Value)5U };

            row4.Append(cell13);
            row4.Append(cell14);
            row4.Append(cell15);
            row4.Append(cell16);

            sheetData1.Append(row1);
            sheetData1.Append(row2);
            sheetData1.Append(row3);
            sheetData1.Append(row4);

            MergeCells mergeCells1 = new MergeCells();
            MergeCell mergeCell1 = new MergeCell() { Reference = "A1:D1" };
            MergeCell mergeCell2 = new MergeCell() { Reference = "A4:D4" };

            mergeCells1.Append(mergeCell1);
            mergeCells1.Append(mergeCell2);
            PageMargins pageMargins1 = new PageMargins() { Left = 0.7D, Right = 0.7D, Top = 0.75D, Bottom = 0.75D, Header = 0.3D, Footer = 0.3D };

            worksheet1.Append(sheetDimension1);
            worksheet1.Append(sheetViews1);
            worksheet1.Append(sheetFormatProperties1);
            worksheet1.Append(columns1);
            worksheet1.Append(sheetData1);
            worksheet1.Append(mergeCells1);
            worksheet1.Append(pageMargins1);

            part.Worksheet = worksheet1;
        }
    }
}
