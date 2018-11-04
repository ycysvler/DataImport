using System;
using System.Collections.Generic;
using System.Text;

namespace HyUtilities
{
    /// <summary>
    /// 通信方式定义
    /// </summary>
    public enum CommMode : byte
    {
        /// <summary>
        /// RS232串口通信方式
        /// </summary>
        RS232 = 0,
        /// <summary>
        /// TCP客户端通信方式
        /// </summary>
        TCP_CLIENT = 1,
        /// <summary>
        /// TCP服务器通信方式
        /// </summary>
        TCP_SERVER = 2,
    }

    /// <summary>
    /// 协议转发通信命令类型
    /// </summary>
    public enum TransCommandType : byte
    {
        /// <summary>
        /// 协议转发DO控制命令
        /// </summary>
        TRANS_DO = 0,
        /// <summary>
        /// 协议转发AO控制命令
        /// </summary>
        TRANS_AO = 1,
    }

    /// <summary>
    /// 通信方向定义
    /// </summary>
    public enum CommOrientaion : byte
    {
        /// <summary>
        /// 发送
        /// </summary>
        SEND = 0,
        /// <summary>
        /// 接收
        /// </summary>
        RECV = 1,
    }

   

}
