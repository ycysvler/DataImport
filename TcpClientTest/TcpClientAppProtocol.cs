/// 文本流协议
/// 传送基本的文本流，不做任何的协议解析
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HyCommBase;
using HyUtilities;
using System.Threading;

namespace TcpClientTest
{
    public class TcpClientAppProtocol : ProtocolBase
    {
        public TcpClientAppProtocol(int timerDriver, bool AutoSendFlag, byte[] AutosendData, bool AutoCheckCommFlag)
            : base(0, 0, 0, timerDriver, 0, AutoCheckCommFlag)
        {
            m_AutoSendFlag = AutoSendFlag;
            m_AutosendData = AutosendData;
        }

        private bool m_AutoSendFlag;
        private byte[] m_AutosendData;

        public byte[] m_recvdata;
        public int m_recvdatalen;

        public override void Frame_ScanFromStream(byte[] aByte, ref byte[] rByte, string IPAddr)
        {
            m_recvdatalen = aByte.Length ;
            m_recvdata = new byte[m_recvdatalen];
            Array.Copy(aByte,m_recvdata, m_recvdatalen);
            rByte = null;
        }

        public override bool Frame_Analysis()
        {
            return true;
        }

        public override void Polling_Init()
        {
            return;
        }

        public override void Main_CommDriver(object source, System.Timers.ElapsedEventArgs e)
        {
            if (!Monitor.TryEnter(driverlock))
                return;
            try
            {
                //处理缓冲区的发送命令
                Main_CommDriver_Send();
            }
            catch (Exception ex)
            {
                string aStr = ex.Message;
            }
            finally
            {
                Monitor.Exit(driverlock);
            }
        }

        public override void Main_SendCmd()
        {
            try
            {
                //直接发送字节流
                string sendIP = se.IPAddr;
                this.m_Comm.SendData(se.DataArea, ref sendIP);
            }
            catch
            {
            }
        }
    }
}
