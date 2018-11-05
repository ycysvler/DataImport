using DataImport.DataAccess.Entitys;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataImport.DataAccess
{
    public class DataLogDAL
    {
        public static int Insert(DataLog item)
        {
            string sql = string.Format("INSERT INTO MDS_IMP_DATA_LOG(FID,TEST_PROJECT_ID,IMP_DATE_TIME,FULL_FLODERNAME,IMPROWSCOUNT,IMP_STATUS,ERROR_MSG,CREATED_BY,CREATION_DATE,LAST_UPDATED_BY,LAST_UPDATE_DATE,LAST_UPDATE_IP,VERSION,IMPFILENAME,OBJECT_TABLE,TYPENAME) ");
            sql += string.Format("VALUES('{0}','{1}',to_date('{2}','yyyy/mm/dd hh24:mi:ss'),'{3}','{4}','{5}','{6}','{7}',to_date('{8}','yyyy/mm/dd hh24:mi:ss'),'{9}',to_date('{10}','yyyy/mm/dd hh24:mi:ss'),'{11}',{12},'{13}','{14}','{15}')",
                item.FID, item.TestProjectID, 
                item.ImpDateTime.ToString("yyyy/MM/dd HH:mm:ss"), 
                item.FullFloderName, 
                item.ImpRowsCount, item.ImpStatus, item.ErrorMsg, item.CreatedBy, 
                item.CreationDate.ToString("yyyy/MM/dd HH:mm:ss"), item.LastUpdatedBy, 
                item.LastUpdateDate.ToString("yyyy/MM/dd HH:mm:ss"), item.LastUpdateIp, 
                item.Version, item.ImpFileName, item.ObjectTable, item.TypeName);

            return OracleHelper.Excuse(sql);
        }

        public static string getSingleTableName(string projectID)
        {
            string sql = string.Format("SELECT  OBJECT_TABLE FROM MDS_IMP_DATA_LOG WHERE TEST_PROJECT_ID='{0}' ORDER BY CREATION_DATE DESC", projectID);

            DataSet ds = OracleHelper.Query(sql);
            if (ds != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt.Rows.Count > 0)
                    return dt.Rows[0][0].ToString();
            }
            return "";
        }

        public static List<DataLog> getList(string projectID)
        {
            List<DataLog> result = new List<DataLog>();

            string sql = string.Format("SELECT * FROM MDS_IMP_DATA_LOG WHERE TEST_PROJECT_ID='{0}'", projectID);

            DataSet ds = OracleHelper.Query(sql);

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                DataLog item = DataLog.Parse(dr);
                result.Add(item);
            }

            return result;
        }

        public static List<DataLog> getDistinctList(string projectID) {

            List<DataLog> result = new List<DataLog>(); 
            string sql = string.Format("SELECT DISTINCT(typename), version,TEST_PROJECT_ID, OBJECT_TABLE FROM MDS_IMP_DATA_LOG WHERE TEST_PROJECT_ID='{0}'", projectID);

            DataSet ds = OracleHelper.Query(sql);

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                DataLog item = DataLog.Parse(dr);
                result.Add(item);
            }

            return result;
        }

        public static List<string> getScriptCodes(string projectID)
        {
            List<string> result = new List<string>();

            string sql = string.Format("select distinct TYPENAME from MDS_IMP_DATA_LOG WHERE TEST_PROJECT_ID='{0}'", projectID);
            DataSet ds = OracleHelper.Query(sql);

            if (ds != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                foreach (DataRow dr in dt.Rows)
                {
                    if (dr[0] != DBNull.Value && (!string.IsNullOrEmpty(dr[0].ToString())))
                        result.Add(dr[0].ToString());
                }
            }

            return result;
        }
    }
}
