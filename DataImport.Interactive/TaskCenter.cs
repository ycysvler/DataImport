using DataImport.DataAccess.Entitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataImport.Interactive
{
    public class TaskCenter
    {
        public static string TaskID { get; set; }
        public static string ScriptCode { get; set; }
        public static string ScriptID { get; set; }
        public static int TaskTimes { get; set; }

        public static TaskInfo CurrentInfo { get; set; }
    }
}
