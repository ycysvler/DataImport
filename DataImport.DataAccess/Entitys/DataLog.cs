using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataImport.DataAccess.Entitys
{
    public class DataLog
    {
        public string FID { get; set; }
        public string TestProjectID{get;set;}
        public DateTime ImpDateTime{get;set;}
        public string FullFloderName{get;set;}
        public int ImpRowsCount{get;set;}
        public string ImpStatus{get;set;}
        public string ErrorMsg{get;set;}
        public string CreatedBy{get;set;}
        public DateTime CreationDate{get;set;}
        public string LastUpdatedBy{get;set;}
        public DateTime LastUpdateDate{get;set;}
        public string LastUpdateIp{get;set;}
        public int Version{get;set;}
        public string ImpFileName { get; set; }
        public string ObjectTable{get;set;} 
        public string TypeName{get;set;}

        public static DataLog Parse(DataRow row)
        {
            DataLog result = new DataLog();

            //result.FID = row["FID"].ToString();
            result.TestProjectID = row["TEST_PROJECT_ID"].ToString();
            result.TypeName = row["TYPENAME"].ToString();
            result.ObjectTable = row["OBJECT_TABLE"].ToString(); 
            result.Version = Convert.ToInt32(row["VERSION"]); 

            return result;
        }
    }
}
