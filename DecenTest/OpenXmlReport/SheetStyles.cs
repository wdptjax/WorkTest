
/*********************************************************************************************
 *	
 * 文件名称:    SheetStyles.cs
 *
 * 作    者:    吴德鹏
 *	
 * 创作日期:    2018-7-24 16:25:59
 * 
 * 备    注:    Excel表格自定义样式
 *              
 *                                
 *               
*********************************************************************************************/

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenXmlReport
{
    /// <summary>
    /// 自定义表格样式
    /// </summary>
    class SheetStyles
    {
        /// <summary>
        /// 数值格式
        /// </summary>
        /// <param name="formatId">格式ID</param>
        /// <param name="formatStr">格式字符串</param>
        /// <returns></returns>
        public static NumberingFormat GetNumberingFormat(uint formatId, string formatStr)
        {
            // ID从176开始
            NumberingFormat format = new NumberingFormat()
            {
                NumberFormatId = formatId,
                FormatCode = formatStr
            };
            return format;
        }
        /// <summary>
        /// 创建字体样式
        /// </summary>
        /// <param name="fontSize">字体大小</param>
        /// <param name="fontName">字体类型</param>
        /// <param name="fontColorString">字体颜色字符串（不带#号）</param>
        /// <param name="underlineStyle">下划线样式</param>
        /// <param name="isBold">是否粗体</param>
        /// <returns></returns>
        public static Font GetFont(UInt32 fontSize, string fontName, string fontColorString = "", UnderlineValues underlineStyle = UnderlineValues.None, bool isBold = false)
        {
            Font font = new Font();
            FontSize size = new FontSize() { Val = fontSize };
            FontName name = new FontName() { Val = fontName };
            font.Append(size);
            font.Append(name);
            if (!string.IsNullOrEmpty(fontColorString))
            {
                Color color = new Color() { Rgb = HexBinaryValue.FromString(fontColorString) };
                font.Append(color);
            }
            if (underlineStyle != UnderlineValues.None)
                font.Append(new Underline() { Val = underlineStyle });
            if (isBold)
                font.Append(new Bold());

            return font;
        }

        /// <summary>
        /// 单元格填充颜色样式
        /// </summary>
        /// <param name="fillType">填充类型</param>
        /// <param name="fillColor">填充颜色</param>
        /// <returns></returns>
        public static Fill GetFill(PatternValues fillType = PatternValues.None, string fillColor = "")
        {
            Fill fill = new Fill();


            PatternFill patternFill = new PatternFill() { PatternType = fillType };
            if (fillType != PatternValues.None && !string.IsNullOrEmpty(fillColor))
            {
                ForegroundColor foregroundColor = new ForegroundColor() { Rgb = HexBinaryValue.FromString(fillColor) };
                BackgroundColor backgroundColor = new BackgroundColor() { Indexed = (UInt32Value)64U };

                patternFill.Append(foregroundColor);
                patternFill.Append(backgroundColor);
            }

            fill.Append(patternFill);

            return fill;
        }

        /// <summary>
        /// 设置单个边框线的样式
        /// </summary>
        /// <param name="borderType">
        /// 边框类型：
        /// 0-上边框
        /// 1-下边框
        /// 2-左边框
        /// 3-右边框
        /// 4-斜边框
        /// </param>
        /// <param name="style"></param>
        /// <param name="borderColor"></param>
        /// <returns></returns>
        public static BorderPropertiesType GetBorderLineStyle(int borderType, BorderStyleValues style = BorderStyleValues.None, string borderColorString = "")
        {
            BorderPropertiesType line = new TopBorder();
            switch (borderType)
            {
                case 0:
                    line = new TopBorder();
                    break;
                case 1:
                    line = new BottomBorder();
                    break;
                case 2:
                    line = new LeftBorder();
                    break;
                case 3:
                    line = new RightBorder();
                    break;
                case 4:
                    line = new DiagonalBorder();
                    break;
                default:
                    return null;
            }
            if (style != BorderStyleValues.None)
            {
                line.Style = style;
                Color Color = new Color() { Rgb = borderColorString };
                line.Append(Color);
            }

            return line;
        }

        /// <summary>
        /// 设置单元格的边框
        /// </summary>
        /// <param name="borderLineStyleList">边框样式集合</param>
        /// <param name="diagonalUp">是否包含斜上边框</param>
        /// <param name="diagonalDown">是否包含斜下边框</param>
        /// <returns></returns>
        public static Border GetBorder(List<BorderPropertiesType> borderLineStyleList, bool diagonalUp = false, bool diagonalDown = false)
        {
            Border border = new Border() { DiagonalUp = diagonalUp, DiagonalDown = diagonalDown };

            borderLineStyleList.ForEach(line => border.Append(line));

            return border;
        }

        /// <summary>
        /// 默认单元格样式？
        /// 需要有这个，但是用途未知
        /// </summary>
        /// <returns></returns>
        public static CellStyleFormats GetCellStyleFormats()
        {
            CellStyleFormats cellStyleFormats = new CellStyleFormats() { Count = (UInt32Value)1U };
            CellFormat cellFormat = new CellFormat() { NumberFormatId = (UInt32Value)0U, FontId = (UInt32Value)0U, FillId = (UInt32Value)0U, BorderId = (UInt32Value)0U, ApplyNumberFormat = false, ApplyFill = false, ApplyAlignment = false, ApplyProtection = false };

            cellStyleFormats.Append(cellFormat);
            return cellStyleFormats;
        }

        /// <summary>
        /// 单元格格式
        /// </summary>
        /// <param name="horizontal">水平对齐</param>
        /// <param name="vertical">垂直对齐</param>
        /// <param name="numId">数值格式</param>
        /// <param name="fontId">字体样式ID</param>
        /// <param name="fillId">填充样式ID</param>
        /// <param name="borderId">边框样式ID</param>
        /// <returns></returns>
        public static CellFormat GetCellFormat(HorizontalAlignmentValues horizontal, VerticalAlignmentValues vertical, UInt32 numId = 0, UInt32 fontId = 0, UInt32 fillId = 0, UInt32 borderId = 0)
        {
            CellFormat cellFormat = new CellFormat()
            {
                NumberFormatId = (UInt32Value)numId,
                FontId = (UInt32Value)fontId,
                FillId = (UInt32Value)fillId,
                BorderId = (UInt32Value)borderId,
                FormatId = 0U,
                ApplyNumberFormat = false,
                ApplyFont = true,
                ApplyFill = true,
                ApplyBorder = true,
                ApplyAlignment = false,
                ApplyProtection = false
            };
            Alignment alignment = new Alignment() { Horizontal = horizontal, Vertical = vertical, WrapText = true };

            cellFormat.Append(alignment);

            return cellFormat;
        }

        /// <summary>
        /// 设置表单格式
        /// </summary>
        /// <param name="fontList"></param>
        /// <param name="fillList"></param>
        /// <param name="borderList"></param>
        /// <param name="cellFormatList"></param>
        /// <returns></returns>
        public static Stylesheet GetStyleSheet(List<NumberingFormat> numFormatList, List<Font> fontList, List<Fill> fillList, List<Border> borderList, List<CellFormat> cellFormatList)
        {
            Stylesheet stylesheet = new Stylesheet();
            if (numFormatList != null && numFormatList.Count > 0)
            {
                NumberingFormats numFormats = new NumberingFormats() { Count = (UInt32)numFormatList.Count };
                numFormatList.ForEach(numf => numFormats.Append(numf));
                stylesheet.Append(numFormats);
            }

            Fonts fonts = new Fonts() { Count = (UInt32)fontList.Count };
            fontList.ForEach(font => fonts.Append(font));

            Fills fills = new Fills() { Count = (UInt32)fillList.Count };
            fillList.ForEach(fill => fills.Append(fill));

            Borders borders = new Borders() { Count = (UInt32)borderList.Count };
            borderList.ForEach(border => borders.Append(border));

            CellStyleFormats cellStyleFormats = GetCellStyleFormats();

            CellFormats cellFormats = new CellFormats() { Count = (UInt32)cellFormatList.Count };
            cellFormatList.ForEach(format => cellFormats.Append(format));

            stylesheet.Append(fonts);
            stylesheet.Append(fills);
            stylesheet.Append(borders);
            stylesheet.Append(cellStyleFormats);
            stylesheet.Append(cellFormats);
            return stylesheet;
        }


        /// <summary>
        /// 在现有的表格样式中追加新的样式-未测试
        /// </summary>
        /// <param name="styles"></param>
        /// <param name="fontList"></param>
        /// <param name="fillList"></param>
        /// <param name="borderList"></param>
        /// <param name="cellFormatList"></param>
        public static void AppendStyles(Stylesheet styles, List<Font> fontList, List<Fill> fillList, List<Border> borderList, List<CellFormat> cellFormatList)
        {
            if (styles == null)
                styles = new Stylesheet();

            Fonts fonts = (Fonts)styles.ChildElements.FirstOrDefault(i => i is Fonts);
            if (fonts == null)
            {
                fonts = new Fonts();
                styles.Append(fonts);
            }
            uint fontIdStart = fonts.Count;
            fonts.Count += (uint)fontList.Count;
            fontList.ForEach(font => fonts.Append(font));

            Fills fills = (Fills)styles.ChildElements.FirstOrDefault(i => i is Fills);
            if (fills == null)
            {
                fills = new Fills();
                styles.Append(fills);
            }
            uint fillIdStart = fills.Count;
            fills.Count += (uint)fillList.Count;
            fillList.ForEach(fill => fills.Append(fill));

            Borders borders = (Borders)styles.ChildElements.FirstOrDefault(i => i is Borders);
            if (borders == null)
            {
                borders = new Borders();
                styles.Append(borders);
            }
            uint borderIdStart = borders.Count;
            borders.Count += (uint)borderList.Count;
            borderList.ForEach(border => borders.Append(border));

            CellFormats cellFormats = (CellFormats)styles.ChildElements.FirstOrDefault(i => i is CellFormats);
            if (cellFormats == null)
            {
                cellFormats = new CellFormats();
                styles.Append(cellFormats);
            }
            cellFormats.Count += (uint)cellFormatList.Count;
            cellFormatList.ForEach(cell =>
            {
                cell.FontId += fontIdStart;
                cell.FillId += fillIdStart;
                cell.BorderId += borderIdStart;
                cellFormats.Append(cell);
            });
        }

    }
}
