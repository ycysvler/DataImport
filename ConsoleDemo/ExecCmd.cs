
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace ConsoleDemo
{
    public class ExecCmd
    {
        public string processName;
        public string workDirectory = "";
        private Process process = new Process();

        public bool createNoWindow = true;

        public event DataReceivedEventHandler ErrorDataReceived;
        public event DataReceivedEventHandler OutputDataReceived;

        public void Exec(string arguments)
        {
            if (processName != null && !processName.Equals(""))

                try
                {
                    process.StartInfo.FileName = processName;        // 外部调用命令路径
                    process.StartInfo.Arguments = arguments;         // 命令参数
                    process.StartInfo.UseShellExecute = false;        // 不使用操作系统外壳程序启动线程

                    if (workDirectory != "")
                        process.StartInfo.WorkingDirectory = workDirectory;         //程序运行目录

                    process.StartInfo.RedirectStandardError = true;   // 外部程序错误输出重定向到StandardError流
                    process.StartInfo.RedirectStandardOutput = true;  // 外部程序正常输出重定向到StandardOutput流

                    process.StartInfo.CreateNoWindow = createNoWindow;   // 不创建进程窗口 

                    // 异步错误输出事件
                    process.ErrorDataReceived += new DataReceivedEventHandler(process_ErrorDataReceived);
                    // 异步正常输出事件
                    process.OutputDataReceived += new DataReceivedEventHandler(process_OutputDataReceived);

                    process.Start();                                  // 启动线程  
                    process.BeginOutputReadLine();                    // 开始异步读取正常输出
                    process.BeginErrorReadLine();                     // 开始异步读取异常输出
                    process.WaitForExit();                            //阻塞等待进程结束                 
                }
                finally
                {
                    process.Close();                                  //关闭进程  
                    process.Dispose();                                //释放资源    
                }
        }

        void process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (OutputDataReceived != null)
                OutputDataReceived(sender, e);
        }

        void process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (ErrorDataReceived != null)
                ErrorDataReceived(sender, e);
        }

        public void Dispose()
        {

            try
            {
                if (!process.HasExited)
                {
                    process.Kill();
                }
                process.Close();                                  //关闭进程  
                process.Dispose();                                //释放资源    
            }
            catch { }
        }
    }
}
