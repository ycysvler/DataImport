using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Text.RegularExpressions;
using System.IO;
using System.Windows.Forms;

namespace HyUtilities
{
    public class HyStringUtility
    {
        /// <summary>
        /// 判断字符串是否为null，或者去除首尾空格后是否是string.Empty
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsStringEmpty(string s)
        {
            return s == null || s.Trim().Length == 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="animString"></param>
        /// <returns></returns>
        public static string[] ParseAnimString(string animString)
        {
            if (IsStringEmpty(animString))
                return null;

            List<string> strList = new List<string>();
            int pos = animString.IndexOf('|');
            if (pos < 0)
            {
                //如果文件不包含扩展名，则自动添加".bmp"
                if (HyStringUtility.IsStringEmpty(Path.GetExtension(animString)))
                {
                    animString += ".bmp";
                }
                strList.Add(animString);
            }
            else
            {
                string prefix = animString.Substring(0, pos);
                string fileex = Path.GetExtension(prefix);     //文件后缀
                prefix = Path.GetFileNameWithoutExtension(prefix);
                //如果文件不包含扩展名，则自动添加".bmp"
                if (HyStringUtility.IsStringEmpty(fileex))
                {
                    fileex = ".bmp";
                }

                string numbers = animString.Substring(pos + 1);

                string[] imgNumbers = numbers.Split('-');

                strList.Add(string.Format(@"{0}{1}{2}", prefix, imgNumbers[0], fileex));

                if (imgNumbers.Length > 1)
                    for (int j = 0; j < imgNumbers[1].Length - 1; j++)
                    {
                        strList.Add(string.Format(@"{0}{1}{2}", prefix, imgNumbers[1][j], fileex));
                    }
            }

            return strList.ToArray();
        }

        /// <summary>
        /// 如果要显示的文字超过了规定的宽度，则截断显示如"xxx..."
        /// 否则显示所有的文字
        /// </summary>
        /// <param name="g"></param>
        /// <param name="f"></param>
        /// <param name="b"></param>
        /// <param name="str"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="TrimFlag">
        /// 是否截断显示,在编辑状态的时候不用截断</param>
        /// <param name="BorderFlag">是否画边框</param>
        /// <returns>
        /// true：有截断
        /// false：无截断
        /// </returns>
        public static bool DrawCenterText(Graphics g, Font f, Brush b, string str,
            int x, int y, int width, int height, bool TrimFlag, bool BorderFlag)
        {
            if (BorderFlag)
            {
                Pen p = Pens.Black;
                g.DrawRectangle(p, x, y, width, height);

                g.FillRectangle(new SolidBrush(Color.FromArgb(128, 255, 255, 255)), new Rectangle(x, y, width, height));

            }

            SizeF strSize = g.MeasureString(str, f);
            if (!TrimFlag)
            {
                g.DrawString(str, f, b,
                    new PointF(
                    x + (width - strSize.Width) / 2,
                    y + (height - strSize.Height) / 2));
                return true;
            }

            string trimstr = str;
            bool trimflag = false;
            if (strSize.Width > width)
            {
                trimflag = true;
                while (strSize.Width > width)
                {
                    trimstr = trimstr.Substring(0, trimstr.Length - 1);
                    if (trimstr.Length == 0) break;
                    strSize = g.MeasureString(string.Format("{0}{1}", trimstr, "..."), f);
                }
            }
            if (trimflag)
            {
                trimstr = string.Format("{0}{1}", trimstr, "...");
            }
            g.DrawString(trimstr, f, b,
                new PointF(
                x + (width - strSize.Width) / 2,
                y + (height - strSize.Height) / 2));
            return trimflag;
        }

        public static bool DrawRightAlignText(Graphics g, Font f, Brush b, string str,
            int x, int y, int width, int height)
        {
            SizeF strSize = g.MeasureString(str, f);
            g.DrawString(str, f, b,
                new PointF(
                x + (width - strSize.Width),
                y + (height - strSize.Height) / 2));
            return true;
        }


        /// <summary>
        /// 字符串转换为数字:支持16进制的"0xNN"格式和普通的数字格式
        /// </summary>
        /// <param name="strInput"></param>
        /// <returns></returns>
        public static int ConvertStringToInt(string strInput)
        {
            try
            {
                int fromBase = 10;
                strInput = strInput.Trim().ToLower();
                if (strInput.StartsWith("0x"))
                {
                    fromBase = 16;
                }
                return Convert.ToInt32(strInput, fromBase);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 16进制颜色值（"#FF0000"或者"0xFF0000"）转换为颜色
        /// 如果转换错误则返回黑色
        /// </summary>
        /// <param name="strColor"></param>
        /// <returns></returns>
        public static Color ConvertStringToColor(string strColor)
        {
            try
            {
                strColor = strColor.Trim().ToLower();
                if ((strColor.StartsWith("#") && strColor.Length == 7) ||
                    (strColor.StartsWith("0x") && strColor.Length == 8))
                {
                    int rgb = Convert.ToInt32("0x" + strColor.Substring(strColor.Length - 6, 6), 16);
                    return Color.FromArgb((0xFF << 24) + rgb);
                }
                return Color.Black;
            }
            catch
            {
                return Color.Black;
            }
        }

        /// <summary>
        /// 用于检查对象或类型名称、非方法属性的名称的正则表达式
        /// </summary>
        private static Regex reg_check_obj_name = new Regex(@"^[a-zA-Z_]\w*$");

        /// <summary>
        /// 检查对象或类型名称的合法性
        /// 检查非方法属性的名称的合法性
        /// </summary>
        /// <param name="name">对象或类型名称，非方法属性的名称</param>
        /// <returns></returns>
        public static bool CheckObjectName(string name)
        {
            bool bl = reg_check_obj_name.IsMatch(name);
            if (!bl)
            {
                MessageBox.Show("名称不合法：名称必须由字母、数字和下划线组成。");
            }
            return bl;
        }

        /// <summary>
        /// 检查作为方法属性名称的字符串是否合法
        /// </summary>
        /// <param name="methodName"></param>
        /// <returns>
        /// true:合法
        /// false:含有非法字符
        /// </returns>
        public static bool CheckMethodName(string methodName)
        {
            for (int i = 0; i < methodName.Length; i++)
            {
                char c = methodName[i];
                if (c == '_' || c == ',' || c == '(' || c == ')' || c == ' ' ||
                    (c >= 48 && c <= 57) ||
                    (c >= 65 && c <= 90) || (c >= 97 && c <= 122))
                { }
                else
                {
                    MessageBox.Show("名称不合法：方法名称必须由字母、数字、下划线、空格和字符( , )组成。");
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 检查字符串是否全部是数字和字母
        /// </summary>
        /// <param name="inputstring"></param>
        /// <returns>
        /// true:合法
        /// false:含有非法字符
        /// </returns>
        public static bool CheckIsNumberOrAlphabet(string inputstring)
        {
            for (int i = 0; i < inputstring.Length; i++)
            {
                char c = inputstring[i];
                if ((c >= 48 && c <= 57) || (c >= 65 && c <= 90) || (c >= 97 && c <= 122))
                { }
                else
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 检查字符串是否全部是数字0~9
        /// </summary>
        /// <param name="inputstring"></param>
        /// <returns>
        /// true:合法
        /// false:含有非法字符
        /// </returns>
        public static bool CheckIsNumber(string inputstring)
        {
            for (int i = 0; i < inputstring.Length; i++)
            {
                char c = inputstring[i];
                if (c >= 48 && c <= 57)
                { }
                else
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 检查作为对象属性值的字符串是否合法：
        /// 不能包含"#"
        /// </summary>
        /// <param name="value"></param>
        /// <returns>
        /// true:合法
        /// false:不合法
        /// </returns>
        public static bool CheckObjectStringValue(string value)
        {
            return !value.Contains("#");
        }

        /// <summary>
        /// 检查工艺代码是否合法，工艺代码应该为3个大写的英文字母，例如白蛋白：BDB
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static bool CheckProjectCode(string code)
        {
            if (code.Length != 3) return false;
            for (int i = 0; i < code.Length; i++)
            {
                char c = code[i];
                if (c < 65 || c > 90) return false;
            }
            return true;
        }

        /// <summary>
        /// 将文件名中的非法字符替换为下划线
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string GetLegalFilename(string filename)
        {
            string ret = filename.Replace('\\', '_');
            ret = ret.Replace('/', '_');
            ret = ret.Replace(':', '_');
            ret = ret.Replace('*', '_');
            ret = ret.Replace('?', '_');
            ret = ret.Replace('"', '_');
            ret = ret.Replace('<', '_');
            ret = ret.Replace('>', '_');
            ret = ret.Replace('|', '_');
            return ret;
        }

        public static double GetFormattedDoubleValue(double dval, int PointNum)
        {
            if (PointNum <= 0) return Convert.ToDouble(dval.ToString("0"));

            string formatstring = "0.";
            for (int i = 0; i < PointNum; i++)
            {
                formatstring += "0";
            }
            return Convert.ToDouble(dval.ToString(formatstring));
        }

        public static string GetIntegerFormattedString(int IntegerNum)
        {
            string formatstring = string.Empty;
            if (IntegerNum > 0)
            {
                formatstring = "0";
                for (int i = 1; i < IntegerNum; i++)
                {
                    formatstring += "0";
                }
            }
            return formatstring;
        }

        public static void GetStringPart(string inputString, ref string x1, ref string x2, ref string x3)
        {
            int step = 0;
            for (int i = 0; i < inputString.Length; i++)
            {
                char c = inputString[i];
                if (c >= 48 && c <= 57) 
                {
                    //数字
                    if (step == 0) x1 += c;
                    else if (step == 1)
                    {
                        step = 2;
                        x3 += c;
                    }
                    else x3 += c;
                }
                else if ((c >= 65 && c <= 90) || (c >= 97 && c <= 122)) 
                {
                    //字母
                    if (step == 0)
                    {
                        step = 1;
                        x2 += c;
                    }
                    else if (step == 1) x2 += c;
                    else x3 += c;
                }
                else
                {
                    //其它
                    if (step == 0) x1 += c;
                    else if (step == 1) x3 += c;
                    else x3 += c;
                }
            }
        }
    }
}
