using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataImport.DataAccess.Entitys
{
    public class DataScriptMap
    {
        public string FID { get; set; }
        public string MdsImpDataScriptRuleID{get;set;}
        public string TableColName{get;set;}
        public string FileColName{get;set;}
        public string TransferType{get;set;}
        public string InsertValueMethod{get;set;}
        private string _transferscript = "";
        public string TransferScript { get { return _transferscript; } set { _transferscript = value; } }
        public string Remark{get;set;}
        public string CreatedBy{get;set;}
        public DateTime CreationDate{get;set;}
        public string LastUpdatedBy{get;set;}
        public DateTime LastUpdateDate{get;set;}
        public string LastUpdateIp{get;set;}
        public int Version{get;set;}

        public static DataScriptMap Parse(DataRow row)
        {
            DataScriptMap result = new DataScriptMap();

            result.FID = row["FID"].ToString();
            result.MdsImpDataScriptRuleID = row["MDS_IMP_DATA_SCRIPT_RULE_ID"].ToString();
            result.TableColName = row["TABLE_COL_NAME"].ToString();
            result.FileColName = row["FILE_COL_NAME"].ToString();
            result.TransferType = row["TRANSFER_TYPE"].ToString();
            result.InsertValueMethod = row["INSERT_VALUE_METHOD"].ToString();
            result.TransferScript = row["TRANSFER_SCRIPT"].ToString(); 
            result.Remark = row["REMARK"].ToString();
            result.CreatedBy = row["CREATED_BY"].ToString();
            result.CreationDate = Convert.ToDateTime(row["CREATION_DATE"]);
            result.LastUpdatedBy = row["LAST_UPDATED_BY"].ToString();
            result.LastUpdateDate = Convert.ToDateTime(row["LAST_UPDATE_DATE"]);
            result.LastUpdateIp = row["LAST_UPDATE_IP"].ToString();
            result.Version = Convert.ToInt32(row["VERSION"]);
 
            return result;
        }
    }
}
