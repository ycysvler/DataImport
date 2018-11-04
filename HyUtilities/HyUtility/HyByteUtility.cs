using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HyUtilities
{
    public class HyByteUtility
    {
        /// <summary>
        /// 判断两个字节数组是否相等
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool IsByteArrayEquals(byte[] a, byte[] b)
        {
            if (a == null || b == null) return false;
            if (a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 检查一个字节的某个bit(bit0～bit7:从低位到高位排列)是否是1
        /// 若是1，则返回1；否则返回0
        /// </summary>
        /// <param name="aByte">待检查的字节</param>
        /// <param name="bitIndex">0～7</param>
        /// <returns></returns>
        public static int CheckBit(byte aByte, int bitIndex)
        {
            if (bitIndex < 0 || bitIndex > 7) return 0;
            return ((aByte >> bitIndex) & 0x01) > 0 ? 1 : 0;
        }

        public static int CheckBit(int iVal, int bitIndex)
        {
            if (bitIndex < 0) return 0;
            return ((iVal >> bitIndex) & 0x01) > 0 ? 1 : 0;
        }

        public static double IEEE754Float(byte[] data, int i)
        {
            int s;
            int e;
            double m;
            double m_dblReturn = 0;

            s = data[i + 2] & 128;
            e = (data[i + 2] & 127) * 2 + (data[i + 3] & 128) / 128;
            m = (Convert.ToDouble((data[i + 3] & 127)) * 65536 + Convert.ToDouble(data[i]) * 256 + Convert.ToDouble(data[i + 1])) / 8388608;
            m_dblReturn = Math.Pow((-1), s) * Math.Pow(2, (e - 127)) * (m + 1);

            return m_dblReturn;

        }

        /// <summary>
        /// 将浮点数转ASCII格式十六进制字符串（符合IEEE-754标准（32））
        /// </summary>
        /// <param name="data">浮点数值</param>
        /// <param name="array"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static bool IEEE754_FloatToString(float data, byte[] array, int index)
        {
            byte[] intBuffer = BitConverter.GetBytes(data);
            for (int i = 0; i < intBuffer.Length; i++)
            {
                if (index + i < array.Length)
                {
                    array[index + i] = intBuffer[i];
                }
            }
            return true;
        }

        public static bool IEEE754_FloatToString_Switch(float data, byte[] array, int index)
        {
            byte[] intBuffer = BitConverter.GetBytes(data);
            for (int i = 0; i < intBuffer.Length; i++)
            {
                if (index + i < array.Length)
                {
                    array[index + i] = intBuffer[intBuffer.Length - 1 - i];
                }
            }
            return true;
        }

        public static double IEEE754Float_Switch1032(byte[] data, int i)
        {
            int s;
            int e;
            double m;
            double m_dblReturn = 0;

            byte[] data1 = new byte[4];
            data1[0] = data[1 + i];
            data1[1] = data[0 + i];
            data1[2] = data[3 + i];
            data1[3] = data[2 + i];

            i = 0;
            m_dblReturn = BitConverter.ToSingle(data1, 0);

            return m_dblReturn;

        }

        public static double IEEE754Float_Switch(byte[] data, int i)
        {
            int s;
            int e;
            double m;
            double m_dblReturn = 0;

            byte[] data1 = new byte[4];
            data1[0] = data[3 + i];
            data1[1] = data[2 + i];
            data1[2] = data[1 + i];
            data1[3] = data[0 + i];
            i = 0;

            m_dblReturn = BitConverter.ToSingle(data1, 0);


            return m_dblReturn;

        }

        public static int GetIntegerFromByteArray(List<int> data, int StartIndex, int EndIndex)
        {
            int ret = 0;
            if (StartIndex < 0 || StartIndex >= data.Count) return 0;

            for (int i = StartIndex; i <= EndIndex; i++)
            {
                if (i >= data.Count) break;
                ret += data[i] << (i - StartIndex);
            }
            return ret;
        }
    }
}
