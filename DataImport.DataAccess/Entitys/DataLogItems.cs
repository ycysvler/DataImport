using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataImport.DataAccess.Entitys
{
    public  class DataLogItems
    {
        public string FID { get; set; }
        public string LogID { get; set; }
        public int ImpStatus { get; set; }
        public string ErrorMsg { get; set; }
        public int RowIndex { get; set; }
        public string Content { get; set; } 
    }
}
