using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace HyCommBase
{
    public class UdpServer
    {
        public int RecvLength;
        public string RecvStr;
        public void StartListen(string RecvIp,int RecvPort)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork,
                SocketType.Dgram,
                ProtocolType.Udp);
            EndPoint localEP = new IPEndPoint(IPAddress.Parse(RecvIp), RecvPort);
            socket.Bind(localEP);
            int Count = 5;

            //启动异步接收
            State state = new State(socket);

            for (int index = 0; index < Count; index ++ )
            {
                if (RecvLength == 0)
                {
                    socket.BeginReceiveFrom(
                       state.Buffer, 0, state.Buffer.Length,
                       SocketFlags.None,
                       ref state.RemoteEP,
                       EndReceiveFromCallback,
                       state);
                    Thread.Sleep(100);  
                }
                else
                {
                    index = Count;
                }
            }

           
            socket.Close();
        }

        //异步接收回调函数
        public void EndReceiveFromCallback(IAsyncResult iar)
        {
            State state = iar.AsyncState as State;
            Socket socket = state.Socket;
            try
            {
                //完成接收
                RecvLength = socket.EndReceiveFrom(iar, ref state.RemoteEP);
                //显示客户端地址和端口
                //Console.WriteLine("客户端终结点：{0}", state.RemoteEP.ToString());
                //显示接收信息
                //Console.WriteLine("接收数据字节数：{0}", RecvLength);
                RecvStr = Encoding.Default.GetString(state.Buffer, 0, RecvLength);
                //Console.WriteLine("来着客户端信息：{0}", RecvStr);
                //向客户端发送信息
                //SendMeaage(socket, state.RemoteEP, "2222333333");
            }
            catch (Exception e)
            {
                //Console.WriteLine("发生异常！异常信息：");
                //Console.WriteLine(e.Message);
                RecvLength = 0;
                RecvStr = string.Empty;
            }
            finally
            {
            
            }
        }

        /// <summary>
        /// 向客户端发送信息
        /// </summary>
        /// <param name="socket">本地Socket（服务器Socket）</param>
        /// <param name="remoteEndPoint">客户端终结点</param>
        /// <param name="Message">信息</param>
        public void SendMeaage(Socket socket, EndPoint remoteEndPoint, string Message)
        {
            byte[] bytes = Encoding.Default.GetBytes(Message);
            socket.SendTo(bytes, remoteEndPoint);
        }
    }

    /// <summary>
    /// 用于异步接收处理的辅助类
    /// </summary>
    public class State
    {
        public State(Socket socket)
        {
            this.Buffer = new byte[1024];
            this.Socket = socket;
            this.RemoteEP = new IPEndPoint(IPAddress.Any, 0);
        }
        /// <summary>
        /// 获取本机（服务器）Socket
        /// </summary>
        public Socket Socket { get; private set; }
        /// <summary>
        /// 获取接收缓冲区
        /// </summary>
        public byte[] Buffer { get; private set; }
        /// <summary>
        /// 获取/设置客户端终结点
        /// </summary>
        public EndPoint RemoteEP;
    }
}