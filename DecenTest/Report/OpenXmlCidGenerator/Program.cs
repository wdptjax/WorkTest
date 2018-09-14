using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System;

namespace OpenXmlCidGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Username: ");
            String user = Console.ReadLine();

            WordprocessingDocument package = WordprocessingDocument.Create(@"C:\Users\" + user + @"\Desktop\test.docx", WordprocessingDocumentType.Document);
            MainDocumentPart main = package.AddMainDocumentPart();
            main.Document = new Document();
            Document document1 = main.Document;
            Body body = document1.AppendChild(new Body());
            document1 = package.MainDocumentPart.Document;
            document1.Save();


            OpenXmlHelper document = new OpenXmlHelper(package, main);
            //You will have to have all the files below if you want to successfully test this.
            //Image Formats
            Paragraph para = new Paragraph();
            Run run = new Run();
            run.AppendChild(new Text("These are Image Formats"));
            para.AppendChild(run);
            document.AddParagraph(para);
            document.AddImage(@"C:\Users\" + user + @"\Desktop\test\test.jpg");
            document.AddImage(@"C:\Users\" + user + @"\Desktop\test\test.gif");
            document.AddImage(@"C:\Users\" + user + @"\Desktop\test\test.png");
            document.AddImage(@"C:\Users\" + user + @"\Desktop\test\test.tif");
            document.AddImage(@"C:\Users\" + user + @"\Desktop\test\test.bmp");
            document.AddImage(@"C:\Users\" + user + @"\Desktop\test\test.ico");
            //Office XML Formats
            para = new Paragraph();
            run = new Run();
            run.AppendChild(new Text("These are Office XML Formats"));
            para.AppendChild(run);
            document.AddParagraph(para);
            document.AddObject(@"C:\Users\" + user + @"\Desktop\test\test.docx", "test.docx");
            document.AddObject(@"C:\Users\" + user + @"\Desktop\test\test.xlsx", "test.xlsx");
            document.AddObject(@"C:\Users\" + user + @"\Desktop\test\test.pptx", "test.pptx");
            //Office Basic Formats
            para = new Paragraph();
            run = new Run();
            run.AppendChild(new Text("These are Office Basic Formats"));
            para.AppendChild(run);
            document.AddParagraph(para);
            document.AddObject(@"C:\Users\" + user + @"\Desktop\test\test.doc", "test.doc");
            document.AddObject(@"C:\Users\" + user + @"\Desktop\test\test.xls", "test.xls");
            document.AddObject(@"C:\Users\" + user + @"\Desktop\test\test.vsd", "test.vsd");
            //Object Formats
            para = new Paragraph();
            run = new Run();
            run.AppendChild(new Text("These are Object Formats"));
            para.AppendChild(run);
            document.AddParagraph(para);
            document.AddObject(@"C:\Users\" + user + @"\Desktop\test\test.xml", "test.xml");
            document.AddObject(@"C:\Users\" + user + @"\Desktop\test\test.txt", "test.txt");
            document.AddObject(@"C:\Users\" + user + @"\Desktop\test\test.pdf", "test.pdf");
            document.AddObject(@"C:\Users\" + user + @"\Desktop\test\test.zip", "test.zip");


            document.Close();
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
    }
}
