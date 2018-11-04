using System;
using System.Collections.Generic;
using System.Text;

namespace HyUtilities
{
    /// <summary>
    /// 16½øÖÆ×Ö·û´®±àÂë
    /// </summary>
    public class HexEncoding : System.Text.Encoding
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="chars"></param>
        /// <param name="index"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public override int GetByteCount(char[] chars, int index, int count)
        {
            return System.Convert.ToInt32(Math.Ceiling(System.Convert.ToDouble(count / 2)));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="chars"></param>
        /// <param name="charIndex"></param>
        /// <param name="charCount"></param>
        /// <param name="bytes"></param>
        /// <param name="byteIndex"></param>
        /// <returns></returns>
        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            if (charCount % 2 != 0)
                throw new Exception("charCount must be even!");

            int pb = byteIndex;
            int pc = charIndex;
            int i = 0;
            while (i < charCount)
            {
                bytes[pb] = byte.Parse(chars[pc].ToString() + chars[pc + 1].ToString(), System.Globalization.NumberStyles.AllowHexSpecifier);
                pb++;
                pc += 2;
                i += 2;
            }
            return pb - byteIndex;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="index"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            return count * 2;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="byteIndex"></param>
        /// <param name="byteCount"></param>
        /// <param name="chars"></param>
        /// <param name="charIndex"></param>
        /// <returns></returns>
        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            int pb = byteIndex;
            int pc = charIndex;
            int i = 0;
            while (i < byteCount)
            {
                string s = bytes[pb].ToString("X").PadLeft(2, '0');
                chars[pc] = s[0];
                pc++;
                chars[pc] = s[1];
                pc++;
                pb++;
                i++;
            }
            return pc - charIndex;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="charCount"></param>
        /// <returns></returns>
        public override int GetMaxByteCount(int charCount)
        {
            return System.Convert.ToInt32(Math.Ceiling(System.Convert.ToDouble(charCount / 2)));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="byteCount"></param>
        /// <returns></returns>
        public override int GetMaxCharCount(int byteCount)
        {
            return byteCount * 2;
        }

        private HexEncoding()
        { }

        #region ¾²Ì¬ÊµÀý
        private static HexEncoding m_Instance;
        /// <summary>
        /// ¾²Ì¬ÊµÀý
        /// </summary>
        public static HexEncoding Instance
        {
            get
            {
                if (m_Instance == null)
                    m_Instance = new HexEncoding();
                return m_Instance;
            }
        }
        #endregion
    }

    /// <summary>
    /// ·ÂVB6µÄChrW×Ö·û´®±àÂë
    /// </summary>
    public class ChrWEncoding
    {
        public static byte[] GetBytes(string s)
        {
            byte[] b = new byte[s.Length];

            for (int i = 0; i < s.Length; i++)
            {
                b[i] = (byte)s[i];
            }
            return b;
        }

        public static string GetString(byte[] b)
        {
            string s = string.Empty;

            for (int i = 0; i < b.Length; i++)
            {
                s += (char)b[i];
            }

            return s;
        }
    }

}