/// **************************************************************************************
/// 通信方式基类：
/// TCP Client/TCP Server/232等通信类均应继承此类
/// **************************************************************************************
using System;
using System.Collections.Generic;
using System.Text;

namespace HyCommBase
{
    public abstract class CommBase
    {
        /// <summary>
        /// 通信方式基类：
        /// TCP Client/TCP Server/232等通信类均应继承此类
        /// </summary>
        protected CommBase()
        {

        }
         
        /// <summary>
        /// 与通信类绑定的通信协议类
        /// </summary>
        public ProtocolBase m_ProtocolHandle;

        /// <summary>
        /// 未处理完的剩余字节数据
        /// </summary>
        public byte[] remainByte;
        /// <summary>
        /// 防止重入的锁
        /// </summary>
        public object remainByteLock= new object();

        /// <summary>
        /// 通信连接状态
        /// </summary>
        private bool m_Connected;
        /// <summary>
        /// 通信连接状态
        /// </summary>
        public bool Connected
        {
            get { return m_Connected; }
            set 
            { 
                m_Connected = value;
                /// 通信连接的状态变化，将改变相应的协议通信状态
                if (this.m_ProtocolHandle != null)
                {
                    this.m_ProtocolHandle.CommStatus = value;
                }
            }
        }

        /// <summary>
        /// 最近接收到数据的时间
        /// </summary>
        public DateTime CommTime;

        /// <summary>
        /// 通信状态错误后，正在尝试重连的标志
        /// </summary>
        public bool ReConnectingFlag;

        /// <summary>
        /// 通信状态错误后，重连开始的时间
        /// </summary>
        public DateTime ReConnectTime;

        /// <summary>
        /// 通过通信发送字节数据
        /// </summary>
        /// <param name="data">要发送的数据</param>
        /// <param name="Target">
        /// 发送目标：
        /// 对于TCP Server通信——
        ///     1、对某个客户端发送：该参数应填写对方的IP地址；
        ///     2、如果要发送到所有的客户端，应填写空字符串或null。这时Target应该返回所有的客户端的IP地址列表"IP1,IP2,..."
        ///         如果没有任何客户端连接，则返回空字符串
        /// 对于TCP Client和串口方式——该参数无用
        /// </param>
        public abstract void SendData(byte[] data, ref string Target);

    }

}
