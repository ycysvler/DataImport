using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using HyCommBase;
using HyUtilities;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Configuration;



namespace TcpClientTest
{
    public class TcpClientApp
    {
        private TCPClient m_Comm;
        private TcpClientAppProtocol m_Protocol;
        private int updTimer = 100;
        private byte[] DataArea = new byte[9];
        
        public int m_process_step ;

        protected System.Timers.Timer timerDriver;
        protected int m_timerDriverInteval;

        private string IpStr = string.Empty;
        private int port ;
        private int m_process_tick = 0;
        public TcpClientAppData m_appdata;
        public int m_appdataindex;
        public UdpServer m_UdpServer;

        public void initCommon(TcpClientAppData appdata, int appdataindex)
        {
            m_appdata = appdata;
            m_appdataindex = appdataindex;

            m_process_step = 0;

            IpStr = appdata.devd[appdataindex].RemoteIp;
            port = appdata.devd[appdataindex].Sendport;

            this.m_timerDriverInteval = 1000;
            this.timerDriver = new System.Timers.Timer(this.m_timerDriverInteval);
            this.timerDriver.AutoReset = true;
            this.timerDriver.Enabled = false;
            this.timerDriver.Elapsed += new System.Timers.ElapsedEventHandler(Main_CommDriver);
        }

        /// <summary>
        /// 协议定时驱动Main_CommDriver的防止重入标志
        /// </summary>
        protected object driverlock = new object();

        public virtual void Main_CommDriver(object source, System.Timers.ElapsedEventArgs e)
        {
            if (!Monitor.TryEnter(driverlock))
                return;

            try
            {
               
            }
            catch (Exception ex)
            {
                string aStr = ex.Message;
                //System.Windows.Forms.MessageBox.Show(aStr);
            }
            finally
            {
                Monitor.Exit(driverlock);
            }
        }

        public int AppMain_start()
        {
            m_Protocol = new TcpClientAppProtocol(updTimer, true, DataArea, false);

            m_Comm = new TCPClient(IpStr, port, CommConst.BUFFERSIZE, m_Protocol);
            m_Protocol.m_Comm = m_Comm;
            //开始尝试连接
            m_Protocol.ClearCache();
            int StartTick = HyTick.TickTimeGet();
            m_Comm.StartClient();
            while (!m_Comm.Connected)
            {
                //等待10秒是否连接成功
                if (HyTick.TickTimeIsArrived(StartTick, 10000))
                {
                    //MessageBox.Show("连接失败！");
                    return -1;
                }
                Application.DoEvents();
            }
            m_Protocol.Main_Start();
            //MessageBox.Show("连接成功！");
            return 1;

        }

        public int AppMain_stop()
        {
            try
            {
                if (m_Protocol == null || m_Comm == null)
                {
                    MessageBox.Show("当前未连接！");
                    return -1;
                }
                m_Protocol.Main_Stop();
                m_Comm.StopClient();
                m_Protocol = null;
                m_Comm = null;

                return 0;
            }
            catch
            {
                return -2;
            }

        }

