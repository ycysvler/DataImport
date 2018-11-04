using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TcpClientTest
{
    class LogHelper
    {
        private static object lk = new object();

        public static void Log(string msg)
        {
            string path = "log";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            path = string.Format("log/{0:yyyyMMdd}.log", DateTime.Now);
            if (!File.Exists(path))
            {
                Stream stream = File.Create(path);
                stream.Close();
            }
            lock (lk)
            {
                System.IO.File.AppendAllText(path, msg + "\r\n");
            }
        }
    }
}
