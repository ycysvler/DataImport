///**************************************************************************************************** 
/// �첽TCP�ͻ���ͨ��ģ��
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
    /// State object�����첽��ȡSocket����
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
        /// Socket��Զ��IP��ַ
        /// </summary>
        public string RemoteIP;

        /// <summary>
        /// Socket��Զ��IP��ַ�Ͷ˿ڡ�192.168.0.58:1066��
        /// </summary>
        public string RemoteEndPoint;
    }

    //�첽TCP�ͻ���ͨ��
    public class TCPClient : CommBase
    {
        Socket mytc;            //����Ŀͻ������Ӷ���

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ServerIP">ָ�����ӵķ�����IP��ַ</param>
        /// <param name="Port">ָ�����ӵķ������˿�</param>
        /// <param name="BufferSize">��������С</param>
        /// <param name="ProtocolHandle">�󶨵�Э�鴦����</param>
        public TCPClient(string ServerIP, int Port, int BufferSize, ProtocolBase ProtocolHandle)
        {
            m_IPAddr = ServerIP;
            m_Port = Port;
            m_BufferSize = BufferSize;
            m_ProtocolHandle = ProtocolHandle;

        }

        //Server IP��ַ
        private string m_IPAddr;

        public string IPAddr
        {
            get { return m_IPAddr; }
            set { m_IPAddr = value; }
        }

        //�˿�
        private int m_Port;

        public int Port
        {
            get { return m_Port; }
            set { m_Port = value; }
        }

        //��������С
        private int m_BufferSize;

        public int BufferSize
        {
            get { return m_BufferSize; }
            set { m_BufferSize = value; }
        }

        /// <summary>
        /// ��ʼ����Server
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
                // ��ʼ�첽����TCP Server
                mytc.BeginConnect(remoteEP,
                    new AsyncCallback(ConnectCallback), mytc);
            }
            catch
            {
                
            }
        }

        /// <summary>
        /// �ر�TCPClient���Ӳ��ͷ���Դ
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

        //��Server��������
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
                //�������ͷŴ��󣬲�����
                this.Connected = false;
            }
            catch (SocketException e)
            {
                this.Connected = false;
                string err = "��������ʱ���ִ���\r\n" + e.Message;
                //MessageBox.Show("��������ʱ���ִ���\r\n" + e.Message);
            }
            catch (Exception e)
            {
                string err = "��������ʱ���ִ���\r\n" + e.Message;
                //MessageBox.Show("��������ʱ���ִ���\r\n" + e.Message);
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

                    //��ֹ���룬����������һ�ν��մ�����
                    lock (remainByteLock)
                    {
                        // ���յ����Ľ��д���
                        if (remainByte == null)
                        {
                            aByte = new byte[bytesRead];
                            Array.Copy(state.buffer, aByte, bytesRead);
                        }
                        else
                        {
                            //�ϴ���δ��������ֽڣ�Ӧ��ƴ����һ�鴦��
                            aByte = new byte[remainByte.Length + bytesRead];
                            Array.Copy(remainByte, aByte, remainByte.Length);
                            Array.Copy(state.buffer, 0, aByte, remainByte.Length, bytesRead);
                        }
                        
                        try
                        {
                            // �ɰ󶨵�ͨ��Э�����������
                            m_ProtocolHandle.Frame_ScanFromStream(aByte, ref remainByte, string.Empty);
                        }
                        catch
                        {
                            remainByte = null;
                        }
                        
                    }

                    // ������������
                    client.BeginReceive(state.buffer, 0, m_BufferSize, 0,
                        new AsyncCallback(ReceiveCallback), state);
                }
                else
                {
                    //������ζ�ŷ������������ر�������
                    this.Connected = false;
                }
            }
            catch (ObjectDisposedException)
            {
                //�������ͷŴ���
                //������ζ�ſͻ��������ر�������
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
                //��������
                MessageBox.Show("��������ʱ���ִ���\r\n" + e.Message);
            }
        }

        /// <summary>
        /// ͨ��TCP Client��������
        /// </summary>
        /// <param name="data">Ҫ���͵�����</param>
        /// <param name="Target">
        /// ����Ŀ�꣺
        /// ����TCP Serverͨ�š���
        ///     1����ĳ���ͻ��˷��ͣ��ò���Ӧ��д�Է���IP��ַ��
        ///     2�����Ҫ���͵����еĿͻ��ˣ�Ӧ��д���ַ�����null����ʱTargetӦ�÷������еĿͻ��˵�IP��ַ�б�"IP1,IP2,..."
        ///         ���û���κοͻ������ӣ��򷵻ؿ��ַ���
        /// ����TCP Client�ʹ��ڷ�ʽ�����ò�������
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
                //MessageBox.Show("Send��������ʱ���ִ���\r\n" + e.Message);
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
                //�������ͷŴ���
                this.Connected = false;
            }
            catch (SocketException)
            {
                this.Connected = false;
            }
            catch (Exception e)
            {
                MessageBox.Show("SendCallback��������ʱ���ִ���\r\n" + e.Message);
            }
        }

    }
}