        //处理总的通讯调度
        public int process(int m_appdataindex )
        {
            bool acmd = false ;

            try
            {
                switch (m_process_step)
                {
                    case 0:  //建立IP端口的连接
                        if (m_appdata.devd[m_appdataindex].ComType == "TCP")
                        {
                            AppMain_start();
                        }
                        m_process_step = 1; //主通信流程调度
                        break;
                    case 1: //判断端口是否连接成功
                        if (m_appdata.devd[m_appdataindex].ComType == "TCP" &&  m_Comm.Connected == true)
                        {
                            m_appdata.devd[m_appdataindex].CommFlag = true;
                            m_appdata.Process_EquCommFlag(m_appdata.devd[m_appdataindex].devAddr, true); //更新设备设备的通讯状态
                            m_process_step = 2;  //连接成功准备发送命令
                        }
                        else
                        {
                            if (m_appdata.devd[m_appdataindex].ComType == "TCP")
                            {
                                m_process_step = 0; //否则进行重新连接
                                m_appdata.devd[m_appdataindex].CommFlag = false;
                                m_appdata.Process_EquCommFlag(m_appdata.devd[m_appdataindex].devAddr, false); //更新设备设备的通讯状态
                            }
                            else
                            {
                                m_appdata.devd[m_appdataindex].CommFlag = true;
                                m_appdata.Process_EquCommFlag(m_appdata.devd[m_appdataindex].devAddr, true);
                                m_process_step = 2;  //连接成功准备发送命令
                            }
                        }
                        break;
                    case 2:
                        acmd =false ;
                        int cmdindex = 0;
                        cmdindex = m_appdata.cmdmany_getcmd(m_appdataindex);
                        if (cmdindex > 0)  //判断是否有批命令发送
                        {
                            if (m_appdata.devd[m_appdataindex].CmdFlag == true ) //  获取当前的命令
                            {
                                // 根据cmdindex获取命令
                                acmd = true;
                            }
                        }
                        if (acmd)  //根据获取到的
                        {
                            string sendIP = string.Empty;
                            string ComType = m_appdata.devd[m_appdataindex].ComType;
                            byte[] sendbyte = m_appdata.Create_cmdbyte_ByIndex(cmdindex - 1);
                            if (ComType == "TCP")
                            {
                                m_Comm.SendData(sendbyte, ref sendIP);
                                m_process_step = 4; //处理发送后的反馈
                            }
                            else if (ComType == "UDP") //UDP方式的命令发送
                            {
                                UdpSendMessage(m_appdataindex, sendbyte);
                                //需要进行处理接收事务
                                if (m_appdata.devd[m_appdataindex].remoteSendDateChk == "TRUE")
                                {
                                    m_appdata.Process_EquComm_RunStatus("01");//暂停当前的进程
                                    m_UdpServer = new UdpServer();
                                    m_UdpServer.StartListen(m_appdata.devd[m_appdataindex].LocalIp, m_appdata.devd[m_appdataindex].Recvport);
                                    if (m_UdpServer.RecvLength > 0 )
                                    {
                                        m_appdata.Process_EquComm_RunStatus("02");//继续执行
                                    }
                                    else
                                    {
                                        //执行程序中止执行
                                        m_appdata.Process_EquComm_RunStatus("03");//中止执行，在这里需要调用
                                    }
                                }
                                m_process_step = 2;
                            }
                            LogHelper.Log(System.DateTime.Now.ToString(HyConst.DATETIME_yMdHmsf_STRING) + "  " + ComType + (cmdindex-1).ToString("00") + "Send:" + HexEncoding.Instance.GetString(sendbyte));
                            m_appdata.Process_EquComm_Message(System.DateTime.Now, m_appdata.GetSendMessage(cmdindex -1), true, true);
                            /*
                            if (cmdindex == 1)
                            {
                                m_appdata.CmdIndexStatus(cmdindex, true);
                            }
                            if (cmdindex > 1 && m_appdata.SequenceCmdList[cmdindex].secs > m_appdata.SequenceCmdList[cmdindex - 1].secs)
                            {
                                m_appdata.CmdIndexStatus(cmdindex, true);
                            }
                            */
                            m_appdata.CmdIndexStatus(cmdindex-1, true);
                           
                        }
                        else
                        {
                            string sendIP = string.Empty;
                            byte[] sendbyte = m_appdata.GetCmd_Bydev(m_appdataindex);
                            m_Comm.SendData(sendbyte, ref sendIP);
                            m_appdata.Process_EquComm_Message(System.DateTime.Now, HexEncoding.Instance.GetString(sendbyte), true, false);
                            m_process_step = 3;
                            m_process_tick = HyTick.GetTickCount();
                        }
                        break;

                    case 3:  //处理获得的数据内容
                        if( m_Protocol.m_recvdatalen > 0) 
                        {
                            //处理接收到的数据
                            Receive_Date(m_appdataindex, m_Protocol.m_recvdata);
                            m_appdata.Process_EquComm_Message(System.DateTime.Now, HexEncoding.Instance.GetString(m_Protocol.m_recvdata), false,true);
                            //处理完了就清除掉数据
                            m_Protocol.m_recvdatalen = 0;
                            //处理完成后继续发送
                            m_process_step = 2;
                        }
                        else 
                        {
                            if (HyTick.TickTimeIsArrived(m_process_tick, 1000))
                            {
                                m_process_step = 2;
                            }
                        }
                        // 判断通信，只有TCP的方式才进行这么处理
                        if (m_Comm.Connected == false && m_appdata.devd[m_appdataindex].ComType == "TCP")
                        {
                            m_process_step = 0;
                        }
                        break;
                    case 4: //Tcp的方式处理控制命令返回协议处理
                        if (m_Protocol.m_recvdatalen > 0)  //只要接收到数据就进行继续发送
                        {
                             m_process_step = 2;
                        }
                        break;
                }
                return 0;
            }
            catch
            {
                return -2;
            }
        }

