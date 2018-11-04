using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HyCommBase;
using HyUtilities;
using System.Reflection;

namespace TcpClientTest
{
    
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

        }

        private TCPClient m_Comm;
        private ByteStreamProtocol m_Protocol;
        private TcpClientAppMain m_TcpMain;
     
        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void cmdStart_Click(object sender, EventArgs e)
        {
            if (!HyNetUtility.IsValidIP(this.txtIP.Text))
            {
                MessageBox.Show("设定的IP地址不正确！");
                this.txtIP.Focus();
                return;
            }

            int port;
            if (!int.TryParse(this.txtPort.Text, out port) || port <= 0 || port > 65535)
            {
                MessageBox.Show("设定的端口不正确！");
                this.txtPort.Focus();
                return;
            }

            if (m_Comm != null && m_Comm.Connected)
            {
                MessageBox.Show("当前已连接！");
                return;
            }


            byte[] autosendData = null;
            if (chkHex.Checked)
            {
                autosendData = HexEncoding.Instance.GetBytes(this.TextBoxSend.Text);
            }
            else
            {
                //用GB2312编码
                autosendData = Encoding.GetEncoding("gb2312").GetBytes(this.TextBoxSend.Text);
            }
            m_Protocol = new ByteStreamProtocol((int)updTimer.Value, checkBoxAuto.Checked, autosendData, false);

            m_Comm = new TCPClient(this.txtIP.Text, port, CommConst.BUFFERSIZE, m_Protocol);
            m_Protocol.m_Comm = m_Comm;

            this.commFrameShow1.Init(m_Protocol);

            //开始尝试连接
            m_Protocol.ClearCache();
            int StartTick = HyTick.TickTimeGet();
            m_Comm.StartClient();
            while (!m_Comm.Connected)
            {
                //等待10秒是否连接成功
                if (HyTick.TickTimeIsArrived(StartTick, 10000))
                {
                    MessageBox.Show("连接失败！");
                    return;
                }
                Application.DoEvents();
            }

            MessageBox.Show("连接成功！");

            m_Protocol.Main_Start();
        }

        private void cmdStop_Click(object sender, EventArgs e)
        {
            //UdpServer m_UdpServer = new UdpServer();
            
            //m_UdpServer.StartListen();
             
            try
            {
                if (m_Protocol == null || m_Comm == null)
                {
                    MessageBox.Show("当前未连接！");
                    return;
                }

                m_Protocol.Main_Stop();

                m_Comm.StopClient();

                m_Protocol = null;
                m_Comm = null;

                MessageBox.Show("已断开连接！");
            }
            catch
            {
            }
        }

        private void ButtonSend_Click(object sender, EventArgs e)
        {
            if (m_Protocol == null || m_Comm == null)
            {
                MessageBox.Show("当前未连接！");
                return;
            }

            byte[] DataArea;
            if (chkHex.Checked)
            {
                DataArea = HexEncoding.Instance.GetBytes(this.TextBoxSend.Text);
            }
            else
            {
                //用GB2312编码
                string astr = string.Format("{0}\r\n",this.TextBoxSend.Text) ;

                DataArea = Encoding.GetEncoding("gb2312").GetBytes(astr +"\r\n");
            }

            m_Protocol.Main_SendToCache(0, 0, DataArea, false, string.Empty);

        }

        private void checkBoxAuto_CheckedChanged(object sender, EventArgs e)
        {
            this.ButtonSend.Enabled = !checkBoxAuto.Checked;
            byte[] ret = new byte[1];
            string ObjIdStr = "siSIG1308";
            ret[0] = (byte)Convert.ToInt32(Convert.ToChar(ObjIdStr.Substring(0,1)));
        }
    }
}
