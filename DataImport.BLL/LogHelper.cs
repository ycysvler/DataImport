using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DataImport.BLL
{
    public class LogHelper
    {
        public static void WriteLog(string log) {

            string path = AppDomain.CurrentDomain.BaseDirectory;

            string filepath = string.Format(@"{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "log");

            if (!Directory.Exists(filepath)) {
                Directory.CreateDirectory(filepath);
            }

            string filename = string.Format(@"{0}{1}\{2}.log", AppDomain.CurrentDomain.BaseDirectory, "log", DateTime.Now.ToString("yyyyMMdd"));

            
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(filename, true))
            {
                file.WriteLine(log);// 直接追加文件末尾，换行   
            }

        }
    }
}