        //根据接收到的数据进行解析
        private void Receive_Date(int m_appdataindex, byte[] RecDate)
        {
            int m_NumberCount = 0;
            float douba = 0;
            int fldatalen = 4;

            //根据接收到的数据进行校验
            //1,判断报文头是否正确
            if (RecDate[0] != 0xAA || RecDate[1] != 0x55)
            {
                return;
            }

            //2,判断设备的地址是否正确
            if (RecDate[2] != m_appdata.devd[m_appdataindex].devAddr)
            {
                return;
            }
            m_NumberCount = RecDate[4];

            if(RecDate [3] == 0x03)
            {
                try
                {
                    for (int i = 0; i < m_NumberCount; i++)
                    {
                        douba = ((int)RecDate[5 + i * fldatalen] * 256 + (int)RecDate[5 + fldatalen * i + 1]) << 16;
                        douba += ((int)RecDate[5 + i * fldatalen + 2] * 256 + (int)RecDate[5 + fldatalen * i + 3]);
                        m_appdata.devd[m_appdataindex].afl[i] = douba;
                    }
                    //处理实时数据显示传递
                    m_appdata.process(m_appdataindex,m_appdata.devd[m_appdataindex].devAddr);
                }
                catch
                {
                }
            }
        }

        //Udp方式发送报文信息
        private void UdpSendMessage(int m_appindex,byte[] sendMessage)
        {
           
            UdpClient UdpSend;
            UdpSend = new UdpClient();
            IPEndPoint remoteIpep = new IPEndPoint(IPAddress.Parse(m_appdata.devd[m_appindex].RemoteIp), m_appdata.devd[m_appindex].Sendport); // 发送到的IP地址和端口号
            UdpSend.Send(sendMessage, sendMessage.Length, remoteIpep);
            UdpSend.Close();
        }

        //Udp方式接收报文信息
        private void UdpRevMessage(int m_appindex)
        {

            UdpClient udpcRecv;
            udpcRecv = new UdpClient(8048);
            byte[] Revdata = new byte[20];
            IPEndPoint remoteIp = new IPEndPoint(IPAddress.Any, 0);
            try
            {
                Revdata = udpcRecv.Receive(ref remoteIp);
                if (Revdata.Length > 0)  //表示已经收到数据  可以进行解析
                {
                    m_appdata.Process_EquComm_Message(System.DateTime.Now, HexEncoding.Instance.GetString(Revdata), false, true);
                }
                else  //如果没有收到，进行程序的中止
                {
                }
            }
            catch
            {
            }
            udpcRecv.Close();
        }

        //Udp方式发送和接收报文信息
        private void UdpSendRecvMessage(int m_appindex, byte[] sendMessage)
        {
            byte[] Revdata;

            try
            {

                IPEndPoint remoteIp = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 15010); // 发送到的IP地址和端口号

                //定义网络类型，数据连接类型和网络协议UDP
                Socket UdpServer = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                UdpServer.SendTo(sendMessage, sendMessage.Length, SocketFlags.None, remoteIp);

                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint Remote = (EndPoint)sender;

                Revdata = new byte[20];
                //对于不存在的IP地址，加入此行代码后，可以在指定时间内解除阻塞模式限制
                int RecvLength = UdpServer.ReceiveFrom(Revdata, ref Remote);
                if (RecvLength > 0)  //表示已经收到数据  可以进行解析
                {
                    m_appdata.Process_EquComm_Message(System.DateTime.Now, HexEncoding.Instance.GetString(Revdata),false,true);
                }
                else  //如果没有收到，进行程序的中止
                {

                }
                UdpServer.Close();
            }
            catch
            {
            }
        }
    }
}
