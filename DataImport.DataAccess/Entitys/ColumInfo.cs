using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataImport.DataAccess.Entitys
{
    public class ColumInfo
    {
        public string FID { get; set; }
        public string TaskTimeID { get; set; }
        public string TableName { get; set; }
        public string ColName { get; set; }
        public string Remark { get; set; }
        public string CreatedBy { get; set; }
        public string CreationDate { get; set; }
        public string LastUpdatedBy { get; set; }
        public string LastUpdateDate { get; set; }
        public string LastUpdateIp { get; set; }
        public int Version { get; set; }
    }
}
