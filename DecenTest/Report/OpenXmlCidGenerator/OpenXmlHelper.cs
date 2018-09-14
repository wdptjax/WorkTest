// OpenXmlHelper.cs
// compile with: /doc:OpenXmlHelper.xml
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Text;
using A = DocumentFormat.OpenXml.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using Ovml = DocumentFormat.OpenXml.Vml.Office;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;
using V = DocumentFormat.OpenXml.Vml;

namespace OpenXmlCidGenerator
{
    /// <summary>
    /// Class to help with Open XML object embedding.
    /// </summary>
    public class OpenXmlHelper
    {
        private string _filePath;
        private WordprocessingDocument _package;
        private Document _document;
        private int shapeId = 1025;
        private int objectId = 1457162041;
        private bool objectsPresent = false;
        private bool showErrors = false;

        public bool ShowErrors
        {
            get { return showErrors; }
            set { showErrors = value; }
        }

        static readonly List<String> validImageExtensions = new List<String>() { ".jpg", ".gif", ".png", ".tif", ".bmp", ".ico" };
        static readonly List<String> officeXmlExtensions = new List<String>() { ".docx", ".xlsx", ".pptx" };
        static readonly List<String> officeBasicExtensions = new List<String>() { ".doc", ".xls", ".vsd" };
        static readonly List<String> validObjectExtensions = new List<String>() { ".xml", ".txt", ".zip", ".pdf", ".wav" };

        /// <summary>
        /// Constructor for using an existing document.
        /// </summary>
        /// <param name="package">The outer portion of the word document.</param>
        /// <param name="mainPart">The main document part of the word document.</param>
        public OpenXmlHelper(WordprocessingDocument package, MainDocumentPart mainPart)
        {
            _package = package;
            _document = mainPart.Document;
        }

        /// <summary>
        /// Constructor for creating a new document.
        /// </summary>
        /// <param name="filePath">Path to where the new file should be created.</param>
        public OpenXmlHelper(string filePath)
        {
            _filePath = filePath;
            if (File.Exists(filePath))
            {
                _package = WordprocessingDocument.Open(filePath, true);
                //TODO: ADD SEPERATOR TO OPENED DOCUMENT BEFORE APPENDING
                Debug.WriteLine("Opened existing file {0}", filePath);
                _document = _package.MainDocumentPart.Document;
            }
            else
            {
                _package = WordprocessingDocument.Create(_filePath, WordprocessingDocumentType.Document);
                if (File.Exists(_filePath))
                {
                    MainDocumentPart main = _package.AddMainDocumentPart();
                    main.Document = new Document();
                    Document document = main.Document;
                    Body body = document.AppendChild(new Body());
                    _document = _package.MainDocumentPart.Document;
                    _document.Save();
                }
                else
                {
                    Debug.WriteLine("Failed to create the file: {0}", filePath);
                }
            }
        }

        /// <summary>
        /// Ensures document is closed correctly.
        /// </summary>
        public void Close()
        {
            _document.Save();
            _package.Close();
        }

        /// <summary>
        /// Generates the run to embed in the document so that objects can be viewed inline.
        /// </summary>
        /// <param name="imagePartId">ID of the image icon from GetIdOfPart.</param>
        /// <param name="objectPartId">ID of the object association from GetIdOfPart.</param>
        /// <param name="extension">File extension</param>
        /// <returns>A run linking all items.</returns>
        private Run GetObjectRun(string imagePartId, string objectPartId, string extension)
        {
            String shapePrefix = "_x0000_i";
            int shapeId = this.shapeId++;
            int objectId = this.objectId++;
            EmbeddedObject embeddedObject = new EmbeddedObject()
            {
                DxaOriginal = "1531",
                DyaOriginal = "990"
            };
            //EmbeddedObjectInit only needs to run if there are no objects already within the document.
            if (!objectsPresent)
            {
                embeddedObject.Append(EmbeddedObjectInit());
                objectsPresent = true;
            }
            V.Shape shape = new V.Shape()
            {
                Id = shapePrefix + shapeId,
                Style = "width:76.35pt;height:49.45pt",
                Ole = null,
                Type = "#_x0000_t75"
            };
            V.ImageData imageData = new V.ImageData()
            {
                Title = "",
                RelationshipId = imagePartId
            };
            shape.Append(imageData);
            embeddedObject.Append(shape);

            String progId = GetProgId(extension);
            Ovml.OleObject oleObject = new Ovml.OleObject()
            {
                Type = Ovml.OleValues.Embed,
                ProgId = progId,
                ShapeId = shapePrefix + shapeId,
                DrawAspect = Ovml.OleDrawAspectValues.Icon,
                ObjectId = "_" + objectId,
                Id = objectPartId
            };
            embeddedObject.Append(oleObject);

            Run r = new Run();
            r.Append(embeddedObject);
            return r;
        }

