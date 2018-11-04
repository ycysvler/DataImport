using System;
using System.Collections.Generic;
using System.Text;

namespace HyCommBase
{
    /// <summary>
    /// 通信常数
    /// </summary>
    public static class CommConst
    {
        /// <summary>
        /// 缓冲区大小
        /// </summary>
        public static int BUFFERSIZE = 204800;
        /// <summary>
        /// 报文最大长度
        /// </summary>
        public static int MAXLENGTH = 1024;
        /// <summary>
        /// 最大重发次数
        /// </summary>
        public static int MAXRESEND = 2;
        /// <summary>
        /// 超时判断毫秒数
        /// </summary>
        public static int WAITTICK = 2000;
        
    }

    /// <summary>
    /// 协议处理和通信处理类配对
    /// </summary>
    public class CommProtocolPair
    {
        /// <summary>
        /// 协议处理类
        /// </summary>
        public ProtocolBase Protocol;
        /// <summary>
        /// 通信处理类
        /// </summary>
        public CommBase Comm;

        /// <summary>
        /// 协议标志字符串
        /// </summary>
        public string ProtocolId;
        /// <summary>
        /// 下行通信协议使用：
        /// 以子站设备号为索引，硬件地址为内容的地址字典
        /// </summary>
        public Dictionary<int, int> DeviceNo;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ProtocolId">协议标志字符串</param>
        /// <param name="DeviceNo">
        /// 下行通信协议使用：
        /// 以子站设备号为索引，硬件地址为内容的地址字典
        /// </param>
        public CommProtocolPair(string ProtocolId, Dictionary<int, int> DeviceNo)
        {
            this.ProtocolId = ProtocolId;
            this.DeviceNo = DeviceNo;
        }
    }

}
