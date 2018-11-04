using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataImport.DataAccess.Entitys
{
    public class ObjtableInfo
    {
        public string FID { get; set; }
        public string ObjectTableCode { get; set; }
        public string ObjectTableName { get; set; }
        public string Status { get; set; }
        public string CreatedBy { get; set; }
        public string CreationDate { get; set; }
        public string LastUpdatedBy { get; set; }
        public string LastUpdateDate { get; set; }
        public string LastUpdateIp { get; set; }
        public int Version { get; set; }

        public static ObjtableInfo Parse(System.Data.DataRow row)
        {
            ObjtableInfo result = new ObjtableInfo();

            result.FID = row["FID"].ToString();
            result.ObjectTableCode = row["OBJECT_TABLE_CODE"].ToString();
            result.ObjectTableName = row["OBJECT_TABLE_NAME"].ToString();
            return result;            
        }

   
    }
}
