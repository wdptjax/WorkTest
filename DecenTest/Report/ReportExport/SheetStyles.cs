
/*********************************************************************************************
 *	
 * 文件名称:    SheetStyles.cs
 *
 * 作    者:    吴德鹏
 *	
 * 创作日期:    2018-7-24 15:50:47
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
using System.Linq;
using System.Text;

namespace ReportExport
{
    class SheetStyles
    {
        // Generates content of part.
        public static void GetSheetStyles(WorkbookStylesPart part)
        {
            Stylesheet stylesheet1 = new Stylesheet();

            Fonts fonts1 = new Fonts() { Count = (UInt32Value)6U };

            Font font1 = new Font();
            FontSize fontSize1 = new FontSize() { Val = 11D };
            Color color1 = new Color() { Rgb = HexBinaryValue.FromString("12009F") };
            FontName fontName1 = new FontName() { Val = "Calibri" };
            FontFamilyNumbering fontFamilyNumbering1 = new FontFamilyNumbering() { Val = 2 };
            FontScheme fontScheme1 = new FontScheme() { Val = FontSchemeValues.Minor };

            font1.Append(fontSize1);
            font1.Append(color1);
            font1.Append(fontName1);
            font1.Append(fontFamilyNumbering1);
            font1.Append(fontScheme1);

            Font font2 = new Font();
            Bold bold1 = new Bold();
            Underline underline1 = new Underline() { Val = UnderlineValues.Single };
            FontSize fontSize2 = new FontSize() { Val = 16D };
            Color color2 = new Color() { Rgb = HexBinaryValue.FromString("0000FF") };
            FontName fontName2 = new FontName() { Val = "宋体" };

            font2.Append(bold1);
            font2.Append(underline1);
            font2.Append(fontSize2);
            font2.Append(color2);
            font2.Append(fontName2);

            Font font3 = new Font();
            FontSize fontSize3 = new FontSize() { Val = 9D };
            Color color3 = new Color() { Indexed = (UInt32Value)8U };
            FontName fontName3 = new FontName() { Val = "宋体" };

            font3.Append(fontSize3);
            font3.Append(color3);
            font3.Append(fontName3);

            Font font4 = new Font();
            FontSize fontSize4 = new FontSize() { Val = 12D };
            Color color4 = new Color() { Indexed = (UInt32Value)8U };
            FontName fontName4 = new FontName() { Val = "宋体" };

            font4.Append(fontSize4);
            font4.Append(color4);
            font4.Append(fontName4);

            Font font5 = new Font();
            FontSize fontSize5 = new FontSize() { Val = 11D };
            Color color5 = new Color() { Indexed = (UInt32Value)8U };
            FontName fontName5 = new FontName() { Val = "宋体" };

            font5.Append(fontSize5);
            font5.Append(color5);
            font5.Append(fontName5);

            Font font6 = new Font();
            Bold bold2 = new Bold();
            FontSize fontSize6 = new FontSize() { Val = 9D };
            Color color6 = new Color() { Indexed = (UInt32Value)8U };
            FontName fontName6 = new FontName() { Val = "宋体" };

            font6.Append(bold2);
            font6.Append(fontSize6);
            font6.Append(color6);
            font6.Append(fontName6);

            fonts1.Append(font1);
            fonts1.Append(font2);
            fonts1.Append(font3);
            fonts1.Append(font4);
            fonts1.Append(font5);
            fonts1.Append(font6);

            Fills fills1 = new Fills() { Count = (UInt32Value)6U };

            Fill fill1 = new Fill();
            PatternFill patternFill1 = new PatternFill();

            fill1.Append(patternFill1);

            Fill fill2 = new Fill();
            PatternFill patternFill2 = new PatternFill() { PatternType = PatternValues.DarkGray };

            fill2.Append(patternFill2);

            Fill fill3 = new Fill();

            PatternFill patternFill3 = new PatternFill() { PatternType = PatternValues.Solid };
            ForegroundColor foregroundColor1 = new ForegroundColor() { Indexed = (UInt32Value)9U };

            patternFill3.Append(foregroundColor1);

            fill3.Append(patternFill3);

            Fill fill4 = new Fill();

            PatternFill patternFill4 = new PatternFill() { PatternType = PatternValues.Solid };
            ForegroundColor foregroundColor2 = new ForegroundColor() { Indexed = (UInt32Value)9U };

            patternFill4.Append(foregroundColor2);

            fill4.Append(patternFill4);

            Fill fill5 = new Fill();

            PatternFill patternFill5 = new PatternFill() { PatternType = PatternValues.Solid };
            ForegroundColor foregroundColor3 = new ForegroundColor() { Indexed = (UInt32Value)9U };

            patternFill5.Append(foregroundColor3);

            fill5.Append(patternFill5);

            Fill fill6 = new Fill();

            PatternFill patternFill6 = new PatternFill() { PatternType = PatternValues.Solid };
            ForegroundColor foregroundColor4 = new ForegroundColor() { Indexed = (UInt32Value)9U };

            patternFill6.Append(foregroundColor4);

            fill6.Append(patternFill6);

            fills1.Append(fill1);
            fills1.Append(fill2);
            fills1.Append(fill3);
            fills1.Append(fill4);
            fills1.Append(fill5);
            fills1.Append(fill6);

            Borders borders1 = new Borders() { Count = (UInt32Value)6U };

            Border border1 = new Border();
            LeftBorder leftBorder1 = new LeftBorder();
            RightBorder rightBorder1 = new RightBorder();
            TopBorder topBorder1 = new TopBorder();
            BottomBorder bottomBorder1 = new BottomBorder();
            DiagonalBorder diagonalBorder1 = new DiagonalBorder();

            border1.Append(leftBorder1);
            border1.Append(rightBorder1);
            border1.Append(topBorder1);
            border1.Append(bottomBorder1);
            border1.Append(diagonalBorder1);

            Border border2 = new Border();
            LeftBorder leftBorder2 = new LeftBorder() { Style = BorderStyleValues.Thin };
            RightBorder rightBorder2 = new RightBorder() { Style = BorderStyleValues.Thin };
            TopBorder topBorder2 = new TopBorder() { Style = BorderStyleValues.Thin };
            BottomBorder bottomBorder2 = new BottomBorder() { Style = BorderStyleValues.Thin };

            border2.Append(leftBorder2);
            border2.Append(rightBorder2);
            border2.Append(topBorder2);
            border2.Append(bottomBorder2);

            Border border3 = new Border();
            LeftBorder leftBorder3 = new LeftBorder() { Style = BorderStyleValues.Thin };
            RightBorder rightBorder3 = new RightBorder() { Style = BorderStyleValues.Thin };
            TopBorder topBorder3 = new TopBorder() { Style = BorderStyleValues.Thin };
            BottomBorder bottomBorder3 = new BottomBorder() { Style = BorderStyleValues.Thin };

            border3.Append(leftBorder3);
            border3.Append(rightBorder3);
            border3.Append(topBorder3);
            border3.Append(bottomBorder3);

            Border border4 = new Border();
            LeftBorder leftBorder4 = new LeftBorder() { Style = BorderStyleValues.Thin };
            RightBorder rightBorder4 = new RightBorder() { Style = BorderStyleValues.Thin };
            TopBorder topBorder4 = new TopBorder() { Style = BorderStyleValues.Thin };
            BottomBorder bottomBorder4 = new BottomBorder() { Style = BorderStyleValues.Thin };

            border4.Append(leftBorder4);
            border4.Append(rightBorder4);
            border4.Append(topBorder4);
            border4.Append(bottomBorder4);

            Border border5 = new Border() { DiagonalDown = true };
            LeftBorder leftBorder5 = new LeftBorder() { Style = BorderStyleValues.Thin };
            RightBorder rightBorder5 = new RightBorder() { Style = BorderStyleValues.Thin };
            TopBorder topBorder5 = new TopBorder() { Style = BorderStyleValues.Thin };
            BottomBorder bottomBorder5 = new BottomBorder() { Style = BorderStyleValues.Thin };
            DiagonalBorder diagonalBorder2 = new DiagonalBorder() { Style = BorderStyleValues.Thin };

            border5.Append(leftBorder5);
            border5.Append(rightBorder5);
            border5.Append(topBorder5);
            border5.Append(bottomBorder5);
            border5.Append(diagonalBorder2);

            Border border6 = new Border();
            LeftBorder leftBorder6 = new LeftBorder() { Style = BorderStyleValues.Thin };
            RightBorder rightBorder6 = new RightBorder() { Style = BorderStyleValues.Thin };
            TopBorder topBorder6 = new TopBorder() { Style = BorderStyleValues.Thin };
            BottomBorder bottomBorder6 = new BottomBorder() { Style = BorderStyleValues.Thin };

            border6.Append(leftBorder6);
            border6.Append(rightBorder6);
            border6.Append(topBorder6);
            border6.Append(bottomBorder6);

            borders1.Append(border1);
            borders1.Append(border2);
            borders1.Append(border3);
            borders1.Append(border4);
            borders1.Append(border5);
            borders1.Append(border6);

            CellStyleFormats cellStyleFormats1 = new CellStyleFormats() { Count = (UInt32Value)1U };
            CellFormat cellFormat1 = new CellFormat() { NumberFormatId = (UInt32Value)0U, FontId = (UInt32Value)0U, FillId = (UInt32Value)0U, BorderId = (UInt32Value)0U, ApplyNumberFormat = false, ApplyFill = false, ApplyAlignment = false, ApplyProtection = false };

            cellStyleFormats1.Append(cellFormat1);

            CellFormats cellFormats1 = new CellFormats() { Count = (UInt32Value)6U };
            CellFormat cellFormat2 = new CellFormat() { NumberFormatId = (UInt32Value)0U, FontId = (UInt32Value)0U, FillId = (UInt32Value)0U, BorderId = (UInt32Value)0U, ApplyNumberFormat = false, ApplyFill = false, ApplyAlignment = false, ApplyProtection = false };

            CellFormat cellFormat3 = new CellFormat() { NumberFormatId = (UInt32Value)0U, FontId = (UInt32Value)1U, FillId = (UInt32Value)2U, BorderId = (UInt32Value)1U, FormatId = (UInt32Value)0U, ApplyNumberFormat = false, ApplyFont = true, ApplyFill = true, ApplyBorder = true, ApplyAlignment = false, ApplyProtection = false };
            Alignment alignment1 = new Alignment() { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center, WrapText = true };

            cellFormat3.Append(alignment1);

            CellFormat cellFormat4 = new CellFormat() { NumberFormatId = (UInt32Value)0U, FontId = (UInt32Value)2U, FillId = (UInt32Value)2U, BorderId = (UInt32Value)1U, FormatId = (UInt32Value)0U, ApplyNumberFormat = false, ApplyFont = true, ApplyFill = true, ApplyBorder = true, ApplyAlignment = false, ApplyProtection = false };
            Alignment alignment2 = new Alignment() { Horizontal = HorizontalAlignmentValues.Left, Vertical = VerticalAlignmentValues.Top, WrapText = true };

            cellFormat4.Append(alignment2);

            CellFormat cellFormat5 = new CellFormat() { NumberFormatId = (UInt32Value)0U, FontId = (UInt32Value)3U, FillId = (UInt32Value)2U, BorderId = (UInt32Value)1U, FormatId = (UInt32Value)0U, ApplyNumberFormat = false, ApplyFont = true, ApplyFill = true, ApplyBorder = true, ApplyAlignment = false, ApplyProtection = false };
            Alignment alignment3 = new Alignment() { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center, WrapText = true };

            cellFormat5.Append(alignment3);

            CellFormat cellFormat6 = new CellFormat() { NumberFormatId = (UInt32Value)0U, FontId = (UInt32Value)4U, FillId = (UInt32Value)2U, BorderId = (UInt32Value)4U, FormatId = (UInt32Value)0U, ApplyNumberFormat = false, ApplyFont = true, ApplyFill = true, ApplyBorder = true, ApplyAlignment = false, ApplyProtection = false };
            Alignment alignment4 = new Alignment() { Horizontal = HorizontalAlignmentValues.Left, Vertical = VerticalAlignmentValues.Top, WrapText = true };

            cellFormat6.Append(alignment4);

            CellFormat cellFormat7 = new CellFormat() { NumberFormatId = (UInt32Value)0U, FontId = (UInt32Value)5U, FillId = (UInt32Value)2U, BorderId = (UInt32Value)1U, FormatId = (UInt32Value)0U, ApplyNumberFormat = false, ApplyFont = true, ApplyFill = true, ApplyBorder = true, ApplyAlignment = false, ApplyProtection = false };
            Alignment alignment5 = new Alignment() { Horizontal = HorizontalAlignmentValues.Left, Vertical = VerticalAlignmentValues.Center, WrapText = true };

            cellFormat7.Append(alignment5);

            cellFormats1.Append(cellFormat2);
            cellFormats1.Append(cellFormat3);
            cellFormats1.Append(cellFormat4);
            cellFormats1.Append(cellFormat5);
            cellFormats1.Append(cellFormat6);
            cellFormats1.Append(cellFormat7);

            stylesheet1.Append(fonts1);
            stylesheet1.Append(fills1);
            stylesheet1.Append(borders1);
            stylesheet1.Append(cellStyleFormats1);
            stylesheet1.Append(cellFormats1);

            part.Stylesheet = stylesheet1;
            part.Stylesheet.Save();
        }

    }
}
