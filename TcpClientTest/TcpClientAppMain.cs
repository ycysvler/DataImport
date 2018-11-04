using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading;

using HyCommBase;
using HyUtilities;
using DataImport.Interactive.Sequences;

namespace TcpClientTest
{
    public class TcpClientAppMain
    {
        public TcpClientApp [] appm;
        public TcpClientAppData appdata;

        //进行通讯类的初始化
        public void init(Sequence sequence)
        {
            int i;
            appdata = new TcpClientAppData();
            appdata.init(sequence);

            appm = new TcpClientApp[sequence.ProtocolItems.Count];  //根据定义的设备数量进行构建数组

            for (i = 0; i < appm.Length; i++)
            {
                appm[i] = new TcpClientApp();
                appm[i].initCommon(appdata, i);
            }
        }


        private Thread SysThread = null;
        public void ThreadEntry()
        {
            try
            {
                while (true)
                {
                    for (int i = 0; i < appm.Length; i++)  //对服务端进行统一处理。
                    {
                        appm[i].process(i);
                        Thread.Sleep(100);
                    }
                }
            }
            catch
            {
            }
        }

        public void start()
        {
            if (SysThread == null)
            {
                SysThread = new Thread(new ThreadStart(ThreadEntry));
                SysThread.Start();
            }
            else if (SysThread.ThreadState == ThreadState.Stopped)
                SysThread.Start();
            else if (SysThread.ThreadState == ThreadState.Aborted)
            {
                SysThread = new Thread(new ThreadStart(ThreadEntry));
                SysThread.Start();
            }
        }


        public void stop()
        {
            if (SysThread != null)
            {
                SysThread.Abort();
                LogHelper.Log("************" + System.DateTime.Now.ToString(HyConst.DATETIME_yMdHmsf_STRING) + "程序运行停止" + "***********");
            }
        }
    }
}
