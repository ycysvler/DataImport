using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataImport.DataAccess.Entitys
{  
    public class ImportLog
    {
        public string Content { get; set; }
        public string FileName { get; set; } 
        public string ProjectCode { get; set; } 
        public string TaskCode { get; set; }
        public string Times { get; set; } 
        public string CreatedBy { get; set; }
        public DateTime CreationDate { get; set; }  
        
    }
}
