///**************************************************************************************************** 
/// 异步TCP客户端通信模块
///****************************************************************************************************
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;

using HyUtilities;
using System.Windows.Forms;

namespace HyCommBase
{
    /// <summary>
    /// State object用于异步读取Socket数据
    /// </summary>
    public class StateObject
    {
        /// <summary>
        ///  Client socket.
        /// </summary>
        public Socket workSocket = null;
        /// <summary>
        ///  Receive buffer.
        /// </summary>
        public byte[] buffer;

        /// <summary>
        /// Socket的远端IP地址
        /// </summary>
        public string RemoteIP;

        /// <summary>
        /// Socket的远端IP地址和端口“192.168.0.58:1066”
        /// </summary>
        public string RemoteEndPoint;
    }

    //异步TCP客户端通信
    public class TCPClient : CommBase
    {
        Socket mytc;            //保存的客户端连接对象

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ServerIP">指定连接的服务器IP地址</param>
        /// <param name="Port">指定连接的服务器端口</param>
        /// <param name="BufferSize">缓冲区大小</param>
        /// <param name="ProtocolHandle">绑定的协议处理类</param>
        public TCPClient(string ServerIP, int Port, int BufferSize, ProtocolBase ProtocolHandle)
        {
            m_IPAddr = ServerIP;
            m_Port = Port;
            m_BufferSize = BufferSize;
            m_ProtocolHandle = ProtocolHandle;

        }

        //Server IP地址
        private string m_IPAddr;

        public string IPAddr
        {
            get { return m_IPAddr; }
            set { m_IPAddr = value; }
        }

        //端口
        private int m_Port;

        public int Port
        {
            get { return m_Port; }
            set { m_Port = value; }
        }

        //缓冲区大小
        private int m_BufferSize;

        public int BufferSize
        {
            get { return m_BufferSize; }
            set { m_BufferSize = value; }
        }

        /// <summary>
        /// 开始连接Server
        /// </summary>
        public void StartClient()
        {
            try
            {
                this.CommTime = DateTime.Now;

                // Create a TCP/IP socket.
                mytc = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(m_IPAddr), m_Port);
                // 开始异步连接TCP Server
                mytc.BeginConnect(remoteEP,
                    new AsyncCallback(ConnectCallback), mytc);
            }
            catch
            {
                
            }
        }

        /// <summary>
        /// 关闭TCPClient连接并释放资源
        /// </summary>
        /// <returns></returns>
        public bool StopClient()
        {
            try
            {
                if (mytc != null)
                {
                    mytc.Shutdown(SocketShutdown.Both);
                    mytc.Close();
                }
                this.Connected = false;
                return true;
            }
            catch
            {
                return false;
            }
        }

        //与Server建立连接
        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.
                client.EndConnect(ar);

                this.Connected = true;
                this.CommTime = DateTime.Now;

                // Create the state object.
                StateObject state = new StateObject();
                state.workSocket = client;
                state.buffer = new byte[m_BufferSize];

                // Begin receiving the data from the remote device.
                client.BeginReceive(state.buffer, 0, m_BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            catch (ObjectDisposedException)
            {
                //对象已释放错误，不处理
                this.Connected = false;
            }
            catch (SocketException e)
            {
                this.Connected = false;
                string err = "建立连接时出现错误：\r\n" + e.Message;
                //MessageBox.Show("建立连接时出现错误：\r\n" + e.Message);
            }
            catch (Exception e)
            {
                string err = "建立连接时出现错误：\r\n" + e.Message;
                //MessageBox.Show("建立连接时出现错误：\r\n" + e.Message);
            }
        }

        protected virtual void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                // Read data from the remote device.
                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    this.CommTime = DateTime.Now;

                    byte[] aByte;

                    //防止重入，并阻塞至上一次接收处理完
                    lock (remainByteLock)
                    {
                        // 接收到报文进行处理
                        if (remainByte == null)
                        {
                            aByte = new byte[bytesRead];
                            Array.Copy(state.buffer, aByte, bytesRead);
                        }
                        else
                        {
                            //上次有未处理完的字节，应该拼起来一块处理
                            aByte = new byte[remainByte.Length + bytesRead];
                            Array.Copy(remainByte, aByte, remainByte.Length);
                            Array.Copy(state.buffer, 0, aByte, remainByte.Length, bytesRead);
                        }
                        
                        try
                        {
                            // 由绑定的通信协议类分析报文
                            m_ProtocolHandle.Frame_ScanFromStream(aByte, ref remainByte, string.Empty);
                        }
                        catch
                        {
                            remainByte = null;
                        }
                        
                    }

                    // 继续接收数据
                    client.BeginReceive(state.buffer, 0, m_BufferSize, 0,
                        new AsyncCallback(ReceiveCallback), state);
                }
                else
                {
                    //这里意味着服务器端主动关闭了连接
                    this.Connected = false;
                }
            }
            catch (ObjectDisposedException)
            {
                //对象已释放错误
                //这里意味着客户端主动关闭了连接
                this.Connected = false;
            }
            catch (SocketException e)
            {
                this.Connected = false;
                switch (e.SocketErrorCode)
                {
                    case SocketError.ConnectionReset:
                        break;
                    case SocketError.HostDown:
                        break;
                    case SocketError.NetworkDown:
                        break;
                    case SocketError.NetworkReset:
                        break;
                    case SocketError.Shutdown:
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                //其他错误
                MessageBox.Show("接收数据时出现错误：\r\n" + e.Message);
            }
        }

        /// <summary>
        /// 通过TCP Client发送数据
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
        public override void SendData(byte[] data, ref string Target)
        {
            try
            {
                // Begin sending the data to the remote device.
                mytc.BeginSend(data, 0, data.Length, 0,
                    new AsyncCallback(SendCallback), mytc);
            }
            catch (SocketException)
            {
                this.Connected = false;
            }
            catch (Exception e)
            {
                //MessageBox.Show("Send发送数据时出现错误：\r\n" + e.Message);
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = client.EndSend(ar);

            }
            catch (ObjectDisposedException)
            {
                //对象已释放错误
                this.Connected = false;
            }
            catch (SocketException)
            {
                this.Connected = false;
            }
            catch (Exception e)
            {
                MessageBox.Show("SendCallback发送数据时出现错误：\r\n" + e.Message);
            }
        }

    }
}
