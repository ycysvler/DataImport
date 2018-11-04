using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Runtime.InteropServices;
using System.Net.NetworkInformation;
using System.Collections;

namespace HyUtilities
{
    public class HyNetUtility
    {
        /// <summary>
        /// 获取本机IP地址
        /// </summary>
        /// <returns></returns>
        public static string GetLocalIPAddress()
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ipAddress in ipHostInfo.AddressList)
            {
                if (ipAddress.ToString() != "0.0.0.0" && IsValidIP(ipAddress.ToString()))
                {
                    return ipAddress.ToString();
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// 判断是否本机IP地址
        /// </summary>
        /// <param name="IPAddr"></param>
        /// <returns></returns>
        public static bool IsLocalIPAddress(string IPAddr)
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ipAddress in ipHostInfo.AddressList)
            {
                if (ipAddress.ToString() == IPAddr.Trim())
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 检查是否合法的IP地址
        /// </summary>
        /// <param name="IPStr">如以下格式的IP地址"192.168.0.1"</param>
        /// <returns></returns>
        public static bool IsValidIP(string IPStr)
        {
            string[] elements = IPStr.Trim().Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
            if (elements.Length != 4) return false;
            foreach (string var in elements)
            {
                int i;
                if (!int.TryParse(var, out i)) return false;
                if (i < 0 || i >= 255) return false;
            }
            return true;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool DeviceIoControl(
            IntPtr HDevice,
            uint dwIoControlCode,
            IntPtr lpInBuffer,
            uint nInBufferSize,
            IntPtr lpOutBuffer,
            uint nOutBufferSize,
            ref uint lpBytesReturned,
            IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        /// <summary>
        /// 获取网卡地址的方法
        /// </summary>
        /// <param name="NicId">网卡的ID ，形式如：{B50027F7-7A42-4F2D-8935-7620F1DB632F} 这样的字符串。</param>
        /// <returns></returns>
        public static string GetNicAddress(string NicId)
        {
            System.IntPtr hDevice = CreateFile("\\\\.\\" + NicId, 0x80000000 | 0x40000000, 0, IntPtr.Zero, 3, 4, IntPtr.Zero);

            if (hDevice.ToInt32() == -1)
            {
                return null;
            }
            uint Len = 0;
            IntPtr Buffer = Marshal.AllocHGlobal(256);

            Marshal.WriteInt32(Buffer, 0x01010101);

            if (!DeviceIoControl(hDevice, 0x170002, Buffer, 4, Buffer, 256, ref Len, IntPtr.Zero))
            {
                Marshal.FreeHGlobal(Buffer);
                CloseHandle(hDevice);
                return null;
            }
            byte[] macBytes = new byte[6];
            Marshal.Copy(Buffer, macBytes, 0, 6);
            Marshal.FreeHGlobal(Buffer);
            CloseHandle(hDevice);
            return new System.Net.NetworkInformation.PhysicalAddress(macBytes).ToString();
        }

        /// <summary>
        /// 获取本机所有的以太网卡的ID
        /// </summary>
        /// <returns></returns>
        public static ArrayList GetAllNic()
        {
            NetworkInterface[] Nics = NetworkInterface.GetAllNetworkInterfaces();
            ArrayList EtherNics = new ArrayList(20);
            foreach (NetworkInterface nic in Nics)
            {
                if (nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    EtherNics.Add(nic.Id);
                }
            }
            return EtherNics;
        }

        /// <summary>
        /// 获取本机所有的以太网卡的硬件地址
        /// </summary>
        /// <returns></returns>
        public static List<string> GetAllNetworkAddress()
        {
            List<string> NetworkAddress = new List<string>();

            ArrayList EtherNics = HyNetUtility.GetAllNic();
            foreach (object Nicid in EtherNics)
            {
                string s = HyNetUtility.GetNicAddress(Nicid.ToString());
                if (s != null) NetworkAddress.Add(s.ToUpper());
            }
            return NetworkAddress;
        }

        /// <summary>
        /// 网卡硬件地址绑定
        /// </summary>
        /// <returns></returns>
        public static bool NetworkAddressAuthentication()
        {
            List<string> NetworkAddress = GetAllNetworkAddress();

            foreach (string nicaddr in NetworkAddress)
            {
                if (HyNetUtility.ValidNetworkAddress.Contains(nicaddr.ToUpper()))
                {
                    return true;
                }
            }
            return false;
        }

        public static List<string> ValidNetworkAddress = new List<string>();

        public static DateTime StartDate = DateTime.Parse("2011-4-1 00:00:00");

        public static int AuthDays = 90;

        /// <summary>
        /// 时间判断
        /// </summary>
        /// <returns></returns>
        public static bool TimeAuthentication()
        {
            if (DateTime.Now > StartDate && StartDate.AddDays(AuthDays) >= DateTime.Now) return true;
            return false;
        }
    }
}
