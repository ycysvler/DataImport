using System;
using System.Collections.Generic;
using System.Text;

namespace HyUtilities
{
    /// <summary>
    /// 
    /// </summary>
    public class Int32ByteArrayConverter
    {
        /// <summary>
        /// 将32位整型转换为长度为4的byte数组, 低字节在前
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static byte[] LConvertToByte(int i)
        {
            byte[] buf = new byte[4];
            LConvertToByte(i, buf, 0);
            return buf;
        }

        /// <summary>
        /// 将32位整型转换为长度为4的byte数组, 低字节在前
        /// </summary>
        /// <param name="i"></param>
        /// <param name="buf"></param>
        /// <param name="index"></param>
        public static void LConvertToByte(int i, byte[] buf, int index)
        {
            if (buf == null || index + 3 >= buf.Length)
                return;

            buf[index] = (byte)((i << 24) >> 24);
            buf[index+1] = (byte)((i << 16) >> 24);
            buf[index+2] = (byte)((i << 8) >> 24);
            buf[index+3] = (byte)(i >> 24);
        }

        /// <summary>
        /// 将长度为4的byte数组(低字节在前)转换为32位整型
        /// </summary>
        /// <param name="buf"></param>
        /// <returns></returns>
        public static int LConvertFromByte(byte[] buf)
        {
            int r = buf[3] << 24;
            r += buf[2] << 16;
            r += buf[1] << 8;
            r += buf[0];
            return r;
        }

        /// <summary>
        /// 将32位整型转换为长度为4的byte数组, 高字节在前
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static byte[] HConvertToByte(int i)
        {
            byte[] buf = new byte[4];
            HConvertToByte(i, buf, 0);
            return buf;
        }

        /// <summary>
        /// 将32位整型转换为长度为4的byte数组, 高字节在前
        /// </summary>
        /// <param name="i"></param>
        /// <param name="buf"></param>
        /// <param name="index"></param>
        public static void HConvertToByte(int i, byte[] buf, int index)
        {
            if (buf == null || index + 3 >= buf.Length)
                return;

            buf[index + 3] = (byte)((i << 24) >> 24);
            buf[index + 2] = (byte)((i << 16) >> 24);
            buf[index + 1] = (byte)((i << 8) >> 24);
            buf[index] = (byte)(i >> 24);
        }

        /// <summary>
        /// 将长度为4的byte数组(高字节在前)转换为32位整型
        /// </summary>
        /// <param name="buf"></param>
        /// <returns></returns>
        public static int HConvertFromByte(byte[] buf)
        {
            int r = buf[0] << 24;
            r += buf[1] << 16;
            r += buf[2] << 8;
            r += buf[3];
            return r;
        }
    }
}
