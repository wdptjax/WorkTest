using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ReportExport
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            ReportExport.OpenXmlReport.ExportReport report = new OpenXmlReport.ExportReport();
            report.Export();
            //WriteRandomValuesSAX(@"E:\Project\Tracker800_V9\trunk\Client\Bin\Data\Report\Test.xlsx", 100, 4);
         
        }

        void WriteRandomValuesSAX(string filename, int numRows, int numCols)
        {
            using (SpreadsheetDocument myDoc = SpreadsheetDocument.Open(filename, true))
            {
                WorkbookPart workbookPart = myDoc.WorkbookPart;
                WorksheetPart worksheetPart = workbookPart.WorksheetParts.Last();

                OpenXmlWriter writer = OpenXmlWriter.Create(worksheetPart);

                Row r = new Row();
                Cell c = new Cell();
                CellValue v = new CellValue("Test");
                c.AppendChild(v);

                writer.WriteStartElement(new Worksheet());
                writer.WriteStartElement(new SheetData());
                for (int row = 4; row < numRows; row++)
                {
                    List<OpenXmlAttribute> attributes = new List<OpenXmlAttribute>();
                    // add the row index attribute to the list
                    attributes.Add(new OpenXmlAttribute("r", null, row.ToString()));
                    writer.WriteStartElement(r, attributes);
                    for (int col = 0; col < numCols; col++)
                    {
                        writer.WriteElement(c);
                    }
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.WriteEndElement();

                writer.Close();
            }
        }
    }
}