        /// <summary>
        /// Creates a ShapeType definition to be used by the document.
        /// </summary>
        /// <returns>A ShapeType Definition.</returns>
        private V.Shapetype EmbeddedObjectInit()
        {
            V.Shapetype shapetype = new V.Shapetype() { Id = "_x0000_t75", CoordinateSize = "21600,21600", Filled = false, Stroked = false, OptionalNumber = 75, PreferRelative = true, EdgePath = "m@4@5l@4@11@9@11@9@5xe" };
            V.Stroke stroke = new V.Stroke() { JoinStyle = V.StrokeJoinStyleValues.Miter };
            V.Path path = new V.Path() { AllowGradientShape = true, ConnectionPointType = Ovml.ConnectValues.Rectangle, AllowExtrusion = false };
            Ovml.Lock _lock = new Ovml.Lock() { Extension = V.ExtensionHandlingBehaviorValues.Edit, AspectRatio = true };
            shapetype.Append(stroke);
            shapetype.Append(GenerateFormulas());
            shapetype.Append(path);
            shapetype.Append(_lock);
            return shapetype;
        }

        /// <summary>
        /// Creates Formulas necessary for embedding objects within a document.
        /// </summary>
        /// <returns>A definition of Formulas to use.</returns>
        private V.Formulas GenerateFormulas()
        {
            V.Formulas formulas = new V.Formulas();
            V.Formula formula1 = new V.Formula() { Equation = "if lineDrawn pixelLineWidth 0" };
            V.Formula formula2 = new V.Formula() { Equation = "sum @0 1 0" };
            V.Formula formula3 = new V.Formula() { Equation = "sum 0 0 @1" };
            V.Formula formula4 = new V.Formula() { Equation = "prod @2 1 2" };
            V.Formula formula5 = new V.Formula() { Equation = "prod @3 21600 pixelWidth" };
            V.Formula formula6 = new V.Formula() { Equation = "prod @3 21600 pixelHeight" };
            V.Formula formula7 = new V.Formula() { Equation = "sum @0 0 1" };
            V.Formula formula8 = new V.Formula() { Equation = "prod @6 1 2" };
            V.Formula formula9 = new V.Formula() { Equation = "prod @7 21600 pixelWidth" };
            V.Formula formula10 = new V.Formula() { Equation = "sum @8 21600 0" };
            V.Formula formula11 = new V.Formula() { Equation = "prod @7 21600 pixelHeight" };
            V.Formula formula12 = new V.Formula() { Equation = "sum @10 21600 0" };
            formulas.Append(formula1);
            formulas.Append(formula2);
            formulas.Append(formula3);
            formulas.Append(formula4);
            formulas.Append(formula5);
            formulas.Append(formula6);
            formulas.Append(formula7);
            formulas.Append(formula8);
            formulas.Append(formula9);
            formulas.Append(formula10);
            formulas.Append(formula11);
            formulas.Append(formula12);
            return formulas;
        }

