using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            demo1();
            demo2();

            Console.ReadLine();
        }

        private static void demo1() {
            ExecCmd ecmd = new ExecCmd();
            ecmd.OutputDataReceived += ecmd_OutputDataReceived;
            ecmd.ErrorDataReceived += ecmd_ErrorDataReceived;
            ecmd.processName = "ping";  // 程序名
            ecmd.Exec(" 127.0.0.1");    // 参数
        }

        private static void demo2() {
            ExecCmd ecmd = new ExecCmd();
            ecmd.OutputDataReceived += ecmd_OutputDataReceived;
            ecmd.ErrorDataReceived += ecmd_ErrorDataReceived;
            ecmd.processName = @"G:\workspace\动控实验数据管理\src\DataImport\Console\bin\Debug\console.exe";
            ecmd.Exec(@"001 8c487298fb4649c3ad82e5b609188931 G:\workspace\动控实验数据管理\src\TestData\data.txt 001");
        }

        static void ecmd_ErrorDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            // 打印异常输出
            Console.WriteLine(e.Data);
        }

        static void ecmd_OutputDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            // 打印正常输出
            Console.WriteLine(e.Data);
        }
    }
}
