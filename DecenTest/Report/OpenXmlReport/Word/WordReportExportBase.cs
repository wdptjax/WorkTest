
/*********************************************************************************************
 *	
 * 文件名称:    WordReportExportBase.cs
 *
 * 作    者:    吴德鹏
 *	
 * 创作日期:    2018-9-10 11:48:10
 * 
 * 备    注:    Word报表导出基类
 *              
 *                                
 *               
*********************************************************************************************/

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using V = DocumentFormat.OpenXml.Vml;
using Ovml = DocumentFormat.OpenXml.Vml.Office;
using OpenXmlCidGenerator;
using System.Runtime.InteropServices;

namespace OpenXmlReport.Word
{
    /// <summary>
    /// Word报表导出基类
    /// </summary>
    public class WordReportExportBase : ReportBase
    {

        private WordprocessingDocument _document = null;

        public WordReportExportBase(IntPtr owner) : base(owner)
        {
        }

        protected override void ClearData()
        {
            throw new NotImplementedException();
        }

        protected override void CreateDocument(string fileName)
        {
            _document = WordprocessingDocument.Create(fileName, DocumentFormat.OpenXml.WordprocessingDocumentType.Document);

        }

        protected override double GetMaxValue()
        {
            throw new NotImplementedException();
        }

        public void Test()
        {
            string file = @"C:\Users\Administrator\Desktop\Report\";
            _document = WordprocessingDocument.Open(file + "111.docx", true);
            MainDocumentPart part = _document.MainDocumentPart;
            if (part.Document == null)
            {
                Document document1 = new Document() { MCAttributes = new MarkupCompatibilityAttributes() { Ignorable = "w14 w15 wp14" } };
                document1.AddNamespaceDeclaration("wpc", "http://schemas.microsoft.com/office/word/2010/wordprocessingCanvas");
                document1.AddNamespaceDeclaration("mc", "http://schemas.openxmlformats.org/markup-compatibility/2006");
                document1.AddNamespaceDeclaration("o", "urn:schemas-microsoft-com:office:office");
                document1.AddNamespaceDeclaration("r", "http://schemas.openxmlformats.org/officeDocument/2006/relationships");
                document1.AddNamespaceDeclaration("m", "http://schemas.openxmlformats.org/officeDocument/2006/math");
                document1.AddNamespaceDeclaration("v", "urn:schemas-microsoft-com:vml");
                document1.AddNamespaceDeclaration("wp14", "http://schemas.microsoft.com/office/word/2010/wordprocessingDrawing");
                document1.AddNamespaceDeclaration("wp", "http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing");
                document1.AddNamespaceDeclaration("w10", "urn:schemas-microsoft-com:office:word");
                document1.AddNamespaceDeclaration("w", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");
                document1.AddNamespaceDeclaration("w14", "http://schemas.microsoft.com/office/word/2010/wordml");
                document1.AddNamespaceDeclaration("w15", "http://schemas.microsoft.com/office/word/2012/wordml");
                document1.AddNamespaceDeclaration("wpg", "http://schemas.microsoft.com/office/word/2010/wordprocessingGroup");
                document1.AddNamespaceDeclaration("wpi", "http://schemas.microsoft.com/office/word/2010/wordprocessingInk");
                document1.AddNamespaceDeclaration("wne", "http://schemas.microsoft.com/office/word/2006/wordml");
                document1.AddNamespaceDeclaration("wps", "http://schemas.microsoft.com/office/word/2010/wordprocessingShape");
                part.Document = document1;
            }
            Body objBody = part.Document.Body;

            #region
            Image img = Icon.ExtractAssociatedIcon(file + "Test.wav").ToBitmap();//SystemIcon.GetIcon(file + "Test.wav", true).ToBitmap();//Bitmap.FromFile(file + "test.png");
            string imgDataStr = GetBinaryStrFromImage(img);
            string audioDataStr = GetBinaryStrFromAudio(file + "Test.wav");

            ImagePart imagePart1 = part.AddImagePart(ImagePartType.Emf);
            GeneratePartContent(imagePart1, imgDataStr);
            //FeedImageFile(imagePart1, img);
            EmbeddedObjectPart embeddedObjectPart1 = part.AddEmbeddedObjectPart("application/vnd.openxmlformats-officedocument.oleObject");
            FeedAudioFile(embeddedObjectPart1, file + "Test.wav");

            var pgh = part.Document.Body.ChildElements.Where(i => i is Paragraph);
            Paragraph lastPgh = (Paragraph)part.Document.Body.ChildElements.LastOrDefault(i => i is Paragraph && ((Paragraph)i).InnerText == " 2:118MHz");

            var rpr = (RunProperties)lastPgh.FirstOrDefault(i => i is Run)?.FirstOrDefault(i => i is RunProperties)?.Clone();

            string bId = "r" + System.Guid.NewGuid().ToString().Replace("-", "");

            Paragraph paragraph1 = new Paragraph() { RsidParagraphAddition = bId, RsidRunAdditionDefault = bId };

            Run run1 = new Run() { RsidRunProperties = bId };

            // 首行缩进
            ParagraphProperties pPr = new ParagraphProperties();
            Indentation ind = new Indentation() { FirstLine = "420" };
            pPr.Append(ind);
            paragraph1.Append(pPr);

            rpr.Italic = new Italic() { Val = true };
            Text text = new Text() { Space = SpaceProcessingModeValues.Preserve };
            text.Text = string.Format("双击打开录音文件-Test.wav");
            run1.Append(rpr);
            run1.Append(text);
            EmbeddedObject embeddedObject1 = new EmbeddedObject() { DxaOriginal = "8281", DyaOriginal = "841" };

            V.Shape shape1 = new V.Shape()
            {
                Id = "r" + System.Guid.NewGuid().ToString().Replace("-", ""),
                Style = "width:10pt;height:10pt",
                Ole = true,
                Type = "#_x0000_t75"
            };
            V.ImageData imageData1 = new V.ImageData() { Title = "", RelationshipId = part.GetIdOfPart(imagePart1) };
            shape1.Append(imageData1);
            Ovml.OleObject oleObject1 = new Ovml.OleObject()
            {
                Type = Ovml.OleValues.Embed,
                ProgId = "Package",
                ShapeId = shape1.Id,
                DrawAspect = Ovml.OleDrawAspectValues.Content,
                ObjectId = "r" + System.Guid.NewGuid().ToString().Replace("-", ""),
                Id = part.GetIdOfPart(embeddedObjectPart1),
            };

            embeddedObject1.Append(shape1);
            embeddedObject1.Append(oleObject1);

            run1.Append(embeddedObject1);

            paragraph1.Append(run1);


            part.Document.Body.InsertAfter(paragraph1, lastPgh);

            #endregion

            part.Document.Save();
            _document.Close();
        }


        /// <summary>
        /// 将图片以二进制流
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public string GetBinaryStrFromImage(Image image)
        {
            MemoryStream ms = new MemoryStream();
            image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            image.Dispose();
            ms.Seek(0, SeekOrigin.Begin);
            BinaryReader br = new BinaryReader(ms);
            byte[] imgBytesIn = br.ReadBytes((int)ms.Length); //将流读入到字节数组中
            ms.Close();
            string stImageByte = Convert.ToBase64String(imgBytesIn);
            return stImageByte;
        }

        public string GetBinaryStrFromAudio(string audioFilepath)
        {
            try
            {
                FileStream fs = new FileStream(audioFilepath, FileMode.Open);
                BinaryReader br = new BinaryReader(fs);
                byte[] data = br.ReadBytes((int)fs.Length);
                fs.Close();
                string audioBytes = Convert.ToBase64String(data);
                return audioBytes;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="part"></param>
        /// <param name="imgBinaryDataStr"></param>
        private void GeneratePartContent(OpenXmlPart part, string imgBinaryDataStr)
        {
            System.IO.Stream data = new System.IO.MemoryStream(System.Convert.FromBase64String(imgBinaryDataStr));
            part.FeedData(data);
            data.Close();
        }
        private void FeedAudioFile(OpenXmlPart part, string file)
        {
            string newPath = GetBinaryName(file);
            StringBuilder sb;
            GenerateOleObject(file, newPath, out sb);
            FileStream fs = new FileStream(newPath, FileMode.Open);
            part.FeedData(fs);
            fs.Close();
            File.Delete(newPath);
        }
        private void FeedImageFile(OpenXmlPart part, Image image)
        {
            MemoryStream ms = new MemoryStream();
            image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            image.Dispose();
            part.FeedData(ms);
            ms.Close();
        }
        /// <summary>
        /// Creates a binary file that can be embedded into an Open XML document.
        /// </summary>
        /// <param name="filePath">Path to the file to embed.</param>
        /// <param name="outputBinaryName">Name of the output file.</param>
        /// <param name="errorMessages">Error messages container.</param>
        /// <returns>Output file created successfully</returns>
        private bool GenerateOleObject(string filePath, string outputBinaryName, out StringBuilder errorMessages)
        {
            bool success = false;
            success = OleObjectHelper.ExportOleFile(filePath, outputBinaryName, out errorMessages);
            return success;
        }

        /// <summary>
        /// Generates the name for the output binary file.
        /// </summary>
        /// <param name="filePath">File path to generate name</param>
        /// <returns>Path with extension replaced by .bin</returns>
        private string GetBinaryName(string filePath)
        {
            return Path.GetFileNameWithoutExtension(filePath) + ".bin";
        }
    }
}