        /// <summary>
        /// Generates an image icon to display inline for file representation.
        /// </summary>
        /// <param name="imagePart">The area to embed the icon.</param>
        /// <param name="fileName">Text to display below the image.</param>
        private void GenerateImage(ImagePart imagePart, String fileName)
        {
            int width = 1200;
            int height = 800;
            Bitmap bitmap = new Bitmap(width, height);
            bitmap.SetResolution(600.0f, 600.0f);
            using (MemoryStream ms = new MemoryStream())
            {
                System.Drawing.Imaging.ImageFormat format = System.Drawing.Imaging.ImageFormat.Png;
                bitmap.Save(ms, format);
                ms.Position = 0;
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    using (System.Drawing.Font font = new System.Drawing.Font("Arial", 0.3f, FontStyle.Regular, GraphicsUnit.Inch))
                    {
                        graphics.SmoothingMode = SmoothingMode.HighQuality;
                        graphics.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;

                        int centerx = (width / 4);
                        graphics.DrawIcon(GetIcon(Path.GetExtension(fileName)), new Rectangle(new Point(centerx, 0), new Size((width * 2) / 4, (height * 3) / 4)));

                        StringFormat sf = new StringFormat();
                        sf.Alignment = StringAlignment.Center;
                        graphics.DrawString(fileName, font, new SolidBrush(System.Drawing.Color.Black), new Point(bitmap.Width / 2, bitmap.Height / 2), sf);
                        bitmap.Save(ms, format);
                        ms.Position = 0;
                        imagePart.FeedData(ms);
                    }

                }
            }
        }

