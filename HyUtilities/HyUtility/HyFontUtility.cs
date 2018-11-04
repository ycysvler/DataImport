using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace HyUtilities
{
    public class HyFontUtility
    {
        private static Font DefaulFont = new Font("宋体", 11, GraphicsUnit.Pixel);

        public static Font ParseFontString(string fontString)
        {
            if (HyStringUtility.IsStringEmpty(fontString))
                return DefaulFont.Clone() as Font;

            try
            {
                string[] sections = fontString.Split('#');
                Font font = new Font(
                        new FontFamily(sections[0]),
                        float.Parse(sections[1]),
                        (FontStyle)(Enum.Parse(typeof(FontStyle), sections[3].Replace(':', ','), true)),
                        (GraphicsUnit)(Enum.Parse(typeof(GraphicsUnit), sections[2], true))
                    );
                return font;
            }
            catch
            {
                return DefaulFont.Clone() as Font;
            }
        }

        public static string GetFontString(Font font)
        {
            return string.Format("{0}#{1}#{2}#{3}",
                font.FontFamily.Name,
                font.Size,
                font.Unit,
                font.Style.ToString().Replace(',', ':'));
        }
    }
}