        /// <summary>
        /// Determines correct icon to use with extension.
        /// </summary>
        /// <param name="extension">Extension of file</param>
        /// <returns>Icon to use for resource</returns>
        private Icon GetIcon(String extension)
        {
            Icon icon = Properties.Resources.GeneratedFileIcon;
            switch (extension)
            {
                case ".docx":
                    icon = Properties.Resources.docx;
                    break;
                case ".xlsx":
                    icon = Properties.Resources.xlsx;
                    break;
                case ".pptx":
                    icon = Properties.Resources.pptx;
                    break;
                case ".doc":
                    icon = Properties.Resources.doc;
                    break;
                case ".xls":
                    icon = Properties.Resources.xls;
                    break;
                case ".vsd":
                    icon = Properties.Resources.vsd;
                    break;
                case ".xml":
                    icon = Properties.Resources.xml;
                    break;
                case ".txt":
                    icon = Properties.Resources.txt;
                    break;
                case ".zip":
                    icon = Properties.Resources.zip;
                    break;
                case ".pdf":
                    icon = Properties.Resources.pdf;
                    break;
                default:
                    icon = Properties.Resources.GeneratedFileIcon;
                    break;
            }
            return icon;
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
            if (showErrors)
            {
                Debug.WriteLine(errorMessages.ToString());
            }
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

        /// <summary>
        /// Embeds an image within the document.
        /// </summary>
        /// <param name="filePath">File path to the image to embed.</param>
        public void AddImage(String filePath)
        {
            Paragraph para = new Paragraph();
            Run run = null;
            if (File.Exists(filePath))
            {
                String extension = Path.GetExtension(filePath);
                if (validImageExtensions.Contains(extension))
                {
                    ImagePartType imagePartType = GetImagePartType(extension);
                    ImagePart imagePart = _package.MainDocumentPart.AddImagePart(imagePartType);
                    using (FileStream stream = new FileStream(filePath, FileMode.Open))
                    {
                        imagePart.FeedData(stream);
                    }
                    using (FileStream stream = new FileStream(filePath, FileMode.Open))
                    {
                        Bitmap image = new Bitmap(stream);
                        run = GetImageRun(_package.MainDocumentPart.GetIdOfPart(imagePart), image);
                        para.AppendChild(run);
                        _document.Append(para);
                    }
                }
                else
                {
                    Debug.WriteLine("Invalid file format: {0}", filePath);
                }
            }
            else
            {
                Debug.WriteLine("File not found: {0}", filePath);
            }
        }

        /// <summary>
        /// Embeds a paragraph to the document.
        /// </summary>
        /// <param name="paragraph">Paragraph to embed.</param>
        public void AddParagraph(Paragraph paragraph)
        {
            _document.Append(paragraph);
        }

        /// <summary>
        /// Embeds an object with an extension included in the validImageExtensions, officeXmlExtensions, officeBasicExtensions, or validObjectExtensions Lists.
        /// </summary>
        /// <param name="realFileName">Full path to the file.</param>
        /// <param name="displayFileName">Name displayed under the file icon in the document.</param>
        public void AddObject(String realFileName, String displayFileName)
        {
            String filePath = realFileName;
            Paragraph para = new Paragraph();
            Run run = null;
            if (File.Exists(filePath))
            {
                MainDocumentPart mainPart = _package.MainDocumentPart;

                ImagePart imagePart = mainPart.AddImagePart(ImagePartType.Png);
                GenerateImage(imagePart, displayFileName);

                String extension = Path.GetExtension(filePath);
                String contentType = GetContentType(extension);
                if (officeXmlExtensions.Contains(extension))
                {
                    EmbeddedPackagePart embeddedPackagePart = _package.MainDocumentPart.AddEmbeddedPackagePart(contentType);
                    using (FileStream stream = new FileStream(filePath, FileMode.Open))
                    {
                        embeddedPackagePart.FeedData(stream);
                    }
                    run = GetObjectRun(mainPart.GetIdOfPart(imagePart), mainPart.GetIdOfPart(embeddedPackagePart), extension);
                }
                else if (officeBasicExtensions.Contains(extension))
                {
                    EmbeddedObjectPart embeddedObjectPart = _package.MainDocumentPart.AddEmbeddedObjectPart(contentType);
                    using (FileStream stream = new FileStream(filePath, FileMode.Open))
                    {
                        embeddedObjectPart.FeedData(stream);
                    }
                    run = GetObjectRun(mainPart.GetIdOfPart(imagePart), mainPart.GetIdOfPart(embeddedObjectPart), extension);
                }
                else if (validObjectExtensions.Contains(extension))
                {
                    StringBuilder errorMessages;
                    EmbeddedObjectPart embeddedObjectPart = _package.MainDocumentPart.AddEmbeddedObjectPart(contentType);
                    String outputBinaryName = GetBinaryName(filePath);
                    bool success = GenerateOleObject(filePath, outputBinaryName, out errorMessages);
                    if (success)
                    {
                        using (FileStream stream = new FileStream(outputBinaryName, FileMode.Open))
                        {
                            embeddedObjectPart.FeedData(stream);
                        }
                        try
                        {
                            File.Delete(outputBinaryName);
                            run = GetObjectRun(mainPart.GetIdOfPart(imagePart), mainPart.GetIdOfPart(embeddedObjectPart), extension);

                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("Failed to remove file: {0}", outputBinaryName);
                        }
                    }
                    else
                    {
                        Debug.WriteLine("Failed to generate OLE Object: {0}", filePath);
                    }
                }
                else
                {
                    Debug.WriteLine("Invalid file format: {0}", filePath);
                }
                para.AppendChild(run);
                _document.Append(para);
            }
            else
            {
                Debug.WriteLine("File not found: {0}", filePath);
            }
        }

        /// <summary>
        /// Determines the correct application to use for opening embedded objects.
        /// </summary>
        /// <param name="extension">Extension of the object</param>
        /// <returns>Content-type string.</returns>
        private String GetContentType(String extension)
        {
            String contentType = null;
            switch (extension)
            {
                case ".doc":
                    contentType = "application/msword";
                    break;
                case ".docx":
                    contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                    break;
                case ".xls":
                    contentType = "application/vnd.ms-excel";
                    break;
                case ".xlsx":
                    contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    break;
                case ".vsd":
                    contentType = "application/vnd.visio";
                    break;
                case ".pptx":
                    contentType = "application/vnd.openxmlformats-officedocument.presentationml.presentation";
                    break;
                default:
                    contentType = "application/vnd.openxmlformats-officedocument.oleObject";
                    break;
            }
            return contentType;
        }

        /// <summary>
        /// Determines the program ID for a given extension.
        /// </summary>
        /// <param name="extension">Extension of the Object</param>
        /// <returns>Program ID to use when opening files.</returns>
        private String GetProgId(String extension)
        {
            String progId = null;
            switch (extension)
            {
                case ".doc":
                    progId = "Word.Document.8";
                    break;
                case ".docx":
                    progId = "Word.Document.12";
                    break;
                case ".xls":
                    progId = "Excel.Sheet.8";
                    break;
                case ".xlsx":
                    progId = "Excel.Sheet.12";
                    break;
                case ".vsd":
                    progId = "Visio.Drawing.11";
                    break;
                case ".pdf":
                    progId = "AcroExch.Document.11";
                    break;
                case ".pptx":
                    progId = "PowerPoint.Show.12";
                    break;
                case ".wav":
                    progId = "cloudmusic.wav";
                    break;
                default:
                    progId = "Package";
                    break;
            }
            return progId;
        }

        /// <summary>
        /// Determing the image type based on extension.
        /// </summary>
        /// <param name="extension">Image extension to get type for.</param>
        /// <returns>Image part type.</returns>
        private ImagePartType GetImagePartType(String extension)
        {
            //Bmp is the default type as this is a non nullable property.
            ImagePartType type = ImagePartType.Bmp;
            switch (extension)
            {
                case ".jpg":
                    type = ImagePartType.Jpeg;
                    break;
                case ".png":
                    type = ImagePartType.Png;
                    break;
                case ".gif":
                    type = ImagePartType.Gif;
                    break;
                case ".tif":
                    type = ImagePartType.Tiff;
                    break;
                case ".bmp":
                    type = ImagePartType.Bmp;
                    break;
                case ".ico":
                    type = ImagePartType.Icon;
                    break;
            }
            return type;
        }

        /// <summary>
        /// Creates a run for an image to be inserted.
        /// </summary>
        /// <param name="relationshipId">Link of the part to embed.</param>
        /// <param name="imageFile">Image to embed.</param>
        /// <returns>A run with the image.</returns>
        private Run GetImageRun(String relationshipId, Bitmap imageFile)
        {
            long imageWidth = (long)((imageFile.Width / imageFile.HorizontalResolution) * 914400L);
            long imageHeight = (long)((imageFile.Height / imageFile.VerticalResolution) * 914400L);
            //Code from Microsoft: http://msdn.microsoft.com/en-us/library/office/bb497430(v=office.15).aspx
            var element =
         new Drawing(
             new DW.Inline(
                 new DW.Extent() { Cx = imageWidth, Cy = imageHeight },
                 new DW.EffectExtent()
                 {
                     LeftEdge = 0L,
                     TopEdge = 0L,
                     RightEdge = 0L,
                     BottomEdge = 0L
                 },
                 new DW.DocProperties()
                 {
                     Id = (UInt32Value)1U,
                     Name = "Picture 1"
                 },
                 new DW.NonVisualGraphicFrameDrawingProperties(
                     new A.GraphicFrameLocks() { NoChangeAspect = true }),
                 new A.Graphic(
                     new A.GraphicData(
                         new PIC.Picture(
                             new PIC.NonVisualPictureProperties(
                                 new PIC.NonVisualDrawingProperties()
                                 {
                                     Id = (UInt32Value)0U,
                                     Name = "New Bitmap Image.jpg"
                                 },
                                 new PIC.NonVisualPictureDrawingProperties()),
                             new PIC.BlipFill(
                                 new A.Blip(
                                     new A.BlipExtensionList(
                                         new A.BlipExtension()
                                         {
                                             Uri =
                                               "{28A0092B-C50C-407E-A947-70E740481C1C}"
                                         })
                                 )
                                 {
                                     Embed = relationshipId,
                                     CompressionState =
                                     A.BlipCompressionValues.Print
                                 },
                                 new A.Stretch(
                                     new A.FillRectangle())),
                             new PIC.ShapeProperties(
                                 new A.Transform2D(
                                     new A.Offset() { X = 0L, Y = 0L },
                                     new A.Extents() { Cx = imageWidth, Cy = imageHeight }),
                                 new A.PresetGeometry(
                                     new A.AdjustValueList()
                                 ) { Preset = A.ShapeTypeValues.Rectangle }))
                     ) { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" })
             )
             {
                 DistanceFromTop = (UInt32Value)0U,
                 DistanceFromBottom = (UInt32Value)0U,
                 DistanceFromLeft = (UInt32Value)0U,
                 DistanceFromRight = (UInt32Value)0U,
             });
            Run run = new Run();
            run.AppendChild(element);
            return run;
        }
    }
}

